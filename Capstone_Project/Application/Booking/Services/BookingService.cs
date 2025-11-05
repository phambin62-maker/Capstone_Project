using BE_Capstone_Project.Application.Bookings.DTOs;
using BE_Capstone_Project.DAO;
using BE_Capstone_Project.Domain.Enums;
using BE_Capstone_Project.Domain.Models;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace BE_Capstone_Project.Application.Bookings.Services
{
    public class BookingService
    {
        private readonly BookingDAO _bookingDAO;

        public BookingService(BookingDAO bookingDAO)
        {
            _bookingDAO = bookingDAO;
        }

        public async Task<IEnumerable<BookingDTO>> GetAllAsync()
        {
            var list = await _bookingDAO.GetAllBookingsAsync();
            return list.Select(b => new BookingDTO
            {
                Id = b.Id,
                UserId = b.UserId,
                TourScheduleId = b.TourScheduleId,
                PaymentStatus = b.PaymentStatus,
                TotalPrice = b.TotalPrice,
                BookingDate = b.BookingDate,
                BookingStatus = b.BookingStatus,
                FullName = $"{b.FirstName} {b.LastName}",
                PhoneNumber = b.PhoneNumber,
                Email = b.Email,
                Username = b.User?.Username,
                TourName = b.TourSchedule?.Tour?.Name
            });
        }

        public async Task<BookingDTO?> GetByIdAsync(int id)
        {
            var b = await _bookingDAO.GetBookingByIdAsync(id);
            if (b == null) return null;

            return new BookingDTO
            {
                Id = b.Id,
                UserId = b.UserId,
                TourScheduleId = b.TourScheduleId,
                PaymentStatus = b.PaymentStatus,
                TotalPrice = b.TotalPrice,
                BookingDate = b.BookingDate,
                BookingStatus = b.BookingStatus,
                FullName = $"{b.FirstName} {b.LastName}",
                PhoneNumber = b.PhoneNumber,
                Email = b.Email,
                Username = b.User?.Username,
                TourName = b.TourSchedule?.Tour?.Name
            };
        }

        public async Task<int> CreateAsync(CreateBookingDTO dto)
        {
            var newBooking = new Booking
            {
                UserId = dto.UserId,
                TourScheduleId = dto.TourScheduleId,
                PaymentStatus = dto.PaymentStatus,
                TotalPrice = dto.TotalPrice,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                CertificateId = dto.CertificateId,
                PaymentMethod = dto.PaymentMethod,
                BookingStatus = dto.BookingStatus ?? BookingStatus.Pending,
                BookingDate = DateTime.UtcNow,
                ExpirationTime = DateTime.UtcNow.AddHours(1)
            };

            return await _bookingDAO.AddBookingAsync(newBooking);
        }

        public async Task<bool> UpdateAsync(int id, CreateBookingDTO dto)
        {
            var existing = await _bookingDAO.GetBookingByIdAsync(id);
            if (existing == null) return false;

            existing.PaymentStatus = dto.PaymentStatus ?? existing.PaymentStatus;
            existing.BookingStatus = dto.BookingStatus ?? existing.BookingStatus;
            existing.FirstName = dto.FirstName;
            existing.LastName = dto.LastName;
            existing.PhoneNumber = dto.PhoneNumber;
            existing.Email = dto.Email;
            existing.CertificateId = dto.CertificateId;
            existing.TotalPrice = dto.TotalPrice;
            existing.PaymentMethod = dto.PaymentMethod;
            existing.ExpirationTime = DateTime.UtcNow.AddHours(1);

            return await _bookingDAO.UpdateBookingAsync(existing);
        }
        public async Task<bool> DeleteAsync(int id)
        {
            return await _bookingDAO.DeleteBookingByIdAsync(id);
        }

        public async Task<IEnumerable<BookingDTO>> GetByUserIdAsync(int userId)
        {
            var list = await _bookingDAO.GetBookingsByUserIdAsync(userId);
            return list.Select(b => new BookingDTO
            {
                Id = b.Id,
                UserId = b.UserId,
                TourScheduleId = b.TourScheduleId,
                PaymentStatus = b.PaymentStatus,
                TotalPrice = b.TotalPrice,
                BookingDate = b.BookingDate,
                BookingStatus = b.BookingStatus,
                FullName = $"{b.FirstName} {b.LastName}",
                PhoneNumber = b.PhoneNumber,
                Email = b.Email,
                Username = b.User?.Username,
                TourName = b.TourSchedule?.Tour?.Name
            });
        }

        public async Task<bool> HasUserBookedTour(int userId, int tourId)
        {
            return await _bookingDAO.HasUserBookedTourAsync(userId, tourId);
        }

        public async Task<Booking?> GetBookingByUserIdAndTourId(int userId, int tourId)
        {
            return await _bookingDAO.GetBookingByUserIdAndTourIdAsync(userId, tourId);
        }
    }
}
