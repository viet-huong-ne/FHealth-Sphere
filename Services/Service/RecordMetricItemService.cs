using Contract.Repositories.Entity;
using Core.Base;
using ModelViews.RecordMetricItemModelViews;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Contract.Repositories.Interface;
using Contract.Services.Interface;
using Microsoft.Extensions.Logging;

namespace Services.Service
{
    public class RecordMetricItemService : IRecordMetricItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RecordMetricItemService> _logger;

        public RecordMetricItemService(ILogger<RecordMetricItemService> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<RecordMetricItem> CreateRecordMetricItem(CreateRecordMetricItemModel model)
        {
            try
            {
                _logger.LogInformation("Attempting to create RecordMetricItem with RecordId: {RecordId}, HealthRecordId: {HealthRecordId}, MetricId: {MetricId}", model.RecordId, model.HealthRecordId, model.MetricId);

                if (model == null)
                {
                    throw new ArgumentNullException(nameof(model), "RecordMetricItem data is required.");
                }

                if (model.Value == null)
                {
                    throw new ArgumentException("Value is required.", nameof(model.Value));
                }

                if (string.IsNullOrWhiteSpace(model.Type))
                {
                    throw new ArgumentException("Type is required.", nameof(model.Type));
                }

                var healthRecordExists = await _unitOfWork.GetRepository<HealthRecord>()
                    .Entities
                    .AnyAsync(hr => hr.Id == model.HealthRecordId && !hr.DeletedTime.HasValue);
                if (!healthRecordExists)
                {
                    throw new KeyNotFoundException($"HealthRecord with ID {model.HealthRecordId} not found.");
                }

                var metricExists = await _unitOfWork.GetRepository<Metric>()
                    .Entities
                    .AnyAsync(m => m.Id == model.MetricId && !m.DeletedTime.HasValue);
                if (!metricExists)
                {
                    throw new KeyNotFoundException($"Metric with ID {model.MetricId} not found.");
                }

                // Kiểm tra duy nhất RecordId nếu cần (tùy ý)
                var existingRecord = await _unitOfWork.GetRepository<RecordMetricItem>()
                    .Entities
                    .FirstOrDefaultAsync(ri => ri.HealthRecord.Id == model.RecordId && !ri.DeletedTime.HasValue);
                if (existingRecord != null)
                {
                    throw new InvalidOperationException($"RecordMetricItem with RecordId {model.RecordId} already exists.");
                }
                var healthRecord = await _unitOfWork.GetRepository<HealthRecord>().GetByIdAsync(model.HealthRecordId);
                var recordMetricItem = new RecordMetricItem
                {
                    HealthRecord = healthRecord,
                    MetricId = model.MetricId,
                    Value = model.Value,
                    Type = model.Type.Trim(),
                    CreatedBy = "System",
                    CreatedTime = DateTimeOffset.Now,
                    LastUpdatedTime = DateTimeOffset.Now
                };

                await _unitOfWork.GetRepository<RecordMetricItem>().InsertAsync(recordMetricItem);
                await _unitOfWork.SaveAsync();
                return recordMetricItem;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create RecordMetricItem: {Message}", ex.InnerException?.Message ?? ex.Message);
                throw;
            }
        }

        public async Task<BasePaginatedList<RecordMetricItem>> GetAllRecordMetricItems(int pageNumber, int pageSize, int? recordId = null, int? healthRecordId = null, int? metricId = null, decimal? value = null, string type = null, string sortBy = null, string sortOrder = "asc", DateTime? createdStartDate = null, DateTime? createdEndDate = null, DateTime? updatedStartDate = null, DateTime? updatedEndDate = null, DateTime? deletedStartDate = null, DateTime? deletedEndDate = null, string createdBy = null, string updatedBy = null, string deletedBy = null, bool? isActive = null)
        {
            try
            {
                _logger.LogInformation("Fetching all RecordMetricItems with filters - PageNumber: {PageNumber}, PageSize: {PageSize}, RecordId: {RecordId}, HealthRecordId: {HealthRecordId}, MetricId: {MetricId}, Value: {Value}, Type: {Type}, SortBy: {SortBy}, SortOrder: {SortOrder}, CreatedStartDate: {CreatedStartDate}, CreatedEndDate: {CreatedEndDate}, UpdatedStartDate: {UpdatedStartDate}, UpdatedEndDate: {UpdatedEndDate}, DeletedStartDate: {DeletedStartDate}, DeletedEndDate: {DeletedEndDate}, CreatedBy: {CreatedBy}, UpdatedBy: {UpdatedBy}, DeletedBy: {DeletedBy}, IsActive: {IsActive}", pageNumber, pageSize, recordId, healthRecordId, metricId, value, type, sortBy, sortOrder, createdStartDate, createdEndDate, updatedStartDate, updatedEndDate, deletedStartDate, deletedEndDate, createdBy, updatedBy, deletedBy, isActive);

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;

                var itemsQuery = _unitOfWork.GetRepository<RecordMetricItem>()
                    .Entities
                    .AsQueryable();

                // Áp dụng bộ lọc
                if (recordId.HasValue)
                {
                    itemsQuery = itemsQuery.Where(ri => ri.HealthRecord.Id == recordId.Value);
                }
                if (healthRecordId.HasValue)
                {
                    itemsQuery = itemsQuery.Where(ri => ri.HealthRecord.Id == healthRecordId.Value);
                }
                if (metricId.HasValue)
                {
                    itemsQuery = itemsQuery.Where(ri => ri.MetricId == metricId.Value);
                }
                if (value.HasValue)
                {
                    itemsQuery = itemsQuery.Where(ri => ri.Value != null && ri.Value == value);
                }
                if (!string.IsNullOrWhiteSpace(type))
                {
                    itemsQuery = itemsQuery.Where(ri => ri.Type != null && ri.Type.Contains(type));
                }
                if (createdStartDate.HasValue)
                {
                    itemsQuery = itemsQuery.Where(ri => ri.CreatedTime.Date >= createdStartDate.Value.Date);
                }
                if (createdEndDate.HasValue)
                {
                    itemsQuery = itemsQuery.Where(ri => ri.CreatedTime.Date <= createdEndDate.Value.Date);
                }
                if (updatedStartDate.HasValue)
                {
                    itemsQuery = itemsQuery.Where(ri => ri.LastUpdatedTime.Date >= updatedStartDate.Value.Date);
                }
                if (updatedEndDate.HasValue)
                {
                    itemsQuery = itemsQuery.Where(ri => ri.LastUpdatedTime.Date <= updatedEndDate.Value.Date);
                }
                if (deletedStartDate.HasValue)
                {
                    itemsQuery = itemsQuery.Where(ri => ri.DeletedTime.HasValue && ri.DeletedTime.Value.Date >= deletedStartDate.Value.Date);
                }
                if (deletedEndDate.HasValue)
                {
                    itemsQuery = itemsQuery.Where(ri => ri.DeletedTime.HasValue && ri.DeletedTime.Value.Date <= deletedEndDate.Value.Date);
                }
                if (!string.IsNullOrWhiteSpace(createdBy))
                {
                    itemsQuery = itemsQuery.Where(ri => ri.CreatedBy != null && ri.CreatedBy.Contains(createdBy));
                }
                if (!string.IsNullOrWhiteSpace(updatedBy))
                {
                    itemsQuery = itemsQuery.Where(ri => ri.LastUpdatedBy != null && ri.LastUpdatedBy.Contains(updatedBy));
                }
                if (!string.IsNullOrWhiteSpace(deletedBy))
                {
                    itemsQuery = itemsQuery.Where(ri => ri.DeletedBy != null && ri.DeletedBy.Contains(deletedBy));
                }
                if (isActive.HasValue)
                {
                    itemsQuery = itemsQuery.Where(ri => (ri.DeletedTime.HasValue == !isActive.Value));
                }

                // Loại bỏ các bản ghi bị soft delete nếu không có bộ lọc DeletedTime hoặc isActive
                if (!deletedStartDate.HasValue && !deletedEndDate.HasValue && !isActive.HasValue)
                {
                    itemsQuery = itemsQuery.Where(ri => !ri.DeletedTime.HasValue);
                }

                // Áp dụng sắp xếp
                if (!string.IsNullOrWhiteSpace(sortBy))
                {
                    switch (sortBy.ToLower())
                    {
                        //case "recordid":
                        //    itemsQuery = sortOrder.ToLower() == "desc"
                        //        ? itemsQuery.OrderByDescending(ri => ri.HealthRecord.Id)
                        //        : itemsQuery.OrderBy(ri => ri.HealthRecord.Id);
                        //    break;
                        case "healthrecordid":
                            itemsQuery = sortOrder.ToLower() == "desc"
                                ? itemsQuery.OrderByDescending(ri => ri.HealthRecord.Id)
                                : itemsQuery.OrderBy(ri => ri.HealthRecord.Id);
                            break;
                        case "metricid":
                            itemsQuery = sortOrder.ToLower() == "desc"
                                ? itemsQuery.OrderByDescending(ri => ri.MetricId)
                                : itemsQuery.OrderBy(ri => ri.MetricId);
                            break;
                        case "value":
                            itemsQuery = sortOrder.ToLower() == "desc"
                                ? itemsQuery.OrderByDescending(ri => ri.Value)
                                : itemsQuery.OrderBy(ri => ri.Value);
                            break;
                        case "type":
                            itemsQuery = sortOrder.ToLower() == "desc"
                                ? itemsQuery.OrderByDescending(ri => ri.Type)
                                : itemsQuery.OrderBy(ri => ri.Type);
                            break;
                        case "createdtime":
                            itemsQuery = sortOrder.ToLower() == "desc"
                                ? itemsQuery.OrderByDescending(ri => ri.CreatedTime)
                                : itemsQuery.OrderBy(ri => ri.CreatedTime);
                            break;
                        case "lastupdatedtime":
                            itemsQuery = sortOrder.ToLower() == "desc"
                                ? itemsQuery.OrderByDescending(ri => ri.LastUpdatedTime)
                                : itemsQuery.OrderBy(ri => ri.LastUpdatedTime);
                            break;
                        case "deletedtime":
                            itemsQuery = sortOrder.ToLower() == "desc"
                                ? itemsQuery.OrderByDescending(ri => ri.DeletedTime ?? DateTimeOffset.MinValue)
                                : itemsQuery.OrderBy(ri => ri.DeletedTime ?? DateTimeOffset.MinValue);
                            break;
                        default:
                            itemsQuery = itemsQuery.OrderByDescending(ri => ri.CreatedTime); // Mặc định
                            break;
                    }
                }
                else
                {
                    itemsQuery = itemsQuery.OrderByDescending(ri => ri.CreatedTime); // Mặc định
                }

                int totalCount = await itemsQuery.CountAsync();

                var items = await itemsQuery
                    .Include(ri => ri.HealthRecord)
                    .Include(ri => ri.Metric)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new BasePaginatedList<RecordMetricItem>(items, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch RecordMetricItems: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<RecordMetricItem> GetRecordMetricItemById(int id)
        {
            try
            {
                _logger.LogInformation("Attempting to get RecordMetricItem with ID: {Id}", id);

                var recordMetricItem = await _unitOfWork.GetRepository<RecordMetricItem>()
                    .Entities
                    .Where(ri => ri.Id == id && !ri.DeletedTime.HasValue)
                    .Include(ri => ri.HealthRecord)
                    .Include(ri => ri.Metric)
                    .FirstOrDefaultAsync();

                if (recordMetricItem == null)
                {
                    throw new KeyNotFoundException($"RecordMetricItem with ID {id} not found or already deleted.");
                }

                return recordMetricItem;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get RecordMetricItem with ID {Id}: {Message}", id, ex.Message);
                throw;
            }
        }
        public async Task<RecordMetricItem> UpdateRecordMetricItem(int id, UpdateRecordMetricItemModel model)
        {
            try
            {
                _logger.LogInformation("Attempting to update RecordMetricItem with ID: {Id}", id);

                if (model == null)
                {
                    throw new ArgumentNullException(nameof(model), "Update data is required.");
                }

                var recordMetricItem = await _unitOfWork.GetRepository<RecordMetricItem>()
                    .Entities
                    .FirstOrDefaultAsync(ri => ri.Id == id && !ri.DeletedTime.HasValue);

                if (recordMetricItem == null)
                {
                    throw new KeyNotFoundException($"RecordMetricItem with ID {id} not found or already deleted.");
                }

                // Kiểm tra sự tồn tại của HealthRecordId nếu được cập nhật
                if (model.HealthRecordId.HasValue)
                {
                    var healthRecordExists = await _unitOfWork.GetRepository<HealthRecord>()
                        .Entities
                        .AnyAsync(hr => hr.Id == model.HealthRecordId.Value && !hr.DeletedTime.HasValue);
                    if (!healthRecordExists)
                    {
                        throw new KeyNotFoundException($"HealthRecord with ID {model.HealthRecordId.Value} not found.");
                    }
                    recordMetricItem.HealthRecord.Id = model.HealthRecordId.Value;
                }

                // Kiểm tra sự tồn tại của MetricId nếu được cập nhật
                if (model.MetricId.HasValue)
                {
                    var metricExists = await _unitOfWork.GetRepository<Metric>()
                        .Entities
                        .AnyAsync(m => m.Id == model.MetricId.Value && !m.DeletedTime.HasValue);
                    if (!metricExists)
                    {
                        throw new KeyNotFoundException($"Metric with ID {model.MetricId.Value} not found.");
                    }
                    recordMetricItem.MetricId = model.MetricId.Value;
                }

                // Kiểm tra duy nhất của RecordId nếu được cập nhật (tùy chọn)
                if (model.RecordId.HasValue && model.RecordId.Value != recordMetricItem.HealthRecord.Id)
                {
                    var existingRecord = await _unitOfWork.GetRepository<RecordMetricItem>()
                        .Entities
                        .FirstOrDefaultAsync(ri => ri.HealthRecord.Id == model.RecordId.Value && ri.Id != id && !ri.DeletedTime.HasValue);
                    if (existingRecord != null)
                    {
                        throw new InvalidOperationException($"RecordMetricItem with RecordId {model.RecordId.Value} already exists.");
                    }
                    //recordMetricItem.RecordId = model.RecordId.Value;
                }

                // Cập nhật các trường khác nếu có
                if (model.Value.HasValue)
                {
                    recordMetricItem.Value = model.Value;
                }

                if (!string.IsNullOrWhiteSpace(model.Type))
                {
                    recordMetricItem.Type = model.Type.Trim();
                }

                // Cập nhật metadata
                recordMetricItem.LastUpdatedTime = DateTimeOffset.Now;
                recordMetricItem.LastUpdatedBy = "System"; // Nên lấy từ context người dùng hiện tại nếu có

                await _unitOfWork.GetRepository<RecordMetricItem>().UpdateAsync(recordMetricItem);
                await _unitOfWork.SaveAsync();
                return recordMetricItem;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update RecordMetricItem with ID {Id}: {Message}", id, ex.InnerException?.Message ?? ex.Message);
                throw;
            }
        }

        public async Task<bool> DeleteRecordMetricItem(int id)
        {
            var recordMetricItem = await _unitOfWork.GetRepository<RecordMetricItem>()
                .Entities
                .FirstOrDefaultAsync(r => r.Id == id && !r.DeletedTime.HasValue);

            if (recordMetricItem == null)
            {
                return false;
            }

            recordMetricItem.DeletedTime = DateTimeOffset.Now;
            recordMetricItem.DeletedBy = "System"; // Nên lấy từ context người dùng hiện tại nếu có

            await _unitOfWork.GetRepository<RecordMetricItem>().UpdateAsync(recordMetricItem);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public Task<HealthRecord> GetHealthRecordById(int id)
        {
            throw new NotImplementedException();
        }
    }
}