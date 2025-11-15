using Microsoft.AspNetCore.Mvc;
using FE_Capstone_Project.Models;
using System.Text.RegularExpressions;
using FE_Capstone_Project.Helpers;
using System.Threading.Tasks;

namespace FE_Capstone_Project.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ApiHelper _apiHelper;

        public PaymentController(ApiHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        public async Task<IActionResult> Result(bool success, string orderDescription = "", string amount = "", string transactionId = "", string paymentMethod = "", string error = "")
        {
            var decoded = Uri.UnescapeDataString(orderDescription);

            int? bookingId = null;
            var match = Regex.Match(decoded, @"#(\d+)");
            if (match.Success)
            {
                bookingId = int.Parse(match.Groups[1].Value);
                PaymentDTO data = new PaymentDTO()
                {
                    BookingId = bookingId.Value,
                    PaymentMethod = paymentMethod,
                    Success = success,
                };
                var paymentResponse = await _apiHelper.PostAsync<PaymentDTO, bool>("Booking/payment-update", data);
            }

            var model = new PaymentResultViewModel
            {
                Success = success,
                OrderDescription = orderDescription,
                Amount = (int.Parse(amount) / 100).ToString(),
                TransactionId = transactionId,
                PaymentMethod = paymentMethod,
                Error = error
            };  

            return View(model);
        }
    }
}
