using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Enums
{
    public enum Role
    {
        [Display(Name = "System Admin")]
        Admin = 1,
        [Display(Name = "User")]
        User = 2,
        [Display(Name = "Owner")]
        Owner = 3,
        [Display(Name = "Super Admin")]
        SuperAdmin = 4
    }
}
