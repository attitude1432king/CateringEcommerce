using CateringEcommerce.Domain.Models.APIModels.Owner;
using CateringEcommerce.Domain.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Models.Owner
{
    public class DecorationsModel
    {
        public long? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int ThemeId { get; set; }
        public string? ThemeName { get; set; }  // used as "theme" in frontend
        public decimal? Price { get; set; }
        public bool Status { get; set; }
        public List<LinkedPackageDto>? LinkedPackages { get; set; }  // full object list for frontend
        public List<MediaFileModel>? Media { get; set; }
    }

    public class DecorationsDto
    {
        public long? Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public int ThemeId { get; set; }

        public decimal? Price { get; set; }
        public Int32[]? LinkedPackageIds { get; set; }  // Package PKID

        public bool Status { get; set; }
        public List<FileUploadDto>? DecorationsMediaFiles { get; set; }
        public List<string>? ExistingDecorationsMediaPaths { get; set; }
    }

    public class DecorationThemeModel
    {
        public int ThemeId { get; set; }
        public string? ThemeName { get; set; }
    }

    public class LinkedPackageDto
    {
        public long? Id { get; set; }
        public string? Name { get; set; }
    }

    public class DecorationFilter
    {
        public string Name { get; set; }
        public List<int> ThemeIds { get; set; }
        public string Status { get; set; }
        public List<int> PackageIds { get; set; }
    }

}
