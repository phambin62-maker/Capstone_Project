using BE_Capstone_Project.Application.Newses.DTOs;
using BE_Capstone_Project.DAO;
using BE_Capstone_Project.Domain.Enums;
using BE_Capstone_Project.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;

namespace BE_Capstone_Project.Application.Newses.Services
{
    public class NewsService
    {
        private readonly NewsDAO _newsDAO;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NewsService(NewsDAO newsDAO, IHttpContextAccessor httpContextAccessor)
        {
            _newsDAO = newsDAO;
            _httpContextAccessor = httpContextAccessor;
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
                UserId = n.UserId
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

        public async Task<IEnumerable<NewsDTO>> GetByStatusAsync(NewsStatus status)
        {
            var newsList = await _newsDAO.GetNewsByStatusAsync(status);
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
                NewsStatus = dto.NewsStatus ?? NewsStatus.Draft
            };
            return await _newsDAO.AddNewsAsync(news);
        }

        public async Task<bool> DeleteAsync(News newsEntity)
        {
            return await _newsDAO.DeleteNewsAsync(newsEntity);
        }
        public async Task<News?> GetNewsEntityByIdAsync(int id)
        {
            return await _newsDAO.GetNewsEntityByIdAsync(id);
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
    }
}