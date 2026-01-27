using CateringEcommerce.Domain.Models.Admin;

namespace CateringEcommerce.Domain.Interfaces.Admin
{
    public interface IAdminPartnerRequestRepository
    {
        // List & Filter
        AdminPartnerRequestListResponse GetAllPartnerRequests(AdminPartnerRequestListRequest request);

        // Detail
        AdminPartnerRequestDetail? GetPartnerRequestById(long ownerId);

        // Actions
        PartnerRequestActionResponse ApprovePartnerRequest(PartnerRequestActionRequest request, long adminId);
        PartnerRequestActionResponse RejectPartnerRequest(PartnerRequestActionRequest request, long adminId);
        PartnerRequestActionResponse RequestAdditionalInfo(PartnerRequestActionRequest request, long adminId);

        // Status Update
        bool UpdatePartnerRequestStatus(long ownerId, string newStatus, long adminId, string? remarks = null);

        // Notes
        bool UpdateInternalNotes(long ownerId, string notes, long adminId);
        bool UpdatePriority(long ownerId, string priority, long adminId);

        // Action Log
        List<PartnerActionLog> GetActionLog(long ownerId);
        bool LogAction(long ownerId, long adminId, string actionType, string? oldStatus, string? newStatus, string? remarks, string? ipAddress);

        // Communication
        PartnerCommunicationResponse SendCommunication(PartnerCommunicationRequest request, long adminId);
        List<PartnerCommunicationHistory> GetCommunicationHistory(long ownerId);
    }
}
