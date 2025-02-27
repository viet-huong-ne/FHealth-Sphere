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

        public async Task<BasePaginatedList<HealthRecord>> GetAllHealthRecords(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10; // Default page size

            var healthRecordsQuery = _unitOfWork.GetRepository<HealthRecord>()
                .Entities
                .Where(hr => !hr.DeletedTime.HasValue)
                .OrderByDescending(hr => hr.CreatedTime);

            int totalCount = await healthRecordsQuery.CountAsync();

            var healthRecords = await healthRecordsQuery
                .Include(hr => hr.Patient)
                .Include(hr => hr.Band)
                .ThenInclude(b => b.BandBrand) // Load BandBrand nếu cần
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new BasePaginatedList<HealthRecord>(healthRecords, totalCount, pageNumber, pageSize);
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