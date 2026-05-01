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
                new NpgsqlParameter("@RegistrationId", registrationId),
                new NpgsqlParameter("@IDProofUrl", (object)idProofUrl ?? DBNull.Value),
                new NpgsqlParameter("@AddressProofUrl", (object)addressProofUrl ?? DBNull.Value),
                new NpgsqlParameter("@CancelledChequeUrl", (object)cancelledChequeUrl ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_SubmitIdentityProofDocuments", parameters);
        }

        // =============================================
        // REGISTRATION SUBMISSION
        // =============================================

        public async Task<long> SubmitRegistrationAsync(SupervisorRegistrationSubmitDto registration)
        {
            var registrationIdParam = new NpgsqlParameter("@RegistrationId", NpgsqlDbType.Bigint) 
            { 
                Direction = ParameterDirection.Output 
            };

            var parameters = new[]
            {
                new NpgsqlParameter("@FirstName", registration.FirstName),
                new NpgsqlParameter("@LastName", registration.LastName),
                new NpgsqlParameter("@Email", registration.Email),
                new NpgsqlParameter("@Phone", registration.Phone),
                new NpgsqlParameter("@Address", registration.Address),
                new NpgsqlParameter("@Pincode", registration.Pincode),
                new NpgsqlParameter("@StateID", registration.StateID),
                new NpgsqlParameter("@CityID", registration.CityID),
                new NpgsqlParameter("@DateOfBirth", registration.DateOfBirth),
                new NpgsqlParameter("@IDProofType", registration.IDProofType),
                new NpgsqlParameter("@IDProofNumber", registration.IDProofNumber),
                new NpgsqlParameter("@HasPriorExperience", registration.HasPriorExperience),
                new NpgsqlParameter("@PriorExperienceDetails", (object)registration.PriorExperienceDetails ?? DBNull.Value),
                registrationIdParam
            };

            await _dbHelper.ExecuteStoredProcedureAsync<object>("sp_SubmitSupervisorRegistration", parameters);

            return registrationIdParam.Value != DBNull.Value ? Convert.ToInt64(registrationIdParam.Value) : 0;
        }

        public async Task<SupervisorRegistrationModel> GetRegistrationByIdAsync(long registrationId)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@RegistrationId", registrationId)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<SupervisorRegistrationModel>(
                "sp_GetRegistrationById", parameters);
        }

        public async Task<SupervisorRegistrationModel> GetRegistrationBySupervisorIdAsync(long supervisorId)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@SupervisorId", supervisorId)
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
                new NpgsqlParameter("@RegistrationId", registrationId),
                new NpgsqlParameter("@NextStage", nextStage),
                new NpgsqlParameter("@ProcessedBy", processedBy),
                new NpgsqlParameter("@Notes", (object)notes ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_ProgressRegistrationStatus", parameters);
        }

        public async Task<bool> RejectRegistrationAsync(long registrationId, long rejectedBy, string reason)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@RegistrationId", registrationId),
                new NpgsqlParameter("@RejectedBy", rejectedBy),
                new NpgsqlParameter("@Reason", reason)
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
                new NpgsqlParameter("@RegistrationId", verification.RegistrationId),
                new NpgsqlParameter("@VerifiedBy", verification.VerifiedBy),
                new NpgsqlParameter("@Passed", verification.Passed),
                new NpgsqlParameter("@IDProofVerified", verification.IDProofVerified),
                new NpgsqlParameter("@AddressProofVerified", verification.AddressProofVerified),
                new NpgsqlParameter("@PhotoVerified", verification.PhotoVerified),
                new NpgsqlParameter("@VerificationNotes", (object)verification.VerificationNotes ?? DBNull.Value)
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
                new NpgsqlParameter("@RegistrationId", interview.RegistrationId),
                new NpgsqlParameter("@InterviewDateTime", interview.InterviewDateTime),
                new NpgsqlParameter("@InterviewType", interview.InterviewType),
                new NpgsqlParameter("@InterviewerName", interview.InterviewerName),
                new NpgsqlParameter("@MeetingLink", (object)interview.MeetingLink ?? DBNull.Value),
                new NpgsqlParameter("@ScheduledBy", interview.ScheduledBy)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_ScheduleQuickInterview", parameters);
        }

        public async Task<bool> SubmitQuickInterviewResultAsync(QuickInterviewResultDto result)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@RegistrationId", result.RegistrationId),
                new NpgsqlParameter("@InterviewedBy", result.InterviewedBy),
                new NpgsqlParameter("@Passed", result.Passed),
                new NpgsqlParameter("@Score", result.Score),
                new NpgsqlParameter("@Notes", (object)result.Notes ?? DBNull.Value)
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
                new NpgsqlParameter("@RegistrationId", registrationId),
                new NpgsqlParameter("@ModuleIds", string.Join(",", moduleIds)),
                new NpgsqlParameter("@AssignedBy", assignedBy)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_AssignCondensedTraining", parameters);
        }

        public async Task<bool> CompleteCondensedTrainingAsync(long registrationId, long completedBy)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@RegistrationId", registrationId),
                new NpgsqlParameter("@CompletedBy", completedBy)
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
                new NpgsqlParameter("@RegistrationId", registrationId),
                new NpgsqlParameter("@ExamDate", examDate),
                new NpgsqlParameter("@ScheduledBy", scheduledBy)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_ScheduleQuickCertification", parameters);
        }

        public async Task<bool> SubmitQuickCertificationResultAsync(QuickCertificationResultDto result)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@RegistrationId", result.RegistrationId),
                new NpgsqlParameter("@Passed", result.Passed),
                new NpgsqlParameter("@ExamScore", result.ExamScore),
                new NpgsqlParameter("@ExamDate", result.ExamDate),
                new NpgsqlParameter("@CertificateNumber", (object)result.CertificateNumber ?? DBNull.Value),
                new NpgsqlParameter("@EvaluatedBy", result.EvaluatedBy)
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
                new NpgsqlParameter("@SupervisorId", banking.SupervisorId),
                new NpgsqlParameter("@AccountHolderName", banking.AccountHolderName),
                new NpgsqlParameter("@BankName", banking.BankName),
                new NpgsqlParameter("@AccountNumber", banking.AccountNumber),
                new NpgsqlParameter("@IFSCCode", banking.IFSCCode),
                new NpgsqlParameter("@BranchName", banking.BranchName),
                new NpgsqlParameter("@AccountType", banking.AccountType),
                new NpgsqlParameter("@CancelledChequeUrl", (object)banking.CancelledChequeUrl ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_SubmitBankingDetails", parameters);
        }

        public async Task<BankingDetailsModel> GetBankingDetailsAsync(long supervisorId)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@SupervisorId", supervisorId)
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
                new NpgsqlParameter("@RegistrationId", registrationId),
                new NpgsqlParameter("@ActivatedBy", activatedBy)
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
                new NpgsqlParameter("@RegistrationId", registrationId)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<List<RegistrationProgressDto>>(
                "sp_GetRegistrationProgress", parameters);
        }

        public async Task<RegistrationWorkflowStatusDto> GetWorkflowStatusAsync(long registrationId)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@RegistrationId", registrationId)
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
                new NpgsqlParameter("@Status", (object)status ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<List<SupervisorRegistrationModel>>(
                "sp_GetAllRegistrations", parameters);
        }

        public async Task<List<SupervisorRegistrationModel>> GetRegistrationsByStageAsync(string stage)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@Stage", stage)
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
                new NpgsqlParameter("@Name", (object)filters.Name ?? DBNull.Value),
                new NpgsqlParameter("@Email", (object)filters.Email ?? DBNull.Value),
                new NpgsqlParameter("@Phone", (object)filters.Phone ?? DBNull.Value),
                new NpgsqlParameter("@Status", (object)filters.Status ?? DBNull.Value),
                new NpgsqlParameter("@CurrentStage", (object)filters.CurrentStage ?? DBNull.Value),
                new NpgsqlParameter("@ZoneId", (object)filters.ZoneId ?? DBNull.Value),
                new NpgsqlParameter("@RegisteredFrom", (object)filters.RegisteredFrom ?? DBNull.Value),
                new NpgsqlParameter("@RegisteredTo", (object)filters.RegisteredTo ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<List<SupervisorRegistrationModel>>(
                "sp_SearchRegistrations", parameters);
        }
    }
}
