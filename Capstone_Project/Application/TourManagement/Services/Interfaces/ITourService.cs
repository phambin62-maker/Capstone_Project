using BE_Capstone_Project.Domain.Models;

namespace BE_Capstone_Project.Application.TourManagement.Services.Interfaces
{
    public interface ITourService
    {
        Task<int> AddTour(Tour tour);
        Task<bool> UpdateTour(Tour tour);
        Task<bool> DeleteTour(int tourId);
        Task<List<Tour>> GetAllTours();
        Task<Tour?> GetTourById(int id);
        Task<List<Tour>> GetToursByCategoryId(int categoryId);
        Task<List<Tour>> GetToursByStartLocationId(int startLocationId);
        Task<List<Tour>> GetToursByEndLocationId(int endLocationId);
        Task<List<Tour>> GetToursByPriceRange(decimal minPrice, decimal maxPrice);
        Task<List<Tour>> SearchTourByName(string name);
        Task<int> GetTotalTourCount();
        Task<List<Tour>> GetPaginatedTours(int page = 1, int pageSize = 10);
    }
}
