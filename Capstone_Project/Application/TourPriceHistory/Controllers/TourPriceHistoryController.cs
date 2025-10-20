using BE_Capstone_Project.Application.TourPriceHistories.DTOs;
using BE_Capstone_Project.Application.TourPriceHistories.Services;
using Microsoft.AspNetCore.Mvc;

namespace BE_Capstone_Project.Application.TourPriceHistories.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TourPriceHistoryController : ControllerBase
    {
        private readonly TourPriceHistoryService _service;

        public TourPriceHistoryController(TourPriceHistoryService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var tph = await _service.GetByIdAsync(id);
            if (tph == null) return NotFound();
            return Ok(tph);
        }

        [HttpGet("tour/{tourId}")]
        public async Task<IActionResult> GetByTourId(int tourId)
        {
            var list = await _service.GetByTourIdAsync(tourId);
            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTourPriceHistoryDTO dto)
        {
            var id = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateTourPriceHistoryDTO dto)
        {
            var success = await _service.UpdateAsync(id, dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
