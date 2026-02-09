/**
 * Validation Schemas using Zod
 * Form validation for supervisor portal
 */

import { z } from 'zod';
import { differenceInYears } from 'date-fns';

// =====================================================
// REGISTRATION WIZARD SCHEMAS
// =====================================================

// Step 1: Personal Details
export const personalDetailsSchema = z.object({
  firstName: z.string()
    .min(2, 'First name must be at least 2 characters')
    .max(50, 'First name cannot exceed 50 characters')
    .regex(/^[a-zA-Z\s]+$/, 'First name can only contain letters'),
  lastName: z.string()
    .min(2, 'Last name must be at least 2 characters')
    .max(50, 'Last name cannot exceed 50 characters')
    .regex(/^[a-zA-Z\s]+$/, 'Last name can only contain letters'),
  email: z.string()
    .email('Invalid email address')
    .toLowerCase(),
  phone: z.string()
    .regex(/^[0-9]{10}$/, 'Phone number must be exactly 10 digits')
    .transform(val => val.trim()),
  dateOfBirth: z.date()
    .refine(
      date => differenceInYears(new Date(), date) >= 18,
      'You must be at least 18 years old'
    )
    .refine(
      date => differenceInYears(new Date(), date) <= 65,
      'Age cannot exceed 65 years'
    ),
});

// Step 2: Address Details
export const addressDetailsSchema = z.object({
  address: z.string()
    .min(10, 'Address must be at least 10 characters')
    .max(500, 'Address cannot exceed 500 characters'),
  preferredZoneId: z.number()
    .int()
    .positive('Please select a preferred zone')
    .optional(),
});

// Step 3: Experience Details
export const experienceDetailsSchema = z
    .object({
        hasPriorExperience: z.boolean(),
        priorExperienceDetails: z
            .string()
            .max(1000, "Experience details cannot exceed 1000 characters")
            .optional(),
    })
    .superRefine((data, ctx) => {
        if (
            data.hasPriorExperience &&
            (!data.priorExperienceDetails ||
                data.priorExperienceDetails.trim().length < 20)
        ) {
            ctx.addIssue({
                path: ["priorExperienceDetails"],
                message:
                    "Please provide at least 20 characters describing your experience",
            });
        }
    });


// Step 4: Identity Proof
export const identityProofSchema = z.object({
  idProofType: z.enum(['AADHAAR', 'PAN', 'VOTER_ID', 'PASSPORT', 'DRIVING_LICENSE'], {
    errorMap: () => ({ message: 'Please select a valid ID proof type' }),
  }),
  idProofNumber: z.string()
    .min(5, 'ID proof number is too short')
    .max(20, 'ID proof number is too long')
    .regex(/^[A-Z0-9]+$/, 'ID proof number can only contain uppercase letters and numbers'),
  idProofUrl: z.string()
    .url('Invalid ID proof file URL')
    .min(1, 'Please upload ID proof document'),
  addressProofUrl: z.string()
    .url('Invalid address proof file URL')
    .min(1, 'Please upload address proof document'),
  photoUrl: z.string()
    .url('Invalid photo file URL')
    .min(1, 'Please upload your photo'),
});

// Step 6: Banking Details
export const bankingDetailsSchema = z.object({
  accountHolderName: z.string()
    .min(3, 'Account holder name is too short')
    .max(100, 'Account holder name is too long')
    .regex(/^[a-zA-Z\s]+$/, 'Account holder name can only contain letters'),
  bankName: z.string()
    .min(2, 'Bank name is too short')
    .max(100, 'Bank name is too long'),
  accountNumber: z.string()
    .min(9, 'Account number must be at least 9 digits')
    .max(18, 'Account number cannot exceed 18 digits')
    .regex(/^[0-9]+$/, 'Account number can only contain digits'),
  ifscCode: z.string()
    .length(11, 'IFSC code must be exactly 11 characters')
    .regex(/^[A-Z]{4}0[A-Z0-9]{6}$/, 'Invalid IFSC code format')
    .toUpperCase(),
  branchName: z.string()
    .min(2, 'Branch name is too short')
    .max(100, 'Branch name is too long'),
  accountType: z.enum(['SAVINGS', 'CURRENT'], {
    errorMap: () => ({ message: 'Please select account type' }),
  }),
  cancelledChequeUrl: z.string()
    .url('Invalid cancelled cheque file URL')
    .min(1, 'Please upload cancelled cheque'),
});

// Complete Registration Schema
export const registrationSchema = personalDetailsSchema
  .merge(addressDetailsSchema)
  .merge(experienceDetailsSchema)
  .merge(identityProofSchema);

// =====================================================
// PRE-EVENT VERIFICATION SCHEMA
// =====================================================

export const preEventVerificationSchema = z.object({
  assignmentId: z.number().int().positive(),
  supervisorId: z.number().int().positive(),

  // Menu Verification
  menuVerified: z.boolean(),
  menuVsContractMatch: z.boolean(),
  menuVerificationNotes: z.string()
    .max(500, 'Notes cannot exceed 500 characters')
    .optional(),
  menuVerificationPhotos: z.array(z.string().url())
    .min(1, 'Please upload at least 1 photo of the menu')
    .max(10, 'Cannot upload more than 10 photos'),

  // Raw Material Verification
  rawMaterialVerified: z.boolean(),
  rawMaterialQualityOK: z.boolean(),
  rawMaterialQuantityOK: z.boolean(),
  rawMaterialNotes: z.string()
    .max(500, 'Notes cannot exceed 500 characters')
    .optional(),
  rawMaterialPhotos: z.array(z.string().url())
    .min(1, 'Please upload at least 1 photo of raw materials')
    .max(10, 'Cannot upload more than 10 photos'),

  // Guest Count Confirmation
  guestCountConfirmed: z.boolean(),
  confirmedGuestCount: z.number()
    .int()
    .min(1, 'Guest count must be at least 1')
    .max(10000, 'Guest count seems unrealistic'),

  // Evidence
  preEventEvidence: z.array(z.object({
    type: z.enum(['PHOTO', 'VIDEO']),
    url: z.string().url(),
    timestamp: z.string().datetime(),
    gpsLocation: z.string().nullable(),
    description: z.string().max(200).optional(),
  })),

  // Issues
  issuesFound: z.boolean(),
  issuesDescription: z.string()
    .max(1000, 'Issue description cannot exceed 1000 characters')
    .refine(
      (val, ctx) => {
        if (ctx.parent.issuesFound && (!val || val.trim().length < 10)) {
          return false;
        }
        return true;
      },
      'Please describe the issues found (minimum 10 characters)'
    )
    .optional(),
});

// =====================================================
// DURING-EVENT SCHEMAS
// =====================================================

// Food Serving Monitor
export const foodServingMonitorSchema = z.object({
  assignmentId: z.number().int().positive(),
  supervisorId: z.number().int().positive(),
  qualityRating: z.number()
    .int()
    .min(1, 'Rating must be between 1 and 5')
    .max(5, 'Rating must be between 1 and 5'),
  temperatureOK: z.boolean(),
  presentationOK: z.boolean(),
  notes: z.string()
    .max(500, 'Notes cannot exceed 500 characters')
    .optional(),
  photos: z.array(z.string().url())
    .max(10, 'Cannot upload more than 10 photos'),
});

// Guest Count Update
export const guestCountUpdateSchema = z.object({
  assignmentId: z.number().int().positive(),
  supervisorId: z.number().int().positive(),
  actualGuestCount: z.number()
    .int()
    .min(1, 'Guest count must be at least 1'),
  notes: z.string()
    .max(200, 'Notes cannot exceed 200 characters')
    .optional(),
  timestamp: z.string().datetime(),
});

// Extra Quantity Request
export const extraQuantityRequestSchema = z.object({
  assignmentId: z.number().int().positive(),
  supervisorId: z.number().int().positive(),
  itemName: z.string()
    .min(2, 'Item name is too short')
    .max(100, 'Item name is too long'),
  extraQuantity: z.number()
    .int()
    .min(1, 'Quantity must be at least 1'),
  extraCost: z.number()
    .min(0, 'Cost cannot be negative'),
  reason: z.string()
    .min(10, 'Please provide a reason (minimum 10 characters)')
    .max(500, 'Reason cannot exceed 500 characters'),
  clientPhone: z.string()
    .regex(/^[0-9]{10}$/, 'Phone number must be exactly 10 digits'),
  approvalMethod: z.enum(['IN_APP', 'OTP', 'SIGNATURE']),
});

// OTP Verification
export const otpVerificationSchema = z.object({
  assignmentId: z.number().int().positive(),
  otpCode: z.string()
    .length(6, 'OTP must be exactly 6 digits')
    .regex(/^[0-9]{6}$/, 'OTP must contain only numbers'),
  clientIPAddress: z.string(),
});

// =====================================================
// POST-EVENT REPORT SCHEMA
// =====================================================

export const postEventReportSchema = z.object({
  assignmentId: z.number().int().positive(),
  supervisorId: z.number().int().positive(),

  // Event Summary
  finalGuestCount: z.number()
    .int()
    .min(1, 'Guest count must be at least 1'),
  eventRating: z.number()
    .int()
    .min(1, 'Rating must be between 1 and 5')
    .max(5, 'Rating must be between 1 and 5'),

  // Client Feedback
  clientName: z.string()
    .min(2, 'Client name is too short')
    .max(100, 'Client name is too long'),
  clientPhone: z.string()
    .regex(/^[0-9]{10}$/, 'Phone number must be exactly 10 digits'),
  clientSatisfactionRating: z.number().int().min(1).max(5),
  foodQualityRating: z.number().int().min(1).max(5),
  foodQuantityRating: z.number().int().min(1).max(5),
  serviceQualityRating: z.number().int().min(1).max(5),
  presentationRating: z.number().int().min(1).max(5),
  wouldRecommend: z.boolean(),
  clientComments: z.string()
    .max(1000, 'Comments cannot exceed 1000 characters')
    .optional(),
  clientSignatureUrl: z.string()
    .url('Invalid signature URL')
    .min(1, 'Client signature is required'),

  // Partner Performance
  vendorPunctualityRating: z.number().int().min(1).max(5),
  vendorPreparationRating: z.number().int().min(1).max(5),
  vendorCooperationRating: z.number().int().min(1).max(5),
  vendorComments: z.string()
    .max(500, 'Comments cannot exceed 500 characters')
    .optional(),

  // Issues
  issuesCount: z.number().int().min(0),
  issues: z.array(z.object({
    issueType: z.string(),
    severity: z.enum(['CRITICAL', 'MAJOR', 'MINOR']),
    description: z.string()
      .min(10, 'Issue description is too short')
      .max(500, 'Issue description is too long'),
    resolution: z.string()
      .max(500, 'Resolution description is too long')
      .optional(),
    timestamp: z.string().datetime(),
    evidenceUrls: z.array(z.string().url()).max(5),
  })),

  // Financial
  finalPayableAmount: z.number()
    .min(0, 'Amount cannot be negative'),
  paymentBreakdown: z.object({
    baseAmount: z.number().min(0),
    taxAmount: z.number().min(0),
    serviceCharges: z.number().min(0),
    extraCharges: z.number().min(0),
    deductions: z.number().min(0),
    totalAmount: z.number().min(0),
  }),

  // Report
  reportSummary: z.string()
    .min(50, 'Report summary must be at least 50 characters')
    .max(2000, 'Report summary cannot exceed 2000 characters'),
  recommendations: z.string()
    .max(1000, 'Recommendations cannot exceed 1000 characters')
    .optional(),
  completionPhotos: z.array(z.string().url())
    .min(2, 'Please upload at least 2 completion photos')
    .max(20, 'Cannot upload more than 20 photos'),
  completionVideos: z.array(z.string().url())
    .max(5, 'Cannot upload more than 5 videos'),
});

// =====================================================
// CHECK-IN SCHEMA
// =====================================================

export const checkInSchema = z.object({
  assignmentId: z.number().int().positive(),
  supervisorId: z.number().int().positive(),
  gpsLocation: z.string()
    .min(1, 'GPS location is required'),
  checkInPhoto: z.string()
    .url('Invalid check-in photo URL')
    .min(1, 'Check-in photo is required'),
  checkInTime: z.string().datetime(),
});

// =====================================================
// ASSIGNMENT ACTION SCHEMAS
// =====================================================

export const acceptAssignmentSchema = z.object({
  assignmentId: z.number().int().positive(),
  supervisorId: z.number().int().positive(),
});

export const rejectAssignmentSchema = z.object({
  assignmentId: z.number().int().positive(),
  supervisorId: z.number().int().positive(),
  reason: z.string()
    .min(10, 'Please provide a reason (minimum 10 characters)')
    .max(500, 'Reason cannot exceed 500 characters'),
});

export const paymentRequestSchema = z.object({
  assignmentId: z.number().int().positive(),
  supervisorId: z.number().int().positive(),
  amount: z.number()
    .min(0, 'Amount cannot be negative')
    .max(1000000, 'Amount seems unrealistic'),
});
