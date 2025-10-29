using FE_Capstone_Project.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace FE_Capstone_Project.Controllers
{
    //[Authorize] // Nếu cần authentication
    public class StaffController : Controller
    {
        private readonly HttpClient _httpClient;
        private const string BASE_API_URL = "https://localhost:5160/api/";

        public StaffController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(BASE_API_URL);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Staff Dashboard";
            return View();
        }

        public IActionResult Tours(int page = 1, int pageSize = 10)
        {
            ViewData["Title"] = "Quản lý Tour";

            try
            {
                // Lấy danh sách tour phân trang - synchronous
                var response = _httpClient.GetAsync($"{BASE_API_URL}Tour/GetPaginatedTours?page={page}&pageSize={pageSize}").Result;

                if (response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsStringAsync().Result;
                    var result = JsonSerializer.Deserialize<TourListResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });


                    var tours = result?.Tours ?? new List<TourViewModel>();

                    // Lấy tổng số tour - synchronous
                    var countResponse = _httpClient.GetAsync($"{BASE_API_URL}Tour/GetTotalTourCount").Result;
                    var totalCount = 0;

                    if (countResponse.IsSuccessStatusCode)
                    {
                        var countContent = countResponse.Content.ReadAsStringAsync().Result;
                        var countResult = JsonSerializer.Deserialize<TourCountResponse>(countContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        totalCount = countResult?.TourCount ?? tours.Count;
                    }
                    else
                    {
                        totalCount = tours.Count;
                    }

                    // Tính toán thông tin phân trang
                    ViewBag.CurrentPage = page;
                    ViewBag.PageSize = pageSize;
                    ViewBag.TotalCount = totalCount;
                    ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                    return View(tours);
                }
                else
                {
                    ViewBag.ErrorMessage = "Không thể tải danh sách tour. Vui lòng thử lại sau.";
                    return View(new List<TourViewModel>());
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Lỗi kết nối đến server. Vui lòng kiểm tra lại kết nối.";
                return View(new List<TourViewModel>());
            }
        }

        public IActionResult TourDetails(int id)
        {
            try
            {
                var response = _httpClient.GetAsync($"{BASE_API_URL}Tour/GetTourById?id={id}").Result;

                if (response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsStringAsync().Result;
                    var result = JsonSerializer.Deserialize<TourDetailResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result?.Tour != null)
                    {
                        ViewData["Title"] = $"Chi tiết Tour - {result.Tour.Name}";
                        return View(result.Tour);
                    }
                }

                TempData["ErrorMessage"] = "Không tìm thấy tour.";
                return RedirectToAction("Tours");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi kết nối đến server.";
                return RedirectToAction("Tours");
            }
        }

        [HttpPost]
        public IActionResult DeleteTour(int id)
        {
            try
            {
                var response = _httpClient.DeleteAsync($"{BASE_API_URL}Tour/DeleteTour?tourId={id}").Result;

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Xóa tour thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Xóa tour thất bại!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi kết nối đến server!";
            }

            return RedirectToAction("Tours");
        }


        public IActionResult Dashboard()
        {
            ViewData["Title"] = "Dashboard";
            return View();
        }

        public IActionResult Bookings()
        {
            ViewData["Title"] = "Quản lý Đặt tour";
            return View();
        }

        public IActionResult Customers()
        {
            ViewData["Title"] = "Quản lý Khách hàng";
            return View();
        }

        public IActionResult Schedules()
        {
            ViewData["Title"] = "Lịch trình Tour";
            return View();
        }

        public IActionResult Tasks()
        {
            ViewData["Title"] = "Công việc của tôi";
            return View();
        }

        public IActionResult Profile()
        {
            ViewData["Title"] = "Hồ sơ cá nhân";
            return View();
        }
    }
}
