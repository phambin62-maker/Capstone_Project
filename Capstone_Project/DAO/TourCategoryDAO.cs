using BE_Capstone_Project.Infrastructure;
using BE_Capstone_Project.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BE_Capstone_Project.DAO
{
    public class TourCategoryDAO
    {
        private readonly OtmsdbContext _context;
        public TourCategoryDAO(OtmsdbContext context)
        {
            _context = context;
        }

        public async Task<int> AddTourCategory(TourCategory tourCategory)
        {
            try
            {
                _context.TourCategories.Add(tourCategory);
                await _context.SaveChangesAsync();
                return tourCategory.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while adding a tour category: {ex.Message}");
                return -1;
            }
        }

        public async Task<bool> UpdateTourCategory(TourCategory tourCategory)
        {
            try
            {
                _context.TourCategories.Update(tourCategory);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while updating the tour category with ID {tourCategory.Id}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteTourCategoryById(int tourCategoryId)
        {
            try
            {
                var tourCategory = await _context.TourCategories.FindAsync(tourCategoryId);
                if (tourCategory != null)
                {
                    _context.TourCategories.Remove(tourCategory);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting the tour category with ID {tourCategoryId}: {ex.Message}");
                return false;
            }
        }

        public async Task<List<TourCategory>> GetAllTourCategories()
        {
            try
            {
                return await _context.TourCategories.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving all tour categories: {ex.Message}");
                return new List<TourCategory>();
            }
        }

        public async Task<TourCategory?> GetTourCategoryById(int tourCategoryId)
        {
            try
            {
                return await _context.TourCategories.FindAsync(tourCategoryId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving the tour category with ID {tourCategoryId}: {ex.Message}");
                return null;
            }
        }
    }
}
