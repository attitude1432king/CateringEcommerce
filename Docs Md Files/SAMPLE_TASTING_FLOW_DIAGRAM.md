# 🎯 SAMPLE TASTING - COMPLETE SYSTEM FLOW DIAGRAM

**Visual representation of the entire sample tasting flow**

---

## 📊 HIGH-LEVEL SYSTEM ARCHITECTURE

```
┌─────────────────────────────────────────────────────────────────────┐
│                          CLIENT SIDE                                 │
│  ┌────────────┐  ┌────────────┐  ┌────────────┐  ┌────────────┐   │
│  │  Browse    │→ │  Select    │→ │  Payment   │→ │  Track     │   │
│  │  Catering  │  │  Items     │  │  (Prepaid) │  │  Order     │   │
│  └────────────┘  └────────────┘  └────────────┘  └────────────┘   │
│                                                           ↓          │
│                                                    ┌────────────┐   │
│                                                    │  Feedback  │   │
│                                                    │  & Convert │   │
│                                                    └────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
                                    ↓
                          ┌──────────────────┐
                          │  PAYMENT GATEWAY │
                          │    (Razorpay)    │
                          └──────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────┐
│                         PARTNER SIDE                                 │
│  ┌────────────┐  ┌────────────┐  ┌────────────┐  ┌────────────┐   │
│  │  Receive   │→ │  Accept OR │→ │  Prepare   │→ │  Request   │   │
│  │  Request   │  │  Reject    │  │  Sample    │  │  Pickup    │   │
│  └────────────┘  └────────────┘  └────────────┘  └────────────┘   │
│                         ↓                                            │
│                  ┌────────────┐                                      │
│                  │ AUTO-REFUND│  (If rejected)                       │
│                  └────────────┘                                      │
└─────────────────────────────────────────────────────────────────────┘
                                    ↓
                     ┌──────────────────────────────┐
                     │  THIRD-PARTY DELIVERY APIs   │
                     │  Dunzo / Porter / Shadowfax  │
                     └──────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────┐
│                      DELIVERY TRACKING                               │
│  ┌────────────┐  ┌────────────┐  ┌────────────┐  ┌────────────┐   │
│  │  Pickup    │→ │  Picked Up │→ │ In Transit │→ │ Delivered  │   │
│  │  Assigned  │  │            │  │ (Live GPS) │  │            │   │
│  └────────────┘  └────────────┘  └────────────┘  └────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 🔄 DETAILED CLIENT FLOW

### **Phase 1: Discovery & Selection**

```
┌─────────────────────────────────────────────────────────────────┐
│                    CLIENT: Browse Catering                      │
└─────────────────────────────────────────────────────────────────┘
                                ↓
                    ┌───────────────────────┐
                    │  View Menu / Packages │
                    └───────────────────────┘
                                ↓
                    ┌───────────────────────┐
                    │ See "Try Sample" CTA  │
                    └───────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│               SAMPLE SELECTION MODAL OPENS                      │
│                                                                 │
│  • Max 3 items allowed                                          │
│  • Each item shows: SamplePrice (NOT package price)            │
│  • Fixed quantity per item (e.g., 250g)                        │
│  • Client cannot change quantity                               │
│                                                                 │
│  Price Breakdown:                                               │
│    Items Total:    ₹450                                         │
│    Delivery:       ₹50                                          │
│    Tax (5%):       ₹22.50                                       │
│    ───────────────────                                          │
│    TOTAL:          ₹522.50                                      │
│                                                                 │
│    [Proceed to Payment] Button                                  │
└─────────────────────────────────────────────────────────────────┘
```

### **Phase 2: Payment**

```
                    ┌───────────────────────┐
                    │  Click "Proceed"      │
                    └───────────────────────┘
                                ↓
                    ┌───────────────────────┐
                    │  Razorpay Modal Opens │
                    │  (PREPAID ONLY)       │
                    │  • Debit/Credit Card  │
                    │  • UPI                │
                    │  • Net Banking        │
                    │  • Wallet             │
                    │  ❌ NO COD            │
                    └───────────────────────┘
                                ↓
                    ┌───────────────────────┐
                    │  Payment Success      │
                    └───────────────────────┘
                                ↓
                    ┌───────────────────────┐
                    │  Sample Order Created │
                    │  Status: REQUESTED    │
                    │  Order #: SMPL-20260207-0001 │
                    └───────────────────────┘
                                ↓
                    ┌───────────────────────┐
                    │  Redirect to Tracking │
                    └───────────────────────┘
```

### **Phase 3: Tracking**

```
┌─────────────────────────────────────────────────────────────────┐
│                    ORDER TRACKING PAGE                          │
│                                                                 │
│  Order #: SMPL-20260207-0001                                    │
│  Status Timeline:                                               │
│                                                                 │
│  ✅ SAMPLE_REQUESTED       (12:30 PM)                          │
│     Payment successful ₹522.50                                  │
│                                                                 │
│  ⏳ Awaiting partner approval...                               │
│     Expected response within 2 hours                            │
│                                                                 │
│  ─────────────────────────────────────────────────────────     │
│                                                                 │
│  IF ACCEPTED:                                                   │
│                                                                 │
│  ✅ SAMPLE_ACCEPTED        (12:45 PM)                          │
│     Rajesh Caterers accepted your request                       │
│                                                                 │
│  ⏳ SAMPLE_PREPARING                                           │
│     Your sample is being prepared...                            │
│                                                                 │
│  ✅ READY_FOR_PICKUP       (1:30 PM)                           │
│     Delivery partner assigned                                   │
│                                                                 │
│  ✅ IN_TRANSIT             (1:45 PM)                           │
│     ┌─────────────────────────────────────┐                   │
│     │    LIVE TRACKING (Like Swiggy)      │                   │
│     │                                     │                   │
│     │  📍 Delivery Partner: Rahul         │                   │
│     │  📞 Phone: +91 98765 43210          │                   │
│     │  🚗 Vehicle: MH12AB1234             │                   │
│     │  ⏰ ETA: 15 minutes                 │                   │
│     │                                     │                   │
│     │  [Google Map with live location]    │                   │
│     │  • Partner's live GPS location      │                   │
│     │  • Your delivery address            │                   │
│     │  • Route path                       │                   │
│     └─────────────────────────────────────┘                   │
│                                                                 │
│  ✅ DELIVERED              (2:00 PM)                           │
│     Sample delivered successfully!                              │
│                                                                 │
│     [Rate Your Sample Experience] Button                        │
│                                                                 │
│  ─────────────────────────────────────────────────────────     │
│                                                                 │
│  IF REJECTED:                                                   │
│                                                                 │
│  ❌ SAMPLE_REJECTED        (12:45 PM)                          │
│     Rajesh Caterers declined your request                       │
│     Reason: Kitchen closed for maintenance                      │
│                                                                 │
│  💰 REFUNDED               (12:46 PM)                          │
│     Full refund of ₹522.50 initiated                           │
│     Expected in 3-5 business days                               │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### **Phase 4: Feedback & Conversion**

```
                    ┌───────────────────────┐
                    │  Click "Rate Sample"  │
                    └───────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│                    FEEDBACK MODAL                               │
│                                                                 │
│  How was your sample?                                           │
│                                                                 │
│  Taste:              ⭐⭐⭐⭐⭐                                  │
│  Hygiene & Packaging: ⭐⭐⭐⭐☆                                  │
│  Overall Experience:  ⭐⭐⭐⭐⭐                                  │
│                                                                 │
│  Additional Feedback:                                           │
│  ┌─────────────────────────────────────────┐                  │
│  │ Excellent taste! Very fresh ingredients  │                  │
│  │ and well packaged.                       │                  │
│  └─────────────────────────────────────────┘                  │
│                                                                 │
│             [Submit Feedback] Button                            │
└─────────────────────────────────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│                   CONVERSION CTA                                │
│                                                                 │
│   🎉 Loved the sample? Book your full event now!                │
│                                                                 │
│   ┌─────────────────────────────────────────────────────────┐ │
│   │  [Proceed with Full Event Booking] (Large Button)       │ │
│   └─────────────────────────────────────────────────────────┘ │
│                                                                 │
│   🎁 Get 5% discount on your first event booking                │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🏭 DETAILED PARTNER FLOW

### **Phase 1: Receive Request**

```
┌─────────────────────────────────────────────────────────────────┐
│              PARTNER DASHBOARD - NOTIFICATIONS                  │
│                                                                 │
│  🔔 New Sample Request (1)                                      │
│                                                                 │
│  Click notification →                                           │
└─────────────────────────────────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│              SAMPLE TASTING REQUESTS TAB                        │
│                                                                 │
│  Pending Requests (1)                                           │
│                                                                 │
│  ┌──────────────────────────────────────────────────────────┐ │
│  │ Order #: SMPL-20260207-0001                              │ │
│  │ Client: Priya Sharma                                     │ │
│  │ Phone: +91 98765 43210                                   │ │
│  │ Items: 3 items                                           │ │
│  │   • Paneer Tikka (₹150)                                 │ │
│  │   • Veg Biryani (₹200)                                  │ │
│  │   • Gulab Jamun (₹100)                                  │ │
│  │ Delivery: Andheri West, Mumbai                           │ │
│  │ Amount: ₹522.50 (Already Paid)                          │ │
│  │ Requested: 12:30 PM (15 minutes ago)                     │ │
│  │                                                          │ │
│  │   [✅ Accept]  [❌ Reject]                              │ │
│  └──────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

### **Phase 2: Partner Decision**

#### **Option A: ACCEPT**

```
                    ┌───────────────────────┐
                    │  Click [Accept]       │
                    └───────────────────────┘
                                ↓
                    ┌───────────────────────┐
                    │  Status: ACCEPTED     │
                    └───────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│                 IN PROGRESS SECTION                             │
│                                                                 │
│  Order #: SMPL-20260207-0001                                    │
│  Status: SAMPLE_ACCEPTED                                        │
│                                                                 │
│  [Mark as Preparing] Button                                     │
└─────────────────────────────────────────────────────────────────┘
                                ↓
                    ┌───────────────────────┐
                    │  Click [Preparing]    │
                    └───────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│  Order #: SMPL-20260207-0001                                    │
│  Status: SAMPLE_PREPARING                                       │
│                                                                 │
│  Estimated Prep Time: 30-45 minutes                             │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐  │
│  │  [🚚 Request Pickup] (Large Primary Button)             │  │
│  └─────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                                ↓
                    ┌───────────────────────┐
                    │  Click [Request       │
                    │  Pickup]              │
                    └───────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│             SYSTEM CALLS DELIVERY API                           │
│  • Dunzo / Porter / Shadowfax                                   │
│  • Pickup Address: Partner Kitchen                              │
│  • Drop Address: Client Location                                │
│  • Delivery Partner Assigned Automatically                      │
└─────────────────────────────────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│  Order #: SMPL-20260207-0001                                    │
│  Status: IN_TRANSIT                                             │
│                                                                 │
│  ℹ️ Delivery in progress                                        │
│  Delivery Partner: Rahul (+91 98765 43210)                      │
│  Tracking ID: DUNZ1234567890                                    │
│  ETA: 15 minutes                                                │
│                                                                 │
│  ⏳ Waiting for delivery confirmation...                       │
└─────────────────────────────────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│  Order #: SMPL-20260207-0001                                    │
│  Status: DELIVERED ✅                                           │
│                                                                 │
│  Delivered at: 2:00 PM                                          │
│  Client feedback: Pending                                       │
└─────────────────────────────────────────────────────────────────┘
```

#### **Option B: REJECT**

```
                    ┌───────────────────────┐
                    │  Click [Reject]       │
                    └───────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│                   REJECTION MODAL                               │
│                                                                 │
│  ⚠️ Important:                                                  │
│  • Client will receive 100% refund (₹522.50)                   │
│  • You will earn ₹0 from this order                            │
│  • This action cannot be undone                                 │
│                                                                 │
│  Rejection Reason (Optional but Recommended):                   │
│  ┌─────────────────────────────────────────────────────────┐  │
│  │ Kitchen closed for maintenance today                     │  │
│  └─────────────────────────────────────────────────────────┘  │
│                                                                 │
│    [Cancel]  [Confirm Rejection]                                │
└─────────────────────────────────────────────────────────────────┘
                                ↓
                    ┌───────────────────────┐
                    │  Click [Confirm]      │
                    └───────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│             SYSTEM AUTO-PROCESSES:                              │
│  1. Status → SAMPLE_REJECTED                                    │
│  2. Razorpay Refund API Called                                  │
│  3. Refund Status → REFUNDED                                    │
│  4. Client Notified                                             │
│  5. Partner Earnings: ₹0                                        │
└─────────────────────────────────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│  Order #: SMPL-20260207-0001                                    │
│  Status: REJECTED ❌                                            │
│                                                                 │
│  Rejection Reason: Kitchen closed for maintenance               │
│  Refund: ₹522.50 (Processed)                                    │
│  Rejected at: 12:45 PM                                          │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🚚 DELIVERY FLOW (AUTOMATED)

### **Third-Party API Integration**

```
┌─────────────────────────────────────────────────────────────────┐
│                 PARTNER CLICKS "REQUEST PICKUP"                 │
└─────────────────────────────────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│              SYSTEM PREPARES DELIVERY REQUEST                   │
│                                                                 │
│  {                                                              │
│    "pickup_details": {                                          │
│      "name": "Rajesh Caterers",                                 │
│      "phone": "+91 98765 43210",                                │
│      "address": "Shop 5, MG Road, Mumbai",                      │
│      "latitude": 19.0760,                                       │
│      "longitude": 72.8777                                       │
│    },                                                           │
│    "drop_details": {                                            │
│      "name": "Priya Sharma",                                    │
│      "phone": "+91 98765 43210",                                │
│      "address": "Flat 302, Andheri West, Mumbai",               │
│      "latitude": 19.1136,                                       │
│      "longitude": 72.8697                                       │
│    },                                                           │
│    "order_details": {                                           │
│      "order_id": "SMPL-20260207-0001",                          │
│      "order_value": 522.50,                                     │
│      "order_type": "SAMPLE_TASTING"                             │
│    }                                                            │
│  }                                                              │
└─────────────────────────────────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│            CALL DELIVERY PROVIDER API                           │
│  (Dunzo / Porter / Shadowfax)                                   │
│                                                                 │
│  Priority Order:                                                │
│  1. Dunzo (Priority 1)                                          │
│  2. Porter (Priority 2)                                         │
│  3. Shadowfax (Priority 3)                                      │
│                                                                 │
│  If Dunzo fails → Try Porter → Try Shadowfax                    │
└─────────────────────────────────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│              DELIVERY PROVIDER RESPONSE                         │
│                                                                 │
│  {                                                              │
│    "status": "success",                                         │
│    "tracking_id": "DUNZ1234567890",                             │
│    "tracking_url": "https://dunzo.in/track/DUNZ1234567890",    │
│    "runner_name": "Rahul Kumar",                                │
│    "runner_phone": "+91 98765 43210",                           │
│    "vehicle_number": "MH12AB1234",                              │
│    "estimated_pickup_time": "2026-02-07T13:45:00",              │
│    "estimated_delivery_time": "2026-02-07T14:00:00"             │
│  }                                                              │
└─────────────────────────────────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│         SYSTEM SAVES TRACKING INFO                              │
│  • Update SampleOrders table                                    │
│  • Insert into SampleDeliveryTracking table                     │
│  • Update status to IN_TRANSIT                                  │
│  • Notify client with tracking link                             │
└─────────────────────────────────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│         LIVE TRACKING (Client sees this)                        │
│                                                                 │
│  Status: IN_TRANSIT                                             │
│                                                                 │
│  📍 Delivery Partner: Rahul Kumar                               │
│  📞 Phone: +91 98765 43210                                      │
│  🚗 Vehicle: MH12AB1234                                         │
│  ⏰ ETA: 15 minutes                                             │
│                                                                 │
│  ┌─────────────────────────────────────────┐                   │
│  │        [Live Map]                        │                   │
│  │                                         │                   │
│  │  • 📍 Partner Kitchen (Pickup)          │                   │
│  │  • 🏠 Your Location (Drop)              │                   │
│  │  • 🚴 Delivery Partner (Live GPS)        │                   │
│  │  • 📍 Route Path                        │                   │
│  │                                         │                   │
│  │  Updates every 10 seconds               │                   │
│  └─────────────────────────────────────────┘                   │
└─────────────────────────────────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│         DELIVERY PROVIDER WEBHOOK                               │
│  (Updates status automatically)                                 │
│                                                                 │
│  Status Updates:                                                │
│  • PICKUP_ASSIGNED → Delivery partner assigned                  │
│  • PICKED_UP → Partner picked up the order                      │
│  • IN_TRANSIT → On the way to delivery                          │
│  • DELIVERED → Successfully delivered                           │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🔒 ABUSE PREVENTION FLOW

```
┌─────────────────────────────────────────────────────────────────┐
│             CLIENT TRIES TO CREATE SAMPLE ORDER                 │
└─────────────────────────────────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│                 VALIDATION CHECKS                               │
│                                                                 │
│  ✅ Rule 1: Max 3 items allowed                                 │
│     • Client selected: 2 items ✅                               │
│                                                                 │
│  ✅ Rule 2: Check user's sample count this month                │
│     • User samples this month: 1 / 2 ✅                         │
│                                                                 │
│  ✅ Rule 3: Check cooldown period (24 hours)                    │
│     • Last sample from this caterer: 3 days ago ✅              │
│                                                                 │
│  ✅ Rule 4: Check if user is blocked                            │
│     • User is not blocked ✅                                    │
│                                                                 │
│  ✅ Rule 5: Check refund abuse                                  │
│     • User refunds: 0 / 3 ✅                                    │
│                                                                 │
│  ✅ Rule 6: Fixed sample quantity                               │
│     • All items have quantity = 1 ✅                            │
│                                                                 │
│  ✅ ALL CHECKS PASSED → Order Allowed                           │
└─────────────────────────────────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│         ABUSE TRACKING UPDATE                                   │
│  • Increment TotalSampleOrders counter                          │
│  • Update LastSampleOrderDate                                   │
│  • Store in SampleOrderAbuseTracking table                      │
└─────────────────────────────────────────────────────────────────┘
```

### **Blocked User Flow**

```
┌─────────────────────────────────────────────────────────────────┐
│        USER TRIES TO CREATE SAMPLE ORDER                        │
│        (User has excessive refunds)                             │
└─────────────────────────────────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│                 VALIDATION CHECKS                               │
│                                                                 │
│  ❌ Rule 4: Check if user is blocked                            │
│     • User refunds: 4 / 3 ❌                                    │
│     • User is BLOCKED                                           │
│     • Block reason: Excessive refunds                           │
│     • Blocked until: 2026-03-07                                 │
└─────────────────────────────────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│                    ERROR MESSAGE                                │
│                                                                 │
│  🚫 Unable to create sample order                               │
│                                                                 │
│  You are temporarily blocked from ordering samples.             │
│  Reason: Excessive refunds detected                             │
│                                                                 │
│  You can order samples again after: March 7, 2026               │
│                                                                 │
│  If you believe this is a mistake, please contact support.      │
└─────────────────────────────────────────────────────────────────┘
```

---

## 💳 PAYMENT & REFUND FLOW

### **Payment Flow**

```
┌─────────────────────────────────────────────────────────────────┐
│         CLIENT CLICKS "PROCEED TO PAYMENT"                      │
└─────────────────────────────────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│         CREATE SAMPLE ORDER (Status: PENDING)                   │
│  • Generate Order Number: SMPL-20260207-0001                    │
│  • Save to database                                             │
│  • PaymentStatus: PENDING                                       │
└─────────────────────────────────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│         CREATE RAZORPAY ORDER                                   │
│  {                                                              │
│    "amount": 52250, // in paise                                 │
│    "currency": "INR",                                           │
│    "notes": {                                                   │
│      "sample_order_id": 1,                                      │
│      "order_type": "SAMPLE_TASTING"                             │
│    }                                                            │
│  }                                                              │
└─────────────────────────────────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│         RAZORPAY MODAL OPENS                                    │
│  • Client enters payment details                                │
│  • Completes payment                                            │
└─────────────────────────────────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│         RAZORPAY WEBHOOK TRIGGERS                               │
│  Event: "payment.captured"                                      │
│                                                                 │
│  {                                                              │
│    "event": "payment.captured",                                 │
│    "payload": {                                                 │
│      "payment": {                                               │
│        "id": "pay_XYZ123",                                      │
│        "amount": 52250,                                         │
│        "status": "captured"                                     │
│      },                                                         │
│      "order": {                                                 │
│        "notes": {                                               │
│          "sample_order_id": 1                                   │
│        }                                                        │
│      }                                                          │
│    }                                                            │
│  }                                                              │
└─────────────────────────────────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│         SYSTEM PROCESSES WEBHOOK                                │
│  1. Verify webhook signature ✅                                 │
│  2. Update SampleOrder:                                         │
│     • PaymentStatus: PAID                                       │
│     • PaymentId: pay_XYZ123                                     │
│     • PaidAt: 2026-02-07 12:30:00                               │
│     • CurrentStatus: SAMPLE_REQUESTED                           │
│  3. Send notification to partner 📧                             │
│  4. Send confirmation email to client 📧                        │
└─────────────────────────────────────────────────────────────────┘
```

### **Refund Flow (Partner Rejection)**

```
┌─────────────────────────────────────────────────────────────────┐
│         PARTNER REJECTS SAMPLE REQUEST                          │
└─────────────────────────────────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│         SYSTEM AUTO-PROCESSES REFUND                            │
│  • Update status: SAMPLE_REJECTED                               │
│  • Store rejection reason                                       │
│  • Partner earnings: ₹0 (CRITICAL)                              │
└─────────────────────────────────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│         CALL RAZORPAY REFUND API                                │
│  {                                                              │
│    "payment_id": "pay_XYZ123",                                  │
│    "amount": 52250, // Full amount                              │
│    "speed": "normal", // 5-7 days                               │
│    "notes": {                                                   │
│      "reason": "PARTNER_REJECTED",                              │
│      "sample_order_id": 1                                       │
│    }                                                            │
│  }                                                              │
└─────────────────────────────────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│         REFUND RESPONSE                                         │
│  {                                                              │
│    "id": "rfnd_ABC123",                                         │
│    "amount": 52250,                                             │
│    "status": "processed",                                       │
│    "created_at": "2026-02-07T12:46:00"                          │
│  }                                                              │
└─────────────────────────────────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│         SYSTEM UPDATES DATABASE                                 │
│  • PaymentStatus: REFUNDED                                      │
│  • RefundedAt: 2026-02-07 12:46:00                              │
│  • RefundAmount: ₹522.50                                        │
│  • CurrentStatus: REFUNDED                                      │
│  • Update abuse tracking (refund counter)                       │
└─────────────────────────────────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│         NOTIFY CLIENT                                           │
│  📧 Email: Refund processed                                     │
│  📱 Push: Your sample request was declined. Refund initiated.   │
│  💰 Amount: ₹522.50                                             │
│  ⏰ Expected in: 5-7 business days                              │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🎯 CONVERSION TRACKING FLOW

```
┌─────────────────────────────────────────────────────────────────┐
│         SAMPLE DELIVERED SUCCESSFULLY                           │
└─────────────────────────────────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│         CLIENT SUBMITS FEEDBACK                                 │
│  • Taste: 5 stars                                               │
│  • Hygiene: 5 stars                                             │
│  • Overall: 5 stars                                             │
│  • Feedback: "Excellent! Will book for my wedding!"             │
└─────────────────────────────────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│         SYSTEM SHOWS CONVERSION CTA                             │
│                                                                 │
│  🎉 Loved the sample? Book your full event now!                 │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐  │
│  │  [Proceed with Full Event Booking] (Large Button)       │  │
│  └─────────────────────────────────────────────────────────┘  │
│                                                                 │
│  🎁 Get 5% discount on your first event booking                 │
└─────────────────────────────────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│         CLIENT CLICKS "PROCEED"                                 │
└─────────────────────────────────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│         REDIRECT TO EVENT BOOKING PAGE                          │
│  • Pre-fill caterer: Rajesh Caterers                            │
│  • Apply 5% discount coupon                                     │
│  • Show sample feedback as trust signal                         │
└─────────────────────────────────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│         CLIENT COMPLETES EVENT BOOKING                          │
└─────────────────────────────────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│         SYSTEM TRACKS CONVERSION                                │
│  • Update SampleOrder:                                          │
│    ConvertedToEventBooking = true                               │
│    EventOrderId = 123                                           │
│    ConvertedAt = 2026-02-07 14:30:00                            │
│  • Track conversion analytics                                   │
│  • Notify partner of successful conversion 🎉                   │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📊 STATUS FLOW DIAGRAM

```
                    ┌──────────────────────┐
                    │  SAMPLE_REQUESTED    │
                    │  (Payment Success)   │
                    └──────────────────────┘
                              │
                 ┌────────────┴────────────┐
                 │                         │
          ┌──────▼──────┐          ┌──────▼──────┐
          │  ACCEPTED   │          │  REJECTED   │
          │             │          │  (100%      │
          │             │          │  Refund)    │
          └──────┬──────┘          └──────┬──────┘
                 │                         │
          ┌──────▼──────┐          ┌──────▼──────┐
          │ PREPARING   │          │  REFUNDED   │
          └──────┬──────┘          └─────────────┘
                 │
          ┌──────▼──────┐
          │ READY FOR   │
          │ PICKUP      │
          └──────┬──────┘
                 │
          ┌──────▼──────┐
          │ IN TRANSIT  │
          │ (Live Track)│
          └──────┬──────┘
                 │
          ┌──────▼──────┐
          │ DELIVERED   │
          │             │
          │ ↓ Feedback  │
          │ ↓ Convert   │
          └─────────────┘
```

---

**END OF FLOW DIAGRAM**

This visual representation shows the complete flow of the sample tasting system following the EXACT specifications provided.
