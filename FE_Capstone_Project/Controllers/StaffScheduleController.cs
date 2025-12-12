using BE_Capstone_Project.Domain.Models;
using FE_Capstone_Project.Filters;
using FE_Capstone_Project.Models;
using FE_Capstone_Project.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FE_Capstone_Project.Controllers
{
    [AuthorizeRole(2)]
    public class StaffScheduleController : Controller
    {
        private readonly HttpClient _httpClient;
        private const string BASE_API_URL = "https://localhost:7160/api/";
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly DataService _dataService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<StaffScheduleController> _logger;

        public StaffScheduleController(
            IHttpClientFactory httpClientFactory,
            ILogger<StaffScheduleController> logger,
            DataService dataService,
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
            _dataService = dataService;
            _httpContextAccessor = httpContextAccessor;
        }
        private string? GetToken()
        {
            return _httpContextAccessor.HttpContext?.Session?.GetString("JwtToken");
        }

        private int GetCurrentUserId()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (int.TryParse(userIdString, out int userId))
            {
                return userId;
            }
            _logger.LogWarning("Could not find 'UserId' in Session.");
            return 0;
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
                    _logger.LogWarning($"[StaffScheduleController] No token found for request to {endpoint}");
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
                        // Try to parse as ApiResponse<T> first
                        var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(responseContent, _jsonOptions);
                        if (apiResponse != null && apiResponse.Success)
                        {
                            return (true, apiResponse.Data, string.Empty);
                        }
                        else if (apiResponse != null)
                        {
                            return (false, default, apiResponse.Message);
                        }
                    }
                    catch (JsonException)
                    {
                        // If ApiResponse parsing fails, try to parse directly as T
                        try
                        {
                            var directResult = JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                            return (true, directResult, string.Empty);
                        }
                        catch (JsonException ex)
                        {
                            _logger.LogError(ex, "Failed to deserialize response as either ApiResponse<T> or T");
                            return (false, default, $"Failed to parse response: {ex.Message}");
                        }
                    }

                    return (false, default, "Failed to parse response");
                }
                else
                {
                    // Try to extract error message from response
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

        public IActionResult Index()
        {
            ViewData["Title"] = "Quản lý Lịch trình Tour";
            return View();
        }

        public async Task<IActionResult> Schedules(
            int? tourId = null,
            int page = 1,
            int pageSize = 3,
            string tourName = null,
            string location = null,
            string category = null,
            string status = null,
            string sort = null,
            string search = null,
            string fromDate = null,
            string toDate = null)
        {
            try
            {
                List<TourScheduleDTO> schedules;
                int totalCount;

                var queryParams = new List<string>
                {
                    $"page={page}",
                    $"pageSize={pageSize}"
                };

                if (tourId.HasValue)
                {
                    queryParams.Add($"tourId={tourId.Value}");
                    ViewData["Title"] = $"Lịch trình Tour #{tourId}";

                    var (tourSuccess, tourResult, tourError) = await CallApiAsync<TourResponse>($"TourSchedule/GetTourNameById?id={tourId}");
                    if (tourSuccess)
                    {
                        ViewBag.TourName = tourResult?.Name ?? $"Tour #{tourId}";
                        ViewData["Title"] = $"Lịch trình - {ViewBag.TourName}";
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to get tour name: {tourError}");
                    }
                }
                else
                {
                    ViewData["Title"] = "Danh sách Lịch trình Tour";
                }

                // Thêm các tham số filter
                if (!string.IsNullOrEmpty(tourName))
                    queryParams.Add($"tourName={Uri.EscapeDataString(tourName)}");
                if (!string.IsNullOrEmpty(location))
                    queryParams.Add($"location={Uri.EscapeDataString(location)}");
                if (!string.IsNullOrEmpty(category))
                    queryParams.Add($"category={Uri.EscapeDataString(category)}");
                if (!string.IsNullOrEmpty(status))
                    queryParams.Add($"status={Uri.EscapeDataString(status)}");
                if (!string.IsNullOrEmpty(sort))
                    queryParams.Add($"sort={Uri.EscapeDataString(sort)}");
                if (!string.IsNullOrEmpty(search))
                    queryParams.Add($"search={Uri.EscapeDataString(search)}");
                if (!string.IsNullOrEmpty(fromDate))
                    queryParams.Add($"fromDate={Uri.EscapeDataString(fromDate)}");
                if (!string.IsNullOrEmpty(toDate))
                    queryParams.Add($"toDate={Uri.EscapeDataString(toDate)}");

                // Gọi API filter với token
                var apiUrl = $"TourSchedule/GetFilteredTourSchedules?{string.Join("&", queryParams)}";
                var (success, result, error) = await CallApiAsync<ApiResponse<List<TourScheduleDTO>>>(apiUrl);

                if (success)
                {
                    schedules = result?.Data ?? new List<TourScheduleDTO>();

                    // Lấy tổng số count với filter
                    var countQueryParams = new List<string>();
                    if (tourId.HasValue)
                        countQueryParams.Add($"tourId={tourId.Value}");
                    if (!string.IsNullOrEmpty(tourName))
                        countQueryParams.Add($"tourName={Uri.EscapeDataString(tourName)}");
                    if (!string.IsNullOrEmpty(location))
                        countQueryParams.Add($"location={Uri.EscapeDataString(location)}");
                    if (!string.IsNullOrEmpty(category))
                        countQueryParams.Add($"category={Uri.EscapeDataString(category)}");
                    if (!string.IsNullOrEmpty(status))
                        countQueryParams.Add($"status={Uri.EscapeDataString(status)}");
                    if (!string.IsNullOrEmpty(search))
                        countQueryParams.Add($"search={Uri.EscapeDataString(search)}");
                    if (!string.IsNullOrEmpty(fromDate))
                        countQueryParams.Add($"fromDate={Uri.EscapeDataString(fromDate)}");
                    if (!string.IsNullOrEmpty(toDate))
                        countQueryParams.Add($"toDate={Uri.EscapeDataString(toDate)}");

                    var countApiUrl = "TourSchedule/GetFilteredTourScheduleCount";
                    if (countQueryParams.Any())
                        countApiUrl += "?" + string.Join("&", countQueryParams);

                    var (countSuccess, countResult, countError) = await CallApiAsync<ApiResponse<int>>(countApiUrl);
                    if (countSuccess)
                    {
                        totalCount = countResult?.Data ?? schedules.Count;
                    }
                    else
                    {
                        totalCount = schedules.Count;
                    }

                    // Set ViewBag values
                    ViewBag.TourId = tourId;
                    ViewBag.CurrentPage = page;
                    ViewBag.PageSize = pageSize;
                    ViewBag.TotalCount = totalCount;
                    ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                    // Store current filter values for view
                    ViewBag.CurrentTourName = tourName;
                    ViewBag.CurrentLocation = location;
                    ViewBag.CurrentCategory = category;
                    ViewBag.CurrentStatus = status;
                    ViewBag.CurrentSort = sort;
                    ViewBag.CurrentSearch = search;
                    ViewBag.CurrentFromDate = fromDate;
                    ViewBag.CurrentToDate = toDate;

                    return View(schedules);
                }
                else
                {
                    ViewBag.ErrorMessage = $"Không thể tải danh sách lịch trình: {error}";
                    return View(new List<TourScheduleDTO>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading schedules");
                ViewBag.ErrorMessage = $"Lỗi kết nối đến server: {ex.Message}";
                return View(new List<TourScheduleDTO>());
            }
        }

        public IActionResult Create(int? tourId = null)
        {
            ViewData["Title"] = "Thêm Lịch trình Mới";
            var model = new CreateTourScheduleRequest();
            if (tourId.HasValue)
            {
                model.TourId = tourId.Value;
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTourScheduleRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                if (model.DepartureDate >= model.ArrivalDate)
                {
                    ModelState.AddModelError("ArrivalDate", "Ngày kết thúc phải sau ngày khởi hành");
                    return View(model);
                }

                var json = JsonSerializer.Serialize(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var (success, result, error) = await CallApiAsync<ApiResponse<int>>("TourSchedule", HttpMethod.Post, content);

                if (success)
                {
                    TempData["SuccessMessage"] = "Add new tour schedule successfully!";
                    return RedirectToAction("Schedules");
                }
                else
                {
                    TempData["ErrorMessage"] = error ?? "Thêm lịch trình thất bại!";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tour schedule");
                TempData["ErrorMessage"] = $"Lỗi hệ thống: {ex.Message}";
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var (success, result, error) = await CallApiAsync<TourScheduleDTO>($"TourSchedule/{id}"); // Remove ApiResponse wrapper

                if (success && result != null)
                {
                    ViewData["Title"] = $"Chỉnh sửa Lịch trình - {result.TourName}";

                    var editModel = new UpdateTourScheduleRequest
                    {
                        DepartureDate = result.DepartureDate ?? DateOnly.FromDateTime(DateTime.Now),
                        ArrivalDate = result.ArrivalDate ?? DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                        ScheduleStatus = result.ScheduleStatus ?? ScheduleStatus.Scheduled
                    };

                    ViewBag.ScheduleId = result.Id;
                    ViewBag.TourName = result.TourName;
                    return View(editModel);
                }

                TempData["ErrorMessage"] = error ?? "Không tìm thấy lịch trình.";
                return RedirectToAction("Schedules");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tour schedule for edit ID: {ScheduleId}", id);
                TempData["ErrorMessage"] = $"Lỗi kết nối đến server: {ex.Message}";
                return RedirectToAction("Schedules");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateTourScheduleRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.ScheduleId = id;
                    return View(model);
                }

                if (model.DepartureDate >= model.ArrivalDate)
                {
                    ModelState.AddModelError("ArrivalDate", "Ngày kết thúc phải sau ngày khởi hành");
                    ViewBag.ScheduleId = id;
                    return View(model);
                }

                var json = JsonSerializer.Serialize(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Sử dụng CallApiAsync với token
                var (success, result, error) = await CallApiAsync<ApiResponse<bool>>($"TourSchedule/{id}", HttpMethod.Put, content);

                if (success)
                {
                    TempData["SuccessMessage"] = "Schedule update successful!";
                    return RedirectToAction("Schedules");
                }
                else
                {
                    TempData["ErrorMessage"] = error ?? "Schedule update failed!";
                    ViewBag.ScheduleId = id;
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tour schedule ID: {ScheduleId}", id);
                TempData["ErrorMessage"] = $"Lỗi hệ thống: {ex.Message}";
                ViewBag.ScheduleId = id;
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var (success, result, error) = await CallApiAsync<ApiResponse<bool>>($"TourSchedule/{id}", HttpMethod.Delete);

                if (success)
                {
                    TempData["SuccessMessage"] = "Xóa lịch trình thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = error ?? "Xóa lịch trình thất bại!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tour schedule ID: {ScheduleId}", id);
                TempData["ErrorMessage"] = $"Lỗi hệ thống: {ex.Message}";
            }

            return RedirectToAction("Schedules");
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var (success, result, error) = await CallApiAsync<TourScheduleDTO>($"TourSchedule/{id}"); 

                if (success && result != null)
                {
                    ViewData["Title"] = $"Chi tiết Lịch trình - {result.TourName}";
                    return View(result);
                }

                TempData["ErrorMessage"] = error ?? "Không tìm thấy lịch trình.";
                return RedirectToAction("Schedules");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tour schedule details ID: {ScheduleId}", id);
                TempData["ErrorMessage"] = $"Lỗi kết nối đến server: {ex.Message}";
                return RedirectToAction("Schedules");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetActiveTours()
        {
            try
            {
                var apiUrl = $"Tour/GetActiveTours";

                var (success, result, error) = await CallApiAsync<ApiResponse<List<Tour>>>(apiUrl);

                if (success)
                {
                    var tours = result?.Data ?? new List<Tour>();

                    var tourList = tours.Select(t => new
                    {
                        id = t.Id,
                        name = t.Name,
                        description = t.Description,
                        startLocationName = t.StartLocation?.LocationName,
                        endLocationName = t.EndLocation?.LocationName,
                        categoryName = t.Category?.CategoryName,
                        price = t.Price?.ToString("N0") + " VND",
                        duration = t.Duration + " day"
                    }).ToList();

                    return Json(tourList);
                }

                return Json(new List<object>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active tours");
                return Json(new List<object>());
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetTourById(int id)
        {
            try
            {
                var (success, result, error) = await CallApiAsync<ApiResponse<Tour>>($"Tour/GetTourById/{id}");

                if (success && result?.Data != null)
                {
                    return Json(new
                    {
                        id = result.Data.Id,
                        name = result.Data.Name
                    });
                }

                return Json(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tour by ID: {TourId}", id);
                return Json(null);
            }
        }
    }
}