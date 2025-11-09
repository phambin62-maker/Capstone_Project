using BE_Capstone_Project.Application.TourManagement.DTOs;

namespace BE_Capstone_Project.Application.TourManagement.Services.Interfaces
{
    public interface ITourScheduleService
    {
        Task<TourScheduleDTO?> GetTourScheduleById(int id);
        Task<List<TourScheduleDTO>> GetAllTourSchedules();
        Task<List<TourScheduleDTO>> GetTourSchedulesByTourId(int tourId);
        Task<int> CreateTourSchedule(CreateTourScheduleRequest request);
        Task<bool> UpdateTourSchedule(int id, UpdateTourScheduleRequest request);
        Task<bool> DeleteTourSchedule(int id);
        Task<List<TourScheduleDTO>> GetPaginatedTourSchedules(int page = 1, int pageSize = 5);
        Task<List<TourScheduleDTO>> GetPaginatedTourSchedulesByTourId(int tourId, int page = 1, int pageSize = 5);
        Task<List<TourScheduleDTO>> GetFilteredTourSchedules(
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
            int pageSize = 10);

        Task<int> GetFilteredTourScheduleCount(
            int? tourId = null,
            string? tourName = null,
            string? location = null,
            string? category = null,
            string? status = null,
            string? search = null,
            string? fromDate = null,
            string? toDate = null);
    }
}
