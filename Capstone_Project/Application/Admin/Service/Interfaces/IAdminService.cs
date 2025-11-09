using BE_Capstone_Project.Application.Admin.DTOs;
using BE_Capstone_Project.Domain.Models;

namespace BE_Capstone_Project.Application.Admin.Service.Interfaces
{
    public interface IAdminService
    {
        Task<(bool Success, string Message, User? Data)> CreateAccountAsync(CreateAccountDto dto);
        Task<(bool Success, string Message)> SetStaffActiveStatusAsync(int userId, bool isActive);
        Task<List<User>> GetAllUsersAsync();
    }
}
    