using FE_Capstone_Project.Filters;
using FE_Capstone_Project.Helpers;
using FE_Capstone_Project.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FE_Capstone_Project.Controllers
{
    [AuthorizeRole(1)] // Chỉ Admin (RoleId = 1) mới được truy cập
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
            try
            {
                // Kiểm tra token trong session
                var token = HttpContext.Session.GetString("JwtToken");
                if (string.IsNullOrEmpty(token))
                {
                    ViewBag.Error = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
                    return RedirectToAction("Login", "AuthWeb");
                }

                // đẩy token lên html  để JS có thể đọc 
                ViewBag.JwtToken = token;
                var endpoint = "admin/get-all-accounts";
                var users = await _apiHelper.GetAsync<List<AccountViewModel>>(endpoint);

                if (users == null)
                {
                    ViewBag.Error = "Không thể tải danh sách tài khoản!";
                    return View(new List<AccountViewModel>());
                }

                return View(users);
            }
            catch (UnauthorizedAccessException ex)
            {
                ViewBag.Error = ex.Message;
                return RedirectToAction("Login", "AuthWeb");
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Lỗi: {ex.Message}";
                return View(new List<AccountViewModel>());
            }
        }
        public IActionResult Report()
        {
            return View();
        }
    }
}