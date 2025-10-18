using BE_Capstone_Project.Domain.Models;

namespace BE_Capstone_Project.Application.TourManagement.Services.Interfaces
{
    public interface ITourImageService
    {
        Task<int> AddTourImage(TourImage tourImage);
        Task<bool> DeleteTourImage(int id);

        Task<List<TourImage>> GetTourImagesByTourId(int tourId);
    }
}
