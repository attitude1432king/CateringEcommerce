# 🎯 SAMPLE TASTING IMPLEMENTATION - COMPLETE TECHNICAL SPECIFICATION

**Version:** 1.0
**Date:** February 7, 2026
**Status:** READY FOR IMPLEMENTATION

---

## 📋 TABLE OF CONTENTS

1. [Architecture Overview](#architecture-overview)
2. [Domain Models](#domain-models)
3. [Repository Layer](#repository-layer)
4. [Service Layer](#service-layer)
5. [API Endpoints](#api-endpoints)
6. [Frontend Flow (Client)](#frontend-flow-client)
7. [Frontend Flow (Partner)](#frontend-flow-partner)
8. [Third-Party Delivery Integration](#third-party-delivery-integration)
9. [Validation & Abuse Prevention](#validation--abuse-prevention)
10. [Payment Integration](#payment-integration)
11. [Notification System](#notification-system)
12. [Testing Checklist](#testing-checklist)

---

## 🏗️ ARCHITECTURE OVERVIEW

### **Key Principles (NON-NEGOTIABLE)**

✅ **Sample pricing NEVER uses package pricing**
✅ **Always prepaid - NO COD or partial payments**
✅ **Max 2-3 items (configurable)**
✅ **Fixed sample quantity per item**
✅ **Partner can accept OR reject (100% refund on rejection)**
✅ **Third-party automated delivery with live tracking**
✅ **Direct conversion to full event booking**

### **System Flow**

```
CLIENT FLOW:
1. Browse Catering → View Menu/Packages
2. Select Sample Items (max 3)
3. Pay Full Amount (Razorpay)
4. Status: SAMPLE_REQUESTED

PARTNER FLOW:
5a. Accept → SAMPLE_ACCEPTED → SAMPLE_PREPARING → READY_FOR_PICKUP
5b. Reject → SAMPLE_REJECTED → AUTO_REFUND (Partner earns ₹0)

DELIVERY FLOW:
6. Partner clicks "Request Pickup"
7. System calls Dunzo/Porter/Shadowfax API
8. Live tracking: PICKUP_ASSIGNED → PICKED_UP → IN_TRANSIT → DELIVERED

CLIENT POST-DELIVERY:
9. Mark as received
10. Provide feedback (taste, hygiene)
11. CTA: "Proceed with Full Event Booking"
```

---

## 📦 DOMAIN MODELS

### **1. SampleOrder.cs**

```csharp
using System;
using System.Collections.Generic;

namespace CateringEcommerce.Domain.Models.Sample
{
    public class SampleOrder
    {
        public long SampleOrderId { get; set; }
        public string SampleOrderNumber { get; set; } // SMPL-YYYYMMDD-XXXX

        // Client & Partner
        public long UserId { get; set; }
        public long CateringId { get; set; }

        // Pricing (NEVER from package)
        public decimal ItemsTotal { get; set; }
        public decimal DeliveryCharge { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }

        // Payment
        public long? PaymentId { get; set; }
        public string PaymentStatus { get; set; } = "PENDING"; // PAID, REFUNDED
        public string PaymentMethod { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? RefundedAt { get; set; }
        public decimal? RefundAmount { get; set; }

        // Order Status (LOCKED ENUM)
        public SampleOrderStatus CurrentStatus { get; set; } = SampleOrderStatus.SAMPLE_REQUESTED;

        // Delivery Info
        public long DeliveryAddressId { get; set; }
        public int? DeliveryProviderId { get; set; }
        public string DeliveryTrackingId { get; set; }
        public string DeliveryPartnerName { get; set; }
        public string DeliveryPartnerPhone { get; set; }
        public DateTime? PickupRequestedAt { get; set; }
        public DateTime? PickedUpAt { get; set; }
        public DateTime? EstimatedDeliveryAt { get; set; }
        public DateTime? ActualDeliveryAt { get; set; }

        // Partner Actions
        public DateTime? PartnerAcceptedAt { get; set; }
        public DateTime? PartnerRejectedAt { get; set; }
        public string RejectionReason { get; set; }

        // Client Feedback
        public string ClientFeedback { get; set; }
        public int? TasteRating { get; set; } // 1-5
        public int? HygieneRating { get; set; } // 1-5
        public int? OverallRating { get; set; } // 1-5
        public DateTime? FeedbackSubmittedAt { get; set; }

        // Conversion Tracking
        public bool ConvertedToEventBooking { get; set; }
        public long? EventOrderId { get; set; }
        public DateTime? ConvertedAt { get; set; }

        // Metadata
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public List<SampleOrderItem> Items { get; set; } = new();
        public SampleDeliveryTracking DeliveryTracking { get; set; }
        public UserAddress DeliveryAddress { get; set; }
        public User User { get; set; }
        public Owner Catering { get; set; }
    }
}
```

### **2. SampleOrderItem.cs**

```csharp
namespace CateringEcommerce.Domain.Models.Sample
{
    public class SampleOrderItem
    {
        public long SampleOrderItemId { get; set; }
        public long SampleOrderId { get; set; }

        // Item Selection
        public long MenuItemId { get; set; }
        public long? PackageId { get; set; } // Optional: if from package

        public string ItemName { get; set; }
        public int SampleQuantity { get; set; } = 1; // FIXED - NO CLIENT CONTROL
        public decimal SamplePrice { get; set; } // PER ITEM SAMPLE PRICE (NOT package price)
        public decimal TotalPrice { get; set; } // SampleQuantity * SamplePrice

        // Metadata
        public DateTime CreatedAt { get; set; }

        // Navigation
        public SampleOrder SampleOrder { get; set; }
        public FoodItem MenuItem { get; set; }
    }
}
```

### **3. SampleDeliveryTracking.cs**

```csharp
namespace CateringEcommerce.Domain.Models.Sample
{
    public class SampleDeliveryTracking
    {
        public long TrackingId { get; set; }
        public long SampleOrderId { get; set; }

        // Delivery Provider
        public int DeliveryProviderId { get; set; }
        public string ExternalTrackingId { get; set; } // Provider's tracking ID
        public string DeliveryPartnerName { get; set; }
        public string DeliveryPartnerPhone { get; set; }
        public string DeliveryPartnerPhoto { get; set; }
        public string VehicleNumber { get; set; }

        // Live Location (Like Swiggy/Zomato)
        public decimal? CurrentLatitude { get; set; }
        public decimal? CurrentLongitude { get; set; }
        public DateTime? LastLocationUpdateAt { get; set; }

        // Status & ETA
        public SampleDeliveryStatus DeliveryStatus { get; set; } = SampleDeliveryStatus.PICKUP_ASSIGNED;
        public DateTime? EstimatedDeliveryAt { get; set; }
        public DateTime? ActualDeliveryAt { get; set; }

        // Metadata
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        public SampleOrder SampleOrder { get; set; }
        public SampleDeliveryProvider DeliveryProvider { get; set; }
    }
}
```

### **4. SampleTastingConfig.cs**

```csharp
namespace CateringEcommerce.Domain.Models.Sample
{
    public class SampleTastingConfig
    {
        public int ConfigId { get; set; }

        // Item Selection (ABUSE PREVENTION)
        public int MaxItemsAllowed { get; set; } = 3;
        public int FixedSampleQuantity { get; set; } = 1;

        // Pricing Model
        public bool FlatSampleFeeEnabled { get; set; } = false;
        public decimal? FlatSampleFeeAmount { get; set; }

        // Delivery
        public bool DeliveryChargeApplicable { get; set; } = true;
        public decimal? MinOrderAmountForFreeDelivery { get; set; }

        // Status
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
```

### **5. MenuItemSamplePricing.cs**

```csharp
namespace CateringEcommerce.Domain.Models.Sample
{
    /// <summary>
    /// CRITICAL: Sample pricing MUST be set per menu item
    /// NEVER derived from package pricing
    /// </summary>
    public class MenuItemSamplePricing
    {
        public long Id { get; set; }
        public long MenuItemId { get; set; }

        // SAMPLE-SPECIFIC PRICING (NOT package-based)
        public decimal SamplePrice { get; set; } // e.g., ₹150-₹300 per item
        public int SampleQuantity { get; set; } = 1; // Fixed sample size

        // Availability
        public bool IsAvailableForSample { get; set; } = true;

        // Partner Control
        public long OwnerId { get; set; }
        public bool IsPartnerApproved { get; set; } = true;

        // Audit
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        public FoodItem MenuItem { get; set; }
        public Owner Owner { get; set; }
    }
}
```

### **6. SampleOrderAbuseTracking.cs**

```csharp
namespace CateringEcommerce.Domain.Models.Sample
{
    public class SampleOrderAbuseTracking
    {
        public int TrackingId { get; set; }
        public long UserId { get; set; }
        public long CateringId { get; set; }

        // Abuse Metrics
        public int TotalSampleOrders { get; set; } = 0;
        public DateTime? LastSampleOrderDate { get; set; }
        public int TotalRefunds { get; set; } = 0;

        // Blocking Rules
        public bool IsBlocked { get; set; } = false;
        public DateTime? BlockedUntil { get; set; }
        public string BlockReason { get; set; }

        // Metadata
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
```

---

## 🗄️ REPOSITORY LAYER

### **ISampleOrderRepository.cs**

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CateringEcommerce.Domain.Models.Sample;

namespace CateringEcommerce.Domain.Interfaces.Sample
{
    public interface ISampleOrderRepository
    {
        // ========== CREATE ==========
        Task<long> CreateSampleOrderAsync(SampleOrder order, List<SampleOrderItem> items);
        Task AddSampleOrderItemsAsync(long sampleOrderId, List<SampleOrderItem> items);

        // ========== READ ==========
        Task<SampleOrder> GetSampleOrderByIdAsync(long sampleOrderId);
        Task<SampleOrder> GetSampleOrderByNumberAsync(string orderNumber);
        Task<List<SampleOrder>> GetUserSampleOrdersAsync(long userId);
        Task<List<SampleOrder>> GetPartnerPendingRequestsAsync(long cateringId);
        Task<List<SampleOrder>> GetPartnerSampleOrdersAsync(long cateringId, SampleOrderStatus? status = null);

        // ========== UPDATE ==========
        Task UpdateSampleOrderStatusAsync(long sampleOrderId, SampleOrderStatus newStatus, string changedBy, long? changedById = null, string notes = null);
        Task UpdatePaymentStatusAsync(long sampleOrderId, string paymentStatus, DateTime? paidAt = null);
        Task UpdateDeliveryTrackingAsync(long sampleOrderId, string trackingId, int providerId);
        Task MarkPartnerAcceptedAsync(long sampleOrderId);
        Task MarkPartnerRejectedAsync(long sampleOrderId, string reason);
        Task MarkReadyForPickupAsync(long sampleOrderId);
        Task MarkDeliveredAsync(long sampleOrderId);
        Task SubmitClientFeedbackAsync(long sampleOrderId, string feedback, int tasteRating, int hygieneRating, int overallRating);
        Task MarkConvertedToEventAsync(long sampleOrderId, long eventOrderId);

        // ========== VALIDATION ==========
        Task<bool> ValidateItemSelectionAsync(long userId, long cateringId, int itemCount);
        Task<bool> CheckAbusePreventionAsync(long userId, long cateringId);
        Task<SampleTastingConfig> GetActiveConfigAsync();
        Task<List<MenuItemSamplePricing>> GetSampleEligibleItemsAsync(long cateringId);

        // ========== ABUSE TRACKING ==========
        Task UpdateAbuseTrackingAsync(long userId, long cateringId);
        Task<SampleOrderAbuseTracking> GetAbuseTrackingAsync(long userId, long cateringId);
    }
}
```

### **SampleOrderRepository.cs (Implementation)**

```csharp
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Sample;
using CateringEcommerce.Domain.Models.Sample;
using CateringEcommerce.Domain.Enums;
using Dapper;

namespace CateringEcommerce.BAL.Base.Sample
{
    public class SampleOrderRepository : ISampleOrderRepository
    {
        private readonly IDatabaseHelper _db;

        public SampleOrderRepository(IDatabaseHelper db)
        {
            _db = db;
        }

        // ========== CREATE ==========

        public async Task<long> CreateSampleOrderAsync(SampleOrder order, List<SampleOrderItem> items)
        {
            using var connection = _db.GetConnection();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Generate order number
                var orderDate = DateTime.Now.ToString("yyyyMMdd");
                var sequence = await GetNextSequenceNumberAsync(orderDate, connection, transaction);
                order.SampleOrderNumber = $"SMPL-{orderDate}-{sequence:D4}";

                // Insert order
                var orderSql = @"
                    INSERT INTO SampleOrders (
                        SampleOrderNumber, UserId, CateringId, DeliveryAddressId,
                        ItemsTotal, DeliveryCharge, TaxAmount, TotalAmount,
                        CurrentStatus, PaymentStatus, CreatedAt, UpdatedAt
                    )
                    OUTPUT INSERTED.SampleOrderId
                    VALUES (
                        @SampleOrderNumber, @UserId, @CateringId, @DeliveryAddressId,
                        @ItemsTotal, @DeliveryCharge, @TaxAmount, @TotalAmount,
                        @CurrentStatus, @PaymentStatus, GETDATE(), GETDATE()
                    )";

                var sampleOrderId = await connection.QuerySingleAsync<long>(orderSql, new
                {
                    order.SampleOrderNumber,
                    order.UserId,
                    order.CateringId,
                    order.DeliveryAddressId,
                    order.ItemsTotal,
                    order.DeliveryCharge,
                    order.TaxAmount,
                    order.TotalAmount,
                    CurrentStatus = (int)SampleOrderStatus.SAMPLE_REQUESTED,
                    PaymentStatus = "PENDING"
                }, transaction);

                // Insert items
                foreach (var item in items)
                {
                    item.SampleOrderId = sampleOrderId;
                    await InsertSampleOrderItemAsync(item, connection, transaction);
                }

                // Insert status history
                await InsertStatusHistoryAsync(sampleOrderId, null, SampleOrderStatus.SAMPLE_REQUESTED, "CLIENT", null, null, connection, transaction);

                transaction.Commit();
                return sampleOrderId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private async Task<int> GetNextSequenceNumberAsync(string orderDate, IDbConnection connection, IDbTransaction transaction)
        {
            var sql = @"
                SELECT ISNULL(MAX(CAST(RIGHT(SampleOrderNumber, 4) AS INT)), 0) + 1
                FROM SampleOrders
                WHERE SampleOrderNumber LIKE @Pattern";

            return await connection.QuerySingleAsync<int>(sql, new { Pattern = $"SMPL-{orderDate}%" }, transaction);
        }

        private async Task InsertSampleOrderItemAsync(SampleOrderItem item, IDbConnection connection, IDbTransaction transaction)
        {
            var sql = @"
                INSERT INTO SampleOrderItems (
                    SampleOrderId, MenuItemId, PackageId, ItemName,
                    SampleQuantity, SamplePrice, TotalPrice, CreatedAt
                )
                VALUES (
                    @SampleOrderId, @MenuItemId, @PackageId, @ItemName,
                    @SampleQuantity, @SamplePrice, @TotalPrice, GETDATE()
                )";

            await connection.ExecuteAsync(sql, item, transaction);
        }

        // ========== READ ==========

        public async Task<SampleOrder> GetSampleOrderByIdAsync(long sampleOrderId)
        {
            using var connection = _db.GetConnection();

            var sql = @"
                SELECT * FROM SampleOrders WHERE SampleOrderId = @SampleOrderId;
                SELECT * FROM SampleOrderItems WHERE SampleOrderId = @SampleOrderId;
                SELECT * FROM SampleDeliveryTracking WHERE SampleOrderId = @SampleOrderId;";

            using var multi = await connection.QueryMultipleAsync(sql, new { sampleOrderId });

            var order = await multi.ReadSingleOrDefaultAsync<SampleOrder>();
            if (order != null)
            {
                order.Items = (await multi.ReadAsync<SampleOrderItem>()).AsList();
                order.DeliveryTracking = await multi.ReadSingleOrDefaultAsync<SampleDeliveryTracking>();
            }

            return order;
        }

        public async Task<List<SampleOrder>> GetPartnerPendingRequestsAsync(long cateringId)
        {
            using var connection = _db.GetConnection();

            var sql = @"
                SELECT
                    so.*,
                    u.FirstName + ' ' + u.LastName AS ClientName,
                    u.Email AS ClientEmail,
                    u.PhoneNumber AS ClientPhone,
                    ua.FullAddress,
                    ua.Latitude,
                    ua.Longitude,
                    (SELECT COUNT(*) FROM SampleOrderItems WHERE SampleOrderId = so.SampleOrderId) AS ItemCount
                FROM SampleOrders so
                INNER JOIN Users u ON so.UserId = u.UserId
                INNER JOIN UserAddresses ua ON so.DeliveryAddressId = ua.AddressId
                WHERE so.CateringId = @CateringId
                AND so.CurrentStatus = @Status
                AND so.PaymentStatus = 'PAID'
                ORDER BY so.CreatedAt ASC";

            return (await connection.QueryAsync<SampleOrder>(sql, new
            {
                cateringId,
                Status = (int)SampleOrderStatus.SAMPLE_REQUESTED
            })).AsList();
        }

        // ========== UPDATE ==========

        public async Task UpdateSampleOrderStatusAsync(long sampleOrderId, SampleOrderStatus newStatus, string changedBy, long? changedById = null, string notes = null)
        {
            using var connection = _db.GetConnection();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Get current status
                var currentStatus = await connection.QuerySingleAsync<int>(
                    "SELECT CurrentStatus FROM SampleOrders WHERE SampleOrderId = @SampleOrderId",
                    new { sampleOrderId },
                    transaction);

                // Update order status
                await connection.ExecuteAsync(
                    "UPDATE SampleOrders SET CurrentStatus = @NewStatus, UpdatedAt = GETDATE() WHERE SampleOrderId = @SampleOrderId",
                    new { sampleOrderId, NewStatus = (int)newStatus },
                    transaction);

                // Insert status history
                await InsertStatusHistoryAsync(sampleOrderId, (SampleOrderStatus)currentStatus, newStatus, changedBy, changedById, notes, connection, transaction);

                // Update specific timestamp fields
                await UpdateStatusTimestampsAsync(sampleOrderId, newStatus, connection, transaction);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private async Task UpdateStatusTimestampsAsync(long sampleOrderId, SampleOrderStatus status, IDbConnection connection, IDbTransaction transaction)
        {
            string sql = status switch
            {
                SampleOrderStatus.SAMPLE_ACCEPTED => "UPDATE SampleOrders SET PartnerAcceptedAt = GETDATE() WHERE SampleOrderId = @SampleOrderId",
                SampleOrderStatus.SAMPLE_REJECTED => "UPDATE SampleOrders SET PartnerRejectedAt = GETDATE() WHERE SampleOrderId = @SampleOrderId",
                SampleOrderStatus.READY_FOR_PICKUP => "UPDATE SampleOrders SET PickupRequestedAt = GETDATE() WHERE SampleOrderId = @SampleOrderId",
                SampleOrderStatus.IN_TRANSIT => "UPDATE SampleOrders SET PickedUpAt = GETDATE() WHERE SampleOrderId = @SampleOrderId",
                SampleOrderStatus.DELIVERED => "UPDATE SampleOrders SET ActualDeliveryAt = GETDATE() WHERE SampleOrderId = @SampleOrderId",
                _ => null
            };

            if (sql != null)
            {
                await connection.ExecuteAsync(sql, new { sampleOrderId }, transaction);
            }
        }

        private async Task InsertStatusHistoryAsync(long sampleOrderId, SampleOrderStatus? fromStatus, SampleOrderStatus toStatus, string changedBy, long? changedById, string notes, IDbConnection connection, IDbTransaction transaction)
        {
            var sql = @"
                INSERT INTO SampleOrderStatusHistory (
                    SampleOrderId, FromStatus, ToStatus, StatusChangedBy, StatusChangedById, Notes, CreatedAt
                )
                VALUES (
                    @SampleOrderId, @FromStatus, @ToStatus, @ChangedBy, @ChangedById, @Notes, GETDATE()
                )";

            await connection.ExecuteAsync(sql, new
            {
                sampleOrderId,
                FromStatus = fromStatus.HasValue ? (int?)fromStatus.Value : null,
                ToStatus = (int)toStatus,
                ChangedBy = changedBy,
                ChangedById = changedById,
                Notes = notes
            }, transaction);
        }

        public async Task MarkPartnerRejectedAsync(long sampleOrderId, string reason)
        {
            using var connection = _db.GetConnection();

            var sql = @"
                UPDATE SampleOrders
                SET RejectionReason = @Reason,
                    PartnerRejectedAt = GETDATE(),
                    UpdatedAt = GETDATE()
                WHERE SampleOrderId = @SampleOrderId";

            await connection.ExecuteAsync(sql, new { sampleOrderId, reason });
        }

        public async Task SubmitClientFeedbackAsync(long sampleOrderId, string feedback, int tasteRating, int hygieneRating, int overallRating)
        {
            using var connection = _db.GetConnection();

            var sql = @"
                UPDATE SampleOrders
                SET ClientFeedback = @Feedback,
                    TasteRating = @TasteRating,
                    HygieneRating = @HygieneRating,
                    OverallRating = @OverallRating,
                    FeedbackSubmittedAt = GETDATE(),
                    UpdatedAt = GETDATE()
                WHERE SampleOrderId = @SampleOrderId";

            await connection.ExecuteAsync(sql, new { sampleOrderId, feedback, tasteRating, hygieneRating, overallRating });
        }

        public async Task MarkConvertedToEventAsync(long sampleOrderId, long eventOrderId)
        {
            using var connection = _db.GetConnection();

            var sql = @"
                UPDATE SampleOrders
                SET ConvertedToEventBooking = 1,
                    EventOrderId = @EventOrderId,
                    ConvertedAt = GETDATE(),
                    UpdatedAt = GETDATE()
                WHERE SampleOrderId = @SampleOrderId";

            await connection.ExecuteAsync(sql, new { sampleOrderId, eventOrderId });
        }

        // ========== VALIDATION ==========

        public async Task<SampleTastingConfig> GetActiveConfigAsync()
        {
            using var connection = _db.GetConnection();

            var sql = "SELECT TOP 1 * FROM SampleTastingConfig WHERE IsActive = 1 ORDER BY ConfigId DESC";
            return await connection.QuerySingleOrDefaultAsync<SampleTastingConfig>(sql);
        }

        public async Task<bool> CheckAbusePreventionAsync(long userId, long cateringId)
        {
            using var connection = _db.GetConnection();

            var sql = @"
                SELECT ISNULL(IsBlocked, 0)
                FROM SampleOrderAbuseTracking
                WHERE UserId = @UserId AND CateringId = @CateringId";

            var isBlocked = await connection.QuerySingleOrDefaultAsync<bool>(sql, new { userId, cateringId });
            return !isBlocked;
        }

        public async Task<List<MenuItemSamplePricing>> GetSampleEligibleItemsAsync(long cateringId)
        {
            using var connection = _db.GetConnection();

            var sql = @"
                SELECT
                    fi.PkID AS MenuItemId,
                    fi.Name AS ItemName,
                    fi.Category,
                    fi.Description,
                    fi.ImagePath,
                    fi.IsVeg,
                    fi.CuisineType,
                    sp.SamplePrice,
                    sp.SampleQuantity,
                    sp.IsAvailableForSample
                FROM FoodItems fi
                INNER JOIN MenuItemSamplePricing sp ON fi.PkID = sp.MenuItemId
                WHERE sp.OwnerId = @CateringId
                AND sp.IsAvailableForSample = 1
                AND fi.IsActive = 1
                ORDER BY fi.Category, fi.Name";

            return (await connection.QueryAsync<MenuItemSamplePricing>(sql, new { cateringId })).AsList();
        }

        // ========== ABUSE TRACKING ==========

        public async Task UpdateAbuseTrackingAsync(long userId, long cateringId)
        {
            using var connection = _db.GetConnection();

            var sql = @"
                IF EXISTS (SELECT 1 FROM SampleOrderAbuseTracking WHERE UserId = @UserId AND CateringId = @CateringId)
                    UPDATE SampleOrderAbuseTracking
                    SET TotalSampleOrders = TotalSampleOrders + 1,
                        LastSampleOrderDate = GETDATE(),
                        UpdatedAt = GETDATE()
                    WHERE UserId = @UserId AND CateringId = @CateringId
                ELSE
                    INSERT INTO SampleOrderAbuseTracking (UserId, CateringId, TotalSampleOrders, LastSampleOrderDate, CreatedAt, UpdatedAt)
                    VALUES (@UserId, @CateringId, 1, GETDATE(), GETDATE(), GETDATE())";

            await connection.ExecuteAsync(sql, new { userId, cateringId });
        }

        // ========== ADDITIONAL METHODS (IMPLEMENT AS NEEDED) ==========

        public Task<SampleOrder> GetSampleOrderByNumberAsync(string orderNumber) => throw new NotImplementedException();
        public Task<List<SampleOrder>> GetUserSampleOrdersAsync(long userId) => throw new NotImplementedException();
        public Task<List<SampleOrder>> GetPartnerSampleOrdersAsync(long cateringId, SampleOrderStatus? status = null) => throw new NotImplementedException();
        public Task AddSampleOrderItemsAsync(long sampleOrderId, List<SampleOrderItem> items) => throw new NotImplementedException();
        public Task UpdatePaymentStatusAsync(long sampleOrderId, string paymentStatus, DateTime? paidAt = null) => throw new NotImplementedException();
        public Task UpdateDeliveryTrackingAsync(long sampleOrderId, string trackingId, int providerId) => throw new NotImplementedException();
        public Task MarkPartnerAcceptedAsync(long sampleOrderId) => throw new NotImplementedException();
        public Task MarkReadyForPickupAsync(long sampleOrderId) => throw new NotImplementedException();
        public Task MarkDeliveredAsync(long sampleOrderId) => throw new NotImplementedException();
        public Task<bool> ValidateItemSelectionAsync(long userId, long cateringId, int itemCount) => throw new NotImplementedException();
        public Task<SampleOrderAbuseTracking> GetAbuseTrackingAsync(long userId, long cateringId) => throw new NotImplementedException();
    }
}
```

---

## ⚙️ SERVICE LAYER

### **ISampleTastingService.cs**

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using CateringEcommerce.Domain.Models.Sample;

namespace CateringEcommerce.Domain.Interfaces.Sample
{
    public interface ISampleTastingService
    {
        // ========== CLIENT SIDE ==========
        Task<(bool Success, string Message, long? OrderId)> CreateSampleOrderAsync(CreateSampleOrderRequest request);
        Task<List<MenuItemSamplePricing>> GetSampleEligibleItemsAsync(long cateringId);
        Task<SampleOrder> GetSampleOrderDetailsAsync(long sampleOrderId);
        Task<List<SampleOrder>> GetMySampleOrdersAsync(long userId);
        Task<bool> SubmitFeedbackAsync(long sampleOrderId, SampleFeedbackRequest feedback);
        Task<SampleDeliveryTracking> GetLiveTrackingAsync(long sampleOrderId);

        // ========== PARTNER SIDE ==========
        Task<List<SampleOrder>> GetPendingRequestsAsync(long cateringId);
        Task<(bool Success, string Message)> AcceptSampleRequestAsync(long sampleOrderId, long partnerId);
        Task<(bool Success, string Message)> RejectSampleRequestAsync(long sampleOrderId, long partnerId, string reason);
        Task<(bool Success, string Message)> MarkAsPreparingAsync(long sampleOrderId, long partnerId);
        Task<(bool Success, string Message)> RequestPickupAsync(long sampleOrderId, long partnerId);

        // ========== VALIDATION ==========
        Task<(bool IsValid, string ErrorMessage)> ValidateSampleOrderAsync(long userId, long cateringId, List<long> menuItemIds);
    }

    public class CreateSampleOrderRequest
    {
        public long UserId { get; set; }
        public long CateringId { get; set; }
        public long DeliveryAddressId { get; set; }
        public List<SampleOrderItemRequest> Items { get; set; }
    }

    public class SampleOrderItemRequest
    {
        public long MenuItemId { get; set; }
        public long? PackageId { get; set; }
    }

    public class SampleFeedbackRequest
    {
        public string Feedback { get; set; }
        public int TasteRating { get; set; }
        public int HygieneRating { get; set; }
        public int OverallRating { get; set; }
    }
}
```

### **SampleTastingService.cs (Implementation)**

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CateringEcommerce.Domain.Interfaces.Sample;
using CateringEcommerce.Domain.Models.Sample;
using CateringEcommerce.Domain.Enums;

namespace CateringEcommerce.BAL.Base.Sample
{
    public class SampleTastingService : ISampleTastingService
    {
        private readonly ISampleOrderRepository _sampleOrderRepo;
        private readonly ISampleDeliveryService _deliveryService;
        private readonly IPaymentService _paymentService;
        private readonly INotificationService _notificationService;

        public SampleTastingService(
            ISampleOrderRepository sampleOrderRepo,
            ISampleDeliveryService deliveryService,
            IPaymentService paymentService,
            INotificationService notificationService)
        {
            _sampleOrderRepo = sampleOrderRepo;
            _deliveryService = deliveryService;
            _paymentService = paymentService;
            _notificationService = notificationService;
        }

        // ========== CLIENT SIDE ==========

        public async Task<(bool Success, string Message, long? OrderId)> CreateSampleOrderAsync(CreateSampleOrderRequest request)
        {
            try
            {
                // STEP 1: VALIDATION (ABUSE PREVENTION)
                var validation = await ValidateSampleOrderAsync(request.UserId, request.CateringId, request.Items.Select(i => i.MenuItemId).ToList());
                if (!validation.IsValid)
                {
                    return (false, validation.ErrorMessage, null);
                }

                // STEP 2: GET SAMPLE PRICES (NEVER FROM PACKAGE)
                var eligibleItems = await _sampleOrderRepo.GetSampleEligibleItemsAsync(request.CateringId);
                var orderItems = new List<SampleOrderItem>();
                decimal itemsTotal = 0;

                foreach (var requestItem in request.Items)
                {
                    var pricingInfo = eligibleItems.FirstOrDefault(i => i.MenuItemId == requestItem.MenuItemId);
                    if (pricingInfo == null || !pricingInfo.IsAvailableForSample)
                    {
                        return (false, $"Item {requestItem.MenuItemId} is not available for sample tasting", null);
                    }

                    var orderItem = new SampleOrderItem
                    {
                        MenuItemId = requestItem.MenuItemId,
                        PackageId = requestItem.PackageId,
                        ItemName = pricingInfo.ItemName,
                        SampleQuantity = pricingInfo.SampleQuantity,
                        SamplePrice = pricingInfo.SamplePrice,
                        TotalPrice = pricingInfo.SamplePrice * pricingInfo.SampleQuantity
                    };

                    orderItems.Add(orderItem);
                    itemsTotal += orderItem.TotalPrice;
                }

                // STEP 3: CALCULATE DELIVERY CHARGE
                var config = await _sampleOrderRepo.GetActiveConfigAsync();
                decimal deliveryCharge = config.DeliveryChargeApplicable ? 50 : 0; // TODO: Calculate based on distance
                decimal taxAmount = itemsTotal * 0.05m; // 5% GST
                decimal totalAmount = itemsTotal + deliveryCharge + taxAmount;

                // STEP 4: CREATE ORDER
                var order = new SampleOrder
                {
                    UserId = request.UserId,
                    CateringId = request.CateringId,
                    DeliveryAddressId = request.DeliveryAddressId,
                    ItemsTotal = itemsTotal,
                    DeliveryCharge = deliveryCharge,
                    TaxAmount = taxAmount,
                    TotalAmount = totalAmount,
                    CurrentStatus = SampleOrderStatus.SAMPLE_REQUESTED,
                    PaymentStatus = "PENDING"
                };

                var sampleOrderId = await _sampleOrderRepo.CreateSampleOrderAsync(order, orderItems);

                // STEP 5: UPDATE ABUSE TRACKING
                await _sampleOrderRepo.UpdateAbuseTrackingAsync(request.UserId, request.CateringId);

                // STEP 6: NOTIFY PARTNER (After payment success - handled by payment webhook)

                return (true, "Sample order created successfully", sampleOrderId);
            }
            catch (Exception ex)
            {
                return (false, $"Error creating sample order: {ex.Message}", null);
            }
        }

        public async Task<List<MenuItemSamplePricing>> GetSampleEligibleItemsAsync(long cateringId)
        {
            return await _sampleOrderRepo.GetSampleEligibleItemsAsync(cateringId);
        }

        // ========== PARTNER SIDE ==========

        public async Task<List<SampleOrder>> GetPendingRequestsAsync(long cateringId)
        {
            return await _sampleOrderRepo.GetPartnerPendingRequestsAsync(cateringId);
        }

        public async Task<(bool Success, string Message)> AcceptSampleRequestAsync(long sampleOrderId, long partnerId)
        {
            try
            {
                var order = await _sampleOrderRepo.GetSampleOrderByIdAsync(sampleOrderId);

                if (order.CurrentStatus != SampleOrderStatus.SAMPLE_REQUESTED)
                {
                    return (false, "Sample request is no longer pending");
                }

                // Update status to ACCEPTED
                await _sampleOrderRepo.UpdateSampleOrderStatusAsync(
                    sampleOrderId,
                    SampleOrderStatus.SAMPLE_ACCEPTED,
                    "PARTNER",
                    partnerId,
                    "Partner accepted the sample request"
                );

                // Notify client
                await _notificationService.SendSampleAcceptedNotificationAsync(order.UserId, sampleOrderId);

                return (true, "Sample request accepted successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error accepting sample request: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> RejectSampleRequestAsync(long sampleOrderId, long partnerId, string reason)
        {
            try
            {
                var order = await _sampleOrderRepo.GetSampleOrderByIdAsync(sampleOrderId);

                if (order.CurrentStatus != SampleOrderStatus.SAMPLE_REQUESTED)
                {
                    return (false, "Sample request is no longer pending");
                }

                // CRITICAL: Partner earns ₹0 on rejection
                // Update status to REJECTED
                await _sampleOrderRepo.UpdateSampleOrderStatusAsync(
                    sampleOrderId,
                    SampleOrderStatus.SAMPLE_REJECTED,
                    "PARTNER",
                    partnerId,
                    reason
                );

                await _sampleOrderRepo.MarkPartnerRejectedAsync(sampleOrderId, reason);

                // AUTO-REFUND 100%
                await _paymentService.ProcessSampleRefundAsync(sampleOrderId, order.TotalAmount, "PARTNER_REJECTED");

                // Notify client
                await _notificationService.SendSampleRejectedNotificationAsync(order.UserId, sampleOrderId, reason);

                return (true, "Sample request rejected and refund initiated");
            }
            catch (Exception ex)
            {
                return (false, $"Error rejecting sample request: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> RequestPickupAsync(long sampleOrderId, long partnerId)
        {
            try
            {
                var order = await _sampleOrderRepo.GetSampleOrderByIdAsync(sampleOrderId);

                if (order.CurrentStatus != SampleOrderStatus.SAMPLE_PREPARING)
                {
                    return (false, "Sample is not ready for pickup yet");
                }

                // Update status to READY_FOR_PICKUP
                await _sampleOrderRepo.UpdateSampleOrderStatusAsync(
                    sampleOrderId,
                    SampleOrderStatus.READY_FOR_PICKUP,
                    "PARTNER",
                    partnerId
                );

                // CALL THIRD-PARTY DELIVERY API (Dunzo/Porter/Shadowfax)
                var deliveryResult = await _deliveryService.RequestPickupAsync(sampleOrderId);

                if (!deliveryResult.Success)
                {
                    return (false, $"Failed to request pickup: {deliveryResult.Message}");
                }

                // Update delivery tracking
                await _sampleOrderRepo.UpdateDeliveryTrackingAsync(sampleOrderId, deliveryResult.TrackingId, deliveryResult.ProviderId);

                // Update status to IN_TRANSIT
                await _sampleOrderRepo.UpdateSampleOrderStatusAsync(
                    sampleOrderId,
                    SampleOrderStatus.IN_TRANSIT,
                    "SYSTEM",
                    null,
                    "Delivery partner assigned"
                );

                // Notify client with live tracking link
                await _notificationService.SendSampleInTransitNotificationAsync(order.UserId, sampleOrderId, deliveryResult.TrackingUrl);

                return (true, "Pickup requested successfully. Delivery partner will arrive soon.");
            }
            catch (Exception ex)
            {
                return (false, $"Error requesting pickup: {ex.Message}");
            }
        }

        // ========== VALIDATION (ABUSE PREVENTION) ==========

        public async Task<(bool IsValid, string ErrorMessage)> ValidateSampleOrderAsync(long userId, long cateringId, List<long> menuItemIds)
        {
            // Rule 1: Check abuse prevention
            var isEligible = await _sampleOrderRepo.CheckAbusePreventionAsync(userId, cateringId);
            if (!isEligible)
            {
                return (false, "You are temporarily blocked from ordering samples from this caterer.");
            }

            // Rule 2: Check max items allowed
            var config = await _sampleOrderRepo.GetActiveConfigAsync();
            if (menuItemIds.Count > config.MaxItemsAllowed)
            {
                return (false, $"Maximum {config.MaxItemsAllowed} items allowed for sample tasting.");
            }

            // Rule 3: Check if items are sample-eligible
            var eligibleItems = await _sampleOrderRepo.GetSampleEligibleItemsAsync(cateringId);
            var eligibleItemIds = eligibleItems.Select(i => i.MenuItemId).ToList();

            foreach (var itemId in menuItemIds)
            {
                if (!eligibleItemIds.Contains(itemId))
                {
                    return (false, $"Item {itemId} is not available for sample tasting.");
                }
            }

            return (true, null);
        }

        // ========== ADDITIONAL METHODS (IMPLEMENT AS NEEDED) ==========

        public Task<SampleOrder> GetSampleOrderDetailsAsync(long sampleOrderId) => throw new NotImplementedException();
        public Task<List<SampleOrder>> GetMySampleOrdersAsync(long userId) => throw new NotImplementedException();
        public Task<bool> SubmitFeedbackAsync(long sampleOrderId, SampleFeedbackRequest feedback) => throw new NotImplementedException();
        public Task<SampleDeliveryTracking> GetLiveTrackingAsync(long sampleOrderId) => throw new NotImplementedException();
        public Task<(bool Success, string Message)> MarkAsPreparingAsync(long sampleOrderId, long partnerId) => throw new NotImplementedException();
    }
}
```

---

## 🌐 API ENDPOINTS

### **CLIENT-SIDE API (SampleTastingController.cs)**

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using CateringEcommerce.Domain.Interfaces.Sample;
using CateringEcommerce.Domain.Models.Sample;

namespace CateringEcommerce.API.Controllers.User
{
    [Authorize]
    [ApiController]
    [Route("api/user/sample-tasting")]
    public class SampleTastingController : ControllerBase
    {
        private readonly ISampleTastingService _sampleService;

        public SampleTastingController(ISampleTastingService sampleService)
        {
            _sampleService = sampleService;
        }

        private long GetUserId() => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        /// <summary>
        /// GET: Get sample-eligible items for a catering
        /// </summary>
        [HttpGet("eligible-items/{cateringId}")]
        public async Task<IActionResult> GetSampleEligibleItems(long cateringId)
        {
            var items = await _sampleService.GetSampleEligibleItemsAsync(cateringId);
            return Ok(new { success = true, data = items });
        }

        /// <summary>
        /// POST: Create sample order (PREPAID ONLY)
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreateSampleOrder([FromBody] CreateSampleOrderRequest request)
        {
            request.UserId = GetUserId();
            var result = await _sampleService.CreateSampleOrderAsync(request);

            if (result.Success)
            {
                return Ok(new { success = true, message = result.Message, sampleOrderId = result.OrderId });
            }

            return BadRequest(new { success = false, message = result.Message });
        }

        /// <summary>
        /// GET: Get my sample orders
        /// </summary>
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMySampleOrders()
        {
            var orders = await _sampleService.GetMySampleOrdersAsync(GetUserId());
            return Ok(new { success = true, data = orders });
        }

        /// <summary>
        /// GET: Get sample order details with live tracking
        /// </summary>
        [HttpGet("{sampleOrderId}")]
        public async Task<IActionResult> GetSampleOrderDetails(long sampleOrderId)
        {
            var order = await _sampleService.GetSampleOrderDetailsAsync(sampleOrderId);
            if (order == null)
            {
                return NotFound(new { success = false, message = "Sample order not found" });
            }

            return Ok(new { success = true, data = order });
        }

        /// <summary>
        /// GET: Get live tracking for sample delivery (LIKE SWIGGY/ZOMATO)
        /// </summary>
        [HttpGet("{sampleOrderId}/live-tracking")]
        public async Task<IActionResult> GetLiveTracking(long sampleOrderId)
        {
            var tracking = await _sampleService.GetLiveTrackingAsync(sampleOrderId);
            if (tracking == null)
            {
                return NotFound(new { success = false, message = "Tracking not available" });
            }

            return Ok(new { success = true, data = tracking });
        }

        /// <summary>
        /// POST: Submit feedback after delivery
        /// </summary>
        [HttpPost("{sampleOrderId}/feedback")]
        public async Task<IActionResult> SubmitFeedback(long sampleOrderId, [FromBody] SampleFeedbackRequest feedback)
        {
            var success = await _sampleService.SubmitFeedbackAsync(sampleOrderId, feedback);

            if (success)
            {
                return Ok(new { success = true, message = "Feedback submitted successfully" });
            }

            return BadRequest(new { success = false, message = "Failed to submit feedback" });
        }
    }
}
```

### **PARTNER-SIDE API (PartnerSampleTastingController.cs)**

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using CateringEcommerce.Domain.Interfaces.Sample;

namespace CateringEcommerce.API.Controllers.Owner
{
    [Authorize(Roles = "Owner")]
    [ApiController]
    [Route("api/owner/sample-tasting")]
    public class PartnerSampleTastingController : ControllerBase
    {
        private readonly ISampleTastingService _sampleService;

        public PartnerSampleTastingController(ISampleTastingService sampleService)
        {
            _sampleService = sampleService;
        }

        private long GetPartnerId() => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        /// <summary>
        /// GET: Get pending sample requests (REQUIRES ACTION)
        /// </summary>
        [HttpGet("pending-requests")]
        public async Task<IActionResult> GetPendingRequests()
        {
            var cateringId = GetPartnerId(); // TODO: Get from partner context
            var requests = await _sampleService.GetPendingRequestsAsync(cateringId);
            return Ok(new { success = true, data = requests });
        }

        /// <summary>
        /// POST: Accept sample request
        /// </summary>
        [HttpPost("{sampleOrderId}/accept")]
        public async Task<IActionResult> AcceptSampleRequest(long sampleOrderId)
        {
            var result = await _sampleService.AcceptSampleRequestAsync(sampleOrderId, GetPartnerId());

            if (result.Success)
            {
                return Ok(new { success = true, message = result.Message });
            }

            return BadRequest(new { success = false, message = result.Message });
        }

        /// <summary>
        /// POST: Reject sample request (AUTO-REFUND 100%)
        /// CRITICAL: Partner earns ₹0 on rejection
        /// </summary>
        [HttpPost("{sampleOrderId}/reject")]
        public async Task<IActionResult> RejectSampleRequest(long sampleOrderId, [FromBody] RejectSampleRequest request)
        {
            var result = await _sampleService.RejectSampleRequestAsync(sampleOrderId, GetPartnerId(), request.Reason);

            if (result.Success)
            {
                return Ok(new { success = true, message = result.Message });
            }

            return BadRequest(new { success = false, message = result.Message });
        }

        /// <summary>
        /// POST: Mark sample as preparing
        /// </summary>
        [HttpPost("{sampleOrderId}/mark-preparing")]
        public async Task<IActionResult> MarkAsPreparing(long sampleOrderId)
        {
            var result = await _sampleService.MarkAsPreparingAsync(sampleOrderId, GetPartnerId());

            if (result.Success)
            {
                return Ok(new { success = true, message = result.Message });
            }

            return BadRequest(new { success = false, message = result.Message });
        }

        /// <summary>
        /// POST: Request pickup (AUTOMATED DELIVERY)
        /// Calls third-party API: Dunzo/Porter/Shadowfax
        /// </summary>
        [HttpPost("{sampleOrderId}/request-pickup")]
        public async Task<IActionResult> RequestPickup(long sampleOrderId)
        {
            var result = await _sampleService.RequestPickupAsync(sampleOrderId, GetPartnerId());

            if (result.Success)
            {
                return Ok(new { success = true, message = result.Message });
            }

            return BadRequest(new { success = false, message = result.Message });
        }
    }

    public class RejectSampleRequest
    {
        public string Reason { get; set; }
    }
}
```

---

## 🎨 FRONTEND FLOW (CLIENT)

### **Step-by-Step UX Flow**

#### **1. Browse Catering & View Menu**

```
Component: CateringDetailPage.jsx

UI Elements:
- [Button] "Try Sample Tasting" (Prominent CTA)
- Shows sample pricing badge on menu items
```

#### **2. Sample Item Selection Modal**

```jsx
// Component: SampleTastingModal.jsx

<Modal title="Try Sample Tasting">
  {/* Config Info */}
  <Alert type="info">
    • Select up to 3 items to taste
    • Fixed sample quantity per item
    • Delivery with live tracking
  </Alert>

  {/* Menu Items Grid */}
  <SampleItemsGrid>
    {menuItems.map(item => (
      <SampleItemCard
        name={item.name}
        samplePrice={item.samplePrice}
        isVeg={item.isVeg}
        image={item.image}
        selected={selectedItems.includes(item.id)}
        onSelect={() => handleItemSelect(item.id)}
        disabled={selectedItems.length >= 3 && !selectedItems.includes(item.id)}
      />
    ))}
  </SampleItemsGrid>

  {/* Price Breakdown */}
  <PriceSummary>
    Items Total: ₹{itemsTotal}
    Delivery: ₹{deliveryCharge}
    Tax: ₹{taxAmount}
    ───────────
    Total: ₹{totalAmount}
  </PriceSummary>

  {/* CTA */}
  <Button
    onClick={handleProceedToPayment}
    disabled={selectedItems.length === 0}
  >
    Proceed to Payment
  </Button>
</Modal>
```

#### **3. Payment Flow**

```
- Razorpay integration (PREPAID ONLY)
- NO COD option
- On payment success:
  → Status: SAMPLE_REQUESTED
  → Notify partner
  → Redirect to order tracking page
```

#### **4. Order Tracking Page**

```jsx
// Component: SampleOrderTracking.jsx

<OrderTrackingPage>
  {/* Status Timeline */}
  <StatusTimeline>
    ✅ SAMPLE_REQUESTED - Payment successful
    ⏳ Awaiting partner approval...

    {/* If ACCEPTED */}
    ✅ SAMPLE_ACCEPTED
    ⏳ Partner is preparing your sample...

    {/* If REJECTED */}
    ❌ SAMPLE_REJECTED
    💰 Full refund initiated (₹{amount})
    Reason: {rejectionReason}
  </StatusTimeline>

  {/* Live Tracking (ONLY when IN_TRANSIT) */}
  {status === 'IN_TRANSIT' && (
    <LiveTrackingMap>
      <DeliveryPartnerCard
        name={partner.name}
        phone={partner.phone}
        vehicle={partner.vehicleNumber}
        eta={partner.eta}
      />
      <Map
        partnerLocation={[lat, lng]}
        deliveryLocation={[destLat, destLng]}
      />
    </LiveTrackingMap>
  )}

  {/* If DELIVERED */}
  {status === 'DELIVERED' && (
    <FeedbackSection>
      <Button onClick={openFeedbackModal}>
        Rate Your Sample Experience
      </Button>
    </FeedbackSection>
  )}
</OrderTrackingPage>
```

#### **5. Feedback Modal**

```jsx
// Component: SampleFeedbackModal.jsx

<Modal title="How was your sample?">
  <StarRating
    label="Taste"
    value={tasteRating}
    onChange={setTasteRating}
  />
  <StarRating
    label="Hygiene & Packaging"
    value={hygieneRating}
    onChange={setHygieneRating}
  />
  <StarRating
    label="Overall Experience"
    value={overallRating}
    onChange={setOverallRating}
  />
  <TextArea
    label="Additional Feedback"
    value={feedback}
    onChange={setFeedback}
  />

  <Button onClick={handleSubmitFeedback}>
    Submit Feedback
  </Button>
</Modal>
```

#### **6. Conversion CTA**

```
After feedback submission:

<ConversionBanner>
  <Icon>🎉</Icon>
  <Text>Loved the sample? Book your full event now!</Text>
  <Button size="large" onClick={redirectToEventBooking}>
    Proceed with Full Event Booking
  </Button>
  <Badge>Get 5% discount on your first event booking</Badge>
</ConversionBanner>
```

---

## 🏭 FRONTEND FLOW (PARTNER)

### **Partner Dashboard - Sample Requests Tab**

```jsx
// Component: PartnerSampleRequests.jsx

<DashboardTab title="Sample Tasting Requests">
  {/* Pending Requests (REQUIRES ACTION) */}
  <Section title="Pending Requests" badge={pendingCount}>
    <Table>
      <thead>
        <tr>
          <th>Order #</th>
          <th>Client</th>
          <th>Items</th>
          <th>Amount</th>
          <th>Requested At</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        {pendingRequests.map(request => (
          <tr key={request.sampleOrderId}>
            <td>{request.sampleOrderNumber}</td>
            <td>
              {request.clientName}<br/>
              {request.clientPhone}
            </td>
            <td>
              <Badge>{request.itemCount} items</Badge>
              <Button size="sm" onClick={() => viewItems(request.sampleOrderId)}>
                View Items
              </Button>
            </td>
            <td>₹{request.totalAmount}</td>
            <td>{formatDate(request.createdAt)}</td>
            <td>
              <ButtonGroup>
                <Button
                  variant="success"
                  onClick={() => handleAccept(request.sampleOrderId)}
                >
                  ✅ Accept
                </Button>
                <Button
                  variant="danger"
                  onClick={() => openRejectModal(request.sampleOrderId)}
                >
                  ❌ Reject
                </Button>
              </ButtonGroup>
            </td>
          </tr>
        ))}
      </tbody>
    </Table>
  </Section>

  {/* Accepted & In Progress */}
  <Section title="In Progress">
    {inProgressOrders.map(order => (
      <SampleOrderCard key={order.sampleOrderId}>
        <OrderInfo>
          Order #: {order.sampleOrderNumber}
          Status: <StatusBadge status={order.currentStatus} />
        </OrderInfo>

        {/* Action Buttons based on status */}
        {order.currentStatus === 'SAMPLE_ACCEPTED' && (
          <Button onClick={() => handleMarkPreparing(order.sampleOrderId)}>
            Mark as Preparing
          </Button>
        )}

        {order.currentStatus === 'SAMPLE_PREPARING' && (
          <Button
            variant="primary"
            size="large"
            onClick={() => handleRequestPickup(order.sampleOrderId)}
          >
            🚚 Request Pickup
          </Button>
        )}

        {order.currentStatus === 'IN_TRANSIT' && (
          <Alert type="info">
            Delivery in progress. Tracking ID: {order.deliveryTrackingId}
          </Alert>
        )}
      </SampleOrderCard>
    ))}
  </Section>
</DashboardTab>
```

### **Reject Modal**

```jsx
// Component: RejectSampleModal.jsx

<Modal title="Reject Sample Request">
  <Alert type="warning">
    ⚠️ Important:
    • Client will receive 100% refund
    • You will earn ₹0 from this order
    • This action cannot be undone
  </Alert>

  <TextArea
    label="Rejection Reason (Optional but Recommended)"
    placeholder="e.g., Out of stock, Kitchen closed today, etc."
    value={rejectionReason}
    onChange={setRejectionReason}
  />

  <ButtonGroup>
    <Button variant="secondary" onClick={closeModal}>
      Cancel
    </Button>
    <Button
      variant="danger"
      onClick={handleConfirmReject}
    >
      Confirm Rejection
    </Button>
  </ButtonGroup>
</Modal>
```

---

## 🚚 THIRD-PARTY DELIVERY INTEGRATION

### **ISampleDeliveryService.cs**

```csharp
using System.Threading.Tasks;

namespace CateringEcommerce.Domain.Interfaces.Sample
{
    public interface ISampleDeliveryService
    {
        Task<DeliveryRequestResult> RequestPickupAsync(long sampleOrderId);
        Task<DeliveryTrackingResult> GetLiveTrackingAsync(string externalTrackingId);
        Task UpdateDeliveryStatusAsync(string externalTrackingId, string status, decimal? lat, decimal? lng);
    }

    public class DeliveryRequestResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string TrackingId { get; set; }
        public int ProviderId { get; set; }
        public string TrackingUrl { get; set; }
        public string DeliveryPartnerName { get; set; }
        public string DeliveryPartnerPhone { get; set; }
    }

    public class DeliveryTrackingResult
    {
        public string Status { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string DeliveryPartnerName { get; set; }
        public string DeliveryPartnerPhone { get; set; }
        public string VehicleNumber { get; set; }
        public DateTime? EstimatedDeliveryAt { get; set; }
    }
}
```

### **Delivery Provider Integration (Dunzo Example)**

```csharp
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CateringEcommerce.Domain.Interfaces.Sample;

namespace CateringEcommerce.BAL.Services
{
    public class DunzoDeliveryService : ISampleDeliveryService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public DunzoDeliveryService(IConfiguration config)
        {
            _httpClient = new HttpClient();
            _apiKey = config["Dunzo:ApiKey"];
            _baseUrl = config["Dunzo:BaseUrl"];
        }

        public async Task<DeliveryRequestResult> RequestPickupAsync(long sampleOrderId)
        {
            try
            {
                // Get sample order details
                var order = await GetSampleOrderDetailsAsync(sampleOrderId);

                // Prepare Dunzo API request
                var requestPayload = new
                {
                    pickup_details = new
                    {
                        name = order.CateringName,
                        phone = order.CateringPhone,
                        address = order.PickupAddress,
                        latitude = order.PickupLatitude,
                        longitude = order.PickupLongitude
                    },
                    drop_details = new
                    {
                        name = order.ClientName,
                        phone = order.ClientPhone,
                        address = order.DeliveryAddress,
                        latitude = order.DeliveryLatitude,
                        longitude = order.DeliveryLongitude
                    },
                    order_details = new
                    {
                        order_id = order.SampleOrderNumber,
                        order_value = order.TotalAmount,
                        order_type = "SAMPLE_TASTING"
                    }
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(requestPayload),
                    Encoding.UTF8,
                    "application/json"
                );

                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
                var response = await _httpClient.PostAsync($"{_baseUrl}/v1/orders/create", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<DunzoOrderResponse>(responseData);

                    return new DeliveryRequestResult
                    {
                        Success = true,
                        TrackingId = result.tracking_id,
                        ProviderId = 1, // Dunzo
                        TrackingUrl = result.tracking_url,
                        DeliveryPartnerName = result.runner_name,
                        DeliveryPartnerPhone = result.runner_phone
                    };
                }

                return new DeliveryRequestResult
                {
                    Success = false,
                    Message = "Failed to create delivery request with Dunzo"
                };
            }
            catch (Exception ex)
            {
                return new DeliveryRequestResult
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        // Similar implementations for Porter and Shadowfax...
    }

    public class DunzoOrderResponse
    {
        public string tracking_id { get; set; }
        public string tracking_url { get; set; }
        public string runner_name { get; set; }
        public string runner_phone { get; set; }
        public string status { get; set; }
    }
}
```

---

## 🛡️ VALIDATION & ABUSE PREVENTION

### **Abuse Prevention Rules**

```csharp
public class SampleTastingAbusePreventionService
{
    private readonly ISampleOrderRepository _repo;

    // Rule 1: Max items per order (Configurable)
    public async Task<bool> ValidateMaxItemsAsync(int itemCount)
    {
        var config = await _repo.GetActiveConfigAsync();
        return itemCount <= config.MaxItemsAllowed;
    }

    // Rule 2: Max samples per user per month
    public async Task<bool> ValidateMonthlySampleLimitAsync(long userId)
    {
        var sampleCount = await _repo.GetUserSampleCountThisMonthAsync(userId);
        var config = await _repo.GetActiveConfigAsync();
        return sampleCount < 2; // Max 2 samples per month per user
    }

    // Rule 3: Cooldown period between samples from same caterer
    public async Task<bool> ValidateCooldownPeriodAsync(long userId, long cateringId)
    {
        var lastSampleDate = await _repo.GetLastSampleDateAsync(userId, cateringId);
        if (lastSampleDate == null) return true;

        var hoursSinceLastSample = (DateTime.Now - lastSampleDate.Value).TotalHours;
        return hoursSinceLastSample >= 24; // 24-hour cooldown
    }

    // Rule 4: Block users with excessive refunds
    public async Task<bool> ValidateRefundAbuseAsync(long userId)
    {
        var refundCount = await _repo.GetUserRefundCountAsync(userId);
        return refundCount < 3; // Max 3 refunds before block
    }

    // Rule 5: Fixed sample quantity (NO client control)
    public async Task<bool> ValidateSampleQuantityAsync(List<SampleOrderItem> items)
    {
        var config = await _repo.GetActiveConfigAsync();
        return items.All(i => i.SampleQuantity == config.FixedSampleQuantity);
    }
}
```

### **Frontend Validation**

```javascript
// utils/sampleTastingValidation.js

export const validateSampleOrder = async (selectedItems, userId, cateringId) => {
  const errors = [];

  // Rule 1: Max items
  if (selectedItems.length > 3) {
    errors.push('You can select maximum 3 items for sample tasting');
  }

  // Rule 2: Min items
  if (selectedItems.length === 0) {
    errors.push('Please select at least 1 item');
  }

  // Rule 3: Check eligibility (API call)
  const eligibility = await checkEligibility(userId, cateringId);
  if (!eligibility.isEligible) {
    errors.push(eligibility.message);
  }

  return {
    isValid: errors.length === 0,
    errors
  };
};

// API call to check abuse prevention
const checkEligibility = async (userId, cateringId) => {
  const response = await fetch('/api/user/sample-tasting/check-eligibility', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ userId, cateringId })
  });
  return response.json();
};
```

---

## 💳 PAYMENT INTEGRATION

### **Payment Flow (Razorpay)**

```csharp
// Payment webhook handler

[HttpPost("api/webhooks/razorpay/sample-payment")]
public async Task<IActionResult> HandleSamplePaymentWebhook([FromBody] RazorpayWebhookPayload payload)
{
    try
    {
        // Verify signature
        if (!_razorpayService.VerifyWebhookSignature(payload))
        {
            return Unauthorized();
        }

        var sampleOrderId = long.Parse(payload.payload.order.notes.sample_order_id);

        if (payload.event == "payment.captured")
        {
            // Payment successful
            await _sampleOrderRepo.UpdatePaymentStatusAsync(sampleOrderId, "PAID", DateTime.Now);

            // Notify partner
            var order = await _sampleOrderRepo.GetSampleOrderByIdAsync(sampleOrderId);
            await _notificationService.SendNewSampleRequestNotificationAsync(order.CateringId, sampleOrderId);
        }
        else if (payload.event == "payment.failed")
        {
            // Payment failed
            await _sampleOrderRepo.UpdatePaymentStatusAsync(sampleOrderId, "FAILED", null);
        }

        return Ok();
    }
    catch (Exception ex)
    {
        // Log error
        return StatusCode(500);
    }
}
```

### **Refund Processing (Partner Rejection)**

```csharp
public async Task ProcessSampleRefundAsync(long sampleOrderId, decimal refundAmount, string reason)
{
    try
    {
        var order = await _sampleOrderRepo.GetSampleOrderByIdAsync(sampleOrderId);

        // Call Razorpay refund API
        var refundResponse = await _razorpayClient.Payment
            .Fetch(order.PaymentGatewayTransactionId)
            .Refund(new Dictionary<string, object>
            {
                { "amount", (int)(refundAmount * 100) }, // Convert to paise
                { "speed", "normal" },
                { "notes", new Dictionary<string, object>
                    {
                        { "reason", reason },
                        { "sample_order_id", sampleOrderId }
                    }
                }
            });

        // Update refund status
        await _sampleOrderRepo.UpdateSampleOrderStatusAsync(
            sampleOrderId,
            SampleOrderStatus.REFUNDED,
            "SYSTEM",
            null,
            $"Refund processed: {refundResponse["id"]}"
        );

        await _sampleOrderRepo.UpdatePaymentStatusAsync(sampleOrderId, "REFUNDED", DateTime.Now);

        // Notify client
        await _notificationService.SendRefundProcessedNotificationAsync(order.UserId, sampleOrderId, refundAmount);
    }
    catch (Exception ex)
    {
        // Log error and retry
        throw;
    }
}
```

---

## 🔔 NOTIFICATION SYSTEM

### **Notification Triggers**

```csharp
public interface ISampleTastingNotificationService
{
    // CLIENT NOTIFICATIONS
    Task SendSampleRequestConfirmationAsync(long userId, long sampleOrderId);
    Task SendSampleAcceptedNotificationAsync(long userId, long sampleOrderId);
    Task SendSampleRejectedNotificationAsync(long userId, long sampleOrderId, string reason);
    Task SendSampleInTransitNotificationAsync(long userId, long sampleOrderId, string trackingUrl);
    Task SendSampleDeliveredNotificationAsync(long userId, long sampleOrderId);
    Task SendRefundProcessedNotificationAsync(long userId, long sampleOrderId, decimal amount);

    // PARTNER NOTIFICATIONS
    Task SendNewSampleRequestNotificationAsync(long cateringId, long sampleOrderId);
    Task SendPickupAssignedNotificationAsync(long cateringId, long sampleOrderId, string deliveryPartnerPhone);
}
```

### **Notification Templates**

```json
// sample_request_confirmation.json
{
  "title": "Sample Order Confirmed!",
  "body": "Your sample order {order_number} has been confirmed. Waiting for partner approval.",
  "type": "SAMPLE_REQUEST",
  "actions": ["VIEW_ORDER"]
}

// sample_accepted.json
{
  "title": "Sample Approved!",
  "body": "{catering_name} has accepted your sample request. Preparation in progress.",
  "type": "SAMPLE_ACCEPTED",
  "actions": ["TRACK_ORDER"]
}

// sample_rejected.json
{
  "title": "Sample Request Declined",
  "body": "{catering_name} couldn't fulfill your sample request. Full refund of ₹{amount} initiated.",
  "type": "SAMPLE_REJECTED",
  "actions": ["VIEW_REFUND"]
}

// sample_in_transit.json
{
  "title": "Sample on the way!",
  "body": "Your sample is out for delivery. Track live location.",
  "type": "SAMPLE_IN_TRANSIT",
  "actions": ["TRACK_LIVE"]
}

// sample_delivered.json
{
  "title": "Sample Delivered!",
  "body": "Your sample has been delivered. How was it? Rate your experience.",
  "type": "SAMPLE_DELIVERED",
  "actions": ["RATE_SAMPLE", "BOOK_EVENT"]
}

// partner_new_request.json
{
  "title": "New Sample Request",
  "body": "You have a new sample tasting request from {client_name} for {item_count} items.",
  "type": "PARTNER_SAMPLE_REQUEST",
  "actions": ["ACCEPT", "REJECT"]
}
```

---

## ✅ TESTING CHECKLIST

### **Backend Tests**

- [ ] Sample order creation with valid items
- [ ] Sample order creation exceeding max items (should fail)
- [ ] Sample order creation with blocked user (should fail)
- [ ] Sample pricing NEVER uses package price
- [ ] Partner acceptance updates status correctly
- [ ] Partner rejection triggers 100% auto-refund
- [ ] Delivery API integration (mock)
- [ ] Live tracking updates correctly
- [ ] Feedback submission stores ratings
- [ ] Conversion tracking links to event order
- [ ] Abuse prevention: max samples per month
- [ ] Abuse prevention: cooldown period
- [ ] Status history audit trail

### **Frontend Tests**

- [ ] Sample item selection (max 3 items)
- [ ] Sample item selection shows per-item pricing
- [ ] Payment flow (Razorpay)
- [ ] Order tracking page shows correct status
- [ ] Live tracking map displays delivery partner location
- [ ] Feedback modal submission
- [ ] Conversion CTA after feedback
- [ ] Partner pending requests list
- [ ] Partner accept/reject actions
- [ ] Partner request pickup button

### **Integration Tests**

- [ ] End-to-end: Client creates order → Partner accepts → Delivery → Feedback
- [ ] End-to-end: Client creates order → Partner rejects → Refund
- [ ] Payment webhook triggers notifications
- [ ] Delivery webhook updates tracking
- [ ] Refund processes correctly

---

## 🚀 DEPLOYMENT CHECKLIST

### **Database**

- [ ] Run `Sample_Tasting_Complete_Schema.sql`
- [ ] Verify all tables created
- [ ] Insert initial configuration
- [ ] Set up sample pricing for menu items

### **Backend**

- [ ] Register services in `Program.cs`:
  ```csharp
  services.AddScoped<ISampleOrderRepository, SampleOrderRepository>();
  services.AddScoped<ISampleTastingService, SampleTastingService>();
  services.AddScoped<ISampleDeliveryService, DunzoDeliveryService>();
  ```
- [ ] Configure delivery provider API keys
- [ ] Set up Razorpay webhooks
- [ ] Test all API endpoints

### **Frontend**

- [ ] Create sample tasting components
- [ ] Integrate payment gateway
- [ ] Implement live tracking map (Google Maps / Mapbox)
- [ ] Test on mobile devices
- [ ] Deploy to staging

### **Third-Party Integrations**

- [ ] Dunzo API key & webhook setup
- [ ] Porter API key & webhook setup
- [ ] Shadowfax API key & webhook setup
- [ ] Razorpay webhook configuration
- [ ] Test delivery API in sandbox

---

## 📄 SUMMARY

This implementation follows the EXACT specifications provided:

✅ **Sample pricing is per-item (NEVER package-based)**
✅ **Always prepaid (NO COD)**
✅ **Max 2-3 items (configurable)**
✅ **Fixed sample quantity**
✅ **Partner accept/reject with auto-refund**
✅ **Automated third-party delivery**
✅ **Live tracking (like Swiggy/Zomato)**
✅ **Direct conversion to full event booking**
✅ **Abuse prevention built-in**
✅ **Clear separation from event orders**

**Next Steps:**
1. Review this specification
2. Implement backend repositories & services
3. Create API endpoints
4. Build frontend components
5. Integrate delivery APIs
6. Test end-to-end
7. Deploy to production

---

**END OF SPECIFICATION**
