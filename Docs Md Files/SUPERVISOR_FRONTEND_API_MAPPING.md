# Supervisor Management - API → UI Field Mapping

## ✅ Backend Verification Status
**All required backend infrastructure is IMPLEMENTED:**
- ✅ Models: SupervisorModel, SupervisorRegistrationModel, CareersApplicationModel
- ✅ Repositories: ISupervisorRepository, IRegistrationRepository, ICareersApplicationRepository
- ✅ Enums: SupervisorType, SupervisorStatus, AuthorityLevel, CompensationType, CertificationStatus
- ✅ Database: Tables and stored procedures exist

---

## 📋 Portal #1: Careers Portal (Core Supervisors)

### 1.1 Application Submission Form

| UI Field Label | Backend Field | Data Type | API Endpoint | Required |
|---|---|---|---|---|
| Full Name | `FullName` | string | POST /api/careers/submit | ✅ |
| Email Address | `Email` | string | POST /api/careers/submit | ✅ |
| Phone Number | `Phone` | string | POST /api/careers/submit | ✅ |
| Alternate Phone | `AlternatePhone` | string | POST /api/careers/submit | ❌ |
| Date of Birth | `DateOfBirth` | DateTime? | POST /api/careers/submit | ✅ |
| City | `City` | string | POST /api/careers/submit | ✅ |
| State | `State` | string | POST /api/careers/submit | ✅ |
| Pincode | `Pincode` | string | POST /api/careers/submit | ✅ |
| Years of Experience | `YearsOfExperience` | int? | POST /api/careers/submit | ✅ |
| Previous Employer | `PreviousEmployer` | string | POST /api/careers/submit | ✅ |
| Specialization | `Specialization` | string | POST /api/careers/submit | ❌ |
| Languages Known | `LanguagesKnown` | List<string> | POST /api/careers/submit | ❌ |
| Identity Type | `IdentityType` | string | POST /api/careers/submit | ✅ |
| Identity Number | `IdentityNumber` | string | POST /api/careers/submit | ✅ |
| Resume Upload | `ResumeUrl` | string (URL) | POST /api/careers/submit | ✅ |
| Photo Upload | `PhotoUrl` | string (URL) | POST /api/careers/submit | ✅ |
| Source | `Source` | enum | AUTO-SET | - |

**DTO Used:** `CareersApplicationSubmitDto`

---

### 1.2 Supervisor Profile Page (Careers)

| UI Section | Backend Field | Display Format | API Endpoint |
|---|---|---|---|
| **Basic Info** | | | |
| Name | `FullName` | Text | GET /api/supervisor/{id} |
| Email | `Email` | Text | GET /api/supervisor/{id} |
| Phone | `Phone` | Text | GET /api/supervisor/{id} |
| Type Badge | `SupervisorType` | Badge (CAREER) | GET /api/supervisor/{id} |
| Authority Badge | `AuthorityLevel` | Badge with icon | GET /api/supervisor/{id} |
| Current Status | `CurrentStatus` | Status chip | GET /api/supervisor/{id} |
| **Verification Status** | | | |
| Resume Screened | `ResumeScreened` | ✅/❌ + Date | GET /api/careers/application/{id} |
| Interview Status | `InterviewResult` | Badge | GET /api/careers/application/{id} |
| Background Check | `BackgroundVerificationResult` | Badge | GET /api/careers/application/{id} |
| Training Completed | `TrainingCompleted` | ✅/❌ + Date | GET /api/careers/application/{id} |
| Certification | `CertificationStatus` | Badge | GET /api/supervisor/{id} |
| Probation Status | `IsProbationPassed` | Badge | GET /api/careers/application/{id} |
| **Authority & Permissions** | | | |
| Can Release Payment | `CanReleasePayment` | Boolean icon | GET /api/supervisor/{id} |
| Can Approve Refund | `CanApproveRefund` | Boolean icon | GET /api/supervisor/{id} |
| Can Mentor Others | `CanMentorOthers` | Boolean icon | GET /api/supervisor/{id} |
| **Performance Metrics** | | | |
| Total Events | `TotalEventsSupervised` | Number | GET /api/supervisor/{id} |
| Average Rating | `AverageRating` | Star rating | GET /api/supervisor/{id} |
| Disputes Resolved | `DisputeResolutionCount` | Number | GET /api/supervisor/{id} |
| **Compensation** | | | |
| Type | `CompensationType` | Text | GET /api/supervisor/{id} |
| Monthly Salary | `MonthlySalary` | Currency | GET /api/supervisor/{id} |
| Incentive % | `IncentivePercentage` | Percentage | GET /api/supervisor/{id} |

**Model Used:** `SupervisorModel`, `CareersApplicationModel`

---

### 1.3 Admin Careers Management View

| UI Feature | Backend Field/Method | API Endpoint | Admin Permission |
|---|---|---|---|
| **Application List** | | | |
| Pending Resume Screening | `GetApplicationsForResumeScreeningAsync()` | GET /api/admin/careers/pending-resume | Admin |
| Pending Interviews | `GetApplicationsForInterviewAsync()` | GET /api/admin/careers/pending-interview | Admin |
| In Training | `GetApplicationsInTrainingAsync()` | GET /api/admin/careers/in-training | Admin |
| In Probation | `GetApplicationsInProbationAsync()` | GET /api/admin/careers/in-probation | Admin |
| **Actions** | | | |
| Screen Resume | `SubmitResumeScreeningAsync()` | POST /api/admin/careers/screen-resume | Admin |
| Schedule Interview | `ScheduleInterviewAsync()` | POST /api/admin/careers/schedule-interview | Admin |
| Submit Interview Result | `SubmitInterviewResultAsync()` | POST /api/admin/careers/interview-result | Admin |
| Initiate Background Check | `InitiateBackgroundCheckAsync()` | POST /api/admin/careers/background-check | Admin |
| Assign Training | `AssignTrainingAsync()` | POST /api/admin/careers/assign-training | Admin |
| Start Probation | `StartProbationAsync()` | POST /api/admin/careers/start-probation | Admin |
| Activate Supervisor | `ActivateSupervisorAsync()` | POST /api/admin/careers/activate | Super Admin |
| Reject Application | `RejectApplicationAsync()` | POST /api/admin/careers/reject | Admin |

---

## 📋 Portal #2: Supervisor Registration Portal (Scale Engine)

### 2.1 Multi-Step Registration Form

#### Step 1: Personal Details

| UI Field | Backend Field | Type | Validation |
|---|---|---|---|
| Full Name | `FullName` | string | Required, 3-100 chars |
| Email | `Email` | string | Required, valid email |
| Phone | `Phone` | string | Required, 10 digits |
| Alternate Phone | `AlternatePhone` | string | Optional, 10 digits |
| Date of Birth | `DateOfBirth` | DateTime? | Required, 18+ years |

#### Step 2: Address Details

| UI Field | Backend Field | Type | Validation |
|---|---|---|---|
| Address Line 1 | `AddressLine1` | string | Required |
| City | `City` | string | Required |
| State | `State` | string | Required |
| Pincode | `Pincode` | string | Required, 6 digits |
| Locality | `Locality` | string | Required |

#### Step 3: Experience Details

| UI Field | Backend Field | Type | Validation |
|---|---|---|---|
| Years of Experience | `YearsOfExperience` | int | Required, 0-50 |
| Previous Employer | `PreviousEmployer` | string | Required |
| Specialization | `Specialization` | string | Optional |
| Languages Known | `LanguagesKnown` | List<string> | Multi-select |

#### Step 4: Identity Proof

| UI Field | Backend Field | Type | Validation |
|---|---|---|---|
| Identity Type | `IdentityType` | enum | Required (AADHAAR/PAN/PASSPORT) |
| Identity Number | `IdentityNumber` | string | Required, format validation |
| Identity Proof Upload | `IdentityProofUrl` | file URL | Required, PDF/JPG/PNG |
| Photo Upload | `PhotoUrl` | file URL | Required, JPG/PNG |

#### Step 5: Availability Setup

| UI Field | Backend Field | Type | Validation |
|---|---|---|---|
| Preferred Cities | `PreferredCities` | List<string> | Required, min 1 |
| Preferred Localities | `PreferredLocalities` | List<string> | Required, min 1 |
| Preferred Event Types | `PreferredEventTypes` | List<string> | Optional |
| Available Days/Week | `AvailableDaysPerWeek` | int | Required, 1-7 |

#### Step 6: Agreement

| UI Field | Backend Field | Type | Validation |
|---|---|---|---|
| I Accept Terms & Conditions | `AgreementAccepted` | boolean | Required (must be true) |
| Agreement Date | `AgreementAcceptedDate` | DateTime | AUTO-SET |
| IP Address | `AgreementIpAddress` | string | AUTO-CAPTURED |

**API Endpoint:** `POST /api/registration/submit`
**DTO Used:** `SubmitSupervisorRegistrationDto`

---

### 2.2 Registration Progress Tracker

| UI Element | Backend Field | Display Type | API Source |
|---|---|---|---|
| **Stage 1: Document Verification** | | | |
| Status | `DocumentVerificationStatus` | Badge | GET /api/registration/progress/{id} |
| Verified By | `DocumentVerifiedBy` | Admin name | GET /api/registration/progress/{id} |
| Verified Date | `DocumentVerifiedDate` | Date | GET /api/registration/progress/{id} |
| Rejection Reason | `DocumentRejectionReason` | Alert text | GET /api/registration/progress/{id} |
| **Stage 2: Short Interview** | | | |
| Interview Scheduled | `InterviewScheduled` | Boolean | GET /api/registration/progress/{id} |
| Interview Date | `InterviewDate` | Date/Time | GET /api/registration/progress/{id} |
| Interview Mode | `InterviewMode` | Text | GET /api/registration/progress/{id} |
| Interview Result | `InterviewResult` | Badge (PASSED/FAILED) | GET /api/registration/progress/{id} |
| **Stage 3: Training** | | | |
| Training Assigned | `TrainingModuleAssigned` | Boolean | GET /api/registration/progress/{id} |
| Training Started | `TrainingStartedDate` | Date | GET /api/registration/progress/{id} |
| Training Progress | `TrainingCompletionPercentage` | Progress bar | GET /api/registration/progress/{id} |
| Training Passed | `TrainingPassed` | Boolean | GET /api/registration/progress/{id} |
| **Stage 4: Certification** | | | |
| Test Scheduled | `CertificationTestAssigned` | Boolean | GET /api/registration/progress/{id} |
| Test Date | `CertificationTestDate` | Date | GET /api/registration/progress/{id} |
| Test Score | `CertificationTestScore` | Percentage | GET /api/registration/progress/{id} |
| Certificate | `CertificationCertificateUrl` | Download link | GET /api/registration/progress/{id} |
| **Final Activation** | | | |
| Activation Status | `ActivationStatus` | Badge | GET /api/registration/progress/{id} |
| Activated Date | `ActivatedDate` | Date | GET /api/registration/progress/{id} |
| Activated By | `ActivatedBy` | Admin name | GET /api/registration/progress/{id} |

**Model Used:** `SupervisorRegistrationModel`, `RegistrationProgressSummaryDto`

---

### 2.3 Registered Supervisor Dashboard

| Dashboard Widget | Backend Field/Method | API Endpoint |
|---|---|---|---|
| **Profile Summary** | | |
| Name | `FullName` | GET /api/supervisor/{id} |
| Status Badge | `CurrentStatus` | GET /api/supervisor/{id} |
| Authority Level | `AuthorityLevel` | GET /api/supervisor/dashboard/{id} |
| Certification Valid Until | `CertificationValidUntil` | GET /api/supervisor/dashboard/{id} |
| **Assigned Events** | | |
| Upcoming Assignments | `UpcomingAssignments` | GET /api/supervisor/dashboard/{id} |
| Pending Approvals | `PendingApprovals` | GET /api/supervisor/dashboard/{id} |
| **Performance Stats** | | |
| Events This Month | `EventsThisMonth` | GET /api/supervisor/dashboard/{id} |
| Total Events | `TotalEventsSupervised` | GET /api/supervisor/dashboard/{id} |
| Average Rating | `AverageRating` | GET /api/supervisor/dashboard/{id} |
| **Earnings** | | |
| Total Earnings | `TotalEarnings` | GET /api/supervisor/dashboard/{id} |
| Earnings This Month | `EarningsThisMonth` | GET /api/supervisor/dashboard/{id} |
| Per Event Rate | `PerEventRate` | GET /api/supervisor/{id} |
| **Authority Capabilities** | | |
| Can Check-in Event | Always TRUE | - |
| Can Quality Check | Always TRUE | - |
| Can Request Extra Payment | Always TRUE | - |
| Can Release Payment | `CanReleasePayment` (FALSE for REGISTERED) | GET /api/supervisor/{id} |
| Can Approve Refund | `CanApproveRefund` (FALSE for REGISTERED) | GET /api/supervisor/{id} |

**DTO Used:** `SupervisorPortalDashboardDto`

---

### 2.4 Admin Registration Management

| Admin View | Backend Method | API Endpoint | Permission Required |
|---|---|---|---|
| **Pending Actions** | | | |
| Document Verification Queue | `GetRegistrationsPendingDocumentVerificationAsync()` | GET /api/admin/registration/pending-docs | Admin |
| Interview Queue | `GetRegistrationsPendingInterviewAsync()` | GET /api/admin/registration/pending-interview | Admin |
| Training Queue | `GetRegistrationsPendingTrainingAsync()` | GET /api/admin/registration/pending-training | Admin |
| Certification Queue | `GetRegistrationsPendingCertificationAsync()` | GET /api/admin/registration/pending-cert | Admin |
| **Admin Actions** | | | |
| Verify Documents | `SubmitDocumentVerificationAsync()` | POST /api/admin/registration/verify-docs | Admin |
| Schedule Interview | `ScheduleQuickInterviewAsync()` | POST /api/admin/registration/schedule-interview | Admin |
| Submit Interview Result | `SubmitQuickInterviewResultAsync()` | POST /api/admin/registration/interview-result | Admin |
| Assign Training | `AssignCondensedTrainingAsync()` | POST /api/admin/registration/assign-training | Admin |
| Submit Cert Result | `SubmitQuickCertificationResultAsync()` | POST /api/admin/registration/cert-result | Admin |
| Activate Supervisor | `ActivateRegisteredSupervisorAsync()` | POST /api/admin/registration/activate | Super Admin |
| Reject Registration | `RejectRegistrationAsync()` | POST /api/admin/registration/reject | Admin |

---

## 📋 Admin Supervisor Management (Both Types)

### 3.1 Supervisor List View

| Column | Backend Field | Filter/Sort | API Endpoint |
|---|---|---|---|
| Supervisor ID | `SupervisorId` | ✅ Sort | GET /api/admin/supervisors |
| Name | `FullName` | ✅ Search | GET /api/admin/supervisors |
| Type | `SupervisorType` | ✅ Filter | GET /api/admin/supervisors |
| Status | `CurrentStatus` | ✅ Filter | GET /api/admin/supervisors |
| Authority Level | `AuthorityLevel` | ✅ Filter | GET /api/admin/supervisors |
| City | `City` | ✅ Filter | GET /api/admin/supervisors |
| Events Supervised | `TotalEventsSupervised` | ✅ Sort | GET /api/admin/supervisors |
| Average Rating | `AverageRating` | ✅ Sort | GET /api/admin/supervisors |
| Certification Status | `CertificationStatus` | ✅ Filter | GET /api/admin/supervisors |
| Is Available | `IsAvailable` | ✅ Filter | GET /api/admin/supervisors |
| Actions | - | - | Multiple endpoints |

**Search Filters:** `SupervisorSearchDto`

---

### 3.2 Authority Management

| Action | Backend Method | API Endpoint | Permission | Notes |
|---|---|---|---|---|
| Check Authority | `CheckSupervisorAuthorityAsync()` | GET /api/admin/supervisor/{id}/authority-check | Admin | Returns: Can perform action? |
| Update Authority Level | `UpdateAuthorityLevelAsync()` | POST /api/admin/supervisor/{id}/authority-level | Super Admin | One-way upgrade only |
| Grant Permission | `GrantPermissionAsync()` | POST /api/admin/supervisor/{id}/grant-permission | Super Admin | Individual permissions |
| Revoke Permission | `RevokePermissionAsync()` | POST /api/admin/supervisor/{id}/revoke-permission | Super Admin | Individual permissions |

**Authority Levels (UI Display):**
- **BASIC** → Badge: Blue, Icon: User
- **INTERMEDIATE** → Badge: Yellow, Icon: UserCheck
- **ADVANCED** → Badge: Orange, Icon: UserCog
- **FULL** → Badge: Green, Icon: ShieldCheck

---

### 3.3 Status Management

| Action | Backend Method | API Endpoint | Permission |
|---|---|---|---|
| Update Status | `UpdateStatusAsync()` | POST /api/admin/supervisor/{id}/status | Admin |
| Activate Supervisor | `ActivateSupervisorAsync()` | POST /api/admin/supervisor/{id}/activate | Super Admin |
| Suspend Supervisor | `SuspendSupervisorAsync()` | POST /api/admin/supervisor/{id}/suspend | Admin |
| Terminate Supervisor | `TerminateSupervisorAsync()` | POST /api/admin/supervisor/{id}/terminate | Super Admin |

---

### 3.4 Assignment View

| UI Element | Backend Source | API Endpoint |
|---|---|---|
| Available Supervisors | `GetAvailableSupervisorsAsync(eventDate, eventType)` | GET /api/admin/supervisor/available |
| Supervisors by Zone | `GetSupervisorsByZoneAsync(zoneId)` | GET /api/admin/supervisor/by-zone/{zoneId} |
| Supervisors by Authority | `GetSupervisorsByAuthorityAsync(authorityLevel)` | GET /api/admin/supervisor/by-authority/{level} |

---

## 🚫 Missing Backend Fields Analysis

### ✅ All Required Fields Present

After thorough review, **NO frontend fields are missing from the backend**. The backend models comprehensively cover:

- ✅ Both portal workflows (Careers & Registration)
- ✅ All mandatory stages and progression
- ✅ Authority and permission management
- ✅ Admin control capabilities
- ✅ Dashboard and analytics
- ✅ Assignment and availability logic

---

## 🔐 Backend Enums (Use Exactly As-Is)

### SupervisorType
```typescript
enum SupervisorType {
  CAREER = "CAREER",
  REGISTERED = "REGISTERED"
}
```

### SupervisorStatus
```typescript
enum SupervisorStatus {
  // Common
  APPLIED = "APPLIED",
  REJECTED = "REJECTED",
  ACTIVE = "ACTIVE",
  SUSPENDED = "SUSPENDED",
  DEACTIVATED = "DEACTIVATED",
  BLACKLISTED = "BLACKLISTED",

  // Careers Specific
  RESUME_SCREENED = "RESUME_SCREENED",
  INTERVIEW_SCHEDULED = "INTERVIEW_SCHEDULED",
  INTERVIEW_PASSED = "INTERVIEW_PASSED",
  BACKGROUND_VERIFICATION = "BACKGROUND_VERIFICATION",
  TRAINING = "TRAINING",
  CERTIFIED = "CERTIFIED",
  PROBATION = "PROBATION",

  // Registration Specific
  DOCUMENT_VERIFICATION = "DOCUMENT_VERIFICATION",
  AWAITING_INTERVIEW = "AWAITING_INTERVIEW",
  AWAITING_TRAINING = "AWAITING_TRAINING",
  AWAITING_CERTIFICATION = "AWAITING_CERTIFICATION"
}
```

### AuthorityLevel
```typescript
enum AuthorityLevel {
  BASIC = "BASIC",           // Check-in, quality check, request extra payment
  INTERMEDIATE = "INTERMEDIATE",   // BASIC + medium complexity issues
  ADVANCED = "ADVANCED",      // INTERMEDIATE + mentor, VIP events
  FULL = "FULL"              // ADVANCED + release payments, approve refunds (Careers only)
}
```

### CompensationType
```typescript
enum CompensationType {
  MONTHLY_SALARY = "MONTHLY_SALARY",   // Careers supervisors
  PER_EVENT = "PER_EVENT",            // Registered supervisors
  HYBRID = "HYBRID"                   // Salary + per event incentive
}
```

### CertificationStatus
```typescript
enum CertificationStatus {
  PENDING = "PENDING",
  CERTIFIED = "CERTIFIED",
  EXPIRED = "EXPIRED",
  SUSPENDED = "SUSPENDED"
}
```

---

## 📦 API Response Formats (Expected by Frontend)

### Success Response
```json
{
  "success": true,
  "data": { /* Model data */ },
  "message": "Operation successful"
}
```

### Error Response
```json
{
  "success": false,
  "errors": ["Error message 1", "Error message 2"],
  "message": "Operation failed"
}
```

### Validation Error Response
```json
{
  "success": false,
  "errors": {
    "FullName": ["Full name is required"],
    "Email": ["Invalid email format"]
  },
  "message": "Validation failed"
}
```

---

## ✅ Frontend Implementation Checklist

- ✅ Use backend enums exactly as defined
- ✅ Display backend validation errors without modification
- ✅ Respect permission flags from backend
- ✅ Never hardcode authority logic
- ✅ Show audit-safe UI (who did what, when)
- ✅ Use date formats from backend (ISO 8601)
- ✅ File uploads: Get pre-signed URLs from backend first
- ✅ Handle both portal types with same component structure (conditional rendering based on `SupervisorType`)

---

**Document Version:** 1.0
**Last Updated:** 2026-01-31
**Backend Status:** ✅ Fully Implemented
**Frontend Status:** 🔄 Ready for Development
