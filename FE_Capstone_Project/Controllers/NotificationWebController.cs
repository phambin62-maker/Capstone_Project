using FE_Capstone_Project.Helpers;
using FE_Capstone_Project.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Text.Json;

public class NotificationWebController : Controller
{
    private readonly ApiHelper _apiHelper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _apiBaseUrl = "api/Notification";

    public NotificationWebController(ApiHelper apiHelper, IHttpContextAccessor httpContextAccessor)
    {
        _apiHelper = apiHelper;
        _httpContextAccessor = httpContextAccessor;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdClaim, out int userId)) { return userId; }
        var userIdString = _httpContextAccessor.HttpContext?.Session.GetString("UserId");
        if (int.TryParse(userIdString, out userId)) { return userId; }
        return 0;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        int userId = GetCurrentUserId();
        if (userId == 0) return RedirectToAction("Login", "AuthWeb"); 

        await _apiHelper.PostAsync<object, object>($"{_apiBaseUrl}/mark-read/{userId}", new { });
        var list = await _apiHelper.GetAsync<List<NotificationViewModel>>($"{_apiBaseUrl}/user/{userId}");

        return View(list ?? new List<NotificationViewModel>());
    }

    [HttpGet("api/[controller]/unread")]
    public async Task<IActionResult> GetUnreadCount()
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

}