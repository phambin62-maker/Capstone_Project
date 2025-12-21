using DocumentFormat.OpenXml.Wordprocessing;
using FE_Capstone_Project.Helpers;
using FE_Capstone_Project.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace FE_Capstone_Project.Controllers
{
    public class TourWebController : Controller
    {
        private readonly ApiHelper _apiHelper;
        private static readonly List<TourViewModel> _toursCache = new();
        private static readonly List<TourViewModel> _topToursCache = new();
        private static readonly List<LocationViewModel> _locationsCache = new();
        private static readonly List<TourCategoryViewModel> _tourCategoriesCache = new();

        private static readonly int pageSize = 9;

        public TourWebController(ApiHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        public async Task<IActionResult> Tours(int page = 1)
        {
            try
            {
                var toursResponse = await _apiHelper.GetAsync<TourListResponse>("Tour/GetAllTours");
                var featuredToursResponse = await _apiHelper.GetAsync<TourListResponse>("Tour/GetTopToursByEachCategories");
                var destinationsResponse = await _apiHelper.GetAsync<LocationsResponse>("Locations/GetAllLocations");
                var categoriesResponse = await _apiHelper.GetAsync<TourCategoriesResponse>("TourCategories/GetAllTourCategories");

                var tours = toursResponse?.Tours.FindAll(t => t.TourStatus) ?? new List<TourViewModel>();
                var featuredTours = featuredToursResponse?.Tours ?? new List<TourViewModel>();
                var destinations = destinationsResponse?.Data ?? new List<LocationViewModel>();
                var categories = categoriesResponse?.Data ?? new List<TourCategoryViewModel>();

                _toursCache.Clear();
                _topToursCache.Clear();
                _locationsCache.Clear();
                _tourCategoriesCache.Clear();

                _toursCache.AddRange(tours);
                _topToursCache.AddRange(featuredTours);
                _locationsCache.AddRange(destinations);
                _tourCategoriesCache.AddRange(categories);

                int totalTours = tours.Count;
                int totalPages = (int)Math.Ceiling(totalTours / (double)pageSize);

                var pagedTours = tours
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewBag.FeaturedTours = featuredTours;
                ViewBag.Destinations = destinations;
                ViewBag.Categories = categories;

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;

                return View(pagedTours);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error connecting to server: {ex.Message}";
                return View(new List<TourViewModel>());
            }
        }

        public async Task<IActionResult> TourDetails(int tourId)
        {
            try
            {
                var username = HttpContext.Session.GetString("UserName");
                var result = await _apiHelper.GetAsync<TourDetailResponse>($"Tour/GetTourById/{tourId}?username={username}");
                var tourSchedules = await _apiHelper.GetAsync<TourScheduleListResponse>($"TourSchedule/tour/available/{tourId}");
                var bookedSeatsResponse = await _apiHelper.GetAsync<List<ScheduleBookedSeatsResponse>>($"Booking/tours/{tourId}/booked-seats");
                var isInWishlist = false;
                if(username != null)
                {
                    isInWishlist = await _apiHelper.GetAsync<bool>($"Wishlist/check/{tourId}?username={username}");
                }
                if (result == null || result.Tour == null)
                {
                    ViewBag.ErrorMessage = "Tour could not be found.";
                    ViewBag.CanComment = false;
                    return View(new TourViewModel());
                }

                ViewBag.TourSchedules = tourSchedules!.Data.Where(ts => ts.ScheduleStatus == ScheduleStatus.Scheduled).ToList();
                ViewBag.CanComment = result.CanComment;
                ViewBag.BookedSeats = bookedSeatsResponse;
                ViewBag.IsInWishlist = isInWishlist;
                return View(result.Tour);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error connecting to server: {ex.Message}";
                ViewBag.CanComment = false;
                return View(new TourViewModel());
            }
        }

        public async Task<IActionResult> FilterTourList(
            string? searchValue,
            int? slcDestination,
            int? slcCategory,
            string? slcDuration,
            string? slcPriceRange)
        {
            if(_toursCache.Count == 0)
            {
                var toursResponse = await _apiHelper.GetAsync<TourListResponse>("Tour/GetAllTours");
                var featuredToursResponse = await _apiHelper.GetAsync<TourListResponse>("Tour/GetTopToursByEachCategories");
                var destinationsResponse = await _apiHelper.GetAsync<LocationsResponse>("Locations/GetAllLocations");
                var categoriesResponse = await _apiHelper.GetAsync<TourCategoriesResponse>("TourCategories/GetAllTourCategories");

                var allTours = toursResponse?.Tours.FindAll(t => t.TourStatus) ?? new List<TourViewModel>();
                var featuredTours = featuredToursResponse?.Tours ?? new List<TourViewModel>();
                var destinations = destinationsResponse?.Data ?? new List<LocationViewModel>();
                var categories = categoriesResponse?.Data ?? new List<TourCategoryViewModel>();

                _toursCache.Clear();
                _topToursCache.Clear();
                _locationsCache.Clear();
                _tourCategoriesCache.Clear();

                _toursCache.AddRange(allTours);
                _topToursCache.AddRange(featuredTours);
                _locationsCache.AddRange(destinations);
                _tourCategoriesCache.AddRange(categories);
            }

            var tours = _toursCache.AsEnumerable();

            if (!string.IsNullOrEmpty(searchValue))
            {
                tours = tours
                .Where(t => t.Name.Contains(searchValue.Trim().ToLower(), StringComparison.OrdinalIgnoreCase))
                .ToList();
            }

            if (slcDestination.HasValue && slcDestination.Value != -1)
            {
                tours = tours
                .Where(t => t.EndLocationId == slcDestination.Value)
                .ToList();
            }

            if (slcCategory.HasValue && slcCategory.Value != -1)
            {
                tours = tours
                .Where(t => t.CategoryId == slcCategory.Value)
                .ToList();
            }

            if (!string.IsNullOrEmpty(slcDuration))
            {
                tours = tours
                .Where(t => MatchDuration(t.Duration, slcDuration))
                .ToList();
            }

            if (!string.IsNullOrEmpty(slcPriceRange))
            {
                tours = tours
                .Where(t => MatchPriceRange(t.Price, slcPriceRange))
                .ToList();
            }

            int page = int.TryParse(HttpContext.Request.Query["page"], out var p) ? p : 1;
            var pagedTours = tours
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalPages = (int)Math.Ceiling(tours.Count() / (double)pageSize);
            ViewBag.CurrentPage = page;

            ViewBag.FeaturedTours = _topToursCache;
            ViewBag.Destinations = _locationsCache;
            ViewBag.Categories = _tourCategoriesCache;

            ViewBag.SearchValue = searchValue;
            ViewBag.SelectedDestination = slcDestination;
            ViewBag.SelectedCategory = slcCategory;
            ViewBag.SelectedDuration = slcDuration;
            ViewBag.SelectedPriceRange = slcPriceRange;

            return View("tours", pagedTours.ToList());
        }

        private bool MatchDuration(int duration, string value)
        {
            if (value.Contains('+')) return duration >= int.Parse(value.TrimEnd('+'));

            var parts = value.Split('-');
            int min = int.Parse(parts[0]), max = int.Parse(parts[1]);
            return duration >= min && duration <= max;
        }

        private bool MatchPriceRange(decimal price, string value)
        {
            if (value.Contains('+')) return price >= decimal.Parse(value.TrimEnd('+'));
            var parts = value.Split('-');
            decimal min = decimal.Parse(parts[0]), max = decimal.Parse(parts[1]);
            return price >= min && price <= max;
        }
    }
}
