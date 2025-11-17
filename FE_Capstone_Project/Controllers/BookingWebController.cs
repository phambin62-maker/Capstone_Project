using BE_Capstone_Project.Application.BookingManagement.DTOs;
using FE_Capstone_Project.Filters;
using FE_Capstone_Project.Helpers;
using FE_Capstone_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BookingRequest = FE_Capstone_Project.Models.BookingRequest;

namespace FE_Capstone_Project.Controllers
{
    //[Authorize(Roles = "Customer")] // Tất cả role đã đăng nhập đều có thể đặt tour
    public class BookingWebController : Controller
    {
        private readonly ApiHelper _apiHelper;

        public BookingWebController(ApiHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        public async Task<IActionResult> Index(int tourId)
        {
            var username = HttpContext.Session.GetString("UserName");

            var tourResponse = await _apiHelper.GetAsync<TourDetailResponse>($"Tour/GetTourById/{tourId}");
            var tourSchedulesResponse = await _apiHelper.GetAsync<TourScheduleListResponse>($"TourSchedule/tour/available/{tourId}");
            var userResponse = await _apiHelper.GetAsync<UserDto>($"User/by-username/{username}");
            var locationsResponse = await _apiHelper.GetAsync<LocationsResponse>("Locations/GetAllLocations");
            var bookedSeatsResponse = await _apiHelper.GetAsync<List<ScheduleBookedSeatsResponse>>($"Booking/tours/{tourId}/booked-seats");

            ViewBag.Tour = tourResponse!.Tour;
            ViewBag.TourSchedules = tourSchedulesResponse!.Data;
            ViewBag.User = userResponse;
            ViewBag.DepartureLocations = locationsResponse!.Data;
            ViewBag.BookedSeats = bookedSeatsResponse;

            var firstImage = tourResponse!.Tour!.TourImages?.First().Image;
            ViewBag.FirstImage = firstImage;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> BookTour(BookingRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid booking data.");

            var username = HttpContext.Session.GetString("UserName");

            var bookingResponse = await _apiHelper.PostAsync<BookingRequest, BookingResponse>(
                $"Booking?username={username}", request);

            if (bookingResponse == null || !bookingResponse.Success)
                return BadRequest("Booking failed.");

            // 🔥 Check the payment method from the form
            if (request.PaymentMethod == "vnpay")
            {
                var paymentRequest = new PaymentRequest()
                {
                    OrderType = "Tour",
                    Amount = bookingResponse.TotalPrice,
                    OrderDescription = $"Booking #{bookingResponse.BookingId} - {bookingResponse.TourName}",
                    Name = $"{bookingResponse.FirstName} {bookingResponse.LastName}",
                };

                var paymentResponse = await _apiHelper.PostAsync<PaymentRequest, PaymentResponse>(
                    $"Payment/create-payment", paymentRequest);

                if (paymentResponse == null || string.IsNullOrEmpty(paymentResponse.PaymentUrl))
                    return BadRequest("Failed to create payment URL.");

                return Redirect(paymentResponse.PaymentUrl);
            }
            else if (request.PaymentMethod == "cash")
            {
                return RedirectToAction("Index", "Home");
            }

            return BadRequest("Invalid payment method.");
        }

            return Redirect(paymentResponse.PaymentUrl);
        }
        // ========== THÊM API MỚI: LẤY DANH SÁCH BOOKINGS CỦA USER ==========

        [HttpGet]
        public async Task<IActionResult> MyBookings()
        {
            try
            {
                var username = HttpContext.Session.GetString("UserName");
                if (string.IsNullOrEmpty(username))
                {
                    return RedirectToAction("Login", "AuthWeb"); // Điều hướng đến trang login nếu chưa đăng nhập
                }

                // Lấy thông tin user để có userId
                var userResponse = await _apiHelper.GetAsync<UserDto>($"User/by-username/{username}");
                if (userResponse == null)
                {
                    TempData["Error"] = "Không thể lấy thông tin người dùng.";
                    return View(new List<BookingDto>());
                }

                // Gọi API lấy danh sách bookings của user
                var bookingsResponse = await _apiHelper.GetAsync<BookingResponse>($"Booking/user/{userResponse.Id}");

                if (bookingsResponse == null || !bookingsResponse.Success)
                {
                    TempData["Error"] = "Không thể tải danh sách đặt tour.";
                    return View(new List<BookingDto>());
                }

                return View(bookingsResponse.Data);
            }
            catch (UnauthorizedAccessException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Đã xảy ra lỗi: {ex.Message}";
                return View(new List<BookingDto>());
            }
        }

        // API endpoint để lấy bookings dưới dạng JSON (cho AJAX calls)
        [HttpGet]
        public async Task<IActionResult> GetUserBookingsJson()
        {
            try
            {
                var username = HttpContext.Session.GetString("UserName");
                if (string.IsNullOrEmpty(username))
                {
                    return Unauthorized(new { success = false, message = "User not logged in" });
                }

                var userResponse = await _apiHelper.GetAsync<UserDto>($"User/by-username/{username}");
                if (userResponse == null)
                {
                    return NotFound(new { success = false, message = "User not found" });
                }

                var bookingsResponse = await _apiHelper.GetAsync<BookingResponse>($"Booking/user/{userResponse.Id}");

                if (bookingsResponse == null || !bookingsResponse.Success)
                {
                    return BadRequest(new { success = false, message = "Failed to get bookings" });
                }

                return Ok(new { success = true, data = bookingsResponse.Data });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Chi tiết booking
        [HttpGet]
        public async Task<IActionResult> BookingDetails(int id)
        {
            try
            {
                var bookingResponse = await _apiHelper.GetAsync<BookingDto>($"Booking/{id}");

                if (bookingResponse == null)
                {
                    TempData["Error"] = "Không tìm thấy thông tin đặt tour.";
                    return RedirectToAction("MyBookings");
                }

                return View(bookingResponse);
            }
            catch (UnauthorizedAccessException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Đã xảy ra lỗi: {ex.Message}";
                return RedirectToAction("MyBookings");
            }
        }
    }
}