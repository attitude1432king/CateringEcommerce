using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using System.Threading.Tasks;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Supervisor;
using CateringEcommerce.Domain.Models.Supervisor;

namespace CateringEcommerce.BAL.Base.Supervisor
{
    /// <summary>
    /// Event Supervision Repository Implementation
    /// Handles complete event lifecycle: Pre-Event → During-Event → Post-Event
    /// </summary>
    public class EventSupervisionRepository : IEventSupervisionRepository
    {
        private readonly IDatabaseHelper _dbHelper;

        public EventSupervisionRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        // =============================================
        // PRE-EVENT VERIFICATION
        // =============================================

        public async Task<bool> SubmitPreEventVerificationAsync(SubmitPreEventVerificationDto request)
        {
            var parameters = new[]
            {
                new SqlParameter("@AssignmentId", request.AssignmentId),
                new SqlParameter("@SupervisorId", request.SupervisorId),
                new SqlParameter("@MenuVerified", request.MenuVerified),
                new SqlParameter("@MenuVsContractMatch", request.MenuVsContractMatch),
                new SqlParameter("@MenuVerificationNotes", (object)request.MenuVerificationNotes ?? DBNull.Value),
                new SqlParameter("@MenuVerificationPhotos", (object)JsonSerializer.Serialize(request.MenuVerificationPhotos ?? new List<string>()) ?? DBNull.Value),
                new SqlParameter("@RawMaterialVerified", request.RawMaterialVerified),
                new SqlParameter("@RawMaterialQualityOK", request.RawMaterialQualityOK),
                new SqlParameter("@RawMaterialQuantityOK", request.RawMaterialQuantityOK),
                new SqlParameter("@RawMaterialNotes", (object)request.RawMaterialNotes ?? DBNull.Value),
                new SqlParameter("@RawMaterialPhotos", (object)JsonSerializer.Serialize(request.RawMaterialPhotos ?? new List<string>()) ?? DBNull.Value),
                new SqlParameter("@GuestCountConfirmed", request.GuestCountConfirmed),
                new SqlParameter("@ConfirmedGuestCount", request.ConfirmedGuestCount),
                new SqlParameter("@PreEventEvidenceUrls", (object)JsonSerializer.Serialize(request.PreEventEvidence ?? new List<TimestampedEvidence>()) ?? DBNull.Value),
                new SqlParameter("@IssuesFound", request.IssuesFound),
                new SqlParameter("@IssuesDescription", (object)request.IssuesDescription ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_SubmitPreEventVerification", parameters);
        }

        public async Task<PreEventVerificationModel> GetPreEventVerificationAsync(long assignmentId)
        {
            var parameters = new[]
            {
                new SqlParameter("@AssignmentId", assignmentId)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<PreEventVerificationModel>(
                "sp_GetPreEventVerification", parameters);
        }

        public async Task<bool> UpdatePreEventChecklistAsync(long checklistId, PreEventVerificationModel updates)
        {
            var parameters = new[]
            {
                new SqlParameter("@ChecklistId", checklistId),
                new SqlParameter("@MenuVerified", updates.MenuVerified),
                new SqlParameter("@RawMaterialVerified", updates.RawMaterialVerified),
                new SqlParameter("@GuestCountConfirmed", updates.GuestCountConfirmed),
                new SqlParameter("@ChecklistPhotos", (object)JsonSerializer.Serialize(updates.ChecklistPhotos ?? new List<string>()) ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_UpdatePreEventChecklist", parameters);
        }

        // =============================================
        // DURING-EVENT MONITORING
        // =============================================

        public async Task<bool> RecordFoodServingMonitorAsync(FoodServingMonitorDto request)
        {
            var parameters = new[]
            {
                new SqlParameter("@AssignmentId", request.AssignmentId),
                new SqlParameter("@SupervisorId", request.SupervisorId),
                new SqlParameter("@QualityRating", request.QualityRating),
                new SqlParameter("@TemperatureOK", request.TemperatureOK),
                new SqlParameter("@PresentationOK", request.PresentationOK),
                new SqlParameter("@Notes", (object)request.Notes ?? DBNull.Value),
                new SqlParameter("@Photos", (object)JsonSerializer.Serialize(request.Photos ?? new List<string>()) ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_RecordFoodServingMonitor", parameters);
        }

        public async Task<bool> UpdateGuestCountAsync(UpdateGuestCountDto request)
        {
            var parameters = new[]
            {
                new SqlParameter("@AssignmentId", request.AssignmentId),
                new SqlParameter("@SupervisorId", request.SupervisorId),
                new SqlParameter("@ActualGuestCount", request.ActualGuestCount),
                new SqlParameter("@Notes", (object)request.Notes ?? DBNull.Value),
                new SqlParameter("@Timestamp", request.Timestamp)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_UpdateGuestCount", parameters);
        }

        public async Task<RequestExtraQuantityResponse> RequestExtraQuantityAsync(RequestExtraQuantityDto request)
        {
            var parameters = new[]
            {
                new SqlParameter("@AssignmentId", request.AssignmentId),
                new SqlParameter("@SupervisorId", request.SupervisorId),
                new SqlParameter("@ItemName", request.ItemName),
                new SqlParameter("@ExtraQuantity", request.ExtraQuantity),
                new SqlParameter("@ExtraCost", request.ExtraCost),
                new SqlParameter("@Reason", request.Reason),
                new SqlParameter("@ClientPhone", request.ClientPhone),
                new SqlParameter("@ApprovalMethod", request.ApprovalMethod.ToString()),
                new SqlParameter("@OTPCode", SqlDbType.VarChar, 10) { Direction = ParameterDirection.Output },
                new SqlParameter("@TrackingId", SqlDbType.BigInt) { Direction = ParameterDirection.Output }
            };

            await _dbHelper.ExecuteStoredProcedureAsync<object>("sp_RequestExtraQuantity", parameters);

            var otpCode = parameters[8].Value?.ToString();
            var trackingId = parameters[9].Value != DBNull.Value ? Convert.ToInt64(parameters[9].Value) : 0;

            return new RequestExtraQuantityResponse
            {
                Success = trackingId > 0,
                Message = trackingId > 0 ? "Extra quantity request submitted successfully" : "Failed to submit request",
                TrackingId = trackingId,
                OTPCode = request.ApprovalMethod == ClientApprovalMethod.OTP ? otpCode : null,
                OTPExpiresAt = request.ApprovalMethod == ClientApprovalMethod.OTP ? DateTime.Now.AddMinutes(10) : null,
                RequiresApproval = true,
                ApprovalMethod = request.ApprovalMethod
            };
        }

        public async Task<OTPVerificationResponse> VerifyClientOTPAsync(VerifyClientOTPDto request)
        {
            var parameters = new[]
            {
                new SqlParameter("@AssignmentId", request.AssignmentId),
                new SqlParameter("@OTPCode", request.OTPCode),
                new SqlParameter("@ClientIPAddress", (object)request.ClientIPAddress ?? DBNull.Value),
                new SqlParameter("@IsValid", SqlDbType.Bit) { Direction = ParameterDirection.Output },
                new SqlParameter("@RemainingAttempts", SqlDbType.Int) { Direction = ParameterDirection.Output },
                new SqlParameter("@IsExpired", SqlDbType.Bit) { Direction = ParameterDirection.Output }
            };

            await _dbHelper.ExecuteStoredProcedureAsync<object>("sp_VerifyClientOTP", parameters);

            var isValid = parameters[3].Value != DBNull.Value && Convert.ToBoolean(parameters[3].Value);
            var remainingAttempts = parameters[4].Value != DBNull.Value ? Convert.ToInt32(parameters[4].Value) : 0;
            var isExpired = parameters[5].Value != DBNull.Value && Convert.ToBoolean(parameters[5].Value);

            return new OTPVerificationResponse
            {
                Success = isValid,
                Message = isValid ? "OTP verified successfully" :
                         isExpired ? "OTP has expired" :
                         remainingAttempts == 0 ? "Maximum attempts reached" :
                         "Invalid OTP",
                OTPVerified = isValid,
                ApprovalStatus = isValid ? ClientApprovalStatus.APPROVED : ClientApprovalStatus.PENDING,
                RemainingAttempts = remainingAttempts,
                IsExpired = isExpired
            };
        }

        public async Task<List<DuringEventTrackingModel>> GetDuringEventTrackingAsync(long assignmentId)
        {
            var parameters = new[]
            {
                new SqlParameter("@AssignmentId", assignmentId)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<List<DuringEventTrackingModel>>(
                "sp_GetDuringEventTracking", parameters);
        }

        // =============================================
        // POST-EVENT COMPLETION
        // =============================================

        public async Task<bool> SubmitPostEventReportAsync(SubmitPostEventReportDto request)
        {
            var parameters = new[]
            {
                new SqlParameter("@AssignmentId", request.AssignmentId),
                new SqlParameter("@SupervisorId", request.SupervisorId),
                new SqlParameter("@FinalGuestCount", request.FinalGuestCount),
                new SqlParameter("@EventRating", request.EventRating),
                new SqlParameter("@ClientName", request.ClientName),
                new SqlParameter("@ClientPhone", request.ClientPhone),
                new SqlParameter("@ClientSatisfactionRating", request.ClientSatisfactionRating),
                new SqlParameter("@FoodQualityRating", request.FoodQualityRating),
                new SqlParameter("@FoodQuantityRating", request.FoodQuantityRating),
                new SqlParameter("@ServiceQualityRating", request.ServiceQualityRating),
                new SqlParameter("@PresentationRating", request.PresentationRating),
                new SqlParameter("@WouldRecommend", request.WouldRecommend),
                new SqlParameter("@ClientComments", (object)request.ClientComments ?? DBNull.Value),
                new SqlParameter("@ClientSignatureUrl", (object)request.ClientSignatureUrl ?? DBNull.Value),
                new SqlParameter("@VendorPunctualityRating", request.VendorPunctualityRating),
                new SqlParameter("@VendorPreparationRating", request.VendorPreparationRating),
                new SqlParameter("@VendorCooperationRating", request.VendorCooperationRating),
                new SqlParameter("@VendorComments", (object)request.VendorComments ?? DBNull.Value),
                new SqlParameter("@IssuesCount", request.IssuesCount),
                new SqlParameter("@IssuesSummary", (object)JsonSerializer.Serialize(request.Issues ?? new List<EventIssue>()) ?? DBNull.Value),
                new SqlParameter("@FinalPayableAmount", request.FinalPayableAmount),
                new SqlParameter("@PaymentBreakdown", (object)JsonSerializer.Serialize(request.PaymentBreakdown) ?? DBNull.Value),
                new SqlParameter("@ReportSummary", request.ReportSummary),
                new SqlParameter("@Recommendations", (object)request.Recommendations ?? DBNull.Value),
                new SqlParameter("@CompletionPhotos", (object)JsonSerializer.Serialize(request.CompletionPhotos ?? new List<string>()) ?? DBNull.Value),
                new SqlParameter("@CompletionVideos", (object)JsonSerializer.Serialize(request.CompletionVideos ?? new List<string>()) ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_SubmitPostEventReport", parameters);
        }

        public async Task<PostEventReportModel> GetPostEventReportAsync(long assignmentId)
        {
            var parameters = new[]
            {
                new SqlParameter("@AssignmentId", assignmentId)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<PostEventReportModel>(
                "sp_GetPostEventReport", parameters);
        }

        public async Task<bool> UpdatePostEventReportAsync(long reportId, PostEventReportModel updates)
        {
            var parameters = new[]
            {
                new SqlParameter("@ReportId", reportId),
                new SqlParameter("@ReportSummary", updates.ReportSummary),
                new SqlParameter("@Recommendations", (object)updates.Recommendations ?? DBNull.Value),
                new SqlParameter("@SupervisorNotes", (object)updates.SupervisorNotes ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_UpdatePostEventReport", parameters);
        }

        public async Task<bool> VerifyPostEventReportAsync(long reportId, long verifiedBy, string verificationNotes)
        {
            var parameters = new[]
            {
                new SqlParameter("@ReportId", reportId),
                new SqlParameter("@VerifiedBy", verifiedBy),
                new SqlParameter("@VerificationNotes", (object)verificationNotes ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_VerifyPostEventReport", parameters);
        }

        // =============================================
        // COMPLETE EVENT SUPERVISION SUMMARY
        // =============================================

        public async Task<EventSupervisionSummaryDto> GetEventSupervisionSummaryAsync(long assignmentId)
        {
            var parameters = new[]
            {
                new SqlParameter("@AssignmentId", assignmentId)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<EventSupervisionSummaryDto>(
                "sp_GetEventSupervisionSummary", parameters);
        }

        // =============================================
        // OTP MANAGEMENT
        // =============================================

        public async Task<string> ResendClientOTPAsync(long assignmentId, string purpose)
        {
            var parameters = new[]
            {
                new SqlParameter("@AssignmentId", assignmentId),
                new SqlParameter("@OTPPurpose", purpose),
                new SqlParameter("@NewOTPCode", SqlDbType.VarChar, 10) { Direction = ParameterDirection.Output }
            };

            await _dbHelper.ExecuteStoredProcedureAsync<object>("sp_ResendClientOTP", parameters);

            return parameters[2].Value?.ToString();
        }

        public async Task<ClientOTPVerificationModel> GetOTPVerificationStatusAsync(string otpCode)
        {
            var parameters = new[]
            {
                new SqlParameter("@OTPCode", otpCode)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<ClientOTPVerificationModel>(
                "sp_GetOTPVerificationStatus", parameters);
        }

        // =============================================
        // EVIDENCE & DOCUMENTATION
        // =============================================

        public async Task<bool> UploadTimestampedEvidenceAsync(long assignmentId, List<TimestampedEvidence> evidence, string phase)
        {
            var parameters = new[]
            {
                new SqlParameter("@AssignmentId", assignmentId),
                new SqlParameter("@EvidenceData", JsonSerializer.Serialize(evidence)),
                new SqlParameter("@Phase", phase) // PRE_EVENT, DURING_EVENT, POST_EVENT
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_UploadTimestampedEvidence", parameters);
        }

        public async Task<Dictionary<string, List<TimestampedEvidence>>> GetAssignmentEvidenceAsync(long assignmentId)
        {
            var parameters = new[]
            {
                new SqlParameter("@AssignmentId", assignmentId)
            };

            var result = await _dbHelper.ExecuteStoredProcedureAsync<dynamic>("sp_GetAssignmentEvidence", parameters);

            // Parse JSON evidence from database
            var evidenceDict = new Dictionary<string, List<TimestampedEvidence>>();

            if (result != null)
            {
                if (result.PreEventEvidence != null)
                    evidenceDict["PRE_EVENT"] = JsonSerializer.Deserialize<List<TimestampedEvidence>>(result.PreEventEvidence.ToString());

                if (result.DuringEventEvidence != null)
                    evidenceDict["DURING_EVENT"] = JsonSerializer.Deserialize<List<TimestampedEvidence>>(result.DuringEventEvidence.ToString());

                if (result.PostEventEvidence != null)
                    evidenceDict["POST_EVENT"] = JsonSerializer.Deserialize<List<TimestampedEvidence>>(result.PostEventEvidence.ToString());
            }

            return evidenceDict;
        }
    }
}
