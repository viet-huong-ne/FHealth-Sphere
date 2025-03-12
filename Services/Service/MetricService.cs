using Contract.Repositories.Entity;
using Core.Base;
using ModelViews.MetricModelViews;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Contract.Repositories.Interface;
using Contract.Services.Interface;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Configuration;


namespace Services.Service
{
    public class MetricService : IMetricService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MetricService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _notificationApiUrl;
        private readonly string _bearerToken;
        private readonly string _token = "cmGkuPYJSv6MNtfg4-od6_:APA91bEfb0yCJMC8nu_v_oHGEPW1e-Enc5QbHGZYm-u_OHUg_6-Y6uFuqRMgd3zgzD8B4vCUFqwFCUYcK4ow7vZZeLNomZDiv-6zspYDQ-zKLJrzOVeahrY";

        public MetricService(IUnitOfWork unitOfWork, ILogger<MetricService> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClientFactory.CreateClient() ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _notificationApiUrl = configuration["NotificationSettings:ApiUrl"] ?? throw new ArgumentNullException(nameof(configuration), "NotificationSettings:ApiUrl is missing.");
            _bearerToken = configuration["NotificationSettings:BearerToken"] ?? throw new ArgumentNullException(nameof(configuration), "NotificationSettings:BearerToken is missing.");
        }

        public async Task<Metric> CreateMetric(CreateMetricModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model), "Metric data is required.");
            }

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                throw new ArgumentException("Name is required.", nameof(model.Name));
            }

            if (string.IsNullOrWhiteSpace(model.Unit))
            {
                throw new ArgumentException("Unit is required.", nameof(model.Unit));
            }

            if (model.MetricGroupId.HasValue)
            {
                var metricGroupExists = await _unitOfWork.GetRepository<MetricGroup>()
                    .Entities
                    .AnyAsync(mg => mg.Id == model.MetricGroupId.Value && !mg.DeletedTime.HasValue);
                if (!metricGroupExists)
                {
                    throw new KeyNotFoundException($"MetricGroup with ID {model.MetricGroupId.Value} not found.");
                }
            }

            var metric = new Metric
            {
                Name = model.Name.Trim(),
                Unit = model.Unit.Trim(),
                MinValue = model.MinValue,
                MaxValue = model.MaxValue,
                DefaultValue = model.DefaultValue,
                MetricGroupId = model.MetricGroupId,
                CreatedBy = "System",
                CreatedTime = DateTimeOffset.Now,
                LastUpdatedTime = DateTimeOffset.Now
            };

            await _unitOfWork.GetRepository<Metric>().InsertAsync(metric);
            await _unitOfWork.SaveAsync();
            return metric;
        }

        public async Task<BasePaginatedList<Metric>> GetAllMetrics(
            int pageNumber,
            int pageSize,
            string name = null,
            string unit = null,
            int? metricGroupId = null,
            string sortBy = null,
            string sortOrder = "asc",
            DateTime? createdStartDate = null,
            DateTime? createdEndDate = null,
            DateTime? updatedStartDate = null,
            DateTime? updatedEndDate = null,
            DateTime? deletedStartDate = null,
            DateTime? deletedEndDate = null,
            string createdBy = null,
            string updatedBy = null,
            string deletedBy = null,
            bool? isActive = null)
        {
            try
            {
                _logger.LogInformation("Fetching all Metrics with filters - PageNumber: {PageNumber}, PageSize: {PageSize}, Name: {Name}, Unit: {Unit}, MetricGroupId: {MetricGroupId}, SortBy: {SortBy}, SortOrder: {SortOrder}, CreatedStartDate: {CreatedStartDate}, CreatedEndDate: {CreatedEndDate}, UpdatedStartDate: {UpdatedStartDate}, UpdatedEndDate: {UpdatedEndDate}, DeletedStartDate: {DeletedStartDate}, DeletedEndDate: {DeletedEndDate}, CreatedBy: {CreatedBy}, UpdatedBy: {UpdatedBy}, DeletedBy: {DeletedBy}, IsActive: {IsActive}",
                    pageNumber, pageSize, name, unit, metricGroupId, sortBy, sortOrder, createdStartDate, createdEndDate, updatedStartDate, updatedEndDate, deletedStartDate, deletedEndDate, createdBy, updatedBy, deletedBy, isActive);

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;

                var metricsQuery = _unitOfWork.GetRepository<Metric>()
                    .Entities
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(name))
                {
                    metricsQuery = metricsQuery.Where(m => m.Name.Contains(name));
                }
                if (!string.IsNullOrWhiteSpace(unit))
                {
                    metricsQuery = metricsQuery.Where(m => m.Unit != null && m.Unit.Contains(unit));
                }
                if (metricGroupId.HasValue)
                {
                    metricsQuery = metricsQuery.Where(m => m.MetricGroupId == metricGroupId.Value);
                }
                if (createdStartDate.HasValue)
                {
                    metricsQuery = metricsQuery.Where(m => m.CreatedTime.Date >= createdStartDate.Value.Date);
                }
                if (createdEndDate.HasValue)
                {
                    metricsQuery = metricsQuery.Where(m => m.CreatedTime.Date <= createdEndDate.Value.Date);
                }
                if (updatedStartDate.HasValue)
                {
                    metricsQuery = metricsQuery.Where(m => m.LastUpdatedTime.Date >= updatedStartDate.Value.Date);
                }
                if (updatedEndDate.HasValue)
                {
                    metricsQuery = metricsQuery.Where(m => m.LastUpdatedTime.Date <= updatedEndDate.Value.Date);
                }
                if (deletedStartDate.HasValue)
                {
                    metricsQuery = metricsQuery.Where(m => m.DeletedTime.HasValue && m.DeletedTime.Value.Date >= deletedStartDate.Value.Date);
                }
                if (deletedEndDate.HasValue)
                {
                    metricsQuery = metricsQuery.Where(m => m.DeletedTime.HasValue && m.DeletedTime.Value.Date <= deletedEndDate.Value.Date);
                }
                if (!string.IsNullOrWhiteSpace(createdBy))
                {
                    metricsQuery = metricsQuery.Where(m => m.CreatedBy != null && m.CreatedBy.Contains(createdBy));
                }
                if (!string.IsNullOrWhiteSpace(updatedBy))
                {
                    metricsQuery = metricsQuery.Where(m => m.LastUpdatedBy != null && m.LastUpdatedBy.Contains(updatedBy));
                }
                if (!string.IsNullOrWhiteSpace(deletedBy))
                {
                    metricsQuery = metricsQuery.Where(m => m.DeletedBy != null && m.DeletedBy.Contains(deletedBy));
                }
                if (isActive.HasValue)
                {
                    metricsQuery = metricsQuery.Where(m => (m.DeletedTime.HasValue == !isActive.Value));
                }

                if (!deletedStartDate.HasValue && !deletedEndDate.HasValue && !isActive.HasValue)
                {
                    metricsQuery = metricsQuery.Where(m => !m.DeletedTime.HasValue);
                }

                if (!string.IsNullOrWhiteSpace(sortBy))
                {
                    switch (sortBy.ToLower())
                    {
                        case "name":
                            metricsQuery = sortOrder.ToLower() == "desc"
                                ? metricsQuery.OrderByDescending(m => m.Name)
                                : metricsQuery.OrderBy(m => m.Name);
                            break;
                        case "unit":
                            metricsQuery = sortOrder.ToLower() == "desc"
                                ? metricsQuery.OrderByDescending(m => m.Unit)
                                : metricsQuery.OrderBy(m => m.Unit);
                            break;
                        case "minvalue":
                            metricsQuery = sortOrder.ToLower() == "desc"
                                ? metricsQuery.OrderByDescending(m => m.MinValue)
                                : metricsQuery.OrderBy(m => m.MinValue);
                            break;
                        case "maxvalue":
                            metricsQuery = sortOrder.ToLower() == "desc"
                                ? metricsQuery.OrderByDescending(m => m.MaxValue)
                                : metricsQuery.OrderBy(m => m.MaxValue);
                            break;
                        case "defaultvalue":
                            metricsQuery = sortOrder.ToLower() == "desc"
                                ? metricsQuery.OrderByDescending(m => m.DefaultValue)
                                : metricsQuery.OrderBy(m => m.DefaultValue);
                            break;
                        case "metricgroupid":
                            metricsQuery = sortOrder.ToLower() == "desc"
                                ? metricsQuery.OrderByDescending(m => m.MetricGroupId)
                                : metricsQuery.OrderBy(m => m.MetricGroupId);
                            break;
                        case "createdtime":
                            metricsQuery = sortOrder.ToLower() == "desc"
                                ? metricsQuery.OrderByDescending(m => m.CreatedTime)
                                : metricsQuery.OrderBy(m => m.CreatedTime);
                            break;
                        case "lastupdatedtime":
                            metricsQuery = sortOrder.ToLower() == "desc"
                                ? metricsQuery.OrderByDescending(m => m.LastUpdatedTime)
                                : metricsQuery.OrderBy(m => m.LastUpdatedTime);
                            break;
                        case "deletedtime":
                            metricsQuery = sortOrder.ToLower() == "desc"
                                ? metricsQuery.OrderByDescending(m => m.DeletedTime ?? DateTimeOffset.MinValue)
                                : metricsQuery.OrderBy(m => m.DeletedTime ?? DateTimeOffset.MinValue);
                            break;
                        default:
                            metricsQuery = metricsQuery.OrderByDescending(m => m.CreatedTime);
                            break;
                    }
                }
                else
                {
                    metricsQuery = metricsQuery.OrderByDescending(m => m.CreatedTime);
                }

                int totalCount = await metricsQuery.CountAsync();

                var metrics = await metricsQuery
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new BasePaginatedList<Metric>(metrics, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch Metrics: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<Metric> GetMetricById(int id)
        {
            try
            {
                _logger.LogInformation("Attempting to get Metric with ID: {Id}", id);

                var metric = await _unitOfWork.GetRepository<Metric>()
                    .Entities
                    .Where(m => m.Id == id && !m.DeletedTime.HasValue)
                    .FirstOrDefaultAsync();

                if (metric == null)
                {
                    throw new KeyNotFoundException($"Metric with ID {id} not found or already deleted.");
                }

                return metric;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Metric with ID {Id}: {Message}", id, ex.Message);
                throw;
            }
        }

        public async Task<Metric> UpdateMetric(int id, UpdateMetricModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model), "Update data is required.");
            }

            var metric = await _unitOfWork.GetRepository<Metric>()
                .Entities
                .FirstOrDefaultAsync(m => m.Id == id && !m.DeletedTime.HasValue);

            if (metric == null)
            {
                throw new KeyNotFoundException($"Metric with ID {id} not found or already deleted.");
            }

            if (model.MetricGroupId.HasValue)
            {
                var metricGroupExists = await _unitOfWork.GetRepository<MetricGroup>()
                    .Entities
                    .AnyAsync(mg => mg.Id == model.MetricGroupId.Value && !mg.DeletedTime.HasValue);
                if (!metricGroupExists)
                {
                    throw new KeyNotFoundException($"MetricGroup with ID {model.MetricGroupId.Value} not found.");
                }
                metric.MetricGroupId = model.MetricGroupId.Value;
            }

            if (!string.IsNullOrWhiteSpace(model.Name))
            {
                metric.Name = model.Name.Trim();
            }

            if (!string.IsNullOrWhiteSpace(model.Unit))
            {
                metric.Unit = model.Unit.Trim();
            }

            if (model.MinValue.HasValue)
            {
                metric.MinValue = model.MinValue.Value;
            }

            if (model.MaxValue.HasValue)
            {
                metric.MaxValue = model.MaxValue.Value;
            }

            if (model.DefaultValue.HasValue)
            {
                metric.DefaultValue = model.DefaultValue.Value;

                // Gọi API và nhận kết quả
                string notificationResult = await SendNotificationIfThresholdExceeded(metric, model.DefaultValue.Value, _token);
                _logger.LogInformation("Notification result for Metric ID {Id}: {Result}", metric.Id, notificationResult);
            }

            metric.LastUpdatedTime = DateTimeOffset.Now;
            metric.LastUpdatedBy = "System";

            await _unitOfWork.GetRepository<Metric>().UpdateAsync(metric);
            await _unitOfWork.SaveAsync();
            return metric;
        }

        public async Task<bool> DeleteMetric(int id)
        {
            var metric = await _unitOfWork.GetRepository<Metric>()
                .Entities
                .FirstOrDefaultAsync(m => m.Id == id && !m.DeletedTime.HasValue);

            if (metric == null)
            {
                return false;
            }

            metric.DeletedTime = DateTimeOffset.Now;
            metric.DeletedBy = "System";

            await _unitOfWork.GetRepository<Metric>().UpdateAsync(metric);
            await _unitOfWork.SaveAsync();
            return true;
        }

        private async Task<string> SendNotificationIfThresholdExceeded(Metric metric, decimal currentValue, string deviceToken)
        {
            try
            {
                bool isThresholdExceeded = false;
                string message = string.Empty;

                // Kiểm tra ngưỡng
                if (metric.MinValue.HasValue && currentValue < metric.MinValue.Value)
                {
                    isThresholdExceeded = true;
                    message = $"Metric {metric.Name} value ({currentValue}) is below the minimum threshold ({metric.MinValue.Value}).";
                }
                else if (metric.MaxValue.HasValue && currentValue > metric.MaxValue.Value)
                {
                    isThresholdExceeded = true;
                    message = $"Metric {metric.Name} value ({currentValue}) is above the maximum threshold ({metric.MaxValue.Value}).";
                }

                if (isThresholdExceeded)
                {
                    _logger.LogWarning("Threshold exceeded for Metric ID: {Id}. Sending notification.", metric.Id);

                    // Chuẩn bị payload
                    var notificationPayload = new
                    {
                        message = new
                        {
                            token = deviceToken,
                            notification = new
                            {
                                body = message,
                                title = "FCM Message"
                            }
                        }
                    };

                    var jsonPayload = JsonSerializer.Serialize(notificationPayload);
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    // Thêm Bearer Token vào header
                    _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _bearerToken);

                    // Gửi yêu cầu đến API
                    var response = await _httpClient.PostAsync(_notificationApiUrl, content);

                    // Đọc phản hồi từ API
                    string responseContent = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Notification sent successfully for Metric ID: {Id}. Response: {Response}", metric.Id, responseContent);
                        return $"Success: {responseContent}";
                    }
                    else
                    {
                        _logger.LogError("Failed to send notification for Metric ID: {Id}. Status: {StatusCode}, Response: {Response}",
                            metric.Id, response.StatusCode, responseContent);
                        return $"Failed: Status {response.StatusCode} - {responseContent}";
                    }
                }

                return "No threshold exceeded, no notification sent.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification for Metric ID: {Id}.", metric.Id);
                return $"Error: {ex.Message}";
            }
        }

        Task<Metric> IMetricService.UpdateMetric(int id, UpdateMetricModel model)
        {
            throw new NotImplementedException();
        }
    }
}