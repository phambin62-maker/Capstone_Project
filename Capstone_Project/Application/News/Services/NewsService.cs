using BE_Capstone_Project.Application.Newses.DTOs;
using BE_Capstone_Project.Application.Newses.Services.Interfaces;
using BE_Capstone_Project.DAO;
using BE_Capstone_Project.Domain.Enums;
using BE_Capstone_Project.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;
using BE_Capstone_Project.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BE_Capstone_Project.Application.Newses.Services
{
    public class NewsService : INewsService
    {
        private readonly NewsDAO _newsDAO;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly OtmsdbContext _context;

        public NewsService(NewsDAO newsDAO, IHttpContextAccessor httpContextAccessor, OtmsdbContext context)
        {
            _newsDAO = newsDAO;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        private string? GetFullImageUrl(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return null;
            if (relativePath.StartsWith("data:image") || relativePath.StartsWith("http"))
                return relativePath;

            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
            return $"{baseUrl}{relativePath.Replace("~/", "/")}";
        }

        private NewsStatus ParseNewsStatus(string? statusString)
        {
            if (string.IsNullOrEmpty(statusString))
            {
                return NewsStatus.Draft;
            }

            if (Enum.TryParse<NewsStatus>(statusString, true, out var statusEnum))
            {
                return statusEnum;
            }

            return NewsStatus.Draft;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdClaim, out int userId);
            return userId;
        }

        private async Task<string> GetUserNameById(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                return $"{user.FirstName} {user.LastName}";
            }
            return "Unknown";
        }

        private NewsDTO MapToDto(News n)
        {
            return new NewsDTO
            {
                Id = n.Id,
                Title = n.Title,
                Image = GetFullImageUrl(n.Image),
                CreatedDate = n.CreatedDate,
                NewsStatus = n.NewsStatus,
                AuthorName = (n.User != null) ? $"{n.User.FirstName} {n.User.LastName}" : null,
                Content = n.Content,
                UserId = n.UserId,
                UpdatedDate = n.UpdatedDate,
                UpdatedAuthor = n.UpdatedAuthor
            };
        }

        public async Task<IEnumerable<NewsDTO>> GetAllAsync()
        {
            var newsList = await _newsDAO.GetAllNewsAsync();
            return newsList.Select(MapToDto);
        }

        public async Task<NewsDTO?> GetByIdAsync(int id)
        {
            var n = await _newsDAO.GetNewsByIdAsync(id);
            if (n == null) return null;
            return MapToDto(n);
        }

        public async Task<IEnumerable<NewsDTO>> GetByStatusAsync(string statusString)
        {
            var statusEnum = ParseNewsStatus(statusString);
            var newsList = await _newsDAO.GetNewsByStatusAsync(statusEnum);
            return newsList.Select(MapToDto);
        }

        public async Task<NewsStatsDTO> GetNewsStatsAsync()
        {
            return await _newsDAO.GetNewsStatsAsync();
        }

        public async Task<IEnumerable<NewsDTO>> GetRecentAsync(int userId)
        {
            var newsList = await _newsDAO.GetRecentNewsByUserIdAsync(userId, 5);
            return newsList.Select(MapToDto);
        }

        public async Task<int> CreateAsync(CreateNewsDTO dto)
        {
            var news = new News
            {
                UserId = dto.UserId,
                Title = dto.Title,
                Content = dto.Content,
                Image = dto.Image,
                CreatedDate = DateTime.UtcNow,
                NewsStatus = ParseNewsStatus(dto.NewsStatus)
            };
            return await _newsDAO.AddNewsAsync(news);
        }

        public async Task<bool> UpdateAsync(News newsEntity)
        {
            try
            {
                await _newsDAO.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating news (Service): {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAsync(News newsEntity)
        {
            return await _newsDAO.DeleteNewsAsync(newsEntity);
        }

        public async Task<News?> GetNewsEntityByIdAsync(int id)
        {
            return await _newsDAO.GetNewsEntityByIdAsync(id);
        }
    }
}