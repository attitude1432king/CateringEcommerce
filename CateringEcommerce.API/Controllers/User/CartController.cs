using CateringEcommerce.API.Helpers;
using CateringEcommerce.Domain.Interfaces.User;
using CateringEcommerce.Domain.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CateringEcommerce.API.Controllers.User
{
    [Authorize]
    [Route("api/User/Cart")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartRepository _cartRepository;
        private readonly ILogger<CartController> _logger;

        public CartController(
            ICartRepository cartRepository,
            ILogger<CartController> logger)
        {
            _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get user's cart
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out long userId))
                {
                    return Unauthorized(new { message = "Invalid user session" });
                }

                var cart = await _cartRepository.GetUserCartAsync(userId);

                if (cart == null)
                {
                    return Ok(new { result = true, message = "Cart is empty", data = (object?)null });
                }

                return Ok(new { result = true, data = cart });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart");
                return StatusCode(500, new { result = false, message = "Failed to get cart" });
            }
        }

        /// <summary>
        /// Add or update cart (replaces existing cart)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto cartDto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out long userId))
                {
                    return Unauthorized(new { message = "Invalid user session" });
                }

                var cartId = await _cartRepository.AddOrUpdateCartAsync(userId, cartDto);

                _logger.LogInformation("Cart saved successfully. UserId: {UserId}, CartId: {CartId}", userId, cartId);

                var cart = await _cartRepository.GetUserCartAsync(userId);

                return Ok(new
                {
                    result = true,
                    message = "Cart saved successfully",
                    data = cart
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding to cart");
                return StatusCode(500, new { result = false, message = "Failed to save cart" });
            }
        }

        /// <summary>
        /// Add an additional food item to cart
        /// </summary>
        [HttpPost("AddItem")]
        public async Task<IActionResult> AddAdditionalItem([FromBody] CartAdditionalItemDto item)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out long userId))
                {
                    return Unauthorized(new { message = "Invalid user session" });
                }

                var success = await _cartRepository.AddAdditionalItemAsync(userId, item);

                if (!success)
                {
                    return BadRequest(new { result = false, message = "Failed to add item. Cart may not exist." });
                }

                var cart = await _cartRepository.GetUserCartAsync(userId);

                return Ok(new
                {
                    result = true,
                    message = "Item added successfully",
                    data = cart
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding additional item");
                return StatusCode(500, new { result = false, message = "Failed to add item" });
            }
        }

        /// <summary>
        /// Remove an additional food item from cart
        /// </summary>
        [HttpDelete("RemoveItem/{foodId}")]
        public async Task<IActionResult> RemoveAdditionalItem(long foodId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out long userId))
                {
                    return Unauthorized(new { message = "Invalid user session" });
                }

                var success = await _cartRepository.RemoveAdditionalItemAsync(userId, foodId);

                if (!success)
                {
                    return NotFound(new { result = false, message = "Item not found in cart" });
                }

                var cart = await _cartRepository.GetUserCartAsync(userId);

                return Ok(new
                {
                    result = true,
                    message = "Item removed successfully",
                    data = cart
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing additional item");
                return StatusCode(500, new { result = false, message = "Failed to remove item" });
            }
        }

        /// <summary>
        /// Clear entire cart
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out long userId))
                {
                    return Unauthorized(new { message = "Invalid user session" });
                }

                var success = await _cartRepository.ClearCartAsync(userId);

                return Ok(new
                {
                    result = true,
                    message = "Cart cleared successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                return StatusCode(500, new { result = false, message = "Failed to clear cart" });
            }
        }

        /// <summary>
        /// Check if user has an active cart
        /// </summary>
        [HttpGet("HasCart")]
        public async Task<IActionResult> HasCart()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out long userId))
                {
                    return Unauthorized(new { message = "Invalid user session" });
                }

                var hasCart = await _cartRepository.HasActiveCartAsync(userId);

                return Ok(new
                {
                    result = true,
                    hasCart
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking cart status");
                return StatusCode(500, new { result = false, message = "Failed to check cart status" });
            }
        }
    }
}
