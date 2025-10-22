using BE_Capstone_Project.Domain.Enums;

namespace BE_Capstone_Project.Application.Newses.DTOs
{
 
    public class NewsDTO
    {
        public int Id { get; set; }                  // Mã tin tức
        public string? Title { get; set; }           // Tiêu đề
        public string? Image { get; set; }           // Ảnh minh họa
        public DateTime? CreatedDate { get; set; }   // Ngày đăng
        public NewsStatus? NewsStatus { get; set; }  // Trạng thái tin (Draft, Published,...)
        public string? AuthorName { get; set; }      // Tên người đăng
    }
}
