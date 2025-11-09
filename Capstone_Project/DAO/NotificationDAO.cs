using BE_Capstone_Project.Infrastructure;
using BE_Capstone_Project.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BE_Capstone_Project.DAO
{
    public class NotificationDAO
    {
        private readonly OtmsdbContext _context;
        public NotificationDAO(OtmsdbContext context)
        {
            _context = context;
        }

        public async Task<int> AddNotificationAsync(Notification notification)
        {
            try
            {
                await _context.Notifications.AddAsync(notification);
                await _context.SaveChangesAsync();
                // SỬA LỖI: Khớp với Model C# (chữ 'i' thường)
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
                // SỬA LỖI: Khớp với Model C# (chữ 'i' thường)
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

        public async Task<List<Notification>> GetAllNotificationsAsync()
        {
            try
            {
                return await _context.Notifications.Include(n => n.User).ToListAsync();
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
                // SỬA LỖI: Khớp với Model C# (chữ 'i' thường)
                return await _context.Notifications.Include(n => n.User).FirstOrDefaultAsync(n => n.Id == notificationId);
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
                var query = _context.Notifications
                    .Include(n => n.User)
                    // SỬA LỖI: Khớp với Model C# (chữ 'd' thường)
                    .Where(n => n.UserId == userId)
                    .AsQueryable();

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
                return await _context.Notifications
                    // SỬA LỖI: Khớp với Model C# (chữ 'd' thường)
                    .Where(n => n.UserId == userId && n.IsRead == false)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while counting unread notifications for user ID {userId}: {ex.Message}");
                return 0;
            }
        }

        public async Task<bool> MarkAllAsReadAsync(int userId)
        {
            try
            {
                var unreadNotifications = await _context.Notifications
                    // SỬA LỖI: Khớp với Model C# (chữ 'd' thường)
                    .Where(n => n.UserId == userId && n.IsRead == false)
                    .ToListAsync();

                if (!unreadNotifications.Any())
                {
                    return true;
                }

                unreadNotifications.ForEach(n => n.IsRead = true);

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while marking all notifications as read for user ID {userId}: {ex.Message}");
                return false;
            }
        }
    }
}