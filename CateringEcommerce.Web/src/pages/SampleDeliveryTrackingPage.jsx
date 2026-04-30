import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import SampleDeliveryTracking from '../components/user/delivery/SampleDeliveryTracking';

/**
 * Sample Delivery Tracking Page
 * Dedicated page for tracking sample food delivery
 */
const SampleDeliveryTrackingPage = () => {
  const { orderId } = useParams();
  const navigate = useNavigate();
  const { user } = useAuth();
  const [orderInfo, setOrderInfo] = useState(null);
  const [loading, setLoading] = useState(true);

  const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

  useEffect(() => {
    if (!user) {
      navigate('/');
      return;
    }
    fetchOrderInfo();
  }, [orderId, user]);

  const fetchOrderInfo = async () => {
    try {
      setLoading(true);

      const response = await fetch(`${API_BASE_URL}/api/User/Orders/${orderId}`, {
        method: 'GET',
        credentials: 'include',
        headers: {
          'Content-Type': 'application/json'
        }
      });

      const data = await response.json();

      if (data.success) {
        setOrderInfo(data.data);
      }
    } catch (err) {
      console.error('Error fetching order info:', err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-neutral-50 py-8">
      <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <div className="mb-8">
          <button
            onClick={() => navigate(`/orders/${orderId}`)}
            className="inline-flex items-center gap-2 text-neutral-600 hover:text-rose-600 transition mb-4"
          >
            <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 19l-7-7m0 0l7-7m-7 7h18" />
            </svg>
            Back to Order Details
          </button>

          <div className="flex items-center gap-4 mb-2">
            <div className="p-3 bg-rose-100 rounded-lg">
              <svg className="h-8 w-8 text-rose-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v13m0-13V6a2 2 0 112 2h-2zm0 0V5.5A2.5 2.5 0 109.5 8H12zm-7 4h14M5 12a2 2 0 110-4h14a2 2 0 110 4M5 12v7a2 2 0 002 2h10a2 2 0 002-2v-7" />
              </svg>
            </div>
            <div>
              <h1 className="text-3xl font-bold text-neutral-800">Sample Delivery Tracking</h1>
              <p className="text-neutral-600">Order ID: {orderId}</p>
            </div>
          </div>

          {orderInfo && (
            <div className="mt-4 flex flex-wrap gap-4 text-sm">
              <div className="flex items-center gap-2">
                <svg className="h-4 w-4 text-neutral-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
                </svg>
                <span className="text-neutral-600">
                  <strong className="text-neutral-800">Catering:</strong> {orderInfo.cateringName}
                </span>
              </div>
              <div className="flex items-center gap-2">
                <svg className="h-4 w-4 text-neutral-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
                <span className="text-neutral-600">
                  <strong className="text-neutral-800">Event Date:</strong>{' '}
                  {new Date(orderInfo.eventDate).toLocaleDateString()}
                </span>
              </div>
            </div>
          )}
        </div>

        {/* Info Card */}
        <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-6 flex gap-3">
          <svg className="h-6 w-6 text-blue-600 flex-shrink-0 mt-0.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <div>
            <h3 className="text-sm font-semibold text-blue-800 mb-1">About Sample Delivery</h3>
            <p className="text-sm text-blue-700">
              Sample deliveries allow you to taste the food before your event. Tracking updates automatically
              every 30 seconds. Contact the catering partner if you have any questions about your sample.
            </p>
          </div>
        </div>

        {/* Tracking Component */}
        {loading ? (
          <div className="bg-white rounded-lg shadow-sm border border-neutral-200 p-12 text-center">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-rose-600 mx-auto mb-4"></div>
            <p className="text-neutral-600">Loading tracking information...</p>
          </div>
        ) : (
          <div className="bg-white rounded-lg shadow-sm border border-neutral-200 overflow-hidden">
            <SampleDeliveryTracking orderId={orderId} />
          </div>
        )}

        {/* Help Section */}
        <div className="mt-8 grid grid-cols-1 md:grid-cols-2 gap-6">
          <div className="bg-white rounded-lg shadow-sm border border-neutral-200 p-6">
            <div className="flex items-start gap-3">
              <div className="p-2 bg-green-100 rounded-lg">
                <svg className="h-5 w-5 text-green-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
              <div>
                <h4 className="font-semibold text-neutral-800 mb-2">What to Expect</h4>
                <ul className="text-sm text-neutral-600 space-y-1">
                  <li>• Sample portions of selected menu items</li>
                  <li>• Delivered by third-party courier</li>
                  <li>• Typically arrives within 60-90 minutes</li>
                  <li>• Taste and provide feedback to partner</li>
                </ul>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-lg shadow-sm border border-neutral-200 p-6">
            <div className="flex items-start gap-3">
              <div className="p-2 bg-rose-100 rounded-lg">
                <svg className="h-5 w-5 text-rose-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M18.364 5.636l-3.536 3.536m0 5.656l3.536 3.536M9.172 9.172L5.636 5.636m3.536 9.192l-3.536 3.536M21 12a9 9 0 11-18 0 9 9 0 0118 0zm-5 0a4 4 0 11-8 0 4 4 0 018 0z" />
                </svg>
              </div>
              <div>
                <h4 className="font-semibold text-neutral-800 mb-2">Need Help?</h4>
                <p className="text-sm text-neutral-600 mb-3">
                  If your sample is delayed or you have questions:
                </p>
                <button
                  onClick={() => navigate(`/orders/${orderId}`)}
                  className="text-sm text-rose-600 hover:text-rose-700 font-medium"
                >
                  Contact Catering Partner →
                </button>
              </div>
            </div>
          </div>
        </div>

        {/* After Delivery */}
        <div className="mt-6 bg-gradient-to-r from-rose-50 to-orange-50 border border-rose-200 rounded-lg p-6">
          <h3 className="text-lg font-semibold text-neutral-800 mb-3">After You Receive Your Sample</h3>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 text-sm">
            <div className="flex items-start gap-2">
              <span className="flex-shrink-0 w-6 h-6 bg-rose-600 text-white rounded-full flex items-center justify-center text-xs font-bold">1</span>
              <p className="text-neutral-700">
                <strong>Taste & Evaluate</strong><br />
                Try each item carefully
              </p>
            </div>
            <div className="flex items-start gap-2">
              <span className="flex-shrink-0 w-6 h-6 bg-rose-600 text-white rounded-full flex items-center justify-center text-xs font-bold">2</span>
              <p className="text-neutral-700">
                <strong>Provide Feedback</strong><br />
                Share your thoughts with partner
              </p>
            </div>
            <div className="flex items-start gap-2">
              <span className="flex-shrink-0 w-6 h-6 bg-rose-600 text-white rounded-full flex items-center justify-center text-xs font-bold">3</span>
              <p className="text-neutral-700">
                <strong>Finalize Menu</strong><br />
                Confirm or adjust your order
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default SampleDeliveryTrackingPage;
