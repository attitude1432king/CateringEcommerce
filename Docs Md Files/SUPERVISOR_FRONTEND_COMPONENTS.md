# Supervisor Management - Frontend Component Structure

## 🎯 Technology Stack
- **Framework:** React 18+ with Vite
- **State Management:** React Context API + useState/useReducer
- **API Client:** Axios
- **Routing:** React Router v6
- **UI Library:** Tailwind CSS + Shadcn/UI components
- **Forms:** React Hook Form + Zod validation
- **Date Handling:** date-fns
- **Icons:** Lucide React

---

## 📁 Directory Structure

```
src/
├── components/
│   ├── supervisor/
│   │   ├── careers/                    # Careers Portal (Core Supervisors)
│   │   │   ├── CareersApplicationForm.jsx
│   │   │   ├── CareersSupervisorProfile.jsx
│   │   │   ├── CareersWorkflowTracker.jsx
│   │   │   └── index.js
│   │   │
│   │   ├── registration/               # Registration Portal (Scale Engine)
│   │   │   ├── RegistrationWizard/
│   │   │   │   ├── Step1PersonalDetails.jsx
│   │   │   │   ├── Step2AddressDetails.jsx
│   │   │   │   ├── Step3ExperienceDetails.jsx
│   │   │   │   ├── Step4IdentityProof.jsx
│   │   │   │   ├── Step5Availability.jsx
│   │   │   │   ├── Step6Agreement.jsx
│   │   │   │   └── RegistrationWizard.jsx
│   │   │   ├── RegistrationProgressTracker.jsx
│   │   │   ├── RegisteredSupervisorDashboard.jsx
│   │   │   └── index.js
│   │   │
│   │   ├── admin/                      # Admin Management (Both Types)
│   │   │   ├── SupervisorListView.jsx
│   │   │   ├── SupervisorDetailsDrawer.jsx
│   │   │   ├── AuthorityManagement.jsx
│   │   │   ├── StatusManagement.jsx
│   │   │   ├── CareersApplicationQueue.jsx
│   │   │   ├── RegistrationQueue.jsx
│   │   │   ├── AssignmentView.jsx
│   │   │   └── index.js
│   │   │
│   │   ├── common/                     # Shared Components
│   │   │   ├── SupervisorCard.jsx
│   │   │   ├── StatusBadge.jsx
│   │   │   ├── AuthorityBadge.jsx
│   │   │   ├── TypeBadge.jsx
│   │   │   ├── ProgressStepper.jsx
│   │   │   ├── DocumentUploader.jsx
│   │   │   ├── AvailabilityCalendar.jsx
│   │   │   └── index.js
│   │   │
│   │   └── hooks/                      # Custom Hooks
│   │       ├── useSupervisorAuth.js
│   │       ├── useSupervisorData.js
│   │       ├── useWorkflowProgress.js
│   │       └── index.js
│   │
├── services/
│   ├── supervisorApi.js               # API service layer
│   └── fileUploadService.js           # File upload utilities
│
├── utils/
│   ├── supervisorEnums.js             # Enum definitions from backend
│   ├── supervisorValidation.js        # Form validation schemas
│   └── supervisorHelpers.js           # Utility functions
│
└── pages/
    ├── CareersPortal.jsx              # Public careers application page
    ├── RegistrationPortal.jsx          # Public registration page
    ├── SupervisorDashboard.jsx         # Supervisor's own dashboard
    └── AdminSupervisorManagement.jsx   # Admin management panel
```

---

## 🔐 Component #1: Careers Application Form

### File: `CareersApplicationForm.jsx`

**Purpose:** Public-facing form for high-trust supervisor applications

**Backend API:**
- `POST /api/careers/submit` → `SubmitCareersApplicationAsync()`

**Props:**
```typescript
interface CareersApplicationFormProps {
  onSuccess: (applicationId: number) => void;
  onError: (error: string) => void;
}
```

**State Management:**
```javascript
const [formData, setFormData] = useState({
  fullName: '',
  email: '',
  phone: '',
  alternatePhone: '',
  dateOfBirth: null,
  city: '',
  state: '',
  pincode: '',
  yearsOfExperience: 0,
  previousEmployer: '',
  specialization: '',
  languagesKnown: [],
  identityType: '',
  identityNumber: '',
  resumeUrl: '',
  photoUrl: '',
  source: 'WEBSITE'
});

const [uploading, setUploading] = useState(false);
const [errors, setErrors] = useState({});
```

**Form Structure:**
```jsx
<form onSubmit={handleSubmit} className="max-w-4xl mx-auto p-6 space-y-8">
  {/* Section 1: Personal Information */}
  <section className="bg-white rounded-lg shadow p-6 space-y-4">
    <h2 className="text-xl font-semibold">Personal Information</h2>
    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
      <Input
        label="Full Name *"
        name="fullName"
        value={formData.fullName}
        onChange={handleChange}
        error={errors.fullName}
        placeholder="Enter full name"
      />
      <Input
        label="Email Address *"
        type="email"
        name="email"
        value={formData.email}
        onChange={handleChange}
        error={errors.email}
      />
      <Input
        label="Phone Number *"
        name="phone"
        value={formData.phone}
        onChange={handleChange}
        error={errors.phone}
        maxLength={10}
      />
      <Input
        label="Alternate Phone"
        name="alternatePhone"
        value={formData.alternatePhone}
        onChange={handleChange}
      />
      <DatePicker
        label="Date of Birth *"
        selected={formData.dateOfBirth}
        onChange={(date) => setFormData(prev => ({...prev, dateOfBirth: date}))}
        maxDate={subYears(new Date(), 18)}
        error={errors.dateOfBirth}
      />
    </div>
  </section>

  {/* Section 2: Address */}
  <section className="bg-white rounded-lg shadow p-6 space-y-4">
    <h2 className="text-xl font-semibold">Address</h2>
    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
      <Input label="City *" name="city" value={formData.city} onChange={handleChange} error={errors.city} />
      <Input label="State *" name="state" value={formData.state} onChange={handleChange} error={errors.state} />
      <Input label="Pincode *" name="pincode" value={formData.pincode} onChange={handleChange} error={errors.pincode} maxLength={6} />
    </div>
  </section>

  {/* Section 3: Professional Experience */}
  <section className="bg-white rounded-lg shadow p-6 space-y-4">
    <h2 className="text-xl font-semibold">Professional Experience</h2>
    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
      <Input
        label="Years of Experience *"
        type="number"
        name="yearsOfExperience"
        value={formData.yearsOfExperience}
        onChange={handleChange}
        error={errors.yearsOfExperience}
        min={0}
        max={50}
      />
      <Input
        label="Previous Employer *"
        name="previousEmployer"
        value={formData.previousEmployer}
        onChange={handleChange}
        error={errors.previousEmployer}
      />
      <Input
        label="Specialization"
        name="specialization"
        value={formData.specialization}
        onChange={handleChange}
        placeholder="e.g., Wedding Events, Corporate Catering"
      />
      <MultiSelect
        label="Languages Known"
        options={languageOptions}
        value={formData.languagesKnown}
        onChange={(languages) => setFormData(prev => ({...prev, languagesKnown: languages}))}
      />
    </div>
  </section>

  {/* Section 4: Identity & Documents */}
  <section className="bg-white rounded-lg shadow p-6 space-y-4">
    <h2 className="text-xl font-semibold">Identity Verification</h2>
    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
      <Select
        label="Identity Type *"
        name="identityType"
        value={formData.identityType}
        onChange={handleChange}
        options={[
          { value: 'AADHAAR', label: 'Aadhaar Card' },
          { value: 'PAN', label: 'PAN Card' },
          { value: 'PASSPORT', label: 'Passport' }
        ]}
        error={errors.identityType}
      />
      <Input
        label="Identity Number *"
        name="identityNumber"
        value={formData.identityNumber}
        onChange={handleChange}
        error={errors.identityNumber}
      />
    </div>

    <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mt-4">
      <DocumentUploader
        label="Resume Upload *"
        accept=".pdf,.doc,.docx"
        onUpload={(url) => setFormData(prev => ({...prev, resumeUrl: url}))}
        error={errors.resumeUrl}
        value={formData.resumeUrl}
      />
      <DocumentUploader
        label="Photo Upload *"
        accept=".jpg,.jpeg,.png"
        onUpload={(url) => setFormData(prev => ({...prev, photoUrl: url}))}
        error={errors.photoUrl}
        value={formData.photoUrl}
      />
    </div>
  </section>

  {/* Submit Button */}
  <div className="flex justify-center">
    <Button
      type="submit"
      disabled={uploading || submitting}
      className="px-8 py-3 bg-blue-600 hover:bg-blue-700 text-white font-semibold rounded-lg"
    >
      {submitting ? 'Submitting...' : 'Submit Application'}
    </Button>
  </div>
</form>
```

**Validation Schema (Zod):**
```javascript
const careersApplicationSchema = z.object({
  fullName: z.string().min(3, "Name must be at least 3 characters").max(100),
  email: z.string().email("Invalid email address"),
  phone: z.string().regex(/^[0-9]{10}$/, "Phone must be 10 digits"),
  alternatePhone: z.string().regex(/^[0-9]{10}$/).optional().or(z.literal('')),
  dateOfBirth: z.date().refine(date => differenceInYears(new Date(), date) >= 18, "Must be 18+ years old"),
  city: z.string().min(2),
  state: z.string().min(2),
  pincode: z.string().regex(/^[0-9]{6}$/, "Pincode must be 6 digits"),
  yearsOfExperience: z.number().min(0).max(50),
  previousEmployer: z.string().min(2),
  specialization: z.string().optional(),
  languagesKnown: z.array(z.string()).optional(),
  identityType: z.enum(['AADHAAR', 'PAN', 'PASSPORT']),
  identityNumber: z.string().min(6),
  resumeUrl: z.string().url("Resume must be uploaded"),
  photoUrl: z.string().url("Photo must be uploaded")
});
```

**API Integration:**
```javascript
const handleSubmit = async (e) => {
  e.preventDefault();

  try {
    // Validate
    careersApplicationSchema.parse(formData);

    setSubmitting(true);
    const response = await supervisorApi.submitCareersApplication(formData);

    if (response.success) {
      onSuccess(response.data.applicationId);
    } else {
      setErrors(response.errors || {});
      onError(response.message);
    }
  } catch (validationError) {
    if (validationError instanceof z.ZodError) {
      const formattedErrors = {};
      validationError.errors.forEach(err => {
        formattedErrors[err.path[0]] = err.message;
      });
      setErrors(formattedErrors);
    }
  } finally {
    setSubmitting(false);
  }
};
```

---

## 🔐 Component #2: Registration Wizard

### File: `RegistrationWizard.jsx`

**Purpose:** Multi-step registration form for on-demand supervisors

**Backend API:**
- `POST /api/registration/submit` → `SubmitRegistrationAsync()`

**State Management:**
```javascript
const [currentStep, setCurrentStep] = useState(1);
const [registrationData, setRegistrationData] = useState({
  // Step 1
  fullName: '',
  email: '',
  phone: '',
  alternatePhone: '',
  dateOfBirth: null,

  // Step 2
  addressLine1: '',
  city: '',
  state: '',
  pincode: '',
  locality: '',

  // Step 3
  yearsOfExperience: 0,
  previousEmployer: '',
  specialization: '',
  languagesKnown: [],

  // Step 4
  identityType: '',
  identityNumber: '',
  identityProofUrl: '',
  photoUrl: '',

  // Step 5
  preferredCities: [],
  preferredLocalities: [],
  preferredEventTypes: [],
  availableDaysPerWeek: 0,

  // Step 6
  agreementAccepted: false
});

const totalSteps = 6;
```

**Component Structure:**
```jsx
<div className="min-h-screen bg-gray-50 py-8">
  <div className="max-w-4xl mx-auto">
    {/* Progress Stepper */}
    <ProgressStepper
      currentStep={currentStep}
      totalSteps={totalSteps}
      steps={[
        'Personal Details',
        'Address',
        'Experience',
        'Identity Proof',
        'Availability',
        'Agreement'
      ]}
    />

    {/* Step Content */}
    <div className="mt-8 bg-white rounded-lg shadow-lg p-8">
      {currentStep === 1 && (
        <Step1PersonalDetails
          data={registrationData}
          onChange={updateRegistrationData}
          errors={errors}
        />
      )}

      {currentStep === 2 && (
        <Step2AddressDetails
          data={registrationData}
          onChange={updateRegistrationData}
          errors={errors}
        />
      )}

      {currentStep === 3 && (
        <Step3ExperienceDetails
          data={registrationData}
          onChange={updateRegistrationData}
          errors={errors}
        />
      )}

      {currentStep === 4 && (
        <Step4IdentityProof
          data={registrationData}
          onChange={updateRegistrationData}
          errors={errors}
        />
      )}

      {currentStep === 5 && (
        <Step5Availability
          data={registrationData}
          onChange={updateRegistrationData}
          errors={errors}
        />
      )}

      {currentStep === 6 && (
        <Step6Agreement
          data={registrationData}
          onChange={updateRegistrationData}
          errors={errors}
        />
      )}
    </div>

    {/* Navigation Buttons */}
    <div className="mt-6 flex justify-between">
      <Button
        variant="outline"
        onClick={handlePrevious}
        disabled={currentStep === 1}
      >
        Previous
      </Button>

      {currentStep < totalSteps ? (
        <Button onClick={handleNext}>
          Next
        </Button>
      ) : (
        <Button onClick={handleSubmit} disabled={submitting}>
          {submitting ? 'Submitting...' : 'Submit Registration'}
        </Button>
      )}
    </div>
  </div>
</div>
```

**Step Validation Logic:**
```javascript
const validateStep = (step) => {
  switch(step) {
    case 1:
      return step1Schema.parse({
        fullName: registrationData.fullName,
        email: registrationData.email,
        phone: registrationData.phone,
        dateOfBirth: registrationData.dateOfBirth
      });
    case 2:
      return step2Schema.parse({
        addressLine1: registrationData.addressLine1,
        city: registrationData.city,
        state: registrationData.state,
        pincode: registrationData.pincode,
        locality: registrationData.locality
      });
    // ... other steps
  }
};

const handleNext = () => {
  try {
    validateStep(currentStep);
    setCurrentStep(prev => prev + 1);
    setErrors({});
  } catch (validationError) {
    // Show errors
    const formattedErrors = {};
    validationError.errors.forEach(err => {
      formattedErrors[err.path[0]] = err.message;
    });
    setErrors(formattedErrors);
  }
};
```

---

## 📊 Component #3: Registration Progress Tracker

### File: `RegistrationProgressTracker.jsx`

**Purpose:** Display registration workflow progress with real-time status

**Backend API:**
- `GET /api/registration/progress/{id}` → `GetRegistrationProgressAsync()`

**Props:**
```typescript
interface RegistrationProgressTrackerProps {
  registrationId: number;
  refreshInterval?: number; // in milliseconds
}
```

**Component:**
```jsx
const RegistrationProgressTracker = ({ registrationId, refreshInterval = 30000 }) => {
  const [progress, setProgress] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchProgress();
    const interval = setInterval(fetchProgress, refreshInterval);
    return () => clearInterval(interval);
  }, [registrationId]);

  const fetchProgress = async () => {
    try {
      const response = await supervisorApi.getRegistrationProgress(registrationId);
      if (response.success) {
        setProgress(response.data);
      }
    } catch (error) {
      console.error('Failed to fetch progress:', error);
    } finally {
      setLoading(false);
    }
  };

  if (loading) return <LoadingSpinner />;

  return (
    <div className="max-w-4xl mx-auto p-6">
      {/* Header */}
      <div className="bg-white rounded-lg shadow p-6 mb-6">
        <h1 className="text-2xl font-bold mb-2">Registration Progress</h1>
        <p className="text-gray-600">Application #{progress?.registrationNumber}</p>
        <div className="mt-4">
          <div className="flex justify-between text-sm mb-2">
            <span>Overall Progress</span>
            <span className="font-semibold">{progress?.progressPercentage}%</span>
          </div>
          <div className="w-full bg-gray-200 rounded-full h-2">
            <div
              className="bg-blue-600 h-2 rounded-full transition-all duration-300"
              style={{ width: `${progress?.progressPercentage}%` }}
            />
          </div>
        </div>
      </div>

      {/* Stage 1: Document Verification */}
      <div className="bg-white rounded-lg shadow p-6 mb-4">
        <div className="flex items-start">
          <StageIcon
            status={progress?.documentVerified ? 'completed' : 'pending'}
            icon={FileCheck}
          />
          <div className="ml-4 flex-1">
            <h3 className="text-lg font-semibold">Document Verification</h3>
            <StatusBadge status={progress?.documentVerificationStatus} />

            {progress?.documentVerified && (
              <div className="mt-2 text-sm text-gray-600">
                <p>✓ Verified by: {progress?.documentVerifiedBy}</p>
                <p>✓ Date: {format(new Date(progress?.documentVerifiedDate), 'PPP')}</p>
              </div>
            )}

            {progress?.documentRejectionReason && (
              <Alert variant="destructive" className="mt-2">
                <AlertCircle className="h-4 w-4" />
                <AlertTitle>Rejection Reason</AlertTitle>
                <AlertDescription>{progress?.documentRejectionReason}</AlertDescription>
              </Alert>
            )}
          </div>
        </div>
      </div>

      {/* Stage 2: Short Interview */}
      <div className="bg-white rounded-lg shadow p-6 mb-4">
        <div className="flex items-start">
          <StageIcon
            status={progress?.interviewCompleted ? 'completed' : progress?.interviewScheduled ? 'in-progress' : 'pending'}
            icon={Video}
          />
          <div className="ml-4 flex-1">
            <h3 className="text-lg font-semibold">Short Interview</h3>

            {progress?.interviewScheduled && !progress?.interviewCompleted && (
              <div className="mt-2">
                <p className="text-sm text-gray-600">
                  📅 Scheduled for: {format(new Date(progress?.interviewDate), 'PPpp')}
                </p>
                <p className="text-sm text-gray-600">
                  💻 Mode: {progress?.interviewMode}
                </p>
              </div>
            )}

            {progress?.interviewCompleted && (
              <div className="mt-2">
                <StatusBadge status={progress?.interviewResult} />
                <p className="text-sm text-gray-600 mt-1">
                  Completed on: {format(new Date(progress?.interviewDate), 'PPP')}
                </p>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Stage 3: Training */}
      <div className="bg-white rounded-lg shadow p-6 mb-4">
        <div className="flex items-start">
          <StageIcon
            status={progress?.trainingPassed ? 'completed' : progress?.trainingModuleAssigned ? 'in-progress' : 'pending'}
            icon={GraduationCap}
          />
          <div className="ml-4 flex-1">
            <h3 className="text-lg font-semibold">Training Module</h3>

            {progress?.trainingModuleAssigned && !progress?.trainingPassed && (
              <div className="mt-2">
                <div className="flex justify-between text-sm mb-1">
                  <span>Progress</span>
                  <span>{progress?.trainingCompletionPercentage}%</span>
                </div>
                <div className="w-full bg-gray-200 rounded-full h-2">
                  <div
                    className="bg-green-600 h-2 rounded-full"
                    style={{ width: `${progress?.trainingCompletionPercentage}%` }}
                  />
                </div>
              </div>
            )}

            {progress?.trainingPassed && (
              <div className="mt-2 text-sm text-gray-600">
                <p>✓ Training completed on: {format(new Date(progress?.trainingCompletedDate), 'PPP')}</p>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Stage 4: Certification */}
      <div className="bg-white rounded-lg shadow p-6 mb-4">
        <div className="flex items-start">
          <StageIcon
            status={progress?.certificationTestPassed ? 'completed' : progress?.certificationTestAssigned ? 'in-progress' : 'pending'}
            icon={Award}
          />
          <div className="ml-4 flex-1">
            <h3 className="text-lg font-semibold">Certification Test</h3>

            {progress?.certificationTestAssigned && !progress?.certificationTestPassed && (
              <p className="text-sm text-gray-600 mt-2">
                📅 Scheduled for: {format(new Date(progress?.certificationTestDate), 'PPP')}
              </p>
            )}

            {progress?.certificationTestPassed && (
              <div className="mt-2">
                <p className="text-sm font-semibold text-green-600">
                  ✓ Passed with {progress?.certificationTestScore}% score
                </p>
                <a
                  href={progress?.certificationCertificateUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="text-sm text-blue-600 hover:underline mt-1 inline-block"
                >
                  📄 Download Certificate
                </a>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Final Activation */}
      <div className="bg-white rounded-lg shadow p-6">
        <div className="flex items-start">
          <StageIcon
            status={progress?.activationStatus === 'ACTIVATED' ? 'completed' : 'pending'}
            icon={CheckCircle}
          />
          <div className="ml-4 flex-1">
            <h3 className="text-lg font-semibold">Activation</h3>
            <StatusBadge status={progress?.activationStatus} />

            {progress?.activationStatus === 'ACTIVATED' && (
              <div className="mt-2 text-sm text-gray-600">
                <p>✓ Activated on: {format(new Date(progress?.activatedDate), 'PPP')}</p>
                <p>✓ Activated by: Admin</p>
              </div>
            )}

            {progress?.activationStatus === 'PENDING' && progress?.pendingActions?.length > 0 && (
              <Alert className="mt-2">
                <AlertCircle className="h-4 w-4" />
                <AlertTitle>Pending Actions</AlertTitle>
                <AlertDescription>
                  <ul className="list-disc list-inside">
                    {progress?.pendingActions.map((action, idx) => (
                      <li key={idx}>{action}</li>
                    ))}
                  </ul>
                </AlertDescription>
              </Alert>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};
```

---

## 🎯 Component #4: Supervisor Dashboard (Own View)

### File: `SupervisorDashboard.jsx`

**Purpose:** Supervisor's own dashboard showing assignments, earnings, and profile

**Backend API:**
- `GET /api/supervisor/dashboard/{id}` → `GetSupervisorDashboardAsync()`

**Component:**
```jsx
const SupervisorDashboard = () => {
  const { supervisorId } = useSupervisorAuth();
  const [dashboard, setDashboard] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchDashboard();
  }, [supervisorId]);

  const fetchDashboard = async () => {
    try {
      const response = await supervisorApi.getSupervisorDashboard(supervisorId);
      if (response.success) {
        setDashboard(response.data);
      }
    } catch (error) {
      console.error('Failed to fetch dashboard:', error);
    } finally {
      setLoading(false);
    }
  };

  if (loading) return <LoadingSpinner />;

  return (
    <div className="min-h-screen bg-gray-50 p-6">
      {/* Header */}
      <div className="bg-white rounded-lg shadow p-6 mb-6">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-bold">{dashboard?.fullName}</h1>
            <div className="flex gap-2 mt-2">
              <TypeBadge type={dashboard?.supervisorType} />
              <StatusBadge status={dashboard?.currentStatus} />
              <AuthorityBadge level={dashboard?.authorityLevel} />
            </div>
          </div>
          <div className="text-right">
            <p className="text-sm text-gray-600">Supervisor ID</p>
            <p className="text-lg font-semibold">#{dashboard?.supervisorId}</p>
          </div>
        </div>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
        <StatCard
          icon={Calendar}
          title="Upcoming Events"
          value={dashboard?.upcomingAssignments}
          color="blue"
        />
        <StatCard
          icon={ClipboardCheck}
          title="Events This Month"
          value={dashboard?.eventsThisMonth}
          color="green"
        />
        <StatCard
          icon={Star}
          title="Average Rating"
          value={dashboard?.averageRating?.toFixed(1)}
          subtitle="out of 5.0"
          color="yellow"
        />
        <StatCard
          icon={DollarSign}
          title="Earnings This Month"
          value={`₹${dashboard?.earningsThisMonth?.toLocaleString()}`}
          color="purple"
        />
      </div>

      {/* Authority Permissions */}
      <div className="bg-white rounded-lg shadow p-6 mb-6">
        <h2 className="text-lg font-semibold mb-4">Your Permissions</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <PermissionCard
            icon={CheckCircle}
            title="Event Check-in"
            granted={true}
            description="Mark attendance and start event supervision"
          />
          <PermissionCard
            icon={ClipboardCheck}
            title="Quality & Quantity Check"
            granted={true}
            description="Verify food quality and serving quantities"
          />
          <PermissionCard
            icon={Plus}
            title="Request Extra Payment"
            granted={true}
            description="Raise additional payment requests"
          />
          <PermissionCard
            icon={DollarSign}
            title="Release Payment"
            granted={dashboard?.canReleasePayment}
            description="Approve final payment release"
            locked={!dashboard?.canReleasePayment}
          />
          <PermissionCard
            icon={RefreshCw}
            title="Approve Refund"
            granted={dashboard?.canApproveRefund}
            description="Approve customer refund requests"
            locked={!dashboard?.canApproveRefund}
          />
          <PermissionCard
            icon={Users}
            title="Mentor Others"
            granted={dashboard?.canMentorOthers}
            description="Guide and train other supervisors"
            locked={!dashboard?.canMentorOthers}
          />
        </div>
      </div>

      {/* Certification Status */}
      {dashboard?.certificationStatus && (
        <div className="bg-white rounded-lg shadow p-6 mb-6">
          <h2 className="text-lg font-semibold mb-4">Certification Status</h2>
          <div className="flex items-center justify-between">
            <div>
              <StatusBadge status={dashboard?.certificationStatus} />
              {dashboard?.certificationValidUntil && (
                <p className="text-sm text-gray-600 mt-2">
                  Valid until: {format(new Date(dashboard.certificationValidUntil), 'PPP')}
                </p>
              )}
            </div>
            {dashboard?.requiresRenewal && (
              <Alert variant="warning">
                <AlertCircle className="h-4 w-4" />
                <AlertTitle>Renewal Required</AlertTitle>
                <AlertDescription>
                  Your certification expires soon. Please complete renewal training.
                </AlertDescription>
              </Alert>
            )}
          </div>
        </div>
      )}

      {/* Performance Summary */}
      <div className="bg-white rounded-lg shadow p-6">
        <h2 className="text-lg font-semibold mb-4">Performance Summary</h2>
        <div className="space-y-4">
          <div className="flex justify-between items-center">
            <span className="text-gray-600">Total Events Supervised</span>
            <span className="font-semibold">{dashboard?.totalEventsSupervised}</span>
          </div>
          <div className="flex justify-between items-center">
            <span className="text-gray-600">Total Earnings</span>
            <span className="font-semibold">₹{dashboard?.totalEarnings?.toLocaleString()}</span>
          </div>
          <div className="flex justify-between items-center">
            <span className="text-gray-600">Pending Approvals</span>
            <span className="font-semibold">{dashboard?.pendingApprovals}</span>
          </div>
        </div>
      </div>
    </div>
  );
};
```

---

## 👨‍💼 Component #5: Admin Supervisor Management

### File: `AdminSupervisorManagement.jsx`

**Purpose:** Admin panel to manage both supervisor types

**Backend APIs:**
- `GET /api/admin/supervisors` → `GetAllSupervisorsAsync()`
- `GET /api/admin/supervisors/search` → `SearchSupervisorsAsync()`

**Component:**
```jsx
const AdminSupervisorManagement = () => {
  const [supervisors, setSupervisors] = useState([]);
  const [filters, setFilters] = useState({
    type: null,
    status: null,
    authorityLevel: null,
    city: '',
    searchQuery: ''
  });
  const [selectedSupervisor, setSelectedSupervisor] = useState(null);
  const [drawerOpen, setDrawerOpen] = useState(false);

  useEffect(() => {
    fetchSupervisors();
  }, [filters]);

  const fetchSupervisors = async () => {
    try {
      const response = await supervisorApi.searchSupervisors(filters);
      if (response.success) {
        setSupervisors(response.data);
      }
    } catch (error) {
      console.error('Failed to fetch supervisors:', error);
    }
  };

  return (
    <div className="p-6">
      {/* Header */}
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold">Supervisor Management</h1>
        <div className="flex gap-2">
          <Button variant="outline" onClick={() => navigate('/admin/careers-queue')}>
            Careers Queue
          </Button>
          <Button variant="outline" onClick={() => navigate('/admin/registration-queue')}>
            Registration Queue
          </Button>
        </div>
      </div>

      {/* Filters */}
      <div className="bg-white rounded-lg shadow p-4 mb-6">
        <div className="grid grid-cols-1 md:grid-cols-5 gap-4">
          <Input
            placeholder="Search by name, email, phone..."
            value={filters.searchQuery}
            onChange={(e) => setFilters(prev => ({...prev, searchQuery: e.target.value}))}
          />
          <Select
            placeholder="Type"
            value={filters.type}
            onChange={(value) => setFilters(prev => ({...prev, type: value}))}
            options={[
              { value: null, label: 'All Types' },
              { value: 'CAREER', label: 'Careers Portal' },
              { value: 'REGISTERED', label: 'Registration Portal' }
            ]}
          />
          <Select
            placeholder="Status"
            value={filters.status}
            onChange={(value) => setFilters(prev => ({...prev, status: value}))}
            options={[
              { value: null, label: 'All Statuses' },
              { value: 'ACTIVE', label: 'Active' },
              { value: 'SUSPENDED', label: 'Suspended' },
              { value: 'PROBATION', label: 'In Probation' }
            ]}
          />
          <Select
            placeholder="Authority Level"
            value={filters.authorityLevel}
            onChange={(value) => setFilters(prev => ({...prev, authorityLevel: value}))}
            options={[
              { value: null, label: 'All Levels' },
              { value: 'BASIC', label: 'Basic' },
              { value: 'INTERMEDIATE', label: 'Intermediate' },
              { value: 'ADVANCED', label: 'Advanced' },
              { value: 'FULL', label: 'Full' }
            ]}
          />
          <Input
            placeholder="Filter by city"
            value={filters.city}
            onChange={(e) => setFilters(prev => ({...prev, city: e.target.value}))}
          />
        </div>
      </div>

      {/* Supervisor Table */}
      <div className="bg-white rounded-lg shadow overflow-hidden">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">ID</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Name</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Type</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Authority</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">City</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Events</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Rating</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Actions</th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {supervisors.map(supervisor => (
              <tr key={supervisor.supervisorId} className="hover:bg-gray-50">
                <td className="px-6 py-4 whitespace-nowrap text-sm">#{supervisor.supervisorId}</td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <div>
                    <p className="text-sm font-medium text-gray-900">{supervisor.fullName}</p>
                    <p className="text-sm text-gray-500">{supervisor.email}</p>
                  </div>
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <TypeBadge type={supervisor.supervisorType} />
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <StatusBadge status={supervisor.currentStatus} />
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <AuthorityBadge level={supervisor.authorityLevel} />
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm">{supervisor.city}</td>
                <td className="px-6 py-4 whitespace-nowrap text-sm">{supervisor.totalEventsSupervised}</td>
                <td className="px-6 py-4 whitespace-nowrap text-sm">
                  {supervisor.averageRating ? (
                    <div className="flex items-center">
                      <Star className="h-4 w-4 text-yellow-400 mr-1" />
                      {supervisor.averageRating.toFixed(1)}
                    </div>
                  ) : '-'}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm">
                  <Button
                    size="sm"
                    variant="outline"
                    onClick={() => {
                      setSelectedSupervisor(supervisor);
                      setDrawerOpen(true);
                    }}
                  >
                    Manage
                  </Button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Supervisor Details Drawer */}
      <SupervisorDetailsDrawer
        supervisor={selectedSupervisor}
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        onUpdate={fetchSupervisors}
      />
    </div>
  );
};
```

---

## 📦 API Service Layer

### File: `supervisorApi.js`

```javascript
import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';

const supervisorApi = {
  // Careers Portal
  submitCareersApplication: (data) =>
    axios.post(`${API_BASE_URL}/api/careers/submit`, data),

  getCareersApplication: (applicationId) =>
    axios.get(`${API_BASE_URL}/api/careers/application/${applicationId}`),

  // Registration Portal
  submitRegistration: (data) =>
    axios.post(`${API_BASE_URL}/api/registration/submit`, data),

  getRegistrationProgress: (registrationId) =>
    axios.get(`${API_BASE_URL}/api/registration/progress/${registrationId}`),

  // Supervisor Dashboard
  getSupervisorDashboard: (supervisorId) =>
    axios.get(`${API_BASE_URL}/api/supervisor/dashboard/${supervisorId}`),

  getSupervisorById: (supervisorId) =>
    axios.get(`${API_BASE_URL}/api/supervisor/${supervisorId}`),

  // Admin APIs
  getAllSupervisors: (type = null, status = null) =>
    axios.get(`${API_BASE_URL}/api/admin/supervisors`, { params: { type, status } }),

  searchSupervisors: (filters) =>
    axios.post(`${API_BASE_URL}/api/admin/supervisors/search`, filters),

  updateSupervisorStatus: (supervisorId, newStatus, updatedBy, notes) =>
    axios.post(`${API_BASE_URL}/api/admin/supervisor/${supervisorId}/status`, {
      newStatus, updatedBy, notes
    }),

  updateAuthorityLevel: (supervisorId, newLevel, updatedBy, reason) =>
    axios.post(`${API_BASE_URL}/api/admin/supervisor/${supervisorId}/authority-level`, {
      newLevel, updatedBy, reason
    }),

  suspendSupervisor: (supervisorId, suspendedBy, reason) =>
    axios.post(`${API_BASE_URL}/api/admin/supervisor/${supervisorId}/suspend`, {
      suspendedBy, reason
    }),

  activateSupervisor: (supervisorId, activatedBy) =>
    axios.post(`${API_BASE_URL}/api/admin/supervisor/${supervisorId}/activate`, {
      activatedBy
    }),

  // Document Upload (assuming pre-signed URL approach)
  getUploadUrl: (fileName, fileType) =>
    axios.post(`${API_BASE_URL}/api/upload/presigned-url`, { fileName, fileType }),

  uploadFile: (file, presignedUrl) =>
    axios.put(presignedUrl, file, {
      headers: { 'Content-Type': file.type }
    })
};

export default supervisorApi;
```

---

## ✅ Implementation Checklist

- [ ] Set up React project with Vite
- [ ] Install dependencies (React Router, Axios, Tailwind, Shadcn/UI, React Hook Form, Zod, date-fns)
- [ ] Create enum definitions matching backend
- [ ] Implement API service layer
- [ ] Build common components (StatusBadge, AuthorityBadge, TypeBadge, etc.)
- [ ] Implement Careers Application Form
- [ ] Implement Registration Wizard (6 steps)
- [ ] Implement Progress Trackers
- [ ] Implement Supervisor Dashboard
- [ ] Implement Admin Management Panel
- [ ] Add form validation with Zod
- [ ] Add file upload functionality
- [ ] Add error handling and loading states
- [ ] Add responsive design
- [ ] Test with backend APIs
- [ ] Add permission-based UI rendering
- [ ] Add audit logging display

---

**Document Version:** 1.0
**Status:** ✅ Ready for Implementation
**Backend Compatibility:** 100%
