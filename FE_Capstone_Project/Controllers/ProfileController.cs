using BE_Capstone_Project.Domain.Models;
using FE_Capstone_Project.Filters;
using FE_Capstone_Project.Helpers;
using FE_Capstone_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static FE_Capstone_Project.Models.WishlistModels;

namespace FE_Capstone_Project.Controllers
{
    [AuthorizeRole(1, 2, 3)]
    public class ProfileController : Controller
    {
        private readonly ApiHelper _apiHelper;
        private static readonly List<WishlistData> _toursCache = new();
        private static readonly int pageSize = 9;

        // Đã sửa lỗi "api/api/"
        private const string NOTIFICATION_API_ENDPOINT = "Notification";

        public ProfileController(ApiHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        private int GetCurrentUserId()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (int.TryParse(userIdString, out int userId))
            {
                return userId;
            }
            return 0;
        }

        // (Hàm Wishlist đã sửa)
        public async Task<IActionResult> Wishlist(int page = 1)
        {
            try
            {
                var username = HttpContext.Session.GetString("UserName");
                var wishlistResponse = await _apiHelper.GetAsync<WishlistResponseModel>($"Wishlist?username={username}");

                if (wishlistResponse == null || wishlistResponse.Wishlist == null || !wishlistResponse.Wishlist.Any())
                {
                    ViewBag.ErrorMessage = "No tours available in your wishlist.";
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

        // === SỬA: HÀM ADD (GIỮ COMMENT ĐỂ TRÁNH TRÙNG) ===
        public async Task<IActionResult> AddToWishlist(int tourId)
        {
            try
            {
                var username = HttpContext.Session.GetString("UserName");
                var userId = GetCurrentUserId();

                if (userId == 0)
                {
                    TempData["ErrorMessage"] = "User session invalid. Please log in again.";
                    return RedirectToAction("TourDetails", "TourWeb", new { tourId });
                }

                var wishlistData = new { tourId, username };
                // API BE (Wishlist) này SẼ TỰ TẠO THÔNG BÁO
                var response = await _apiHelper.PostAsync<object, WishlistData>($"Wishlist", wishlistData);

                if (response == null || response.TourId <= 0)
                {
                    TempData["ErrorMessage"] = "Could not add to wishlist. (API Error)";
                }

                // (KHỐI NÀY VẪN BỊ COMMENT ĐỂ TRÁNH TRÙNG)
                /*
                if (response != null && response.TourId > 0)
                {
                    try { ... } catch (Exception ex) { ... }
                }
                */

                return RedirectToAction("TourDetails", "TourWeb", new { tourId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error connecting to server: {ex.Message}";
                return RedirectToAction("TourDetails", "TourWeb", new { tourId });
            }
        }

        // === SỬA: HÀM REMOVE (PHỤC HỒI CODE ĐỂ TẠO THÔNG BÁO) ===
        public async Task<IActionResult> RemoveFromWishlist(int tourId, string tourName)
        {
            try
            {
                var username = HttpContext.Session.GetString("UserName");
                var userId = GetCurrentUserId();

                if (userId == 0)
                {
                    TempData["ErrorMessage"] = "User session invalid. Please log in again.";
                    return RedirectToAction("TourDetails", "TourWeb", new { tourId });
                }

                // API BE (Wishlist) này KHÔNG TẠO THÔNG BÁO
                await _apiHelper.DeleteAsync($"Wishlist/tour/{tourId}?username={username}");

                // VÌ VẬY, CHÚNG TA PHẢI TẠO NÓ Ở ĐÂY (FE)
                try
                {
                    var notificationDto = new
                    {
                        UserId = userId,
                        Title = "Removed from Wishlist",
                        Message = $"Tour '{tourName ?? "N/A"}' has been removed from your wishlist.",
                        NotificationType = "System"
                    };
                    // Dòng này (FE) sẽ tạo thông báo
                    _ = _apiHelper.PostAsync<object, object>(NOTIFICATION_API_ENDPOINT, notificationDto);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send remove-wishlist notification from FE: {ex.Message}");
                }

                return RedirectToAction("TourDetails", "TourWeb", new { tourId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error connecting to server: {ex.Message}";
                return RedirectToAction("TourDetails", "TourWeb", new { tourId });
            }
        }

        public async Task<IActionResult> MyBookings()
        {
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var bookingsResponse = await _apiHelper.GetAsync<List<UserBookingResponse>>($"Booking/user/{username}");

            return View(bookingsResponse);
        }
    }
}