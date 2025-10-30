using BE_Capstone_Project.Application.Auth.DTOs;
using BE_Capstone_Project.DAO;
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

        public UserService(UserDAO userDAO)
        {
            _userDAO = userDAO;
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
