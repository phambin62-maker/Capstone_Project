using BE_Capstone_Project.Application.LocationManagement.Services.Interfaces;
using BE_Capstone_Project.DAO;
using BE_Capstone_Project.Domain.Models;
using BE_Capstone_Project.Infrastructure;

namespace BE_Capstone_Project.Application.LocationManagement.Services
{
    public class LocationService : ILocationService
    {
        public readonly LocationDAO _locationDAO;

        public LocationService(LocationDAO locationDAO)
        {
            _locationDAO = locationDAO;
        }

        public async Task<List<Location>> GetAllLocations()
        {
            return await _locationDAO.GetAllLocationsAsync();
        }
    }
}
