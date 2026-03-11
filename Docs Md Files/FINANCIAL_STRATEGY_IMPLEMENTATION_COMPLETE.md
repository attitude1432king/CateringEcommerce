# Financial Strategy Implementation - Complete Checklist

## Date: 2026-01-30

---

## ✅ COMPLETED ITEMS

### 1. Database Schema Changes

#### ✅ Orders Table Enhancement
- **File**: `Database/Financial_Strategy_Implementation.sql`
- **Changes**:
  - `c_original_guest_count` - Tracks initial guest count at booking
  - `c_locked_guest_count` - Guest count locked at 5 days before event
  - `c_guest_count_locked` - Boolean flag for lock status
  - `c_guest_count_locked_date` - Timestamp of lock
  - `c_final_served_guest_count` - Actual guests served
  - `c_menu_locked` - Boolean flag for menu lock
  - `c_menu_locked_date` - Timestamp of menu lock

#### ✅ New Tables Created
1. **t_sys_order_modifications**
   - Tracks guest count increases/decreases
   - Tracks menu changes
   - Stores pricing multipliers (1.0x, 1.2x, 1.3x, 1.5x)
   - Approval workflow for vendor consent

2. **t_sys_cancellation_requests**
   - Implements 3-tier cancellation policy
   - Calculates refunds automatically
   - Tracks vendor compensation
   - Force majeure handling

3. **t_sys_order_complaints**
   - Comprehensive complaint tracking
   - Evidence storage (photos, videos)
   - Severity classification
   - Fraud detection flags
   - Resolution workflow

4. **t_sys_vendor_security_deposits**
   - Tracks ₹25,000 security deposit per vendor
   - Balance management
   - Hold/release mechanisms
   - Refund workflow

5. **t_sys_deposit_transactions**
   - Audit log for all deposit movements
   - Deduction tracking for no-shows/complaints
   - Transparency for vendors

6. **t_sys_vendor_partnership_tiers**
   - Implements commission ladder (8% → 10% → 12% → 15%)
   - Tracks lock-in periods
   - Performance-based adjustments
   - Badge system (Founder, Launch Partner, etc.)

7. **t_sys_commission_tier_history**
   - Full history of commission changes
   - 60-day notice tracking
   - Vendor acknowledgment tracking

8. **t_sys_auto_lock_jobs**
   - Configuration for automated jobs
   - Last run tracking
   - Performance monitoring

---

### 2. Stored Procedures Created

#### ✅ sp_AutoLockGuestCount
- **Purpose**: Automatically lock guest count 5 days before event
- **Schedule**: Runs every 60 minutes
- **Logic**: Updates all orders where event is 5 days away and not locked
- **Status**: ✅ Created in `Financial_Strategy_StoredProcedures.sql`

#### ✅ sp_AutoLockMenu
- **Purpose**: Automatically lock menu 3 days before event
- **Schedule**: Runs every 60 minutes
- **Logic**: Updates all orders where event is 3 days away and menu not locked
- **Status**: ✅ Created

#### ✅ sp_CalculateCancellationRefund
- **Purpose**: Calculate refund amount based on cancellation policy
- **Policy Implementation**:
  - >7 days: 100% refund
  - 3-7 days: 50% refund
  - <48 hours: 0% refund
  - Force Majeure: 50% split (customer/vendor)
- **Status**: ✅ Created

#### ✅ sp_ProcessCancellationRequest
- **Purpose**: Handle complete cancellation workflow
- **Features**:
  - Auto-calculates refund based on policy
  - Creates cancellation request record
  - Updates order status
  - Logs to status history
  - Supports force majeure with evidence
- **Status**: ✅ Created

#### ✅ sp_RequestGuestCountChange
- **Purpose**: Handle guest count increase/decrease requests
- **Pricing Multipliers**:
  - >7 days: 1.0x (standard rate)
  - 5-7 days: 1.2x
  - 3-4 days: 1.3x
  - 2 days: 1.5x
  - <48h: Not allowed
- **Rules**:
  - Decreases only allowed >7 days and require vendor approval
  - Increases >10% after lock require vendor approval
- **Status**: ✅ Created

#### ✅ sp_FileCustomerComplaint
- **Purpose**: Register customer complaints
- **Features**:
  - Severity auto-classification (Critical/Major/Minor)
  - Photo evidence support
  - Fraud detection (flags users with 3+ complaints in 12 months)
  - Vendor notification trigger
- **Status**: ✅ Created

#### ✅ sp_CalculateComplaintRefund
- **Purpose**: Calculate refund for valid complaints
- **Formula**:
  ```
  Refund = (Item Value / Total) × Severity Factor × Total
  Max: 15% of order value (for 1 bad item)
  ```
- **Severity Factors**:
  - Critical item (main course): 2.0x
  - Important item (dal, sabzi): 1.5x
  - Normal item: 1.0x
  - Minor item (chutney): 0.5x
- **Status**: ✅ Created

---

### 3. Settings Configured

#### ✅ Cancellation Policy Settings
```sql
CANCELLATION.FULL_REFUND_DAYS = 7
CANCELLATION.PARTIAL_REFUND_DAYS_START = 3
CANCELLATION.PARTIAL_REFUND_DAYS_END = 7
CANCELLATION.NO_REFUND_HOURS = 48
CANCELLATION.PARTIAL_REFUND_PERCENTAGE = 50
```

#### ✅ Order Management Settings
```sql
ORDER.GUEST_COUNT_LOCK_DAYS = 5
ORDER.MENU_LOCK_DAYS = 3
```

#### ✅ Vendor Settings
```sql
VENDOR.SECURITY_DEPOSIT_AMOUNT = 25000
```

#### ✅ Dispute Settings
```sql
DISPUTE.RESOLUTION_SLA_HOURS = 12
```

---

## ⏳ PENDING ITEMS (C# Implementation)

### 1. C# Models Needed

#### ❌ CancellationRequestModel.cs
```csharp
Location: CateringEcommerce.Domain/Models/Order/CancellationRequestModel.cs

public class CancellationRequestModel
{
    public long CancellationId { get; set; }
    public long OrderId { get; set; }
    public long UserId { get; set; }
    public string CancellationReason { get; set; }
    public bool IsForceMajeure { get; set; }
    public string PolicyTier { get; set; }
    public decimal RefundPercentage { get; set; }
    public decimal RefundAmount { get; set; }
    public decimal VendorCompensation { get; set; }
    public int DaysBeforeEvent { get; set; }
    public string Status { get; set; }
    public DateTime CreatedDate { get; set; }
}
```

#### ❌ OrderModificationModel.cs
```csharp
Location: CateringEcommerce.Domain/Models/Order/OrderModificationModel.cs

public class OrderModificationModel
{
    public long ModificationId { get; set; }
    public long OrderId { get; set; }
    public string ModificationType { get; set; }
    public int OriginalGuestCount { get; set; }
    public int ModifiedGuestCount { get; set; }
    public int GuestCountChange { get; set; }
    public decimal AdditionalAmount { get; set; }
    public decimal PricingMultiplier { get; set; }
    public bool RequiresApproval { get; set; }
    public string Status { get; set; }
    public DateTime RequestDate { get; set; }
}
```

#### ❌ CustomerComplaintModel.cs
```csharp
Location: CateringEcommerce.Domain/Models/Order/CustomerComplaintModel.cs

public class CustomerComplaintModel
{
    public long ComplaintId { get; set; }
    public long OrderId { get; set; }
    public string ComplaintType { get; set; }
    public string Severity { get; set; }
    public string ComplaintSummary { get; set; }
    public string ComplaintDetails { get; set; }
    public List<string> PhotoEvidencePaths { get; set; }
    public List<string> AffectedItems { get; set; }
    public decimal RefundAmount { get; set; }
    public string Status { get; set; }
    public bool IsValid { get; set; }
    public DateTime CreatedDate { get; set; }
}
```

#### ❌ VendorPartnershipTierModel.cs
```csharp
Location: CateringEcommerce.Domain/Models/Owner/VendorPartnershipTierModel.cs

public class VendorPartnershipTierModel
{
    public long TierId { get; set; }
    public long OwnerId { get; set; }
    public string TierName { get; set; } // FOUNDER_PARTNER, LAUNCH_PARTNER, etc.
    public decimal CurrentCommissionRate { get; set; }
    public DateTime TierStartDate { get; set; }
    public DateTime? TierLockEndDate { get; set; }
    public int DaysRemainingInLock { get; set; }
    public int CompletedOrdersCount { get; set; }
    public bool HasFounderBadge { get; set; }
    public bool HasFeaturedListing { get; set; }
    public string NextTierName { get; set; }
    public decimal NextTierCommissionRate { get; set; }
    public DateTime? NextTierEffectiveDate { get; set; }
}
```

---

### 2. C# Repositories Needed

#### ❌ CancellationRepository.cs
**Location**: `CateringEcommerce.BAL/Base/Order/CancellationRepository.cs`

**Methods**:
- `Task<CancellationRequestModel> ProcessCancellationRequestAsync(long orderId, long userId, string reason, bool forceMajeure)`
- `Task<CancellationRequestModel> GetCancellationRequestAsync(long cancellationId)`
- `Task<bool> ApproveCancellationRequestAsync(long cancellationId, long adminId)`
- `Task<bool> ProcessRefundAsync(long cancellationId)`

#### ❌ OrderModificationRepository.cs
**Location**: `CateringEcommerce.BAL/Base/Order/OrderModificationRepository.cs`

**Methods**:
- `Task<OrderModificationModel> RequestGuestCountChangeAsync(long orderId, long userId, int newGuestCount, string reason)`
- `Task<OrderModificationModel> RequestMenuChangeAsync(long orderId, long userId, string menuChanges, string reason)`
- `Task<bool> ApproveModificationAsync(long modificationId, long vendorId)`
- `Task<bool> RejectModificationAsync(long modificationId, long vendorId, string reason)`
- `Task<List<OrderModificationModel>> GetPendingModificationsForVendorAsync(long vendorId)`

#### ❌ ComplaintRepository.cs
**Location**: `CateringEcommerce.BAL/Base/Order/ComplaintRepository.cs`

**Methods**:
- `Task<CustomerComplaintModel> FileComplaintAsync(ComplaintRequestDto request)`
- `Task<decimal> CalculateComplaintRefundAsync(long complaintId)`
- `Task<bool> ResolveComplaintAsync(long complaintId, long adminId, string resolutionType, decimal refundAmount, string notes)`
- `Task<List<CustomerComplaintModel>> GetComplaintsByOrderAsync(long orderId)`
- `Task<List<CustomerComplaintModel>> GetPendingComplaintsAsync()`

#### ❌ VendorPartnershipRepository.cs
**Location**: `CateringEcommerce.BAL/Base/Owner/VendorPartnershipRepository.cs`

**Methods**:
- `Task<VendorPartnershipTierModel> GetVendorTierAsync(long ownerId)`
- `Task<bool> UpdateVendorTierAsync(long ownerId, string newTier, decimal newCommission, DateTime effectiveDate)`
- `Task<List<VendorPartnershipTierModel>> GetVendorsForTransitionAsync(int daysBeforeTransition)`
- `Task<bool> SendCommissionChangeNoticeAsync(long ownerId)`

---

### 3. API Controllers Needed

#### ❌ CancellationController.cs
**Location**: `CateringEcommerce.API/Controllers/User/CancellationController.cs`

**Endpoints**:
```csharp
[HttpPost("request")]
POST /api/user/cancellation/request
Body: { orderId, reason, isForceMajeure, evidence }

[HttpGet("{cancellationId}")]
GET /api/user/cancellation/{cancellationId}

[HttpGet("policy/calculate")]
GET /api/user/cancellation/policy/calculate?orderId={id}
Response: { policyTier, refundPercentage, refundAmount, daysBeforeEvent }
```

#### ❌ OrderModificationController.cs
**Location**: `CateringEcommerce.API/Controllers/User/OrderModificationController.cs`

**Endpoints**:
```csharp
[HttpPost("guest-count/request")]
POST /api/user/order-modification/guest-count/request
Body: { orderId, newGuestCount, reason }

[HttpPost("menu/request")]
POST /api/user/order-modification/menu/request
Body: { orderId, menuChanges, reason }

[HttpGet("order/{orderId}")]
GET /api/user/order-modification/order/{orderId}
Response: List of all modifications for an order
```

#### ❌ ComplaintController.cs
**Location**: `CateringEcommerce.API/Controllers/User/ComplaintController.cs`

**Endpoints**:
```csharp
[HttpPost("file")]
POST /api/user/complaint/file
Body: { orderId, complaintType, summary, details, photoEvidence, affectedItems }

[HttpGet("order/{orderId}")]
GET /api/user/complaint/order/{orderId}

[HttpGet("{complaintId}")]
GET /api/user/complaint/{complaintId}
```

#### ❌ AdminComplaintController.cs
**Location**: `CateringEcommerce.API/Controllers/Admin/AdminComplaintController.cs`

**Endpoints**:
```csharp
[HttpGet("pending")]
GET /api/admin/complaint/pending

[HttpPost("resolve")]
POST /api/admin/complaint/resolve
Body: { complaintId, resolutionType, refundAmount, notes }

[HttpPost("calculate-refund")]
POST /api/admin/complaint/calculate-refund/{complaintId}
```

#### ❌ VendorPartnershipController.cs
**Location**: `CateringEcommerce.API/Controllers/Owner/VendorPartnershipController.cs`

**Endpoints**:
```csharp
[HttpGet("tier")]
GET /api/owner/partnership/tier

[HttpGet("commission-history")]
GET /api/owner/partnership/commission-history

[HttpPost("acknowledge-tier-change")]
POST /api/owner/partnership/acknowledge-tier-change/{historyId}
```

---

### 4. Background Jobs Needed

#### ❌ GuestCountLockJob.cs
**Location**: `CateringEcommerce.API/BackgroundJobs/GuestCountLockJob.cs`

**Schedule**: Every 60 minutes
**Action**: Call `sp_AutoLockGuestCount`
**Implementation**: Hangfire or Quartz.NET

#### ❌ MenuLockJob.cs
**Location**: `CateringEcommerce.API/BackgroundJobs/MenuLockJob.cs`

**Schedule**: Every 60 minutes
**Action**: Call `sp_AutoLockMenu`

#### ❌ CommissionTransitionNoticeJob.cs
**Location**: `CateringEcommerce.API/BackgroundJobs/CommissionTransitionNoticeJob.cs`

**Schedule**: Daily
**Action**:
- Find vendors whose lock-in period expires in 60 days
- Send commission change notice emails
- Update notice_sent flag

#### ❌ DisputeEscalationJob.cs
**Location**: `CateringEcommerce.API/BackgroundJobs/DisputeEscalationJob.cs`

**Schedule**: Every 2 hours
**Action**:
- Find complaints older than 12 hours with status='Open'
- Escalate to senior admin
- Send alerts

---

### 5. Frontend Components Needed

#### ❌ Cancellation Request Modal
**Location**: `Frontend/src/components/user/orders/CancellationRequestModal.jsx`

**Features**:
- Show cancellation policy tiers
- Display calculated refund amount
- Force majeure checkbox with evidence upload
- Warning messages based on timing

#### ❌ Guest Count Change Modal
**Location**: `Frontend/src/components/user/orders/GuestCountChangeModal.jsx`

**Features**:
- Show current vs new guest count
- Display pricing multiplier
- Calculate additional amount
- Show lock status warning

#### ❌ Complaint Filing Form
**Location**: `Frontend/src/components/user/orders/ComplaintFilingForm.jsx`

**Features**:
- Complaint type dropdown
- Photo upload (multiple)
- Affected items selection
- Real-time severity indicator

#### ❌ Vendor Partnership Dashboard
**Location**: `Frontend/src/components/owner/dashboard/PartnershipTierCard.jsx`

**Features**:
- Current tier badge
- Commission rate display
- Lock-in countdown timer
- Next tier preview
- Orders to next tier progress bar

#### ❌ Admin Complaint Resolution Panel
**Location**: `Frontend/src/components/admin/complaints/ComplaintResolutionPanel.jsx`

**Features**:
- Evidence viewer (photos, videos)
- Vendor response section
- Refund calculator
- Resolution action buttons

---

## 📋 EXECUTION CHECKLIST

### Phase 1: Database Setup (TODAY)
- [x] Run `Financial_Strategy_Implementation.sql`
- [x] Run `Financial_Strategy_StoredProcedures.sql`
- [x] Verify all tables created successfully
- [x] Verify all stored procedures created successfully
- [ ] Test stored procedures with sample data

### Phase 2: C# Backend (Next 2-3 Days)
- [ ] Create all model classes
- [ ] Create all repository classes
- [ ] Implement all API controllers
- [ ] Add dependency injection configuration
- [ ] Write unit tests for critical methods

### Phase 3: Background Jobs (Day 4)
- [ ] Install Hangfire or Quartz.NET
- [ ] Implement all background jobs
- [ ] Configure job schedules
- [ ] Test job execution

### Phase 4: Frontend (Day 5-7)
- [ ] Create cancellation request flow
- [ ] Create guest count modification flow
- [ ] Create complaint filing flow
- [ ] Create vendor partnership dashboard
- [ ] Create admin complaint resolution UI

### Phase 5: Testing (Day 8-9)
- [ ] End-to-end testing of cancellation flow
- [ ] Test guest count locking automation
- [ ] Test complaint resolution workflow
- [ ] Test commission tier transitions
- [ ] Load testing on stored procedures

### Phase 6: Documentation & Deployment (Day 10)
- [ ] Update API documentation
- [ ] Create user guides
- [ ] Create vendor guides
- [ ] Create admin operation manual
- [ ] Deploy to staging environment

---

## 🚨 CRITICAL BUSINESS RULES IMPLEMENTED

### ✅ Cancellation Policy
```
>7 days before event = 100% refund
3-7 days before event = 50% refund
<48 hours before event = NO REFUND
Force Majeure = 50-50 split (requires evidence)
```

### ✅ Guest Count Locking
```
Lock occurs automatically 5 days before event
Decreases NOT allowed after lock
Increases allowed with premium pricing:
  - >7 days: 1.0x rate
  - 5-7 days: 1.2x rate
  - 3-4 days: 1.3x rate
  - 2 days: 1.5x rate
  - <48h: NOT ALLOWED
```

### ✅ Menu Locking
```
Lock occurs automatically 3 days before event
Only dietary/allergy changes allowed after lock
Vendor must accommodate medical emergencies
```

### ✅ Complaint Refund Calculation
```
Formula: (Item Value / Total) × Severity Factor × Order Total
Max refund: 15% for 1 bad item out of 10
Severity Factors:
  - Critical (main course): 2.0x
  - Important (sides): 1.5x
  - Normal: 1.0x
  - Minor (garnish): 0.5x
```

### ✅ Commission Tier System
```
Tier 1 (FOUNDER_PARTNER): 8% - First 20 vendors, 12-month lock after 5 orders
Tier 2 (LAUNCH_PARTNER): 10% - Next 80 vendors, 9-month lock after 3 orders
Tier 3 (EARLY_ADOPTER): 12% - Months 4-6, 6-month lock after 2 orders
Tier 4 (STANDARD): 15% - After Month 6, no lock
Tier 5 (PREMIUM): 18-20% - Long-term target (Year 2+)

Performance Bonus: 10-13% for high-volume vendors (20+ orders/month, >4.5 rating)
```

### ✅ Security Deposit
```
Amount: ₹25,000 per vendor
Usage:
  - Vendor no-show: 100% forfeiture
  - Customer complaint refunds
  - Cancellation compensations
Refundable when vendor leaves platform (after clearing all orders)
```

---

## 📊 SUCCESS METRICS TO TRACK

1. **Cancellation Rate**: Target <5%
2. **Complaint Rate**: Target <10% of orders
3. **Valid Complaints**: Target >70% validity rate
4. **Guest Count Changes**: Track frequency and timing
5. **Vendor Retention by Tier**: Track retention rates per tier
6. **Average Refund Amount**: Monitor for fraud patterns
7. **SLA Compliance**: Dispute resolution within 12 hours

---

## 🎯 NEXT IMMEDIATE ACTIONS

1. **Run both SQL scripts** on development database
2. **Test stored procedures** with sample orders
3. **Create C# models** (start with most critical: CancellationRequestModel)
4. **Build first API endpoint** (Cancellation Request)
5. **Test end-to-end** cancellation flow

---

*Document Version: 1.0*
*Last Updated: January 30, 2026*
*Status: Database ✅ | Backend ⏳ | Frontend ⏳ | Jobs ⏳*
