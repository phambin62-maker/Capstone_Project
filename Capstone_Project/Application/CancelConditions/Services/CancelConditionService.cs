using BE_Capstone_Project.Application.CancelConditions.DTOs;
using BE_Capstone_Project.Application.CancelConditions.Services.Interfaces;
using BE_Capstone_Project.DAO;
using BE_Capstone_Project.Domain.Enums;
using BE_Capstone_Project.Domain.Models;

namespace BE_Capstone_Project.Application.CancelConditions.Services
{
    public class CancelConditionService : ICancelConditionService
    {
        private readonly CancelConditionDAO _dao;

        public CancelConditionService(CancelConditionDAO dao)
        {
            _dao = dao;
        }

        public async Task<List<CancelConditionDTO>> GetAllAsync()
        {
            var list = await _dao.GetAllCancelConditionsAsync();
            return list.Select(c => new CancelConditionDTO
            {
                Id = c.Id,
                Title = c.Title,
                MinDaysBeforeTrip = c.MinDaysBeforeTrip,
                RefundPercent = c.RefundPercent,
                CreatedDate = c.CreatedDate,
                CancelStatus = c.CancelStatus
            }).ToList();
        }

        public async Task<CancelConditionDTO?> GetByIdAsync(int id)
        {
            var c = await _dao.GetCancelConditionByIdAsync(id);
            if (c == null) return null;

            return new CancelConditionDTO
            {
                Id = c.Id,
                Title = c.Title,
                MinDaysBeforeTrip = c.MinDaysBeforeTrip,
                RefundPercent = c.RefundPercent,
                CreatedDate = c.CreatedDate,
                CancelStatus = c.CancelStatus
            };
        }

        public async Task<List<CancelConditionDTO>> GetByStatusAsync(CancelStatus status)
        {
            var list = await _dao.GetCancelConditionsByStatusAsync(status);
            return list.Select(c => new CancelConditionDTO
            {
                Id = c.Id,
                Title = c.Title,
                MinDaysBeforeTrip = c.MinDaysBeforeTrip,
                RefundPercent = c.RefundPercent,
                CreatedDate = c.CreatedDate,
                CancelStatus = c.CancelStatus
            }).ToList();
        }

        public async Task<int> CreateAsync(CancelConditionCreateDTO dto)
        {
            var model = new CancelCondition
            {
                Title = dto.Title,
                MinDaysBeforeTrip = dto.MinDaysBeforeTrip,
                RefundPercent = dto.RefundPercent,
                CancelStatus = dto.CancelStatus,
                CreatedDate = DateTime.Now
            };
            return await _dao.AddCancelConditionAsync(model);
        }

        public async Task<bool> UpdateAsync(CancelConditionUpdateDTO dto)
        {
            var model = await _dao.GetCancelConditionByIdAsync(dto.Id);
            if (model == null) return false;

            model.Title = dto.Title;
            model.MinDaysBeforeTrip = dto.MinDaysBeforeTrip;
            model.RefundPercent = dto.RefundPercent;
            model.CancelStatus = dto.CancelStatus;

            return await _dao.UpdateCancelConditionAsync(model);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _dao.DeleteCancelConditionByIdAsync(id);
        }
    }
}
