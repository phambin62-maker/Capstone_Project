using BE_Capstone_Project.Application.BookingManagement.Services.Interfaces;
using BE_Capstone_Project.Application.Services;
using BE_Capstone_Project.Application.TourManagement.DTOs;
using BE_Capstone_Project.Application.TourManagement.Services.Interfaces;
using BE_Capstone_Project.Domain.Enums;
using BE_Capstone_Project.Domain.Models;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace BE_Capstone_Project.Application.TourManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TourController : ControllerBase
    {
        private readonly ITourService _tourService;
        private readonly ITourImageService _tourImageService;
        private readonly IUserService _userService;
        private readonly BookingService _bookingService;
        private readonly ILogger<TourController> _logger;
        public TourController(
            ITourService tourService, 
            ITourImageService tourImageService, 
            IUserService userService, 
            BookingService bookingService,
            ILogger<TourController> logger)
        {
            _tourService = tourService;
            _tourImageService = tourImageService;
            _userService = userService;
            _bookingService = bookingService;
            _logger = logger; 
        }

        [HttpPost("AddTour")]
        public async Task<IActionResult> AddTour([FromForm] TourDTO tour, [FromForm] List<IFormFile> images)
        {
            var tourToAdd = new Tour
            {
                Name = tour.Name,
                Description = tour.Description,
                Price = tour.Price,
                Duration = tour.Duration,
                StartLocationId = tour.StartLocationId,
                EndLocationId = tour.EndLocationId,
                CategoryId = tour.CategoryId,
                CancelConditionId = tour.CancelConditionId,
                ChildDiscount = tour.ChildDiscount,
                GroupDiscount = tour.GroupDiscount,
                GroupNumber = tour.GroupNumber,
                MinSeats = tour.MinSeats,
                MaxSeats = tour.MaxSeats,
                TourStatus = true,
            };
            var result = await _tourService.AddTour(tourToAdd);

            if (result == -1) return BadRequest(new { message = "Failed to add tour" });

            // Xử lý upload ảnh
            var imagePaths = new List<string>();
            if (images != null && images.Count > 0)
            {
                foreach (var image in images)
                {
                    if (image.Length > 0)
                    {
                        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
                        var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "tours");

                        if (!Directory.Exists(imagesPath))
                        {
                            Directory.CreateDirectory(imagesPath);
                        }

                        var fullPath = Path.Combine(imagesPath, fileName);

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        var relativePath = $"/images/tours/{fileName}";
                        imagePaths.Add(relativePath);

                        var imageToAdd = new TourImage
                        {
                            TourId = result,
                            Image = relativePath,
                        };

                        await _tourImageService.AddTourImage(imageToAdd);
                    }
                }
            }

            return Ok(new { message = "Tour added successfully", tourId = result });
        }

        [HttpPost("UpdateTour")]
        public async Task<IActionResult> UpdateTour([FromForm] TourDTO tour, [FromForm] List<IFormFile> images)
        {
            var tourToUpdate = await _tourService.GetTourById(tour.Id.Value);
            if (tourToUpdate == null) return BadRequest(new { message = "Failed to update tour" });

            tourToUpdate.Name = tour.Name;
            tourToUpdate.Description = tour.Description;
            tourToUpdate.Price = tour.Price;
            tourToUpdate.Duration = tour.Duration;
            tourToUpdate.StartLocationId = tour.StartLocationId;
            tourToUpdate.EndLocationId = tour.EndLocationId;
            tourToUpdate.CategoryId = tour.CategoryId;
            tourToUpdate.CancelConditionId = tour.CancelConditionId;
            tourToUpdate.ChildDiscount = tour.ChildDiscount;
            tourToUpdate.GroupDiscount = tour.GroupDiscount;
            tourToUpdate.GroupNumber = tour.GroupNumber;
            tourToUpdate.MinSeats = tour.MinSeats;
            tourToUpdate.MaxSeats = tour.MaxSeats;

            var result = await _tourService.UpdateTour(tourToUpdate);

            if (!result) return BadRequest(new { message = "Failed to update tour" });

            // CHỈ THÊM: Thêm ảnh mới vào ảnh cũ, không xóa ảnh cũ
            if (images != null && images.Count > 0)
            {
                foreach (var image in images)
                {
                    if (image.Length > 0)
                    {
                        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
                        var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "tours");

                        if (!Directory.Exists(imagesPath))
                        {
                            Directory.CreateDirectory(imagesPath);
                        }

                        var fullPath = Path.Combine(imagesPath, fileName);

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        var relativePath = $"/images/tours/{fileName}";

                        var imageToAdd = new TourImage
                        {
                            TourId = tour.Id.Value,
                            Image = relativePath
                        };

                        await _tourImageService.AddTourImage(imageToAdd);
                    }
                }
            }

            return Ok(new { message = "Tour updated successfully" });
        }

        [HttpDelete("DeleteTour")]
        public async Task<IActionResult> DeleteTour(int tourId)
        {
            var imagesToDelete = await _tourImageService.GetTourImagesByTourId(tourId);
            foreach (var image in imagesToDelete)
            {
                await _tourImageService.DeleteTourImage(image.Id);
            }

            var result = await _tourService.DeleteTour(tourId);

            if (!result) return BadRequest(new { message = "Failed to delete tour" });

            return Ok(new { message = "Tour deleted successfully" });
        }

        [HttpGet("GetAllTours")]
        public async Task<IActionResult> GetAllTours()
        {
            var tours = await _tourService.GetAllTours();

            if (tours == null || !tours.Any())
                return Ok(new { message = "No tours found", tours });

            return Ok(new { message = $"Found {tours.Count} tours", tours });
        }

        [HttpGet("GetTourById/{id}")]
        public async Task<IActionResult> GetTourById(int id, [FromQuery] string? username)
        {
            var tour = await _tourService.GetTourById(id);

            if (tour == null)
                return NotFound(new { message = $"No tour found with id {id}" });

            bool canComment = false;

            if(!string.IsNullOrEmpty(username))
            {
                var user = await _userService.GetUserByUsername(username);
                if (user != null)
                {
                    canComment = await _bookingService.HasUserBookedTour(user.Id, id);
                }
            }

            return Ok(new { message = $"Tour with id {id} found successfully", tour, canComment });
        }

        [HttpGet("GetToursByCategoryId")]
        public async Task<IActionResult> GetToursByCategoryId(int categoryId)
        {
            var tours = await _tourService.GetToursByCategoryId(categoryId);

            if (tours == null || !tours.Any())
                return Ok(new { message = $"No tours found with category id {categoryId}", tours });

            return Ok(new { message = $"Tours with category id {categoryId} found successfully", tours });
        }

        [HttpGet("GetToursByStartLocationId")]
        public async Task<IActionResult> GetToursByStartLocationId(int startLocationId)
        {
            var tours = await _tourService.GetToursByStartLocationId(startLocationId);

            if (tours == null || !tours.Any())
                return Ok(new { message = $"No tours found with start location id {startLocationId}", tours });

            return Ok(new { message = $"Tours with start location id {startLocationId} found successfully", tours });
        }

        [HttpGet("GetToursByEndLocationId")]
        public async Task<IActionResult> GetToursByEndLocationId(int endLocationId)
        {
            var tours = await _tourService.GetToursByEndLocationId(endLocationId);

            if (tours == null || !tours.Any())
                return Ok(new { message = $"No tours found with end location id {endLocationId}", tours });

            return Ok(new { message = $"Tours with end location id {endLocationId} found successfully", tours });
        }

        [HttpGet("GetToursByPriceRange")]
        public async Task<IActionResult> GetToursByPriceRange(decimal minPrice, decimal maxPrice)
        {
            var tours = await _tourService.GetToursByPriceRange(minPrice, maxPrice);

            if (tours == null || !tours.Any())
                return Ok(new { message = $"No tours found in price range {minPrice}-{maxPrice}", tours });

            return Ok(new { message = $"Tours in price range {minPrice}-{maxPrice} found successfully", tours });
        }

        [HttpGet("GetToursByScheduleId/{scheduleId}")]
        public async Task<IActionResult> GetTourByScheduleId(int scheduleId)
        {
            var tour = await _tourService.GetTourByScheduleId(scheduleId);
            if (tour == null)
                return NotFound(new { message = $"No tour found with schedule id {scheduleId}" });
            return Ok(new { message = $"Tour with schedule id {scheduleId} found successfully", tour });
        }

        [HttpGet("SearchTourByName")]
        public async Task<IActionResult> SearchTourByName(string name)
        {
            var tours = await _tourService.SearchTourByName(name);

            if (tours == null || !tours.Any())
                return NotFound(new { message = $"No tours found with name '{name}'" });

            return Ok(new { message = $"Tours with name `{name}` found successfully", tours });
        }


        [HttpGet("GetTotalTourCount")]
        public async Task<IActionResult> GetTotalTourCount()
        {
            var tourCount = await _tourService.GetTotalTourCount();

            if (tourCount == 0) return Ok(new { message = $"There are no tours", tourCount });

            return Ok(new { message = $"There are {tourCount} tours", tourCount });
        }

        [HttpGet("GetPaginatedTours")]
        public async Task<IActionResult> GetPaginatedTours(int page = 1, int pageSize = 10)
        {
            var tours = await _tourService.GetPaginatedTours(page, pageSize);

            if (tours == null || !tours.Any())
                return Ok(new { message = "No tours found", tours });

            return Ok(new { message = $"Found {tours.Count} tours", tours });
        }

        [HttpPost("ToggleTourStatus")]
        public async Task<IActionResult> ToggleTourStatus(int tourId)
        {
            try
            {
                var tour = await _tourService.GetTourById(tourId);
                if (tour == null)
                    return NotFound(new { message = $"Không tìm thấy tour với id {tourId}" });

                tour.TourStatus = !tour.TourStatus;
                var newStatus = tour.TourStatus;

                var result = await _tourService.UpdateTour(tour);

                if (!result)
                    return BadRequest(new { message = "Thay đổi trạng thái thất bại" });

                return Ok(new
                {
                    message = (bool)newStatus ? "Tour đã được hiển thị" : "Tour đã được ẩn",
                    newStatus = newStatus
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi khi thay đổi trạng thái", error = ex.Message });
            }
        }

        [HttpGet("GetTopToursByEachCategories")]
        public async Task<IActionResult> GetTopToursByEachCategories()
        {
            var tours = await _tourService.GetTopToursByEachCategories();

            if (tours == null || !tours.Any())
                return Ok(new { message = "No tours found", tours });

            return Ok(new { message = $"Found {tours.Count} tours", tours });
        }

        [HttpGet("GetFilteredTours")]
        public async Task<IActionResult> GetFilteredTours(
        int page = 1,
        int pageSize = 10,
        bool? status = null,  
        int? startLocation = null,
        int? endLocation = null,
        int? category = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string sort = null,
        string search = null)
        {
            var tours = await _tourService.GetFilteredTours(
                page, pageSize, status, startLocation, endLocation,
                category, minPrice, maxPrice, sort, search);

            if (tours == null || !tours.Any())
                return Ok(new { message = "No tours found", tours });

            return Ok(new { message = $"Found {tours.Count} tours", tours });
        }

        [HttpGet("GetFilteredTourCount")]
        public async Task<IActionResult> GetFilteredTourCount(
            bool? status = null,
            int? startLocation = null,
            int? endLocation = null,
            int? category = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string search = null)
        {
            var count = await _tourService.GetFilteredTourCount(
                status, startLocation, endLocation, category, minPrice, maxPrice, search);

            return Ok(new { message = $"Filtered tour count: {count}", tourCount = count });
        }

        [HttpPost("UploadTourImages")]
        public async Task<IActionResult> UploadTourImages(int tourId, List<IFormFile> images)
        {
            try
            {
                var uploadedImages = new List<string>();

                foreach (var image in images)
                {
                    if (image.Length > 0)
                    {
                        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";

                        var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "tours");

                        // Đảm bảo thư mục tồn tại
                        if (!Directory.Exists(imagesPath))
                        {
                            Directory.CreateDirectory(imagesPath);
                        }

                        var fullPath = Path.Combine(imagesPath, fileName);

                        // Lưu file
                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        // Lưu đường dẫn tương đối để trả về frontend
                        var relativePath = $"/images/tours/{fileName}";
                        uploadedImages.Add(relativePath);
                    }
                }

                return Ok(new { message = "Images uploaded successfully", images = uploadedImages });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to upload images", error = ex.Message });
            }
        }
        [HttpGet("GetImage")]
        public IActionResult GetImage([FromQuery] string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                    return NotFound(new { message = "Image path is required" });

                // Xử lý đường dẫn an toàn
                var safePath = path.TrimStart('/');
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", safePath);

                if (!System.IO.File.Exists(fullPath))
                {
                    // Trả về ảnh mặc định nếu không tìm thấy
                    var defaultImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "default-tour.jpg");
                    if (System.IO.File.Exists(defaultImagePath))
                    {
                        var defaultImageBytes = System.IO.File.ReadAllBytes(defaultImagePath);
                        return File(defaultImageBytes, "image/jpeg");
                    }
                    return NotFound(new { message = "Image not found" });
                }

                var imageBytes = System.IO.File.ReadAllBytes(fullPath);
                var contentType = GetContentType(fullPath);

                return File(imageBytes, contentType);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error loading image", error = ex.Message });
            }
        }

        private string GetContentType(string path)
        {
            var extension = Path.GetExtension(path).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",             
                _ => "application/octet-stream"
            };
        }

        [HttpDelete("DeleteTourImage")]
        public async Task<IActionResult> DeleteTourImage(int imageId)
        {
            try
            {
                _logger.LogInformation($"Attempting to delete tour image with ID: {imageId}");

                // Lấy thông tin image từ database
                var image = await _tourImageService.GetTourImageById(imageId);

                if (image == null)
                {
                    _logger.LogWarning($"Image not found with ID: {imageId}");
                    return NotFound(new
                    {
                        success = false,
                        message = "Image not found"
                    });
                }

                _logger.LogInformation($"Found image: {image.Image}");

                // Xóa file vật lý nếu tồn tại
                if (!string.IsNullOrEmpty(image.Image))
                {
                    try
                    {
                        var imagePath = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            image.Image.TrimStart('/')
                        );

                        _logger.LogInformation($"Attempting to delete physical file: {imagePath}");

                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                            _logger.LogInformation($"Physical file deleted successfully");
                        }
                        else
                        {
                            _logger.LogWarning($"Physical file not found: {imagePath}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error deleting physical file: {image.Image}");
                        // Tiếp tục xóa record trong database dù file vật lý không xóa được
                    }
                }

                // Xóa record trong database
                var result = await _tourImageService.DeleteTourImage(imageId);

                if (!result)
                {
                    _logger.LogError($"Failed to delete image record from database: {imageId}");
                    return BadRequest(new
                    {
                        success = false,
                        message = "Failed to delete image from database"
                    });
                }

                _logger.LogInformation($"Image deleted successfully: {imageId}");

                return Ok(new
                {
                    success = true,
                    message = "Image deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception while deleting image: {imageId}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error deleting image",
                    error = ex.Message
                });
            }
        }
        [HttpGet("GetActiveTours")]
        public async Task<ActionResult<ApiResponse<List<Tour>>>> GetActiveTours([FromQuery] string search = "")
        {
            try
            {
                var tours = await _tourService.GetActiveTours(search);
                return Ok(new ApiResponse<List<Tour>>(true, "Active tours retrieved successfully", tours));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<Tour>>(false, $"An error occurred: {ex.Message}", new List<Tour>()));
            }
        }
    }
}
