using BE_Capstone_Project.Application.TourManagement.DTOs;
using BE_Capstone_Project.Application.TourManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BE_Capstone_Project.Application.TourManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid user ID");
            }
            return userId;
        }

        [HttpGet]
        public async Task<ActionResult<List<WishlistResponse>>> GetUserWishlist()
        {
            try
            {
                var userId = GetCurrentUserId();
                var wishlist = await _wishlistService.GetUserWishlistAsync(userId);
                return Ok(wishlist);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<WishlistResponse>> AddToWishlist([FromBody] AddWishlistRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var wishlistItem = await _wishlistService.AddToWishlistAsync(userId, request.TourId);
                return Ok(wishlistItem);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{wishlistId}")]
        public async Task<ActionResult> RemoveFromWishlist(int wishlistId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _wishlistService.RemoveFromWishlistAsync(userId, wishlistId);

                if (!result)
                    return NotFound("Wishlist item not found");

                return Ok(new { message = "Removed from wishlist successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("tour/{tourId}")]
        public async Task<ActionResult> RemoveFromWishlistByTourId(int tourId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _wishlistService.RemoveFromWishlistByTourIdAsync(userId, tourId);

                if (!result)
                    return NotFound("Tour not found in wishlist");

                return Ok(new { message = "Removed from wishlist successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("check/{tourId}")]
        public async Task<ActionResult<bool>> CheckTourInWishlist(int tourId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var isInWishlist = await _wishlistService.IsTourInWishlistAsync(userId, tourId);
                return Ok(isInWishlist);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{wishlistId}")]
        public async Task<ActionResult<WishlistResponse>> GetWishlistItem(int wishlistId)
        {
            try
            {
                var wishlistItem = await _wishlistService.GetWishlistByIdAsync(wishlistId);

                if (wishlistItem == null)
                    return NotFound("Wishlist item not found");

                return Ok(wishlistItem);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
