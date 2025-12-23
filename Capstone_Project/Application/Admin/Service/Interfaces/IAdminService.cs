using BE_Capstone_Project.Application.Admin.DTOs;
using BE_Capstone_Project.Domain.Models;

namespace BE_Capstone_Project.Application.Admin.Service.Interfaces
{
    public interface IAdminService
    {
        Task<(bool Success, string Message, User? Data)> CreateAccountAsync(CreateAccountDto dto);
        Task<(bool Success, string Message)> SetStaffActiveStatusAsync(int userId, bool isActive);
        Task<List<User>> GetAllUsersAsync();
        Task<AccountStatisticsDto> GetAccountStatisticsAsync();
        Task<PagedResultDto<User>> GetFilteredAccountsAsync(int? roleId, string? status, string? search, int page, int pageSize);
        Task<User?> GetAccountByIdAsync(int id);
        Task<(bool Success, string Message)> UpdateAccountAsync(int id, UpdateAccountDto dto);
        Task<(bool Success, string Message)> DeleteAccountAsync(int id);
        Task<(bool Success, string Message)> SetAccountStatusAsync(int userId, bool isActive);
    }
}
    