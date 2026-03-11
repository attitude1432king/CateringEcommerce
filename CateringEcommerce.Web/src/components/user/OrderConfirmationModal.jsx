import React from 'react';
import { useNavigate } from 'react-router-dom';

const OrderConfirmationModal = ({ order, onClose }) => {
  const navigate = useNavigate();

  const handleViewOrder = () => {
    navigate(`/orders/${order.orderId}`);
    onClose();
  };

  const handleContinueBrowsing = () => {
    navigate('/');
    onClose();
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-lg max-w-md w-full p-6 shadow-xl">
        {/* Success Icon */}
        <div className="flex justify-center mb-4">
          <div className="w-20 h-20 bg-green-100 rounded-full flex items-center justify-center">
            <svg
              className="w-12 h-12 text-green-500"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth="2"
                d="M5 13l4 4L19 7"
              />
            </svg>
          </div>
        </div>

        {/* Success Message */}
        <h2 className="text-2xl font-bold text-center text-gray-900 mb-2">
          Order Placed Successfully!
        </h2>
        <p className="text-center text-gray-600 mb-6">
          Thank you for your order. We have received your request.
        </p>

        {/* Order Details */}
        <div className="bg-gray-50 rounded-lg p-4 mb-6">
          <div className="space-y-2">
            <div className="flex justify-between">
              <span className="text-gray-600">Order Number:</span>
              <span className="font-semibold text-red-500">{order.orderNumber}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-gray-600">Event Date:</span>
              <span className="font-medium">
                {new Date(order.eventDate).toLocaleDateString('en-IN', {
                  day: 'numeric',
                  month: 'long',
                  year: 'numeric'
                })}
              </span>
            </div>
            <div className="flex justify-between">
              <span className="text-gray-600">Total Amount:</span>
              <span className="font-semibold text-lg">₹{order.totalAmount.toFixed(2)}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-gray-600">Payment Method:</span>
              <span className="font-medium">{order.paymentMethod}</span>
            </div>
            {order.paymentMethod === 'BankTransfer' && (
              <div className="flex justify-between">
                <span className="text-gray-600">Payment Status:</span>
                <span className="font-medium text-yellow-600">{order.paymentStatus}</span>
              </div>
            )}
          </div>
        </div>

        {/* Next Steps */}
        <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-6">
          <h3 className="font-semibold text-blue-900 mb-2">What's Next?</h3>
          <ul className="text-sm text-blue-800 space-y-1">
            <li>• You will receive an email and SMS confirmation shortly</li>
            <li>• The catering service will contact you to confirm details</li>
            <li>• You can track your order status in "My Orders"</li>
            {order.paymentMethod === 'BankTransfer' && (
              <li>• Your payment proof will be verified within 24 hours</li>
            )}
          </ul>
        </div>

        {/* Action Buttons */}
        <div className="flex flex-col space-y-3">
          <button
            onClick={handleViewOrder}
            className="w-full px-6 py-3 bg-red-500 text-white rounded-lg hover:bg-red-600 transition-colors font-medium"
          >
            View Order Details
          </button>
          <button
            onClick={handleContinueBrowsing}
            className="w-full px-6 py-3 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors font-medium"
          >
            Continue Browsing
          </button>
        </div>
      </div>
    </div>
  );
};

export default OrderConfirmationModal;
