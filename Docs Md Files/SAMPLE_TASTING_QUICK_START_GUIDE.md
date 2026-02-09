# 🚀 SAMPLE TASTING - QUICK START IMPLEMENTATION GUIDE

**Step-by-step guide to implement the complete sample tasting feature**

---

## 📋 PREREQUISITES

- [ ] .NET 8.0 SDK installed
- [ ] SQL Server 2019+ running
- [ ] Node.js 18+ for frontend
- [ ] Razorpay account (test/live keys)
- [ ] Dunzo/Porter API credentials (optional for testing)

---

## ⚡ QUICK IMPLEMENTATION STEPS

### **STEP 1: DATABASE SETUP (5 minutes)**

1. **Run the database schema:**
   ```bash
   # Navigate to Database folder
   cd Database

   # Run the schema script
   sqlcmd -S localhost -d CateringEcommerce -i Sample_Tasting_Complete_Schema.sql
   ```

2. **Verify tables created:**
   ```sql
   -- Check if all tables exist
   SELECT name FROM sys.tables WHERE name LIKE 'Sample%'

   -- Expected tables:
   -- Sample_Orders
   -- Sample_Order_Items
   -- Sample_Delivery_Tracking
   -- Sample_Refunds
   -- Menu_Item_Sample_Pricing
   -- System_Sample_Configuration
   -- Sample_Order_Abuse_Tracking (from main schema)
   ```

3. **Set sample prices for menu items:**
   ```sql
   -- Add sample pricing for existing menu items
   INSERT INTO Menu_Item_Sample_Pricing (MenuItemID, SamplePrice, SampleQuantity, IsAvailableForSample, OwnerID)
   SELECT
       PkID AS MenuItemID,
       CASE
           WHEN Category LIKE '%Main%' THEN 200
           WHEN Category LIKE '%Starter%' THEN 150
           WHEN Category LIKE '%Dessert%' THEN 100
           ELSE 175
       END AS SamplePrice,
       1 AS SampleQuantity,
       1 AS IsAvailableForSample,
       OwnerID
   FROM FoodItems
   WHERE IsActive = 1;
   ```

---

### **STEP 2: BACKEND - DOMAIN MODELS (10 minutes)**

1. **Create enum files (already exist):**
   - ✅ `CateringEcommerce.Domain/Enums/SampleOrderStatus.cs`
   - ✅ `CateringEcommerce.Domain/Enums/SampleDeliveryStatus.cs`
   - ✅ `CateringEcommerce.Domain/Enums/DeliveryProvider.cs`

2. **Create domain model folder:**
   ```bash
   mkdir CateringEcommerce.Domain/Models/Sample
   ```

3. **Copy models from implementation doc:**
   Create these files in `Models/Sample/`:
   - `SampleOrder.cs`
   - `SampleOrderItem.cs`
   - `SampleDeliveryTracking.cs`
   - `SampleTastingConfig.cs`
   - `MenuItemSamplePricing.cs`
   - `SampleOrderAbuseTracking.cs`

   **Copy the code from:** `SAMPLE_TASTING_IMPLEMENTATION_COMPLETE.md` → Section "Domain Models"

---

### **STEP 3: BACKEND - REPOSITORY LAYER (20 minutes)**

1. **Create repository interface:**
   ```bash
   mkdir CateringEcommerce.Domain/Interfaces/Sample
   ```

   Create file: `CateringEcommerce.Domain/Interfaces/Sample/ISampleOrderRepository.cs`

   **Copy the code from:** `SAMPLE_TASTING_IMPLEMENTATION_COMPLETE.md` → Section "Repository Layer"

2. **Create repository implementation:**
   ```bash
   mkdir CateringEcommerce.BAL/Base/Sample
   ```

   Create file: `CateringEcommerce.BAL/Base/Sample/SampleOrderRepository.cs`

   **Copy the code from:** `SAMPLE_TASTING_IMPLEMENTATION_COMPLETE.md` → Section "Repository Layer"

---

### **STEP 4: BACKEND - SERVICE LAYER (20 minutes)**

1. **Create service interface:**
   Create file: `CateringEcommerce.Domain/Interfaces/Sample/ISampleTastingService.cs`

   **Copy the code from:** `SAMPLE_TASTING_IMPLEMENTATION_COMPLETE.md` → Section "Service Layer"

2. **Create service implementation:**
   Create file: `CateringEcommerce.BAL/Base/Sample/SampleTastingService.cs`

   **Copy the code from:** `SAMPLE_TASTING_IMPLEMENTATION_COMPLETE.md` → Section "Service Layer"

3. **Create delivery service interface:**
   Create file: `CateringEcommerce.Domain/Interfaces/Sample/ISampleDeliveryService.cs`

   **Copy the code from:** `SAMPLE_TASTING_IMPLEMENTATION_COMPLETE.md` → Section "Third-Party Delivery Integration"

---

### **STEP 5: BACKEND - API CONTROLLERS (15 minutes)**

1. **Create client-side controller:**
   Create file: `CateringEcommerce.API/Controllers/User/SampleTastingController.cs`

   **Copy the code from:** `SAMPLE_TASTING_IMPLEMENTATION_COMPLETE.md` → Section "API Endpoints" → "CLIENT-SIDE API"

2. **Create partner-side controller:**
   Create file: `CateringEcommerce.API/Controllers/Owner/PartnerSampleTastingController.cs`

   **Copy the code from:** `SAMPLE_TASTING_IMPLEMENTATION_COMPLETE.md` → Section "API Endpoints" → "PARTNER-SIDE API"

---

### **STEP 6: BACKEND - DEPENDENCY INJECTION (5 minutes)**

**Update:** `CateringEcommerce.API/Program.cs`

```csharp
// Add these lines in the service registration section

// Sample Tasting Services
builder.Services.AddScoped<ISampleOrderRepository, SampleOrderRepository>();
builder.Services.AddScoped<ISampleTastingService, SampleTastingService>();
builder.Services.AddScoped<ISampleDeliveryService, DunzoDeliveryService>();

// Payment Service (if not already registered)
builder.Services.AddScoped<IPaymentService, RazorpayPaymentService>();

// Notification Service (if not already registered)
builder.Services.AddScoped<INotificationService, NotificationService>();
```

---

### **STEP 7: BACKEND - CONFIGURATION (5 minutes)**

**Update:** `CateringEcommerce.API/appsettings.json`

```json
{
  "SampleTasting": {
    "MaxItemsAllowed": 3,
    "FixedSampleQuantity": 1,
    "DeliveryChargeApplicable": true,
    "FlatDeliveryCharge": 50,
    "MaxSamplesPerUserPerMonth": 2,
    "SampleCooldownHours": 24
  },

  "Dunzo": {
    "BaseUrl": "https://apis.dunzo.in",
    "ApiKey": "YOUR_DUNZO_API_KEY",
    "ClientId": "YOUR_CLIENT_ID",
    "IsEnabled": true
  },

  "Porter": {
    "BaseUrl": "https://api.porter.in",
    "ApiKey": "YOUR_PORTER_API_KEY",
    "IsEnabled": false
  },

  "Shadowfax": {
    "BaseUrl": "https://api.shadowfax.in",
    "ApiKey": "YOUR_SHADOWFAX_API_KEY",
    "IsEnabled": false
  },

  "Razorpay": {
    "KeyId": "YOUR_RAZORPAY_KEY_ID",
    "KeySecret": "YOUR_RAZORPAY_KEY_SECRET",
    "WebhookSecret": "YOUR_WEBHOOK_SECRET"
  }
}
```

---

### **STEP 8: FRONTEND - API SERVICE (10 minutes)**

1. **Create API service file:**
   ```bash
   cd CateringEcommerce.Web/Frontend/src/services
   touch sampleTastingApi.js
   ```

2. **Add API methods:**
   ```javascript
   // sampleTastingApi.js

   import axios from 'axios';

   const API_BASE = '/api/user/sample-tasting';
   const PARTNER_API_BASE = '/api/owner/sample-tasting';

   // ========== CLIENT SIDE ==========

   export const getSampleEligibleItems = async (cateringId) => {
     const response = await axios.get(`${API_BASE}/eligible-items/${cateringId}`);
     return response.data;
   };

   export const createSampleOrder = async (orderData) => {
     const response = await axios.post(`${API_BASE}/create`, orderData);
     return response.data;
   };

   export const getMySampleOrders = async () => {
     const response = await axios.get(`${API_BASE}/my-orders`);
     return response.data;
   };

   export const getSampleOrderDetails = async (sampleOrderId) => {
     const response = await axios.get(`${API_BASE}/${sampleOrderId}`);
     return response.data;
   };

   export const getLiveTracking = async (sampleOrderId) => {
     const response = await axios.get(`${API_BASE}/${sampleOrderId}/live-tracking`);
     return response.data;
   };

   export const submitFeedback = async (sampleOrderId, feedbackData) => {
     const response = await axios.post(`${API_BASE}/${sampleOrderId}/feedback`, feedbackData);
     return response.data;
   };

   // ========== PARTNER SIDE ==========

   export const getPartnerPendingRequests = async () => {
     const response = await axios.get(`${PARTNER_API_BASE}/pending-requests`);
     return response.data;
   };

   export const acceptSampleRequest = async (sampleOrderId) => {
     const response = await axios.post(`${PARTNER_API_BASE}/${sampleOrderId}/accept`);
     return response.data;
   };

   export const rejectSampleRequest = async (sampleOrderId, reason) => {
     const response = await axios.post(`${PARTNER_API_BASE}/${sampleOrderId}/reject`, { reason });
     return response.data;
   };

   export const markAsPreparing = async (sampleOrderId) => {
     const response = await axios.post(`${PARTNER_API_BASE}/${sampleOrderId}/mark-preparing`);
     return response.data;
   };

   export const requestPickup = async (sampleOrderId) => {
     const response = await axios.post(`${PARTNER_API_BASE}/${sampleOrderId}/request-pickup`);
     return response.data;
   };
   ```

---

### **STEP 9: FRONTEND - CLIENT COMPONENTS (30 minutes)**

1. **Create component folder:**
   ```bash
   mkdir CateringEcommerce.Web/Frontend/src/components/user/sample-tasting
   ```

2. **Create these components:**

   **A. SampleTastingModal.jsx**
   ```jsx
   import React, { useState, useEffect } from 'react';
   import { Modal, Button, Alert, Card, Badge } from 'react-bootstrap';
   import { getSampleEligibleItems, createSampleOrder } from '../../../services/sampleTastingApi';
   import { initiatePayment } from '../../../services/paymentApi';

   const SampleTastingModal = ({ show, onHide, cateringId, cateringName }) => {
     const [items, setItems] = useState([]);
     const [selectedItems, setSelectedItems] = useState([]);
     const [loading, setLoading] = useState(false);
     const [config, setConfig] = useState({ maxItemsAllowed: 3 });

     useEffect(() => {
       if (show) {
         loadSampleItems();
       }
     }, [show]);

     const loadSampleItems = async () => {
       try {
         const response = await getSampleEligibleItems(cateringId);
         setItems(response.data);
       } catch (error) {
         console.error('Error loading sample items:', error);
       }
     };

     const handleItemSelect = (itemId) => {
       if (selectedItems.includes(itemId)) {
         setSelectedItems(selectedItems.filter(id => id !== itemId));
       } else {
         if (selectedItems.length < config.maxItemsAllowed) {
           setSelectedItems([...selectedItems, itemId]);
         }
       }
     };

     const calculateTotal = () => {
       const itemsTotal = items
         .filter(item => selectedItems.includes(item.menuItemId))
         .reduce((sum, item) => sum + item.samplePrice, 0);

       const deliveryCharge = 50;
       const taxAmount = itemsTotal * 0.05;
       const total = itemsTotal + deliveryCharge + taxAmount;

       return { itemsTotal, deliveryCharge, taxAmount, total };
     };

     const handleProceedToPayment = async () => {
       setLoading(true);
       try {
         // Create sample order
         const orderData = {
           cateringId,
           deliveryAddressId: 1, // Get from user context
           items: selectedItems.map(itemId => ({
             menuItemId: itemId,
             packageId: null
           }))
         };

         const response = await createSampleOrder(orderData);

         if (response.success) {
           // Initiate payment
           const { total } = calculateTotal();
           await initiatePayment({
             amount: total,
             orderId: response.sampleOrderId,
             orderType: 'SAMPLE_TASTING',
             onSuccess: () => {
               window.location.href = `/sample-orders/${response.sampleOrderId}`;
             }
           });
         }
       } catch (error) {
         console.error('Error creating sample order:', error);
       } finally {
         setLoading(false);
       }
     };

     const { itemsTotal, deliveryCharge, taxAmount, total } = calculateTotal();

     return (
       <Modal show={show} onHide={onHide} size="lg">
         <Modal.Header closeButton>
           <Modal.Title>Try Sample Tasting - {cateringName}</Modal.Title>
         </Modal.Header>
         <Modal.Body>
           <Alert variant="info">
             • Select up to {config.maxItemsAllowed} items to taste<br/>
             • Fixed sample quantity per item<br/>
             • Delivery with live tracking
           </Alert>

           <div className="row">
             {items.map(item => (
               <div key={item.menuItemId} className="col-md-6 mb-3">
                 <Card
                   className={selectedItems.includes(item.menuItemId) ? 'border-primary' : ''}
                   onClick={() => handleItemSelect(item.menuItemId)}
                   style={{ cursor: 'pointer' }}
                 >
                   <Card.Body>
                     <Card.Title>
                       {item.itemName}
                       {item.isVeg && <Badge bg="success" className="ms-2">Veg</Badge>}
                     </Card.Title>
                     <Card.Text>
                       Sample Price: ₹{item.samplePrice}<br/>
                       Quantity: {item.sampleQuantity} portion
                     </Card.Text>
                     {selectedItems.includes(item.menuItemId) && (
                       <Badge bg="primary">Selected ✓</Badge>
                     )}
                   </Card.Body>
                 </Card>
               </div>
             ))}
           </div>

           <hr/>

           <div className="price-breakdown">
             <h6>Price Breakdown:</h6>
             <div className="d-flex justify-content-between">
               <span>Items Total:</span>
               <span>₹{itemsTotal.toFixed(2)}</span>
             </div>
             <div className="d-flex justify-content-between">
               <span>Delivery:</span>
               <span>₹{deliveryCharge.toFixed(2)}</span>
             </div>
             <div className="d-flex justify-content-between">
               <span>Tax (5%):</span>
               <span>₹{taxAmount.toFixed(2)}</span>
             </div>
             <hr/>
             <div className="d-flex justify-content-between">
               <strong>Total:</strong>
               <strong>₹{total.toFixed(2)}</strong>
             </div>
           </div>
         </Modal.Body>
         <Modal.Footer>
           <Button variant="secondary" onClick={onHide}>
             Cancel
           </Button>
           <Button
             variant="primary"
             onClick={handleProceedToPayment}
             disabled={selectedItems.length === 0 || loading}
           >
             {loading ? 'Processing...' : 'Proceed to Payment'}
           </Button>
         </Modal.Footer>
       </Modal>
     );
   };

   export default SampleTastingModal;
   ```

   **B. SampleOrderTracking.jsx**
   ```jsx
   import React, { useState, useEffect } from 'react';
   import { useParams } from 'react-router-dom';
   import { Card, Button, Badge, Alert } from 'react-bootstrap';
   import { getSampleOrderDetails, getLiveTracking } from '../../../services/sampleTastingApi';
   import LiveTrackingMap from './LiveTrackingMap';
   import SampleFeedbackModal from './SampleFeedbackModal';

   const SampleOrderTracking = () => {
     const { sampleOrderId } = useParams();
     const [order, setOrder] = useState(null);
     const [tracking, setTracking] = useState(null);
     const [showFeedbackModal, setShowFeedbackModal] = useState(false);

     useEffect(() => {
       loadOrderDetails();
       const interval = setInterval(() => {
         if (order?.currentStatus === 'IN_TRANSIT') {
           loadLiveTracking();
         }
       }, 10000); // Update every 10 seconds

       return () => clearInterval(interval);
     }, [sampleOrderId]);

     const loadOrderDetails = async () => {
       const response = await getSampleOrderDetails(sampleOrderId);
       setOrder(response.data);
     };

     const loadLiveTracking = async () => {
       const response = await getLiveTracking(sampleOrderId);
       setTracking(response.data);
     };

     const getStatusBadge = (status) => {
       const statusColors = {
         'SAMPLE_REQUESTED': 'warning',
         'SAMPLE_ACCEPTED': 'info',
         'SAMPLE_REJECTED': 'danger',
         'SAMPLE_PREPARING': 'info',
         'READY_FOR_PICKUP': 'primary',
         'IN_TRANSIT': 'primary',
         'DELIVERED': 'success',
         'REFUNDED': 'secondary'
       };
       return <Badge bg={statusColors[status] || 'secondary'}>{status.replace(/_/g, ' ')}</Badge>;
     };

     if (!order) return <div>Loading...</div>;

     return (
       <div className="container mt-4">
         <h2>Sample Order Tracking</h2>
         <Card className="mb-4">
           <Card.Body>
             <h5>Order #{order.sampleOrderNumber}</h5>
             <p>Status: {getStatusBadge(order.currentStatus)}</p>
             <p>Total Amount: ₹{order.totalAmount}</p>

             {/* Status Timeline */}
             <div className="status-timeline mt-4">
               {/* Render timeline based on status */}
               {/* Implementation here */}
             </div>

             {/* Live Tracking (Only when IN_TRANSIT) */}
             {order.currentStatus === 'IN_TRANSIT' && tracking && (
               <LiveTrackingMap tracking={tracking} />
             )}

             {/* Rejected Alert */}
             {order.currentStatus === 'SAMPLE_REJECTED' && (
               <Alert variant="danger">
                 <h6>Sample Request Declined</h6>
                 <p>Reason: {order.rejectionReason}</p>
                 <p>Full refund of ₹{order.refundAmount} has been initiated.</p>
               </Alert>
             )}

             {/* Feedback Button (Only when DELIVERED) */}
             {order.currentStatus === 'DELIVERED' && !order.feedbackSubmittedAt && (
               <Button variant="primary" onClick={() => setShowFeedbackModal(true)}>
                 Rate Your Sample Experience
               </Button>
             )}

             {/* Conversion CTA (After feedback) */}
             {order.feedbackSubmittedAt && !order.convertedToEventBooking && (
               <Alert variant="success">
                 <h5>🎉 Loved the sample? Book your full event now!</h5>
                 <Button
                   variant="success"
                   size="lg"
                   onClick={() => window.location.href = `/catering/${order.cateringId}/book`}
                 >
                   Proceed with Full Event Booking
                 </Button>
                 <br/>
                 <small>Get 5% discount on your first event booking</small>
               </Alert>
             )}
           </Card.Body>
         </Card>

         <SampleFeedbackModal
           show={showFeedbackModal}
           onHide={() => setShowFeedbackModal(false)}
           sampleOrderId={sampleOrderId}
           onSubmitSuccess={loadOrderDetails}
         />
       </div>
     );
   };

   export default SampleOrderTracking;
   ```

---

### **STEP 10: FRONTEND - PARTNER COMPONENTS (20 minutes)**

1. **Create partner component folder:**
   ```bash
   mkdir CateringEcommerce.Web/Frontend/src/components/owner/sample-tasting
   ```

2. **Create PartnerSampleRequests.jsx** (see implementation doc for full code)

---

### **STEP 11: TESTING (30 minutes)**

1. **Test Backend APIs:**
   ```bash
   # Start the API
   cd CateringEcommerce.API
   dotnet run

   # Test endpoints using Postman or curl
   curl -X GET http://localhost:5000/api/user/sample-tasting/eligible-items/1
   ```

2. **Test Frontend:**
   ```bash
   # Start frontend dev server
   cd CateringEcommerce.Web/Frontend
   npm run dev

   # Open browser: http://localhost:3000
   # Test sample tasting flow
   ```

3. **Test Payment Flow (Razorpay Test Mode):**
   - Use test card: 4111 1111 1111 1111
   - CVV: Any 3 digits
   - Expiry: Any future date

4. **Test Webhook (ngrok):**
   ```bash
   # Install ngrok
   ngrok http 5000

   # Copy ngrok URL and set in Razorpay webhook settings
   https://your-ngrok-url.ngrok.io/api/webhooks/razorpay/sample-payment
   ```

---

### **STEP 12: DEPLOYMENT CHECKLIST**

**Before Production:**

- [ ] Update Razorpay keys to live mode
- [ ] Configure Dunzo/Porter production API keys
- [ ] Set up SMS/Email notifications
- [ ] Test end-to-end flow with real payments
- [ ] Enable application insights/logging
- [ ] Set up database backups
- [ ] Configure CORS for frontend domain
- [ ] Test on mobile devices
- [ ] Load test API endpoints
- [ ] Set up monitoring alerts

---

## 🛠️ TROUBLESHOOTING

### **Common Issues:**

1. **Payment not reflecting:**
   - Check webhook URL is accessible
   - Verify Razorpay webhook secret
   - Check logs for webhook errors

2. **Delivery API failing:**
   - Verify API keys are correct
   - Check if delivery provider is enabled in config
   - Test with mock delivery service first

3. **Sample prices not showing:**
   - Verify `Menu_Item_Sample_Pricing` table has data
   - Check `IsAvailableForSample` = 1
   - Verify menu items are active

4. **User blocked unexpectedly:**
   - Check `SampleOrderAbuseTracking` table
   - Verify cooldown period settings
   - Check refund count

---

## 📚 ADDITIONAL RESOURCES

- **Full Implementation Doc:** `SAMPLE_TASTING_IMPLEMENTATION_COMPLETE.md`
- **Flow Diagrams:** `SAMPLE_TASTING_FLOW_DIAGRAM.md`
- **Database Schema:** `Database/Sample_Tasting_Complete_Schema.sql`

---

## 🎯 ESTIMATED TIME TO COMPLETE

| Task | Time |
|------|------|
| Database Setup | 5 min |
| Backend Models | 10 min |
| Backend Repository | 20 min |
| Backend Service | 20 min |
| Backend Controllers | 15 min |
| Backend DI & Config | 10 min |
| Frontend API Service | 10 min |
| Frontend Client Components | 30 min |
| Frontend Partner Components | 20 min |
| Testing | 30 min |
| **Total** | **~3 hours** |

---

## ✅ SUCCESS CRITERIA

Your implementation is complete when:

- [ ] Client can browse catering and see "Try Sample" button
- [ ] Client can select up to 3 items and see sample prices
- [ ] Client can complete payment (Razorpay)
- [ ] Partner receives notification of new sample request
- [ ] Partner can accept or reject request
- [ ] Rejection triggers 100% auto-refund
- [ ] Partner can request pickup
- [ ] Delivery API is called and tracking ID is stored
- [ ] Client can see live tracking map
- [ ] Client can submit feedback after delivery
- [ ] Conversion CTA appears after feedback
- [ ] Abuse prevention rules work correctly
- [ ] All statuses flow correctly

---

## 🚀 NEXT STEPS AFTER IMPLEMENTATION

1. **Analytics & Reporting:**
   - Track conversion rates (sample → full event)
   - Monitor rejection rates by partner
   - Analyze most popular sample items

2. **Optimizations:**
   - A/B test sample pricing
   - Test different max item limits
   - Optimize delivery provider selection

3. **Future Enhancements:**
   - Sample combo offers
   - Free sample for first-time users
   - Sample subscription plans
   - Partner sample analytics dashboard

---

**END OF QUICK START GUIDE**

Good luck with your implementation! 🎉
