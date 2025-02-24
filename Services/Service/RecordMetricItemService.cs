using Contract.Repositories.Entity;
using Core.Base;
using ModelViews.RecordMetricItemModelViews;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Contract.Repositories.Interface;
using Contract.Services.Interface;

namespace FHealthSphere.Services.Services
{
    public class RecordMetricItemService : IRecordMetricItemService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RecordMetricItemService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<RecordMetricItem> CreateRecordMetricItem(CreateRecordMetricItemModel model)
        {
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

            // Kiểm tra RecordId tồn tại
            var healthRecordExists = await _unitOfWork.GetRepository<HealthRecord>()
                .Entities
                .AnyAsync(hr => hr.Id == model.RecordId && !hr.DeletedTime.HasValue);
            if (!healthRecordExists)
            {
                throw new KeyNotFoundException($"HealthRecord with ID {model.RecordId} not found.");
            }

            // Kiểm tra MetricId tồn tại
            var metricExists = await _unitOfWork.GetRepository<Metric>()
                .Entities
                .AnyAsync(m => m.Id == model.MetricId && !m.DeletedTime.HasValue);
            if (!metricExists)
            {
                throw new KeyNotFoundException($"Metric with ID {model.MetricId} not found.");
            }

            var recordMetricItem = new RecordMetricItem
            {
                RecordId = model.RecordId,
                MetricId = model.MetricId,
                Value = model.Value.Trim(),
                Type = model.Type.Trim(),
                CreatedBy = "System", // Nên lấy từ context người dùng hiện tại nếu có
                CreatedTime = DateTimeOffset.Now,
                LastUpdatedTime = DateTimeOffset.Now
            };

            await _unitOfWork.GetRepository<RecordMetricItem>().InsertAsync(recordMetricItem);
            await _unitOfWork.SaveAsync();
            return recordMetricItem;
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

        public async Task<RecordMetricItem> UpdateRecordMetricItem(int id, UpdateRecordMetricItemModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model), "Update data is required.");
            }

            var recordMetricItem = await _unitOfWork.GetRepository<RecordMetricItem>()
                .Entities
                .FirstOrDefaultAsync(r => r.Id == id && !r.DeletedTime.HasValue);

            if (recordMetricItem == null)
            {
                throw new KeyNotFoundException($"RecordMetricItem with ID {id} not found or already deleted.");
            }

            // Kiểm tra RecordId nếu được cập nhật
            if (model.RecordId.HasValue)
            {
                var healthRecordExists = await _unitOfWork.GetRepository<HealthRecord>()
                    .Entities
                    .AnyAsync(hr => hr.Id == model.RecordId.Value && !hr.DeletedTime.HasValue);
                if (!healthRecordExists)
                {
                    throw new KeyNotFoundException($"HealthRecord with ID {model.RecordId.Value} not found.");
                }
                recordMetricItem.RecordId = model.RecordId.Value;
            }

            // Kiểm tra MetricId nếu được cập nhật
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

            if (!string.IsNullOrWhiteSpace(model.Value))
            {
                recordMetricItem.Value = model.Value.Trim();
            }

            if (!string.IsNullOrWhiteSpace(model.Type))
            {
                recordMetricItem.Type = model.Type.Trim();
            }

            recordMetricItem.LastUpdatedTime = DateTimeOffset.Now;
            recordMetricItem.LastUpdatedBy = "System"; // Nên lấy từ context người dùng hiện tại nếu có

            await _unitOfWork.GetRepository<RecordMetricItem>().UpdateAsync(recordMetricItem);
            await _unitOfWork.SaveAsync();
            return recordMetricItem;
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
    }
}