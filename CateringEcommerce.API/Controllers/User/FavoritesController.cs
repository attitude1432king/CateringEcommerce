using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CateringEcommerce.BAL.Base.User;
using CateringEcommerce.Domain.Models.User;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;

namespace CateringEcommerce.API.Controllers.User
{
    /// <summary>
    /// Controller for managing user favorites/wishlist
    /// </summary>
    [Route("api/User/[controller]")]
    [ApiController]
    [Authorize]
    public class FavoritesController : ControllerBase
    {
        private readonly IFavoritesRepository _favoritesRepository;

        public FavoritesController(IFavoritesRepository favoritesRepository)
        {
            _favoritesRepository = favoritesRepository;
        }

        /// <summary>
        /// Get current user ID from JWT claims
        /// </summary>
        private long GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out long userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token");
            }
            return userId;
        }

        /// <summary>
        /// Add a catering to user's favorites
        /// </summary>
        /// <param name="request">Contains catering ID</param>
        /// <returns>Success response</returns>
        [HttpPost("Add")]
        public async Task<IActionResult> AddFavorite([FromBody] FavoriteRequest request)
        {
            try
            {
                if (request.CateringId <= 0)
                {
                    return BadRequest(new { result = false, message = "Invalid catering ID" });
                }

                var userId = GetCurrentUserId();
                var result = await _favoritesRepository.AddFavoriteAsync(userId, request.CateringId);

                if (result)
                {
                    return Ok(new
                    {
                        result = true,
                        message = "Added to favorites successfully",
                        data = new { userId, cateringId = request.CateringId }
                    });
                }

                return BadRequest(new { result = false, message = "Failed to add to favorites" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { result = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { result = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { result = false, message = "An error occurred while adding to favorites", error = ex.Message });
            }
        }

        /// <summary>
        /// Remove a catering from user's favorites
        /// </summary>
        /// <param name="cateringId">Catering ID to remove</param>
        /// <returns>Success response</returns>
        [HttpDelete("{cateringId}")]
        public async Task<IActionResult> RemoveFavorite(long cateringId)
        {
            try
            {
                if (cateringId <= 0)
                {
                    return BadRequest(new { result = false, message = "Invalid catering ID" });
                }

                var userId = GetCurrentUserId();
                var result = await _favoritesRepository.RemoveFavoriteAsync(userId, cateringId);

                if (result)
                {
                    return Ok(new
                    {
                        result = true,
                        message = "Removed from favorites successfully",
                        data = new { userId, cateringId }
                    });
                }

                return NotFound(new { result = false, message = "Favorite not found" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { result = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { result = false, message = "An error occurred while removing from favorites", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all favorites for the current user with pagination
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20)</param>
        /// <returns>Paginated favorites list</returns>
        [HttpGet]
        public async Task<IActionResult> GetFavorites([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
                {
                    return BadRequest(new { result = false, message = "Invalid pagination parameters" });
                }

                var userId = GetCurrentUserId();
                var (favorites, totalCount) = await _favoritesRepository.GetUserFavoritesAsync(userId, pageNumber, pageSize);

                return Ok(new
                {
                    result = true,
                    message = "Favorites retrieved successfully",
                    data = new
                    {
                        favorites,
                        pagination = new
                        {
                            currentPage = pageNumber,
                            pageSize,
                            totalCount,
                            totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                        }
                    }
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { result = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { result = false, message = "An error occurred while retrieving favorites", error = ex.Message });
            }
        }

        /// <summary>
        /// Check if a specific catering is in user's favorites
        /// </summary>
        /// <param name="cateringId">Catering ID to check</param>
        /// <returns>Boolean indicating favorite status</returns>
        [HttpGet("Check/{cateringId}")]
        public async Task<IActionResult> IsFavorite(long cateringId)
        {
            try
            {
                if (cateringId <= 0)
                {
                    return BadRequest(new { result = false, message = "Invalid catering ID" });
                }

                var userId = GetCurrentUserId();
                var isFavorite = await _favoritesRepository.IsFavoriteAsync(userId, cateringId);

                return Ok(new
                {
                    result = true,
                    data = new { cateringId, isFavorite }
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { result = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { result = false, message = "An error occurred while checking favorite status", error = ex.Message });
            }
        }

        /// <summary>
        /// Get total favorites count for the current user
        /// </summary>
        /// <returns>Total count</returns>
        [HttpGet("Count")]
        public async Task<IActionResult> GetFavoritesCount()
        {
            try
            {
                var userId = GetCurrentUserId();
                var count = await _favoritesRepository.GetFavoritesCountAsync(userId);

                return Ok(new
                {
                    result = true,
                    data = new { count }
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { result = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { result = false, message = "An error occurred while retrieving favorites count", error = ex.Message });
            }
        }

        /// <summary>
        /// Get favorite status for multiple caterings (batch check)
        /// </summary>
        /// <param name="request">List of catering IDs</param>
        /// <returns>Dictionary of catering IDs to favorite status</returns>
        [HttpPost("Status")]
        public async Task<IActionResult> GetFavoriteStatus([FromBody] BatchFavoriteRequest request)
        {
            try
            {
                if (request.CateringIds == null || request.CateringIds.Count == 0)
                {
                    return BadRequest(new { result = false, message = "Catering IDs list cannot be empty" });
                }

                if (request.CateringIds.Count > 50)
                {
                    return BadRequest(new { result = false, message = "Maximum 50 catering IDs allowed per request" });
                }

                var userId = GetCurrentUserId();
                var statusDict = await _favoritesRepository.GetFavoriteStatusAsync(userId, request.CateringIds);

                return Ok(new
                {
                    result = true,
                    data = statusDict
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { result = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { result = false, message = "An error occurred while retrieving favorite status", error = ex.Message });
            }
        }

        /// <summary>
        /// Toggle favorite status (add if not exists, remove if exists)
        /// </summary>
        /// <param name="request">Contains catering ID</param>
        /// <returns>New favorite status</returns>
        [HttpPost("Toggle")]
        public async Task<IActionResult> ToggleFavorite([FromBody] FavoriteRequest request)
        {
            try
            {
                if (request.CateringId <= 0)
                {
                    return BadRequest(new { result = false, message = "Invalid catering ID" });
                }

                var userId = GetCurrentUserId();
                var isNowFavorite = await _favoritesRepository.ToggleFavoriteAsync(userId, request.CateringId);

                return Ok(new
                {
                    result = true,
                    message = isNowFavorite ? "Added to favorites" : "Removed from favorites",
                    data = new { cateringId = request.CateringId, isFavorite = isNowFavorite }
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { result = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { result = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { result = false, message = "An error occurred while toggling favorite", error = ex.Message });
            }
        }
    }

    /// <summary>
    /// Request model for adding/toggling favorite
    /// </summary>
    public class FavoriteRequest
    {
        public long CateringId { get; set; }
    }

    /// <summary>
    /// Request model for batch favorite status check
    /// </summary>
    public class BatchFavoriteRequest
    {
        public List<long> CateringIds { get; set; }
    }
}
