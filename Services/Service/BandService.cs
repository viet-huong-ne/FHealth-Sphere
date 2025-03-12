using Contract.Repositories.Entity;
using Core.Base;
using ModelViews.BandModelViews;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Contract.Repositories.Interface;
using Microsoft.Extensions.Logging;
using Contract.Services.Interface;

namespace Services.Service
{
    public class BandService : IBandService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BandService> _logger;

        public BandService(IUnitOfWork unitOfWork, ILogger<BandService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Band> CreateBand(CreateBandModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Image))
            {
                throw new ArgumentException("Invalid input data.");
            }

            var patientExists = await _unitOfWork.GetRepository<Account>()
                .Entities
                .AnyAsync(p => p.Id == model.PatientId);
            if (!patientExists)
            {
                throw new KeyNotFoundException($"Patient with ID {model.PatientId} not found.");
            }

            var bandBrandExists = await _unitOfWork.GetRepository<BandBrand>()
                .Entities
                .AnyAsync(b => b.Id == model.BandBrandId && !b.DeletedTime.HasValue);
            if (!bandBrandExists)
            {
                throw new KeyNotFoundException($"BandBrand with ID {model.BandBrandId} not found.");
            }
            var bandbrand = await _unitOfWork.GetRepository<BandBrand>().GetByIdAsync(model.BandBrandId);
            var band = new Band
            {
                PatientId = model.PatientId,
                Image = model.Image.Trim(),
                BandCode = model.BandCode?.Trim(),
                BandBrand = bandbrand,
                CreatedBy = "System",
                CreatedTime = DateTimeOffset.Now,
                LastUpdatedTime = DateTimeOffset.Now
            };

            await _unitOfWork.GetRepository<Band>().InsertAsync(band);
            await _unitOfWork.SaveAsync();
            return band;
        }

        public async Task<BasePaginatedList<Band>> GetAllBands(int pageNumber, int pageSize, int? patientId = null, string image = null, string bandCode = null, string sortBy = null, string sortOrder = "asc", DateTime? createdStartDate = null, DateTime? createdEndDate = null, DateTime? updatedStartDate = null, DateTime? updatedEndDate = null, DateTime? deletedStartDate = null, DateTime? deletedEndDate = null, string createdBy = null, string updatedBy = null, string deletedBy = null, bool? isActive = null)
        {
            try
            {
                _logger.LogInformation("Fetching all Bands with filters - PageNumber: {PageNumber}, PageSize: {PageSize}, PatientId: {PatientId}, Image: {Image}, SortBy: {SortBy}, SortOrder: {SortOrder}, CreatedStartDate: {CreatedStartDate}, CreatedEndDate: {CreatedEndDate}, UpdatedStartDate: {UpdatedStartDate}, UpdatedEndDate: {UpdatedEndDate}, DeletedStartDate: {DeletedStartDate}, DeletedEndDate: {DeletedEndDate}, CreatedBy: {CreatedBy}, UpdatedBy: {UpdatedBy}, DeletedBy: {DeletedBy}, IsActive: {IsActive}", pageNumber, pageSize, patientId, image, sortBy, sortOrder, createdStartDate, createdEndDate, updatedStartDate, updatedEndDate, deletedStartDate, deletedEndDate, createdBy, updatedBy, deletedBy, isActive);

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;

                var bandsQuery = _unitOfWork.GetRepository<Band>()
                    .Entities
                    .AsQueryable();

                // Apply filters
                if (patientId.HasValue)
                {
                    bandsQuery = bandsQuery.Where(b => b.PatientId == patientId.Value);
                }
                if (!string.IsNullOrWhiteSpace(image))
                {
                    bandsQuery = bandsQuery.Where(b => b.Image.Contains(image));
                }
                if (!string.IsNullOrWhiteSpace(bandCode))
                {
                    bandsQuery = bandsQuery.Where(b => b.BandCode != null && b.BandCode.Contains(bandCode));
                }
                if (createdStartDate.HasValue)
                {
                    bandsQuery = bandsQuery.Where(b => b.CreatedTime.Date >= createdStartDate.Value.Date);
                }
                if (createdEndDate.HasValue)
                {
                    bandsQuery = bandsQuery.Where(b => b.CreatedTime.Date <= createdEndDate.Value.Date);
                }
                if (updatedStartDate.HasValue)
                {
                    bandsQuery = bandsQuery.Where(b => b.LastUpdatedTime.Date >= updatedStartDate.Value.Date);
                }
                if (updatedEndDate.HasValue)
                {
                    bandsQuery = bandsQuery.Where(b => b.LastUpdatedTime.Date <= updatedEndDate.Value.Date);
                }
                if (deletedStartDate.HasValue)
                {
                    bandsQuery = bandsQuery.Where(b => b.DeletedTime.HasValue && b.DeletedTime.Value.Date >= deletedStartDate.Value.Date);
                }
                if (deletedEndDate.HasValue)
                {
                    bandsQuery = bandsQuery.Where(b => b.DeletedTime.HasValue && b.DeletedTime.Value.Date <= deletedEndDate.Value.Date);
                }
                if (!string.IsNullOrWhiteSpace(createdBy))
                {
                    bandsQuery = bandsQuery.Where(b => b.CreatedBy != null && b.CreatedBy.Contains(createdBy));
                }
                if (!string.IsNullOrWhiteSpace(updatedBy))
                {
                    bandsQuery = bandsQuery.Where(b => b.LastUpdatedBy != null && b.LastUpdatedBy.Contains(updatedBy));
                }
                if (!string.IsNullOrWhiteSpace(deletedBy))
                {
                    bandsQuery = bandsQuery.Where(b => b.DeletedBy != null && b.DeletedBy.Contains(deletedBy));
                }
                if (isActive.HasValue)
                {
                    bandsQuery = bandsQuery.Where(b => (b.DeletedTime.HasValue == !isActive.Value));
                }

                // Exclude soft-deleted records if no DeletedTime or isActive filters are applied
                if (!deletedStartDate.HasValue && !deletedEndDate.HasValue && !isActive.HasValue)
                {
                    bandsQuery = bandsQuery.Where(b => !b.DeletedTime.HasValue);
                }

                // Apply sorting
                if (!string.IsNullOrWhiteSpace(sortBy))
                {
                    switch (sortBy.ToLower())
                    {
                        case "patientid":
                            bandsQuery = sortOrder.ToLower() == "desc"
                                ? bandsQuery.OrderByDescending(b => b.PatientId)
                                : bandsQuery.OrderBy(b => b.PatientId);
                            break;
                        //case "bandbrandid":
                        //    bandsQuery = sortOrder.ToLower() == "desc"
                        //        ? bandsQuery.OrderByDescending(b => b.BandBrandId)
                        //        : bandsQuery.OrderBy(b => b.BandBrandId);
                        //    break;
                        case "createdtime":
                            bandsQuery = sortOrder.ToLower() == "desc"
                                ? bandsQuery.OrderByDescending(b => b.CreatedTime)
                                : bandsQuery.OrderBy(b => b.CreatedTime);
                            break;
                        case "lastupdatedtime":
                            bandsQuery = sortOrder.ToLower() == "desc"
                                ? bandsQuery.OrderByDescending(b => b.LastUpdatedTime)
                                : bandsQuery.OrderBy(b => b.LastUpdatedTime);
                            break;
                        case "deletedtime":
                            bandsQuery = sortOrder.ToLower() == "desc"
                                ? bandsQuery.OrderByDescending(b => b.DeletedTime ?? DateTimeOffset.MinValue)
                                : bandsQuery.OrderBy(b => b.DeletedTime ?? DateTimeOffset.MinValue);
                            break;
                        case "image":
                            bandsQuery = sortOrder.ToLower() == "desc"
                                ? bandsQuery.OrderByDescending(b => b.Image)
                                : bandsQuery.OrderBy(b => b.Image);
                            break;
                        case "bandcode":
                            bandsQuery = sortOrder.ToLower() == "desc"
                                ? bandsQuery.OrderByDescending(b => b.BandCode)
                                : bandsQuery.OrderBy(b => b.BandCode);
                            break;
                        default:
                            bandsQuery = bandsQuery.OrderByDescending(b => b.CreatedTime); // Default
                            break;
                    }
                }
                else
                {
                    bandsQuery = bandsQuery.OrderByDescending(b => b.CreatedTime); // Default
                }

                int totalCount = await bandsQuery.CountAsync();

                var bands = await bandsQuery
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new BasePaginatedList<Band>(bands, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch Bands: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<Band> GetBandById(int id)
        {
            try
            {
                _logger.LogInformation("Attempting to get Band with ID: {Id}", id);

                var band = await _unitOfWork.GetRepository<Band>()
                    .Entities
                    .Where(b => b.Id == id && !b.DeletedTime.HasValue)
                    .FirstOrDefaultAsync();

                if (band == null)
                {
                    throw new KeyNotFoundException($"Band with ID {id} not found or already deleted.");
                }

                return band;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Band with ID {Id}: {Message}", id, ex.Message);
                throw;
            }
        }

        public async Task<Band> UpdateBand(int id, UpdateBandModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model), "Update data is required.");
            }

            var band = await _unitOfWork.GetRepository<Band>().Entities.FirstOrDefaultAsync(b => b.Id == id && !b.DeletedTime.HasValue);
            if (band == null)
            {
                throw new KeyNotFoundException($"Band with ID {id} not found or already deleted.");
            }

            if (model.PatientId.HasValue)
            {
                var patientExists = await _unitOfWork.GetRepository<Account>().Entities.AnyAsync(p => p.Id == model.PatientId.Value);
                if (!patientExists)
                {
                    throw new KeyNotFoundException($"Patient with ID {model.PatientId.Value} not found.");
                }
                band.PatientId = model.PatientId.Value;
            }

            //if (model.BandBrandId.HasValue)
            //{
            //    var bandBrandExists = await _unitOfWork.GetRepository<BandBrand>().Entities.AnyAsync(b => b.Id == model.BandBrandId.Value && !b.DeletedTime.HasValue);
            //    if (!bandBrandExists)
            //    {
            //        throw new KeyNotFoundException($"BandBrand with ID {model.BandBrandId.Value} not found.");
            //    }
            //    band.BandBrandId = model.BandBrandId.Value;
            //}

            if (!string.IsNullOrWhiteSpace(model.Image))
            {
                band.Image = model.Image.Trim();
            }
            if (!string.IsNullOrWhiteSpace(model.BandCode)) band.BandCode = model.BandCode.Trim();

            band.LastUpdatedTime = DateTimeOffset.Now;
            band.LastUpdatedBy = "System";

            await _unitOfWork.GetRepository<Band>().UpdateAsync(band);
            await _unitOfWork.SaveAsync();
            return band;
        }

        public async Task<bool> DeleteBand(int id)
        {
            var band = await _unitOfWork.GetRepository<Band>().Entities.FirstOrDefaultAsync(b => b.Id == id && !b.DeletedTime.HasValue);
            if (band == null)
            {
                return false;
            }

            band.DeletedTime = DateTimeOffset.Now;
            band.DeletedBy = "System";

            await _unitOfWork.GetRepository<Band>().UpdateAsync(band);
            await _unitOfWork.SaveAsync();
            return true;
        }


    }
}