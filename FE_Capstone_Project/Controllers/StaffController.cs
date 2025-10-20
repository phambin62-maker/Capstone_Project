using Microsoft.AspNetCore.Mvc;

namespace FE_Capstone_Project.Controllers
{
    public class StaffController : Controller
    {
        public IActionResult Tours()
        {
            return View();
        }
        public IActionResult Blog()
        {
            return View();
        }
        public IActionResult BlogDetail()
        {
            return View();
        }
        public IActionResult StarterPage() 
        { 
            return View("Views/Staff/starter-page.cshtml");
        }
        public IActionResult Destinations()
        {
            return View("destinations");
        }
        public IActionResult DestinationsDetails()
        {
            return View("destination-details");
        }
    }
}
