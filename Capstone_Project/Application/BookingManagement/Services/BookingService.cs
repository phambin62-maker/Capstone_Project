using BE_Capstone_Project.Application.BookingManagement.DTOs;
using BE_Capstone_Project.Application.BookingManagement.Services.Interfaces;
using BE_Capstone_Project.DAO;
using BE_Capstone_Project.Domain.Enums;
using BE_Capstone_Project.Domain.Models;
using DocumentFormat.OpenXml.Office2010.Excel;
using BE_Capstone_Project.Application.Notifications.Services;
using BE_Capstone_Project.Application.Notifications.DTOs;
// using BE_Capstone_Project.Domain.Enums; // (Dòng này bị lặp, đã xóa)
using System; 
using System.Linq; 
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using BE_Capstone_Project.Infrastructure;
using Microsoft.EntityFrameworkCore; 

namespace BE_Capstone_Project.Application.BookingManagement.Services
{
    public class BookingService : IBookingService
    {
        private readonly BookingDAO _bookingDAO;
        private readonly BookingCustomerDAO _bookingCustomerDAO;
        private readonly OtmsdbContext _context;

        // === 1. DỊCH VỤ THÔNG BÁO ===
        private readonly NotificationService _notificationService;

        // === 2. SỬA LỖI CONSTRUCTOR (THÊM NotificationService VÀO) ===
        public BookingService(BookingDAO bookingDAO,
                              BookingCustomerDAO bookingCustomerDAO,
                              NotificationService notificationService,
                              OtmsdbContext otmsdbContext) 
        {
            _bookingDAO = bookingDAO;
            _bookingCustomerDAO = bookingCustomerDAO;
            _notificationService = notificationService;
            _context = otmsdbContext;
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
        public async Task<BookingListResponse> GetBookingsForStaffAsync(BookingSearchRequest request)
        {
            var query = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.TourSchedule)
                    .ThenInclude(ts => ts.Tour)
                .Include(b => b.BookingCustomers)
                .AsQueryable();

            // Search by username, full name, email, phone, or tour name
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(b =>
                    b.User.Username.ToLower().Contains(searchTerm) ||
                    (b.FirstName + " " + b.LastName).ToLower().Contains(searchTerm) ||
                    b.Email.ToLower().Contains(searchTerm) ||
                    b.PhoneNumber.Contains(searchTerm) ||
                    b.TourSchedule.Tour.Name.ToLower().Contains(searchTerm)
                );
            }

            // Filter by booking status
            if (request.BookingStatus.HasValue)
            {
                query = query.Where(b => b.BookingStatus == request.BookingStatus.Value);
            }

            // Filter by payment status
            if (request.PaymentStatus.HasValue)
            {
                query = query.Where(b => b.PaymentStatus == request.PaymentStatus.Value);
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var bookings = await query
                .OrderByDescending(b => b.BookingDate)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var bookingDTOs = bookings.Select(b => new StaffBookingDTO
            {
                Id = b.Id,
                UserName = b.User.Username,
                FullName = $"{b.FirstName} {b.LastName}",
                Email = b.Email,
                PhoneNumber = b.PhoneNumber,
                TourName = b.TourSchedule.Tour.Name,
                DepartureDate = b.TourSchedule.DepartureDate,
                ArrivalDate = b.TourSchedule.ArrivalDate,
                PaymentStatus = b.PaymentStatus,
                BookingStatus = b.BookingStatus,
                TotalPrice = b.TotalPrice,
                BookingDate = b.BookingDate,
                ExpirationTime = b.ExpirationTime,
                AdultCount = b.BookingCustomers.Count(bc => bc.CustomerType == CustomerType.Adult),
                ChildCount = b.BookingCustomers.Count(bc => bc.CustomerType == CustomerType.Child),
                InfantCount = b.BookingCustomers.Count(bc => bc.CustomerType == CustomerType.Infant)
            }).ToList();

            return new BookingListResponse
            {
                Bookings = bookingDTOs.Select(b => new BookingDTO
                {
                    Id = b.Id,
                    UserId = bookings.First(x => x.Id == b.Id).UserId,
                    TourScheduleId = bookings.First(x => x.Id == b.Id).TourScheduleId,
                    PaymentStatus = b.PaymentStatus,
                    BookingStatus = b.BookingStatus,
                    TotalPrice = b.TotalPrice,
                    BookingDate = b.BookingDate,
                    FullName = b.FullName,
                    PhoneNumber = b.PhoneNumber,
                    Email = b.Email,
                    Username = b.UserName,
                    TourName = b.TourName
                }).ToList(),
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };
        }

        public async Task<StaffBookingDTO?> GetBookingDetailForStaffAsync(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.TourSchedule)
                    .ThenInclude(ts => ts.Tour)
                .Include(b => b.BookingCustomers)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null) return null;

            return new StaffBookingDTO
            {
                Id = booking.Id,
                UserName = booking.User.Username,
                FullName = $"{booking.FirstName} {booking.LastName}",
                Email = booking.Email,
                PhoneNumber = booking.PhoneNumber,
                TourName = booking.TourSchedule.Tour.Name,
                DepartureDate = booking.TourSchedule.DepartureDate,
                ArrivalDate = booking.TourSchedule.ArrivalDate,
                PaymentStatus = booking.PaymentStatus,
                BookingStatus = booking.BookingStatus,
                TotalPrice = booking.TotalPrice,
                BookingDate = booking.BookingDate,
                ExpirationTime = booking.ExpirationTime,
                AdultCount = booking.BookingCustomers.Count(bc => bc.CustomerType == CustomerType.Adult),
                ChildCount = booking.BookingCustomers.Count(bc => bc.CustomerType == CustomerType.Child),
                InfantCount = booking.BookingCustomers.Count(bc => bc.CustomerType == CustomerType.Infant)
            };
        }

        public async Task<bool> UpdateBookingStatusAsync(int bookingId, UpdateBookingStatusRequest request)
        {
            var booking = await _bookingDAO.GetBookingByIdAsync(bookingId);
            if (booking == null) return false;

            var oldStatus = booking.BookingStatus;
            booking.BookingStatus = request.BookingStatus;

            var updateSuccess = await _bookingDAO.UpdateBookingAsync(booking);

            if (updateSuccess)
            {
                // Send notification to user about status change
                try
                {
                    var tourName = booking.TourSchedule?.Tour?.Name ?? "Your Tour";
                    var notificationDto = new CreateNotificationDTO
                    {
                        UserId = booking.UserId,
                        Title = "Booking Status Updated",
                        Message = $"Your booking for '{tourName}' has been updated from {oldStatus} to {request.BookingStatus}. {request.Note}",
                        NotificationType = NotificationType.System
                    };

                    _ = _notificationService.CreateAsync(notificationDto);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send status update notification: {ex.Message}");
                }
            }

            return updateSuccess;
        }

        public async Task<bool> UpdatePaymentStatusByStaffAsync(int bookingId, UpdatePaymentStatusRequest request)
        {
            var booking = await _bookingDAO.GetBookingByIdAsync(bookingId);
            if (booking == null) return false;

            var oldStatus = booking.PaymentStatus;
            booking.PaymentStatus = request.PaymentStatus;

            // If payment is completed, update booking status to confirmed
            if (request.PaymentStatus == PaymentStatus.Completed && booking.BookingStatus == BookingStatus.Pending)
            {
                booking.BookingStatus = BookingStatus.Confirmed;
            }

            var updateSuccess = await _bookingDAO.UpdateBookingAsync(booking);

            if (updateSuccess)
            {
                // Send notification to user about payment status change
                try
                {
                    var tourName = booking.TourSchedule?.Tour?.Name ?? "Your Tour";
                    var notificationDto = new CreateNotificationDTO
                    {
                        UserId = booking.UserId,
                        Title = "Payment Status Updated",
                        Message = $"Payment status for your booking '{tourName}' has been updated from {oldStatus} to {request.PaymentStatus}. {request.Note}",
                        NotificationType = NotificationType.System
                    };

                    _ = _notificationService.CreateAsync(notificationDto);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send payment status notification: {ex.Message}");
                }
            }

            return updateSuccess;
        }

        public async Task<List<BookingStatus>> GetAvailableBookingStatusesAsync()
        {
            return await Task.FromResult(Enum.GetValues(typeof(BookingStatus))
                .Cast<BookingStatus>()
                .ToList());
        }

        public async Task<List<PaymentStatus>> GetAvailablePaymentStatusesAsync()
        {
            return await Task.FromResult(Enum.GetValues(typeof(PaymentStatus))
                .Cast<PaymentStatus>()
                .ToList());
        }
    }
}