using Contract.Repositories.Entity;
using Contract.Repositories.Interface;
using Contract.Services.Interface;
using Core.Base;
using Microsoft.EntityFrameworkCore;
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

        public BandBrandService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BandBrand> CreateBandBrand(CreateBandBrandModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Name))
            {
                throw new ArgumentNullException("không có dữ liệu");
            }
            try
            {
                // create band brand from model
                var Brand = new BandBrand
                {
                    NameBrand = model.Name,
                    CreatedBy = "Huong",
                    CreatedTime = DateTimeOffset.Now,
                    LastUpdatedTime = DateTimeOffset.Now
                };
                await _unitOfWork.GetRepository<BandBrand>().InsertAsync(Brand);
                await _unitOfWork.SaveAsync();
                return Brand;
            }
            catch (Exception ex)
            {
                throw new Exception("can't create band brand", ex);
            }

        }

        public async Task<BasePaginatedList<BandBrand>> GetAllBandBrand(int pageNumber, int pageSize)
        {
            //get list band brand which aren't deleted and descending order
            IQueryable<BandBrand> BandBrandsQuery = _unitOfWork.GetRepository<BandBrand>()
                .Entities
                .Where(p => !p.DeletedTime.HasValue)
                .OrderByDescending(p => p.CreatedTime);
            // count all brands that have not been deleted
            int TotalCount = await BandBrandsQuery.CountAsync();

            // get 
            var brands = await BandBrandsQuery
                .OrderBy(s => s.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return new BasePaginatedList<BandBrand>(brands, TotalCount, pageNumber, pageSize);
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

        public Task<BandBrand> UpdateBandBrand(int id, UpdateBandBrandModel model)
        {
            throw new NotImplementedException();
        }
    }
}
