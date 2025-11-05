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

        public async Task<IEnumerable<NotificationDTO>> GetAllAsync()
        {
            var list = await _notificationDAO.GetAllNotificationsAsync();
            return list.Select(n => new NotificationDTO
            {
                Id = n.Id,
                UserId = n.UserId,
                Title = n.Title,
                Message = n.Message,
                CreatedDate = n.CreatedDate,
                NotificationType = n.NotificationType,
                Username = n.User?.Username
            });
        }

        public async Task<NotificationDTO?> GetByIdAsync(int id)
        {
            var n = await _notificationDAO.GetNotificationByIdAsync(id);
            if (n == null) return null;

            return new NotificationDTO
            {
                Id = n.Id,
                UserId = n.UserId,
                Title = n.Title,
                Message = n.Message,
                CreatedDate = n.CreatedDate,
                NotificationType = n.NotificationType,
                Username = n.User?.Username
            };
        }

        public async Task<int> CreateAsync(CreateNotificationDTO dto)
        {
            var newNoti = new Notification
            {
                UserId = dto.UserId,
                Title = dto.Title,
                Message = dto.Message,
                NotificationType = dto.NotificationType ?? NotificationType.System,
                CreatedDate = DateTime.UtcNow
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

        public async Task<IEnumerable<NotificationDTO>> GetByUserIdAsync(int userId)
        {
            var list = await _notificationDAO.GetNotificationsByUserIdAsync(userId);
            return list.Select(n => new NotificationDTO
            {
                Id = n.Id,
                UserId = n.UserId,
                Title = n.Title,
                Message = n.Message,
                CreatedDate = n.CreatedDate,
                NotificationType = n.NotificationType,
                Username = n.User?.Username
            });
        }
    }
}
