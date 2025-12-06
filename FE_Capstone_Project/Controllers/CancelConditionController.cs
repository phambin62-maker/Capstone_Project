using FE_Capstone_Project.Filters;
using FE_Capstone_Project.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;
using System.Net;
using BE_Capstone_Project.Domain.Enums;
using BE_Capstone_Project.Domain.Enums;

namespace FE_Capstone_Project.Controllers
{
    [AuthorizeRole(2)] // Assuming 2 is Staff role
    public class CancelConditionController : Controller
    {
        private readonly HttpClient _httpClient;
        private const string BASE_API_URL = "https://localhost:7160/api/";
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CancelConditionController> _logger;

        public CancelConditionController(
            IHttpClientFactory httpClientFactory,
            ILogger<CancelConditionController> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(BASE_API_URL);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
            _httpContextAccessor = httpContextAccessor;
        }

        private string? GetToken()
        {
            return _httpContextAccessor.HttpContext?.Session?.GetString("JwtToken");
        }

        private async Task<(bool Success, T Data, string Error)> CallApiAsync<T>(
            string endpoint,
            HttpMethod method = null,
            HttpContent content = null)
        {
            try
            {
                var request = new HttpRequestMessage(method ?? HttpMethod.Get, endpoint);
                if (content != null)
                    request.Content = content;

                var token = GetToken();
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning($"[CancelConditionController] No token found for request to {endpoint}");
                    return (false, default, "Token not found. Please log in again.");
                }
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"API Call: {endpoint}, Status: {response.StatusCode}, Response: {responseContent}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return (false, default, "Login session has expired. Please log in again.");
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    return (false, default, "You don't have permission to perform this action.");
                }

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(responseContent, _jsonOptions);
                        if (apiResponse != null && apiResponse.Success)
                        {
                            return (true, apiResponse.Data, string.Empty);
                        }
                        else if (apiResponse != null)
                        {
                            return (false, default, apiResponse.Message);
                        }
                        
                        // Fallback if not standard ApiResponse
                        var directResult = JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                         return (true, directResult, string.Empty);
                    }
                    catch (JsonException ex)
                    {
                         // Try direct T deserialization as fallback
                        try
                        {
                            var directResult = JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                            return (true, directResult, string.Empty);
                        }
                        catch
                        {
                            return (false, default, $"Failed to parse response: {ex.Message}");
                        }
                    }
                }
                else
                {
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, _jsonOptions);
                        return (false, default, errorResponse?.Message ?? $"API Error: {response.StatusCode}");
                    }
                    catch
                    {
                        return (false, default, $"API Error: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API call exception: {Endpoint}", endpoint);
                return (false, default, $"Exception: {ex.Message}");
            }
        }

        public async Task<IActionResult> Index(string keyword, int pageIndex = 1, int pageSize = 10)
        {
            ViewData["Title"] = "Manage Cancel Conditions";
            
            var queryParams = new List<string>
            {
                $"pageIndex={pageIndex}",
                $"pageSize={pageSize}"
            };
            if (!string.IsNullOrEmpty(keyword))
            {
                queryParams.Add($"keyword={Uri.EscapeDataString(keyword)}");
            }

            var apiUrl = $"CancelCondition/search?{string.Join("&", queryParams)}";
            var (success, result, error) = await CallApiAsync<PagedResultDto<CancelConditionDTO>>(apiUrl);

            if (success)
            {
                 ViewBag.CurrentPage = result.Page;
                 ViewBag.PageSize = result.PageSize;
                 ViewBag.TotalCount = result.TotalCount;
                 ViewBag.TotalPages = result.TotalPages;
                 ViewBag.Keyword = keyword;
                 
                 return View(result.Data);
            }
            else
            {
                ViewBag.ErrorMessage = $"Error loading data: {error}";
                return View(new List<CancelConditionDTO>());
            }
        }

        public IActionResult Create()
        {
            ViewData["Title"] = "Add New Cancel Condition";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CancelConditionCreateRequest model)
        {
            if (!ModelState.IsValid) return View(model);

            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var (success, result, error) = await CallApiAsync<object>("CancelCondition", HttpMethod.Post, content);

            if (success)
            {
                TempData["SuccessMessage"] = "Cancel condition added successfully!";
                return RedirectToAction("Index");
            }
            else
            {
                // Display the specific error from Backend (e.g., Duplicate title)
                TempData["ErrorMessage"] = !string.IsNullOrEmpty(error) ? error : "Failed to add cancel condition!";
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var (success, result, error) = await CallApiAsync<CancelConditionDTO>($"CancelCondition/{id}");
            if (success && result != null)
            {
                ViewData["Title"] = "Update Cancel Condition";
                var updateModel = new CancelConditionUpdateRequest
                {
                    Id = result.Id,
                    Title = result.Title,
                    MinDaysBeforeTrip = result.MinDaysBeforeTrip,
                    RefundPercent = result.RefundPercent,
                    CancelStatus = result.CancelStatus
                };
                return View(updateModel);
            }

            TempData["ErrorMessage"] = error ?? "Cancel condition not found.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CancelConditionUpdateRequest model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var (success, result, error) = await CallApiAsync<object>("CancelCondition", HttpMethod.Put, content);

            if (success)
            {
                TempData["SuccessMessage"] = "Update successful!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["ErrorMessage"] = !string.IsNullOrEmpty(error) ? error : "Update failed!";
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, result, error) = await CallApiAsync<object>($"CancelCondition/{id}", HttpMethod.Delete);
            if (success)
            {
                TempData["SuccessMessage"] = "Delete successful!";
            }
            else
            {
                TempData["ErrorMessage"] = error ?? "Delete failed!";
            }
            return RedirectToAction("Index");
        }
    }
}


