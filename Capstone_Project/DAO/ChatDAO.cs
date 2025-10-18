using BE_Capstone_Project.Infrastructure;
using BE_Capstone_Project.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BE_Capstone_Project.DAO
{
    public class ChatDAO
    {
        private readonly OtmsdbContext _context;
        public ChatDAO(OtmsdbContext context)
        {
            _context = context;
        }

        public async Task<int> AddChatAsync(Chat chat)
        {
            try
            {
                await _context.Chats.AddAsync(chat);
                await _context.SaveChangesAsync();
                return chat.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while adding a chat: {ex.Message}");
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
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving chats for Customer ID {customerId} and Staff ID {staffId}: {ex.Message}");
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
