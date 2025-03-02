using Contract.Repositories.Entity;
using Contract.Services.Interface;
using Core.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelViews.MetricModelViews;
using System.Threading.Tasks;

namespace FHealthSphere.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class MetricsController : ControllerBase
    {
        private readonly IMetricService _metricService;

        public MetricsController(IMetricService metricService)
        {
            _metricService = metricService;
        }

        [HttpGet]
        public async Task<ActionResult<BasePaginatedList<Metric>>> GetAllMetrics(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string name = null,
    [FromQuery] string unit = null,
    [FromQuery] int? metricGroupId = null,
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

                var metrics = await _metricService.GetAllMetrics(pageNumber, pageSize, name, unit, metricGroupId, sortBy, sortOrder, createdStartDate, createdEndDate, updatedStartDate, updatedEndDate, deletedStartDate, deletedEndDate, createdBy, updatedBy, deletedBy, isActive);
                return Ok(BaseResponse<BasePaginatedList<Metric>>.OkResponse(metrics));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving Metrics: {ex.Message}");
            }
        }
        [HttpGet("{id}")] // Thêm phương thức Get by Id
        public async Task<ActionResult<Metric>> GetMetricById(int id)
        {
            try
            {
                var metric = await _metricService.GetMetricById(id);
                return Ok(BaseResponse<Metric>.OkResponse(metric));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to get Metric with ID {id}: {ex.Message}");
            }
        }

        // POST: api/Metrics
        [HttpPost]
        public async Task<ActionResult<Metric>> CreateMetric([FromBody] CreateMetricModel model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest("Request body is required.");
                }

                var metric = await _metricService.CreateMetric(model);
                return Ok(BaseResponse<Metric>.OkResponse(metric));
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
                return StatusCode(500, $"Failed to create Metric: {ex.Message}");
            }
        }

        // PUT: api/Metrics/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<Metric>> UpdateMetric(int id, [FromBody] UpdateMetricModel model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest("Request body is required.");
                }

                var metric = await _metricService.UpdateMetric(id, model);
                return Ok(BaseResponse<Metric>.OkResponse(metric));
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
                return StatusCode(500, $"Failed to update Metric with ID {id}: {ex.Message}");
            }
        }

        // DELETE: api/Metrics/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMetric(int id)
        {
            try
            {
                var result = await _metricService.DeleteMetric(id);
                if (!result)
                {
                    return NotFound($"Metric with ID {id} not found or already deleted.");
                }
                return Ok($"Metric with ID {id} successfully soft deleted.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to delete Metric with ID {id}: {ex.Message}");
            }
        }

    }
}