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

        public async Task<bool> DeleteUserById(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting the user with ID {userId}: {ex.Message}");
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
                Console.WriteLine($"An error occurred while retrieving all users: {ex.Message}");
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
    }
}
