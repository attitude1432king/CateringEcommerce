using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Models.Admin;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace CateringEcommerce.BAL.Base.Admin
{
    /// <summary>
    /// Repository for Admin Analytics
    /// Provides comprehensive analytics data for admin dashboard
    /// </summary>
    public class AdminAnalyticsRepository
    {
        private readonly IDatabaseHelper _dbHelper;
        public AdminAnalyticsRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        // =============================================
        // Dashboard Metrics
        // =============================================

        public async Task<DashboardMetrics> GetDashboardMetricsAsync(DashboardMetricsRequest request)
        {
            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@FromDate", (object)request.FromDate ?? DBNull.Value),
                new NpgsqlParameter("@ToDate", (object)request.ToDate ?? DBNull.Value)
            };

            var dt = await _dbHelper.ExecuteStoredProcedureAsync<DataTable>("sp_Admin_GetDashboardMetrics", parameters.ToArray());

            if (dt != null && dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                return new DashboardMetrics
                {
                    TotalUsers = Convert.ToInt32(row["TotalUsers"]),
                    UsersChangePercent = Convert.ToDecimal(row["UsersChangePercent"]),
                    ActiveCaterings = Convert.ToInt32(row["ActiveCaterings"]),
                    CateringsChangePercent = Convert.ToDecimal(row["CateringsChangePercent"]),
                    TotalOrders = Convert.ToInt32(row["TotalOrders"]),
                    OrdersChangePercent = Convert.ToDecimal(row["OrdersChangePercent"]),
                    TotalRevenue = Convert.ToDecimal(row["TotalRevenue"]),
                    RevenueChangePercent = Convert.ToDecimal(row["RevenueChangePercent"]),
                    TotalCommission = Convert.ToDecimal(row["TotalCommission"]),
                    AverageOrderValue = Convert.ToDecimal(row["AverageOrderValue"]),
                    PendingApprovals = Convert.ToInt32(row["PendingApprovals"]),
                    AverageRating = Convert.ToDecimal(row["AverageRating"]),
                    PeriodStart = Convert.ToDateTime(row["PeriodStart"]),
                    PeriodEnd = Convert.ToDateTime(row["PeriodEnd"])
                };
            }

            return new DashboardMetrics();
        }

        // =============================================
        // Revenue Chart
        // =============================================

        public async Task<RevenueChartResponse> GetRevenueChartAsync(RevenueChartRequest request)
        {
            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@FromDate", (object)request.FromDate ?? DBNull.Value),
                new NpgsqlParameter("@ToDate", (object)request.ToDate ?? DBNull.Value),
                new NpgsqlParameter("@Granularity", request.Granularity ?? "day")
            };

            var dt = await _dbHelper.ExecuteStoredProcedureAsync<DataTable>("sp_Admin_GetRevenueChart", parameters.ToArray());

            var dataPoints = new List<RevenueChartDataPoint>();
            decimal totalRevenue = 0;
            decimal totalCommission = 0;
            int totalOrders = 0;

            if (dt == null || dt.Rows.Count == 0)
            {
                return new RevenueChartResponse
                {
                    DataPoints = dataPoints,
                    TotalRevenue = totalRevenue,
                    TotalCommission = totalCommission,
                    TotalOrders = totalOrders
                };
            }

            foreach (DataRow row in dt.Rows)
            {
                var dataPoint = new RevenueChartDataPoint
                {
                    Date = Convert.ToDateTime(row["Date"]),
                    Label = row["Label"].ToString(),
                    Revenue = Convert.ToDecimal(row["Revenue"]),
                    Commission = Convert.ToDecimal(row["Commission"]),
                    OrderCount = Convert.ToInt32(row["OrderCount"])
                };

                dataPoints.Add(dataPoint);
                totalRevenue += dataPoint.Revenue;
                totalCommission += dataPoint.Commission;
                totalOrders += dataPoint.OrderCount;
            }

            return new RevenueChartResponse
            {
                DataPoints = dataPoints,
                TotalRevenue = totalRevenue,
                TotalCommission = totalCommission,
                TotalOrders = totalOrders
            };
        }

        // =============================================
        // Order Analytics
        // =============================================

        public async Task<OrderAnalyticsResponse> GetOrderAnalyticsAsync(OrderAnalyticsRequest request)
        {
            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@FromDate", (object)request.FromDate ?? DBNull.Value),
                new NpgsqlParameter("@ToDate", (object)request.ToDate ?? DBNull.Value)
            };

            var dt = await _dbHelper.ExecuteStoredProcedureAsync<DataTable>("sp_Admin_GetOrderStatusDistribution", parameters.ToArray());

            var statusDistribution = new List<OrderStatusDistribution>();
            int totalOrders = 0;
            int completedOrders = 0;
            int pendingOrders = 0;
            int cancelledOrders = 0;

            if (dt == null || dt.Rows.Count == 0)
            {
                return new OrderAnalyticsResponse
                {
                    StatusDistribution = statusDistribution,
                    TotalOrders = totalOrders,
                    CompletedOrders = completedOrders,
                    PendingOrders = pendingOrders,
                    CancelledOrders = cancelledOrders,
                    AverageOrderValue = 0
                };
            }

            foreach (DataRow row in dt.Rows)
            {
                var status = row["Status"].ToString();
                var count = Convert.ToInt32(row["Count"]);

                statusDistribution.Add(new OrderStatusDistribution
                {
                    Status = status,
                    Count = count,
                    Percentage = Convert.ToDecimal(row["Percentage"])
                });

                totalOrders += count;

                if (status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                    completedOrders = count;
                else if (status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                    pendingOrders = count;
                else if (status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
                    cancelledOrders = count;
            }

            return new OrderAnalyticsResponse
            {
                StatusDistribution = statusDistribution,
                TotalOrders = totalOrders,
                CompletedOrders = completedOrders,
                PendingOrders = pendingOrders,
                CancelledOrders = cancelledOrders,
                AverageOrderValue = 0 // Calculated from metrics
            };
        }

        // =============================================
        // Partner Analytics
        // =============================================

        public async Task<PartnerAnalyticsResponse> GetTopPartnersAsync(PartnerAnalyticsRequest request)
        {
            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@FromDate", (object)request.FromDate ?? DBNull.Value),
                new NpgsqlParameter("@ToDate", (object)request.ToDate ?? DBNull.Value),
                new NpgsqlParameter("@Limit", request.Limit)
            };

            var dt = await _dbHelper.ExecuteStoredProcedureAsync<DataTable>("sp_Admin_GetTopPerformingPartners", parameters.ToArray());

            var topPartners = new List<TopPerformingPartner>();

            if (dt == null || dt.Rows.Count == 0)
            {
                return new PartnerAnalyticsResponse
                {
                    TopPartners = topPartners,
                    TotalActivePartners = 0,
                    NewPartnersInPeriod = 0
                };
            }

            foreach (DataRow row in dt.Rows)
            {
                topPartners.Add(new TopPerformingPartner
                {
                    CateringOwnerId = Convert.ToInt64(row["CateringOwnerId"]),
                    BusinessName = row["BusinessName"].ToString(),
                    ContactPerson = row["ContactPerson"].ToString(),
                    City = row["City"].ToString(),
                    TotalOrders = Convert.ToInt32(row["TotalOrders"]),
                    TotalRevenue = Convert.ToDecimal(row["TotalRevenue"]),
                    AverageRating = Convert.ToDecimal(row["AverageRating"]),
                    UniqueCustomers = Convert.ToInt32(row["UniqueCustomers"])
                });
            }

            return new PartnerAnalyticsResponse
            {
                TopPartners = topPartners,
                TotalActivePartners = topPartners.Count,
                NewPartnersInPeriod = 0 // Would need additional query
            };
        }

        // =============================================
        // Recent Orders
        // =============================================

        public async Task<List<RecentOrderItem>> GetRecentOrdersAsync(int limit)
        {
            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@Limit", limit)
            };

            var dt = await _dbHelper.ExecuteStoredProcedureAsync<DataTable>("sp_Admin_GetRecentOrders", parameters.ToArray());

            var orders = new List<RecentOrderItem>();

            if (dt == null || dt.Rows.Count == 0)
            {
                return orders;
            }

            foreach (DataRow row in dt.Rows)
            {
                orders.Add(new RecentOrderItem
                {
                    OrderId = Convert.ToInt64(row["OrderId"]),
                    OrderNumber = row["OrderNumber"].ToString(),
                    CustomerName = row["CustomerName"].ToString(),
                    CustomerEmail = row["CustomerEmail"].ToString(),
                    CateringName = row["CateringName"].ToString(),
                    TotalAmount = Convert.ToDecimal(row["TotalAmount"]),
                    OrderStatus = row["OrderStatus"].ToString(),
                    PaymentStatus = row["PaymentStatus"].ToString(),
                    EventDate = Convert.ToDateTime(row["EventDate"]),
                    OrderDate = Convert.ToDateTime(row["OrderDate"])
                });
            }

            return orders;
        }

        // =============================================
        // Category Analytics
        // =============================================

        public async Task<CategoryAnalyticsResponse> GetPopularCategoriesAsync(CategoryAnalyticsRequest request)
        {
            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@FromDate", (object)request.FromDate ?? DBNull.Value),
                new NpgsqlParameter("@ToDate", (object)request.ToDate ?? DBNull.Value),
                new NpgsqlParameter("@Limit", request.Limit)
            };

            var dt = await _dbHelper.ExecuteStoredProcedureAsync<DataTable>("sp_Admin_GetPopularCategories", parameters.ToArray());

            var categories = new List<PopularCategory>();

            if (dt == null || dt.Rows.Count == 0)
            {
                return new CategoryAnalyticsResponse
                {
                    PopularCategories = categories,
                    TotalCategories = 0
                };
            }

            foreach (DataRow row in dt.Rows)
            {
                categories.Add(new PopularCategory
                {
                    CategoryId = Convert.ToInt64(row["CategoryId"]),
                    CategoryName = row["CategoryName"].ToString(),
                    OrderCount = Convert.ToInt32(row["OrderCount"]),
                    TotalQuantity = Convert.ToInt32(row["TotalQuantity"]),
                    TotalRevenue = Convert.ToDecimal(row["TotalRevenue"])
                });
            }

            return new CategoryAnalyticsResponse
            {
                PopularCategories = categories,
                TotalCategories = categories.Count
            };
        }

        // =============================================
        // User Growth
        // =============================================

        public async Task<UserGrowthResponse> GetUserGrowthAsync(UserGrowthRequest request)
        {
            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@FromDate", (object)request.FromDate ?? DBNull.Value),
                new NpgsqlParameter("@ToDate", (object)request.ToDate ?? DBNull.Value),
                new NpgsqlParameter("@Granularity", request.Granularity ?? "day")
            };

            var dt = await _dbHelper.ExecuteStoredProcedureAsync<DataTable>("sp_Admin_GetUserGrowth", parameters.ToArray());

            var dataPoints = new List<UserGrowthDataPoint>();
            int totalNewUsers = 0;

            if (dt == null || dt.Rows.Count == 0)
            {
                return new UserGrowthResponse
                {
                    DataPoints = dataPoints,
                    TotalNewUsers = 0,
                    TotalUsers = 0
                };
            }

            foreach (DataRow row in dt.Rows)
            {
                var newUsers = Convert.ToInt32(row["NewUsers"]);
                dataPoints.Add(new UserGrowthDataPoint
                {
                    Date = Convert.ToDateTime(row["Date"]),
                    Label = row["Label"].ToString(),
                    NewUsers = newUsers,
                    CumulativeUsers = Convert.ToInt32(row["CumulativeUsers"])
                });

                totalNewUsers += newUsers;
            }

            return new UserGrowthResponse
            {
                DataPoints = dataPoints,
                TotalNewUsers = totalNewUsers,
                TotalUsers = dataPoints.LastOrDefault()?.CumulativeUsers ?? 0
            };
        }

        // =============================================
        // City Revenue
        // =============================================

        public async Task<CityAnalyticsResponse> GetCityRevenueAsync(CityAnalyticsRequest request)
        {
            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@FromDate", (object)request.FromDate ?? DBNull.Value),
                new NpgsqlParameter("@ToDate", (object)request.ToDate ?? DBNull.Value),
                new NpgsqlParameter("@Limit", request.Limit)
            };

            var dt = await _dbHelper.ExecuteStoredProcedureAsync<DataTable>("sp_Admin_GetRevenueByCity", parameters.ToArray());

            var cityRevenues = new List<CityRevenue>();

            if (dt == null || dt.Rows.Count == 0)
            {
                return new CityAnalyticsResponse
                {
                    CityRevenues = cityRevenues,
                    TotalActiveCities = 0
                };
            }

            foreach (DataRow row in dt.Rows)
            {
                cityRevenues.Add(new CityRevenue
                {
                    CityId = Convert.ToInt64(row["CityId"]),
                    CityName = row["CityName"].ToString(),
                    TotalOrders = Convert.ToInt32(row["TotalOrders"]),
                    TotalRevenue = Convert.ToDecimal(row["TotalRevenue"]),
                    ActivePartners = Convert.ToInt32(row["ActivePartners"])
                });
            }

            return new CityAnalyticsResponse
            {
                CityRevenues = cityRevenues,
                TotalActiveCities = cityRevenues.Count
            };
        }

        // =============================================
        // Export Analytics
        // =============================================

        public async Task<AnalyticsExportResponse> ExportAnalyticsAsync(AnalyticsExportRequest request)
        {
            // This would generate Excel/CSV/PDF files
            // For now, return a placeholder response
            await Task.CompletedTask;

            return new AnalyticsExportResponse
            {
                FileName = $"analytics_export_{DateTime.Now:yyyyMMdd_HHmmss}.{request.Format}",
                FileUrl = "/exports/analytics_export.xlsx",
                FileSizeBytes = 0,
                GeneratedAt = DateTime.Now
            };
        }
    }
}
