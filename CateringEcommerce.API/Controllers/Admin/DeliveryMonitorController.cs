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

namespace CateringEcommerce.API.Controllers.Admin
{
    /// <summary>
    /// Admin Delivery Monitoring Controller
    /// Admin can monitor all deliveries and override status when needed
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/Admin/[controller]")]
    public class DeliveryMonitorController : ControllerBase
    {
        private readonly ILogger<DeliveryMonitorController> _logger;
        private readonly ICurrentUserService _currentUser;
        private readonly IConfiguration _configuration;
        private readonly string _connStr;

        public DeliveryMonitorController(
            ILogger<DeliveryMonitorController> logger,
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
        // GET: api/Admin/DeliveryMonitor
        // Get all deliveries for monitoring
        // ===================================
        [HttpGet]
        public async Task<IActionResult> GetAllDeliveries()
        {
            try
            {
                // TODO: Add admin role check
                // if (!_currentUser.IsAdmin)
                // {
                //     return ApiResponseHelper.Failure("Access denied. Admin only.");
                // }

                _logger.LogInformation("Admin fetching all deliveries for monitoring");

                var service = new EventDeliveryService(_connStr);
                var deliveries = await service.GetAdminDeliveryMonitorAsync();

                return ApiResponseHelper.Success(deliveries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching admin delivery monitor");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching delivery information."));
            }
        }

        // ===================================
        // GET: api/Admin/DeliveryMonitor/{orderId}
        // Get specific delivery details
        // ===================================
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetDeliveryByOrder(long orderId)
        {
            try
            {
                // TODO: Add admin role check

                _logger.LogInformation($"Admin fetching delivery for order {orderId}");

                var service = new EventDeliveryService(_connStr);
                var delivery = await service.GetEventDeliveryByOrderIdAsync(orderId);

                if (delivery == null)
                {
                    return ApiResponseHelper.Failure("Delivery not found.");
                }

                return ApiResponseHelper.Success(delivery);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching delivery for order {orderId}");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching delivery information."));
            }
        }

        // ===================================
        // GET: api/Admin/DeliveryMonitor/timeline/{orderId}
        // Get delivery timeline with full history
        // ===================================
        [HttpGet("timeline/{orderId}")]
        public async Task<IActionResult> GetDeliveryTimeline(long orderId)
        {
            try
            {
                // TODO: Add admin role check

                _logger.LogInformation($"Admin fetching delivery timeline for order {orderId}");

                var service = new EventDeliveryService(_connStr);
                var timeline = await service.GetDeliveryTimelineAsync(orderId);

                return ApiResponseHelper.Success(timeline);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching delivery timeline for order {orderId}");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching delivery timeline."));
            }
        }

        // ===================================
        // POST: api/Admin/DeliveryMonitor/override
        // Override delivery status (admin only)
        // ===================================
        [HttpPost("override")]
        public async Task<IActionResult> OverrideDeliveryStatus([FromBody] AdminOverrideRequest request)
        {
            try
            {
                // TODO: Add admin role check and get admin user ID
                long adminUserId = _currentUser.UserId;
                if (adminUserId <= 0)
                {
                    return ApiResponseHelper.Failure("Admin not authenticated.");
                }

                _logger.LogInformation($"Admin {adminUserId} overriding delivery {request.EventDeliveryId} to status {request.NewStatus}");
                _logger.LogWarning($"ADMIN OVERRIDE: Delivery {request.EventDeliveryId} status changed to {request.NewStatus}. Reason: {request.Notes}");

                var service = new EventDeliveryService(_connStr);
                var updatedDelivery = await service.AdminOverrideStatusAsync(
                    request.EventDeliveryId,
                    request.NewStatus,
                    adminUserId,
                    request.Notes ?? "Admin override - no reason provided"
                );

                return ApiResponseHelper.Success(updatedDelivery, "Delivery status overridden successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error overriding delivery status");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while overriding delivery status."));
            }
        }
    }

    /// <summary>
    /// Request model for admin override
    /// </summary>
    public class AdminOverrideRequest
    {
        public long EventDeliveryId { get; set; }
        public EventDeliveryStatus NewStatus { get; set; }
        public string? Notes { get; set; }
    }
}
