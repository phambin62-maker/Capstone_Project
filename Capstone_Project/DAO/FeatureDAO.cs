using BE_Capstone_Project.Domain.Models;
using BE_Capstone_Project.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BE_Capstone_Project.DAO
{
    public class FeatureDAO
    {
        private readonly OtmsdbContext _context;

        public FeatureDAO(OtmsdbContext context)
        {
            _context = context;
        }

        public async Task<List<Feature>> GetActiveFeaturesAsync()
        {
            return await _context.Features
                .Where(f => f.IsActive)
                .OrderBy(f => f.DisplayOrder)
                .ThenBy(f => f.Id)
                .ToListAsync();
        }

        public async Task<Feature?> GetFeatureByIdAsync(int id)
        {
            return await _context.Features
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<List<Feature>> GetAllFeaturesAsync()
        {
            return await _context.Features
                .OrderBy(f => f.DisplayOrder)
                .ThenBy(f => f.Id)
                .ToListAsync();
        }

        public async Task<int> AddFeatureAsync(Feature feature)
        {
            try
            {
                await _context.Features.AddAsync(feature);
                await _context.SaveChangesAsync();
                return feature.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding feature: {ex.Message}");
                return -1;
            }
        }

        public async Task<bool> UpdateFeatureAsync(Feature feature)
        {
            try
            {
                _context.Features.Update(feature);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating feature: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteFeatureAsync(int id)
        {
            try
            {
                var feature = await _context.Features.FindAsync(id);
                if (feature == null) return false;

                _context.Features.Remove(feature);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting feature: {ex.Message}");
                return false;
            }
        }
    }
}

