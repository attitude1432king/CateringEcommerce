using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CateringEcommerce.Domain.Models.Supervisor;

namespace CateringEcommerce.Domain.Interfaces.Supervisor
{
    /// <summary>
    /// Supervisor Repository - Core CRUD and Authority Management
    /// </summary>
    public interface ISupervisorRepository
    {
        // =============================================
        // BASIC CRUD
        // =============================================

        Task<SupervisorModel> GetSupervisorByIdAsync(long supervisorId);
        Task<SupervisorModel> GetSupervisorByEmailAsync(string email);
        Task<SupervisorModel> GetSupervisorByPhoneAsync(string phone);
        Task<List<SupervisorModel>> GetAllSupervisorsAsync(SupervisorType? type = null, string status = null);
        Task<bool> UpdateSupervisorAsync(long supervisorId, UpdateSupervisorDto updates);
        Task<bool> DeleteSupervisorAsync(long supervisorId);

        // =============================================
        // AUTHORITY MANAGEMENT
        // =============================================

        /// <summary>
        /// Check supervisor's authority for specific action
        /// Returns: CanPerformAction, RequiresApproval, AuthorityLevel
        /// </summary>
        Task<AuthorityCheckResult> CheckSupervisorAuthorityAsync(long supervisorId, string actionType);

        /// <summary>
        /// Update supervisor authority level
        /// Only upgrades allowed (BASIC → INTERMEDIATE → ADVANCED → FULL)
        /// </summary>
        Task<bool> UpdateAuthorityLevelAsync(long supervisorId, AuthorityLevel newLevel, long updatedBy, string reason);

        /// <summary>
        /// Grant specific permissions to supervisor
        /// </summary>
        Task<bool> GrantPermissionAsync(long supervisorId, string permissionType, long grantedBy);

        /// <summary>
        /// Revoke specific permissions from supervisor
        /// </summary>
        Task<bool> RevokePermissionAsync(long supervisorId, string permissionType, long revokedBy);

        // =============================================
        // STATUS MANAGEMENT
        // =============================================

        /// <summary>
        /// Update supervisor status (part of workflow progression)
        /// </summary>
        Task<bool> UpdateStatusAsync(long supervisorId, string newStatus, long updatedBy, string notes);

        /// <summary>
        /// Activate supervisor (after all requirements met)
        /// </summary>
        Task<bool> ActivateSupervisorAsync(long supervisorId, long activatedBy);

        /// <summary>
        /// Suspend supervisor (temporary)
        /// </summary>
        Task<bool> SuspendSupervisorAsync(long supervisorId, long suspendedBy, string reason);

        /// <summary>
        /// Terminate supervisor (permanent)
        /// </summary>
        Task<bool> TerminateSupervisorAsync(long supervisorId, long terminatedBy, string reason);

        // =============================================
        // DASHBOARD & ANALYTICS
        // =============================================

        Task<SupervisorDashboardDto> GetSupervisorDashboardAsync(long supervisorId);
        Task<List<SupervisorPerformanceDto>> GetSupervisorPerformanceReportAsync(DateTime fromDate, DateTime toDate);
        Task<SupervisorStatisticsDto> GetSupervisorStatisticsAsync(SupervisorType? type = null);

        // =============================================
        // AVAILABILITY & SCHEDULING
        // =============================================

        Task<bool> UpdateAvailabilityAsync(long supervisorId, List<AvailabilitySlot> availability);
        Task<List<AvailabilitySlot>> GetAvailabilityAsync(long supervisorId, DateTime date);
        Task<List<SupervisorModel>> GetAvailableSupervisorsAsync(DateTime eventDate, string eventType, long? zoneId = null);

        // =============================================
        // SEARCH & FILTERING
        // =============================================

        Task<List<SupervisorModel>> SearchSupervisorsAsync(SupervisorSearchDto filters);
        Task<List<SupervisorModel>> GetSupervisorsByZoneAsync(long zoneId);
        Task<List<SupervisorModel>> GetSupervisorsByAuthorityAsync(AuthorityLevel authorityLevel);
    }

    #region DTOs

    public class UpdateSupervisorDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public long? ZoneId { get; set; }
        public string EmergencyContactName { get; set; }
        public string EmergencyContactPhone { get; set; }
    }

    public class AuthorityCheckResult
    {
        public bool CanPerformAction { get; set; }
        public bool RequiresApproval { get; set; }
        public AuthorityLevel CurrentAuthority { get; set; }
        public string Message { get; set; }
    }

    public class SupervisorDashboardDto
    {
        public SupervisorModel Supervisor { get; set; }
        public int TotalAssignments { get; set; }
        public int CompletedAssignments { get; set; }
        public int UpcomingAssignments { get; set; }
        public decimal AverageRating { get; set; }
        public decimal TotalEarnings { get; set; }
        public int PendingPayments { get; set; }
        public List<RecentAssignment> RecentAssignments { get; set; }
    }

    public class RecentAssignment
    {
        public long AssignmentId { get; set; }
        public string AssignmentNumber { get; set; }
        public DateTime EventDate { get; set; }
        public string Status { get; set; }
        public string VendorName { get; set; }
    }

    public class SupervisorPerformanceDto
    {
        public long SupervisorId { get; set; }
        public string SupervisorName { get; set; }
        public SupervisorType SupervisorType { get; set; }
        public int TotalEvents { get; set; }
        public decimal AverageRating { get; set; }
        public int IssuesReported { get; set; }
        public int CriticalIssues { get; set; }
        public decimal OnTimePercentage { get; set; }
        public decimal ClientSatisfactionAvg { get; set; }
    }

    public class SupervisorStatisticsDto
    {
        public int TotalSupervisors { get; set; }
        public int ActiveSupervisors { get; set; }
        public int CareerSupervisors { get; set; }
        public int RegisteredSupervisors { get; set; }
        public int SuspendedSupervisors { get; set; }
        public Dictionary<AuthorityLevel, int> SupervisorsByAuthority { get; set; }
        public Dictionary<string, int> SupervisorsByStatus { get; set; }
    }

    public class AvailabilitySlot
    {
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class SupervisorSearchDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public SupervisorType? SupervisorType { get; set; }
        public AuthorityLevel? AuthorityLevel { get; set; }
        public string Status { get; set; }
        public long? ZoneId { get; set; }
        public bool? IsActive { get; set; }
    }

    #endregion
}
