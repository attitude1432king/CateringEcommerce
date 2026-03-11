import React, { useState, useEffect } from 'react';
import { Clock, Users, ChefHat, AlertCircle, CheckCircle, XCircle } from 'lucide-react';
import { DisabledButton } from '../../common/safety';

/**
 * PendingApprovalsWidget Component
 *
 * Displays pending approval requests for vendor dashboard:
 * - Menu change requests
 * - Guest count increase requests
 * - 2-hour response countdown timer
 */

const PendingApprovalsWidget = ({
  pendingApprovals = [],
  onApprove,
  onReject,
  isLoading = false
}) => {
  const [timeRemaining, setTimeRemaining] = useState({});

  // Calculate time remaining for each approval
  useEffect(() => {
    const interval = setInterval(() => {
      const now = new Date();
      const remaining = {};

      pendingApprovals.forEach(approval => {
        const deadline = new Date(approval.deadline || approval.requestedAt);
        const responseTime = approval.responseTimeHours || 2;
        deadline.setHours(deadline.getHours() + responseTime);

        const diff = deadline - now;

        if (diff > 0) {
          const hours = Math.floor(diff / (1000 * 60 * 60));
          const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
          remaining[approval.id] = { hours, minutes, isUrgent: hours < 1 };
        } else {
          remaining[approval.id] = { hours: 0, minutes: 0, isExpired: true };
        }
      });

      setTimeRemaining(remaining);
    }, 60000); // Update every minute

    // Initial calculation
    const now = new Date();
    const initial = {};
    pendingApprovals.forEach(approval => {
      const deadline = new Date(approval.deadline || approval.requestedAt);
      const responseTime = approval.responseTimeHours || 2;
      deadline.setHours(deadline.getHours() + responseTime);

      const diff = deadline - now;

      if (diff > 0) {
        const hours = Math.floor(diff / (1000 * 60 * 60));
        const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
        initial[approval.id] = { hours, minutes, isUrgent: hours < 1 };
      } else {
        initial[approval.id] = { hours: 0, minutes: 0, isExpired: true };
      }
    });
    setTimeRemaining(initial);

    return () => clearInterval(interval);
  }, [pendingApprovals]);

  // Get icon based on approval type
  const getApprovalIcon = (type) => {
    const icons = {
      'menu-change': ChefHat,
      'guest-increase': Users,
      'special-request': AlertCircle
    };
    return icons[type] || AlertCircle;
  };

  // Sort by urgency
  const sortedApprovals = [...pendingApprovals].sort((a, b) => {
    const timeA = timeRemaining[a.id];
    const timeB = timeRemaining[b.id];

    if (!timeA || !timeB) return 0;

    if (timeA.isExpired && !timeB.isExpired) return -1;
    if (!timeA.isExpired && timeB.isExpired) return 1;
    if (timeA.isUrgent && !timeB.isUrgent) return -1;
    if (!timeA.isUrgent && timeB.isUrgent) return 1;

    const totalA = timeA.hours * 60 + timeA.minutes;
    const totalB = timeB.hours * 60 + timeB.minutes;
    return totalA - totalB;
  });

  if (pendingApprovals.length === 0) {
    return (
      <div className="bg-white rounded-lg p-6 shadow-sm border border-gray-200">
        <h3 className="font-semibold text-lg mb-4 flex items-center gap-2">
          <CheckCircle className="w-5 h-5 text-green-600" />
          Pending Approvals
        </h3>
        <div className="text-center py-8">
          <CheckCircle className="w-12 h-12 text-green-500 mx-auto mb-3" />
          <p className="text-gray-600">No pending approvals</p>
          <p className="text-sm text-gray-500 mt-1">All requests have been processed</p>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg p-6 shadow-sm border border-gray-200">
      <div className="flex items-center justify-between mb-4">
        <h3 className="font-semibold text-lg flex items-center gap-2">
          <Clock className="w-5 h-5 text-orange-600" />
          Pending Approvals
        </h3>
        <span className="bg-orange-100 text-orange-800 text-sm font-semibold px-3 py-1 rounded-full">
          {pendingApprovals.length} pending
        </span>
      </div>

      <div className="space-y-3">
        {sortedApprovals.map(approval => {
          const Icon = getApprovalIcon(approval.type);
          const time = timeRemaining[approval.id] || { hours: 0, minutes: 0 };

          return (
            <div
              key={approval.id}
              className={`border-2 rounded-lg p-4 ${
                time.isExpired
                  ? 'border-red-300 bg-red-50'
                  : time.isUrgent
                  ? 'border-orange-300 bg-orange-50'
                  : 'border-gray-300 bg-white'
              }`}
            >
              {/* Header */}
              <div className="flex items-start justify-between mb-3">
                <div className="flex items-start gap-3 flex-1">
                  <div className={`
                    w-10 h-10 rounded-full flex items-center justify-center flex-shrink-0
                    ${time.isExpired ? 'bg-red-200' : time.isUrgent ? 'bg-orange-200' : 'bg-blue-100'}
                  `}>
                    <Icon className={`w-5 h-5 ${
                      time.isExpired ? 'text-red-700' : time.isUrgent ? 'text-orange-700' : 'text-blue-700'
                    }`} />
                  </div>

                  <div className="flex-1">
                    <h4 className="font-semibold text-gray-900 mb-1">
                      {approval.title || 'Request'}
                    </h4>
                    <p className="text-sm text-gray-600 mb-2">{approval.description}</p>

                    {/* Request Details */}
                    <div className="text-xs text-gray-600 space-y-1">
                      <p><strong>Order:</strong> #{approval.orderNumber}</p>
                      <p><strong>Customer:</strong> {approval.customerName}</p>
                      {approval.eventDate && (
                        <p><strong>Event Date:</strong> {new Date(approval.eventDate).toLocaleDateString()}</p>
                      )}
                    </div>
                  </div>
                </div>

                {/* Countdown Timer */}
                <div className={`text-right flex-shrink-0 ml-4`}>
                  {time.isExpired ? (
                    <div className="bg-red-600 text-white px-3 py-2 rounded-lg">
                      <p className="text-xs font-medium">EXPIRED</p>
                      <p className="text-xs">Auto-rejected</p>
                    </div>
                  ) : (
                    <div className={`px-3 py-2 rounded-lg ${
                      time.isUrgent ? 'bg-orange-600 animate-pulse' : 'bg-blue-600'
                    } text-white`}>
                      <p className="text-xs font-medium">Time Left</p>
                      <p className="text-lg font-bold">
                        {time.hours}h {time.minutes}m
                      </p>
                    </div>
                  )}
                </div>
              </div>

              {/* Additional Info */}
              {approval.additionalInfo && (
                <div className="bg-gray-100 rounded p-2 mb-3 text-xs text-gray-700">
                  {approval.additionalInfo}
                </div>
              )}

              {/* Action Buttons */}
              {!time.isExpired && (
                <div className="flex gap-2 mt-3">
                  <button
                    onClick={() => onApprove(approval.id)}
                    disabled={isLoading}
                    className="flex-1 px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors font-medium text-sm flex items-center justify-center gap-2 disabled:opacity-50"
                  >
                    <CheckCircle className="w-4 h-4" />
                    Approve
                  </button>
                  <button
                    onClick={() => onReject(approval.id)}
                    disabled={isLoading}
                    className="flex-1 px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors font-medium text-sm flex items-center justify-center gap-2 disabled:opacity-50"
                  >
                    <XCircle className="w-4 h-4" />
                    Reject
                  </button>
                </div>
              )}
            </div>
          );
        })}
      </div>

      {/* Urgent Warning */}
      {sortedApprovals.some(a => timeRemaining[a.id]?.isUrgent) && (
        <div className="mt-4 bg-orange-100 border border-orange-300 rounded-lg p-3">
          <div className="flex items-start gap-2">
            <AlertCircle className="w-5 h-5 text-orange-700 flex-shrink-0" />
            <p className="text-sm text-orange-900">
              <strong>Urgent action required!</strong> Some requests expire in less than 1 hour.
              Late responses may result in automatic rejection.
            </p>
          </div>
        </div>
      )}
    </div>
  );
};

export default PendingApprovalsWidget;
