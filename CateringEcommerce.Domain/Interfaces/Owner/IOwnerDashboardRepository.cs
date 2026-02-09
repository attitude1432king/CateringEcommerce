using System.Collections.Generic;
using System.Threading.Tasks;
using CateringEcommerce.Domain.Models.Owner;

namespace CateringEcommerce.Domain.Interfaces.Owner
{
    public interface IOwnerDashboardRepository
    {
        /// <summary>
        /// Get dashboard metrics with percentage changes
        /// </summary>
        Task<DashboardMetricsDto> GetDashboardMetrics(long ownerId);

        /// <summary>
        /// Get revenue chart data by period (day, week, month, year)
        /// </summary>
        Task<RevenueChartDto> GetRevenueChart(long ownerId, string period = "month");

        /// <summary>
        /// Get orders chart data by period
        /// </summary>
        Task<OrderChartDto> GetOrdersChart(long ownerId, string period = "month");

        /// <summary>
        /// Get recent orders
        /// </summary>
        Task<List<RecentOrderDto>> GetRecentOrders(long ownerId, int limit = 5);

        /// <summary>
        /// Get upcoming events within specified days
        /// </summary>
        Task<List<UpcomingEventDto>> GetUpcomingEvents(long ownerId, int days = 7);

        /// <summary>
        /// Get top performing menu items
        /// </summary>
        Task<List<TopMenuItemDto>> GetTopMenuItems(long ownerId, int limit = 10);

        /// <summary>
        /// Get performance insights
        /// </summary>
        Task<PerformanceInsightsDto> GetPerformanceInsights(long ownerId);

        /// <summary>
        /// Get revenue breakdown by event type, payment status, etc.
        /// </summary>
        Task<RevenueBreakdownDto> GetRevenueBreakdown(long ownerId);
    }
}
