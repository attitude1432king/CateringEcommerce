using CateringEcommerce.Domain.Models.Admin;

namespace CateringEcommerce.Domain.Interfaces.Admin
{
    public interface IAdminSupervisorRepository
    {
        // Tab 1: Pending Supervisor Requests
        AdminSupervisorRegistrationListResponse GetRegistrationRequests(AdminSupervisorRegistrationListRequest request);
        bool UpdateSupervisorStatus(AdminSupervisorStatusUpdate request);

        // Tab 2: Approved Supervisors
        AdminActiveSupervisorListResponse GetActiveSupervisors(AdminActiveSupervisorListRequest request);
        bool BlockSupervisor(long supervisorId, long blockedBy, string? reason);
        bool UnblockSupervisor(long supervisorId, long unblockedBy);
        bool DeleteSupervisor(long supervisorId, long deletedBy);
        bool RestoreSupervisor(long supervisorId, long restoredBy);
        List<AdminSupervisorExportItem> GetSupervisorsForExport(AdminActiveSupervisorListRequest request);
    }
}
