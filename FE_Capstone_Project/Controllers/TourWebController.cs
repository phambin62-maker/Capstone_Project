using DocumentFormat.OpenXml.Wordprocessing;
using FE_Capstone_Project.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace FE_Capstone_Project.Controllers
{
    public class TourWebController : Controller
    {
        private readonly HttpClient _httpClient;
        private const string BASE_API_URL = "https://localhost:5160/api/";

        public TourWebController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(BASE_API_URL);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public IActionResult Tours()
        {
            try
            {
                var response = _httpClient.GetAsync($"{BASE_API_URL}Tour/GetAllTours").Result;

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
                    ViewBag.TotalCount = totalCount;

                    return View(tours);
                }
                else
                {
                    ViewBag.ErrorMessage = "Tour list could not be loaded. Please try again later.";
                    return View(new List<TourViewModel>());
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error connecting to server. Please check your connection again.";
                return View(new List<TourViewModel>());
            }
        }

        public IActionResult TourDetails(int tourId)
        {
            try
            {
                var response = _httpClient.GetAsync($"{BASE_API_URL}Tour/GetTourById/{tourId}").Result;

                if (response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsStringAsync().Result;
                    var result = JsonSerializer.Deserialize<TourDetailResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    var tour = result.Tour ?? new TourViewModel();

                    return View(tour);
                }
                else
                {
                    ViewBag.ErrorMessage = "Tour list could not be loaded. Please try again later.";
                    return View(new TourViewModel());
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error connecting to server. Please check your connection again.";
                return View(new TourViewModel());
            }
        }
    }
}
