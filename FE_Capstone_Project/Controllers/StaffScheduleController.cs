using FE_Capstone_Project.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace FE_Capstone_Project.Controllers
{
    public class StaffScheduleController : Controller
    {
        private readonly HttpClient _httpClient;
        private const string BASE_API_URL = "https://localhost:7160/api/";

        public StaffScheduleController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(BASE_API_URL);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Quản lý Lịch trình Tour";
            return View();
        }

        // GET: Hiển thị danh sách lịch trình
        public async Task<IActionResult> Schedules(int page = 1, int pageSize = 10)
        {
            ViewData["Title"] = "Danh sách Lịch trình Tour";

            try
            {
                var response = await _httpClient.GetAsync($"TourSchedule/GetPaginatedTourSchedules?page={page}&pageSize={pageSize}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<List<TourScheduleDTO>>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    var schedules = result?.Data ?? new List<TourScheduleDTO>();

                    // Lấy tổng số lịch trình (có thể cần API riêng)
                    ViewBag.CurrentPage = page;
                    ViewBag.PageSize = pageSize;
                    ViewBag.TotalCount = schedules.Count;
                    ViewBag.TotalPages = (int)Math.Ceiling(schedules.Count / (double)pageSize);

                    return View(schedules);
                }
                else
                {
                    ViewBag.ErrorMessage = "Không thể tải danh sách lịch trình.";
                    return View(new List<TourScheduleDTO>());
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Lỗi kết nối đến server.";
                return View(new List<TourScheduleDTO>());
            }
        }

        // GET: Hiển thị lịch trình theo tour
        public async Task<IActionResult> SchedulesByTour(int tourId, int page = 1, int pageSize = 5)
        {
            ViewData["Title"] = "Lịch trình Tour";

            try
            {
                var response = await _httpClient.GetAsync($"TourSchedule/GetPaginatedTourSchedules/{tourId}?page={page}&pageSize={pageSize}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<List<TourScheduleDTO>>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    var schedules = result?.Data ?? new List<TourScheduleDTO>();
                    ViewBag.TourId = tourId;
                    ViewBag.CurrentPage = page;
                    ViewBag.PageSize = pageSize;

                    return View("TourSchedules", schedules);
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể tải lịch trình tour.";
                    return RedirectToAction("Schedules");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi kết nối đến server.";
                return RedirectToAction("Schedules");
            }
        }

        // GET: Hiển thị form tạo lịch trình
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

        // POST: Tạo lịch trình mới
        [HttpPost]
        public async Task<IActionResult> Create(CreateTourScheduleRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var json = JsonSerializer.Serialize(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("TourSchedule", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Thêm lịch trình thành công!";
                    return RedirectToAction("Schedules");
                }
                else
                {
                    var errorResult = JsonSerializer.Deserialize<ApiResponse<int>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    TempData["ErrorMessage"] = errorResult?.Message ?? "Thêm lịch trình thất bại!";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi hệ thống: {ex.Message}";
                return View(model);
            }
        }

        // GET: Hiển thị form chỉnh sửa
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"TourSchedule/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<TourScheduleDTO>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result?.Data != null)
                    {
                        ViewData["Title"] = $"Chỉnh sửa Lịch trình - {result.Data.TourName}";

                        var editModel = new UpdateTourScheduleRequest
                        {
                            DepartureDate = result.Data.DepartureDate ?? DateOnly.FromDateTime(DateTime.Now),
                            ArrivalDate = result.Data.ArrivalDate ?? DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                            ScheduleStatus = result.Data.ScheduleStatus ?? ScheduleStatus.Scheduled
                        };

                        ViewBag.ScheduleId = result.Data.Id;
                        return View(editModel);
                    }
                }

                TempData["ErrorMessage"] = "Không tìm thấy lịch trình.";
                return RedirectToAction("Schedules");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi kết nối đến server.";
                return RedirectToAction("Schedules");
            }
        }

        // POST: Cập nhật lịch trình
        [HttpPost]
        public async Task<IActionResult> Edit(int id, UpdateTourScheduleRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.ScheduleId = id;
                    return View(model);
                }

                var json = JsonSerializer.Serialize(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"TourSchedule/{id}", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Cập nhật lịch trình thành công!";
                    return RedirectToAction("Schedules");
                }
                else
                {
                    var errorResult = JsonSerializer.Deserialize<ApiResponse<bool>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    TempData["ErrorMessage"] = errorResult?.Message ?? "Cập nhật lịch trình thất bại!";
                    ViewBag.ScheduleId = id;
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi hệ thống: {ex.Message}";
                ViewBag.ScheduleId = id;
                return View(model);
            }
        }

        // POST: Xóa lịch trình
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"TourSchedule/{id}");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Xóa lịch trình thành công!";
                }
                else
                {
                    var errorResult = JsonSerializer.Deserialize<ApiResponse<bool>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    TempData["ErrorMessage"] = errorResult?.Message ?? "Xóa lịch trình thất bại!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi hệ thống: {ex.Message}";
            }

            return RedirectToAction("Schedules");
        }

        // GET: Chi tiết lịch trình
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"TourSchedule/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<TourScheduleDTO>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result?.Data != null)
                    {
                        ViewData["Title"] = $"Chi tiết Lịch trình - {result.Data.TourName}";
                        return View(result.Data);
                    }
                }

                TempData["ErrorMessage"] = "Không tìm thấy lịch trình.";
                return RedirectToAction("Schedules");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi kết nối đến server.";
                return RedirectToAction("Schedules");
            }
        }
    }
}