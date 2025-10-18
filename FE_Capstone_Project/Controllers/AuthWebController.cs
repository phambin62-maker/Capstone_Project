using FE_Capstone_Project.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace FE_Capstone_Project.Controllers
{
    public class AuthWebController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "http://localhost:5160/api/auth"; // backend API URL

        public AuthWebController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpGet]
        public IActionResult Login()
        {
            ViewData["HideHeader"] = true;
            return View(new AuthViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(AuthViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Chuẩn bị dữ liệu gửi đi
            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Gọi API backend
            var response = await _httpClient.PostAsync($"{_baseUrl}/login", content);

            if (response.IsSuccessStatusCode)
            {
                // Đọc nội dung JSON trả về 1 lần duy nhất
                var responseString = await response.Content.ReadAsStringAsync();

                // Giải mã JSON (ví dụ backend trả về {"token": "...", "username": "..."} )
                var loginResult = JsonSerializer.Deserialize<LoginResponse>(responseString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Lưu session
                if (loginResult != null)
                {
                    HttpContext.Session.SetString("FirstName", loginResult.FirstName ?? "");
                    HttpContext.Session.SetString("JwtToken", loginResult.Token ?? "");
                }

                // Chuyển đến trang Home
                return RedirectToAction("Index", "Home");
            }

            ViewData["HideHeader"] = true;
            model.Message = "Sai thông tin đăng nhập hoặc tài khoản không tồn tại.";
            return View(model);
        }
        public async Task<IActionResult> Logout()
        {
            // Xóa session
            HttpContext.Session.Remove("FirstName");
            HttpContext.Session.Remove("JwtToken");
            // Chuyển đến trang Login
            return RedirectToAction("Index", "Home");
        }
        public async Task<IActionResult> Register()
        {
            return View();
        }
        public async Task<IActionResult> Profile()
        {
            return View();
        }
        public class LoginResponse
        {
            public string Token { get; set; }
            public string FirstName { get; set; }
        }
    }
}
