using BE_Capstone_Project.Application.Admin.DTOs;
using BE_Capstone_Project.Application.Admin.Service.Interfaces;
using BE_Capstone_Project.DAO;
using BE_Capstone_Project.Domain.Enums;
using BE_Capstone_Project.Domain.Models;
using System.Security.Cryptography;
using System.Text;

namespace BE_Capstone_Project.Application.Admin.Service
{
    public class AdminService : IAdminService
    {
        private readonly UserDAO _userDao;

        public AdminService(UserDAO userDao)
        {
            _userDao = userDao;
        }

        //  Tạo tài khoản staff
        public async Task<(bool Success, string Message, User? Data)> CreateStaffAsync(CreateAccountDto dto)
        {
            if (await _userDao.IsEmailExists(dto.Email))
                return (false, "Email already exists", null);

            if (await _userDao.IsUsernameExists(dto.Username))
                return (false, "Username already exists", null);

            var hash = ComputeSha256Hash(dto.Password);

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = hash,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                RoleId = (int)RoleType.Staff,
                UserStatus = UserStatus.Active,
                Provider = "Local"
            };

            var userId = await _userDao.AddUser(user);
            if (userId <= 0)
                return (false, "Failed to create staff", null);

            return (true, "Staff created successfully", user);
        }

        //  Active/Inactive staff
        public async Task<(bool Success, string Message)> SetStaffActiveStatusAsync(int userId, bool isActive)
        {
            var staff = await _userDao.GetUserById(userId);
            if (staff == null)
                return (false, "Staff not found");

            if (staff.RoleId != (int)RoleType.Staff)
                return (false, "User is not a staff account");

            staff.UserStatus = isActive ? UserStatus.Active : UserStatus.Banned;
            await _userDao.UpdateStaff(staff);

            return (true, $"Staff {(isActive ? "activated" : "deactivated")} successfully");
        }
        public async Task<(bool Success, string Message, User? Data)> CreateAccountAsync(CreateAccountDto dto)
        {
            if (await _userDao.IsEmailExists(dto.Email))
                return (false, "Email already exists", null);

            if (await _userDao.IsUsernameExists(dto.Username))
                return (false, "Username already exists", null);

            var hash = ComputeSha256Hash(dto.Password);

            // Xác định vai trò
            RoleType roleType = dto.Role.ToLower() switch
            {
                "admin" => RoleType.Admin,
                "staff" => RoleType.Staff,
                "customer" => RoleType.Customer,
                _ => RoleType.Customer
            };

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = hash,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                RoleId = (int)roleType,
                UserStatus = dto.IsActive ? UserStatus.Active : UserStatus.Banned,
                Provider = "Local"
            };

            var userId = await _userDao.AddUser(user);
            if (userId <= 0)
                return (false, "Failed to create account", null);

            return (true, "Account created successfully", user);
        }
        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = await _userDao.GetAllUsers();
            Console.WriteLine($"[AdminService] Found {users.Count} users");
            return users;
        }

        private static string ComputeSha256Hash(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(bytes);
        }

        // Get account statistics
        public async Task<AccountStatisticsDto> GetAccountStatisticsAsync()
        {
            var (total, active, inactive) = await _userDao.GetAccountStatisticsAsync();
            
            return new AccountStatisticsDto
            {
                TotalAccounts = total,
                ActiveAccounts = active,
                InactiveAccounts = inactive
               
            };
        }

        // Get filtered accounts with pagination
        public async Task<PagedResultDto<User>> GetFilteredAccountsAsync(int? roleId, string? status, string? search, int page, int pageSize)
        {
            UserStatus? userStatus = null;
            if (!string.IsNullOrWhiteSpace(status))
            {
                userStatus = status.ToLower() switch
                {
                    "active" => UserStatus.Active,
                    "inactive" or "banned" => UserStatus.Banned,
                    _ => null
                };
            }

            var (users, totalCount) = await _userDao.GetFilteredAccountsAsync(roleId, userStatus, search, page, pageSize);
            
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedResultDto<User>
            {
                Data = users,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        // Get account by ID
        public async Task<User?> GetAccountByIdAsync(int id)
        {
            return await _userDao.GetUserById(id);
        }

        // Update account
        public async Task<(bool Success, string Message)> UpdateAccountAsync(int id, UpdateAccountDto dto)
        {
            var user = await _userDao.GetUserById(id);
            if (user == null)
                return (false, "Account not found");

            // Check if email is being changed and if it already exists
            if (user.Email != dto.Email && await _userDao.IsEmailExists(dto.Email))
                return (false, "Email already exists");
            // Determine role
            RoleType roleType = dto.Role.ToLower() switch
            {
                "admin" => RoleType.Admin,
                "staff" => RoleType.Staff,
                "customer" => RoleType.Customer,
                _ => (RoleType)user.RoleId
            };

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Email = dto.Email;
            user.PhoneNumber = dto.PhoneNumber;
            user.RoleId = (int)roleType;

            // Update password if provided
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.PasswordHash = ComputeSha256Hash(dto.Password);
            }

            var result = await _userDao.UpdateAccountAsync(user);
            if (!result)
                return (false, "Failed to update account");

            return (true, "Account updated successfully");
        }

        // Delete account (with cascade delete - removes all related data)
        public async Task<(bool Success, string Message)> DeleteAccountAsync(int id)
        {
            var user = await _userDao.GetUserById(id);
            if (user == null)
                return (false, "Account not found");

            // Prevent deleting admin accounts (optional safety check)
            if (user.RoleId == (int)RoleType.Admin)
                return (false, "Cannot delete admin account");

            try
            {
                // Delete user and all related data (cascade delete)
                var result = await _userDao.DeleteUserById(id);
                if (!result)
                    return (false, "Failed to delete account and related data");

                return (true, "Account deleted successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AdminService] Error deleting account: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[AdminService] Inner exception: {ex.InnerException.Message}");
                }
                return (false, $"An error occurred while deleting the account: {ex.Message}");
            }
        }

        // Set account status (for all account types)
        public async Task<(bool Success, string Message)> SetAccountStatusAsync(int userId, bool isActive)
        {
            var user = await _userDao.GetUserById(userId);
            if (user == null)
                return (false, "Account not found");

            var result = await _userDao.SetAccountStatusAsync(userId, isActive);
            if (!result)
                return (false, "Failed to update account status");

            return (true, $"Account {(isActive ? "activated" : "deactivated")} successfully");
        }
    }
}
