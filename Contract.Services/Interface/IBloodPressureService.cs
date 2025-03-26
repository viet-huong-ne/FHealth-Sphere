using Contract.Repositories.Entity;
using Core.Base;
using ModelViews.BloodPressureModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Services.Interface
{
    public interface IBloodPressureService
    {
        Task<IEnumerable<BasePaginatedList<BloodPressureModel>>> GetAllAsync(int pageNumber, int pageSize, string name = null, string sortBy = null, string sortOrder = "asc", DateTime? createdStartDate = null, DateTime? createdEndDate = null, DateTime? updatedStartDate = null, DateTime? updatedEndDate = null, DateTime? deletedStartDate = null, DateTime? deletedEndDate = null, string createdBy = null, string updatedBy = null, string deletedBy = null);
        Task<BloodPressureModel> GetByIdAsync(int id);
        Task<BloodPressureClassification> CreateAsync(CreateBloodPressureModel classification);
        Task<bool> UpdateAsync(int id, UpdateBloodPressureModel classification);
        Task<bool> DeleteAsync(int id);
        Task<BloodPressureClassification?> CheckBloodPressure(decimal systolic, decimal diastolic);
    }
}
