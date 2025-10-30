using BE_Capstone_Project.Domain.Enums;

namespace BE_Capstone_Project.Application.Notifications.DTOs
{
    public class NotificationDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }
        public DateTime? CreatedDate { get; set; }
        public NotificationType? NotificationType { get; set; }
        public string? Username { get; set; } 
    }
}
