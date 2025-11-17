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
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "AuthWeb");

            var bookingsResponse = await _apiHelper.GetAsync<List<UserBookingResponse>>($"Booking/user/{username}");

            return View(bookingsResponse);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            try
            {
                Console.WriteLine($"=== FE CANCEL REQUEST ===");
                Console.WriteLine($"Booking ID: {bookingId}");

                var username = HttpContext.Session.GetString("UserName");
                if (string.IsNullOrEmpty(username))
                {
                    Console.WriteLine("No username in session");
                    TempData["ErrorMessage"] = "User session invalid. Please log in again.";
                    return RedirectToAction("MyBookings");
                }

                Console.WriteLine($"Username from session: {username}");

                // Tạo request data
                var cancelRequest = new
                {
                    Username = username
                };

                Console.WriteLine($"Calling API: Booking/user/{bookingId}/cancel");

                // Gọi API để hủy booking
                var response = await _apiHelper.PutAsync<object, ApiResponse<object>>(
                    $"Booking/user/{bookingId}/cancel",
                    cancelRequest
                );

                Console.WriteLine($"API Response: {response != null}");
                Console.WriteLine($"API Success: {response?.Success}");
                Console.WriteLine($"API Message: {response?.Message}");

                if (response != null && response.Success)
                {
                    Console.WriteLine("Cancellation successful in FE");
                    TempData["SuccessMessage"] = "Booking cancelled successfully!";

                    // Tạo thông báo
                    try
                    {
                        var userId = GetCurrentUserId();
                        var notificationDto = new
                        {
                            UserId = userId,
                            Title = "Booking Cancelled",
                            Message = $"Your booking #{bookingId} has been cancelled successfully.",
                            NotificationType = "System"
                        };
                        await _apiHelper.PostAsync<object, object>(NOTIFICATION_API_ENDPOINT, notificationDto);
                        Console.WriteLine("Notification sent");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to send cancellation notification: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"Cancellation failed: {response?.Message}");
                    TempData["ErrorMessage"] = response?.Message ?? "Failed to cancel booking. Please try again.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in FE: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = $"Error cancelling booking: {ex.Message}";
            }

            return RedirectToAction("MyBookings");
        }
        public class ApiResponse<T>
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public T Data { get; set; }
        }
    }
}