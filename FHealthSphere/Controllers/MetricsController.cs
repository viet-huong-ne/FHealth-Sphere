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

        // GET: api/Metrics?pageNumber=1&pageSize=10
        [HttpGet]
        public async Task<ActionResult<BasePaginatedList<Metric>>> GetAllMetrics([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var metrics = await _metricService.GetAllMetrics(pageNumber, pageSize);
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