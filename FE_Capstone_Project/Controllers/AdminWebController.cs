using FE_Capstone_Project.Helpers;
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
        
        private readonly string _baseUrl = "https://localhost:7160/api/admin"; // backend API URL
        //private readonly ILogger<AuthWebController> _logger;
        private readonly ApiHelper _apiHelper;
        public AdminWebController(IHttpClientFactory httpClientFactory, ApiHelper apiHelper)
        {
            
            _apiHelper = apiHelper;

        }
        [HttpGet]
        public async Task<IActionResult> Account()
        {
            var endpoint = $"{_baseUrl}/get-all-accounts";
            var users = await _apiHelper.GetAsync<List<AccountViewModel>>(endpoint);

            if (users == null)
            {
                ViewBag.Error = "Không thể tải danh sách tài khoản!";
                return View(new List<AccountViewModel>());
            }

            return View(users);
        }
        public IActionResult Report()
        {
            return View();
        }
    }
}