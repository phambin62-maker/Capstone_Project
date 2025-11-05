using BE_Capstone_Project.Domain.Enums;

namespace BE_Capstone_Project.Application.Bookings.DTOs
{
    public class BookingDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TourScheduleId { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
        public decimal? TotalPrice { get; set; }
        public DateTime? BookingDate { get; set; }
        public BookingStatus? BookingStatus { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? TourName { get; set; }
        public string? Username { get; set; }
    }
}
