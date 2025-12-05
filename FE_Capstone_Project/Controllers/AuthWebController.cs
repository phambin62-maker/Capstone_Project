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
        private readonly string _baseUrl = "https://localhost:7160/api/Auth"; // backend API URL
        private readonly ILogger<AuthWebController> _logger;

        public AuthWebController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<AuthWebController>();

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
            ViewData["HideHeader"] = true;
            if (!ModelState.IsValid)
                return View(model);
            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/login", content);

            if (!response.IsSuccessStatusCode)
            {
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
            var userId = int.Parse(payload.ContainsKey("UserId") ? payload["UserId"].ToString() : "0");
            HttpContext.Session.SetInt32("UserId", userId);
            HttpContext.Session.SetString("JwtToken", loginResult.Token);
            HttpContext.Session.SetString("UserName", firstName);
            HttpContext.Session.SetString("UserEmail", email);
            HttpContext.Session.SetInt32("UserRoleId", loginResult.RoleId);
            
            switch (loginResult.RoleId)
            {
                case 3:
                    return RedirectToAction("Index", "Home");
                case 2:
                    return RedirectToAction("Index", "Staff");
                case 1:
                    return RedirectToAction("Account", "AdminWeb");
                default:
                    return RedirectToAction("Index", "Home");
            }
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

            if (email == null)
            {
                _logger.LogWarning("Google login không có email, chuyển hướng về trang Login");
                return RedirectToAction("Login", "AuthWeb");
            }

            HttpContext.Session.SetString("UserName", name ?? "");
            HttpContext.Session.SetString("UserEmail", email);

            // Gửi yêu cầu đến BE
            var userPayload = new
            {
                Email = email,
                FullName = name,
                Provider = "Google"
            };

            var requestUrl = $"{_baseUrl}/google-sync";
            var response = await _httpClient.PostAsJsonAsync(requestUrl, userPayload);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                var token = root.TryGetProperty("token", out var tokenElement) ? tokenElement.GetString() : null;
                var message = root.TryGetProperty("message", out var msgElement) ? msgElement.GetString() : "Đăng nhập thành công";

                // === SỬA 2: ĐỌC USERID TỪ BE RESPONSE ===
                var userId = root.TryGetProperty("userId", out var userIdElement) ? userIdElement.GetInt32() : 0;

                if (!string.IsNullOrEmpty(token))
                {
                    HttpContext.Session.SetString("JwtToken", token);

                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(token);
                    var payload = jwtToken.Payload;

                    var username = payload.ContainsKey("unique_name") ? payload["unique_name"].ToString()
                                   : payload.ContainsKey("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")
                                     ? payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"].ToString()
                                     : name ?? email.Split('@')[0];

                    var roleId = payload.ContainsKey("RoleId") ? int.Parse(payload["RoleId"].ToString() ?? "3") : 3;

                    HttpContext.Session.SetString("UserName", username ?? name ?? "");
                    HttpContext.Session.SetString("UserEmail", email);
                    HttpContext.Session.SetInt32("UserRoleId", roleId);
                    HttpContext.Session.SetInt32("UserId", userId);


                    _logger.LogInformation($"Lưu token vào session thành công cho {email}, username: {username}");
                }
                else
                {
                    _logger.LogWarning($"BE không trả về token cho {email}. Message: {message}");
                }
                return RedirectToAction("Index", "Home");
            }
            else
            {
                var err = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Không thể đồng bộ user Google: {err}");
                return RedirectToAction("Login", "AuthWeb");
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            ViewData["HideHeader"] = true;
            return View(new RegisterViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            ViewData["HideHeader"] = true;
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
                return View(model);
            }

            try
            {
                var apiUrl = "https://localhost:7160/api/auth/register";
                var json = JsonSerializer.Serialize(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "🎉 Đăng ký thành công! Vui lòng đăng nhập.";
                    return RedirectToAction("Login");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ViewBag.Error = "Trùng Email hoặc User Name ";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "⚠️ Không thể kết nối đến máy chủ: " + ex.Message;
                return View(model);
            }
        }

        public IActionResult CheckSession()
        {
            var username = HttpContext.Session.GetString("UserName");
            var token = HttpContext.Session.GetString("JwtToken");
            var name = HttpContext.Session.GetString("UserName");
            var email = HttpContext.Session.GetString("UserEmail");

            // === THÊM KIỂM TRA USERID ===
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(userId))
            {
                return Content("Session chưa được lưu hoặc đã hết hạn. (Thiếu UserId, UserName hoặc Email)");
            }

            return Content($"Session hợp lệ! UserName: {name}, UserId: {userId}, Token: {token?.Substring(0, 15)}...");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var email = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(token))
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

        [HttpPost]
        public async Task<IActionResult> Profile(UserProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Dữ liệu không hợp lệ.";
                return View(model);
            }

            try
            {
                var token = HttpContext.Session.GetString("JwtToken");
                if (string.IsNullOrEmpty(token))
                {
                    TempData["Error"] = "Phiên đăng nhập đã hết hạn.";
                    return RedirectToAction("Login");
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"https://localhost:7160/api/user/update-profile", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Update Infomation successfully.";
                    return RedirectToAction("profile");
                }

                TempData["Error"] = "Không thể cập nhật thông tin.";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật profile");
                TempData["Error"] = "Đã xảy ra lỗi máy chủ.";
                return View(model);
            }
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

        public IActionResult Users()
        {
            return View();
        }

    }

    public class LoginResponse
    {
        public string Token { get; set; }
        public int RoleId { get; set; }
       
    }
}