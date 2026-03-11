using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Enums.Admin
{
    public enum AdminEnums
    {

    }

    public enum ApprovalStatus
    {
        [Display(Name = "Pending")]
        Pending = 1,
        [Display(Name = "Approved")]
        Approved = 2,
        [Display(Name = "Rejected")]
        Rejected = 3,
        [Display(Name = "Under Review")]
        UnderReview = 4,
        [Display(Name = "More Info Requested")]
        Info_Requested = 5
    }

    public enum SupervisorApprovalStatus
    {
        [Display(Name = "Pending")]
        Pending = 0,
        [Display(Name = "Approved")]
        Approved = 1,
        [Display(Name = "Rejected")]
        Rejected = 2,
        [Display(Name = "Under Review")]
        UnderReview = 3,
        [Display(Name = "Info Requested")]
        InfoRequested = 4
    }

    public enum PriorityStatus
    {
        [Display(Name = "Low")]
        Low = 0,
        [Display(Name = "Normal")]
        Normal = 1,
        [Display(Name = "High")]
        High = 2,
        [Display(Name = "Urgent")]
        Urgent = 3
    }
}
