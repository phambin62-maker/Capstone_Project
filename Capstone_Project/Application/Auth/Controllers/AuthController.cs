
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
        private readonly ILogger<AuthController> _logger;
        public AuthController(OtmsdbContext context, AuthService authService, IUserService userService)
        {
            _context = context;
            _authService = authService;
            _userService = userService;

        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                return BadRequest("Username already exists");
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = HashPassword(request.Password),
                RoleId = 3,
                UserStatus = UserStatus.Active
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Register successful" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
                return Unauthorized("Invalid username or password");

            var token = _authService.GenerateJwtToken(user);
            return Ok(new {user.FirstName,user.RoleId, token });
        }
        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> Profile()
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Cannot find username in token");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return NotFound("User not found");
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
                return StatusCode(500, new { result.Message });

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
