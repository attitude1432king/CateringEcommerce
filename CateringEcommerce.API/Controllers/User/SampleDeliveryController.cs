using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Base.Common;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Models.Delivery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CateringEcommerce.API.Controllers.User
{
    /// <summary>
    /// Sample Delivery Controller - Third-party real-time tracking
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/User/[controller]")]
    public class SampleDeliveryController : ControllerBase
    {
        private readonly ILogger<SampleDeliveryController> _logger;
        private readonly ICurrentUserService _currentUser;
        private readonly IConfiguration _configuration;
        private readonly string _connStr;

        public SampleDeliveryController(
            ILogger<SampleDeliveryController> logger,
            ICurrentUserService currentUser,
            IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _connStr = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
        }

        // ===================================
        // GET: api/User/SampleDelivery/{orderId}
        // Get sample delivery info by order ID
        // ===================================
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetSampleDelivery(long orderId)
        {
            try
            {
                long userId = _currentUser.UserId;
                if (userId <= 0)
                {
                    return ApiResponseHelper.Failure("User not authenticated.");
                }

                _logger.LogInformation($"User {userId} fetching sample delivery for order {orderId}");

                var service = new SampleDeliveryService(_connStr);
                var delivery = await service.GetSampleDeliveryByOrderIdAsync(orderId);

                if (delivery == null)
                {
                    return ApiResponseHelper.Failure("Sample delivery not found for this order.");
                }

                // Verify user owns this order (basic security check)
                if (delivery.UserId != userId)
                {
                    _logger.LogWarning($"User {userId} attempted to access sample delivery for order {orderId} belonging to user {delivery.UserId}");
                    return ApiResponseHelper.Failure("Access denied.");
                }

                return ApiResponseHelper.Success(delivery);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching sample delivery for order {orderId}");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching delivery information."));
            }
        }

        // ===================================
        // GET: api/User/SampleDelivery/track/{orderId}
        // Get tracking information with live status
        // ===================================
        [HttpGet("track/{orderId}")]
        public async Task<IActionResult> GetTrackingInfo(long orderId)
        {
            try
            {
                long userId = _currentUser.UserId;
                if (userId <= 0)
                {
                    return ApiResponseHelper.Failure("User not authenticated.");
                }

                _logger.LogInformation($"User {userId} tracking sample delivery for order {orderId}");

                var service = new SampleDeliveryService(_connStr);
                var tracking = await service.GetTrackingInfoAsync(orderId);

                if (tracking == null)
                {
                    return ApiResponseHelper.Failure("Tracking information not available.");
                }

                // Verify user owns this order
                if (tracking.UserId != userId)
                {
                    _logger.LogWarning($"User {userId} attempted to track order {orderId} belonging to user {tracking.UserId}");
                    return ApiResponseHelper.Failure("Access denied.");
                }

                return ApiResponseHelper.Success(tracking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching tracking info for order {orderId}");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching tracking information."));
            }
        }
    }
}
