using BE_Capstone_Project.Application.Locations.DTOs;

namespace BE_Capstone_Project.Application.Locations.Services.Interfaces
{
    public interface ILocationService
    {
        Task<ServiceResult<LocationDTO>> GetLocationByIdAsync(int id);
        Task<ServiceResult<List<LocationDTO>>> GetAllLocationsAsync();
        Task<ServiceResult<int>> CreateLocationAsync(CreateLocationDTO createDto);
        Task<ServiceResult<bool>> UpdateLocationAsync(int id, UpdateLocationDTO updateDto);
        Task<ServiceResult<bool>> DeleteLocationAsync(int id);
    }
    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}
