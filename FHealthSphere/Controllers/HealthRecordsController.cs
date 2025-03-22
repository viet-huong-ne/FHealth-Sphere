using Contract.Repositories.Entity;
using Contract.Services.Interface;
using Core.Base;
using Microsoft.AspNetCore.Mvc;
using ModelViews.HealthRecordModelViews;

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

        [HttpPost]
        public async Task<ActionResult<HealthRecord>> CreateHealthRecordCombined([FromBody] CreateHealthRecordCombinedModel model)
        {
            try
            {
                var healthRecord = await _healthRecordService.CreateHealthRecordCombined(model);
                return Ok(BaseResponse<HealthRecord>.OkResponse(healthRecord));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to create HealthRecord: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<ActionResult<BasePaginatedList<HealthRecord>>> GetAllHealthRecordsCombined(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? patientId = null,
            [FromQuery] int? bandId = null,
            [FromQuery] string ghiChu = null,
            [FromQuery] string sortBy = null,
            [FromQuery] string sortOrder = "desc",
            [FromQuery] DateTime? createdStartDate = null,
            [FromQuery] DateTime? createdEndDate = null)
        {
            try
            {
                var healthRecords = await _healthRecordService.GetAllHealthRecordsCombined(pageNumber, pageSize, patientId, bandId, ghiChu, sortBy, sortOrder, createdStartDate, createdEndDate);
                return Ok(BaseResponse<BasePaginatedList<HealthRecord>>.OkResponse(healthRecords));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving HealthRecords: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<HealthRecord>> GetHealthRecordCombinedById(int id)
        {
            try
            {
                var healthRecord = await _healthRecordService.GetHealthRecordCombinedById(id);
                return Ok(BaseResponse<HealthRecord>.OkResponse(healthRecord));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to get HealthRecord with ID {id}: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<HealthRecord>> UpdateHealthRecordCombined(int id, [FromBody] UpdateHealthRecordCombinedModel model)
        {
            try
            {
                var healthRecord = await _healthRecordService.UpdateHealthRecordCombined(id, model);
                return Ok(BaseResponse<HealthRecord>.OkResponse(healthRecord));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to update HealthRecord with ID {id}: {ex.Message}");
            }
        }

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
                return Ok($"HealthRecord with ID {id} and its RecordMetricItems successfully soft deleted.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to delete HealthRecord with ID {id}: {ex.Message}");
            }
        }
    }
}