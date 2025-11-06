using BE_Capstone_Project.Application.Payment.VnPayService.Interfaces;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using static BE_Capstone_Project.Application.Payment.DTOs.PaymentInfDto;

namespace BE_Capstone_Project.Application.Payment.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IVnPayService _vnPayService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IVnPayService vnPayService, ILogger<PaymentController> logger)
        {
            _vnPayService = vnPayService;
            _logger = logger;
        }

        [HttpPost("create-payment")]
        public IActionResult CreatePaymentUrlVnpay([FromBody] PaymentInformationModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
                _logger.LogInformation($"Created payment URL for order: {model.OrderDescription}");

                return Ok(new { PaymentUrl = url });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating payment URL: {ex.Message}");
                return StatusCode(500, "An error occurred while processing payment");
            }
        }

        [HttpGet("payment-callback")]
        public IActionResult PaymentCallbackVnpay()
        {
            try
            {
                _logger.LogInformation($"Raw URL: {Request.GetDisplayUrl()}");
                _logger.LogInformation($"Query string: {Request.QueryString}");

                foreach (var query in Request.Query)
                {
                    _logger.LogInformation($"Parameter: {query.Key}={query.Value}");
                }

                var response = _vnPayService.PaymentExecute(Request.Query);
                _logger.LogInformation($"Payment callback processed for transaction: {response.TransactionId}");

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing payment callback: {ex.Message}");
                return StatusCode(500, "An error occurred while processing payment callback");
            }
        }
    }
}