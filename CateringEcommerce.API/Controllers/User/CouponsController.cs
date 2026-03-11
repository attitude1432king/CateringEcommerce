using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Base.User;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Interfaces.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CateringEcommerce.API.Controllers.User
{
    [Route("api/User/Coupons")]
    [ApiController]
    public class CouponsController : ControllerBase
    {
        private readonly ILogger<CouponsController> _logger;
        private readonly ICurrentUserService _currentUser;
        private readonly ICouponService _couponService;

        public CouponsController(
            ILogger<CouponsController> logger,
            ICurrentUserService currentUser,
            ICouponService couponService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _couponService = couponService ?? throw new ArgumentNullException(nameof(couponService));
        }

        /// <summary>
        /// Gets all available coupons for a specific caterer
        /// Public endpoint - no authentication required (for browsing)
        /// </summary>
        /// <param name="cateringId">The caterer's owner ID</param>
        /// <returns>List of available coupons with details</returns>
        [HttpGet("Available/{cateringId:long}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableCoupons(long cateringId)
        {
            if (cateringId <= 0)
                return ApiResponseHelper.Failure("Invalid catering ID.");

            _logger.LogInformation(
                "GetAvailableCoupons started | CateringId={CateringId}",
                cateringId);

            try
            {
                var couponService = _couponService;

                // Get userId if authenticated (for filtering by user usage limits)
                long? userId = null;
                if (_currentUser.UserId > 0)
                    userId = _currentUser.UserId;

                var coupons = await couponService.GetAvailableCouponsAsync(cateringId, userId);

                // Transform to user-friendly response
                var response = coupons.Select(c => new
                {
                    discountId = c.ID,
                    couponCode = c.Code,
                    name = c.Name,
                    description = c.Description,
                    discountType = ((DiscountMode)c.Mode).ToString(),
                    discountValue = c.Value,
                    minOrderValue = c.MinOrderValue,
                    maxDiscount = c.MaxDiscount,
                    validFrom = c.StartDate,
                    validTo = c.EndDate,
                    isActive = c.IsActive
                }).ToList();

                _logger.LogInformation(
                    "GetAvailableCoupons completed | CateringId={CateringId} | Count={Count}",
                    cateringId, response.Count);

                return ApiResponseHelper.Success(response, "Coupons fetched successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "GetAvailableCoupons failed | CateringId={CateringId}",
                    cateringId);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred while fetching coupons.");
            }
        }

        /// <summary>
        /// Validates a coupon code for a specific order
        /// Checks all conditions: active status, date validity, min order value, usage limits
        /// Public endpoint - no authentication required (validation happens at checkout)
        /// </summary>
        /// <param name="request">Validation request with code, cateringId, and orderValue</param>
        /// <returns>Validated coupon details or error message</returns>
        [HttpPost("Validate")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateCoupon([FromBody] CouponValidationRequest request)
        {
            if (request == null)
                return ApiResponseHelper.Failure("Invalid request payload.");

            if (string.IsNullOrWhiteSpace(request.Code))
                return ApiResponseHelper.Failure("Coupon code is required.");

            if (request.CateringId <= 0)
                return ApiResponseHelper.Failure("Invalid catering ID.");

            if (request.OrderValue <= 0)
                return ApiResponseHelper.Failure("Invalid order value.");

            _logger.LogInformation(
                "ValidateCoupon started | CateringId={CateringId} | Code={Code} | OrderValue={OrderValue}",
                request.CateringId, request.Code, request.OrderValue);

            try
            {
                var couponService = _couponService;

                // Get userId if authenticated
                long? userId = null;
                if (_currentUser.UserId > 0)
                    userId = _currentUser.UserId;

                var coupon = await couponService.ValidateCouponAsync(
                    request.Code.Trim().ToUpper(),
                    request.CateringId,
                    request.OrderValue,
                    userId
                );

                if (coupon == null)
                {
                    _logger.LogWarning(
                        "ValidateCoupon failed - Invalid coupon | CateringId={CateringId} | Code={Code}",
                        request.CateringId, request.Code);

                    return ApiResponseHelper.Failure(
                        "Invalid coupon code or coupon is not applicable to this order.",
                        "warning");
                }

                // Calculate discount amount
                decimal discountAmount = 0;
                if (coupon.Mode == DiscountMode.Percentage.GetHashCode())
                {
                    discountAmount = (request.OrderValue * coupon.Value) / 100;
                    if (coupon.MaxDiscount.HasValue && discountAmount > coupon.MaxDiscount.Value)
                    {
                        discountAmount = coupon.MaxDiscount.Value;
                    }
                }
                else // Flat discount
                {
                    discountAmount = Math.Min(coupon.Value, request.OrderValue);
                }

                var response = new
                {
                    discountId = coupon.ID,
                    couponCode = coupon.Code,
                    name = coupon.Name,
                    description = coupon.Description,
                    discountType = ((DiscountMode)coupon.Mode).ToString(),
                    discountValue = coupon.Value,
                    minOrderValue = coupon.MinOrderValue,
                    maxDiscount = coupon.MaxDiscount,
                    validFrom = coupon.StartDate,
                    validTo = coupon.EndDate,
                    calculatedDiscount = Math.Round(discountAmount, 2)
                };

                _logger.LogInformation(
                    "ValidateCoupon completed successfully | CateringId={CateringId} | Code={Code} | Discount={Discount}",
                    request.CateringId, request.Code, discountAmount);

                return ApiResponseHelper.Success(response, "Coupon is valid and applied successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ValidateCoupon failed | CateringId={CateringId} | Code={Code}",
                    request.CateringId, request.Code);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred while validating the coupon.");
            }
        }
    }

    /// <summary>
    /// Request model for coupon validation
    /// </summary>
    public class CouponValidationRequest
    {
        public string Code { get; set; } = string.Empty;
        public long CateringId { get; set; }
        public decimal OrderValue { get; set; }
    }
}
