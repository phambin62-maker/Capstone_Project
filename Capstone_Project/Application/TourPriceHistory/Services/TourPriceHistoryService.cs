using BE_Capstone_Project.Application.TourPriceHistories.DTOs;
using BE_Capstone_Project.DAO;
using BE_Capstone_Project.Domain.Models;

namespace BE_Capstone_Project.Application.TourPriceHistories.Services
{
    public class TourPriceHistoryService
    {
        private readonly TourPriceHistoryDAO _dao;

        public TourPriceHistoryService(TourPriceHistoryDAO dao)
        {
            _dao = dao;
        }

        public async Task<IEnumerable<TourPriceHistoryDTO>> GetAllAsync()
        {
            var list = await _dao.GetAllTourPriceHistoriesAsync();
            return list.Select(t => new TourPriceHistoryDTO
            {
                Id = t.Id,
                TourId = t.TourId,
                Price = t.Price,
                ChildrenDiscount = t.ChildrenDiscount,
                GroupDiscount = t.GroupDiscount,
                GroupNumber = t.GroupNumber,
                StartPriceDate = t.StartPriceDate,
                EndPriceDate = t.EndPriceDate,
                UpdatedDate = t.UpdatedDate
            });
        }

        public async Task<TourPriceHistoryDTO?> GetByIdAsync(int id)
        {
            var tph = await _dao.GetTourPriceHistoryByIdAsync(id);
            if (tph == null) return null;

            return new TourPriceHistoryDTO
            {
                Id = tph.Id,
                TourId = tph.TourId,
                Price = tph.Price,
                ChildrenDiscount = tph.ChildrenDiscount,
                GroupDiscount = tph.GroupDiscount,
                GroupNumber = tph.GroupNumber,
                StartPriceDate = tph.StartPriceDate,
                EndPriceDate = tph.EndPriceDate,
                UpdatedDate = tph.UpdatedDate
            };
        }

        public async Task<int> CreateAsync(CreateTourPriceHistoryDTO dto)
        {
            var tph = new TourPriceHistory
            {
                TourId = dto.TourId,
                Price = dto.Price,
                ChildrenDiscount = dto.ChildrenDiscount,
                GroupDiscount = dto.GroupDiscount,
                GroupNumber = dto.GroupNumber,
                StartPriceDate = dto.StartPriceDate,
                EndPriceDate = dto.EndPriceDate,
                UpdatedDate = DateTime.UtcNow
            };

            return await _dao.AddTourPriceHistoryAsync(tph);
        }

        public async Task<bool> UpdateAsync(int id, CreateTourPriceHistoryDTO dto)
        {
            var existing = await _dao.GetTourPriceHistoryByIdAsync(id);
            if (existing == null) return false;

            existing.Price = dto.Price;
            existing.ChildrenDiscount = dto.ChildrenDiscount;
            existing.GroupDiscount = dto.GroupDiscount;
            existing.GroupNumber = dto.GroupNumber;
            existing.StartPriceDate = dto.StartPriceDate;
            existing.EndPriceDate = dto.EndPriceDate;
            existing.UpdatedDate = DateTime.UtcNow;

            return await _dao.UpdateTourPriceHistoryAsync(existing);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _dao.DeleteTourPriceHistoryByIdAsync(id);
        }

        public async Task<IEnumerable<TourPriceHistoryDTO>> GetByTourIdAsync(int tourId)
        {
            var list = await _dao.GetTourPriceHistoriesByTourIdAsync(tourId);
            return list.Select(t => new TourPriceHistoryDTO
            {
                Id = t.Id,
                TourId = t.TourId,
                Price = t.Price,
                ChildrenDiscount = t.ChildrenDiscount,
                GroupDiscount = t.GroupDiscount,
                GroupNumber = t.GroupNumber,
                StartPriceDate = t.StartPriceDate,
                EndPriceDate = t.EndPriceDate,
                UpdatedDate = t.UpdatedDate
            });
        }
    }
}
