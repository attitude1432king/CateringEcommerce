using System;
using System.Collections.Generic;

namespace CateringEcommerce.Domain.Models.Owner
{
    // Dashboard Metrics DTO
    public class DashboardMetricsDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal RevenueChange { get; set; } // Percentage change
        public int TotalOrders { get; set; }
        public decimal OrdersChange { get; set; }
        public int PendingOrders { get; set; }
        public decimal PendingOrdersChange { get; set; }
        public int TotalCustomers { get; set; }
        public decimal CustomersChange { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal AverageOrderValueChange { get; set; }
        public int UpcomingEvents { get; set; }
        public decimal CustomerSatisfaction { get; set; } // Average rating
    }

    // Revenue Chart DTO
    public class RevenueChartDto
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<decimal> Data { get; set; } = new List<decimal>();
        public string Period { get; set; } // day, week, month, year
        public decimal TotalRevenue { get; set; }
        public decimal AverageRevenue { get; set; }
    }

    // Orders Chart DTO
    public class OrderChartDto
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<int> Data { get; set; } = new List<int>();
        public string Period { get; set; }
        public int TotalOrders { get; set; }
        public Dictionary<string, int> OrdersByStatus { get; set; } = new Dictionary<string, int>();
    }

    // Recent Order DTO
    public class RecentOrderDto
    {
        public long OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string CustomerName { get; set; }
        public string EventType { get; set; }
        public DateTime EventDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; }
        public DateTime OrderDate { get; set; }
        public int GuestCount { get; set; }
    }

    // Upcoming Event DTO
    public class UpcomingEventDto
    {
        public long OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string EventType { get; set; }
        public DateTime EventDate { get; set; }
        public string EventTime { get; set; }
        public string VenueAddress { get; set; }
        public int GuestCount { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; }
        public int DaysUntilEvent { get; set; }
        public bool IsUrgent { get; set; }
    }

    // Top Menu Item DTO
    public class TopMenuItemDto
    {
        public long MenuItemId { get; set; }
        public string MenuItemName { get; set; }
        public string Category { get; set; }
        public int OrderCount { get; set; }
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageRating { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
    }

    // Performance Insights DTO
    public class PerformanceInsightsDto
    {
        public decimal RevenueGrowth { get; set; }
        public decimal OrderGrowth { get; set; }
        public decimal CustomerRetentionRate { get; set; }
        public decimal AverageDeliveryRating { get; set; }
        public int CancellationRate { get; set; }
        public string BestPerformingCategory { get; set; }
        public string PeakOrderDay { get; set; }
        public decimal PendingPaymentsAmount { get; set; }
    }

    // Revenue Breakdown DTO
    public class RevenueBreakdownDto
    {
        public Dictionary<string, decimal> ByEventType { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, decimal> ByPaymentStatus { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, decimal> ByMonth { get; set; } = new Dictionary<string, decimal>();
        public decimal GrossRevenue { get; set; }
        public decimal NetRevenue { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
    }
}
