using Contract.Repositories.Entity;
using Contract.Services.Interface;
using Core.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModelViews.BandBrandModelViews;
using Services.Service;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace FHealthSphere.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BandBrandsController : ControllerBase
    {
        private readonly IBandBrandService _brandService;
        public BandBrandsController(IBandBrandService brandService)
        {
            _brandService = brandService;
        }

        [HttpGet]
        public async Task<ActionResult<BasePaginatedList<BandBrand>>> GetAllBandBrands(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string name = null,
    [FromQuery] string sortBy = null,
    [FromQuery] string sortOrder = "asc",
    [FromQuery] DateTime? createdStartDate = null,
    [FromQuery] DateTime? createdEndDate = null,
    [FromQuery] DateTime? updatedStartDate = null,
    [FromQuery] DateTime? updatedEndDate = null,
    [FromQuery] DateTime? deletedStartDate = null,
    [FromQuery] DateTime? deletedEndDate = null,
    [FromQuery] string createdBy = null,
    [FromQuery] string updatedBy = null,
    [FromQuery] string deletedBy = null,
    [FromQuery] bool? isActive = null)
        {
            try
            {
                // Validation định dạng ngày
                if (createdStartDate.HasValue && !createdStartDate.Value.ToString("yyyy-MM-dd").Equals(createdStartDate.Value.ToString("yyyy-MM-dd")))
                {
                    return BadRequest("Invalid createdStartDate format. Use yyyy-MM-dd.");
                }
                if (createdEndDate.HasValue && !createdEndDate.Value.ToString("yyyy-MM-dd").Equals(createdEndDate.Value.ToString("yyyy-MM-dd")))
                {
                    return BadRequest("Invalid createdEndDate format. Use yyyy-MM-dd.");
                }
                if (updatedStartDate.HasValue && !updatedStartDate.Value.ToString("yyyy-MM-dd").Equals(updatedStartDate.Value.ToString("yyyy-MM-dd")))
                {
                    return BadRequest("Invalid updatedStartDate format. Use yyyy-MM-dd.");
                }
                if (updatedEndDate.HasValue && !updatedEndDate.Value.ToString("yyyy-MM-dd").Equals(updatedEndDate.Value.ToString("yyyy-MM-dd")))
                {
                    return BadRequest("Invalid updatedEndDate format. Use yyyy-MM-dd.");
                }
                if (deletedStartDate.HasValue && !deletedStartDate.Value.ToString("yyyy-MM-dd").Equals(deletedStartDate.Value.ToString("yyyy-MM-dd")))
                {
                    return BadRequest("Invalid deletedStartDate format. Use yyyy-MM-dd.");
                }
                if (deletedEndDate.HasValue && !deletedEndDate.Value.ToString("yyyy-MM-dd").Equals(deletedEndDate.Value.ToString("yyyy-MM-dd")))
                {
                    return BadRequest("Invalid deletedEndDate format. Use yyyy-MM-dd.");
                }

                // Validation khoảng thời gian
                if (createdStartDate.HasValue && createdEndDate.HasValue && createdStartDate > createdEndDate)
                {
                    return BadRequest("createdStartDate must be less than or equal to createdEndDate.");
                }
                if (updatedStartDate.HasValue && updatedEndDate.HasValue && updatedStartDate > updatedEndDate)
                {
                    return BadRequest("updatedStartDate must be less than or equal to updatedEndDate.");
                }
                if (deletedStartDate.HasValue && deletedEndDate.HasValue && deletedStartDate > deletedEndDate)
                {
                    return BadRequest("deletedStartDate must be less than or equal to deletedEndDate.");
                }

                var bandBrands = await _brandService.GetAllBandBrands(pageNumber, pageSize, name, sortBy, sortOrder, createdStartDate, createdEndDate, updatedStartDate, updatedEndDate, deletedStartDate, deletedEndDate, createdBy, updatedBy, deletedBy, isActive);
                return Ok(BaseResponse<BasePaginatedList<BandBrand>>.OkResponse(bandBrands));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving BandBrands: {ex.Message}");
            }
        }
        [HttpGet("{id}")] // Thêm phương thức Get by Id
        public async Task<ActionResult<BandBrand>> GetBandBrandById(int id)
        {
            try
            {
                var bandBrand = await _brandService.GetBandBrandById(id);
                return Ok(BaseResponse<BandBrand>.OkResponse(bandBrand));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to get BandBrand with ID {id}: {ex.Message}");
            }
        }
        [HttpPost]
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
