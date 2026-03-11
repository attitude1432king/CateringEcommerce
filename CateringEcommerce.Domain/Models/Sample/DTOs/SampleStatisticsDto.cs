using System.Collections.Generic;

namespace CateringEcommerce.Domain.Models.Sample.DTOs
{
    /// <summary>
    /// Statistics DTO for partner dashboard
    /// </summary>
    public class PartnerSampleStatisticsDto
    {
        public long CateringID { get; set; }
        public int TotalSampleRequests { get; set; }
        public int AcceptedSamples { get; set; }
        public int RejectedSamples { get; set; }
        public int CompletedSamples { get; set; }
        public int PendingApproval { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageRating { get; set; }
        public int ConversionCount { get; set; }
        public decimal ConversionRate { get; set; }
        public decimal ConversionRevenue { get; set; }
        public List<PopularSampleItemDto> PopularItems { get; set; } = new List<PopularSampleItemDto>();
        public List<SampleRevenueByMonth> RevenueByMonth { get; set; } = new List<SampleRevenueByMonth>();
    }

    /// <summary>
    /// Popular sample item statistics
    /// </summary>
    public class PopularSampleItemDto
    {
        public long MenuItemID { get; set; }
        public string MenuItemName { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal AverageRating { get; set; }
        public int ConversionCount { get; set; }
    }

    /// <summary>
    /// Revenue statistics by month
    /// </summary>
    public class SampleRevenueByMonth
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int SampleCount { get; set; }
        public decimal Revenue { get; set; }
        public int ConversionCount { get; set; }
        public decimal ConversionRevenue { get; set; }
    }

    /// <summary>
    /// Admin statistics for sample tasting feature
    /// </summary>
    public class AdminSampleStatisticsDto
    {
        public int TotalSamplesOrdered { get; set; }
        public int ActiveSamples { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalConversions { get; set; }
        public decimal OverallConversionRate { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int TotalRefunds { get; set; }
        public decimal RefundAmount { get; set; }
        public List<TopPerformingCateringDto> TopCaterings { get; set; } = new List<TopPerformingCateringDto>();
        public List<DeliveryProviderStats> DeliveryStats { get; set; } = new List<DeliveryProviderStats>();
    }

    /// <summary>
    /// Top performing catering statistics
    /// </summary>
    public class TopPerformingCateringDto
    {
        public long CateringID { get; set; }
        public string CateringName { get; set; } = string.Empty;
        public int SampleCount { get; set; }
        public decimal Revenue { get; set; }
        public decimal AverageRating { get; set; }
        public decimal ConversionRate { get; set; }
    }

    /// <summary>
    /// Delivery provider statistics
    /// </summary>
    public class DeliveryProviderStats
    {
        public string ProviderName { get; set; } = string.Empty;
        public int TotalDeliveries { get; set; }
        public int SuccessfulDeliveries { get; set; }
        public int FailedDeliveries { get; set; }
        public decimal SuccessRate { get; set; }
        public decimal AverageDeliveryTime { get; set; } // in minutes
    }
}
