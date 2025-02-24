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
        Task<BasePaginatedList<MetricGroup>> GetAllMetricGroups(int pageNumber, int pageSize);
        Task<MetricGroup> UpdateMetricGroup(int id, UpdateMetricGroupModel model);
        Task<bool> DeleteMetricGroup(int id);
    }
}
