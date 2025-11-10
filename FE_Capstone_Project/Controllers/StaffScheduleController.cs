using BE_Capstone_Project.Domain.Models;
using FE_Capstone_Project.Filters;
using FE_Capstone_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace FE_Capstone_Project.Controllers
{
    [AuthorizeRole(2)]
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

                // Build API URL với các tham số filter
                var queryParams = new List<string>
                {
                    $"page={page}",
                    $"pageSize={pageSize}"
                };

                if (tourId.HasValue)
                {
                    queryParams.Add($"tourId={tourId.Value}");
                    ViewData["Title"] = $"Lịch trình Tour #{tourId}";

                    var tourResponse = await _httpClient.GetAsync($"TourSchedule/GetTourNameById?id={tourId}");
                    if (tourResponse.IsSuccessStatusCode)
                    {
                        var tourContent = await tourResponse.Content.ReadAsStringAsync();
                        var tourResult = JsonSerializer.Deserialize<TourResponse>(tourContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        ViewBag.TourName = tourResult?.Name ?? $"Tour #{tourId}";
                        ViewData["Title"] = $"Lịch trình - {ViewBag.TourName}";
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

                // Gọi API filter
                var apiUrl = $"TourSchedule/GetFilteredTourSchedules?{string.Join("&", queryParams)}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<List<TourScheduleDTO>>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

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

                    var countResponse = await _httpClient.GetAsync(countApiUrl);
                    if (countResponse.IsSuccessStatusCode)
                    {
                        var countContent = await countResponse.Content.ReadAsStringAsync();
                        var countResult = JsonSerializer.Deserialize<ApiResponse<int>>(countContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
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
                    ViewBag.ErrorMessage = "Không thể tải danh sách lịch trình.";
                    return View(new List<TourScheduleDTO>());
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Lỗi kết nối đến server: {ex.Message}";
                return View(new List<TourScheduleDTO>());
            }
        }

        // Các method khác giữ nguyên...
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
                        ViewBag.TourName = result.Data.TourName;
                        return View(editModel);
                    }
                }

                TempData["ErrorMessage"] = "Không tìm thấy lịch trình.";
                return RedirectToAction("Schedules");
            }
            catch (Exception ex)
            {
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

        [HttpPost]
        [ValidateAntiForgeryToken]
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
                TempData["ErrorMessage"] = $"Lỗi kết nối đến server: {ex.Message}";
                return RedirectToAction("Schedules");
            }
        }
        
        [HttpGet]
        public async Task<IActionResult> GetActiveTours(string search = "")
        {
            try
            {
                var apiUrl = $"Tour/GetActiveTours?search={Uri.EscapeDataString(search)}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<List<Tour>>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    var tours = result?.Data ?? new List<Tour>();

                    // Transform to simple DTO for selection
                    var tourList = tours.Select(t => new
                    {
                        id = t.Id,
                        name = t.Name,
                        description = t.Description,
                        startLocationName = t.StartLocation?.LocationName,
                        endLocationName = t.EndLocation?.LocationName,
                        categoryName = t.Category?.CategoryName,
                        price = t.Price?.ToString("N0") + " VND",
                        duration = t.Duration + " ngày"
                    }).ToList();

                    return Json(tourList);
                }

                return Json(new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new List<object>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTourById(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Tour/GetTourById/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<Tour>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result?.Data != null)
                    {
                        return Json(new
                        {
                            id = result.Data.Id,
                            name = result.Data.Name
                        });
                    }
                }

                return Json(null);
            }
            catch (Exception ex)
            {
                return Json(null);
            }
        }
    }
}