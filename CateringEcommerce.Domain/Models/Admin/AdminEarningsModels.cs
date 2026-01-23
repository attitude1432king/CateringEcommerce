namespace CateringEcommerce.Domain.Models.Admin
{
    #region Earnings Management Models

    public class AdminEarningsSummary
    {
        public decimal TotalPlatformEarnings { get; set; }
        public decimal TotalOrderValue { get; set; }
        public decimal TotalCommission { get; set; }
        public decimal AverageCommissionRate { get; set; }
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public decimal ThisMonthEarnings { get; set; }
        public decimal LastMonthEarnings { get; set; }
        public decimal GrowthPercentage { get; set; }
    }

    public class AdminEarningsByDateRequest
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string GroupBy { get; set; } = "Day"; // Day, Week, Month, Year
    }

    public class AdminEarningsByDateItem
    {
        public string Period { get; set; } = string.Empty;
        public decimal TotalOrderValue { get; set; }
        public decimal PlatformCommission { get; set; }
        public int OrderCount { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }

    public class AdminEarningsByCateringRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SortBy { get; set; } = "TotalEarnings";
        public string? SortOrder { get; set; } = "DESC";
    }

    public class AdminEarningsByCateringItem
    {
        public long CateringId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public decimal TotalOrderValue { get; set; }
        public decimal PlatformCommission { get; set; }
        public decimal CommissionRate { get; set; }
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
    }

    public class AdminEarningsByCateringResponse
    {
        public List<AdminEarningsByCateringItem> Caterings { get; set; } = new();
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public decimal GrandTotalCommission { get; set; }
    }

    public class AdminMonthlyReportItem
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal TotalOrderValue { get; set; }
        public decimal PlatformCommission { get; set; }
        public int OrderCount { get; set; }
        public int NewCaterings { get; set; }
        public int NewUsers { get; set; }
    }

    #endregion
}
