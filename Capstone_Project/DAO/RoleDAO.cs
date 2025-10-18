using BE_Capstone_Project.Infrastructure;
using BE_Capstone_Project.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BE_Capstone_Project.DAO
{
    public class RoleDAO
    {
        private readonly OtmsdbContext _context;
        public RoleDAO(OtmsdbContext context)
        {
            _context = context;
        }
        public async Task<int> AddRoleAsync(Role role)
        {
            try
            {
                await _context.Roles.AddAsync(role);
                await _context.SaveChangesAsync();
                return role.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while adding a role: {ex.Message}");
                return -1;
            }
        }
        public async Task<bool> UpdateRoleAsync(Role role)
        {
            try
            {
                _context.Roles.Update(role);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while updating the role with ID {role.Id}: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> DeleteRoleByIdAsync(int roleId)
        {
            try
            {
                var role = await _context.Roles.FindAsync(roleId);
                if (role != null)
                {
                    _context.Roles.Remove(role);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting the role with ID {roleId}: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            try
            {
                return await _context.Roles.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving all roles: {ex.Message}");
                return new List<Role>();
            }
        }

        public async Task<Role?> GetRoleByIdAsync(int roleId)
        {
            try
            {
                return await _context.Roles.FindAsync(roleId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving the role with ID {roleId}: {ex.Message}");
                return null;
            }
        }
    }
}
