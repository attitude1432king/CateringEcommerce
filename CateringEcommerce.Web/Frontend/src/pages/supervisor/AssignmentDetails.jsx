/**
 * AssignmentDetails Page
 * Single assignment detail view with actions
 */

import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft, CheckCircle, XCircle, LogIn, IndianRupee } from 'lucide-react';
import { getAssignmentById, requestPaymentRelease } from '../../services/api/supervisor/assignmentApi';
import { AssignmentStatusBadge, PaymentStatusBadge } from '../../components/supervisor/common/badges';
import { formatCurrency, formatTimestamp } from '../../utils/supervisor/helpers';
import { AssignmentStatus } from '../../utils/supervisor/supervisorEnums';
import AcceptRejectModal from '../../components/supervisor/assignments/AcceptRejectModal';
import CheckInComponent from '../../components/supervisor/assignments/CheckInComponent';
import toast from 'react-hot-toast';

const AssignmentDetails = () => {
  const { assignmentId } = useParams();
  const navigate = useNavigate();
  const [assignment, setAssignment] = useState(null);
  const [loading, setLoading] = useState(true);
  const [showAcceptModal, setShowAcceptModal] = useState(false);
  const [showRejectModal, setShowRejectModal] = useState(false);
  const [showCheckIn, setShowCheckIn] = useState(false);

  useEffect(() => {
    fetchAssignment();
  }, [assignmentId]);

  const fetchAssignment = async () => {
    try {
      const response = await getAssignmentById(assignmentId);
      if (response.success) {
        setAssignment(response.data);
      }
    } catch (error) {
      console.error('Failed to fetch assignment:', error);
      toast.error('Failed to load assignment');
    } finally {
      setLoading(false);
    }
  };

  const handleRequestPayment = async () => {
    try {
      const supervisorId = localStorage.getItem('supervisorId');
      const response = await requestPaymentRelease(
        assignmentId,
        supervisorId,
        assignment.supervisorFee
      );

      if (response.success) {
        toast.success('Payment release requested');
        fetchAssignment();
      } else {
        toast.error(response.message);
      }
    } catch (error) {
      toast.error('Failed to request payment');
    }
  };

  if (loading) {
    return (
      <div className="flex justify-center items-center min-h-screen">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  if (!assignment) {
    return (
      <div className="flex justify-center items-center min-h-screen">
        <p className="text-gray-600">Assignment not found</p>
      </div>
    );
  }

  const canAccept = assignment.assignmentStatus === AssignmentStatus.ASSIGNED;
  const canCheckIn = assignment.assignmentStatus === AssignmentStatus.ACCEPTED && !assignment.checkedIn;
  const canRequestPayment =
    assignment.assignmentStatus === AssignmentStatus.COMPLETED &&
    !assignment.paymentReleaseRequested;

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <button
          onClick={() => navigate('/supervisor/assignments')}
          className="flex items-center gap-2 text-gray-600 hover:text-gray-900 mb-6"
        >
          <ArrowLeft className="w-4 h-4" />
          Back to Assignments
        </button>

        {/* Main Card */}
        <div className="bg-white rounded-lg shadow-md p-6">
          {/* Title */}
          <div className="flex items-start justify-between mb-6">
            <div>
              <h1 className="text-2xl font-bold text-gray-900">
                {assignment.assignmentNumber}
              </h1>
              <p className="text-sm text-gray-600 mt-1">
                Order: {assignment.orderNumber}
              </p>
            </div>
            <div className="flex flex-col items-end gap-2">
              <AssignmentStatusBadge status={assignment.assignmentStatus} />
              {assignment.checkedIn && (
                <span className="text-xs text-green-600 flex items-center gap-1">
                  <CheckCircle className="w-3 h-3" />
                  Checked In
                </span>
              )}
            </div>
          </div>

          {/* Event Details */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
            <DetailRow label="Event Date" value={formatTimestamp(assignment.eventDate, 'long')} />
            <DetailRow label="Event Type" value={assignment.eventType} />
            <DetailRow label="Location" value={assignment.eventLocation} />
            <DetailRow label="Vendor" value={assignment.vendorName} />
            <DetailRow label="Supervisor Fee" value={formatCurrency(assignment.supervisorFee)} />
            <DetailRow
              label="Assigned Date"
              value={formatTimestamp(assignment.assignedDate, 'short')}
            />
          </div>

          {/* Notes */}
          {assignment.assignmentNotes && (
            <div className="mb-6">
              <h3 className="text-sm font-medium text-gray-700 mb-2">Notes</h3>
              <p className="text-sm text-gray-600 bg-gray-50 rounded-lg p-3">
                {assignment.assignmentNotes}
              </p>
            </div>
          )}

          {/* Payment Status */}
          {assignment.assignmentStatus === AssignmentStatus.COMPLETED && (
            <div className="mb-6 bg-blue-50 border border-blue-200 rounded-lg p-4">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium text-blue-900">Payment Status</p>
                  <div className="mt-1">
                    <PaymentStatusBadge
                      status={
                        assignment.paymentReleaseApproved
                          ? 'RELEASED'
                          : assignment.paymentReleaseRequested
                          ? 'PENDING'
                          : 'NOT_REQUESTED'
                      }
                    />
                  </div>
                </div>
                {canRequestPayment && (
                  <button
                    onClick={handleRequestPayment}
                    className="px-4 py-2 bg-blue-600 text-white rounded-lg text-sm font-medium hover:bg-blue-700 flex items-center gap-2"
                  >
                    <IndianRupee className="w-4 h-4" />
                    Request Payment
                  </button>
                )}
              </div>
            </div>
          )}

          {/* Actions */}
          <div className="flex gap-3">
            {canAccept && (
              <>
                <button
                  onClick={() => setShowAcceptModal(true)}
                  className="flex-1 px-4 py-2 bg-green-600 text-white rounded-lg font-medium hover:bg-green-700 flex items-center justify-center gap-2"
                >
                  <CheckCircle className="w-4 h-4" />
                  Accept Assignment
                </button>
                <button
                  onClick={() => setShowRejectModal(true)}
                  className="flex-1 px-4 py-2 bg-red-600 text-white rounded-lg font-medium hover:bg-red-700 flex items-center justify-center gap-2"
                >
                  <XCircle className="w-4 h-4" />
                  Reject Assignment
                </button>
              </>
            )}

            {canCheckIn && (
              <button
                onClick={() => setShowCheckIn(true)}
                className="flex-1 px-4 py-2 bg-blue-600 text-white rounded-lg font-medium hover:bg-blue-700 flex items-center justify-center gap-2"
              >
                <LogIn className="w-4 h-4" />
                Check In
              </button>
            )}

            {assignment.assignmentStatus === AssignmentStatus.IN_PROGRESS && (
              <button
                onClick={() => navigate(`/supervisor/event/${assignmentId}`)}
                className="flex-1 px-4 py-2 bg-blue-600 text-white rounded-lg font-medium hover:bg-blue-700"
              >
                Continue Event Supervision
              </button>
            )}
          </div>
        </div>
      </div>

      {/* Modals */}
      {showAcceptModal && (
        <AcceptRejectModal
          assignmentId={assignmentId}
          action="accept"
          onClose={() => setShowAcceptModal(false)}
          onSuccess={() => {
            setShowAcceptModal(false);
            fetchAssignment();
          }}
        />
      )}

      {showRejectModal && (
        <AcceptRejectModal
          assignmentId={assignmentId}
          action="reject"
          onClose={() => setShowRejectModal(false)}
          onSuccess={() => {
            setShowRejectModal(false);
            navigate('/supervisor/assignments');
          }}
        />
      )}

      {showCheckIn && (
        <CheckInComponent
          assignmentId={assignmentId}
          onClose={() => setShowCheckIn(false)}
          onSuccess={() => {
            setShowCheckIn(false);
            fetchAssignment();
          }}
        />
      )}
    </div>
  );
};

const DetailRow = ({ label, value }) => (
  <div>
    <p className="text-sm text-gray-600">{label}</p>
    <p className="text-base font-medium text-gray-900 mt-1">{value}</p>
  </div>
);

export default AssignmentDetails;
