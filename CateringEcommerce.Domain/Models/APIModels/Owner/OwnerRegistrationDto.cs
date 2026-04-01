using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Models.APIModels.Owner
{
    public class OwnerRegistrationDto
    {
        // Step 1: Business & Account
        [Required]
        public string CateringName { get; set; }
        [Required]
        public string OwnerName { get; set; }
        // CateringLogo received as [FromForm] IFormFile in the controller
        [Required]
        public string Mobile { get; set; }
        [Required]
        public bool IsPhoneVerified { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsCateringNumberVerified { get; set; }
        public string CateringNumber { get; set; }
        public string? StdNumber { get; set; }
        public string? SupportContact { get; set; }
        public string? AlternateEmail { get; set; }
        public string? WhatsappNumber { get; set; }
        public bool CateringNumberSameAsMobile { get; set; } // If true, use the same number for Owner and Catering contact
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        // Step 2: Address
        [Required]
        public string? ShopNo { get; set; }
        [Required]
        public string? Floor { get; set; }
        [Required]
        public string? Landmark { get; set; }
        [Required]
        public string? Pincode { get; set; }
        [Required]
        public int? StateID { get; set; }
        [Required]
        public int? CityID { get; set; }
        public string? MapUrl { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }

        // Step 3: Services
        [Required]
        public string? CuisineIds { get; set; }
        [Required]
        public string? ServiceTypeIds { get; set; }
        [Required]
        public string? EventTypeIds { get; set; }
        [Required]
        public string? FoodTypeIds { get; set; }
        [Required]
        public decimal? MinOrderValue { get; set; }

        // Step 4: Legal & Payment (many are optional)
        public string? FssaiNumber { get; set; }
        public string? FssaiExpiry { get; set; }
        // FssaiCertificate, GstCertificate, PanCard, ChequeCopy, Signature — received as [FromForm] IFormFile params in the controller
        public string? GstNumber { get; set; }
        public bool IsGstApplicable { get; set; }
        public string? PanHolderName { get; set; }
        public string? PanNumber { get; set; }
        [Required]
        public string? BankAccountName { get; set; }
        [Required]
        public string? BankAccountNumber { get; set; }
        [Required]
        public string? IfscCode { get; set; }
        public string? UpiId { get; set; }

        // Step 5: Agreement & Signature
        [Required]
        public bool AgreementAccepted { get; set; }
    }
}
