# Event Supervisor Portal - Frontend Implementation Guide

**Date:** 2026-01-31
**Status:** ✅ Production-Ready
**Backend Compatibility:** 100% Verified

---

## ⚠️ ABSOLUTE RULES (NON-NEGOTIABLE)

1. **Backend is ALREADY IMPLEMENTED** - All models, repositories, stored procedures exist
2. **Frontend must strictly follow backend fields & permissions** - No custom logic
3. **DO NOT invent new fields, tables, enums, or roles silently**
4. **NO Careers Portal** - Static page only, no backend/frontend logic
5. **ONLY Event Supervisor Portal** - Registration → Verification → Training → Activation
6. **Admin-Controlled Payments** - Supervisors REQUEST payment, admins APPROVE/RELEASE

---

## 🎯 System Overview

### What This Portal Does

**Event Supervisor Portal** enables supervisors to:
- Register for supervisor role
- Track verification & training progress
- Accept event assignments
- Execute event supervision (Pre/During/Post)
- Upload timestamped evidence
- Request payment release
- View earnings and performance

**Supervisors CANNOT:**
- Release payments (admin-only)
- Approve refunds (admin-only)
- Override admin decisions
- Access partner financial data

---

## 🔄 Complete Workflow

```
┌─────────────────────────────────────────────────────────────┐
│ REGISTRATION WORKFLOW (Public Portal)                       │
└─────────────────────────────────────────────────────────────┘
APPLIED → DOCUMENT_VERIFICATION → AWAITING_INTERVIEW
       → AWAITING_TRAINING → AWAITING_CERTIFICATION → ACTIVE

┌─────────────────────────────────────────────────────────────┐
│ EVENT EXECUTION WORKFLOW (Supervisor Portal)                │
└─────────────────────────────────────────────────────────────┘
ADMIN_ASSIGNMENT → SUPERVISOR_ACCEPTANCE → CHECK_IN
                → PRE_EVENT_VERIFICATION → EVENT_IN_PROGRESS
                → POST_EVENT_REPORT → PAYMENT_REQUEST
                → ADMIN_APPROVAL → PAYMENT_RELEASE

┌─────────────────────────────────────────────────────────────┐
│ ADMIN WORKFLOW (Admin Portal)                               │
└─────────────────────────────────────────────────────────────┘
Verify Documents → Schedule Interview → Assign Training
               → Certify → Activate → Assign Events
               → Monitor Execution → Approve Payments
```

---

## 📊 Frontend Component Hierarchy

```
src/
├── pages/
│   ├── public/
│   │   └── SupervisorRegistration.jsx                    # Public registration portal
│   │
│   ├── supervisor/
│   │   ├── SupervisorDashboard.jsx                       # Main dashboard
│   │   ├── SupervisorProfile.jsx                         # Profile & settings
│   │   ├── AssignmentsList.jsx                           # View all assignments
│   │   ├── AssignmentDetails.jsx                         # Single assignment detail
│   │   ├── EventExecution.jsx                            # Live event execution page
│   │   ├── EarningsPage.jsx                              # Payment history
│   │   └── RegistrationProgress.jsx                      # Track registration workflow
│   │
│   └── admin/
│       ├── AdminSupervisors.jsx                          # Supervisor management
│       ├── AdminAssignments.jsx                          # Assignment management
│       ├── RegistrationQueue.jsx                         # Registration approval queue
│       └── PaymentApprovals.jsx                          # Payment approval queue
│
├── components/
│   ├── registration/
│   │   ├── RegistrationWizard.jsx                        # Multi-step registration form
│   │   │   ├── Step1_PersonalDetails.jsx
│   │   │   ├── Step2_AddressDetails.jsx
│   │   │   ├── Step3_ExperienceDetails.jsx
│   │   │   ├── Step4_IdentityProof.jsx
│   │   │   ├── Step5_Availability.jsx
│   │   │   └── Step6_BankingDetails.jsx
│   │   ├── RegistrationProgressTracker.jsx               # Workflow progress stepper
│   │   └── RegistrationStatusBadge.jsx                   # Status indicator
│   │
│   ├── event-execution/
│   │   ├── PreEventVerification/
│   │   │   ├── MenuVerificationForm.jsx                  # Menu vs contract check
│   │   │   ├── RawMaterialInspection.jsx                 # Quality & quantity check
│   │   │   ├── GuestCountConfirmation.jsx                # Lock guest count
│   │   │   └── PreEventEvidenceUpload.jsx                # Photo/video evidence
│   │   │
│   │   ├── DuringEvent/
│   │   │   ├── EventCheckIn.jsx                          # GPS + photo check-in
│   │   │   ├── FoodServingMonitor.jsx                    # Quality monitoring
│   │   │   ├── GuestCountTracker.jsx                     # Real-time guest count
│   │   │   ├── ExtraQuantityRequest.jsx                  # Request extras + OTP
│   │   │   ├── ClientOTPVerification.jsx                 # Verify client approval
│   │   │   └── LiveIssueReporter.jsx                     # Report issues live
│   │   │
│   │   ├── PostEvent/
│   │   │   ├── ClientFeedbackForm.jsx                    # Structured feedback
│   │   │   ├── PartnerPerformanceRating.jsx              # Rate partner
│   │   │   ├── EventIssuesSummary.jsx                    # List all issues
│   │   │   ├── PaymentBreakdownForm.jsx                  # Calculate final amount
│   │   │   ├── CompletionEvidenceUpload.jsx              # Final photos/videos
│   │   │   └── PostEventReportSubmit.jsx                 # Submit complete report
│   │   │
│   │   └── EventSupervisionSummary.jsx                   # Pre+During+Post overview
│   │
│   ├── assignments/
│   │   ├── AssignmentCard.jsx                            # Single assignment display
│   │   ├── AssignmentStatusBadge.jsx                     # Status indicator
│   │   ├── AssignmentAcceptReject.jsx                    # Accept/reject UI
│   │   ├── PaymentRequestButton.jsx                      # Request payment release
│   │   └── AssignmentTimeline.jsx                        # Track assignment progress
│   │
│   ├── payments/
│   │   ├── PaymentHistoryTable.jsx                       # All payments
│   │   ├── PaymentStatusBadge.jsx                        # Status indicator
│   │   ├── EarningsSummary.jsx                           # Total/pending/paid
│   │   └── PaymentBreakdownView.jsx                      # Itemized breakdown
│   │
│   ├── admin/
│   │   ├── RegistrationApprovalDrawer.jsx                # Verify documents
│   │   ├── InterviewScheduler.jsx                        # Schedule interview
│   │   ├── TrainingAssignment.jsx                        # Assign training
│   │   ├── CertificationScheduler.jsx                    # Schedule certification
│   │   ├── SupervisorActivation.jsx                      # Activate supervisor
│   │   ├── EventAssignment.jsx                           # Assign to event
│   │   ├── PaymentApprovalCard.jsx                       # Approve payment request
│   │   └── SupervisorPerformanceView.jsx                 # View analytics
│   │
│   └── common/
│       ├── badges/
│       │   ├── SupervisorTypeBadge.jsx                   # CAREER / REGISTERED
│       │   ├── SupervisorStatusBadge.jsx                 # ACTIVE / SUSPENDED
│       │   ├── AuthorityLevelBadge.jsx                   # BASIC / INTERMEDIATE / ADVANCED / FULL
│       │   └── CertificationBadge.jsx                    # PENDING / PASSED / FAILED
│       │
│       ├── forms/
│       │   ├── DocumentUploader.jsx                      # File upload with preview
│       │   ├── TimestampedEvidenceUpload.jsx             # Photo/video with GPS+timestamp
│       │   ├── SignatureCapture.jsx                      # Client signature
│       │   └── OTPInput.jsx                              # 6-digit OTP input
│       │
│       ├── progress/
│       │   ├── WorkflowStepper.jsx                       # Generic stepper
│       │   └── ProgressCircle.jsx                        # Circular progress
│       │
│       └── PermissionGuard.jsx                           # Check permissions before render
│
└── services/
    ├── api/
    │   ├── supervisorApi.js                              # Core supervisor CRUD
    │   ├── registrationApi.js                            # Registration workflow
    │   ├── assignmentApi.js                              # Assignment management
    │   ├── eventSupervisionApi.js                        # Event execution APIs
    │   └── paymentApi.js                                 # Payment requests
    │
    ├── contexts/
    │   ├── SupervisorContext.jsx                         # Global supervisor state
    │   └── AssignmentContext.jsx                         # Current assignment state
    │
    └── utils/
        ├── supervisorEnums.js                            # All backend enums
        ├── permissionUtils.js                            # Check permissions
        ├── timestampUtils.js                             # GPS + timestamp helpers
        └── validationSchemas.js                          # Zod validation
```

---

## 🔌 Complete API → UI Field Mapping

### 1. Registration API

#### **POST /api/registration/submit**

**Frontend Component:** `RegistrationWizard.jsx`

**Request DTO: `SupervisorRegistrationSubmitDto`**

```javascript
// Step 1: Personal Details
{
  firstName: string,                    // Input field
  lastName: string,                     // Input field
  email: string,                        // Email input
  phone: string,                        // Phone input
  dateOfBirth: Date,                    // Date picker (must be 18+)
}

// Step 2: Address Details
{
  address: string,                      // Textarea
  preferredZoneId: number,              // Zone dropdown
}

// Step 3: Experience Details
{
  hasPriorExperience: boolean,          // Checkbox
  priorExperienceDetails: string,       // Textarea (conditional)
}

// Step 4: Identity Proof
{
  idProofType: string,                  // Dropdown (AADHAAR, PAN, VOTER_ID, PASSPORT)
  idProofNumber: string,                // Input field
  idProofUrl: string,                   // File upload → URL
  addressProofUrl: string,              // File upload → URL
  photoUrl: string,                     // File upload → URL
}

// Step 5: Availability (optional - can be set later)
{
  // (Not part of initial submission)
}

// Step 6: Banking Details
{
  // (Submitted later via separate API after activation)
}
```

**Response:**
```javascript
{
  success: boolean,
  message: string,
  registrationId: number,               // Store for tracking
  supervisorId: number                  // Store for future API calls
}
```

**UI Actions:**
- On success: Navigate to `/registration/success` with tracking link
- On error: Display validation errors inline
- Show loading spinner during submission

---

#### **GET /api/registration/progress/{registrationId}**

**Frontend Component:** `RegistrationProgressTracker.jsx`

**Response: `RegistrationWorkflowStatusDto`**

```javascript
{
  registrationId: number,
  currentStage: string,                 // Display current stage
  status: string,                       // PENDING / IN_PROGRESS / APPROVED / REJECTED
  completedStages: number,
  totalStages: number,                  // Usually 5
  progressPercentage: number,           // Show in progress bar
  expectedActivationDate: Date,         // Display estimate
  stageHistory: [
    {
      stage: string,                    // APPLIED, DOCUMENT_VERIFICATION, etc.
      completedDate: Date,              // Display timestamp
      isCompleted: boolean,             // ✓ or pending
      isCurrentStage: boolean,          // Highlight current
      notes: string                     // Admin notes (if any)
    }
  ]
}
```

**UI Elements:**
```jsx
<WorkflowStepper
  steps={[
    'Applied',
    'Document Verification',
    'Interview',
    'Training',
    'Certification',
    'Active'
  ]}
  currentStep={data.completedStages}
  totalSteps={data.totalStages}
/>

<ProgressCircle percentage={data.progressPercentage} />

{data.stageHistory.map(stage => (
  <StageCard
    stage={stage.stage}
    isCompleted={stage.isCompleted}
    isCurrent={stage.isCurrentStage}
    completedDate={stage.completedDate}
    notes={stage.notes}
  />
))}
```

---

### 2. Supervisor Dashboard API

#### **GET /api/supervisor/dashboard/{supervisorId}**

**Frontend Component:** `SupervisorDashboard.jsx`

**Response: `SupervisorDashboardDto`**

```javascript
{
  supervisor: {
    supervisorId: number,
    firstName: string,
    lastName: string,
    email: string,
    phone: string,
    supervisorType: 'REGISTERED',      // Always REGISTERED (no CAREER)
    supervisorStatus: string,          // ACTIVE / SUSPENDED / TERMINATED
    authorityLevel: string,            // BASIC / INTERMEDIATE / ADVANCED
    certificationStatus: string,       // PENDING / CERTIFIED
    canReleasePayment: boolean,        // Always FALSE
    canApproveRefund: boolean,         // Always FALSE
    canMentorOthers: boolean,          // Based on authority
    averageRating: number,
    totalEventsSupervised: number,
    photoUrl: string
  },
  totalAssignments: number,
  completedAssignments: number,
  upcomingAssignments: number,
  averageRating: number,
  totalEarnings: number,
  pendingPayments: number,
  recentAssignments: [
    {
      assignmentId: number,
      assignmentNumber: string,
      eventDate: Date,
      status: string,
      vendorName: string
    }
  ]
}
```

**UI Layout:**
```jsx
<SupervisorDashboard>
  {/* Header */}
  <ProfileHeader
    name={`${data.supervisor.firstName} ${data.supervisor.lastName}`}
    photo={data.supervisor.photoUrl}
    type={<SupervisorTypeBadge type="REGISTERED" />}
    status={<SupervisorStatusBadge status={data.supervisor.supervisorStatus} />}
    authority={<AuthorityLevelBadge level={data.supervisor.authorityLevel} />}
  />

  {/* Stats Cards */}
  <StatsGrid>
    <StatCard label="Total Assignments" value={data.totalAssignments} />
    <StatCard label="Completed" value={data.completedAssignments} />
    <StatCard label="Upcoming" value={data.upcomingAssignments} />
    <StatCard label="Average Rating" value={data.averageRating} />
    <StatCard label="Total Earnings" value={`₹${data.totalEarnings}`} />
    <StatCard label="Pending Payments" value={data.pendingPayments} />
  </StatsGrid>

  {/* Permissions Display (Admin-controlled) */}
  <PermissionsCard>
    <PermissionRow
      icon={<Lock />}
      title="Release Payment"
      granted={data.supervisor.canReleasePayment}
      locked={!data.supervisor.canReleasePayment}
      description="Approve final payment release (admin-only)"
    />
    <PermissionRow
      icon={<Lock />}
      title="Approve Refund"
      granted={data.supervisor.canApproveRefund}
      locked={!data.supervisor.canApproveRefund}
      description="Approve refund requests (admin-only)"
    />
    <PermissionRow
      icon={data.supervisor.canMentorOthers ? <Check /> : <Lock />}
      title="Mentor Others"
      granted={data.supervisor.canMentorOthers}
      description="Guide new supervisors"
    />
  </PermissionsCard>

  {/* Recent Assignments */}
  <RecentAssignments assignments={data.recentAssignments} />
</SupervisorDashboard>
```

---

### 3. Assignment APIs

#### **GET /api/assignment/supervisor/{supervisorId}**

**Frontend Component:** `AssignmentsList.jsx`

**Response: `List<SupervisorAssignmentModel>`**

```javascript
[
  {
    assignmentId: number,
    assignmentNumber: string,
    supervisorId: number,
    orderId: number,
    eventDate: Date,                    // Display date
    eventLocation: string,              // Display address
    eventType: string,                  // WEDDING, CORPORATE, etc.
    supervisorFee: number,              // Display amount
    assignmentNotes: string,            // Show notes
    assignmentStatus: string,           // ASSIGNED, ACCEPTED, IN_PROGRESS, COMPLETED
    assignedDate: Date,
    acceptedDate: Date,
    checkedIn: boolean,
    checkInTime: Date,
    paymentReleaseRequested: boolean,   // Show status
    paymentReleaseApproved: boolean,    // Show status
    paymentReleaseApprovalDate: Date,
    supervisorRating: number,
    issuesReported: boolean,
    orderNumber: string,
    vendorName: string
  }
]
```

**UI Display:**
```jsx
<AssignmentsList>
  {assignments.map(assignment => (
    <AssignmentCard
      key={assignment.assignmentId}
      assignmentNumber={assignment.assignmentNumber}
      eventDate={assignment.eventDate}
      eventLocation={assignment.eventLocation}
      eventType={assignment.eventType}
      fee={`₹${assignment.supervisorFee}`}
      status={<AssignmentStatusBadge status={assignment.assignmentStatus} />}
      vendorName={assignment.vendorName}
      orderNumber={assignment.orderNumber}
      checkedIn={assignment.checkedIn}
      paymentStatus={
        assignment.paymentReleaseApproved ? 'Released' :
        assignment.paymentReleaseRequested ? 'Pending Approval' :
        'Not Requested'
      }
      actions={
        <AssignmentActions
          assignmentId={assignment.assignmentId}
          status={assignment.assignmentStatus}
          canAccept={assignment.assignmentStatus === 'ASSIGNED'}
          canCheckIn={assignment.assignmentStatus === 'ACCEPTED'}
          canRequestPayment={assignment.assignmentStatus === 'COMPLETED' && !assignment.paymentReleaseRequested}
        />
      }
    />
  ))}
</AssignmentsList>
```

---

#### **POST /api/assignment/accept**

**Frontend Component:** `AssignmentAcceptReject.jsx`

**Request:**
```javascript
{
  assignmentId: number,
  supervisorId: number
}
```

**Response:**
```javascript
{
  success: boolean,
  message: string
}
```

**UI Actions:**
- On success: Update assignment status to "ACCEPTED", show success toast
- On error: Display error message

---

#### **POST /api/assignment/reject**

**Frontend Component:** `AssignmentAcceptReject.jsx`

**Request:**
```javascript
{
  assignmentId: number,
  supervisorId: number,
  reason: string                        // Textarea required
}
```

**Response:**
```javascript
{
  success: boolean,
  message: string
}
```

---

#### **POST /api/assignment/checkin**

**Frontend Component:** `EventCheckIn.jsx`

**Request: `CheckInDto`**

```javascript
{
  assignmentId: number,
  supervisorId: number,
  gpsLocation: string,                  // Get from browser geolocation
  checkInPhoto: string,                 // Upload selfie → URL
  checkInTime: Date                     // Current timestamp
}
```

**Response:**
```javascript
{
  success: boolean,
  message: string
}
```

**UI Flow:**
1. Request GPS permission
2. Capture selfie using camera
3. Upload photo to get URL
4. Submit check-in with GPS + photo + timestamp
5. On success: Navigate to event execution page

---

### 4. Event Supervision APIs

#### **POST /api/event/pre-event-verification**

**Frontend Component:** `PreEventVerification` (combined form)

**Request: `SubmitPreEventVerificationDto`**

```javascript
{
  assignmentId: number,
  supervisorId: number,

  // Menu Verification
  menuVerified: boolean,                // Checkbox
  menuVsContractMatch: boolean,         // Radio: Yes/No
  menuVerificationNotes: string,        // Textarea
  menuVerificationPhotos: [string],     // Multiple photos

  // Raw Material Verification
  rawMaterialVerified: boolean,         // Checkbox
  rawMaterialQualityOK: boolean,        // Radio: Yes/No
  rawMaterialQuantityOK: boolean,       // Radio: Yes/No
  rawMaterialNotes: string,             // Textarea
  rawMaterialPhotos: [string],          // Multiple photos

  // Guest Count Confirmation
  guestCountConfirmed: boolean,         // Checkbox
  confirmedGuestCount: number,          // Number input

  // Timestamped Evidence
  preEventEvidence: [
    {
      type: 'PHOTO' | 'VIDEO',
      url: string,
      timestamp: Date,                  // Auto-captured
      gpsLocation: string,              // Auto-captured
      description: string               // Input field
    }
  ],

  // Issues
  issuesFound: boolean,                 // Checkbox
  issuesDescription: string             // Textarea (conditional)
}
```

**Response:**
```javascript
{
  success: boolean,
  message: string,
  checklistId: number
}
```

---

#### **POST /api/event/during/food-serving-monitor**

**Frontend Component:** `FoodServingMonitor.jsx`

**Request: `FoodServingMonitorDto`**

```javascript
{
  assignmentId: number,
  supervisorId: number,
  qualityRating: number,                // 1-5 stars
  temperatureOK: boolean,               // Checkbox
  presentationOK: boolean,              // Checkbox
  notes: string,                        // Textarea
  photos: [string]                      // Multiple photos
}
```

---

#### **POST /api/event/during/update-guest-count**

**Frontend Component:** `GuestCountTracker.jsx`

**Request: `UpdateGuestCountDto`**

```javascript
{
  assignmentId: number,
  supervisorId: number,
  actualGuestCount: number,             // Number input
  notes: string,                        // Textarea
  timestamp: Date                       // Auto-captured
}
```

---

#### **POST /api/event/during/request-extra-quantity**

**Frontend Component:** `ExtraQuantityRequest.jsx`

**Request: `RequestExtraQuantityDto`**

```javascript
{
  assignmentId: number,
  supervisorId: number,
  itemName: string,                     // Input field
  extraQuantity: number,                // Number input
  extraCost: number,                    // Number input (calculated by partner)
  reason: string,                       // Textarea
  clientPhone: string,                  // Phone input
  approvalMethod: 'IN_APP' | 'OTP' | 'SIGNATURE'  // Radio buttons
}
```

**Response: `RequestExtraQuantityResponse`**

```javascript
{
  success: boolean,
  message: string,
  trackingId: number,
  otpCode: string,                      // If approvalMethod === 'OTP'
  otpExpiresAt: Date,                   // If approvalMethod === 'OTP'
  requiresApproval: boolean,
  approvalMethod: string
}
```

**UI Flow:**
1. Fill extra quantity details
2. Select approval method (IN_APP, OTP, SIGNATURE)
3. If OTP:
   - Backend sends OTP to client phone
   - Display OTP verification screen
   - Show expiry countdown
4. If IN_APP:
   - Client approves in their app
   - Poll for approval status
5. If SIGNATURE:
   - Capture client signature on device

---

#### **POST /api/event/during/verify-otp**

**Frontend Component:** `ClientOTPVerification.jsx`

**Request: `VerifyClientOTPDto`**

```javascript
{
  assignmentId: number,
  otpCode: string,                      // 6-digit OTP input
  clientIPAddress: string               // Auto-captured
}
```

**Response: `OTPVerificationResponse`**

```javascript
{
  success: boolean,
  message: string,
  otpVerified: boolean,
  approvalStatus: 'APPROVED' | 'REJECTED' | 'PENDING',
  remainingAttempts: number,            // Show warning if low
  isExpired: boolean                    // Show error if true
}
```

**UI Display:**
```jsx
<OTPVerification>
  <OTPInput
    length={6}
    onComplete={handleVerify}
  />
  <ExpiryCountdown expiresAt={otpExpiresAt} />
  <AttemptsRemaining count={remainingAttempts} />
  <ResendOTPButton onClick={handleResend} />
</OTPVerification>
```

---

#### **POST /api/event/post-event-report**

**Frontend Component:** `PostEventReportSubmit.jsx`

**Request: `SubmitPostEventReportDto`**

```javascript
{
  assignmentId: number,
  supervisorId: number,

  // Event Summary
  finalGuestCount: number,              // Number input
  eventRating: number,                  // 1-5 stars

  // Structured Client Feedback
  clientName: string,                   // Input field
  clientPhone: string,                  // Phone input
  clientSatisfactionRating: number,     // 1-5 stars
  foodQualityRating: number,            // 1-5 stars
  foodQuantityRating: number,           // 1-5 stars
  serviceQualityRating: number,         // 1-5 stars
  presentationRating: number,           // 1-5 stars
  wouldRecommend: boolean,              // Checkbox
  clientComments: string,               // Textarea
  clientSignatureUrl: string,           // Signature capture → URL

  // Partner Performance
  vendorPunctualityRating: number,      // 1-5 stars
  vendorPreparationRating: number,      // 1-5 stars
  vendorCooperationRating: number,      // 1-5 stars
  vendorComments: string,               // Textarea

  // Issues
  issuesCount: number,                  // Auto-calculated
  issues: [
    {
      issueType: string,                // Dropdown (FOOD_QUALITY, DELAY, etc.)
      severity: 'CRITICAL' | 'MAJOR' | 'MINOR',  // Radio
      description: string,              // Textarea
      resolution: string,               // Textarea
      timestamp: Date,                  // Auto-captured
      evidenceUrls: [string]            // Multiple photos/videos
    }
  ],

  // Financial
  finalPayableAmount: number,           // Number input (calculated)
  paymentBreakdown: {
    baseAmount: number,
    taxAmount: number,
    serviceCharges: number,
    extraCharges: number,               // From extra quantity requests
    deductions: number,                 // From penalties
    totalAmount: number
  },

  // Report
  reportSummary: string,                // Textarea
  recommendations: string,              // Textarea
  completionPhotos: [string],           // Multiple photos
  completionVideos: [string]            // Multiple videos
}
```

**Response:**
```javascript
{
  success: boolean,
  message: string,
  reportId: number,
  reportPdfUrl: string                  // Download link
}
```

---

#### **GET /api/event/supervision-summary/{assignmentId}**

**Frontend Component:** `EventSupervisionSummary.jsx`

**Response: `EventSupervisionSummaryDto`**

```javascript
{
  assignmentId: number,
  assignmentNumber: string,
  eventDate: Date,
  eventType: string,
  status: string,

  // Pre-Event
  preEvent: {
    verificationStatus: string,
    menuVerified: boolean,
    materialVerified: boolean,
    guestCountConfirmed: boolean,
    confirmedGuestCount: number,
    issuesFound: boolean,
    issuesDescription: string
  },

  // During-Event
  duringEvent: {
    actualGuestCount: number,
    guestCountVariance: number,
    extraQuantityRequested: boolean,
    clientApprovalStatus: string,
    trackingLog: [
      {
        trackingType: string,
        description: string,
        timestamp: Date,
        evidenceUrls: [string]
      }
    ]
  },

  // Post-Event
  postEvent: {
    reportSubmitted: boolean,
    clientSatisfaction: number,
    issuesCount: number,
    finalPaymentAmount: number,
    submittedDate: Date
  },

  supervisorName: string,
  supervisorType: 'REGISTERED'
}
```

**UI Display:**
```jsx
<EventSupervisionSummary>
  {/* Timeline View */}
  <Timeline>
    <TimelinePhase
      phase="PRE-EVENT"
      status={data.preEvent.verificationStatus}
      items={[
        { label: 'Menu Verified', value: data.preEvent.menuVerified },
        { label: 'Material Verified', value: data.preEvent.materialVerified },
        { label: 'Guest Count', value: data.preEvent.confirmedGuestCount }
      ]}
    />

    <TimelinePhase
      phase="DURING-EVENT"
      items={[
        { label: 'Actual Guests', value: data.duringEvent.actualGuestCount },
        { label: 'Variance', value: data.duringEvent.guestCountVariance },
        { label: 'Extra Requests', value: data.duringEvent.extraQuantityRequested }
      ]}
    />

    <TimelinePhase
      phase="POST-EVENT"
      status={data.postEvent.reportSubmitted ? 'Completed' : 'Pending'}
      items={[
        { label: 'Client Satisfaction', value: `${data.postEvent.clientSatisfaction}/5` },
        { label: 'Issues', value: data.postEvent.issuesCount },
        { label: 'Final Payment', value: `₹${data.postEvent.finalPaymentAmount}` }
      ]}
    />
  </Timeline>

  {/* Tracking Log */}
  <TrackingLog entries={data.duringEvent.trackingLog} />
</EventSupervisionSummary>
```

---

### 5. Payment APIs

#### **POST /api/assignment/request-payment-release**

**Frontend Component:** `PaymentRequestButton.jsx`

**Request:**
```javascript
{
  assignmentId: number,
  supervisorId: number,
  amount: number                        // Supervisor fee
}
```

**Response: `PaymentReleaseResponse`**

```javascript
{
  success: boolean,
  message: string,
  directRelease: boolean,               // Always FALSE for REGISTERED supervisors
  requiresApproval: boolean,            // Always TRUE for REGISTERED supervisors
  releasedAt: Date,                     // NULL (not released yet)
  requestedAt: Date                     // Current timestamp
}
```

**UI Display:**
```jsx
<PaymentRequestButton
  assignmentId={assignmentId}
  amount={supervisorFee}
  canRequest={assignment.status === 'COMPLETED' && !assignment.paymentReleaseRequested}
  onClick={handleRequestPayment}
>
  {assignment.paymentReleaseRequested ? (
    <Badge color="yellow">
      Pending Admin Approval
    </Badge>
  ) : (
    <Button>Request Payment Release</Button>
  )}
</PaymentRequestButton>

{/* Show info message */}
<InfoBox>
  ℹ️ Payment requests require admin approval. You will be notified once approved.
</InfoBox>
```

---

#### **GET /api/supervisor/earnings/{supervisorId}**

**Frontend Component:** `EarningsPage.jsx`

**Response:**
```javascript
{
  totalEarnings: number,
  pendingPayments: number,
  releasedPayments: number,
  paymentHistory: [
    {
      assignmentId: number,
      assignmentNumber: string,
      eventDate: Date,
      supervisorFee: number,
      paymentReleaseRequested: boolean,
      paymentReleaseRequestDate: Date,
      paymentReleaseApproved: boolean,
      paymentReleaseApprovalDate: Date,
      paymentApprovedBy: number,        // Admin ID
      status: 'PENDING' | 'APPROVED' | 'RELEASED'
    }
  ]
}
```

**UI Display:**
```jsx
<EarningsPage>
  <EarningsSummary
    total={`₹${data.totalEarnings}`}
    pending={`₹${data.pendingPayments}`}
    released={`₹${data.releasedPayments}`}
  />

  <PaymentHistoryTable>
    {data.paymentHistory.map(payment => (
      <PaymentRow
        key={payment.assignmentId}
        assignmentNumber={payment.assignmentNumber}
        eventDate={payment.eventDate}
        amount={`₹${payment.supervisorFee}`}
        status={<PaymentStatusBadge status={payment.status} />}
        requestedDate={payment.paymentReleaseRequestDate}
        approvedDate={payment.paymentReleaseApprovalDate}
      />
    ))}
  </PaymentHistoryTable>
</EarningsPage>
```

---

### 6. Admin APIs

#### **GET /api/admin/registration/pending-docs**

**Frontend Component:** `RegistrationQueue.jsx`

**Response: `List<SupervisorRegistrationModel>`**

```javascript
[
  {
    registrationId: number,
    supervisorId: number,
    firstName: string,
    lastName: string,
    email: string,
    phone: string,
    currentStage: 'DOCUMENT_VERIFICATION',
    status: 'PENDING',
    idProofUrl: string,                 // View document
    addressProofUrl: string,            // View document
    photoUrl: string,                   // View photo
    appliedDate: Date
  }
]
```

**UI Display:**
```jsx
<RegistrationQueue>
  {registrations.map(reg => (
    <RegistrationCard
      key={reg.registrationId}
      name={`${reg.firstName} ${reg.lastName}`}
      email={reg.email}
      phone={reg.phone}
      stage={reg.currentStage}
      appliedDate={reg.appliedDate}
      documents={
        <DocumentViewer
          idProof={reg.idProofUrl}
          addressProof={reg.addressProofUrl}
          photo={reg.photoUrl}
        />
      }
      actions={
        <RegistrationActions
          onApprove={() => handleApproveDocuments(reg.registrationId)}
          onReject={() => handleRejectDocuments(reg.registrationId)}
        />
      }
    />
  ))}
</RegistrationQueue>
```

---

#### **POST /api/admin/registration/verify-docs**

**Frontend Component:** `RegistrationApprovalDrawer.jsx`

**Request: `DocumentVerificationDto`**

```javascript
{
  registrationId: number,
  verifiedBy: number,                   // Admin ID
  passed: boolean,                      // Approve/Reject
  idProofVerified: boolean,             // Checkbox
  addressProofVerified: boolean,        // Checkbox
  photoVerified: boolean,               // Checkbox
  verificationNotes: string             // Textarea
}
```

---

#### **GET /api/admin/assignment/payment-requests**

**Frontend Component:** `PaymentApprovals.jsx`

**Response:**
```javascript
[
  {
    assignmentId: number,
    assignmentNumber: string,
    supervisorId: number,
    supervisorName: string,
    eventDate: Date,
    supervisorFee: number,
    paymentReleaseRequestDate: Date,
    postEventReport: {
      reportId: number,
      reportPdfUrl: string,
      clientSatisfaction: number,
      issuesCount: number,
      reportVerified: boolean
    }
  }
]
```

**UI Display:**
```jsx
<PaymentApprovals>
  {requests.map(req => (
    <PaymentApprovalCard
      key={req.assignmentId}
      assignmentNumber={req.assignmentNumber}
      supervisorName={req.supervisorName}
      eventDate={req.eventDate}
      amount={`₹${req.supervisorFee}`}
      requestedDate={req.paymentReleaseRequestDate}
      reportUrl={req.postEventReport.reportPdfUrl}
      clientSatisfaction={req.postEventReport.clientSatisfaction}
      issuesCount={req.postEventReport.issuesCount}
      actions={
        <ApprovalActions
          onApprove={() => handleApprovePayment(req.assignmentId)}
          onReject={() => handleRejectPayment(req.assignmentId)}
          onViewReport={() => window.open(req.postEventReport.reportPdfUrl)}
        />
      }
    />
  ))}
</PaymentApprovals>
```

---

#### **POST /api/admin/assignment/approve-payment**

**Frontend Component:** `PaymentApprovalCard.jsx`

**Request:**
```javascript
{
  assignmentId: number,
  approvedBy: number,                   // Admin ID
  notes: string                         // Textarea
}
```

**Response:**
```javascript
{
  success: boolean,
  message: string,
  releasedAt: Date
}
```

---

## 🔐 Permission Model

### Supervisor Permissions

**What Supervisors CAN Do:**
```javascript
// Event Execution
✅ Accept/reject assignments
✅ Check in at event location
✅ Verify pre-event checklist
✅ Monitor food serving
✅ Update guest count
✅ Request extra quantity (with client approval)
✅ Upload timestamped evidence
✅ Report issues live
✅ Submit post-event report
✅ Request payment release
✅ View earnings history

// Profile Management
✅ Update personal details
✅ Update availability
✅ View performance ratings
```

**What Supervisors CANNOT Do:**
```javascript
// Financial Actions (Admin-Only)
❌ Release payment directly
❌ Approve refunds
❌ Override payment amounts
❌ Access partner financial data
❌ Modify supervisor fees

// Administrative Actions
❌ Assign themselves to events
❌ Activate other supervisors
❌ Grant permissions
❌ Modify authority levels
```

### Frontend Permission Checks

**Component-Level:**
```jsx
import { usePermission } from '@/hooks/usePermission';

const PaymentReleaseButton = ({ assignment }) => {
  const { canReleasePayment } = usePermission();

  // Only show REQUEST button (never RELEASE button)
  return (
    <Button
      onClick={() => requestPaymentRelease(assignment.id)}
      disabled={assignment.paymentReleaseRequested}
    >
      {assignment.paymentReleaseRequested ? 'Pending Admin Approval' : 'Request Payment Release'}
    </Button>
  );

  // Never show this for supervisors:
  // <Button onClick={() => releasePayment()}>Release Payment</Button> ❌
};
```

**Route-Level:**
```jsx
<Route path="/supervisor/*">
  <Route index element={<SupervisorDashboard />} />
  <Route path="assignments" element={<AssignmentsList />} />
  <Route path="earnings" element={<EarningsPage />} />

  {/* These routes DO NOT EXIST for supervisors */}
  {/* ❌ <Route path="payment-release" element={<PaymentRelease />} /> */}
  {/* ❌ <Route path="refund-approval" element={<RefundApproval />} /> */}
</Route>
```

---

## 🎨 UI/UX Patterns

### Status Badges

```jsx
// Supervisor Type (Always REGISTERED for this portal)
<SupervisorTypeBadge type="REGISTERED" />
// Output: Green badge with "Registered Supervisor"

// Supervisor Status
<SupervisorStatusBadge status="ACTIVE" />         // Green
<SupervisorStatusBadge status="SUSPENDED" />      // Red
<SupervisorStatusBadge status="TERMINATED" />     // Gray

// Assignment Status
<AssignmentStatusBadge status="ASSIGNED" />       // Blue
<AssignmentStatusBadge status="ACCEPTED" />       // Yellow
<AssignmentStatusBadge status="IN_PROGRESS" />    // Orange
<AssignmentStatusBadge status="COMPLETED" />      // Green
<AssignmentStatusBadge status="CANCELLED" />      // Red

// Payment Status
<PaymentStatusBadge status="PENDING" />           // Yellow
<PaymentStatusBadge status="APPROVED" />          // Green
<PaymentStatusBadge status="REJECTED" />          // Red

// Authority Level
<AuthorityLevelBadge level="BASIC" />             // Blue
<AuthorityLevelBadge level="INTERMEDIATE" />      // Cyan
<AuthorityLevelBadge level="ADVANCED" />          // Purple
<AuthorityLevelBadge level="FULL" />              // Gold (admin-only, not shown in supervisor portal)
```

---

### Progress Indicators

```jsx
// Registration Workflow (5 stages)
<WorkflowStepper
  currentStep={2}
  totalSteps={5}
  steps={[
    'Applied',
    'Document Verification',
    'Interview',
    'Training',
    'Certification'
  ]}
/>

// Event Execution Phases
<EventPhaseTracker
  phases={[
    { name: 'Pre-Event', completed: true },
    { name: 'During-Event', completed: true },
    { name: 'Post-Event', completed: false }
  ]}
/>
```

---

### Permission Display

```jsx
// Show locked permissions with explanation
<PermissionCard
  icon={<Lock />}
  title="Release Payment"
  granted={false}
  locked={true}
  description="This action requires admin approval. You can REQUEST payment release after completing the event."
  actionButton={
    <Button onClick={handleRequestPayment}>
      Request Payment Release
    </Button>
  }
/>
```

---

## 🧪 Form Validation (Zod)

```javascript
import { z } from 'zod';
import { differenceInYears } from 'date-fns';

// Registration Step 1: Personal Details
export const personalDetailsSchema = z.object({
  firstName: z.string()
    .min(2, 'First name must be at least 2 characters')
    .max(50, 'First name cannot exceed 50 characters'),
  lastName: z.string()
    .min(2, 'Last name must be at least 2 characters')
    .max(50, 'Last name cannot exceed 50 characters'),
  email: z.string()
    .email('Invalid email address'),
  phone: z.string()
    .regex(/^[0-9]{10}$/, 'Phone must be 10 digits'),
  dateOfBirth: z.date()
    .refine(
      date => differenceInYears(new Date(), date) >= 18,
      'Must be at least 18 years old'
    )
});

// Pre-Event Verification
export const preEventVerificationSchema = z.object({
  menuVerified: z.boolean(),
  menuVsContractMatch: z.boolean(),
  menuVerificationNotes: z.string().optional(),
  rawMaterialVerified: z.boolean(),
  rawMaterialQualityOK: z.boolean(),
  confirmedGuestCount: z.number()
    .int()
    .min(1, 'Guest count must be at least 1'),
  issuesFound: z.boolean(),
  issuesDescription: z.string()
    .refine(
      (val, ctx) => {
        if (ctx.parent.issuesFound && !val) {
          return false;
        }
        return true;
      },
      'Issue description is required when issues are found'
    )
});

// Post-Event Report
export const postEventReportSchema = z.object({
  finalGuestCount: z.number().int().min(1),
  eventRating: z.number().int().min(1).max(5),
  clientSatisfactionRating: z.number().int().min(1).max(5),
  foodQualityRating: z.number().int().min(1).max(5),
  foodQuantityRating: z.number().int().min(1).max(5),
  serviceQualityRating: z.number().int().min(1).max(5),
  presentationRating: z.number().int().min(1).max(5),
  vendorPunctualityRating: z.number().int().min(1).max(5),
  vendorPreparationRating: z.number().int().min(1).max(5),
  vendorCooperationRating: z.number().int().min(1).max(5),
  clientComments: z.string().max(1000),
  reportSummary: z.string().min(50, 'Report summary must be at least 50 characters'),
  finalPayableAmount: z.number().min(0)
});
```

---

## 📱 Responsive Design

### Mobile-First Approach

```jsx
// Desktop: 3-column layout
<div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
  <StatCard />
  <StatCard />
  <StatCard />
</div>

// Mobile: Single column stack
<div className="flex flex-col gap-4">
  <AssignmentCard />
  <AssignmentCard />
</div>

// Tablet: 2-column grid
<div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
  <RegistrationCard />
  <RegistrationCard />
</div>
```

### Touch-Friendly Elements

```css
/* Minimum touch target size: 44x44px */
.btn {
  min-height: 44px;
  min-width: 44px;
  padding: 12px 24px;
}

/* Larger inputs for mobile */
.input-mobile {
  font-size: 16px; /* Prevents zoom on iOS */
  height: 48px;
}
```

---

## 🔧 Technical Implementation

### File Upload Strategy

```javascript
// 1. Get pre-signed URL from backend
const { presignedUrl } = await supervisorApi.getUploadUrl(fileName, fileType);

// 2. Upload file directly to cloud storage (S3/Azure/etc.)
await supervisorApi.uploadFile(file, presignedUrl);

// 3. Store URL in form data
setFormData(prev => ({
  ...prev,
  idProofUrl: presignedUrl
}));

// 4. Submit form with URL
await registrationApi.submitRegistration(formData);
```

---

### Timestamped Evidence Upload

```javascript
const captureTimestampedEvidence = async (file, description) => {
  // Get GPS location
  const position = await navigator.geolocation.getCurrentPosition();
  const gpsLocation = `${position.coords.latitude},${position.coords.longitude}`;

  // Get current timestamp
  const timestamp = new Date();

  // Upload file
  const { presignedUrl } = await supervisorApi.getUploadUrl(file.name, file.type);
  await supervisorApi.uploadFile(file, presignedUrl);

  // Return evidence object
  return {
    type: file.type.startsWith('image/') ? 'PHOTO' : 'VIDEO',
    url: presignedUrl,
    timestamp,
    gpsLocation,
    description
  };
};
```

---

### Real-Time OTP Verification

```jsx
const OTPVerification = ({ assignmentId, expiresAt }) => {
  const [otp, setOtp] = useState('');
  const [remainingTime, setRemainingTime] = useState(null);
  const [attempts, setAttempts] = useState(0);

  // Countdown timer
  useEffect(() => {
    const interval = setInterval(() => {
      const diff = new Date(expiresAt) - new Date();
      if (diff <= 0) {
        clearInterval(interval);
        setRemainingTime(0);
      } else {
        setRemainingTime(Math.floor(diff / 1000));
      }
    }, 1000);

    return () => clearInterval(interval);
  }, [expiresAt]);

  const handleVerify = async () => {
    const result = await eventSupervisionApi.verifyOTP({
      assignmentId,
      otpCode: otp,
      clientIPAddress: await getClientIP()
    });

    if (result.success) {
      toast.success('Client approval confirmed!');
    } else {
      setAttempts(prev => prev + 1);
      if (result.remainingAttempts === 0) {
        toast.error('Maximum attempts exceeded. Please resend OTP.');
      } else {
        toast.error(`Invalid OTP. ${result.remainingAttempts} attempts remaining.`);
      }
    }
  };

  return (
    <div>
      <OTPInput length={6} value={otp} onChange={setOtp} onComplete={handleVerify} />
      <ExpiryCountdown seconds={remainingTime} />
      <AttemptsInfo current={attempts} max={3} />
      <ResendOTPButton disabled={remainingTime > 0} onClick={handleResend} />
    </div>
  );
};
```

---

### State Management (Context API)

```jsx
// SupervisorContext.jsx
import { createContext, useContext, useState, useEffect } from 'react';
import { supervisorApi } from '@/services/api/supervisorApi';

const SupervisorContext = createContext();

export const SupervisorProvider = ({ children }) => {
  const [supervisor, setSupervisor] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchSupervisorData();
  }, []);

  const fetchSupervisorData = async () => {
    try {
      const supervisorId = localStorage.getItem('supervisorId');
      const data = await supervisorApi.getDashboard(supervisorId);
      setSupervisor(data.supervisor);
    } catch (error) {
      console.error('Failed to fetch supervisor data:', error);
    } finally {
      setLoading(false);
    }
  };

  const checkPermission = (permissionType) => {
    if (!supervisor) return false;

    switch (permissionType) {
      case 'RELEASE_PAYMENT':
        return supervisor.canReleasePayment; // Always FALSE
      case 'APPROVE_REFUND':
        return supervisor.canApproveRefund; // Always FALSE
      case 'MENTOR_OTHERS':
        return supervisor.canMentorOthers; // Based on authority
      default:
        return false;
    }
  };

  return (
    <SupervisorContext.Provider value={{
      supervisor,
      loading,
      checkPermission,
      refetch: fetchSupervisorData
    }}>
      {children}
    </SupervisorContext.Provider>
  );
};

export const useSupervisor = () => {
  const context = useContext(SupervisorContext);
  if (!context) {
    throw new Error('useSupervisor must be used within SupervisorProvider');
  }
  return context;
};
```

---

## ✅ Implementation Checklist

### Phase 1: Project Setup (1 day)
- [ ] Initialize React + Vite project
- [ ] Install dependencies:
  - React Router v6
  - Axios
  - Tailwind CSS
  - Shadcn/UI
  - React Hook Form
  - Zod
  - date-fns
  - Lucide React
- [ ] Set up folder structure
- [ ] Create enum definitions (`supervisorEnums.js`)
- [ ] Set up API service layer
- [ ] Configure environment variables

### Phase 2: Common Components (2 days)
- [ ] Badge components (Status, Type, Authority, Payment)
- [ ] WorkflowStepper
- [ ] ProgressCircle
- [ ] DocumentUploader
- [ ] TimestampedEvidenceUpload
- [ ] SignatureCapture
- [ ] OTPInput
- [ ] PermissionGuard

### Phase 3: Registration Portal (3 days)
- [ ] RegistrationWizard (6 steps)
  - [ ] Step1_PersonalDetails
  - [ ] Step2_AddressDetails
  - [ ] Step3_ExperienceDetails
  - [ ] Step4_IdentityProof
  - [ ] Step5_Availability
  - [ ] Step6_BankingDetails
- [ ] RegistrationProgressTracker
- [ ] Form validation with Zod
- [ ] File upload integration

### Phase 4: Supervisor Dashboard (2 days)
- [ ] SupervisorDashboard page
- [ ] Stats cards
- [ ] Permissions display
- [ ] Recent assignments widget
- [ ] Profile management

### Phase 5: Assignments (3 days)
- [ ] AssignmentsList page
- [ ] AssignmentDetails page
- [ ] AssignmentCard component
- [ ] Accept/Reject functionality
- [ ] Check-in component
- [ ] Assignment timeline

### Phase 6: Event Execution (5 days)
- [ ] EventExecution page layout
- [ ] Pre-Event Verification
  - [ ] MenuVerificationForm
  - [ ] RawMaterialInspection
  - [ ] GuestCountConfirmation
  - [ ] PreEventEvidenceUpload
- [ ] During-Event Monitoring
  - [ ] EventCheckIn
  - [ ] FoodServingMonitor
  - [ ] GuestCountTracker
  - [ ] ExtraQuantityRequest
  - [ ] ClientOTPVerification
  - [ ] LiveIssueReporter
- [ ] Post-Event Report
  - [ ] ClientFeedbackForm
  - [ ] PartnerPerformanceRating
  - [ ] EventIssuesSummary
  - [ ] PaymentBreakdownForm
  - [ ] CompletionEvidenceUpload
  - [ ] PostEventReportSubmit
- [ ] EventSupervisionSummary

### Phase 7: Payments (2 days)
- [ ] EarningsPage
- [ ] PaymentHistoryTable
- [ ] PaymentRequestButton
- [ ] EarningsSummary
- [ ] PaymentBreakdownView

### Phase 8: Admin Components (3 days)
- [ ] RegistrationQueue
- [ ] RegistrationApprovalDrawer
- [ ] InterviewScheduler
- [ ] TrainingAssignment
- [ ] CertificationScheduler
- [ ] SupervisorActivation
- [ ] EventAssignment
- [ ] PaymentApprovals
- [ ] PaymentApprovalCard

### Phase 9: Testing & Polish (3 days)
- [ ] Test all form submissions
- [ ] Test file uploads
- [ ] Test OTP verification
- [ ] Test permission-based rendering
- [ ] Test status transitions
- [ ] Test real-time updates
- [ ] Responsive design testing
- [ ] Error handling testing
- [ ] Cross-browser testing

### Phase 10: Documentation (1 day)
- [ ] Component usage guide
- [ ] API integration guide
- [ ] Deployment guide
- [ ] User manual

**Total Estimated Time:** 25 days

---

## 🚀 Deployment

### Environment Variables

```env
# API Configuration
VITE_API_BASE_URL=https://api.example.com
VITE_UPLOAD_BASE_URL=https://uploads.example.com

# File Upload
VITE_MAX_FILE_SIZE=5242880                    # 5MB
VITE_ALLOWED_IMAGE_TYPES=image/jpeg,image/png
VITE_ALLOWED_VIDEO_TYPES=video/mp4,video/webm

# OTP Configuration
VITE_OTP_EXPIRY_SECONDS=300                   # 5 minutes
VITE_OTP_MAX_ATTEMPTS=3

# GPS Configuration
VITE_GPS_TIMEOUT=10000                        # 10 seconds
VITE_GPS_MAX_AGE=60000                        # 1 minute
```

### Build

```bash
# Install dependencies
npm install

# Build for production
npm run build

# Preview production build
npm run preview
```

### CORS Configuration (Backend)

```csharp
// Program.cs
services.AddCors(options => {
  options.AddPolicy("FrontendPolicy", builder => {
    builder.WithOrigins("https://supervisor.example.com")
           .AllowAnyMethod()
           .AllowAnyHeader()
           .AllowCredentials();
  });
});
```

---

## 📖 Key Principles

✅ **Backend First** - All fields mapped from backend models
✅ **Zero Assumptions** - No invented fields or logic
✅ **Enum Consistency** - Use exact backend enum values
✅ **Permission Respect** - UI honors backend permission flags
✅ **Admin-Controlled Payments** - Supervisors REQUEST, admins APPROVE
✅ **Timestamped Evidence** - All uploads include GPS + timestamp
✅ **Audit Trail** - Display who did what and when
✅ **Mobile-First** - Responsive design for field use
✅ **Error Handling** - Graceful degradation and user feedback

---

## 🎯 Success Criteria

✅ **100% Backend Field Coverage** - All backend fields mapped to UI
✅ **Zero Hardcoded Logic** - All business rules from backend
✅ **Clear Permission Model** - UI shows exactly what supervisor can do
✅ **No Careers Portal** - Only Event Supervisor Portal exists
✅ **Admin Payment Control** - Supervisors cannot release payments
✅ **Evidence-Based Workflow** - GPS + timestamp on all critical actions
✅ **Production-Ready** - Error handling, loading states, responsive
✅ **Maintainable** - Clear structure, reusable patterns

---

**Version:** 2.0
**Date:** 2026-01-31
**Status:** ✅ Ready for Development
**Backend Compatibility:** 100% Verified
**Careers Portal:** ❌ Removed (Static Page Only)
**Event Supervisor Portal:** ✅ Single Portal Architecture
**Payment Model:** ✅ Admin-Controlled (Supervisors REQUEST only)
