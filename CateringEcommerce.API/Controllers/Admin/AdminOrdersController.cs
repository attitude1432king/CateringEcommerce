using CateringEcommerce.API.Filters;
using CateringEcommerce.API.Helpers;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.AspNetCore.Mvc;

namespace CateringEcommerce.API.Controllers.Admin
{
    [Route("api/admin/orders")]
    [ApiController]
    [AdminAuthorize]
    public class AdminOrdersController : ControllerBase
    {
        private readonly IAdminOrderRepository _orderRepository;
        private readonly ILogger<AdminOrdersController> _logger;

        public AdminOrdersController(
            IAdminOrderRepository orderRepository,
            ILogger<AdminOrdersController> logger)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get paginated list of orders with filtering
        /// Permission Required: ORDER_VIEW
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery] AdminOrderListRequest request)
        {
            try
            {
                var result = await _orderRepository.GetOrdersAsync(request);
                return ApiResponseHelper.Success(result, "Orders retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve orders");
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get order details by ID
        /// Permission Required: ORDER_VIEW
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(long id)
        {
            try
            {
                var order = await _orderRepository.GetOrderByIdAsync(id);

                if (order == null)
                    return ApiResponseHelper.Failure("Order not found.");

                return ApiResponseHelper.Success(order, "Order details retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve order details for OrderId: {OrderId}", id);
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get order statistics
        /// Permission Required: ORDER_VIEW
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetOrderStats()
        {
            try
            {
                var stats = await _orderRepository.GetOrderStatsAsync();
                return ApiResponseHelper.Success(stats, "Order statistics retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve order statistics");
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update order status
        /// Permission Required: ORDER_EDIT
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(long id, [FromBody] UpdateOrderStatusDto request)
        {
            try
            {
                var adminIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
                if (adminIdClaim == null || !long.TryParse(adminIdClaim.Value, out long adminId))
                {
                    return ApiResponseHelper.Failure("Invalid admin session.");
                }

                var updateRequest = new AdminOrderUpdateStatusRequest
                {
                    OrderId = id,
                    NewStatus = request.NewStatus,
                    Remarks = request.Remarks,
                    UpdatedBy = adminId
                };

                bool success = await _orderRepository.UpdateOrderStatusAsync(updateRequest);

                if (!success)
                    return ApiResponseHelper.Failure("Failed to update order status.");

                return ApiResponseHelper.Success(null, "Order status updated successfully.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Order status update validation failed for OrderId: {OrderId}", id);
                return ApiResponseHelper.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update order status for OrderId: {OrderId}", id);
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Cancel order (admin initiated)
        /// Permission Required: ORDER_CANCEL or Super Admin
        /// </summary>
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(long id, [FromBody] CancelOrderDto request)
        {
            try
            {
                var adminIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
                if (adminIdClaim == null || !long.TryParse(adminIdClaim.Value, out long adminId))
                {
                    return ApiResponseHelper.Failure("Invalid admin session.");
                }

                bool success = await _orderRepository.CancelOrderAsync(id, adminId, request.CancellationReason);

                if (!success)
                    return ApiResponseHelper.Failure("Failed to cancel order.");

                return ApiResponseHelper.Success(null, "Order cancelled successfully. Refund processing should be initiated separately.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Order cancellation validation failed for OrderId: {OrderId}", id);
                return ApiResponseHelper.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cancel order for OrderId: {OrderId}", id);
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Export orders to CSV
        /// Permission Required: ORDER_EXPORT
        /// </summary>
        [HttpGet("export")]
        public async Task<IActionResult> ExportOrders([FromQuery] AdminOrderListRequest request)
        {
            try
            {
                var csvData = await _orderRepository.ExportOrdersAsync(request);
                var fileName = $"Orders_Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                return File(csvData, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export orders");
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }
    }

    #region DTOs

    public class UpdateOrderStatusDto
    {
        public string NewStatus { get; set; } = string.Empty;
        public string? Remarks { get; set; }
    }

    public class CancelOrderDto
    {
        public string CancellationReason { get; set; } = string.Empty;
    }

    #endregion
}
