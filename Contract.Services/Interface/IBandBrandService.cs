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
        Task<BasePaginatedList<BandBrand>> GetAllBandBrands(int pageNumber, int pageSize, string name = null, string sortBy = null, string sortOrder = "asc", DateTime? createdStartDate = null, DateTime? createdEndDate = null, DateTime? updatedStartDate = null, DateTime? updatedEndDate = null, DateTime? deletedStartDate = null, DateTime? deletedEndDate = null, string createdBy = null, string updatedBy = null, string deletedBy = null, bool? isActive = null);
        Task<BandBrand> UpdateBandBrand(int id, UpdateBandBrandModel model);
        Task<bool> DeleteBandBrand(int id);
        Task<BandBrand> GetBandBrandById(int id);
    }
}
