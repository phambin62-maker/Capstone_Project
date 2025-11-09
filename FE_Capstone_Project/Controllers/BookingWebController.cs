using FE_Capstone_Project.Helpers;
using FE_Capstone_Project.Models;
using Microsoft.AspNetCore.Mvc;

namespace FE_Capstone_Project.Controllers
{
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

            ViewBag.Tour = tourResponse.Tour;
            ViewBag.TourSchedules = tourSchedulesResponse.Data;
            ViewBag.User = userResponse;
            ViewBag.DepartureLocations = locationsResponse.Data;

            var firstImage = tourResponse.Tour.TourImages?.First().Image;
            ViewBag.FirstImage = firstImage;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> BookTour(BookingRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid booking data.");

            var username = HttpContext.Session.GetString("UserName");

            var bookingResponse = await _apiHelper.PostAsync<BookingRequest, BookingResponse>($"Booking?username={username}", request);
            
            if (bookingResponse == null || !bookingResponse.Success)
                return BadRequest("Booking failed.");

            var paymentRequest = new PaymentRequest()
            {
                OrderType = "Tour",
                Amount = bookingResponse.TotalPrice,
                OrderDescription = $"Booking #{bookingResponse.BookingId} - {bookingResponse.TourName}",
                Name = $"{bookingResponse.FirstName} {bookingResponse.LastName}",
            };

            var paymentResponse = await _apiHelper.PostAsync<PaymentRequest, PaymentResponse>($"Payment/create-payment", paymentRequest);

            if (paymentResponse == null || string.IsNullOrEmpty(paymentResponse.PaymentUrl))
                return BadRequest("Failed to create payment URL.");

            return Redirect(paymentResponse.PaymentUrl);
        }
    }
}