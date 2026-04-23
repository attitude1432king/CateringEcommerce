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
    public class CareersApplicationRepository : ICareersApplicationRepository
    {
        private readonly IDatabaseHelper _dbHelper;

        public CareersApplicationRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        // =============================================
        // APPLICATION SUBMISSION
        // =============================================

        public async Task<long> SubmitCareersApplicationAsync(CareersApplicationSubmitDto application)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@FirstName", application.FirstName),
                new NpgsqlParameter("@LastName", application.LastName),
                new NpgsqlParameter("@Email", application.Email),
                new NpgsqlParameter("@Phone", application.Phone),
                new NpgsqlParameter("@Address", application.Address),
                new NpgsqlParameter("@DateOfBirth", application.DateOfBirth),
                new NpgsqlParameter("@ResumeUrl", application.ResumeUrl),
                new NpgsqlParameter("@CoverLetter", (object)application.CoverLetter ?? DBNull.Value),
                new NpgsqlParameter("@YearsOfExperience", application.YearsOfExperience),
                new NpgsqlParameter("@PreviousEmployer", (object)application.PreviousEmployer ?? DBNull.Value),
                new NpgsqlParameter("@References", (object)application.References ?? DBNull.Value),
                new NpgsqlParameter("@ApplicationId", NpgsqlDbType.Bigint) { Direction = ParameterDirection.Output }
            };

            await _dbHelper.ExecuteStoredProcedureAsync<object>("sp_SubmitCareersApplication", parameters);

            return parameters[11].Value != DBNull.Value ? Convert.ToInt64(parameters[11].Value) : 0;
        }

        public async Task<CareersApplicationModel> GetApplicationByIdAsync(long applicationId)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@ApplicationId", applicationId)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<CareersApplicationModel>(
                "sp_GetCareersApplicationById", parameters);
        }

        public async Task<CareersApplicationModel> GetApplicationBySupervisorIdAsync(long supervisorId)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@SupervisorId", supervisorId)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<CareersApplicationModel>(
                "sp_GetCareersApplicationBySupervisorId", parameters);
        }

        // =============================================
        // STAGE PROGRESSION
        // =============================================

        public async Task<bool> ProgressApplicationStageAsync(long applicationId, string nextStage, long processedBy, string notes)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@ApplicationId", applicationId),
                new NpgsqlParameter("@NextStage", nextStage),
                new NpgsqlParameter("@ProcessedBy", processedBy),
                new NpgsqlParameter("@Notes", (object)notes ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_ProgressCareersApplication", parameters);
        }

        public async Task<bool> RejectApplicationAsync(long applicationId, long rejectedBy, string reason)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@ApplicationId", applicationId),
                new NpgsqlParameter("@RejectedBy", rejectedBy),
                new NpgsqlParameter("@Reason", reason)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_RejectCareersApplication", parameters);
        }

        // =============================================
        // STAGE 2: RESUME SCREENING
        // =============================================

        public async Task<bool> SubmitResumeScreeningAsync(ResumeScreeningDto screening)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@ApplicationId", screening.ApplicationId),
                new NpgsqlParameter("@ScreenedBy", screening.ScreenedBy),
                new NpgsqlParameter("@Passed", screening.Passed),
                new NpgsqlParameter("@ResumeScore", screening.ResumeScore),
                new NpgsqlParameter("@ScreeningNotes", (object)screening.ScreeningNotes ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_SubmitResumeScreening", parameters);
        }

        public async Task<List<CareersApplicationModel>> GetApplicationsForResumeScreeningAsync()
        {
            return await _dbHelper.ExecuteStoredProcedureAsync<List<CareersApplicationModel>>(
                "sp_GetApplicationsForResumeScreening", null);
        }

        // =============================================
        // STAGE 3: INTERVIEW
        // =============================================

        public async Task<bool> ScheduleInterviewAsync(ScheduleInterviewDto interview)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@ApplicationId", interview.ApplicationId),
                new NpgsqlParameter("@InterviewDateTime", interview.InterviewDateTime),
                new NpgsqlParameter("@InterviewType", interview.InterviewType),
                new NpgsqlParameter("@InterviewerName", interview.InterviewerName),
                new NpgsqlParameter("@MeetingLink", (object)interview.MeetingLink ?? DBNull.Value),
                new NpgsqlParameter("@ScheduledBy", interview.ScheduledBy)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_ScheduleInterview", parameters);
        }

        public async Task<bool> SubmitInterviewResultAsync(InterviewResultDto result)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@ApplicationId", result.ApplicationId),
                new NpgsqlParameter("@InterviewedBy", result.InterviewedBy),
                new NpgsqlParameter("@Passed", result.Passed),
                new NpgsqlParameter("@InterviewScore", result.InterviewScore),
                new NpgsqlParameter("@InterviewNotes", (object)result.InterviewNotes ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_SubmitInterviewResult", parameters);
        }

        public async Task<List<CareersApplicationModel>> GetApplicationsForInterviewAsync()
        {
            return await _dbHelper.ExecuteStoredProcedureAsync<List<CareersApplicationModel>>(
                "sp_GetApplicationsForInterview", null);
        }

        // =============================================
        // STAGE 4: BACKGROUND VERIFICATION
        // =============================================

        public async Task<bool> InitiateBackgroundCheckAsync(long applicationId, long initiatedBy)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@ApplicationId", applicationId),
                new NpgsqlParameter("@InitiatedBy", initiatedBy)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_InitiateBackgroundCheck", parameters);
        }

        public async Task<bool> SubmitBackgroundCheckResultAsync(BackgroundCheckResultDto result)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@ApplicationId", result.ApplicationId),
                new NpgsqlParameter("@Passed", result.Passed),
                new NpgsqlParameter("@VerificationAgency", result.VerificationAgency),
                new NpgsqlParameter("@VerificationDate", result.VerificationDate),
                new NpgsqlParameter("@VerificationReportUrl", (object)result.VerificationReportUrl ?? DBNull.Value),
                new NpgsqlParameter("@Notes", (object)result.Notes ?? DBNull.Value),
                new NpgsqlParameter("@SubmittedBy", result.SubmittedBy)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_SubmitBackgroundCheckResult", parameters);
        }

        public async Task<List<CareersApplicationModel>> GetApplicationsPendingBackgroundCheckAsync()
        {
            return await _dbHelper.ExecuteStoredProcedureAsync<List<CareersApplicationModel>>(
                "sp_GetApplicationsPendingBackgroundCheck", null);
        }

        // =============================================
        // STAGE 5: TRAINING
        // =============================================

        public async Task<bool> AssignTrainingAsync(long applicationId, List<long> moduleIds, long assignedBy)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@ApplicationId", applicationId),
                new NpgsqlParameter("@ModuleIds", string.Join(",", moduleIds)),
                new NpgsqlParameter("@AssignedBy", assignedBy)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_AssignTraining", parameters);
        }

        public async Task<bool> RecordTrainingProgressAsync(long applicationId, long moduleId, int progressPercentage)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@ApplicationId", applicationId),
                new NpgsqlParameter("@ModuleId", moduleId),
                new NpgsqlParameter("@ProgressPercentage", progressPercentage)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_RecordTrainingProgress", parameters);
        }

        public async Task<bool> CompleteTrainingAsync(long applicationId, long completedBy)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@ApplicationId", applicationId),
                new NpgsqlParameter("@CompletedBy", completedBy)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_CompleteTraining", parameters);
        }

        public async Task<List<CareersApplicationModel>> GetApplicationsInTrainingAsync()
        {
            return await _dbHelper.ExecuteStoredProcedureAsync<List<CareersApplicationModel>>(
                "sp_GetApplicationsInTraining", null);
        }

        // =============================================
        // STAGE 6: CERTIFICATION
        // =============================================

        public async Task<bool> ScheduleCertificationExamAsync(long applicationId, DateTime examDate, long scheduledBy)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@ApplicationId", applicationId),
                new NpgsqlParameter("@ExamDate", examDate),
                new NpgsqlParameter("@ScheduledBy", scheduledBy)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_ScheduleCertificationExam", parameters);
        }

        public async Task<bool> SubmitCertificationResultAsync(CertificationResultDto result)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@ApplicationId", result.ApplicationId),
                new NpgsqlParameter("@Passed", result.Passed),
                new NpgsqlParameter("@ExamScore", result.ExamScore),
                new NpgsqlParameter("@ExamDate", result.ExamDate),
                new NpgsqlParameter("@CertificateNumber", (object)result.CertificateNumber ?? DBNull.Value),
                new NpgsqlParameter("@CertificateUrl", (object)result.CertificateUrl ?? DBNull.Value),
                new NpgsqlParameter("@EvaluatedBy", result.EvaluatedBy)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_SubmitCertificationResult", parameters);
        }

        public async Task<List<CareersApplicationModel>> GetApplicationsPendingCertificationAsync()
        {
            return await _dbHelper.ExecuteStoredProcedureAsync<List<CareersApplicationModel>>(
                "sp_GetApplicationsPendingCertification", null);
        }

        // =============================================
        // STAGE 7: PROBATION
        // =============================================

        public async Task<bool> StartProbationAsync(long applicationId, int probationDays, long startedBy)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@ApplicationId", applicationId),
                new NpgsqlParameter("@ProbationDays", probationDays),
                new NpgsqlParameter("@StartedBy", startedBy)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_StartProbation", parameters);
        }

        public async Task<bool> CompleteProbationAsync(long applicationId, bool passed, long evaluatedBy, string evaluation)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@ApplicationId", applicationId),
                new NpgsqlParameter("@Passed", passed),
                new NpgsqlParameter("@EvaluatedBy", evaluatedBy),
                new NpgsqlParameter("@Evaluation", (object)evaluation ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_CompleteProbation", parameters);
        }

        public async Task<List<CareersApplicationModel>> GetApplicationsInProbationAsync()
        {
            return await _dbHelper.ExecuteStoredProcedureAsync<List<CareersApplicationModel>>(
                "sp_GetApplicationsInProbation", null);
        }

        // =============================================
        // FINAL ACTIVATION
        // =============================================

        public async Task<bool> ActivateSupervisorAsync(long applicationId, long activatedBy)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@ApplicationId", applicationId),
                new NpgsqlParameter("@ActivatedBy", activatedBy)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_ActivateCareerSupervisor", parameters);
        }

        // =============================================
        // WORKFLOW TRACKING
        // =============================================

        public async Task<List<ApplicationProgressDto>> GetApplicationProgressAsync(long applicationId)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@ApplicationId", applicationId)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<List<ApplicationProgressDto>>(
                "sp_GetApplicationProgress", parameters);
        }

        public async Task<ApplicationWorkflowStatusDto> GetWorkflowStatusAsync(long applicationId)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@ApplicationId", applicationId)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<ApplicationWorkflowStatusDto>(
                "sp_GetApplicationWorkflowStatus", parameters);
        }

        // =============================================
        // ADMIN QUERIES
        // =============================================

        public async Task<List<CareersApplicationModel>> GetAllApplicationsAsync(string status = null)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@Status", (object)status ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<List<CareersApplicationModel>>(
                "sp_GetAllCareersApplications", parameters);
        }

        public async Task<List<CareersApplicationModel>> GetApplicationsByStageAsync(string stage)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@Stage", stage)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<List<CareersApplicationModel>>(
                "sp_GetApplicationsByStage", parameters);
        }

        public async Task<ApplicationStatisticsDto> GetApplicationStatisticsAsync()
        {
            return await _dbHelper.ExecuteStoredProcedureAsync<ApplicationStatisticsDto>(
                "sp_GetApplicationStatistics", null);
        }

        public async Task<List<CareersApplicationModel>> SearchApplicationsAsync(ApplicationSearchDto filters)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@Name", (object)filters.Name ?? DBNull.Value),
                new NpgsqlParameter("@Email", (object)filters.Email ?? DBNull.Value),
                new NpgsqlParameter("@Phone", (object)filters.Phone ?? DBNull.Value),
                new NpgsqlParameter("@Status", (object)filters.Status ?? DBNull.Value),
                new NpgsqlParameter("@CurrentStage", (object)filters.CurrentStage ?? DBNull.Value),
                new NpgsqlParameter("@AppliedFrom", (object)filters.AppliedFrom ?? DBNull.Value),
                new NpgsqlParameter("@AppliedTo", (object)filters.AppliedTo ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<List<CareersApplicationModel>>(
                "sp_SearchCareersApplications", parameters);
        }
    }
}
