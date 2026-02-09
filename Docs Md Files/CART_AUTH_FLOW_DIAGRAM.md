# Cart-to-Checkout Authentication Flow Diagram

## 🎯 Complete User Journey

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    USER BROWSES CATERING SERVICES                       │
│                         (NO LOGIN REQUIRED)                             │
└──────────────────────────────┬──────────────────────────────────────────┘
                               │
                               ▼
                    ┌──────────────────────┐
                    │ Selects Package or   │
                    │ Menu Items           │
                    └──────────┬───────────┘
                               │
                               ▼
                    ┌──────────────────────┐
                    │ Clicks "Add to Cart" │
                    └──────────┬───────────┘
                               │
                               ▼
                    ┌──────────────────────┐
                    │   Cart Created       │
                    │   (In Memory)        │
                    └──────────┬───────────┘
                               │
                               ▼
              ┌────────────────────────────────┐
              │  FloatingCartButton Appears    │
              │  (Bottom-right of screen)      │
              │  Shows: Count + Total Amount   │
              └────────────┬───────────────────┘
                           │
                           ▼
              ┌────────────────────────────────┐
              │ User Clicks FloatingCartButton │
              │        OR "View Cart"          │
              └────────────┬───────────────────┘
                           │
                           ▼
         ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓
         ┃   EnhancedCartDrawer Slides In      ┃
         ┃   (Swiggy/Zomato Style)             ┃
         ┃   ┌───────────────────────────────┐ ┃
         ┃   │ Caterer Info with Logo        │ ┃
         ┃   │ Package Details               │ ┃
         ┃   │ Add-ons List                  │ ┃
         ┃   │ Bill Summary (Subtotal + GST) │ ┃
         ┃   │ Total Amount                  │ ┃
         ┃   └───────────────────────────────┘ ┃
         ┗━━━━━━━━━━━━━━━━┳━━━━━━━━━━━━━━━━━━━━┛
                          │
                          ▼
         ┌────────────────────────────────────┐
         │ User Clicks "Proceed to Checkout"  │
         └────────────┬───────────────────────┘
                      │
                      ▼
         ┌────────────────────────────────────┐
         │   useAuthGuard.requireAuth()       │
         │   Checks Authentication Status     │
         └────────────┬───────────────────────┘
                      │
        ┌─────────────┴──────────────┐
        │                            │
        ▼                            ▼
┌───────────────┐           ┌───────────────┐
│ AUTHENTICATED │           │ NOT           │
│ (User Logged  │           │ AUTHENTICATED │
│  In)          │           │               │
└───────┬───────┘           └───────┬───────┘
        │                           │
        │                           ▼
        │              ┏━━━━━━━━━━━━━━━━━━━━━━━━━┓
        │              ┃   AuthModal Appears     ┃
        │              ┃   (Modal Overlay)       ┃
        │              ┃ ┌─────────────────────┐ ┃
        │              ┃ │ Mobile OTP Option   │ ┃
        │              ┃ │ Google OAuth Button │ ┃
        │              ┃ │ Facebook OAuth Btn  │ ┃
        │              ┃ │ Signup Option       │ ┃
        │              ┃ └─────────────────────┘ ┃
        │              ┗━━━━━━━━━┳━━━━━━━━━━━━━━━┛
        │                        │
        │              ┌─────────┴─────────┐
        │              │                   │
        │              ▼                   ▼
        │    ┌─────────────────┐  ┌─────────────────┐
        │    │  Mobile OTP     │  │  OAuth Login    │
        │    │  Flow           │  │  (Google/FB)    │
        │    └────────┬────────┘  └────────┬────────┘
        │             │                    │
        │             ▼                    ▼
        │    ┌─────────────────┐  ┌─────────────────┐
        │    │ 1. Enter Phone  │  │ 1. Redirect to  │
        │    │ 2. Receive OTP  │  │    OAuth provider│
        │    │ 3. Verify OTP   │  │ 2. User approves│
        │    │ 4. Trust Device │  │ 3. OAuth callback│
        │    │    (Optional)   │  │ 4. Backend auth │
        │    └────────┬────────┘  └────────┬────────┘
        │             │                    │
        │             └──────────┬─────────┘
        │                        │
        │                        ▼
        │              ┌─────────────────────┐
        │              │ Authentication      │
        │              │ Successful          │
        │              │ - User data stored  │
        │              │ - Token generated   │
        │              │ - Cart persisted    │
        │              │   to localStorage   │
        │              └──────────┬──────────┘
        │                         │
        └─────────────────────────┘
                      │
                      ▼
         ┌────────────────────────────────────┐
         │  Navigate to /checkout             │
         │  (EnhancedCheckoutPage)            │
         └────────────┬───────────────────────┘
                      │
                      ▼
         ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓
         ┃   EnhancedCheckoutPage             ┃
         ┃   ┌────────────────────────────┐   ┃
         ┃   │ Step 1: Event Details      │   ┃
         ┃   │ - Event Date & Time        │   ┃
         ┃   │ - Event Type               │   ┃
         ┃   │ - Event Location           │   ┃
         ┃   │ - Special Instructions     │   ┃
         ┃   └────────────┬───────────────┘   ┃
         ┃                ▼                    ┃
         ┃   ┌────────────────────────────┐   ┃
         ┃   │ Step 2: Address & Contact  │   ┃
         ┃   │ - Delivery Address         │   ┃
         ┃   │ - Contact Person           │   ┃
         ┃   │ - Contact Phone            │   ┃
         ┃   │ - Contact Email            │   ┃
         ┃   └────────────┬───────────────┘   ┃
         ┃                ▼                    ┃
         ┃   ┌────────────────────────────┐   ┃
         ┃   │ Step 3: Payment & Review   │   ┃
         ┃   │ - Payment Method           │   ┃
         ┃   │ - Order Summary            │   ┃
         ┃   │ - Terms & Conditions       │   ┃
         ┃   │ - Place Order Button       │   ┃
         ┃   └────────────┬───────────────┘   ┃
         ┗━━━━━━━━━━━━━━━━┻━━━━━━━━━━━━━━━━━━━┛
                          │
                          ▼
         ┌────────────────────────────────────┐
         │  Order Placed Successfully         │
         │  - Order Confirmation Modal        │
         │  - Order ID Generated              │
         │  - Cart Cleared                    │
         │  - Navigate to Orders Page         │
         └────────────────────────────────────┘
```

---

## 🔄 State Management Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                     CART CONTEXT STATE                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌────────────────────────────────────────────────────┐        │
│  │ UNAUTHENTICATED USER                               │        │
│  │  - cart: Stored in memory only                     │        │
│  │  - isCartOpen: false                               │        │
│  │  - localStorage: No cart data                      │        │
│  └────────────────────────────────────────────────────┘        │
│                         │                                       │
│                         │ User logs in                          │
│                         ▼                                       │
│  ┌────────────────────────────────────────────────────┐        │
│  │ AUTHENTICATED USER                                 │        │
│  │  - cart: Synced to localStorage: cart_${userId}    │        │
│  │  - isCartOpen: true/false                          │        │
│  │  - Cart persists across page refreshes             │        │
│  └────────────────────────────────────────────────────┘        │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🔐 Authentication State Flow

```
┌──────────────────────────────────────────────────────────┐
│                  AUTH CONTEXT STATE                      │
├──────────────────────────────────────────────────────────┤
│                                                          │
│  ┌────────────────────────────────────────┐             │
│  │ INITIAL STATE                          │             │
│  │  - isAuthenticated: false              │             │
│  │  - user: null                          │             │
│  │  - token: null                         │             │
│  └──────────────┬─────────────────────────┘             │
│                 │                                        │
│                 │ User clicks "Proceed to Checkout"      │
│                 ▼                                        │
│  ┌────────────────────────────────────────┐             │
│  │ useAuthGuard.requireAuth() CALLED      │             │
│  │  - Checks: isAuthenticated && user     │             │
│  │  - Result: FALSE                       │             │
│  │  - Action: Show AuthModal              │             │
│  │  - Store: auth_redirect → '/checkout'  │             │
│  └──────────────┬─────────────────────────┘             │
│                 │                                        │
│                 │ User completes authentication          │
│                 ▼                                        │
│  ┌────────────────────────────────────────┐             │
│  │ AUTHENTICATED STATE                    │             │
│  │  - isAuthenticated: true               │             │
│  │  - user: { pkid, name, role, token }   │             │
│  │  - token: JWT token                    │             │
│  └──────────────┬─────────────────────────┘             │
│                 │                                        │
│                 │ Check localStorage for auth_redirect   │
│                 ▼                                        │
│  ┌────────────────────────────────────────┐             │
│  │ POST-AUTH REDIRECT                     │             │
│  │  - Read: auth_redirect from localStorage│            │
│  │  - Navigate to: /checkout              │             │
│  │  - Clear: auth_redirect                │             │
│  └────────────────────────────────────────┘             │
│                                                          │
└──────────────────────────────────────────────────────────┘
```

---

## 🎨 Component Hierarchy

```
App.jsx
├── AppHeader.jsx
│   ├── UserNotifications.jsx
│   ├── Cart Icon Button (triggers toggleCart)
│   └── User Dropdown Menu
│
├── Main Content (React Router Outlet)
│   ├── HomePage
│   ├── CateringListPage
│   ├── CateringDetailPage
│   └── EnhancedCheckoutPage ★ NEW
│       ├── EnhancedProgressStepper
│       ├── EventDetailsForm
│       ├── AddressContactForm
│       ├── PaymentReviewForm
│       └── OrderConfirmationModal
│
├── AppFooter.jsx
│
├── AuthModal.jsx (Existing - Updated)
│   ├── Login View
│   │   ├── Mobile OTP Input
│   │   ├── Google OAuth Button
│   │   └── Facebook OAuth Button
│   ├── OTP Verification View
│   └── Signup View
│
├── EnhancedCartDrawer.jsx ★ NEW
│   ├── Caterer Info Section
│   ├── Package Details Section
│   ├── Additional Items Section
│   ├── Bill Summary Section
│   ├── Authentication Notice (if not logged in)
│   └── Proceed to Checkout Button (uses useAuthGuard)
│
└── FloatingCartButton.jsx ★ NEW
    ├── Cart Icon with Badge
    ├── Cart Count Display
    └── Total Amount Display
```

---

## 📦 Data Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                        CART DATA FLOW                           │
└─────────────────────────────────────────────────────────────────┘

1. USER ADDS TO CART
   ───────────────────────────────────────────────────────────────
   CateringDetailPage
        │
        ├─→ Calls: addToCart(cateringData)
        │
        ▼
   CartContext.addToCart()
        │
        ├─→ Creates cart object:
        │   {
        │     cateringId, cateringName, cateringLogo,
        │     packageId, packageName, packagePrice,
        │     guestCount, eventDate, eventType,
        │     decorationId, additionalItems,
        │     baseAmount, taxAmount, totalAmount
        │   }
        │
        ├─→ If authenticated:
        │   localStorage.setItem(`cart_${userId}`, JSON.stringify(cart))
        │
        └─→ setIsCartOpen(true)


2. USER VIEWS CART
   ───────────────────────────────────────────────────────────────
   FloatingCartButton OR AppHeader Cart Icon
        │
        ├─→ Calls: toggleCart()
        │
        ▼
   CartContext.toggleCart()
        │
        └─→ setIsCartOpen(!isCartOpen)
        │
        ▼
   EnhancedCartDrawer
        │
        └─→ Displays cart data from CartContext


3. USER PROCEEDS TO CHECKOUT
   ───────────────────────────────────────────────────────────────
   EnhancedCartDrawer
        │
        ├─→ User clicks "Proceed to Checkout"
        │
        ▼
   useAuthGuard.requireAuth(
        () => navigate('/checkout'),
        '/checkout'
   )
        │
        ├─→ If NOT authenticated:
        │   ├─→ setShowAuthModal(true)
        │   └─→ localStorage.setItem('auth_redirect', '/checkout')
        │
        └─→ If authenticated:
            └─→ navigate('/checkout')


4. USER PLACES ORDER
   ───────────────────────────────────────────────────────────────
   EnhancedCheckoutPage
        │
        ├─→ User fills form (3 steps)
        │
        ├─→ User clicks "Place Order"
        │
        ▼
   createOrder(orderPayload)
        │
        ├─→ API Call: POST /api/orders/create
        │
        ├─→ Response: { orderId, orderDetails }
        │
        ├─→ CartContext.clearCart()
        │
        └─→ Navigate to /orders
```

---

## 🔒 Security Considerations

```
┌─────────────────────────────────────────────────────────────────┐
│                    SECURITY LAYERS                              │
└─────────────────────────────────────────────────────────────────┘

1. DEVICE FINGERPRINTING
   ───────────────────────────────────────────────────────────────
   ┌─────────────────────────────────────────┐
   │ getOrGenerateFingerprint()              │
   │  - User Agent                           │
   │  - Screen Resolution                    │
   │  - Timezone                             │
   │  - Language                             │
   │  - Platform                             │
   │  - Canvas Fingerprint                   │
   │  → Hash: SHA-256                        │
   └─────────────────────────────────────────┘


2. DEVICE TRUST (30-DAY)
   ───────────────────────────────────────────────────────────────
   ┌─────────────────────────────────────────┐
   │ User Login                              │
   │  ├─→ Check: trustDevice checkbox        │
   │  ├─→ Send: deviceFingerprint            │
   │  ├─→ Backend: Save device fingerprint   │
   │  └─→ Backend: Set expiry (30 days)      │
   │                                         │
   │ Next Login (Same Device)                │
   │  ├─→ Send: deviceFingerprint            │
   │  ├─→ Backend: Match fingerprint         │
   │  ├─→ Backend: Check expiry              │
   │  └─→ Result: Reduced OTP friction       │
   └─────────────────────────────────────────┘


3. JWT TOKEN MANAGEMENT
   ───────────────────────────────────────────────────────────────
   ┌─────────────────────────────────────────┐
   │ Token Storage                           │
   │  ├─→ localStorage: user token           │
   │  └─→ AuthContext: in-memory token       │
   │                                         │
   │ Token Usage                             │
   │  ├─→ API Requests: Bearer token         │
   │  └─→ Header: Authorization: Bearer {token}│
   └─────────────────────────────────────────┘


4. CART VALIDATION
   ───────────────────────────────────────────────────────────────
   ┌─────────────────────────────────────────┐
   │ Backend Validation                      │
   │  ├─→ Verify: userId matches token       │
   │  ├─→ Verify: cateringId exists          │
   │  ├─→ Verify: packageId exists           │
   │  ├─→ Verify: price calculations         │
   │  └─→ Verify: user not placing duplicate │
   └─────────────────────────────────────────┘
```

---

**Last Updated:** February 6, 2026
**Status:** ✅ Implementation Complete
