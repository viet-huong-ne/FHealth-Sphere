using Contract.Repositories.Entity;
using Contract.Services.Interface;
using Core.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelViews.HealthRecordModelViews;
using System.Threading.Tasks;

namespace FHealthSphere.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthRecordsController : ControllerBase
    {
        private readonly IHealthRecordService _healthRecordService;

        public HealthRecordsController(IHealthRecordService healthRecordService)
        {
            _healthRecordService = healthRecordService;
        }

        // GET: api/HealthRecords?pageNumber=1&pageSize=10
        [HttpGet]
        public async Task<ActionResult<BasePaginatedList<HealthRecord>>> GetAllHealthRecords([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var healthRecords = await _healthRecordService.GetAllHealthRecords(pageNumber, pageSize);
                return Ok(BaseResponse<BasePaginatedList<HealthRecord>>.OkResponse(healthRecords));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving HealthRecords: {ex.Message}");
            }
        }

        // POST: api/HealthRecords
        [HttpPost]
        public async Task<ActionResult<HealthRecord>> CreateHealthRecord([FromBody] CreateHealthRecordModel model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest("Request body is required.");
                }

                var healthRecord = await _healthRecordService.CreateHealthRecord(model);
                return Ok(BaseResponse<HealthRecord>.OkResponse(healthRecord));
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
                return StatusCode(500, $"Failed to create HealthRecord: {ex.Message}");
            }
        }

        // PUT: api/HealthRecords/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<HealthRecord>> UpdateHealthRecord(int id, [FromBody] UpdateHealthRecordModel model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest("Request body is required.");
                }

                var healthRecord = await _healthRecordService.UpdateHealthRecord(id, model);
                return Ok(BaseResponse<HealthRecord>.OkResponse(healthRecord));
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
                return StatusCode(500, $"Failed to update HealthRecord with ID {id}: {ex.Message}");
            }
        }

        // DELETE: api/HealthRecords/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHealthRecord(int id)
        {
            try
            {
                var result = await _healthRecordService.DeleteHealthRecord(id);
                if (!result)
                {
                    return NotFound($"HealthRecord with ID {id} not found or already deleted.");
                }
                return Ok($"HealthRecord with ID {id} successfully soft deleted.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to delete HealthRecord with ID {id}: {ex.Message}");
            }
        }
    }
}