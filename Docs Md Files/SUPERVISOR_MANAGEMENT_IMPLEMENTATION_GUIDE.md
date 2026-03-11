# Supervisor Management System - Complete Implementation Guide

**Two-Portal Strategy**: Careers Portal (Core) + Registration Portal (Scale)
**Date**: January 30, 2026
**Status**: Database & Models Ready | APIs & Repositories In Progress

---

## 🎯 IMPLEMENTATION OVERVIEW

### ✅ Phase 1: Database Schema (COMPLETE)
**File**: `Database/Supervisor_Management_Schema.sql`

**Tables Created** (8 total):
1. ✅ `t_sys_supervisor` - Main supervisor entity (shared by both portals)
2. ✅ `t_sys_careers_application` - Careers portal workflow (strict pipeline)
3. ✅ `t_sys_supervisor_registration` - Registration portal workflow (fast activation)
4. ✅ `t_sys_supervisor_assignment` - Event assignments
5. ✅ `t_sys_supervisor_action_log` - Complete audit trail
6. ✅ `t_sys_supervisor_training_module` - Training modules
7. ✅ `t_sys_supervisor_training_progress` - Training progress tracking
8. ✅ `t_sys_assignment_eligibility_rule` - Configurable assignment rules

### ✅ Phase 2: Stored Procedures (COMPLETE)
**File**: `Database/Supervisor_Management_StoredProcedures.sql`

**Procedures Created** (7 total):
1. ✅ `sp_CheckSupervisorAuthority` - Authority validation (CRITICAL)
2. ✅ `sp_ProgressCareersApplication` - Careers workflow progression
3. ✅ `sp_ProgressRegistrationStatus` - Registration workflow progression
4. ✅ `sp_FindEligibleSupervisors` - Smart supervisor matching
5. ✅ `sp_AssignSupervisorToEvent` - Assignment creation
6. ✅ `sp_SupervisorCheckIn` - Event check-in
7. ✅ `sp_RequestPaymentRelease` - Payment release with authority check

### ✅ Phase 3: Domain Models (COMPLETE - 3 files)
**Files Created**:
1. ✅ `CateringEcommerce.Domain/Models/Supervisor/SupervisorModel.cs`
   - SupervisorModel (base entity)
   - SupervisorType enum (CAREER / REGISTERED)
   - SupervisorStatus enum (lifecycle)
   - AuthorityLevel enum (BASIC / INTERMEDIATE / ADVANCED / FULL)
   - DTOs: Create, Update, Dashboard

2. ✅ `CateringEcommerce.Domain/Models/Supervisor/CareersApplicationModel.cs`
   - CareersApplicationModel (6-stage pipeline)
   - DTOs for each stage: Resume screening, Interview, Background check, Training, Certification, Probation
   - Progress tracking DTO

3. ✅ `CateringEcommerce.Domain/Models/Supervisor/SupervisorRegistrationModel.cs`
   - SupervisorRegistrationModel (4-stage pipeline)
   - DTOs for each stage: Document verification, Interview, Training, Certification
   - Activation DTO
   - Banking setup DTO

---

## 🔄 Phase 4: REMAINING WORK

### 4.1 Complete Domain Models (3 more files needed)

#### File: `SupervisorAssignmentModel.cs`
```csharp
namespace CateringEcommerce.Domain.Models.Supervisor
{
    public class SupervisorAssignmentModel
    {
        public long AssignmentId { get; set; }
        public long OrderId { get; set; }
        public long SupervisorId { get; set; }
        public string AssignmentNumber { get; set; }
        public DateTime AssignedDate { get; set; }
        public long AssignedBy { get; set; }

        // Event Details
        public DateTime EventDate { get; set; }
        public string EventType { get; set; }
        public string EventLocation { get; set; }
        public int EstimatedGuests { get; set; }
        public decimal EventValue { get; set; }

        // Status
        public AssignmentStatus Status { get; set; }

        // Check-in/out
        public DateTime? CheckInTime { get; set; }
        public string CheckInLocation { get; set; }
        public DateTime? CheckOutTime { get; set; }

        // Quality & Quantity Checks
        public bool QualityCheckDone { get; set; }
        public int? QualityRating { get; set; }
        public bool QuantityVerified { get; set; }
        public string QuantityNotes { get; set; }
        public string IssuesReported { get; set; }

        // Payment
        public bool PaymentReleaseRequested { get; set; }
        public bool PaymentReleaseApproved { get; set; }
        public long? PaymentApprovedBy { get; set; }
        public decimal ExtraChargesAmount { get; set; }
        public string ExtraChargesReason { get; set; }

        // Supervisor Compensation
        public decimal SupervisorPayoutAmount { get; set; }
        public string SupervisorPayoutStatus { get; set; }

        // Ratings
        public int? VendorRating { get; set; }
        public string VendorFeedback { get; set; }
        public int? AdminRating { get; set; }
    }

    public enum AssignmentStatus
    {
        ASSIGNED,
        ACCEPTED,
        REJECTED,
        CHECKED_IN,
        IN_PROGRESS,
        COMPLETED,
        CANCELLED,
        NO_SHOW
    }

    // DTOs
    public class AssignSupervisorDto { /* fields */ }
    public class AcceptAssignmentDto { /* fields */ }
    public class CheckInDto { /* fields */ }
    public class QualityCheckDto { /* fields */ }
    public class RequestPaymentReleaseDto { /* fields */ }
    public class ApprovePaymentReleaseDto { /* fields */ }
}
```

#### File: `SupervisorActionLogModel.cs`
```csharp
// Audit log model for all supervisor actions
public class SupervisorActionLogModel
{
    public long LogId { get; set; }
    public long SupervisorId { get; set; }
    public long? AssignmentId { get; set; }
    public long? OrderId { get; set; }
    public string ActionType { get; set; }
    public string ActionDescription { get; set; }
    public string ActionData { get; set; } // JSON
    public string ActionResult { get; set; }
    public string AuthorityLevelRequired { get; set; }
    public bool AuthorityCheckPassed { get; set; }
    public long? OverrideByAdmin { get; set; }
    public string EvidenceUrls { get; set; }
    public string GPSLocation { get; set; }
    public DateTime CreatedDate { get; set; }
}
```

#### File: `TrainingModuleModel.cs`
```csharp
// Training module and progress models
public class TrainingModuleModel { /* fields */ }
public class TrainingProgressModel { /* fields */ }
public class AssignmentEligibilityRuleModel { /* fields */ }
```

### 4.2 Repository Interfaces (4 files needed)

#### File: `ISupervisorRepository.cs`
```csharp
public interface ISupervisorRepository
{
    // Supervisor CRUD
    Task<SupervisorModel> GetSupervisorAsync(long supervisorId);
    Task<SupervisorModel> GetSupervisorByEmailAsync(string email);
    Task<List<SupervisorModel>> GetSupervisorsByTypeAsync(SupervisorType type);
    Task<List<SupervisorModel>> GetActiveSupervisorsAsync(string city = null);
    Task<bool> UpdateSupervisorStatusAsync(long supervisorId, SupervisorStatus newStatus, string reason);
    Task<bool> UpdateSupervisorProfileAsync(UpdateSupervisorProfileDto dto);
    Task<bool> SuspendSupervisorAsync(long supervisorId, long suspendedBy, string reason);
    Task<bool> DeactivateSupervisorAsync(long supervisorId, long deactivatedBy, string reason);
    Task<SupervisorDashboardDto> GetSupervisorDashboardAsync(long supervisorId);

    // Authority Management
    Task<AuthorityCheckResult> CheckAuthorityAsync(long supervisorId, string requiredAction);
    Task<bool> UpdateAuthorityLevelAsync(long supervisorId, AuthorityLevel newLevel);
}
```

#### File: `ICareersApplicationRepository.cs`
```csharp
public interface ICareersApplicationRepository
{
    // Application Submission
    Task<long> SubmitApplicationAsync(SubmitCareersApplicationDto dto);
    Task<CareersApplicationModel> GetApplicationAsync(long applicationId);
    Task<List<CareersApplicationModel>> GetPendingApplicationsAsync();

    // Resume Screening
    Task<bool> ScreenResumeAsync(ResumeScreeningDto dto);

    // Interview Management
    Task<bool> ScheduleInterviewAsync(ScheduleInterviewDto dto);
    Task<bool> RecordInterviewResultAsync(RecordInterviewResultDto dto);

    // Background Verification
    Task<bool> RecordBackgroundVerificationAsync(BackgroundVerificationDto dto);

    // Training & Certification
    Task<bool> AssignTrainingAsync(AssignTrainingDto dto);
    Task<bool> RecordCertificationResultAsync(CertificationTestResultDto dto);

    // Probation
    Task<bool> AssignProbationAsync(AssignProbationDto dto);
    Task<bool> EvaluateProbationAsync(ProbationEvaluationDto dto);

    // Final Decision
    Task<bool> RecordFinalDecisionAsync(FinalHiringDecisionDto dto);

    // Progress Tracking
    Task<CareersApplicationProgressDto> GetApplicationProgressAsync(long applicationId);
}
```

#### File: `IRegistrationRepository.cs`
```csharp
public interface IRegistrationRepository
{
    // Registration Submission
    Task<long> SubmitRegistrationAsync(SubmitSupervisorRegistrationDto dto);
    Task<SupervisorRegistrationModel> GetRegistrationAsync(long registrationId);
    Task<List<SupervisorRegistrationModel>> GetPendingRegistrationsAsync();

    // Document Verification
    Task<bool> VerifyDocumentsAsync(VerifyDocumentsDto dto);

    // Short Interview
    Task<bool> ScheduleShortInterviewAsync(ScheduleShortInterviewDto dto);
    Task<bool> RecordShortInterviewResultAsync(RecordShortInterviewResultDto dto);

    // Training Module
    Task<bool> AssignTrainingModuleAsync(AssignTrainingModuleDto dto);
    Task<bool> UpdateTrainingProgressAsync(UpdateTrainingProgressDto dto);

    // Certification Test
    Task<bool> RecordCertificationTestResultAsync(CertificationTestResultDto dto);

    // Activation
    Task<bool> ActivateSupervisorAsync(ActivationDecisionDto dto);

    // Banking Setup
    Task<bool> SubmitBankingDetailsAsync(SubmitBankingDetailsDto dto);
    Task<bool> VerifyBankingDetailsAsync(long registrationId, long verifiedBy);

    // Progress Tracking
    Task<RegistrationProgressDto> GetRegistrationProgressAsync(long registrationId);
}
```

#### File: `ISupervisorAssignmentRepository.cs`
```csharp
public interface ISupervisorAssignmentRepository
{
    // Assignment Management
    Task<long> AssignSupervisorAsync(AssignSupervisorDto dto);
    Task<SupervisorAssignmentModel> GetAssignmentAsync(long assignmentId);
    Task<List<SupervisorAssignmentModel>> GetSupervisorAssignmentsAsync(long supervisorId, AssignmentStatus? status = null);
    Task<List<SupervisorAssignmentModel>> GetOrderAssignmentsAsync(long orderId);

    // Supervisor Actions
    Task<bool> AcceptAssignmentAsync(long assignmentId, long supervisorId);
    Task<bool> RejectAssignmentAsync(long assignmentId, long supervisorId, string reason);
    Task<bool> CheckInAsync(CheckInDto dto);
    Task<bool> CheckOutAsync(long assignmentId, long supervisorId);

    // Quality & Quantity Checks
    Task<bool> SubmitQualityCheckAsync(QualityCheckDto dto);
    Task<bool> SubmitQuantityCheckAsync(QuantityCheckDto dto);

    // Payment Release
    Task<PaymentReleaseResult> RequestPaymentReleaseAsync(RequestPaymentReleaseDto dto);
    Task<bool> ApprovePaymentReleaseAsync(ApprovePaymentReleaseDto dto);

    // Supervisor Matching
    Task<List<SupervisorModel>> FindEligibleSupervisorsAsync(long orderId);

    // Rating & Review
    Task<bool> RateSupervisorAsync(long assignmentId, int rating, string feedback, long ratedBy);
}
```

### 4.3 Repository Implementations (4 files needed)

Create implementations for all 4 interfaces in `CateringEcommerce.BAL/Base/Supervisor/` folder:
1. `SupervisorRepository.cs`
2. `CareersApplicationRepository.cs`
3. `RegistrationRepository.cs`
4. `SupervisorAssignmentRepository.cs`

### 4.4 API Controllers (6 controllers needed)

#### 1. `CareersApplicationController.cs` (Admin Portal)
**Route**: `/api/admin/careers-application`
- POST `/submit` - Submit new careers application
- GET `/pending` - Get all pending applications
- GET `/{applicationId}` - Get application details
- POST `/screen-resume` - Screen resume
- POST `/schedule-interview` - Schedule interview
- POST `/record-interview` - Record interview result
- POST `/background-verification` - Record background check
- POST `/assign-training` - Assign training batch
- POST `/record-certification` - Record certification result
- POST `/assign-probation` - Assign probation
- POST `/evaluate-probation` - Evaluate probation
- POST `/final-decision` - Make final hiring decision
- GET `/progress/{applicationId}` - Get application progress

#### 2. `SupervisorRegistrationController.cs` (Public + Admin)
**Route**: `/api/supervisor/registration` (public) + `/api/admin/registration` (admin)
- POST `/register` - Submit registration (public)
- GET `/my-registration` - Get own registration status (public)
- GET `/pending` - Get pending registrations (admin)
- POST `/verify-documents` - Verify documents (admin)
- POST `/schedule-interview` - Schedule interview (admin)
- POST `/record-interview` - Record interview result (admin)
- POST `/assign-training` - Assign training module (admin)
- POST `/update-training-progress` - Update training progress
- POST `/submit-certification-test` - Submit certification test
- POST `/record-certification` - Record certification result (admin)
- POST `/activate` - Activate supervisor (admin)
- POST `/banking-details` - Submit banking details (post-activation)
- GET `/progress/{registrationId}` - Get registration progress

#### 3. `SupervisorManagementController.cs` (Admin Portal)
**Route**: `/api/admin/supervisor`
- GET `/all` - Get all supervisors
- GET `/{supervisorId}` - Get supervisor details
- GET `/by-type/{type}` - Get supervisors by type (CAREER/REGISTERED)
- GET `/active` - Get all active supervisors
- PUT `/update-profile` - Update supervisor profile
- PUT `/update-status` - Update supervisor status
- PUT `/update-authority` - Update authority level
- POST `/suspend` - Suspend supervisor
- POST `/deactivate` - Deactivate supervisor
- GET `/dashboard/{supervisorId}` - Get supervisor dashboard
- POST `/check-authority` - Check authority for action

#### 4. `SupervisorAssignmentController.cs` (Admin Portal)
**Route**: `/api/admin/assignment`
- GET `/eligible-supervisors/{orderId}` - Find eligible supervisors
- POST `/assign` - Assign supervisor to event
- GET `/by-supervisor/{supervisorId}` - Get supervisor's assignments
- GET `/by-order/{orderId}` - Get order's assignments
- GET `/{assignmentId}` - Get assignment details
- POST `/approve-payment` - Approve payment release
- POST `/rate-supervisor` - Rate supervisor performance

#### 5. `SupervisorPortalController.cs` (Supervisor Portal)
**Route**: `/api/supervisor/portal`
- GET `/dashboard` - Get supervisor dashboard
- GET `/my-assignments` - Get own assignments
- POST `/accept-assignment` - Accept assignment
- POST `/reject-assignment` - Reject assignment
- POST `/check-in` - Check in to event
- POST `/check-out` - Check out from event
- POST `/quality-check` - Submit quality check
- POST `/quantity-check` - Submit quantity check
- POST `/request-payment-release` - Request payment release
- POST `/report-issue` - Report issue
- POST `/upload-evidence` - Upload photos/videos
- GET `/earnings` - Get earnings summary

#### 6. `SupervisorActionLogController.cs` (Admin Portal)
**Route**: `/api/admin/supervisor-actions`
- GET `/by-supervisor/{supervisorId}` - Get supervisor's action log
- GET `/by-assignment/{assignmentId}` - Get assignment's action log
- GET `/unauthorized-attempts` - Get unauthorized action attempts
- GET `/admin-overrides` - Get actions overridden by admin

### 4.5 Dependency Injection (Program.cs)

```csharp
// Register Supervisor Repositories
builder.Services.AddScoped<ISupervisorRepository, SupervisorRepository>();
builder.Services.AddScoped<ICareersApplicationRepository, CareersApplicationRepository>();
builder.Services.AddScoped<IRegistrationRepository, RegistrationRepository>();
builder.Services.AddScoped<ISupervisorAssignmentRepository, SupervisorAssignmentRepository>();
```

---

## 🔐 CRITICAL BUSINESS RULES (ENFORCED)

### 1. Authority Level Enforcement
```
Action                      | CAREER (FULL) | CAREER (ADVANCED) | REGISTERED
---------------------------|---------------|-------------------|-------------
Check-in/out               | ✅            | ✅                | ✅
Quality check              | ✅            | ✅                | ✅
Quantity check             | ✅            | ✅                | ✅
Request extra payment      | ✅            | ✅                | ✅
Release payment            | ✅            | ❌                | ❌
Approve refund             | ✅            | ❌                | ❌
Mentor supervisors         | ✅            | ✅                | ❌
Override decisions         | ✅            | ❌                | ❌
```

### 2. Workflow Progression Rules

**Careers Portal (STRICT PIPELINE)**:
```
APPLIED
  ↓ (Resume screening required)
RESUME_SCREENED
  ↓ (Interview required)
INTERVIEW_PASSED
  ↓ (Background check required)
BACKGROUND_VERIFICATION
  ↓ (Training required)
TRAINING
  ↓ (Certification required)
CERTIFIED
  ↓ (Probation required - 90 days)
PROBATION
  ↓ (Evaluation required)
ACTIVE (Full authority granted)
```

**Registration Portal (FAST ACTIVATION)**:
```
APPLIED
  ↓ (Document verification required)
DOCUMENT_VERIFICATION
  ↓ (Short interview required)
AWAITING_INTERVIEW
  ↓ (Training module required)
AWAITING_TRAINING
  ↓ (Certification test required)
AWAITING_CERTIFICATION
  ↓ (Auto-activation if all pass)
ACTIVE (Limited authority - BASIC level)
```

### 3. Assignment Eligibility Rules (Configurable)

```sql
-- Example rules in t_sys_assignment_eligibility_rule

-- Rule 1: VIP Events → CAREER supervisors only
Event Type: VIP
Min Value: NULL
Required Type: CAREER
Required Authority: ADVANCED
Priority: 1

-- Rule 2: New Vendor → CAREER supervisors only
Is New Vendor: TRUE
Required Type: CAREER
Required Authority: INTERMEDIATE
Priority: 2

-- Rule 3: High Value → CAREER preferred
Min Value: 100000
Required Type: EITHER (but CAREER gets priority)
Required Authority: INTERMEDIATE
Priority: 3

-- Rule 4: Medium Events → Either type
Min Value: 25000
Max Value: 100000
Required Type: EITHER
Required Authority: BASIC
Priority: 4

-- Rule 5: Small Events → REGISTERED OK
Max Value: 25000
Required Type: EITHER
Required Authority: BASIC
Priority: 5
```

### 4. Payment Release Authorization

**CAREER Supervisors (FULL authority)**:
- Can release payment directly (instant)
- No admin approval needed
- Action logged for audit

**REGISTERED Supervisors (BASIC authority)**:
- Can only REQUEST payment release
- Admin approval MANDATORY
- Action logged with "requires approval" flag

**Implementation**:
```csharp
if (supervisorType == SupervisorType.CAREER && canReleasePayment)
{
    // Direct release
    payment.Status = "Released";
    payment.ReleasedBy = supervisorId;
}
else
{
    // Request approval
    payment.Status = "Pending_Approval";
    payment.RequestedBy = supervisorId;
    // Notify admin
}
```

---

## 📊 DATABASE SETUP GUIDE

### Step 1: Run Schema Script
```sql
-- Run this first
USE CateringEcommerce;
GO
-- Execute: Database/Supervisor_Management_Schema.sql
```

### Step 2: Run Stored Procedures
```sql
-- Run this second
-- Execute: Database/Supervisor_Management_StoredProcedures.sql
```

### Step 3: Insert Default Rules
```sql
-- Insert default assignment eligibility rules
INSERT INTO t_sys_assignment_eligibility_rule (
    c_rule_name, c_is_vip_event, c_required_supervisor_type,
    c_required_authority_level, c_priority
) VALUES
('VIP Events Rule', 1, 'CAREER', 'ADVANCED', 1),
('New Vendor Rule', NULL, 'CAREER', 'INTERMEDIATE', 2),
('High Value Rule', NULL, 'EITHER', 'INTERMEDIATE', 3);
```

### Step 4: Create Training Modules
```sql
-- Insert default training modules
INSERT INTO t_sys_supervisor_training_module (
    c_module_code, c_module_name, c_module_type,
    c_is_mandatory, c_duration_hours, c_passing_score
) VALUES
('SUP-101', 'Event Supervision Basics', 'BOTH', 1, 4, 70),
('SUP-201', 'Quality Control Standards', 'BOTH', 1, 3, 75),
('SUP-301', 'Payment & Settlement Process', 'CAREER', 1, 2, 80),
('SUP-401', 'Dispute Resolution', 'CAREER', 1, 2, 75);
```

---

## 🧪 TESTING CHECKLIST

### Unit Tests Needed
- [ ] Authority level validation
- [ ] Status progression validation
- [ ] Payment release authorization
- [ ] Assignment eligibility matching

### Integration Tests Needed
- [ ] Complete careers application flow (end-to-end)
- [ ] Complete registration flow (end-to-end)
- [ ] Supervisor assignment flow
- [ ] Payment release with authority check

### Test Scenarios
1. **Careers Application**: Submit → Screen → Interview → Train → Certify → Probation → Activate
2. **Registration**: Submit → Verify → Interview → Train → Certify → Activate
3. **Assignment**: Find eligible → Assign → Accept → Check-in → Quality check → Payment release
4. **Authority Test**: REGISTERED supervisor tries to release payment (should fail)
5. **Admin Override**: Admin approves payment release requested by REGISTERED supervisor

---

## 📋 IMPLEMENTATION PRIORITY

### High Priority (Must Have)
1. ✅ Database schema
2. ✅ Stored procedures
3. ✅ Domain models (core 3 files)
4. 🔄 Complete remaining models (3 files)
5. 🔄 Repository interfaces (4 files)
6. 🔄 Repository implementations (4 files)
7. 🔄 Core API controllers (6 files)
8. 🔄 Dependency injection setup

### Medium Priority (Should Have)
- Training module management UI
- Advanced analytics dashboard
- Automated assignment algorithm
- Mobile app integration

### Low Priority (Nice to Have)
- AI-based supervisor matching
- Predictive analytics
- Performance benchmarking
- Gamification features

---

## 🚀 DEPLOYMENT CHECKLIST

- [ ] Run database schema script
- [ ] Run stored procedures script
- [ ] Insert default rules and training modules
- [ ] Deploy backend APIs
- [ ] Configure authentication for supervisor portal
- [ ] Test careers application flow
- [ ] Test registration flow
- [ ] Test assignment flow
- [ ] Verify authority level enforcement
- [ ] Verify payment release authorization
- [ ] Set up monitoring and logging
- [ ] Train admin team on both portals

---

**Status**: Foundation Complete (Database + Models)
**Next Step**: Implement repositories and API controllers
**ETA**: 6-8 hours for complete implementation

---

*Last Updated: January 30, 2026*
*Architect: Senior Product Architect & Backend Engineer*
