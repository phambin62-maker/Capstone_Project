using FE_Capstone_Project.Helpers;
using FE_Capstone_Project.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace FE_Capstone_Project.Controllers
{
    // Đảm bảo chỉ Staff mới vào được
    [FE_Capstone_Project.Filters.AuthorizeRole(2)]
    public class StaffNotificationWebController : Controller
    {
        private readonly ApiHelper _apiHelper;
        private readonly string _apiBaseUrl = "Notification";

        public StaffNotificationWebController(ApiHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        private int GetCurrentUserId()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            return userId.HasValue ? userId.Value : 0;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                int userId = GetCurrentUserId();
                if (userId == 0) return RedirectToAction("Login", "AuthWeb");

                // Lấy danh sách thông báo
                var list = await _apiHelper.GetAsync<List<NotificationViewModel>>($"{_apiBaseUrl}/user/{userId}");
                return View(list ?? new List<NotificationViewModel>());
            }
            catch (Exception ex)
            {
                return View(new List<NotificationViewModel>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            int userId = GetCurrentUserId();
            if (userId > 0) await _apiHelper.PostAsync<object, object>($"{_apiBaseUrl}/mark-read/{userId}", new { });
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAll()
        {
            int userId = GetCurrentUserId();
            if (userId > 0) await _apiHelper.DeleteAsync($"{_apiBaseUrl}/user/{userId}");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSingle(int notificationId)
        {
            await _apiHelper.DeleteAsync($"{_apiBaseUrl}/{notificationId}");
            return RedirectToAction("Index");
        }
    }
}