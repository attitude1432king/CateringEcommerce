using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using CateringEcommerce.Domain.Models.User;
using CateringEcommerce.BAL.Configuration;

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
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    var query = $@"
                        INSERT INTO {Table.SysUserFavorites}
                        (
                            c_userid,
                            c_ownerid,
                            c_added_date,
                            c_is_active,
                            c_removed_date,
                            c_createddate
                        )
                        VALUES
                        (
                            @UserId,
                            @CateringId,
                            NOW(),
                            TRUE,
                            NULL,
                            NOW()
                        )
                        ON CONFLICT (c_userid, c_ownerid)
                        DO UPDATE SET
                            c_is_active = TRUE,
                            c_added_date = NOW(),
                            c_removed_date = NULL
                        RETURNING c_favorite_id;";

                    var result = await connection.ExecuteScalarAsync<long?>(query, new
                    {
                        UserId = userId,
                        CateringId = cateringId
                    });

                    return result.HasValue;
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
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
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var query = $@"
                    UPDATE {Table.SysUserFavorites}
                    SET c_is_active = FALSE,
                        c_removed_date = NOW()
                    WHERE c_userid = @UserId
                      AND c_ownerid = @CateringId
                      AND c_is_active = TRUE;";

                var rowsAffected = await connection.ExecuteAsync(query, new
                {
                    UserId = userId,
                    CateringId = cateringId
                });

                return rowsAffected > 0;
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
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var offset = Math.Max(0, (pageNumber - 1) * pageSize);
                var query = $@"
                    SELECT
                        f.c_favorite_id AS FavoriteId,
                        f.c_ownerid AS CateringId,
                        c.c_catering_name AS CateringName,
                        c.c_logo_path AS LogoUrl,
                        COALESCE((
                            SELECT AVG(r.c_rating::double precision)
                            FROM t_sys_reviews r
                            WHERE r.c_ownerid = c.c_ownerid
                              AND r.c_is_approved = TRUE
                        ), 0) AS AverageRating,
                        COALESCE(c.c_min_order_value, 0) AS MinOrderValue,
                        COALESCE(c.c_is_online, FALSE) AS IsOnline,
                        f.c_added_date AS AddedDate
                    FROM {Table.SysUserFavorites} f
                    INNER JOIN {Table.SysCateringOwner} c ON f.c_ownerid = c.c_ownerid
                    WHERE f.c_userid = @UserId
                      AND f.c_is_active = TRUE
                      AND c.c_is_active = TRUE
                    ORDER BY f.c_added_date DESC
                    LIMIT @PageSize OFFSET @Offset;

                    SELECT COUNT(*)
                    FROM {Table.SysUserFavorites} f
                    INNER JOIN {Table.SysCateringOwner} c ON f.c_ownerid = c.c_ownerid
                    WHERE f.c_userid = @UserId
                      AND f.c_is_active = TRUE
                      AND c.c_is_active = TRUE;";

                using (var multi = await connection.QueryMultipleAsync(query, new
                {
                    UserId = userId,
                    PageSize = pageSize,
                    Offset = offset
                }))
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
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var query = $@"
                    SELECT EXISTS (
                        SELECT 1
                        FROM {Table.SysUserFavorites}
                        WHERE c_userid = @UserId
                          AND c_ownerid = @CateringId
                          AND c_is_active = TRUE
                    );";

                return await connection.QuerySingleAsync<bool>(query, new
                {
                    UserId = userId,
                    CateringId = cateringId
                });
            }
        }

        /// <summary>
        /// Get total favorites count for a user
        /// </summary>
        public async Task<int> GetFavoritesCountAsync(long userId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var query = $@"
                    SELECT COUNT(*)
                    FROM {Table.SysUserFavorites}
                    WHERE c_userid = @UserId
                      AND c_is_active = TRUE;";

                return await connection.QuerySingleAsync<int>(query, new { UserId = userId });
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

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var query = $@"
                    SELECT
                        ids.catering_id AS CateringId,
                        EXISTS (
                            SELECT 1
                            FROM {Table.SysUserFavorites} f
                            WHERE f.c_userid = @UserId
                              AND f.c_ownerid = ids.catering_id
                              AND f.c_is_active = TRUE
                        ) AS IsFavorite
                    FROM UNNEST(@CateringIds) AS ids(catering_id);";

                var results = await connection.QueryAsync<FavoriteStatusDto>(query, new
                {
                    UserId = userId,
                    CateringIds = cateringIds.ToArray()
                });

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
