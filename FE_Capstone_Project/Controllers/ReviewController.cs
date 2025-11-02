using FE_Capstone_Project.Helpers;
using FE_Capstone_Project.Models;
using Microsoft.AspNetCore.Mvc;

namespace FE_Capstone_Project.Controllers
{
    public class ReviewController : Controller
    {
        private readonly ApiHelper _apiHelper;

        public ReviewController(ApiHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        [HttpPost]
        public async Task<IActionResult> PostComment(int tourId, string comment, int rating)
        {
            try
            {
                var username = HttpContext.Session.GetString("UserName");
                if (string.IsNullOrEmpty(username))
                {
                    TempData["ErrorMessage"] = "You must be logged in to post a review.";
                    return RedirectToAction("TourDetails", "TourWeb", new { tourId });
                }

                var reviewData = new
                {
                    username = username,
                    tourId = tourId,
                    stars = rating,
                    comment = comment
                };

                var response = await _apiHelper.PostAsync<object, object>("Review/AddReview", reviewData);

                return RedirectToAction("TourDetails", "TourWeb", new { tourId });
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error connecting to server: {ex.Message}";
                return RedirectToAction("TourDetails", "TourWeb", new { tourId });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditComment(int tourId, int reviewId, string comment, int rating)
        {
            try
            {
                var reviewData = new
                {
                    id = reviewId,
                    stars = rating,
                    comment = comment
                };

                var response = await _apiHelper.PostAsync<object, object>("Review/EditReview", reviewData);

                return RedirectToAction("TourDetails", "TourWeb", new { tourId });
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error connecting to server: {ex.Message}";
                return RedirectToAction("TourDetails", "TourWeb", new { tourId });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteComment(int tourId, int reviewId)
        {
            try
            {
                var response = await _apiHelper.DeleteAsync($"Review/DeleteReview/{reviewId}");
                return RedirectToAction("TourDetails", "TourWeb", new { tourId });
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error connecting to server: {ex.Message}";
                return RedirectToAction("TourDetails", "TourWeb", new { tourId });
            }
        }
    }
}
