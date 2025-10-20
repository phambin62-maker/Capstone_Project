using BE_Capstone_Project.Infrastructure;
using BE_Capstone_Project.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BE_Capstone_Project.DAO
{
    public class TourPriceHistoryDAO
    {
        private readonly OtmsdbContext _context;

        public TourPriceHistoryDAO(OtmsdbContext context)
        {
            _context = context;
        }

        public async Task<int> AddTourPriceHistoryAsync(TourPriceHistory tourPriceHistory)
        {
            try
            {
                await _context.TourPriceHistories.AddAsync(tourPriceHistory);
                await _context.SaveChangesAsync();
                return tourPriceHistory.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding TourPriceHistory: {ex.Message}");
                return -1;
            }
        }

        public async Task<bool> UpdateTourPriceHistoryAsync(TourPriceHistory tourPriceHistory)
        {
            try
            {
                _context.TourPriceHistories.Update(tourPriceHistory);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating TourPriceHistory (ID: {tourPriceHistory.Id}): {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteTourPriceHistoryByIdAsync(int id)
        {
            try
            {
                var tph = await _context.TourPriceHistories.FindAsync(id);
                if (tph == null) return false;

                _context.TourPriceHistories.Remove(tph);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting TourPriceHistory (ID: {id}): {ex.Message}");
                return false;
            }
        }

        public async Task<List<TourPriceHistory>> GetAllTourPriceHistoriesAsync()
        {
            try
            {
                return await _context.TourPriceHistories
                    .Include(t => t.Tour)
                    .OrderByDescending(t => t.UpdatedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving all TourPriceHistories: {ex.Message}");
                return new List<TourPriceHistory>();
            }
        }

        public async Task<TourPriceHistory?> GetTourPriceHistoryByIdAsync(int id)
        {
            try
            {
                return await _context.TourPriceHistories
                    .Include(t => t.Tour)
                    .FirstOrDefaultAsync(t => t.Id == id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving TourPriceHistory (ID: {id}): {ex.Message}");
                return null;
            }
        }

        public async Task<List<TourPriceHistory>> GetTourPriceHistoriesByTourIdAsync(int tourId)
        {
            try
            {
                return await _context.TourPriceHistories
                    .Where(t => t.TourId == tourId)
                    .OrderByDescending(t => t.UpdatedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving TourPriceHistories (TourID: {tourId}): {ex.Message}");
                return new List<TourPriceHistory>();
            }
        }
    }
}
