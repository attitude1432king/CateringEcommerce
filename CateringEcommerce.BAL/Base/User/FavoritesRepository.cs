using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using CateringEcommerce.Domain.Models.User;

namespace CateringEcommerce.BAL.Base.User
{
    /// <summary>
    /// Repository for managing user favorites/wishlist
    /// </summary>
    public class FavoritesRepository : IFavoritesRepository
    {
        private readonly string _connectionString;

        public FavoritesRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
        }

        /// <summary>
        /// Add a catering to user's favorites
        /// </summary>
        public async Task<bool> AddFavoriteAsync(long userId, long cateringId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@UserId", userId);
                    parameters.Add("@CateringId", cateringId);

                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "sp_User_AddFavorite",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    return result != null;
                }
            }
            catch (SqlException ex)
            {
                // Handle constraint violations (e.g., foreign key violations)
                if (ex.Number == 547) // Foreign key violation
                {
                    throw new InvalidOperationException("Invalid user ID or catering ID.", ex);
                }
                throw;
            }
        }

        /// <summary>
        /// Remove a catering from user's favorites
        /// </summary>
        public async Task<bool> RemoveFavoriteAsync(long userId, long cateringId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                parameters.Add("@CateringId", cateringId);

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_User_RemoveFavorite",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result?.RowsAffected > 0;
            }
        }

        /// <summary>
        /// Get all favorites for a user with pagination
        /// </summary>
        public async Task<(List<FavoriteCateringDto> Favorites, int TotalCount)> GetUserFavoritesAsync(
            long userId,
            int pageNumber = 1,
            int pageSize = 20)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                parameters.Add("@PageNumber", pageNumber);
                parameters.Add("@PageSize", pageSize);

                using (var multi = await connection.QueryMultipleAsync(
                    "sp_User_GetFavorites",
                    parameters,
                    commandType: CommandType.StoredProcedure))
                {
                    var favorites = (await multi.ReadAsync<FavoriteCateringDto>()).ToList();
                    var totalCount = (await multi.ReadSingleAsync<int>());

                    return (favorites, totalCount);
                }
            }
        }

        /// <summary>
        /// Check if a specific catering is in user's favorites
        /// </summary>
        public async Task<bool> IsFavoriteAsync(long userId, long cateringId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                parameters.Add("@CateringId", cateringId);

                var result = await connection.QuerySingleAsync<int>(
                    "sp_User_IsFavorite",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result == 1;
            }
        }

        /// <summary>
        /// Get total favorites count for a user
        /// </summary>
        public async Task<int> GetFavoritesCountAsync(long userId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);

                var result = await connection.QuerySingleAsync<int>(
                    "sp_User_GetFavoritesCount",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result;
            }
        }

        /// <summary>
        /// Get favorite status for multiple caterings (batch check)
        /// </summary>
        public async Task<Dictionary<long, bool>> GetFavoriteStatusAsync(long userId, List<long> cateringIds)
        {
            if (cateringIds == null || !cateringIds.Any())
            {
                return new Dictionary<long, bool>();
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                parameters.Add("@CateringIds", string.Join(",", cateringIds));

                var results = await connection.QueryAsync<FavoriteStatusDto>(
                    "sp_User_GetFavoriteStatus",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return results.ToDictionary(r => r.CateringId, r => r.IsFavorite);
            }
        }

        /// <summary>
        /// Toggle favorite status (add if not exists, remove if exists)
        /// </summary>
        public async Task<bool> ToggleFavoriteAsync(long userId, long cateringId)
        {
            var isFavorite = await IsFavoriteAsync(userId, cateringId);

            if (isFavorite)
            {
                await RemoveFavoriteAsync(userId, cateringId);
                return false; // Removed
            }
            else
            {
                await AddFavoriteAsync(userId, cateringId);
                return true; // Added
            }
        }
    }

    /// <summary>
    /// DTO for batch favorite status check
    /// </summary>
    public class FavoriteStatusDto
    {
        public long CateringId { get; set; }
        public bool IsFavorite { get; set; }
    }
}
