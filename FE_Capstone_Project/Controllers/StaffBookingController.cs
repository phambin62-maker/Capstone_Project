using BE_Capstone_Project.Domain.Enums;
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
using BookingListResponse = FE_Capstone_Project.Models.BookingListResponse;
using StaffBookingDTO = FE_Capstone_Project.Models.StaffBookingDTO;
using UpdateBookingStatusRequest = FE_Capstone_Project.Models.UpdateBookingStatusRequest;
using UpdatePaymentStatusRequest = FE_Capstone_Project.Models.UpdatePaymentStatusRequest;

namespace FE_Capstone_Project.Controllers
{
    [AuthorizeRole(2)] 
    public class StaffBookingController : Controller
    {
        private readonly HttpClient _httpClient;
        private const string BASE_API_URL = "https://localhost:7160/api/";
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly DataService _dataService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<StaffBookingController> _logger;

        public StaffBookingController(
            IHttpClientFactory httpClientFactory,
            ILogger<StaffBookingController> logger,
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

                // Add Token to Request
                var token = GetToken();
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning($"[StaffBookingController] No token found for request to {endpoint}");
                    return (false, default, "Token not found. Please log in again.");
                }
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"API Call: {endpoint}, Status: {response.StatusCode}, Response: {responseContent}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning($"Unauthorized access to {endpoint}");
                    return (false, default, "Login session has expired. Please log in again.");
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning($"Forbidden access to {endpoint}. User lacks required permissions.");
                    return (false, default, "You don't have permission to perform this action. Please contact administrator.");
                }

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        // Try to parse directly as T first
                        var directResult = JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                        return (true, directResult, string.Empty);
                    }
                    catch (JsonException)
                    {
                        // If direct parsing fails, try to parse as ApiResponse<T>
                        try
                        {
                            var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(responseContent, _jsonOptions);
                            if (apiResponse != null && apiResponse.Success)
                            {
                                return (true, apiResponse.Data, string.Empty);
                            }
                            return (false, default, apiResponse?.Message ?? "API response indicated failure");
                        }
                        catch (JsonException ex)
                        {
                            _logger.LogError(ex, "Failed to deserialize response as either T or ApiResponse<T>");
                            return (false, default, $"Failed to parse response: {ex.Message}");
                        }
                    }
                }
                else
                {
                    // Try to parse error message from API
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, _jsonOptions);
                        return (false, default, errorResponse?.Message ?? $"API Error: {response.StatusCode}");
                    }
                    catch
                    {
                        return (false, default, $"API Error: {response.StatusCode} - {responseContent}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API call exception: {Endpoint}", endpoint);
                return (false, default, $"Exception: {ex.Message}");
            }
        }


        public async Task<IActionResult> Bookings(
            string searchTerm = null,
            BookingStatus? bookingStatus = null,
            PaymentStatus? paymentStatus = null,
            int page = 1,
            int pageSize = 10)
        {
            try
            {
                // Build query parameters
                var queryParams = new List<string>
                {
                    $"page={page}",
                    $"pageSize={pageSize}"
                };

                if (!string.IsNullOrEmpty(searchTerm))
                    queryParams.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");
                if (bookingStatus.HasValue)
                    queryParams.Add($"bookingStatus={bookingStatus.Value}");
                if (paymentStatus.HasValue)
                    queryParams.Add($"paymentStatus={paymentStatus.Value}");

                // Gọi API để lấy danh sách booking
                var apiUrl = $"Booking/staff/bookings?{string.Join("&", queryParams)}";
                _logger.LogInformation($"Calling API: {apiUrl}");

                var (success, result, error) = await CallApiAsync<BookingListResponse>(apiUrl);

                if (success && result != null)
                {
                    var bookings = result.Bookings ?? new List<BookingDto>();
                    var totalCount = result.TotalCount;

                    // Lấy danh sách trạng thái cho dropdown
                    await LoadStatusDropdowns();

                    // Set ViewBag values
                    ViewBag.CurrentPage = page;
                    ViewBag.PageSize = pageSize;
                    ViewBag.TotalCount = totalCount;
                    ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                    // Store current filter values for view
                    ViewBag.CurrentSearchTerm = searchTerm;
                    ViewBag.CurrentBookingStatus = bookingStatus;
                    ViewBag.CurrentPaymentStatus = paymentStatus;

                    return View(bookings);
                }
                else
                {
                    _logger.LogWarning($"Failed to load bookings: {error}");
                    ViewBag.ErrorMessage = $"Không thể tải danh sách booking: {error}";
                    await LoadStatusDropdowns();
                    return View(new List<BookingDto>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading bookings");
                ViewBag.ErrorMessage = $"Lỗi kết nối đến server: {ex.Message}";
                await LoadStatusDropdowns();
                return View(new List<BookingDto>());
            }
        }

        // Action để hiển thị view danh sách booking (chỉ trả về view)
        [HttpGet]
        public IActionResult BookingsView()
        {
            return View("Bookings");
        }

        // API endpoint để lấy dữ liệu booking (cho AJAX calls)
        [HttpGet]
        public async Task<IActionResult> GetBookingsData(
            string searchTerm = null,
            BookingStatus? bookingStatus = null,
            PaymentStatus? paymentStatus = null,
            int page = 1,
            int pageSize = 10)
        {
            try
            {
                // Build query parameters
                var queryParams = new List<string>
                {
                    $"page={page}",
                    $"pageSize={pageSize}"
                };

                if (!string.IsNullOrEmpty(searchTerm))
                    queryParams.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");
                if (bookingStatus.HasValue)
                    queryParams.Add($"bookingStatus={bookingStatus.Value}");
                if (paymentStatus.HasValue)
                    queryParams.Add($"paymentStatus={paymentStatus.Value}");

                var apiUrl = $"Booking/staff/bookings?{string.Join("&", queryParams)}";
                var (success, result, error) = await CallApiAsync<BookingListResponse>(apiUrl);

                if (success && result != null)
                {
                    return Json(new
                    {
                        success = true,
                        data = result.Bookings,
                        totalCount = result.TotalCount,
                        page = result.Page,
                        pageSize = result.PageSize,
                        totalPages = result.TotalPages
                    });
                }
                else
                {
                    return Json(new { success = false, error = error ?? "Failed to load bookings" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetBookingsData");
                return Json(new { success = false, error = ex.Message });
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                // Gọi API để lấy chi tiết booking
                var (success, result, error) = await CallApiAsync<StaffBookingDTO>($"Booking/staff/bookings/{id}");

                if (success && result != null)
                {
                    ViewData["Title"] = $"Chi tiết Booking #{result.Id}";

                    // Lấy danh sách trạng thái cho dropdown
                    await LoadStatusDropdowns();

                    return View(result);
                }

                TempData["ErrorMessage"] = error ?? "Không tìm thấy booking.";
                return RedirectToAction("Bookings");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading booking details ID: {BookingId}", id);
                TempData["ErrorMessage"] = $"Lỗi kết nối đến server: {ex.Message}";
                return RedirectToAction("Bookings");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBookingStatus(int id, UpdateBookingStatusRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Dữ liệu không hợp lệ.";
                    return RedirectToAction("Details", new { id });
                }

                var json = JsonSerializer.Serialize(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var (success, result, error) = await CallApiAsync<object>($"Booking/staff/bookings/{id}/status", HttpMethod.Put, content);

                if (success)
                {
                    TempData["SuccessMessage"] = "Cập nhật trạng thái booking thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = error ?? "Cập nhật trạng thái booking thất bại!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking status ID: {BookingId}", id);
                TempData["ErrorMessage"] = $"Lỗi hệ thống: {ex.Message}";
            }

            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePaymentStatus(int id, UpdatePaymentStatusRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Dữ liệu không hợp lệ.";
                    return RedirectToAction("Details", new { id });
                }

                var json = JsonSerializer.Serialize(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var (success, result, error) = await CallApiAsync<object>($"Booking/staff/bookings/{id}/payment-status", HttpMethod.Put, content);

                if (success)
                {
                    TempData["SuccessMessage"] = "Cập nhật trạng thái thanh toán thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = error ?? "Cập nhật trạng thái thanh toán thất bại!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment status ID: {BookingId}", id);
                TempData["ErrorMessage"] = $"Lỗi hệ thống: {ex.Message}";
            }

            return RedirectToAction("Details", new { id });
        }
       

        private async Task LoadStatusDropdowns()
        {
            try
            {
                var (bookingSuccess, bookingResult, bookingError) = await CallApiAsync<List<BookingStatus>>("Booking/staff/booking-statuses");
                if (bookingSuccess)
                {
                    ViewBag.BookingStatuses = bookingResult ?? new List<BookingStatus>();
                }
                else
                {
                    ViewBag.BookingStatuses = Enum.GetValues(typeof(BookingStatus)).Cast<BookingStatus>().ToList();
                }

                // Lấy danh sách payment status
                var (paymentSuccess, paymentResult, paymentError) = await CallApiAsync<List<PaymentStatus>>("Booking/staff/payment-statuses");
                if (paymentSuccess)
                {
                    ViewBag.PaymentStatuses = paymentResult ?? new List<PaymentStatus>();
                }
                else
                {
                    ViewBag.PaymentStatuses = Enum.GetValues(typeof(PaymentStatus)).Cast<PaymentStatus>().ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading status dropdowns");
                ViewBag.BookingStatuses = Enum.GetValues(typeof(BookingStatus)).Cast<BookingStatus>().ToList();
                ViewBag.PaymentStatuses = Enum.GetValues(typeof(PaymentStatus)).Cast<PaymentStatus>().ToList();
            }
        }

        
    }

    // Thêm class ApiResponse để deserialize response từ API
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T Data { get; set; }
    }
}