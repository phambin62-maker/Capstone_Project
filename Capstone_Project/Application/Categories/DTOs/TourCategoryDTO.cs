namespace BE_Capstone_Project.Application.Categories.DTOs
{
    public class TourCategoryDTO
    {
        public int Id { get; set; }
        public string? CategoryName { get; set; }
    }

    public class CreateTourCategoryDTO
    {
        public string? CategoryName { get; set; }
    }

    public class UpdateTourCategoryDTO
    {
        public string? CategoryName { get; set; }
    }
}
