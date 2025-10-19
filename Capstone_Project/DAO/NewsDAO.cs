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
                Console.WriteLine($"An error occurred while adding news: {ex.Message}");
                return -1;
            }
        }

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
                Console.WriteLine($"An error occurred while updating the news with ID {news.Id}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteNewsByIdAsync(int newsId)
        {
            try
            {
                var news = await _context.News.FindAsync(newsId);
                if (news != null)
                {
                    _context.News.Remove(news);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting the news with ID {newsId}: {ex.Message}");
                return false;
            }
        }

        public async Task<List<News>> GetAllNewsAsync()
        {
            try
            {
                return await _context.News.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving all news: {ex.Message}");
                return new List<News>();
            }
        }

        public async Task<News?> GetNewsByIdAsync(int newsId)
        {
            try
            {
                return await _context.News.FindAsync(newsId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving the news with ID {newsId}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<News>> GetNewsByUserIdAsync(int userId)
        {
            try
            {
                return await _context.News
                    .Where(n => n.UserId == userId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving news for user ID {userId}: {ex.Message}");
                return new List<News>();
            }
        }
    }
}
