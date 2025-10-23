using BE_Capstone_Project.Application.TourManagement.DTOs;
using BE_Capstone_Project.Application.TourManagement.Services.Interfaces;
using BE_Capstone_Project.DAO;
using BE_Capstone_Project.Domain.Enums;
using BE_Capstone_Project.Domain.Models;

namespace BE_Capstone_Project.Application.TourManagement.Services
{
    public class TourScheduleService : ITourScheduleService
    {
        private readonly TourScheduleDAO _tourScheduleDAO;
        public TourScheduleService(TourScheduleDAO tourScheduleDAO) => _tourScheduleDAO = tourScheduleDAO;

        public async Task<TourScheduleDTO?> GetTourScheduleById(int id)
        {
            var tourSchedule = await _tourScheduleDAO.GetTourScheduleById(id);
            return MapToDTO(tourSchedule);
        }

        public async Task<List<TourScheduleDTO>> GetAllTourSchedules()
        {
            var tourSchedules = await _tourScheduleDAO.GetAllTourSchedules();
            return tourSchedules.Select(MapToDTO).ToList();
        }

        public async Task<List<TourScheduleDTO>> GetTourSchedulesByTourId(int tourId)
        {
            var tourSchedules = await _tourScheduleDAO.GetTourSchedulesByTourId(tourId);
            return tourSchedules.Select(MapToDTO).ToList();
        }

        public async Task<int> CreateTourSchedule(CreateTourScheduleRequest request)
        {
            if (request.DepartureDate >= request.ArrivalDate)
                throw new ArgumentException("Departure date must be before arrival date");

            var tourSchedule = new TourSchedule
            {
                TourId = request.TourId,
                DepartureDate = request.DepartureDate,
                ArrivalDate = request.ArrivalDate,
                ScheduleStatus = ScheduleStatus.Scheduled
            };

            return await _tourScheduleDAO.AddTourSchedule(tourSchedule);
        }

        public async Task<bool> UpdateTourSchedule(int id, UpdateTourScheduleRequest request)
        {
            if (request.DepartureDate >= request.ArrivalDate)
                throw new ArgumentException("Departure date must be before arrival date");

            var existingSchedule = await _tourScheduleDAO.GetTourScheduleById(id);
            if (existingSchedule == null)
                throw new KeyNotFoundException($"Tour schedule with ID {id} not found");

            existingSchedule.DepartureDate = request.DepartureDate;
            existingSchedule.ArrivalDate = request.ArrivalDate;
            existingSchedule.ScheduleStatus = request.ScheduleStatus;

            return await _tourScheduleDAO.UpdateTourSchedule(existingSchedule);
        }

        public async Task<bool> DeleteTourSchedule(int id)
        {
            return await _tourScheduleDAO.DeleteTourScheduleById(id);
        }

        public async Task<List<TourScheduleDTO>> GetPaginatedTourSchedules(int page, int pageSize)
        {
            var tourSchedules = await _tourScheduleDAO.GetPaginatedTourSchedules(page, pageSize);
            return tourSchedules.Select(MapToDTO).ToList();
        }

        public async Task<List<TourScheduleDTO>> GetPaginatedTourSchedulesByTourId(int tourId, int page, int pageSize)
        {
            var tourSchedules = await _tourScheduleDAO.GetPaginatedTourSchedulesByTourId(tourId, page, pageSize);
            return tourSchedules.Select(MapToDTO).ToList();
        }

        private TourScheduleDTO MapToDTO(TourSchedule tourSchedule)
        {
            if (tourSchedule == null) return null;
            return new TourScheduleDTO
            {
                Id = tourSchedule.Id,
                TourId = tourSchedule.TourId,
                DepartureDate = tourSchedule.DepartureDate,
                ArrivalDate = tourSchedule.ArrivalDate,
                ScheduleStatus = tourSchedule.ScheduleStatus,
                TourName = tourSchedule.Tour?.Name
            };
        }
    }
}
