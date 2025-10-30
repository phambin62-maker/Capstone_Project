using BE_Capstone_Project.Infrastructure;
using BE_Capstone_Project.Domain.Models;
using Microsoft.EntityFrameworkCore;
using BE_Capstone_Project.Domain.Enums;

namespace BE_Capstone_Project.DAO
{
    public class BookingDAO
    {
        private readonly OtmsdbContext _context;
        public BookingDAO(OtmsdbContext context)
        {
            _context = context;
        }

        public async Task<int> AddBookingAsync(Booking booking)
        {
            try
            {
                await _context.Bookings.AddAsync(booking);
                await _context.SaveChangesAsync();
                return booking.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while adding a booking: {ex.Message}");
                return -1;
            }
        }

        public async Task<bool> UpdateBookingAsync(Booking booking)
        {
            try
            {
                _context.Bookings.Update(booking);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while updating the booking with ID {booking.Id}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteBookingByIdAsync(int bookingId)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(bookingId);
                if (booking != null)
                {
                    _context.Bookings.Remove(booking);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting the booking with ID {bookingId}: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Booking>> GetAllBookingsAsync()
        {
            try
            {
                return await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.TourSchedule)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving all bookings: {ex.Message}");
                return new List<Booking>();
            }
        }

        public async Task<Booking?> GetBookingByIdAsync(int bookingId)
        {
            try{
                return await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.TourSchedule)
                    .FirstOrDefaultAsync(b => b.Id == bookingId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving the booking with ID {bookingId}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Booking>> GetBookingsByUserIdAsync(int userId)
        {
            try
            {
                return await _context.Bookings
                .Where(b => b.UserId == userId)
                .Include(b => b.User)
                .Include(b => b.TourSchedule)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving bookings for user ID {userId}: {ex.Message}");
                return new List<Booking>();
            }
        }

        public async Task<List<Booking>> GetBookingsByTourScheduleIdAsync(int tourScheduleId)
        {
            try
            {
                return await _context.Bookings
                .Where(b => b.TourScheduleId == tourScheduleId)
                .Include(b => b.User)
                .Include(b => b.TourSchedule)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving bookings for tour schedule ID {tourScheduleId}: {ex.Message}");
                return new List<Booking>();
            }
        }

        public async Task<bool> HasUserBookedTourAsync(int userId, int tourId)
        {
            try
            {
                return await _context.Bookings
                .Include(b => b.TourSchedule)
                .AnyAsync(b =>
                    b.UserId == userId &&
                    b.TourSchedule.TourId == tourId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while checking booking for user ID {userId} and tour ID {tourId}: {ex.Message}");
                return false;
            }
        }

        public async Task<Booking?> GetBookingByUserIdAndTourIdAsync(int userId, int tourId)
        {
            try
            {
                return await _context.Bookings
                .FirstOrDefaultAsync(b =>
                    b.UserId == userId &&
                    b.TourSchedule.TourId == tourId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving booking for user ID {userId} and tour ID {tourId}: {ex.Message}");
                return null;
            }
        }
    }
}
