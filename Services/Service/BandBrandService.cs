using Contract.Repositories.Entity;
using Contract.Repositories.Interface;
using Contract.Services.Interface;
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
    }
}
