using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Enums
{
    /// <summary>
    /// Reasons for sample order refunds
    /// </summary>
    public enum RefundReason
    {
        [Display(Name = "Partner Rejected")]
        PARTNER_REJECTED = 1,

        [Display(Name = "Delivery Failed")]
        DELIVERY_FAILED = 2,

        [Display(Name = "Customer Request")]
        CUSTOMER_REQUEST = 3,

        [Display(Name = "Quality Issue")]
        QUALITY_ISSUE = 4,

        [Display(Name = "System Error")]
        SYSTEM_ERROR = 5
    }
}
