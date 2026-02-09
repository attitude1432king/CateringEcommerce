# Admin Partner Approval & Rejection Flow - Implementation Guide

## 📋 Table of Contents

1. [Overview](#overview)
2. [Architecture & Design Decisions](#architecture--design-decisions)
3. [Enum-Based Implementation](#enum-based-implementation)
4. [API Endpoints](#api-endpoints)
5. [Database Schema](#database-schema)
6. [Why NOT to Reuse OwnerProfile.cs](#why-not-to-reuse-ownerprofilecs)
7. [Usage Examples](#usage-examples)
8. [Testing Guide](#testing-guide)

---

## Overview

This implementation provides a **clean, scalable, and enum-driven** Partner Request Approval & Rejection flow for the Admin side of the Catering E-commerce platform.

### Key Features

✅ **Enum-Driven Design**: Uses `ApprovalStatus` and `PriorityStatus` enums with INT database columns
✅ **Strict Validation**: Prevents duplicate approvals, invalid state transitions
✅ **Separation of Concerns**: Admin review logic completely separated from post-approval partner operations
✅ **Auditable**: All status changes are trackable and logged
✅ **Clean DTOs**: Returns enum display names to frontend (not raw integers or strings)
✅ **Read-Only Admin View**: Admin sees registration data but does NOT edit it

---

## Architecture & Design Decisions

### 1. **NEW Implementation (Not Modification)**

We created **entirely new** files instead of modifying the existing `AdminPartnerRequestRepository.cs`:

- **New Repository**: `AdminPartnerApprovalRepository.cs`
- **New DTOs**: `PartnerApprovalModels.cs`
- **New Controller**: `PartnerApprovalController.cs`
- **New Interface**: `IAdminPartnerApprovalRepository.cs`
- **New Helper**: `EnumHelper.cs`

**Why?**

The existing implementation used string-based status values ("PENDING", "APPROVED") instead of INT-based enums. A complete rewrite ensures:

- No risk of breaking existing functionality
- Clean enum-based design from ground up
- Easy to compare old vs new implementation
- Migration path is clear

### 2. **Enum-First Design**

All status and priority values are **stored as INT** in the database and mapped to C# enums:

```csharp
// Database: c_approval_status INT
public enum ApprovalStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    UnderReview = 4,
    Info_Requested = 5
}

// Database: c_priority INT
public enum PriorityStatus
{
    Low = 0,
    Normal = 1,
    High = 2,
    Urgent = 3
}
```

**Benefits:**

- Type-safe: Compile-time checking prevents invalid values
- Database-efficient: INT columns are faster than VARCHAR
- No magic strings: All status comparisons use enum values
- Maintainable: Adding new statuses is straightforward

### 3. **Clean Separation: Registration vs Operations**

| Aspect | Admin Review (NEW) | Partner Operations (Existing) |
|--------|-------------------|-------------------------------|
| **Purpose** | Review registration data | Manage operational data |
| **File** | `AdminPartnerApprovalRepository.cs` | `OwnerProfile.cs` |
| **When Used** | BEFORE approval | AFTER approval |
| **Data Source** | Registration tables | Operational tables |
| **Permissions** | Admin only | Partner only |
| **Operations** | Read-only review, approve/reject | Full CRUD on partner profile |

**⚠️ CRITICAL**: Never mix these two concerns!

---

## Enum-Based Implementation

### EnumHelper Utility

The `EnumHelper` class provides conversions between enum values, INT codes, and display names:

```csharp
// Convert INT → Enum Display Name
var statusName = EnumHelper.GetDisplayNameFromInt<ApprovalStatus>(1);
// Result: "Pending"

// Convert INT → Enum
var status = EnumHelper.GetEnumFromInt<ApprovalStatus>(1);
// Result: ApprovalStatus.Pending

// Convert Enum → INT
var statusId = EnumHelper.GetIntValue(ApprovalStatus.Approved);
// Result: 2

// Validate INT value
var isValid = EnumHelper.IsValidEnumValue<ApprovalStatus>(5);
// Result: true

// Get all enum options for dropdown
var options = EnumHelper.GetEnumDictionary<ApprovalStatus>();
// Result: { 1: "Pending", 2: "Approved", 3: "Rejected", ... }
```

### DTOs Return Both ID and Display Name

All DTOs return **both** the enum INT value and the display name:

```csharp
public class PartnerRequestListItem
{
    public int ApprovalStatusId { get; set; }           // 1, 2, 3...
    public string ApprovalStatusName { get; set; }      // "Pending", "Approved"...

    public int PriorityId { get; set; }                 // 0, 1, 2, 3
    public string PriorityName { get; set; }            // "Low", "Normal"...
}
```

**Frontend Benefits:**

- Display: Use `ApprovalStatusName` for user-friendly text
- Filtering: Use `ApprovalStatusId` for precise filtering
- Styling: Use `ApprovalStatusId` for conditional CSS classes

---

## API Endpoints

### Base URL: `/api/admin/partners`

### 1. **Get Pending Partner Requests**

```http
GET /api/admin/partners/pending
```

**Query Parameters:**

| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `pageNumber` | int | Page number (default: 1) | `1` |
| `pageSize` | int | Items per page (default: 20) | `20` |
| `searchTerm` | string | Search in name, email, phone | `"Mumbai Caterers"` |
| `approvalStatusId` | int | Filter by status (1-5) | `1` (Pending) |
| `priorityId` | int | Filter by priority (0-3) | `2` (High) |
| `cityId` | int | Filter by city | `1401` (Mumbai) |
| `fromDate` | date | Registration date from | `2025-01-01` |
| `toDate` | date | Registration date to | `2025-01-31` |
| `sortBy` | string | Column to sort | `c_createddate` |
| `sortOrder` | string | ASC or DESC | `DESC` |

**Response:**

```json
{
  "success": true,
  "message": "Partner requests retrieved successfully",
  "data": {
    "requests": [
      {
        "ownerId": 12345,
        "businessName": "Mumbai Caterers",
        "ownerName": "Rajesh Kumar",
        "phone": "9876543210",
        "email": "rajesh@example.com",
        "city": "Mumbai",
        "state": "Maharashtra",
        "approvalStatusId": 1,
        "approvalStatusName": "Pending",
        "priorityId": 1,
        "priorityName": "Normal",
        "registrationDate": "2025-01-15T10:30:00",
        "approvedDate": null,
        "documentCount": 5
      }
    ],
    "totalCount": 25,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 2,
    "stats": {
      "totalRequests": 100,
      "pendingCount": 25,
      "approvedCount": 60,
      "rejectedCount": 10,
      "underReviewCount": 3,
      "infoRequestedCount": 2
    }
  }
}
```

### 2. **Get Partner Registration Detail**

```http
GET /api/admin/partners/{partnerId}/registration-detail
```

**Response:**

```json
{
  "success": true,
  "message": "Partner request details retrieved successfully",
  "data": {
    "ownerId": 12345,
    "businessName": "Mumbai Caterers",
    "ownerName": "Rajesh Kumar",
    "email": "rajesh@example.com",
    "phone": "9876543210",
    "supportContact": "9876543211",
    "whatsAppNumber": "9876543210",
    "logoPath": "/uploads/logos/12345.jpg",
    "approvalStatusId": 1,
    "approvalStatusName": "Pending",
    "priorityId": 1,
    "priorityName": "Normal",
    "registrationDate": "2025-01-15T10:30:00",
    "emailVerified": true,
    "phoneVerified": true,
    "address": {
      "building": "Shop 5, Empire Plaza",
      "street": "MG Road",
      "area": "Andheri West",
      "cityName": "Mumbai",
      "stateName": "Maharashtra",
      "pincode": "400058"
    },
    "legalCompliance": {
      "fssaiNumber": "12345678901234",
      "fssaiExpiryDate": "2026-12-31",
      "fssaiCertificatePath": "/uploads/fssai/12345.pdf",
      "gstApplicable": true,
      "gstNumber": "27XXXXX1234X1Z5",
      "panNumber": "ABCDE1234F"
    },
    "bankDetails": {
      "accountNumber": "1234567890",
      "accountHolderName": "Rajesh Kumar",
      "ifscCode": "HDFC0001234",
      "upiId": "rajesh@paytm"
    },
    "documents": [
      {
        "mediaId": 1001,
        "documentTypeId": 1,
        "fileName": "fssai_certificate.pdf",
        "filePath": "/uploads/documents/fssai_certificate.pdf",
        "extension": ".pdf",
        "uploadedAt": "2025-01-15T10:35:00"
      }
    ],
    "photos": [
      {
        "mediaId": 2001,
        "fileName": "kitchen_photo_1.jpg",
        "filePath": "/uploads/photos/kitchen_photo_1.jpg",
        "extension": ".jpg",
        "uploadedAt": "2025-01-15T10:40:00"
      }
    ]
  }
}
```

### 3. **Approve Partner Request**

```http
POST /api/admin/partners/{partnerId}/approve
```

**Request Body:**

```json
{
  "remarks": "All documents verified. Approved.",
  "sendNotification": true
}
```

**Response:**

```json
{
  "success": true,
  "message": "Partner request approved successfully",
  "data": {
    "success": true,
    "message": "Partner request approved successfully",
    "newStatusId": 2,
    "newStatusName": "Approved"
  }
}
```

**Validation:**

- ✅ Partner must be in `Pending` (1) or `UnderReview` (4) status
- ❌ Cannot approve already `Approved` partners
- ❌ Cannot approve `Rejected` partners

### 4. **Reject Partner Request**

```http
POST /api/admin/partners/{partnerId}/reject
```

**Request Body:**

```json
{
  "rejectionReason": "Incomplete FSSAI documentation. Please resubmit with valid certificate.",
  "sendNotification": true
}
```

**Response:**

```json
{
  "success": true,
  "message": "Partner request rejected",
  "data": {
    "success": true,
    "message": "Partner request rejected",
    "newStatusId": 3,
    "newStatusName": "Rejected"
  }
}
```

**Validation:**

- ✅ Rejection reason is **MANDATORY**
- ✅ Can reject partners in `Pending`, `UnderReview`, or `Info_Requested` status
- ❌ Cannot reject already `Approved` partners

### 5. **Update Priority**

```http
PUT /api/admin/partners/{partnerId}/priority
```

**Request Body:**

```json
{
  "priorityId": 2
}
```

**Priority Values:**

- `0` = Low
- `1` = Normal (default)
- `2` = High
- `3` = Urgent

### 6. **Get Enum Options (for UI dropdowns)**

```http
GET /api/admin/partners/enums/approval-statuses
```

**Response:**

```json
{
  "success": true,
  "data": [
    { "id": 1, "name": "Pending" },
    { "id": 2, "name": "Approved" },
    { "id": 3, "name": "Rejected" },
    { "id": 4, "name": "Under Review" },
    { "id": 5, "name": "More Info Requested" }
  ]
}
```

```http
GET /api/admin/partners/enums/priorities
```

**Response:**

```json
{
  "success": true,
  "data": [
    { "id": 0, "name": "Low" },
    { "id": 1, "name": "Normal" },
    { "id": 2, "name": "High" },
    { "id": 3, "name": "Urgent" }
  ]
}
```

---

## Database Schema

### Column Types (CORRECTED)

```sql
-- t_sys_catering_owner table
c_approval_status INT NOT NULL DEFAULT 1,    -- ApprovalStatus enum
c_priority INT NOT NULL DEFAULT 1,           -- PriorityStatus enum
c_approved_date DATETIME NULL,
c_approved_by BIGINT NULL,
c_rejection_reason NVARCHAR(1000) NULL
```

### Enum Mappings

**ApprovalStatus:**

| Value | Enum Name | Display Name | Description |
|-------|-----------|--------------|-------------|
| 1 | `Pending` | "Pending" | Initial state after registration |
| 2 | `Approved` | "Approved" | Admin approved the request |
| 3 | `Rejected` | "Rejected" | Admin rejected the request |
| 4 | `UnderReview` | "Under Review" | Admin is actively reviewing |
| 5 | `Info_Requested` | "More Info Requested" | Admin needs more information |

**PriorityStatus:**

| Value | Enum Name | Display Name | Description |
|-------|-----------|--------------|-------------|
| 0 | `Low` | "Low" | Low priority request |
| 1 | `Normal` | "Normal" | Standard priority (default) |
| 2 | `High` | "High" | High priority request |
| 3 | `Urgent` | "Urgent" | Requires immediate attention |

### Migration Script

Run the migration script to fix incorrect VARCHAR columns:

```bash
sqlcmd -S localhost -d CateringEcommerce -i Database/Admin_PartnerApproval_EnumFix_Migration.sql
```

Or execute in SQL Server Management Studio:

```sql
-- See: Database/Admin_PartnerApproval_EnumFix_Migration.sql
```

---

## Why NOT to Reuse OwnerProfile.cs

### ⚠️ Critical Distinction

| Aspect | AdminPartnerApprovalRepository | OwnerProfile |
|--------|-------------------------------|--------------|
| **Purpose** | Review registration data | Manage operational profile |
| **When** | BEFORE approval | AFTER approval |
| **User** | Admin | Partner |
| **Access** | Read-only registration data | Full CRUD on profile |
| **Data** | Static registration snapshot | Dynamic operational data |
| **Updates** | Admin cannot edit partner data | Partner can update their own data |

### Example Scenario

**Step 1: Partner Registers**

- Partner fills registration form
- Data goes into: `t_sys_catering_owner`, `t_sys_catering_owner_addresses`, etc.
- Status: `c_approval_status = 1` (Pending)

**Step 2: Admin Reviews (Uses AdminPartnerApprovalRepository)**

- Admin sees ALL registration data using `GetPartnerRequestDetail()`
- Admin reviews documents, photos, legal compliance
- Admin either approves or rejects

**Step 3A: If APPROVED**

- Status: `c_approval_status = 2` (Approved)
- Partner account becomes active
- Partner can now login and use `OwnerProfile.cs` to manage their profile

**Step 3B: If REJECTED**

- Status: `c_approval_status = 3` (Rejected)
- Partner remains inactive
- Rejection reason is stored

### Why Separation Matters

1. **Security**: Admin should not have write access to partner operational data
2. **Audit**: Registration data remains unchanged for audit trail
3. **Permissions**: Different user roles require different data access
4. **Maintainability**: Clear separation of concerns
5. **Scalability**: Can evolve independently

### Code Organization

```
CateringEcommerce.BAL/
├── Base/
│   ├── Admin/
│   │   └── AdminPartnerApprovalRepository.cs    ← Admin reviews BEFORE approval
│   └── Owner/
│       └── Dashboard/
│           └── OwnerProfile.cs                    ← Partner manages AFTER approval
```

**Never confuse these two!**

---

## Usage Examples

### Frontend: Display Partner Request List

```javascript
// Fetch pending partner requests
const response = await fetch('/api/admin/partners/pending?pageNumber=1&pageSize=20');
const data = await response.json();

// Display in table
data.data.requests.forEach(request => {
  console.log({
    id: request.ownerId,
    business: request.businessName,
    owner: request.ownerName,
    status: request.approvalStatusName,  // "Pending"
    priority: request.priorityName,      // "Normal"

    // Use ID for styling
    statusClass: `status-${request.approvalStatusId}`,  // "status-1"
    priorityClass: `priority-${request.priorityId}`     // "priority-1"
  });
});
```

### Frontend: Approve Partner

```javascript
const approvePartner = async (partnerId) => {
  const response = await fetch(`/api/admin/partners/${partnerId}/approve`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      remarks: 'All documents verified',
      sendNotification: true
    })
  });

  const response = await response.json();

  if (response.result) {
    alert(`Partner approved! New status: ${result.data.newStatusName}`);
  } else {
    alert(`Error: ${result.message}`);
  }
};
```

### Frontend: Filter by Status and Priority

```javascript
// Get dropdown options
const statusesResponse = await fetch('/api/admin/partners/enums/approval-statuses');
const statuses = await statusesResponse.json();

// Populate dropdown
statuses.data.forEach(status => {
  const option = `<option value="${status.id}">${status.name}</option>`;
  document.getElementById('statusFilter').innerHTML += option;
});

// Filter requests
const filterRequests = async (statusId, priorityId) => {
  const params = new URLSearchParams({
    approvalStatusId: statusId,
    priorityId: priorityId,
    pageNumber: 1,
    pageSize: 20
  });

  const response = await fetch(`/api/admin/partners/pending?${params}`);
  const data = await response.json();

  // Display filtered results
  displayResults(data.data.requests);
};
```

---

## Testing Guide

### 1. **Test Enum Conversion**

```csharp
// Test EnumHelper
[Test]
public void TestEnumHelper_ConvertIntToDisplayName()
{
    var displayName = EnumHelper.GetDisplayNameFromInt<ApprovalStatus>(1);
    Assert.AreEqual("Pending", displayName);
}

[Test]
public void TestEnumHelper_ConvertEnumToInt()
{
    var intValue = EnumHelper.GetIntValue(ApprovalStatus.Approved);
    Assert.AreEqual(2, intValue);
}
```

### 2. **Test Approval Validation**

```csharp
[Test]
public void TestApproval_CannotApproveAlreadyApprovedPartner()
{
    var repository = new AdminPartnerApprovalRepository(connStr);

    // First approval should succeed
    var result1 = repository.ApprovePartnerRequest(12345, 1, null);
    Assert.IsTrue(result1.Success);

    // Second approval should fail
    var result2 = repository.ApprovePartnerRequest(12345, 1, null);
    Assert.IsFalse(result2.Success);
    Assert.That(result2.Message, Does.Contain("Cannot approve"));
}
```

### 3. **Test Rejection Validation**

```csharp
[Test]
public void TestRejection_RequiresReason()
{
    var repository = new AdminPartnerApprovalRepository(connStr);

    // Rejection without reason should fail
    var result = repository.RejectPartnerRequest(12345, 1, "");
    Assert.IsFalse(result.Success);
    Assert.That(result.Message, Does.Contain("required"));
}
```

### 4. **Integration Test: Full Approval Flow**

```csharp
[Test]
public void TestFullApprovalFlow()
{
    var repository = new AdminPartnerApprovalRepository(connStr);
    long partnerId = 12345;
    long adminId = 1;

    // 1. Partner should be in Pending status
    var detail = repository.GetPartnerRequestDetail(partnerId);
    Assert.AreEqual(1, detail.ApprovalStatusId);
    Assert.AreEqual("Pending", detail.ApprovalStatusName);

    // 2. Approve partner
    var result = repository.ApprovePartnerRequest(partnerId, adminId, "Verified");
    Assert.IsTrue(result.Success);
    Assert.AreEqual(2, result.NewStatusId);
    Assert.AreEqual("Approved", result.NewStatusName);

    // 3. Verify status changed
    var updatedDetail = repository.GetPartnerRequestDetail(partnerId);
    Assert.AreEqual(2, updatedDetail.ApprovalStatusId);
    Assert.IsNotNull(updatedDetail.ApprovedDate);
    Assert.AreEqual(adminId, updatedDetail.ApprovedBy);
}
```

### 5. **API Endpoint Tests (Postman)**

#### Test 1: Get Pending Requests

```http
GET http://localhost:5000/api/admin/partners/pending
Authorization: Bearer {admin_token}
```

**Expected:** 200 OK with list of pending partners

#### Test 2: Approve Partner

```http
POST http://localhost:5000/api/admin/partners/12345/approve
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "remarks": "All documents verified",
  "sendNotification": true
}
```

**Expected:** 200 OK with success message

#### Test 3: Reject Partner

```http
POST http://localhost:5000/api/admin/partners/12346/reject
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "rejectionReason": "Invalid FSSAI certificate",
  "sendNotification": true
}
```

**Expected:** 200 OK with success message

#### Test 4: Duplicate Approval (Should Fail)

```http
POST http://localhost:5000/api/admin/partners/12345/approve
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "remarks": "Second approval attempt"
}
```

**Expected:** 400 Bad Request with error message

---

## 🎯 Summary

### What We Built

1. ✅ **Enum-based design** with INT database columns
2. ✅ **Clean separation** between registration review and partner operations
3. ✅ **Strict validation** for approval/rejection actions
4. ✅ **Type-safe** enum conversions using EnumHelper
5. ✅ **Clear DTOs** that return both ID and display name
6. ✅ **RESTful APIs** with comprehensive documentation
7. ✅ **Database migration** to fix column types

### Key Takeaways

- **Use enums**: Type-safe, maintainable, efficient
- **Separate concerns**: AdminPartnerApprovalRepository ≠ OwnerProfile
- **Validate strictly**: Prevent invalid state transitions
- **Return enum names**: Frontend doesn't need to know enum mappings
- **Read-only review**: Admin reviews but doesn't edit partner data

### Next Steps

1. Run database migration: `Admin_PartnerApproval_EnumFix_Migration.sql`
2. Test API endpoints using Postman
3. Update frontend to use new enum-based endpoints
4. Implement notification sending (email/SMS)
5. Add audit logging for all approval/rejection actions

---

**📝 Document Version**: 1.0
**🗓️ Last Updated**: 2025-01-25
**👨‍💻 Author**: Senior Backend Engineer & Business Analyst
