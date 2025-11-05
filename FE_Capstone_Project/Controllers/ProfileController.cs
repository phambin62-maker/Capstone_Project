using BE_Capstone_Project.Domain.Models;
using FE_Capstone_Project.Helpers;
using FE_Capstone_Project.Models;
using Microsoft.AspNetCore.Mvc;
using static FE_Capstone_Project.Models.WishlistModels;

namespace FE_Capstone_Project.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ApiHelper _apiHelper;
        private static readonly List<WishlistData> _toursCache = new();
        private static readonly int pageSize = 9;

        public ProfileController(ApiHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        public async Task<IActionResult> Wishlist(int page = 1)
        {
            try
            {
                var username = HttpContext.Session.GetString("UserName");
                var wishlistResponse = await _apiHelper.GetAsync<WishlistResponseModel>($"Wishlist?username={username}");

                if (wishlistResponse == null || wishlistResponse.Wishlist == null)
                {
                    ViewBag.ErrorMessage = "No tours available.";
                    return View(new List<WishlistData>());
                }

                var wishlistData = wishlistResponse.Wishlist;

                _toursCache.Clear();

                _toursCache.AddRange(wishlistData);

                int totalTours = wishlistData.Count;
                int totalPages = (int)Math.Ceiling(totalTours / (double)pageSize);

                var pagedTours = wishlistData
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;

                return View(pagedTours);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error connecting to server: {ex.Message}";
                return View(new List<WishlistData>());
            }
        }

        public async Task<IActionResult> AddToWishlist(int tourId)
        {
            try
            {
                var username = HttpContext.Session.GetString("UserName");
                var wishlistData = new { tourId, username };
                var response = await _apiHelper.PostAsync<object, object>($"Wishlist", wishlistData);

                return RedirectToAction("TourDetails", "TourWeb", new { tourId });
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error connecting to server: {ex.Message}";
                return RedirectToAction("TourDetails", "TourWeb", new { tourId });
            }
        }

        public async Task<IActionResult> RemoveFromWishlist(int tourId)
        {
            try
            {
                var username = HttpContext.Session.GetString("UserName");
                var response = await _apiHelper.DeleteAsync($"Wishlist/tour/{tourId}?username={username}");

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
