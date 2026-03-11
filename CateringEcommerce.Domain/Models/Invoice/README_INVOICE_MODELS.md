# Invoice Domain Models Documentation

## Overview
This directory contains all domain models, DTOs (Data Transfer Objects), and interfaces for the Invoice & Payment system.

**Created:** 2026-02-20
**Version:** 1.0.0
**Payment Model:** Model A (Installment) ONLY - 40% / 35% / 25%

---

## 📁 File Structure

```
CateringEcommerce.Domain/
├── Enums/
│   └── InvoiceEnums.cs          ✅ All invoice-related enumerations
├── Models/
│   └── Invoice/
│       ├── InvoiceModels.cs     ✅ All invoice DTOs
│       └── README_INVOICE_MODELS.md (this file)
└── Interfaces/
    └── Invoice/
        └── IInvoiceRepository.cs ✅ Invoice repository interface
```

---

## 🎯 Enumerations (InvoiceEnums.cs)

### 1. **InvoiceType** - 3 Values
Defines the type of invoice (ONLY Model A - Installment)

| Enum Value | Int | Description | When Generated | Nature |
|------------|-----|-------------|----------------|--------|
| `BOOKING` | 1 | 40% Advance Payment | Partner approves order | Proforma Invoice |
| `PRE_EVENT` | 2 | 35% Pre-Event Payment | Guest lock date reached | Tax Invoice |
| `FINAL` | 3 | 25% + Extras After Event | Event completed | Tax Invoice |

**Note:** `FULL_PAYMENT` (Model B) was removed - only installment model supported.

---

### 2. **InvoiceStatus** - 7 Values
Tracks the lifecycle of an invoice

| Enum Value | Int | Description | Next Status |
|------------|-----|-------------|-------------|
| `DRAFT` | 0 | Being created, not finalized | UNPAID |
| `UNPAID` | 1 | Generated, awaiting payment | PAID, OVERDUE, EXPIRED |
| `PARTIALLY_PAID` | 2 | Partial payment received | PAID |
| `PAID` | 3 | Fully paid | - |
| `OVERDUE` | 4 | Past due date | PAID, EXPIRED |
| `EXPIRED` | 5 | Validity period expired | CANCELLED |
| `CANCELLED` | 6 | Cancelled (final state) | - |

**Status Flow:**
```
DRAFT → UNPAID → PAID ✓
             ↓
          OVERDUE → EXPIRED → CANCELLED
```

---

### 3. **PaymentStageType** - 3 Values
The three payment stages (percentages LOCKED)

| Enum Value | Int | Percentage | When Due | Cumulative % |
|------------|-----|------------|----------|--------------|
| `BOOKING` | 1 | 40.00% | Within 7 days | 40% |
| `PRE_EVENT` | 2 | 35.00% | Before event (3 days) | 75% |
| `FINAL` | 3 | 25.00% | After event (7 days) | 100% |

**Critical Rule:** Event cannot start unless 75% paid (BOOKING + PRE_EVENT)

---

### 4. **InvoiceLineItemType** - 9 Values
Categories of charges on an invoice

| Type | Description | Appears In |
|------|-------------|------------|
| `PACKAGE` | Catering packages | All invoices |
| `FOOD_ITEM` | Individual food items | All invoices |
| `DECORATION` | Decoration services | All invoices |
| `EXTRA_GUEST` | Guests beyond locked count | FINAL only |
| `ADDON` | Items added after menu lock | FINAL only |
| `OVERTIME` | Staff overtime charges | FINAL only |
| `DELIVERY` | Delivery fees | All invoices |
| `STAFF` | Additional staff | All invoices |
| `OTHER` | Miscellaneous | All invoices |

---

### 5. **InvoiceAuditAction** - 12 Values
Actions tracked in audit log

| Action | When Logged |
|--------|-------------|
| `GENERATED` | Invoice created |
| `VIEWED` | Invoice viewed by user |
| `DOWNLOADED` | PDF downloaded |
| `PAID` | Payment received |
| `PAYMENT_FAILED` | Payment attempt failed |
| `CANCELLED` | Invoice cancelled |
| `REGENERATED` | New version created |
| `STATUS_CHANGED` | Admin changed status |
| `EMAIL_SENT` | Invoice emailed |
| `SMS_SENT` | Invoice SMS sent |
| `REMINDER_SENT` | Payment reminder sent |
| `MARKED_OVERDUE` | Auto-marked overdue |

---

### 6. **PaymentScheduleStatus** - 4 Values
Status of payment stage in schedule

| Status | Description |
|--------|-------------|
| `PENDING` | Awaiting payment |
| `PAID` | Payment received |
| `OVERDUE` | Past due date |
| `CANCELLED` | Stage cancelled |

---

### 7. **PaymentTriggerEvent** - 4 Values
Events that trigger invoice generation

| Event | Triggers | Description |
|-------|----------|-------------|
| `ORDER_APPROVED` | BOOKING invoice | Partner accepts order |
| `GUEST_LOCK_DATE` | PRE_EVENT invoice | 5 days before event |
| `EVENT_COMPLETED` | FINAL invoice | Supervisor completes event |
| `MANUAL_TRIGGER` | Any invoice | Admin manual generation |

---

### 8. **InvoiceUserType** - 5 Values
Who performed an action

| Type | Description |
|------|-------------|
| `USER` | Customer |
| `ADMIN` | Admin user |
| `OWNER` | Catering partner |
| `SUPERVISOR` | Event supervisor |
| `SYSTEM` | Automated action |

---

## 📦 Models (InvoiceModels.cs)

### Core DTOs

#### 1. **InvoiceDto** (Response Model)
Complete invoice details returned to clients.

**Key Properties:**
- **Identification:** InvoiceId, InvoiceNumber, OrderId
- **Financial:** Subtotal, CGST, SGST, TotalAmount, BalanceDue
- **GST Compliance:** CompanyGstin, CustomerGstin, PlaceOfSupply, SacCode
- **Payment:** RazorpayOrderId, RazorpayPaymentId, PaymentDate
- **Collections:** LineItems, OrderSummary, PaymentHistory
- **Computed:** IsOverdue, DaysUntilDue, IsPaymentPending

**Usage:**
```csharp
var invoice = await _invoiceRepo.GetInvoiceByIdAsync(123);
if (invoice.IsOverdue) {
    // Send reminder
}
```

---

#### 2. **InvoiceLineItemDto**
Individual line item on an invoice.

**Key Properties:**
- ItemType, Description, Quantity, UnitPrice
- Tax breakdown (CGST, SGST per item)
- Discount tracking
- Sequence for display order

**Example:**
```
Package: Wedding Deluxe - 100 plates @ ₹500 = ₹50,000
+ CGST (9%): ₹4,500
+ SGST (9%): ₹4,500
= Total: ₹59,000
```

---

#### 3. **CreateInvoiceDto** (Request Model)
Request to generate a new invoice.

**Required Fields:**
- OrderId ✅
- InvoiceType ✅
- PaymentStageType ✅
- Subtotal ✅
- LineItems ✅

**Validation:**
- Subtotal > 0
- At least 1 line item
- Valid invoice type (1, 2, or 3 only)

---

#### 4. **PaymentScheduleDto**
Complete payment schedule for an order.

**Shows:**
- All 3 stages with amounts and status
- Total paid vs pending
- Payment progress percentage
- Current and next stages

**Computed Properties:**
- `IsFullyPaid` - All stages paid
- `CanStartEvent` - 75%+ paid (40% + 35%)

**Example:**
```csharp
var schedule = await _invoiceRepo.GetPaymentScheduleAsync(orderId);
if (!schedule.CanStartEvent) {
    throw new Exception("Cannot start event - payment < 75%");
}
```

---

#### 5. **PaymentStageDto**
Individual payment stage in the schedule.

**Key Properties:**
- StageType, Percentage, Amount
- DueDate, Status
- Associated InvoiceId
- Reminder tracking

**Computed Properties:**
- `IsInvoiceGenerated`
- `IsPaid`, `IsPending`, `IsOverdue`
- `DaysUntilDue`

---

### Request DTOs

#### 6. **InvoiceGenerationRequestDto**
Auto-generate invoice based on order.

**Use Case:** Background job or API call to generate invoice.

**Optional Fields for FINAL:**
- ExtraGuestCount
- ExtraGuestCharges
- AddonCharges
- OvertimeCharges

---

#### 7. **InvoicePdfRequestDto**
Request to generate PDF.

**Options:**
- RegeneratePdf (force regeneration)
- Watermark (custom text)
- IncludeCompanyLogo

---

#### 8. **InvoiceListRequestDto**
Pagination and filtering for invoice lists.

**Filters:**
- OrderId, UserId, OwnerId
- InvoiceType, Status
- Date range (StartDate, EndDate)
- Amount range (MinAmount, MaxAmount)
- Search term (invoice number, customer name)
- IsOverdue, IsPaid flags

**Sorting:**
- SortBy (InvoiceDate, TotalAmount, etc.)
- SortOrder (ASC/DESC)

---

#### 9. **UpdateInvoiceStatusDto**
Change invoice status (admin action).

**Fields:**
- InvoiceId ✅
- NewStatus ✅
- Remarks (optional)
- UpdatedBy (admin ID)

---

#### 10. **LinkPaymentToInvoiceDto**
Link Razorpay payment after success.

**Required Fields:**
- InvoiceId ✅
- RazorpayOrderId ✅
- RazorpayPaymentId ✅
- RazorpaySignature ✅ (for verification)
- AmountPaid ✅
- PaymentMethod ✅

**Critical:** Signature MUST be verified before calling this!

---

### Response DTOs

#### 11. **InvoiceListResponseDto**
Paginated invoice list response.

**Structure:**
```csharp
{
    Invoices: [ {...}, {...} ],
    TotalCount: 150,
    PageNumber: 1,
    PageSize: 20,
    TotalPages: 8,
    HasPreviousPage: false,
    HasNextPage: true
}
```

---

#### 12. **InvoiceSummaryDto**
Lightweight invoice for list views.

**Includes:**
- Invoice number, type, status
- Total amount, balance due
- Order number, customer name
- IsOverdue, DaysUntilDue

**Excludes:**
- Line items (performance optimization)
- Full order details

---

### Supporting DTOs

#### 13. **GstBreakdownDto**
GST tax calculation breakdown.

**Shows:**
- Taxable amount
- CGST % and amount
- SGST % and amount
- Total tax
- Place of supply
- SAC code

---

#### 14. **InvoiceOrderSummaryDto**
Order context for invoice.

**Includes:**
- Event details (date, time, location)
- Guest count (original, locked, final)
- Customer details
- Partner details (name, GSTIN, address)

---

#### 15. **InvoicePaymentHistoryDto**
Payment transaction history.

**Tracks:**
- Multiple payments per invoice
- Payment method
- Razorpay transaction IDs
- Status (success/failed)

---

#### 16. **InvoiceAuditLogDto**
Audit trail entry.

**Captures:**
- Who did what, when
- Status changes (old → new)
- Amount changes
- IP address, user agent
- Remarks

---

#### 17. **InvoiceStatisticsDto**
Dashboard metrics.

**Metrics:**
- Total invoices count
- Unpaid, Paid, Overdue counts
- Total amounts (invoice, paid, pending, overdue)
- Counts by stage (Booking, Pre-Event, Final)
- Average invoice amount
- Average payment time (days)
- Payment success rate (%)

**Dictionaries:**
- InvoicesByStatus
- RevenueByStage

---

## 🔌 Interface (IInvoiceRepository.cs)

### Method Categories

#### Invoice Generation (2 methods)
- `GenerateInvoiceAsync()` - Auto-generate based on order
- `CreateInvoiceAsync()` - Manual creation

#### Invoice Retrieval (8 methods)
- `GetInvoiceByIdAsync()`
- `GetInvoiceByNumberAsync()`
- `GetInvoicesByOrderIdAsync()`
- `GetInvoiceByOrderAndTypeAsync()`
- `GetInvoicesAsync()` - Paginated list
- `GetInvoicesByUserIdAsync()`
- `GetInvoicesByOwnerIdAsync()`

#### Invoice Update (5 methods)
- `UpdateInvoiceStatusAsync()`
- `LinkPaymentToInvoiceAsync()`
- `UpdateInvoicePdfPathAsync()`
- `RecalculateFinalInvoiceAsync()`
- `RegenerateInvoiceAsync()`

#### Payment Schedule (7 methods)
- `CreatePaymentScheduleAsync()`
- `GetPaymentScheduleAsync()`
- `GetPaymentStageAsync()`
- `UpdatePaymentStageStatusAsync()`
- `GetOrdersForAutoInvoiceGenerationAsync()`
- `UpdatePaymentReminderSentAsync()`

#### Audit (2 methods)
- `LogInvoiceAuditAsync()`
- `GetInvoiceAuditLogAsync()`

#### Statistics (3 methods)
- `GetInvoiceStatisticsAsync()`
- `GetOverdueInvoicesAsync()`
- `GetInvoicesDueSoonAsync()`

#### Validation (5 methods)
- `InvoiceExistsAsync()`
- `CanPayInvoiceAsync()`
- `CanGenerateNextStageInvoiceAsync()`
- `GetTotalPaidAmountAsync()`
- `GetPaymentProgressPercentageAsync()`

**Total:** 32 interface methods

---

## 🎨 Design Patterns Used

### 1. **DTO Pattern**
Separate models for request/response to decouple API from database.

**Benefits:**
- Clean API contracts
- Version control
- Validation separation

### 2. **Repository Pattern**
Interface-based data access.

**Benefits:**
- Testability (mock repository)
- Swappable implementations
- Business logic separation

### 3. **Enum Pattern**
Type-safe enumerations with display attributes.

**Benefits:**
- No magic strings/numbers
- IntelliSense support
- Display names for UI

### 4. **Computed Properties**
Calculated fields in DTOs.

**Benefits:**
- No redundant data storage
- Always up-to-date
- Encapsulated logic

---

## ✅ Validation Rules

### Invoice Creation
- ✅ Subtotal > 0
- ✅ At least 1 line item
- ✅ Invoice type must be 1, 2, or 3
- ✅ Payment percentage must be 40.00, 35.00, or 25.00
- ✅ Line item quantity > 0
- ✅ Due date in future (for new invoices)

### Status Transitions
- ✅ DRAFT → UNPAID only
- ✅ UNPAID → PAID, OVERDUE, EXPIRED
- ✅ OVERDUE → PAID, EXPIRED
- ✅ PAID is final (cannot change)
- ✅ CANCELLED is final

### Payment Linkage
- ✅ Razorpay signature verified
- ✅ Amount > 0
- ✅ Amount <= balance due
- ✅ Invoice status allows payment

---

## 🔐 Security Considerations

### Authorization
- Users can only access their own invoices
- Owners can view invoices for their orders (read-only)
- Admins have full access
- Supervisors can view assigned event invoices

### Data Privacy
- PII in separate fields (can be masked)
- Audit log tracks all access
- IP address logged for downloads

### Payment Security
- Razorpay signature MUST be verified
- No hardcoded amounts (always from database)
- Transaction IDs logged for reconciliation

---

## 📊 Database Mapping

### DTO ↔ Table Mapping

| DTO | Database Table |
|-----|----------------|
| `InvoiceDto` | `t_sys_invoice` |
| `InvoiceLineItemDto` | `t_sys_invoice_line_items` |
| `PaymentScheduleDto` | `t_sys_payment_schedule` |
| `InvoiceAuditLogDto` | `t_sys_invoice_audit_log` |

### Enum ↔ Database Mapping

| Enum | Database Column | Type |
|------|-----------------|------|
| `InvoiceType` | `c_invoice_type` | INT |
| `InvoiceStatus` | `c_status` | VARCHAR |
| `PaymentStageType` | `c_payment_stage_type` | VARCHAR |
| `InvoiceLineItemType` | `c_item_type` | VARCHAR |

---

## 🚀 Usage Examples

### Example 1: Generate Booking Invoice
```csharp
var request = new InvoiceGenerationRequestDto
{
    OrderId = 123,
    InvoiceType = InvoiceType.BOOKING,
    TriggeredBy = adminId,
    TriggeredByType = InvoiceUserType.SYSTEM
};

long invoiceId = await _invoiceRepo.GenerateInvoiceAsync(request);
```

### Example 2: Check Payment Progress
```csharp
var schedule = await _invoiceRepo.GetPaymentScheduleAsync(orderId);
if (schedule.PaymentProgressPercentage < 75)
{
    throw new Exception("Cannot start event - insufficient payment");
}
```

### Example 3: Link Razorpay Payment
```csharp
var paymentData = new LinkPaymentToInvoiceDto
{
    InvoiceId = invoiceId,
    RazorpayOrderId = "order_xyz123",
    RazorpayPaymentId = "pay_abc456",
    RazorpaySignature = "verified_signature",
    AmountPaid = 11800.00m,
    PaymentMethod = "UPI"
};

await _invoiceRepo.LinkPaymentToInvoiceAsync(paymentData);
```

### Example 4: Get Overdue Invoices
```csharp
var overdueInvoices = await _invoiceRepo.GetOverdueInvoicesAsync();
foreach (var invoice in overdueInvoices)
{
    await SendPaymentReminder(invoice.InvoiceId);
}
```

---

## 📈 Performance Considerations

### Lazy Loading
- Line items loaded only when needed
- Payment history optional
- Audit log on-demand

### Pagination
- Always use `InvoiceListRequestDto` for lists
- Default page size: 20
- Max page size: 100

### Caching Candidates
- Invoice statistics (cache for 5 minutes)
- Payment schedule (cache until payment)
- Invoice PDF path (cache permanently)

---

## 🧪 Testing Checklist

- [ ] All enums have valid int values
- [ ] All DTOs have data annotations
- [ ] All interface methods documented
- [ ] Computed properties tested
- [ ] Validation rules enforced
- [ ] Status transitions validated
- [ ] Payment linkage secure
- [ ] Audit logging comprehensive

---

## 📝 Next Steps

**Task #4:** Implement `IInvoiceRepository` with:
- All 32 interface methods
- 15+ stored procedures
- Full error handling
- Transaction management
- Audit logging

**Task #5:** Payment state machine
**Task #6:** PDF generation
**Task #7:** API controllers

---

**Created by:** Tech Lead - Invoice System Implementation
**Last Updated:** 2026-02-20
**Status:** ✅ COMPLETE - Ready for Implementation
