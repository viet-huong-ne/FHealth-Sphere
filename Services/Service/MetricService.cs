using Contract.Repositories.Entity;
using Core.Base;
using ModelViews.MetricModelViews;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Contract.Repositories.Interface;
using Contract.Services.Interface;
using Microsoft.Extensions.Logging;

namespace FHealthSphere.Services.Services
{
    public class MetricService : IMetricService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MetricService> _logger;

        public MetricService(IUnitOfWork unitOfWork, ILogger<MetricService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

            // Kiểm tra MetricGroupId tồn tại nếu có
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
                CreatedBy = "System", // Nên lấy từ context người dùng hiện tại nếu có
                CreatedTime = DateTimeOffset.Now,
                LastUpdatedTime = DateTimeOffset.Now
            };

            await _unitOfWork.GetRepository<Metric>().InsertAsync(metric);
            await _unitOfWork.SaveAsync();
            return metric;
        }

        public async Task<BasePaginatedList<Metric>> GetAllMetrics(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10; // Default page size

            var metricsQuery = _unitOfWork.GetRepository<Metric>()
                .Entities
                .Where(m => !m.DeletedTime.HasValue)
                .OrderByDescending(m => m.CreatedTime);

            int totalCount = await metricsQuery.CountAsync();

            var metrics = await metricsQuery
                .Include(m => m.MetricGroup)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new BasePaginatedList<Metric>(metrics, totalCount, pageNumber, pageSize);
        }

        public async Task<Metric> GetMetricById(int id)
        {
            try
            {
                _logger.LogInformation("Attempting to get Metric with ID: {Id}", id);

                var metric = await _unitOfWork.GetRepository<Metric>()
                    .Entities
                    .Where(m => m.Id == id && !m.DeletedTime.HasValue)
                    .Include(m => m.MetricGroup)
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

            // Kiểm tra MetricGroupId nếu được cập nhật
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
            }

            metric.LastUpdatedTime = DateTimeOffset.Now;
            metric.LastUpdatedBy = "System"; // Nên lấy từ context người dùng hiện tại nếu có

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
            metric.DeletedBy = "System"; // Nên lấy từ context người dùng hiện tại nếu có

            await _unitOfWork.GetRepository<Metric>().UpdateAsync(metric);
            await _unitOfWork.SaveAsync();
            return true;
        }

    }
}