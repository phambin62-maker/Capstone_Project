using BE_Capstone_Project.Application.Newses.DTOs;
using BE_Capstone_Project.Application.Newses.Services;
using BE_Capstone_Project.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;
using BE_Capstone_Project.Domain.Models;
using System.Security.Claims;
using BE_Capstone_Project.Infrastructure;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Globalization; // Cần cho NormalizeString
using System.Text; // Cần cho NormalizeString

namespace BE_Capstone_Project.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private readonly NewsService _newsService;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly OtmsdbContext _context;

        public NewsController(NewsService newsService, IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor, OtmsdbContext context)
        {
            _newsService = newsService;
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        // === CÁC HÀM HELPER (ĐÃ DI CHUYỂN VÀ THÊM VÀO) ===

        private int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdClaim, out int userId);
            return userId;
        }

        private async Task<string> GetUserNameById(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                return $"{user.FirstName} {user.LastName}";
            }
            return "Unknown";
        }

        // SỬA: Đã di chuyển ParseNewsStatus từ Service sang Controller
        private NewsStatus ParseNewsStatus(string? statusString)
        {
            if (string.IsNullOrEmpty(statusString))
            {
                return NewsStatus.Draft; // Mặc định
            }
            if (Enum.TryParse<NewsStatus>(statusString, true, out var statusEnum))
            {
                return statusEnum;
            }
            return NewsStatus.Draft;
        }

        // === CÁC LỆNH GET (ĐỌC) ===

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
        public async Task<IActionResult> GetByStatus(string status)
        {
            var news = await _newsService.GetByStatusAsync(status);
            return Ok(news);
        }

        [HttpGet("stats")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetStats()
        {
            var stats = await _newsService.GetNewsStatsAsync();
            return Ok(stats);
        }

        [HttpGet("recent/{userId}")]
        public async Task<IActionResult> GetRecentByUserId(int userId)
        {
            var notifications = await _newsService.GetRecentAsync(userId);
            return Ok(notifications);
        }

        // === CÁC LỆNH "VIẾT" (WRITE) ===

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create([FromForm] CreateNewsFormDTO formDto)
        {
            if (formDto == null) return BadRequest("Form data is null.");

            string? imageUrl = null;
            if (formDto.ImageFile != null && formDto.ImageFile.Length > 0)
            {
                var originalExtension = Path.GetExtension(formDto.ImageFile.FileName).ToLower();
                if (originalExtension == ".heic" || originalExtension == ".heif")
                {
                    return StatusCode(415, "HEIC files are not supported. Please upload JPG or PNG.");
                }

                var uploadsFolderPath = Path.Combine(_environment.WebRootPath, "images", "news");
                if (!Directory.Exists(uploadsFolderPath))
                {
                    Directory.CreateDirectory(uploadsFolderPath);
                }
                var fileName = Guid.NewGuid().ToString() + originalExtension;
                var filePath = Path.Combine(uploadsFolderPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await formDto.ImageFile.CopyToAsync(stream);
                }
                imageUrl = $"/images/news/{fileName}";
            }

            var createNewsDto = new CreateNewsDTO
            {
                UserId = formDto.UserId,
                Title = formDto.Title,
                Content = formDto.Content,
                Image = imageUrl,
                NewsStatus = formDto.NewsStatus // (DTO đã chấp nhận string)
            };

            var newId = await _newsService.CreateAsync(createNewsDto);
            if (newId <= 0)
                return BadRequest("Không thể tạo tin tức.");

            return CreatedAtAction(nameof(GetById), new { id = newId }, new { id = newId });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Update(int id, [FromForm] EditNewsFormDTO formDto)
        {
            if (id != formDto.Id)
            {
                return BadRequest("ID không khớp.");
            }

            var existingNewsEntity = await _newsService.GetNewsEntityByIdAsync(id);
            if (existingNewsEntity == null)
                return NotFound("Không tìm thấy tin tức để cập nhật.");

            string? oldRelativePath = existingNewsEntity.Image;
            string? newRelativePath = oldRelativePath;

            if (formDto.ImageFile != null && formDto.ImageFile.Length > 0)
            {
                var originalExtension = Path.GetExtension(formDto.ImageFile.FileName).ToLower();
                if (originalExtension == ".heic" || originalExtension == ".heif")
                {
                    return StatusCode(415, "HEIC files are not supported. Please upload JPG or PNG.");
                }

                var uploadsFolderPath = Path.Combine(_environment.WebRootPath, "images", "news");
                if (!Directory.Exists(uploadsFolderPath)) Directory.CreateDirectory(uploadsFolderPath);

                var fileName = Guid.NewGuid().ToString() + originalExtension;
                var filePath = Path.Combine(uploadsFolderPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await formDto.ImageFile.CopyToAsync(stream);
                }
                newRelativePath = $"/images/news/{fileName}";

                if (!string.IsNullOrEmpty(oldRelativePath) && !oldRelativePath.StartsWith("data:image"))
                {
                    try
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, oldRelativePath.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }
                    catch (Exception ex) { Console.WriteLine($"Error deleting old file: {ex.Message}"); }
                }
            }

            existingNewsEntity.Title = formDto.Title;
            existingNewsEntity.Content = formDto.Content;
            existingNewsEntity.Image = newRelativePath;

            // SỬA LỖI CS1061: Chuyển string (từ DTO) sang Enum (cho Entity)
            if (!string.IsNullOrEmpty(formDto.NewsStatus))
            {
                existingNewsEntity.NewsStatus = ParseNewsStatus(formDto.NewsStatus);
            }

            existingNewsEntity.UserId = formDto.UserId;

            var updaterId = GetCurrentUserId();
            existingNewsEntity.UpdatedDate = DateTime.UtcNow;
            existingNewsEntity.UpdatedAuthor = await GetUserNameById(updaterId);

            var success = await _newsService.UpdateAsync(existingNewsEntity);
            if (!success)
                return NotFound("Cập nhật thất bại.");

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Delete(int id)
        {
            var existingNewsEntity = await _newsService.GetNewsEntityByIdAsync(id);
            if (existingNewsEntity == null)
                return NotFound();

            string? relativePath = existingNewsEntity.Image;

            var success = await _newsService.DeleteAsync(existingNewsEntity);
            if (!success) return NotFound("Xóa database thất bại.");

            if (!string.IsNullOrEmpty(relativePath) && !relativePath.StartsWith("data:image"))
            {
                try
                {
                    var filePath = Path.Combine(_environment.WebRootPath, relativePath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting file on DELETE: {ex.Message}");
                }
            }

            return NoContent();
        }
    }
}