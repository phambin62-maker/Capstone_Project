using BE_Capstone_Project.Application.BookingManagement.DTOs;
using BE_Capstone_Project.Domain.Enums;

namespace FE_Capstone_Project.Models
{
    public class TravelerDTO
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string IdentityID { get; set; } = string.Empty;
        public CustomerType CustomerType { get; set; }

        public override string ToString()
        {
            return $"Traveler: {FirstName} {LastName}, Email: {Email}, Phone: {PhoneNumber}, " +
                   $"ID: {IdentityID}, Type: {CustomerType}";
        }
    }

    public class BookingRequest
    {
        public int Tour_Schedule { get; set; }
        public int Adults { get; set; }
        public int Children { get; set; }
        public int Infants { get; set; }

        public string First_Name { get; set; } = string.Empty;
        public string Last_Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Certificate_Id { get; set; } = string.Empty;

        public List<TravelerDTO> Travelers { get; set; } = new();

        public override string ToString()
        {
            string travelerInfo = Travelers != null && Travelers.Count > 0
                ? string.Join("\n  ", Travelers.Select(t => t.ToString()))
                : "No travelers";

            return $"BookingRequest:\n" +
                   $"  Tour Schedule: {Tour_Schedule}\n" +
                   $"  Adults: {Adults}, Children: {Children}, Infants: {Infants}\n" +
                   $"  Name: {First_Name} {Last_Name}\n" +
                   $"  Email: {Email}, Phone: {Phone}\n" +
                   $"  Certificate ID: {Certificate_Id}\n" +
                   $"  Travelers:\n  {travelerInfo}";
        }
    }
    public class BookingDto
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

    public class BookingResponse
    {
        public int BookingId { get; set; }
        public int TotalPrice { get; set; }
        public string TourName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool Success { get; set; }
        public List<BookingDto> Data { get; set; } = new List<BookingDto>();

        public override string ToString()
        {
            return $"Booking Success: {Success}\n" +
                   $"Booking ID: {BookingId}\n" +
                   $"Tour Name: {TourName}\n" +
                   $"Customer: {FirstName} {LastName}\n" +
                   $"Total Price: {TotalPrice:C}\n" +
                   $"Message: {Message}";
        }
    }

    public class ScheduleBookedSeatsResponse
    {
        public int TourScheduleId { get; set; }
        public int BookedSeats { get; set; }
    }

    public class UserBookingResponse
    {
        public int BookingId { get; set; }
        public string TourName { get; set; } = string.Empty;
        public DateTime DepartureDate { get; set; }
        public DateTime ArrivalDate { get; set; }
        public int Adults { get; set; }
        public int Children { get; set; }
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; }
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
    public class StaffBookingDTO
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string TourName { get; set; } = string.Empty;
        public DateTime? DepartureDate { get; set; }
        public DateTime? ArrivalDate { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
        public BookingStatus? BookingStatus { get; set; }
        public decimal? TotalPrice { get; set; }
        public DateTime? BookingDate { get; set; }
        public DateTime? ExpirationTime { get; set; }
        public int AdultCount { get; set; }
        public int ChildCount { get; set; }
        public int InfantCount { get; set; }
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
    public class BookingStatistics
    {
        public int TotalBookings { get; set; }
        public int PendingBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public int CompletedBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingPayments { get; set; }
        public int CompletedPayments { get; set; }
    }


    
}
