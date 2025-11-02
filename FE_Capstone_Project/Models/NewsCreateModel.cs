namespace FE_Capstone_Project.Models
{
    public class NewsCreateModel
    {
        public int UserId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public String? Image { get; set; }
        public string? NewsStatus { get; set; } 
    }

    public class EditNewsModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Image { get; set; }
        public string? ImageUrl { get; set; } 
        public string? NewsStatus { get; set; }  
    }

    public class NewsViewModel
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Image { get; set; }
        public string? AuthorName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? NewsStatus { get; set; } 
    }
}
