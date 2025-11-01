using BE_Capstone_Project.Domain.Models;

namespace BE_Capstone_Project.Application.TourCategoryManagement.Services.Interfaces
{
    public interface ITourCategoryService
    {
        Task<List<TourCategory>> GetAllTourCategories();
    }
}
