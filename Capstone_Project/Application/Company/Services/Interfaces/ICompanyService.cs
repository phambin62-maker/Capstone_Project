using BE_Capstone_Project.Application.Company.DTOs;

namespace BE_Capstone_Project.Application.Company.Services.Interfaces
{
    public interface ICompanyService
    {
        Task<CompanyDTO?> GetActiveCompanyAsync();
        Task<CompanyDTO?> GetCompanyByIdAsync(int companyId);
        Task<List<CompanyDTO>> GetAllCompaniesAsync();
    }
}

