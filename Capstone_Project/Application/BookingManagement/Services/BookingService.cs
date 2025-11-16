using BE_Capstone_Project.Application.BookingManagement.DTOs;
using BE_Capstone_Project.Application.BookingManagement.Services.Interfaces;
using BE_Capstone_Project.DAO;
using BE_Capstone_Project.Domain.Enums;
using BE_Capstone_Project.Domain.Models;
using DocumentFormat.OpenXml.Office2010.Excel;
using BE_Capstone_Project.Application.Notifications.Services;
using BE_Capstone_Project.Application.Notifications.DTOs;
// using BE_Capstone_Project.Domain.Enums; // (Dòng này bị lặp, đã xóa)
using System; // <-- Thêm Using này
using System.Linq; // <-- Thêm Using này
using System.Collections.Generic; // <-- Thêm Using này
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults; // <-- Thêm Using này

namespace BE_Capstone_Project.Application.BookingManagement.Services
{
    public class BookingService : IBookingService
    {
        private readonly BookingDAO _bookingDAO;
        private readonly BookingCustomerDAO _bookingCustomerDAO;

        // === 1. DỊCH VỤ THÔNG BÁO ===
        private readonly NotificationService _notificationService;

        // === 2. SỬA LỖI CONSTRUCTOR (THÊM NotificationService VÀO) ===
        public BookingService(BookingDAO bookingDAO,
                              BookingCustomerDAO bookingCustomerDAO,
                              NotificationService notificationService) // Thêm vào đây
        {
            _bookingDAO = bookingDAO;
            _bookingCustomerDAO = bookingCustomerDAO;
            _notificationService = notificationService; // Thêm dòng gán này
        }
        // === KẾT THÚC SỬA LỖI ===

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

        // === 3. SỬA HÀM CREATEASYNC (THÊM LOGIC THÔNG BÁO) ===
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
                ExpirationTime = DateTime.UtcNow.AddHours(1),
                RefundAmount = dto.RefundAmount,
                RefundDate = dto.RefundDate,
                PaymentDate = dto.PaymentDate
            };

            var newBookingId = await _bookingDAO.AddBookingAsync(newBooking);

            // NẾU LƯU THÀNH CÔNG:
            if (newBookingId > 0)
            {
                // === BẮT ĐẦU THÊM LOGIC THÔNG BÁO ===
                try
                {
                    // Lấy lại booking vừa tạo để có TourName
                    var savedBooking = await _bookingDAO.GetBookingByIdAsync(newBookingId);
                    if (savedBooking != null) // Kiểm tra an toàn
                    {
                        var tourName = savedBooking.TourSchedule?.Tour?.Name ?? "Your Tour";

                        var notificationDto = new CreateNotificationDTO
                        {
                            UserId = savedBooking.UserId,
                            Title = "Booking Successful",
                            Message = $"Your booking for tour '{tourName}' has been confirmed.",
                            NotificationType = NotificationType.System
                        };

                        // Gọi Service (BE) để tạo thông báo
                        _ = _notificationService.CreateAsync(notificationDto);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send booking notification: {ex.Message}");
                }
                // === KẾT THÚC LOGIC THÔNG BÁO ===
            }

            return newBookingId; // Trả về ID
        }
        // === KẾT THÚC SỬA LỖI ===


        // === 4. SỬA HÀM UPDATEASYNC (THÊM LOGIC CANCEL) ===
        public async Task<bool> UpdateAsync(int id, CreateBookingDTO dto)
        {
            var existing = await _bookingDAO.GetBookingByIdAsync(id);
            if (existing == null) return false;

            // Kiểm tra xem đây có phải là một hành động HỦY (Cancel) hay không
            bool isCancelling = (dto.BookingStatus == BookingStatus.Cancelled &&
                                 existing.BookingStatus != BookingStatus.Cancelled);

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

            var updateSuccess = await _bookingDAO.UpdateBookingAsync(existing);

            // NẾU CẬP NHẬT THÀNH CÔNG VÀ LÀ HÀNH ĐỘNG HỦY
            if (updateSuccess && isCancelling)
            {
                // === BẮT ĐẦU THÊM LOGIC THÔNG BÁO ===
                try
                {
                    var tourName = existing.TourSchedule?.Tour?.Name ?? "Your Tour";

                    var notificationDto = new CreateNotificationDTO
                    {
                        UserId = existing.UserId,
                        Title = "Booking Cancelled",
                        Message = $"Your booking for tour '{tourName}' has been successfully cancelled.",
                        NotificationType = NotificationType.System
                    };

                    // Gọi Service (BE) để tạo thông báo
                    _ = _notificationService.CreateAsync(notificationDto);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send cancellation notification: {ex.Message}");
                }
                // === KẾT THÚC LOGIC THÔNG BÁO ===
            }

            return updateSuccess;
        }
        // === KẾT THÚC SỬA LỖI ===

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

        public async Task<IEnumerable<UserBookingDTO>> GetByUserIdAsync2(int userId)
        {
            var list = await _bookingDAO.GetBookingsByUserIdAsync(userId);
            return list.Select(b => new UserBookingDTO
            {
                BookingId = b.Id,
                TourName = b.TourSchedule!.Tour!.Name!,
                DepartureDate = b.TourSchedule!.DepartureDate!.Value,
                ArrivalDate = b.TourSchedule!.ArrivalDate!.Value,
                Adults = b.BookingCustomers.Count(bc => bc.CustomerType == CustomerType.Adult),
                Children = b.BookingCustomers.Count(bc => bc.CustomerType == CustomerType.Child),
                TotalPrice = b.TotalPrice!.Value,
                Status = b.BookingStatus!.Value,
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

        public async Task AddBookingCustomersToBookId(int bookingId, List<BookingCustomerDTO> bookingCustomers)
        {
            foreach (BookingCustomerDTO dto in bookingCustomers)
            {
                BookingCustomer bookingCustomer = new BookingCustomer
                {
                    BookingId = bookingId,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    IdentityId = dto.IdentityID,
                    CustomerType = dto.CustomerType
                };
                await _bookingCustomerDAO.AddBookingCustomerAsync(bookingCustomer);
            }
        }

        public async Task<bool> UpdatePaymentStatus(PaymentDTO payment)
        {
            return await _bookingDAO.UpdatePaymentStatusAsync(payment);
        }

        public async Task<int> GetCustomerCountByBooking(int bookingId)
        {
            return await _bookingCustomerDAO.GetCustomerCountByBookingAsync(bookingId);
        }

        public async Task<List<ScheduleBookedSeatsDTO>> GetBookedSeatsByTour(int tourId)
        {
            return await _bookingCustomerDAO.GetBookedSeatsByTourAsync(tourId);
        }
    }
}