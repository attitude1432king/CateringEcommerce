using System;
using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Models.User
{
    // ===================================
    // CREATE ADDRESS DTO
    // ===================================
    public class CreateAddressDto
    {
        [Required]
        [MaxLength(50)]
        public string AddressLabel { get; set; } = string.Empty; // Home, Office, Other

        [Required]
        [MaxLength(500)]
        public string FullAddress { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Landmark { get; set; }

        [Required]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string State { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Pincode must be 6 digits")]
        [MaxLength(10)]
        public string Pincode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ContactPerson { get; set; } = string.Empty;

        [Required]
        [Phone]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Phone number must be a valid 10-digit Indian number")]
        [MaxLength(20)]
        public string ContactPhone { get; set; } = string.Empty;

        public bool IsDefault { get; set; } = false;
    }

    // ===================================
    // UPDATE ADDRESS DTO
    // ===================================
    public class UpdateAddressDto
    {
        [Required]
        public long AddressId { get; set; }

        [Required]
        [MaxLength(50)]
        public string AddressLabel { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string FullAddress { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Landmark { get; set; }

        [Required]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string State { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Pincode must be 6 digits")]
        [MaxLength(10)]
        public string Pincode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ContactPerson { get; set; } = string.Empty;

        [Required]
        [Phone]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Phone number must be a valid 10-digit Indian number")]
        [MaxLength(20)]
        public string ContactPhone { get; set; } = string.Empty;

        public bool IsDefault { get; set; } = false;
    }

    // ===================================
    // SAVED ADDRESS DTO (Response)
    // ===================================
    public class SavedAddressDto
    {
        public long AddressId { get; set; }
        public long UserId { get; set; }
        public string AddressLabel { get; set; } = string.Empty;
        public string FullAddress { get; set; } = string.Empty;
        public string? Landmark { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
    }
}
