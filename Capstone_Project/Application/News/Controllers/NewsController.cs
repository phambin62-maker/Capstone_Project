using BE_Capstone_Project.Application.Newses.DTOs;
using BE_Capstone_Project.Application.Newses.Services;
using BE_Capstone_Project.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace BE_Capstone_Project.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private readonly NewsService _newsService;

        public NewsController(NewsService newsService)
        {
            _newsService = newsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var news = await _newsService.GetAllAsync();
            return Ok(news);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _newsService.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetByStatus(NewsStatus status)
        {
            var news = await _newsService.GetByStatusAsync(status);
            return Ok(news);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateNewsDTO dto)
        {
            var newId = await _newsService.CreateAsync(dto);
            if (newId <= 0)
                return BadRequest("Không thể tạo tin tức.");

            return CreatedAtAction(nameof(GetById), new { id = newId }, new { id = newId });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateNewsDTO dto)
        {
            var success = await _newsService.UpdateAsync(id, dto);
            if (!success)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _newsService.DeleteAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}
