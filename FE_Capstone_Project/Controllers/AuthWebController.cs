using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using FE_Capstone_Project.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace FE_Capstone_Project.Controllers
{
    public class AuthWebController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://localhost:7160/api/auth"; // backend API URL

        public AuthWebController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpGet]
        public IActionResult Login()
        {
            var username = HttpContext.Session.GetString("UserName");
            if (!string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Index", "Home");
            }

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

            if (!response.IsSuccessStatusCode)
            {
                ViewData["HideHeader"] = true;
                model.Message = "Sai thông tin đăng nhập hoặc tài khoản không tồn tại.";
                return View(model);
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<LoginResponse>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (loginResult == null || string.IsNullOrEmpty(loginResult.Token))
            {
                model.Message = "Không nhận được token từ server.";
                return View(model);
            }


            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(loginResult.Token);
            var payload = jwtToken.Payload;

            var firstName = payload.ContainsKey("unique_name") ? payload["unique_name"].ToString() : "";
            var email = payload.ContainsKey("email") ? payload["email"].ToString() : "";

            HttpContext.Session.SetString("JwtToken", loginResult.Token);
            HttpContext.Session.SetString("UserName", firstName);
            HttpContext.Session.SetString("UserEmail", email);

            return RedirectToAction("Index", "Home");
            ;
        }

        [HttpGet]
        public IActionResult GoogleLogin()
        {
            var redirectUrl = Url.Action("GoogleResponse", "AuthWeb");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var claims = result.Principal.Identities.FirstOrDefault()?.Claims.ToList();

            var email = claims?.FirstOrDefault(c => c.Type.Contains("email"))?.Value;
            var name = claims?.FirstOrDefault(c => c.Type.Contains("givenname"))?.Value
                       ?? claims?.FirstOrDefault(c => c.Type.Contains("name"))?.Value;

            HttpContext.Session.SetString("UserName", name ?? "");
            HttpContext.Session.SetString("UserEmail", email ?? "");
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Register()
        {
            return View();
        }
        public IActionResult CheckSession()
        {
            var username = HttpContext.Session.GetString("UserName");
            var token = HttpContext.Session.GetString("JwtToken");
            var name = HttpContext.Session.GetString("UserName");
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
            {
                return Content("Session chưa được lưu hoặc đã hết hạn.");
            }

            return Content($"Session hợp lệ! UserName: {name}, Token: {email.Substring(0, 15)}...");
        }
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var username = HttpContext.Session.GetString("UserName");

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync($"{_baseUrl}/profile");
            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Không thể lấy thông tin người dùng.";
                return View();
            }

            var json = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<UserProfileViewModel>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return View(user);
        }

        private static Dictionary<string, object> DecodeJwtPayload(string token)
        {
            var parts = token.Split('.');
            if (parts.Length != 3)
                return new Dictionary<string, object>();

            var payload = parts[1];
            payload = payload.Replace('-', '+').Replace('_', '/');
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            var bytes = Convert.FromBase64String(payload);
            var json = Encoding.UTF8.GetString(bytes);

            return JsonSerializer.Deserialize<Dictionary<string, object>>(json);
        }
    }
    public class LoginResponse
    {
        public string Token { get; set; }
    }
}
