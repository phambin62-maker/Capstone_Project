using BE_Capstone_Project.Application.Company.DTOs;
using BE_Capstone_Project.Domain.Models;
using FE_Capstone_Project.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace FE_Capstone_Project.ViewComponents
{
    public class SiteInfoViewComponent : ViewComponent
    {
        private readonly ApiHelper _apiHelper;
        public SiteInfoViewComponent(ApiHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                var company = await _apiHelper.GetAsync<CompanyDTO>("Company/active");

                return View(company ?? new CompanyDTO());
            }
            catch
            {
                return View(new CompanyDTO());
            }
        }
    }
}