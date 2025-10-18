using BE_Capstone_Project.Infrastructure;
using BE_Capstone_Project.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BE_Capstone_Project.DAO
{
    public class NewsDAO
    {
        private readonly OtmsdbContext _context;

        public NewsDAO(OtmsdbContext context)
        {
            _context = context;
        }

        // 🔹 Thêm tin tức mới
        public async Task<int> AddNewsAsync(News news)
        {
            try
            {
                await _context.News.AddAsync(news);
                await _context.SaveChangesAsync();
                return news.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while adding news: {ex.Message}");
                return -1;
            }
        }

        // 🔹 Cập nhật tin tức
        public async Task<bool> UpdateNewsAsync(News news)
        {
            try
            {
                _context.News.Update(news);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while updating news ID {news.Id}: {ex.Message}");
                return false;
            }
        }

        // 🔹 Xóa tin tức theo ID
        public async Task<bool> DeleteNewsByIdAsync(int newsId)
        {
            try
            {
                var news = await _context.News.FindAsync(newsId);
                if (news == null)
                    return false;

                _context.News.Remove(news);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while deleting news ID {newsId}: {ex.Message}");
                return false;
            }
        }

        // 🔹 Lấy toàn bộ danh sách tin
        public async Task<List<News>> GetAllNewsAsync()
        {
            try
            {
                return await _context.News
                    .Include(n => n.User) // ✅ lấy cả thông tin người đăng
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while retrieving all news: {ex.Message}");
                return new List<News>();
            }
        }

        // 🔹 Lấy tin theo ID
        public async Task<News?> GetNewsByIdAsync(int newsId)
        {
            try
            {
                return await _context.News
                    .Include(n => n.User)
                    .FirstOrDefaultAsync(n => n.Id == newsId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while retrieving news ID {newsId}: {ex.Message}");
                return null;
            }
        }

        // 🔹 Lấy tin theo User
        public async Task<List<News>> GetNewsByUserIdAsync(int userId)
        {
            try
            {
                return await _context.News
                    .Where(n => n.UserId == userId)
                    .Include(n => n.User)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while retrieving news by user ID {userId}: {ex.Message}");
                return new List<News>();
            }
        }

        // 🔹 Lấy tin theo trạng thái (NewsStatus)
        public async Task<List<News>> GetNewsByStatusAsync(byte status)
        {
            try
            {
                return await _context.News
                    .Where(n => n.NewsStatus == (Domain.Enums.NewsStatus)status)
                    .Include(n => n.User)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error while retrieving news by status {status}: {ex.Message}");
                return new List<News>();
            }
        }
    }
}
