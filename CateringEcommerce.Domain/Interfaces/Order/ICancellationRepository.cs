using CateringEcommerce.Domain.Models.Order;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CateringEcommerce.Domain.Interfaces.Order
{
    public interface ICancellationRepository
    {
        /// <summary>
        /// Calculate cancellation refund based on policy
        /// </summary>
        Task<CancellationPolicyResponse> CalculateCancellationRefundAsync(long orderId);

        /// <summary>
        /// Process cancellation request
        /// </summary>
        Task<CancellationRequestModel> ProcessCancellationRequestAsync(CreateCancellationRequestDto request);

        /// <summary>
        /// Get cancellation request by ID
        /// </summary>
        Task<CancellationRequestModel> GetCancellationRequestAsync(long cancellationId);

        /// <summary>
        /// Get cancellation request by order ID
        /// </summary>
        Task<CancellationRequestModel> GetCancellationRequestByOrderAsync(long orderId);

        /// <summary>
        /// Get all cancellation requests for a user
        /// </summary>
        Task<List<CancellationRequestModel>> GetUserCancellationRequestsAsync(long userId);

        /// <summary>
        /// Approve cancellation request (Admin)
        /// </summary>
        Task<bool> ApproveCancellationRequestAsync(long cancellationId, long adminId, string adminNotes);

        /// <summary>
        /// Reject cancellation request (Admin)
        /// </summary>
        Task<bool> RejectCancellationRequestAsync(long cancellationId, long adminId, string rejectionReason);

        /// <summary>
        /// Process refund for approved cancellation
        /// </summary>
        Task<bool> ProcessRefundAsync(long cancellationId, string refundTransactionId, string refundMethod);

        /// <summary>
        /// Get pending cancellation requests (Admin)
        /// </summary>
        Task<List<CancellationRequestModel>> GetPendingCancellationRequestsAsync();
    }
}
