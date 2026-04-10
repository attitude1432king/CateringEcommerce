using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Models.User
{
    /// <summary>
    /// Request model for adding/updating cart
    /// </summary>
    public class AddToCartDto
    {
        [Required]
        public long CateringId { get; set; }

        public long? PackageId { get; set; }

        [Required]
        [Range(1, 100000)]
        public int GuestCount { get; set; }

        public DateTime? EventDate { get; set; }

        [MaxLength(100)]
        public string? EventType { get; set; }

        [MaxLength(500)]
        public string? EventLocation { get; set; }

        [MaxLength(2000)]
        public string? SpecialRequirements { get; set; }

        public decimal BaseAmount { get; set; }

        public decimal DecorationAmount { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal TotalAmount { get; set; }

        public long? DecorationId { get; set; }

        [MaxLength(200)]
        public string? DecorationName { get; set; }

        public decimal DecorationPrice { get; set; }

        public List<CartDecorationDto> StandaloneDecorations { get; set; } = new List<CartDecorationDto>();

        public List<CartAdditionalItemDto> AdditionalItems { get; set; } = new List<CartAdditionalItemDto>();
    }

    public class CartDecorationDto
    {
        [Required]
        public long DecorationId { get; set; }

        [MaxLength(200)]
        public string? Name { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
    }

    /// <summary>
    /// Additional food items in cart
    /// </summary>
    public class CartAdditionalItemDto
    {
        [Required]
        public long FoodId { get; set; }

        [Required]
        [Range(1, 1000)]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }
    }

    /// <summary>
    /// Cart response model
    /// </summary>
    public class CartResponseDto
    {
        public long CartId { get; set; }
        public long UserId { get; set; }
        public long CateringId { get; set; }
        public string CateringName { get; set; } = string.Empty;
        public string? CateringLogo { get; set; }
        public long? PackageId { get; set; }
        public string? PackageName { get; set; }
        public int GuestCount { get; set; }
        public DateTime? EventDate { get; set; }
        public string? EventType { get; set; }
        public string? EventLocation { get; set; }
        public string? SpecialRequirements { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal DecorationAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public long? DecorationId { get; set; }
        public string? DecorationName { get; set; }
        public decimal DecorationPrice { get; set; }
        public List<CartDecorationDto> StandaloneDecorations { get; set; } = new List<CartDecorationDto>();
        public List<CartAdditionalItemResponseDto> AdditionalItems { get; set; } = new List<CartAdditionalItemResponseDto>();
    }

    /// <summary>
    /// Additional item in cart response
    /// </summary>
    public class CartAdditionalItemResponseDto
    {
        public long CartItemId { get; set; }
        public long FoodId { get; set; }
        public string FoodName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice => Quantity * Price;
    }
}
