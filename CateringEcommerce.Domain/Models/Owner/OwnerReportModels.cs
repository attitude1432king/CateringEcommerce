using System;
using System.Collections.Generic;

namespace CateringEcommerce.Domain.Models.Owner
{
    // Report Filter DTO
    public class ReportFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string EventType { get; set; }
        public string ReportType { get; set; } // Sales, Revenue, Customer, MenuPerformance
        public string GroupBy { get; set; } = "Month"; // Day, Week, Month, Quarter, Year
        public bool IncludeCharts { get; set; } = true;
    }

    // Sales Report DTO
    public class SalesReportDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Summary
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int PendingOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int TotalGuestsServed { get; set; }

        // Event Breakdown
        public Dictionary<string, SalesBreakdownDto> EventTypeBreakdown { get; set; } = new Dictionary<string, SalesBreakdownDto>();

        // Time Series Data
        public List<SalesTimeSeriesDto> TimeSeries { get; set; } = new List<SalesTimeSeriesDto>();

        // Comparison
        public decimal RevenueGrowth { get; set; }
        public decimal OrderGrowth { get; set; }
    }

    // Sales Breakdown DTO
    public class SalesBreakdownDto
    {
        public int OrderCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal Percentage { get; set; }
    }

    // Sales Time Series DTO
    public class SalesTimeSeriesDto
    {
        public string Period { get; set; } // Date label
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
        public int GuestsServed { get; set; }
    }

    // Revenue Report DTO
    public class RevenueReportDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Revenue Summary
        public decimal GrossRevenue { get; set; }
        public decimal NetRevenue { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalDiscounts { get; set; }
        public decimal DeliveryCharges { get; set; }
        public decimal PendingPayments { get; set; }

        // Payment Breakdown
        public Dictionary<string, decimal> PaymentMethodBreakdown { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, decimal> PaymentStatusBreakdown { get; set; } = new Dictionary<string, decimal>();

        // Monthly Revenue
        public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new List<MonthlyRevenueDto>();

        // Revenue by Event Type
        public Dictionary<string, decimal> RevenueByEventType { get; set; } = new Dictionary<string, decimal>();

        // Comparison
        public decimal RevenueGrowth { get; set; }
        public decimal ProfitMargin { get; set; }
    }

    // Monthly Revenue DTO
    public class MonthlyRevenueDto
    {
        public string Month { get; set; }
        public int Year { get; set; }
        public decimal GrossRevenue { get; set; }
        public decimal NetRevenue { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public int OrderCount { get; set; }
    }

    // Customer Report DTO
    public class CustomerReportDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Customer Summary
        public int TotalCustomers { get; set; }
        public int NewCustomers { get; set; }
        public int ReturningCustomers { get; set; }
        public decimal CustomerRetentionRate { get; set; }
        public decimal AverageLifetimeValue { get; set; }
        public decimal CustomerSatisfactionScore { get; set; }

        // Customer Distribution
        public Dictionary<string, int> CustomersByType { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> CustomersByCity { get; set; } = new Dictionary<string, int>();

        // Top Customers
        public List<TopCustomerDto> TopCustomers { get; set; } = new List<TopCustomerDto>();

        // Customer Acquisition
        public List<CustomerAcquisitionDto> CustomerAcquisition { get; set; } = new List<CustomerAcquisitionDto>();

        // Churn Analysis
        public int ChurnedCustomers { get; set; }
        public decimal ChurnRate { get; set; }
    }

    // Customer Acquisition DTO
    public class CustomerAcquisitionDto
    {
        public string Month { get; set; }
        public int NewCustomers { get; set; }
        public int ReturningCustomers { get; set; }
        public decimal AcquisitionCost { get; set; }
    }

    // Menu Performance Report DTO
    public class MenuPerformanceReportDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Summary
        public int TotalMenuItems { get; set; }
        public int ActiveItems { get; set; }
        public decimal TotalMenuRevenue { get; set; }

        // Top Performers
        public List<MenuItemPerformanceDto> TopItems { get; set; } = new List<MenuItemPerformanceDto>();

        // Low Performers
        public List<MenuItemPerformanceDto> LowPerformingItems { get; set; } = new List<MenuItemPerformanceDto>();

        // Category Performance
        public Dictionary<string, CategoryPerformanceDto> CategoryPerformance { get; set; } = new Dictionary<string, CategoryPerformanceDto>();

        // Recommendations
        public List<string> Recommendations { get; set; } = new List<string>();
    }

    // Menu Item Performance DTO
    public class MenuItemPerformanceDto
    {
        public long MenuItemId { get; set; }
        public string MenuItemName { get; set; }
        public string Category { get; set; }
        public int OrderCount { get; set; }
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageRating { get; set; }
        public decimal Price { get; set; }
        public decimal RevenuePercentage { get; set; }
        public string PerformanceCategory { get; set; } // Hot, Average, Cold
    }

    // Category Performance DTO
    public class CategoryPerformanceDto
    {
        public string CategoryName { get; set; }
        public int ItemCount { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageRating { get; set; }
        public decimal RevenuePercentage { get; set; }
    }

    // Financial Report DTO
    public class FinancialReportDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Income
        public decimal TotalIncome { get; set; }
        public decimal FoodRevenue { get; set; }
        public decimal DecorationRevenue { get; set; }
        public decimal StaffRevenue { get; set; }
        public decimal OtherRevenue { get; set; }

        // Expenses (if tracked)
        public decimal TotalExpenses { get; set; }

        // Net Profit
        public decimal NetProfit { get; set; }
        public decimal ProfitMargin { get; set; }

        // Outstanding
        public decimal OutstandingReceivables { get; set; }
        public List<OutstandingPaymentDto> OutstandingPayments { get; set; } = new List<OutstandingPaymentDto>();
    }

    // Outstanding Payment DTO
    public class OutstandingPaymentDto
    {
        public long OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string CustomerName { get; set; }
        public DateTime EventDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceAmount { get; set; }
        public int DaysOverdue { get; set; }
    }

    // Report Export DTO
    public class ReportExportDto
    {
        public string ReportType { get; set; }
        public string Format { get; set; } // CSV, PDF
        public string FileName { get; set; }
        public byte[] FileContent { get; set; }
        public string ContentType { get; set; }
    }
}
