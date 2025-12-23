using BE_Capstone_Project.Infrastructure;
using BE_Capstone_Project.Domain.Models;
using Microsoft.EntityFrameworkCore;
using BE_Capstone_Project.Domain.Enums;

namespace BE_Capstone_Project.DAO
{
    public class CancelConditionDAO
    {
        private readonly OtmsdbContext _context;
        public CancelConditionDAO(OtmsdbContext context)
        {
            _context = context;
        }
        public async Task<int> AddCancelConditionAsync(CancelCondition cancelCondition)
        {
            try
            {
                await _context.CancelConditions.AddAsync(cancelCondition);
                await _context.SaveChangesAsync();
                return cancelCondition.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while adding a cancel condition: {ex.Message}");
                return -1;
            }
        }
        public async Task<bool> UpdateCancelConditionAsync(CancelCondition cancelCondition)
        {
            try
            {
                _context.CancelConditions.Update(cancelCondition);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while updating the cancel condition with ID {cancelCondition.Id}: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> DeleteCancelConditionByIdAsync(int cancelConditionId)
        {
            try
            {
                var cancelCondition = await _context.CancelConditions.FindAsync(cancelConditionId);
                if (cancelCondition != null)
                {
                    _context.CancelConditions.Remove(cancelCondition);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting the cancel condition with ID {cancelConditionId}: {ex.Message}");
                return false;
            }
        }

        public async Task<List<CancelCondition>> GetAllCancelConditionsAsync()
        {
            try
            {
                return await _context.CancelConditions
                    .Include(cc => cc.Tours)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving all cancel conditions: {ex.Message}");
                return new List<CancelCondition>();
            }
        }

        public async Task<CancelCondition?> GetCancelConditionByIdAsync(int cancelConditionId)
        {
            try
            {
                return await _context.CancelConditions
                    .Include(cc => cc.Tours)
                    .FirstOrDefaultAsync(cc => cc.Id == cancelConditionId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving the cancel condition with ID {cancelConditionId}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<CancelCondition>> GetCancelConditionsByStatusAsync(CancelStatus status)
        {
            try
            {
                return await _context.CancelConditions
                    .Where(cc => cc.CancelStatus == status)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving cancel conditions by status {status}: {ex.Message}");
                return new List<CancelCondition>();
            }
        }

        public async Task<List<CancelCondition>> GetCancelConditionsByTourIdAsync(int tourId)
        {
            try
            {
                return await _context.Tours
                    .Where(t => t.Id == tourId)
                    .Select(t => t.CancelCondition)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving cancel conditions for tour ID {tourId}: {ex.Message}");
                return new List<CancelCondition>();
            }
        }
        public async Task<(List<CancelCondition>, int)> GetCancelConditionsPagingAsync(string keyword, int pageIndex, int pageSize)
        {
            try
            {
                var query = _context.CancelConditions.AsQueryable();

                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(cc => cc.Title.Contains(keyword));
                }

                int totalCount = await query.CountAsync();

                var list = await query
                    .Include(cc => cc.Tours)
                    .OrderByDescending(cc => cc.Id)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (list, totalCount);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving paged cancel conditions: {ex.Message}");
                return (new List<CancelCondition>(), 0);
            }
        }
    }
}
