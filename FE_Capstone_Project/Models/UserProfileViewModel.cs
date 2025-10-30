using BE_Capstone_Project.Domain.Enums;
using System.Text.Json.Serialization;

namespace FE_Capstone_Project.Models
{
    public class UserProfileViewModel
    {
        public string? Username { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Image { get; set; }
    }

    public class UserDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("username")]
        public string Username { get; set; }
        [JsonPropertyName("firstname")]
        public string FirstName { get; set; }
        [JsonPropertyName("lastname")]
        public string LastName { get; set; }
        [JsonPropertyName("phoneNumber")]
        public string PhoneNumber { get; set; }
        [JsonPropertyName("email")]
        public string Email { get; set; }
    }
}
