namespace BE_Capstone_Project.Application.BookingManagement.DTOs
{
    public class BookingSuccessResponse
    {
        public int BookingId { get; set; }
        public int TotalPrice { get; set; }
        public string TourName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool Success { get; set; }

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
}
