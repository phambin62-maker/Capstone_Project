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
    }
}
