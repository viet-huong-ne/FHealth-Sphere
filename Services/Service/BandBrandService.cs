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
            catch (Exception ex) {
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
                .Skip((pageNumber -1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return new BasePaginatedList<BandBrand> (brands, TotalCount, pageNumber, pageSize);
        }
    }
}
