using CateringEcommerce.Domain.Enums;

namespace CateringEcommerce.Domain.Models.Owner
{
    public class MediaFileModel
    {
        public long Id { get; set; }
        public long? ReferenceId { get; set; }
        public string? MediaType { get; set; } // e.g., ".jpg", ".mp4"
        public string? FileName { get; set; } // e.g., "image1"
        public string? FilePath { get; set; } // e.g., "/uploads/images/image1.jpg"
        public DocumentType DocumentType { get; set; } // e.g., 1 for image, 2 for video
        public DateTime? UploadedDate { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
