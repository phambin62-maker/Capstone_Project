using BE_Capstone_Project.Application.Company.DTOs;
using BE_Capstone_Project.Application.Company.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BE_Capstone_Project.Application.Company.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeatureController : ControllerBase
    {
        private readonly IFeatureService _featureService;

        public FeatureController(IFeatureService featureService)
        {
            _featureService = featureService;
        }

        [HttpGet("active")]
        [AllowAnonymous] // Tất cả đều có thể xem features
        public async Task<IActionResult> GetActiveFeatures()
        {
            try
            {
                var features = await _featureService.GetActiveFeaturesAsync();
                return Ok(features);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching features", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous] // Tất cả đều có thể xem feature
        public async Task<IActionResult> GetFeatureById(int id)
        {
            try
            {
                var feature = await _featureService.GetFeatureByIdAsync(id);
                if (feature == null)
                    return NotFound(new { message = $"Feature with ID {id} not found" });

                return Ok(feature);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching feature", error = ex.Message });
            }
        }

        [HttpGet]
        [AllowAnonymous] 
        public async Task<IActionResult> GetAllFeatures()
        {
            try
            {
                var features = await _featureService.GetAllFeaturesAsync();
                return Ok(features);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching features", error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")] // Chỉ Admin và Staff mới được thêm feature
        public async Task<IActionResult> AddFeature([FromBody] FeatureDTO feature)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var featureId = await _featureService.AddFeatureAsync(feature);
                if (featureId <= 0)
                    return BadRequest(new { message = "Failed to add feature" });

                return CreatedAtAction(nameof(GetFeatureById), new { id = featureId }, feature);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error adding feature", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")] // Chỉ Admin và Staff mới được cập nhật feature
        public async Task<IActionResult> UpdateFeature(int id, [FromBody] FeatureDTO feature)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _featureService.UpdateFeatureAsync(id, feature);
                if (!result)
                    return NotFound(new { message = $"Feature with ID {id} not found" });

                return Ok(new { message = "Feature updated successfully", feature });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating feature", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Staff")] // Chỉ Admin và Staff mới được xóa feature
        public async Task<IActionResult> DeleteFeature(int id)
        {
            try
            {
                var result = await _featureService.DeleteFeatureAsync(id);
                if (!result)
                    return NotFound(new { message = $"Feature with ID {id} not found" });

                return Ok(new { message = "Feature deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting feature", error = ex.Message });
            }
        }
    }
}

