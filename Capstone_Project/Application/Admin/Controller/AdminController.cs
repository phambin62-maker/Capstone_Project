using BE_Capstone_Project.Application.Admin.DTOs;
using BE_Capstone_Project.Application.Admin.Service;
using BE_Capstone_Project.Application.Admin.Service.Interfaces;
using BE_Capstone_Project.Application.BookingManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace BE_Capstone_Project.Application.Admin.Controller
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<AdminController> _logger;
        public AdminController(IAdminService adminService, ILogger<AdminController> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }
        [HttpPost("create-account")]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _adminService.CreateAccountAsync(dto);

            if (!result.Success)
                return BadRequest(new { result.Message });

            return Ok(new
            {
                result.Message,
                Account = new
                {
                    result.Data.Id,
                    result.Data.Username,
                    result.Data.Email,
                    result.Data.FirstName,
                    result.Data.LastName,
                    result.Data.PhoneNumber,
                    result.Data.RoleId,
                    result.Data.UserStatus
                }
            });
        }
        [HttpGet("get-all-accounts")]
        public async Task<IActionResult> GetAllAccounts()
        {
            _logger.LogInformation("➡️ [GetAllAccounts] Request received");

            try
            {
                var users = await _adminService.GetAllUsersAsync();

                if (users == null)
                {
                    _logger.LogWarning("⚠️ [GetAllAccounts] Service returned null (possible DAO/context issue)");
                    return NotFound("No users found or service returned null");
                }

                _logger.LogInformation("✅ [GetAllAccounts] Retrieved {Count} users", users.Count);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [GetAllAccounts] Failed to retrieve users");
                return StatusCode(500, "Internal Server Error");
            }
        }

        //Active / Inactive staff
        [HttpPut("staff/{id}/status")]
        public async Task<IActionResult> SetStaffStatus(int id, [FromQuery] bool isActive)
        {
            var result = await _adminService.SetStaffActiveStatusAsync(id, isActive);
            if (!result.Success)
                return BadRequest(new { result.Message });

            return Ok(new { result.Message });
        }
    }
}