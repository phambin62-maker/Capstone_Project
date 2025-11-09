using BE_Capstone_Project.Domain.Enums;

namespace FE_Capstone_Project.Models
{
    public class NotificationViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }
        public DateTime CreatedDate { get; set; }
        public NotificationType NotificationType { get; set; }
        public bool IsRead { get; set; }
    }
}
