using BE_Capstone_Project.Domain.Enums;

namespace BE_Capstone_Project.Application.BookingManagement.DTOs
{
    public class BookingCustomerDTO
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? IdentityID { get; set; }
        public CustomerType CustomerType { get; set; }
        public override string ToString()
        {
            return $"Traveler: {FirstName} {LastName}, Email: {Email}, Phone: {PhoneNumber}, " +
                   $"ID: {IdentityID}, Type: {CustomerType}";
        }
    }
}
