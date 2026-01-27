import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { getOrderDetails, cancelOrder } from '../services/orderApi';

const OrderDetailPage = () => {
  const { orderId } = useParams();
  const navigate = useNavigate();
  const [order, setOrder] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);
  const [isCancelling, setIsCancelling] = useState(false);
  const [showCancelModal, setShowCancelModal] = useState(false);
  const [cancelReason, setCancelReason] = useState('');

  useEffect(() => {
    if (orderId) {
      fetchOrderDetails();
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

  const handleCancelOrder = async () => {
    if (!cancelReason.trim()) {
      alert('Please provide a cancellation reason');
      return;
    }

    setIsCancelling(true);

    try {
      const response = await cancelOrder(orderId, cancelReason);

      if (response.result) {
        alert('Order cancelled successfully');
        setShowCancelModal(false);
        fetchOrderDetails(); // Refresh order details
      } else {
        alert(response.message || 'Failed to cancel order');
      }
    } catch (error) {
      console.error('Error cancelling order:', error);
      alert(error.message || 'An error occurred while cancelling the order');
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

  const getStatusColor = (status) => {
    const colors = {
      Pending: 'bg-yellow-100 text-yellow-800',
      Confirmed: 'bg-blue-100 text-blue-800',
      InProgress: 'bg-purple-100 text-purple-800',
      Completed: 'bg-green-100 text-green-800',
      Cancelled: 'bg-red-100 text-red-800'
    };
    return colors[status] || 'bg-gray-100 text-gray-800';
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-100 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-red-500 mx-auto"></div>
          <p className="mt-4 text-gray-600">Loading order details...</p>
        </div>
      </div>
    );
  }

  if (error || !order) {
    return (
      <div className="min-h-screen bg-gray-100 py-8">
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
    <div className="min-h-screen bg-gray-100 py-8">
      <div className="max-w-4xl mx-auto px-4">
        {/* Header */}
        <div className="mb-6">
          <button
            onClick={() => navigate('/my-orders')}
            className="text-gray-600 hover:text-gray-900 mb-4 flex items-center"
          >
            <svg className="w-5 h-5 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M15 19l-7-7 7-7" />
            </svg>
            Back to Orders
          </button>
          <div className="flex justify-between items-start">
            <div>
              <h1 className="text-3xl font-bold">Order #{order.orderNumber}</h1>
              <p className="text-gray-600 mt-1">
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

        {/* Catering Info */}
        <div className="bg-white rounded-lg p-6 shadow-sm mb-4">
          <h2 className="font-semibold text-lg mb-4">Catering Service</h2>
          <div className="flex items-center gap-4">
            {order.cateringLogo && (
              <img
                src={order.cateringLogo}
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
              <p className="text-gray-600 text-sm">Event Date</p>
              <p className="font-medium">{new Date(order.eventDate).toLocaleDateString('en-IN', {
                day: 'numeric',
                month: 'long',
                year: 'numeric'
              })}</p>
            </div>
            <div>
              <p className="text-gray-600 text-sm">Event Time</p>
              <p className="font-medium">{order.eventTime}</p>
            </div>
            <div>
              <p className="text-gray-600 text-sm">Event Type</p>
              <p className="font-medium">{order.eventType}</p>
            </div>
            <div>
              <p className="text-gray-600 text-sm">Guest Count</p>
              <p className="font-medium">{order.guestCount}</p>
            </div>
            <div className="col-span-2">
              <p className="text-gray-600 text-sm">Event Location</p>
              <p className="font-medium">{order.eventLocation}</p>
            </div>
            {order.specialInstructions && (
              <div className="col-span-2">
                <p className="text-gray-600 text-sm">Special Instructions</p>
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
              <p className="text-gray-600 text-sm">Contact Person</p>
              <p className="font-medium">{order.contactPerson}</p>
            </div>
            <div>
              <p className="text-gray-600 text-sm">Contact Phone</p>
              <p className="font-medium">{order.contactPhone}</p>
            </div>
            <div className="col-span-2">
              <p className="text-gray-600 text-sm">Contact Email</p>
              <p className="font-medium">{order.contactEmail}</p>
            </div>
            <div className="col-span-2">
              <p className="text-gray-600 text-sm">Delivery Address</p>
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
                    <p className="text-sm text-gray-600">{item.itemType} × {item.quantity}</p>
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
            <div className="flex justify-between text-sm text-gray-600 mt-4">
              <span>Payment Method</span>
              <span className="font-medium">{order.paymentMethod}</span>
            </div>
            <div className="flex justify-between text-sm text-gray-600">
              <span>Payment Status</span>
              <span className="font-medium">{order.paymentStatus}</span>
            </div>
          </div>
        </div>

        {/* Cancel Order Button */}
        {canCancelOrder(order) && (
          <div className="bg-white rounded-lg p-6 shadow-sm">
            <button
              onClick={() => setShowCancelModal(true)}
              className="px-6 py-3 bg-red-500 text-white rounded-lg hover:bg-red-600 transition-colors"
            >
              Cancel Order
            </button>
            <p className="text-sm text-gray-600 mt-2">
              You can cancel this order within 2 hours of placement
            </p>
          </div>
        )}
      </div>

      {/* Cancel Order Modal */}
      {showCancelModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg max-w-md w-full p-6">
            <h2 className="text-2xl font-bold mb-4">Cancel Order</h2>
            <p className="text-gray-600 mb-4">
              Please provide a reason for cancellation:
            </p>
            <textarea
              value={cancelReason}
              onChange={(e) => setCancelReason(e.target.value)}
              placeholder="e.g., Change of plans, Found better option, etc."
              rows={4}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-red-500 focus:border-transparent mb-4"
            />
            <div className="flex gap-3">
              <button
                onClick={() => setShowCancelModal(false)}
                disabled={isCancelling}
                className="flex-1 px-6 py-3 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors disabled:opacity-50"
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
    </div>
  );
};

export default OrderDetailPage;
