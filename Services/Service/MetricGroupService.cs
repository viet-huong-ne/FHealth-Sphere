using Contract.Repositories.Entity;
using Core.Base;
using ModelViews.MetricGroupModelViews;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Contract.Repositories.Interface;
using Contract.Services.Interface;
using Microsoft.Extensions.Logging;

namespace Services.Service
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

        public async Task<BasePaginatedList<MetricGroup>> GetAllMetricGroups(int pageNumber, int pageSize, string name = null, string sortBy = null, string sortOrder = "asc", DateTime? createdStartDate = null, DateTime? createdEndDate = null, DateTime? updatedStartDate = null, DateTime? updatedEndDate = null, DateTime? deletedStartDate = null, DateTime? deletedEndDate = null, string createdBy = null, string updatedBy = null, string deletedBy = null, bool? isActive = null)
        {
            try
            {
                _logger.LogInformation("Fetching all MetricGroups with filters - PageNumber: {PageNumber}, PageSize: {PageSize}, Name: {Name}, SortBy: {SortBy}, SortOrder: {SortOrder}, CreatedStartDate: {CreatedStartDate}, CreatedEndDate: {CreatedEndDate}, UpdatedStartDate: {UpdatedStartDate}, UpdatedEndDate: {UpdatedEndDate}, DeletedStartDate: {DeletedStartDate}, DeletedEndDate: {DeletedEndDate}, CreatedBy: {CreatedBy}, UpdatedBy: {UpdatedBy}, DeletedBy: {DeletedBy}, IsActive: {IsActive}", pageNumber, pageSize, name, sortBy, sortOrder, createdStartDate, createdEndDate, updatedStartDate, updatedEndDate, deletedStartDate, deletedEndDate, createdBy, updatedBy, deletedBy, isActive);

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;

                var groupsQuery = _unitOfWork.GetRepository<MetricGroup>()
                    .Entities
                    .AsQueryable();

                // Áp dụng bộ lọc
                if (!string.IsNullOrWhiteSpace(name))
                {
                    groupsQuery = groupsQuery.Where(mg => mg.Name.Contains(name));
                }
                if (createdStartDate.HasValue)
                {
                    groupsQuery = groupsQuery.Where(mg => mg.CreatedTime.Date >= createdStartDate.Value.Date);
                }
                if (createdEndDate.HasValue)
                {
                    groupsQuery = groupsQuery.Where(mg => mg.CreatedTime.Date <= createdEndDate.Value.Date);
                }
                if (updatedStartDate.HasValue)
                {
                    groupsQuery = groupsQuery.Where(mg => mg.LastUpdatedTime.Date >= updatedStartDate.Value.Date);
                }
                if (updatedEndDate.HasValue)
                {
                    groupsQuery = groupsQuery.Where(mg => mg.LastUpdatedTime.Date <= updatedEndDate.Value.Date);
                }
                if (deletedStartDate.HasValue)
                {
                    groupsQuery = groupsQuery.Where(mg => mg.DeletedTime.HasValue && mg.DeletedTime.Value.Date >= deletedStartDate.Value.Date);
                }
                if (deletedEndDate.HasValue)
                {
                    groupsQuery = groupsQuery.Where(mg => mg.DeletedTime.HasValue && mg.DeletedTime.Value.Date <= deletedEndDate.Value.Date);
                }
                if (!string.IsNullOrWhiteSpace(createdBy))
                {
                    groupsQuery = groupsQuery.Where(mg => mg.CreatedBy != null && mg.CreatedBy.Contains(createdBy));
                }
                if (!string.IsNullOrWhiteSpace(updatedBy))
                {
                    groupsQuery = groupsQuery.Where(mg => mg.LastUpdatedBy != null && mg.LastUpdatedBy.Contains(updatedBy));
                }
                if (!string.IsNullOrWhiteSpace(deletedBy))
                {
                    groupsQuery = groupsQuery.Where(mg => mg.DeletedBy != null && mg.DeletedBy.Contains(deletedBy));
                }
                if (isActive.HasValue)
                {
                    groupsQuery = groupsQuery.Where(mg => (mg.DeletedTime.HasValue == !isActive.Value));
                }

                // Loại bỏ các bản ghi bị soft delete nếu không có bộ lọc DeletedTime hoặc isActive
                if (!deletedStartDate.HasValue && !deletedEndDate.HasValue && !isActive.HasValue)
                {
                    groupsQuery = groupsQuery.Where(mg => !mg.DeletedTime.HasValue);
                }

                // Áp dụng sắp xếp
                if (!string.IsNullOrWhiteSpace(sortBy))
                {
                    switch (sortBy.ToLower())
                    {
                        case "displayorder":
                            groupsQuery = sortOrder.ToLower() == "desc"
                                ? groupsQuery.OrderByDescending(mg => mg.DisplayOrder)
                                : groupsQuery.OrderBy(mg => mg.DisplayOrder);
                            break;
                        case "name":
                            groupsQuery = sortOrder.ToLower() == "desc"
                                ? groupsQuery.OrderByDescending(mg => mg.Name)
                                : groupsQuery.OrderBy(mg => mg.Name);
                            break;
                        case "createdtime":
                            groupsQuery = sortOrder.ToLower() == "desc"
                                ? groupsQuery.OrderByDescending(mg => mg.CreatedTime)
                                : groupsQuery.OrderBy(mg => mg.CreatedTime);
                            break;
                        case "lastupdatedtime":
                            groupsQuery = sortOrder.ToLower() == "desc"
                                ? groupsQuery.OrderByDescending(mg => mg.LastUpdatedTime)
                                : groupsQuery.OrderBy(mg => mg.LastUpdatedTime);
                            break;
                        case "deletedtime":
                            groupsQuery = sortOrder.ToLower() == "desc"
                                ? groupsQuery.OrderByDescending(mg => mg.DeletedTime ?? DateTimeOffset.MinValue)
                                : groupsQuery.OrderBy(mg => mg.DeletedTime ?? DateTimeOffset.MinValue);
                            break;
                        default:
                            groupsQuery = groupsQuery.OrderByDescending(mg => mg.CreatedTime); // Mặc định
                            break;
                    }
                }
                else
                {
                    groupsQuery = groupsQuery.OrderByDescending(mg => mg.CreatedTime); // Mặc định
                }

                int totalCount = await groupsQuery.CountAsync();

                var groups = await groupsQuery
                    .Include(mg => mg.Tags)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new BasePaginatedList<MetricGroup>(groups, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch MetricGroups: {Message}", ex.Message);
                throw;
            }
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