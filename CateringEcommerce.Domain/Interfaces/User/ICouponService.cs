using CateringEcommerce.Domain.Models.Owner;

namespace CateringEcommerce.Domain.Interfaces.User
{
    /// <summary>
    /// User-facing coupon service interface
    /// Provides methods for browsing and validating discount coupons
    /// </summary>
    public interface ICouponService
    {
        /// <summary>
        /// Gets all available and valid coupons for a specific caterer
        /// Filters out expired, inactive, and usage-limit-reached coupons
        /// </summary>
        /// <param name="cateringId">The caterer's owner ID</param>
        /// <param name="userId">Optional user ID to check per-user usage limits</param>
        /// <returns>List of available coupons</returns>
        Task<List<DiscountModel>> GetAvailableCouponsAsync(long cateringId, long? userId = null);

        /// <summary>
        /// Validates a coupon code for a specific order
        /// Checks all conditions: active status, date validity, min order value, usage limits
        /// </summary>
        /// <param name="couponCode">The coupon code to validate</param>
        /// <param name="cateringId">The caterer's owner ID</param>
        /// <param name="orderValue">The order subtotal amount</param>
        /// <param name="userId">Optional user ID to check per-user usage limits</param>
        /// <returns>Validated discount model or null if invalid</returns>
        Task<DiscountModel?> ValidateCouponAsync(string couponCode, long cateringId, decimal orderValue, long? userId = null);

        /// <summary>
        /// Gets a specific coupon by code and caterer ID
        /// </summary>
        /// <param name="couponCode">The coupon code</param>
        /// <param name="cateringId">The caterer's owner ID</param>
        /// <returns>Discount model or null if not found</returns>
        Task<DiscountModel?> GetCouponByCodeAsync(string couponCode, long cateringId);

        /// <summary>
        /// Checks how many times a user has used a specific coupon
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="discountId">The discount ID</param>
        /// <returns>Usage count</returns>
        Task<int> GetUserCouponUsageCountAsync(long userId, long discountId);

        /// <summary>
        /// Gets platform-wide featured active offers for the home page
        /// Returns the most recently created active, non-expired discounts across all caterers
        /// </summary>
        /// <param name="limit">Maximum number of offers to return (default 6)</param>
        /// <returns>List of active discount offers</returns>
        Task<List<DiscountModel>> GetFeaturedOffersAsync(int limit = 6);
    }
}
