using BE_Capstone_Project.Application.TourManagement.DTOs;

namespace BE_Capstone_Project.Application.TourManagement.Services.Interfaces
{
    public interface IWishlistService
    {
        Task<WishlistResponse> AddToWishlistAsync(int userId, int tourId);
        Task<bool> RemoveFromWishlistAsync(int userId, int wishlistId);
        Task<bool> RemoveFromWishlistByTourIdAsync(int userId, int tourId);
        Task<List<WishlistResponse>> GetUserWishlistAsync(int userId);
        Task<bool> IsTourInWishlistAsync(int userId, int tourId);
        Task<WishlistResponse> GetWishlistByIdAsync(int wishlistId);
    }
}
