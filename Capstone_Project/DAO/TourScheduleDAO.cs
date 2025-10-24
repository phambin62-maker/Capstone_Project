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
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Where(ts => ts.TourId == tourId)
                    .ToListAsync();

                return paginatedTourSchedules;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving paginated tour schedules by tourId: {tourId}: {ex.Message}");
                return new List<TourSchedule>();
            }
        }
    }
}
