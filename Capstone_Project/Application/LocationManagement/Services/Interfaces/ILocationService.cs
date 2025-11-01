using BE_Capstone_Project.Domain.Models;

namespace BE_Capstone_Project.Application.LocationManagement.Services.Interfaces
{
    public interface ILocationService
    {
        Task<List<Location>> GetAllLocations();
    }
}
