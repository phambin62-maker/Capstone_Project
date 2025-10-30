using BE_Capstone_Project.Application.ReviewManagement.Services.Interfaces;
using BE_Capstone_Project.DAO;
using BE_Capstone_Project.Domain.Models;

namespace BE_Capstone_Project.Application.ReviewManagement.Services
{
    public class ReviewService : IReviewService
    {
        private readonly ReviewDAO _reviewDAO;
        public ReviewService(ReviewDAO reviewDAO)
        {
            _reviewDAO = reviewDAO;
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
    }
}
