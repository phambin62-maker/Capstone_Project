using BE_Capstone_Project.Application.Categories.DTOs;
using BE_Capstone_Project.Application.Categories.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BE_Capstone_Project.Application.Categories.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TourCategoriesController : ControllerBase
    {
        private readonly ITourCategoryService _tourCategoryService;

        public TourCategoriesController(ITourCategoryService tourCategoryService)
        {
            _tourCategoryService = tourCategoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTourCategories()
        {
            var result = await _tourCategoryService.GetAllTourCategoriesAsync();
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }
            return Ok(result.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTourCategoryById(int id)
        {
            var result = await _tourCategoryService.GetTourCategoryByIdAsync(id);
            if (!result.Success)
            {
                return NotFound(new { message = result.Message });
            }
            return Ok(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTourCategory([FromBody] CreateTourCategoryDTO createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _tourCategoryService.CreateTourCategoryAsync(createDto);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return CreatedAtAction(nameof(GetTourCategoryById), new { id = result.Data }, new { id = result.Data, message = result.Message });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTourCategory(int id, [FromBody] UpdateTourCategoryDTO updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _tourCategoryService.UpdateTourCategoryAsync(id, updateDto);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTourCategory(int id)
        {
            var result = await _tourCategoryService.DeleteTourCategoryAsync(id);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }
    }
}
