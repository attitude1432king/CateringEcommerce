using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;
using System.Threading.Tasks;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Supervisor;
using CateringEcommerce.Domain.Models.Supervisor;
using NpgsqlTypes;

namespace CateringEcommerce.BAL.Base.Supervisor
{
    public class SupervisorAssignmentRepository : ISupervisorAssignmentRepository
    {
        private readonly IDatabaseHelper _dbHelper;

        public SupervisorAssignmentRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        // =============================================
        // ASSIGNMENT CREATION & ELIGIBILITY
        // =============================================

        public async Task<List<EligibleSupervisorDto>> FindEligibleSupervisorsAsync(FindEligibleSupervisorsDto criteria)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@EventDate", criteria.EventDate),
                new NpgsqlParameter("@EventType", criteria.EventType),
                new NpgsqlParameter("@OrderValue", criteria.OrderValue),
                new NpgsqlParameter("@GuestCount", criteria.GuestCount),
                new NpgsqlParameter("@ZoneId", (object)criteria.ZoneId ?? DBNull.Value),
                new NpgsqlParameter("@IsVIPEvent", criteria.IsVIPEvent),
                new NpgsqlParameter("@IsNewVendor", criteria.IsNewVendor)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<List<EligibleSupervisorDto>>(
                "sp_FindEligibleSupervisors", parameters);
        }

        public async Task<long> AssignSupervisorToEventAsync(AssignSupervisorDto assignment)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@OrderId", assignment.OrderId),
                new NpgsqlParameter("@SupervisorId", assignment.SupervisorId),
                new NpgsqlParameter("@EventDate", assignment.EventDate),
                new NpgsqlParameter("@EventLocation", assignment.EventLocation),
                new NpgsqlParameter("@SupervisorFee", assignment.SupervisorFee),
                new NpgsqlParameter("@AssignmentNotes", (object)assignment.AssignmentNotes ?? DBNull.Value),
                new NpgsqlParameter("@AssignedBy", assignment.AssignedBy),
                new NpgsqlParameter("@AssignmentId", NpgsqlDbType.Bigint) { Direction = ParameterDirection.Output }
            };

            await _dbHelper.ExecuteStoredProcedureAsync<object>("sp_AssignSupervisorToEvent", parameters);

            return parameters[7].Value != DBNull.Value ? Convert.ToInt64(parameters[7].Value) : 0;
        }

        public async Task<List<long>> BulkAssignSupervisorsAsync(long orderId, List<long> supervisorIds, long assignedBy)
        {
            var assignmentIds = new List<long>();

            foreach (var supervisorId in supervisorIds)
            {
                var parameters = new[]
                {
                    new NpgsqlParameter("@OrderId", orderId),
                    new NpgsqlParameter("@SupervisorId", supervisorId),
                    new NpgsqlParameter("@AssignedBy", assignedBy),
                    new NpgsqlParameter("@AssignmentId", NpgsqlDbType.Bigint) { Direction = ParameterDirection.Output }
                };

                await _dbHelper.ExecuteStoredProcedureAsync<object>("sp_BulkAssignSupervisor", parameters);

                var assignmentId = parameters[3].Value != DBNull.Value ? Convert.ToInt64(parameters[3].Value) : 0;
                if (assignmentId > 0)
                    assignmentIds.Add(assignmentId);
            }

            return assignmentIds;
        }

        // =============================================
        // ASSIGNMENT RETRIEVAL
        // =============================================

        public async Task<SupervisorAssignmentModel> GetAssignmentByIdAsync(long assignmentId)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@AssignmentId", assignmentId)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<SupervisorAssignmentModel>(
                "sp_GetAssignmentById", parameters);
        }

        public async Task<List<SupervisorAssignmentModel>> GetAssignmentsBySupervisorAsync(long supervisorId, string status = null)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@SupervisorId", supervisorId),
                new NpgsqlParameter("@Status", (object)status ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<List<SupervisorAssignmentModel>>(
                "sp_GetAssignmentsBySupervisor", parameters);
        }

        public async Task<List<SupervisorAssignmentModel>> GetAssignmentsByOrderAsync(long orderId)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@OrderId", orderId)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<List<SupervisorAssignmentModel>>(
                "sp_GetAssignmentsByOrder", parameters);
        }

        public async Task<List<SupervisorAssignmentModel>> GetAllAssignmentsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@FromDate", (object)fromDate ?? DBNull.Value),
                new NpgsqlParameter("@ToDate", (object)toDate ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<List<SupervisorAssignmentModel>>(
                "sp_GetAllAssignments", parameters);
        }

        // =============================================
        // SUPERVISOR ACTIONS
        // =============================================

        public async Task<bool> AcceptAssignmentAsync(long assignmentId, long supervisorId)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@AssignmentId", assignmentId),
                new NpgsqlParameter("@SupervisorId", supervisorId)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_AcceptAssignment", parameters);
        }

        public async Task<bool> RejectAssignmentAsync(long assignmentId, long supervisorId, string reason)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@AssignmentId", assignmentId),
                new NpgsqlParameter("@SupervisorId", supervisorId),
                new NpgsqlParameter("@Reason", reason)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_RejectAssignment", parameters);
        }

        public async Task<bool> CheckInAsync(CheckInDto checkIn)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@AssignmentId", checkIn.AssignmentId),
                new NpgsqlParameter("@SupervisorId", checkIn.SupervisorId),
                new NpgsqlParameter("@GPSLocation", (object)checkIn.GPSLocation ?? DBNull.Value),
                new NpgsqlParameter("@CheckInPhoto", (object)checkIn.CheckInPhoto ?? DBNull.Value),
                new NpgsqlParameter("@CheckInTime", checkIn.CheckInTime)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_SupervisorCheckIn", parameters);
        }

        public async Task<PaymentReleaseResponse> RequestPaymentReleaseAsync(long assignmentId, long supervisorId, decimal amount)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@AssignmentId", assignmentId),
                new NpgsqlParameter("@SupervisorId", supervisorId),
                new NpgsqlParameter("@Amount", amount),
                new NpgsqlParameter("@DirectRelease", NpgsqlDbType.Boolean) { Direction = ParameterDirection.Output },
                new NpgsqlParameter("@RequiresApproval", NpgsqlDbType.Boolean) { Direction = ParameterDirection.Output }
            };

            await _dbHelper.ExecuteStoredProcedureAsync<object>("sp_RequestPaymentRelease", parameters);

            var directRelease = parameters[3].Value != DBNull.Value && Convert.ToBoolean(parameters[3].Value);
            var requiresApproval = parameters[4].Value != DBNull.Value && Convert.ToBoolean(parameters[4].Value);

            return new PaymentReleaseResponse
            {
                Success = true,
                Message = directRelease ? "Payment released successfully" : "Payment release request submitted for admin approval",
                DirectRelease = directRelease,
                RequiresApproval = requiresApproval,
                ReleasedAt = directRelease ? DateTime.Now : (DateTime?)null,
                RequestedAt = requiresApproval ? DateTime.Now : (DateTime?)null
            };
        }

        public async Task<bool> ApprovePaymentReleaseAsync(long assignmentId, long approvedBy, string notes)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@AssignmentId", assignmentId),
                new NpgsqlParameter("@ApprovedBy", approvedBy),
                new NpgsqlParameter("@Notes", (object)notes ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_ApprovePaymentRelease", parameters);
        }

        // =============================================
        // ASSIGNMENT STATUS MANAGEMENT
        // =============================================

        public async Task<bool> UpdateAssignmentStatusAsync(long assignmentId, string newStatus, long updatedBy, string notes)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@AssignmentId", assignmentId),
                new NpgsqlParameter("@NewStatus", newStatus),
                new NpgsqlParameter("@UpdatedBy", updatedBy),
                new NpgsqlParameter("@Notes", (object)notes ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_UpdateAssignmentStatus", parameters);
        }

        public async Task<bool> CancelAssignmentAsync(long assignmentId, long cancelledBy, string reason)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@AssignmentId", assignmentId),
                new NpgsqlParameter("@CancelledBy", cancelledBy),
                new NpgsqlParameter("@Reason", reason)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_CancelAssignment", parameters);
        }

        public async Task<bool> CompleteAssignmentAsync(long assignmentId, long completedBy)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@AssignmentId", assignmentId),
                new NpgsqlParameter("@CompletedBy", completedBy)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_CompleteAssignment", parameters);
        }

        // =============================================
        // ANALYTICS & REPORTING
        // =============================================

        public async Task<List<AssignmentSummaryDto>> GetUpcomingAssignmentsAsync(int daysAhead = 7)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@DaysAhead", daysAhead)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<List<AssignmentSummaryDto>>(
                "sp_GetUpcomingAssignments", parameters);
        }

        public async Task<List<AssignmentSummaryDto>> GetOverdueAssignmentsAsync()
        {
            return await _dbHelper.ExecuteStoredProcedureAsync<List<AssignmentSummaryDto>>(
                "sp_GetOverdueAssignments", null);
        }

        public async Task<AssignmentStatisticsDto> GetAssignmentStatisticsAsync(DateTime fromDate, DateTime toDate)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@FromDate", fromDate),
                new NpgsqlParameter("@ToDate", toDate)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<AssignmentStatisticsDto>(
                "sp_GetAssignmentStatistics", parameters);
        }

        public async Task<List<SupervisorWorkloadDto>> GetSupervisorWorkloadAsync(DateTime fromDate, DateTime toDate)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@FromDate", fromDate),
                new NpgsqlParameter("@ToDate", toDate)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<List<SupervisorWorkloadDto>>(
                "sp_GetSupervisorWorkload", parameters);
        }

        // =============================================
        // SEARCH & FILTERING
        // =============================================

        public async Task<List<SupervisorAssignmentModel>> SearchAssignmentsAsync(AssignmentSearchDto filters)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@SupervisorId", (object)filters.SupervisorId ?? DBNull.Value),
                new NpgsqlParameter("@OrderId", (object)filters.OrderId ?? DBNull.Value),
                new NpgsqlParameter("@Status", (object)filters.Status ?? DBNull.Value),
                new NpgsqlParameter("@EventDateFrom", (object)filters.EventDateFrom ?? DBNull.Value),
                new NpgsqlParameter("@EventDateTo", (object)filters.EventDateTo ?? DBNull.Value),
                new NpgsqlParameter("@SupervisorType", (object)filters.SupervisorType?.ToString() ?? DBNull.Value),
                new NpgsqlParameter("@PaymentReleased", (object)filters.PaymentReleased ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<List<SupervisorAssignmentModel>>(
                "sp_SearchAssignments", parameters);
        }
    }
}
