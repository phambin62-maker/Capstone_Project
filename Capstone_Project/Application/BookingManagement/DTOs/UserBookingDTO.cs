using BE_Capstone_Project.Domain.Enums;

namespace BE_Capstone_Project.Application.BookingManagement.DTOs
{
    public class UserBookingDTO
    {
        public int BookingId { get; set; }
        public string TourName { get; set; } = string.Empty;
        public DateOnly DepartureDate { get; set; }
        public DateOnly ArrivalDate { get; set; }
        public int Adults { get; set; }
        public int Children { get; set; }
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; }
    }
}
