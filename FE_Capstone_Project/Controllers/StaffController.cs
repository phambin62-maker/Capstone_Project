using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BE_Capstone_Project.Controllers
{
    //[Authorize] // Nếu cần authentication
    public class StaffController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Staff Dashboard";
            return View();
        }

        public IActionResult Tours()
        {
            ViewData["Title"] = "Quản lý Tour";
            return View();
        }

        public IActionResult Dashboard()
        {
            ViewData["Title"] = "Dashboard";
            return View();
        }

        public IActionResult Bookings()
        {
            ViewData["Title"] = "Quản lý Đặt tour";
            return View();
        }

        public IActionResult Customers()
        {
            ViewData["Title"] = "Quản lý Khách hàng";
            return View();
        }

        public IActionResult Schedules()
        {
            ViewData["Title"] = "Lịch trình Tour";
            return View();
        }

        public IActionResult Tasks()
        {
            ViewData["Title"] = "Công việc của tôi";
            return View();
        }

        public IActionResult Profile()
        {
            ViewData["Title"] = "Hồ sơ cá nhân";
            return View();
        }
    }
}