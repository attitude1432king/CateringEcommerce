using CateringEcommerce.Domain.Models.User;
using System.Threading.Tasks;

namespace CateringEcommerce.Domain.Interfaces.User
{
    /// <summary>
    /// Repository interface for user cart operations
    /// </summary>
    public interface ICartRepository
    {
        /// <summary>
        /// Add or replace cart for user (only one cart allowed per user)
        /// </summary>
        Task<long> AddOrUpdateCartAsync(long userId, AddToCartDto cartDto);

        /// <summary>
        /// Get user's cart
        /// </summary>
        Task<CartResponseDto?> GetUserCartAsync(long userId);

        /// <summary>
        /// Add an additional food item to cart
        /// </summary>
        Task<bool> AddAdditionalItemAsync(long userId, CartAdditionalItemDto item);

        /// <summary>
        /// Remove an additional food item from cart
        /// </summary>
        Task<bool> RemoveAdditionalItemAsync(long userId, long foodId);

        /// <summary>
        /// Clear user's cart completely
        /// </summary>
        Task<bool> ClearCartAsync(long userId);

        /// <summary>
        /// Check if user has an active cart
        /// </summary>
        Task<bool> HasActiveCartAsync(long userId);
    }
}
