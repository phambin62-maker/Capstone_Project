using BE_Capstone_Project.Application.CancelConditions.DTOs;
using BE_Capstone_Project.Application.CancelConditions.Services.Interfaces;
using BE_Capstone_Project.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BE_Capstone_Project.Application.CancelConditions.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CancelConditionController : ControllerBase
    {
        private readonly ICancelConditionService _service;

        public CancelConditionController(ICancelConditionService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllAsync();
            return Ok(new { success = true, data = list });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { success = false, message = "Cancel condition not found" });

            return Ok(new { success = true, data = result });
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetByStatus(CancelStatus status)
        {
            var result = await _service.GetByStatusAsync(status);
            return Ok(new { success = true, data = result });
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? keyword, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _service.GetPagingAsync(keyword ?? "", pageIndex, pageSize);
            return Ok(new { success = true, data = result });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create([FromBody] CancelConditionCreateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = errors
                });
            }

            try 
            {
                var id = await _service.CreateAsync(dto);
                if (id <= 0)
                    return BadRequest(new { success = false, message = "Create failed" });

                return Ok(new
                {
                    success = true,
                    message = "Created successfully",
                    data = new { id }
                });
            }
            catch (InvalidOperationException ex)
            {
                 return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Update([FromBody] CancelConditionUpdateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = errors
                });
            }

            try
            {
                var success = await _service.UpdateAsync(dto);
                if (!success)
                    return NotFound(new { success = false, message = "Cancel condition not found" });

                return Ok(new { success = true, message = "Updated successfully" });
            }
            catch (InvalidOperationException ex)
            {
                 return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success)
                return NotFound(new { success = false, message = "Cancel condition not found" });

            return Ok(new { success = true, message = "Deleted successfully" });
        }
    }
}