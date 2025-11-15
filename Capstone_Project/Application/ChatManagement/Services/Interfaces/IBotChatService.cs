using BE_Capstone_Project.Application.ChatManagement.DTOs;

namespace BE_Capstone_Project.Application.ChatManagement.Services.Interfaces
{
    /// <summary>
    /// Service xử lý logic chat với bot (AI Assistant)
    /// </summary>
    public interface IBotChatService
    {
        /// <summary>
        /// Tạo tin nhắn từ customer đến bot
        /// </summary>
        Task<ChatDTO?> CreateBotChatAsync(CreateChatDTO createChatDto);

        /// <summary>
        /// Lấy conversation giữa customer và bot
        /// </summary>
        Task<ChatConversationDTO?> GetBotConversationAsync(int customerId);

        /// <summary>
        /// Tìm hoặc tạo bot user trong database
        /// </summary>
        Task<int> GetOrCreateBotUserAsync();

        /// <summary>
        /// Kiểm tra xem staffId có phải là bot không
        /// </summary>
        bool IsBot(int staffId);
    }
}

