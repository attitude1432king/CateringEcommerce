using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Enums
{
    /// <summary>
    /// Third-party delivery service providers for sample orders
    /// </summary>
    public enum DeliveryProvider
    {
        [Display(Name = "Dunzo")]
        DUNZO = 1,

        [Display(Name = "Porter")]
        PORTER = 2,

        [Display(Name = "Shadowfax")]
        SHADOWFAX = 3
    }
}
