using BE_Capstone_Project.Application.TourManagement.Services.Interfaces;
using BE_Capstone_Project.DAO;
using BE_Capstone_Project.Domain.Models;

namespace BE_Capstone_Project.Application.TourManagement.Services
{
    public class TourService : ITourService
    {
        private readonly TourDAO _tourDAO;

        public TourService(TourDAO tourDAO)
        {
            _tourDAO = tourDAO;
        }

        public async Task<int> AddTour(Tour tour)
        {
            return await _tourDAO.AddTourAsync(tour);
        }

        public async Task<bool> UpdateTour(Tour tour)
        {
            return await _tourDAO.UpdateTourAsync(tour);
        }

        public async Task<bool> DeleteTour(int tourId)
        {
            return await _tourDAO.DeleteTourByIdAsync(tourId);
        }

        public async Task<List<Tour>> GetAllTours()
        {
            return await _tourDAO.GetAllToursAsync();
        }

        public async Task<Tour?> GetTourById(int id)
        {
            return await _tourDAO.GetTourByIdAsync(id);
        }

        public async Task<List<Tour>> GetToursByCategoryId(int categoryId)
        {
            return await _tourDAO.GetToursByCategoryIdAsync(categoryId);
        }

        public async Task<List<Tour>> GetToursByStartLocationId(int startLocationId)
        {
            return await _tourDAO.GetToursByStartLocationIdAsync(startLocationId);
        }

        public async Task<List<Tour>> GetToursByEndLocationId(int endLocationId)
        {
            return await _tourDAO.GetToursByEndLocationIdAsync(endLocationId);
        }

        public async Task<List<Tour>> GetToursByPriceRange(decimal minPrice, decimal maxPrice)
        {
            return await _tourDAO.GetToursByPriceRangeAsync(minPrice, maxPrice);
        }

        public async Task<List<Tour>> SearchTourByName(string name)
        {
            return await _tourDAO.SearchToursByNameAsync(name);
        }

        public async Task<int> GetTotalTourCount()
        {
            return await _tourDAO.GetTotalTourCountAsync();
        }

        public async Task<List<Tour>> GetPaginatedTours(int page = 1, int pageSize = 10)
        {
            return await _tourDAO.GetPaginatedToursAsync(page, pageSize);
        }

        public async Task<List<Tour>> GetTopToursByEachCategories()
        {
            return await _tourDAO.GetTopToursByEachCategoriesAsync();
        }
    }
}
