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

            // Example: Save booking to database
            // var booking = new Booking { ... };
            // await _context.Bookings.AddAsync(booking);
            // await _context.SaveChangesAsync();

            // For now, just confirm receipt
            Console.WriteLine($"request: {request.ToString()}");

            // Redirect or return confirmation
            return RedirectToAction("Tours", "TourWeb");
        }
    }
}