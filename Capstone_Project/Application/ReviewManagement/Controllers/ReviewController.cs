using BE_Capstone_Project.Application.ReviewManagement.DTOs;
using BE_Capstone_Project.Domain.Models;
using BE_Capstone_Project.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BE_Capstone_Project.Application.ReviewManagement.Services.Interfaces;

namespace BE_Capstone_Project.Application.ReviewManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        public ReviewController(IReviewService reviewService) 
        {
            _reviewService = reviewService;
        }

        [HttpPost("AddReview")]
        public async Task<IActionResult> AddReview(ReviewDTO review)
        {
            var reviewToAdd = new Review()
            {
                UserId = review.UserId,
                TourId = review.TourId,
                BookingId = review.BookingId,
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
        public async Task<IActionResult> EditReview(ReviewDTO review)
        {
            var reviewToUpdate = await _reviewService.GetReviewById(review.Id);
            if (reviewToUpdate == null) return BadRequest(new { message = "Failed to update review" });

            reviewToUpdate.Stars = review.Stars;
            reviewToUpdate.Comment = review.Comment;

            var result = await _reviewService.UpdateReview(reviewToUpdate);

            if (!result) return BadRequest(new { message = "Failed to update review" });

            return Ok(new { message = "Review updated successfully" });
        }

        [HttpDelete("DeleteReview")]
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
    }
}
