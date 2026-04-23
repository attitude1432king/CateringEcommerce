using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;
using System.Text.Json;
using System.Threading.Tasks;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Supervisor;
using CateringEcommerce.Domain.Models.Supervisor;
using NpgsqlTypes;

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
                new NpgsqlParameter("@AssignmentId", request.AssignmentId),
                new NpgsqlParameter("@SupervisorId", request.SupervisorId),
                new NpgsqlParameter("@MenuVerified", request.MenuVerified),
                new NpgsqlParameter("@MenuVsContractMatch", request.MenuVsContractMatch),
                new NpgsqlParameter("@MenuVerificationNotes", (object)request.MenuVerificationNotes ?? DBNull.Value),
                new NpgsqlParameter("@MenuVerificationPhotos", (object)JsonSerializer.Serialize(request.MenuVerificationPhotos ?? new List<string>()) ?? DBNull.Value),
                new NpgsqlParameter("@RawMaterialVerified", request.RawMaterialVerified),
                new NpgsqlParameter("@RawMaterialQualityOK", request.RawMaterialQualityOK),
                new NpgsqlParameter("@RawMaterialQuantityOK", request.RawMaterialQuantityOK),
                new NpgsqlParameter("@RawMaterialNotes", (object)request.RawMaterialNotes ?? DBNull.Value),
                new NpgsqlParameter("@RawMaterialPhotos", (object)JsonSerializer.Serialize(request.RawMaterialPhotos ?? new List<string>()) ?? DBNull.Value),
                new NpgsqlParameter("@GuestCountConfirmed", request.GuestCountConfirmed),
                new NpgsqlParameter("@ConfirmedGuestCount", request.ConfirmedGuestCount),
                new NpgsqlParameter("@PreEventEvidenceUrls", (object)JsonSerializer.Serialize(request.PreEventEvidence ?? new List<TimestampedEvidence>()) ?? DBNull.Value),
                new NpgsqlParameter("@IssuesFound", request.IssuesFound),
                new NpgsqlParameter("@IssuesDescription", (object)request.IssuesDescription ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_SubmitPreEventVerification", parameters);
        }

        public async Task<PreEventVerificationModel> GetPreEventVerificationAsync(long assignmentId)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@AssignmentId", assignmentId)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<PreEventVerificationModel>(
                "sp_GetPreEventVerification", parameters);
        }

        public async Task<bool> UpdatePreEventChecklistAsync(long checklistId, PreEventVerificationModel updates)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@ChecklistId", checklistId),
                new NpgsqlParameter("@MenuVerified", updates.MenuVerified),
                new NpgsqlParameter("@RawMaterialVerified", updates.RawMaterialVerified),
                new NpgsqlParameter("@GuestCountConfirmed", updates.GuestCountConfirmed),
                new NpgsqlParameter("@ChecklistPhotos", (object)JsonSerializer.Serialize(updates.ChecklistPhotos ?? new List<string>()) ?? DBNull.Value)
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
                new NpgsqlParameter("@AssignmentId", request.AssignmentId),
                new NpgsqlParameter("@SupervisorId", request.SupervisorId),
                new NpgsqlParameter("@QualityRating", request.QualityRating),
                new NpgsqlParameter("@TemperatureOK", request.TemperatureOK),
                new NpgsqlParameter("@PresentationOK", request.PresentationOK),
                new NpgsqlParameter("@Notes", (object)request.Notes ?? DBNull.Value),
                new NpgsqlParameter("@Photos", (object)JsonSerializer.Serialize(request.Photos ?? new List<string>()) ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_RecordFoodServingMonitor", parameters);
        }

        public async Task<bool> UpdateGuestCountAsync(UpdateGuestCountDto request)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@AssignmentId", request.AssignmentId),
                new NpgsqlParameter("@SupervisorId", request.SupervisorId),
                new NpgsqlParameter("@ActualGuestCount", request.ActualGuestCount),
                new NpgsqlParameter("@Notes", (object)request.Notes ?? DBNull.Value),
                new NpgsqlParameter("@Timestamp", request.Timestamp)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_UpdateGuestCount", parameters);
        }

        public async Task<RequestExtraQuantityResponse> RequestExtraQuantityAsync(RequestExtraQuantityDto request)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@AssignmentId", request.AssignmentId),
                new NpgsqlParameter("@SupervisorId", request.SupervisorId),
                new NpgsqlParameter("@ItemName", request.ItemName),
                new NpgsqlParameter("@ExtraQuantity", request.ExtraQuantity),
                new NpgsqlParameter("@ExtraCost", request.ExtraCost),
                new NpgsqlParameter("@Reason", request.Reason),
                new NpgsqlParameter("@ClientPhone", request.ClientPhone),
                new NpgsqlParameter("@ApprovalMethod", request.ApprovalMethod.ToString()),
                new NpgsqlParameter("@OTPCode", NpgsqlDbType.Varchar, 10) { Direction = ParameterDirection.Output },
                new NpgsqlParameter("@TrackingId", NpgsqlDbType.Bigint) { Direction = ParameterDirection.Output }
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
                new NpgsqlParameter("@AssignmentId", request.AssignmentId),
                new NpgsqlParameter("@OTPCode", request.OTPCode),
                new NpgsqlParameter("@ClientIPAddress", (object)request.ClientIPAddress ?? DBNull.Value),
                new NpgsqlParameter("@IsValid", NpgsqlDbType.Boolean) { Direction = ParameterDirection.Output },
                new NpgsqlParameter("@RemainingAttempts", NpgsqlDbType.Integer) { Direction = ParameterDirection.Output },
                new NpgsqlParameter("@IsExpired", NpgsqlDbType.Boolean) { Direction = ParameterDirection.Output }
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
                new NpgsqlParameter("@AssignmentId", assignmentId)
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
                new NpgsqlParameter("@AssignmentId", request.AssignmentId),
                new NpgsqlParameter("@SupervisorId", request.SupervisorId),
                new NpgsqlParameter("@FinalGuestCount", request.FinalGuestCount),
                new NpgsqlParameter("@EventRating", request.EventRating),
                new NpgsqlParameter("@ClientName", request.ClientName),
                new NpgsqlParameter("@ClientPhone", request.ClientPhone),
                new NpgsqlParameter("@ClientSatisfactionRating", request.ClientSatisfactionRating),
                new NpgsqlParameter("@FoodQualityRating", request.FoodQualityRating),
                new NpgsqlParameter("@FoodQuantityRating", request.FoodQuantityRating),
                new NpgsqlParameter("@ServiceQualityRating", request.ServiceQualityRating),
                new NpgsqlParameter("@PresentationRating", request.PresentationRating),
                new NpgsqlParameter("@WouldRecommend", request.WouldRecommend),
                new NpgsqlParameter("@ClientComments", (object)request.ClientComments ?? DBNull.Value),
                new NpgsqlParameter("@ClientSignatureUrl", (object)request.ClientSignatureUrl ?? DBNull.Value),
                new NpgsqlParameter("@VendorPunctualityRating", request.VendorPunctualityRating),
                new NpgsqlParameter("@VendorPreparationRating", request.VendorPreparationRating),
                new NpgsqlParameter("@VendorCooperationRating", request.VendorCooperationRating),
                new NpgsqlParameter("@VendorComments", (object)request.VendorComments ?? DBNull.Value),
                new NpgsqlParameter("@IssuesCount", request.IssuesCount),
                new NpgsqlParameter("@IssuesSummary", (object)JsonSerializer.Serialize(request.Issues ?? new List<EventIssue>()) ?? DBNull.Value),
                new NpgsqlParameter("@FinalPayableAmount", request.FinalPayableAmount),
                new NpgsqlParameter("@PaymentBreakdown", (object)JsonSerializer.Serialize(request.PaymentBreakdown) ?? DBNull.Value),
                new NpgsqlParameter("@ReportSummary", request.ReportSummary),
                new NpgsqlParameter("@Recommendations", (object)request.Recommendations ?? DBNull.Value),
                new NpgsqlParameter("@CompletionPhotos", (object)JsonSerializer.Serialize(request.CompletionPhotos ?? new List<string>()) ?? DBNull.Value),
                new NpgsqlParameter("@CompletionVideos", (object)JsonSerializer.Serialize(request.CompletionVideos ?? new List<string>()) ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_SubmitPostEventReport", parameters);
        }

        public async Task<PostEventReportModel> GetPostEventReportAsync(long assignmentId)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@AssignmentId", assignmentId)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<PostEventReportModel>(
                "sp_GetPostEventReport", parameters);
        }

        public async Task<bool> UpdatePostEventReportAsync(long reportId, PostEventReportModel updates)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@ReportId", reportId),
                new NpgsqlParameter("@ReportSummary", updates.ReportSummary),
                new NpgsqlParameter("@Recommendations", (object)updates.Recommendations ?? DBNull.Value),
                new NpgsqlParameter("@SupervisorNotes", (object)updates.SupervisorNotes ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_UpdatePostEventReport", parameters);
        }

        public async Task<bool> VerifyPostEventReportAsync(long reportId, long verifiedBy, string verificationNotes)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@ReportId", reportId),
                new NpgsqlParameter("@VerifiedBy", verifiedBy),
                new NpgsqlParameter("@VerificationNotes", (object)verificationNotes ?? DBNull.Value)
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
                new NpgsqlParameter("@AssignmentId", assignmentId)
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
                new NpgsqlParameter("@AssignmentId", assignmentId),
                new NpgsqlParameter("@OTPPurpose", purpose),
                new NpgsqlParameter("@NewOTPCode", NpgsqlDbType.Varchar, 10) { Direction = ParameterDirection.Output }
            };

            await _dbHelper.ExecuteStoredProcedureAsync<object>("sp_ResendClientOTP", parameters);

            return parameters[2].Value?.ToString();
        }

        public async Task<ClientOTPVerificationModel> GetOTPVerificationStatusAsync(string otpCode)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@OTPCode", otpCode)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<ClientOTPVerificationModel>(
                "sp_GetOTPVerificationStatus", parameters);
        }

        // =============================================
        // EVIDENCE & DOCUMENTATION
        // =============================================

        /// <summary>
        /// Validate photo upload requirements before allowing submission.
        /// Business Rule: Minimum 3 photos per phase, GPS mandatory, timestamps validated.
        /// </summary>
        private bool ValidatePhotoRequirements(List<TimestampedEvidence> evidence, string phase)
        {
            if (evidence == null || evidence.Count == 0)
            {
                throw new InvalidOperationException($"No evidence provided for {phase}");
            }

            var photoCount = evidence.Count(e => e.Type == "PHOTO");

            // Minimum requirements based on phase
            int minimumPhotos = phase switch
            {
                "PRE_EVENT" => 3,    // Menu, raw materials, setup
                "DURING_EVENT" => 3, // Food serving, guest crowd, ambiance
                "POST_EVENT" => 3,   // Cleanup, leftover management, final state
                _ => 3
            };

            if (photoCount < minimumPhotos)
            {
                throw new InvalidOperationException(
                    $"{phase} requires minimum {minimumPhotos} photos. Provided: {photoCount}");
            }

            // Validate GPS location is present
            var missingGPS = evidence.Where(e => string.IsNullOrWhiteSpace(e.GPSLocation)).ToList();
            if (missingGPS.Any())
            {
                throw new InvalidOperationException(
                    $"{missingGPS.Count} evidence item(s) missing GPS location. GPS is mandatory for all uploads.");
            }

            // Validate timestamp is within reasonable range (not future, not too old)
            var now = DateTime.UtcNow;
            var invalidTimestamps = evidence.Where(e =>
                e.Timestamp > now.AddMinutes(5) ||   // Not more than 5 minutes in future (clock skew)
                e.Timestamp < now.AddDays(-7)         // Not older than 7 days
            ).ToList();

            if (invalidTimestamps.Any())
            {
                throw new InvalidOperationException(
                    $"{invalidTimestamps.Count} evidence item(s) have invalid timestamps. " +
                    "Timestamps must be recent and not in the future.");
            }

            return true;
        }

        public async Task<bool> UploadTimestampedEvidenceAsync(long assignmentId, List<TimestampedEvidence> evidence, string phase)
        {
            // Validate photo requirements before upload
            ValidatePhotoRequirements(evidence, phase);

            var parameters = new[]
            {
                new NpgsqlParameter("@AssignmentId", assignmentId),
                new NpgsqlParameter("@EvidenceData", JsonSerializer.Serialize(evidence)),
                new NpgsqlParameter("@Phase", phase) // PRE_EVENT, DURING_EVENT, POST_EVENT
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_UploadTimestampedEvidence", parameters);
        }

        public async Task<Dictionary<string, List<TimestampedEvidence>>> GetAssignmentEvidenceAsync(long assignmentId)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@AssignmentId", assignmentId)
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
