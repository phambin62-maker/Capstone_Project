using BE_Capstone_Project.Infrastructure;
using BE_Capstone_Project.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BE_Capstone_Project.DAO
{
    public class TourImageDAO
    {
        private readonly OtmsdbContext _context;
        public TourImageDAO(OtmsdbContext context)
        {
            _context = context;
        }

        public async Task<int> AddTourImageAsync(TourImage tourImage)
        {
            try
            {
                await _context.TourImages.AddAsync(tourImage);
                await _context.SaveChangesAsync();
                return tourImage.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while adding a tour image: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> AddTourImagesAsync(List<TourImage> tourImages)
        {
            try
            {
                if (tourImages == null || tourImages.Count == 0) return 0;

                await _context.TourImages.AddRangeAsync(tourImages);
                int addedCount = await _context.SaveChangesAsync();

                return addedCount;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while adding tour images: {ex.Message}");
                return -1;
            }
        }

        public async Task<bool> UpdateTourImageAsync(TourImage tourImage)
        {
            try
            {
                _context.TourImages.Update(tourImage);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while updating the tour image with ID {tourImage.Id}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteTourImageByIdAsync(int tourImageId)
        {
            try
            {
                var tourImage = await _context.TourImages.FindAsync(tourImageId);
                if (tourImage != null)
                {
                    _context.TourImages.Remove(tourImage);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting the tour image with ID {tourImageId}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteTourImagesByTourIdAsync(int tourId)
        {
            try
            {
                var tourImages = await _context.TourImages.Where(ti => ti.TourId == tourId).ToListAsync();

                if (tourImages == null || tourImages.Count == 0) return true;

                _context.TourImages.RemoveRange(tourImages);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting the tour images for tour with ID {tourId}: {ex.Message}");
                return false;
            }
        }

        public async Task<List<TourImage>> GetAllTourImagesAsync()
        {
            try
            {
                return await _context.TourImages.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving all tour images: {ex.Message}");
                return new List<TourImage>();
            }
        }

        public async Task<TourImage?> GetTourImageByIdAsync(int tourImageId)
        {
            try
            {
                return await _context.TourImages.FindAsync(tourImageId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving the tour image with ID {tourImageId}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<TourImage>> GetTourImagesByTourIdAsync(int tourId)
        {
            try
            {
                return await _context.TourImages
                    .Where(ti => ti.TourId == tourId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving tour images for tour ID {tourId}: {ex.Message}");
                return new List<TourImage>();
            }
        }
    }
}
