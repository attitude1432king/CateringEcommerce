using System.Collections.Generic;

namespace CateringEcommerce.Domain.Models.Sample.DTOs
{
    /// <summary>
    /// DTO for checking sample availability for a catering
    /// </summary>
    public class SampleAvailabilityDto
    {
        public long CateringID { get; set; }
        public bool SampleAvailable { get; set; }
        public string? UnavailabilityReason { get; set; }
        public int AvailableItemsCount { get; set; }
        public decimal MinimumOrderAmount { get; set; }
        public decimal DeliveryCharge { get; set; }
        public int MaxItemsAllowed { get; set; }
        public int MinItemsRequired { get; set; }
        public bool RequiresPartnerApproval { get; set; }
        public List<SampleMenuItemDto> AvailableItems { get; set; } = new List<SampleMenuItemDto>();
    }

    /// <summary>
    /// DTO for menu items available for sampling
    /// </summary>
    public class SampleMenuItemDto
    {
        public long MenuItemID { get; set; }
        public string MenuItemName { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? CuisineType { get; set; }
        public bool IsVeg { get; set; }
        public decimal SamplePrice { get; set; }
        public int SampleQuantity { get; set; }
        public string SampleSize { get; set; } = string.Empty; // e.g., "250g"
        public bool IsAvailableForSample { get; set; }
        public long? PackageID { get; set; }
        public string? PackageName { get; set; }
    }

    /// <summary>
    /// DTO for user's sample order eligibility
    /// </summary>
    public class UserSampleEligibilityDto
    {
        public bool IsEligible { get; set; }
        public string? IneligibilityReason { get; set; }
        public int SamplesOrderedThisMonth { get; set; }
        public int MaxSamplesPerMonth { get; set; }
        public int HoursUntilNextSample { get; set; }
        public bool HasPendingOrder { get; set; }
        public long? PendingSampleOrderID { get; set; }
    }
}
