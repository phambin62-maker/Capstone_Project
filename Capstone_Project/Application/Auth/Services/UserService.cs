using BE_Capstone_Project.Application.Auth.DTOs;
using BE_Capstone_Project.Application.Auth.Services;
using BE_Capstone_Project.DAO;
using BE_Capstone_Project.Domain.Enums;
using BE_Capstone_Project.Domain.Models;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static BE_Capstone_Project.Application.Auth.DTOs.UserDTOs;

namespace BE_Capstone_Project.Application.Services
{
    public class UserService : IUserService
    {
        private readonly UserDAO _userDAO;
        private readonly ILogger<UserService> _logger;
        private readonly AuthService _authService;
        public UserService(UserDAO userDAO, ILogger<UserService> logger, AuthService authService)
        {
            _userDAO = userDAO;
            _logger = logger;
            _authService = authService;
        }

        public async Task<bool> UpdateUserAsync(UpdateUserDto request)
        {
            try
            {
                var user = await _userDAO.GetUserById(request.Id);
                if (user == null)
                    return (false);

                if (!string.IsNullOrEmpty(request.NewPassword))
                {
                    request.NewPassword = HashPassword(request.NewPassword);
                }

                return await _userDAO.UpdateUser(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Service] UpdateUserAsync error: {ex.Message}");
                return false;
            }
        }
        public async Task<(bool Success, string Message, int? UserId, string? Token)> SyncGoogleUserAsync(GoogleUserDto dto)
        {
            try
            {
                if (string.IsNullOrEmpty(dto.Email))
                {
                    _logger.LogWarning("Yêu cầu đồng bộ Google thiếu email.");
                    return (false, "Thiếu email", null, null);
                }

                // Kiểm tra user tồn tại
                var existingUser = await _userDAO.GetByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    _logger.LogInformation($"User Google đã tồn tại: {dto.Email} (ID={existingUser.Id})");

                    // Check if account is banned
                    if (existingUser.UserStatus == UserStatus.Banned)
                    {
                        _logger.LogWarning($"User Google bị banned cố gắng đăng nhập: {dto.Email}");
                        return (false, "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên để được hỗ trợ.", null, null);
                    }

                    // Check if account is active
                    if (existingUser.UserStatus != UserStatus.Active)
                    {
                        _logger.LogWarning($"User Google chưa được kích hoạt: {dto.Email}");
                        return (false, "Tài khoản của bạn chưa được kích hoạt. Vui lòng liên hệ quản trị viên.", null, null);
                    }
                    var emailPart = dto.Email.Split('@');
                    var Baseusername = emailPart[0];
                    var userName = Baseusername;
                    // Nếu user Google đã tồn tại nhưng không có Username, tạo username từ email
                    if (string.IsNullOrEmpty(existingUser.Username))
                    {
                        
                        
                        // Đảm bảo username là unique
                        var counters = 1;
                        while (await _userDAO.IsUsernameExists(userName))
                        {
                            userName = $"{Baseusername}{counters}";
                            counters++;
                        }
                        
                        // Update trực tiếp trong database
                        existingUser.Username = userName;
                        existingUser.Provider = dto.Provider ?? "Google";
                        
                        // Sử dụng UpdateAccountAsync để update Username
                        var updatedUser = new User
                        {
                            Id = existingUser.Id,
                            Username = userName,
                            FirstName = existingUser.FirstName,
                            LastName = existingUser.LastName,
                            Email = existingUser.Email,
                            PhoneNumber = existingUser.PhoneNumber,
                            RoleId = existingUser.RoleId,
                            Provider = dto.Provider ?? "Google"
                        };
                        
                        await _userDAO.UpdateAccountAsync(updatedUser);
                        
                        // Lấy lại user sau khi update
                        existingUser = await _userDAO.GetByEmailAsync(dto.Email);
                        _logger.LogInformation($"Đã tạo username '{userName}' cho user Google: {dto.Email}");
                    }

                    // Sinh token JWT cho user đã tồn tại
                    var existingToken = _authService.GenerateJwtToken(existingUser);

                    return (true, "User đã tồn tại", existingUser.Id, existingToken);
                }
                // nếu chưa có tài khoản trong db thì thêm mới tài khoản gg vào trong db 
                var emailParts = dto.Email.Split('@');
                var baseUsername = emailParts[0];
                var username = baseUsername;
                var counter = 1;
                while (await _userDAO.IsUsernameExists(username))
                {
                    username = $"{baseUsername}{counter}";
                    counter++;
                }
                var user = new User
                {
                    Username = username,
                    Email = dto.Email,
                    FirstName = dto.FullName?.Split(' ').FirstOrDefault() ?? "",
                    LastName = dto.FullName?.Split(' ').LastOrDefault() ?? "",
                    RoleId = (int)RoleType.Customer,
                    UserStatus = UserStatus.Active,
                    Provider = dto.Provider ?? "Google",
                    // PasswordHash có thể null cho Google user vì họ không dùng password
                    PasswordHash = null
                };

                await _userDAO.CreateAsync(user);
                _logger.LogInformation($"Tạo user Google mới thành công: {dto.Email}");

                // Lấy lại user sau khi tạo để sinh token
                var createdUser = await _userDAO.GetByEmailAsync(dto.Email);
                if (createdUser == null)
                {
                    _logger.LogError($"Không thể truy xuất user sau khi tạo: {dto.Email}");
                    return (false, "Lỗi khi truy xuất user vừa tạo", null, null);
                }

                var token = _authService.GenerateJwtToken(createdUser);

                return (true, "Tạo user mới thành công", createdUser.Id, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi đồng bộ user Google: {dto.Email}");
                return (false, "Đồng bộ thất bại", null, null);
            }
        }



        public async Task<User?> GetUserByUsername(string username)
        {
            return await _userDAO.GetUserByUsername(username);
        }

        public async Task<List<User>> GetAllUsers()
        {
            return await _userDAO.GetAllUsers();
        }

        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
        public bool VerifyPassword(string inputPassword, string storedHash)
        {
            var inputHash = HashPassword(inputPassword);
            return inputHash == storedHash;
        }
    }
}
