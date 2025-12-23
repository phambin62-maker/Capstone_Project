using BE_Capstone_Project.Domain.Enums;
using BE_Capstone_Project.Domain.Models;
using BE_Capstone_Project.Infrastructure;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;

namespace BE_Capstone_Project.DAO
{
    public class TourScheduleDAO
    {
        private readonly OtmsdbContext _context;
        public TourScheduleDAO(OtmsdbContext context)
        {
            _context = context;
        }
        public async Task<int> AddTourSchedule(TourSchedule tourSchedule)
        {
            try
            {
                _context.TourSchedules.Add(tourSchedule);
                await _context.SaveChangesAsync();
                return tourSchedule.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while adding a tour schedule: {ex.Message}");
                return -1;
            }
        }
        public async Task<bool> ExistsAsync(int tourId, DateOnly departureDate)
{
        return await _context.TourSchedules.AnyAsync(ts =>
            ts.TourId == tourId &&
            ts.DepartureDate == departureDate
        );
}
        public async Task<bool> ExistsForUpdateAsync(int tourId, DateOnly departureDate, int excludeId)
        {
            return await _context.TourSchedules.AnyAsync(ts =>
                ts.TourId == tourId &&
                ts.DepartureDate == departureDate &&
                ts.Id != excludeId
            );
        }


        public async Task<bool> UpdateTourSchedule(TourSchedule tourSchedule)
        {
            try
            {
                _context.TourSchedules.Update(tourSchedule);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while updating the tour schedule with ID {tourSchedule.Id}: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> DeleteTourScheduleById(int tourScheduleId)
        {
            try
            {
                var tourSchedule = await _context.TourSchedules.FindAsync(tourScheduleId);
                if (tourSchedule != null)
                {
                    _context.TourSchedules.Remove(tourSchedule);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting the tour schedule with ID {tourScheduleId}: {ex.Message}");
                return false;
            }
        }

        public async Task<List<TourSchedule>> GetAllTourSchedules()
        {
            try
            {
                return await _context.TourSchedules
                    .Include(ts => ts.Tour)
                        .ThenInclude(t => t.StartLocation) 
                    .Include(ts => ts.Tour)
                        .ThenInclude(t => t.EndLocation) 
                    .Include(ts => ts.Tour)
                        .ThenInclude(t => t.Category)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving all tour schedules: {ex.Message}");
                return new List<TourSchedule>();
            }
        }

        public async Task<TourSchedule?> GetTourScheduleById(int tourScheduleId)
        {
            try
            {
                return await _context.TourSchedules
                    .Include(ts => ts.Tour)
                    .FirstOrDefaultAsync(ts => ts.Id == tourScheduleId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving the tour schedule with ID {tourScheduleId}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<TourSchedule>> GetTourSchedulesByTourId(int tourId)
        {
            try
            {
                return await _context.TourSchedules
                    .Where(ts => ts.TourId == tourId)
                    .Include(ts => ts.Tour)
                        .ThenInclude(t => t.StartLocation)
                    .Include(ts => ts.Tour)
                        .ThenInclude(t => t.EndLocation)
                    .Include(ts => ts.Tour)
                        .ThenInclude(t => t.Category)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving tour schedules for tour ID {tourId}: {ex.Message}");
                return new List<TourSchedule>();
            }
        }

        public async Task<List<TourSchedule>> GetPaginatedTourSchedules(int page = 1, int pageSize = 5)
        {
            try
            {
                var paginatedTourSchedules = await _context.TourSchedules
                    .Include(ts => ts.Tour)
                        .ThenInclude(t => t.StartLocation)
                    .Include(ts => ts.Tour)
                        .ThenInclude(t => t.EndLocation)
                    .Include(ts => ts.Tour)
                        .ThenInclude(t => t.Category)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return paginatedTourSchedules;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving paginated tour schedules: {ex.Message}");
                return new List<TourSchedule>();
            }
        }

        public async Task<List<TourSchedule>> GetPaginatedTourSchedulesByTourId(int tourId, int page = 1, int pageSize = 5)
        {
            try
            {
                var paginatedTourSchedules = await _context.TourSchedules
                    .Where(ts => ts.TourId == tourId)
                    .Include(ts => ts.Tour)
                        .ThenInclude(t => t.StartLocation)
                    .Include(ts => ts.Tour)
                        .ThenInclude(t => t.EndLocation)
                    .Include(ts => ts.Tour)
                        .ThenInclude(t => t.Category)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return paginatedTourSchedules;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving paginated tour schedules by tourId: {tourId}: {ex.Message}");
                return new List<TourSchedule>();
            }
        }

        public async Task<List<TourSchedule>> GetFilteredTourSchedules(
            int? tourId = null,
            string? tourName = null,
            string? location = null,
            string? category = null,
            string? status = null,
            string? sort = null,
            string? search = null,
            string? fromDate = null,
            string? toDate = null,
            int page = 1,
            int pageSize = 10)
        {
            try
            {
                var query = _context.TourSchedules
                    .Include(ts => ts.Tour)
                        .ThenInclude(t => t.StartLocation)
                    .Include(ts => ts.Tour)
                        .ThenInclude(t => t.EndLocation)
                    .Include(ts => ts.Tour)
                        .ThenInclude(t => t.Category)
                    .AsQueryable();

                // Apply filters
                query = ApplyFilters(query, tourId, tourName, location, category, status, search, fromDate, toDate);

                // Apply sorting
                query = ApplySorting(query, sort);

                // Apply pagination
                var paginatedTourSchedules = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return paginatedTourSchedules;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving filtered tour schedules: {ex.Message}");
                return new List<TourSchedule>();
            }
        }

        // Hàm đếm với filter
        public async Task<int> GetFilteredTourScheduleCount(
            int? tourId = null,
            string? tourName = null,
            string? location = null,
            string? category = null,
            string? status = null,
            string? search = null,
            string? fromDate = null,
            string? toDate = null)
        {
            try
            {
                var query = _context.TourSchedules
                    .Include(ts => ts.Tour)
                        .ThenInclude(t => t.StartLocation)
                    .Include(ts => ts.Tour)
                        .ThenInclude(t => t.EndLocation)
                    .Include(ts => ts.Tour)
                        .ThenInclude(t => t.Category)
                    .AsQueryable();

                // Apply filters
                query = ApplyFilters(query, tourId, tourName, location, category, status, search, fromDate, toDate);

                return await query.CountAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while counting filtered tour schedules: {ex.Message}");
                return 0;
            }
        }

        public async Task<bool> IsScheduleFullAsync(int tourScheduleId)
        {
            var maxSeats = await _context.TourSchedules
                .Where(ts => ts.Id == tourScheduleId)
                .Select(ts => ts.Tour.MaxSeats)
                .FirstOrDefaultAsync();

            if (maxSeats == 0) return false;

            var currentCustomers = await _context.BookingCustomers
                .Where(bc => bc.Booking.TourScheduleId == tourScheduleId &&
                             !(bc.Booking.BookingStatus == BookingStatus.Cancelled))
                .CountAsync();

            return currentCustomers >= maxSeats;
        }

        // Helper method to apply filters
        private IQueryable<TourSchedule> ApplyFilters(
            IQueryable<TourSchedule> query,
            int? tourId,
            string? tourName,
            string? location,
            string? category,
            string? status,
            string? search,
            string? fromDate,
            string? toDate)
        {
            // Tour ID filter
            if (tourId.HasValue)
            {
                query = query.Where(ts => ts.TourId == tourId.Value);
            }

            // Tour name filter
            if (!string.IsNullOrEmpty(tourName))
            {
                query = query.Where(ts => ts.Tour.Name.Contains(tourName));
            }

            // Location filter (search in both start and end locations)
            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(ts =>
                    ts.Tour.StartLocation.LocationName.Contains(location) ||
                    ts.Tour.EndLocation.LocationName.Contains(location));
            }

            // Category filter
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(ts => ts.Tour.Category.CategoryName.Contains(category));
            }

            // Status filter
            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<ScheduleStatus>(status, out var statusEnum))
                {
                    query = query.Where(ts => ts.ScheduleStatus == statusEnum);
                }
            }

            // Search filter (search in multiple fields)
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(ts =>
                    ts.Tour.Name.Contains(search) ||
                    ts.Tour.StartLocation.LocationName.Contains(search) ||
                    ts.Tour.EndLocation.LocationName.Contains(search) ||
                    ts.Tour.Category.CategoryName.Contains(search));
            }

            // Date range filter
            if (!string.IsNullOrEmpty(fromDate) && DateOnly.TryParse(fromDate, out var fromDateParsed))
            {
                query = query.Where(ts => ts.DepartureDate >= fromDateParsed);
            }

            if (!string.IsNullOrEmpty(toDate) && DateOnly.TryParse(toDate, out var toDateParsed))
            {
                query = query.Where(ts => ts.ArrivalDate <= toDateParsed);
            }

            return query;
        }

        // Helper method to apply sorting
        private IQueryable<TourSchedule> ApplySorting(IQueryable<TourSchedule> query, string? sort)
        {
            return sort?.ToLower() switch
            {
                "date_asc" => query.OrderBy(ts => ts.DepartureDate),
                "date_desc" => query.OrderByDescending(ts => ts.DepartureDate),
                _ => query.OrderBy(ts => ts.DepartureDate) // Default sort by departure date ascending
            };
        }
    }
}
