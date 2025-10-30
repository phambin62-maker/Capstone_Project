using BE_Capstone_Project.Application.CancelConditions.DTOs;
using BE_Capstone_Project.Domain.Enums;

namespace BE_Capstone_Project.Application.CancelConditions.Services.Interfaces
{
    public interface ICancelConditionService
    {
        Task<List<CancelConditionDTO>> GetAllAsync();
        Task<CancelConditionDTO?> GetByIdAsync(int id);
        Task<List<CancelConditionDTO>> GetByStatusAsync(CancelStatus status);
        Task<int> CreateAsync(CancelConditionCreateDTO dto);
        Task<bool> UpdateAsync(CancelConditionUpdateDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}
