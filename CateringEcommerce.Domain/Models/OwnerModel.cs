namespace CateringEcommerce.Domain.Models
{
    public class OwnerModel
    {
        internal OwnerBusinessModel OwnerBusiness { get; set; }
        internal CateringAddressModel CateringAddress { get; set; }
        internal OwnerLegalModel CateringLegal { get; set; }
        internal OwnerServicesModel CateringServices { get; set; }
        internal OwnerBankDetailsModel OwnerBankDetails { get; set; }
    }

    internal class OwnerBusinessModel
    {
        public Int64 OwnerPkID { get; set; }
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
        public string? WhatsappNumber { get; set; }
        public bool IsOnline { get; set; }
    }

    internal class CateringAddressModel
    {
        public string? ShopNo { get; set; }
        public string? Street { get; set; }
        public string? Area { get; set; }
        public string? Pincode { get; set; }
        public int StateId { get; set; }
        public int CityId { get; set; }
        public Decimal Latitude { get; set; }
        public Decimal Longitude { get; set; }
        public string? MapUrl { get; set; }
    }

    internal class OwnerLegalModel
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

    internal class OwnerServicesModel
    {
        public int[]? Cuisines { get; set; }
        public int[]? ServiceTypes { get; set; }
        public int MinOrderValue { get; set; }
        public int[]? EventTypes { get; set; }
        public bool IsDeliveryAvailable { get; set; }
        public string[]? ServingSlots { get; set; } // JSON string of available time slots
        public int DeliveryRediusKm { get; set; }
    }

    internal class OwnerBankDetailsModel
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
}
