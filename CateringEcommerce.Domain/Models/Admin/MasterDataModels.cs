using System;
using System.Collections.Generic;

namespace CateringEcommerce.Domain.Models.Admin
{
    // Base class for all master data items
    public class MasterDataItemBase
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? ModifiedByName { get; set; }
        public int UsageCount { get; set; }
    }

    // City master data item
    public class CityMasterItem : MasterDataItemBase
    {
        public int StateId { get; set; }
        public string StateName { get; set; } = string.Empty;
    }

    // Food category master data item
    public class FoodCategoryMasterItem : MasterDataItemBase
    {
        public bool IsGlobal { get; set; }
    }

    // Catering type master data item (covers Food Type, Cuisine Type, Event Type, Service Type)
    public class CateringTypeMasterItem : MasterDataItemBase
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }

    // Guest category master data item
    public class GuestCategoryMasterItem : MasterDataItemBase { }

    // Theme master data item
    public class ThemeMasterItem : MasterDataItemBase { }

    // ===== Request/Response Models =====

    // Generic list request for master data
    public class MasterDataListRequest
    {
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public int? CategoryId { get; set; }
        public int? StateId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string SortBy { get; set; } = "DisplayOrder";
        public string SortOrder { get; set; } = "ASC";
    }

    // Generic paginated response for master data
    public class MasterDataListResponse<T> where T : MasterDataItemBase
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    // Create master data request
    public class CreateMasterDataRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        public int? CategoryId { get; set; }
        public int? StateId { get; set; }
        public bool? IsGlobal { get; set; }
    }

    // Update master data request
    public class UpdateMasterDataRequest
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool? IsGlobal { get; set; }
    }

    // Update status request
    public class UpdateStatusRequest
    {
        public long Id { get; set; }
        public bool IsActive { get; set; }
    }

    // Update display order request
    public class UpdateDisplayOrderRequest
    {
        public List<DisplayOrderItem> Items { get; set; } = new();
    }

    // Display order item
    public class DisplayOrderItem
    {
        public long Id { get; set; }
        public int DisplayOrder { get; set; }
    }

    // Usage check response
    public class UsageCheckResponse
    {
        public bool CanDeactivate { get; set; }
        public int UsageCount { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> UsageDetails { get; set; } = new();
    }

    // State dropdown item
    public class StateDropdownItem
    {
        public int StateId { get; set; }
        public string StateName { get; set; } = string.Empty;
    }
}
