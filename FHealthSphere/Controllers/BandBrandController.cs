using Contract.Repositories.Entity;
using Contract.Services.Interface;
using Core.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModelViews.BandBrandModelViews;
using System.Runtime.InteropServices;

namespace FHealthSphere.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BandBrandController : ControllerBase
    {
        private readonly IBandBrandService _brandService;
        public BandBrandController(IBandBrandService brandService)
        {
            _brandService = brandService;
        }
        [HttpGet("band-brand")]
        public async Task<ActionResult<BasePaginatedList<BandBrand>>> GetAllBandBrand(int pageNumber = 1, int pageSize = 6)
        {
            try
            {
                var Brands = await _brandService.GetAllBandBrand(pageNumber, pageSize);
                Console.WriteLine(DateTimeOffset.Now);
                return Ok(BaseResponse<BasePaginatedList<BandBrand>>.OkResponse(Brands));
            }
            catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("band-brand")]
        [Authorize(Roles = "Admin")]
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
