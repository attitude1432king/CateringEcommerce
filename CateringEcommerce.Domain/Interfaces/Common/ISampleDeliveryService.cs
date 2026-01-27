using CateringEcommerce.Domain.Models.Delivery;

namespace CateringEcommerce.Domain.Interfaces.Common
{
    /// <summary>
    /// Service interface for Sample Delivery (Third-party tracking)
    /// </summary>
    public interface ISampleDeliveryService
    {
        /// <summary>
        /// Create a new sample delivery request with third-party provider
        /// </summary>
        Task<SampleDeliveryDto> CreateSampleDeliveryAsync(CreateSampleDeliveryRequest request);

        /// <summary>
        /// Get sample delivery details by order ID
        /// </summary>
        Task<SampleDeliveryDto?> GetSampleDeliveryByOrderIdAsync(long orderId);

        /// <summary>
        /// Update sample delivery status (typically from third-party webhook)
        /// </summary>
        Task<bool> UpdateSampleDeliveryStatusAsync(UpdateSampleDeliveryStatusRequest request);

        /// <summary>
        /// Get tracking information for user
        /// </summary>
        Task<SampleDeliveryDto?> GetTrackingInfoAsync(long orderId);
    }
}
