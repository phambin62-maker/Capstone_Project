using System;
using BE_Capstone_Project.Domain.Enums;
using BE_Capstone_Project.Domain.Models;
using BE_Capstone_Project.Infrastructure;
using Microsoft.EntityFrameworkCore;
using static BE_Capstone_Project.Application.Auth.DTOs.UserDTOs;

namespace BE_Capstone_Project.DAO
{
    public class UserDAO
    {
        private readonly OtmsdbContext _context;
        public UserDAO(OtmsdbContext context)
        {
            _context = context;
        }

        public async Task<int> AddUser(User user)
        {
            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return user.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while adding a user: {ex.Message}");
                return -1;
            }
        }
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> UpdateUser(UpdateUserDto request)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.Id);
                if (user == null)
                    return false;

                // Cập nhật các thông tin mới
                user.FirstName = request.FirstName ?? user.FirstName;
                user.LastName = request.LastName ?? user.LastName;
                user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;

                if (!string.IsNullOrEmpty(request.NewPassword))
                {
                    user.PasswordHash = request.NewPassword;
                }

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DAO] UpdateUser error: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> UpdateStaff(User updatedStaff)
        {
            try
            {
                var existingStaff = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == updatedStaff.Id && u.RoleId == (int)RoleType.Staff);

                if (existingStaff == null)
                    return false;

                existingStaff.FirstName = updatedStaff.FirstName ?? existingStaff.FirstName;
                existingStaff.LastName = updatedStaff.LastName ?? existingStaff.LastName;
                existingStaff.Email = updatedStaff.Email ?? existingStaff.Email;
                existingStaff.PhoneNumber = updatedStaff.PhoneNumber ?? existingStaff.PhoneNumber;
                existingStaff.Image = updatedStaff.Image ?? existingStaff.Image;
                if (!string.IsNullOrEmpty(updatedStaff.PasswordHash))
                    existingStaff.PasswordHash = updatedStaff.PasswordHash;
                if (updatedStaff.UserStatus.HasValue)
                    existingStaff.UserStatus = updatedStaff.UserStatus;

                _context.Users.Update(existingStaff);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UserDAO] UpdateStaff error: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> HasRelatedBookings(int userId)
        {
            try
            {
                return await _context.Bookings.AnyAsync(b => b.UserId == userId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UserDAO] Error checking related bookings: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteUserById(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return false;

                
                
                // Delete Reviews (has FK to Booking, so delete first)
                var reviews = await _context.Reviews.Where(r => r.UserId == userId).ToListAsync();
                if (reviews.Any())
                {
                    _context.Reviews.RemoveRange(reviews);
                }

                //  Delete Bookings (has FK to TourSchedule, but we only delete user's bookings)
                var bookings = await _context.Bookings.Where(b => b.UserId == userId).ToListAsync();
                if (bookings.Any())
                {
                    // Delete BookingCustomers related to these bookings
                    var bookingIds = bookings.Select(b => b.Id).ToList();
                    var bookingCustomers = await _context.BookingCustomers
                        .Where(bc => bookingIds.Contains(bc.BookingId))
                        .ToListAsync();
                    if (bookingCustomers.Any())
                    {
                        _context.BookingCustomers.RemoveRange(bookingCustomers);
                    }
                    
                    _context.Bookings.RemoveRange(bookings);
                }

                //  Delete Notifications
                var notifications = await _context.Notifications.Where(n => n.UserId == userId).ToListAsync();
                if (notifications.Any())
                {
                    _context.Notifications.RemoveRange(notifications);
                }

                // Delete News
                var news = await _context.News.Where(n => n.UserId == userId).ToListAsync();
                if (news.Any())
                {
                    _context.News.RemoveRange(news);
                }

                //  Delete Wishlists
                var wishlists = await _context.Wishlists.Where(w => w.UserId == userId).ToListAsync();
                if (wishlists.Any())
                {
                    _context.Wishlists.RemoveRange(wishlists);
                }

                //  Delete Chats (both as Customer and Staff)
                var chats = await _context.Chats
                    .Where(c => c.CustomerId == userId || c.StaffId == userId)
                    .ToListAsync();
                if (chats.Any())
                {
                    _context.Chats.RemoveRange(chats);
                }

                // Save all deletions
                await _context.SaveChangesAsync();

                //  Finally, delete the User
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting the user with ID {userId}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        public async Task<List<User>> GetAllUsers()
        {
            try
            {
                return await _context.Users.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [UserDAO] Error in GetAllUsers: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"🔍 Inner: {ex.InnerException.Message}");
                return new List<User>();
            }
        }


        public async Task<User?> GetUserById(int userId)
        {
            try
            {
                return await _context.Users.FindAsync(userId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving the user with ID {userId}: {ex.Message}");
                return null;
            }
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            try
            {
                return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving the user with email {email}: {ex.Message}");
                return null;
            }
        }

        public async Task<User?> GetUserByUsername(string username)
        {
            try
            {
                return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving the user with username {username}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<User>> GetUsersByRoleId(int roleId)
        {
            try
            {
                return await _context.Users.Where(u => u.RoleId == roleId).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving users with role id {roleId}: {ex.Message}");
                return new List<User>();
            }
        }

        public async Task<List<User>> GetActiveUsers()
        {
            try
            {
                return await _context.Users.Where(u => u.UserStatus == UserStatus.Active).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving active users: {ex.Message}");
                return new List<User>();
            }
        }

        public async Task<bool> IsUsernameExists(string username)
        {
            try
            {
                return await _context.Users.AnyAsync(u => u.Username == username);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while checking if username {username} exists: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> IsEmailExists(string email)
        {
            try
            {
                return await _context.Users.AnyAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while checking if email {email} exists: {ex.Message}");
                return false;
            }
        }
        

        // Get account statistics
        public async Task<(int Total, int Active, int Inactive)> GetAccountStatisticsAsync()
        {
            try
            {
                var now = DateTime.Now;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);

                var total = await _context.Users.CountAsync();
                var active = await _context.Users.CountAsync(u => u.UserStatus == UserStatus.Active);
                var inactive = await _context.Users.CountAsync(u => u.UserStatus == UserStatus.Banned);
                return (total, active, inactive);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UserDAO] Error in GetAccountStatisticsAsync: {ex.Message}");
                return (0, 0, 0);
            }
        }

        // Get filtered accounts with pagination
        public async Task<(List<User> Users, int TotalCount)> GetFilteredAccountsAsync(
            int? roleId = null,
            UserStatus? status = null,
            string? search = null,
            int page = 1,
            int pageSize = 10)
        {
            try
            {
                var query = _context.Users.AsQueryable();

                // Filter by role
                if (roleId.HasValue)
                {
                    query = query.Where(u => u.RoleId == roleId.Value);
                }

                // Filter by status
                if (status.HasValue)
                {
                    query = query.Where(u => u.UserStatus == status.Value);
                }

                // Search by name or email
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.ToLower();
                    query = query.Where(u =>
                        (u.FirstName != null && u.FirstName.ToLower().Contains(search)) ||
                        (u.LastName != null && u.LastName.ToLower().Contains(search)) ||
                        (u.Email != null && u.Email.ToLower().Contains(search)) ||
                        (u.Username != null && u.Username.ToLower().Contains(search))
                    );
                }

                var totalCount = await query.CountAsync();

                var users = await query
                    .OrderByDescending(u => u.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (users, totalCount);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UserDAO] Error in GetFilteredAccountsAsync: {ex.Message}");
                return (new List<User>(), 0);
            }
        }

        // Update account
        public async Task<bool> UpdateAccountAsync(User user)
        {
            try
            {
                var existingUser = await _context.Users.FindAsync(user.Id);
                if (existingUser == null)
                    return false;

                existingUser.FirstName = user.FirstName ?? existingUser.FirstName;
                existingUser.LastName = user.LastName ?? existingUser.LastName;
                existingUser.Email = user.Email ?? existingUser.Email;
                existingUser.PhoneNumber = user.PhoneNumber ?? existingUser.PhoneNumber;
                existingUser.RoleId = user.RoleId;
                
                if (!string.IsNullOrEmpty(user.PasswordHash))
                    existingUser.PasswordHash = user.PasswordHash;

                _context.Users.Update(existingUser);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UserDAO] Error in UpdateAccountAsync: {ex.Message}");
                return false;
            }
        }

        // Set account status (for all account types, not just staff)
        public async Task<bool> SetAccountStatusAsync(int userId, bool isActive)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return false;

                user.UserStatus = isActive ? UserStatus.Active : UserStatus.Banned;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UserDAO] Error in SetAccountStatusAsync: {ex.Message}");
                return false;
            }
        }
    }
}
