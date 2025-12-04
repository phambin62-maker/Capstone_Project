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
        /// View hiển thị chatbot DenserAI cho iframe widget
        /// </summary>
        [AllowAnonymous]
        [HttpGet("/ChatWeb/BotWidget")]
        public IActionResult BotWidget()
        {
            return View("BotWidget");
        }
    }
}

