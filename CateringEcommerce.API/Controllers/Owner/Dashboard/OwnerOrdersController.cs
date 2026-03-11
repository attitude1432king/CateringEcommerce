using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Base.Owner.Dashboard;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Interfaces.Owner;
using CateringEcommerce.Domain.Models.Owner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CateringEcommerce.API.Controllers.Owner.Dashboard
{
    /// <summary>
    /// Owner Orders Controller
    /// Provides order management functionality for partner owners
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/Owner/[controller]")]
    public class OwnerOrdersController : ControllerBase
    {
        private readonly ILogger<OwnerOrdersController> _logger;
        private readonly IOwnerOrderRepository _ownerOrderRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly string _connStr;

        public OwnerOrdersController(
            IOwnerOrderRepository ownerOrderRepository,
            ILogger<OwnerOrdersController> logger,
            ICurrentUserService currentUser,
            IConfiguration configuration)
        {
            _ownerOrderRepository = ownerOrderRepository ?? throw new ArgumentNullException(nameof(ownerOrderRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _connStr = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
        }

        /// <summary>
        /// Get filtered and paginated orders list
        /// </summary>
        /// <param name="filter">Order filter parameters</param>
        /// <returns>Paginated orders list</returns>
        [HttpPost("list")]
        public async Task<IActionResult> GetOrdersList([FromBody] OrderFilterDto filter)
        {
            try
            {
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                {
                    return ApiResponseHelper.Failure("Owner not authenticated.");
                }

                _logger.LogInformation($"Getting orders list for owner {ownerId}, page: {filter.Page}");

                var orders = await _ownerOrderRepository.GetOrdersList(ownerId, filter);

                return ApiResponseHelper.Success(orders, "Orders list retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders list");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while retrieving orders list."));
            }
        }

        /// <summary>
        /// Get complete order details
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>Order details</returns>
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderDetails(long orderId)
        {
            try
            {
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                {
                    return ApiResponseHelper.Failure("Owner not authenticated.");
                }

                _logger.LogInformation($"Getting order details for owner {ownerId}, order: {orderId}");

                var orderDetails = await _ownerOrderRepository.GetOrderDetails(ownerId, orderId);

                return ApiResponseHelper.Success(orderDetails, "Order details retrieved successfully.");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized access to order {orderId}: {ex.Message}");
                return ApiResponseHelper.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting order details for order {orderId}");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while retrieving order details."));
            }
        }

        /// <summary>
        /// Update order status
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="statusUpdate">Status update data</param>
        /// <returns>Success or failure</returns>
        [HttpPut("{orderId}/status")]
        public async Task<IActionResult> UpdateOrderStatus(long orderId, [FromBody] OrderStatusUpdateDto statusUpdate)
        {
            try
            {
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                {
                    return ApiResponseHelper.Failure("Owner not authenticated.");
                }

                _logger.LogInformation($"Updating order status for owner {ownerId}, order: {orderId}, new status: {statusUpdate.NewStatus}");

                var success = await _ownerOrderRepository.UpdateOrderStatus(ownerId, orderId, statusUpdate);

                if (success)
                {
                    return ApiResponseHelper.Success(null, "Order status updated successfully.");
                }
                else
                {
                    return ApiResponseHelper.Failure("Failed to update order status.");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized status update for order {orderId}: {ex.Message}");
                return ApiResponseHelper.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating order status for order {orderId}");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while updating order status."));
            }
        }

        /// <summary>
        /// Get order status history timeline
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>Status history</returns>
        [HttpGet("{orderId}/history")]
        public async Task<IActionResult> GetOrderStatusHistory(long orderId)
        {
            try
            {
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                {
                    return ApiResponseHelper.Failure("Owner not authenticated.");
                }

                _logger.LogInformation($"Getting order status history for owner {ownerId}, order: {orderId}");


                // Validate ownership first
                if (!await _ownerOrderRepository.ValidateOrderOwnership(ownerId, orderId))
                {
                    return ApiResponseHelper.Failure("Order does not belong to this owner.");
                }

                var history = await _ownerOrderRepository.GetOrderStatusHistory(orderId);

                return ApiResponseHelper.Success(history, "Order status history retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting order status history for order {orderId}");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while retrieving order status history."));
            }
        }

        /// <summary>
        /// Get booking request statistics (today/week/month counts)
        /// </summary>
        /// <returns>Booking request stats</returns>
        [HttpGet("booking-request-stats")]
        public async Task<IActionResult> GetBookingRequestStats()
        {
            try
            {
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                {
                    return ApiResponseHelper.Failure("Owner not authenticated.");
                }

                _logger.LogInformation($"Getting booking request stats for owner {ownerId}");

                var stats = await _ownerOrderRepository.GetBookingRequestStats(ownerId);

                return ApiResponseHelper.Success(stats, "Booking request statistics retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking request stats");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while retrieving booking request statistics."));
            }
        }

        /// <summary>
        /// Get order statistics
        /// </summary>
        /// <returns>Order statistics</returns>
        [HttpGet("stats")]
        public async Task<IActionResult> GetOrderStats()
        {
            try
            {
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                {
                    return ApiResponseHelper.Failure("Owner not authenticated.");
                }

                _logger.LogInformation($"Getting order stats for owner {ownerId}");

                var stats = await _ownerOrderRepository.GetOrderStats(ownerId);

                return ApiResponseHelper.Success(stats, "Order statistics retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order stats");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while retrieving order statistics."));
            }
        }
    }
}
