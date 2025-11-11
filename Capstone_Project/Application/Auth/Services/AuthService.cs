using BE_Capstone_Project.Domain.Enums;
using BE_Capstone_Project.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BE_Capstone_Project.Application.Auth.Services
{
    public class AuthService 
    {
        private readonly IConfiguration _config;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IConfiguration config)
        {
            _config = config;
            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<AuthService>();
        }

        public string GenerateJwtToken(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user), "User cannot be null when generating JWT.");

            var jwtSettings = _config.GetSection("Jwt");

            //  Đảm bảo không null
            var username = !string.IsNullOrEmpty(user.Username)
                ? user.Username
                : (user.Email ?? $"User_{user.Id}");

            var email = user.Email ?? "";
            
            // Convert roleId to role name for authorization
            var roleName = user.RoleId switch
            {
                1 => "Admin",
                2 => "Staff",
                3 => "Customer",
                _ => "Customer"
            };
            
            var roleId = user.RoleId > 0 ? user.RoleId.ToString() : ((int)RoleType.Customer).ToString();

            var claims = new List<Claim>
            {
                new Claim("UserId", user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // For User.Identity.Name and GetCurrentUserId()
                new Claim(ClaimTypes.Name, username), // For User.Identity.Name
                new Claim(JwtRegisteredClaimNames.UniqueName, username),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(ClaimTypes.Role, roleName), // Use role name for authorization
                new Claim("RoleId", roleId), // Keep roleId for reference
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["DurationInMinutes"] ?? "60")),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            _logger.LogInformation($"JWT token created for user ID {user.Id}, Email={email}, Provider={(user.Provider ?? "Local")}");

            return tokenString;
        }


    }
}
