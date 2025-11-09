using BE_Capstone_Project.Application.Notifications.DTOs;
using BE_Capstone_Project.DAO;
using BE_Capstone_Project.Domain.Enums;
using BE_Capstone_Project.Domain.Models;

namespace BE_Capstone_Project.Application.Notifications.Services
{
    public class NotificationService
    {
        private readonly NotificationDAO _notificationDAO;

        public NotificationService(NotificationDAO notificationDAO)
        {
            _notificationDAO = notificationDAO;
        }

        private NotificationDTO MapToDto(Notification n)
        {
            return new NotificationDTO
            {
                Id = n.Id,
                // ĐÃ SỬA: Khớp với Model (UserId chữ d thường)
                UserId = n.UserId,
                Title = n.Title,
                Message = n.Message,
                CreatedDate = n.CreatedDate,
                NotificationType = n.NotificationType,
                Username = n.User?.Username,
                IsRead = n.IsRead
            };
        }

        public async Task<IEnumerable<NotificationDTO>> GetAllAsync()
        {
            var list = await _notificationDAO.GetAllNotificationsAsync();
            return list.Select(MapToDto);
        }

        public async Task<NotificationDTO?> GetByIdAsync(int id)
        {
            var n = await _notificationDAO.GetNotificationByIdAsync(id);
            if (n == null) return null;

            return MapToDto(n);
        }

        public async Task<int> CreateAsync(CreateNotificationDTO dto)
        {
            var newNoti = new Notification
            {
                // ĐÃ SỬA: Khớp với Model (UserId chữ d thường)
                UserId = dto.UserId,
                Title = dto.Title,
                Message = dto.Message,
                NotificationType = dto.NotificationType ?? NotificationType.System,
                CreatedDate = DateTime.UtcNow,
                IsRead = false
            };

            return await _notificationDAO.AddNotificationAsync(newNoti);
        }

        public async Task<bool> UpdateAsync(int id, CreateNotificationDTO dto)
        {
            var existing = await _notificationDAO.GetNotificationByIdAsync(id);
            if (existing == null) return false;

            existing.Title = dto.Title;
            existing.Message = dto.Message;
            existing.NotificationType = dto.NotificationType ?? existing.NotificationType;

            return await _notificationDAO.UpdateNotificationAsync(existing);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _notificationDAO.DeleteNotificationByIdAsync(id);
        }

        public async Task<IEnumerable<NotificationDTO>> GetByUserIdAsync(int userId, bool? isRead = null)
        {
            var list = await _notificationDAO.GetNotificationsByUserIdAsync(userId, isRead);
            return list.Select(MapToDto);
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _notificationDAO.GetUnreadCountAsync(userId);
        }

        public async Task<bool> MarkAllAsReadAsync(int userId)
        {
            return await _notificationDAO.MarkAllAsReadAsync(userId);
        }
    }
}