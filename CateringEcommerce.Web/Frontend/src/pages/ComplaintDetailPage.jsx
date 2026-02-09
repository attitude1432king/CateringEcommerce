import React, { useState, useEffect } from 'react';
import { useNavigate, useParams, useLocation } from 'react-router-dom';
import { getComplaintDetails } from '../services/complaintApi';
import { ComplaintStatusTracker } from '../components/user/complaint';
import { ArrowLeft, CheckCircle } from 'lucide-react';

const ComplaintDetailPage = () => {
  const navigate = useNavigate();
  const { complaintId } = useParams();
  const location = useLocation();

  const [complaint, setComplaint] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);
  const [successMessage, setSuccessMessage] = useState(location.state?.success ? location.state.message : null);

  useEffect(() => {
    fetchComplaintDetails();
  }, [complaintId]);

  useEffect(() => {
    // Clear success message after 5 seconds
    if (successMessage) {
      const timer = setTimeout(() => {
        setSuccessMessage(null);
      }, 5000);
      return () => clearTimeout(timer);
    }
  }, [successMessage]);

  const fetchComplaintDetails = async () => {
    setIsLoading(true);
    setError(null);

    try {
      const response = await getComplaintDetails(complaintId);

      if (response.success && response.data) {
        // Transform backend data to match ComplaintStatusTracker expected format
        const transformedComplaint = {
          complaintId: response.data.complaintId,
          orderNumber: response.data.orderId,
          type: response.data.complaintType?.replace(/_/g, ' '),
          severity: response.data.severity?.toLowerCase(),
          description: response.data.complaintDetails || response.data.complaintSummary,
          submittedAt: response.data.reportedAt || response.data.createdDate,
          status: mapBackendStatusToFrontend(response.data.status),

          // Timeline events
          timeline: buildTimeline(response.data),

          // Partner response
          partnerResponse: response.data.partnerResponse,
          partnerEvidence: response.data.partnerOfferedReplacement ? [
            { url: '/placeholder-evidence.jpg' } // Placeholder - need actual evidence URLs
          ] : [],

          // Resolution details
          resolution: response.data.status?.toLowerCase() === 'resolved' ? {
            message: response.data.resolutionNotes || 'Your complaint has been resolved.',
            refundAmount: response.data.refundAmount || 0,
            refundStatus: response.data.refundAmount > 0 ? 'Processing' : 'N/A'
          } : null,

          // Rejection details
          rejectionDetails: response.data.status?.toLowerCase() === 'rejected' ? {
            reason: response.data.validityReason || 'Complaint did not meet resolution criteria',
            explanation: response.data.resolutionNotes
          } : null
        };

        setComplaint(transformedComplaint);
      } else {
        setError(response.message || 'Failed to load complaint details');
      }
    } catch (error) {
      console.error('Error fetching complaint:', error);
      setError('An error occurred while loading complaint details');
    } finally {
      setIsLoading(false);
    }
  };

  const mapBackendStatusToFrontend = (status) => {
    const statusMap = {
      'Open': 'under-review',
      'Under_Investigation': 'partner-notified',
      'Resolved': 'resolved-approved',
      'Rejected': 'rejected',
      'Escalated': 'partner-responded'
    };
    return statusMap[status] || 'under-review';
  };

  const buildTimeline = (data) => {
    const timeline = [];

    // Complaint filed
    timeline.push({
      title: 'Complaint Filed',
      description: 'Your complaint has been successfully submitted',
      timestamp: data.reportedAt || data.createdDate,
      type: 'success'
    });

    // Partner notified
    if (data.partnerNotifiedDate) {
      timeline.push({
        title: 'Partner Notified',
        description: 'The catering partner has been notified about your complaint',
        timestamp: data.partnerNotifiedDate,
        type: 'info'
      });
    }

    // Partner responded
    if (data.partnerResponseDate) {
      timeline.push({
        title: 'Partner Responded',
        description: data.partnerResponse ? 'Partner has provided their response' : 'Partner has acknowledged the complaint',
        timestamp: data.partnerResponseDate,
        type: 'info'
      });
    }

    // Admin review
    if (data.reviewedDate) {
      timeline.push({
        title: 'Admin Review Completed',
        description: 'Our team has completed the review of your complaint',
        timestamp: data.reviewedDate,
        type: 'info'
      });
    }

    // Resolution
    if (data.resolvedDate) {
      const isApproved = data.status?.toLowerCase() === 'resolved';
      timeline.push({
        title: isApproved ? 'Complaint Resolved' : 'Complaint Rejected',
        description: isApproved
          ? `Refund of ₹${data.refundAmount?.toFixed(2)} has been approved`
          : data.validityReason || 'Complaint has been rejected',
        timestamp: data.resolvedDate,
        type: isApproved ? 'success' : 'error'
      });
    }

    return timeline;
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-100 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-red-500 mx-auto"></div>
          <p className="mt-4 text-gray-600">Loading complaint details...</p>
        </div>
      </div>
    );
  }

  if (error || !complaint) {
    return (
      <div className="min-h-screen bg-gray-100 py-8">
        <div className="max-w-4xl mx-auto px-4">
          <button
            onClick={() => navigate('/complaints')}
            className="mb-4 flex items-center gap-2 text-blue-600 hover:text-blue-700 font-medium"
          >
            <ArrowLeft className="w-5 h-5" />
            Back to Complaints
          </button>

          <div className="bg-red-50 border border-red-200 text-red-800 px-6 py-4 rounded-lg">
            <h3 className="font-semibold mb-2">Error Loading Complaint</h3>
            <p>{error || 'Complaint not found'}</p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-100 py-8">
      <div className="max-w-5xl mx-auto px-4">
        {/* Navigation */}
        <button
          onClick={() => navigate('/complaints')}
          className="mb-4 flex items-center gap-2 text-blue-600 hover:text-blue-700 font-medium"
        >
          <ArrowLeft className="w-5 h-5" />
          Back to Complaints
        </button>

        {/* Success Message */}
        {successMessage && (
          <div className="mb-6 bg-green-50 border-2 border-green-300 text-green-800 px-6 py-4 rounded-lg flex items-center gap-3">
            <CheckCircle className="w-6 h-6 flex-shrink-0" />
            <div>
              <p className="font-semibold">{successMessage}</p>
              <p className="text-sm mt-1">We will review your complaint and notify you of the outcome.</p>
            </div>
          </div>
        )}

        {/* Complaint Status Tracker */}
        <ComplaintStatusTracker complaint={complaint} />

        {/* Additional Actions */}
        <div className="mt-6 bg-white rounded-lg p-6 shadow-sm">
          <h3 className="font-semibold mb-4">Need Help?</h3>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="border-2 border-gray-200 rounded-lg p-4">
              <h4 className="font-medium mb-2">Contact Support</h4>
              <p className="text-sm text-gray-600 mb-3">
                Have questions about your complaint? Our support team is here to help.
              </p>
              <button className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors text-sm">
                Contact Support
              </button>
            </div>

            <div className="border-2 border-gray-200 rounded-lg p-4">
              <h4 className="font-medium mb-2">View Order</h4>
              <p className="text-sm text-gray-600 mb-3">
                Review the original order details and delivery information.
              </p>
              <button
                onClick={() => navigate(`/orders/${complaint.orderNumber}`)}
                className="px-4 py-2 border-2 border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors text-sm"
              >
                View Order Details
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ComplaintDetailPage;
