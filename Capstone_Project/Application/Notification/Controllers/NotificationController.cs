using BE_Capstone_Project.Application.Notifications.DTOs;
using BE_Capstone_Project.Application.Notifications.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BE_Capstone_Project.Application.Notifications.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationController(INotificationService service)
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
            var noti = await _service.GetByIdAsync(id);
            if (noti == null) return NotFound();
            return Ok(noti);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(int userId, [FromQuery] bool? isRead)
        {
            var list = await _service.GetByUserIdAsync(userId, isRead);
            return Ok(list);
        }

        [HttpGet("unread/{userId}")]
        public async Task<IActionResult> GetUnreadCount(int userId)
        {
            var count = await _service.GetUnreadCountAsync(userId);
            return Ok(new { count = count });
        }

        [HttpPost("mark-read/{userId}")]
        public async Task<IActionResult> MarkAllAsRead(int userId)
        {
            var success = await _service.MarkAllAsReadAsync(userId);
            if (!success) return BadRequest("Could not mark all notifications as read.");
            return Ok(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateNotificationDTO dto)
        {
            var id = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateNotificationDTO dto)
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

        [HttpDelete("user/{userId}")]
        public async Task<IActionResult> DeleteAllByUserId(int userId)
        {
            var success = await _service.DeleteAllByUserIdAsync(userId);
            if (!success) return BadRequest("Could not delete notifications.");
            return Ok(new { success = true });
        }

        [HttpPost("mark-read-single/{notificationId}/{userId}")]
        public async Task<IActionResult> MarkSingleAsRead(int notificationId, int userId)
        {
            var result = await _service.MarkAsReadAsync(notificationId, userId);
            if (!result)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpGet("recent/{userId}")]
        public async Task<IActionResult> GetRecentByUserId(int userId)
        {
            var notifications = await _service.GetRecentAsync(userId);
            return Ok(notifications);
        }
    }
}