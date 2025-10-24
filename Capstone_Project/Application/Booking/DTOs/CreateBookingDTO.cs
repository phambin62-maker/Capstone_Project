using BE_Capstone_Project.Domain.Enums;

namespace BE_Capstone_Project.Application.Bookings.DTOs
{
    public class CreateBookingDTO
    {
        public int UserId { get; set; }
        public int TourScheduleId { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
        public decimal? TotalPrice { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? CertificateId { get; set; }
        public BookingStatus? BookingStatus { get; set; } = (BookingStatus?)Domain.Enums.BookingStatus.Pending;
        public byte? PaymentMethod { get; set; }
    }
}
