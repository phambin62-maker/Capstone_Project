using BE_Capstone_Project.Application.Payment.Library;
using BE_Capstone_Project.Application.Payment.VnPayService.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.RegularExpressions;
using static BE_Capstone_Project.Application.Payment.DTOs.PaymentInfDto;
using Microsoft.AspNetCore.Http.Extensions;
namespace BE_Capstone_Project.Application.Payment.VnPayService
{
    public class VnPayService : IVnPayService
    {
        
        private readonly IConfiguration _configuration;
        private readonly ILogger<VnPayService> _logger;
        private const int MIN_AMOUNT = 1000;
        private const int MAX_AMOUNT = 1000000000;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public VnPayService(IConfiguration configuration, ILogger<VnPayService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            ValidateConfiguration();
        }

        private void ValidateConfiguration()
        {
            var requiredConfigs = new[]
            {
                "Vnpay:TmnCode",
                "Vnpay:HashSecret",
                "Vnpay:BaseUrl",
                "Vnpay:Command",
                "Vnpay:CurrCode",
                "Vnpay:Version",
                "Vnpay:Locale",
                "TimeZoneId"
            };

            foreach (var config in requiredConfigs)
            {
                if (string.IsNullOrEmpty(_configuration[config]))
                {
                    throw new InvalidOperationException($"Missing configuration: {config}");
                }
            }
        }

        public string CreatePaymentUrl(PaymentInformationModel model, HttpContext context)
        {
            try
            {
                ValidatePaymentModel(model);

                var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"]);
                var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
                var ticketId = $"{DateTime.Now.Ticks}{model.Name.GetHashCode()}";

                var vnPay = new VnPayLibrary();
                var urlCallBack = _configuration["PaymentCallBack:ReturnUrl"];
                if (string.IsNullOrEmpty(urlCallBack))
                {
                    urlCallBack = "https://localhost:7160/api/payment/payment-callback";
                }
                Console.WriteLine(urlCallBack);

                // Đảm bảo TmnCode không có khoảng trắng
                var tmnCode = _configuration["Vnpay:TmnCode"]?.Trim();

                // Encode OrderInfo đúng cách
                var orderInfo = WebUtility.UrlEncode($"{model.Name} {model.OrderDescription} {model.Amount}");
                var ipAddress = context?.Connection?.RemoteIpAddress?.ToString() ?? "127.0.0.1";
                var requestData = new Dictionary<string, string>
                {
                    {"vnp_Version", _configuration["Vnpay:Version"]},
                    {"vnp_Command", _configuration["Vnpay:Command"]},
                    {"vnp_TmnCode", tmnCode},
                    {"vnp_Amount", ((long)(model.Amount * 100)).ToString()},
                    {"vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss")},
                    {"vnp_CurrCode", _configuration["Vnpay:CurrCode"]},
                    {"vnp_IpAddr", ipAddress},
                    {"vnp_Locale", _configuration["Vnpay:Locale"]},
                    {"vnp_OrderInfo", orderInfo},
                    {"vnp_OrderType", model.OrderType?.Trim() ?? "other"},
                    {"vnp_ReturnUrl", urlCallBack},
                    {"vnp_TxnRef", ticketId}
                };

                foreach (var item in requestData)
                {
                    vnPay.AddRequestData(item.Key, item.Value);
                }

                var paymentUrl = vnPay.CreateRequestUrl(
                    _configuration["Vnpay:BaseUrl"],
                    _configuration["Vnpay:HashSecret"]

                );

                _logger.LogInformation($"Created payment URL for transaction {ticketId}");
                return paymentUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating payment URL: {ex.Message}");
                throw new ApplicationException("Failed to create payment URL", ex);
            }
        }

        public PaymentResponseModel PaymentExecute(IQueryCollection collections)
        {
            try
            {
                // Log toàn bộ dữ liệu collections nhận được
                _logger.LogInformation("Received payment callback with {Count} parameters", collections?.Count ?? 0);
                if (collections != null)
                {
                    foreach (var item in collections)
                    {
                        _logger.LogInformation("Parameter {Key}: {Value}", item.Key, item.Value.ToString());
                    }
                }

                if (collections == null || !collections.Any())
                {
                    _logger.LogWarning("Received empty callback data");
                    var requestUrl = _httpContextAccessor?.HttpContext?.Request?.GetDisplayUrl();
                    _logger.LogWarning("Request URL: {RequestUrl}", requestUrl ?? "No URL available");

                    // Log thêm thông tin Request
                    var request = _httpContextAccessor?.HttpContext?.Request;
                    if (request != null)
                    {
                        _logger.LogWarning("Request Method: {Method}", request.Method);
                        _logger.LogWarning("Request Headers: {@Headers}",
                            request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()));
                    }

                    throw new ArgumentException("Payment response data is empty");
                }

                var vnPay = new VnPayLibrary();
                var hashSecret = _configuration["Vnpay:HashSecret"]
                    ?? throw new InvalidOperationException("VNPay HashSecret is not configured");

                var response = vnPay.GetFullResponseData(collections, hashSecret);

                if (!response.Success)
                {
                    _logger.LogWarning("Payment validation failed for transaction {TransactionId}",
                        response.TransactionId ?? "Unknown");

                    // Log thêm thông tin response để debug
                    _logger.LogWarning("Payment response details: {@Response}", response);

                    return new PaymentResponseModel
                    {
                        Success = false,
                        PaymentMethod = "VnPay",
                        OrderDescription = "Payment validation failed",
                        VnPayResponseCode = response.VnPayResponseCode // Thêm mã lỗi vào response
                    };
                }

                // Log successful transaction
                _logger.LogInformation(
                    "Successful payment: TransactionId={TransactionId}, OrderId={OrderId}, Amount={Amount}",
                    response.TransactionId,
                    response.OrderId,
                    collections["vnp_Amount"]);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment response");
                throw new ApplicationException("Failed to process payment response", ex);
            }
        }

        private void ValidatePaymentModel(PaymentInformationModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var validationErrors = new List<string>();

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                validationErrors.Add("Name is required");
            }

            if (string.IsNullOrWhiteSpace(model.OrderType))
            {
                validationErrors.Add("OrderType is required");
            }

            if (string.IsNullOrWhiteSpace(model.OrderDescription))
            {
                validationErrors.Add("OrderDescription is required");
            }

            if (model.Amount < MIN_AMOUNT || model.Amount > MAX_AMOUNT)
            {
                validationErrors.Add($"Amount must be between {MIN_AMOUNT} and {MAX_AMOUNT}");
            }

            if (validationErrors.Any())
            {
                throw new ArgumentException(string.Join(", ", validationErrors));
            }
        }

        private string SanitizeOrderInfo(string orderInfo)
        {
            if (string.IsNullOrEmpty(orderInfo))
                return string.Empty;

            // Remove special characters and limit length
            var sanitized = Regex.Replace(orderInfo, @"[^\w\s-]", "");
            return sanitized.Length > 255 ? sanitized.Substring(0, 255) : sanitized;
        }

        private string SanitizeOrderType(string orderType)
        {
            if (string.IsNullOrEmpty(orderType))
                return "other";

            // Ensure orderType only contains alphanumeric characters
            return Regex.Replace(orderType, @"[^\w]", "");
        }
    }
}