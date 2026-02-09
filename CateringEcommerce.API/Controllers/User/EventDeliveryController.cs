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
    /// Event Delivery Controller - Status-based delivery (NO GPS)
    /// User has READ-ONLY access to delivery status
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/User/[controller]")]
    public class EventDeliveryController : ControllerBase
    {
        private readonly ILogger<EventDeliveryController> _logger;
        private readonly ICurrentUserService _currentUser;
        private readonly IEventDeliveryService _eventDeliveryService;

        public EventDeliveryController(
            ILogger<EventDeliveryController> logger,
            ICurrentUserService currentUser,
            IEventDeliveryService eventDeliveryService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _eventDeliveryService = eventDeliveryService ?? throw new ArgumentNullException(nameof(eventDeliveryService));
        }

        // ===================================
        // GET: api/User/EventDelivery/{orderId}
        // Get event delivery info by order ID (READ-ONLY)
        // ===================================
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetEventDelivery(long orderId)
        {
            try
            {
                long userId = _currentUser.UserId;
                if (userId <= 0)
                {
                    return ApiResponseHelper.Failure("User not authenticated.");
                }

                _logger.LogInformation($"User {userId} fetching event delivery for order {orderId}");

                var service = _eventDeliveryService;
                var delivery = await service.GetEventDeliveryByOrderIdAsync(orderId);

                if (delivery == null)
                {
                    return ApiResponseHelper.Failure("Event delivery not found for this order.");
                }

                // TODO: Add validation to check if user owns this order
                // For now, we'll return the delivery info

                return ApiResponseHelper.Success(delivery);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching event delivery for order {orderId}");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching delivery information."));
            }
        }

        // ===================================
        // GET: api/User/EventDelivery/timeline/{orderId}
        // Get delivery timeline with full status history (READ-ONLY)
        // ===================================
        [HttpGet("timeline/{orderId}")]
        public async Task<IActionResult> GetDeliveryTimeline(long orderId)
        {
            try
            {
                long userId = _currentUser.UserId;
                if (userId <= 0)
                {
                    return ApiResponseHelper.Failure("User not authenticated.");
                }

                _logger.LogInformation($"User {userId} fetching delivery timeline for order {orderId}");

                var service = _eventDeliveryService;
                var timeline = await service.GetDeliveryTimelineAsync(orderId);

                // TODO: Add validation to check if user owns this order

                return ApiResponseHelper.Success(timeline);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching delivery timeline for order {orderId}");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching delivery timeline."));
            }
        }
    }
}
