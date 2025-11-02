using BE_Capstone_Project.Application.ReviewManagement.DTOs;
using FE_Capstone_Project.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;

namespace FE_Capstone_Project.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://localhost:7160/api/review";
        public HomeController(ILogger<HomeController> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public  async Task<IActionResult> IndexAsync()
        {
            var firstName = HttpContext.Session.GetString("FirstName");
            ViewBag.FirstName = firstName;
            List<ReviewViewModel> reviews = new();

            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/get-all");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    reviews = JsonSerializer.Deserialize<List<ReviewViewModel>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    reviews = reviews?.OrderByDescending(r => r.CreatedDate)
                                     
                                     .ToList() ?? new();
                }
                else
                {
                    ViewBag.Error = "Không thể tải bình luận.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Lỗi khi gọi API review: {ex.Message}";
            }

            // 🔹 Truyền thêm danh sách review sang View (qua ViewBag hoặc ViewModel)
            ViewBag.Reviews = reviews;

            return View(reviews);

        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
