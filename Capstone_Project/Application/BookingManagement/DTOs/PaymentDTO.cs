namespace BE_Capstone_Project.Application.BookingManagement.DTOs
{
    public class PaymentDTO
    {
        public int BookingId { get; set; }
        public bool Success { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
    }
}
