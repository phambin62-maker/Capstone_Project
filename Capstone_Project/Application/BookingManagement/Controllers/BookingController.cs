using BE_Capstone_Project.Application.BookingManagement.DTOs;
using BE_Capstone_Project.Application.BookingManagement.Services.Interfaces;
using BE_Capstone_Project.Application.Services;
using BE_Capstone_Project.Application.TourManagement.Services.Interfaces;
using BE_Capstone_Project.Domain.Enums;
using BE_Capstone_Project.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BE_Capstone_Project.Application.BookingManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IUserService _userService;
        private readonly ITourService _tourService;
        private readonly ITourScheduleService _tourScheduleService;

        public BookingController(
            IBookingService bookingService,
            IUserService userService,
            ITourService tourService,
            ITourScheduleService tourScheduleService)
        {
            _bookingService = bookingService;
            _userService = userService;
            _tourService = tourService;
            _tourScheduleService = tourScheduleService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAll()
        {
            var list = await _bookingService.GetAllAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        [Authorize] // Cần đăng nhập để xem booking
        public async Task<IActionResult> GetById(int id)
        {
            var booking = await _bookingService.GetByIdAsync(id);
            if (booking == null) return NotFound();
            return Ok(booking);
        }

        //[HttpGet("user/{userId}")]
        //[Authorize] // Cần đăng nhập để xem booking của user
        //public async Task<IActionResult> GetByUserId(int userId)
        //{
        //    var list = await _bookingService.GetByUserIdAsync(userId);
        //    return Ok(list);
        //}

        [HttpGet("user/{username}")]
        /*[Authorize] */// Cần đăng nhập để xem booking của user
        public async Task<IActionResult> GetBookingsByUsername(string username)
        {
            var user = await _userService.GetUserByUsername(username);
            if (user == null) return NotFound();
            var list = await _bookingService.GetByUserIdAsync2(user!.Id);
            if (list == null) return NotFound();
            return Ok(list);
        }

        [HttpPost]
        [Authorize] // Cần đăng nhập để tạo booking
        public async Task<IActionResult> Create([FromBody] BookingRequest request, [FromQuery] string username)
        {
            try
            {
                var user = await _userService.GetUserByUsername(username);
                var tour = await _tourService.GetTourByScheduleId(request.Tour_Schedule);
                decimal totalPrice = CalculateTotalPrice(request, tour!);

                CreateBookingDTO booking = new CreateBookingDTO
                {
                    UserId = user!.Id,
                    TourScheduleId = request.Tour_Schedule,
                    FirstName = request.First_Name,
                    LastName = request.Last_Name,
                    PhoneNumber = request.Phone,
                    Email = request.Email,
                    CertificateId = request.Certificate_Id,

                    PaymentStatus = PaymentStatus.Pending,
                    BookingStatus = BookingStatus.Pending,

                    RefundAmount = null,
                    RefundDate = null,
                    PaymentMethod = null,
                    PaymentDate = null,
                    ExpirationTime = request.PaymentMethod == "vnpay" ? DateTime.Now.AddMinutes(10) : DateTime.Now.AddDays(3),
                    BookingDate = DateTime.Now,
                    TotalPrice = totalPrice
                };

                var bookingId = await _bookingService.CreateAsync(booking);
                if (bookingId == 0)
                {
                    return BadRequest(new { message = "Failed to create booking.", success = false });
                }
                else
                {
                    await _bookingService.AddBookingCustomersToBookId(bookingId, request.Travelers);
                    return Ok(new BookingSuccessResponse
                    {
                        BookingId = bookingId,
                        TotalPrice = (int)totalPrice,
                        TourName = tour!.Name!,
                        FirstName = request.First_Name,
                        LastName = request.Last_Name,
                        Message = "Booking created successfully",
                        Success = true
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return BadRequest(new { message = ex.Message, success = false });
            }
        }


        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] CreateBookingDTO dto)
        {
            var success = await _bookingService.UpdateAsync(id, dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _bookingService.DeleteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpPost("payment-update")]
        public async Task<IActionResult> UpdatePaymentStatus(PaymentDTO payment)
        {
            var success = await _bookingService.UpdatePaymentStatus(payment);
            if (!success) return NotFound();
            return Ok(true);
        }

        [HttpGet("tours/{tourId}/booked-seats")]
        public async Task<IActionResult> GetBookedSeats(int tourId)
        {
            var result = await _bookingService.GetBookedSeatsByTour(tourId);
            if (result == null) return NotFound();
            return Ok(result);
        }
        [HttpPut("user/{bookingId}/cancel")]
        [Authorize]
        public async Task<IActionResult> CancelBookingByUser(int bookingId, [FromBody] CancelBookingRequest request)
        {
            try
            {
                var user = await _userService.GetUserByUsername(request.Username);
                if (user == null)
                    return Unauthorized(new { message = "User not found", success = false });

                var booking = await _bookingService.GetByIdAsync(bookingId);
                if (booking == null || booking.UserId != user.Id)
                    return Unauthorized(new { message = "Booking not found or not authorized", success = false });
                var cancelValidation = await _bookingService.ValidateCancelConditionAsync(bookingId);
                if (!cancelValidation.CanCancel)
                {
                    return BadRequest(new
                    {
                        message = cancelValidation.Message,
                        success = false,
                        canCancel = false
                    });
                }
                var cancelRequest = new UpdateBookingStatusRequest
                {
                    BookingStatus = BookingStatus.Cancelled
                };

                var success = await _bookingService.UpdateBookingStatusAsync(bookingId, cancelRequest);
                if (!success)
                    return BadRequest(new { message = "Cannot cancel booking", success = false });

                return Ok(new
                {
                    message = "Booking cancelled successfully",
                    success = true,
                    refundAmount = cancelValidation.RefundAmount,
                    refundPercent = cancelValidation.RefundPercent
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, success = false });
            }
        }
        [HttpGet("{bookingId}/cancel-validation")]
        [Authorize]
        public async Task<IActionResult> CheckCancelConditions(int bookingId)
        {
            try
            {
                var validation = await _bookingService.ValidateCancelConditionAsync(bookingId);
                return Ok(validation);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, success = false });
            }
        }


        [HttpGet("staff/bookings")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetBookingsForStaff([FromQuery] BookingSearchRequest request)
        {
            try
            {
                var result = await _bookingService.GetBookingsForStaffAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, success = false });
            }
        }

        [HttpGet("staff/bookings/{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetBookingDetailForStaff(int id)
        {
            try
            {
                var booking = await _bookingService.GetBookingDetailForStaffAsync(id);
                if (booking == null) return NotFound();
                return Ok(booking);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, success = false });
            }
        }

        [HttpPut("staff/bookings/{id}/status")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateBookingStatus(int id, [FromBody] UpdateBookingStatusRequest request)
        {
            try
            {
                var success = await _bookingService.UpdateBookingStatusAsync(id, request);
                if (!success) return NotFound();
                return Ok(new { message = "Booking status updated successfully", success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, success = false });
            }
        }

        [HttpPut("staff/bookings/{id}/payment-status")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdatePaymentStatusByStaff(int id, [FromBody] UpdatePaymentStatusRequest request)
        {
            try
            {
                var success = await _bookingService.UpdatePaymentStatusByStaffAsync(id, request);
                if (!success) return NotFound();
                return Ok(new { message = "Payment status updated successfully", success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, success = false });
            }
        }

        [HttpGet("staff/booking-statuses")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAvailableBookingStatuses()
        {
            try
            {
                var statuses = await _bookingService.GetAvailableBookingStatusesAsync();
                return Ok(statuses);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, success = false });
            }
        }

        [HttpGet("staff/payment-statuses")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAvailablePaymentStatuses()
        {
            try
            {
                var statuses = await _bookingService.GetAvailablePaymentStatusesAsync();
                return Ok(statuses);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, success = false });
            }
        }

        private decimal CalculateTotalPrice(BookingRequest request, Tour tour)
        {
            if (tour.Price == null)
                throw new Exception("Tour price is not set.");

            int totalPeople = request.Adults + request.Children + request.Infants;

            decimal basePrice = tour.Price.Value;
            decimal childPrice = basePrice * (1 - (tour.ChildDiscount ?? 0) / 100);

            decimal groupDiscount = 0;
            if (totalPeople >= 6 && tour.GroupDiscount.HasValue)
            {
                groupDiscount = tour.GroupDiscount.Value / 100;
            }

            decimal totalPrice = (request.Adults * basePrice) + (request.Children * childPrice);

            totalPrice *= (1 - groupDiscount);

            return totalPrice;
        }
    }
}