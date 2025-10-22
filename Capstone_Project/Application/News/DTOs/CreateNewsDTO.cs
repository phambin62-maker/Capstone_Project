using BE_Capstone_Project.Domain.Enums;

namespace BE_Capstone_Project.Application.Newses.DTOs
{
    /// <summary>
    /// DTO dùng để tạo hoặc cập nhật tin tức.
    /// </summary>
    public class CreateNewsDTO
    {
        public int UserId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Image { get; set; }

        // Mặc định tin mới tạo là Draft (chưa đăng)
        public NewsStatus? NewsStatus { get; set; } 
    }
}
