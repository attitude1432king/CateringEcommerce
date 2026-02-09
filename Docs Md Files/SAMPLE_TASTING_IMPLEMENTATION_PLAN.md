# 🍽️ SAMPLE TASTING IMPLEMENTATION - COMPLETE SPECIFICATION

**Date**: February 7, 2026
**Status**: Ready for Implementation
**Scope**: LOCKED - No Redesign Allowed

---

## 🎯 BUSINESS REQUIREMENTS (LOCKED)

### Purpose
- Build customer trust through paid tasting
- Validate taste & hygiene before large events
- Drive conversion to full event bookings
- Prevent abuse and free tasting exploitation

### Key Constraints
- ✅ Paid service (no free samples)
- ✅ Limited scope (2-3 items max)
- ✅ Third-party delivery only
- ✅ Live tracking mandatory
- ✅ Clear conversion path to full booking

---

## 📊 DATABASE SCHEMA

### 1. Sample_Orders Table
```sql
CREATE TABLE Sample_Orders (
    SampleOrderID BIGINT PRIMARY KEY IDENTITY(1,1),
    UserID BIGINT NOT NULL,
    CateringID BIGINT NOT NULL,

    -- Pricing
    SamplePriceTotal DECIMAL(10,2) NOT NULL,
    DeliveryCharge DECIMAL(10,2) DEFAULT 0,
    TotalAmount DECIMAL(10,2) NOT NULL,

    -- Status
    Status VARCHAR(50) NOT NULL, -- SAMPLE_REQUESTED, SAMPLE_ACCEPTED, etc.

    -- Addresses
    DeliveryAddressID BIGINT NOT NULL,
    PickupAddress NVARCHAR(500) NOT NULL, -- Partner kitchen address

    -- Payment
    PaymentID BIGINT NULL,
    PaymentStatus VARCHAR(50) DEFAULT 'PENDING',
    IsPaid BIT DEFAULT 0,

    -- Partner Response
    PartnerResponseDate DATETIME NULL,
    RejectionReason NVARCHAR(500) NULL,

    -- Delivery Tracking
    DeliveryPartner VARCHAR(50) NULL, -- 'DUNZO', 'PORTER', 'SHADOWFAX'
    DeliveryPartnerOrderID VARCHAR(100) NULL,
    DeliveryPartnerName VARCHAR(200) NULL,
    DeliveryPartnerPhone VARCHAR(20) NULL,
    EstimatedDeliveryTime DATETIME NULL,
    ActualDeliveryTime DATETIME NULL,

    -- Feedback & Conversion
    ClientFeedback NVARCHAR(1000) NULL,
    TasteRating INT NULL, -- 1-5
    HygieneRating INT NULL, -- 1-5
    ConvertedToEventOrder BIT DEFAULT 0,
    EventOrderID BIGINT NULL,

    -- Audit
    CreatedDate DATETIME DEFAULT GETDATE(),
    LastModifiedDate DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (UserID) REFERENCES Users(PkID),
    FOREIGN KEY (CateringID) REFERENCES Owner(PkID),
    FOREIGN KEY (DeliveryAddressID) REFERENCES User_Addresses(PkID),
    FOREIGN KEY (EventOrderID) REFERENCES Orders(PkID)
);

CREATE INDEX IX_Sample_Orders_UserID ON Sample_Orders(UserID);
CREATE INDEX IX_Sample_Orders_CateringID ON Sample_Orders(CateringID);
CREATE INDEX IX_Sample_Orders_Status ON Sample_Orders(Status);
CREATE INDEX IX_Sample_Orders_CreatedDate ON Sample_Orders(CreatedDate DESC);
```

### 2. Sample_Order_Items Table
```sql
CREATE TABLE Sample_Order_Items (
    SampleItemID BIGINT PRIMARY KEY IDENTITY(1,1),
    SampleOrderID BIGINT NOT NULL,

    -- Item Reference
    MenuItemID BIGINT NOT NULL,
    MenuItemName NVARCHAR(200) NOT NULL,

    -- Sample Pricing (PER ITEM - NOT FROM PACKAGE)
    SamplePrice DECIMAL(10,2) NOT NULL,

    -- Fixed Quantity (No Client Control)
    SampleQuantity INT DEFAULT 1 NOT NULL,

    -- Item Details (Snapshot)
    Category VARCHAR(100) NULL,
    Description NVARCHAR(500) NULL,
    ImageUrl NVARCHAR(500) NULL,

    -- Source Tracking
    IsFromPackage BIT DEFAULT 0,
    PackageID BIGINT NULL,

    CreatedDate DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (SampleOrderID) REFERENCES Sample_Orders(SampleOrderID) ON DELETE CASCADE,
    FOREIGN KEY (MenuItemID) REFERENCES FoodItems(PkID)
);

CREATE INDEX IX_Sample_Order_Items_SampleOrderID ON Sample_Order_Items(SampleOrderID);
```

### 3. Sample_Delivery_Tracking Table
```sql
CREATE TABLE Sample_Delivery_Tracking (
    TrackingID BIGINT PRIMARY KEY IDENTITY(1,1),
    SampleOrderID BIGINT NOT NULL,

    -- Live Tracking Data
    DeliveryStatus VARCHAR(50) NOT NULL, -- PICKUP_ASSIGNED, PICKED_UP, IN_TRANSIT, DELIVERED
    Latitude DECIMAL(10,8) NULL,
    Longitude DECIMAL(11,8) NULL,

    -- Partner Info
    PartnerName VARCHAR(200) NULL,
    PartnerPhone VARCHAR(20) NULL,
    VehicleNumber VARCHAR(50) NULL,

    -- ETA
    EstimatedArrival DATETIME NULL,

    -- Status Update
    StatusMessage NVARCHAR(500) NULL,
    Timestamp DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (SampleOrderID) REFERENCES Sample_Orders(SampleOrderID) ON DELETE CASCADE
);

CREATE INDEX IX_Sample_Delivery_Tracking_SampleOrderID ON Sample_Delivery_Tracking(SampleOrderID);
CREATE INDEX IX_Sample_Delivery_Tracking_Timestamp ON Sample_Delivery_Tracking(Timestamp DESC);
```

### 4. Sample_Refunds Table
```sql
CREATE TABLE Sample_Refunds (
    RefundID BIGINT PRIMARY KEY IDENTITY(1,1),
    SampleOrderID BIGINT NOT NULL,

    RefundAmount DECIMAL(10,2) NOT NULL,
    RefundReason VARCHAR(50) NOT NULL, -- 'PARTNER_REJECTED', 'DELIVERY_FAILED', etc.
    RefundStatus VARCHAR(50) DEFAULT 'PENDING',

    PaymentGatewayRefundID VARCHAR(100) NULL,
    RefundInitiatedDate DATETIME DEFAULT GETDATE(),
    RefundCompletedDate DATETIME NULL,

    Notes NVARCHAR(500) NULL,

    FOREIGN KEY (SampleOrderID) REFERENCES Sample_Orders(SampleOrderID)
);

CREATE INDEX IX_Sample_Refunds_SampleOrderID ON Sample_Refunds(SampleOrderID);
```

### 5. Menu_Item_Sample_Pricing Table
```sql
CREATE TABLE Menu_Item_Sample_Pricing (
    ID BIGINT PRIMARY KEY IDENTITY(1,1),
    MenuItemID BIGINT NOT NULL,

    -- Sample-Specific Pricing (NOT derived from package)
    SamplePrice DECIMAL(10,2) NOT NULL,
    SampleQuantity INT DEFAULT 1, -- Fixed sample size

    IsAvailableForSample BIT DEFAULT 1,

    CreatedDate DATETIME DEFAULT GETDATE(),
    LastModifiedDate DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (MenuItemID) REFERENCES FoodItems(PkID) ON DELETE CASCADE,

    CONSTRAINT UQ_MenuItem_Sample UNIQUE(MenuItemID)
);

CREATE INDEX IX_Menu_Item_Sample_Pricing_MenuItemID ON Menu_Item_Sample_Pricing(MenuItemID);
```

### 6. System_Sample_Configuration Table
```sql
CREATE TABLE System_Sample_Configuration (
    ConfigID INT PRIMARY KEY IDENTITY(1,1),

    -- Item Limits
    MaxSampleItemsAllowed INT DEFAULT 3,
    MinSampleItemsRequired INT DEFAULT 1,

    -- Pricing Options
    PricingModel VARCHAR(50) DEFAULT 'PER_ITEM', -- 'PER_ITEM' or 'FIXED_FEE'
    FixedSampleFee DECIMAL(10,2) NULL,

    -- Delivery
    DefaultDeliveryProvider VARCHAR(50) DEFAULT 'DUNZO',
    EnableLiveTracking BIT DEFAULT 1,

    -- Business Rules
    RequirePartnerApproval BIT DEFAULT 1,
    AutoRejectAfterHours INT DEFAULT 24,

    -- Conversion
    ShowConversionCTAAfterDelivery BIT DEFAULT 1,

    IsActive BIT DEFAULT 1,
    LastModifiedDate DATETIME DEFAULT GETDATE()
);

-- Insert default configuration
INSERT INTO System_Sample_Configuration (MaxSampleItemsAllowed, PricingModel)
VALUES (3, 'PER_ITEM');
```

---

## 🎨 DOMAIN MODELS & ENUMS

### SampleOrderStatus Enum
```csharp
public enum SampleOrderStatus
{
    SAMPLE_REQUESTED = 1,
    SAMPLE_ACCEPTED = 2,
    SAMPLE_REJECTED = 3,
    SAMPLE_PREPARING = 4,
    READY_FOR_PICKUP = 5,
    IN_TRANSIT = 6,
    DELIVERED = 7,
    REFUNDED = 8
}
```

### DeliveryStatus Enum
```csharp
public enum SampleDeliveryStatus
{
    PICKUP_ASSIGNED = 1,
    PICKED_UP = 2,
    IN_TRANSIT = 3,
    DELIVERED = 4,
    FAILED = 5
}
```

### DeliveryProvider Enum
```csharp
public enum DeliveryProvider
{
    DUNZO = 1,
    PORTER = 2,
    SHADOWFAX = 3
}
```

### SamplePricingModel Enum
```csharp
public enum SamplePricingModel
{
    PER_ITEM = 1,
    FIXED_FEE = 2
}
```

---

## 🔄 STATUS FLOW DIAGRAM

```
CLIENT INITIATES:
[SAMPLE_REQUESTED] → (Client pays upfront)
         ↓
PARTNER DECISION:
    ↙          ↘
[REJECTED]   [ACCEPTED]
(Auto-refund)     ↓
                  ↓
[SAMPLE_PREPARING] → Partner prepares
         ↓
[READY_FOR_PICKUP] → Partner requests pickup
         ↓
DELIVERY FLOW:
[PICKUP_ASSIGNED] → Delivery partner assigned
         ↓
[PICKED_UP] → Partner hands over
         ↓
[IN_TRANSIT] → Live tracking active
         ↓
[DELIVERED] → Client receives & rates
         ↓
CLIENT ACTION:
→ Provide feedback
→ Convert to full event booking
```

---

## 🚀 API ENDPOINTS

### Client APIs

#### 1. Get Available Sample Items
```
GET /api/User/Sample/available-items/{cateringId}
Response: List of menu items with sample pricing
```

#### 2. Create Sample Order
```
POST /api/User/Sample/create
Request: {
  cateringId,
  itemIds[],
  deliveryAddressId
}
Response: {sampleOrderId, totalAmount, paymentLink}
```

#### 3. Get Sample Order Details
```
GET /api/User/Sample/{sampleOrderId}
Response: Order details, items, status, tracking
```

#### 4. Track Delivery
```
GET /api/User/Sample/track/{sampleOrderId}
Response: Live tracking data, ETA, partner info
```

#### 5. Submit Feedback
```
POST /api/User/Sample/feedback
Request: {sampleOrderId, tasteRating, hygieneRating, comments}
```

#### 6. Convert to Event Booking
```
POST /api/User/Sample/convert-to-event
Request: {sampleOrderId, eventDetails}
Response: Event order created
```

### Partner APIs

#### 1. Get Pending Sample Requests
```
GET /api/Owner/Sample/pending
Response: List of SAMPLE_REQUESTED orders
```

#### 2. Accept Sample Request
```
POST /api/Owner/Sample/accept/{sampleOrderId}
Response: Status updated to SAMPLE_ACCEPTED
```

#### 3. Reject Sample Request
```
POST /api/Owner/Sample/reject
Request: {sampleOrderId, rejectionReason}
Response: Auto-refund initiated
```

#### 4. Update Sample Status
```
PUT /api/Owner/Sample/update-status
Request: {sampleOrderId, newStatus}
Allowed: PREPARING, READY_FOR_PICKUP
```

#### 5. Request Pickup
```
POST /api/Owner/Sample/request-pickup/{sampleOrderId}
Response: Delivery partner assigned, tracking started
```

### Admin APIs

#### 1. Monitor Sample Orders
```
GET /api/Admin/Sample/all
Query: status, dateRange, cateringId
```

#### 2. Configure Sample Settings
```
PUT /api/Admin/Sample/configuration
Request: {maxItems, pricingModel, deliveryProvider}
```

---

## 🛡️ VALIDATION & ABUSE PREVENTION

### Client-Side Validation
```csharp
public class SampleOrderValidation
{
    // Rule 1: Max items allowed
    if (itemIds.Count > config.MaxSampleItemsAllowed)
        return Error("Maximum {config.MaxSampleItemsAllowed} items allowed");

    // Rule 2: Check if items are sample-eligible
    foreach (var itemId in itemIds)
    {
        if (!IsSampleEligible(itemId))
            return Error("Item not available for sampling");
    }

    // Rule 3: One active sample per user per catering
    if (HasActiveSampleOrder(userId, cateringId))
        return Error("You already have an active sample order");

    // Rule 4: Prevent rapid successive orders (cooldown)
    if (HasRecentSampleOrder(userId, hours: 24))
        return Error("Please wait before requesting another sample");
}
```

### Pricing Validation
```csharp
public class SamplePricingValidation
{
    // CRITICAL: Never use package price
    public decimal CalculateSamplePrice(List<long> itemIds)
    {
        decimal total = 0;

        foreach (var itemId in itemIds)
        {
            var samplePrice = GetItemSamplePrice(itemId);

            if (samplePrice == null)
                throw new Exception($"Item {itemId} has no sample pricing");

            // MUST use sample-specific price, NOT package price
            total += samplePrice.SamplePrice;
        }

        return total;
    }
}
```

---

## 📱 FRONTEND UX FLOW

### Client Flow

#### Step 1: Browse Catering → "Try Sample" CTA
```jsx
<CateringCard>
  <button onClick={openSampleModal}>
    🍽️ Try Sample Tasting (₹{samplePrice})
  </button>
</CateringCard>
```

#### Step 2: Sample Selection Modal
```jsx
<SampleSelectionModal>
  <h3>Select items for tasting (Max {maxItems})</h3>
  {menuItems.map(item => (
    <SampleItem
      item={item}
      samplePrice={item.samplePrice}
      isSelected={selectedItems.includes(item.id)}
      onToggle={handleToggle}
      disabled={selectedItems.length >= maxItems && !isSelected}
    />
  ))}

  <PricingSummary>
    Items: ₹{itemsTotal}
    Delivery: ₹{deliveryCharge}
    Total: ₹{total}
  </PricingSummary>

  <button onClick={proceedToPayment}>
    Pay ₹{total} & Request Sample
  </button>
</SampleSelectionModal>
```

#### Step 3: Payment
```jsx
<PaymentGateway
  amount={total}
  orderId={sampleOrderId}
  onSuccess={showTrackingPage}
/>
```

#### Step 4: Live Tracking
```jsx
<SampleTrackingPage>
  <OrderStatus status={currentStatus} />

  {status === 'IN_TRANSIT' && (
    <LiveMap
      deliveryPartnerLocation={liveLocation}
      destination={userAddress}
      eta={estimatedArrival}
    />
  )}

  <DeliveryPartnerCard
    name={partnerName}
    phone={partnerPhone}
    vehicle={vehicleNumber}
  />
</SampleTrackingPage>
```

#### Step 5: Post-Delivery Feedback
```jsx
<FeedbackModal>
  <RatingStars label="Taste Quality" onChange={setTasteRating} />
  <RatingStars label="Hygiene" onChange={setHygieneRating} />
  <textarea placeholder="Your feedback..." />

  <button onClick={submitFeedback}>Submit Feedback</button>

  {feedbackSubmitted && (
    <ConversionCTA>
      <h3>Loved the food? Book for your event!</h3>
      <button onClick={proceedToEventBooking}>
        Book Full Event →
      </button>
    </ConversionCTA>
  )}
</FeedbackModal>
```

### Partner Flow

#### Partner Dashboard - Pending Samples
```jsx
<PartnerSampleRequests>
  {pendingRequests.map(request => (
    <SampleRequestCard
      orderId={request.id}
      customerName={request.customerName}
      items={request.items}
      amount={request.amount}
      deliveryAddress={request.address}
    >
      <button onClick={() => acceptSample(request.id)}>
        ✅ Accept
      </button>
      <button onClick={() => openRejectModal(request.id)}>
        ❌ Reject
      </button>
    </SampleRequestCard>
  ))}
</PartnerSampleRequests>
```

#### Reject Modal
```jsx
<RejectSampleModal>
  <select onChange={setRejectionReason}>
    <option>Items not available</option>
    <option>Outside delivery area</option>
    <option>High demand - cannot accommodate</option>
  </select>

  <p className="warning">
    ⚠️ Customer will be refunded 100%
  </p>

  <button onClick={confirmReject}>Confirm Rejection</button>
</RejectSampleModal>
```

#### Accepted Sample - Status Updates
```jsx
<AcceptedSampleCard>
  <StatusProgress currentStatus={status} />

  {status === 'ACCEPTED' && (
    <button onClick={markAsPreparing}>
      👨‍🍳 Mark as Preparing
    </button>
  )}

  {status === 'PREPARING' && (
    <button onClick={requestPickup}>
      📦 Ready for Pickup
    </button>
  )}

  {status === 'READY_FOR_PICKUP' && (
    <DeliveryPartnerAssignment
      partner={deliveryPartner}
      eta={pickupETA}
    />
  )}
</AcceptedSampleCard>
```

---

## 🚚 DELIVERY INTEGRATION

### Third-Party Delivery Service Interface
```csharp
public interface IDeliveryService
{
    Task<DeliveryOrderResponse> CreatePickupRequest(
        DeliveryOrderRequest request);

    Task<DeliveryTrackingInfo> GetLiveTracking(
        string deliveryOrderId);

    Task<bool> CancelDelivery(string deliveryOrderId);
}
```

### Dunzo Implementation
```csharp
public class DunzoDeliveryService : IDeliveryService
{
    public async Task<DeliveryOrderResponse> CreatePickupRequest(
        DeliveryOrderRequest request)
    {
        var dunzoRequest = new
        {
            pickup = new
            {
                address = request.PickupAddress,
                contact = request.PartnerPhone
            },
            drop = new
            {
                address = request.DeliveryAddress,
                contact = request.CustomerPhone
            },
            item_description = "Sample Tasting Order",
            is_live_tracking = true
        };

        var response = await _httpClient.PostAsJsonAsync(
            "/v1/orders", dunzoRequest);

        return await response.Content
            .ReadFromJsonAsync<DeliveryOrderResponse>();
    }
}
```

---

## ✅ IMPLEMENTATION CHECKLIST

### Database Layer
- [ ] Create all 6 tables with proper indexes
- [ ] Add foreign key constraints
- [ ] Create stored procedures for complex queries
- [ ] Add audit triggers

### Domain Layer
- [ ] Create all enums
- [ ] Create domain models
- [ ] Add validation attributes
- [ ] Create DTOs for API

### Repository Layer
- [ ] ISampleOrderRepository
- [ ] ISampleDeliveryTrackingRepository
- [ ] ISamplePricingRepository
- [ ] IDeliveryServiceRepository

### Service Layer
- [ ] SampleOrderService (business logic)
- [ ] SamplePricingService (validation)
- [ ] DeliveryIntegrationService (3rd party)
- [ ] SampleNotificationService (alerts)

### API Layer
- [ ] User/SampleController (6 endpoints)
- [ ] Owner/SampleController (5 endpoints)
- [ ] Admin/SampleController (2 endpoints)

### Frontend Layer
- [ ] Client sample selection UI
- [ ] Live tracking page
- [ ] Feedback & conversion flow
- [ ] Partner sample management dashboard

### Integration
- [ ] Payment gateway integration
- [ ] Delivery API integration (Dunzo/Porter)
- [ ] Real-time tracking with SignalR
- [ ] Notification system (email/SMS/push)

### Testing
- [ ] Unit tests for pricing logic
- [ ] Integration tests for status flow
- [ ] E2E tests for complete journey
- [ ] Load testing for abuse scenarios

---

## 🎯 KEY SUCCESS METRICS

1. **Conversion Rate**: Sample → Full Event Booking
2. **Rejection Rate**: Partner rejection frequency
3. **Delivery Success**: On-time delivery rate
4. **Customer Satisfaction**: Average taste + hygiene rating
5. **Abuse Prevention**: Blocked repeated sample requests

---

**IMPLEMENTATION STATUS**: Ready to Begin
**LOCKED SPEC**: Do NOT modify requirements
**NEXT STEP**: Begin database migration creation
