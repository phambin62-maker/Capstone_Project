using BE_Capstone_Project.Application.ChatManagement.DTOs;

namespace BE_Capstone_Project.Application.ChatManagement.Services.Interfaces
{
    public interface IChatService
    {
        Task<ChatDTO?> CreateChatAsync(CreateChatDTO createChatDto);
        Task<List<ChatDTO>> GetChatsByCustomerIdAndStaffIdAsync(int customerId, int staffId);
        Task<List<ChatConversationDTO>> GetConversationsByCustomerIdAsync(int customerId);
        Task<List<ChatConversationDTO>> GetConversationsByStaffIdAsync(int staffId);
        Task<List<ChatConversationDTO>> GetAllConversationsAsync();
        Task<ChatConversationDTO?> GetConversationAsync(int customerId, int staffId);
        Task<int> GetUnreadCountByCustomerIdAsync(int customerId);
        Task<int> GetUnreadCountByStaffIdAsync(int staffId);
    }
}

