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
        Task<BasePaginatedList<Metric>> GetAllMetrics(int pageNumber, int pageSize, string name = null, string unit = null, int? metricGroupId = null, string sortBy = null, string sortOrder = "asc", DateTime? createdStartDate = null, DateTime? createdEndDate = null, DateTime? updatedStartDate = null, DateTime? updatedEndDate = null, DateTime? deletedStartDate = null, DateTime? deletedEndDate = null, string createdBy = null, string updatedBy = null, string deletedBy = null, bool? isActive = null);
        Task<Metric> UpdateMetric(int id, UpdateMetricModel model);
        Task<bool> DeleteMetric(int id);
        Task<Metric> GetMetricById(int id);
        Task SendNotificationAsync(string title, string body, string token);
    }
}
