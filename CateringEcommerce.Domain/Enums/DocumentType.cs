
using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Enums
{
    public enum DocumentType
    {
        [Display(Name = "Logo")]
        Logo = 0,
        [Display(Name = "Food")]
        Food = 1,
        [Display(Name = "Kitchen")]
        Kitchen = 2,
        [Display(Name = "EventSetup")]
        EventSetup = 3,
        [Display(Name = "Staff")]
        Staff = 4,
        [Display(Name = "FoodMaking")]
        Making = 5,
        [Display(Name = "Menu")]
        Menu = 6,
        [Display(Name = "Promo")]
        Promo = 7,
        [Display(Name = "Banner")]
        Banner = 8,
        [Display(Name = "Packaging")]
        Packaging = 9,
        [Display(Name = "ChefProfile")]
        ChefProfile = 10,
        [Display(Name = "Recipe")]
        Recipe = 11,
        [Display(Name = "ClientReview")]
        ClientReview = 12,
        [Display(Name = "QuoteTemplate")]
        QuoteTemplate = 13,
        [Display(Name = "ServiceCatalog")]
        ServiceCatalog = 14,
        [Display(Name = "Instruction")]
        Instruction = 15,
        [Display(Name = "Brand")]
        Brand = 16,
        [Display(Name = "Portfolio")]
        Portfolio = 17
    }

    public enum CertificateType
    {
        [Display(Name = "FSSAI")]
        FSSAI = 1,
        [Display(Name = "GST")]
        GST = 2,
        [Display(Name = "PAN")]
        PAN = 3,
        [Display(Name = "Other")]
        Other = 4
    }
}
