using BE_Capstone_Project.Application.Company.DTOs;
using BE_Capstone_Project.Application.Company.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BE_Capstone_Project.Application.Company.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        [HttpGet("active")]
        [AllowAnonymous] // Tất cả đều có thể xem thông tin công ty
        public async Task<IActionResult> GetActiveCompany()
        {
            try
            {
                var company = await _companyService.GetActiveCompanyAsync();
                if (company == null)
                    return NotFound(new { message = "No active company found" });

                return Ok(company);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching company data", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous] // Tất cả đều có thể xem thông tin công ty
        public async Task<IActionResult> GetCompanyById(int id)
        {
            try
            {
                var company = await _companyService.GetCompanyByIdAsync(id);
                if (company == null)
                    return NotFound(new { message = $"Company with ID {id} not found" });

                return Ok(company);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching company data", error = ex.Message });
            }
        }

        
    }
}

