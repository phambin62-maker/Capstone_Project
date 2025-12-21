using BE_Capstone_Project.Domain.Enums;

namespace BE_Capstone_Project.Application.BookingManagement.DTOs
{
    public class UserBookingDTO
    {
        public int BookingId { get; set; }
        public string TourName { get; set; } = string.Empty;
        public DateOnly DepartureDate { get; set; }
        public int Adults { get; set; }
        public int Children { get; set; }
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public CancelValidationResult? CancelCondition { get; set; }
        public bool CanCancel { get; set; }
        public string? CancelMessage { get; set; }
    }
    public class CancelConditionDTO
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public byte? MinDaysBeforeTrip { get; set; }
        public byte? RefundPercent { get; set; }
        public CancelStatus? CancelStatus { get; set; }
    }

    public class CancelValidationResult
    {
        public bool CanCancel { get; set; }
        public string? Message { get; set; }
        public decimal? RefundAmount { get; set; }
        public int? RefundPercent { get; set; }
        public CancelConditionDTO? AppliedCondition { get; set; }
    }
}
