using BE_Capstone_Project.Application.BookingManagement.DTOs;
using BE_Capstone_Project.Application.BookingManagement.Services.Interfaces;
using BE_Capstone_Project.DAO;
using BE_Capstone_Project.Domain.Enums;
using BE_Capstone_Project.Domain.Models;
using DocumentFormat.OpenXml.Office2010.Excel;
using BE_Capstone_Project.Application.Notifications.Services.Interfaces;
using BE_Capstone_Project.Application.Notifications.DTOs;
using System;
using System.Globalization;
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
        private readonly INotificationService _notificationService;

        public BookingService(BookingDAO bookingDAO,
                              BookingCustomerDAO bookingCustomerDAO,
                              INotificationService notificationService,
                              OtmsdbContext otmsdbContext)
        {
            _bookingDAO = bookingDAO;
            _bookingCustomerDAO = bookingCustomerDAO;
            _notificationService = notificationService;
            _context = otmsdbContext;
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
                ExpirationTime = dto.ExpirationTime,
                RefundAmount = dto.RefundAmount,
                RefundDate = dto.RefundDate,
                PaymentDate = dto.PaymentDate
            };

            var newBookingId = await _bookingDAO.AddBookingAsync(newBooking);

            if (newBookingId > 0)
            {
                try
                {
                    var savedBooking = await _bookingDAO.GetBookingByIdAsync(newBookingId);
                    if (savedBooking != null)
                    {
                        var tourName = savedBooking.TourSchedule?.Tour?.Name ?? "Your Tour";

                        string userMessage = $"Booking #{newBookingId}: Your booking for tour '{tourName}' has been created.";

                        if (savedBooking.PaymentStatus == PaymentStatus.Pending)
                        {
                            userMessage += " Note: Your booking is Unpaid. Please complete the payment to secure your booking.";
                        }

                        var notificationDto = new CreateNotificationDTO
                        {
                            UserId = savedBooking.UserId,
                            Title = "Booking Created",
                            Message = userMessage,
                            NotificationType = NotificationType.System
                        };

                        await _notificationService.CreateAsync(notificationDto);

                        var allStaff = await _context.Users
                            .Where(u => u.RoleId == 2)
                            .ToListAsync();

                        foreach (var staff in allStaff)
                        {
                            var staffNoti = new CreateNotificationDTO
                            {
                                UserId = staff.Id,
                                Title = "New Booking Alert",
                                Message = $"New Booking #{newBookingId}: Customer {savedBooking.FirstName} {savedBooking.LastName} has booked tour '{tourName}'.",
                                NotificationType = NotificationType.System
                            };

                            await _notificationService.CreateAsync(staffNoti);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send booking notification: {ex.Message}");
                }
            }

            return newBookingId;
        }


        public async Task<bool> UpdateAsync(int id, CreateBookingDTO dto)
        {
            var existing = await _bookingDAO.GetBookingByIdAsync(id);
            if (existing == null) return false;

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

            if (updateSuccess && isCancelling)
            {
                try
                {
                    var tourName = existing.TourSchedule?.Tour?.Name ?? "Your Tour";

                    var notificationDto = new CreateNotificationDTO
                    {
                        UserId = existing.UserId,
                        Title = "Booking Cancelled",
                        Message = $"Booking #{existing.Id}: Your booking for tour '{tourName}' has been successfully cancelled.",
                        NotificationType = NotificationType.System
                    };

                    await _notificationService.CreateAsync(notificationDto);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send cancellation notification: {ex.Message}");
                }
            }

            return updateSuccess;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var success = await _bookingCustomerDAO.DeleteBookingCustomerByBookingIdAsync(id);
            if (!success) return false;

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
            var result = new List<UserBookingDTO>();
            foreach (var b in list)
            {
                var cancelValidation = await ValidateCancelConditionAsync(b.Id);

                var userBooking = new UserBookingDTO
                {
                    BookingId = b.Id,
                    TourName = b.TourSchedule!.Tour!.Name!,
                    DepartureDate = b.TourSchedule!.DepartureDate!.Value,
                    Adults = b.BookingCustomers.Count(bc => bc.CustomerType == CustomerType.Adult),
                    Children = b.BookingCustomers.Count(bc => bc.CustomerType == CustomerType.Child || bc.CustomerType == CustomerType.Infant),
                    TotalPrice = b.TotalPrice!.Value,
                    Status = b.BookingStatus!.Value,
                    PaymentStatus = b.PaymentStatus!.Value,
                    CancelCondition = cancelValidation,
                    CanCancel = cancelValidation.CanCancel,
                    CancelMessage = cancelValidation.Message
                };

                result.Add(userBooking);
            }

            return result;
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
            var success = await _bookingDAO.UpdatePaymentStatusAsync(payment);
            if (success)
            {
                try
                {
                    var booking = await _bookingDAO.GetBookingByIdAsync(payment.BookingId);
                    if (booking != null)
                    {
                        var tourName = booking.TourSchedule?.Tour?.Name ?? "Your Tour";
                        var notificationDto = new CreateNotificationDTO
                        {
                            UserId = booking.UserId,
                            Title = "Payment Successful",
                            Message = $"Booking #{booking.Id}: Your payment for tour '{tourName}' has been successfully processed via {payment.PaymentMethod}.",
                            NotificationType = NotificationType.System
                        };
                        await _notificationService.CreateAsync(notificationDto);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send payment notification: {ex.Message}");
                }
            }
            return success;
        }

        public async Task<int> GetCustomerCountByBooking(int bookingId)
        {
            return await _bookingCustomerDAO.GetCustomerCountByBookingAsync(bookingId);
        }

        public async Task<List<ScheduleBookedSeatsDTO>> GetBookedSeatsByTour(int tourId)
        {
            return await _bookingCustomerDAO.GetBookedSeatsByTourAsync(tourId);
        }

        public async Task DeleteExpiredPendingBookingsAsync()
        {
            await _bookingDAO.DeleteExpiredPendingBookingsAsync();
        }
        public async Task<BookingListResponse> GetBookingsForStaffAsync(BookingSearchRequest request)
        {
            var query = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.TourSchedule)
                    .ThenInclude(ts => ts.Tour)
                .Include(b => b.BookingCustomers)
                .AsQueryable();

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
                try
                {
                    var tourName = booking.TourSchedule?.Tour?.Name ?? "Your Tour";

                    var notificationDto = new CreateNotificationDTO
                    {
                        UserId = booking.UserId,
                        Title = "Booking Status Updated",
                        Message = $"Booking #{booking.Id}: Your booking for '{tourName}' has been updated from {oldStatus} to {request.BookingStatus}. {request.Note}",
                        NotificationType = NotificationType.System
                    };
                    await _notificationService.CreateAsync(notificationDto);

                    if (request.BookingStatus == BookingStatus.Cancelled)
                    {
                        var allStaff = await _context.Users
                            .Where(u => u.RoleId == 2)
                            .ToListAsync();

                        if (allStaff.Any())
                        {
                            foreach (var staff in allStaff)
                            {
                                var staffNoti = new CreateNotificationDTO
                                {
                                    UserId = staff.Id,
                                    Title = "Booking Cancelled Alert",
                                    Message = $"Customer {booking.FirstName} {booking.LastName} has cancelled Booking #{booking.Id} for tour '{tourName}'.",
                                    NotificationType = NotificationType.System
                                };

                                await _notificationService.CreateAsync(staffNoti);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send notification: {ex.Message}");
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

            if (request.PaymentStatus == PaymentStatus.Completed && booking.BookingStatus == BookingStatus.Pending)
            {
                booking.BookingStatus = BookingStatus.Confirmed;
            }
            else if (request.PaymentStatus == PaymentStatus.Refunded)
            {
                booking.BookingStatus = BookingStatus.Cancelled;
            }

            var updateSuccess = await _bookingDAO.UpdateBookingAsync(booking);

            if (updateSuccess)
            {
                try
                {
                    var tourName = booking.TourSchedule?.Tour?.Name ?? "Your Tour";
                    var notificationDto = new CreateNotificationDTO
                    {
                        UserId = booking.UserId,
                        Title = "Payment Status Updated",
                        Message = $"Booking #{booking.Id}: Payment status for your booking '{tourName}' has been updated from {oldStatus} to {request.PaymentStatus}. {request.Note}",
                        NotificationType = NotificationType.System
                    };

                    await _notificationService.CreateAsync(notificationDto);
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
        public async Task<CancelValidationResult> ValidateCancelConditionAsync(int bookingId)
        {


            var booking = await _bookingDAO.GetBookingByIdAsync(bookingId);
            if (booking == null)
            {
                Console.WriteLine($"Booking {bookingId} not found");
                return new CancelValidationResult
                {
                    CanCancel = false,
                    Message = "Booking not found"
                };
            }
            if (booking.BookingStatus == BookingStatus.Completed)
            {
                Console.WriteLine($"Booking {bookingId} is already completed");

                return new CancelValidationResult
                {
                    CanCancel = false,
                    Message = "The tour has been completed.",
                };
            }
            if (booking.BookingStatus == BookingStatus.Cancelled)
            {
                Console.WriteLine($"Booking {bookingId} is already cancelled");

                var schedule = booking.TourSchedule;
                var tourInfo = schedule?.Tour;
                decimal? calculatedRefundAmount = 0;
                string successMessage = "Cancel successful!";

                if (tourInfo != null && tourInfo.CancelConditionId != null)
                {
                    var cancelCondition = await _context.CancelConditions
                        .FirstOrDefaultAsync(cc => cc.Id == tourInfo.CancelConditionId);

                    if (cancelCondition != null && cancelCondition.RefundPercent > 0 &&
                        booking.TotalPrice.HasValue && booking.TotalPrice.Value > 0)
                    {
                        calculatedRefundAmount = booking.TotalPrice.Value * (cancelCondition.RefundPercent / 100m);
                        calculatedRefundAmount = Math.Round(calculatedRefundAmount.Value, 2);

                        if (calculatedRefundAmount > 0)
                        {
                            successMessage += $" Refund amount: {calculatedRefundAmount.Value.ToString("N0")} VND";
                        }
                    }
                }

                return new CancelValidationResult
                {
                    CanCancel = false,
                    Message = successMessage,
                    RefundAmount = calculatedRefundAmount
                };
            }
            if (booking.BookingStatus == BookingStatus.Pending)
            {
                Console.WriteLine($"Booking {bookingId} is still pending");

                return new CancelValidationResult
                {
                    CanCancel = false,
                    Message = "Cannot cancel a booking that is still pending. Please wait for confirmation or contact support.",
                };
            }
            if (booking.PaymentStatus == PaymentStatus.Pending)
            {
                Console.WriteLine($"Payment for booking {bookingId} is still pending");

                return new CancelValidationResult
                {
                    CanCancel = false,
                    Message = "Cannot cancel a booking with a pending payment.",
                };
            }
            if (booking.BookingStatus != BookingStatus.Confirmed)
            {
                Console.WriteLine($"Booking status invalid for cancellation: {booking.BookingStatus}");
                return new CancelValidationResult
                {
                    CanCancel = false,
                    Message = $"Cannot cancel booking with status: {booking.BookingStatus}"
                };
            }

            var tourSchedule = booking.TourSchedule;
            if (tourSchedule?.DepartureDate == null)
            {
                Console.WriteLine("Tour schedule or departure date is null");
                return new CancelValidationResult
                {
                    CanCancel = false,
                    Message = "Tour schedule information is missing"
                };
            }

            var departureDate = tourSchedule.DepartureDate.Value;
            var currentDateOnly = DateOnly.FromDateTime(DateTime.Now);

            if (departureDate <= currentDateOnly)
            {
                Console.WriteLine($"Departure date has passed: {departureDate}, Current: {currentDateOnly}");
                return new CancelValidationResult
                {
                    CanCancel = false,
                    Message = $"Cannot cancel booking. The tour has already departed on {departureDate:dd/MM/yyyy}."
                };
            }

            var tour = tourSchedule.Tour;
            if (tour == null)
            {
                Console.WriteLine($"Tour is NULL for booking {bookingId}");

                return new CancelValidationResult
                {
                    CanCancel = true,
                    Message = "Cancellation allowed (tour information not available)",
                    RefundAmount = 0,
                    RefundPercent = 0,
                    AppliedCondition = null
                };
            }



            if (tour.CancelConditionId == null)
            {
                Console.WriteLine("Tour has no cancel condition ID");
                return new CancelValidationResult
                {
                    CanCancel = true,
                    Message = "Cancellation allowed but no refund policy available",
                    RefundAmount = 0,
                    RefundPercent = 0,
                    AppliedCondition = null
                };
            }

            var tourCancelCondition = await _context.CancelConditions
                .FirstOrDefaultAsync(cc => cc.Id == tour.CancelConditionId);

            if (tourCancelCondition == null)
            {
                Console.WriteLine($"Cancel condition not found for ID: {tour.CancelConditionId}");
                return new CancelValidationResult
                {
                    CanCancel = true,
                    Message = "Cancellation allowed (policy not found - no refund)",
                    RefundAmount = 0,
                    RefundPercent = 0,
                    AppliedCondition = null
                };
            }



            if (tourCancelCondition.CancelStatus != CancelStatus.Active)
            {
                Console.WriteLine($"Cancel condition is not active: {tourCancelCondition.CancelStatus}");
                return new CancelValidationResult
                {
                    CanCancel = true,
                    Message = $"Cancellation allowed - {tourCancelCondition.Title} (no refund)",
                    RefundAmount = 0,
                    RefundPercent = 0,
                    AppliedCondition = null
                };
            }

            var bookingDate = booking.BookingDate.Value;
            var daysSinceBooking = (DateTime.Now - bookingDate).Days;

            Console.WriteLine($"Booking Date: {bookingDate}, Days since booking: {daysSinceBooking}");

            if (daysSinceBooking > tourCancelCondition.MinDaysBeforeTrip)
            {
                Console.WriteLine($"Cannot cancel - exceeded {tourCancelCondition.MinDaysBeforeTrip} days limit");
                return new CancelValidationResult
                {
                    CanCancel = false,
                    Message = $"Cannot cancel booking. Cancellation must be within {tourCancelCondition.MinDaysBeforeTrip} days from booking date. (Current: {daysSinceBooking} days)"
                };
            }

            bool hasRefund = tourCancelCondition.RefundPercent > 0;
            string message = hasRefund
                ? tourCancelCondition.Title
                : $"{tourCancelCondition.Title} - No refund";

            decimal? refundAmount = 0;
            int? refundPercent = tourCancelCondition.RefundPercent;

            if (hasRefund && booking.TotalPrice.HasValue && booking.TotalPrice.Value > 0)
            {
                refundAmount = booking.TotalPrice.Value * (tourCancelCondition.RefundPercent / 100m);
                refundAmount = Math.Round(refundAmount.Value, 2);
            }

            return new CancelValidationResult
            {
                CanCancel = true,
                Message = message,
                RefundAmount = refundAmount,
                RefundPercent = refundPercent,
                AppliedCondition = new CancelConditionDTO
                {
                    Id = tourCancelCondition.Id,
                    Title = tourCancelCondition.Title,
                    MinDaysBeforeTrip = tourCancelCondition.MinDaysBeforeTrip,
                    RefundPercent = tourCancelCondition.RefundPercent,
                    CancelStatus = tourCancelCondition.CancelStatus
                }
            };
        }

    }
}