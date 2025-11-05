using Microsoft.AspNetCore.Mvc;

namespace FE_Capstone_Project.Controllers
{
    public class BookingWebController : Controller
    {
        public IActionResult booking()
        {
            return View();
        }
    }
}
