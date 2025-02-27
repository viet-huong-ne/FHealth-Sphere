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
        Task<BasePaginatedList<RecordMetricItem>> GetAllRecordMetricItems(int pageNumber, int pageSize);
        Task<RecordMetricItem> UpdateRecordMetricItem(int id, UpdateRecordMetricItemModel model);
        Task<bool> DeleteRecordMetricItem(int id);
        Task<HealthRecord> GetHealthRecordById(int id);
        Task<RecordMetricItem> GetRecordMetricItemById(int id);
    }
}
