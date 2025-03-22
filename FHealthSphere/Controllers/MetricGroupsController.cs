using Contract.Repositories.Entity;
using Contract.Services.Interface;
using Core.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelViews.MetricGroupModelViews;
using System.Threading.Tasks;

namespace FHealthSphere.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class MetricGroupsController : ControllerBase
    {
        private readonly IMetricGroupService _metricGroupService;

        public MetricGroupsController(IMetricGroupService metricGroupService)
        {
            _metricGroupService = metricGroupService;
        }

        // GET: api/MetricGroups?pageNumber=1&pageSize=10
        [HttpGet]
        public async Task<ActionResult<BasePaginatedList<MetricGroup>>> GetAllMetricGroups(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string name = null,
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

                var metricGroups = await _metricGroupService.GetAllMetricGroups(pageNumber, pageSize, name, sortBy, sortOrder, createdStartDate, createdEndDate, updatedStartDate, updatedEndDate, deletedStartDate, deletedEndDate, createdBy, updatedBy, deletedBy, isActive);
                return Ok(BaseResponse<BasePaginatedList<MetricGroup>>.OkResponse(metricGroups));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving MetricGroups: {ex.Message}");
            }
        }
        [HttpGet("{id}")] // Thêm phương thức Get by Id
        public async Task<ActionResult<MetricGroup>> GetMetricGroupById(int id)
        {
            try
            {
                var metricGroup = await _metricGroupService.GetMetricGroupById(id);
                return Ok(BaseResponse<MetricGroup>.OkResponse(metricGroup));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to get MetricGroup with ID {id}: {ex.Message}");
            }
        }

        // POST: api/MetricGroups
        [HttpPost]
        public async Task<ActionResult<MetricGroup>> CreateMetricGroup([FromBody] CreateMetricGroupModel model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest("Request body is required.");
                }

                var metricGroup = await _metricGroupService.CreateMetricGroup(model);
                return Ok(BaseResponse<MetricGroup>.OkResponse(metricGroup));
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to create MetricGroup: {ex.Message}");
            }
        }

        // PUT: api/MetricGroups/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<MetricGroup>> UpdateMetricGroup(int id, [FromBody] UpdateMetricGroupModel model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest("Request body is required.");
                }

                var metricGroup = await _metricGroupService.UpdateMetricGroup(id, model);
                return Ok(BaseResponse<MetricGroup>.OkResponse(metricGroup));
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
                return StatusCode(500, $"Failed to update MetricGroup with ID {id}: {ex.Message}");
            }
        }

        // DELETE: api/MetricGroups/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMetricGroup(int id)
        {
            try
            {
                var result = await _metricGroupService.DeleteMetricGroup(id);
                if (!result)
                {
                    return NotFound($"MetricGroup with ID {id} not found or already deleted.");
                }
                return Ok($"MetricGroup with ID {id} successfully soft deleted.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to delete MetricGroup with ID {id}: {ex.Message}");
            }
        }
    }
}