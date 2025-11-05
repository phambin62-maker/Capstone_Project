
using BE_Capstone_Project.Application.ReviewManagement.DTOs;
using BE_Capstone_Project.Application.ReviewManagement.Services.Interfaces;
using BE_Capstone_Project.DAO;
using BE_Capstone_Project.Domain.Models;

namespace BE_Capstone_Project.Application.ReviewManagement.Services
{
    public class ReviewService : IReviewService
    {
        private readonly ReviewDAO _reviewDAO;
        private readonly ILogger<ReviewService> _logger = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        }).CreateLogger<ReviewService>();
        public ReviewService(ReviewDAO reviewDAO, ILogger<ReviewService> logger)
        {
            _reviewDAO = reviewDAO;
            _logger = logger;
        }

        public async Task<int> AddReview(Review review)
        {
            return await _reviewDAO.AddReviewAsync(review);
        }

        public async Task<bool> UpdateReview(Review review)
        {
            return await _reviewDAO.UpdateReviewAsync(review);
        }

        public async Task<bool> DeleteReview(int reviewId)
        {
            return await _reviewDAO.DeleteReviewByIdAsync(reviewId);
        }

        public async Task<List<Review>> GetReviewsByTourId(int tourId)
        {
            return await _reviewDAO.GetReviewsByTourIdAsync(tourId);
        }

        public async Task<int> GetReviewCountByTourId(int tourId)
        {
            return await _reviewDAO.GetReviewCountByTourIdAsync(tourId);
        }

        public async Task<Review?> GetReviewById(int reviewId)
        {
            return await _reviewDAO.GetReviewByIdAsync(reviewId);
        }
        public async Task<List<ReviewPopDTO>> GetAllReviewsAsync()
        {
            _logger.LogInformation("[ReviewService]  Fetching all reviews...");

            var reviews = await _reviewDAO.GetAllReviewsAsync();

            var result = reviews.Select(r => new ReviewPopDTO
            {
                Id = r.Id,
                UserName = r.User?.Username,
                TourName = r.Tour?.Name,
                Stars = r.Stars,
                Comment = r.Comment,
                CreatedDate = r.CreatedDate
            }).ToList();

            _logger.LogInformation($"[ReviewService]  Retrieved {result.Count} reviews.");
            return result;
        }
        public async Task<List<TourRatingDTO>> GetTourPopAsync()
        {
            return await _reviewDAO.GetTourRatingsAsync();
        }
    }
}
