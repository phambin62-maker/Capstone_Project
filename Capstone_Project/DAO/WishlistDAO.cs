using BE_Capstone_Project.Infrastructure;
using BE_Capstone_Project.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BE_Capstone_Project.DAO
{
    public class WishlistDAO
    {
        private readonly OtmsdbContext _context;
        public WishlistDAO(OtmsdbContext context)
        {
            _context = context;
        }

        public async Task<int> AddWishlistAsync(Wishlist wishlist)
        {
            try
            {
                await _context.Wishlists.AddAsync(wishlist);
                await _context.SaveChangesAsync();
                return wishlist.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while adding a wishlist: {ex.Message}");
                return -1;
            }
        }

        public async Task<bool> UpdateWishlistAsync(Wishlist wishlist)
        {
            try
            {
                _context.Wishlists.Update(wishlist);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while updating the wishlist with ID {wishlist.Id}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteWishlistByIdAsync(int wishlistId)
        {
            try
            {
                var wishlist = await _context.Wishlists.FindAsync(wishlistId);
                if (wishlist != null)
                {
                    _context.Wishlists.Remove(wishlist);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting the wishlist with ID {wishlistId}: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Wishlist>> GetAllWishlistsAsync()
        {
            try
            {
                return await _context.Wishlists.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving all wishlists: {ex.Message}");
                return new List<Wishlist>();
            }
        }

        public async Task<Wishlist?> GetWishlistByIdAsync(int wishlistId)
        {
            try
            {
                return await _context.Wishlists.FindAsync(wishlistId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving the wishlist with ID {wishlistId}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Wishlist>> GetWishlistsByUserIdAsync(int userId)
        {
            try
            {
                return await _context.Wishlists
                    .Where(w => w.UserId == userId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving wishlists for user ID {userId}: {ex.Message}");
                return new List<Wishlist>();
            }
        }

        public async Task<List<Wishlist>> GetWishlistsByTourIdAsync(int tourId)
        {
            try
            {
                return await _context.Wishlists
                    .Where(w => w.TourId == tourId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving wishlists for tour ID {tourId}: {ex.Message}");
                return new List<Wishlist>();
            }
        }
    }
}
