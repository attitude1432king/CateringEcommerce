using CateringEcommerce.API.Filters;
using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Base.Admin;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.AspNetCore.Mvc;

namespace CateringEcommerce.API.Controllers.Admin
{
    [Route("api/admin/dashboard")]
    [ApiController]
    [AdminAuthorize]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IAdminDashboardRepository _dashboardRepository;
        private readonly AdminAnalyticsRepository _analyticsRepo;
        private readonly ILogger<AdminDashboardController> _logger;

        public AdminDashboardController(
            IAdminDashboardRepository dashboardRepository,
            AdminAnalyticsRepository analyticsRepo,
            ILogger<AdminDashboardController> logger)
        {
            _dashboardRepository = dashboardRepository ?? throw new ArgumentNullException(nameof(dashboardRepository));
            _analyticsRepo = analyticsRepo ?? throw new ArgumentNullException(nameof(analyticsRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get complete dashboard metrics including stats, charts, and recent data (Legacy)
        /// </summary>
        [HttpGet("metrics")]
        public IActionResult GetDashboardMetrics()
        {
            try
            {
                var metrics = _dashboardRepository.GetDashboardMetrics();
                return ApiResponseHelper.Success(metrics, "Dashboard metrics retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get dashboard metrics");
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        // =============================================
        // COMPREHENSIVE ANALYTICS ENDPOINTS
        // =============================================

        /// <summary>
        /// Get dashboard metrics with date range and percentage changes
        /// </summary>
        [HttpGet("v2/metrics")]
        public async Task<IActionResult> GetDashboardMetricsV2([FromQuery] DashboardMetricsRequest request)
        {
            try
            {
                var metrics = await _analyticsRepo.GetDashboardMetricsAsync(request);
                return ApiResponseHelper.Success(metrics, "Dashboard metrics retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get revenue chart data with customizable date range and granularity
        /// </summary>
        [HttpGet("revenue-chart")]
        public async Task<IActionResult> GetRevenueChart([FromQuery] RevenueChartRequest request)
        {
            try
            {
                var chartData = await _analyticsRepo.GetRevenueChartAsync(request);
                return ApiResponseHelper.Success(chartData, "Revenue chart data retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get order analytics including status distribution
        /// </summary>
        [HttpGet("order-analytics")]
        public async Task<IActionResult> GetOrderAnalytics([FromQuery] OrderAnalyticsRequest request)
        {
            try
            {
                var analytics = await _analyticsRepo.GetOrderAnalyticsAsync(request);
                return ApiResponseHelper.Success(analytics, "Order analytics retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get top performing partners
        /// </summary>
        [HttpGet("top-partners")]
        public async Task<IActionResult> GetTopPartners([FromQuery] PartnerAnalyticsRequest request)
        {
            try
            {
                var partners = await _analyticsRepo.GetTopPartnersAsync(request);
                return ApiResponseHelper.Success(partners, "Top partners retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get recent orders
        /// </summary>
        [HttpGet("recent-orders")]
        public async Task<IActionResult> GetRecentOrders([FromQuery] int limit = 10)
        {
            try
            {
                var orders = await _analyticsRepo.GetRecentOrdersAsync(limit);
                return ApiResponseHelper.Success(orders, "Recent orders retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get popular food categories
        /// </summary>
        [HttpGet("popular-categories")]
        public async Task<IActionResult> GetPopularCategories([FromQuery] CategoryAnalyticsRequest request)
        {
            try
            {
                var categories = await _analyticsRepo.GetPopularCategoriesAsync(request);
                return ApiResponseHelper.Success(categories, "Popular categories retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get user growth analytics
        /// </summary>
        [HttpGet("user-growth")]
        public async Task<IActionResult> GetUserGrowth([FromQuery] UserGrowthRequest request)
        {
            try
            {
                var growth = await _analyticsRepo.GetUserGrowthAsync(request);
                return ApiResponseHelper.Success(growth, "User growth data retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get revenue by city
        /// </summary>
        [HttpGet("city-revenue")]
        public async Task<IActionResult> GetCityRevenue([FromQuery] CityAnalyticsRequest request)
        {
            try
            {
                var cityRevenue = await _analyticsRepo.GetCityRevenueAsync(request);
                return ApiResponseHelper.Success(cityRevenue, "City revenue data retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Export analytics data
        /// </summary>
        [HttpPost("export")]
        public async Task<IActionResult> ExportAnalytics([FromBody] AnalyticsExportRequest request)
        {
            try
            {
                var exportResult = await _analyticsRepo.ExportAnalyticsAsync(request);
                return ApiResponseHelper.Success(exportResult, "Analytics exported successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }
    }
}
