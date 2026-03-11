/**
 * EventSupervisionSummary Component
 * Displays complete event supervision summary (Pre + During + Post)
 */

import { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import { ClipboardCheck, Clock, Users, Star, AlertTriangle, CheckCircle2, XCircle, FileText } from 'lucide-react';
import { eventSupervisionApi } from '../../../../services/api/supervisor';
import toast from 'react-hot-toast';

const EventSupervisionSummary = ({ assignmentId }) => {
  const [summary, setSummary] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadSummary();
  }, [assignmentId]);

  const loadSummary = async () => {
    try {
      const response = await eventSupervisionApi.getEventSupervisionSummary(assignmentId);
      if (response.success) {
        setSummary(response.data?.data);
      } else {
        toast.error('Failed to load summary');
      }
    } catch {
      toast.error('Error loading supervision summary');
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="bg-white rounded-lg shadow-md p-8 text-center">
        <div className="animate-spin w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full mx-auto" />
        <p className="text-sm text-gray-500 mt-4">Loading summary...</p>
      </div>
    );
  }

  if (!summary) {
    return (
      <div className="bg-white rounded-lg shadow-md p-8 text-center">
        <XCircle className="w-12 h-12 text-gray-300 mx-auto mb-3" />
        <p className="text-gray-500">No supervision summary available</p>
      </div>
    );
  }

  const StatusBadge = ({ status }) => {
    const styles = {
      COMPLETED: 'bg-green-100 text-green-800',
      VERIFIED: 'bg-blue-100 text-blue-800',
      PENDING: 'bg-yellow-100 text-yellow-800',
      FAILED: 'bg-red-100 text-red-800',
    };
    return (
      <span className={`text-xs px-2 py-0.5 rounded-full ${styles[status] || 'bg-gray-100 text-gray-800'}`}>
        {status}
      </span>
    );
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="bg-white rounded-lg shadow-md p-6">
        <div className="flex items-center gap-3 mb-4">
          <ClipboardCheck className="w-6 h-6 text-blue-600" />
          <h2 className="text-xl font-semibold text-gray-900">Event Supervision Summary</h2>
        </div>

        {/* Quick Stats */}
        <div className="grid grid-cols-4 gap-4">
          <div className="text-center p-3 bg-gray-50 rounded-lg">
            <Clock className="w-5 h-5 text-gray-600 mx-auto mb-1" />
            <p className="text-lg font-bold text-gray-900">{summary.totalDuration || '--'}</p>
            <p className="text-xs text-gray-500">Duration</p>
          </div>
          <div className="text-center p-3 bg-gray-50 rounded-lg">
            <Users className="w-5 h-5 text-purple-600 mx-auto mb-1" />
            <p className="text-lg font-bold text-gray-900">{summary.actualGuestCount || '--'}</p>
            <p className="text-xs text-gray-500">Guests Served</p>
          </div>
          <div className="text-center p-3 bg-gray-50 rounded-lg">
            <Star className="w-5 h-5 text-yellow-500 mx-auto mb-1" />
            <p className="text-lg font-bold text-gray-900">{summary.overallRating || '--'}/5</p>
            <p className="text-xs text-gray-500">Rating</p>
          </div>
          <div className="text-center p-3 bg-gray-50 rounded-lg">
            <AlertTriangle className="w-5 h-5 text-red-500 mx-auto mb-1" />
            <p className="text-lg font-bold text-gray-900">{summary.issuesCount || 0}</p>
            <p className="text-xs text-gray-500">Issues</p>
          </div>
        </div>
      </div>

      {/* Pre-Event Summary */}
      <div className="bg-white rounded-lg shadow-md p-6">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-semibold text-gray-900">Pre-Event Verification</h3>
          <StatusBadge status={summary.preEvent?.status || 'PENDING'} />
        </div>
        {summary.preEvent ? (
          <div className="space-y-2">
            <div className="flex justify-between text-sm">
              <span className="text-gray-600">Checklist Items Completed</span>
              <span className="font-medium">{summary.preEvent.completedItems || 0}/{summary.preEvent.totalItems || 0}</span>
            </div>
            <div className="w-full bg-gray-200 rounded-full h-2">
              <div
                className="bg-green-500 h-2 rounded-full"
                style={{ width: `${summary.preEvent.totalItems ? (summary.preEvent.completedItems / summary.preEvent.totalItems) * 100 : 0}%` }}
              />
            </div>
            {summary.preEvent.criticalIssues > 0 && (
              <p className="text-sm text-red-600">{summary.preEvent.criticalIssues} critical issues noted</p>
            )}
          </div>
        ) : (
          <p className="text-sm text-gray-500">Not yet completed</p>
        )}
      </div>

      {/* During-Event Summary */}
      <div className="bg-white rounded-lg shadow-md p-6">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-semibold text-gray-900">During-Event Monitoring</h3>
          <StatusBadge status={summary.duringEvent?.status || 'PENDING'} />
        </div>
        {summary.duringEvent ? (
          <div className="space-y-3">
            <div className="grid grid-cols-2 gap-3">
              <div className="bg-gray-50 rounded-lg p-3">
                <p className="text-xs text-gray-500">Food Stages Monitored</p>
                <p className="text-lg font-bold">{summary.duringEvent.foodStagesCompleted || 0}</p>
              </div>
              <div className="bg-gray-50 rounded-lg p-3">
                <p className="text-xs text-gray-500">Guest Count Updates</p>
                <p className="text-lg font-bold">{summary.duringEvent.guestCountUpdates || 0}</p>
              </div>
              <div className="bg-gray-50 rounded-lg p-3">
                <p className="text-xs text-gray-500">Extra Qty Requests</p>
                <p className="text-lg font-bold">{summary.duringEvent.extraQuantityRequests || 0}</p>
              </div>
              <div className="bg-gray-50 rounded-lg p-3">
                <p className="text-xs text-gray-500">Issues Reported</p>
                <p className="text-lg font-bold">{summary.duringEvent.issuesReported || 0}</p>
              </div>
            </div>
          </div>
        ) : (
          <p className="text-sm text-gray-500">Not yet started</p>
        )}
      </div>

      {/* Post-Event Summary */}
      <div className="bg-white rounded-lg shadow-md p-6">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-semibold text-gray-900">Post-Event Report</h3>
          <StatusBadge status={summary.postEvent?.status || 'PENDING'} />
        </div>
        {summary.postEvent ? (
          <div className="space-y-3">
            <div className="grid grid-cols-2 gap-3">
              {['foodQuality', 'service', 'hygiene', 'punctuality'].map((key) => (
                <div key={key} className="flex items-center justify-between text-sm">
                  <span className="text-gray-600 capitalize">{key.replace(/([A-Z])/g, ' $1')}</span>
                  <div className="flex gap-0.5">
                    {[1, 2, 3, 4, 5].map((s) => (
                      <Star
                        key={s}
                        className={`w-4 h-4 ${s <= (summary.postEvent[`${key}Rating`] || 0) ? 'text-yellow-400 fill-yellow-400' : 'text-gray-200'}`}
                      />
                    ))}
                  </div>
                </div>
              ))}
            </div>
            {summary.postEvent.clientFeedback && (
              <div className="bg-gray-50 rounded-lg p-3 mt-3">
                <p className="text-xs text-gray-500 mb-1">Client Feedback</p>
                <p className="text-sm text-gray-700">{summary.postEvent.clientFeedback}</p>
              </div>
            )}
          </div>
        ) : (
          <p className="text-sm text-gray-500">Not yet submitted</p>
        )}
      </div>

      {/* Payment Info */}
      {summary.payment && (
        <div className="bg-white rounded-lg shadow-md p-6">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-lg font-semibold text-gray-900">Payment</h3>
            <StatusBadge status={summary.payment.status || 'PENDING'} />
          </div>
          <div className="grid grid-cols-3 gap-4">
            <div className="text-center p-3 bg-green-50 rounded-lg">
              <p className="text-xs text-gray-500">Base Amount</p>
              <p className="text-lg font-bold text-green-700">
                {'\u20B9'}{summary.payment.baseAmount?.toLocaleString() || '0'}
              </p>
            </div>
            <div className="text-center p-3 bg-blue-50 rounded-lg">
              <p className="text-xs text-gray-500">Bonus</p>
              <p className="text-lg font-bold text-blue-700">
                {'\u20B9'}{summary.payment.bonus?.toLocaleString() || '0'}
              </p>
            </div>
            <div className="text-center p-3 bg-purple-50 rounded-lg">
              <p className="text-xs text-gray-500">Total</p>
              <p className="text-lg font-bold text-purple-700">
                {'\u20B9'}{summary.payment.total?.toLocaleString() || '0'}
              </p>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

EventSupervisionSummary.propTypes = {
  assignmentId: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired,
};

export default EventSupervisionSummary;
