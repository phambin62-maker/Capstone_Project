using BE_Capstone_Project.Domain.Models;
using BE_Capstone_Project.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BE_Capstone_Project.DAO
{
    public class CompanyDAO
    {
        private readonly OtmsdbContext _context;

        public CompanyDAO(OtmsdbContext context)
        {
            _context = context;
        }

        public async Task<Company?> GetActiveCompanyAsync()
        {
            return await _context.Companies
                .Where(c => c.IsActive)
                .FirstOrDefaultAsync();
        }

        public async Task<Company?> GetCompanyByIdAsync(int companyId)
        {
            return await _context.Companies
                .FirstOrDefaultAsync(c => c.CompanyID == companyId);
        }

        public async Task<List<Company>> GetAllCompaniesAsync()
        {
            return await _context.Companies.ToListAsync();
        }
    }
}

