using Contract.Repositories.Entity;
using Microsoft.AspNetCore.Mvc;
using ModelViews.HealthRecordModelViews;
using Services.Service;

namespace FHealthSphere.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FirebaseController : Controller
    {
        private readonly FirebaseService _firebaseService;
        public FirebaseController (FirebaseService firebaseService)
        {
            _firebaseService = firebaseService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateHealthRecord([FromBody] CreateHealthRecordModel record)
        {
            if (record == null)
            {
                return BadRequest("Invalid data.");
            }

            bool isAdded = await _firebaseService.AddHealthRecordAsync(record);
            if (isAdded)
            {
                return Ok(new { message = "Record added successfully!" });
            }
            return StatusCode(500, "Failed to add record.");
        }
    }
}
