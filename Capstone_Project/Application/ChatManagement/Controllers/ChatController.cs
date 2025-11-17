using BE_Capstone_Project.Application.ChatManagement.DTOs;
using BE_Capstone_Project.Application.ChatManagement.Hubs;
using BE_Capstone_Project.Application.ChatManagement.Services.Interfaces;
using BE_Capstone_Project.DAO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Linq;
using System.Security.Claims;

namespace BE_Capstone_Project.Application.ChatManagement.Controllers
{
    [ApiController]
    [Route("api/chat")]
    [Authorize]
   

    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly UserDAO _userDAO;
        private readonly ILogger<ChatController> _logger;

        public ChatController(
            IChatService chatService,
            IHubContext<ChatHub> hubContext,
            UserDAO userDAO,
            ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _hubContext = hubContext;
            _userDAO = userDAO;
            _logger = logger;
        }

        /// <summary>
        /// Lấy UserId từ JWT token
        /// </summary>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
                return userId;
            return 0;
        }

        /// <summary>
        /// Lấy RoleId từ JWT token
        /// </summary>
        private int GetCurrentRoleId()
        {
            var roleIdClaim = User.FindFirst("RoleId")?.Value;
            if (int.TryParse(roleIdClaim, out int roleId))
                return roleId;
            return 0;
        }

        /// <summary>
        /// Gửi tin nhắn mới
        /// </summary>
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] CreateChatDTO createChatDto)
        {
            try
            {
                // Kiểm tra ModelState validation
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .SelectMany(x => x.Value?.Errors.Select(e => new { Field = x.Key, Message = e.ErrorMessage }))
                        .ToList();
                    
                    _logger.LogWarning("Model validation failed: {Errors}", string.Join(", ", errors.Select(e => $"{e.Field}: {e.Message}")));
                    
                    return BadRequest(new 
                    { 
                        message = "Validation failed",
                        errors = errors
                    });
                }

                // Kiểm tra null
                if (createChatDto == null)
                {
                    _logger.LogWarning("Request body is null");
                    return BadRequest(new { message = "Request body is required" });
                }

                // Kiểm tra các giá trị bắt buộc
                // StaffId phải là staff thật (> 0)
                if (createChatDto.StaffId <= 0)
                {
                    _logger.LogWarning("StaffId is invalid: {StaffId}", createChatDto.StaffId);
                    return BadRequest(new { message = "StaffId is required and must be a valid staff ID (greater than 0)" });
                }

                if (createChatDto.CustomerId <= 0)
                {
                    _logger.LogWarning("CustomerId is invalid: {CustomerId}", createChatDto.CustomerId);
                    return BadRequest(new { message = "CustomerId is required and must be valid" });
                }

                if (string.IsNullOrWhiteSpace(createChatDto.Message))
                {
                    _logger.LogWarning("Message is empty or null");
                    return BadRequest(new { message = "Message is required and cannot be empty" });
                }

                var currentUserId = GetCurrentUserId();
                var currentRoleId = GetCurrentRoleId();

                if (currentUserId == 0)
                    return Unauthorized(new { message = "Invalid user token" });

                // Validate: Customer chỉ có thể gửi đến staff
                if (currentRoleId == 3) // Customer
                {
                    if (createChatDto.CustomerId != currentUserId)
                        return Forbid("You can only send messages as yourself");
                }
                // Staff có thể gửi đến bất kỳ customer nào

                // Set sender ID
                createChatDto.SenderId = currentUserId;

                // Set ChatType dựa trên role của người gửi
                if (currentRoleId == 3) // Customer
                {
                    createChatDto.ChatType = (int)Domain.Enums.ChatType.Customer;
                }
                else if (currentRoleId == 2) // Staff
                {
                    createChatDto.ChatType = (int)Domain.Enums.ChatType.Staff;
                }

                // Tạo chat trong database - chỉ xử lý chat với staff thật
                ChatDTO? chatDto = await _chatService.CreateChatAsync(createChatDto);

                if (chatDto == null)
                {
                    _logger.LogError("Failed to create chat message: CustomerId={CustomerId}, StaffId={StaffId}, Message={Message}",
                        createChatDto.CustomerId, createChatDto.StaffId, createChatDto.Message);
                    
                    // Kiểm tra và trả về thông báo lỗi chi tiết hơn
                    var customer = await _userDAO.GetUserById(createChatDto.CustomerId);
                    var staff = await _userDAO.GetUserById(createChatDto.StaffId);
                    
                    string errorMessage = "Failed to create chat message.";
                    if (customer == null)
                    {
                        errorMessage += " Customer ID is invalid or not found.";
                    }
                    if (staff == null)
                    {
                        errorMessage += " Staff ID is invalid or not found.";
                    }
                    
                    return BadRequest(new { message = errorMessage });
                }

                // Update senderId in DTO
                chatDto.SenderId = createChatDto.SenderId;

                // Gửi real-time message qua SignalR
                if (currentRoleId == 3) // Customer gửi đến Staff
                {
                    var customer = await GetUserInfoAsync(createChatDto.CustomerId);
                    
                    // Gửi đến staff
                    await _hubContext.Clients.Group($"User_{createChatDto.StaffId}")
                        .SendAsync("ReceiveMessage", new
                        {
                            id = chatDto.Id,
                            staffId = chatDto.StaffId,
                            customerId = chatDto.CustomerId,
                            message = chatDto.Message,
                            customerName = customer?.Name,
                            sentDate = chatDto.SentDate,
                            senderId = createChatDto.SenderId
                        });

                    // Gửi lại cho chính customer để cập nhật UI
                    await _hubContext.Clients.Group($"User_{createChatDto.CustomerId}")
                        .SendAsync("ReceiveMessage", new
                        {
                            id = chatDto.Id,
                            staffId = chatDto.StaffId,
                            customerId = chatDto.CustomerId,
                            message = chatDto.Message,
                            customerName = customer?.Name,
                            sentDate = chatDto.SentDate,
                            senderId = createChatDto.SenderId
                        });

                    // Notification cho staff
                    await _hubContext.Clients.Group($"User_{createChatDto.StaffId}")
                        .SendAsync("NewMessageNotification", new
                        {
                            senderName = customer?.Name ?? "Customer",
                            preview = chatDto.Message?.Length > 50 
                                ? chatDto.Message.Substring(0, 50) + "..." 
                                : chatDto.Message,
                            timestamp = DateTime.UtcNow
                        });
                }
                else if (currentRoleId == 2) // Staff gửi đến Customer
                {
                    var staff = await GetUserInfoAsync(createChatDto.StaffId);
                    
                    // Gửi đến customer
                    await _hubContext.Clients.Group($"User_{createChatDto.CustomerId}")
                        .SendAsync("ReceiveMessage", new
                        {
                            id = chatDto.Id,
                            staffId = chatDto.StaffId,
                            customerId = chatDto.CustomerId,
                            message = chatDto.Message,
                            staffName = staff?.Name,
                            sentDate = chatDto.SentDate,
                            senderId = createChatDto.SenderId
                        });

                    // Gửi lại cho chính staff để cập nhật UI
                    await _hubContext.Clients.Group($"User_{createChatDto.StaffId}")
                        .SendAsync("ReceiveMessage", new
                        {
                            id = chatDto.Id,
                            staffId = chatDto.StaffId,
                            customerId = chatDto.CustomerId,
                            message = chatDto.Message,
                            staffName = staff?.Name,
                            sentDate = chatDto.SentDate,
                            senderId = createChatDto.SenderId
                        });

                    // Notification cho customer
                    await _hubContext.Clients.Group($"User_{createChatDto.CustomerId}")
                        .SendAsync("NewMessageNotification", new
                        {
                            senderName = staff?.Name ?? "Staff",
                            preview = chatDto.Message?.Length > 50 
                                ? chatDto.Message.Substring(0, 50) + "..." 
                                : chatDto.Message,
                            timestamp = DateTime.UtcNow
                        });
                }

                return Ok(chatDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy lịch sử chat giữa customer và staff
        /// </summary>
        [HttpGet("conversation/{customerId}/{staffId}")]
        public async Task<IActionResult> GetConversation(int customerId, int staffId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentRoleId = GetCurrentRoleId();

                // Validate: Customer chỉ xem được chat của mình
                if (currentRoleId == 3 && customerId != currentUserId)
                    return Forbid("You can only view your own conversations");

                // StaffId phải là staff thật
                if (staffId <= 0)
                {
                    return BadRequest(new { message = "Invalid staff ID" });
                }

                var conversation = await _chatService.GetConversationAsync(customerId, staffId);
                if (conversation == null)
                {
                    // Tạo conversation mới nếu chưa có
                    var customer = await _userDAO.GetUserById(customerId);
                    var staff = await _userDAO.GetUserById(staffId);
                    
                    conversation = new ChatConversationDTO
                    {
                        CustomerId = customerId,
                        CustomerName = $"{customer?.FirstName} {customer?.LastName}",
                        CustomerEmail = customer?.Email,
                        StaffId = staffId,
                        StaffName = $"{staff?.FirstName} {staff?.LastName}",
                        Messages = new List<ChatDTO>()
                    };
                }

                return Ok(conversation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversation");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy danh sách cuộc trò chuyện của customer
        /// </summary>
        [HttpGet("my-conversations")]
        public async Task<IActionResult> GetMyConversations()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentRoleId = GetCurrentRoleId();

                if (currentUserId == 0)
                    return Unauthorized(new { message = "Invalid user token" });

                List<ChatConversationDTO> conversations;

                if (currentRoleId == 3) // Customer
                {
                    conversations = await _chatService.GetConversationsByCustomerIdAsync(currentUserId);
                }
                else if (currentRoleId == 2) // Staff
                {
                    conversations = await _chatService.GetConversationsByStaffIdAsync(currentUserId);
                }
                else if (currentRoleId == 1) // Admin
                {
                    conversations = await _chatService.GetAllConversationsAsync();
                }
                else
                {
                    return Forbid("Invalid role");
                }

                return Ok(conversations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversations");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy danh sách cuộc trò chuyện (cho Staff/Admin)
        /// </summary>
        [HttpGet("conversations")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAllConversations()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentRoleId = GetCurrentRoleId();

                List<ChatConversationDTO> conversations;

                if (currentRoleId == 2) // Staff
                {
                    conversations = await _chatService.GetConversationsByStaffIdAsync(currentUserId);
                }
                else // Admin
                {
                    conversations = await _chatService.GetAllConversationsAsync();
                }

                return Ok(conversations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all conversations");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy số tin nhắn chưa đọc
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentRoleId = GetCurrentRoleId();

                int count = 0;
                if (currentRoleId == 3) // Customer
                {
                    count = await _chatService.GetUnreadCountByCustomerIdAsync(currentUserId);
                }
                else if (currentRoleId == 2) // Staff
                {
                    count = await _chatService.GetUnreadCountByStaffIdAsync(currentUserId);
                }

                return Ok(new { unreadCount = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy danh sách staff để customer chọn chat
        /// </summary>
        [HttpGet("staff-list")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStaffList()
        {
            try
            {
                // Lấy danh sách staff active (đã loại trừ bot trong UserDAO)
                var staffUsers = await _userDAO.GetActiveStaffAsync();
                
                var staffList = staffUsers
                    .Select(u => new
                    {
                        id = u.Id,
                        firstName = u.FirstName,
                        lastName = u.LastName,
                        email = u.Email,
                        fullName = $"{u.FirstName} {u.LastName}",
                        isBot = false
                    })
                    .ToList();

                _logger.LogInformation("Retrieved {Count} active staff members", staffList.Count);
                return Ok(staffList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting staff list");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Helper: Lấy thông tin user
        /// </summary>
        private async Task<UserInfo?> GetUserInfoAsync(int userId)
        {
            try
            {
                var user = await _userDAO.GetUserById(userId);
                if (user == null)
                    return null;

                return new UserInfo(
                    Name: $"{user.FirstName} {user.LastName}",
                    Email: user.Email ?? ""
                );
            }
            catch
            {
                return null;
            }
        }

    }
}

