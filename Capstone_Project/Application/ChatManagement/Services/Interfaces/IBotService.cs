namespace BE_Capstone_Project.Application.ChatManagement.Services.Interfaces
{
    /// <summary>
    /// Service xử lý logic AI Bot (OpenAI integration)
    /// </summary>
    public interface IBotService
    {
        /// <summary>
        /// Xử lý tin nhắn từ customer và trả về phản hồi từ bot
        /// </summary>
        /// <param name="customerId">ID của customer</param>
        /// <param name="customerMessage">Tin nhắn từ customer</param>
        /// <param name="staffId">ID của staff (bot ID = -1)</param>
        /// <returns>Phản hồi từ bot, hoặc null/empty nếu có lỗi</returns>
        Task<string?> ProcessCustomerMessageAsync(int customerId, string customerMessage, int staffId);
    }
}

