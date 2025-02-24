using Contract.Repositories.Entity;
using Core.Base;
using ModelViews.BandModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Service
{
    public interface IBandService
    {
        Task<Band> CreateBand(CreateBandModel model);
        Task<BasePaginatedList<Band>> GetAllBands(int pageNumber, int pageSize);
        Task<Band> UpdateBand(int id, UpdateBandModel model);
        Task<bool> DeleteBand(int id);
    }
}
