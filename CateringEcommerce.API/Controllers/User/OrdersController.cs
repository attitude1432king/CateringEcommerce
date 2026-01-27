using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Base.User;
using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.Services;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CateringEcommerce.API.Controllers.User
{
    [Authorize]
    [ApiController]
    [Route("api/User/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly ILogger<OrdersController> _logger;
        private readonly ICurrentUserService _currentUser;
        private readonly IConfiguration _configuration;
        private readonly IFileStorageService _fileStorageService;
        private readonly string _connStr;

        public OrdersController(
            ILogger<OrdersController> logger,
            ICurrentUserService currentUser,
            IConfiguration configuration,
            IFileStorageService fileStorageService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
            _connStr = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
        }

        // ===================================
        // POST: api/User/Orders/Create
        // Create new order
        // ===================================
        [HttpPost("Create")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto orderData)
        {
            try
            {
                // Validate model
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Get authenticated user ID
                long userId = _currentUser.UserId;
                if (userId <= 0)
                {
                    return ApiResponseHelper.Failure("User not authenticated.");
                }

                // Validate order data
                if (orderData == null)
                {
                    return ApiResponseHelper.Failure("Invalid order data.");
                }

                if (orderData.OrderItems == null || orderData.OrderItems.Count == 0)
                {
                    return ApiResponseHelper.Failure("Order must contain at least one item.");
                }

                _logger.LogInformation($"Creating order for user {userId}, catering {orderData.CateringId}");

                // Create services
                INotificationService notificationService = new NotificationService(_configuration);
                OrderService orderService = new OrderService(_connStr, notificationService, _fileStorageService);

                // Create order
                OrderDto order = await orderService.CreateOrderAsync(userId, orderData);

                _logger.LogInformation($"Order created successfully: {order.OrderNumber}");

                return ApiResponseHelper.Success(order, "Order placed successfully!");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Order creation failed: {ex.Message}");
                return ApiResponseHelper.Failure(ex.Message, "warning");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while creating your order. Please try again."));
            }
        }

        // ===================================
        // GET: api/User/Orders
        // Get user's orders (paginated)
        // ===================================
        [HttpGet]
        public async Task<IActionResult> GetUserOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                // Get authenticated user ID
                long userId = _currentUser.UserId;
                if (userId <= 0)
                {
                    return ApiResponseHelper.Failure("User not authenticated.");
                }

                // Validate pagination
                if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
                {
                    return ApiResponseHelper.Failure("Invalid pagination parameters.");
                }

                _logger.LogInformation($"Fetching orders for user {userId}, page {pageNumber}, size {pageSize}");

                // Create service
                INotificationService notificationService = new NotificationService(_configuration);
                OrderService orderService = new OrderService(_connStr, notificationService, _fileStorageService);

                // Get orders
                List<OrderListItemDto> orders = await orderService.GetUserOrdersAsync(userId, pageNumber, pageSize);

                return ApiResponseHelper.Success(new
                {
                    orders,
                    pageNumber,
                    pageSize,
                    count = orders.Count
                }, "Orders retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user orders");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching your orders."));
            }
        }

        // ===================================
        // GET: api/User/Orders/{orderId}
        // Get order details by ID
        // ===================================
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderDetails(long orderId)
        {
            try
            {
                // Get authenticated user ID
                long userId = _currentUser.UserId;
                if (userId <= 0)
                {
                    return ApiResponseHelper.Failure("User not authenticated.");
                }

                if (orderId <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid order ID.");
                }

                _logger.LogInformation($"Fetching order details for order {orderId}, user {userId}");

                // Create service
                INotificationService notificationService = new NotificationService(_configuration);
                OrderService orderService = new OrderService(_connStr, notificationService, _fileStorageService);

                // Get order details
                OrderDto? order = await orderService.GetOrderDetailsAsync(userId, orderId);

                if (order == null)
                {
                    return ApiResponseHelper.Failure("Order not found.", "warning");
                }

                return ApiResponseHelper.Success(order, "Order details retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching order details for order {orderId}");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while fetching order details."));
            }
        }

        // ===================================
        // POST: api/User/Orders/{orderId}/Cancel
        // Cancel an order
        // ===================================
        [HttpPost("{orderId}/Cancel")]
        public async Task<IActionResult> CancelOrder(long orderId, [FromBody] CancelOrderDto cancelData)
        {
            try
            {
                // Validate model
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Get authenticated user ID
                long userId = _currentUser.UserId;
                if (userId <= 0)
                {
                    return ApiResponseHelper.Failure("User not authenticated.");
                }

                if (orderId <= 0)
                {
                    return ApiResponseHelper.Failure("Invalid order ID.");
                }

                if (cancelData == null || string.IsNullOrWhiteSpace(cancelData.Reason))
                {
                    return ApiResponseHelper.Failure("Cancellation reason is required.");
                }

                _logger.LogInformation($"Cancelling order {orderId} for user {userId}");

                // Create service
                INotificationService notificationService = new NotificationService(_configuration);
                OrderService orderService = new OrderService(_connStr, notificationService, _fileStorageService);

                // Cancel order
                bool cancelled = await orderService.CancelOrderAsync(userId, orderId, cancelData.Reason);

                if (cancelled)
                {
                    _logger.LogInformation($"Order {orderId} cancelled successfully");
                    return ApiResponseHelper.Success(null, "Order cancelled successfully.");
                }
                else
                {
                    return ApiResponseHelper.Failure("Failed to cancel order.", "warning");
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Order cancellation failed: {ex.Message}");
                return ApiResponseHelper.Failure(ex.Message, "warning");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error cancelling order {orderId}");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while cancelling your order."));
            }
        }
    }
}
