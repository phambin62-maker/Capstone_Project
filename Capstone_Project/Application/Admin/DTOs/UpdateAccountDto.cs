using System.ComponentModel.DataAnnotations;

namespace BE_Capstone_Project.Application.Admin.DTOs
{
    public class UpdateAccountDto
    {
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [RegularExpression(@"^(0)[0-9]{9}$", ErrorMessage = "Phone number must be 10 digits and start with 0.")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Role is required.")]
        public string Role { get; set; } = string.Empty;

        public string? Password { get; set; }
    }
}

