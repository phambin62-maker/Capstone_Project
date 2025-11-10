using BE_Capstone_Project.Application.Company.DTOs;

namespace BE_Capstone_Project.Application.Company.Services.Interfaces
{
    public interface IFeatureService
    {
        Task<List<FeatureDTO>> GetActiveFeaturesAsync();
        Task<FeatureDTO?> GetFeatureByIdAsync(int id);
        Task<List<FeatureDTO>> GetAllFeaturesAsync();
        Task<int> AddFeatureAsync(FeatureDTO feature);
        Task<bool> UpdateFeatureAsync(int id, FeatureDTO feature);
        Task<bool> DeleteFeatureAsync(int id);
    }
}

