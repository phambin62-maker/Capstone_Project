using BE_Capstone_Project.Domain.Models;

namespace BE_Capstone_Project.Application.TourManagement.Services.Interfaces
{
    public interface ITourImageService
    {
        Task<int> AddTourImage(TourImage tourImage);
        Task<int> AddTourImages(List<TourImage> tourImages);
        Task<bool> UpdateTourImage(TourImage tourImage);
        Task<bool> DeleteTourImage(int id);
        Task<bool> DeleteTourImagesByTourId(int tourId);
        Task<List<TourImage>> GetTourImagesByTourId(int tourId);
        Task<TourImage?> GetTourImageById(int id);
    }
}
