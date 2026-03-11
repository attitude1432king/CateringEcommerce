# Quick Integration Guide
## How to Use the New UX Components

---

## 🚀 Quick Start

### 1. Import Components

```jsx
// Payment Timeline
import PaymentTimeline from '@/components/user/order/PaymentTimeline';

// Trust Badges
import { TrustBadge, PlatformProtectedBadge } from '@/components/common/badges';

// Guest Count
import { GuestCountControl } from '@/components/user/order/guestcount';

// Menu Management
import { MenuChangePanel } from '@/components/user/order/menu';

// Complaints
import { ComplaintSubmissionWizard, ComplaintStatusTracker } from '@/components/user/complaint';

// Safety Components
import { DisabledButton, MonetaryImpactPreview, ConfirmActionModal } from '@/components/common/safety';

// Vendor Dashboard
import { PendingApprovalsWidget, ProcurementReminder, EventProofUpload } from '@/components/owner/dashboard/vendor-enhancements';
```

---

## 📦 Component Integration Examples

### Payment Timeline (Order Detail Page)

```jsx
function OrderDetailPage() {
  const { orderId } = useParams();
  const [order, setOrder] = useState(null);

  return (
    <div className="container mx-auto p-4">
      {/* Platform Protection Badge */}
      <PlatformProtectedBadge variant="compact" className="mb-4" />

      {/* Payment Milestones */}
      <PaymentTimeline order={order} layout="horizontal" />

      {/* Order Timeline */}
      <OrderTimeline order={order} />

      {/* Rest of order details... */}
    </div>
  );
}
```

### Trust Badges (Catering Card)

```jsx
import { TrustBadge, getTrustLevel } from '@/components/common/badges';

function CateringCard({ catering }) {
  const trustLevel = getTrustLevel(
    catering.totalOrders || 0,
    catering.averageRating || 0,
    catering.isKYCVerified !== false
  );

  return (
    <div className="catering-card">
      {/* ... card content ... */}

      {trustLevel && (
        <TrustBadge
          level={trustLevel}
          orderCount={catering.totalOrders}
          rating={catering.averageRating}
          size="sm"
          inline={true}
        />
      )}
    </div>
  );
}
```

### Guest Count Control (Modification Page)

```jsx
function OrderModificationPage() {
  const handleGuestCountChange = async (newCount, priceImpact) => {
    try {
      const response = await updateGuestCount(orderId, newCount);
      if (response.result) {
        toast.success('Guest count updated successfully');
        refreshOrder();
      }
    } catch (error) {
      toast.error('Failed to update guest count');
    }
  };

  return (
    <GuestCountControl
      order={order}
      onGuestCountChange={handleGuestCountChange}
      isLoading={isUpdating}
    />
  );
}
```

### Menu Changes (Modification Page)

```jsx
function OrderMenuPage() {
  const handleMenuChange = async (changes) => {
    try {
      const response = await submitMenuChanges(orderId, changes);
      if (response.result) {
        toast.success('Menu change request submitted');
      }
    } catch (error) {
      toast.error('Failed to submit changes');
    }
  };

  const handleAllergyEmergency = async (allergyData) => {
    try {
      await notifyChefEmergency(orderId, allergyData);
      toast.success('Chef has been notified immediately');
    } catch (error) {
      toast.error('Failed to send emergency notification');
    }
  };

  return (
    <MenuChangePanel
      order={order}
      menuItems={order.menuItems}
      onMenuChange={handleMenuChange}
      onAllergyEmergency={handleAllergyEmergency}
      isLoading={isSubmitting}
    />
  );
}
```

### Complaint Submission

```jsx
function ComplaintPage() {
  const [showWizard, setShowWizard] = useState(false);

  const handleSubmitComplaint = async (complaintData) => {
    try {
      const formData = new FormData();
      formData.append('orderId', complaintData.orderId);
      formData.append('type', complaintData.type);
      formData.append('description', complaintData.description);
      formData.append('severity', complaintData.severity);
      formData.append('guestsAffected', complaintData.guestsAffected);

      complaintData.media.forEach((media, index) => {
        formData.append(`media[${index}]`, media.file);
      });

      const response = await submitComplaint(formData);
      if (response.result) {
        toast.success('Complaint submitted successfully');
        setShowWizard(false);
        navigate(`/complaints/${response.data.complaintId}`);
      }
    } catch (error) {
      toast.error('Failed to submit complaint');
    }
  };

  return (
    <div>
      <button onClick={() => setShowWizard(true)}>
        File Complaint
      </button>

      {showWizard && (
        <ComplaintSubmissionWizard
          order={order}
          onSubmit={handleSubmitComplaint}
          onCancel={() => setShowWizard(false)}
          isLoading={isSubmitting}
        />
      )}
    </div>
  );
}
```

### Safety Components (Generic Usage)

```jsx
function SomeActionPage() {
  const [showConfirm, setShowConfirm] = useState(false);

  return (
    <>
      {/* Disabled Button with Tooltip */}
      <DisabledButton
        onClick={handleAction}
        disabled={!canPerformAction}
        disabledReason="Guest count is locked. Changes not allowed."
        variant="primary"
        icon={Edit}
      >
        Modify Order
      </DisabledButton>

      {/* Monetary Impact Preview */}
      <MonetaryImpactPreview
        currentAmount={1000}
        newAmount={1200}
        breakdown={[
          { label: '5 additional guests', amount: 200, type: 'add' }
        ]}
        showPercentage={true}
        warningMessage="Increase will be charged immediately"
      />

      {/* Confirmation Modal */}
      <ConfirmActionModal
        isOpen={showConfirm}
        onClose={() => setShowConfirm(false)}
        onConfirm={handleConfirm}
        title="Confirm Cancellation"
        description="This will cancel your order permanently."
        type="danger"
        isIrreversible={true}
        requiresConfirmation={true}
        monetaryImpact={{
          currentAmount: 5000,
          newAmount: 3500,
          warningMessage: 'Only 70% refund due to late cancellation'
        }}
      />
    </>
  );
}
```

### Vendor Dashboard Widgets

```jsx
function VendorDashboard() {
  const handleApprove = async (approvalId) => {
    await approveRequest(approvalId);
    toast.success('Request approved');
    refreshApprovals();
  };

  const handleReject = async (approvalId) => {
    await rejectRequest(approvalId);
    toast.success('Request rejected');
    refreshApprovals();
  };

  const handleProofUpload = async (proofData) => {
    const formData = new FormData();
    formData.append('type', proofData.type);
    formData.append('file', proofData.file);
    formData.append('notes', proofData.notes);

    await uploadProof(proofData.orderId, formData);
    toast.success('Proof uploaded successfully');
    refreshOrder();
  };

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
      {/* Pending Approvals */}
      <PendingApprovalsWidget
        pendingApprovals={approvals}
        onApprove={handleApprove}
        onReject={handleReject}
        isLoading={isProcessing}
      />

      {/* Procurement Reminders */}
      <ProcurementReminder
        upcomingOrders={upcomingOrders}
      />

      {/* Event Proof Upload (for specific order) */}
      <EventProofUpload
        order={currentOrder}
        existingProofs={currentOrder.proofs}
        onUploadProof={handleProofUpload}
        isLoading={isUploading}
      />
    </div>
  );
}
```

---

## 🎨 Styling Notes

All components use Tailwind CSS and are fully customizable via className prop:

```jsx
<PaymentTimeline
  order={order}
  layout="horizontal"
  className="my-custom-class"
/>
```

### Color Customization

Components use these Tailwind color classes:
- Blue: Info, default states
- Green: Success, approved, completed
- Red: Errors, rejected, failed
- Amber/Orange: Warnings, urgent
- Purple: Vendor-specific actions
- Gray: Disabled, neutral

---

## 📱 Responsive Design

All components are mobile-responsive by default:
- Grid layouts collapse on mobile
- Buttons adjust to full-width on small screens
- Modals use bottom sheets on mobile
- Touch-friendly targets (44x44px minimum)

---

## ⚠️ Important Notes

### Date Handling
- Components expect JavaScript Date objects or ISO strings
- All calculations use local timezone
- Consider timezone conversion for multi-region deployments

### File Uploads
- Camera capture works only on HTTPS or localhost
- Max file size should be enforced server-side
- Consider CDN for uploaded images

### Real-Time Updates
- Countdown timers update every minute
- For live updates, integrate WebSocket or polling
- Consider React Query for cache invalidation

### Error Handling
- All async operations should be wrapped in try-catch
- Use toast notifications for user feedback
- Log errors to monitoring service

---

## 🔧 Customization

### Override Default Behaviors

```jsx
// Custom validation
<DisabledButton
  disabled={myCustomValidation()}
  disabledReason={getCustomReason()}
/>

// Custom styling
<MonetaryImpactPreview
  className="my-4 shadow-lg"
  warningMessage={getCustomWarning()}
/>

// Custom confirmation flow
<ConfirmActionModal
  requiresConfirmation={isHighRisk}
  confirmationText="DELETE"
/>
```

---

## 🧪 Testing Integration

```jsx
import { render, screen, fireEvent } from '@testing-library/react';
import { DisabledButton } from '@/components/common/safety';

test('shows disabled reason on hover', async () => {
  render(
    <DisabledButton
      disabled={true}
      disabledReason="Action not allowed"
    >
      Click Me
    </DisabledButton>
  );

  const button = screen.getByRole('button');
  fireEvent.mouseEnter(button);

  expect(screen.getByText('Action not allowed')).toBeInTheDocument();
});
```

---

## 🚦 Backend Integration Checklist

- [ ] Order details endpoint returns payment milestone data
- [ ] Guest count modification endpoint with validation
- [ ] Menu change request submission endpoint
- [ ] Complaint submission with file upload
- [ ] Vendor approval endpoints
- [ ] Proof upload endpoint with timestamp
- [ ] Real-time notification system (optional)

---

## 💡 Best Practices

1. **Always show monetary impact before confirmation**
   ```jsx
   // Bad
   <button onClick={handleUpdate}>Update</button>

   // Good
   <MonetaryImpactPreview {...priceChange} />
   <ConfirmActionModal monetaryImpact={priceChange} />
   ```

2. **Use disabled states with reasons**
   ```jsx
   // Bad
   {canEdit && <button>Edit</button>}

   // Good
   <DisabledButton
     disabled={!canEdit}
     disabledReason="Locked 5 days before event"
   >
     Edit
   </DisabledButton>
   ```

3. **Provide loading states**
   ```jsx
   <DisabledButton
     loading={isSubmitting}
     disabled={isSubmitting}
   >
     Submit
   </DisabledButton>
   ```

4. **Combine safety components**
   ```jsx
   <DisabledButton onClick={() => setShowModal(true)}>
     Cancel Order
   </DisabledButton>

   <ConfirmActionModal
     isIrreversible={true}
     monetaryImpact={refundInfo}
   />
   ```

---

## 📚 Additional Resources

- [Tailwind CSS Documentation](https://tailwindcss.com)
- [Lucide React Icons](https://lucide.dev)
- [React Hook Form](https://react-hook-form.com) (for forms)
- [React Query](https://tanstack.com/query) (for data fetching)

---

**Happy Coding!** 🎉
