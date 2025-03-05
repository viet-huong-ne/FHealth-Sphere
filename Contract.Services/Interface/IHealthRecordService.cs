using Contract.Repositories.Entity;
using Core.Base;
using ModelViews.HealthRecordModelViews;

namespace Contract.Services.Interface
{
    public interface IHealthRecordService
    {
        Task<HealthRecord> CreateHealthRecordCombined(CreateHealthRecordCombinedModel model);
        Task<BasePaginatedList<HealthRecord>> GetAllHealthRecordsCombined(int pageNumber, int pageSize, int? patientId = null, int? bandId = null, string ghiChu = null, string sortBy = null, string sortOrder = "asc", DateTime? createdStartDate = null, DateTime? createdEndDate = null);
        Task<HealthRecord> GetHealthRecordCombinedById(int id);
        Task<HealthRecord> UpdateHealthRecordCombined(int id, UpdateHealthRecordCombinedModel model);
        Task<bool> DeleteHealthRecord(int id);
    }
}