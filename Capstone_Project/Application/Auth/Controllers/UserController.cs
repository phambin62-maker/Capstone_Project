using BE_Capstone_Project.Application.Auth.DTOs;
using BE_Capstone_Project.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using static BE_Capstone_Project.Application.Auth.DTOs.UserDTOs;

namespace BE_Capstone_Project.Application.Auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [Authorize]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserDto request)
        {
            if (request == null)
                return BadRequest("User data is required.");

            
            var userIdClaim = User.FindFirst("UserId")?.Value
                              ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new { message = "Invalid or missing token." });

            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { message = "Invalid user ID in token." });

            request.Id = userId;

            var success = await _userService.UpdateUserAsync(request);

            if (success)
                return Ok(new { message = "Profile updated successfully." });

            return StatusCode(500, new { message = "An error occurred while updating the profile." });
        }
    }
}
