using BE_Capstone_Project.Infrastructure;
using BE_Capstone_Project.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BE_Capstone_Project.DAO
{
    public class ReviewDAO
    {
        private readonly OtmsdbContext _context;
        public ReviewDAO(OtmsdbContext context)
        {
            _context = context;
        }
        public async Task<int> AddReviewAsync(Review review)
        {
            try
            {
                await _context.Reviews.AddAsync(review);
                await _context.SaveChangesAsync();
                return review.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while adding a review: {ex.Message}");
                return -1;
            }
        }
        public async Task<bool> UpdateReviewAsync(Review review)
        {
            try
            {
                _context.Reviews.Update(review);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while updating the review with ID {review.Id}: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> DeleteReviewByIdAsync(int reviewId)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(reviewId);
                if (review != null)
                {
                    _context.Reviews.Remove(review);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting the review with ID {reviewId}: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Review>> GetAllReviewsAsync()
        {
            return await _context.Reviews
     .Include(r => r.User)
     .Include(r => r.Tour)
     .Select(r => new Review
     {
         Id = r.Id,
         Comment = r.Comment,
         Stars = r.Stars,
         CreatedDate = r.CreatedDate,
         ReviewStatus = r.ReviewStatus,
         User = new User { Username = r.User.Username },
         Tour = new Tour { Name = r.Tour.Name }
     })
     .ToListAsync();
        }
    


        public async Task<Review?> GetReviewByIdAsync(int reviewId)
        {
            try
            {
                return await _context.Reviews.FindAsync(reviewId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving the review with ID {reviewId}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Review>> GetReviewsByTourIdAsync(int tourId)
        {
            try
            {
                return await _context.Reviews
                    .Where(r => r.TourId == tourId)
                    .Include(r => r.User)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving reviews for tour ID {tourId}: {ex.Message}");
                return new List<Review>();
            }
        }

        public async Task<List<Review>> GetReviewsByUserIdAsync(int userId)
        {
            try
            {
                return await _context.Reviews
                    .Where(r => r.UserId == userId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving reviews for user ID {userId}: {ex.Message}");
                return new List<Review>();
            }
        }

        public async Task<List<Review>> GetReviewsByBookingIdAsync(int bookingId)
        {
            try
            {
                return await _context.Reviews
                    .Where(r => r.BookingId == bookingId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving reviews for booking ID {bookingId}: {ex.Message}");
                return new List<Review>();
            }
        }

        public async Task<int> GetReviewCountByTourIdAsync(int tourId)
        {
            try
            {
                return await _context.Reviews
                    .CountAsync(r => r.TourId == tourId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while counting reviews for tour ID {tourId}: {ex.Message}");
                return 0;
            }
        }

        public async Task<double?> GetAverageRatingByTourIdAsync(int tourId)
        {
            try
            {
                return await _context.Reviews
                    .Where(r => r.TourId == tourId && r.Stars.HasValue)
                    .AverageAsync(r => (double?)r.Stars);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while calculating average rating for tour ID {tourId}: {ex.Message}");
                return null;
            }
        }
    }
}
