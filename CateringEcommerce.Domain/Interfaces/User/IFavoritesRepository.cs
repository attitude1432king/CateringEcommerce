using System.Collections.Generic;
using System.Threading.Tasks;
using CateringEcommerce.Domain.Models.User;

namespace CateringEcommerce.BAL.Base.User
{
    /// <summary>
    /// Interface for Favorites Repository
    /// </summary>
    public interface IFavoritesRepository
    {
        /// <summary>
        /// Add a catering to user's favorites
        /// </summary>
        Task<bool> AddFavoriteAsync(long userId, long cateringId);

        /// <summary>
        /// Remove a catering from user's favorites
        /// </summary>
        Task<bool> RemoveFavoriteAsync(long userId, long cateringId);

        /// <summary>
        /// Get all favorites for a user with pagination
        /// </summary>
        Task<(List<FavoriteCateringDto> Favorites, int TotalCount)> GetUserFavoritesAsync(
            long userId,
            int pageNumber = 1,
            int pageSize = 20);

        /// <summary>
        /// Check if a specific catering is in user's favorites
        /// </summary>
        Task<bool> IsFavoriteAsync(long userId, long cateringId);

        /// <summary>
        /// Get total favorites count for a user
        /// </summary>
        Task<int> GetFavoritesCountAsync(long userId);

        /// <summary>
        /// Get favorite status for multiple caterings (batch check)
        /// </summary>
        Task<Dictionary<long, bool>> GetFavoriteStatusAsync(long userId, List<long> cateringIds);

        /// <summary>
        /// Toggle favorite status (add if not exists, remove if exists)
        /// </summary>
        Task<bool> ToggleFavoriteAsync(long userId, long cateringId);
    }
}
