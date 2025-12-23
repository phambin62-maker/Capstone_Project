using BE_Capstone_Project.Domain.Enums;

namespace BE_Capstone_Project.Application.BookingManagement.DTOs
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
    public class BookingSearchRequest
    {
        public string? SearchTerm { get; set; }
        public BookingStatus? BookingStatus { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class BookingListResponse
    {
        public List<BookingDTO> Bookings { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class UpdateBookingStatusRequest
    {
        public BookingStatus BookingStatus { get; set; }
        public string? Note { get; set; }
    }

    public class UpdatePaymentStatusRequest
    {
        public PaymentStatus PaymentStatus { get; set; }
        public string? Note { get; set; }
    }

    public class StaffBookingDTO
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string TourName { get; set; } = string.Empty;
        public DateOnly? DepartureDate { get; set; }
        public DateOnly? ArrivalDate { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
        public BookingStatus? BookingStatus { get; set; }
        public decimal? TotalPrice { get; set; }
        public DateTime? BookingDate { get; set; }
        public DateTime? ExpirationTime { get; set; }
        public int AdultCount { get; set; }
        public int ChildCount { get; set; }
        public int InfantCount { get; set; }
    }
}
