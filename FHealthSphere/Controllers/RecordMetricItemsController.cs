using Contract.Repositories.Entity;
using Contract.Services.Interface;
using Core.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelViews.RecordMetricItemModelViews;
using System.Threading.Tasks;

namespace FHealthSphere.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class RecordMetricItemsController : ControllerBase
    {
        private readonly IRecordMetricItemService _recordMetricItemService;

        public RecordMetricItemsController(IRecordMetricItemService recordMetricItemService)
        {
            _recordMetricItemService = recordMetricItemService;
        }

        [HttpGet]
        public async Task<ActionResult<BasePaginatedList<RecordMetricItem>>> GetAllRecordMetricItems(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] int? recordId = null,
    [FromQuery] int? healthRecordId = null,
    [FromQuery] int? metricId = null,
    [FromQuery] decimal? value = null,
    [FromQuery] string type = null,
    [FromQuery] string sortBy = null,
    [FromQuery] string sortOrder = "desc",
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

                var recordMetricItems = await _recordMetricItemService.GetAllRecordMetricItems(pageNumber, pageSize, recordId, healthRecordId, metricId, value, type, sortBy, sortOrder, createdStartDate, createdEndDate, updatedStartDate, updatedEndDate, deletedStartDate, deletedEndDate, createdBy, updatedBy, deletedBy, isActive);
                return Ok(BaseResponse<BasePaginatedList<RecordMetricItem>>.OkResponse(recordMetricItems));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving RecordMetricItems: {ex.Message}");
            }
        }
        [HttpGet("{id}")] // Thêm phương thức Get by Id
        public async Task<ActionResult<RecordMetricItem>> GetRecordMetricItemById(int id)
        {
            try
            {
                var recordMetricItem = await _recordMetricItemService.GetRecordMetricItemById(id);
                return Ok(BaseResponse<RecordMetricItem>.OkResponse(recordMetricItem));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to get RecordMetricItem with ID {id}: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<RecordMetricItem>> CreateRecordMetricItem([FromBody] CreateRecordMetricItemModel model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest("Request body is required.");
                }

                var recordMetricItem = await _recordMetricItemService.CreateRecordMetricItem(model);
                return Ok(BaseResponse<RecordMetricItem>.OkResponse(recordMetricItem));
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
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to create RecordMetricItem: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        // PUT: api/RecordMetricItems/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<RecordMetricItem>> UpdateRecordMetricItem(int id, [FromBody] UpdateRecordMetricItemModel model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest("Request body is required.");
                }

                var recordMetricItem = await _recordMetricItemService.UpdateRecordMetricItem(id, model);
                return Ok(BaseResponse<RecordMetricItem>.OkResponse(recordMetricItem));
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
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to update RecordMetricItem with ID {id}: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        // DELETE: api/RecordMetricItems/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRecordMetricItem(int id)
        {
            try
            {
                var result = await _recordMetricItemService.DeleteRecordMetricItem(id);
                if (!result)
                {
                    return NotFound($"RecordMetricItem with ID {id} not found or already deleted.");
                }
                return Ok($"RecordMetricItem with ID {id} successfully soft deleted.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to delete RecordMetricItem with ID {id}: {ex.Message}");
            }
        }
    }
}