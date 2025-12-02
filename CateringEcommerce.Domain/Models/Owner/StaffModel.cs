using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Models.APIModels.Owner;
using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Models.Owner
{
    public class StaffModel
    {
        public long? ID { get; set; }
        [Required]
        public string? Name { get; set; }
        [Required]
        public string? Contact { get; set; }
        [Required]
        public string? Gender { get; set; }
        [Required]
        public string? Role { get; set; }
        public string? OtherRole { get; set; }
        public int? CategoryId { get; set; }
        public string? Expertise { get; set; }
        public int? Experience { get; set; }
        public string? salaryType { get; set; }
        public decimal SalaryAmount { get; set; }
        public StaffMediaModel[] Photo { get; set; } = Array.Empty<StaffMediaModel>();
        public StaffMediaModel[] IdProof { get; set; } = Array.Empty<StaffMediaModel>();
        public StaffMediaModel[] Resume { get; set; } = Array.Empty<StaffMediaModel>();
        public bool Availability { get; set; }

    }

    public class StaffDto
    {
        public long? ID { get; set; }
        [Required]
        public string? Name { get; set; }
        [Required]
        public string? Contact { get; set; }
        [Required]
        public string? Gender { get; set; }
        [Required]
        public string? Role { get; set; }
        public string? OtherRole { get; set; }
        public int? CategoryId { get; set; }
        public int? Experience { get; set; }
        public string? salaryType { get; set; }
        public decimal SalaryAmount { get; set; }
        public FileUploadDto? Profile { get; set; }
        public FileUploadDto? IdentityDocument { get; set; }
        public FileUploadDto? ResumeDocument { get; set; }   
        public bool Availability { get; set; }
        // This list will contain the relative paths of any files
        // the user explicitly removed.
        public List<string>? FilesToDelete { get; set; }
    }

    public class StaffMediaModel
    {
        public string? Type { get; set; }
        public string? Path { get; set; }
    }

    public class StaffFilter
    {
        public string? Name { get; set; }
        public string? Role  { get; set; }
        public string? Status { get; set; }
    }
}
