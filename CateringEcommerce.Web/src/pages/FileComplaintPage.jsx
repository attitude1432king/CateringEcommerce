import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { getOrderDetails } from '../services/orderApi';
import { fileComplaint } from '../services/complaintApi';
import { ComplaintSubmissionWizard } from '../components/user/complaint';

const FileComplaintPage = () => {
  const navigate = useNavigate();
  const { orderId } = useParams();

  const [order, setOrder] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    fetchOrderDetails();
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
      console.error('Error fetching order:', error);
      setError('An error occurred while loading order details');
    } finally {
      setIsLoading(false);
    }
  };

  const handleSubmitComplaint = async (complaintData) => {
    setIsSubmitting(true);
    setError(null);

    try {
      // Map the wizard data to API format
      const apiPayload = {
        orderId: parseInt(orderId),
        complaintType: complaintData.type.toUpperCase(),
        complaintSummary: complaintData.description.substring(0, 200),
        complaintDetails: complaintData.description,
        photoEvidencePaths: complaintData.media
          .filter(m => m.file.type.startsWith('image/'))
          .map(m => m.file.name),
        videoEvidencePaths: complaintData.media
          .filter(m => m.file.type.startsWith('video/'))
          .map(m => m.file.name),
        affectedItems: complaintData.affectedItems || [],
        guestComplaintCount: complaintData.guestsAffected,
        issueOccurredAt: new Date().toISOString()
      };

      const response = await fileComplaint(apiPayload);

      if (response.success) {
        // Show success message and redirect to complaint detail page
        navigate(`/complaints/${response.data.complaintId}`, {
          state: {
            success: true,
            message: 'Complaint filed successfully!'
          }
        });
      } else {
        setError(response.message || 'Failed to file complaint');
      }
    } catch (error) {
      console.error('Error filing complaint:', error);
      setError(error.message || 'An error occurred while filing your complaint');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCancel = () => {
    navigate(`/orders/${orderId}`);
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

  if (error && !order) {
    return (
      <div className="min-h-screen bg-gray-100 py-8">
        <div className="max-w-4xl mx-auto px-4">
          <div className="bg-red-50 border border-red-200 text-red-800 px-6 py-4 rounded-lg">
            <h3 className="font-semibold mb-2">Error Loading Order</h3>
            <p>{error}</p>
            <button
              onClick={() => navigate('/my-orders')}
              className="mt-4 px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700 transition-colors"
            >
              Back to My Orders
            </button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-100 py-8">
      <div className="max-w-6xl mx-auto px-4">
        {/* Page Header */}
        <div className="mb-6">
          <h1 className="text-3xl font-bold mb-2">File a Complaint</h1>
          <p className="text-gray-600">
            Order #{order?.orderNumber} - {order?.cateringName}
          </p>
        </div>

        {/* Error Message */}
        {error && (
          <div className="mb-6 bg-red-50 border border-red-200 text-red-800 px-6 py-4 rounded-lg">
            {error}
          </div>
        )}

        {/* Complaint Wizard */}
        {order && (
          <ComplaintSubmissionWizard
            order={{
              orderId: order.orderId,
              orderNumber: order.orderNumber,
              totalAmount: order.totalAmount,
              guestCount: order.guestCount || 50
            }}
            onSubmit={handleSubmitComplaint}
            onCancel={handleCancel}
            isLoading={isSubmitting}
          />
        )}
      </div>
    </div>
  );
};

export default FileComplaintPage;
