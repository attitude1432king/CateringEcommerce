using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Models.APIModels
{
    public class OwnerRegistrationDto
    {
        // Step 1: Business & Account
        [Required]
        public string CateringName { get; set; }
        [Required]
        public string OwnerName { get; set; }
        public string? CateringLogo { get; set; } // Will be a Base64 string
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
        public string ShopNo { get; set; }
        [Required]
        public string? Floor { get; set; }
        [Required]
        public string? Landmark { get; set; }
        [Required]
        public string? Pincode { get; set; }
        [Required]
        public string? State { get; set; }
        [Required]
        public string? City { get; set; }
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

        public List<CateringMediaDto> CateringMedia { get; set; }
        // Step 4: Legal & Payment (many are optional)
        public string? FssaiNumber { get; set; }
        public string? FssaiExpiry { get; set; } // Optional, if FSSAI is applicable
        public FileUploadDto? FssaiCertificate { get; set; } // Base64 PDF
        public string? GstNumber { get; set; }
        public bool IsGstApplicable { get; set; }
        public FileUploadDto? GstCertificate { get; set; } // Base64 PDF
        public string? PanHolderName { get; set; }
        public string? PanNumber { get; set; }
        public FileUploadDto? PanCard { get; set; } // Base64 Image/PDF
        [Required]
        public string BankAccountName { get; set; }
        [Required]
        public string BankAccountNumber { get; set; }
        [Required]
        public string IfscCode { get; set; }
        public string? ChequePath { get; set; } // Base64 Image of Cheque
        public string? UpiId { get; set; } // Optional, for UPI payments
    }

    // Changed from private to internal to fix CS1527
    public class CateringMediaDto
    {
        public string Base64Data { get; set; }
        public string Type { get; set; } // e.g., "image/jpeg" or "video/mp4"
        public string FileName { get; set; }
    }

    public class FileUploadDto
    {
        public string Base64 { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
