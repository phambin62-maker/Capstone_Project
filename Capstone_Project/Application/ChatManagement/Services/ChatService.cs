using BE_Capstone_Project.Application.ChatManagement.DTOs;
using BE_Capstone_Project.Application.ChatManagement.Services.Interfaces;
using BE_Capstone_Project.DAO;
using BE_Capstone_Project.Domain.Enums;
using BE_Capstone_Project.Domain.Models;
using Microsoft.Extensions.Logging;

namespace BE_Capstone_Project.Application.ChatManagement.Services
{
    public class ChatService : IChatService
    {
        private readonly ChatDAO _chatDAO;
        private readonly UserDAO _userDAO;
        private readonly ILogger<ChatService> _logger;

        public ChatService(ChatDAO chatDAO, UserDAO userDAO, ILogger<ChatService> logger)
        {
            _chatDAO = chatDAO;
            _userDAO = userDAO;
            _logger = logger;
        }

        public async Task<ChatDTO?> CreateChatAsync(CreateChatDTO createChatDto)
        {
            try
            {
                _logger.LogInformation("Creating chat: CustomerId={CustomerId}, StaffId={StaffId}, MessageLength={MessageLength}, ChatType={ChatType}",
                    createChatDto.CustomerId, createChatDto.StaffId, createChatDto.Message?.Length ?? 0, createChatDto.ChatType);

                // Kiểm tra CustomerId có tồn tại không
                var customer = await _userDAO.GetUserById(createChatDto.CustomerId);
                if (customer == null)
                {
                    _logger.LogError("Customer not found: CustomerId={CustomerId}", createChatDto.CustomerId);
                    return null;
                }

                // Kiểm tra StaffId có tồn tại không (chỉ xử lý chat với staff thật, không xử lý bot)
                var staff = await _userDAO.GetUserById(createChatDto.StaffId);
                if (staff == null)
                {
                    _logger.LogError("Staff not found: StaffId={StaffId}", createChatDto.StaffId);
                    return null;
                }

                var chat = new Chat
                {
                    StaffId = createChatDto.StaffId,
                    CustomerId = createChatDto.CustomerId,
                    Message = createChatDto.Message,
                    ChatType = (ChatType?)createChatDto.ChatType,
                    SentDate = DateTime.UtcNow
                };

                var chatId = await _chatDAO.AddChatAsync(chat);
                if (chatId <= 0)
                {
                    _logger.LogError("Failed to create chat: AddChatAsync returned chatId={ChatId}", chatId);
                    return null;
                }

                var createdChat = await _chatDAO.GetChatByIdAsync(chatId);
                if (createdChat == null)
                {
                    _logger.LogError("Failed to retrieve created chat: chatId={ChatId}", chatId);
                    return null;
                }

                _logger.LogInformation("Chat created successfully: chatId={ChatId}", chatId);
                return MapToDTO(createdChat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating chat: CustomerId={CustomerId}, StaffId={StaffId}, Message={Message}",
                    createChatDto.CustomerId, createChatDto.StaffId, createChatDto.Message);
                return null;
            }
        }

        public async Task<List<ChatDTO>> GetChatsByCustomerIdAndStaffIdAsync(int customerId, int staffId)
        {
            try
            {
                var chats = await _chatDAO.GetChatsByCustomerIdAndStaffIdWithUsersAsync(customerId, staffId);
                return chats.Select(MapToDTO).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting chats: {ex.Message}");
                return new List<ChatDTO>();
            }
        }

        public async Task<List<ChatConversationDTO>> GetConversationsByCustomerIdAsync(int customerId)
        {
            try
            {
                var chats = await _chatDAO.GetChatsByCustomerIdAsync(customerId);
                var groupedChats = chats
                    .GroupBy(c => c.StaffId)
                    .Select(g => new
                    {
                        StaffId = g.Key,
                        Chats = g.OrderByDescending(c => c.SentDate).ToList()
                    })
                    .ToList();

                var conversations = new List<ChatConversationDTO>();

                foreach (var group in groupedChats)
                {
                    var staff = await _userDAO.GetUserById(group.StaffId);
                    var lastChat = group.Chats.First();
                    var customer = await _userDAO.GetUserById(customerId);

                    conversations.Add(new ChatConversationDTO
                    {
                        CustomerId = customerId,
                        CustomerName = $"{customer?.FirstName} {customer?.LastName}",
                        CustomerEmail = customer?.Email,
                        StaffId = group.StaffId,
                        StaffName = $"{staff?.FirstName} {staff?.LastName}",
                        LastMessage = lastChat.Message,
                        LastMessageDate = lastChat.SentDate,
                        Messages = group.Chats.OrderBy(c => c.SentDate).Select(MapToDTO).ToList()
                    });
                }

                return conversations.OrderByDescending(c => c.LastMessageDate).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting conversations by customer: {ex.Message}");
                return new List<ChatConversationDTO>();
            }
        }

        public async Task<List<ChatConversationDTO>> GetConversationsByStaffIdAsync(int staffId)
        {
            try
            {
                var chats = await _chatDAO.GetChatsByStaffIdAsync(staffId);
                var groupedChats = chats
                    .GroupBy(c => c.CustomerId)
                    .Select(g => new
                    {
                        CustomerId = g.Key,
                        Chats = g.OrderByDescending(c => c.SentDate).ToList()
                    })
                    .ToList();

                var conversations = new List<ChatConversationDTO>();

                foreach (var group in groupedChats)
                {
                    var customer = await _userDAO.GetUserById(group.CustomerId);
                    var lastChat = group.Chats.First();
                    var staff = await _userDAO.GetUserById(staffId);

                    conversations.Add(new ChatConversationDTO
                    {
                        CustomerId = group.CustomerId,
                        CustomerName = $"{customer?.FirstName} {customer?.LastName}",
                        CustomerEmail = customer?.Email,
                        StaffId = staffId,
                        StaffName = $"{staff?.FirstName} {staff?.LastName}",
                        LastMessage = lastChat.Message,
                        LastMessageDate = lastChat.SentDate,
                        Messages = group.Chats.OrderBy(c => c.SentDate).Select(MapToDTO).ToList()
                    });
                }

                return conversations.OrderByDescending(c => c.LastMessageDate).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting conversations by staff: {ex.Message}");
                return new List<ChatConversationDTO>();
            }
        }

        public async Task<List<ChatConversationDTO>> GetAllConversationsAsync()
        {
            try
            {
                var allChats = await _chatDAO.GetAllChatsAsync();
                var groupedChats = allChats
                    .GroupBy(c => new { c.CustomerId, c.StaffId })
                    .Select(g => new
                    {
                        g.Key.CustomerId,
                        g.Key.StaffId,
                        Chats = g.OrderByDescending(c => c.SentDate).ToList()
                    })
                    .ToList();

                var conversations = new List<ChatConversationDTO>();

                foreach (var group in groupedChats)
                {
                    var customer = await _userDAO.GetUserById(group.CustomerId);
                    var staff = await _userDAO.GetUserById(group.StaffId);
                    var lastChat = group.Chats.First();

                    conversations.Add(new ChatConversationDTO
                    {
                        CustomerId = group.CustomerId,
                        CustomerName = $"{customer?.FirstName} {customer?.LastName}",
                        CustomerEmail = customer?.Email,
                        StaffId = group.StaffId,
                        StaffName = $"{staff?.FirstName} {staff?.LastName}",
                        LastMessage = lastChat.Message,
                        LastMessageDate = lastChat.SentDate,
                        Messages = group.Chats.OrderBy(c => c.SentDate).Select(MapToDTO).ToList()
                    });
                }

                return conversations.OrderByDescending(c => c.LastMessageDate).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all conversations: {ex.Message}");
                return new List<ChatConversationDTO>();
            }
        }

        public async Task<ChatConversationDTO?> GetConversationAsync(int customerId, int staffId)
        {
            try
            {
                // Chỉ xử lý chat với staff thật, không xử lý bot ở đây
                var chats = await _chatDAO.GetChatsByCustomerIdAndStaffIdWithUsersAsync(customerId, staffId);
                var customer = await _userDAO.GetUserById(customerId);
                var staff = await _userDAO.GetUserById(staffId);

                // Nếu không có chat nào, vẫn trả về conversation với messages rỗng
                var conversation = new ChatConversationDTO
                {
                    CustomerId = customerId,
                    CustomerName = $"{customer?.FirstName} {customer?.LastName}",
                    CustomerEmail = customer?.Email,
                    StaffId = staffId,
                    StaffName = $"{staff?.FirstName} {staff?.LastName}",
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
                _logger.LogError(ex, "Error getting conversation: CustomerId={CustomerId}, StaffId={StaffId}", customerId, staffId);
                return null;
            }
        }

        public async Task<int> GetUnreadCountByCustomerIdAsync(int customerId)
        {
            // TODO: Implement unread count logic when IsRead field is added
            return 0;
        }

        public async Task<int> GetUnreadCountByStaffIdAsync(int staffId)
        {
            // TODO: Implement unread count logic when IsRead field is added
            return 0;
        }


        private ChatDTO MapToDTO(Chat chat)
        {
            // Determine sender ID by checking if message was sent by customer or staff
            // This is a simple approach - you might want to add SenderId field to Chat table
            int? senderId = null;
            // For now, we'll determine sender based on the message content or add a field later
            
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
                SenderId = senderId // Will be set by controller based on current user
            };
        }
    }
}

