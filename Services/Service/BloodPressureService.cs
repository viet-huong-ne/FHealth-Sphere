using Contract.Repositories.Entity;
using Contract.Repositories.Interface;
using Contract.Services.Interface;
using Core.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelViews.BloodPressureModelViews;
using Repositories.Base;
using Repositories.UOW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Service
{
    public class BloodPressureService : IBloodPressureService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BloodPressureService> _logger;

        public BloodPressureService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<BasePaginatedList<BloodPressureModel>>> GetAllAsync(int pageNumber = 1,
    int pageSize = 10,
    string? name = null,
    string? sortBy = null,
    string sortOrder = "asc",
    DateTime? createdStartDate = null,
    DateTime? createdEndDate = null,
    DateTime? updatedStartDate = null,
    DateTime? updatedEndDate = null,
    DateTime? deletedStartDate = null,
    DateTime? deletedEndDate = null,
    string? createdBy = null,
    string? updatedBy = null,
    string? deletedBy = null)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;

                var query = _unitOfWork.GetRepository<BloodPressureClassification>()
                    .Entities
                    .AsQueryable();

                // 🔍 Apply Filters
                if (!string.IsNullOrWhiteSpace(name))
                {
                    query = query.Where(b => b.Name.Contains(name));
                }
                if (createdStartDate.HasValue)
                {
                    query = query.Where(b => b.CreatedTime.Date >= createdStartDate.Value.Date);
                }
                if (createdEndDate.HasValue)
                {
                    query = query.Where(b => b.CreatedTime.Date <= createdEndDate.Value.Date);
                }
                if (updatedStartDate.HasValue)
                {
                    query = query.Where(b => b.LastUpdatedTime.Date >= updatedStartDate.Value.Date);
                }
                if (updatedEndDate.HasValue)
                {
                    query = query.Where(b => b.LastUpdatedTime.Date <= updatedEndDate.Value.Date);
                }
                if (deletedStartDate.HasValue)
                {
                    query = query.Where(b => b.DeletedTime.HasValue && b.DeletedTime.Value.Date >= deletedStartDate.Value.Date);
                }
                if (deletedEndDate.HasValue)
                {
                    query = query.Where(b => b.DeletedTime.HasValue && b.DeletedTime.Value.Date <= deletedEndDate.Value.Date);
                }
                if (!string.IsNullOrWhiteSpace(createdBy))
                {
                    query = query.Where(b => b.CreatedBy != null && b.CreatedBy.Contains(createdBy));
                }
                if (!string.IsNullOrWhiteSpace(updatedBy))
                {
                    query = query.Where(b => b.LastUpdatedBy != null && b.LastUpdatedBy.Contains(updatedBy));
                }
                if (!string.IsNullOrWhiteSpace(deletedBy))
                {
                    query = query.Where(b => b.DeletedBy != null && b.DeletedBy.Contains(deletedBy));
                }

                // 🔍 Sorting
                if (!string.IsNullOrWhiteSpace(sortBy))
                {
                    switch (sortBy.ToLower())
                    {
                        case "name":
                            query = sortOrder.ToLower() == "desc"
                                ? query.OrderByDescending(b => b.Name)
                                : query.OrderBy(b => b.Name);
                            break;
                        case "systolicmin":
                            query = sortOrder.ToLower() == "desc"
                                ? query.OrderByDescending(b => b.SystolicMin)
                                : query.OrderBy(b => b.SystolicMin);
                            break;
                        case "systolicmax":
                            query = sortOrder.ToLower() == "desc"
                                ? query.OrderByDescending(b => b.SystolicMax)
                                : query.OrderBy(b => b.SystolicMax);
                            break;
                        case "diastolicmin":
                            query = sortOrder.ToLower() == "desc"
                                ? query.OrderByDescending(b => b.DiastolicMin)
                                : query.OrderBy(b => b.DiastolicMin);
                            break;
                        case "diastolicmax":
                            query = sortOrder.ToLower() == "desc"
                                ? query.OrderByDescending(b => b.DiastolicMax)
                                : query.OrderBy(b => b.DiastolicMax);
                            break;
                        case "createdtime":
                            query = sortOrder.ToLower() == "desc"
                                ? query.OrderByDescending(b => b.CreatedTime)
                                : query.OrderBy(b => b.CreatedTime);
                            break;
                        case "lastupdatedtime":
                            query = sortOrder.ToLower() == "desc"
                                ? query.OrderByDescending(b => b.LastUpdatedTime)
                                : query.OrderBy(b => b.LastUpdatedTime);
                            break;
                        case "deletedtime":
                            query = sortOrder.ToLower() == "desc"
                                ? query.OrderByDescending(b => b.DeletedTime ?? DateTimeOffset.MinValue)
                                : query.OrderBy(b => b.DeletedTime ?? DateTimeOffset.MinValue);
                            break;
                        default:
                            query = query.OrderByDescending(b => b.CreatedTime); // Default
                            break;
                    }
                }
                else
                {
                    query = query.OrderByDescending(b => b.CreatedTime); // Default
                }

                // Paging
                int totalCount = await query.CountAsync();

                var items = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(b => new BloodPressureModel
                    {
                        Id = b.Id,
                        Name = b.Name,
                        SystolicMin = b.SystolicMin,
                        SystolicMax = b.SystolicMax,
                        DiastolicMin = b.DiastolicMin,
                        DiastolicMax = b.DiastolicMax
                    })
                    .ToListAsync();

                return new List<BasePaginatedList<BloodPressureModel>> { new BasePaginatedList<BloodPressureModel>(items, totalCount, pageNumber, pageSize) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch BloodPressure records: {Message}", ex.Message);
                throw;
            }
        
        }

        public async Task<BloodPressureModel> GetByIdAsync(int id)
        {
              return await _unitOfWork.GetRepository<BloodPressureClassification>()
                .Entities
                .Where(x => x.Id == id && !x.DeletedTime.HasValue)
                .Select(x => new BloodPressureModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    SystolicMin = x.SystolicMin,
                    SystolicMax = x.SystolicMax,
                    DiastolicMin = x.DiastolicMin,
                    DiastolicMax = x.DiastolicMax
                })
                .FirstOrDefaultAsync();
        }

        public async Task<BloodPressureClassification> CreateAsync(CreateBloodPressureModel model)
        {
            var classification = new BloodPressureClassification
            {
                Name = model.Name,
                SystolicMin = model.SystolicMin,
                SystolicMax = model.SystolicMax,
                DiastolicMin = model.DiastolicMin,
                DiastolicMax = model.DiastolicMax,
                CreatedTime = DateTimeOffset.Now,
                CreatedBy = "System"
            };

            await _unitOfWork.GetRepository<BloodPressureClassification>().InsertAsync(classification);
            await _unitOfWork.SaveAsync();
            return classification;
        }

        // Update classification if there are changes
        public async Task<bool> UpdateAsync(int id, UpdateBloodPressureModel model)
        {
            var classification = await _unitOfWork.GetRepository<BloodPressureClassification>()
                                                  .GetByIdAsync(id);

            if (classification == null)
                return false;

            bool isModified = false;

            // Check for non-null values and update only changed fields
            if (model.Name != null && classification.Name != model.Name)
            {
                classification.Name = model.Name;
                isModified = true;
            }
            if (model.SystolicMin.HasValue && classification.SystolicMin != model.SystolicMin.Value)
            {
                classification.SystolicMin = model.SystolicMin.Value;
                isModified = true;
            }
            if (model.SystolicMax.HasValue && classification.SystolicMax != model.SystolicMax.Value)
            {
                classification.SystolicMax = model.SystolicMax.Value;
                isModified = true;
            }
            if (model.DiastolicMin.HasValue && classification.DiastolicMin != model.DiastolicMin.Value)
            {
                classification.DiastolicMin = model.DiastolicMin.Value;
                isModified = true;
            }
            if (model.DiastolicMax.HasValue && classification.DiastolicMax != model.DiastolicMax.Value)
            {
                classification.DiastolicMax = model.DiastolicMax.Value;
                isModified = true;
            }

            if (!isModified)
                return true; // No changes detected

            classification.LastUpdatedTime = DateTimeOffset.Now;
            classification.LastUpdatedBy = "System";

            _unitOfWork.GetRepository<BloodPressureClassification>().Update(classification);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<BloodPressureClassification?> CheckBloodPressure(decimal systolic, decimal diastolic)
        {
            return await _unitOfWork.GetRepository<BloodPressureClassification>()
                .Entities
                .FirstOrDefaultAsync(c =>
                    systolic >= c.SystolicMin && systolic <= c.SystolicMax &&
                    diastolic >= c.DiastolicMin && diastolic <= c.DiastolicMax);
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var bloodPresure = await _unitOfWork.GetRepository<BloodPressureClassification>()
                .Entities
                .FirstOrDefaultAsync(m => m.Id == id && !m.DeletedTime.HasValue);

            if (bloodPresure == null)
            {
                return false;
            }

            bloodPresure.DeletedTime = DateTimeOffset.Now;
            bloodPresure.DeletedBy = "System";

            await _unitOfWork.GetRepository<BloodPressureClassification>().UpdateAsync(bloodPresure);
            await _unitOfWork.SaveAsync();
            return true;
        }
    }
}
