using BE_Capstone_Project.Application.Newses.DTOs;
using BE_Capstone_Project.DAO;
using BE_Capstone_Project.Domain.Enums;
using BE_Capstone_Project.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BE_Capstone_Project.Application.Newses.Services
{
    public class NewsService
    {
        private readonly NewsDAO _newsDAO;

        public NewsService(NewsDAO newsDAO)
        {
            _newsDAO = newsDAO;
        }

        public async Task<IEnumerable<NewsDTO>> GetAllAsync()
        {
            var newsList = await _newsDAO.GetAllNewsAsync();
            return newsList.Select(n => new NewsDTO
            {
                Id = n.Id,
                Title = n.Title,
                Image = n.Image,
                CreatedDate = n.CreatedDate,
                NewsStatus = n.NewsStatus,
                AuthorName = n.User?.Username,
                Content = n.Content
            });
        }

        public async Task<NewsDTO?> GetByIdAsync(int id)
        {
            var n = await _newsDAO.GetNewsByIdAsync(id);
            if (n == null) return null;

            return new NewsDTO
            {
                Id = n.Id,
                Title = n.Title,
                Image = n.Image,
                CreatedDate = n.CreatedDate,
                NewsStatus = n.NewsStatus,
                AuthorName = n.User?.Username,
                Content = n.Content
            };
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

        public async Task<bool> UpdateAsync(int id, CreateNewsDTO dto)
        {
            var existing = await _newsDAO.GetNewsByIdAsync(id);
            if (existing == null)
                return false;

            existing.Title = dto.Title;
            existing.Content = dto.Content;
            existing.Image = dto.Image;
            existing.NewsStatus = dto.NewsStatus ?? existing.NewsStatus;

            return await _newsDAO.UpdateNewsAsync(existing);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _newsDAO.DeleteNewsByIdAsync(id);
        }

        public async Task<IEnumerable<NewsDTO>> GetByStatusAsync(NewsStatus status)
        {
            var newsList = await _newsDAO.GetNewsByStatusAsync(status);
            return newsList.Select(n => new NewsDTO
            {
                Id = n.Id,
                Title = n.Title,
                Image = n.Image,
                CreatedDate = n.CreatedDate,
                NewsStatus = n.NewsStatus,
                AuthorName = n.User?.Username,
                Content = n.Content
            });
        }
    }
}