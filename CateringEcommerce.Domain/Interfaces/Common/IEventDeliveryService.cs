using CateringEcommerce.Domain.Models.Delivery;

namespace CateringEcommerce.Domain.Interfaces.Common
{
    /// <summary>
    /// Service interface for Event Catering Delivery (Status-based, NO GPS)
    /// </summary>
    public interface IEventDeliveryService
    {
        /// <summary>
        /// Initialize event delivery for an order
        /// </summary>
        Task<EventDeliveryDto> InitEventDeliveryAsync(InitEventDeliveryRequest request);

        /// <summary>
        /// Get event delivery details by order ID
        /// </summary>
        Task<EventDeliveryDto?> GetEventDeliveryByOrderIdAsync(long orderId);

        /// <summary>
        /// Update event delivery status with validation
        /// </summary>
        Task<EventDeliveryDto> UpdateEventDeliveryStatusAsync(UpdateEventDeliveryStatusRequest request);

        /// <summary>
        /// Get delivery timeline with status history
        /// </summary>
        Task<DeliveryTimelineResponse> GetDeliveryTimelineAsync(long orderId);

        /// <summary>
        /// Validate if status transition is allowed
        /// </summary>
        bool IsValidStatusTransition(EventDeliveryStatus currentStatus, EventDeliveryStatus newStatus);

        /// <summary>
        /// Get all active deliveries for partner (owner)
        /// </summary>
        Task<List<EventDeliveryDto>> GetPartnerActiveDeliveriesAsync(long ownerId);

        /// <summary>
        /// Admin: Get all deliveries for monitoring
        /// </summary>
        Task<List<AdminDeliveryMonitorDto>> GetAdminDeliveryMonitorAsync();

        /// <summary>
        /// Admin: Override delivery status
        /// </summary>
        Task<EventDeliveryDto> AdminOverrideStatusAsync(long eventDeliveryId, EventDeliveryStatus newStatus, long adminUserId, string notes);
    }
}
