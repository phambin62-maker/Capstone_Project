using BE_Capstone_Project.Application.Admin.DTOs;
using BE_Capstone_Project.Application.Admin.Service;
using BE_Capstone_Project.Application.Admin.Service.Interfaces;
using BE_Capstone_Project.Application.BookingManagement.Services;
using BE_Capstone_Project.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BE_Capstone_Project.Application.Admin.Controller
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles="Admin")]

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
            _logger.LogInformation("[GetAllAccounts] Request received");

            try
            {
                var users = await _adminService.GetAllUsersAsync();

                if (users == null)
                {
                    _logger.LogWarning("[GetAllAccounts] Service returned null (possible DAO/context issue)");
                    return NotFound("No users found or service returned null");
                }

                _logger.LogInformation("[GetAllAccounts] Retrieved {Count} users", users.Count);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetAllAccounts] Failed to retrieve users");
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

        // Get account statistics
        [HttpGet("accounts/statistics")]
        public async Task<IActionResult> GetAccountStatistics()
        {
            try
            {
                var statistics = await _adminService.GetAccountStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetAccountStatistics] Failed to retrieve statistics");
                return StatusCode(500, "Internal Server Error");
            }
        }

        // Get filtered accounts with pagination
        [HttpGet("accounts")]
        public async Task<IActionResult> GetFilteredAccounts(
            [FromQuery] int? roleId,
            [FromQuery] string? status,
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _adminService.GetFilteredAccountsAsync(roleId, status, search, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetFilteredAccounts] Failed to retrieve accounts");
                return StatusCode(500, "Internal Server Error");
            }
        }

        // Get account by ID
        [HttpGet("account/{id}")]
        public async Task<IActionResult> GetAccountById(int id)
        {
            try
            {
                var account = await _adminService.GetAccountByIdAsync(id);
                if (account == null)
                    return NotFound(new { Message = "Account not found" });

                return Ok(new
                {
                    id = account.Id,
                    firstName = account.FirstName,
                    lastName = account.LastName,
                    email = account.Email,
                    phoneNumber = account.PhoneNumber,
                    roleId = account.RoleId,
                    role = account.RoleId switch
                    {
                        1 => "Admin",
                        2 => "Staff",
                        _ => "Customer"
                    },
                    userStatus = account.UserStatus?.ToString() ?? "Unknown"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetAccountById] Failed to retrieve account");
                return StatusCode(500, "Internal Server Error");
            }
        }

        // Update account
        [HttpPut("account/{id}")]
        public async Task<IActionResult> UpdateAccount(int id, [FromBody] UpdateAccountDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _adminService.UpdateAccountAsync(id, dto);
                if (!result.Success)
                    return BadRequest(new { result.Message });

                return Ok(new { result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UpdateAccount] Failed to update account");
                return StatusCode(500, "Internal Server Error");
            }
        }

        // Delete account
        [HttpDelete("account/{id}")]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            try
            {
                var result = await _adminService.DeleteAccountAsync(id);
                if (!result.Success)
                    return BadRequest(new { result.Message });

                return Ok(new { result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteAccount] Failed to delete account");
                return StatusCode(500, "Internal Server Error");
            }
        }

        // Set account status (for all account types)
        [HttpPut("account/{id}/status")]
        public async Task<IActionResult> SetAccountStatus(int id, [FromQuery] bool isActive)
        {
            try
            {
                var result = await _adminService.SetAccountStatusAsync(id, isActive);
                if (!result.Success)
                    return BadRequest(new { result.Message });

                return Ok(new { result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SetAccountStatus] Failed to update account status");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}