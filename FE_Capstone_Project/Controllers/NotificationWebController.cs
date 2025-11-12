using FE_Capstone_Project.Helpers;
using FE_Capstone_Project.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Text.Json;
using System.Linq;
using System;

public class NotificationWebController : Controller
{
    private readonly ApiHelper _apiHelper;
    private readonly string _apiBaseUrl = "Notification";

    public NotificationWebController(ApiHelper apiHelper)
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

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "AuthWeb");
            var list = await _apiHelper.GetAsync<List<NotificationViewModel>>($"{_apiBaseUrl}/user/{userId}");
            return View(list ?? new List<NotificationViewModel>());
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = ex.Message;
            return View(new List<NotificationViewModel>());
        }
    }

    [HttpPost]
    public async Task<IActionResult> MarkAllAsRead()
    {
        try
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Index");
            await _apiHelper.PostAsync<object, object>($"{_apiBaseUrl}/mark-read/{userId}", new { });
            return RedirectToAction("Index");
        }
        catch (Exception)
        {
            return RedirectToAction("Index");
        }
    }

    // === THÊM MỚI 1: ACTION "DELETE ALL" ===
    [HttpPost]
    public async Task<IActionResult> DeleteAll()
    {
        try
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Index");

            // Gọi API (BE) [HttpDelete] .../api/Notification/user/{userId}
            await _apiHelper.DeleteAsync($"{_apiBaseUrl}/user/{userId}");

            return RedirectToAction("Index");
        }
        catch (Exception)
        {
            return RedirectToAction("Index");
        }
    }

    // === THÊM MỚI 2: ACTION "DELETE SINGLE" ===
    [HttpPost]
    public async Task<IActionResult> DeleteSingle(int notificationId)
    {
        try
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Index");

            // Gọi API (BE) [HttpDelete] .../api/Notification/{id}
            await _apiHelper.DeleteAsync($"{_apiBaseUrl}/{notificationId}");

            return RedirectToAction("Index");
        }
        catch (Exception)
        {
            return RedirectToAction("Index");
        }
    }

    // (Các API cho JavaScript giữ nguyên)
    [HttpGet("api/[controller]/unread")]
    public async Task<IActionResult> GetUnreadCount()
    {
        try
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Ok(new { count = 0 });
            var result = await _apiHelper.GetAsync<JsonElement>($"{_apiBaseUrl}/unread/{userId}");
            int count = 0;
            if (result.ValueKind != JsonValueKind.Undefined &&
                result.TryGetProperty("count", out var countProperty) &&
                countProperty.TryGetInt32(out count))
            {
                return Ok(new { count = count });
            }
            return Ok(new { count = 0 });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"ApiHelper Error (unread): {ex.Message}" });
        }
    }

    [HttpGet("api/[controller]/recent")]
    public async Task<IActionResult> GetRecentNotifications()
    {
        try
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Ok(new List<NotificationViewModel>());
            var list = await _apiHelper.GetAsync<List<NotificationViewModel>>($"{_apiBaseUrl}/recent/{userId}");
            return Ok(list ?? new List<NotificationViewModel>());
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"ApiHelper Error (recent): {ex.Message}" });
        }
    }

    [HttpPost("api/[controller]/mark-dropdown-read")]
    public async Task<IActionResult> MarkDropdownRead()
    {
        try
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();
            await _apiHelper.PostAsync<object, object>($"{_apiBaseUrl}/mark-read/{userId}", new { });
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"ApiHelper Error (mark-read): {ex.Message}" });
        }
    }

    [HttpPost("api/[controller]/mark-read-single/{notificationId}")]
    public async Task<IActionResult> MarkSingleAsRead(int notificationId)
    {
        try
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();
            var result = await _apiHelper.PostAsync<object, bool>(
                $"{_apiBaseUrl}/mark-read-single/{notificationId}/{userId}",
                new { }
            );
            return Ok(new { success = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}