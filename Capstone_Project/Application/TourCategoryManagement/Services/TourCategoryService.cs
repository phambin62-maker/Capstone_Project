using BE_Capstone_Project.Application.TourCategoryManagement.Services.Interfaces;
using BE_Capstone_Project.DAO;
using BE_Capstone_Project.Domain.Models;

namespace BE_Capstone_Project.Application.TourCategoryManagement.Services
{
    public class TourCategoryService : ITourCategoryService
    {
        private readonly TourCategoryDAO _tourCategoryDAO;

        public TourCategoryService(TourCategoryDAO tourCategoryDAO)
        {
            _tourCategoryDAO = tourCategoryDAO;
        }

        public async Task<List<TourCategory>> GetAllTourCategories()
        {
            return await _tourCategoryDAO.GetAllTourCategories();
        }
    }
}
