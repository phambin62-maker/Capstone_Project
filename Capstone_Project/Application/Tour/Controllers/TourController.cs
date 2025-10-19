using BE_Capstone_Project.Application.Tour.DTOs;
using BE_Capstone_Project.Application.Tour.Services;
using Microsoft.AspNetCore.Mvc;

namespace BE_Capstone_Project.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TourController : ControllerBase
    {
        private readonly TourService _tourService; 

        public TourController(TourService tourService)
        {
            _tourService = tourService;
        }

        // 🔹 Lấy tất cả tour
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tours = await _tourService.GetAllAsync();
            return Ok(tours);
        }

        // 🔹 Lấy tour theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var tour = await _tourService.GetByIdAsync(id);
            if (tour == null)
                return NotFound();
            return Ok(tour);
        }

        // 🔹 Thêm mới tour
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTourDTO dto)
        {
            var newId = await _tourService.CreateAsync(dto);
            if (newId <= 0)
                return BadRequest("Không thể tạo tour.");

            return CreatedAtAction(nameof(GetById), new { id = newId }, new { id = newId });
        }

        // 🔹 Cập nhật tour
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateTourDTO dto)
        {
            var success = await _tourService.UpdateAsync(id, dto);
            if (!success)
                return NotFound();

            return NoContent();
        }

        // 🔹 Xóa tour
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _tourService.DeleteAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}
