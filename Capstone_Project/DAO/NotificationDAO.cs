using BE_Capstone_Project.Infrastructure;
using BE_Capstone_Project.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BE_Capstone_Project.DAO
{
    public class NotificationDAO
    {
        private readonly OtmsdbContext _context;
        public NotificationDAO(OtmsdbContext context)
        {
            _context = context;
        }

        // (Các hàm Add, Update, Delete... giữ nguyên)
        public async Task<int> AddNotificationAsync(Notification notification)
        {
            try
            {
                await _context.Notifications.AddAsync(notification);
                await _context.SaveChangesAsync();
                return notification.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while adding a notification: {ex.Message}");
                return -1;
            }
        }
        public async Task<bool> UpdateNotificationAsync(Notification notification)
        {
            try
            {
                _context.Notifications.Update(notification);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while updating the notification with ID {notification.Id}: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> DeleteNotificationByIdAsync(int notificationId)
        {
            try
            {
                var notification = await _context.Notifications.FindAsync(notificationId);
                if (notification != null)
                {
                    _context.Notifications.Remove(notification);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting the notification with ID {notificationId}: {ex.Message}");
                return false;
            }
        }

        // (Các hàm "ĐỌC" (Get) phải có .AsNoTracking() - Đã đúng)
        public async Task<List<Notification>> GetAllNotificationsAsync()
        {
            try
            {
                return await _context.Notifications.Include(n => n.User).AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving all notifications: {ex.Message}");
                return new List<Notification>();
            }
        }
        public async Task<Notification?> GetNotificationByIdAsync(int notificationId)
        {
            try
            {
                return await _context.Notifications.Include(n => n.User).AsNoTracking().FirstOrDefaultAsync(n => n.Id == notificationId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving the notification with ID {notificationId}: {ex.Message}");
                return null;
            }
        }
        public async Task<List<Notification>> GetNotificationsByUserIdAsync(int userId, bool? isRead = null)
        {
            try
            {
                var query = _context.Notifications.Include(n => n.User).Where(n => n.UserId == userId).AsNoTracking().AsQueryable();
                if (isRead.HasValue)
                {
                    query = query.Where(n => n.IsRead == isRead.Value);
                }
                return await query.OrderByDescending(n => n.CreatedDate).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving notifications for user ID {userId}: {ex.Message}");
                return new List<Notification>();
            }
        }
        public async Task<int> GetUnreadCountAsync(int userId)
        {
            try
            {
                return await _context.Notifications.AsNoTracking().Where(n => n.UserId == userId && n.IsRead == false).CountAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while counting unread notifications for user ID {userId}: {ex.Message}");
                return 0;
            }
        }
        public async Task<List<Notification>> GetRecentNotificationsAsync(int userId, int count = 5)
        {
            try
            {
                return await _context.Notifications.Include(n => n.User).Where(n => n.UserId == userId).AsNoTracking().OrderByDescending(n => n.CreatedDate).Take(count).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving recent notifications for user ID {userId}: {ex.Message}");
                return new List<Notification>();
            }
        }

        // (Hàm click-để-đọc, giữ nguyên)
        public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
        {
            try
            {
                var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
                if (notification == null || notification.IsRead)
                {
                    return false;
                }
                notification.IsRead = true;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking notification {notificationId} as read: {ex.Message}");
                return false;
            }
        }

        // === BẮT ĐẦU SỬA LỖI ===
        // (Sửa hàm này để dùng ExecuteUpdateAsync, hiệu quả hơn và tránh lỗi cache)
        public async Task<bool> MarkAllAsReadAsync(int userId)
        {
            try
            {
                // Dùng ExecuteUpdateAsync để cập nhật thẳng xuống DB
                // mà không cần tải (Load) bất cứ thứ gì vào bộ đệm (cache)
                await _context.Notifications
                    .Where(n => n.UserId == userId && n.IsRead == false)
                    .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true)); // Đặt IsRead = true

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while marking all notifications as read for user ID {userId}: {ex.Message}");
                return false;
            }
        }
        
        public async Task<bool> DeleteAllByUserIdAsync(int userId)
        {
            try
            {
                // Dùng ExecuteDeleteAsync để xóa thẳng xuống DB
                // (Hiệu quả, không gây lỗi cache)
                await _context.Notifications
                    .Where(n => n.UserId == userId)
                    .ExecuteDeleteAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting all notifications for user ID {userId}: {ex.Message}");
                return false;
            }
        }
    }
}