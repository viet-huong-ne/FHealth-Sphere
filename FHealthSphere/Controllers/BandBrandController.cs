using Contract.Repositories.Entity;
using Contract.Services.Interface;
using Core.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModelViews.BandBrandModelViews;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

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
        // PUT: api/BandBrands/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<BandBrand>> UpdateBandBrand(int id, [FromBody] UpdateBandBrandModel model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest("Request body is required.");
                }

                var brand = await _brandService.UpdateBandBrand(id, model);
                return Ok(BaseResponse<BandBrand>.OkResponse(brand));
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to update BandBrand with ID {id}: {ex.Message}");
            }
        }

        // DELETE: api/BandBrands/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBandBrand(int id)
        {
            try
            {
                var result = await _brandService.DeleteBandBrand(id);
                if (!result)
                {
                    return NotFound($"BandBrand with ID {id} not found or already deleted.");
                }
                return Ok($"BandBrand with ID {id} successfully soft deleted.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to delete BandBrand with ID {id}: {ex.Message}");
            }
        }

    }
}
