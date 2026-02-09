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
                new SqlParameter("@FirstName", application.FirstName),
                new SqlParameter("@LastName", application.LastName),
                new SqlParameter("@Email", application.Email),
                new SqlParameter("@Phone", application.Phone),
                new SqlParameter("@Address", application.Address),
                new SqlParameter("@DateOfBirth", application.DateOfBirth),
                new SqlParameter("@ResumeUrl", application.ResumeUrl),
                new SqlParameter("@CoverLetter", (object)application.CoverLetter ?? DBNull.Value),
                new SqlParameter("@YearsOfExperience", application.YearsOfExperience),
                new SqlParameter("@PreviousEmployer", (object)application.PreviousEmployer ?? DBNull.Value),
                new SqlParameter("@References", (object)application.References ?? DBNull.Value),
                new SqlParameter("@ApplicationId", SqlDbType.BigInt) { Direction = ParameterDirection.Output }
            };

            await _dbHelper.ExecuteStoredProcedureAsync<object>("sp_SubmitCareersApplication", parameters);

            return parameters[11].Value != DBNull.Value ? Convert.ToInt64(parameters[11].Value) : 0;
        }

        public async Task<CareersApplicationModel> GetApplicationByIdAsync(long applicationId)
        {
            var parameters = new[]
            {
                new SqlParameter("@ApplicationId", applicationId)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<CareersApplicationModel>(
                "sp_GetCareersApplicationById", parameters);
        }

        public async Task<CareersApplicationModel> GetApplicationBySupervisorIdAsync(long supervisorId)
        {
            var parameters = new[]
            {
                new SqlParameter("@SupervisorId", supervisorId)
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
                new SqlParameter("@ApplicationId", applicationId),
                new SqlParameter("@NextStage", nextStage),
                new SqlParameter("@ProcessedBy", processedBy),
                new SqlParameter("@Notes", (object)notes ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_ProgressCareersApplication", parameters);
        }

        public async Task<bool> RejectApplicationAsync(long applicationId, long rejectedBy, string reason)
        {
            var parameters = new[]
            {
                new SqlParameter("@ApplicationId", applicationId),
                new SqlParameter("@RejectedBy", rejectedBy),
                new SqlParameter("@Reason", reason)
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
                new SqlParameter("@ApplicationId", screening.ApplicationId),
                new SqlParameter("@ScreenedBy", screening.ScreenedBy),
                new SqlParameter("@Passed", screening.Passed),
                new SqlParameter("@ResumeScore", screening.ResumeScore),
                new SqlParameter("@ScreeningNotes", (object)screening.ScreeningNotes ?? DBNull.Value)
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
                new SqlParameter("@ApplicationId", interview.ApplicationId),
                new SqlParameter("@InterviewDateTime", interview.InterviewDateTime),
                new SqlParameter("@InterviewType", interview.InterviewType),
                new SqlParameter("@InterviewerName", interview.InterviewerName),
                new SqlParameter("@MeetingLink", (object)interview.MeetingLink ?? DBNull.Value),
                new SqlParameter("@ScheduledBy", interview.ScheduledBy)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_ScheduleInterview", parameters);
        }

        public async Task<bool> SubmitInterviewResultAsync(InterviewResultDto result)
        {
            var parameters = new[]
            {
                new SqlParameter("@ApplicationId", result.ApplicationId),
                new SqlParameter("@InterviewedBy", result.InterviewedBy),
                new SqlParameter("@Passed", result.Passed),
                new SqlParameter("@InterviewScore", result.InterviewScore),
                new SqlParameter("@InterviewNotes", (object)result.InterviewNotes ?? DBNull.Value)
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
                new SqlParameter("@ApplicationId", applicationId),
                new SqlParameter("@InitiatedBy", initiatedBy)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_InitiateBackgroundCheck", parameters);
        }

        public async Task<bool> SubmitBackgroundCheckResultAsync(BackgroundCheckResultDto result)
        {
            var parameters = new[]
            {
                new SqlParameter("@ApplicationId", result.ApplicationId),
                new SqlParameter("@Passed", result.Passed),
                new SqlParameter("@VerificationAgency", result.VerificationAgency),
                new SqlParameter("@VerificationDate", result.VerificationDate),
                new SqlParameter("@VerificationReportUrl", (object)result.VerificationReportUrl ?? DBNull.Value),
                new SqlParameter("@Notes", (object)result.Notes ?? DBNull.Value),
                new SqlParameter("@SubmittedBy", result.SubmittedBy)
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
                new SqlParameter("@ApplicationId", applicationId),
                new SqlParameter("@ModuleIds", string.Join(",", moduleIds)),
                new SqlParameter("@AssignedBy", assignedBy)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_AssignTraining", parameters);
        }

        public async Task<bool> RecordTrainingProgressAsync(long applicationId, long moduleId, int progressPercentage)
        {
            var parameters = new[]
            {
                new SqlParameter("@ApplicationId", applicationId),
                new SqlParameter("@ModuleId", moduleId),
                new SqlParameter("@ProgressPercentage", progressPercentage)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_RecordTrainingProgress", parameters);
        }

        public async Task<bool> CompleteTrainingAsync(long applicationId, long completedBy)
        {
            var parameters = new[]
            {
                new SqlParameter("@ApplicationId", applicationId),
                new SqlParameter("@CompletedBy", completedBy)
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
                new SqlParameter("@ApplicationId", applicationId),
                new SqlParameter("@ExamDate", examDate),
                new SqlParameter("@ScheduledBy", scheduledBy)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_ScheduleCertificationExam", parameters);
        }

        public async Task<bool> SubmitCertificationResultAsync(CertificationResultDto result)
        {
            var parameters = new[]
            {
                new SqlParameter("@ApplicationId", result.ApplicationId),
                new SqlParameter("@Passed", result.Passed),
                new SqlParameter("@ExamScore", result.ExamScore),
                new SqlParameter("@ExamDate", result.ExamDate),
                new SqlParameter("@CertificateNumber", (object)result.CertificateNumber ?? DBNull.Value),
                new SqlParameter("@CertificateUrl", (object)result.CertificateUrl ?? DBNull.Value),
                new SqlParameter("@EvaluatedBy", result.EvaluatedBy)
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
                new SqlParameter("@ApplicationId", applicationId),
                new SqlParameter("@ProbationDays", probationDays),
                new SqlParameter("@StartedBy", startedBy)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<bool>("sp_StartProbation", parameters);
        }

        public async Task<bool> CompleteProbationAsync(long applicationId, bool passed, long evaluatedBy, string evaluation)
        {
            var parameters = new[]
            {
                new SqlParameter("@ApplicationId", applicationId),
                new SqlParameter("@Passed", passed),
                new SqlParameter("@EvaluatedBy", evaluatedBy),
                new SqlParameter("@Evaluation", (object)evaluation ?? DBNull.Value)
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
                new SqlParameter("@ApplicationId", applicationId),
                new SqlParameter("@ActivatedBy", activatedBy)
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
                new SqlParameter("@ApplicationId", applicationId)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<List<ApplicationProgressDto>>(
                "sp_GetApplicationProgress", parameters);
        }

        public async Task<ApplicationWorkflowStatusDto> GetWorkflowStatusAsync(long applicationId)
        {
            var parameters = new[]
            {
                new SqlParameter("@ApplicationId", applicationId)
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
                new SqlParameter("@Status", (object)status ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<List<CareersApplicationModel>>(
                "sp_GetAllCareersApplications", parameters);
        }

        public async Task<List<CareersApplicationModel>> GetApplicationsByStageAsync(string stage)
        {
            var parameters = new[]
            {
                new SqlParameter("@Stage", stage)
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
                new SqlParameter("@Name", (object)filters.Name ?? DBNull.Value),
                new SqlParameter("@Email", (object)filters.Email ?? DBNull.Value),
                new SqlParameter("@Phone", (object)filters.Phone ?? DBNull.Value),
                new SqlParameter("@Status", (object)filters.Status ?? DBNull.Value),
                new SqlParameter("@CurrentStage", (object)filters.CurrentStage ?? DBNull.Value),
                new SqlParameter("@AppliedFrom", (object)filters.AppliedFrom ?? DBNull.Value),
                new SqlParameter("@AppliedTo", (object)filters.AppliedTo ?? DBNull.Value)
            };

            return await _dbHelper.ExecuteStoredProcedureAsync<List<CareersApplicationModel>>(
                "sp_SearchCareersApplications", parameters);
        }
    }
}
