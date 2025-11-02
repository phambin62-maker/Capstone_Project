using BE_Capstone_Project.Application.Bookings.Services;
using BE_Capstone_Project.Application.ReviewManagement.DTOs;
using BE_Capstone_Project.Application.ReviewManagement.Services.Interfaces;
using BE_Capstone_Project.Application.Services;
using BE_Capstone_Project.Domain.Enums;
using BE_Capstone_Project.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BE_Capstone_Project.Application.ReviewManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly IUserService _userService;
        private readonly BookingService _bookingService;
        private readonly ILogger<ReviewController> _logger;
        public ReviewController(IReviewService reviewService, IUserService userService, BookingService bookingService, ILogger<ReviewController> logger) 
        {
            _reviewService = reviewService;
            _userService = userService;
            _bookingService = bookingService;
            _logger = logger;
        }

        [HttpPost("AddReview")]
        public async Task<IActionResult> AddReview([FromBody] ReviewCreateDTO review)
        {
            var user = await _userService.GetUserByUsername(review.Username);
            if (user == null) return BadRequest(new { message = "User does not exist" });

            var booking = await _bookingService.GetBookingByUserIdAndTourId(user.Id, review.TourId);
            if (booking == null) return BadRequest(new { message = "User has not booked this tour" });

            var reviewToAdd = new Review()
            {
                UserId = user.Id,
                TourId = review.TourId,
                BookingId = booking.Id,
                Stars = review.Stars,
                Comment = review.Comment,
                CreatedDate = DateTime.Now,
                ReviewStatus = true,
            };

            var result = await _reviewService.AddReview(reviewToAdd);

            if (result == -1) return BadRequest(new { message = "Failed to add review" });

            return Ok(new { message = "Review added successfully", reviewId = result });
        }

        [HttpPost("EditReview")]
        public async Task<IActionResult> EditReview([FromBody] ReviewUpdateDTO review)
        {
            var reviewToUpdate = await _reviewService.GetReviewById(review.Id);
            if (reviewToUpdate == null) return BadRequest(new { message = "Failed to update review" });

            reviewToUpdate.Stars = review.Stars;
            reviewToUpdate.Comment = review.Comment;

            var result = await _reviewService.UpdateReview(reviewToUpdate);

            if (!result) return BadRequest(new { message = "Failed to update review" });

            return Ok(new { message = "Review updated successfully" });
        }

        [HttpDelete("DeleteReview/{reviewId}")]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            var result = await _reviewService.DeleteReview(reviewId);

            if (!result) return BadRequest(new { message = "Failed to delete review" });

            return Ok(new { message = "Review deleted successfully" });
        }

        [HttpGet("GetReviewsByTourId")]
        public async Task<IActionResult> GetReviewsByTourId(int tourId)
        {
            var reviews = await _reviewService.GetReviewsByTourId(tourId);
            foreach (var r in reviews) Console.WriteLine(r.Comment);
            var reviewCount = await _reviewService.GetReviewCountByTourId(tourId);

            if (reviews == null || reviewCount == 0) return Ok(new { message = "There are no review in the tour" });

            return Ok(new { message = "Review are fetched successfully", reviews, reviewCount });
        }
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllReviews()
        {
            try
            {
                _logger.LogInformation("[ReviewController] 🚀 /api/review/get-all called.");

                var reviews = await _reviewService.GetAllReviewsAsync();

                if (reviews == null || !reviews.Any())
                {
                    _logger.LogWarning("[ReviewController] ⚠️ No reviews found.");
                    return NotFound(new { message = "No reviews available." });
                }

                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ReviewController] ❌ Error while getting reviews.");
                return StatusCode(500, new { message = "Error fetching reviews." });
            }
        }
    }
}
