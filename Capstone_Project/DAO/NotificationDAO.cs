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

        public async Task<List<Notification>> GetAllNotificationsAsync()
        {
            try
            {
                return await _context.Notifications.ToListAsync();
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
                return await _context.Notifications.FindAsync(notificationId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving the notification with ID {notificationId}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Notification>> GetNotificationsByUserIdAsync(int userId)
        {
            try
            {
                return await _context.Notifications
                    .Where(n => n.UserId == userId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving notifications for user ID {userId}: {ex.Message}");
                return new List<Notification>();
            }
        }
    }
}
