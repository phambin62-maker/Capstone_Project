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

        public async Task<List<TourScheduleDTO>> GetPaginatedTourSchedules(int page = 1, int pageSize = 5)
        {
            var tourSchedules = await _tourScheduleDAO.GetPaginatedTourSchedules(page, pageSize);
            return tourSchedules.Select(MapToDTO).ToList();
        }

        public async Task<List<TourScheduleDTO>> GetPaginatedTourSchedulesByTourId(int tourId, int page = 1, int pageSize = 5)
        {
            var tourSchedules = await _tourScheduleDAO.GetPaginatedTourSchedulesByTourId(tourId, page, pageSize);
            return tourSchedules.Select(MapToDTO).ToList();
        }
        public async Task<List<TourScheduleDTO>> GetFilteredTourSchedules(
            int? tourId = null,
            string? tourName = null,
            string? location = null,
            string? category = null,
            string? status = null,
            string? sort = null,
            string? search = null,
            string? fromDate = null,
            string? toDate = null,
            int page = 1,
            int pageSize = 10)
        {
            try
            {
                var tourSchedules = await _tourScheduleDAO.GetFilteredTourSchedules(
                    tourId, tourName, location, category, status, sort, search, fromDate, toDate, page, pageSize);

                return tourSchedules.Select(ts => MapToDTO(ts)).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in TourScheduleService.GetFilteredTourSchedules: {ex.Message}");
                return new List<TourScheduleDTO>();
            }
        }

        public async Task<int> GetFilteredTourScheduleCount(
            int? tourId = null,
            string? tourName = null,
            string? location = null,
            string? category = null,
            string? status = null,
            string? search = null,
            string? fromDate = null,
            string? toDate = null)
        {
            try
            {
                return await _tourScheduleDAO.GetFilteredTourScheduleCount(
                    tourId, tourName, location, category, status, search, fromDate, toDate);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in TourScheduleService.GetFilteredTourScheduleCount: {ex.Message}");
                return 0;
            }
        }

        public async Task<bool> IsScheduleFullAsync(int tourScheduleId)
        {
            return await _tourScheduleDAO.IsScheduleFullAsync(tourScheduleId);
        }

        private TourScheduleDTO MapToDTO(TourSchedule tourSchedule)
        {
            if (tourSchedule == null) return null;
            return new TourScheduleDTO
            {
                Id = tourSchedule.Id,
                TourId = tourSchedule.TourId,
                TourName = tourSchedule.Tour?.Name,
                StartLocation = tourSchedule.Tour?.StartLocation?.LocationName,
                EndLocation = tourSchedule.Tour?.EndLocation?.LocationName,
                CategoryName = tourSchedule.Tour?.Category?.CategoryName,
                DepartureDate = tourSchedule.DepartureDate,
                ArrivalDate = tourSchedule.ArrivalDate,
                ScheduleStatus = tourSchedule.ScheduleStatus,

            };
        }

    }
}
