using BE_Capstone_Project.Infrastructure;
using BE_Capstone_Project.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BE_Capstone_Project.DAO
{
    public class ChatDAO
    {
        private readonly OtmsdbContext _context;
        private readonly ILogger<ChatDAO> _logger;
        
        public ChatDAO(OtmsdbContext context, ILogger<ChatDAO> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> AddChatAsync(Chat chat)
        {
            try
            {
                _logger.LogInformation("Adding chat to database: CustomerId={CustomerId}, StaffId={StaffId}, MessageLength={MessageLength}",
                    chat.CustomerId, chat.StaffId, chat.Message?.Length ?? 0);

                await _context.Chats.AddAsync(chat);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Chat added successfully: chatId={ChatId}", chat.Id);
                return chat.Id;
            }
            catch (DbUpdateException dbEx)
            {
                var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                _logger.LogError(dbEx, "Database error while adding chat: CustomerId={CustomerId}, StaffId={StaffId}. Error: {Error}",
                    chat.CustomerId, chat.StaffId, innerMessage);
                
                // Kiểm tra nếu là lỗi foreign key constraint
                if (innerMessage.Contains("FOREIGN KEY") || innerMessage.Contains("The INSERT statement conflicted"))
                {
                    _logger.LogError("Foreign key constraint violation. CustomerId={CustomerId} or StaffId={StaffId} may not exist in User table.",
                        chat.CustomerId, chat.StaffId);
                }
                
                return -1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding a chat: CustomerId={CustomerId}, StaffId={StaffId}",
                    chat.CustomerId, chat.StaffId);
                return -1;
            }
        }

        public async Task<bool> UpdateChatAsync(Chat chat)
        {
            try
            {
                _context.Chats.Update(chat);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while updating the chat with ID {chat.Id}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteChatByIdAsync(int chatId)
        {
            try
            {
                var chat = await _context.Chats.FindAsync(chatId);
                if (chat != null)
                {
                    _context.Chats.Remove(chat);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting the chat with ID {chatId}: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Chat>> GetAllChatsAsync()
        {
            try
            {
                return await _context.Chats.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving all chats: {ex.Message}");
                return new List<Chat>();
            }
        }

        public async Task<Chat?> GetChatByIdAsync(int chatId)
        {
            try
            {
                return await _context.Chats.FindAsync(chatId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving the chat with ID {chatId}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Chat>> GetChatsByCustomerIdAndStaffIdAsync(int customerId, int staffId)
        {
            try
            {
                return await _context.Chats
                    .Where(c => c.CustomerId == customerId && c.StaffId == staffId)
                    .OrderBy(c => c.SentDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving chats for Customer ID {customerId} and Staff ID {staffId}: {ex.Message}");
                return new List<Chat>();
            }
        }

        public async Task<List<Chat>> GetChatsByCustomerIdAndStaffIdWithUsersAsync(int customerId, int staffId)
        {
            try
            {
                return await _context.Chats
                    .Include(c => c.Customer)
                    .Include(c => c.Staff)
                    .Where(c => c.CustomerId == customerId && c.StaffId == staffId)
                    .OrderBy(c => c.SentDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving chats with users for Customer ID {customerId} and Staff ID {staffId}: {ex.Message}");
                return new List<Chat>();
            }
        }

        public async Task<List<Chat>> GetChatsByStaffIdAsync(int staffId)
        {
            try
            {
                return await _context.Chats
                    .Where(c => c.StaffId == staffId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving chats for Staff ID {staffId}: {ex.Message}");
                return new List<Chat>();
            }
        }

        public async Task<List<Chat>> GetChatsByCustomerIdAsync(int customerId)
        {
            try
            {
                return await _context.Chats
                    .Where(c => c.CustomerId == customerId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving chats for Customer ID {customerId}: {ex.Message}");
                return new List<Chat>();
            }
        }
    }
}
