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
    public class ReviewPopDTO 
    {
        public int? Id { get; set; }
        public string? UserName { get; set; }
        public string? TourName { get; set; }
        public byte? Stars { get; set; }
        public string? Comment { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
