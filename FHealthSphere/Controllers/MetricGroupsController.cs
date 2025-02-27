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
        public async Task<ActionResult<BasePaginatedList<MetricGroup>>> GetAllMetricGroups([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var metricGroups = await _metricGroupService.GetAllMetricGroups(pageNumber, pageSize);
                return Ok(BaseResponse<BasePaginatedList<MetricGroup>>.OkResponse(metricGroups));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving MetricGroups: {ex.Message}");
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