using Contract.Repositories.Entity;
using Core.Base;
using ModelViews.MetricModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Services.Interface
{
    public interface IMetricService
    {
        Task<Metric> CreateMetric(CreateMetricModel model);
        Task<BasePaginatedList<Metric>> GetAllMetrics(int pageNumber, int pageSize);
        Task<Metric> UpdateMetric(int id, UpdateMetricModel model);
        Task<bool> DeleteMetric(int id);
    }
}
