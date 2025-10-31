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

        public TourWebController(ApiHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        public async Task<IActionResult> Tours()
        {
            try
            {
                var allToursTask = _apiHelper.GetAsync<TourListResponse>("Tour/GetAllTours");
                var topToursTask = _apiHelper.GetAsync<TourListResponse>("Tour/GetTopToursByEachCategories");

                await Task.WhenAll(allToursTask, topToursTask);

                var allToursResult = allToursTask.Result;
                var topToursResult = topToursTask.Result;

                var tours = allToursResult?.Tours ?? new List<TourViewModel>();
                var featuredTours = topToursResult?.Tours ?? new List<TourViewModel>();

                ViewBag.FeaturedTours = featuredTours;
                return View(tours);
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
                if (result == null || result.Tour == null)
                {
                    ViewBag.ErrorMessage = "Tour could not be found.";
                    return View(new TourViewModel());
                }

                ViewBag.CanComment = result.CanComment;
                return View(result.Tour);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error connecting to server: {ex.Message}";
                return View(new TourViewModel());
            }
        }
    }
}
