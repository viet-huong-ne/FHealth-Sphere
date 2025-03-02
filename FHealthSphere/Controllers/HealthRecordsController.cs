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

        [HttpGet]
        public async Task<ActionResult<BasePaginatedList<HealthRecord>>> GetAllHealthRecords(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] int? patientId = null,
    [FromQuery] int? bandId = null,
    [FromQuery] string ghiChu = null,
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

                var healthRecords = await _healthRecordService.GetAllHealthRecords(pageNumber, pageSize, patientId, bandId, ghiChu, sortBy, sortOrder, createdStartDate, createdEndDate, updatedStartDate, updatedEndDate, deletedStartDate, deletedEndDate, createdBy, updatedBy, deletedBy, isActive);
                return Ok(BaseResponse<BasePaginatedList<HealthRecord>>.OkResponse(healthRecords));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving HealthRecords: {ex.Message}");
            }
        }
        [HttpGet("{id}")] // Thêm phương thức Get by Id
        public async Task<ActionResult<HealthRecord>> GetHealthRecordById(int id)
        {
            try
            {
                var healthRecord = await _healthRecordService.GetHealthRecordById(id);
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