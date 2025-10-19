using BE_Capstone_Project.Application.TourManagement.DTOs;
using BE_Capstone_Project.Application.TourManagement.Services.Interfaces;
using BE_Capstone_Project.Domain.Enums;
using BE_Capstone_Project.Domain.Models;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BE_Capstone_Project.Application.TourManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TourController : ControllerBase
    {
        private readonly ITourService _tourService;
        private readonly ITourImageService _tourImageService;
        public TourController(ITourService tourService, ITourImageService tourImageService)
        {
            _tourService = tourService;
            _tourImageService = tourImageService;
        }

        [HttpPost("AddTour")]
        public async Task<IActionResult> AddTour([FromForm] TourDTO tour, [FromForm] List<string?> images)
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

            foreach (var image in images)
            {
                var imageToAdd = new TourImage
                {
                    TourId = result,
                    Image = image,
                };

                await _tourImageService.AddTourImage(imageToAdd);
            }

            return Ok(new { message = "Tour added successfully", tourId = result });
        }

        [HttpPost("UpdateTour")]
        public async Task<IActionResult> UpdateTour([FromForm] TourDTO tour)
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

        [HttpGet("GetTourById")]
        public async Task<IActionResult> GetTourById(int id)
        {
            var tour = await _tourService.GetTourById(id);

            if (tour == null)
                return NotFound(new { message = $"No tour found with id {id}" });

            return Ok(new { message = $"Tour with id {id} found successfully", tour });
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
    }
}
