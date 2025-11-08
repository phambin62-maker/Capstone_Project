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

        // 🧩 Tạo tài khoản staff
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
            var builder = new StringBuilder();
            foreach (var b in bytes)
                builder.Append(b.ToString("x2"));
            return builder.ToString();
        }
    }
}
