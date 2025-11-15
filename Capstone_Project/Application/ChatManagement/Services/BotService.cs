using BE_Capstone_Project.Application.ChatManagement.Services.Interfaces;
using BE_Capstone_Project.DAO;
using BE_Capstone_Project.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BE_Capstone_Project.Application.ChatManagement.Services
{
    /// <summary>
    /// Service xử lý logic AI Bot (OpenAI integration)
    /// </summary>
    public class BotService : IBotService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BotService> _logger;
        private readonly UserDAO _userDAO;
        private readonly ChatDAO _chatDAO;
        private readonly IConfiguration _configuration;
        private readonly string? _openAiApiKey;
        private readonly string? _openAiModel;
        private readonly string _openAiApiUrl;

        public BotService(
            IHttpClientFactory httpClientFactory,
            ILogger<BotService> logger,
            UserDAO userDAO,
            ChatDAO chatDAO,
            IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
            _userDAO = userDAO;
            _chatDAO = chatDAO;
            _configuration = configuration;

            // Lấy cấu hình từ appsettings.json
            _openAiApiKey = _configuration["OpenAI:ApiKey"];
            _openAiModel = _configuration["OpenAI:Model"] ?? "gpt-3.5-turbo";
            _openAiApiUrl = _configuration["OpenAI:ApiUrl"] ?? "https://api.openai.com/v1/chat/completions";

            // Cấu hình HttpClient
            if (!string.IsNullOrEmpty(_openAiApiKey))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _openAiApiKey);
            }
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<string?> ProcessCustomerMessageAsync(int customerId, string customerMessage, int staffId)
        {
            try
            {
                // Kiểm tra API key
                if (string.IsNullOrEmpty(_openAiApiKey))
                {
                    _logger.LogWarning("OpenAI API key is not configured");
                    return "Xin lỗi, dịch vụ AI tạm thời không khả dụng. Vui lòng thử lại sau.";
                }

                // Lấy thông tin customer để tạo context
                var customer = await _userDAO.GetUserById(customerId);
                var customerName = customer != null ? $"{customer.FirstName} {customer.LastName}" : "Khách hàng";

                // Lấy lịch sử chat gần đây để tạo context
                var recentChats = await GetRecentChatHistoryAsync(customerId, staffId, 5);

                // Tạo system prompt
                var systemPrompt = CreateSystemPrompt(customerName);

                // Tạo messages cho OpenAI API
                var messages = new List<object>
                {
                    new { role = "system", content = systemPrompt }
                };

                // Thêm lịch sử chat
                foreach (var chat in recentChats)
                {
                    if (chat.ChatType != ChatType.Customer) // Assuming Customer is an enum member
                    {
                        messages.Add(new { role = "user", content = chat.Message });
                    }
                    else // Staff/Bot message
                    {
                        messages.Add(new { role = "assistant", content = chat.Message });
                    }
                }

                // Thêm tin nhắn hiện tại
                messages.Add(new { role = "user", content = customerMessage });

                // Gọi OpenAI API
                var response = await CallAIAsync(systemPrompt, customerMessage, messages);

                if (string.IsNullOrWhiteSpace(response))
                {
                    _logger.LogWarning("Bot returned empty response for customer {CustomerId}", customerId);
                    return null;
                }

                _logger.LogInformation("Bot responded to customer {CustomerId}: {Response}", customerId, response.Substring(0, Math.Min(50, response.Length)));
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing customer message: CustomerId={CustomerId}, Message={Message}", customerId, customerMessage);
                return "Xin lỗi, đã có lỗi xảy ra khi xử lý tin nhắn của bạn. Vui lòng thử lại sau.";
            }
        }

        /// <summary>
        /// Gọi OpenAI API để lấy phản hồi
        /// </summary>
        private async Task<string?> CallAIAsync(string systemPrompt, string userMessage, List<object> messages)
        {
            try
            {
                var requestBody = new
                {
                    model = _openAiModel,
                    messages = messages,
                    temperature = 0.7,
                    max_tokens = 500
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_openAiApiUrl, content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("OpenAI API error: StatusCode={StatusCode}, Response={Response}", 
                        response.StatusCode, errorContent);

                    // Xử lý lỗi rate limit
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        _logger.LogWarning("OpenAI API rate limit exceeded");
                        return "Xin lỗi, hệ thống đang quá tải. Vui lòng thử lại sau vài giây.";
                    }

                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(responseContent);
                
                var choice = jsonDoc.RootElement.GetProperty("choices")[0];
                var message = choice.GetProperty("message");
                var contents = message.GetProperty("content").GetString();

                return contents;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP error calling OpenAI API");
                return null;
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("OpenAI API request timeout");
                return "Xin lỗi, yêu cầu đang mất quá nhiều thời gian. Vui lòng thử lại.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling OpenAI API");
                return null;
            }
        }

        /// <summary>
        /// Lấy lịch sử chat gần đây
        /// </summary>
        private async Task<List<Domain.Models.Chat>> GetRecentChatHistoryAsync(int customerId, int staffId, int limit)
        {
            try
            {
                // Nếu là bot, cần tìm bot user thật
                int actualStaffId = staffId;
                if (staffId == -1)
                {
                    var botUser = await _userDAO.GetBotUserAsync();
                    if (botUser != null)
                    {
                        actualStaffId = botUser.Id;
                    }
                }

                var chats = await _chatDAO.GetChatsByCustomerIdAndStaffIdAsync(customerId, actualStaffId);
                return chats.OrderByDescending(c => c.SentDate).Take(limit).OrderBy(c => c.SentDate).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat history");
                return new List<Domain.Models.Chat>();
            }
        }

        /// <summary>
        /// Tạo system prompt cho bot
        /// </summary>
        private string CreateSystemPrompt(string customerName)
        {
            return $@"Bạn là một trợ lý AI thân thiện và chuyên nghiệp của một công ty du lịch (OTMS - Online Tour Management System). 
Nhiệm vụ của bạn là hỗ trợ khách hàng {customerName} với các câu hỏi về:
- Thông tin về các tour du lịch
- Đặt tour và thanh toán
- Hủy tour và chính sách hoàn tiền
- Câu hỏi chung về dịch vụ

Hãy trả lời một cách:
- Thân thiện, nhiệt tình và chuyên nghiệp
- Ngắn gọn, rõ ràng và dễ hiểu
- Bằng tiếng Việt
- Nếu không chắc chắn, hãy đề nghị khách hàng liên hệ với nhân viên hỗ trợ

Hãy bắt đầu cuộc trò chuyện một cách tự nhiên và hữu ích.";
        }
    }
}

