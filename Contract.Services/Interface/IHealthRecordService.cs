using Contract.Repositories.Entity;
using Core.Base;
using ModelViews.HealthRecordModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Services.Interface
{
    public interface IHealthRecordService
    {
        Task<HealthRecord> CreateHealthRecord(CreateHealthRecordModel model);
        Task<BasePaginatedList<HealthRecord>> GetAllHealthRecords(int pageNumber, int pageSize, int? patientId = null, int? bandId = null, string ghiChu = null, string sortBy = null, string sortOrder = "asc", DateTime? createdStartDate = null, DateTime? createdEndDate = null, DateTime? updatedStartDate = null, DateTime? updatedEndDate = null, DateTime? deletedStartDate = null, DateTime? deletedEndDate = null, string createdBy = null, string updatedBy = null, string deletedBy = null, bool? isActive = null);
        Task<HealthRecord> UpdateHealthRecord(int id, UpdateHealthRecordModel model);
        Task<bool> DeleteHealthRecord(int id);
        Task<HealthRecord> GetHealthRecordById(int id);
    }
}
