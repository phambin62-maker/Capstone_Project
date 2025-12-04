using BE_Capstone_Project.Application.BookingManagement.DTOs;
using BE_Capstone_Project.Domain.Enums;
using BE_Capstone_Project.Domain.Models;

namespace BE_Capstone_Project.Application.BookingManagement.Services.Interfaces
{
    public interface IBookingService
    {
         Task<IEnumerable<BookingDTO>> GetAllAsync();
        Task<BookingDTO?> GetByIdAsync(int id);
        Task<int> CreateAsync(CreateBookingDTO dto);
        Task<bool> UpdateAsync(int id, CreateBookingDTO dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<BookingDTO>> GetByUserIdAsync(int userId);
        Task<bool> HasUserBookedTour(int userId, int tourId);
        Task<Booking?> GetBookingByUserIdAndTourId(int userId, int tourId);
        Task AddBookingCustomersToBookId(int bookingId, List<BookingCustomerDTO> bookingCustomers);
        Task<bool> UpdatePaymentStatus(PaymentDTO payment);
        Task<int> GetCustomerCountByBooking(int bookingId);
        Task<List<ScheduleBookedSeatsDTO>> GetBookedSeatsByTour(int tourId);
        Task<IEnumerable<UserBookingDTO>> GetByUserIdAsync2(int userId);
        Task DeleteExpiredPendingBookingsAsync();
        Task<BookingListResponse> GetBookingsForStaffAsync(BookingSearchRequest request);
        Task<StaffBookingDTO?> GetBookingDetailForStaffAsync(int id);
        Task<bool> UpdateBookingStatusAsync(int bookingId, UpdateBookingStatusRequest request);
        Task<bool> UpdatePaymentStatusByStaffAsync(int bookingId, UpdatePaymentStatusRequest request);
        Task<List<BookingStatus>> GetAvailableBookingStatusesAsync();
        Task<List<PaymentStatus>> GetAvailablePaymentStatusesAsync();
        Task<CancelValidationResult> ValidateCancelConditionAsync(int bookingId);
        
    }
}
