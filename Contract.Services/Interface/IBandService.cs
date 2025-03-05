using Contract.Repositories.Entity;
using Core.Base;
using ModelViews.BandModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Services.Interface
{
    public interface IBandService
    {
        Task<Band> CreateBand(CreateBandModel model);
        Task<BasePaginatedList<Band>> GetAllBands(int pageNumber, int pageSize, int? patientId = null, int? bandBrandId = null, string image = null, string bandCode = null, string sortBy = null, string sortOrder = "asc", DateTime? createdStartDate = null, DateTime? createdEndDate = null, DateTime? updatedStartDate = null, DateTime? updatedEndDate = null, DateTime? deletedStartDate = null, DateTime? deletedEndDate = null, string createdBy = null, string updatedBy = null, string deletedBy = null, bool? isActive = null);
        Task<Band> UpdateBand(int id, UpdateBandModel model);
        Task<bool> DeleteBand(int id);
        Task<Band> GetBandById(int id);
    }
}
