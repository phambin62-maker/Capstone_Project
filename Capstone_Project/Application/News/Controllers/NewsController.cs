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

namespace BE_Capstone_Project.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private readonly NewsService _newsService;
        private readonly IWebHostEnvironment _environment;

        public NewsController(NewsService newsService, IWebHostEnvironment environment)
        {
            _newsService = newsService;
            _environment = environment;
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
        public async Task<IActionResult> GetByStatus(NewsStatus status)
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

        // (API này dùng cho lỗi hiệu năng của Chuông)
        [HttpGet("recent/{userId}")]
        public async Task<IActionResult> GetRecentByUserId(int userId)
        {
            // Note: API này đang bị nhầm lẫn, nó gọi NewsService.GetRecentAsync
            // (Đáng lẽ phải gọi NotificationService, nhưng file này là NewsController)
            // Tuy nhiên, tôi sẽ giữ nguyên theo code bạn gửi.
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
                // Chặn file .HEIC
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
                NewsStatus = formDto.NewsStatus
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

            // 1. Lấy Entity (Model) gốc (Fetch #1 - Lần DUY NHẤT)
            var existingNewsEntity = await _newsService.GetNewsEntityByIdAsync(id);
            if (existingNewsEntity == null)
                return NotFound("Không tìm thấy tin tức để cập nhật.");

            string? oldRelativePath = existingNewsEntity.Image;
            string? newRelativePath = oldRelativePath; // Mặc định là ảnh cũ

            // 2. Xử lý file upload (nếu có file mới)
            if (formDto.ImageFile != null && formDto.ImageFile.Length > 0)
            {
                var originalExtension = Path.GetExtension(formDto.ImageFile.FileName).ToLower();
                // Chặn file .HEIC
                if (originalExtension == ".heic" || originalExtension == ".heif")
                {
                    return StatusCode(415, "HEIC files are not supported. Please upload JPG or PNG.");
                }

                // 2a. Lưu file mới
                var uploadsFolderPath = Path.Combine(_environment.WebRootPath, "images", "news");
                if (!Directory.Exists(uploadsFolderPath)) Directory.CreateDirectory(uploadsFolderPath);

                var fileName = Guid.NewGuid().ToString() + originalExtension;
                var filePath = Path.Combine(uploadsFolderPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await formDto.ImageFile.CopyToAsync(stream);
                }
                newRelativePath = $"/images/news/{fileName}"; // Lấy đường dẫn mới

                // 2b. Xóa file cũ (nếu có)
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
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deleting old file: {ex.Message}");
                    }
                }
            }

            // 3. Cập nhật Entity
            existingNewsEntity.Title = formDto.Title;
            existingNewsEntity.Content = formDto.Content;
            existingNewsEntity.Image = newRelativePath;
            existingNewsEntity.NewsStatus = formDto.NewsStatus ?? existingNewsEntity.NewsStatus;
            existingNewsEntity.UserId = formDto.UserId;

            // 4. Gọi Service Update (chỉ để lưu)
            var success = await _newsService.UpdateAsync(existingNewsEntity);
            if (!success)
                return NotFound("Cập nhật thất bại.");

            return NoContent();
        }

        // (Sửa lỗi "Không xóa được")
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Delete(int id)
        {
            // 1. Lấy Entity (Fetch #1 - Lần DUY NHẤT)
            var existingNewsEntity = await _newsService.GetNewsEntityByIdAsync(id);
            if (existingNewsEntity == null)
                return NotFound();

            string? relativePath = existingNewsEntity.Image;

            // 2. Gọi service để xóa (truyền entity đã được theo dõi)
            var success = await _newsService.DeleteAsync(existingNewsEntity);
            if (!success) return NotFound("Xóa database thất bại.");

            // 3. Nếu xóa DB thành công, thì xóa file
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