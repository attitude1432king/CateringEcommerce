using System;
using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Models.Sample.DTOs
{
    /// <summary>
    /// Request DTO for partner (catering owner) to accept/reject sample request
    /// </summary>
    public class PartnerSampleActionRequest
    {
        [Required(ErrorMessage = "Action is required")]
        [MaxLength(20)]
        public string Action { get; set; } = string.Empty; // "ACCEPT" or "REJECT"

        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        public DateTime? EstimatedPreparationTime { get; set; }

        [MaxLength(500)]
        public string? PreparationNotes { get; set; }
    }

    /// <summary>
    /// Request DTO for partner to update sample preparation status
    /// </summary>
    public class UpdateSamplePreparationRequest
    {
        [Required(ErrorMessage = "Status is required")]
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty; // "SAMPLE_PREPARING", "READY_FOR_PICKUP"

        public DateTime? ReadyForPickupTime { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Request DTO for initiating delivery pickup
    /// </summary>
    public class InitiateDeliveryRequest
    {
        [Required(ErrorMessage = "Delivery provider is required")]
        [MaxLength(50)]
        public string DeliveryProvider { get; set; } = string.Empty;

        public DateTime? ScheduledPickupTime { get; set; }

        [MaxLength(500)]
        public string? SpecialInstructions { get; set; }
    }
}
