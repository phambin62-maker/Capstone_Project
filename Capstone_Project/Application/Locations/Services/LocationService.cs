using BE_Capstone_Project.Application.Locations.DTOs;
using BE_Capstone_Project.Application.Locations.Services.Interfaces;
using BE_Capstone_Project.DAO;
using BE_Capstone_Project.Domain.Models;

namespace BE_Capstone_Project.Application.Locations.Services
{
    public class LocationService : ILocationService
    {
        private readonly LocationDAO _locationDAO;

        public LocationService(LocationDAO locationDAO)
        {
            _locationDAO = locationDAO;
        }

        public async Task<ServiceResult<LocationDTO>> GetLocationByIdAsync(int id)
        {
            try
            {
                var location = await _locationDAO.GetLocationByIdAsync(id);
                if (location == null)
                {
                    return new ServiceResult<LocationDTO>
                    {
                        Success = false,
                        Message = "Location not found"
                    };
                }

                var locationDto = new LocationDTO
                {
                    Id = location.Id,
                    LocationName = location.LocationName
                };

                return new ServiceResult<LocationDTO>
                {
                    Success = true,
                    Data = locationDto
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<LocationDTO>
                {
                    Success = false,
                    Message = $"Error retrieving location: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResult<List<LocationDTO>>> GetAllLocationsAsync()
        {
            try
            {
                var locations = await _locationDAO.GetAllLocationsAsync();
                var locationDtos = locations.Select(l => new LocationDTO
                {
                    Id = l.Id,
                    LocationName = l.LocationName
                }).ToList();

                return new ServiceResult<List<LocationDTO>>
                {
                    Success = true,
                    Data = locationDtos
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<List<LocationDTO>>
                {
                    Success = false,
                    Message = $"Error retrieving locations: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResult<int>> CreateLocationAsync(CreateLocationDTO createDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(createDto.LocationName))
                {
                    return new ServiceResult<int>
                    {
                        Success = false,
                        Message = "Location name is required"
                    };
                }

                var location = new Location
                {
                    LocationName = createDto.LocationName
                };

                var locationId = await _locationDAO.AddLocationAsync(location);

                if (locationId > 0)
                {
                    return new ServiceResult<int>
                    {
                        Success = true,
                        Data = locationId,
                        Message = "Location created successfully"
                    };
                }
                else
                {
                    return new ServiceResult<int>
                    {
                        Success = false,
                        Message = "Failed to create location"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ServiceResult<int>
                {
                    Success = false,
                    Message = $"Error creating location: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResult<bool>> UpdateLocationAsync(int id, UpdateLocationDTO updateDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(updateDto.LocationName))
                {
                    return new ServiceResult<bool>
                    {
                        Success = false,
                        Message = "Location name is required"
                    };
                }

                var existingLocation = await _locationDAO.GetLocationByIdAsync(id);
                if (existingLocation == null)
                {
                    return new ServiceResult<bool>
                    {
                        Success = false,
                        Message = "Location not found"
                    };
                }

                existingLocation.LocationName = updateDto.LocationName;
                var result = await _locationDAO.UpdateLocationAsync(existingLocation);

                return new ServiceResult<bool>
                {
                    Success = result,
                    Data = result,
                    Message = result ? "Location updated successfully" : "Failed to update location"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = $"Error updating location: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResult<bool>> DeleteLocationAsync(int id)
        {
            try
            {
                var result = await _locationDAO.DeleteLocationByIdAsync(id);
                return new ServiceResult<bool>
                {
                    Success = result,
                    Data = result,
                    Message = result ? "Location deleted successfully" : "Location not found or could not be deleted"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = $"Error deleting location: {ex.Message}"
                };
            }
        }
    }
}
