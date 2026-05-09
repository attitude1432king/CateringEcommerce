import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { getOrderDetails, cancelOrder } from '../services/orderApi';
import { canReviewOrder, getReviewByOrder } from '../services/reviewApi';
import { usePayment } from '../contexts/PaymentContext'; // P0 FIX: Add payment context
import { useAuth } from '../contexts/AuthContext'; // P0 FIX: Add auth context
import PaymentTimeline from '../components/user/order/PaymentTimeline';
import OrderTimeline from '../components/user/order/OrderTimeline';
import LiveEventBanner from '../components/user/order/LiveEventBanner';
import PostEventPaymentSection from '../components/user/order/PostEventPaymentSection';
import { PlatformProtectedBadge } from '../components/common/badges';
import ReviewSubmissionModal from '../components/user/review/ReviewSubmissionModal';
import StarRating from '../components/common/StarRating';
import toast from 'react-hot-toast'; // P2 FIX: Add toast for better UX

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL.replace(/\/$/, '');

const OrderDetailPage = () => {
  const { orderId } = useParams();
  const navigate = useNavigate();
  const { user } = useAuth(); // P0 FIX: Get user for payment
  const { isRazorpayLoaded, openRazorpayCheckout, createRazorpayOrder } = usePayment(); // P0 FIX: Payment context
  const [order, setOrder] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);
  const [isCancelling, setIsCancelling] = useState(false);
  const [showCancelModal, setShowCancelModal] = useState(false);
  const [cancelReason, setCancelReason] = useState('');

  // Review states
  const [canReview, setCanReview] = useState(false);
  const [existingReview, setExistingReview] = useState(null);
  const [showReviewModal, setShowReviewModal] = useState(false);
  const [reviewCheckLoading, setReviewCheckLoading] = useState(false);

  useEffect(() => {
    if (orderId) {
      fetchOrderDetails();
      checkReviewEligibility();
    }
  }, [orderId]);

  const fetchOrderDetails = async () => {
    setIsLoading(true);
    setError(null);

    try {
      const response = await getOrderDetails(orderId);

      if (response.result && response.data) {
        setOrder(response.data);
      } else {
        setError(response.message || 'Failed to load order details');
      }
    } catch (error) {
      console.error('Error fetching order details:', error);
      setError('An error occurred while loading order details');
    } finally {
      setIsLoading(false);
    }
  };

  const checkReviewEligibility = async () => {
    setReviewCheckLoading(true);
    try {
      // Check if user can review this order
      const canReviewResponse = await canReviewOrder(orderId);
      if (canReviewResponse.result && canReviewResponse.data) {
        setCanReview(canReviewResponse.data.canReview);

        // If already reviewed, fetch the existing review
        if (canReviewResponse.data.alreadyReviewed) {
          const existingReviewResponse = await getReviewByOrder(orderId);
          if (existingReviewResponse.result && existingReviewResponse.data) {
            setExistingReview(existingReviewResponse.data);
          }
        }
      }
    } catch (error) {
      console.error('Error checking review eligibility:', error);
    } finally {
      setReviewCheckLoading(false);
    }
  };

  const handleReviewSubmitted = (reviewData) => {
    // Refresh review status after submission
    checkReviewEligibility();
    // P2 FIX: Use toast instead of alert
    toast.success('Thank you for your review!');
  };

  const handleCancelOrder = async () => {
    if (!cancelReason.trim()) {
      // P2 FIX: Use toast instead of alert
      toast.error('Please provide a cancellation reason');
      return;
    }

    setIsCancelling(true);

    try {
      const response = await cancelOrder(orderId, cancelReason);

      if (response.result) {
        // P2 FIX: Use toast instead of alert
        toast.success('Order cancelled successfully');
        setShowCancelModal(false);
        fetchOrderDetails(); // Refresh order details
      } else {
        // P2 FIX: Use toast instead of alert
        toast.error(response.message || 'Failed to cancel order');
      }
    } catch (error) {
      console.error('Error cancelling order:', error);
      // P2 FIX: Use toast instead of alert
      toast.error(error.message || 'An error occurred while cancelling the order');
    } finally {
      setIsCancelling(false);
    }
  };

  const canCancelOrder = (order) => {
    if (!order) return false;
    if (order.orderStatus !== 'Pending') return false;

    // Check if within 2 hours of creation
    const createdTime = new Date(order.createdDate).getTime();
    const currentTime = new Date().getTime();
    const hoursSinceCreation = (currentTime - createdTime) / (1000 * 60 * 60);

    return hoursSinceCreation <= 2;
  };

  const canFileComplaint = (order) => {
    if (!order) return false;

    // Allow complaints only for Completed orders
    if (order.orderStatus !== 'Completed') return false;

    // Check if within 7 days of event date
    const eventTime = new Date(order.eventDate).getTime();
    const currentTime = new Date().getTime();
    const daysSinceEvent = (currentTime - eventTime) / (1000 * 60 * 60 * 24);

    return daysSinceEvent <= 7;
  };

  const getStatusColor = (status) => {
    const colors = {
      Pending: 'bg-yellow-100 text-yellow-800',
      Confirmed: 'bg-blue-100 text-blue-800',
      InProgress: 'bg-purple-100 text-purple-800',
      Completed: 'bg-green-100 text-green-800',
      Cancelled: 'bg-red-100 text-red-800'
    };
    return colors[status] || 'bg-neutral-100 text-neutral-800';
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-neutral-100 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-red-500 mx-auto"></div>
          <p className="mt-4 text-neutral-600">Loading order details...</p>
        </div>
      </div>
    );
  }

  if (error || !order) {
    return (
      <div className="min-h-screen bg-neutral-100 py-8">
        <div className="max-w-4xl mx-auto px-4">
          <div className="bg-red-50 border border-red-200 text-red-800 px-4 py-3 rounded-lg">
            {error || 'Order not found'}
          </div>
          <button
            onClick={() => navigate('/my-orders')}
            className="mt-4 px-6 py-3 bg-gray-500 text-white rounded-lg hover:bg-gray-600"
          >
            Back to Orders
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-neutral-100 py-8">
      <div className="max-w-4xl mx-auto px-4">
        {/* Header */}
        <div className="mb-6">
          <button
            onClick={() => navigate('/my-orders')}
            className="text-neutral-600 hover:text-neutral-900 mb-4 flex items-center"
          >
            <svg className="w-5 h-5 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M15 19l-7-7 7-7" />
            </svg>
            Back to Orders
          </button>
          <div className="flex justify-between items-start">
            <div>
              <h1 className="text-3xl font-bold">Order #{order.orderNumber}</h1>
              <p className="text-neutral-600 mt-1">
                Placed on {new Date(order.createdDate).toLocaleDateString('en-IN', {
                  day: 'numeric',
                  month: 'long',
                  year: 'numeric',
                  hour: '2-digit',
                  minute: '2-digit'
                })}
              </p>
            </div>
            <span className={`inline-block px-4 py-2 rounded-full text-sm font-medium ${getStatusColor(order.orderStatus)}`}>
              {order.orderStatus}
            </span>
          </div>
        </div>

        {/* Live Event Banner (InProgress only) */}
        {order.orderStatus === 'InProgress' && order.liveEventStatus && (
          <LiveEventBanner order={order} liveEventStatus={order.liveEventStatus} />
        )}

        {/* Post-Event Payment Section (Completed + supervisor report submitted) */}
        {order.orderStatus === 'Completed' && order.liveEventStatus?.supervisorReportSubmitted && (
          <PostEventPaymentSection order={order} liveEventStatus={order.liveEventStatus} />
        )}

        {/* Platform Protection Badge */}
        <PlatformProtectedBadge variant="compact" className="mb-4" />

        {/* Payment Timeline */}
        <PaymentTimeline order={order} layout="horizontal" />

        {/* Order Timeline */}
        <OrderTimeline order={order} />

        {/* Catering Info */}
        <div className="bg-white rounded-lg p-6 shadow-sm mb-4">
          <h2 className="font-semibold text-lg mb-4">Catering Service</h2>
          <div className="flex items-center gap-4">
            {order.cateringLogo && (
              <img
                src={`${API_BASE_URL}${ order.cateringLogo }`}
                alt={order.cateringName}
                className="w-20 h-20 rounded-lg object-cover"
              />
            )}
            <div>
              <h3 className="font-semibold text-xl">{order.cateringName}</h3>
            </div>
          </div>
        </div>

        {/* Event Details */}
        <div className="bg-white rounded-lg p-6 shadow-sm mb-4">
          <h2 className="font-semibold text-lg mb-4">Event Details</h2>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <p className="text-neutral-600 text-sm">Event Date</p>
              <p className="font-medium">{new Date(order.eventDate).toLocaleDateString('en-IN', {
                day: 'numeric',
                month: 'long',
                year: 'numeric'
              })}</p>
            </div>
            <div>
              <p className="text-neutral-600 text-sm">Event Time</p>
              <p className="font-medium">{order.eventTime}</p>
            </div>
            <div>
              <p className="text-neutral-600 text-sm">Event Type</p>
              <p className="font-medium">{order.eventType}</p>
            </div>
            <div>
              <p className="text-neutral-600 text-sm">Guest Count</p>
              <p className="font-medium">{order.guestCount}</p>
            </div>
            <div className="col-span-2">
              <p className="text-neutral-600 text-sm">Event Location</p>
              <p className="font-medium">{order.eventLocation}</p>
            </div>
            {order.specialInstructions && (
              <div className="col-span-2">
                <p className="text-neutral-600 text-sm">Special Instructions</p>
                <p className="font-medium">{order.specialInstructions}</p>
              </div>
            )}
          </div>
        </div>

        {/* Contact Information */}
        <div className="bg-white rounded-lg p-6 shadow-sm mb-4">
          <h2 className="font-semibold text-lg mb-4">Contact & Delivery</h2>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <p className="text-neutral-600 text-sm">Contact Person</p>
              <p className="font-medium">{order.contactPerson}</p>
            </div>
            <div>
              <p className="text-neutral-600 text-sm">Contact Phone</p>
              <p className="font-medium">{order.contactPhone}</p>
            </div>
            <div className="col-span-2">
              <p className="text-neutral-600 text-sm">Contact Email</p>
              <p className="font-medium">{order.contactEmail}</p>
            </div>
            <div className="col-span-2">
              <p className="text-neutral-600 text-sm">Delivery Address</p>
              <p className="font-medium">{order.deliveryAddress}</p>
            </div>
          </div>
        </div>

        {/* Order Items */}
        {order.orderItems && order.orderItems.length > 0 && (
          <div className="bg-white rounded-lg p-6 shadow-sm mb-4">
            <h2 className="font-semibold text-lg mb-4">Order Items</h2>
            <div className="space-y-3">
              {order.orderItems.map((item, index) => (
                <div key={index} className="flex justify-between border-b pb-3 last:border-b-0">
                  <div>
                    <p className="font-medium">{item.itemName}</p>
                    <p className="text-sm text-neutral-600">{item.itemType} × {item.quantity}</p>
                  </div>
                  <p className="font-semibold">₹{item.totalPrice.toFixed(2)}</p>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Payment Summary */}
        <div className="bg-white rounded-lg p-6 shadow-sm mb-4">
          <h2 className="font-semibold text-lg mb-4">Payment Summary</h2>
          <div className="space-y-2">
            <div className="flex justify-between">
              <span>Base Amount</span>
              <span>₹{order.baseAmount.toFixed(2)}</span>
            </div>
            <div className="flex justify-between">
              <span>Tax (18%)</span>
              <span>₹{order.taxAmount.toFixed(2)}</span>
            </div>
            {order.discountAmount > 0 && (
              <div className="flex justify-between text-green-600">
                <span>Discount</span>
                <span>-₹{order.discountAmount.toFixed(2)}</span>
              </div>
            )}
            <div className="flex justify-between font-bold text-lg border-t pt-2">
              <span>Total Amount</span>
              <span className="text-red-500">₹{order.totalAmount.toFixed(2)}</span>
            </div>
            <div className="flex justify-between text-sm text-neutral-600 mt-4">
              <span>Payment Method</span>
              <span className="font-medium">{order.paymentMethod}</span>
            </div>
            <div className="flex justify-between text-sm text-neutral-600">
              <span>Payment Status</span>
              <span className="font-medium">{order.paymentStatus}</span>
            </div>
          </div>
        </div>

        {/* Review Section */}
        {order.orderStatus === 'Completed' && !reviewCheckLoading && (
          <div className="bg-white rounded-lg p-6 shadow-sm mb-4">
            <h2 className="font-semibold text-lg mb-4">Your Review</h2>

            {existingReview ? (
              // Display existing review
              <div className="space-y-4">
                <div className="flex items-center justify-between">
                  <div>
                    <StarRating rating={existingReview.overallRating} readonly size="lg" showValue />
                    {existingReview.reviewTitle && (
                      <h3 className="font-semibold text-neutral-900 mt-2">{existingReview.reviewTitle}</h3>
                    )}
                  </div>
                  <span className="text-sm text-neutral-500">
                    {new Date(existingReview.reviewDate).toLocaleDateString('en-IN', {
                      day: 'numeric',
                      month: 'long',
                      year: 'numeric'
                    })}
                  </span>
                </div>

                {existingReview.reviewComment && (
                  <p className="text-neutral-700">{existingReview.reviewComment}</p>
                )}

                {/* Detailed Ratings */}
                {(existingReview.foodQualityRating || existingReview.hygieneRating ||
                  existingReview.staffBehaviorRating || existingReview.decorationRating ||
                  existingReview.punctualityRating) && (
                  <div className="grid grid-cols-2 gap-3 pt-3 border-t">
                    {existingReview.foodQualityRating > 0 && (
                      <div>
                        <StarRating
                          label="Food Quality"
                          rating={existingReview.foodQualityRating}
                          readonly
                          size="sm"
                        />
                      </div>
                    )}
                    {existingReview.hygieneRating > 0 && (
                      <div>
                        <StarRating
                          label="Hygiene"
                          rating={existingReview.hygieneRating}
                          readonly
                          size="sm"
                        />
                      </div>
                    )}
                    {existingReview.staffBehaviorRating > 0 && (
                      <div>
                        <StarRating
                          label="Staff Behavior"
                          rating={existingReview.staffBehaviorRating}
                          readonly
                          size="sm"
                        />
                      </div>
                    )}
                    {existingReview.decorationRating > 0 && (
                      <div>
                        <StarRating
                          label="Decoration"
                          rating={existingReview.decorationRating}
                          readonly
                          size="sm"
                        />
                      </div>
                    )}
                    {existingReview.punctualityRating > 0 && (
                      <div>
                        <StarRating
                          label="Punctuality"
                          rating={existingReview.punctualityRating}
                          readonly
                          size="sm"
                        />
                      </div>
                    )}
                  </div>
                )}

                {/* Owner Reply */}
                {existingReview.ownerReply && (
                  <div className="bg-neutral-50 p-4 rounded-lg border-l-4 border-red-500">
                    <p className="text-sm font-semibold text-neutral-900 mb-1">Response from {order.cateringName}</p>
                    <p className="text-neutral-700 text-sm">{existingReview.ownerReply}</p>
                    <p className="text-xs text-neutral-500 mt-2">
                      {new Date(existingReview.ownerReplyDate).toLocaleDateString('en-IN')}
                    </p>
                  </div>
                )}
              </div>
            ) : canReview ? (
              // Show review button
              <div className="text-center py-4">
                <p className="text-neutral-600 mb-4">
                  Your order has been completed! Share your experience with {order.cateringName}
                </p>
                <button
                  onClick={() => setShowReviewModal(true)}
                  className="px-6 py-3 bg-red-500 text-white rounded-lg hover:bg-red-600 transition-colors font-medium inline-flex items-center gap-2"
                >
                  <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                    <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                  </svg>
                  Write a Review
                </button>
              </div>
            ) : (
              <p className="text-neutral-500 text-center py-4">
                Reviews can only be submitted for completed orders
              </p>
            )}
          </div>
        )}

        {/* Cancel Order Button */}
        {canCancelOrder(order) && (
          <div className="bg-white rounded-lg p-6 shadow-sm mb-4">
            <button
              onClick={() => setShowCancelModal(true)}
              className="px-6 py-3 bg-red-500 text-white rounded-lg hover:bg-red-600 transition-colors"
            >
              Cancel Order
            </button>
            <p className="text-sm text-neutral-600 mt-2">
              You can cancel this order within 2 hours of placement
            </p>
          </div>
        )}

        {/* File Complaint Button */}
        {canFileComplaint(order) && (
          <div className="bg-white rounded-lg p-6 shadow-sm">
            <div className="flex items-start gap-4">
              <div className="flex-shrink-0 p-3 bg-amber-100 rounded-lg">
                <svg className="w-6 h-6 text-amber-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M8 10h.01M12 10h.01M16 10h.01M9 16H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-5l-5 5v-5z" />
                </svg>
              </div>
              <div className="flex-1">
                <h3 className="font-semibold text-lg mb-1">Have an Issue with Your Order?</h3>
                <p className="text-neutral-600 text-sm mb-4">
                  If you experienced any problems with the food quality, service, or delivery,
                  you can file a complaint. We take your concerns seriously and will review them promptly.
                </p>
                <button
                  onClick={() => navigate(`/complaints/file/${order.orderId}`)}
                  className="px-6 py-3 bg-amber-500 text-white rounded-lg hover:bg-amber-600 transition-colors font-medium inline-flex items-center gap-2"
                >
                  <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                  </svg>
                  File a Complaint
                </button>
                <p className="text-xs text-neutral-500 mt-2">
                  Complaints must be filed within 7 days of the event date
                </p>
              </div>
            </div>
          </div>
        )}
      </div>

      {/* Cancel Order Modal */}
      {showCancelModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg max-w-md w-full p-6">
            <h2 className="text-2xl font-bold mb-4">Cancel Order</h2>
            <p className="text-neutral-600 mb-4">
              Please provide a reason for cancellation:
            </p>
            <textarea
              value={cancelReason}
              onChange={(e) => setCancelReason(e.target.value)}
              placeholder="e.g., Change of plans, Found better option, etc."
              rows={4}
              className="w-full px-4 py-2 border border-neutral-300 rounded-lg focus:ring-2 focus:ring-red-500 focus:border-transparent mb-4"
            />
            <div className="flex gap-3">
              <button
                onClick={() => setShowCancelModal(false)}
                disabled={isCancelling}
                className="flex-1 px-6 py-3 border border-neutral-300 text-neutral-700 rounded-lg hover:bg-neutral-50 transition-colors disabled:opacity-50"
              >
                Keep Order
              </button>
              <button
                onClick={handleCancelOrder}
                disabled={isCancelling}
                className="flex-1 px-6 py-3 bg-red-500 text-white rounded-lg hover:bg-red-600 transition-colors disabled:opacity-50"
              >
                {isCancelling ? 'Cancelling...' : 'Cancel Order'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Review Submission Modal */}
      <ReviewSubmissionModal
        isOpen={showReviewModal}
        onClose={() => setShowReviewModal(false)}
        order={{
          orderId: order?.orderId,
          cateringId: order?.ownerId,
          cateringName: order?.cateringName
        }}
        onReviewSubmitted={handleReviewSubmitted}
      />
    </div>
  );
};

export default OrderDetailPage;
