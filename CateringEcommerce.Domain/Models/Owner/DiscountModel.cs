using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Models.Owner
{
    public class DiscountModel
    {
        public long? ID { get; set; }
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }
        [Required]
        public int Type { get; set; } // individual, package, entires
        [Required]
        public int Mode { get; set; } // percentage, flat
        [Required]
        public decimal Value { get; set; }
        public string? Code { get; set; }
        public decimal? MaxDiscount { get; set; }
        public decimal? MinOrderValue { get; set; }
        public List<long> SelectedItems { get; set; } = new List<long>(); // IDs of food items or packages
        [Required]
        public DateOnly? StartDate { get; set; }
        [Required]
        public DateOnly? EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public bool AutoDisable { get; set; } = true;
        public int? MaxUsesPerOrder { get; set; }
        public int? MaxUsesPerUser { get; set; }
        public bool IsStackable { get; set; } = false;

    }

    public class DiscountDto
    {
        public long? ID { get; set; }
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }
        [Required]
        public int Type { get; set; } // individual, package, entires
        [Required]
        public int Mode { get; set; } // percentage, flat
        [Required]
        public decimal Value { get; set; }
        public decimal? MaxDiscount { get; set; }
        public decimal? MinOrderValue { get; set; }
        public List<long> SelectedItems { get; set; } = new List<long>(); // IDs of food items or packages
        [Required]
        public DateOnly? StartDate { get; set; }
        [Required]
        public DateOnly? EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public bool AutoDisable { get; set; } = true;
        public int? MaxUsesPerOrder { get; set; }
        public int? MaxUsesPerUser { get; set; }
        public bool IsStackable { get; set; } = false;
        public bool IsChangeDiscountCode { get; set; } = false;
    }

    public class DiscountFilter
    {
        public string? Name { get; set; }
        public int Type { get; set; }
        public string? Status { get; set; }
    }
}
