using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Enums
{
    public enum DiscountType
    {
        All = 0,
        [Display(Name = "Item")]
        Item = 1,
        [Display(Name = "Package")]
        Package = 2,
        [Display(Name = "Entire Catering")]
        EntireCatering = 3
    }

    public enum DiscountMode
    {
        [Display(Name = "Percentage")]
        Percentage = 1,
        [Display(Name = "Flat")]
        Flat = 2
    }

    public enum DiscountStatus
    {
        [Display(Name = "Active")]
        Active = 1,
        [Display(Name = "Expired")]
        Expired = 2,
        [Display(Name = "Disabled")]
        Disabled = 3
    }
}
