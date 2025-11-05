namespace BE_Capstone_Project.Application.ReviewManagement.DTOs
{
    public class ReviewUpdateDTO
    {
        public int Id { get; set; }
        public byte Stars { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}
