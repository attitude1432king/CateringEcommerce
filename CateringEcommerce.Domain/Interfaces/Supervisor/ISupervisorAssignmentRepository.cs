using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CateringEcommerce.Domain.Models.Supervisor;

namespace CateringEcommerce.Domain.Interfaces.Supervisor
{
    /// <summary>
    /// Supervisor Assignment Repository - Event Assignment Management
    /// Handles assignment creation, acceptance, check-in, completion, payment release
    /// </summary>
    public interface ISupervisorAssignmentRepository
    {
        // =============================================
        // ASSIGNMENT CREATION & ELIGIBILITY
        // =============================================

        /// <summary>
        /// Find eligible supervisors for event based on rules
        /// Uses sp_FindEligibleSupervisors stored procedure
        /// </summary>
        Task<List<EligibleSupervisorDto>> FindEligibleSupervisorsAsync(FindEligibleSupervisorsDto criteria);

        /// <summary>
        /// Assign supervisor to event
        /// Uses sp_AssignSupervisorToEvent stored procedure
        /// </summary>
        Task<long> AssignSupervisorToEventAsync(AssignSupervisorDto assignment);

        /// <summary>
        /// Bulk assign multiple supervisors to event
        /// </summary>
        Task<List<long>> BulkAssignSupervisorsAsync(long orderId, List<long> supervisorIds, long assignedBy);

        // =============================================
        // ASSIGNMENT RETRIEVAL
        // =============================================

        Task<SupervisorAssignmentModel> GetAssignmentByIdAsync(long assignmentId);
        Task<List<SupervisorAssignmentModel>> GetAssignmentsBySupervisorAsync(long supervisorId, string status = null);
        Task<List<SupervisorAssignmentModel>> GetAssignmentsByOrderAsync(long orderId);
        Task<List<SupervisorAssignmentModel>> GetAllAssignmentsAsync(DateTime? fromDate = null, DateTime? toDate = null);

        // =============================================
        // SUPERVISOR ACTIONS
        // =============================================

        /// <summary>
        /// Supervisor accepts assignment
        /// </summary>
        Task<bool> AcceptAssignmentAsync(long assignmentId, long supervisorId);

        /// <summary>
        /// Supervisor rejects assignment
        /// </summary>
        Task<bool> RejectAssignmentAsync(long assignmentId, long supervisorId, string reason);

        /// <summary>
        /// Supervisor checks in at event location
        /// Uses sp_SupervisorCheckIn stored procedure
        /// </summary>
        Task<bool> CheckInAsync(CheckInDto checkIn);

        /// <summary>
        /// Supervisor requests payment release
        /// CAREER (FULL authority): Direct release
        /// REGISTERED: Request only (requires admin approval)
        /// Uses sp_RequestPaymentRelease stored procedure
        /// </summary>
        Task<PaymentReleaseResponse> RequestPaymentReleaseAsync(long assignmentId, long supervisorId, decimal amount);

        /// <summary>
        /// Admin approves payment release request (for REGISTERED supervisors)
        /// </summary>
        Task<bool> ApprovePaymentReleaseAsync(long assignmentId, long approvedBy, string notes);

        // =============================================
        // ASSIGNMENT STATUS MANAGEMENT
        // =============================================

        Task<bool> UpdateAssignmentStatusAsync(long assignmentId, string newStatus, long updatedBy, string notes);
        Task<bool> CancelAssignmentAsync(long assignmentId, long cancelledBy, string reason);
        Task<bool> CompleteAssignmentAsync(long assignmentId, long completedBy);

        // =============================================
        // ANALYTICS & REPORTING
        // =============================================

        Task<List<AssignmentSummaryDto>> GetUpcomingAssignmentsAsync(int daysAhead = 7);
        Task<List<AssignmentSummaryDto>> GetOverdueAssignmentsAsync();
        Task<AssignmentStatisticsDto> GetAssignmentStatisticsAsync(DateTime fromDate, DateTime toDate);
        Task<List<SupervisorWorkloadDto>> GetSupervisorWorkloadAsync(DateTime fromDate, DateTime toDate);

        // =============================================
        // SEARCH & FILTERING
        // =============================================

        Task<List<SupervisorAssignmentModel>> SearchAssignmentsAsync(AssignmentSearchDto filters);
    }

    #region DTOs

    public class FindEligibleSupervisorsDto
    {
        public long OrderId { get; set; }
        public DateTime EventDate { get; set; }
        public string EventType { get; set; }
        public decimal OrderValue { get; set; }
        public int GuestCount { get; set; }
        public long? ZoneId { get; set; }
        public bool IsVIPEvent { get; set; }
        public bool IsNewVendor { get; set; }
    }

    public class EligibleSupervisorDto
    {
        public long SupervisorId { get; set; }
        public string SupervisorName { get; set; }
        public SupervisorType SupervisorType { get; set; }
        public AuthorityLevel AuthorityLevel { get; set; }
        public int EligibilityScore { get; set; }
        public string EligibilityReason { get; set; }
        public bool IsAvailable { get; set; }
        public int AssignmentCount { get; set; }
        public decimal AverageRating { get; set; }
    }

    public class AssignSupervisorDto
    {
        public long OrderId { get; set; }
        public long SupervisorId { get; set; }
        public DateTime EventDate { get; set; }
        public string EventLocation { get; set; }
        public decimal SupervisorFee { get; set; }
        public string AssignmentNotes { get; set; }
        public long AssignedBy { get; set; }
    }

    public class CheckInDto
    {
        public long AssignmentId { get; set; }
        public long SupervisorId { get; set; }
        public string GPSLocation { get; set; }
        public string CheckInPhoto { get; set; }
        public DateTime CheckInTime { get; set; }
    }

    public class PaymentReleaseResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public bool DirectRelease { get; set; } // True for CAREER (FULL), False for REGISTERED
        public bool RequiresApproval { get; set; }
        public DateTime? ReleasedAt { get; set; }
        public DateTime? RequestedAt { get; set; }
    }

    public class AssignmentSummaryDto
    {
        public long AssignmentId { get; set; }
        public string AssignmentNumber { get; set; }
        public long SupervisorId { get; set; }
        public string SupervisorName { get; set; }
        public long OrderId { get; set; }
        public string OrderNumber { get; set; }
        public DateTime EventDate { get; set; }
        public string Status { get; set; }
        public string EventLocation { get; set; }
        public decimal SupervisorFee { get; set; }
    }

    public class AssignmentStatisticsDto
    {
        public int TotalAssignments { get; set; }
        public int CompletedAssignments { get; set; }
        public int PendingAssignments { get; set; }
        public int CancelledAssignments { get; set; }
        public Dictionary<string, int> AssignmentsByStatus { get; set; }
        public Dictionary<SupervisorType, int> AssignmentsByType { get; set; }
        public decimal TotalSupervisorFees { get; set; }
        public decimal AverageAssignmentRating { get; set; }
    }

    public class SupervisorWorkloadDto
    {
        public long SupervisorId { get; set; }
        public string SupervisorName { get; set; }
        public SupervisorType SupervisorType { get; set; }
        public int TotalAssignments { get; set; }
        public int CompletedAssignments { get; set; }
        public int PendingAssignments { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal AverageRating { get; set; }
        public int WorkloadPercentage { get; set; }
    }

    public class AssignmentSearchDto
    {
        public long? SupervisorId { get; set; }
        public long? OrderId { get; set; }
        public string Status { get; set; }
        public DateTime? EventDateFrom { get; set; }
        public DateTime? EventDateTo { get; set; }
        public SupervisorType? SupervisorType { get; set; }
        public bool? PaymentReleased { get; set; }
    }

    #endregion
}
