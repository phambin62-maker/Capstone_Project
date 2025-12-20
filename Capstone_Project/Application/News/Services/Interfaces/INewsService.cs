using BE_Capstone_Project.Application.Newses.DTOs;
using BE_Capstone_Project.Domain.Models; 
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BE_Capstone_Project.Application.Newses.Services.Interfaces
{
    public interface INewsService
    {
        Task<IEnumerable<NewsDTO>> GetAllAsync();
        Task<NewsDTO?> GetByIdAsync(int id);
        Task<IEnumerable<NewsDTO>> GetByStatusAsync(string statusString);
        Task<NewsStatsDTO> GetNewsStatsAsync();
        Task<IEnumerable<NewsDTO>> GetRecentAsync(int userId);
        Task<int> CreateAsync(CreateNewsDTO dto);

        Task<bool> UpdateAsync(News newsEntity);
        Task<bool> DeleteAsync(News newsEntity);

        Task<News?> GetNewsEntityByIdAsync(int id);
    }
}