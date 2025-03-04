using Contract.Repositories.Entity;
using Core.Base;
using ModelViews.RecordMetricItemModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Services.Interface
{
    public interface IRecordMetricItemService
    {
        Task<RecordMetricItem> CreateRecordMetricItem(CreateRecordMetricItemModel model);
        Task<BasePaginatedList<RecordMetricItem>> GetAllRecordMetricItems(int pageNumber, int pageSize, int? recordId = null, int? healthRecordId = null, int? metricId = null, string value = null, string type = null, string sortBy = null, string sortOrder = "asc", DateTime? createdStartDate = null, DateTime? createdEndDate = null, DateTime? updatedStartDate = null, DateTime? updatedEndDate = null, DateTime? deletedStartDate = null, DateTime? deletedEndDate = null, string createdBy = null, string updatedBy = null, string deletedBy = null, bool? isActive = null);
        Task<RecordMetricItem> UpdateRecordMetricItem(int id, UpdateRecordMetricItemModel model);
        Task<bool> DeleteRecordMetricItem(int id);
        Task<RecordMetricItem> GetRecordMetricItemById(int id);
    }
}
