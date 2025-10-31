namespace BE_Capstone_Project.Application.ReviewManagement.DTOs
{
    public class ReviewCreateDTO
    {
        public string Username { get; set; } = string.Empty;
        public int TourId { get; set; }
        public byte Stars { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}
