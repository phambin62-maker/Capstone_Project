namespace BE_Capstone_Project.Application.ReviewManagement.DTOs
{
    public class TourRatingDTO
    {
        public int TourId { get; set; }
        public string? TourName { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public decimal AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public string? Image { get; set; }
    }
}
