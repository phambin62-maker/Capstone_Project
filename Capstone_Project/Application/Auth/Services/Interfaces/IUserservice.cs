using BE_Capstone_Project.Application.Auth.DTOs;
using BE_Capstone_Project.Domain.Models;
using System.Threading.Tasks;
using static BE_Capstone_Project.Application.Auth.DTOs.UserDTOs;

namespace BE_Capstone_Project.Application.Services
{
    public interface IUserService
    {
        Task<bool> UpdateUserAsync(UpdateUserDto request);
        Task<(bool Success, string Message, int? UserId,string? Token)> SyncGoogleUserAsync(GoogleUserDto dto);
        Task<User?> GetUserByUsername(string username);
        Task<List<User>> GetAllUsers();
    }
}
