using CateringEcommerce.Domain.Models.Owner;

namespace CateringEcommerce.Domain.Interfaces.Owner
{
    /// <summary>
    /// Order modification service for owner operations
    /// </summary>
    public interface IOrderModificationService
    {
        /// <summary>
        /// Create a new modification request
        /// </summary>
        Task<OrderModificationDto> CreateModificationAsync(CreateOrderModificationDto modificationData);

        /// <summary>
        /// Get all modifications for an order with summary
        /// </summary>
        Task<OrderModificationsSummaryDto> GetOrderModificationsAsync(long orderId);

        /// <summary>
        /// Approve a modification request (User approves Owner's modification)
        /// </summary>
        Task<OrderModificationDto> ApproveModificationAsync(ApproveOrderModificationDto approvalData);

        /// <summary>
        /// Reject a modification request (User rejects Owner's modification)
        /// </summary>
        Task<OrderModificationDto> RejectModificationAsync(RejectOrderModificationDto rejectionData);
    }
}
