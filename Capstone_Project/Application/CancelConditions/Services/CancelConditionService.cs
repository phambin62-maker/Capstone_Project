using BE_Capstone_Project.Application.CancelConditions.DTOs;
using BE_Capstone_Project.Application.CancelConditions.Services.Interfaces;
using BE_Capstone_Project.DAO;
using BE_Capstone_Project.Domain.Enums;
using BE_Capstone_Project.Domain.Models;
using BE_Capstone_Project.Application.Admin.DTOs;

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

        public async Task<PagedResultDto<CancelConditionDTO>> GetPagingAsync(string keyword, int pageIndex, int pageSize)
        {
            var (list, totalCount) = await _dao.GetCancelConditionsPagingAsync(keyword, pageIndex, pageSize);

            var dtos = list.Select(c => new CancelConditionDTO
            {
                Id = c.Id,
                Title = c.Title,
                MinDaysBeforeTrip = c.MinDaysBeforeTrip,
                RefundPercent = c.RefundPercent,
                CreatedDate = c.CreatedDate,
                CancelStatus = c.CancelStatus
            }).ToList();

            return new PagedResultDto<CancelConditionDTO>
            {
                Data = dtos,
                TotalCount = totalCount,
                Page = pageIndex,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
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
            // Check for duplicate title
            var existing = (await _dao.GetAllCancelConditionsAsync())
                            .FirstOrDefault(c => c.Title.ToLower() == dto.Title.ToLower());
            
            if (existing != null)
            {
                throw new InvalidOperationException($"Cancel condition with title '{dto.Title}' already exists.");
            }

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

            // Check for duplicate title (excluding self)
            var existing = (await _dao.GetAllCancelConditionsAsync())
                            .FirstOrDefault(c => c.Title.ToLower() == dto.Title.ToLower() && c.Id != dto.Id);

            if (existing != null)
            {
                throw new InvalidOperationException($"Cancel condition with title '{dto.Title}' already exists.");
            }

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
