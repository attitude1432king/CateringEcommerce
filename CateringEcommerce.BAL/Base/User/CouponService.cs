using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.User;
using CateringEcommerce.Domain.Models.Owner;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CateringEcommerce.BAL.Base.User
{
    /// <summary>
    /// User-facing coupon service implementation
    /// Handles browsing and validation of discount coupons for customers
    /// </summary>
    public class CouponService : ICouponService
    {
        private readonly IDatabaseHelper _dbHelper;
        public CouponService(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        /// <summary>
        /// Gets all available and valid coupons for a specific caterer
        /// Only returns coupons that are:
        /// - Active (c_isactive = 1)
        /// - Not deleted (c_is_deleted = 0)
        /// - Within valid date range
        /// - Have not reached usage limits
        /// - Type is "EntireCatering" (applies to whole order)
        /// </summary>
        public async Task<List<DiscountModel>> GetAvailableCouponsAsync(long cateringId, long? userId = null)
        {
            try
            {
                string query = $@"
                    SELECT
                        c_discountid AS ID,
                        c_discount_name AS Name,
                        c_discount_description AS Description,
                        c_discount_code AS Code,
                        c_discount_type AS Type,
                        c_discount_mode AS Mode,
                        c_discount_value AS Value,
                        c_min_order_value AS MinOrderValue,
                        c_max_discount_value AS MaxDiscount,
                        c_max_uses_per_order AS MaxUsesPerOrder,
                        c_max_uses_per_user AS MaxUsesPerUser,
                        c_is_stackable AS IsStackable,
                        c_startdate AS StartDate,
                        c_enddate AS EndDate,
                        c_isactive AS IsActive
                    FROM {Table.SysCateringDiscount}
                    WHERE c_ownerid = @CateringId
                        AND c_is_deleted = 0
                        AND c_isactive = 1
                        AND c_startdate <= CAST(GETDATE() AS DATE)
                        AND c_enddate >= CAST(GETDATE() AS DATE)
                        AND c_discount_type = @EntireCateringType
                    ORDER BY c_discount_value DESC, c_createddate DESC";

                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@CateringId", cateringId),
                    new SqlParameter("@EntireCateringType", DiscountType.EntireCatering.GetHashCode())
                };

                var discountData = await _dbHelper.ExecuteAsync(query, parameters.ToArray());

                if (discountData.Rows.Count == 0)
                    return new List<DiscountModel>();

                var coupons = discountData.AsEnumerable()
                    .Select(row => new DiscountModel
                    {
                        ID = row.GetValue<long?>("ID"),
                        Name = row.GetValue<string>("Name"),
                        Description = row.GetValue<string>("Description"),
                        Code = row.GetValue<string>("Code"),
                        Type = row.GetValue<int>("Type"),
                        Mode = row.GetValue<int>("Mode"),
                        Value = row.GetValue<decimal>("Value"),
                        MinOrderValue = row.GetValue<decimal?>("MinOrderValue"),
                        MaxDiscount = row.GetValue<decimal?>("MaxDiscount"),
                        MaxUsesPerOrder = row.GetValue<int>("MaxUsesPerOrder"),
                        MaxUsesPerUser = row.GetValue<int>("MaxUsesPerUser"),
                        IsStackable = row.GetValue<bool>("IsStackable"),
                        StartDate = row.IsNull("StartDate") ? null : DateOnly.FromDateTime(row.Field<DateTime>("StartDate")),
                        EndDate = row.IsNull("EndDate") ? null : DateOnly.FromDateTime(row.Field<DateTime>("EndDate")),
                        IsActive = row.GetValue<bool>("IsActive")
                    })
                    .ToList();

                // Filter by user-specific usage limits if userId provided
                if (userId.HasValue && userId.Value > 0)
                {
                    var filteredCoupons = new List<DiscountModel>();
                    foreach (var coupon in coupons)
                    {
                        if (!coupon.ID.HasValue) continue;

                        // Check if user has exceeded usage limit for this coupon
                        if (coupon.MaxUsesPerUser > 0)
                        {
                            var usageCount = await GetUserCouponUsageCountAsync(userId.Value, coupon.ID.Value);
                            if (usageCount >= coupon.MaxUsesPerUser)
                                continue; // Skip this coupon
                        }

                        filteredCoupons.Add(coupon);
                    }
                    return filteredCoupons;
                }

                return coupons;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Validates a coupon code for a specific order
        /// Performs comprehensive validation:
        /// 1. Coupon exists and belongs to caterer
        /// 2. Active status
        /// 3. Date validity
        /// 4. Minimum order value requirement
        /// 5. User usage limits
        /// 6. Type is EntireCatering
        /// </summary>
        public async Task<DiscountModel?> ValidateCouponAsync(string couponCode, long cateringId, decimal orderValue, long? userId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(couponCode))
                    return null;

                // Get the coupon
                var coupon = await GetCouponByCodeAsync(couponCode.Trim().ToUpper(), cateringId);

                if (coupon == null)
                    return null;

                // Validation 1: Must be active
                if (!coupon.IsActive)
                    return null;

                // Validation 2: Must be within valid date range
                var today = DateOnly.FromDateTime(DateTime.Today);
                if (coupon.StartDate.HasValue && today < coupon.StartDate.Value)
                    return null;

                if (coupon.EndDate.HasValue && today > coupon.EndDate.Value)
                    return null;

                // Validation 3: Minimum order value
                if (coupon.MinOrderValue.HasValue && orderValue < coupon.MinOrderValue.Value)
                    return null;

                // Validation 4: Must be EntireCatering type
                if (coupon.Type != DiscountType.EntireCatering.GetHashCode())
                    return null;

                // Validation 5: User-specific usage limit
                if (userId.HasValue && userId.Value > 0 && coupon.MaxUsesPerUser > 0 && coupon.ID.HasValue)
                {
                    var usageCount = await GetUserCouponUsageCountAsync(userId.Value, coupon.ID.Value);
                    if (usageCount >= coupon.MaxUsesPerUser)
                        return null;
                }

                // All validations passed
                return coupon;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Gets a specific coupon by code and caterer ID
        /// </summary>
        public async Task<DiscountModel?> GetCouponByCodeAsync(string couponCode, long cateringId)
        {
            try
            {
                string query = $@"
                    SELECT
                        c_discountid AS ID,
                        c_discount_name AS Name,
                        c_discount_description AS Description,
                        c_discount_code AS Code,
                        c_discount_type AS Type,
                        c_discount_mode AS Mode,
                        c_discount_value AS Value,
                        c_min_order_value AS MinOrderValue,
                        c_max_discount_value AS MaxDiscount,
                        c_max_uses_per_order AS MaxUsesPerOrder,
                        c_max_uses_per_user AS MaxUsesPerUser,
                        c_is_stackable AS IsStackable,
                        c_startdate AS StartDate,
                        c_enddate AS EndDate,
                        c_isactive AS IsActive
                    FROM {Table.SysCateringDiscount}
                    WHERE c_ownerid = @CateringId
                        AND c_discount_code = @CouponCode
                        AND c_is_deleted = 0";

                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@CateringId", cateringId),
                    new SqlParameter("@CouponCode", couponCode.ToUpper())
                };

                var result = await _dbHelper.ExecuteAsync(query, parameters.ToArray());

                if (result.Rows.Count == 0)
                    return null;

                var row = result.Rows[0];
                return new DiscountModel
                {
                    ID = row.GetValue<long?>("ID"),
                    Name = row.GetValue<string>("Name"),
                    Description = row.GetValue<string>("Description"),
                    Code = row.GetValue<string>("Code"),
                    Type = row.GetValue<int>("Type"),
                    Mode = row.GetValue<int>("Mode"),
                    Value = row.GetValue<decimal>("Value"),
                    MinOrderValue = row.GetValue<decimal?>("MinOrderValue"),
                    MaxDiscount = row.GetValue<decimal?>("MaxDiscount"),
                    MaxUsesPerOrder = row.GetValue<int>("MaxUsesPerOrder"),
                    MaxUsesPerUser = row.GetValue<int>("MaxUsesPerUser"),
                    IsStackable = row.GetValue<bool>("IsStackable"),
                    StartDate = row.IsNull("StartDate") ? null : DateOnly.FromDateTime(row.Field<DateTime>("StartDate")),
                    EndDate = row.IsNull("EndDate") ? null : DateOnly.FromDateTime(row.Field<DateTime>("EndDate")),
                    IsActive = row.GetValue<bool>("IsActive")
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Checks how many times a user has used a specific coupon
        /// Counts successful usage from t_sys_catering_discount_usage table
        /// </summary>
        public async Task<int> GetUserCouponUsageCountAsync(long userId, long discountId)
        {
            try
            {
                string query = $@"
                    SELECT COUNT(1)
                    FROM t_sys_catering_discount_usage
                    WHERE c_userid = @UserId
                        AND c_discount_id = @DiscountId
                        AND c_usage_status = 1"; // 1 = Applied successfully

                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@UserId", userId),
                    new SqlParameter("@DiscountId", discountId)
                };

                var result = await _dbHelper.ExecuteScalarAsync(query, parameters.ToArray());
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
