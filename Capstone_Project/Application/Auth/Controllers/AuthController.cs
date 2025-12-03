
using BE_Capstone_Project.Application.Auth.Services;
using BE_Capstone_Project.Application.Services;
using BE_Capstone_Project.Domain.Enums;
using BE_Capstone_Project.Domain.Models;
using BE_Capstone_Project.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static BE_Capstone_Project.Application.Auth.DTOs.UserDTOs;

namespace BE_Capstone_Project.Application.Auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly OtmsdbContext _context;
        private readonly AuthService _authService;
        private readonly IUserService _userService;
        public AuthController(OtmsdbContext context, AuthService authService, IUserService userService)
        {
            _context = context;
            _authService = authService;
            _userService = userService;

        }
[HttpPost("register")]
public async Task<IActionResult> Register([FromBody] RegisterDto request)
{
    try
    {
        if (request == null)
            return BadRequest("Request body is missing.");

        if (string.IsNullOrWhiteSpace(request.Username) || 
            string.IsNullOrWhiteSpace(request.Email) || 
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Username, Email, and Password are required.");
        }
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
        {
            Console.WriteLine($"[Register] ❌ Username '{request.Username}' already exists.");
            return BadRequest("Username already exists");
        }
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            Console.WriteLine($"[Register] ❌ Email '{request.Email}' already exists.");
            return BadRequest("Email already exists");
        }
        var user = new User
        {
            Username = request.Username.Trim(),
            Email = request.Email.Trim(),
            PasswordHash = HashPassword(request.Password),
            RoleId = 3,
            UserStatus = UserStatus.Active
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        Console.WriteLine($"[Register] ✅ User '{request.Username}' registered successfully.");

        return Ok(new { message = "Register successful" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Register] ⚠️ Exception: {ex.Message}");
        return StatusCode(500, "Internal server error");
    }
}


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            
            // Check if user exists
            if (user == null)
                return Unauthorized("Invalid username");

            // Check if password is correct
            if (!VerifyPassword(request.Password, user.PasswordHash))
                return Unauthorized("Invalid password");

            // Check if account is banned
            if (user.UserStatus == UserStatus.Banned)
            {
                return Unauthorized(new 
                { 
                    message = "AccountBanned",
                    error = "AccountBanned",
                    status = "Banned"
                });
            }

            // Check if account is active
            if (user.UserStatus != UserStatus.Active)
            {
                return Unauthorized(new 
                { 
                    message = "Your account are AccountInactive.Please contacr to admin.",
                    error = "AccountInactive",
                    status = user.UserStatus?.ToString() ?? "Unknown"
                });
            }

            var token = _authService.GenerateJwtToken(user);
            return Ok(new {user.FirstName,user.RoleId, token });
        }
        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> Profile()
        {
            // Lấy username và email từ token (Claim)
            var username = User.Identity?.Name;
            var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            Console.WriteLine($"[Profile] Username in token: {username}");
            Console.WriteLine($"[Profile] Email in token: {email}");

            if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(email))
            {
                Console.WriteLine("[Profile] ❌ Không tìm thấy username hoặc email trong token");
                return Unauthorized("Cannot find username or email in token");
            }

            // Tìm user bằng username trước, fallback sang email nếu cần
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username || u.Email == email);

            if (user == null)
            {
                Console.WriteLine("[Profile] ❌ Không tìm thấy user trong DB");
                return NotFound("User not found");
            }

            Console.WriteLine($"[Profile] ✅ Đã tìm thấy user: {user.Username}");

            return Ok(new
            {
                user.Username,
                user.Email,
                user.FirstName,
                user.LastName,
                user.PhoneNumber
            });
        }

        [HttpPost("google-sync")]
        public async Task<IActionResult> GoogleSync([FromBody] GoogleUserDto dto)
        {
            var result = await _userService.SyncGoogleUserAsync(dto);

            if (!result.Success)
            {
                // Check if it's a banned account error
                if (result.Message.Contains("bị khóa") || result.Message.Contains("banned"))
                {
                    return Unauthorized(new 
                    { 
                        message = result.Message,
                        error = "AccountBanned",
                        status = "Banned"
                    });
                }
                
                // Other errors return 500
                return StatusCode(500, new { message = result.Message });
            }

            return Ok(new
            {
                Success = true,
                Message = result.Message,
                UserId = result.UserId,
                Email = dto.Email,
                Token = result.Token

            });
        }

        // Helper
        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
        private static bool VerifyPassword(string password, string hash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hash;
        }
        
    }
}
