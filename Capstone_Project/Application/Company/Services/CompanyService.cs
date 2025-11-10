using BE_Capstone_Project.Application.Company.DTOs;
using BE_Capstone_Project.Application.Company.Services.Interfaces;
using BE_Capstone_Project.DAO;

namespace BE_Capstone_Project.Application.Company.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly CompanyDAO _companyDao;

        public CompanyService(CompanyDAO companyDao)
        {
            _companyDao = companyDao;
        }

        public async Task<CompanyDTO?> GetActiveCompanyAsync()
        {
            var company = await _companyDao.GetActiveCompanyAsync();
            if (company == null) return null;

            return MapToDTO(company);
        }

        public async Task<CompanyDTO?> GetCompanyByIdAsync(int companyId)
        {
            var company = await _companyDao.GetCompanyByIdAsync(companyId);
            if (company == null) return null;

            return MapToDTO(company);
        }

        public async Task<List<CompanyDTO>> GetAllCompaniesAsync()
        {
            var companies = await _companyDao.GetAllCompaniesAsync();
            return companies.Select(MapToDTO).ToList();
        }

        private static CompanyDTO MapToDTO(Domain.Models.Company company)
        {
            return new CompanyDTO
            {
                CompanyID = company.CompanyID,
                CompanyName = company.CompanyName,
                LicenseNumber = company.LicenseNumber,
                TaxCode = company.TaxCode,
                Email = company.Email,
                Phone = company.Phone,
                Website = company.Website,
                Address = company.Address,
                Description = company.Description,
                LogoUrl = company.LogoUrl,
                FoundedYear = company.FoundedYear,
                AboutUsTitle = company.AboutUsTitle,
                AboutUsDescription1 = company.AboutUsDescription1,
                AboutUsDescription2 = company.AboutUsDescription2,
                AboutUsImageUrl = company.AboutUsImageUrl,
                AboutUsImageAlt = company.AboutUsImageAlt,
                ExperienceNumber = company.ExperienceNumber,
                ExperienceText = company.ExperienceText,
                HappyTravelersCount = company.HappyTravelersCount,
                CountriesCoveredCount = company.CountriesCoveredCount,
                YearsExperienceCount = company.YearsExperienceCount,
                IsActive = company.IsActive
            };
        }
    }
}

