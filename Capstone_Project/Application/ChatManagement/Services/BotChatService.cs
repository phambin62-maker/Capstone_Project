using BE_Capstone_Project.Application.ChatManagement.DTOs;
using BE_Capstone_Project.Application.ChatManagement.Services.Interfaces;
using BE_Capstone_Project.DAO;
using BE_Capstone_Project.Domain.Enums;
using BE_Capstone_Project.Domain.Models;
using Microsoft.Extensions.Logging;

namespace BE_Capstone_Project.Application.ChatManagement.Services
{
    /// <summary>
    /// Service xử lý logic chat với bot (AI Assistant)
    /// </summary>
    public class BotChatService : IBotChatService
    {
        private readonly ChatDAO _chatDAO;
        private readonly UserDAO _userDAO;
        private readonly ILogger<BotChatService> _logger;
        private const int BOT_STAFF_ID = -1; // ID đặc biệt cho bot

        public BotChatService(ChatDAO chatDAO, UserDAO userDAO, ILogger<BotChatService> logger)
        {
            _chatDAO = chatDAO;
            _userDAO = userDAO;
            _logger = logger;
        }

        public bool IsBot(int staffId)
        {
            return staffId == BOT_STAFF_ID;
        }

        public async Task<ChatDTO?> CreateBotChatAsync(CreateChatDTO createChatDto)
        {
            try
            {
                _logger.LogInformation("Creating bot chat: CustomerId={CustomerId}, MessageLength={MessageLength}",
                    createChatDto.CustomerId, createChatDto.Message?.Length ?? 0);

                // Kiểm tra CustomerId có tồn tại không
                var customer = await _userDAO.GetUserById(createChatDto.CustomerId);
                if (customer == null)
                {
                    _logger.LogError("Customer not found: CustomerId={CustomerId}", createChatDto.CustomerId);
                    return null;
                }

                // Tìm hoặc tạo bot user
                var botUserId = await GetOrCreateBotUserAsync();
                if (botUserId <= 0)
                {
                    _logger.LogError("Failed to get or create bot user.");
                    return null;
                }

                // Tạo chat với bot
                var chat = new Chat
                {
                    StaffId = botUserId, // Sử dụng ID thật của bot user
                    CustomerId = createChatDto.CustomerId,
                    Message = createChatDto.Message,
                    ChatType = (ChatType?)createChatDto.ChatType,
                    SentDate = DateTime.UtcNow
                };

                var chatId = await _chatDAO.AddChatAsync(chat);
                if (chatId <= 0)
                {
                    _logger.LogError("Failed to create bot chat: AddChatAsync returned chatId={ChatId}", chatId);
                    return null;
                }

                var createdChat = await _chatDAO.GetChatByIdAsync(chatId);
                if (createdChat == null)
                {
                    _logger.LogError("Failed to retrieve created bot chat: chatId={ChatId}", chatId);
                    return null;
                }

                _logger.LogInformation("Bot chat created successfully: chatId={ChatId}", chatId);
                return MapToDTO(createdChat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bot chat: CustomerId={CustomerId}, Message={Message}",
                    createChatDto.CustomerId, createChatDto.Message);
                return null;
            }
        }

        public async Task<ChatConversationDTO?> GetBotConversationAsync(int customerId)
        {
            try
            {
                // Tìm bot user
                var botUser = await _userDAO.GetBotUserAsync();
                if (botUser == null)
                {
                    // Nếu không tìm thấy bot user, vẫn trả về conversation rỗng
                    var emtyCustomer = await _userDAO.GetUserById(customerId);
                    return new ChatConversationDTO
                    {
                        CustomerId = customerId,
                        CustomerName = $"{emtyCustomer?.FirstName} {emtyCustomer?.LastName}",
                        CustomerEmail = emtyCustomer?.Email,
                        StaffId = BOT_STAFF_ID, // Giữ nguyên -1 để frontend nhận biết là bot
                        StaffName = "AI Assistant",
                        Messages = new List<ChatDTO>()
                    };
                }

                var chats = await _chatDAO.GetChatsByCustomerIdAndStaffIdWithUsersAsync(customerId, botUser.Id);
                var customer = await _userDAO.GetUserById(customerId);

                var conversation = new ChatConversationDTO
                {
                    CustomerId = customerId,
                    CustomerName = $"{customer?.FirstName} {customer?.LastName}",
                    CustomerEmail = customer?.Email,
                    StaffId = BOT_STAFF_ID, // Giữ nguyên -1 để frontend nhận biết là bot
                    StaffName = "AI Assistant",
                    Messages = chats.Select(MapToDTO).ToList()
                };

                if (chats.Any())
                {
                    conversation.LastMessage = chats.OrderByDescending(c => c.SentDate).First().Message;
                    conversation.LastMessageDate = chats.OrderByDescending(c => c.SentDate).First().SentDate;
                }

                return conversation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bot conversation: CustomerId={CustomerId}", customerId);
                return null;
            }
        }

        public async Task<int> GetOrCreateBotUserAsync()
        {
            try
            {
                // Tìm user bot trong database
                var botUser = await _userDAO.GetBotUserAsync();
                
                if (botUser != null)
                {
                    _logger.LogInformation("Found bot user in database: botUserId={BotUserId}", botUser.Id);
                    return botUser.Id;
                }

                // Nếu không tìm thấy, tạo bot user mới
                _logger.LogInformation("Bot user not found. Creating new bot user...");
                
                var newBotUser = new User
                {
                    FirstName = "AI",
                    LastName = "Assistant",
                    Email = "bot@otms.com",
                    Username = "bot",
                    PasswordHash = "", // Bot không cần password
                    RoleId = 2, // Staff role
                    UserStatus = Domain.Enums.UserStatus.Active,
                    Provider = "System"
                };

                var botUserId = await _userDAO.AddUser(newBotUser);
                if (botUserId > 0)
                {
                    _logger.LogInformation("Bot user created successfully: botUserId={BotUserId}", botUserId);
                    return botUserId;
                }
                else
                {
                    _logger.LogError("Failed to create bot user.");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting or creating bot user");
                return -1;
            }
        }

        private ChatDTO MapToDTO(Chat chat)
        {
            return new ChatDTO
            {
                Id = chat.Id,
                StaffId = chat.StaffId,
                StaffName = $"{chat.Staff?.FirstName} {chat.Staff?.LastName}",
                CustomerId = chat.CustomerId,
                CustomerName = $"{chat.Customer?.FirstName} {chat.Customer?.LastName}",
                Message = chat.Message,
                ChatType = (int?)chat.ChatType,
                SentDate = chat.SentDate,
                SenderId = null // Will be set by controller based on current user
            };
        }
    }
}

