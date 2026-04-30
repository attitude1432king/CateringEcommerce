import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { getUserOrders, cancelOrder } from '../services/orderApi';
import { X } from 'lucide-react';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL.replace(/\/$/, '');

const MyOrdersPage = () => {
  const navigate = useNavigate();
  const [orders, setOrders] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1); // P1 FIX: Track total pages for pagination
  const pageSize = 10;

  // P2 FIX: Cancel order modal state
  const [showCancelModal, setShowCancelModal] = useState(false);
  const [selectedOrderId, setSelectedOrderId] = useState(null);
  const [cancelReason, setCancelReason] = useState('');
  const [isCancelling, setIsCancelling] = useState(false);

  useEffect(() => {
    fetchOrders();
  }, [currentPage]);

  const fetchOrders = async () => {
    setIsLoading(true);
    setError(null);

    try {
      const response = await getUserOrders(currentPage, pageSize);

      if (response.result && response.data) {
        setOrders(response.data.orders || []);
        // P1 FIX: Set total pages for pagination controls
        setTotalPages(response.data.totalPages || 1);
      } else {
        setError(response.message || 'Failed to load orders');
      }
    } catch (error) {
      console.error('Error fetching orders:', error);
      setError('An error occurred while loading your orders');
    } finally {
      setIsLoading(false);
    }
  };

  // P2 FIX: Cancel order functions
  const handleCancelClick = (orderId, event) => {
    event.stopPropagation(); // Prevent navigation to order details
    setSelectedOrderId(orderId);
    setShowCancelModal(true);
  };

  const handleCancelOrder = async () => {
    if (!cancelReason.trim()) {
      alert('Please provide a reason for cancellation');
      return;
    }

    setIsCancelling(true);
    try {
      const response = await cancelOrder(selectedOrderId, cancelReason);

      if (response.result) {
        alert('Order cancelled successfully. Refund will be processed according to our cancellation policy.');
        setShowCancelModal(false);
        setCancelReason('');
        setSelectedOrderId(null);
        fetchOrders(); // Refresh orders list
      } else {
        alert(response.message || 'Failed to cancel order');
      }
    } catch (error) {
      console.error('Error cancelling order:', error);
      alert('An error occurred while cancelling the order');
    } finally {
      setIsCancelling(false);
    }
  };

  const canCancelOrder = (orderStatus) => {
    // Only allow cancellation for Pending, Confirmed, and InProgress orders
    return ['Pending', 'Confirmed', 'InProgress'].includes(orderStatus);
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
          <p className="mt-4 text-gray-600">Loading your orders...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-100 py-8">
      <div className="max-w-6xl mx-auto px-4">
        <h1 className="text-3xl font-bold mb-8">My Orders</h1>

        {error && (
          <div className="bg-red-50 border border-red-200 text-red-800 px-4 py-3 rounded-lg mb-6">
            {error}
          </div>
        )}

        {orders.length === 0 ? (
          <div className="bg-white rounded-lg p-12 text-center shadow-sm">
            <svg
              className="w-24 h-24 mx-auto text-gray-300 mb-4"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth="2"
                d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
              />
            </svg>
            <h2 className="text-2xl font-semibold mb-2">No orders yet</h2>
            <p className="text-gray-600 mb-6">Start browsing to place your first order</p>
            <button
              onClick={() => navigate('/')}
              className="px-6 py-3 bg-red-500 text-white rounded-lg hover:bg-red-600 transition-colors"
            >
              Browse Caterings
            </button>
          </div>
        ) : (
          <>
            <div className="space-y-4">
              {orders.map((order) => (
              <div
                key={order.orderId}
                className="bg-white rounded-lg p-6 shadow-sm hover:shadow-md transition-shadow cursor-pointer"
                onClick={() => navigate(`/orders/${order.orderId}`)}
              >
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <div className="flex items-center gap-4 mb-3">
                      {order.cateringLogo && (
                        <img
                          src={`${API_BASE_URL}${order.cateringLogo}`}
                          alt={order.cateringName}
                          className="w-16 h-16 rounded-lg object-cover"
                        />
                      )}
                      <div>
                        <h3 className="font-semibold text-lg">{order.cateringName}</h3>
                        <p className="text-sm text-gray-600">Order #{order.orderNumber}</p>
                      </div>
                    </div>

                    <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
                      <div>
                        <p className="text-gray-600">Event Date</p>
                        <p className="font-medium">
                          {new Date(order.eventDate).toLocaleDateString('en-IN', {
                            day: 'numeric',
                            month: 'short',
                            year: 'numeric'
                          })}
                        </p>
                      </div>
                      <div>
                        <p className="text-gray-600">Event Type</p>
                        <p className="font-medium">{order.eventType}</p>
                      </div>
                      <div>
                        <p className="text-gray-600">Guests</p>
                        <p className="font-medium">{order.guestCount}</p>
                      </div>
                      <div>
                        <p className="text-gray-600">Total Amount</p>
                        <p className="font-semibold text-lg text-red-500">
                          ₹{order.totalAmount.toFixed(2)}
                        </p>
                      </div>
                    </div>
                  </div>

                  <div className="ml-4 text-right flex flex-col items-end gap-2">
                    <span
                      className={`inline-block px-3 py-1 rounded-full text-sm font-medium ${getStatusColor(
                        order.orderStatus
                      )}`}
                    >
                      {order.orderStatus}
                    </span>
                    <p className="text-xs text-gray-500">
                      {order.paymentMethod}
                    </p>
                    {/* P2 FIX: Cancel order button */}
                    {canCancelOrder(order.orderStatus) && (
                      <button
                        onClick={(e) => handleCancelClick(order.orderId, e)}
                        className="px-3 py-1.5 bg-red-500 text-white text-xs font-medium rounded hover:bg-red-600 transition-colors"
                      >
                        Cancel Order
                      </button>
                    )}
                  </div>
                </div>
                </div>
              ))}
            </div>

            {/* P1 FIX: Add pagination controls */}
            {!isLoading && orders.length > 0 && totalPages > 1 && (
              <div className="flex items-center justify-center gap-2 mt-8">
                <button
                  onClick={() => setCurrentPage(prev => Math.max(1, prev - 1))}
                  disabled={currentPage === 1}
                  className="px-4 py-2 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  Previous
                </button>

                {[...Array(Math.min(5, totalPages))].map((_, index) => (
                  <button
                    key={index + 1}
                    onClick={() => setCurrentPage(index + 1)}
                    className={`px-4 py-2 rounded-lg transition-colors ${
                      currentPage === index + 1
                        ? 'bg-red-500 text-white'
                        : 'bg-white border border-gray-300 hover:bg-gray-50'
                    }`}
                  >
                    {index + 1}
                  </button>
                ))}

                <button
                  onClick={() => setCurrentPage(prev => Math.min(totalPages, prev + 1))}
                  disabled={currentPage === totalPages}
                  className="px-4 py-2 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  Next
                </button>
              </div>
            )}
          </>
        )}
      </div>

      {/* P2 FIX: Cancel Order Modal */}
      {showCancelModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg max-w-md w-full shadow-2xl">
            {/* Modal Header */}
            <div className="flex items-center justify-between p-6 border-b">
              <h2 className="text-xl font-bold text-gray-900">Cancel Order</h2>
              <button
                onClick={() => {
                  setShowCancelModal(false);
                  setCancelReason('');
                  setSelectedOrderId(null);
                }}
                className="text-gray-400 hover:text-gray-600 transition-colors"
              >
                <X className="w-5 h-5" />
              </button>
            </div>

            {/* Modal Body */}
            <div className="p-6 space-y-4">
              <div className="bg-yellow-50 border-l-4 border-yellow-400 p-4 rounded">
                <div className="flex">
                  <div className="flex-shrink-0">
                    <svg className="h-5 w-5 text-yellow-400" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
                      <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                    </svg>
                  </div>
                  <div className="ml-3">
                    <h3 className="text-sm font-medium text-yellow-800">Important</h3>
                    <div className="mt-2 text-sm text-yellow-700">
                      <p>Cancellation refund will be calculated based on:</p>
                      <ul className="list-disc list-inside mt-1 space-y-1">
                        <li>Time remaining before event</li>
                        <li>Current order status</li>
                        <li>Our cancellation policy</li>
                      </ul>
                    </div>
                  </div>
                </div>
              </div>

              <div>
                <label htmlFor="cancelReason" className="block text-sm font-medium text-gray-700 mb-2">
                  Reason for Cancellation <span className="text-red-500">*</span>
                </label>
                <textarea
                  id="cancelReason"
                  value={cancelReason}
                  onChange={(e) => setCancelReason(e.target.value)}
                  rows={4}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-transparent"
                  placeholder="Please provide a reason for cancelling this order..."
                />
              </div>
            </div>

            {/* Modal Footer */}
            <div className="flex items-center justify-end gap-3 p-6 border-t bg-gray-50">
              <button
                onClick={() => {
                  setShowCancelModal(false);
                  setCancelReason('');
                  setSelectedOrderId(null);
                }}
                className="px-4 py-2 text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors font-medium"
                disabled={isCancelling}
              >
                Keep Order
              </button>
              <button
                onClick={handleCancelOrder}
                disabled={isCancelling || !cancelReason.trim()}
                className="px-4 py-2 bg-red-500 text-white rounded-lg hover:bg-red-600 transition-colors font-medium disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isCancelling ? 'Cancelling...' : 'Confirm Cancellation'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default MyOrdersPage;
