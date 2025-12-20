using BE_Capstone_Project.Application.Notifications.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BE_Capstone_Project.Application.Notifications.Services.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationDTO>> GetAllAsync();
        Task<NotificationDTO?> GetByIdAsync(int id);
        Task<IEnumerable<NotificationDTO>> GetByUserIdAsync(int userId, bool? isRead = null);
        Task<int> GetUnreadCountAsync(int userId);
        Task<bool> MarkAllAsReadAsync(int userId);
        Task<int> CreateAsync(CreateNotificationDTO dto);
        Task<bool> UpdateAsync(int id, CreateNotificationDTO dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteAllByUserIdAsync(int userId);
        Task<IEnumerable<NotificationDTO>> GetRecentAsync(int userId);
        Task<bool> MarkAsReadAsync(int notificationId, int userId);
    }
}