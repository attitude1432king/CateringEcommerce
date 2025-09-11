namespace CateringEcommerce.Domain.Models.APIModels.Owner
{
    public class UpdateOwnerProfileDto
    {
        public BusinessSettingsDto OwnerBusiness { get; set; }
        public AddressSettingsDto CateringAddress { get; set; }
        public ServicesSettingsDto CateringServices { get; set; }
        public LegalPaymentSettingsDto OwnerLegalDocument { get; set; }

        // Represents an existing media item (for kitchen photos/videos)
        public class ExistingMediaDto
        {
            public string? Id { get; set; }
            public string? Type { get; set; }
            public string? Path { get; set; }
        }

        public class BusinessSettingsDto
        {
            public string? CateringName { get; set; }
            public string? OwnerName { get; set; }
            public string? CateringNumber { get; set; }
            public string? StdCode { get; set; }
            public string? WhatsAppNumber { get; set; }
            public string? SupportEmail { get; set; }
            public FileUploadDto? NewLogoFile { get; set; } // For uploading a new logo
        }

        public class AddressSettingsDto
        {
            public string? ShopNo { get; set; }
            public string? Floor { get; set; }
            public string? Street { get; set; }
            public string? Pincode { get; set; }
            public string? State { get; set; }
            public string? City { get; set; }
            public string? Latitude { get; set; }
            public string? Longitude { get; set; }
        }

        public class ServicesSettingsDto
        {
            public int DeliveryRediusKm { get; set; }
            public List<int>? ServingSlots { get; set; }
            public int MinOrderValue { get; set; }
            public List<int>? CuisineTypeIds { get; set; }
            public List<int>? FoodTypeIds { get; set; }
            public List<int>? ServiceTypeIds { get; set; }
            public List<int>? EventTypeIds { get; set; }
            public List<FileUploadDto>? NewKitchenMediaFiles { get; set; }
        }

        public class LegalPaymentSettingsDto
        {
            public string? FssaiNumber { get; set; }
            public string? FssaiExpiryDate { get; set; }
            public bool IsGstApplicable { get; set; }
            public string? GstNumber { get; set; }
            public string? PanHolderName { get; set; }
            public string? PanNumber { get; set; }
            public string? AccountHolderName { get; set; }
            public string? BankAccountNumber { get; set; }
            public string? IfscCode { get; set; }
            public string? UpiId { get; set; }
        }
    }
}
