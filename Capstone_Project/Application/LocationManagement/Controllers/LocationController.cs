using BE_Capstone_Project.Application.LocationManagement.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BE_Capstone_Project.Application.LocationManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _locationService;

        public LocationController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        [HttpGet("GetAllLocations")]
        public async Task<IActionResult> GetAllLocations()
        {
            var locations = await _locationService.GetAllLocations();
            if(locations == null || locations.Count == 0)
            {
                return NotFound(new { message = "No locations found" });
            }
            return Ok(new { message = "Locations fetched successfully", locations });
        }
    }
}
