/**
 * Supervisor System Enums
 * Maps directly to backend CateringEcommerce.Domain.Models.Supervisor enums
 * DO NOT modify these values - they must match backend exactly
 */

// =====================================================
// SUPERVISOR TYPE
// =====================================================
export const SupervisorType = {
  CAREER: 'CAREER',           // Core supervisors (NOT USED in this portal)
  REGISTERED: 'REGISTERED'    // Event supervisors (ONLY TYPE in this portal)
};

// =====================================================
// SUPERVISOR STATUS
// =====================================================
export const SupervisorStatus = {
  APPLIED: 'APPLIED',                           // Initial application submitted
  DOCUMENT_VERIFICATION: 'DOCUMENT_VERIFICATION', // Verifying ID/address proof
  AWAITING_INTERVIEW: 'AWAITING_INTERVIEW',     // Interview scheduled
  INTERVIEW_PASSED: 'INTERVIEW_PASSED',         // Interview completed successfully
  AWAITING_TRAINING: 'AWAITING_TRAINING',       // Training assigned
  TRAINING_IN_PROGRESS: 'TRAINING_IN_PROGRESS', // Currently in training
  AWAITING_CERTIFICATION: 'AWAITING_CERTIFICATION', // Certification exam scheduled
  CERTIFIED: 'CERTIFIED',                       // Certification passed
  ACTIVE: 'ACTIVE',                             // Activated, can receive assignments
  SUSPENDED: 'SUSPENDED',                       // Temporarily suspended
  TERMINATED: 'TERMINATED',                     // Permanently terminated
  REJECTED: 'REJECTED'                          // Application rejected
};

// =====================================================
// AUTHORITY LEVEL
// =====================================================
export const AuthorityLevel = {
  BASIC: 'BASIC',             // Can check-in, verify, monitor
  INTERMEDIATE: 'INTERMEDIATE', // Basic + can handle minor issues
  ADVANCED: 'ADVANCED',       // Intermediate + can mentor others
  FULL: 'FULL'                // Advanced + payment release (ADMIN ONLY - not used in supervisor portal)
};

// =====================================================
// COMPENSATION TYPE
// =====================================================
export const CompensationType = {
  PER_EVENT: 'PER_EVENT',     // Paid per event (REGISTERED supervisors)
  MONTHLY_SALARY: 'MONTHLY_SALARY', // Monthly salary (CAREER supervisors - not used)
  HYBRID: 'HYBRID'            // Salary + event bonus (CAREER - not used)
};

// =====================================================
// CERTIFICATION STATUS
// =====================================================
export const CertificationStatus = {
  PENDING: 'PENDING',         // Not yet certified
  PASSED: 'PASSED',           // Certification passed
  FAILED: 'FAILED',           // Certification failed
  EXPIRED: 'EXPIRED'          // Certification expired
};

// =====================================================
// ASSIGNMENT STATUS
// =====================================================
export const AssignmentStatus = {
  ASSIGNED: 'ASSIGNED',       // Admin assigned supervisor to event
  ACCEPTED: 'ACCEPTED',       // Supervisor accepted assignment
  REJECTED: 'REJECTED',       // Supervisor rejected assignment
  IN_PROGRESS: 'IN_PROGRESS', // Event in progress
  COMPLETED: 'COMPLETED',     // Event completed, report submitted
  CANCELLED: 'CANCELLED'      // Assignment cancelled
};

// =====================================================
// REGISTRATION STAGES
// =====================================================
export const RegistrationStage = {
  APPLIED: 'APPLIED',
  DOCUMENT_VERIFICATION: 'DOCUMENT_VERIFICATION',
  AWAITING_INTERVIEW: 'AWAITING_INTERVIEW',
  AWAITING_TRAINING: 'AWAITING_TRAINING',
  AWAITING_CERTIFICATION: 'AWAITING_CERTIFICATION',
  ACTIVE: 'ACTIVE'
};

// =====================================================
// DURING EVENT TRACKING TYPE
// =====================================================
export const DuringEventTrackingType = {
  GUEST_COUNT_UPDATE: 'GUEST_COUNT_UPDATE',
  FOOD_SERVING_CHECK: 'FOOD_SERVING_CHECK',
  EXTRA_QUANTITY_REQUEST: 'EXTRA_QUANTITY_REQUEST',
  CLIENT_APPROVAL: 'CLIENT_APPROVAL',
  ISSUE_REPORTED: 'ISSUE_REPORTED',
  QUALITY_CHECK: 'QUALITY_CHECK'
};

// =====================================================
// CLIENT APPROVAL METHOD
// =====================================================
export const ClientApprovalMethod = {
  IN_APP: 'IN_APP',           // Client approves in their app
  OTP: 'OTP',                 // OTP sent to client phone
  SIGNATURE: 'SIGNATURE'      // Client signature on device
};

// =====================================================
// CLIENT APPROVAL STATUS
// =====================================================
export const ClientApprovalStatus = {
  PENDING: 'PENDING',
  APPROVED: 'APPROVED',
  REJECTED: 'REJECTED'
};

// =====================================================
// PAYMENT STATUS
// =====================================================
export const PaymentStatus = {
  NOT_REQUESTED: 'NOT_REQUESTED', // Supervisor hasn't requested payment yet
  PENDING: 'PENDING',             // Payment requested, awaiting admin approval
  APPROVED: 'APPROVED',           // Admin approved payment
  RELEASED: 'RELEASED',           // Payment released to supervisor
  REJECTED: 'REJECTED'            // Payment request rejected
};

// =====================================================
// EVIDENCE TYPE
// =====================================================
export const EvidenceType = {
  PHOTO: 'PHOTO',
  VIDEO: 'VIDEO'
};

// =====================================================
// ISSUE SEVERITY
// =====================================================
export const IssueSeverity = {
  CRITICAL: 'CRITICAL',       // Immediate attention required
  MAJOR: 'MAJOR',             // Significant issue
  MINOR: 'MINOR'              // Minor issue, can be addressed later
};

// =====================================================
// ID PROOF TYPES
// =====================================================
export const IDProofType = {
  AADHAAR: 'AADHAAR',
  PAN: 'PAN',
  VOTER_ID: 'VOTER_ID',
  PASSPORT: 'PASSPORT',
  DRIVING_LICENSE: 'DRIVING_LICENSE'
};

// =====================================================
// BANK ACCOUNT TYPE
// =====================================================
export const BankAccountType = {
  SAVINGS: 'SAVINGS',
  CURRENT: 'CURRENT'
};

// =====================================================
// INTERVIEW TYPE
// =====================================================
export const InterviewType = {
  VIDEO: 'VIDEO',
  PHONE: 'PHONE',
  IN_PERSON: 'IN_PERSON'
};

// =====================================================
// HELPER FUNCTIONS
// =====================================================

/**
 * Get display label for supervisor status
 */
export const getStatusLabel = (status) => {
  const labels = {
    [SupervisorStatus.APPLIED]: 'Applied',
    [SupervisorStatus.DOCUMENT_VERIFICATION]: 'Document Verification',
    [SupervisorStatus.AWAITING_INTERVIEW]: 'Awaiting Interview',
    [SupervisorStatus.INTERVIEW_PASSED]: 'Interview Passed',
    [SupervisorStatus.AWAITING_TRAINING]: 'Awaiting Training',
    [SupervisorStatus.TRAINING_IN_PROGRESS]: 'Training in Progress',
    [SupervisorStatus.AWAITING_CERTIFICATION]: 'Awaiting Certification',
    [SupervisorStatus.CERTIFIED]: 'Certified',
    [SupervisorStatus.ACTIVE]: 'Active',
    [SupervisorStatus.SUSPENDED]: 'Suspended',
    [SupervisorStatus.TERMINATED]: 'Terminated',
    [SupervisorStatus.REJECTED]: 'Rejected'
  };
  return labels[status] || status;
};

/**
 * Get display label for assignment status
 */
export const getAssignmentStatusLabel = (status) => {
  const labels = {
    [AssignmentStatus.ASSIGNED]: 'Assigned',
    [AssignmentStatus.ACCEPTED]: 'Accepted',
    [AssignmentStatus.REJECTED]: 'Rejected',
    [AssignmentStatus.IN_PROGRESS]: 'In Progress',
    [AssignmentStatus.COMPLETED]: 'Completed',
    [AssignmentStatus.CANCELLED]: 'Cancelled'
  };
  return labels[status] || status;
};

/**
 * Get display label for authority level
 */
export const getAuthorityLevelLabel = (level) => {
  const labels = {
    [AuthorityLevel.BASIC]: 'Basic',
    [AuthorityLevel.INTERMEDIATE]: 'Intermediate',
    [AuthorityLevel.ADVANCED]: 'Advanced',
    [AuthorityLevel.FULL]: 'Full Authority'
  };
  return labels[level] || level;
};

/**
 * Get color for supervisor status badge
 */
export const getStatusColor = (status) => {
  const colors = {
    [SupervisorStatus.ACTIVE]: 'green',
    [SupervisorStatus.CERTIFIED]: 'blue',
    [SupervisorStatus.SUSPENDED]: 'red',
    [SupervisorStatus.TERMINATED]: 'gray',
    [SupervisorStatus.REJECTED]: 'red',
    [SupervisorStatus.APPLIED]: 'yellow',
    [SupervisorStatus.DOCUMENT_VERIFICATION]: 'yellow',
    [SupervisorStatus.AWAITING_INTERVIEW]: 'yellow',
    [SupervisorStatus.INTERVIEW_PASSED]: 'blue',
    [SupervisorStatus.AWAITING_TRAINING]: 'yellow',
    [SupervisorStatus.TRAINING_IN_PROGRESS]: 'blue',
    [SupervisorStatus.AWAITING_CERTIFICATION]: 'yellow'
  };
  return colors[status] || 'gray';
};

/**
 * Get color for assignment status badge
 */
export const getAssignmentStatusColor = (status) => {
  const colors = {
    [AssignmentStatus.ASSIGNED]: 'blue',
    [AssignmentStatus.ACCEPTED]: 'yellow',
    [AssignmentStatus.IN_PROGRESS]: 'orange',
    [AssignmentStatus.COMPLETED]: 'green',
    [AssignmentStatus.REJECTED]: 'red',
    [AssignmentStatus.CANCELLED]: 'red'
  };
  return colors[status] || 'gray';
};

/**
 * Get color for payment status badge
 */
export const getPaymentStatusColor = (status) => {
  const colors = {
    [PaymentStatus.NOT_REQUESTED]: 'gray',
    [PaymentStatus.PENDING]: 'yellow',
    [PaymentStatus.APPROVED]: 'blue',
    [PaymentStatus.RELEASED]: 'green',
    [PaymentStatus.REJECTED]: 'red'
  };
  return colors[status] || 'gray';
};

/**
 * Get registration workflow steps
 */
export const getRegistrationSteps = () => [
  { stage: RegistrationStage.APPLIED, label: 'Applied' },
  { stage: RegistrationStage.DOCUMENT_VERIFICATION, label: 'Document Verification' },
  { stage: RegistrationStage.AWAITING_INTERVIEW, label: 'Interview' },
  { stage: RegistrationStage.AWAITING_TRAINING, label: 'Training' },
  { stage: RegistrationStage.AWAITING_CERTIFICATION, label: 'Certification' },
  { stage: RegistrationStage.ACTIVE, label: 'Active' }
];

/**
 * Check if supervisor can perform action based on permission flags
 */
export const canPerformAction = (supervisor, action) => {
  if (!supervisor) return false;

  switch (action) {
    case 'RELEASE_PAYMENT':
      return supervisor.canReleasePayment; // Always FALSE for supervisors
    case 'APPROVE_REFUND':
      return supervisor.canApproveRefund; // Always FALSE for supervisors
    case 'MENTOR_OTHERS':
      return supervisor.canMentorOthers; // Based on authority level
    case 'CHECK_IN':
      return supervisor.supervisorStatus === SupervisorStatus.ACTIVE;
    case 'ACCEPT_ASSIGNMENT':
      return supervisor.supervisorStatus === SupervisorStatus.ACTIVE;
    default:
      return false;
  }
};

/**
 * Get issue severity display info
 */
export const getIssueSeverityInfo = (severity) => {
  const info = {
    [IssueSeverity.CRITICAL]: { label: 'Critical', color: 'red', icon: '🔴' },
    [IssueSeverity.MAJOR]: { label: 'Major', color: 'orange', icon: '🟠' },
    [IssueSeverity.MINOR]: { label: 'Minor', color: 'yellow', icon: '🟡' }
  };
  return info[severity] || { label: severity, color: 'gray', icon: '⚪' };
};
