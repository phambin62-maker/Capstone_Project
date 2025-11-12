using BE_Capstone_Project.Application.Notifications.DTOs;
using BE_Capstone_Project.DAO;
using BE_Capstone_Project.Domain.Enums;
using BE_Capstone_Project.Domain.Models;
using System; // Thêm
using System.Collections.Generic; // Thêm
using System.Linq; // Thêm
using System.Threading.Tasks; // Thêm

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

        // === HÀM MỚI (Sửa lỗi hiệu năng) ===
        public async Task<IEnumerable<NotificationDTO>> GetRecentAsync(int userId)
        {
            // Gọi hàm DAO mới (chỉ lấy 5 cái)
            var list = await _notificationDAO.GetRecentNotificationsAsync(userId, 5);
            return list.Select(MapToDto);
        }

        // === HÀM MỚI (Sửa lỗi "Click để đọc") ===
        public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
        {
            // Chúng ta chuyển cả 2 ID cho DAO để bảo mật
            return await _notificationDAO.MarkAsReadAsync(notificationId, userId);
        }
        public async Task<bool> DeleteAllByUserIdAsync(int userId)
        {
            return await _notificationDAO.DeleteAllByUserIdAsync(userId);
        }
    }
}