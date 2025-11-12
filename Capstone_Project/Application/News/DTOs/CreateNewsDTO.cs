using BE_Capstone_Project.Domain.Enums;

namespace BE_Capstone_Project.Application.Newses.DTOs
{
    public class CreateNewsDTO
    {
        public int UserId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Image { get; set; }

        public NewsStatus? NewsStatus { get; set; } 
    }

    public class CreateNewsFormDTO
    {
        public int UserId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public IFormFile? ImageFile { get; set; } 
        public NewsStatus? NewsStatus { get; set; }
    }

    public class EditNewsFormDTO
    {
        public int Id { get; set; } 
        public int UserId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public IFormFile? ImageFile { get; set; }
        public NewsStatus? NewsStatus { get; set; }
    }
}
