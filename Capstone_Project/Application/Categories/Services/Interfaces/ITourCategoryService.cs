using BE_Capstone_Project.Application.Categories.DTOs;
using BE_Capstone_Project.Application.Locations.Services.Interfaces;

namespace BE_Capstone_Project.Application.Categories.Services.Interfaces
{
    public interface ITourCategoryService
    {
        Task<ServiceResult<TourCategoryDTO>> GetTourCategoryByIdAsync(int id);
        Task<ServiceResult<List<TourCategoryDTO>>> GetAllTourCategoriesAsync();
        Task<ServiceResult<int>> CreateTourCategoryAsync(CreateTourCategoryDTO createDto);
        Task<ServiceResult<bool>> UpdateTourCategoryAsync(int id, UpdateTourCategoryDTO updateDto);
        Task<ServiceResult<bool>> DeleteTourCategoryAsync(int id);
    }
}
