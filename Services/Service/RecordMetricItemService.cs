using Contract.Repositories.Entity;
using Core.Base;
using ModelViews.RecordMetricItemModelViews;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Contract.Repositories.Interface;
using Contract.Services.Interface;
using Microsoft.Extensions.Logging;

namespace FHealthSphere.Services.Services
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

                if (string.IsNullOrWhiteSpace(model.Value))
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
                    .FirstOrDefaultAsync(ri => ri.RecordId == model.RecordId && !ri.DeletedTime.HasValue);
                if (existingRecord != null)
                {
                    throw new InvalidOperationException($"RecordMetricItem with RecordId {model.RecordId} already exists.");
                }

                var recordMetricItem = new RecordMetricItem
                {
                    RecordId = model.RecordId, // Gán giá trị tùy chỉnh
                    HealthRecordId = model.HealthRecordId,
                    MetricId = model.MetricId,
                    Value = model.Value.Trim(),
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

        public async Task<BasePaginatedList<RecordMetricItem>> GetAllRecordMetricItems(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10; // Default page size

            var recordMetricItemsQuery = _unitOfWork.GetRepository<RecordMetricItem>()
                .Entities
                .Where(r => !r.DeletedTime.HasValue)
                .OrderByDescending(r => r.CreatedTime);

            int totalCount = await recordMetricItemsQuery.CountAsync();

            var recordMetricItems = await recordMetricItemsQuery
                .Include(r => r.HealthRecord)
                .ThenInclude(hr => hr.Patient)
                .Include(r => r.HealthRecord)
                .ThenInclude(hr => hr.Band)
                .ThenInclude(b => b.BandBrand)
                .Include(r => r.Metric)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new BasePaginatedList<RecordMetricItem>(recordMetricItems, totalCount, pageNumber, pageSize);
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
                    recordMetricItem.HealthRecordId = model.HealthRecordId.Value;
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
                if (model.RecordId.HasValue && model.RecordId.Value != recordMetricItem.RecordId)
                {
                    var existingRecord = await _unitOfWork.GetRepository<RecordMetricItem>()
                        .Entities
                        .FirstOrDefaultAsync(ri => ri.RecordId == model.RecordId.Value && ri.Id != id && !ri.DeletedTime.HasValue);
                    if (existingRecord != null)
                    {
                        throw new InvalidOperationException($"RecordMetricItem with RecordId {model.RecordId.Value} already exists.");
                    }
                    recordMetricItem.RecordId = model.RecordId.Value;
                }

                // Cập nhật các trường khác nếu có
                if (!string.IsNullOrWhiteSpace(model.Value))
                {
                    recordMetricItem.Value = model.Value.Trim();
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