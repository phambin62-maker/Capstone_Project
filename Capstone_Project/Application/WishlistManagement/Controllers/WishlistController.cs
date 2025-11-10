using BE_Capstone_Project.Application.BookingManagement.Services;
using BE_Capstone_Project.Application.Services;
using BE_Capstone_Project.Application.WishlistManagement.DTOs;
using BE_Capstone_Project.Application.WishlistManagement.Services.Interfaces;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BE_Capstone_Project.Application.WishlistManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistService _wishlistService;
        private readonly IUserService _userService;

        public WishlistController(IWishlistService wishlistService, IUserService userService)
        {
            _wishlistService = wishlistService;
            _userService = userService;
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
        public async Task<IActionResult> GetUserWishlist([FromQuery] string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return BadRequest("Username is required.");

            try
            {
                var user = await _userService.GetUserByUsername(username);
                if (user == null)
                    return NotFound("User not found.");

                var wishlist = await _wishlistService.GetUserWishlistAsync(user.Id);

                return Ok(new
                {
                    message = $"Wishlist for user '{username}' retrieved successfully.",
                    wishlist
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<WishlistResponse>> AddToWishlist([FromBody] AddWishlistRequest request)
        {
            try
            {
                var user = await _userService.GetUserByUsername(request.Username);
                var wishlistItem = await _wishlistService.AddToWishlistAsync(user.Id, request.TourId);
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
        public async Task<ActionResult> RemoveFromWishlistByTourId(int tourId, [FromQuery] string username)
        {
            try
            {
                var user = await _userService.GetUserByUsername(username);
                var result = await _wishlistService.RemoveFromWishlistByTourIdAsync(user.Id, tourId);

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
        public async Task<IActionResult> CheckTourInWishlist(int tourId, [FromQuery] string username)
        {
            try
            {
                Console.WriteLine($"Received username: '{username}'");
                var user = await _userService.GetUserByUsername(username);
                Console.WriteLine(" User fetched: " + (user != null ? user.Username : "null"));
                var isInWishlist = await _wishlistService.IsTourInWishlistAsync(user.Id, tourId);
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
