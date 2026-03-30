using CateringEcommerce.Domain.Models.APIModels.Owner;

namespace CateringEcommerce.Domain.Models.Owner
{
    public class BannerDto
    {
        public long? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImagePath { get; set; }
        public string? LinkUrl { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int ClickCount { get; set; }
        public int ViewCount { get; set; }
    }

    public class BannerFilter
    {
        public string? Title { get; set; }
        public bool? IsActive { get; set; }
    }
}
