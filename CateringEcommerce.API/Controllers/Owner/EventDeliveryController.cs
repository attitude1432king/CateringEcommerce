using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Base.Common;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Models.Delivery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CateringEcommerce.API.Controllers.Owner
{
    /// <summary>
    /// Partner Event Delivery Management Controller
    /// Partner can initialize and update event delivery status
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/Owner/[controller]")]
    public class EventDeliveryController : ControllerBase
    {
        private readonly IDatabaseHelper _dbHelper;
        private readonly ILogger<EventDeliveryController> _logger;
        private readonly ICurrentUserService _currentUser;
        private readonly IConfiguration _configuration;
        private readonly string _connStr;

        public EventDeliveryController(
            IDatabaseHelper dbHelper,
            ILogger<EventDeliveryController> logger,
            ICurrentUserService currentUser,
            IConfiguration configuration)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _connStr = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
        }

        // ===================================
        // POST: api/Owner/EventDelivery/init
        // Initialize event delivery for an order
        // ===================================
        [HttpPost("init")]
        public async Task<IActionResult> InitEventDelivery([FromBody] InitEventDeliveryRequest request)
        {
            try
            {
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                {
                    return ApiResponseHelper.Failure("Partner not authenticated.");
                }

                // Ensure ownerId matches the request
                request.OwnerId = ownerId;

                _logger.LogInformation($"Partner {ownerId} initializing event delivery for order {request.OrderId}");

                var service = new EventDeliveryService(_dbHelper);
                var delivery = await service.InitEventDeliveryAsync(request);

                _logger.LogInformation($"Event delivery initialized successfully for order {request.OrderId}");

                return ApiResponseHelper.Success(delivery, "Event delivery initialized successfully.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Event delivery init failed: {ex.Message}");
                return ApiResponseHelper.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing event delivery");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while initializing event delivery."));
            }
        }

        // ===================================
        // PUT: api/Owner/EventDelivery/update-status
        // Update event delivery status (validates status flow)
        // ===================================
        [HttpPut("update-status")]
        public async Task<IActionResult> UpdateDeliveryStatus([FromBody] UpdateEventDeliveryStatusRequest request)
        {
            try
            {
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                {
                    return ApiResponseHelper.Failure("Partner not authenticated.");
                }

                // Set changed by info
                request.ChangedByUserId = ownerId;
                request.ChangedByType = "Partner";

                _logger.LogInformation($"Partner {ownerId} updating event delivery {request.EventDeliveryId} to status {request.NewStatus}");

                var service = new EventDeliveryService(_dbHelper);

                // Verify partner owns this delivery
                var currentDelivery = await service.GetEventDeliveryByOrderIdAsync(request.EventDeliveryId);
                if (currentDelivery != null && currentDelivery.OwnerId != ownerId)
                {
                    _logger.LogWarning($"Partner {ownerId} attempted to update delivery belonging to partner {currentDelivery.OwnerId}");
                    return ApiResponseHelper.Failure("Access denied.");
                }

                var updatedDelivery = await service.UpdateEventDeliveryStatusAsync(request);

                _logger.LogInformation($"Event delivery {request.EventDeliveryId} updated successfully to {request.NewStatus}");

                // TODO: Send delivery status notification to customer
                // Commented out until EventDeliveryDto model is updated with required properties
                // (CustomerName, CustomerEmail, CustomerPhone, OrderNumber, DeliveryDate, DeliveryTime, DeliveryAddress, PartnerName)
                _logger.LogInformation("Delivery status updated. EventDeliveryId: {EventDeliveryId}, NewStatus: {Status}",
                    request.EventDeliveryId, request.NewStatus);

                return ApiResponseHelper.Success(updatedDelivery, "Delivery status updated successfully.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Status update failed: {ex.Message}");
                return ApiResponseHelper.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating event delivery status");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while updating delivery status."));
            }
        }

        // ===================================
        // GET: api/Owner/EventDelivery/active
        // Get all active deliveries for the partner
        // ===================================
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveDeliveries()
        {
            try
            {
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                {
                    return ApiResponseHelper.Failure("Partner not authenticated.");
                }

                _logger.LogInformation($"Partner {ownerId} fetching active deliveries");

                var service = new EventDeliveryService(_dbHelper);
                var deliveries = await service.GetPartnerActiveDeliveriesAsync(ownerId);

                return ApiResponseHelper.Success(deliveries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching active deliveries");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching active deliveries."));
            }
        }

        // ===================================
        // GET: api/Owner/EventDelivery/{orderId}
        // Get event delivery details by order ID
        // ===================================
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetEventDelivery(long orderId)
        {
            try
            {
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                {
                    return ApiResponseHelper.Failure("Partner not authenticated.");
                }

                _logger.LogInformation($"Partner {ownerId} fetching event delivery for order {orderId}");

                var service = new EventDeliveryService(_dbHelper);
                var delivery = await service.GetEventDeliveryByOrderIdAsync(orderId);

                if (delivery == null)
                {
                    return ApiResponseHelper.Failure("Event delivery not found.");
                }

                // Verify partner owns this delivery
                if (delivery.OwnerId != ownerId)
                {
                    _logger.LogWarning($"Partner {ownerId} attempted to access delivery belonging to partner {delivery.OwnerId}");
                    return ApiResponseHelper.Failure("Access denied.");
                }

                return ApiResponseHelper.Success(delivery);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching event delivery for order {orderId}");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching delivery information."));
            }
        }

        // ===================================
        // GET: api/Owner/EventDelivery/timeline/{orderId}
        // Get delivery timeline with status history
        // ===================================
        [HttpGet("timeline/{orderId}")]
        public async Task<IActionResult> GetDeliveryTimeline(long orderId)
        {
            try
            {
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                {
                    return ApiResponseHelper.Failure("Partner not authenticated.");
                }

                _logger.LogInformation($"Partner {ownerId} fetching delivery timeline for order {orderId}");

                var service = new EventDeliveryService(_dbHelper);
                var timeline = await service.GetDeliveryTimelineAsync(orderId);

                if (timeline.EventDelivery != null && timeline.EventDelivery.OwnerId != ownerId)
                {
                    _logger.LogWarning($"Partner {ownerId} attempted to access timeline for delivery belonging to partner {timeline.EventDelivery.OwnerId}");
                    return ApiResponseHelper.Failure("Access denied.");
                }

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
