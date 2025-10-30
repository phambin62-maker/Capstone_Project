namespace BE_Capstone_Project.Application.ReviewManagement.DTOs
{
    public class ReviewDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public int TourId { get; set; }

        public int BookingId { get; set; }

        public byte? Stars { get; set; }

        public string? Comment { get; set; }
    }
}
