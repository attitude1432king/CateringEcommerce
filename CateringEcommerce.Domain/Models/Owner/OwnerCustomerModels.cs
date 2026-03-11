using System;
using System.Collections.Generic;

namespace CateringEcommerce.Domain.Models.Owner
{
    // Customer Filter DTO
    public class CustomerFilterDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string CustomerType { get; set; } // New, Regular, VIP
        public string SearchTerm { get; set; } // Name, email, phone
        public string SortBy { get; set; } = "LastOrderDate"; // LastOrderDate, TotalOrders, LifetimeValue
        public string SortOrder { get; set; } = "DESC";
        public DateTime? RegisteredAfter { get; set; }
        public decimal? MinLifetimeValue { get; set; }
    }

    // Customer List DTO
    public class CustomerListDto
    {
        public long CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string CustomerType { get; set; } // New, Regular, VIP
        public int TotalOrders { get; set; }
        public decimal LifetimeValue { get; set; }
        public DateTime? LastOrderDate { get; set; }
        public DateTime RegisteredDate { get; set; }
        public decimal AverageOrderValue { get; set; }
        public string PreferredEventType { get; set; }
    }

    // Customer Detail DTO
    public class CustomerDetailDto
    {
        public long CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string CustomerType { get; set; }
        public DateTime RegisteredDate { get; set; }

        // Statistics
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal LifetimeValue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal OutstandingBalance { get; set; }

        // Preferences
        public string PreferredEventType { get; set; }
        public int AverageGuestCount { get; set; }
        public List<string> FavoriteMenuItems { get; set; } = new List<string>();

        // Recent Activity
        public DateTime? LastOrderDate { get; set; }
        public string LastOrderStatus { get; set; }
        public DateTime? NextEventDate { get; set; }

        // Contact Info
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Pincode { get; set; }
    }

    // Customer Order DTO
    public class CustomerOrderDto
    {
        public long OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string EventType { get; set; }
        public DateTime EventDate { get; set; }
        public DateTime OrderDate { get; set; }
        public int GuestCount { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; }
        public string PaymentStatus { get; set; }
        public decimal Rating { get; set; }
        public string ReviewText { get; set; }
    }

    // Customer Insights DTO
    public class CustomerInsightsDto
    {
        public int TotalCustomers { get; set; }
        public int NewCustomersThisMonth { get; set; }
        public int ReturningCustomers { get; set; }
        public decimal CustomerRetentionRate { get; set; }
        public decimal AverageLifetimeValue { get; set; }
        public decimal CustomerSatisfactionScore { get; set; }

        // Top Customers
        public List<TopCustomerDto> TopCustomers { get; set; } = new List<TopCustomerDto>();

        // Customer Distribution
        public Dictionary<string, int> CustomersByType { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> CustomersByEventType { get; set; } = new Dictionary<string, int>();

        // Trends
        public List<CustomerTrendDto> MonthlyTrends { get; set; } = new List<CustomerTrendDto>();
    }

    // Top Customer DTO
    public class TopCustomerDto
    {
        public long CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int TotalOrders { get; set; }
        public decimal LifetimeValue { get; set; }
        public DateTime LastOrderDate { get; set; }
    }

    // Customer Trend DTO
    public class CustomerTrendDto
    {
        public string Month { get; set; }
        public int NewCustomers { get; set; }
        public int ReturningCustomers { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    // Paginated Customers Response DTO
    public class PaginatedCustomersDto
    {
        public List<CustomerListDto> Customers { get; set; } = new List<CustomerListDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }

    // Customer Order History DTO
    public class CustomerOrderHistoryDto
    {
        public long CustomerId { get; set; }
        public string CustomerName { get; set; }
        public List<CustomerOrderDto> Orders { get; set; } = new List<CustomerOrderDto>();
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
    }
}
