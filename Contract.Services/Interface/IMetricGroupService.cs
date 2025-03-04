using Contract.Repositories.Entity;
using Core.Base;
using ModelViews.MetricGroupModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Services.Interface
{
    public interface IMetricGroupService
    {
        Task<MetricGroup> CreateMetricGroup(CreateMetricGroupModel model);
        Task<BasePaginatedList<MetricGroup>> GetAllMetricGroups(int pageNumber, int pageSize, string name = null, string sortBy = null, string sortOrder = "asc", DateTime? createdStartDate = null, DateTime? createdEndDate = null, DateTime? updatedStartDate = null, DateTime? updatedEndDate = null, DateTime? deletedStartDate = null, DateTime? deletedEndDate = null, string createdBy = null, string updatedBy = null, string deletedBy = null, bool? isActive = null);
        Task<MetricGroup> UpdateMetricGroup(int id, UpdateMetricGroupModel model);
        Task<bool> DeleteMetricGroup(int id);
        Task<MetricGroup> GetMetricGroupById(int id);
    }
}
