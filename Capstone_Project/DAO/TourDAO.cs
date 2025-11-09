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
                return await _context.Tours
                    .Include(t => t.TourImages)
                    .Include(t => t.Reviews)
                    .ToListAsync();
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
                return await _context.Tours
                    .Include(t => t.TourImages)
                    .Include(t => t.Reviews).ThenInclude(r => r.User)
                    .Include(t => t.StartLocation)
                    .Include(t => t.EndLocation)
                    .FirstOrDefaultAsync(t => t.Id == tourId);
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
                    .Include(t => t.TourImages)
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
                    .Include(t => t.TourImages)
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
                    .Include(t => t.TourImages)
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
                    .Include(t => t.TourImages)
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
                    .Include(t => t.TourImages)
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
                    .Include(t => t.TourImages)
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

        public async Task<List<Tour>> GetPaginatedToursAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var paginatedTours = await _context.Tours
                    .Include(t => t.TourImages)
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

        public async Task<List<Tour>> GetTopToursByEachCategoriesAsync()
        {
            try
            {
                // 1) Booking counts per TourId via Bookings -> TourSchedules -> TourId
                var bookingCountsDict = await _context.Bookings
                    .Join(_context.TourSchedules,
                          b => b.TourScheduleId,
                          ts => ts.Id,
                          (b, ts) => new { ts.TourId })
                    .GroupBy(x => x.TourId)
                    .Select(g => new { TourId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.TourId, x => x.Count);

                // 2) Average stars per TourId from Reviews
                var avgStarsDict = await _context.Reviews
                    .GroupBy(r => r.TourId)
                    .Select(g => new { TourId = g.Key, AvgStars = g.Average(r => (double?)r.Stars) })
                    .ToDictionaryAsync(x => x.TourId, x => x.AvgStars ?? 0.0);

                // 3) Load tours (include images if desired)
                var tours = await _context.Tours
                    .Include(t => t.TourImages)
                    .Include(t => t.Category)
                    .ToListAsync();

                // 4) Project with counts and avg stars (0 when missing)
                var toursWithMetrics = tours
                    .Select(t => new
                    {
                        Tour = t,
                        CategoryId = t.CategoryId,
                        BookingCount = bookingCountsDict.TryGetValue(t.Id, out var bc) ? bc : 0,
                        AvgStars = avgStarsDict.TryGetValue(t.Id, out var asv) ? asv : 0.0
                    })
                    .ToList();

                // 5) For each category pick the top tour (booking count desc, then avg stars desc, then id tie-breaker)
                var topToursByCategory = toursWithMetrics
                    .GroupBy(x => x.CategoryId)
                    .Select(g => g
                        .OrderByDescending(x => x.BookingCount)
                        .ThenByDescending(x => x.AvgStars)
                        .ThenBy(x => x.Tour.Id)
                        .First().Tour)
                    .ToList();

                return topToursByCategory;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving top tours by each category: {ex.Message}");
                return new List<Tour>();
            }
        }

        public async Task<List<Tour>> GetFilteredToursAsync(
            int page = 1,
            int pageSize = 10,
            bool? status = null,  
            int? startLocation = null,
            int? endLocation = null,
            int? category = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string sort = null,
            string search = null)
        {
            try
            {
                var query = _context.Tours
                    .Include(t => t.TourImages)
                    .Include(t => t.Reviews)
                    .Include(t => t.StartLocation)
                    .Include(t => t.EndLocation)
                    .Include(t => t.Category)
                    .AsQueryable();


                if (status.HasValue)
                {
                    query = query.Where(t => t.TourStatus == status.Value);
                }

                if (startLocation.HasValue)
                {
                    query = query.Where(t => t.StartLocationId == startLocation.Value);
                }

                if (endLocation.HasValue)
                {
                    query = query.Where(t => t.EndLocationId == endLocation.Value);
                }

                if (category.HasValue)
                {
                    query = query.Where(t => t.CategoryId == category.Value);
                }

                if (minPrice.HasValue)
                {
                    query = query.Where(t => t.Price >= minPrice.Value);
                }

                if (maxPrice.HasValue)
                {
                    query = query.Where(t => t.Price <= maxPrice.Value);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(t => t.Name.Contains(search) || t.Description.Contains(search));
                }

                if (!string.IsNullOrEmpty(sort))
                {
                    if (sort.ToLower() == "asc")
                        query = query.OrderBy(t => t.Price);
                    else if (sort.ToLower() == "desc")
                        query = query.OrderByDescending(t => t.Price);
                }
                else
                {
                    query = query.OrderBy(t => t.Id);
                }

                var paginatedTours = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return paginatedTours;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving filtered tours: {ex.Message}");
                return new List<Tour>();
            }
        }

        public async Task<int> GetFilteredTourCountAsync(
            bool? status = null,
            int? startLocation = null,
            int? endLocation = null,
            int? category = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string search = null)
        {
            try
            {
                var query = _context.Tours.AsQueryable();

                // Áp dụng các filter (giống như trong GetFilteredToursAsync)
                if (status.HasValue)
                {
                    query = query.Where(t => t.TourStatus == status.Value);
                }

                if (startLocation.HasValue)
                {
                    query = query.Where(t => t.StartLocationId == startLocation.Value);
                }

                if (endLocation.HasValue)
                {
                    query = query.Where(t => t.EndLocationId == endLocation.Value);
                }

                if (category.HasValue)
                {
                    query = query.Where(t => t.CategoryId == category.Value);
                }

                if (minPrice.HasValue)
                {
                    query = query.Where(t => t.Price >= minPrice.Value);
                }

                if (maxPrice.HasValue)
                {
                    query = query.Where(t => t.Price <= maxPrice.Value);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(t => t.Name.Contains(search) || t.Description.Contains(search));
                }

                return await query.CountAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while counting filtered tours: {ex.Message}");
                return 0;
            }
        }
        // Thêm method này vào TourDAO
        public async Task<List<Tour>> GetActiveTours(string search = "")
        {
            try
            {
                var query = _context.Tours
                    .Where(t => t.TourStatus == true) // Only active tours
                    .Include(t => t.StartLocation)
                    .Include(t => t.EndLocation)
                    .Include(t => t.Category)
                    .AsQueryable();

                // Apply search filter
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(t =>
                        t.Name.Contains(search) ||
                        (t.Description != null && t.Description.Contains(search))
                    );
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in TourDAO.GetActiveTours: {ex.Message}");
                return new List<Tour>();
            }
        }
    }
}
