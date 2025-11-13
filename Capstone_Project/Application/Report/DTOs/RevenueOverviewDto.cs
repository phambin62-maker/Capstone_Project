namespace BE_Capstone_Project.Application.Report.DTOs
{
    public class RevenueOverviewDto
    {
        public DateOnly From { get; set; }
        public DateOnly To { get; set; }
        public long TotalBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public int UniqueBookingUsers { get; set; }
        public int UniqueCustomers { get; set; }

        public decimal AverageRating { get; set; }
        public List<TourRevenueDto> TopToursByRevenue { get; set; } = new();
        public List<TourPopularityDto> TopToursByBookings { get; set; } = new();

    }
}
