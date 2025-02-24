using Contract.Repositories.Entity;
using Core.Base;
using ModelViews.BandBrandModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Services.Interface
{
    public interface IBandBrandService
    {
        Task<BandBrand> CreateBandBrand(CreateBandBrandModel model);
        Task<BasePaginatedList<BandBrand>> GetAllBandBrand(int pageNumber, int pageSize);
        Task<BandBrand> UpdateBandBrand(int id, UpdateBandBrandModel model);
        Task<bool> DeleteBandBrand(int id);
    }
}
