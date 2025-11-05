using BE_Capstone_Project.Domain.Enums;

namespace BE_Capstone_Project.Application.Notifications.DTOs
{
    public class CreateNotificationDTO
    {
        public int UserId { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }
        public NotificationType? NotificationType { get; set; }
    }
}
