using CateringEcommerce.Domain.Models.Order;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CateringEcommerce.Domain.Interfaces.Order
{
    public interface IOrderModificationRepository
    {
        /// <summary>
        /// Request guest count change
        /// </summary>
        Task<ModificationRequestResponse> RequestGuestCountChangeAsync(GuestCountChangeRequestDto request);

        /// <summary>
        /// Request menu change
        /// </summary>
        Task<ModificationRequestResponse> RequestMenuChangeAsync(MenuChangeRequestDto request);

        /// <summary>
        /// Get modification by ID
        /// </summary>
        Task<OrderModificationModel> GetModificationAsync(long modificationId);

        /// <summary>
        /// Get all modifications for an order
        /// </summary>
        Task<List<OrderModificationModel>> GetOrderModificationsAsync(long orderId);

        /// <summary>
        /// Get all pending modifications (Admin view)
        /// </summary>
        Task<List<OrderModificationModel>> GetPendingModificationsAsync();

        /// <summary>
        /// Get pending modifications for partner approval
        /// </summary>
        Task<List<OrderModificationModel>> GetPendingModificationsForPartnerAsync(long partnerId);

        /// <summary>
        /// Approve modification (Partner/Admin)
        /// </summary>
        Task<bool> ApproveModificationAsync(long modificationId, long approvedBy, string approvedByType);

        /// <summary>
        /// Reject modification (Partner/Admin)
        /// </summary>
        Task<bool> RejectModificationAsync(long modificationId, long rejectedBy, string rejectionReason);

        /// <summary>
        /// Mark modification payment as collected
        /// </summary>
        Task<bool> MarkModificationPaidAsync(long modificationId, long paymentTransactionId);
    }
}
