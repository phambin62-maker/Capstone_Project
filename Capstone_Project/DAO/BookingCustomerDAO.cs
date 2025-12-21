using BE_Capstone_Project.Application.BookingManagement.DTOs;
using BE_Capstone_Project.Domain.Models;
using BE_Capstone_Project.Infrastructure;
using BE_Capstone_Project.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BE_Capstone_Project.DAO
{
    public class BookingCustomerDAO
    {
        private readonly OtmsdbContext _context;
        public BookingCustomerDAO(OtmsdbContext context)
        {
            _context = context;
        }

        public async Task<int> AddBookingCustomerAsync(BookingCustomer bookingCustomer)
        {
            try
            {
                await _context.BookingCustomers.AddAsync(bookingCustomer);
                await _context.SaveChangesAsync();
                return bookingCustomer.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while adding a booking customer: {ex.Message}");
                return -1;
            }
        }

        public async Task<bool> UpdateBookingCustomerAsync(BookingCustomer bookingCustomer)
        {
            try
            {
                _context.BookingCustomers.Update(bookingCustomer);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while updating the booking customer with ID {bookingCustomer.Id}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteBookingCustomerByIdAsync(int bookingCustomerId)
        {
            try
            {
                var bookingCustomer = await _context.BookingCustomers.FindAsync(bookingCustomerId);
                if (bookingCustomer != null)
                {
                    _context.BookingCustomers.Remove(bookingCustomer);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting the booking customer with ID {bookingCustomerId}: {ex.Message}");
                return false;
            }
        }

        public async Task<List<BookingCustomer>> GetAllBookingCustomersAsync()
        {
            try
            {
                return await _context.BookingCustomers
                    .Include(bc => bc.Booking)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving all booking customers: {ex.Message}");
                return new List<BookingCustomer>();
            }
        }

        public async Task<BookingCustomer?> GetBookingCustomerByIdAsync(int bookingCustomerId)
        {
            try
            {
                return await _context.BookingCustomers
                    .Include(bc => bc.Booking)
                    .FirstOrDefaultAsync(bc => bc.Id == bookingCustomerId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving the booking customer with ID {bookingCustomerId}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<BookingCustomer>> GetBookingCustomersByBookingIdAsync(int bookingId)
        {
            try
            {
                return await _context.BookingCustomers
                    .Where(bc => bc.BookingId == bookingId)
                    .Include(bc => bc.Booking)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving booking customers for booking ID {bookingId}: {ex.Message}");
                return new List<BookingCustomer>();
            }
        }

        public async Task<int> GetCustomerCountByBookingAsync(int bookingId)
        {
            return await _context.BookingCustomers
                .Where(bc => bc.BookingId == bookingId)
                .CountAsync();
        }

        public async Task<List<ScheduleBookedSeatsDTO>> GetBookedSeatsByTourAsync(int tourId)
        {
            return await _context.BookingCustomers
                .Where(bc => bc.Booking.TourSchedule.TourId == tourId &&
                             !(bc.Booking.BookingStatus == BookingStatus.Cancelled &&
                               bc.Booking.PaymentStatus == PaymentStatus.Refunded))
                .GroupBy(bc => bc.Booking.TourScheduleId)
                .Select(g => new ScheduleBookedSeatsDTO
                {
                    TourScheduleId = g.Key,
                    BookedSeats = g.Count()
                })
                .ToListAsync();
        }

        public async Task<bool> DeleteBookingCustomerByBookingIdAsync(int bookingId)
        {
            var bks = await _context.BookingCustomers.Where(bc => bc.BookingId == bookingId).ToListAsync();
            if(bks == null || bks.Count == 0)
            {
                return false;
            }
            _context.BookingCustomers.RemoveRange(bks);
            return true;
        }
    }
}
