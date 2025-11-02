using BE_Capstone_Project.Application.TourManagement.DTOs;
using BE_Capstone_Project.Application.TourManagement.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BE_Capstone_Project.Application.TourManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TourScheduleController : ControllerBase
    {
        private readonly ITourScheduleService _tourScheduleService;
        private readonly ITourService _tourService;
        public TourScheduleController(ITourService tourService, ITourScheduleService tourScheduleService)
        {
            _tourService = tourService;
            _tourScheduleService = tourScheduleService; 
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<TourScheduleDTO>>>> GetAllTourSchedules()
        {
            try
            {
                var schedules = await _tourScheduleService.GetAllTourSchedules();
                return Ok(new ApiResponse<List<TourScheduleDTO>>(true, "Tour schedules retrieved successfully", schedules));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<TourScheduleDTO>>(false, $"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<TourScheduleDTO>>> GetTourScheduleById(int id)
        {
            try
            {
                var schedule = await _tourScheduleService.GetTourScheduleById(id);
                if (schedule == null) return NotFound(new ApiResponse<TourScheduleDTO>(false, $"Tour schedule with ID {id} not found"));
                return Ok(new ApiResponse<TourScheduleDTO>(true, "Tour schedule retrieved successfully", schedule));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<TourScheduleDTO>(false, $"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("tour/{tourId}")]
        public async Task<ActionResult<ApiResponse<List<TourScheduleDTO>>>> GetTourSchedulesByTourId(int tourId)
        {
            try
            {
                var schedules = await _tourScheduleService.GetTourSchedulesByTourId(tourId);
                return Ok(new ApiResponse<List<TourScheduleDTO>>(true, "Tour schedules retrieved successfully", schedules));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<TourScheduleDTO>>(false, $"Internal server error: {ex.Message}"));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<int>>> CreateTourSchedule([FromBody] CreateTourScheduleRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(new ApiResponse<int>(false, "Invalid request data"));

                var scheduleId = await _tourScheduleService.CreateTourSchedule(request);
                if (scheduleId > 0)
                    return CreatedAtAction(nameof(GetTourScheduleById), new { id = scheduleId },
                        new ApiResponse<int>(true, "Tour schedule created successfully", scheduleId));
                else
                    return StatusCode(500, new ApiResponse<int>(false, "Failed to create tour schedule"));
            }
            catch (ArgumentException ex) { return BadRequest(new ApiResponse<int>(false, ex.Message)); }
            catch (Exception ex) { return StatusCode(500, new ApiResponse<int>(false, $"Internal server error: {ex.Message}")); }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateTourSchedule(int id, [FromBody] UpdateTourScheduleRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(new ApiResponse<bool>(false, "Invalid request data"));

                var result = await _tourScheduleService.UpdateTourSchedule(id, request);
                if (result) return Ok(new ApiResponse<bool>(true, "Tour schedule updated successfully", true));
                else return StatusCode(500, new ApiResponse<bool>(false, "Failed to update tour schedule"));
            }
            catch (KeyNotFoundException ex) { return NotFound(new ApiResponse<bool>(false, ex.Message)); }
            catch (ArgumentException ex) { return BadRequest(new ApiResponse<bool>(false, ex.Message)); }
            catch (Exception ex) { return StatusCode(500, new ApiResponse<bool>(false, $"Internal server error: {ex.Message}")); }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteTourSchedule(int id)
        {
            try
            {
                var result = await _tourScheduleService.DeleteTourSchedule(id);
                if (result) return Ok(new ApiResponse<bool>(true, "Tour schedule deleted successfully", true));
                else return NotFound(new ApiResponse<bool>(false, $"Tour schedule with ID {id} not found"));
            }
            catch (Exception ex) { return StatusCode(500, new ApiResponse<bool>(false, $"Internal server error: {ex.Message}")); }
        }

        [HttpGet("GetPaginatedTourSchedules")]
        public async Task<ActionResult<ApiResponse<bool>>> GetPaginatedTourSchedules(int page, int pageSize)
        {
            try
            {
                var schedules = await _tourScheduleService.GetPaginatedTourSchedules(page, pageSize);
                return Ok(new ApiResponse<List<TourScheduleDTO>>(true, "Tour schedules retrieved successfully", schedules));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<TourScheduleDTO>>(false, $"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("GetPaginatedTourSchedules/{tourId}")]
        public async Task<ActionResult<ApiResponse<List<TourScheduleDTO>>>> GetPaginatedTourSchedulesByTourId(int tourId, int page = 1, int pageSize = 5)
        {
            try
            {
                var schedules = await _tourScheduleService.GetPaginatedTourSchedulesByTourId(tourId, page, pageSize);
                return Ok(new ApiResponse<List<TourScheduleDTO>>(true, "Tour schedules retrieved successfully", schedules));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<TourScheduleDTO>>(false, $"Internal server error: {ex.Message}"));
            }
        }
        [HttpGet("GetTourNameById")]
        public async Task<IActionResult> GetTourNameById(int id)
        {
            try
            {
                var tour = await _tourService.GetTourById(id);
                if (tour == null)
                    return NotFound(new { message = $"Không tìm thấy tour với id {id}" });

                return Ok(new { id = tour.Id, name = tour.Name });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi khi lấy tên tour", error = ex.Message });
            }
        }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public ApiResponse(bool success, string message, T data = default) => (Success, Message, Data) = (success, message, data);
    }
}
