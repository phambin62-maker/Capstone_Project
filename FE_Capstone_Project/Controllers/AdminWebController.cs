using FE_Capstone_Project.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FE_Capstone_Project.Controllers
{
    public class AdminWebController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://localhost:7160/api/User"; // backend API URL
        private readonly ILogger<AuthWebController> _logger;
        public AdminWebController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();

        }
        public IActionResult Account()
        {
            return View();
        }
        public IActionResult Report()
        {
            return View();
        }
    }
}