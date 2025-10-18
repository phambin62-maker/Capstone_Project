using BE_Capstone_Project.Application.Tour.DTOs;
using BE_Capstone_Project.DAO;
using BE_Capstone_Project.Domain.Enums;
using BE_Capstone_Project.Domain.Models;
using BE_Capstone_Project.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BE_Capstone_Project.Application.Tour.Services
{
    public class TourService
    {
        private readonly TourDAO _tourDAO;
        private readonly OtmsdbContext _context;

        public TourService(OtmsdbContext context)
        {
            _context = context;
            _tourDAO = new TourDAO(context); 
        }

        // 🔹 Lấy tất cả tour (có thông tin liên kết)
        public async Task<IEnumerable<TourDTO>> GetAllAsync()
        {
            var tours = await _context.Tours
                .Include(t => t.StartLocation)
                .Include(t => t.EndLocation)
                .Include(t => t.Category)
                .Include(t => t.CancelCondition)
                .ToListAsync();

            return tours.Select(t => new TourDTO
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Price = t.Price,
                Duration = t.Duration,
                StartLocationId = t.StartLocationId,
                EndLocationId = t.EndLocationId,
                CategoryId = t.CategoryId,
                CancelConditionId = t.CancelConditionId,
                ChildDiscount = t.ChildDiscount,
                GroupDiscount = t.GroupDiscount,
                GroupNumber = t.GroupNumber,
                MinSeats = t.MinSeats,
                MaxSeats = t.MaxSeats,
                TourStatus = t.TourStatus,
                StartLocationName = t.StartLocation?.LocationName,
                EndLocationName = t.EndLocation?.LocationName,
                CategoryName = t.Category?.CategoryName,
                CancelConditionTitle = t.CancelCondition?.Title
            }).ToList();
        }

        // 🔹 Lấy tour theo ID
        public async Task<TourDTO?> GetByIdAsync(int id)
        {
            var tour = await _context.Tours
                .Include(t => t.StartLocation)
                .Include(t => t.EndLocation)
                .Include(t => t.Category)
                .Include(t => t.CancelCondition)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tour == null) return null;

            return new TourDTO
            {
                Id = tour.Id,
                Name = tour.Name,
                Description = tour.Description,
                Price = tour.Price,
                Duration = tour.Duration,
                StartLocationId = tour.StartLocationId,
                EndLocationId = tour.EndLocationId,
                CategoryId = tour.CategoryId,
                CancelConditionId = tour.CancelConditionId,
                ChildDiscount = tour.ChildDiscount,
                GroupDiscount = tour.GroupDiscount,
                GroupNumber = tour.GroupNumber,
                MinSeats = tour.MinSeats,
                MaxSeats = tour.MaxSeats,
                TourStatus = tour.TourStatus,
                StartLocationName = tour.StartLocation?.LocationName,
                EndLocationName = tour.EndLocation?.LocationName,
                CategoryName = tour.Category?.CategoryName,
                CancelConditionTitle = tour.CancelCondition?.Title
            };
        }

        // 🔹 Thêm tour mới
        public async Task<int> CreateAsync(CreateTourDTO dto)
        {
            var newTour = new BE_Capstone_Project.Domain.Models.Tour
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Duration = dto.Duration,
                StartLocationId = dto.StartLocationId,
                EndLocationId = dto.EndLocationId,
                CategoryId = dto.CategoryId,
                CancelConditionId = dto.CancelConditionId,
                ChildDiscount = dto.ChildDiscount,
                GroupDiscount = dto.GroupDiscount,
                GroupNumber = dto.GroupNumber,
                MinSeats = dto.MinSeats,
                MaxSeats = dto.MaxSeats,
                
            };

            return await _tourDAO.AddTourAsync(newTour);
        }
    


// 🔹 Cập nhật tour
public async Task<bool> UpdateAsync(int id, CreateTourDTO dto)
        {
            var existingTour = await _tourDAO.GetTourByIdAsync(id);
            if (existingTour == null) return false;

            existingTour.Name = dto.Name;
            existingTour.Description = dto.Description;
            existingTour.Price = dto.Price;
            existingTour.Duration = dto.Duration;
            existingTour.StartLocationId = dto.StartLocationId;
            existingTour.EndLocationId = dto.EndLocationId;
            existingTour.CategoryId = dto.CategoryId;
            existingTour.CancelConditionId = dto.CancelConditionId;
            existingTour.ChildDiscount = dto.ChildDiscount;
            existingTour.GroupDiscount = dto.GroupDiscount;
            existingTour.GroupNumber = dto.GroupNumber;
            existingTour.MinSeats = dto.MinSeats;
            existingTour.MaxSeats = dto.MaxSeats;

            return await _tourDAO.UpdateTourAsync(existingTour);
        }

        // 🔹 Xóa tour
        public async Task<bool> DeleteAsync(int id)
        {
            return await _tourDAO.DeleteTourByIdAsync(id);
        }

        // 🔹 Tìm tour theo tên
        public async Task<List<TourDTO>> SearchByNameAsync(string name)
        {
            var tours = await _tourDAO.SearchToursByNameAsync(name);
            return tours.Select(t => new TourDTO
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Price = t.Price,
                Duration = t.Duration,
                CategoryId = t.CategoryId,
                StartLocationId = t.StartLocationId,
                EndLocationId = t.EndLocationId,
                CancelConditionId = t.CancelConditionId,
                TourStatus = t.TourStatus
            }).ToList();
        }
    }
}
