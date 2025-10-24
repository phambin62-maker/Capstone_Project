using Microsoft.AspNetCore.Mvc;

namespace FE_Capstone_Project.Controllers
{
    public class TourWebController : Controller
    {
        public IActionResult Tour()
        {
            return View("tours");
        }
        public IActionResult TourDetails()
        {
            return View("tourdetails");
        }
    }
}
