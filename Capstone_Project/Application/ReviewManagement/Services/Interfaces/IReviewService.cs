using BE_Capstone_Project.Application.ReviewManagement.DTOs;
using BE_Capstone_Project.Domain.Models;

namespace BE_Capstone_Project.Application.ReviewManagement.Services.Interfaces
{
    public interface IReviewService
    {
        Task<int> AddReview(Review review);
        Task<bool> UpdateReview(Review review);
        Task<bool> DeleteReview(int reviewId);
        Task<List<Review>> GetReviewsByTourId(int tourId);
        Task<int> GetReviewCountByTourId(int tourId);
        Task<Review?> GetReviewById(int reviewId);
        Task<List<ReviewPopDTO>> GetAllReviewsAsync();
    }
}
