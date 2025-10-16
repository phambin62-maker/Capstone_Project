namespace BE_Capstone_Project.DTOs
{
    public class TourPopularityDto
    {
        public int TourId { get; set; }
        public string TourName { get; set; } = string.Empty;
        public int BookingsCount { get; set; }
        public decimal? Revenue { get; set; }
    }
}
