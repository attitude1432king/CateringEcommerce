using CateringEcommerce.Domain.Enums;

namespace CateringEcommerce.Domain.Models.Owner
{
    public class OwnerModel
    {
        public OwnerBusinessModel OwnerBusiness { get; set; }
        public CateringAddressModel CateringAddress { get; set; }
        public OwnerLegalModel OwnerLegalDocument { get; set; }
        public CateringServicesModel CateringServices { get; set; }
    }

    public class OwnerBusinessModel
    {
        public long PkID { get; set; }
        public string? CateringName { get; set; }
        public string? OwnerName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? CateringNumber { get; set; }
        public string? LogoPath { get; set; }
        public string? StdNumber { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsPhoneVerified { get; set; }
        public bool IsVerifiedBy_Admin { get; set; }
        public string? SupportContact { get; set; }
        public string? AlternateEmail { get; set; }
        public string? WhatsAppNumber { get; set; }
        public bool IsOnline { get; set; }
    }

    public class CateringAddressModel
    {
        public string? ShopNo { get; set; }
        public string? Street { get; set; }
        public string? Area { get; set; }
        public string? Pincode { get; set; }
        public string? State { get; set; }
        public string? City { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string? MapUrl { get; set; }
    }

    public class OwnerLegalModel : OwnerBankDetailsModel
    {
        public string? FssaiNumber { get; set; }
        public DateTime FssaiExpiryDate { get; set; }
        public string? FssaiCertificatePath { get; set; }
        public string? GstNumber { get; set; }
        public bool IsGstApplicable { get; set; }
        public string? GstCertificatePath { get; set; }
        public string? PanHolderName { get; set; }
        public string? PanNumber { get; set; }
        public string? PanCertificatePath { get; set; }

    }

    public class CateringServicesModel
    {
        public int[]? CuisineTypeIds { get; set; }
        public int[]? ServiceTypeIds { get; set; }
        public int MinOrderValue { get; set; }
        public int DailyBookingCapacity { get; set; }
        public int[]? EventTypeIds { get; set; }
        public int[]? FoodTypeIds { get; set; }
        public int[]? ServingSlots { get; set; } // JSON string of available time slots
        public int DeliveryRediusKm { get; set; }
        public List<MediaFileModel>? KitchenMedia { get; set; }
    }

    public class OwnerBankDetailsModel
    {
        public string? AccountHolderName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? IfscCode { get; set; }
        public string? ChequePath { get; set; }
        public string? UpiId { get; set; }
    }

    public class ServiceTypeDetails
    {
        public int TypeId { get; set; }
        public string ServiceName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }

    //public class MediaFileModel
    //{
    //    public long Id { get; set; }
    //    public string? FilePath { get; set; }
    //    public string? MediaType { get; set; }
    //    public string? FileName { get; set; }
    //    public DocumentType DocumentType { get; set; }
    //}
}
