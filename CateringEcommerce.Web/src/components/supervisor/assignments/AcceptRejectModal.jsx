/**
 * AcceptRejectModal Component
 * Modal for accepting or rejecting assignments
 */

import { useState } from 'react';
import PropTypes from 'prop-types';
import { X } from 'lucide-react';
import { acceptAssignment, rejectAssignment } from '../../../services/api/supervisor/assignmentApi';

import toast from 'react-hot-toast';

const AcceptRejectModal = ({ assignmentId, action, onClose, onSuccess }) => {
  const [reason, setReason] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async () => {
    if (action === 'reject' && reason.trim().length < 10) {
      toast.error('Please provide a reason (minimum 10 characters)');
      return;
    }

    setSubmitting(true);
    try {
      const supervisorId = localStorage.getItem('supervisorId');

      const response =
        action === 'accept'
          ? await acceptAssignment(assignmentId, supervisorId)
          : await rejectAssignment(assignmentId, supervisorId, reason);

      if (response.success) {
        toast.success(
          action === 'accept' ? 'Assignment accepted' : 'Assignment rejected'
        );
        onSuccess();
      } else {
        toast.error(response.message);
      }
    } catch (error) {
      console.error('Action error:', error);
      toast.error(`Failed to ${action} assignment`);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-lg max-w-md w-full p-6">
        {/* Header */}
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-xl font-semibold text-gray-900">
            {action === 'accept' ? 'Accept Assignment' : 'Reject Assignment'}
          </h2>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Content */}
        {action === 'accept' ? (
          <div className="mb-6">
            <p className="text-gray-600">
              Are you sure you want to accept this assignment? You will be expected to attend the event on the scheduled date.
            </p>
          </div>
        ) : (
          <div className="mb-6">
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Reason for Rejection <span className="text-red-500">*</span>
            </label>
            <textarea
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              rows={4}
              className="block w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="Please provide a detailed reason..."
            />
            <p className="text-xs text-gray-500 mt-1">
              Minimum 10 characters required
            </p>
          </div>
        )}

        {/* Actions */}
        <div className="flex gap-3">
          <button
            onClick={onClose}
            disabled={submitting}
            className="flex-1 px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50"
          >
            Cancel
          </button>
          <button
            onClick={handleSubmit}
            disabled={submitting}
            className={`flex-1 px-4 py-2 rounded-lg text-sm font-medium text-white disabled:opacity-50 ${
              action === 'accept'
                ? 'bg-green-600 hover:bg-green-700'
                : 'bg-red-600 hover:bg-red-700'
            }`}
          >
            {submitting
              ? 'Processing...'
              : action === 'accept'
              ? 'Accept'
              : 'Reject'}
          </button>
        </div>
      </div>
    </div>
  );
};

AcceptRejectModal.propTypes = {
  assignmentId: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired,
  action: PropTypes.oneOf(['accept', 'reject']).isRequired,
  onClose: PropTypes.func.isRequired,
  onSuccess: PropTypes.func.isRequired,
};

export default AcceptRejectModal;
