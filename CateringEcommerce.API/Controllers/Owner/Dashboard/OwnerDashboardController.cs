using CateringEcommerce.API.Helpers;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Interfaces.Owner;
using CateringEcommerce.API.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CateringEcommerce.API.Controllers.Owner.Dashboard
{
    /// <summary>
    /// Owner Dashboard Controller
    /// Provides dashboard metrics, charts, and analytics for partner owners
    /// </summary>
    [OwnerAuthorize]
    [ApiController]
    [Route("api/Owner/[controller]")]
    public class OwnerDashboardController : ControllerBase
    {
        private readonly ILogger<OwnerDashboardController> _logger;
        private readonly ICurrentUserService _currentUser;
        private readonly IOwnerDashboardRepository _dashboardRepository;

        public OwnerDashboardController(
            ILogger<OwnerDashboardController> logger,
            ICurrentUserService currentUser,
            IOwnerDashboardRepository dashboardRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _dashboardRepository = dashboardRepository ?? throw new ArgumentNullException(nameof(dashboardRepository));
        }

        /// <summary>
        /// Get dashboard metrics with percentage changes
        /// </summary>
        /// <returns>Dashboard metrics</returns>
        [HttpGet("metrics")]
        public async Task<IActionResult> GetDashboardMetrics()
        {
            try
            {
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                {
                    return ApiResponseHelper.Failure("Owner not authenticated.");
                }

                _logger.LogInformation($"Getting dashboard metrics for owner {ownerId}");

                var metrics = await _dashboardRepository.GetDashboardMetrics(ownerId);

                return ApiResponseHelper.Success(metrics, "Dashboard metrics retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard metrics");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while retrieving dashboard metrics."));
            }
        }

        /// <summary>
        /// Get revenue chart data by period
        /// </summary>
        /// <param name="period">Period: day, week, month, year</param>
        /// <returns>Revenue chart data</returns>
        [HttpGet("revenue-chart")]
        public async Task<IActionResult> GetRevenueChart([FromQuery] string period = "month")
        {
            try
            {
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                {
                    return ApiResponseHelper.Failure("Owner not authenticated.");
                }

                _logger.LogInformation($"Getting revenue chart for owner {ownerId}, period: {period}");

                var chart = await _dashboardRepository.GetRevenueChart(ownerId, period);

                return ApiResponseHelper.Success(chart, "Revenue chart data retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting revenue chart");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while retrieving revenue chart."));
            }
        }

        /// <summary>
        /// Get orders chart data by period
        /// </summary>
        /// <param name="period">Period: day, week, month, year</param>
        /// <returns>Orders chart data</returns>
        [HttpGet("orders-chart")]
        public async Task<IActionResult> GetOrdersChart([FromQuery] string period = "month")
        {
            try
            {
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                {
                    return ApiResponseHelper.Failure("Owner not authenticated.");
                }

                _logger.LogInformation($"Getting orders chart for owner {ownerId}, period: {period}");

                var chart = await _dashboardRepository.GetOrdersChart(ownerId, period);

                return ApiResponseHelper.Success(chart, "Orders chart data retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders chart");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while retrieving orders chart."));
            }
        }

        /// <summary>
        /// Get recent orders
        /// </summary>
        /// <param name="limit">Number of recent orders to retrieve (default: 5)</param>
        /// <returns>List of recent orders</returns>
        [HttpGet("recent-orders")]
        public async Task<IActionResult> GetRecentOrders([FromQuery] int limit = 5)
        {
            try
            {
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                {
                    return ApiResponseHelper.Failure("Owner not authenticated.");
                }

                _logger.LogInformation($"Getting recent orders for owner {ownerId}, limit: {limit}");

                var orders = await _dashboardRepository.GetRecentOrders(ownerId, limit);

                return ApiResponseHelper.Success(orders, "Recent orders retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent orders");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while retrieving recent orders."));
            }
        }

        /// <summary>
        /// Get upcoming events within specified days
        /// </summary>
        /// <param name="days">Number of days ahead to check (default: 7)</param>
        /// <returns>List of upcoming events</returns>
        [HttpGet("upcoming-events")]
        public async Task<IActionResult> GetUpcomingEvents([FromQuery] int days = 7)
        {
            try
            {
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                {
                    return ApiResponseHelper.Failure("Owner not authenticated.");
                }

                _logger.LogInformation($"Getting upcoming events for owner {ownerId}, days: {days}");

                var events = await _dashboardRepository.GetUpcomingEvents(ownerId, days);

                return ApiResponseHelper.Success(events, "Upcoming events retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting upcoming events");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while retrieving upcoming events."));
            }
        }

        /// <summary>
        /// Get top performing menu items
        /// </summary>
        /// <param name="limit">Number of top items to retrieve (default: 10)</param>
        /// <returns>List of top menu items</returns>
        [HttpGet("top-items")]
        public async Task<IActionResult> GetTopMenuItems([FromQuery] int limit = 10)
        {
            try
            {
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                {
                    return ApiResponseHelper.Failure("Owner not authenticated.");
                }

                _logger.LogInformation($"Getting top menu items for owner {ownerId}, limit: {limit}");

                var items = await _dashboardRepository.GetTopMenuItems(ownerId, limit);

                return ApiResponseHelper.Success(items, "Top menu items retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top menu items");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while retrieving top menu items."));
            }
        }

        /// <summary>
        /// Get performance insights
        /// </summary>
        /// <returns>Performance insights</returns>
        [HttpGet("insights")]
        public async Task<IActionResult> GetPerformanceInsights()
        {
            try
            {
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                {
                    return ApiResponseHelper.Failure("Owner not authenticated.");
                }

                _logger.LogInformation($"Getting performance insights for owner {ownerId}");

                var insights = await _dashboardRepository.GetPerformanceInsights(ownerId);

                return ApiResponseHelper.Success(insights, "Performance insights retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance insights");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while retrieving performance insights."));
            }
        }

        /// <summary>
        /// Get revenue breakdown by event type, payment status, etc.
        /// </summary>
        /// <returns>Revenue breakdown</returns>
        [HttpGet("revenue-breakdown")]
        public async Task<IActionResult> GetRevenueBreakdown()
        {
            try
            {
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                {
                    return ApiResponseHelper.Failure("Owner not authenticated.");
                }

                _logger.LogInformation($"Getting revenue breakdown for owner {ownerId}");

                var breakdown = await _dashboardRepository.GetRevenueBreakdown(ownerId);

                return ApiResponseHelper.Success(breakdown, "Revenue breakdown retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting revenue breakdown");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while retrieving revenue breakdown."));
            }
        }
    }
}
