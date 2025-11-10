using BE_Capstone_Project.Application.TourManagement.DTOs;
using BE_Capstone_Project.Application.TourManagement.Services.Interfaces;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

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


        [HttpGet("tour/available/{tourId}")]
        public async Task<ActionResult<ApiResponse<List<TourScheduleDTO>>>> GetTourSchedulesByTourIdAndYear(int tourId)
        {
            try
            {
                var schedules = await _tourScheduleService.GetTourSchedulesByTourId(tourId);

                var validSchedules = schedules
                .Where(s => s.DepartureDate.HasValue &&
                            s.DepartureDate.Value >= DateOnly.FromDateTime(DateTime.Today))
                .OrderBy(s => s.DepartureDate)
                .ToList();

                return Ok(new ApiResponse<List<TourScheduleDTO>>(true, "Tour schedules retrieved successfully", validSchedules));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<TourScheduleDTO>>(false, $"Internal server error: {ex.Message}"));
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")] // Chỉ Admin và Staff mới được tạo lịch tour
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
        [Authorize(Roles = "Admin,Staff")] // Chỉ Admin và Staff mới được cập nhật lịch tour
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
        [Authorize(Roles = "Admin,Staff")] // Chỉ Admin và Staff mới được xóa lịch tour
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
        // GET: api/TourSchedule/GetFilteredTourSchedules
        [HttpGet("GetFilteredTourSchedules")]
        public async Task<ActionResult<ApiResponse<List<TourScheduleDTO>>>> GetFilteredTourSchedules(
            [FromQuery] int? tourId = null,
            [FromQuery] string? tourName = null,
            [FromQuery] string? location = null,
            [FromQuery] string? category = null,
            [FromQuery] string? status = null,
            [FromQuery] string? sort = null,
            [FromQuery] string? search = null,
            [FromQuery] string? fromDate = null,
            [FromQuery] string? toDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var tourSchedules = await _tourScheduleService.GetFilteredTourSchedules(
                    tourId, tourName, location, category, status, sort, search, fromDate, toDate, page, pageSize);

                return Ok(new ApiResponse<List<TourScheduleDTO>>(
                    success: true,
                    message: "Filtered tour schedules retrieved successfully",
                    data: tourSchedules
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<TourScheduleDTO>>(
                    success: false,
                    message: $"An error occurred: {ex.Message}",
                    data: new List<TourScheduleDTO>()
                ));
            }
        }

        // GET: api/TourSchedule/GetFilteredTourScheduleCount
        [HttpGet("GetFilteredTourScheduleCount")]
        public async Task<ActionResult<ApiResponse<int>>> GetFilteredTourScheduleCount(
            [FromQuery] int? tourId = null,
            [FromQuery] string? tourName = null,
            [FromQuery] string? location = null,
            [FromQuery] string? category = null,
            [FromQuery] string? status = null,
            [FromQuery] string? search = null,
            [FromQuery] string? fromDate = null,
            [FromQuery] string? toDate = null)
        {
            try
            {
                var count = await _tourScheduleService.GetFilteredTourScheduleCount(
                    tourId, tourName, location, category, status, search, fromDate, toDate);

                return Ok(new ApiResponse<int>(
                    success: true,
                    message: "Filtered tour schedule count retrieved successfully",
                    data: count
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<int>(
                    success: false,
                    message: $"An error occurred: {ex.Message}",
                    data: 0
                ));
            }
        }

    }

    public class ApiResponse<T>
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("data")]
        public T Data { get; set; }

        public ApiResponse(bool success, string message, T data = default)
        {
            Success = success;
            Message = message;
            Data = data;
        }
    }
}
