## Common Supervisor Components

Reusable UI components for the Event Supervisor Portal.

## Components

### Badges

#### SupervisorStatusBadge
Displays supervisor status with color-coded badge.

```jsx
import { SupervisorStatusBadge } from '@/components/supervisor/common';

<SupervisorStatusBadge status="ACTIVE" />
<SupervisorStatusBadge status="SUSPENDED" />
```

**Props:**
- `status` (string, required) - Supervisor status from enum
- `className` (string) - Additional CSS classes

**Colors:**
- Green: ACTIVE
- Blue: CERTIFIED, INTERVIEW_PASSED
- Yellow: APPLIED, DOCUMENT_VERIFICATION, AWAITING_*
- Red: SUSPENDED, REJECTED, TERMINATED
- Gray: Default

---

#### SupervisorTypeBadge
Displays supervisor type (CAREER vs REGISTERED).

```jsx
<SupervisorTypeBadge type="REGISTERED" />
```

**Props:**
- `type` (string, required) - "CAREER" or "REGISTERED"

---

#### AuthorityLevelBadge
Displays authority level with icon.

```jsx
<AuthorityLevelBadge level="BASIC" showIcon={true} />
```

**Props:**
- `level` (string, required) - "BASIC" | "INTERMEDIATE" | "ADVANCED" | "FULL"
- `showIcon` (boolean) - Show shield icon
- `className` (string)

---

#### AssignmentStatusBadge
Displays assignment status.

```jsx
<AssignmentStatusBadge status="IN_PROGRESS" />
```

**Props:**
- `status` (string, required) - Assignment status from enum

---

#### PaymentStatusBadge
Displays payment request status.

```jsx
<PaymentStatusBadge status="PENDING" />
```

**Props:**
- `status` (string, required) - Payment status from enum

---

### Progress Indicators

#### WorkflowStepper
Step-by-step progress indicator with completion tracking.

```jsx
import { WorkflowStepper } from '@/components/supervisor/common';

<WorkflowStepper
  steps={['Applied', 'Verified', 'Training', 'Certified', 'Active']}
  currentStep={3}
/>
```

**Props:**
- `steps` (array, required) - Array of step labels or objects with `{label, stage}`
- `currentStep` (number, required) - Current step index (1-based)
- `className` (string)

**Features:**
- Check marks for completed steps
- Blue highlight for current step
- Gray for pending steps
- Connecting lines between steps

---

#### ProgressCircle
Circular progress indicator with percentage.

```jsx
import { ProgressCircle } from '@/components/supervisor/common';

<ProgressCircle
  percentage={75}
  size={120}
  color="#3b82f6"
  showPercentage={true}
/>
```

**Props:**
- `percentage` (number, required) - 0-100
- `size` (number) - Diameter in pixels (default: 120)
- `strokeWidth` (number) - Circle thickness (default: 8)
- `color` (string) - Progress color (default: blue)
- `backgroundColor` (string) - Background circle color (default: gray)
- `showPercentage` (boolean) - Display percentage text (default: true)

---

### Form Components

#### DocumentUploader
File upload with preview and validation.

```jsx
import { DocumentUploader } from '@/components/supervisor/common';

<DocumentUploader
  label="ID Proof"
  accept="image/*"
  category="image"
  onUploadComplete={(url, file) => console.log('Uploaded:', url)}
  onRemove={() => console.log('Removed')}
  required={true}
  error={errors.idProof}
/>
```

**Props:**
- `label` (string) - Field label
- `accept` (string) - HTML accept attribute (default: "image/*")
- `category` (string) - "image" | "video" | "document"
- `maxSize` (number) - Max file size in bytes
- `onUploadComplete` (function) - Callback with (url, file)
- `onRemove` (function) - Callback when file removed
- `value` (object) - Current file preview
- `error` (string) - Error message
- `helperText` (string) - Helper text
- `required` (boolean)

**Features:**
- Drag & drop support
- Auto-upload to pre-signed URL
- Image preview
- File size validation
- File type validation

---

#### TimestampedEvidenceUpload
Upload photos/videos with GPS + timestamp capture.

```jsx
import { TimestampedEvidenceUpload } from '@/components/supervisor/common';

<TimestampedEvidenceUpload
  label="Pre-Event Photos"
  type="PHOTO"
  maxFiles={10}
  evidenceList={evidenceList}
  onEvidenceAdded={(evidence) => setEvidenceList([...evidenceList, evidence])}
  onEvidenceRemoved={(index) => setEvidenceList(evidenceList.filter((_, i) => i !== index))}
/>
```

**Props:**
- `label` (string)
- `type` (string) - "PHOTO" | "VIDEO"
- `onEvidenceAdded` (function) - Callback with evidence object
- `onEvidenceRemoved` (function) - Callback with index
- `maxFiles` (number) - Max files (default: 10)
- `evidenceList` (array) - Current evidence array

**Evidence Object:**
```javascript
{
  type: 'PHOTO' | 'VIDEO',
  url: 'https://...',
  localUrl: 'data:image/...',
  timestamp: '2026-01-31T12:00:00Z',
  gpsLocation: '12.9716,77.5946',
  description: 'Optional description',
  fileName: 'photo.jpg',
  fileSize: 1024000
}
```

**Features:**
- Auto GPS capture
- Auto timestamp
- Mobile camera integration
- Description field for each evidence
- Preview thumbnails

---

#### SignatureCapture
Canvas-based signature capture.

```jsx
import { SignatureCapture } from '@/components/supervisor/common';

<SignatureCapture
  label="Client Signature"
  width={400}
  height={200}
  onSave={(dataUrl) => setSignature(dataUrl)}
/>
```

**Props:**
- `label` (string)
- `onSave` (function) - Callback with base64 data URL
- `width` (number) - Canvas width (default: 400)
- `height` (number) - Canvas height (default: 200)
- `strokeColor` (string) - Pen color (default: black)
- `strokeWidth` (number) - Pen thickness (default: 2)
- `backgroundColor` (string) - Background color (default: white)

**Features:**
- Touch and mouse support
- Clear button
- Save as base64 PNG

---

#### OTPInput
6-digit OTP input with auto-focus.

```jsx
import { OTPInput } from '@/components/supervisor/common';

<OTPInput
  length={6}
  value={otp}
  onChange={(value) => setOtp(value)}
  onComplete={(value) => handleVerify(value)}
  error={otpError}
/>
```

**Props:**
- `length` (number) - Number of digits (default: 6)
- `value` (string) - Current OTP value
- `onChange` (function) - Callback with current value
- `onComplete` (function) - Callback when all digits filled
- `disabled` (boolean)
- `error` (boolean) - Show error state

**Features:**
- Auto-focus next input
- Paste support
- Arrow key navigation
- Backspace handling
- Numeric input only

---

### Permission Guard

#### PermissionGuard
Conditionally render based on supervisor permissions.

```jsx
import { PermissionGuard } from '@/components/supervisor/common';

<PermissionGuard
  supervisor={supervisor}
  permission="release_payment"
  showLocked={true}
  lockedMessage="This action requires admin approval"
>
  <button>Release Payment</button>
</PermissionGuard>
```

**Props:**
- `supervisor` (object, required) - Supervisor with permission flags
- `permission` (string, required) - Permission to check
- `children` (node, required) - Content to protect
- `fallback` (node) - Render instead if no permission
- `showLocked` (boolean) - Show locked overlay (default: false)
- `lockedMessage` (string) - Message in lock overlay

**Permissions:**
- `release_payment` - Check `canReleasePayment`
- `approve_refund` - Check `canApproveRefund`
- `mentor_others` - Check `canMentorOthers`

**Modes:**
1. **Hide**: Don't show locked=true (`fallback` rendered if provided)
2. **Show Locked**: `showLocked=true` shows disabled UI with lock overlay

---

## Usage Examples

### Complete Registration Form

```jsx
import {
  WorkflowStepper,
  DocumentUploader,
  SupervisorStatusBadge
} from '@/components/supervisor/common';

function RegistrationForm() {
  const [idProofUrl, setIdProofUrl] = useState('');

  return (
    <div>
      <WorkflowStepper
        steps={['Applied', 'Verified', 'Training', 'Active']}
        currentStep={1}
      />

      <DocumentUploader
        label="ID Proof"
        category="image"
        onUploadComplete={(url) => setIdProofUrl(url)}
        required={true}
      />

      <SupervisorStatusBadge status="APPLIED" />
    </div>
  );
}
```

### Event Evidence Upload

```jsx
import { TimestampedEvidenceUpload } from '@/components/supervisor/common';

function PreEventVerification() {
  const [photos, setPhotos] = useState([]);

  return (
    <TimestampedEvidenceUpload
      label="Menu Photos"
      type="PHOTO"
      maxFiles={5}
      evidenceList={photos}
      onEvidenceAdded={(evidence) => setPhotos([...photos, evidence])}
      onEvidenceRemoved={(index) =>
        setPhotos(photos.filter((_, i) => i !== index))
      }
    />
  );
}
```

### OTP Verification

```jsx
import { OTPInput } from '@/components/supervisor/common';

function OTPVerification({ expiresAt }) {
  const [otp, setOtp] = useState('');

  return (
    <div>
      <OTPInput
        length={6}
        value={otp}
        onChange={setOtp}
        onComplete={handleVerify}
      />
      <p>Expires in: {formatCountdown(expiresAt)}</p>
    </div>
  );
}
```

---

## Styling

All components use Tailwind CSS classes and follow the design system:

**Colors:**
- Primary: Blue (blue-500, blue-600)
- Success: Green (green-500, green-600)
- Warning: Yellow (yellow-500, yellow-600)
- Error: Red (red-500, red-600)
- Gray scale for neutral elements

**Typography:**
- Font: System UI stack
- Sizes: text-xs, text-sm, text-base, text-lg
- Weights: font-normal, font-medium, font-semibold, font-bold

**Spacing:**
- Gap: gap-2, gap-3, gap-4
- Padding: p-2, p-3, p-4, p-6
- Margin: mt-2, mb-3, etc.
