using CateringEcommerce.Domain.Models.Order;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CateringEcommerce.Domain.Interfaces.Order
{
    public interface IComplaintRepository
    {
        /// <summary>
        /// File a customer complaint
        /// </summary>
        Task<FileComplaintResponse> FileComplaintAsync(FileComplaintDto request);

        /// <summary>
        /// Calculate refund for complaint
        /// </summary>
        Task<ComplaintRefundCalculation> CalculateComplaintRefundAsync(long complaintId);

        /// <summary>
        /// Get complaint by ID
        /// </summary>
        Task<CustomerComplaintModel> GetComplaintAsync(long complaintId);

        /// <summary>
        /// Get all complaints for an order
        /// </summary>
        Task<List<CustomerComplaintModel>> GetComplaintsByOrderAsync(long orderId);

        /// <summary>
        /// Get all complaints by user
        /// </summary>
        Task<List<CustomerComplaintModel>> GetComplaintsByUserAsync(long userId);

        /// <summary>
        /// Get pending complaints (Admin)
        /// </summary>
        Task<List<CustomerComplaintModel>> GetPendingComplaintsAsync();

        /// <summary>
        /// Resolve complaint (Admin)
        /// </summary>
        Task<bool> ResolveComplaintAsync(ResolveComplaintDto request);

        /// <summary>
        /// Add partner response to complaint
        /// </summary>
        Task<bool> AddPartnerResponseAsync(long complaintId, long partnerId, string response, bool admitsFault, bool offeredReplacement);

        /// <summary>
        /// Escalate complaint
        /// </summary>
        Task<bool> EscalateComplaintAsync(long complaintId);
    }
}
