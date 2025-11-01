using BE_Capstone_Project.Application.TourCategoryManagement.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BE_Capstone_Project.Application.TourCategoryManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TourCategoryController : ControllerBase
    {
        private readonly ITourCategoryService _tourCategoryService;

        public TourCategoryController(ITourCategoryService tourCategoryService)
        {
            _tourCategoryService = tourCategoryService;
        }

        [HttpGet("GetAllTourCategories")]
        public async Task<IActionResult> GetAllTourCategories()
        {
            var categories = await _tourCategoryService.GetAllTourCategories();
            if (categories == null || categories.Count == 0)
            {
                return NotFound(new { message = "No tour categories found" });
            }
            return Ok(new { message = "Tour categories fetched successfully", categories });
        }
    }
}
