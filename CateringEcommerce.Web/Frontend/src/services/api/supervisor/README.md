# Supervisor API Services

This directory contains all API service modules for the Event Supervisor Portal.

## Structure

```
supervisor/
├── apiConfig.js              # Axios configuration, interceptors, error handling
├── supervisorApi.js          # Core supervisor CRUD operations
├── registrationApi.js        # Registration workflow APIs
├── assignmentApi.js          # Assignment management APIs
├── eventSupervisionApi.js    # Event execution (Pre/During/Post) APIs
└── index.js                  # Central export
```

## Usage

### Import all APIs

```javascript
import supervisorApis from '@/services/api/supervisor';

// Use APIs
const dashboard = await supervisorApis.supervisor.getDashboard(supervisorId);
const registration = await supervisorApis.registration.submitRegistration(data);
```

### Import specific APIs

```javascript
import { supervisorApi, registrationApi } from '@/services/api/supervisor';

const dashboard = await supervisorApi.getDashboard(supervisorId);
const progress = await registrationApi.getRegistrationProgress(regId);
```

## API Modules

### supervisorApi.js
- `getDashboard(supervisorId)` - Get dashboard data
- `getSupervisorById(supervisorId)` - Get supervisor details
- `updateSupervisor(supervisorId, updates)` - Update profile
- `updateAvailability(supervisorId, slots)` - Update availability
- `getAvailability(supervisorId, date)` - Get availability
- `checkAuthority(supervisorId, actionType)` - Check permissions

### registrationApi.js
- `submitRegistration(registration)` - Submit new registration
- `getRegistrationProgress(registrationId)` - Track workflow progress
- `submitBankingDetails(bankingDetails)` - Submit bank account info

**Admin APIs:**
- `getPendingDocumentVerification()` - Get pending docs
- `verifyDocuments(verification)` - Approve/reject documents
- `scheduleInterview(interview)` - Schedule interview
- `activateSupervisor(registrationId, activatedBy)` - Activate supervisor

### assignmentApi.js
- `getAssignmentsBySupervisor(supervisorId, status)` - Get assignments
- `getAssignmentById(assignmentId)` - Get single assignment
- `acceptAssignment(assignmentId, supervisorId)` - Accept assignment
- `rejectAssignment(assignmentId, supervisorId, reason)` - Reject assignment
- `checkIn(checkInData)` - Check in at event
- `requestPaymentRelease(assignmentId, supervisorId, amount)` - Request payment
- `getEarnings(supervisorId)` - Get earnings history

**Admin APIs:**
- `findEligibleSupervisors(criteria)` - Find available supervisors
- `assignSupervisorToEvent(assignment)` - Assign supervisor
- `getPaymentRequests()` - Get pending payment requests
- `approvePaymentRelease(assignmentId, approvedBy, notes)` - Approve payment

### eventSupervisionApi.js

**Pre-Event:**
- `submitPreEventVerification(verification)` - Submit pre-event checklist
- `getPreEventVerification(assignmentId)` - Get verification details

**During-Event:**
- `recordFoodServingMonitor(monitorData)` - Monitor food quality
- `updateGuestCount(guestCountData)` - Update guest count
- `requestExtraQuantity(extraQuantityRequest)` - Request extras
- `verifyClientOTP(otpData)` - Verify client OTP
- `resendClientOTP(assignmentId, purpose)` - Resend OTP
- `reportIssue(issueData)` - Report live issue

**Post-Event:**
- `submitPostEventReport(report)` - Submit completion report
- `getPostEventReport(assignmentId)` - Get report details

**Summary:**
- `getEventSupervisionSummary(assignmentId)` - Get Pre+During+Post summary

## Error Handling

All APIs use standardized error handling:

```javascript
const result = await supervisorApi.getDashboard(id);

if (result.success) {
  console.log(result.data);
} else {
  console.error(result.message);
  console.error(result.errors); // Validation errors if any
}
```

## Authentication

APIs automatically include the auth token from localStorage:

```javascript
// Token is added by interceptor
localStorage.setItem('supervisorToken', token);
```

## File Uploads

Use the upload helpers:

```javascript
import { getUploadUrl, uploadFile } from '@/services/api/supervisor';

// 1. Get pre-signed URL
const { data } = await getUploadUrl(fileName, fileType);

// 2. Upload file
await uploadFile(file, data.presignedUrl);

// 3. Use URL in API call
await registrationApi.submitRegistration({
  ...data,
  idProofUrl: data.presignedUrl
});
```

## Environment Variables

Required in `.env`:

```env
VITE_API_BASE_URL=https://localhost:44368
VITE_UPLOAD_BASE_URL=https://localhost:44368/uploads
```
