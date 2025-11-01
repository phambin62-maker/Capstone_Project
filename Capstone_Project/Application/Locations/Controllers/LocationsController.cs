using BE_Capstone_Project.Application.Locations.DTOs;
using BE_Capstone_Project.Application.Locations.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BE_Capstone_Project.Application.Locations.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationsController : ControllerBase
    {
        private readonly ILocationService _locationService;

        public LocationsController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLocations()
        {
            var result = await _locationService.GetAllLocationsAsync();
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }
            return Ok(result.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLocationById(int id)
        {
            var result = await _locationService.GetLocationByIdAsync(id);
            if (!result.Success)
            {
                return NotFound(new { message = result.Message });
            }
            return Ok(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> CreateLocation([FromBody] CreateLocationDTO createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _locationService.CreateLocationAsync(createDto);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return CreatedAtAction(nameof(GetLocationById), new { id = result.Data }, new { id = result.Data, message = result.Message });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLocation(int id, [FromBody] UpdateLocationDTO updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _locationService.UpdateLocationAsync(id, updateDto);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            var result = await _locationService.DeleteLocationAsync(id);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }

        [HttpGet("GetAllLocations")]
        public async Task<IActionResult> GetAllLocations2()
        {
            var locations = await _locationService.GetAllLocationsAsync();
            if (locations.Data == null || locations.Data.Count == 0)
            {
                return NotFound(new { message = "No locations found" });
            }
            return Ok(new { message = "Locations fetched successfully", locations });
        }
    }
}
