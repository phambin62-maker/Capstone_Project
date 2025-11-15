using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace BE_Capstone_Project.Application.ChatManagement.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(ILogger<ChatHub> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Khi client kết nối, thêm vào group theo UserId
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
                _logger.LogInformation($"User {userId} connected with connection ID: {Context.ConnectionId}");
            }
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Khi client ngắt kết nối
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
                _logger.LogInformation($"User {userId} disconnected. Connection ID: {Context.ConnectionId}");
            }
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Gửi tin nhắn đến customer cụ thể
        /// </summary>
        public async Task SendMessageToCustomer(int customerId, int staffId, string message, string staffName)
        {
            var senderId = GetUserId();
            _logger.LogInformation($"Staff {staffId} sending message to Customer {customerId}: {message}");

            // Gửi đến customer
            await Clients.Group($"User_{customerId}").SendAsync("ReceiveMessage", new
            {
                staffId,
                customerId,
                message,
                staffName,
                sentDate = DateTime.UtcNow,
                senderId = staffId
            });

            // Gửi lại cho chính staff để cập nhật UI
            await Clients.Group($"User_{staffId}").SendAsync("ReceiveMessage", new
            {
                staffId,
                customerId,
                message,
                staffName,
                sentDate = DateTime.UtcNow,
                senderId = staffId
            });
        }

        /// <summary>
        /// Gửi tin nhắn đến staff cụ thể
        /// </summary>
        public async Task SendMessageToStaff(int customerId, int staffId, string message, string customerName)
        {
            var senderId = GetUserId();
            _logger.LogInformation($"Customer {customerId} sending message to Staff {staffId}: {message}");

            // Gửi đến staff
            await Clients.Group($"User_{staffId}").SendAsync("ReceiveMessage", new
            {
                staffId,
                customerId,
                message,
                customerName,
                sentDate = DateTime.UtcNow,
                senderId = customerId
            });

            // Gửi lại cho chính customer để cập nhật UI
            await Clients.Group($"User_{customerId}").SendAsync("ReceiveMessage", new
            {
                staffId,
                customerId,
                message,
                customerName,
                sentDate = DateTime.UtcNow,
                senderId = customerId
            });
        }

        /// <summary>
        /// Thông báo có tin nhắn mới (cho notification)
        /// </summary>
        public async Task NotifyNewMessage(int recipientId, string senderName, string preview)
        {
            await Clients.Group($"User_{recipientId}").SendAsync("NewMessageNotification", new
            {
                senderName,
                preview,
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Lấy UserId từ Claims
        /// </summary>
        private string? GetUserId()
        {
            return Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}

