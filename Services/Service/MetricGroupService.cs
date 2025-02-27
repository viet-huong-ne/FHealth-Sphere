using Contract.Repositories.Entity;
using Core.Base;
using ModelViews.MetricGroupModelViews;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Contract.Repositories.Interface;
using Contract.Services.Interface;
using Microsoft.Extensions.Logging;

namespace FHealthSphere.Services.Services
{
    public class MetricGroupService : IMetricGroupService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<MetricGroupService> _logger;

        public MetricGroupService(IUnitOfWork unitOfWork, ILogger<MetricGroupService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<MetricGroup> CreateMetricGroup(CreateMetricGroupModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model), "MetricGroup data is required.");
            }

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                throw new ArgumentException("Name is required.", nameof(model.Name));
            }

            if (string.IsNullOrWhiteSpace(model.Status))
            {
                throw new ArgumentException("Status is required.", nameof(model.Status));
            }

            var metricGroup = new MetricGroup
            {
                Name = model.Name.Trim(),
                DisplayOrder = model.DisplayOrder,
                Status = model.Status.Trim(),
                CreatedBy = "System", // Nên lấy từ context người dùng hiện tại nếu có
                CreatedTime = DateTimeOffset.Now,
                LastUpdatedTime = DateTimeOffset.Now
            };

            await _unitOfWork.GetRepository<MetricGroup>().InsertAsync(metricGroup);
            await _unitOfWork.SaveAsync();
            return metricGroup;
        }

        public async Task<BasePaginatedList<MetricGroup>> GetAllMetricGroups(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10; // Default page size

            var metricGroupsQuery = _unitOfWork.GetRepository<MetricGroup>()
                .Entities
                .Where(mg => !mg.DeletedTime.HasValue)
                .OrderBy(mg => mg.DisplayOrder); // Sắp xếp theo DisplayOrder

            int totalCount = await metricGroupsQuery.CountAsync();

            var metricGroups = await metricGroupsQuery
                .Include(mg => mg.Tags) // Load danh sách Metrics nếu cần
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new BasePaginatedList<MetricGroup>(metricGroups, totalCount, pageNumber, pageSize);
        }
        public async Task<MetricGroup> GetMetricGroupById(int id)
        {
            try
            {
                _logger.LogInformation("Attempting to get MetricGroup with ID: {Id}", id);

                var metricGroup = await _unitOfWork.GetRepository<MetricGroup>()
                    .Entities
                    .Where(mg => mg.Id == id && !mg.DeletedTime.HasValue)
                    .Include(mg => mg.Tags)
                    .FirstOrDefaultAsync();

                if (metricGroup == null)
                {
                    throw new KeyNotFoundException($"MetricGroup with ID {id} not found or already deleted.");
                }

                return metricGroup;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get MetricGroup with ID {Id}: {Message}", id, ex.Message);
                throw;
            }
        }
        public async Task<MetricGroup> UpdateMetricGroup(int id, UpdateMetricGroupModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model), "Update data is required.");
            }

            var metricGroup = await _unitOfWork.GetRepository<MetricGroup>()
                .Entities
                .FirstOrDefaultAsync(mg => mg.Id == id && !mg.DeletedTime.HasValue);

            if (metricGroup == null)
            {
                throw new KeyNotFoundException($"MetricGroup with ID {id} not found or already deleted.");
            }

            if (!string.IsNullOrWhiteSpace(model.Name))
            {
                metricGroup.Name = model.Name.Trim();
            }

            if (model.DisplayOrder.HasValue)
            {
                metricGroup.DisplayOrder = model.DisplayOrder.Value;
            }

            if (!string.IsNullOrWhiteSpace(model.Status))
            {
                metricGroup.Status = model.Status.Trim();
            }

            metricGroup.LastUpdatedTime = DateTimeOffset.Now;
            metricGroup.LastUpdatedBy = "System"; // Nên lấy từ context người dùng hiện tại nếu có

            await _unitOfWork.GetRepository<MetricGroup>().UpdateAsync(metricGroup);
            await _unitOfWork.SaveAsync();
            return metricGroup;
        }

        public async Task<bool> DeleteMetricGroup(int id)
        {
            var metricGroup = await _unitOfWork.GetRepository<MetricGroup>()
                .Entities
                .FirstOrDefaultAsync(mg => mg.Id == id && !mg.DeletedTime.HasValue);

            if (metricGroup == null)
            {
                return false;
            }

            metricGroup.DeletedTime = DateTimeOffset.Now;
            metricGroup.DeletedBy = "System"; // Nên lấy từ context người dùng hiện tại nếu có

            await _unitOfWork.GetRepository<MetricGroup>().UpdateAsync(metricGroup);
            await _unitOfWork.SaveAsync();
            return true;
        }


    }
}