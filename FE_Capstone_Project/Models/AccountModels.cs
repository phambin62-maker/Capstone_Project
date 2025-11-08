using System.ComponentModel.DataAnnotations;

namespace FE_Capstone_Project.Models
{
    public class AccountModels
    {
        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public string Username => Email.Split('@')[0];
    }
}

