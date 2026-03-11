using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Supervisor;
using CateringEcommerce.Domain.Models.Supervisor;

namespace CateringEcommerce.BAL.Base.Supervisor
{
    public class RegistrationRepository : IRegistrationRepository
    {
        private readonly IDatabaseHelper _dbHelper;

        public RegistrationRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        // =============================================
        // DOCUMENT MANAGEMENT
        // =============================================

        public async Task<bool> SubmitIdentityProofDocumentsAsync(long registrationId, string idProofUrl, string addressProofUrl, string photoUrl, string cancelledChequeUrl)
        {
            var parameters = new[]
            {
                new SqlParameter("@RegistrationId", registrationId),
                new SqlParameter("@IDProofUrl", (object)idProofUrl ?? DBNull.Value),
                new SqlParameter("@AddressProofUrl", (object)addressProofUrl ?? DBNull.Value),
                new SqlParameter("@CancelledChequeUrl", (object)cancelledChequeUrl ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_SubmitIdentityProofDocuments", parameters);
        }

        // =============================================
        // REGISTRATION SUBMISSION
        // =============================================

        public async Task<long> SubmitRegistrationAsync(SupervisorRegistrationSubmitDto registration)
        {
            var registrationIdParam = new SqlParameter("@RegistrationId", SqlDbType.BigInt) 
            { 
                Direction = ParameterDirection.Output 
            };

            var parameters = new[]
            {
                new SqlParameter("@FirstName", registration.FirstName),
                new SqlParameter("@LastName", registration.LastName),
                new SqlParameter("@Email", registration.Email),
                new SqlParameter("@Phone", registration.Phone),
                new SqlParameter("@Address", registration.Address),
                new SqlParameter("@Pincode", registration.Pincode),
                new SqlParameter("@StateID", registration.StateID),
                new SqlParameter("@CityID", registration.CityID),
                new SqlParameter("@DateOfBirth", registration.DateOfBirth),
                new SqlParameter("@IDProofType", registration.IDProofType),
                new SqlParameter("@IDProofNumber", registration.IDProofNumber),
                new SqlParameter("@HasPriorExperience", registration.HasPriorExperience),
                new SqlParameter("@PriorExperienceDetails", (object)registration.PriorExperienceDetails ?? DBNull.Value),
                registrationIdParam
            };

            await _dbHelper.ExecuteStoredProcedureAsync<object>("sp_SubmitSupervisorRegistration", parameters);

            return registrationIdParam.Value != DBNull.Value ? Convert.ToInt64(registrationIdParam.Value) : 0;
        }

        public async Task<SupervisorRegistrationModel> GetRegistrationByIdAsync(long registrationId)
        {
            var parameters = new[]
            {
                new SqlParameter("@RegistrationId", registrationId)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<SupervisorRegistrationModel>(
                "sp_GetRegistrationById", parameters);
        }

        public async Task<SupervisorRegistrationModel> GetRegistrationBySupervisorIdAsync(long supervisorId)
        {
            var parameters = new[]
            {
                new SqlParameter("@SupervisorId", supervisorId)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<SupervisorRegistrationModel>(
                "sp_GetRegistrationBySupervisorId", parameters);
        }

        // =============================================
        // STAGE PROGRESSION
        // =============================================

        public async Task<bool> ProgressRegistrationStageAsync(long registrationId, string nextStage, long processedBy, string notes)
        {
            var parameters = new[]
            {
                new SqlParameter("@RegistrationId", registrationId),
                new SqlParameter("@NextStage", nextStage),
                new SqlParameter("@ProcessedBy", processedBy),
                new SqlParameter("@Notes", (object)notes ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_ProgressRegistrationStatus", parameters);
        }

        public async Task<bool> RejectRegistrationAsync(long registrationId, long rejectedBy, string reason)
        {
            var parameters = new[]
            {
                new SqlParameter("@RegistrationId", registrationId),
                new SqlParameter("@RejectedBy", rejectedBy),
                new SqlParameter("@Reason", reason)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_RejectRegistration", parameters);
        }

        // =============================================
        // STAGE 1: DOCUMENT VERIFICATION
        // =============================================

        public async Task<bool> SubmitDocumentVerificationAsync(DocumentVerificationDto verification)
        {
            var parameters = new[]
            {
                new SqlParameter("@RegistrationId", verification.RegistrationId),
                new SqlParameter("@VerifiedBy", verification.VerifiedBy),
                new SqlParameter("@Passed", verification.Passed),
                new SqlParameter("@IDProofVerified", verification.IDProofVerified),
                new SqlParameter("@AddressProofVerified", verification.AddressProofVerified),
                new SqlParameter("@PhotoVerified", verification.PhotoVerified),
                new SqlParameter("@VerificationNotes", (object)verification.VerificationNotes ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_SubmitDocumentVerification", parameters);
        }

        public async Task<List<SupervisorRegistrationModel>> GetRegistrationsPendingDocumentVerificationAsync()
        {
            return await _dbHelper.ExecuteStoredProcedureAsync<List<SupervisorRegistrationModel>>(
                "sp_GetRegistrationsPendingDocumentVerification", null);
        }

        // =============================================
        // STAGE 2: INTERVIEW
        // =============================================

        public async Task<bool> ScheduleQuickInterviewAsync(QuickInterviewDto interview)
        {
            var parameters = new[]
            {
                new SqlParameter("@RegistrationId", interview.RegistrationId),
                new SqlParameter("@InterviewDateTime", interview.InterviewDateTime),
                new SqlParameter("@InterviewType", interview.InterviewType),
                new SqlParameter("@InterviewerName", interview.InterviewerName),
                new SqlParameter("@MeetingLink", (object)interview.MeetingLink ?? DBNull.Value),
                new SqlParameter("@ScheduledBy", interview.ScheduledBy)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_ScheduleQuickInterview", parameters);
        }

        public async Task<bool> SubmitQuickInterviewResultAsync(QuickInterviewResultDto result)
        {
            var parameters = new[]
            {
                new SqlParameter("@RegistrationId", result.RegistrationId),
                new SqlParameter("@InterviewedBy", result.InterviewedBy),
                new SqlParameter("@Passed", result.Passed),
                new SqlParameter("@Score", result.Score),
                new SqlParameter("@Notes", (object)result.Notes ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_SubmitQuickInterviewResult", parameters);
        }

        public async Task<List<SupervisorRegistrationModel>> GetRegistrationsPendingInterviewAsync()
        {
            return await _dbHelper.ExecuteStoredProcedureAsync<List<SupervisorRegistrationModel>>(
                "sp_GetRegistrationsPendingInterview", null);
        }

        // =============================================
        // STAGE 3: TRAINING
        // =============================================

        public async Task<bool> AssignCondensedTrainingAsync(long registrationId, List<long> moduleIds, long assignedBy)
        {
            var parameters = new[]
            {
                new SqlParameter("@RegistrationId", registrationId),
                new SqlParameter("@ModuleIds", string.Join(",", moduleIds)),
                new SqlParameter("@AssignedBy", assignedBy)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_AssignCondensedTraining", parameters);
        }

        public async Task<bool> CompleteCondensedTrainingAsync(long registrationId, long completedBy)
        {
            var parameters = new[]
            {
                new SqlParameter("@RegistrationId", registrationId),
                new SqlParameter("@CompletedBy", completedBy)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_CompleteCondensedTraining", parameters);
        }

        public async Task<List<SupervisorRegistrationModel>> GetRegistrationsPendingTrainingAsync()
        {
            return await _dbHelper.ExecuteStoredProcedureAsync<List<SupervisorRegistrationModel>>(
                "sp_GetRegistrationsPendingTraining", null);
        }

        // =============================================
        // STAGE 4: CERTIFICATION
        // =============================================

        public async Task<bool> ScheduleQuickCertificationAsync(long registrationId, DateTime examDate, long scheduledBy)
        {
            var parameters = new[]
            {
                new SqlParameter("@RegistrationId", registrationId),
                new SqlParameter("@ExamDate", examDate),
                new SqlParameter("@ScheduledBy", scheduledBy)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_ScheduleQuickCertification", parameters);
        }

        public async Task<bool> SubmitQuickCertificationResultAsync(QuickCertificationResultDto result)
        {
            var parameters = new[]
            {
                new SqlParameter("@RegistrationId", result.RegistrationId),
                new SqlParameter("@Passed", result.Passed),
                new SqlParameter("@ExamScore", result.ExamScore),
                new SqlParameter("@ExamDate", result.ExamDate),
                new SqlParameter("@CertificateNumber", (object)result.CertificateNumber ?? DBNull.Value),
                new SqlParameter("@EvaluatedBy", result.EvaluatedBy)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_SubmitQuickCertificationResult", parameters);
        }

        public async Task<List<SupervisorRegistrationModel>> GetRegistrationsPendingCertificationAsync()
        {
            return await _dbHelper.ExecuteStoredProcedureAsync<List<SupervisorRegistrationModel>>(
                "sp_GetRegistrationsPendingCertification", null);
        }

        // =============================================
        // BANKING DETAILS
        // =============================================

        public async Task<bool> SubmitBankingDetailsAsync(BankingDetailsDto banking)
        {
            var parameters = new[]
            {
                new SqlParameter("@SupervisorId", banking.SupervisorId),
                new SqlParameter("@AccountHolderName", banking.AccountHolderName),
                new SqlParameter("@BankName", banking.BankName),
                new SqlParameter("@AccountNumber", banking.AccountNumber),
                new SqlParameter("@IFSCCode", banking.IFSCCode),
                new SqlParameter("@BranchName", banking.BranchName),
                new SqlParameter("@AccountType", banking.AccountType),
                new SqlParameter("@CancelledChequeUrl", (object)banking.CancelledChequeUrl ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_SubmitBankingDetails", parameters);
        }

        public async Task<BankingDetailsModel> GetBankingDetailsAsync(long supervisorId)
        {
            var parameters = new[]
            {
                new SqlParameter("@SupervisorId", supervisorId)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<BankingDetailsModel>(
                "sp_GetBankingDetails", parameters);
        }

        // =============================================
        // FINAL ACTIVATION
        // =============================================

        public async Task<bool> ActivateRegisteredSupervisorAsync(long registrationId, long activatedBy)
        {
            var parameters = new[]
            {
                new SqlParameter("@RegistrationId", registrationId),
                new SqlParameter("@ActivatedBy", activatedBy)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_ActivateRegisteredSupervisor", parameters);
        }

        // =============================================
        // WORKFLOW TRACKING
        // =============================================

        public async Task<List<RegistrationProgressDto>> GetRegistrationProgressAsync(long registrationId)
        {
            var parameters = new[]
            {
                new SqlParameter("@RegistrationId", registrationId)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<List<RegistrationProgressDto>>(
                "sp_GetRegistrationProgress", parameters);
        }

        public async Task<RegistrationWorkflowStatusDto> GetWorkflowStatusAsync(long registrationId)
        {
            var parameters = new[]
            {
                new SqlParameter("@RegistrationId", registrationId)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<RegistrationWorkflowStatusDto>(
                "sp_GetRegistrationWorkflowStatus", parameters);
        }

        // =============================================
        // ADMIN QUERIES
        // =============================================

        public async Task<List<SupervisorRegistrationModel>> GetAllRegistrationsAsync(string status = null)
        {
            var parameters = new[]
            {
                new SqlParameter("@Status", (object)status ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<List<SupervisorRegistrationModel>>(
                "sp_GetAllRegistrations", parameters);
        }

        public async Task<List<SupervisorRegistrationModel>> GetRegistrationsByStageAsync(string stage)
        {
            var parameters = new[]
            {
                new SqlParameter("@Stage", stage)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<List<SupervisorRegistrationModel>>(
                "sp_GetRegistrationsByStage", parameters);
        }

        public async Task<RegistrationStatisticsDto> GetRegistrationStatisticsAsync()
        {
            return await _dbHelper.ExecuteStoredProcedureAsync<RegistrationStatisticsDto>(
                "sp_GetRegistrationStatistics", null);
        }

        public async Task<List<SupervisorRegistrationModel>> SearchRegistrationsAsync(RegistrationSearchDto filters)
        {
            var parameters = new[]
            {
                new SqlParameter("@Name", (object)filters.Name ?? DBNull.Value),
                new SqlParameter("@Email", (object)filters.Email ?? DBNull.Value),
                new SqlParameter("@Phone", (object)filters.Phone ?? DBNull.Value),
                new SqlParameter("@Status", (object)filters.Status ?? DBNull.Value),
                new SqlParameter("@CurrentStage", (object)filters.CurrentStage ?? DBNull.Value),
                new SqlParameter("@ZoneId", (object)filters.ZoneId ?? DBNull.Value),
                new SqlParameter("@RegisteredFrom", (object)filters.RegisteredFrom ?? DBNull.Value),
                new SqlParameter("@RegisteredTo", (object)filters.RegisteredTo ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<List<SupervisorRegistrationModel>>(
                "sp_SearchRegistrations", parameters);
        }
    }
}
