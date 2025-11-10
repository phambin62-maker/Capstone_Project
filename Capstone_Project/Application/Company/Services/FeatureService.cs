using BE_Capstone_Project.Application.Company.DTOs;
using BE_Capstone_Project.Application.Company.Services.Interfaces;
using BE_Capstone_Project.DAO;
using BE_Capstone_Project.Domain.Models;

namespace BE_Capstone_Project.Application.Company.Services
{
    public class FeatureService : IFeatureService
    {
        private readonly FeatureDAO _featureDao;

        public FeatureService(FeatureDAO featureDao)
        {
            _featureDao = featureDao;
        }

        public async Task<List<FeatureDTO>> GetActiveFeaturesAsync()
        {
            var features = await _featureDao.GetActiveFeaturesAsync();
            return features.Select(MapToDTO).ToList();
        }

        public async Task<FeatureDTO?> GetFeatureByIdAsync(int id)
        {
            var feature = await _featureDao.GetFeatureByIdAsync(id);
            if (feature == null) return null;

            return MapToDTO(feature);
        }

        public async Task<List<FeatureDTO>> GetAllFeaturesAsync()
        {
            var features = await _featureDao.GetAllFeaturesAsync();
            return features.Select(MapToDTO).ToList();
        }

        public async Task<int> AddFeatureAsync(FeatureDTO dto)
        {
            var feature = new Feature
            {
                Icon = dto.Icon,
                Title = dto.Title,
                Description = dto.Description,
                Delay = dto.Delay,
                DisplayOrder = dto.DisplayOrder,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            return await _featureDao.AddFeatureAsync(feature);
        }

        public async Task<bool> UpdateFeatureAsync(int id, FeatureDTO dto)
        {
            var feature = await _featureDao.GetFeatureByIdAsync(id);
            if (feature == null) return false;

            feature.Icon = dto.Icon;
            feature.Title = dto.Title;
            feature.Description = dto.Description;
            feature.Delay = dto.Delay;
            feature.DisplayOrder = dto.DisplayOrder;
            feature.IsActive = dto.IsActive;
            feature.UpdatedAt = DateTime.UtcNow;

            return await _featureDao.UpdateFeatureAsync(feature);
        }

        public async Task<bool> DeleteFeatureAsync(int id)
        {
            return await _featureDao.DeleteFeatureAsync(id);
        }

        private static FeatureDTO MapToDTO(Feature feature)
        {
            return new FeatureDTO
            {
                Id = feature.Id,
                Icon = feature.Icon,
                Title = feature.Title,
                Description = feature.Description,
                Delay = feature.Delay,
                DisplayOrder = feature.DisplayOrder,
                IsActive = feature.IsActive
            };
        }
    }
}

