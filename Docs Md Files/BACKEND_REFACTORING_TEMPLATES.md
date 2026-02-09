# Backend C# Refactoring Templates
## Correct Domain Terminology: Owner/Partner (NOT Vendor)

---

## 📋 Domain Models

### OwnerPayment.cs

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CateringEcommerce.Domain.Models.Owner
{
    /// <summary>
    /// Represents payment settlement for a catering owner/partner
    /// </summary>
    [Table("t_owner_payment")]
    public class OwnerPayment
    {
        [Key]
        [Column("c_owner_payment_id")]
        public long OwnerPaymentId { get; set; }

        [Required]
        [Column("c_owner_id")]
        public long OwnerId { get; set; }

        [Required]
        [Column("c_order_id")]
        public long OrderId { get; set; }

        [Column("c_settlement_amount")]
        [Range(0, double.MaxValue)]
        public decimal SettlementAmount { get; set; }

        [Column("c_platform_service_fee")]
        [Range(0, double.MaxValue)]
        public decimal PlatformServiceFee { get; set; }

        [Column("c_net_settlement_amount")]
        [Range(0, double.MaxValue)]
        public decimal NetSettlementAmount { get; set; }

        [Column("c_status")]
        [MaxLength(20)]
        public string Status { get; set; } // PENDING, ESCROWED, RELEASED, FAILED

        [Column("c_payment_method")]
        [MaxLength(50)]
        public string PaymentMethod { get; set; }

        [Column("c_transaction_reference")]
        [MaxLength(100)]
        public string TransactionReference { get; set; }

        [Column("c_escrowed_at")]
        public DateTime? EscrowedAt { get; set; }

        [Column("c_released_at")]
        public DateTime? ReleasedAt { get; set; }

        [Column("c_failed_at")]
        public DateTime? FailedAt { get; set; }

        [Column("c_failure_reason")]
        [MaxLength(500)]
        public string FailureReason { get; set; }

        [Column("c_created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("c_updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("OwnerId")]
        public virtual Owner Owner { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }
    }
}
```

### OwnerSettlement.cs

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CateringEcommerce.Domain.Models.Owner
{
    /// <summary>
    /// Represents aggregated settlement for a catering partner
    /// </summary>
    [Table("t_owner_settlement")]
    public class OwnerSettlement
    {
        [Key]
        [Column("c_settlement_id")]
        public long SettlementId { get; set; }

        [Required]
        [Column("c_owner_id")]
        public long OwnerId { get; set; }

        [Column("c_settlement_period_start")]
        public DateTime PeriodStart { get; set; }

        [Column("c_settlement_period_end")]
        public DateTime PeriodEnd { get; set; }

        [Column("c_total_gross_amount")]
        [Range(0, double.MaxValue)]
        public decimal TotalGrossAmount { get; set; }

        [Column("c_total_platform_fee")]
        [Range(0, double.MaxValue)]
        public decimal TotalPlatformFee { get; set; }

        [Column("c_total_adjustments")]
        public decimal TotalAdjustments { get; set; } // Can be negative

        [Column("c_net_settlement_amount")]
        [Range(0, double.MaxValue)]
        public decimal NetSettlementAmount { get; set; }

        [Column("c_status")]
        [MaxLength(20)]
        public string Status { get; set; } // PENDING, PROCESSING, COMPLETED, FAILED

        [Column("c_processed_at")]
        public DateTime? ProcessedAt { get; set; }

        [Column("c_payment_batch_id")]
        [MaxLength(100)]
        public string PaymentBatchId { get; set; }

        [Column("c_created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("c_updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("OwnerId")]
        public virtual Owner Owner { get; set; }
    }
}
```

### PartnerApprovalRequest.cs

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CateringEcommerce.Domain.Models.Partner
{
    /// <summary>
    /// Represents approval requests requiring catering partner response
    /// (Menu changes, guest count increases, etc.)
    /// </summary>
    [Table("t_partner_approval_request")]
    public class PartnerApprovalRequest
    {
        [Key]
        [Column("c_approval_id")]
        public long ApprovalId { get; set; }

        [Required]
        [Column("c_owner_id")]
        public long OwnerId { get; set; }

        [Required]
        [Column("c_order_id")]
        public long OrderId { get; set; }

        [Required]
        [Column("c_request_type")]
        [MaxLength(50)]
        public string RequestType { get; set; } // MENU_CHANGE, GUEST_INCREASE, SPECIAL_REQUEST

        [Required]
        [Column("c_description")]
        [MaxLength(1000)]
        public string Description { get; set; }

        [Column("c_request_data")]
        public string RequestData { get; set; } // JSON payload

        [Column("c_requested_by_user_id")]
        public long RequestedByUserId { get; set; }

        [Column("c_requested_at")]
        public DateTime RequestedAt { get; set; } = DateTime.Now;

        [Column("c_deadline")]
        public DateTime Deadline { get; set; }

        [Column("c_response_time_hours")]
        public int ResponseTimeHours { get; set; } = 24;

        [Column("c_status")]
        [MaxLength(20)]
        public string Status { get; set; } // PENDING, APPROVED, REJECTED, EXPIRED

        [Column("c_approved_at")]
        public DateTime? ApprovedAt { get; set; }

        [Column("c_approved_by_owner_id")]
        public long? ApprovedByOwnerId { get; set; }

        [Column("c_rejected_at")]
        public DateTime? RejectedAt { get; set; }

        [Column("c_rejection_reason")]
        [MaxLength(500)]
        public string RejectionReason { get; set; }

        [Column("c_partner_notes")]
        [MaxLength(1000)]
        public string PartnerNotes { get; set; }

        [Column("c_created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("c_updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("OwnerId")]
        public virtual Owner Owner { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }
    }
}
```

### OwnerPayoutSchedule.cs

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CateringEcommerce.Domain.Models.Owner
{
    /// <summary>
    /// Scheduled payouts for catering partners
    /// </summary>
    [Table("t_owner_payout_schedule")]
    public class OwnerPayoutSchedule
    {
        [Key]
        [Column("c_schedule_id")]
        public long ScheduleId { get; set; }

        [Required]
        [Column("c_owner_id")]
        public long OwnerId { get; set; }

        [Column("c_settlement_id")]
        public long? SettlementId { get; set; }

        [Column("c_scheduled_amount")]
        [Range(0, double.MaxValue)]
        public decimal ScheduledAmount { get; set; }

        [Column("c_scheduled_date")]
        public DateTime ScheduledDate { get; set; }

        [Column("c_is_released")]
        public bool IsReleased { get; set; } = false;

        [Column("c_released_at")]
        public DateTime? ReleasedAt { get; set; }

        [Column("c_release_method")]
        [MaxLength(50)]
        public string ReleaseMethod { get; set; } // BANK_TRANSFER, UPI, WALLET

        [Column("c_transaction_id")]
        [MaxLength(100)]
        public string TransactionId { get; set; }

        [Column("c_status")]
        [MaxLength(20)]
        public string Status { get; set; } // SCHEDULED, PROCESSING, RELEASED, FAILED

        [Column("c_created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("c_updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("OwnerId")]
        public virtual Owner Owner { get; set; }

        [ForeignKey("SettlementId")]
        public virtual OwnerSettlement OwnerSettlement { get; set; }
    }
}
```

---

## 📋 Enums

### OwnerPaymentStatus.cs

```csharp
namespace CateringEcommerce.Domain.Enums
{
    public enum OwnerPaymentStatus
    {
        PENDING = 1,
        ESCROWED = 2,
        RELEASED = 3,
        FAILED = 4,
        REFUNDED = 5,
        CANCELLED = 6
    }
}
```

### OwnerSettlementStatus.cs

```csharp
namespace CateringEcommerce.Domain.Enums
{
    public enum OwnerSettlementStatus
    {
        PENDING = 1,
        PROCESSING = 2,
        COMPLETED = 3,
        FAILED = 4,
        CANCELLED = 5
    }
}
```

### PartnerApprovalStatus.cs

```csharp
namespace CateringEcommerce.Domain.Enums
{
    public enum PartnerApprovalStatus
    {
        PENDING = 1,
        APPROVED = 2,
        REJECTED = 3,
        EXPIRED = 4,
        CANCELLED = 5
    }
}
```

### PartnerApprovalType.cs

```csharp
namespace CateringEcommerce.Domain.Enums
{
    public enum PartnerApprovalType
    {
        MENU_CHANGE = 1,
        GUEST_COUNT_INCREASE = 2,
        SPECIAL_REQUEST = 3,
        EVENT_MODIFICATION = 4
    }
}
```

---

## 📋 DTOs (Data Transfer Objects)

### OwnerSettlementDto.cs

```csharp
using System;

namespace CateringEcommerce.Domain.Models.DTOs
{
    public class OwnerSettlementDto
    {
        public long OwnerId { get; set; }
        public string OwnerName { get; set; }
        public decimal SettlementAmount { get; set; }
        public decimal PlatformServiceFee { get; set; }
        public decimal NetAmount { get; set; }
        public string Status { get; set; }
        public DateTime? ReleasedAt { get; set; }
        public string PaymentMethod { get; set; }
    }
}
```

### PartnerApprovalRequestDto.cs

```csharp
using System;

namespace CateringEcommerce.Domain.Models.DTOs
{
    public class PartnerApprovalRequestDto
    {
        public long OwnerId { get; set; }
        public long OrderId { get; set; }
        public string RequestType { get; set; }
        public string Description { get; set; }
        public string RequestData { get; set; }
        public DateTime Deadline { get; set; }
        public int ResponseTimeHours { get; set; }
    }

    public class PartnerApprovalResponseDto
    {
        public long ApprovalId { get; set; }
        public bool Approved { get; set; }
        public string ResponseNotes { get; set; }
        public string RejectionReason { get; set; }
    }
}
```

### ReleaseOwnerSettlementDto.cs

```csharp
namespace CateringEcommerce.Domain.Models.DTOs
{
    public class ReleaseOwnerSettlementDto
    {
        public long OwnerPaymentId { get; set; }
        public long OrderId { get; set; }
        public string ReleaseMethod { get; set; }
        public string AdminNotes { get; set; }
    }
}
```

---

## 📋 Service Interfaces

### IOwnerPaymentService.cs

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using CateringEcommerce.Domain.Models.Owner;
using CateringEcommerce.Domain.Models.DTOs;

namespace CateringEcommerce.Domain.Interfaces.Services
{
    public interface IOwnerPaymentService
    {
        Task<OwnerPayment> CreateOwnerPaymentAsync(long orderId, long ownerId);
        Task<OwnerPayment> GetOwnerPaymentByOrderIdAsync(long orderId);
        Task<List<OwnerPayment>> GetOwnerPaymentsAsync(long ownerId, string status = null);
        Task<bool> ReleaseSettlementAsync(long ownerPaymentId, ReleaseOwnerSettlementDto dto);
        Task<decimal> CalculateNetSettlementAsync(decimal grossAmount, long ownerId);
        Task<bool> ProcessFailedPaymentAsync(long ownerPaymentId, string failureReason);
    }
}
```

### IPartnerApprovalService.cs

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using CateringEcommerce.Domain.Models.Partner;
using CateringEcommerce.Domain.Models.DTOs;

namespace CateringEcommerce.Domain.Interfaces.Services
{
    public interface IPartnerApprovalService
    {
        Task<PartnerApprovalRequest> CreateApprovalRequestAsync(PartnerApprovalRequestDto dto);
        Task<PartnerApprovalRequest> GetApprovalByIdAsync(long approvalId);
        Task<List<PartnerApprovalRequest>> GetPendingApprovalsAsync(long ownerId);
        Task<bool> ApproveRequestAsync(long approvalId, long ownerId, PartnerApprovalResponseDto dto);
        Task<bool> RejectRequestAsync(long approvalId, long ownerId, PartnerApprovalResponseDto dto);
        Task HandleExpiredApprovalsAsync();
    }
}
```

### IOwnerSettlementService.cs

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CateringEcommerce.Domain.Models.Owner;

namespace CateringEcommerce.Domain.Interfaces.Services
{
    public interface IOwnerSettlementService
    {
        Task<OwnerSettlement> CreateSettlementAsync(long ownerId, DateTime periodStart, DateTime periodEnd);
        Task<List<OwnerSettlement>> GetPendingSettlementsAsync();
        Task<List<OwnerSettlement>> GetOwnerSettlementsAsync(long ownerId, string status = null);
        Task<bool> ProcessSettlementAsync(long settlementId);
        Task<bool> MarkSettlementCompletedAsync(long settlementId, string paymentBatchId);
    }
}
```

---

## 📋 Controllers

### OwnerPaymentController.cs

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using CateringEcommerce.Domain.Interfaces.Services;
using CateringEcommerce.Domain.Models.DTOs;

namespace CateringEcommerce.API.Controllers.Owner
{
    [ApiController]
    [Route("api/owner/payments")]
    [Authorize(Roles = "Owner,Partner")]
    public class OwnerPaymentController : ControllerBase
    {
        private readonly IOwnerPaymentService _ownerPaymentService;

        public OwnerPaymentController(IOwnerPaymentService ownerPaymentService)
        {
            _ownerPaymentService = ownerPaymentService;
        }

        [HttpGet("settlements")]
        public async Task<IActionResult> GetSettlements([FromQuery] long ownerId, [FromQuery] string status = null)
        {
            var settlements = await _ownerPaymentService.GetOwnerPaymentsAsync(ownerId, status);
            return Ok(new { result = true, data = settlements });
        }

        [HttpGet("settlements/{orderId}")]
        public async Task<IActionResult> GetSettlementByOrder(long orderId)
        {
            var settlement = await _ownerPaymentService.GetOwnerPaymentByOrderIdAsync(orderId);
            if (settlement == null)
                return NotFound(new { result = false, message = "Settlement not found" });

            return Ok(new { result = true, data = settlement });
        }

        [HttpGet("calculate-settlement")]
        public async Task<IActionResult> CalculateSettlement([FromQuery] decimal grossAmount, [FromQuery] long ownerId)
        {
            var netAmount = await _ownerPaymentService.CalculateNetSettlementAsync(grossAmount, ownerId);
            return Ok(new { result = true, data = new { netAmount, platformFee = grossAmount - netAmount } });
        }
    }
}
```

### PartnerApprovalController.cs

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using CateringEcommerce.Domain.Interfaces.Services;
using CateringEcommerce.Domain.Models.DTOs;

namespace CateringEcommerce.API.Controllers.Partner
{
    [ApiController]
    [Route("api/partner/approvals")]
    [Authorize(Roles = "Owner,Partner")]
    public class PartnerApprovalController : ControllerBase
    {
        private readonly IPartnerApprovalService _approvalService;

        public PartnerApprovalController(IPartnerApprovalService approvalService)
        {
            _approvalService = approvalService;
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingApprovals([FromQuery] long ownerId)
        {
            var approvals = await _approvalService.GetPendingApprovalsAsync(ownerId);
            return Ok(new { result = true, data = approvals });
        }

        [HttpGet("{approvalId}")]
        public async Task<IActionResult> GetApprovalById(long approvalId)
        {
            var approval = await _approvalService.GetApprovalByIdAsync(approvalId);
            if (approval == null)
                return NotFound(new { result = false, message = "Approval request not found" });

            return Ok(new { result = true, data = approval });
        }

        [HttpPost("{approvalId}/approve")]
        public async Task<IActionResult> ApproveRequest(long approvalId, [FromBody] PartnerApprovalResponseDto dto)
        {
            var ownerId = long.Parse(User.FindFirst("OwnerId")?.Value ?? "0");
            var result = await _approvalService.ApproveRequestAsync(approvalId, ownerId, dto);

            if (result)
                return Ok(new { result = true, message = "Request approved successfully" });

            return BadRequest(new { result = false, message = "Failed to approve request" });
        }

        [HttpPost("{approvalId}/reject")]
        public async Task<IActionResult> RejectRequest(long approvalId, [FromBody] PartnerApprovalResponseDto dto)
        {
            var ownerId = long.Parse(User.FindFirst("OwnerId")?.Value ?? "0");
            var result = await _approvalService.RejectRequestAsync(approvalId, ownerId, dto);

            if (result)
                return Ok(new { result = true, message = "Request rejected successfully" });

            return BadRequest(new { result = false, message = "Failed to reject request" });
        }
    }
}
```

### AdminOwnerSettlementController.cs

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using CateringEcommerce.Domain.Interfaces.Services;
using CateringEcommerce.Domain.Models.DTOs;

namespace CateringEcommerce.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/owner-settlements")]
    [Authorize(Roles = "Admin")]
    public class AdminOwnerSettlementController : ControllerBase
    {
        private readonly IOwnerPaymentService _ownerPaymentService;
        private readonly IOwnerSettlementService _settlementService;

        public AdminOwnerSettlementController(
            IOwnerPaymentService ownerPaymentService,
            IOwnerSettlementService settlementService)
        {
            _ownerPaymentService = ownerPaymentService;
            _settlementService = settlementService;
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingSettlements()
        {
            var settlements = await _settlementService.GetPendingSettlementsAsync();
            return Ok(new { result = true, data = settlements });
        }

        [HttpPost("{settlementId}/release")]
        public async Task<IActionResult> ReleaseSettlement(long settlementId, [FromBody] ReleaseOwnerSettlementDto dto)
        {
            var result = await _ownerPaymentService.ReleaseSettlementAsync(settlementId, dto);

            if (result)
                return Ok(new { result = true, message = "Settlement released successfully to catering partner" });

            return BadRequest(new { result = false, message = "Failed to release settlement" });
        }

        [HttpPost("{settlementId}/process")]
        public async Task<IActionResult> ProcessSettlement(long settlementId)
        {
            var result = await _settlementService.ProcessSettlementAsync(settlementId);

            if (result)
                return Ok(new { result = true, message = "Settlement processing initiated" });

            return BadRequest(new { result = false, message = "Failed to process settlement" });
        }
    }
}
```

---

## 📋 Repository Interfaces

### IOwnerPaymentRepository.cs

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using CateringEcommerce.Domain.Models.Owner;

namespace CateringEcommerce.Domain.Interfaces.Repositories
{
    public interface IOwnerPaymentRepository
    {
        Task<OwnerPayment> CreateAsync(OwnerPayment payment);
        Task<OwnerPayment> GetByIdAsync(long ownerPaymentId);
        Task<OwnerPayment> GetByOrderIdAsync(long orderId);
        Task<List<OwnerPayment>> GetByOwnerIdAsync(long ownerId, string status = null);
        Task<bool> UpdateAsync(OwnerPayment payment);
        Task<bool> UpdateStatusAsync(long ownerPaymentId, string status);
        Task<List<OwnerPayment>> GetPendingPaymentsAsync();
    }
}
```

### IPartnerApprovalRepository.cs

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using CateringEcommerce.Domain.Models.Partner;

namespace CateringEcommerce.Domain.Interfaces.Repositories
{
    public interface IPartnerApprovalRepository
    {
        Task<PartnerApprovalRequest> CreateAsync(PartnerApprovalRequest request);
        Task<PartnerApprovalRequest> GetByIdAsync(long approvalId);
        Task<List<PartnerApprovalRequest>> GetPendingByOwnerIdAsync(long ownerId);
        Task<List<PartnerApprovalRequest>> GetExpiredRequestsAsync();
        Task<bool> UpdateAsync(PartnerApprovalRequest request);
        Task<bool> UpdateStatusAsync(long approvalId, string status);
    }
}
```

---

## ✅ Key Changes Summary

1. **All "Vendor" → "Owner" or "Partner"**
2. **Table names use `t_owner_*` and `t_partner_*`**
3. **Column names use `c_owner_id`, NOT `c_vendor_id`**
4. **Payment flow: Customer → Admin Escrow → Owner Settlement**
5. **No "vendor commission" - use "Platform Service Fee"**
6. **No "vendor payout" - use "Partner Settlement"**
7. **Controllers: OwnerPaymentController, PartnerApprovalController**
8. **Services: IOwnerPaymentService, IPartnerApprovalService**
9. **DTOs: OwnerSettlementDto, PartnerApprovalRequestDto**
10. **Enums: OwnerPaymentStatus, PartnerApprovalStatus**

---

## 🚫 What NOT to Do

```csharp
// ❌ WRONG - DO NOT CREATE
public class VendorPayment { }
public interface IVendorPaymentService { }
public class VendorPaymentController { }
public enum VendorPaymentStatus { }

// ❌ WRONG - DO NOT USE
[Column("c_vendor_id")]
[Table("t_vendor_payment")]
[Route("api/vendor/*")]

// ❌ WRONG - DO NOT WRITE
var vendorPayout = await GetVendorPayoutAsync();
await ReleaseVendorCommissionAsync();
```

---

**All backend code must reference: Owner, Partner, Catering Owner**
**NEVER: Vendor**
