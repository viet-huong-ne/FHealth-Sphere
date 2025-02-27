using Contract.Repositories.Entity;
using Contract.Services.Interface;
using Core.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelViews.BandModelViews;
using Services.Service;
using System.Threading.Tasks;

namespace FHealthSphere.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class BandsController : ControllerBase
    {
        private readonly IBandService _bandService;

        public BandsController(IBandService bandService)
        {
            _bandService = bandService;
        }

        // GET: api/Bands?pageNumber=1&pageSize=10
        [HttpGet]
        public async Task<ActionResult<BasePaginatedList<Band>>> GetAllBands([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var bands = await _bandService.GetAllBands(pageNumber, pageSize);
                return Ok(BaseResponse<BasePaginatedList<Band>>.OkResponse(bands));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving Bands: {ex.Message}");
            }
        }

        // POST: api/Bands
        [HttpPost]
        public async Task<ActionResult<Band>> CreateBand([FromBody] CreateBandModel model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest("Request body is required.");
                }

                var band = await _bandService.CreateBand(model);
                return Ok(BaseResponse<Band>.OkResponse(band));
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to create Band: {ex.Message}");
            }
        }

        // PUT: api/Bands/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<Band>> UpdateBand(int id, [FromBody] UpdateBandModel model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest("Request body is required.");
                }

                var band = await _bandService.UpdateBand(id, model);
                return Ok(BaseResponse<Band>.OkResponse(band));
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to update Band with ID {id}: {ex.Message}");
            }
        }

        // DELETE: api/Bands/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBand(int id)
        {
            try
            {
                var result = await _bandService.DeleteBand(id);
                if (!result)
                {
                    return NotFound($"Band with ID {id} not found or already deleted.");
                }
                return Ok($"Band with ID {id} successfully soft deleted.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to delete Band with ID {id}: {ex.Message}");
            }
        }
    }
}