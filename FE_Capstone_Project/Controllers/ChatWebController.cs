using FE_Capstone_Project.Filters;
using FE_Capstone_Project.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FE_Capstone_Project.Controllers
{
    [Authorize]
    public class ChatWebController : Controller
    {
        private readonly ApiHelper _apiHelper;
        private readonly ILogger<ChatWebController> _logger;

        public ChatWebController(ApiHelper apiHelper, ILogger<ChatWebController> logger)
        {
            _apiHelper = apiHelper;
            _logger = logger;
        }

        /// <summary>
        /// Trang danh sách cuộc trò chuyện (cho Customer)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var token = HttpContext.Session.GetString("JwtToken");
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Login", "AuthWeb");
                }

                ViewBag.JwtToken = token;

                // Lấy danh sách cuộc trò chuyện
                var conversations = await _apiHelper.GetAsync<List<dynamic>>("chat/my-conversations");
                
                if (conversations == null)
                {
                    conversations = new List<dynamic>();
                }

                return View(conversations);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "AuthWeb");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading chat index");
                ViewBag.Error = "Không thể tải danh sách cuộc trò chuyện";
                return View(new List<dynamic>());
            }
        }

        /// <summary>
        /// Trang chat với staff cụ thể (cho Customer)
        /// </summary>
        [HttpGet("conversation/{staffId}")]
        public async Task<IActionResult> Conversation(int staffId)
        {
            try
            {
                var token = HttpContext.Session.GetString("JwtToken");
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Login", "AuthWeb");
                }

                ViewBag.JwtToken = token;

                // Lấy UserId từ session
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return RedirectToAction("Login", "AuthWeb");
                }

                // Lấy lịch sử chat (hoặc tạo mới nếu chưa có)
                var conversation = await _apiHelper.GetAsync<dynamic>($"chat/conversation/{userId}/{staffId}");

                // Nếu không có conversation, API sẽ trả về conversation rỗng với messages = []
                // Không cần tạo mới ở đây vì API đã xử lý

                ViewBag.CustomerId = userId;
                ViewBag.StaffId = staffId;
                ViewBag.Conversation = conversation ?? new System.Dynamic.ExpandoObject();

                return View();
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "AuthWeb");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading conversation");
                ViewBag.Error = "Không thể tải cuộc trò chuyện";
                return View();
            }
        }

        /// <summary>
        /// Trang quản lý chat (cho Staff)
        /// </summary>
        [HttpGet("manage")]
        [AuthorizeRole(2)] // Chỉ Staff
        public async Task<IActionResult> Manage()
        {
            try
            {
                var token = HttpContext.Session.GetString("JwtToken");
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Login", "AuthWeb");
                }

                ViewBag.JwtToken = token;

                // Lấy danh sách cuộc trò chuyện
                var conversations = await _apiHelper.GetAsync<List<dynamic>>("chat/conversations");

                if (conversations == null)
                {
                    conversations = new List<dynamic>();
                }

                return View(conversations);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "AuthWeb");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading chat manage");
                ViewBag.Error = "Không thể tải danh sách cuộc trò chuyện";
                return View(new List<dynamic>());
            }
        }

        /// <summary>
        /// Trang chat với customer cụ thể (cho Staff)
        /// </summary>
        [HttpGet("manage/conversation/{customerId}")]
        [AuthorizeRole(2)] // Chỉ Staff
        public async Task<IActionResult> StaffConversation(int customerId)
        {
            try
            {
                var token = HttpContext.Session.GetString("JwtToken");
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Login", "AuthWeb");
                }

                ViewBag.JwtToken = token;

                // Lấy StaffId từ session
                var staffId = HttpContext.Session.GetInt32("UserId");
                if (staffId == null)
                {
                    return RedirectToAction("Login", "AuthWeb");
                }

                // Lấy lịch sử chat (hoặc tạo mới nếu chưa có)
                var conversation = await _apiHelper.GetAsync<dynamic>($"chat/conversation/{customerId}/{staffId}");

                // Nếu không có conversation, API sẽ trả về conversation rỗng với messages = []
                // Không cần tạo mới ở đây vì API đã xử lý

                ViewBag.CustomerId = customerId;
                ViewBag.StaffId = staffId;
                ViewBag.Conversation = conversation ?? new System.Dynamic.ExpandoObject();

                return View("StaffConversation");
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "AuthWeb");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading staff conversation");
                ViewBag.Error = "Không thể tải cuộc trò chuyện";
                return View("StaffConversation");
            }
        }
    }
}

