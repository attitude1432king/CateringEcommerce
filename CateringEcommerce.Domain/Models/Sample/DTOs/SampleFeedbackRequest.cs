using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Models.Sample.DTOs
{
    /// <summary>
    /// Request DTO for submitting customer feedback on sample order
    /// </summary>
    public class SampleFeedbackRequest
    {
        [Required(ErrorMessage = "Taste rating is required")]
        [Range(1, 5, ErrorMessage = "Taste rating must be between 1 and 5")]
        public int TasteRating { get; set; }

        [Required(ErrorMessage = "Hygiene rating is required")]
        [Range(1, 5, ErrorMessage = "Hygiene rating must be between 1 and 5")]
        public int HygieneRating { get; set; }

        [Required(ErrorMessage = "Overall rating is required")]
        [Range(1, 5, ErrorMessage = "Overall rating must be between 1 and 5")]
        public int OverallRating { get; set; }

        [MaxLength(1000, ErrorMessage = "Feedback cannot exceed 1000 characters")]
        public string? ClientFeedback { get; set; }

        public bool InterestedInFullOrder { get; set; } = false;
    }
}
