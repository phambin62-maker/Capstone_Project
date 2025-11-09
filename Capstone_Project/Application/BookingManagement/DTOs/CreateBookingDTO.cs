using BE_Capstone_Project.Domain.Enums;

namespace BE_Capstone_Project.Application.BookingManagement.DTOs
{
    public class CreateBookingDTO
    {
        public int UserId { get; set; }
        public int TourScheduleId { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
        public decimal? TotalPrice { get; set; }
        public DateOnly? RefundDate { get; set; }
        public decimal? RefundAmount { get; set; }
        public byte? PaymentMethod { get; set; }
        public DateOnly? PaymentDate { get; set; }
        public DateTime? ExpirationTime { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? CertificateId { get; set; }
        public DateTime? BookingDate { get; set; }
        public BookingStatus? BookingStatus { get; set; } = (BookingStatus?)Domain.Enums.BookingStatus.Pending;
    }
}
