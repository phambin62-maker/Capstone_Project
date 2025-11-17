namespace BE_Capstone_Project.Application.Report.DTOs
{
    public class BookingDetailDto
    {
        public int BookingId { get; set; }
        public string TourName { get; set; }
        public string CustomerName { get; set; }
        public DateTime? BookingDate { get; set; } // Sửa thành DateTime? (nullable)
        public decimal? TotalPrice { get; set; }
        public string Status { get; set; }
    }
}
