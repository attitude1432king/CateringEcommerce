using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Models.User;

/// <summary>
/// User-side catering models for browsing, searching, viewing, and booking services.
/// These models are READ-ONLY and BOOKING-FOCUSED, separate from admin/owner management models.
/// </summary>

#region Catering List & Search Models

/// <summary>
/// Represents a catering business in search/browse results.
/// Used when users are looking for caterers in their city.
/// </summary>
public class CateringBusinessListDto
{
    public long Id { get; set; }
    public string? CateringName { get; set; }
    public string? LogoUrl { get; set; }
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public decimal MinOrderValue { get; set; }
    public string? Status { get; set; }
    public int DeliveryRadiusKm { get; set; }
    public bool IsOnline { get; set; }
    public string? City { get; set; }
    public string? Area { get; set; }
    public double DistanceKm { get; set; } // Distance from user's location
    public List<string>? CuisineTypes { get; set; }
    public List<string>? ServiceTypes { get; set; }
}

/// <summary>
/// Detailed catering profile when user views a specific caterer.
/// </summary>
public class CateringDetailDto
{
    public long CateringId { get; set; }
    public string? CateringName { get; set; }
    public string? OwnerName { get; set; }
    public string? LogoUrl { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? CateringNumber { get; set; }
    public string? WhatsAppNumber { get; set; }
    public string? SupportEmail { get; set; }
    
    // Address Information
    public string? ShopNo { get; set; }
    public string? Street { get; set; }
    public string? Area { get; set; }
    public string? Pincode { get; set; }
    public string? State { get; set; }
    public string? City { get; set; }
    public string? Latitude { get; set; }
    public string? Longitude { get; set; }
    public string? MapUrl { get; set; }
    
    // Business Details
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public decimal MinOrderValue { get; set; }
    public int DeliveryRadiusKm { get; set; }
    public bool IsOnline { get; set; }
    public string? Description { get; set; }
    
    // Service Offerings
    public List<ServiceOfferingDto>? CuisineTypes { get; set; }
    public List<ServiceOfferingDto>? FoodTypes { get; set; }
    public List<ServiceOfferingDto>? ServiceTypes { get; set; }
    public List<ServiceOfferingDto>? EventTypes { get; set; }
    
    // Media
    public List<CateringMediaDto>? KitchenPhotos { get; set; }
    public List<CateringMediaDto>? KitchenVideos { get; set; }
    
    // Verification Status
    public bool IsVerifiedByAdmin { get; set; }
    public DateTime? VerificationDate { get; set; }
}

/// <summary>
/// Search filter model for browsing caterers.
/// </summary>
public class CateringSearchFilterDto
{
    public string? City { get; set; }
    public string? SearchKeyword { get; set; }
    public List<int>? CuisineTypeIds { get; set; }
    public List<int>? ServiceTypeIds { get; set; }
    public List<int>? EventTypeIds { get; set; }
    public decimal? MinOrderValueFrom { get; set; }
    public decimal? MinOrderValueTo { get; set; }
    public int? DeliveryRadiusKm { get; set; }
    public double? MinRating { get; set; }
    public bool? OnlineOnly { get; set; }
    public bool? VerifiedOnly { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

#endregion

#region Service Offering Models

/// <summary>
/// Represents a service offering (cuisine type, food type, service type, event type).
/// </summary>
public class ServiceOfferingDto
{
    public int TypeId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
}

#endregion

#region Menu & Pricing Models

/// <summary>
/// Represents a food item available from a caterer.
/// Users view this when browsing the menu.
/// </summary>
public class CateringFoodItemDto
{
    public long FoodItemId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int? CuisineTypeId { get; set; }
    public string? CuisineTypeName { get; set; }
    public decimal Price { get; set; }
    public bool IsVegetarian { get; set; }
    public bool IsSpicy { get; set; }
    public bool IsSampleTasted { get; set; }
    public List<string>? ImageUrls { get; set; }
    public bool IsAvailable { get; set; }
    public string? Allergens { get; set; }
}

/// <summary>
/// Represents a food category (e.g., Starters, Main Course, Desserts).
/// </summary>
public class FoodCategoryDisplayDto
{
    public int CategoryId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int ItemCount { get; set; }
}

/// <summary>
/// Represents a catering package/combo offered by the caterer.
/// </summary>
public class CateringPackageDto
{
    public long PackageId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal PricePerPerson { get; set; }
    public int MinGuests { get; set; }
    public int MaxGuests { get; set; }
    public List<PackageItemDetailDto>? Items { get; set; }
    public bool IsAvailable { get; set; }
    public string? IconUrl { get; set; }
}

/// <summary>
/// Details of items included in a package.
/// </summary>
public class PackageItemDetailDto
{
    public long PackageItemId { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int Quantity { get; set; }
    public string? Description { get; set; }
}

#endregion

#region Decoration Models

/// <summary>
/// Represents a decoration theme offered by a caterer
/// </summary>
public class DecorationDto
{
    public long DecorationId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int ThemeId { get; set; }
    public string? ThemeName { get; set; }
    public string? ThemeDescription { get; set; }
    public decimal Price { get; set; }
    public string? IncludedInPackageIds { get; set; } // CSV of package IDs
    public bool IsAvailable { get; set; }
    public string? ThumbnailUrl { get; set; }
}

#endregion

#region Booking & Quotation Models

/// <summary>
/// Request model for getting a quotation from a caterer.
/// </summary>
public class QuotationRequestDto
{
    [Required]
    public long CateringId { get; set; }
    
    [Required]
    public DateTime EventDate { get; set; }
    
    [Required]
    public int GuestCount { get; set; }
    
    public int? EventTypeId { get; set; }
    public int? ServiceTypeId { get; set; }
    public string? EventLocation { get; set; }
    public string? SpecialRequirements { get; set; }
    public List<long>? SelectedFoodItemIds { get; set; } // If user wants specific items
    public List<long>? SelectedPackageIds { get; set; }   // If user wants packages
}

/// <summary>
/// Quotation response from caterer.
/// Shows pricing breakdown for user's event.
/// </summary>
public class QuotationResponseDto
{
    public long QuotationId { get; set; }
    public long CateringId { get; set; }
    public string? CateringName { get; set; }
    public DateTime EventDate { get; set; }
    public int GuestCount { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DeliveryCharges { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Status { get; set; } // Pending, Approved, Rejected
    public string? Notes { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public List<QuotationItemDto>? Items { get; set; }
}

/// <summary>
/// Individual item in a quotation.
/// </summary>
public class QuotationItemDto
{
    public long ItemId { get; set; }
    public string? ItemName { get; set; }
    public string? ItemType { get; set; } // "FoodItem", "Package"
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

/// <summary>
/// Booking request model for confirming an event.
/// </summary>
public class BookingRequestDto
{
    [Required]
    public long QuotationId { get; set; }
    
    [Required]
    public DateTime EventDate { get; set; }
    
    [Required]
    public string? EventLocation { get; set; }
    
    [Required]
    public int GuestCount { get; set; }
    
    public string? EventType { get; set; }
    public string? ServiceType { get; set; }
    public string? SpecialRequirements { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
}

#endregion

#region Reviews & Ratings Models

/// <summary>
/// Represents a review/rating left by a user for a caterer.
/// </summary>
public class CateringReviewDto
{
    public long ReviewId { get; set; }
    public long CateringId { get; set; }
    public long UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserPhotoUrl { get; set; }
    public int Rating { get; set; } // 1-5 stars
    public string? Title { get; set; }
    public string? ReviewText { get; set; }
    public DateTime ReviewDate { get; set; }
    public bool WouldRecommend { get; set; }
    public List<string>? PhotoUrls { get; set; }
}

/// <summary>
/// Review submission model.
/// </summary>
public class SubmitReviewDto
{
    [Required]
    public long CateringId { get; set; }
    
    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }
    
    [MaxLength(100)]
    public string? Title { get; set; }
    
    [MaxLength(1000)]
    public string? ReviewText { get; set; }
    
    public bool WouldRecommend { get; set; }
    public List<string>? PhotoUrls { get; set; } // Base64 images
}

#endregion

#region Media Models

/// <summary>
/// Represents media (photos/videos) from a caterer's kitchen.
/// </summary>
public class CateringMediaDto
{
    public long MediaId { get; set; }
    public string? MediaUrl { get; set; }
    public string? MediaType { get; set; } // "Image", "Video"
    public string? Caption { get; set; }
    public int DisplayOrder { get; set; }
}

#endregion

#region Availability Models

/// <summary>
/// Represents time slots available for catering service.
/// </summary>
public class CateringAvailabilityDto
{
    public long CateringId { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public List<TimeSlotDto>? AvailableSlots { get; set; }
    public bool IsAvailableYear { get; set; }
}

/// <summary>
/// Individual time slot availability.
/// </summary>
public class TimeSlotDto
{
    public int SlotId { get; set; }
    public string? SlotName { get; set; } // e.g., "Breakfast 7-10 AM", "Lunch 12-3 PM"
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsAvailable { get; set; }
    public string? DayOfWeek { get; set; } // Optional: specific days
}

#endregion

#region Favorites & Wishlist Models

/// <summary>
/// User's favorite/bookmarked caterer.
/// </summary>
public class FavoriteCateringDto
{
    public long FavoriteId { get; set; }
    public long CateringId { get; set; }
    public string? CateringName { get; set; }
    public string? LogoUrl { get; set; }
    public double AverageRating { get; set; }
    public decimal MinOrderValue { get; set; }
    public bool IsOnline { get; set; }
    public DateTime AddedDate { get; set; }
}

#endregion

#region Search Results Models

/// <summary>
/// Paginated search results for caterers.
/// </summary>
public class CateringSearchResultDto
{
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public List<CateringBusinessListDto>? Results { get; set; }
}

#endregion

#region Homepage Models

/// <summary>
/// Homepage statistics displayed in the stats section
/// </summary>
public class HomePageStatsDto
{
    public int TotalEventsCatered { get; set; }
    public int TotalCateringPartners { get; set; }
    public int TotalHappyCustomers { get; set; }
    public decimal SatisfactionRate { get; set; }
}

/// <summary>
/// Featured caterer for homepage display
/// </summary>
public class FeaturedCatererDto
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? Cuisine { get; set; }
    public double Rating { get; set; }
    public int Reviews { get; set; }
    public string? Image { get; set; }
    public int MinOrder { get; set; }
    public List<string>? Specialties { get; set; }
    public bool Verified { get; set; }
    public bool Featured { get; set; }
}

/// <summary>
/// Testimonial for homepage display
/// </summary>
public class HomePageTestimonialDto
{
    public long Id { get; set; }
    public string? Text { get; set; }
    public string? Author { get; set; }
    public string? Role { get; set; }
    public int Rating { get; set; }
    public string? Location { get; set; }
    public string? Image { get; set; }
    public string? Event { get; set; }
}

/// <summary>
/// Service category for homepage display
/// </summary>
public class ServiceCategoryDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Offer { get; set; }
    public string? Image { get; set; }
    public string? Link { get; set; }
    public string? Gradient { get; set; }
    public string? BgGradient { get; set; }
}

#endregion