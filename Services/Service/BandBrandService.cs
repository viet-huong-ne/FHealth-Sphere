using Contract.Repositories.Entity;
using Contract.Repositories.Interface;
using Contract.Services.Interface;
using Core.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelViews.BandBrandModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Service
{
    public class BandBrandService : IBandBrandService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BandBrandService> _logger;

        public BandBrandService(ILogger<BandBrandService> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<BandBrand> CreateBandBrand(CreateBandBrandModel model)
        {
            try
            {
                _logger.LogInformation("Attempting to create BandBrand with name: {Name}", model.Name);
                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    throw new ArgumentException("Name is required and cannot be empty or whitespace.");
                }

                if (model.Name.Length > 255)
                {
                    throw new ArgumentException("Name cannot exceed 255 characters.");
                }

                var existingBandBrand = await _unitOfWork.GetRepository<BandBrand>()
                    .Entities
                    .FirstOrDefaultAsync(b => b.NameBrand.ToLower() == model.Name.ToLower() && !b.DeletedTime.HasValue);

                if (existingBandBrand != null)
                {
                    throw new InvalidOperationException("A BandBrand with this name already exists.");
                }

                var bandBrand = new BandBrand
                {
                    NameBrand = model.Name.Trim(),
                    CreatedBy = "System",
                    CreatedTime = DateTimeOffset.Now,
                    LastUpdatedTime = DateTimeOffset.Now
                };

                await _unitOfWork.GetRepository<BandBrand>().InsertAsync(bandBrand);
                await _unitOfWork.SaveAsync();
                _logger.LogInformation("Successfully created BandBrand with ID: {Id}", bandBrand.Id);
                return bandBrand;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create BandBrand: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<BasePaginatedList<BandBrand>> GetAllBandBrands(int pageNumber, int pageSize, string name = null, string sortBy = null, string sortOrder = "asc", DateTime? createdStartDate = null, DateTime? createdEndDate = null, DateTime? updatedStartDate = null, DateTime? updatedEndDate = null, DateTime? deletedStartDate = null, DateTime? deletedEndDate = null, string createdBy = null, string updatedBy = null, string deletedBy = null, bool? isActive = null)
        {
            try
            {
                _logger.LogInformation("Fetching all BandBrands with filters - PageNumber: {PageNumber}, PageSize: {PageSize}, Name: {Name}, SortBy: {SortBy}, SortOrder: {SortOrder}, CreatedStartDate: {CreatedStartDate}, CreatedEndDate: {CreatedEndDate}, UpdatedStartDate: {UpdatedStartDate}, UpdatedEndDate: {UpdatedEndDate}, DeletedStartDate: {DeletedStartDate}, DeletedEndDate: {DeletedEndDate}, CreatedBy: {CreatedBy}, UpdatedBy: {UpdatedBy}, DeletedBy: {DeletedBy}, IsActive: {IsActive}", pageNumber, pageSize, name, sortBy, sortOrder, createdStartDate, createdEndDate, updatedStartDate, updatedEndDate, deletedStartDate, deletedEndDate, createdBy, updatedBy, deletedBy, isActive);

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;

                var bandsQuery = _unitOfWork.GetRepository<BandBrand>()
                    .Entities
                    .AsQueryable();

                // Áp dụng bộ lọc
                if (!string.IsNullOrWhiteSpace(name))
                {
                    bandsQuery = bandsQuery.Where(b => b.NameBrand.Contains(name));
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

                // Loại bỏ các bản ghi bị soft delete nếu không có bộ lọc DeletedTime hoặc isActive
                if (!deletedStartDate.HasValue && !deletedEndDate.HasValue && !isActive.HasValue)
                {
                    bandsQuery = bandsQuery.Where(b => !b.DeletedTime.HasValue);
                }

                // Áp dụng sắp xếp
                if (!string.IsNullOrWhiteSpace(sortBy))
                {
                    switch (sortBy.ToLower())
                    {
                        case "namebrand":
                            bandsQuery = sortOrder.ToLower() == "desc"
                                ? bandsQuery.OrderByDescending(b => b.NameBrand)
                                : bandsQuery.OrderBy(b => b.NameBrand);
                            break;
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
                        default:
                            bandsQuery = bandsQuery.OrderByDescending(b => b.CreatedTime); // Mặc định
                            break;
                    }
                }
                else
                {
                    bandsQuery = bandsQuery.OrderByDescending(b => b.CreatedTime); // Mặc định
                }

                int totalCount = await bandsQuery.CountAsync();

                var bands = await bandsQuery
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new BasePaginatedList<BandBrand>(bands, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch BandBrands: {Message}", ex.Message);
                throw;
            }
        }
        public async Task<BandBrand> GetBandBrandById(int id)
        {
            try
            {
                _logger.LogInformation("Attempting to get BandBrand with ID: {Id}", id);

                var bandBrand = await _unitOfWork.GetRepository<BandBrand>()
                    .Entities
                    .FirstOrDefaultAsync(bb => bb.Id == id && !bb.DeletedTime.HasValue);

                if (bandBrand == null)
                {
                    throw new KeyNotFoundException($"BandBrand with ID {id} not found or already deleted.");
                }

                return bandBrand;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get BandBrand with ID {Id}: {Message}", id, ex.Message);
                throw;
            }
        }
        public async Task<BandBrand> UpdateBandBrand(int id, CreateBandBrandModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Name))
            {
                throw new ArgumentNullException(nameof(model), "Update data is required and Name cannot be empty or whitespace.");
            }

            if (model.Name.Length > 255)
            {
                throw new ArgumentException("Name cannot exceed 255 characters.", nameof(model.Name));
            }

            var bandBrand = await _unitOfWork.GetRepository<BandBrand>()
                .Entities
                .FirstOrDefaultAsync(b => b.Id == id && !b.DeletedTime.HasValue);

            if (bandBrand == null)
            {
                throw new KeyNotFoundException($"BandBrand with ID {id} not found or already deleted.");
            }

            // Kiểm tra trùng lặp (ngoại trừ bản thân)
            var existingBandBrand = await _unitOfWork.GetRepository<BandBrand>()
                .Entities
                .FirstOrDefaultAsync(b => b.NameBrand.ToLower() == model.Name.ToLower()
                    && b.Id != id && !b.DeletedTime.HasValue);

            if (existingBandBrand != null)
            {
                throw new InvalidOperationException("A BandBrand with this name already exists.");
            }

            bandBrand.NameBrand = model.Name.Trim();
            bandBrand.LastUpdatedTime = DateTimeOffset.Now;
            bandBrand.LastUpdatedBy = "Huong"; // Nên lấy từ context người dùng hiện tại nếu có

            try
            {
                await _unitOfWork.GetRepository<BandBrand>().UpdateAsync(bandBrand);
                await _unitOfWork.SaveAsync();
                return bandBrand;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update BandBrand with ID {id}.", ex);
            }
        }

        public async Task<bool> DeleteBandBrand(int id)
        {
            var bandBrand = await _unitOfWork.GetRepository<BandBrand>()
                .Entities
                .FirstOrDefaultAsync(b => b.Id == id && !b.DeletedTime.HasValue);

            if (bandBrand == null)
            {
                return false; // Không tìm thấy hoặc đã bị xóa (soft delete)
            }

            bandBrand.DeletedTime = DateTimeOffset.Now;
            bandBrand.DeletedBy = "Huong"; // Nên lấy từ context người dùng hiện tại nếu có

            try
            {
                await _unitOfWork.GetRepository<BandBrand>().UpdateAsync(bandBrand);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete BandBrand with ID {id}.", ex);
            }
        }

        public async Task<BandBrand> UpdateBandBrand(int id, UpdateBandBrandModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Name))
            {
                throw new ArgumentNullException(nameof(model), "Update data is required and Name cannot be empty or whitespace.");
            }

            if (model.Name.Length > 255)
            {
                throw new ArgumentException("Name cannot exceed 255 characters.", nameof(model.Name));
            }

            var bandBrand = await _unitOfWork.GetRepository<BandBrand>()
                .Entities
                .FirstOrDefaultAsync(b => b.Id == id && !b.DeletedTime.HasValue);

            if (bandBrand == null)
            {
                throw new KeyNotFoundException($"BandBrand with ID {id} not found or already deleted.");
            }

            var existingBandBrand = await _unitOfWork.GetRepository<BandBrand>()
                .Entities
                .FirstOrDefaultAsync(b => b.NameBrand.ToLower() == model.Name.ToLower()
                    && b.Id != id && !b.DeletedTime.HasValue);

            if (existingBandBrand != null)
            {
                throw new InvalidOperationException("A BandBrand with this name already exists.");
            }

            bandBrand.NameBrand = model.Name.Trim();
            bandBrand.LastUpdatedTime = DateTimeOffset.Now;
            bandBrand.LastUpdatedBy = "System"; // Nên lấy từ context người dùng hiện tại nếu có

            try
            {
                await _unitOfWork.GetRepository<BandBrand>().UpdateAsync(bandBrand);
                await _unitOfWork.SaveAsync();
                return bandBrand;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update BandBrand with ID {id}.", ex);
            }
        }

    }
}
