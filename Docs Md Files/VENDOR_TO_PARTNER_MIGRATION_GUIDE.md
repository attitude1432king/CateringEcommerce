# VENDOR → PARTNER/OWNER MIGRATION GUIDE
## Complete Refactoring Strategy

**Purpose:** Eliminate all "vendor" terminology and replace with "Catering Owner" / "Partner"
**Scope:** Frontend, Backend, Database, Documentation
**Date:** January 30, 2026

---

## 📋 PHASE 1: FRONTEND COMPONENT REFACTORING

### Step 1.1: Search and Replace (Automated)

Run these global search-and-replace operations:

```bash
# In all .jsx/.js files:
Find: "vendor approval"
Replace: "partner approval"

Find: "Vendor approval"
Replace: "Partner approval"

Find: "vendor response"
Replace: "partner response"

Find: "Vendor response"
Replace: "Partner response"

Find: "vendor notified"
Replace: "partner notified"

Find: "Vendor notified"
Replace: "Partner notified"

Find: "vendor-notified"
Replace: "partner-notified"

Find: "vendor-responded"
Replace: "partner-responded"

Find: "vendorResponse"
Replace: "partnerResponse"

Find: "vendorEvidence"
Replace: "partnerEvidence"

Find: "requiresVendorApproval"
Replace: "requiresPartnerApproval"

Find: "vendorId"
Replace: "ownerId"

Find: "VendorId"
Replace: "OwnerId"
```

### Step 1.2: Manual Component Updates

#### TrustBadge.jsx
```jsx
// OLD (WRONG)
export const getTrustLevel = (orderCount, rating, isKYCVerified) => {
  // Vendor stats
}

// NEW (CORRECT)
export const getPartnerTrustLevel = (orderCount, rating, isKYCVerified) => {
  // Catering partner stats
}
```

#### ComplaintStatusTracker.jsx
```jsx
// OLD (WRONG)
const statusConfig = {
  'vendor-notified': { label: 'Vendor Notified', ... },
  'vendor-responded': { label: 'Vendor Responded', ... }
};

// NEW (CORRECT)
const statusConfig = {
  'partner-notified': { label: 'Partner Notified', ... },
  'partner-responded': { label: 'Partner Responded', ... }
};
```

### Step 1.3: Directory Restructuring

```bash
# Rename directories
mv components/owner/dashboard/vendor-enhancements \
   components/owner/dashboard/partner-features

# Or completely remove "vendor" from path
mv components/owner/dashboard/vendor-enhancements \
   components/owner/dashboard/owner-tools
```

---

## 📋 PHASE 2: BACKEND REFACTORING

### Step 2.1: Model Renaming

```bash
# Delete these files (if they exist):
rm Domain/Models/Vendor/*.cs
rm Domain/Interfaces/IVendorPaymentService.cs
rm BAL/Services/VendorPaymentService.cs
rm API/Controllers/VendorPaymentController.cs
```

### Step 2.2: Create New Models

Create these files with CORRECT naming:
- `Domain/Models/Owner/OwnerPayment.cs`
- `Domain/Models/Owner/OwnerSettlement.cs`
- `Domain/Models/Partner/PartnerApprovalRequest.cs`
- `Domain/Interfaces/Services/IOwnerPaymentService.cs`
- `Domain/Interfaces/Services/IPartnerApprovalService.cs`
- `BAL/Services/OwnerPaymentService.cs`
- `API/Controllers/Owner/OwnerPaymentController.cs`
- `API/Controllers/Partner/PartnerApprovalController.cs`

### Step 2.3: Enum Refactoring

```csharp
// DELETE (if exists)
public enum VendorPaymentStatus { }
public enum VendorApprovalStatus { }

// CREATE (correct)
public enum OwnerPaymentStatus {
    PENDING = 1,
    ESCROWED = 2,
    RELEASED = 3,
    FAILED = 4
}

public enum PartnerApprovalStatus {
    PENDING = 1,
    APPROVED = 2,
    REJECTED = 3,
    EXPIRED = 4
}
```

### Step 2.4: Controller Routing Updates

```csharp
// OLD (WRONG)
[Route("api/vendor/payments")]
public class VendorPaymentController { }

// NEW (CORRECT)
[Route("api/owner/payments")]
public class OwnerPaymentController { }

[Route("api/partner/approvals")]
public class PartnerApprovalController { }
```

---

## 📋 PHASE 3: DATABASE MIGRATION

### Step 3.1: Check Existing Tables

```sql
-- Verify which tables exist
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE '%vendor%'
   OR TABLE_NAME LIKE '%owner%';
```

### Step 3.2: Create New Tables (if needed)

```sql
-- Run the schema creation script
-- See: Database/OWNER_PAYMENT_SYSTEM_SCHEMA.sql

-- Creates:
-- t_owner_payment
-- t_owner_settlement
-- t_owner_payout_schedule
-- t_partner_approval_request
-- t_partner_response_history
```

### Step 3.3: Migrate Data (if vendor tables exist)

```sql
-- ONLY if you have existing t_vendor_payment table

-- Migrate data from vendor table to owner table
INSERT INTO t_owner_payment (
    c_owner_id,
    c_order_id,
    c_settlement_amount,
    c_platform_service_fee,
    c_net_settlement_amount,
    c_status,
    c_created_at
)
SELECT
    c_vendor_id, -- Maps to c_owner_id
    c_order_id,
    c_payout_amount, -- Maps to c_settlement_amount
    c_commission, -- Maps to c_platform_service_fee
    c_net_amount, -- Maps to c_net_settlement_amount
    c_status,
    c_created_at
FROM t_vendor_payment;

-- After verification, drop old table
-- DROP TABLE t_vendor_payment; -- CAREFUL!
```

### Step 3.4: Update Foreign Keys

```sql
-- If you have existing FK columns named c_vendor_id
-- Rename them to c_owner_id

-- Example for orders table:
IF EXISTS (SELECT * FROM sys.columns
           WHERE object_id = OBJECT_ID('t_order')
           AND name = 'c_vendor_id')
BEGIN
    EXEC sp_rename 't_order.c_vendor_id', 'c_owner_id', 'COLUMN';
END
```

### Step 3.5: Create Stored Procedures

```sql
-- Run the stored procedures script
-- See: Database/OWNER_PAYMENT_STORED_PROCEDURES.sql

-- Creates procedures with CORRECT naming:
-- sp_CreateOwnerPayment
-- sp_ReleaseOwnerSettlement
-- sp_CreatePartnerApprovalRequest
-- etc.
```

---

## 📋 PHASE 4: API CONTRACT UPDATES

### Step 4.1: Update API Client Code

```javascript
// OLD (WRONG) - Delete these
export const getVendorPayments = async (vendorId) => {
  return fetchApi(`/api/vendor/payments?vendorId=${vendorId}`);
};

// NEW (CORRECT) - Create these
export const getOwnerPayments = async (ownerId) => {
  return fetchApi(`/api/owner/payments/settlements?ownerId=${ownerId}`);
};

export const getPartnerApprovals = async (ownerId) => {
  return fetchApi(`/api/partner/approvals/pending?ownerId=${ownerId}`);
};

export const approvePartnerRequest = async (approvalId, ownerId) => {
  return fetchApi(`/api/partner/approvals/${approvalId}/approve`, 'POST', { ownerId });
};
```

### Step 4.2: Update Request/Response DTOs

```typescript
// OLD (WRONG)
interface VendorPaymentDto {
  vendorId: number;
  payoutAmount: number;
}

// NEW (CORRECT)
interface OwnerSettlementDto {
  ownerId: number;
  settlementAmount: number;
  platformServiceFee: number;
  netAmount: number;
}

interface PartnerApprovalRequestDto {
  ownerId: number;
  orderId: number;
  requestType: string;
  description: string;
}
```

---

## 📋 PHASE 5: UX COPY UPDATES

### Step 5.1: Toast Notifications

```javascript
// OLD (WRONG)
toast.success('Vendor payment released');
toast.error('Vendor approval failed');
toast.info('Waiting for vendor response');

// NEW (CORRECT)
toast.success('Settlement released to catering partner');
toast.error('Partner approval failed');
toast.info('Waiting for catering partner response');
```

### Step 5.2: UI Labels

```jsx
// OLD (WRONG)
<h1>Vendor Dashboard</h1>
<p>Vendor Earnings: ₹10,000</p>
<button>Approve Vendor</button>

// NEW (CORRECT)
<h1>Partner Dashboard</h1>
<p>Owner Earnings: ₹10,000</p>
<button>Approve Partner Request</button>
```

### Step 5.3: Alert Messages

```jsx
// OLD (WRONG)
alert('Vendor has approved your request');
alert('Vendor payout processed');

// NEW (CORRECT)
alert('Catering partner has approved your request');
alert('Partner settlement processed');
```

---

## 📋 PHASE 6: TESTING & VERIFICATION

### Step 6.1: Code Search Verification

Run these searches to ensure NO vendor references remain:

```bash
# Search all files for "vendor"
grep -r "vendor" --include="*.jsx" --include="*.js" --include="*.cs" --include="*.sql"

# Search for capitalized "Vendor"
grep -r "Vendor" --include="*.jsx" --include="*.js" --include="*.cs" --include="*.sql"

# Search for vendor in variable names
grep -r "vendorId\|vendorPayment\|vendorResponse" --include="*.jsx" --include="*.js"

# Search for vendor in database
grep -r "vendor_\|c_vendor" --include="*.sql"
```

### Step 6.2: Manual Verification Checklist

```markdown
Frontend:
- [ ] No "vendor" in file names
- [ ] No "vendor" in component names
- [ ] No "vendor" in function names
- [ ] No "vendor" in variable names (vendorId, vendorResponse, etc.)
- [ ] No "vendor" in UI text/labels
- [ ] All API calls use /owner/* or /partner/* routes

Backend:
- [ ] No VendorPayment classes
- [ ] No IVendorService interfaces
- [ ] No VendorController controllers
- [ ] All models use Owner/Partner naming
- [ ] All routes use /api/owner/* or /api/partner/*

Database:
- [ ] No t_vendor_* tables
- [ ] No c_vendor_id columns
- [ ] All FK references use c_owner_id
- [ ] All procedures use Owner/Partner naming

Documentation:
- [ ] All docs updated with correct terminology
- [ ] API documentation uses Owner/Partner
- [ ] Comments use Owner/Partner terminology
```

### Step 6.3: Integration Testing

```javascript
// Test API endpoints
test('GET /api/owner/payments/settlements returns settlements', async () => {
  const response = await fetch('/api/owner/payments/settlements?ownerId=1');
  expect(response.ok).toBe(true);
  const data = await response.json();
  expect(data.result).toBe(true);
});

test('POST /api/partner/approvals/{id}/approve works', async () => {
  const response = await fetch('/api/partner/approvals/1/approve', {
    method: 'POST',
    body: JSON.stringify({ ownerId: 1, notes: 'Approved' })
  });
  expect(response.ok).toBe(true);
});
```

---

## 📋 PHASE 7: DEPLOYMENT

### Step 7.1: Database Deployment

```sql
-- Run in order:
1. Database/OWNER_PAYMENT_SYSTEM_SCHEMA.sql
2. Database/OWNER_PAYMENT_STORED_PROCEDURES.sql
3. Migrate data from old tables (if any)
4. Verify data integrity
5. Drop old vendor tables (after backup!)
```

### Step 7.2: Backend Deployment

```bash
# Build backend
dotnet build CateringEcommerce.API/CateringEcommerce.API.csproj

# Run tests
dotnet test

# Publish
dotnet publish -c Release
```

### Step 7.3: Frontend Deployment

```bash
cd CateringEcommerce.Web/Frontend

# Install dependencies
npm install

# Build for production
npm run build

# Test build
npm run preview
```

---

## 📋 PHASE 8: ROLLBACK PLAN (IF NEEDED)

### If Issues Arise:

```sql
-- 1. Database rollback (if you kept backups)
-- Restore from backup before running migration

-- 2. Code rollback
git revert <commit-hash>

-- 3. Quick terminology revert (emergency only)
# Run search-replace in reverse
Find: "partner approval"
Replace: "vendor approval"
# etc.
```

---

## 📋 QUICK REFERENCE CARD

### Terminology Cheat Sheet

| Context | ❌ WRONG | ✅ CORRECT |
|---------|----------|------------|
| Entity | Vendor | Catering Owner / Partner |
| ID | vendorId / VendorId | ownerId / OwnerId |
| Payment | Vendor Payout | Partner Settlement / Owner Settlement |
| Fee | Vendor Commission | Platform Service Fee |
| Approval | Vendor Approval | Partner Approval |
| Response | Vendor Response | Partner Response |
| Dashboard | Vendor Dashboard | Partner Dashboard / Owner Dashboard |
| API Route | /api/vendor/* | /api/owner/* or /api/partner/* |
| Table | t_vendor_payment | t_owner_payment |
| Column | c_vendor_id | c_owner_id |
| Class | VendorPayment | OwnerPayment |
| Service | IVendorService | IOwnerPaymentService |
| Enum | VendorStatus | OwnerPaymentStatus |

---

## ✅ COMPLETION CHECKLIST

```markdown
Migration Complete When:
- [ ] All frontend components updated
- [ ] All backend classes renamed
- [ ] All database tables created with correct names
- [ ] All API endpoints updated
- [ ] All stored procedures created
- [ ] All UI labels corrected
- [ ] All API client code updated
- [ ] All documentation updated
- [ ] Code search shows ZERO "vendor" references
- [ ] Integration tests pass
- [ ] Manual testing complete
- [ ] Deployment successful
```

---

## 🚨 COMMON PITFALLS TO AVOID

1. **Don't skip the search-and-replace step**
   - Manual updates are error-prone
   - Use regex search across all files

2. **Don't forget about comments**
   - Code comments often contain vendor terminology
   - Update JSDoc comments and inline comments

3. **Don't assume backend is aligned**
   - Frontend may use "owner" but backend still has "vendor"
   - Coordinate with backend team

4. **Don't ignore database column names**
   - Frontend props should match database columns
   - Use c_owner_id, NOT c_vendor_id

5. **Don't mix terminology**
   - Be consistent: either "Owner" or "Partner", not both
   - Partner = customer-facing
   - Owner = internal/technical

---

## 📞 SUPPORT & ESCALATION

### If You Encounter Issues:

1. **Database FK conflicts**
   - Check existing relationships
   - Update FKs before renaming columns

2. **API 404 errors after deployment**
   - Verify all routes updated on server
   - Check API client code matches backend routes

3. **Frontend props mismatch**
   - Ensure API response keys match frontend expectations
   - Use ownerId consistently in DTOs

4. **Merge conflicts in Git**
   - Prioritize partner/owner terminology
   - Reject any "vendor" references during conflict resolution

---

**Migration Guide Version:** 1.0
**Last Updated:** January 30, 2026
**Target:** Complete Vendor → Owner/Partner Refactoring
