using BE_Capstone_Project.Domain.Enums;
using BE_Capstone_Project.Domain.Models;
using BE_Capstone_Project.Infrastructure;
using BE_Capstone_Project.Application.Newses.DTOs; // Thêm DTO
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BE_Capstone_Project.DAO
{
    public class NewsDAO
    {
        private readonly OtmsdbContext _context;
        public NewsDAO(OtmsdbContext context)
        {
            _context = context;
        }

        // === CÁC HÀM "VIẾT" (WRITE) ===

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

        public async Task<bool> DeleteNewsAsync(News newsEntity)
        {
            try
            {
                _context.News.Remove(newsEntity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting the news with ID {newsEntity.Id}: {ex.Message}");
                return false;
            }
        }

        // Hàm này (dùng cho Service) chỉ gọi SaveChanges (để sửa lỗi Update)
        public async Task<bool> UpdateNewsAsync(News news)
        {
            try
            {
                // Không gọi _context.Update(news)
                // vì entity đã được theo dõi (tracked) bởi Controller
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while updating the news with ID {news.Id}: {ex.Message}");
                return false;
            }
        }

        // Hàm này (dùng cho Service)
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        // === CÁC HÀM "ĐỌC" (READ) - LUÔN DÙNG .AsNoTracking() ===

        public async Task<List<News>> GetAllNewsAsync()
        {
            try
            {
                return await _context.News
                    .Include(n => n.User) // Sửa: Thêm .Include
                    .AsNoTracking()       // Sửa: Thêm AsNoTracking
                    .OrderByDescending(n => n.CreatedDate)
                    .ToListAsync();
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
                return await _context.News
                    .Include(n => n.User) // Sửa: Thêm .Include
                    .AsNoTracking()       // Sửa: Thêm AsNoTracking
                    .FirstOrDefaultAsync(n => n.Id == newsId);
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
                    .Include(n => n.User) // Sửa: Thêm .Include
                    .AsNoTracking()       // Sửa: Thêm AsNoTracking
                    .Where(n => n.UserId == userId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving news for user ID {userId}: {ex.Message}");
                return new List<News>();
            }
        }

        public async Task<List<News>> GetNewsByStatusAsync(NewsStatus status)
        {
            try
            {
                return await _context.News
                    .Include(n => n.User) // Sửa: Thêm .Include
                    .AsNoTracking()       // Sửa: Thêm AsNoTracking
                    .Where(n => n.NewsStatus == status)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving news for status {status}: {ex.Message}");
                return new List<News>();
            }
        }

        // (Hàm này dùng cho Stats Cards - Đã sửa)
        public async Task<NewsStatsDTO> GetNewsStatsAsync()
        {
            var stats = new NewsStatsDTO();
            stats.Total = await _context.News.AsNoTracking().CountAsync();
            stats.Published = await _context.News.AsNoTracking().CountAsync(n => n.NewsStatus == NewsStatus.Published);
            stats.Draft = await _context.News.AsNoTracking().CountAsync(n => n.NewsStatus == NewsStatus.Draft);
            stats.Hidden = await _context.News.AsNoTracking().CountAsync(n => n.NewsStatus == NewsStatus.Hidden);
            return stats;
        }

        // (Hàm này dùng cho Lỗi Hiệu Năng - Đã sửa)
        public async Task<List<News>> GetRecentNewsByUserIdAsync(int userId, int count = 5)
        {
            try
            {
                return await _context.News
                    .Include(n => n.User)
                    .Where(n => n.UserId == userId)
                    .AsNoTracking()
                    .OrderByDescending(n => n.CreatedDate)
                    .Take(count) // Chỉ lấy 5
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving recent news for user {userId}: {ex.Message}");
                return new List<News>();
            }
        }

        // (Hàm này dùng cho Lỗi Update - KHÔNG DÙNG .AsNoTracking())
        public async Task<News?> GetNewsEntityByIdAsync(int newsId)
        {
            try
            {
                return await _context.News
                    .Include(n => n.User)
                    .FirstOrDefaultAsync(n => n.Id == newsId); // (Không có .AsNoTracking())
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving the news entity with ID {newsId}: {ex.Message}");
                return null;
            }
        }
    }
}