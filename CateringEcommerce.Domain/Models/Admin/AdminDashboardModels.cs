namespace CateringEcommerce.Domain.Models.Admin
{
    #region Dashboard Models

    public class AdminDashboardMetrics
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int NewUsersToday { get; set; }
        public int NewUsersThisMonth { get; set; }

        public int TotalCaterings { get; set; }
        public int ActiveCaterings { get; set; }
        public int PendingApprovals { get; set; }
        public int NewCateringsToday { get; set; }
        public int NewCateringsThisMonth { get; set; }

        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int TodayOrders { get; set; }
        public int ThisMonthOrders { get; set; }

        public decimal TotalRevenue { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal ThisMonthRevenue { get; set; }
        public decimal TotalCommission { get; set; }
        public decimal AverageOrderValue { get; set; }

        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int ReviewsThisMonth { get; set; }

        public List<AdminTopCatering> TopCaterings { get; set; } = new();
        public List<AdminRecentOrder> RecentOrders { get; set; } = new();
        public List<AdminRevenueChart> RevenueChart { get; set; } = new();
    }

    public class AdminTopCatering
    {
        public long CateringId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public decimal TotalEarnings { get; set; }
        public int TotalOrders { get; set; }
        public decimal Rating { get; set; }
    }

    public class AdminRecentOrder
    {
        public long OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CateringName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime EventDate { get; set; }
    }

    public class AdminRevenueChart
    {
        public string Date { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal Commission { get; set; }
        public int OrderCount { get; set; }
    }

    #endregion
}
