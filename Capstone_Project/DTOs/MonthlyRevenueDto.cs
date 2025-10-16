namespace BE_Capstone_Project.DTOs
{
    public class MonthlyRevenueDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalRevenue { get; set; }
        public int BookingsCount { get; set; }
    }
}
