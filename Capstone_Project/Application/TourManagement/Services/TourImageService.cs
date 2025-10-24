using BE_Capstone_Project.Application.TourManagement.Services.Interfaces;
using BE_Capstone_Project.DAO;
using BE_Capstone_Project.Domain.Models;

namespace BE_Capstone_Project.Application.TourManagement.Services
{
    public class TourImageService : ITourImageService
    {
        private readonly TourImageDAO _tourImageDAO;

        public TourImageService(TourImageDAO tourImageDAO)
        {
            _tourImageDAO = tourImageDAO;
        }

        public async Task<int> AddTourImage(TourImage tourImage)
        {
            return await _tourImageDAO.AddTourImageAsync(tourImage);
        }

        public async Task<int> AddTourImages(List<TourImage> tourImages)
        {
            return await _tourImageDAO.AddTourImagesAsync(tourImages);
        }

        public async Task<bool> UpdateTourImage(TourImage tourImage)
        {
            return await _tourImageDAO.UpdateTourImageAsync(tourImage);
        }

        public async Task<bool> DeleteTourImage(int id)
        {
            return await _tourImageDAO.DeleteTourImageByIdAsync(id);
        }

        public async Task<bool> DeleteTourImagesByTourId(int tourId)
        {
            return await _tourImageDAO.DeleteTourImagesByTourIdAsync(tourId);
        }

        public async Task<List<TourImage>> GetTourImagesByTourId(int tourId)
        {
            return await _tourImageDAO.GetTourImagesByTourIdAsync(tourId);
        }
    }
}
