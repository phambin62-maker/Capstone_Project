using Microsoft.AspNetCore.Mvc;
using FE_Capstone_Project.Models;

namespace FE_Capstone_Project.Controllers
{
    public class PaymentController : Controller
    {
        public IActionResult Result(bool success, string orderDescription = "", string amount = "", string transactionId = "", string paymentMethod = "", string error = "")
        {
            var model = new PaymentResultViewModel
            {
                Success = success,
                OrderDescription = orderDescription,
                Amount = amount,
                TransactionId = transactionId,
                PaymentMethod = paymentMethod,
                Error = error
            };

            return View(model);
        }
    }
}
