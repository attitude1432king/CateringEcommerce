using CateringEcommerce.API.Helpers;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Interfaces.Owner;
using CateringEcommerce.Domain.Models.Owner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CateringEcommerce.API.Controllers.Owner.Dashboard
{
    /// <summary>
    /// Owner Customers Controller
    /// Provides customer management and analytics for partner owners
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/Owner/[controller]")]
    public class OwnerCustomersController : ControllerBase
    {
        private readonly ILogger<OwnerCustomersController> _logger;
        private readonly ICurrentUserService _currentUser;
        private readonly IOwnerCustomerRepository _customerRepository;

        public OwnerCustomersController(
            ILogger<OwnerCustomersController> logger,
            ICurrentUserService currentUser,
            IOwnerCustomerRepository customerRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
        }

        /// <summary>
        /// Get filtered and paginated customers list
        /// </summary>
        /// <param name="filter">Customer filter parameters</param>
        /// <returns>Paginated customers list</returns>
        [HttpPost("list")]
        public async Task<IActionResult> GetCustomersList([FromBody] CustomerFilterDto filter)
        {
            try
            {
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                {
                    return ApiResponseHelper.Failure("Owner not authenticated.");
                }

                _logger.LogInformation($"Getting customers list for owner {ownerId}, page: {filter.Page}");

                var customers = await _customerRepository.GetCustomersList(ownerId, filter);

                return ApiResponseHelper.Success(customers, "Customers list retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customers list");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while retrieving customers list."));
            }
        }

        /// <summary>
        /// Get customer details with statistics
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <returns>Customer details</returns>
        [HttpGet("{customerId}")]
        public async Task<IActionResult> GetCustomerDetails(long customerId)
        {
            try
            {
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                {
                    return ApiResponseHelper.Failure("Owner not authenticated.");
                }

                _logger.LogInformation($"Getting customer details for owner {ownerId}, customer: {customerId}");

                var customerDetails = await _customerRepository.GetCustomerDetails(ownerId, customerId);

                return ApiResponseHelper.Success(customerDetails, "Customer details retrieved successfully.");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized access to customer {customerId}: {ex.Message}");
                return ApiResponseHelper.Failure($"Unauthorized access to customer {customerId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting customer details for customer {customerId}");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while retrieving customer details."));
            }
        }

        /// <summary>
        /// Get customer order history
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <returns>Customer order history</returns>
        [HttpGet("{customerId}/orders")]
        public async Task<IActionResult> GetCustomerOrderHistory(long customerId)
        {
            try
            {
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                {
                    return ApiResponseHelper.Failure("Owner not authenticated.");
                }

                _logger.LogInformation($"Getting order history for owner {ownerId}, customer: {customerId}");

                var orderHistory = await _customerRepository.GetCustomerOrderHistory(ownerId, customerId);

                return ApiResponseHelper.Success(orderHistory, "Customer order history retrieved successfully.");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized access to customer {customerId}: {ex.Message}");
                return ApiResponseHelper.Failure($"Unauthorized access to customer {customerId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting order history for customer {customerId}");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while retrieving customer order history."));
            }
        }

        /// <summary>
        /// Get customer insights and analytics
        /// </summary>
        /// <returns>Customer insights</returns>
        [HttpGet("insights")]
        public async Task<IActionResult> GetCustomerInsights()
        {
            try
            {
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                {
                    return ApiResponseHelper.Failure("Owner not authenticated.");
                }

                _logger.LogInformation($"Getting customer insights for owner {ownerId}");

                var insights = await _customerRepository.GetCustomerInsights(ownerId);

                return ApiResponseHelper.Success(insights, "Customer insights retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer insights");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while retrieving customer insights."));
            }
        }

        /// <summary>
        /// Get top customers by revenue or order count
        /// </summary>
        /// <param name="limit">Number of top customers to retrieve (default: 10)</param>
        /// <param name="sortBy">Sort by: LifetimeValue or TotalOrders (default: LifetimeValue)</param>
        /// <returns>List of top customers</returns>
        [HttpGet("top")]
        public async Task<IActionResult> GetTopCustomers([FromQuery] int limit = 10, [FromQuery] string sortBy = "LifetimeValue")
        {
            try
            {
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                {
                    return ApiResponseHelper.Failure("Owner not authenticated.");
                }

                _logger.LogInformation($"Getting top customers for owner {ownerId}, limit: {limit}, sortBy: {sortBy}");

                var topCustomers = await _customerRepository.GetTopCustomers(ownerId, limit, sortBy);

                return ApiResponseHelper.Success(topCustomers, "Top customers retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top customers");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while retrieving top customers."));
            }
        }
    }
}
