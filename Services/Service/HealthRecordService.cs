using Contract.Repositories.Entity;
using Core.Base;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Contract.Repositories.Interface;
using Contract.Services.Interface;
using ModelViews.HealthRecordModelViews;
using Microsoft.Extensions.Logging;

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

        public async Task<HealthRecord> CreateHealthRecord(CreateHealthRecordModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model), "HealthRecord data is required.");
            }

            if (string.IsNullOrWhiteSpace(model.GhiChu))
            {
                throw new ArgumentException("GhiChu is required.", nameof(model.GhiChu));
            }

            // Kiểm tra PatientId tồn tại
            var patientExists = await _unitOfWork.GetRepository<Account>()
                .Entities
                .AnyAsync(a => a.Id == model.PatientId && !a.DeletedTime.HasValue);
            if (!patientExists)
            {
                throw new KeyNotFoundException($"Patient with ID {model.PatientId} not found.");
            }

            // Kiểm tra BandId tồn tại
            var bandExists = await _unitOfWork.GetRepository<Band>()
                .Entities
                .AnyAsync(b => b.Id == model.BandId && !b.DeletedTime.HasValue);
            if (!bandExists)
            {
                throw new KeyNotFoundException($"Band with ID {model.BandId} not found.");
            }

            var healthRecord = new HealthRecord
            {
                PatientId = model.PatientId,
                BandId = model.BandId,
                GhiChu = model.GhiChu.Trim(),
                CreatedBy = "System", // Nên lấy từ context người dùng hiện tại nếu có
                CreatedTime = DateTimeOffset.Now,
                LastUpdatedTime = DateTimeOffset.Now
            };

            await _unitOfWork.GetRepository<HealthRecord>().InsertAsync(healthRecord);
            await _unitOfWork.SaveAsync();
            return healthRecord;
        }

        public async Task<BasePaginatedList<HealthRecord>> GetAllHealthRecords(int pageNumber, int pageSize, int? patientId = null, int? bandId = null, string ghiChu = null, string sortBy = null, string sortOrder = "asc", DateTime? createdStartDate = null, DateTime? createdEndDate = null, DateTime? updatedStartDate = null, DateTime? updatedEndDate = null, DateTime? deletedStartDate = null, DateTime? deletedEndDate = null, string createdBy = null, string updatedBy = null, string deletedBy = null, bool? isActive = null)
        {
            try
            {
                _logger.LogInformation("Fetching all HealthRecords with filters - PageNumber: {PageNumber}, PageSize: {PageSize}, PatientId: {PatientId}, BandId: {BandId}, GhiChu: {GhiChu}, SortBy: {SortBy}, SortOrder: {SortOrder}, CreatedStartDate: {CreatedStartDate}, CreatedEndDate: {CreatedEndDate}, UpdatedStartDate: {UpdatedStartDate}, UpdatedEndDate: {UpdatedEndDate}, DeletedStartDate: {DeletedStartDate}, DeletedEndDate: {DeletedEndDate}, CreatedBy: {CreatedBy}, UpdatedBy: {UpdatedBy}, DeletedBy: {DeletedBy}, IsActive: {IsActive}", pageNumber, pageSize, patientId, bandId, ghiChu, sortBy, sortOrder, createdStartDate, createdEndDate, updatedStartDate, updatedEndDate, deletedStartDate, deletedEndDate, createdBy, updatedBy, deletedBy, isActive);

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;

                var recordsQuery = _unitOfWork.GetRepository<HealthRecord>()
                    .Entities
                    .AsQueryable();

                // Áp dụng bộ lọc
                if (patientId.HasValue)
                {
                    recordsQuery = recordsQuery.Where(hr => hr.PatientId == patientId.Value);
                }
                if (bandId.HasValue)
                {
                    recordsQuery = recordsQuery.Where(hr => hr.BandId == bandId.Value);
                }
                if (!string.IsNullOrWhiteSpace(ghiChu))
                {
                    recordsQuery = recordsQuery.Where(hr => hr.GhiChu != null && hr.GhiChu.Contains(ghiChu));
                }
                if (createdStartDate.HasValue)
                {
                    recordsQuery = recordsQuery.Where(hr => hr.CreatedTime.Date >= createdStartDate.Value.Date);
                }
                if (createdEndDate.HasValue)
                {
                    recordsQuery = recordsQuery.Where(hr => hr.CreatedTime.Date <= createdEndDate.Value.Date);
                }
                if (updatedStartDate.HasValue)
                {
                    recordsQuery = recordsQuery.Where(hr => hr.LastUpdatedTime.Date >= updatedStartDate.Value.Date);
                }
                if (updatedEndDate.HasValue)
                {
                    recordsQuery = recordsQuery.Where(hr => hr.LastUpdatedTime.Date <= updatedEndDate.Value.Date);
                }
                if (deletedStartDate.HasValue)
                {
                    recordsQuery = recordsQuery.Where(hr => hr.DeletedTime.HasValue && hr.DeletedTime.Value.Date >= deletedStartDate.Value.Date);
                }
                if (deletedEndDate.HasValue)
                {
                    recordsQuery = recordsQuery.Where(hr => hr.DeletedTime.HasValue && hr.DeletedTime.Value.Date <= deletedEndDate.Value.Date);
                }
                if (!string.IsNullOrWhiteSpace(createdBy))
                {
                    recordsQuery = recordsQuery.Where(hr => hr.CreatedBy != null && hr.CreatedBy.Contains(createdBy));
                }
                if (!string.IsNullOrWhiteSpace(updatedBy))
                {
                    recordsQuery = recordsQuery.Where(hr => hr.LastUpdatedBy != null && hr.LastUpdatedBy.Contains(updatedBy));
                }
                if (!string.IsNullOrWhiteSpace(deletedBy))
                {
                    recordsQuery = recordsQuery.Where(hr => hr.DeletedBy != null && hr.DeletedBy.Contains(deletedBy));
                }
                if (isActive.HasValue)
                {
                    recordsQuery = recordsQuery.Where(hr => (hr.DeletedTime.HasValue == !isActive.Value));
                }

                // Loại bỏ các bản ghi bị soft delete nếu không có bộ lọc DeletedTime hoặc isActive
                if (!deletedStartDate.HasValue && !deletedEndDate.HasValue && !isActive.HasValue)
                {
                    recordsQuery = recordsQuery.Where(hr => !hr.DeletedTime.HasValue);
                }

                // Áp dụng sắp xếp
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
                        case "lastupdatedtime":
                            recordsQuery = sortOrder.ToLower() == "desc"
                                ? recordsQuery.OrderByDescending(hr => hr.LastUpdatedTime)
                                : recordsQuery.OrderBy(hr => hr.LastUpdatedTime);
                            break;
                        case "deletedtime":
                            recordsQuery = sortOrder.ToLower() == "desc"
                                ? recordsQuery.OrderByDescending(hr => hr.DeletedTime ?? DateTimeOffset.MinValue)
                                : recordsQuery.OrderBy(hr => hr.DeletedTime ?? DateTimeOffset.MinValue);
                            break;
                        case "ghichu":
                            recordsQuery = sortOrder.ToLower() == "desc"
                                ? recordsQuery.OrderByDescending(hr => hr.GhiChu)
                                : recordsQuery.OrderBy(hr => hr.GhiChu);
                            break;
                        default:
                            recordsQuery = recordsQuery.OrderByDescending(hr => hr.CreatedTime); // Mặc định
                            break;
                    }
                }
                else
                {
                    recordsQuery = recordsQuery.OrderByDescending(hr => hr.CreatedTime); // Mặc định
                }

                int totalCount = await recordsQuery.CountAsync();

                var records = await recordsQuery
                    .Include(hr => hr.Patient)
                    .Include(hr => hr.Band)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new BasePaginatedList<HealthRecord>(records, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch HealthRecords: {Message}", ex.Message);
                throw;
            }
        }
        public async Task<HealthRecord> GetHealthRecordById(int id)
        {
            try
            {
                _logger.LogInformation("Attempting to get HealthRecord with ID: {Id}", id);

                var healthRecord = await _unitOfWork.GetRepository<HealthRecord>()
                    .Entities
                    .Where(hr => hr.Id == id && !hr.DeletedTime.HasValue)
                    .Include(hr => hr.Patient)
                    .Include(hr => hr.Band)
                    .FirstOrDefaultAsync();

                if (healthRecord == null)
                {
                    throw new KeyNotFoundException($"HealthRecord with ID {id} not found or already deleted.");
                }

                return healthRecord;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get HealthRecord with ID {Id}: {Message}", id, ex.Message);
                throw;
            }
        }

        public async Task<HealthRecord> UpdateHealthRecord(int id, UpdateHealthRecordModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model), "Update data is required.");
            }

            var healthRecord = await _unitOfWork.GetRepository<HealthRecord>()
                .Entities
                .FirstOrDefaultAsync(hr => hr.Id == id && !hr.DeletedTime.HasValue);

            if (healthRecord == null)
            {
                throw new KeyNotFoundException($"HealthRecord with ID {id} not found or already deleted.");
            }

            // Kiểm tra PatientId nếu được cập nhật
            if (model.PatientId.HasValue)
            {
                var patientExists = await _unitOfWork.GetRepository<Account>()
                    .Entities
                    .AnyAsync(a => a.Id == model.PatientId.Value && !a.DeletedTime.HasValue);
                if (!patientExists)
                {
                    throw new KeyNotFoundException($"Patient with ID {model.PatientId.Value} not found.");
                }
                healthRecord.PatientId = model.PatientId.Value;
            }

            // Kiểm tra BandId nếu được cập nhật
            if (model.BandId.HasValue)
            {
                var bandExists = await _unitOfWork.GetRepository<Band>()
                    .Entities
                    .AnyAsync(b => b.Id == model.BandId.Value && !b.DeletedTime.HasValue);
                if (!bandExists)
                {
                    throw new KeyNotFoundException($"Band with ID {model.BandId.Value} not found.");
                }
                healthRecord.BandId = model.BandId.Value;
            }

            if (!string.IsNullOrWhiteSpace(model.GhiChu))
            {
                healthRecord.GhiChu = model.GhiChu.Trim();
            }

            healthRecord.LastUpdatedTime = DateTimeOffset.Now;
            healthRecord.LastUpdatedBy = "System"; // Nên lấy từ context người dùng hiện tại nếu có

            await _unitOfWork.GetRepository<HealthRecord>().UpdateAsync(healthRecord);
            await _unitOfWork.SaveAsync();
            return healthRecord;
        }

        public async Task<bool> DeleteHealthRecord(int id)
        {
            var healthRecord = await _unitOfWork.GetRepository<HealthRecord>()
                .Entities
                .FirstOrDefaultAsync(hr => hr.Id == id && !hr.DeletedTime.HasValue);

            if (healthRecord == null)
            {
                return false;
            }

            healthRecord.DeletedTime = DateTimeOffset.Now;
            healthRecord.DeletedBy = "System"; // Nên lấy từ context người dùng hiện tại nếu có

            await _unitOfWork.GetRepository<HealthRecord>().UpdateAsync(healthRecord);
            await _unitOfWork.SaveAsync();
            return true;
        }

    }
}