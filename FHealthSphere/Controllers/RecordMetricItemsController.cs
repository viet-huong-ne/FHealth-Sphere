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

        // GET: api/RecordMetricItems?pageNumber=1&pageSize=10
        [HttpGet]
        public async Task<ActionResult<BasePaginatedList<RecordMetricItem>>> GetAllRecordMetricItems([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var recordMetricItems = await _recordMetricItemService.GetAllRecordMetricItems(pageNumber, pageSize);
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