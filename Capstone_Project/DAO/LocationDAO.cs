using BE_Capstone_Project.Infrastructure;
using BE_Capstone_Project.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BE_Capstone_Project.DAO
{
    public class LocationDAO
    {
        private readonly OtmsdbContext _context;
        public LocationDAO(OtmsdbContext context)
        {
            _context = context;
        }

        public async Task<int> AddLocationAsync(Location location)
        {
            try
            {
                await _context.Locations.AddAsync(location);
                await _context.SaveChangesAsync();
                return location.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while adding a location: {ex.Message}");
                return -1;
            }
        }

        public async Task<bool> UpdateLocationAsync(Location location)
        {
            try
            {
                _context.Locations.Update(location);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while updating the location with ID {location.Id}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteLocationByIdAsync(int locationId)
        {
            try
            {
                var location = await _context.Locations.FindAsync(locationId);
                if (location != null)
                {
                    _context.Locations.Remove(location);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting the location with ID {locationId}: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Location>> GetAllLocationsAsync()
        {
            try
            {
                return await _context.Locations.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving all locations: {ex.Message}");
                return new List<Location>();
            }
        }
        
        public async Task<Location?> GetLocationByIdAsync(int locationId)
        {
            try
            {
                return await _context.Locations.FindAsync(locationId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving the location with ID {locationId}: {ex.Message}");
                return null;
            }
        }
    }
}
