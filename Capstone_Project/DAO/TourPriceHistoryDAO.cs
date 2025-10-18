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

        public async Task<int> AddTourPriceHistory(TourPriceHistory tourPriceHistory)
        {
            try
            {
                _context.TourPriceHistories.Add(tourPriceHistory);
                await _context.SaveChangesAsync();
                return tourPriceHistory.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while adding a tour price history: {ex.Message}");
                return -1;
            }
        }

        public async Task<bool> UpdateTourPriceHistory(TourPriceHistory tourPriceHistory)
        {
            try
            {
                _context.TourPriceHistories.Update(tourPriceHistory);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while updating the tour price history with ID {tourPriceHistory.Id}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteTourPriceHistoryById(int tourPriceHistoryId)
        {
            try
            {
                var tourPriceHistory = await _context.TourPriceHistories.FindAsync(tourPriceHistoryId);
                if (tourPriceHistory != null)
                {
                    _context.TourPriceHistories.Remove(tourPriceHistory);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting the tour price history with ID {tourPriceHistoryId}: {ex.Message}");
                return false;
            }
        }

        public async Task<List<TourPriceHistory>> GetAllTourPriceHistories()
        {
            try
            {
                return await _context.TourPriceHistories.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving all tour price histories: {ex.Message}");
                return new List<TourPriceHistory>();
            }
        }

        public async Task<TourPriceHistory?> GetTourPriceHistoryById(int tourPriceHistoryId)
        {
            try
            {
                return await _context.TourPriceHistories.FindAsync(tourPriceHistoryId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving the tour price history with ID {tourPriceHistoryId}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<TourPriceHistory>> GetTourPriceHistoriesByTourId(int tourId)
        {
            try
            {
                return await _context.TourPriceHistories
                    .Where(tph => tph.TourId == tourId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving tour price histories for tour ID {tourId}: {ex.Message}");
                return new List<TourPriceHistory>();
            }
        }
    }
}
