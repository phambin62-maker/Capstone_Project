using FE_Capstone_Project.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
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
            // Nếu đã đăng nhập (session có username) thì quay lại Home
            var username = HttpContext.Session.GetString("UserName");
            if (!string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Index", "Home");
            }

            // Nếu chưa đăng nhập -> hiện trang login
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
                    HttpContext.Session.SetString("UserName", loginResult.FirstName ?? "");
                    HttpContext.Session.SetString("JwtToken", loginResult.Token ?? "");
                }

                // Chuyển đến trang Home
                return RedirectToAction("Index", "Home");
            }

            ViewData["HideHeader"] = true;
            model.Message = "Sai thông tin đăng nhập hoặc tài khoản không tồn tại.";
            return View(model);
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
            var claims = result.Principal.Identities.FirstOrDefault().Claims.ToList();

            var email = claims.FirstOrDefault(c => c.Type.Contains("email"))?.Value;
            var name = claims.FirstOrDefault(c => c.Type.Contains("givenname"))?.Value
                       ?? claims.FirstOrDefault(c => c.Type.Contains("name"))?.Value;

            // Lưu session
            HttpContext.Session.SetString("UserName", name ?? "");
            HttpContext.Session.SetString("UserEmail", email ?? "");

            // 🔹 Tạo claims cho cookie đăng nhập nội bộ của ứng dụng
            var appClaims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, name ?? ""),
        new Claim(ClaimTypes.Email, email ?? "")
    };

            var appIdentity = new ClaimsIdentity(appClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            var appPrincipal = new ClaimsPrincipal(appIdentity);

            // 🔹 Ghi cookie đăng nhập
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, appPrincipal);

            // 🔹 Chuyển hướng về trang chính hoặc profile
            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public IActionResult CheckLogin()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return Json(new
                {
                    IsAuthenticated = true,
                    Name = User.Identity.Name,
                    Email = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value
                });
            }

            return Json(new { IsAuthenticated = false });
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
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var username= HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(token)&string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login");
            }
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
    }

    
    public class LoginResponse
        {
            public string Token { get; set; }
            public string FirstName { get; set; }
        }
    }

