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

        private const string NOTIFICATION_API_ENDPOINT = "Notification";

        public ProfileController(ApiHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        // === BẮT ĐẦU SỬA LỖI (ĐỌC INT THAY VÌ STRING) ===
        private int GetCurrentUserId()
        {
            // Sửa: Đọc Int32 (số) để khớp với AuthWebController
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue && userId.Value > 0)
            {
                return userId.Value;
            }
            return 0;
        }
        // === KẾT THÚC SỬA LỖI ===

        // === SỬA LỖI LOGIC (DÙNG UserName) ===
        public async Task<IActionResult> Wishlist(int page = 1)
        {
            try
            {
                // Sửa: Dùng "UserName" (là tên) thay vì "UserEmail"
                var username = HttpContext.Session.GetString("UserName");
                if (string.IsNullOrEmpty(username))
                {
                    TempData["ErrorMessage"] = "User session invalid. Please log in again.";
                    return View(new List<WishlistData>());
                }

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

        // === SỬA LỖI LOGIC (DÙNG UserName) ===
        public async Task<IActionResult> AddToWishlist(int tourId)
        {
            try
            {
                // Sửa: Dùng "UserName" (là tên)
                var username = HttpContext.Session.GetString("UserName");
                var userId = GetCurrentUserId(); // Hàm này giờ đã đọc Int32 (đúng)

                if (userId == 0 || string.IsNullOrEmpty(username))
                {
                    TempData["ErrorMessage"] = "User session invalid. Please log in again.";
                    return RedirectToAction("TourDetails", "TourWeb", new { tourId });
                }

                // Gửi "TourId" và "Username" (chữ hoa) để khớp với BE
                var wishlistData = new { TourId = tourId, Username = username };

                // API BE (Wishlist) này SẼ TỰ TẠO THÔNG BÁO
                var response = await _apiHelper.PostAsync<object, WishlistData>($"Wishlist", wishlistData);

                if (response == null || response.TourId <= 0)
                {
                    TempData["ErrorMessage"] = "Could not add to wishlist. (API Error)";
                }

                return RedirectToAction("TourDetails", "TourWeb", new { tourId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error connecting to server: {ex.Message}";
                return RedirectToAction("TourDetails", "TourWeb", new { tourId });
            }
        }

        // === SỬA LỖI LOGIC (DÙNG UserName) ===
        public async Task<IActionResult> RemoveFromWishlist(int tourId, string tourName)
        {
            try
            {
                // Sửa: Dùng "UserName" (là tên)
                var username = HttpContext.Session.GetString("UserName");
                var userId = GetCurrentUserId(); // Hàm này giờ đã đọc Int32 (đúng)

                if (userId == 0 || string.IsNullOrEmpty(username))
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

        // === SỬA LỖI LOGIC (DÙNG UserName) ===
        public async Task<IActionResult> MyBookings()
        {
            // Sửa: Dùng "UserName" (là tên)
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "AuthWeb");

            var bookingsResponse = await _apiHelper.GetAsync<List<UserBookingResponse>>($"Booking/user/{username}");

            return View(bookingsResponse);
        }
    }
}