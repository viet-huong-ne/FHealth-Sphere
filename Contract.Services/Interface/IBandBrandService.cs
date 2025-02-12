using Contract.Repositories.Entity;
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
    }
}
