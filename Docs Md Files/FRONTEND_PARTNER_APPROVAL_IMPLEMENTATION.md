# Frontend UI/UX - Partner Approval Implementation Guide

## 📋 Overview

This document outlines all **Frontend UI/UX changes** for the Admin Partner Approval & Rejection flow, working with the new enum-based backend.

---

## ✅ What Was Implemented

### 1. **New API Service** (Enum-based)
**File**: `src/services/partnerApprovalApi.js`

- Works with INT-based enum values (not strings)
- Exports `ApprovalStatus` and `PriorityStatus` enums for frontend use
- Clean API methods for all partner approval operations

**Key Methods**:
```javascript
// Get pending requests with enum-based filters
partnerApprovalApi.getPendingRequests({
  approvalStatusId: 1,  // INT enum value
  priorityId: 2,        // INT enum value
  ...
});

// Get partner detail
partnerApprovalApi.getPartnerDetail(partnerId);

// Approve partner
partnerApprovalApi.approvePartner(partnerId, { remarks: '...' });

// Reject partner
partnerApprovalApi.rejectPartner(partnerId, { rejectionReason: '...' });

// Update priority
partnerApprovalApi.updatePriority(partnerId, priorityId);

// Get enum options for dropdowns
partnerApprovalApi.getApprovalStatuses();
partnerApprovalApi.getPriorities();
```

---

### 2. **Updated PartnerStatusBadge Component** (Enum-based)
**File**: `src/components/admin/partner-requests/PartnerStatusBadge.jsx`

**Changes**:
- Now accepts `statusId` (INT) and `statusName` (string) props
- Maps enum IDs to visual styling
- Backwards compatible with both enum IDs and display names

**Usage**:
```jsx
<PartnerStatusBadge
  statusId={request.approvalStatusId}    // INT: 1, 2, 3, 4, 5
  statusName={request.approvalStatusName} // Display: "Pending", "Approved"...
/>
```

**Supported Status IDs**:
- `1` = Pending (Orange)
- `2` = Approved (Green)
- `3` = Rejected (Red)
- `4` = Under Review (Blue)
- `5` = Info Requested (Purple)

---

### 3. **Updated PartnerRequestsTable Component**
**File**: `src/components/admin/partner-requests/PartnerRequestsTable.jsx`

**Changes**:
- Updated status badge to use `approvalStatusId` and `approvalStatusName`
- Table now expects enum-based data from backend

**Data Structure Expected**:
```javascript
{
  ownerId: 12345,
  businessName: "Mumbai Caterers",
  ownerName: "Rajesh Kumar",
  phone: "9876543210",
  email: "rajesh@example.com",
  city: "Mumbai",
  state: "Maharashtra",
  approvalStatusId: 1,           // ✅ INT enum value
  approvalStatusName: "Pending",  // ✅ Display name
  priorityId: 1,                  // ✅ INT enum value
  priorityName: "Normal",         // ✅ Display name
  registrationDate: "2025-01-15T10:30:00",
  documentCount: 5
}
```

---

### 4. **Updated AdminPartnerRequests Page** (MAJOR REWRITE)
**File**: `src/pages/admin/AdminPartnerRequests.jsx`

**Key Changes**:

#### Filters Use Enum IDs
```javascript
const [filters, setFilters] = useState({
  approvalStatusId: null,  // ✅ INT (was string "PENDING")
  priorityId: null,        // ✅ INT (was string "NORMAL")
  cityId: null,
  fromDate: null,
  toDate: null,
  searchTerm: '',
  pageNumber: 1,
  pageSize: 20,
  sortBy: 'c_createddate',
  sortOrder: 'DESC'
});
```

#### Quick Filter Cards (Enum-based)
```javascript
const quickFilters = [
  {
    label: 'All',
    value: null,
    count: stats.totalRequests
  },
  {
    label: 'Pending',
    value: ApprovalStatus.PENDING,  // ✅ Uses enum (1)
    count: stats.pendingCount
  },
  {
    label: 'Under Review',
    value: ApprovalStatus.UNDER_REVIEW,  // ✅ Uses enum (4)
    count: stats.underReviewCount
  },
  {
    label: 'Info Requested',
    value: ApprovalStatus.INFO_REQUESTED,  // ✅ Uses enum (5)
    count: stats.infoRequestedCount
  }
];
```

#### API Integration
```javascript
// ✅ Uses new enum-based API
const fetchPartnerRequests = async () => {
  const result = await partnerApprovalApi.getPendingRequests(filters);

  if (result.success) {
    setRequests(result.data.requests || []);
    setStats(result.data.stats || {});
    setPagination({ ...result.data });
  }
};
```

---

## 🎨 UI/UX Features

### 1. **Stats Dashboard Cards**
- 4 quick filter cards showing counts
- Click to filter by status
- Active state highlighting
- Color-coded for each status type

### 2. **Advanced Filters Panel**
- Collapsible filter panel
- Status dropdown (enum-based)
- Priority dropdown (enum-based)
- City selection
- Date range picker
- Real-time filtering

### 3. **Search Bar**
- Full-text search across:
  - Business name
  - Owner name
  - Phone number
  - Email address
- Debounced search (automatic)

### 4. **Partner Requests Table**
Features:
- Responsive design
- Logo/avatar display
- Document & photo counts
- Status badges with icons
- Location information
- Submission date
- "View Details" button
- Pagination controls

### 5. **Partner Detail Drawer**
- Slide-out panel from right
- Complete registration information
- Structured sections:
  - Business details
  - Owner contact info
  - Address
  - Legal compliance (FSSAI, GST, PAN)
  - Bank account details
  - Service operations
  - Documents & photos
- Action buttons (Approve/Reject)
- Status display

### 6. **Action Modals**
- Approve confirmation modal
- Reject modal with reason input (mandatory)
- Request info modal
- Success/error notifications

---

## 📊 Data Flow Diagram

```
┌─────────────────────────────────────────────────┐
│ Admin Partner Requests Page                     │
├─────────────────────────────────────────────────┤
│                                                 │
│  ┌──────────────────────────────────────────┐  │
│  │ Stats Cards (Quick Filters)              │  │
│  │ - All, Pending, Under Review, Info Req'd│  │
│  │ - Uses ApprovalStatus enum IDs          │  │
│  └──────────────────────────────────────────┘  │
│                                                 │
│  ┌──────────────────────────────────────────┐  │
│  │ Advanced Filters Panel                   │  │
│  │ - Status Dropdown (enum IDs)             │  │
│  │ - Priority Dropdown (enum IDs)           │  │
│  │ - City, Date Range, etc.                 │  │
│  └──────────────────────────────────────────┘  │
│                                                 │
│  ┌──────────────────────────────────────────┐  │
│  │ Search Bar                               │  │
│  │ - Full-text search                       │  │
│  └──────────────────────────────────────────┘  │
│                                                 │
│  ┌──────────────────────────────────────────┐  │
│  │ Partner Requests Table                   │  │
│  │ - Shows list with enum-based statuses   │  │
│  │ - Click "View Details" opens drawer     │  │
│  └──────────────────────────────────────────┘  │
│                                                 │
│  ┌──────────────────────────────────────────┐  │
│  │ Pagination Controls                      │  │
│  └──────────────────────────────────────────┘  │
│                                                 │
└─────────────────────────────────────────────────┘
                      │
                      │ Click "View Details"
                      ▼
┌─────────────────────────────────────────────────┐
│ Partner Detail Drawer (Slide-out)              │
├─────────────────────────────────────────────────┤
│                                                 │
│  ┌──────────────────────────────────────────┐  │
│  │ Header                                   │  │
│  │ - Business Name                          │  │
│  │ - Request ID                             │  │
│  └──────────────────────────────────────────┘  │
│                                                 │
│  ┌──────────────────────────────────────────┐  │
│  │ Status & Actions                         │  │
│  │ - Current Status Badge                   │  │
│  │ - [Approve] [Reject] [Request Info]      │  │
│  └──────────────────────────────────────────┘  │
│                                                 │
│  ┌──────────────────────────────────────────┐  │
│  │ Business Information Section             │  │
│  └──────────────────────────────────────────┘  │
│                                                 │
│  ┌──────────────────────────────────────────┐  │
│  │ Owner Contact Section                    │  │
│  └──────────────────────────────────────────┘  │
│                                                 │
│  ┌──────────────────────────────────────────┐  │
│  │ Address Section                          │  │
│  └──────────────────────────────────────────┘  │
│                                                 │
│  ┌──────────────────────────────────────────┐  │
│  │ Legal Compliance Section                 │  │
│  │ - FSSAI, GST, PAN                        │  │
│  └──────────────────────────────────────────┘  │
│                                                 │
│  ┌──────────────────────────────────────────┐  │
│  │ Bank Details Section                     │  │
│  └──────────────────────────────────────────┘  │
│                                                 │
│  ┌──────────────────────────────────────────┐  │
│  │ Documents & Photos Grid                  │  │
│  │ - View/Download documents                │  │
│  └──────────────────────────────────────────┘  │
│                                                 │
└─────────────────────────────────────────────────┘
                      │
                      │ Click "Approve" or "Reject"
                      ▼
┌─────────────────────────────────────────────────┐
│ Action Modal (Approve/Reject)                   │
├─────────────────────────────────────────────────┤
│                                                 │
│  ┌──────────────────────────────────────────┐  │
│  │ Approve Modal                            │  │
│  │ - Optional remarks                       │  │
│  │ - Send notification checkbox             │  │
│  │ - [Confirm] [Cancel]                     │  │
│  └──────────────────────────────────────────┘  │
│                  OR                             │
│  ┌──────────────────────────────────────────┐  │
│  │ Reject Modal                             │  │
│  │ - Rejection reason (MANDATORY)           │  │
│  │ - Send notification checkbox             │  │
│  │ - [Confirm] [Cancel]                     │  │
│  └──────────────────────────────────────────┘  │
│                                                 │
└─────────────────────────────────────────────────┘
                      │
                      │ Submit action
                      ▼
┌─────────────────────────────────────────────────┐
│ API Call                                        │
│ - partnerApprovalApi.approvePartner()           │
│ - partnerApprovalApi.rejectPartner()            │
└─────────────────────────────────────────────────┘
                      │
                      │ Success/Error
                      ▼
┌─────────────────────────────────────────────────┐
│ Toast Notification                              │
│ - Success: "Partner approved successfully"      │
│ - Error: "Failed to approve partner"            │
└─────────────────────────────────────────────────┘
                      │
                      │ On success
                      ▼
┌─────────────────────────────────────────────────┐
│ Refresh List & Close Drawer                    │
└─────────────────────────────────────────────────┘
```

---

## 🔧 Configuration & Setup

### 1. Environment Variables
```env
VITE_API_BASE_URL=https://localhost:44368
```

### 2. Required Dependencies
Already installed in your project:
- React
- React Router
- lucide-react (icons)
- react-hot-toast (notifications)

### 3. API Endpoint Configuration
The API base URL is configured in:
- `src/services/partnerApprovalApi.js`

---

## 🧪 Testing the UI

### Test Case 1: View Pending Requests
1. Navigate to `/admin/partner-requests`
2. Should see quick filter cards with counts
3. Default view shows all pending requests
4. Click "Pending" card → should filter to show only pending

### Test Case 2: Search Functionality
1. Type in search bar: "Mumbai"
2. Table should filter results in real-time
3. Clear search → should show all results again

### Test Case 3: Advanced Filters
1. Click "Filters" button
2. Select status: "Under Review"
3. Select priority: "High"
4. Select date range
5. Apply filters
6. Results should update accordingly

### Test Case 4: View Partner Details
1. Click "View Details" on any request
2. Drawer should slide in from right
3. All sections should display data correctly
4. Documents and photos should be visible

### Test Case 5: Approve Partner
1. Open detail drawer for a PENDING request
2. Click "Approve" button
3. Approve modal should appear
4. Add optional remarks
5. Click "Confirm"
6. Should see success toast
7. Drawer should close
8. List should refresh
9. Status should change to "Approved"

### Test Case 6: Reject Partner
1. Open detail drawer for a PENDING request
2. Click "Reject" button
3. Reject modal should appear
4. Try to submit WITHOUT reason → should show error
5. Enter rejection reason
6. Click "Confirm"
7. Should see success toast
8. Status should change to "Rejected"

### Test Case 7: Pagination
1. If more than 20 results:
2. Pagination controls should appear
3. Click "Next" → should load next page
4. Click page number → should jump to that page
5. Click "Previous" → should go back

---

## 🎯 Key UI/UX Improvements

### 1. **Enum-Based Design**
- ✅ Uses INT values internally
- ✅ Displays user-friendly names
- ✅ Type-safe filtering
- ✅ Consistent with backend

### 2. **Performance**
- ✅ Lazy loading for large lists
- ✅ Optimized re-renders
- ✅ Debounced search
- ✅ Pagination for large datasets

### 3. **User Experience**
- ✅ Clear visual hierarchy
- ✅ Color-coded statuses
- ✅ Responsive design
- ✅ Loading states
- ✅ Empty states
- ✅ Error handling
- ✅ Toast notifications

### 4. **Accessibility**
- ✅ Keyboard navigation
- ✅ Proper ARIA labels
- ✅ Screen reader support
- ✅ Focus management

---

## 📝 Code Structure Summary

```
src/
├── services/
│   └── partnerApprovalApi.js          ✅ NEW - Enum-based API service
│
├── components/
│   └── admin/
│       └── partner-requests/
│           ├── PartnerStatusBadge.jsx    ✅ UPDATED - Enum support
│           ├── PartnerRequestsTable.jsx  ✅ UPDATED - Enum data
│           ├── PartnerDetailDrawer.jsx   (Existing, needs enum update)
│           ├── PartnerActionModal.jsx    (Existing, needs enum update)
│           └── PartnerFilters.jsx        (Existing, needs enum update)
│
└── pages/
    └── admin/
        └── AdminPartnerRequests.jsx      ✅ UPDATED - Complete rewrite
```

---

## 🚀 Next Steps to Complete

### Still Need to Update:
1. **PartnerDetailDrawer.jsx** - Update to use enum-based status
2. **PartnerActionModal.jsx** - Update approve/reject logic to use new API
3. **PartnerFilters.jsx** - Add enum-based dropdowns for status and priority

### Recommended Enhancements:
1. Add priority badge component
2. Add bulk approval functionality
3. Add export to Excel/CSV
4. Add advanced document preview
5. Add action history timeline
6. Add email template preview

---

## 🎨 Visual Design Tokens

### Status Colors
```css
/* Pending */
background: #FFF7ED (orange-50)
text: #9A3412 (orange-800)
border: #FED7AA (orange-200)

/* Approved */
background: #F0FDF4 (green-50)
text: #166534 (green-800)
border: #BBF7D0 (green-200)

/* Rejected */
background: #FEF2F2 (red-50)
text: #991B1B (red-800)
border: #FECACA (red-200)

/* Under Review */
background: #EFF6FF (blue-50)
text: #1E40AF (blue-800)
border: #BFDBFE (blue-200)

/* Info Requested */
background: #FAF5FF (purple-50)
text: #6B21A8 (purple-800)
border: #E9D5FF (purple-200)
```

---

## 📚 Related Documentation

- [Backend Implementation Guide](./ADMIN_PARTNER_APPROVAL_IMPLEMENTATION.md)
- [API Documentation](./ADMIN_PARTNER_APPROVAL_IMPLEMENTATION.md#api-endpoints)
- [Database Schema](./ADMIN_PARTNER_APPROVAL_IMPLEMENTATION.md#database-schema)
- [Enum Reference](./ADMIN_PARTNER_APPROVAL_IMPLEMENTATION.md#enum-mappings)

---

## ✅ Summary

### What's Working:
- ✅ Enum-based API service
- ✅ Status badge with enum support
- ✅ Requests table with enum data
- ✅ Main page with enum-based filtering
- ✅ Quick filter cards
- ✅ Search functionality
- ✅ Pagination

### What Needs Update:
- ⏳ Detail drawer (status check logic)
- ⏳ Action modal (approve/reject API calls)
- ⏳ Filters panel (enum dropdowns)

---

**📝 Document Version**: 1.0
**🗓️ Last Updated**: 2026-01-25
**👨‍💻 Author**: Senior Frontend Engineer & UX Designer
