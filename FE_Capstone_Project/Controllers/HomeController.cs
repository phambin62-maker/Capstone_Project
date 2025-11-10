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

        public async Task<IActionResult> Index()
        {
            var model = new HomePageViewModel();
            var firstName = HttpContext.Session.GetString("FirstName");
            ViewBag.FirstName = firstName;

            try
            {
                // --- Gọi API review ---
                var reviewResponse = await _httpClient.GetAsync($"{_baseUrl}/get-all");
                if (reviewResponse.IsSuccessStatusCode)
                {
                    var json = await reviewResponse.Content.ReadAsStringAsync();
                    model.Reviews = JsonSerializer.Deserialize<List<ReviewViewModel>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new();
                }

                // --- Gọi API rating ---
                var ratingResponse = await _httpClient.GetAsync($"{_baseUrl}/tour-ratings");
                if (ratingResponse.IsSuccessStatusCode)
                {
                    var json = await ratingResponse.Content.ReadAsStringAsync();
                    model.TourRatings = JsonSerializer.Deserialize<List<TourRatingViewModel>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error fetching data: {ex.Message}";
            }

            return View(model);
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

        public IActionResult Forbidden()
        {
            ViewData["Title"] = "Không có quyền truy cập";
            return View();
        }
    }
}
