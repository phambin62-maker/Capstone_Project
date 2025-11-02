namespace FE_Capstone_Project.Models
{
    public class ReviewViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string TourName { get; set; } = string.Empty;
        public int Stars { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}
