using BE_Capstone_Project.Domain.Enums;

namespace BE_Capstone_Project.Application.Newses.DTOs
{
 
    public class NewsDTO
    {
        public int Id { get; set; }                 
        public string? Title { get; set; }          
        public string? Image { get; set; }           
        public DateTime? CreatedDate { get; set; }   
        public NewsStatus? NewsStatus { get; set; }  
        public string? AuthorName { get; set; }
        public string Content { get; set; }
        public int UserId { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedAuthor { get; set; }
    }
}
