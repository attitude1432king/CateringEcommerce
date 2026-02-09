using System;
using System.Collections.Generic;

namespace CateringEcommerce.Domain.Models.Admin
{
    // =============================================
    // Dashboard Metrics Models
    // =============================================

    public class DashboardMetrics
    {
        public int TotalUsers { get; set; }
        public decimal UsersChangePercent { get; set; }
        public int ActiveCaterings { get; set; }
        public decimal CateringsChangePercent { get; set; }
        public int TotalOrders { get; set; }
        public decimal OrdersChangePercent { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal RevenueChangePercent { get; set; }
        public decimal TotalCommission { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int PendingApprovals { get; set; }
        public decimal AverageRating { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }

    public class DashboardMetricsRequest
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    // =============================================
    // Chart Data Models
    // =============================================

    public class RevenueChartDataPoint
    {
        public DateTime Date { get; set; }
        public string Label { get; set; }
        public decimal Revenue { get; set; }
        public decimal Commission { get; set; }
        public int OrderCount { get; set; }
    }

    public class RevenueChartRequest
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Granularity { get; set; } = "day"; // day, week, month
    }

    public class RevenueChartResponse
    {
        public List<RevenueChartDataPoint> DataPoints { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCommission { get; set; }
        public int TotalOrders { get; set; }
    }

    // =============================================
    // Order Analytics Models
    // =============================================

    public class OrderStatusDistribution
    {
        public string Status { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class OrderAnalyticsRequest
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class OrderAnalyticsResponse
    {
        public List<OrderStatusDistribution> StatusDistribution { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int CompletedOrders { get; set; }
        public int PendingOrders { get; set; }
        public int CancelledOrders { get; set; }
    }

    // =============================================
    // Partner Analytics Models
    // =============================================

    public class TopPerformingPartner
    {
        public long CateringOwnerId { get; set; }
        public string BusinessName { get; set; }
        public string ContactPerson { get; set; }
        public string City { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageRating { get; set; }
        public int UniqueCustomers { get; set; }
    }

    public class PartnerAnalyticsRequest
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Limit { get; set; } = 10;
    }

    public class PartnerAnalyticsResponse
    {
        public List<TopPerformingPartner> TopPartners { get; set; }
        public int TotalActivePartners { get; set; }
        public int NewPartnersInPeriod { get; set; }
    }

    // =============================================
    // Recent Orders Models
    // =============================================

    public class RecentOrderItem
    {
        public long OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CateringName { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime EventDate { get; set; }
        public DateTime OrderDate { get; set; }
    }

    // =============================================
    // Category Analytics Models
    // =============================================

    public class PopularCategory
    {
        public long CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int OrderCount { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class CategoryAnalyticsRequest
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Limit { get; set; } = 10;
    }

    public class CategoryAnalyticsResponse
    {
        public List<PopularCategory> PopularCategories { get; set; }
        public int TotalCategories { get; set; }
    }

    // =============================================
    // User Growth Models
    // =============================================

    public class UserGrowthDataPoint
    {
        public DateTime Date { get; set; }
        public string Label { get; set; }
        public int NewUsers { get; set; }
        public int CumulativeUsers { get; set; }
    }

    public class UserGrowthRequest
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Granularity { get; set; } = "day"; // day, month
    }

    public class UserGrowthResponse
    {
        public List<UserGrowthDataPoint> DataPoints { get; set; }
        public int TotalNewUsers { get; set; }
        public int TotalUsers { get; set; }
    }

    // =============================================
    // City Analytics Models
    // =============================================

    public class CityRevenue
    {
        public long CityId { get; set; }
        public string CityName { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int ActivePartners { get; set; }
    }

    public class CityAnalyticsRequest
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Limit { get; set; } = 10;
    }

    public class CityAnalyticsResponse
    {
        public List<CityRevenue> CityRevenues { get; set; }
        public int TotalActiveCities { get; set; }
    }

    // =============================================
    // Export Models
    // =============================================

    public class AnalyticsExportRequest
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string ExportType { get; set; } // revenue, orders, partners, users
        public string Format { get; set; } = "excel"; // excel, csv, pdf
    }

    public class AnalyticsExportResponse
    {
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public long FileSizeBytes { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}
