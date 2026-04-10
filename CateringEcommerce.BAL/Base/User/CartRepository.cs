using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.User;
using CateringEcommerce.Domain.Models.User;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace CateringEcommerce.BAL.Base.User
{
    public class CartRepository : ICartRepository
    {
        private readonly IDatabaseHelper _dbHelper;

        public CartRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        }

        public async Task<long> AddOrUpdateCartAsync(long userId, AddToCartDto cartDto)
        {
            try
            {
                long? primaryDecorationId = cartDto.DecorationId;

                // First, check if cart exists and delete it (only one cart per user)
                await ClearCartAsync(userId);

                // Insert new cart
                var query = $@"
                    INSERT INTO {Table.SysUserCart}
                    (c_userid, c_ownerid, c_packageid, c_guest_count, c_event_date, c_event_type,
                     c_event_location, c_special_requirements, c_decoration_id, c_base_amount,
                     c_decoration_amount, c_tax_amount, c_total_amount, c_createddate)
                    OUTPUT INSERTED.c_cartid
                    VALUES
                    (@UserId, @CateringId, @PackageId, @GuestCount, @EventDate, @EventType,
                     @EventLocation, @SpecialRequirements, @DecorationId, @BaseAmount,
                     @DecorationAmount, @TaxAmount, @TotalAmount, GETDATE())";

                var parameters = new[]
                {
                    new SqlParameter("@UserId", userId),
                    new SqlParameter("@CateringId", cartDto.CateringId),
                    new SqlParameter("@PackageId", (object?)cartDto.PackageId ?? DBNull.Value),
                    new SqlParameter("@GuestCount", cartDto.GuestCount),
                    new SqlParameter("@EventDate", (object?)cartDto.EventDate ?? DBNull.Value),
                    new SqlParameter("@EventType", (object?)cartDto.EventType ?? DBNull.Value),
                    new SqlParameter("@EventLocation", (object?)cartDto.EventLocation ?? DBNull.Value),
                    new SqlParameter("@SpecialRequirements", (object?)cartDto.SpecialRequirements ?? DBNull.Value),
                    new SqlParameter("@DecorationId", (object?)primaryDecorationId ?? DBNull.Value),
                    new SqlParameter("@BaseAmount", cartDto.BaseAmount),
                    new SqlParameter("@DecorationAmount", cartDto.DecorationAmount),
                    new SqlParameter("@TaxAmount", cartDto.TaxAmount),
                    new SqlParameter("@TotalAmount", cartDto.TotalAmount)
                };

                var dt = await Task.Run(() => _dbHelper.Execute(query, parameters));

                if (dt.Rows.Count > 0)
                {
                    var cartId = Convert.ToInt64(dt.Rows[0][0]);

                    // Add additional items
                    foreach (var item in cartDto.AdditionalItems)
                    {
                        await AddAdditionalItemInternalAsync(cartId, item);
                    }

                    foreach (var decoration in cartDto.StandaloneDecorations)
                    {
                        await AddDecorationInternalAsync(cartId, decoration);
                    }

                    return cartId;
                }

                throw new Exception("Failed to create cart");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding cart: {ex.Message}", ex);
            }
        }

        public async Task<CartResponseDto?> GetUserCartAsync(long userId)
        {
            try
            {
                var query = $@"
                    SELECT
                        c.*,
                        co.c_restaurant_name as CateringName,
                        co.c_logo as CateringLogo,
                        p.c_package_name as PackageName
                    FROM {Table.SysUserCart} c
                    LEFT JOIN {Table.SysCateringOwner} co ON c.c_ownerid = co.c_ownerid
                    LEFT JOIN {Table.SysMenuPackage} p ON c.c_packageid = p.c_packageid
                    WHERE c.c_userid = @UserId";

                var parameters = new[]
                {
                    new SqlParameter("@UserId", userId)
                };

                var dt = await Task.Run(() => _dbHelper.Execute(query, parameters));

                if (dt.Rows.Count == 0)
                    return null;

                var row = dt.Rows[0];
                var cartId = Convert.ToInt64(row["c_cartid"]);

                var cart = new CartResponseDto
                {
                    CartId = cartId,
                    UserId = Convert.ToInt64(row["c_userid"]),
                    CateringId = Convert.ToInt64(row["c_ownerid"]),
                    CateringName = row["CateringName"]?.ToString() ?? string.Empty,
                    CateringLogo = row["CateringLogo"] != DBNull.Value ? row["CateringLogo"]?.ToString() : null,
                    PackageId = row["c_packageid"] != DBNull.Value ? Convert.ToInt64(row["c_packageid"]) : null,
                    PackageName = row["PackageName"] != DBNull.Value ? row["PackageName"]?.ToString() : null,
                    GuestCount = Convert.ToInt32(row["c_guest_count"]),
                    EventDate = row["c_event_date"] != DBNull.Value ? Convert.ToDateTime(row["c_event_date"]) : null,
                    EventType = row["c_event_type"] != DBNull.Value ? row["c_event_type"]?.ToString() : null,
                    EventLocation = row["c_event_location"] != DBNull.Value ? row["c_event_location"]?.ToString() : null,
                    SpecialRequirements = row["c_special_requirements"] != DBNull.Value ? row["c_special_requirements"]?.ToString() : null,
                    BaseAmount = Convert.ToDecimal(row["c_base_amount"]),
                    DecorationAmount = Convert.ToDecimal(row["c_decoration_amount"]),
                    TaxAmount = Convert.ToDecimal(row["c_tax_amount"]),
                    TotalAmount = Convert.ToDecimal(row["c_total_amount"]),
                    CreatedDate = Convert.ToDateTime(row["c_createddate"]),
                    ModifiedDate = row["c_modifieddate"] != DBNull.Value ? Convert.ToDateTime(row["c_modifieddate"]) : null
                };

                // Get additional items
                cart.AdditionalItems = await GetCartAdditionalItemsAsync(cartId);
                cart.StandaloneDecorations = await GetCartDecorationsAsync(cartId);
                cart.DecorationId = row["c_decoration_id"] != DBNull.Value ? Convert.ToInt64(row["c_decoration_id"]) : null;

                if (cart.DecorationId.HasValue)
                {
                    var primaryDecoration = await GetPrimaryDecorationAsync(cart.DecorationId.Value);
                    cart.DecorationName = primaryDecoration?.Name;
                    cart.DecorationPrice = primaryDecoration?.Price ?? 0;
                }

                return cart;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting cart: {ex.Message}", ex);
            }
        }

        public async Task<bool> AddAdditionalItemAsync(long userId, CartAdditionalItemDto item)
        {
            try
            {
                var cart = await GetUserCartAsync(userId);
                if (cart == null)
                    return false;

                return await AddAdditionalItemInternalAsync(cart.CartId, item);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding additional item: {ex.Message}", ex);
            }
        }

        public async Task<bool> RemoveAdditionalItemAsync(long userId, long foodId)
        {
            try
            {
                var cart = await GetUserCartAsync(userId);
                if (cart == null)
                    return false;

                var query = $@"
                    DELETE FROM {Table.SysCartFoodItems}
                    WHERE c_cartid = @CartId AND c_foodid = @FoodId";

                var parameters = new[]
                {
                    new SqlParameter("@CartId", cart.CartId),
                    new SqlParameter("@FoodId", foodId)
                };

                var result = await Task.Run(() => _dbHelper.ExecuteNonQuery(query, parameters));

                return result > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error removing additional item: {ex.Message}", ex);
            }
        }

        public async Task<bool> ClearCartAsync(long userId)
        {
            try
            {
                var query = $@"
                    DELETE FROM {Table.SysUserCart}
                    WHERE c_userid = @UserId";

                var parameters = new[]
                {
                    new SqlParameter("@UserId", userId)
                };

                var result = await Task.Run(() => _dbHelper.ExecuteNonQuery(query, parameters));

                return result >= 0; // 0 is OK (no cart to clear)
            }
            catch (Exception ex)
            {
                throw new Exception($"Error clearing cart: {ex.Message}", ex);
            }
        }

        public async Task<bool> HasActiveCartAsync(long userId)
        {
            try
            {
                var cart = await GetUserCartAsync(userId);
                return cart != null;
            }
            catch
            {
                return false;
            }
        }

        // ===================================
        // PRIVATE HELPER METHODS
        // ===================================

        private async Task<List<CartAdditionalItemResponseDto>> GetCartAdditionalItemsAsync(long cartId)
        {
            try
            {
                var query = $@"
                    SELECT
                        cf.c_cart_item_id,
                        cf.c_foodid,
                        cf.c_quantity,
                        cf.c_price,
                        f.c_item_name as FoodName
                    FROM {Table.SysCartFoodItems} cf
                    LEFT JOIN {Table.SysFoodItems} f ON cf.c_foodid = f.c_foodid
                    WHERE cf.c_cartid = @CartId";

                var parameters = new[]
                {
                    new SqlParameter("@CartId", cartId)
                };

                var dt = await Task.Run(() => _dbHelper.Execute(query, parameters));

                var items = new List<CartAdditionalItemResponseDto>();

                foreach (DataRow row in dt.Rows)
                {
                    items.Add(new CartAdditionalItemResponseDto
                    {
                        CartItemId = Convert.ToInt64(row["c_cart_item_id"]),
                        FoodId = Convert.ToInt64(row["c_foodid"]),
                        FoodName = row["FoodName"]?.ToString() ?? string.Empty,
                        Quantity = Convert.ToInt32(row["c_quantity"]),
                        Price = Convert.ToDecimal(row["c_price"])
                    });
                }

                return items;
            }
            catch
            {
                return new List<CartAdditionalItemResponseDto>();
            }
        }

        private async Task<List<CartDecorationDto>> GetCartDecorationsAsync(long cartId)
        {
            try
            {
                var query = $@"
                    SELECT
                        cd.c_decoration_id,
                        d.c_decoration_name,
                        cd.c_price
                    FROM {Table.SysCartDecorations} cd
                    INNER JOIN {Table.SysCateringDecorations} d ON cd.c_decoration_id = d.c_decoration_id
                    WHERE cd.c_cartid = @CartId";

                var parameters = new[]
                {
                    new SqlParameter("@CartId", cartId)
                };

                var dt = await Task.Run(() => _dbHelper.Execute(query, parameters));
                var decorations = new List<CartDecorationDto>();

                foreach (DataRow row in dt.Rows)
                {
                    decorations.Add(new CartDecorationDto
                    {
                        DecorationId = Convert.ToInt64(row["c_decoration_id"]),
                        Name = row["c_decoration_name"]?.ToString(),
                        Price = Convert.ToDecimal(row["c_price"])
                    });
                }

                return decorations;
            }
            catch
            {
                return new List<CartDecorationDto>();
            }
        }

        private async Task<CartDecorationDto?> GetPrimaryDecorationAsync(long decorationId)
        {
            try
            {
                var query = $@"
                    SELECT TOP 1
                        d.c_decoration_id,
                        d.c_decoration_name,
                        d.c_price
                    FROM {Table.SysCateringDecorations} d
                    WHERE d.c_decoration_id = @DecorationId";

                var parameters = new[]
                {
                    new SqlParameter("@DecorationId", decorationId)
                };

                var dt = await Task.Run(() => _dbHelper.Execute(query, parameters));

                if (dt.Rows.Count == 0)
                {
                    return null;
                }

                var row = dt.Rows[0];
                return new CartDecorationDto
                {
                    DecorationId = Convert.ToInt64(row["c_decoration_id"]),
                    Name = row["c_decoration_name"]?.ToString(),
                    Price = row["c_price"] != DBNull.Value ? Convert.ToDecimal(row["c_price"]) : 0
                };
            }
            catch
            {
                return null;
            }
        }

        private async Task<bool> AddAdditionalItemInternalAsync(long cartId, CartAdditionalItemDto item)
        {
            try
            {
                var query = $@"
                    INSERT INTO {Table.SysCartFoodItems}
                    (c_cartid, c_foodid, c_quantity, c_price, c_createddate)
                    VALUES (@CartId, @FoodId, @Quantity, @Price, GETDATE())";

                var parameters = new[]
                {
                    new SqlParameter("@CartId", cartId),
                    new SqlParameter("@FoodId", item.FoodId),
                    new SqlParameter("@Quantity", item.Quantity),
                    new SqlParameter("@Price", item.Price)
                };

                var result = await Task.Run(() => _dbHelper.ExecuteNonQuery(query, parameters));

                return result > 0;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> AddDecorationInternalAsync(long cartId, CartDecorationDto decoration)
        {
            try
            {
                var query = $@"
                    INSERT INTO {Table.SysCartDecorations}
                    (c_cartid, c_decoration_id, c_price, c_createddate)
                    VALUES (@CartId, @DecorationId, @Price, GETDATE())";

                var parameters = new[]
                {
                    new SqlParameter("@CartId", cartId),
                    new SqlParameter("@DecorationId", decoration.DecorationId),
                    new SqlParameter("@Price", decoration.Price)
                };

                var result = await Task.Run(() => _dbHelper.ExecuteNonQuery(query, parameters));
                return result > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
