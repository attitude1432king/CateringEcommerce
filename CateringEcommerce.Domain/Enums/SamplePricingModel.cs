using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Enums
{
    /// <summary>
    /// Pricing model for sample orders
    /// </summary>
    public enum SamplePricingModel
    {
        [Display(Name = "Per Item")]
        PER_ITEM = 1,

        [Display(Name = "Fixed Fee")]
        FIXED_FEE = 2
    }
}
