using Contract.Repositories.Entity;
using Core.Base;
using ModelViews.BandModelViews;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Contract.Repositories.Interface;
using Services.Service;

namespace FHealthSphere.Services.Services
{
    public class BandService : IBandService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BandService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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

            var band = new Band
            {
                PatientId = model.PatientId,
                Image = model.Image.Trim(),
                BandBrandId = model.BandBrandId,
                CreatedBy = "System",
                CreatedTime = DateTimeOffset.Now,
                LastUpdatedTime = DateTimeOffset.Now
            };

            await _unitOfWork.GetRepository<Band>().InsertAsync(band);
            await _unitOfWork.SaveAsync();
            return band;
        }

        public async Task<BasePaginatedList<Band>> GetAllBands(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _unitOfWork.GetRepository<Band>().Entities
                .Where(b => !b.DeletedTime.HasValue)
                .OrderByDescending(b => b.CreatedTime);

            var totalCount = await query.CountAsync();
            var bands = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new BasePaginatedList<Band>(bands, totalCount, pageNumber, pageSize);
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

            if (model.BandBrandId.HasValue)
            {
                var bandBrandExists = await _unitOfWork.GetRepository<BandBrand>().Entities.AnyAsync(b => b.Id == model.BandBrandId.Value && !b.DeletedTime.HasValue);
                if (!bandBrandExists)
                {
                    throw new KeyNotFoundException($"BandBrand with ID {model.BandBrandId.Value} not found.");
                }
                band.BandBrandId = model.BandBrandId.Value;
            }

            if (!string.IsNullOrWhiteSpace(model.Image))
            {
                band.Image = model.Image.Trim();
            }

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
