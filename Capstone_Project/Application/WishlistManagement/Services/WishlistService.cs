using BE_Capstone_Project.Application.WishlistManagement.DTOs;
using BE_Capstone_Project.Application.WishlistManagement.Services.Interfaces;
using BE_Capstone_Project.DAO;
using BE_Capstone_Project.Domain.Models;
using BE_Capstone_Project.Infrastructure;
using Microsoft.EntityFrameworkCore;
using BE_Capstone_Project.Application.Notifications.Services;
using BE_Capstone_Project.Application.Notifications.DTOs;
using BE_Capstone_Project.Domain.Enums;
using System;
using System.Linq;

namespace BE_Capstone_Project.Application.WishlistManagement.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly WishlistDAO _wishlistDAO;
        private readonly OtmsdbContext _context;
        private readonly NotificationService _notificationService;

        public WishlistService(WishlistDAO wishlistDAO, OtmsdbContext context, NotificationService notificationService)
        {
            _wishlistDAO = wishlistDAO;
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<WishlistResponse> AddToWishlistAsync(int userId, int tourId)
        {
            var tour = await _context.Tours
                .Include(t => t.TourImages)
                .FirstOrDefaultAsync(t => t.Id == tourId);

            if (tour == null)
                throw new ArgumentException("Tour not found");

            var existingWishlist = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.TourId == tourId);

            if (existingWishlist != null)
                throw new InvalidOperationException("Tour is already in wishlist");

            var wishlist = new Wishlist
            {
                UserId = userId,
                TourId = tourId
            };

            var wishlistId = await _wishlistDAO.AddWishlistAsync(wishlist);

            if (wishlistId == -1)
                throw new Exception("Failed to add to wishlist");

            try
            {
                var notificationDto = new CreateNotificationDTO
                {
                    UserId = userId,
                    Title = "Added to Wishlist",
                    Message = $"Tour '{tour.Name}' has been added to your wishlist.",
                    NotificationType = NotificationType.System
                };
                await _notificationService.CreateAsync(notificationDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send wishlist notification: {ex.Message}");
            }

            return new WishlistResponse
            {
                Id = wishlistId,
                TourId = tourId,
                TourName = tour.Name,
                TourPrice = tour.Price,
                Duration = tour.Duration,
                TourImage = tour.TourImages.FirstOrDefault()?.Image,
                AddedDate = DateTime.UtcNow
            };
        }

        public async Task<bool> RemoveFromWishlistAsync(int userId, int wishlistId)
        {
            var wishlist = await _wishlistDAO.GetWishlistByIdAsync(wishlistId);
            if (wishlist == null || wishlist.UserId != userId)
                return false;

            return await _wishlistDAO.DeleteWishlistByIdAsync(wishlistId);
        }

        public async Task<bool> RemoveFromWishlistByTourIdAsync(int userId, int tourId)
        {
            var wishlists = await _wishlistDAO.GetWishlistsByUserIdAsync(userId);
            var wishlist = wishlists.FirstOrDefault(w => w.TourId == tourId);

            if (wishlist == null)
                return false;

            return await _wishlistDAO.DeleteWishlistByIdAsync(wishlist.Id);
        }

        public async Task<List<WishlistResponse>> GetUserWishlistAsync(int userId)
        {
            var wishlists = await _context.Wishlists
                .Where(w => w.UserId == userId)
                .Include(w => w.Tour)
                    .ThenInclude(t => t.TourImages)
                .Include(w => w.Tour)
                    .ThenInclude(t => t.Category)
                .OrderByDescending(w => w.Id)
                .ToListAsync();

            return wishlists.Select(w => new WishlistResponse
            {
                Id = w.Id,
                TourId = w.TourId,
                TourName = w.Tour.Name,
                TourPrice = w.Tour.Price,
                Duration = w.Tour.Duration,
                TourImage = w.Tour.TourImages.FirstOrDefault()?.Image,
                AddedDate = DateTime.UtcNow
            }).ToList();
        }

        public async Task<bool> IsTourInWishlistAsync(int userId, int tourId)
        {
            var wishlists = await _wishlistDAO.GetWishlistsByUserIdAsync(userId);
            return wishlists.Any(w => w.TourId == tourId);
        }

        public async Task<WishlistResponse> GetWishlistByIdAsync(int wishlistId)
        {
            var wishlist = await _context.Wishlists
                .Include(w => w.Tour)
                .ThenInclude(t => t.TourImages)
                .FirstOrDefaultAsync(w => w.Id == wishlistId);

            if (wishlist == null)
                return null;

            return new WishlistResponse
            {
                Id = wishlist.Id,
                TourId = wishlist.TourId,
                TourName = wishlist.Tour.Name,
                TourPrice = wishlist.Tour.Price,
                Duration = wishlist.Tour.Duration,
                TourImage = wishlist.Tour.TourImages.FirstOrDefault()?.Image,
                AddedDate = DateTime.UtcNow
            };
        }
    }
}