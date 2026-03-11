import React from 'react';
import {
  Clock,
  MessageSquare,
  CheckCircle,
  XCircle,
  FileText,
  DollarSign,
  AlertTriangle
} from 'lucide-react';

/**
 * ComplaintStatusTracker Component
 *
 * Track complaint resolution progress:
 * - Under Review
 * - Partner Responded
 * - Resolved
 * - Rejected (with reason)
 */

const ComplaintStatusTracker = ({ complaint }) => {
  // Status configuration
  const statusConfig = {
    'under-review': {
      label: 'Under Review',
      icon: Clock,
      color: 'blue',
      description: 'Our team is reviewing your complaint and evidence'
    },
    'partner-notified': {
      label: 'Partner Notified',
      icon: MessageSquare,
      color: 'purple',
      description: 'Partner has been notified and has 24 hours to respond'
    },
    'partner-responded': {
      label: 'Partner Responded',
      icon: FileText,
      color: 'amber',
      description: 'Partner has provided their response. Final review in progress.'
    },
    'resolved-approved': {
      label: 'Resolved - Approved',
      icon: CheckCircle,
      color: 'green',
      description: 'Complaint approved. Refund processed.'
    },
    'resolved-partial': {
      label: 'Resolved - Partial Approval',
      icon: DollarSign,
      color: 'green',
      description: 'Complaint partially approved with adjusted refund'
    },
    'rejected': {
      label: 'Rejected',
      icon: XCircle,
      color: 'red',
      description: 'Complaint rejected after review'
    }
  };

  const currentStatus = statusConfig[complaint.status] || statusConfig['under-review'];
  const StatusIcon = currentStatus.icon;

  // Timeline events
  const timelineEvents = complaint.timeline || [];

  return (
    <div className="bg-white rounded-lg shadow-sm border-2 border-gray-200 p-6">
      {/* Header */}
      <div className="mb-6">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-2xl font-bold">Complaint #{complaint.complaintId}</h2>
          <span className={`
            px-4 py-2 rounded-full text-sm font-semibold flex items-center gap-2
            ${currentStatus.color === 'blue' ? 'bg-blue-100 text-blue-800' : ''}
            ${currentStatus.color === 'purple' ? 'bg-purple-100 text-purple-800' : ''}
            ${currentStatus.color === 'amber' ? 'bg-amber-100 text-amber-800' : ''}
            ${currentStatus.color === 'green' ? 'bg-green-100 text-green-800' : ''}
            ${currentStatus.color === 'red' ? 'bg-red-100 text-red-800' : ''}
          `}>
            <StatusIcon className="w-4 h-4" />
            {currentStatus.label}
          </span>
        </div>

        <p className="text-gray-600">{currentStatus.description}</p>
      </div>

      {/* Complaint Details */}
      <div className="bg-gray-50 rounded-lg p-4 mb-6">
        <h3 className="font-semibold mb-3">Complaint Details</h3>
        <dl className="grid grid-cols-2 gap-4 text-sm">
          <div>
            <dt className="text-gray-600 mb-1">Type</dt>
            <dd className="font-medium">{complaint.type}</dd>
          </div>
          <div>
            <dt className="text-gray-600 mb-1">Severity</dt>
            <dd className="font-medium capitalize">{complaint.severity}</dd>
          </div>
          <div>
            <dt className="text-gray-600 mb-1">Submitted On</dt>
            <dd className="font-medium">
              {new Date(complaint.submittedAt).toLocaleDateString('en-IN', {
                day: 'numeric',
                month: 'long',
                year: 'numeric'
              })}
            </dd>
          </div>
          <div>
            <dt className="text-gray-600 mb-1">Order #</dt>
            <dd className="font-medium">{complaint.orderNumber}</dd>
          </div>
          <div className="col-span-2">
            <dt className="text-gray-600 mb-1">Description</dt>
            <dd className="font-medium text-gray-900">{complaint.description}</dd>
          </div>
        </dl>
      </div>

      {/* Timeline */}
      <div className="mb-6">
        <h3 className="font-semibold mb-4">Progress Timeline</h3>
        <div className="relative pl-8">
          {/* Vertical Line */}
          <div className="absolute left-3 top-0 bottom-0 w-0.5 bg-gray-200"></div>

          {/* Timeline Events */}
          <div className="space-y-6">
            {timelineEvents.map((event, index) => {
              const EventIcon = event.icon || Clock;
              const isLast = index === timelineEvents.length - 1;

              return (
                <div key={index} className="relative flex items-start gap-4">
                  {/* Icon Circle */}
                  <div className={`
                    w-6 h-6 rounded-full flex items-center justify-center flex-shrink-0 relative z-10 ring-4 ring-white
                    ${event.type === 'success' ? 'bg-green-600' : ''}
                    ${event.type === 'info' ? 'bg-blue-600' : ''}
                    ${event.type === 'warning' ? 'bg-amber-600' : ''}
                    ${event.type === 'error' ? 'bg-red-600' : ''}
                    ${!event.type ? 'bg-gray-400' : ''}
                  `}>
                    <div className="w-2 h-2 bg-white rounded-full"></div>
                  </div>

                  {/* Content */}
                  <div className="flex-1 pb-6">
                    <p className="font-medium text-gray-900 mb-1">{event.title}</p>
                    <p className="text-sm text-gray-600 mb-1">{event.description}</p>
                    <p className="text-xs text-gray-500">
                      {new Date(event.timestamp).toLocaleString('en-IN', {
                        day: 'numeric',
                        month: 'short',
                        hour: '2-digit',
                        minute: '2-digit'
                      })}
                    </p>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      </div>

      {/* Partner Response (if available) */}
      {complaint.partnerResponse && (
        <div className="bg-purple-50 border-2 border-purple-200 rounded-lg p-4 mb-6">
          <div className="flex items-start gap-3 mb-3">
            <MessageSquare className="w-5 h-5 text-purple-700 flex-shrink-0" />
            <div>
              <h3 className="font-semibold text-purple-900 mb-1">Partner Response</h3>
              <p className="text-sm text-purple-800">{complaint.partnerResponse}</p>
            </div>
          </div>
          {complaint.partnerEvidence && complaint.partnerEvidence.length > 0 && (
            <div className="mt-3 pt-3 border-t border-purple-200">
              <p className="text-xs text-purple-700 mb-2">Partner Evidence:</p>
              <div className="grid grid-cols-4 gap-2">
                {complaint.partnerEvidence.map((evidence, idx) => (
                  <img
                    key={idx}
                    src={evidence.url}
                    alt={`Partner evidence ${idx + 1}`}
                    className="w-full h-20 object-cover rounded border border-purple-200"
                  />
                ))}
              </div>
            </div>
          )}
        </div>
      )}

      {/* Resolution Details (if resolved) */}
      {(complaint.status === 'resolved-approved' || complaint.status === 'resolved-partial') && complaint.resolution && (
        <div className="bg-green-50 border-2 border-green-300 rounded-lg p-6 mb-6">
          <div className="flex items-start gap-3 mb-4">
            <CheckCircle className="w-6 h-6 text-green-700 flex-shrink-0" />
            <div className="flex-1">
              <h3 className="font-semibold text-green-900 text-lg mb-2">Resolution Details</h3>
              <p className="text-sm text-green-800 mb-4">{complaint.resolution.message}</p>

              {/* Refund Details */}
              <div className="bg-white rounded-lg p-4">
                <div className="grid grid-cols-2 gap-4 text-sm">
                  <div>
                    <p className="text-gray-600 mb-1">Approved Refund</p>
                    <p className="text-2xl font-bold text-green-700">
                      ₹{complaint.resolution.refundAmount.toFixed(2)}
                    </p>
                  </div>
                  <div>
                    <p className="text-gray-600 mb-1">Refund Status</p>
                    <p className="font-semibold text-green-700">
                      {complaint.resolution.refundStatus || 'Processing'}
                    </p>
                    <p className="text-xs text-gray-600 mt-1">
                      Expected in 5-7 business days
                    </p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Rejection Details (if rejected) */}
      {complaint.status === 'rejected' && complaint.rejectionDetails && (
        <div className="bg-red-50 border-2 border-red-300 rounded-lg p-6">
          <div className="flex items-start gap-3">
            <XCircle className="w-6 h-6 text-red-700 flex-shrink-0" />
            <div className="flex-1">
              <h3 className="font-semibold text-red-900 text-lg mb-2">Complaint Rejected</h3>
              <p className="text-sm text-red-800 mb-4">{complaint.rejectionDetails.reason}</p>

              {complaint.rejectionDetails.explanation && (
                <div className="bg-white rounded-lg p-3 text-sm text-gray-700">
                  <p className="font-medium mb-1">Detailed Explanation:</p>
                  <p>{complaint.rejectionDetails.explanation}</p>
                </div>
              )}

              <div className="mt-4 bg-red-100 rounded p-3">
                <p className="text-xs text-red-900">
                  <strong>Note:</strong> If you believe this decision is incorrect, you may file an appeal
                  within 7 days by contacting support with additional evidence.
                </p>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Next Steps (if under review) */}
      {(complaint.status === 'under-review' || complaint.status === 'partner-notified' || complaint.status === 'partner-responded') && (
        <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
          <div className="flex items-start gap-2">
            <AlertTriangle className="w-5 h-5 text-blue-700 flex-shrink-0 mt-0.5" />
            <div className="text-sm text-blue-900">
              <p className="font-medium mb-2">What happens next?</p>
              <ul className="list-disc list-inside space-y-1">
                {complaint.status === 'under-review' && (
                  <>
                    <li>Our team will review your evidence and description</li>
                    <li>Partner will be notified and given 24 hours to respond</li>
                    <li>You'll receive a decision within 48-72 hours</li>
                  </>
                )}
                {complaint.status === 'partner-notified' && (
                  <>
                    <li>Partner has 24 hours to provide their response</li>
                    <li>Final decision will be made after partner response or timeout</li>
                  </>
                )}
                {complaint.status === 'partner-responded' && (
                  <>
                    <li>Final review is in progress considering both sides</li>
                    <li>Decision will be communicated within 24 hours</li>
                  </>
                )}
              </ul>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default ComplaintStatusTracker;
