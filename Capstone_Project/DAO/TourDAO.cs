using BE_Capstone_Project.Infrastructure;
using BE_Capstone_Project.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BE_Capstone_Project.DAO
{
    public class TourDAO
    {
        private readonly OtmsdbContext _context;
        public TourDAO(OtmsdbContext context)
        {
            _context = context;
        }

        public async Task<int> AddTourAsync(Tour tour)
        {
            try
            {
                await _context.Tours.AddAsync(tour);
                await _context.SaveChangesAsync();
                return tour.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while adding a tour: {ex.Message}");
                return -1;
            }
        }

        public async Task<bool> UpdateTourAsync(Tour tour)
        {
            try
            {
                _context.Tours.Update(tour);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while updating the tour with ID {tour.Id}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteTourByIdAsync(int tourId)
        {
            try
            {
                var tour = await _context.Tours.FindAsync(tourId);
                if (tour != null)
                {
                    _context.Tours.Remove(tour);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting the tour with ID {tourId}: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Tour>> GetAllToursAsync()
        {
            try
            {
                return await _context.Tours.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving all tours: {ex.Message}");
                return new List<Tour>();
            }
        }

        public async Task<Tour?> GetTourByIdAsync(int tourId)
        {
            try
            {
                return await _context.Tours.FindAsync(tourId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving the tour with ID {tourId}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Tour>> GetToursByCategoryIdAsync(int categoryId)
        {
            try
            {
                return await _context.Tours
                    .Where(t => t.CategoryId == categoryId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving tours for category ID {categoryId}: {ex.Message}");
                return new List<Tour>();
            }
        }

        public async Task<List<Tour>> GetToursByStartLocationIdAsync(int startLocationId)
        {
            try
            {
                return await _context.Tours
                    .Where(t => t.StartLocationId == startLocationId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving tours for start location ID {startLocationId}: {ex.Message}");
                return new List<Tour>();
            }
        }

        public async Task<List<Tour>> GetToursByEndLocationIdAsync(int endLocationId)
        {
            try
            {
                return await _context.Tours
                    .Where(t => t.EndLocationId == endLocationId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving tours for end location ID {endLocationId}: {ex.Message}");
                return new List<Tour>();
            }
        }

        public async Task<List<Tour>> GetToursByCancelConditionIdAsync(int cancelConditionId)
        {
            try
            {
                return await _context.Tours
                    .Where(t => t.CancelConditionId == cancelConditionId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving tours for cancel condition ID {cancelConditionId}: {ex.Message}");
                return new List<Tour>();
            }
        }

        public async Task<List<Tour>> GetToursByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            try
            {
                return await _context.Tours
                    .Where(t => t.Price >= minPrice && t.Price <= maxPrice)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving tours in the price range {minPrice} - {maxPrice}: {ex.Message}");
                return new List<Tour>();
            }
        }

        public async Task<List<Tour>> SearchToursByNameAsync(string name)
        {
            try
            {
                return await _context.Tours
                    .Where(t => t.Name != null && t.Name.Contains(name))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while searching for tours with name containing '{name}': {ex.Message}");
                return new List<Tour>();
            }
        }

        public async Task<int> GetTotalTourCountAsync()
        {
            try
            {
                return await _context.Tours.CountAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while counting tours: {ex.Message}");
                return 0;
            }
        }

        public async Task<List<Tour>> GetPaginatedToursAsync(int page, int pageSize)
        {
            try
            {
                var paginatedTours = await _context.Tours
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return paginatedTours;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving paginated tours: {ex.Message}");
                return new List<Tour>();
            }
        }
    }
}
