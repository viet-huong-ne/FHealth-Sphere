using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Contract.Repositories.Entity;
using Repositories.Base;
using Contract.Services.Interface;
using Services.Service;
using ModelViews.BloodPressureModelViews;
using Core.Base;

namespace FHealthSphere.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BloodPressureClassificationsController : ControllerBase
    {
        private readonly IBloodPressureService _service;

        public BloodPressureClassificationsController(IBloodPressureService service)
        {
            _service = service;
        }

        // GET: api/BloodPressureClassifications
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BasePaginatedList<BloodPressureModel>>>> GetAll([FromQuery] int pageNumber = 1,
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
    [FromQuery] string deletedBy = null)
        {
            try
            {
                var result = await _service.GetAllAsync(
                    pageNumber, pageSize, name, sortBy, sortOrder,
                    createdStartDate, createdEndDate,
                    updatedStartDate, updatedEndDate,
                    deletedStartDate, deletedEndDate,
                    createdBy, updatedBy, deletedBy);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        // GET: api/BloodPressureClassifications/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BloodPressureClassification>> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound();

            return Ok(result);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int Id, UpdateBloodPressureModel bloodPressureClassification)
        {

            var updated = await _service.UpdateAsync(Id, bloodPressureClassification);
            if (!updated)
                return NotFound();

            return NoContent();
        }
        [HttpPost("check-classification")]
        public async Task<IActionResult> CheckBloodPressureClassification([FromBody] CheckBloodPressureModel model)
        {
            var classification = await _service.CheckBloodPressure(model.Systolic, model.Diastolic);

            if (classification == null)
                return NotFound(new { Message = "No matching classification found." });

            return Ok(new { Classification = classification.Name });
        }

        // POST: api/BloodPressureClassifications
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<BloodPressureClassification>> PostBloodPressureClassification(CreateBloodPressureModel model)
        {
            var result = await _service.CreateAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        // DELETE: api/BloodPressureClassifications/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}
