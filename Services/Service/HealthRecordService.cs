using Contract.Repositories.Entity;
using Core.Base;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Contract.Repositories.Interface;
using Contract.Services.Interface;
using ModelViews.HealthRecordModelViews;
using Microsoft.Extensions.Logging;
using ModelViews.RecordMetricItemModelViews;
using Microsoft.Identity.Client;
using Core.Store;
using Core.Utils;

namespace FHealthSphere.Services.Services
{
    public class HealthRecordService : IHealthRecordService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<HealthRecordService> _logger;

        public HealthRecordService(IUnitOfWork unitOfWork, ILogger<HealthRecordService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HealthRecord> CreateHealthRecordCombined(CreateHealthRecordCombinedModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model), "HealthRecord data is required.");

            if (string.IsNullOrWhiteSpace(model.GhiChu))
                throw new ArgumentException("GhiChu is required.", nameof(model.GhiChu));

            // Kiểm tra PatientId
            var patientExists = await _unitOfWork.GetRepository<Account>()
                .Entities.AnyAsync(a => a.Id == model.PatientId && !a.DeletedTime.HasValue);
            if (!patientExists)
                throw new KeyNotFoundException($"Patient with ID {model.PatientId} not found.");

            // Kiểm tra BandId
            var bandExists = await _unitOfWork.GetRepository<Band>()
                .Entities.AnyAsync(b => b.Id == model.BandId && !b.DeletedTime.HasValue);
            if (!bandExists)
                throw new KeyNotFoundException($"Band with ID {model.BandId} not found.");

            var healthRecord = new HealthRecord
            {
                PatientId = model.PatientId,
                BandId = model.BandId,
                GhiChu = model.GhiChu.Trim(),
                CreatedBy = "System",
                CreatedTime = DateTimeOffset.Now,
                LastUpdatedTime = DateTimeOffset.Now,
                RecordMetricItems = new List<RecordMetricItem>()
            };

            // Thêm RecordMetricItems nếu có
            if (model.RecordMetricItems != null && model.RecordMetricItems.Any())
            {
                foreach (var item in model.RecordMetricItems)
                {
                    var metricExists = await _unitOfWork.GetRepository<Metric>()
                        .Entities.AnyAsync(m => m.Id == item.MetricId && !m.DeletedTime.HasValue);
                    if (!metricExists)
                        throw new KeyNotFoundException($"Metric with ID {item.MetricId} not found.");

                    healthRecord.RecordMetricItems.Add(new RecordMetricItem
                    {
                        MetricId = item.MetricId,
                        Value = item.Value,
                        Type = item.Type?.Trim(),
                        CreatedBy = "System",
                        CreatedTime = DateTimeOffset.Now,
                        LastUpdatedTime = DateTimeOffset.Now
                    });
                }
            }

            await _unitOfWork.GetRepository<HealthRecord>().InsertAsync(healthRecord);
            await _unitOfWork.SaveAsync();
            return healthRecord;
        }

        public async Task<BaseResponse<DailyHealthRecordModel>> GetDailyAverage(DateTime date, int? patientId)
        {
            try
            {
                var startDate = date.Date;
                var endDate = startDate.AddDays(1);

                // Query data
                var query = _unitOfWork.GetRepository<RecordMetricItem>()
                    .Entities
                    .Include(r => r.HealthRecord)
                    .Where(r => r.CreatedTime >= startDate && r.CreatedTime < endDate);

                if (patientId.HasValue)
                {
                    query = query.Where(r => r.HealthRecord.PatientId == patientId.Value);
                }

                var dailyMetrics = await query
                    .GroupBy(r => r.MetricId)
                    .Select(g => new RecordMetricItemModel
                    {
                        MetricId = g.Key.Value,
                        AverageValue = g.Average(x => x.Value)
                    })
                    .ToListAsync();

                var result = new DailyHealthRecordModel
                {
                    Date = startDate.ToString("yyyy-MM-dd"),
                    PatientId = patientId,
                    Metrics = dailyMetrics
                };

                return BaseResponse<DailyHealthRecordModel>.OkResponse(result);
            }
            catch (Exception ex)
            {
                return new BaseResponse<DailyHealthRecordModel>(StatusCodeHelper.ServerError, StatusCodeHelper.ServerError.Name(), $"Internal server error: {ex.Message}");
            }
        }

        // Weekly Average
        public async Task<BaseResponse<WeeklyMetricViewModel>> GetWeeklyAverage(DateTime startDate, int? patientId)
        {
            try
            {
                var startOfWeek = startDate.Date;
                var endOfWeek = startOfWeek.AddDays(7);

                var query = _unitOfWork.GetRepository<RecordMetricItem>()
                    .Entities
                    .Include(r => r.HealthRecord)
                    .Where(r => r.CreatedTime >= startOfWeek && r.CreatedTime < endOfWeek);

                if (patientId.HasValue)
                {
                    query = query.Where(r => r.HealthRecord.PatientId == patientId.Value);
                }

                var weeklyData = await query
                    .GroupBy(r => new { r.MetricId, Date = r.CreatedTime.Date })
                    .Select(g => new
                    {
                        g.Key.Date,
                        g.Key.MetricId,
                        AverageValue = g.Average(x => x.Value)
                    })
                    .OrderBy(g => g.Date)
                    .ToListAsync();

                var groupedResult = weeklyData
                    .GroupBy(x => x.Date)
                    .Select(day => new DailyHealthRecordModel
                    {
                        Date = day.Key.ToString("yyyy-MM-dd"),
                        PatientId = patientId,
                        Metrics = day.Select(m => new RecordMetricItemModel
                        {
                            MetricId = m.MetricId.Value,
                            AverageValue = m.AverageValue
                        }).ToList()
                    })
                    .ToList();

                decimal? weeklyAverage = weeklyData
                    .Where(m => m.AverageValue.HasValue)
                    .Average(m => m.AverageValue);
                var result = new WeeklyMetricViewModel
                {
                    WeekStartDate = startOfWeek.ToString("yyyy-MM-dd"),
                    PatientId = patientId,
                    WeeklyAverage = weeklyAverage,
                    DailyAverages = groupedResult
                };

                return BaseResponse<WeeklyMetricViewModel>.OkResponse(result);
            }
            catch (Exception ex)
            {
                return new BaseResponse<WeeklyMetricViewModel>(StatusCodeHelper.ServerError, StatusCodeHelper.ServerError.Name(), $"Internal server error: {ex.Message}");
            }
        }
        public async Task<BasePaginatedList<HealthRecord>> GetAllHealthRecordsCombined(int pageNumber, int pageSize, int? patientId = null, int? bandId = null, string ghiChu = null, string sortBy = null, string sortOrder = "asc", DateTime? createdStartDate = null, DateTime? createdEndDate = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var recordsQuery = _unitOfWork.GetRepository<HealthRecord>()
                .Entities
                .Where(hr => !hr.DeletedTime.HasValue)
                .Include(hr => hr.RecordMetricItems)
                .AsQueryable();

            // Áp dụng bộ lọc
            if (patientId.HasValue)
                recordsQuery = recordsQuery.Where(hr => hr.PatientId == patientId.Value);
            if (bandId.HasValue)
                recordsQuery = recordsQuery.Where(hr => hr.BandId == bandId.Value);
            if (!string.IsNullOrWhiteSpace(ghiChu))
                recordsQuery = recordsQuery.Where(hr => hr.GhiChu != null && hr.GhiChu.Contains(ghiChu));
            if (createdStartDate.HasValue)
                recordsQuery = recordsQuery.Where(hr => hr.CreatedTime.Date >= createdStartDate.Value.Date);
            if (createdEndDate.HasValue)
                recordsQuery = recordsQuery.Where(hr => hr.CreatedTime.Date <= createdEndDate.Value.Date);

            // Sắp xếp
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "patientid":
                        recordsQuery = sortOrder.ToLower() == "desc"
                            ? recordsQuery.OrderByDescending(hr => hr.PatientId)
                            : recordsQuery.OrderBy(hr => hr.PatientId);
                        break;
                    case "bandid":
                        recordsQuery = sortOrder.ToLower() == "desc"
                            ? recordsQuery.OrderByDescending(hr => hr.BandId)
                            : recordsQuery.OrderBy(hr => hr.BandId);
                        break;
                    case "createdtime":
                        recordsQuery = sortOrder.ToLower() == "desc"
                            ? recordsQuery.OrderByDescending(hr => hr.CreatedTime)
                            : recordsQuery.OrderBy(hr => hr.CreatedTime);
                        break;
                    default:
                        recordsQuery = recordsQuery.OrderByDescending(hr => hr.CreatedTime);
                        break;
                }
            }
            else
            {
                recordsQuery = recordsQuery.OrderByDescending(hr => hr.CreatedTime);
            }

            int totalCount = await recordsQuery.CountAsync();
            var records = await recordsQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new BasePaginatedList<HealthRecord>(records, totalCount, pageNumber, pageSize);
        }

        public async Task<HealthRecord> GetHealthRecordCombinedById(int id)
        {
            var healthRecord = await _unitOfWork.GetRepository<HealthRecord>()
                .Entities
                .Where(hr => hr.Id == id && !hr.DeletedTime.HasValue)
                .Include(hr => hr.RecordMetricItems)
                .FirstOrDefaultAsync();

            if (healthRecord == null)
                throw new KeyNotFoundException($"HealthRecord with ID {id} not found or already deleted.");

            return healthRecord;
        }

        public async Task<HealthRecord> UpdateHealthRecordCombined(int id, UpdateHealthRecordCombinedModel model)
        {
            var healthRecord = await _unitOfWork.GetRepository<HealthRecord>()
                .Entities
                .Include(hr => hr.RecordMetricItems)
                .FirstOrDefaultAsync(hr => hr.Id == id && !hr.DeletedTime.HasValue);

            if (healthRecord == null)
                throw new KeyNotFoundException($"HealthRecord with ID {id} not found or already deleted.");

            if (model.PatientId.HasValue)
            {
                var patientExists = await _unitOfWork.GetRepository<Account>()
                    .Entities.AnyAsync(a => a.Id == model.PatientId.Value && !a.DeletedTime.HasValue);
                if (!patientExists)
                    throw new KeyNotFoundException($"Patient with ID {model.PatientId.Value} not found.");
                healthRecord.PatientId = model.PatientId.Value;
            }

            if (model.BandId.HasValue)
            {
                var bandExists = await _unitOfWork.GetRepository<Band>()
                    .Entities.AnyAsync(b => b.Id == model.BandId.Value && !b.DeletedTime.HasValue);
                if (!bandExists)
                    throw new KeyNotFoundException($"Band with ID {model.BandId.Value} not found.");
                healthRecord.BandId = model.BandId.Value;
            }

            if (!string.IsNullOrWhiteSpace(model.GhiChu))
                healthRecord.GhiChu = model.GhiChu.Trim();

            // Cập nhật RecordMetricItems
            if (model.RecordMetricItems != null && model.RecordMetricItems.Any())
            {
                healthRecord.RecordMetricItems.Clear(); // Xóa các item cũ
                foreach (var item in model.RecordMetricItems)
                {
                    var metricExists = await _unitOfWork.GetRepository<Metric>()
                        .Entities.AnyAsync(m => m.Id == item.MetricId && !m.DeletedTime.HasValue);
                    if (!metricExists)
                        throw new KeyNotFoundException($"Metric with ID {item.MetricId} not found.");

                    healthRecord.RecordMetricItems.Add(new RecordMetricItem
                    {
                        MetricId = item.MetricId,
                        Value = item.Value,
                        Type = item.Type?.Trim(),
                        CreatedBy = "System",
                        CreatedTime = DateTimeOffset.Now,
                        LastUpdatedTime = DateTimeOffset.Now
                    });
                }
            }

            healthRecord.LastUpdatedTime = DateTimeOffset.Now;
            healthRecord.LastUpdatedBy = "System";

            await _unitOfWork.GetRepository<HealthRecord>().UpdateAsync(healthRecord);
            await _unitOfWork.SaveAsync();
            return healthRecord;
        }

        public async Task<bool> DeleteHealthRecord(int id)
        {
            var healthRecord = await _unitOfWork.GetRepository<HealthRecord>()
                .Entities
                .Include(hr => hr.RecordMetricItems) // Bao gồm RecordMetricItems để truy cập
                .FirstOrDefaultAsync(hr => hr.Id == id && !hr.DeletedTime.HasValue);

            if (healthRecord == null)
            {
                return false;
            }

            // Xóa mềm HealthRecord
            healthRecord.DeletedTime = DateTimeOffset.Now;
            healthRecord.DeletedBy = "System";

            // Xóa mềm tất cả RecordMetricItems liên quan
            foreach (var item in healthRecord.RecordMetricItems.Where(ri => !ri.DeletedTime.HasValue))
            {
                item.DeletedTime = DateTimeOffset.Now;
                item.DeletedBy = "System";
            }

            await _unitOfWork.GetRepository<HealthRecord>().UpdateAsync(healthRecord);
            await _unitOfWork.SaveAsync();
            return true;
        }
    }
}