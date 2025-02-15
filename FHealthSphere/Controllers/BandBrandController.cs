using Contract.Repositories.Entity;
using Contract.Services.Interface;
using Core.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModelViews.BandBrandModelViews;
using System.Runtime.InteropServices;

namespace FHealthSphere.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BandBrandController : ControllerBase
    {
        private readonly IBandBrandService _brandService;
        public BandBrandController(IBandBrandService brandService)
        {
            _brandService = brandService;
        }
        [HttpGet("Brand")]
        public async Task<ActionResult<BasePaginatedList<BandBrand>>> GetAllBandBrand(int pageNumber, int pageSize)
        {
            try
            {
                var Brands = await _brandService.GetAllBandBrand(pageNumber, pageSize);
                return Ok(BaseResponse<BasePaginatedList<BandBrand>>.OkResponse(Brands));
            }
            catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("new-Brand")]
        public async Task<IActionResult> AddBandBrand([FromBody] CreateBandBrandModel model)
        {
            try
            {
                var brand = await _brandService.CreateBandBrand(model);
                return Ok(BaseResponse<BandBrand>.OkResponse(brand)); // return band brand created
            }
            catch (Exception ex) 
            { 
                return BadRequest(ex.Message);
            }
        }

    }
}
