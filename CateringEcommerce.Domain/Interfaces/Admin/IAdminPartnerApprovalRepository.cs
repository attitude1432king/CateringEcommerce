using CateringEcommerce.Domain.Enums.Admin;
using CateringEcommerce.Domain.Models.Admin;

namespace CateringEcommerce.Domain.Interfaces.Admin
{
    /// <summary>
    /// Repository interface for Admin Partner Request Approval & Rejection Flow
    /// Handles ONLY registration-time data review and approval workflow
    /// </summary>
    public interface IAdminPartnerApprovalRepository
    {
        /// <summary>
        /// Gets filtered list of partner requests with pagination
        /// </summary>
        PartnerRequestListResponse GetPendingPartnerRequests(PartnerRequestFilterRequest filter);

        /// <summary>
        /// Gets complete detail of a specific partner request for admin review
        /// </summary>
        PartnerRequestDetailResponse? GetPartnerRequestDetail(long ownerId);

        /// <summary>
        /// Approves a partner request (validates: must be PENDING or UNDER_REVIEW)
        /// </summary>
        ApprovalActionResult ApprovePartnerRequest(long ownerId, long adminId, string? remarks);

        /// <summary>
        /// Rejects a partner request (rejection reason is mandatory)
        /// </summary>
        ApprovalActionResult RejectPartnerRequest(long ownerId, long adminId, string rejectionReason);

        /// <summary>
        /// Updates priority for a partner request using PriorityStatus enum
        /// </summary>
        bool UpdatePriority(long ownerId, PriorityStatus priority, long adminId);
    }
}
