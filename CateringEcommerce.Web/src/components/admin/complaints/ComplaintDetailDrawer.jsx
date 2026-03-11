import React from 'react';
import {
  X,
  User,
  Calendar,
  FileText,
  AlertCircle,
  Image as ImageIcon,
  Video,
  Users
} from 'lucide-react';

const ComplaintDetailDrawer = ({ complaint, isOpen, onClose, onResolve, onEscalate }) => {
  if (!isOpen || !complaint) return null;

  const formatDate = (dateString) => {
    if (!dateString) return 'N/A';
    return new Date(dateString).toLocaleDateString('en-IN', {
      day: 'numeric',
      month: 'long',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const getSeverityColor = (severity) => {
    const severityLower = severity?.toLowerCase() || '';
    const colors = {
      'critical': 'bg-red-100 text-red-800 border-red-300',
      'major': 'bg-orange-100 text-orange-800 border-orange-300',
      'minor': 'bg-yellow-100 text-yellow-800 border-yellow-300'
    };
    return colors[severityLower] || 'bg-gray-100 text-gray-800 border-gray-300';
  };

  const photoEvidence = complaint.photoEvidencePaths
    ? JSON.parse(complaint.photoEvidencePaths)
    : [];
  const videoEvidence = complaint.videoEvidencePaths
    ? JSON.parse(complaint.videoEvidencePaths)
    : [];

  return (
    <>
      {/* Overlay */}
      <div
        className="fixed inset-0 bg-black bg-opacity-50 z-40 transition-opacity"
        onClick={onClose}
      ></div>

      {/* Drawer */}
      <div className="fixed right-0 top-0 h-full w-full md:w-2/3 lg:w-1/2 bg-white shadow-2xl z-50 overflow-y-auto">
        {/* Header */}
        <div className="sticky top-0 bg-white border-b border-gray-200 px-6 py-4 flex items-center justify-between z-10">
          <div>
            <h2 className="text-2xl font-bold text-gray-900">
              Complaint #{complaint.complaintId}
            </h2>
            <p className="text-sm text-gray-600">Order #{complaint.orderId}</p>
          </div>
          <button
            onClick={onClose}
            className="p-2 hover:bg-gray-100 rounded-lg transition-colors"
          >
            <X className="w-6 h-6 text-gray-600" />
          </button>
        </div>

        {/* Content */}
        <div className="p-6 space-y-6">
          {/* Complaint Details */}
          <div className="bg-gray-50 rounded-lg p-4">
            <h3 className="font-semibold text-lg mb-3 flex items-center gap-2">
              <FileText className="w-5 h-5 text-blue-600" />
              Complaint Details
            </h3>

            <div className="grid grid-cols-2 gap-4 mb-4">
              <div>
                <p className="text-xs text-gray-600 mb-1">Type</p>
                <p className="font-medium">{complaint.complaintType?.replace(/_/g, ' ')}</p>
              </div>
              <div>
                <p className="text-xs text-gray-600 mb-1">Severity</p>
                <span className={`inline-block px-3 py-1 rounded text-sm font-medium border ${getSeverityColor(complaint.severity)}`}>
                  {complaint.severity || 'N/A'}
                </span>
              </div>
              <div>
                <p className="text-xs text-gray-600 mb-1">Filed On</p>
                <p className="font-medium">{formatDate(complaint.reportedAt || complaint.createdDate)}</p>
              </div>
              <div>
                <p className="text-xs text-gray-600 mb-1">Status</p>
                <p className="font-medium capitalize">{complaint.status?.replace(/_/g, ' ')}</p>
              </div>
            </div>

            <div className="mb-4">
              <p className="text-xs text-gray-600 mb-1">Summary</p>
              <p className="font-medium">{complaint.complaintSummary || 'N/A'}</p>
            </div>

            <div>
              <p className="text-xs text-gray-600 mb-1">Detailed Description</p>
              <p className="text-sm text-gray-800 bg-white rounded p-3">
                {complaint.complaintDetails || 'No detailed description provided.'}
              </p>
            </div>
          </div>

          {/* Guest/Item Impact */}
          <div className="bg-blue-50 rounded-lg p-4">
            <h3 className="font-semibold text-lg mb-3 flex items-center gap-2">
              <Users className="w-5 h-5 text-blue-600" />
              Impact Details
            </h3>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <p className="text-xs text-gray-600 mb-1">Guests Affected</p>
                <p className="text-2xl font-bold text-blue-700">
                  {complaint.guestComplaintCount || 0} / {complaint.totalGuestCount || 'N/A'}
                </p>
              </div>
              <div>
                <p className="text-xs text-gray-600 mb-1">Items Affected</p>
                <p className="text-2xl font-bold text-blue-700">
                  {complaint.affectedItemCount || 0} / {complaint.totalItemCount || 'N/A'}
                </p>
              </div>
            </div>

            {complaint.affectedItems && (
              <div className="mt-3">
                <p className="text-xs text-gray-600 mb-1">Affected Items</p>
                <p className="text-sm text-gray-800">
                  {Array.isArray(complaint.affectedItems)
                    ? complaint.affectedItems.join(', ')
                    : complaint.affectedItems}
                </p>
              </div>
            )}
          </div>

          {/* Evidence */}
          {(photoEvidence.length > 0 || videoEvidence.length > 0) && (
            <div className="bg-amber-50 rounded-lg p-4">
              <h3 className="font-semibold text-lg mb-3 flex items-center gap-2">
                <ImageIcon className="w-5 h-5 text-amber-600" />
                Evidence
              </h3>

              {photoEvidence.length > 0 && (
                <div className="mb-4">
                  <p className="text-sm text-gray-700 mb-2 flex items-center gap-2">
                    <ImageIcon className="w-4 h-4" />
                    Photos ({photoEvidence.length})
                  </p>
                  <div className="grid grid-cols-3 gap-2">
                    {photoEvidence.map((photo, idx) => (
                      <div key={idx} className="aspect-square bg-gray-200 rounded-lg flex items-center justify-center">
                        <ImageIcon className="w-8 h-8 text-gray-400" />
                        <p className="text-xs text-gray-600 absolute">{photo}</p>
                      </div>
                    ))}
                  </div>
                </div>
              )}

              {videoEvidence.length > 0 && (
                <div>
                  <p className="text-sm text-gray-700 mb-2 flex items-center gap-2">
                    <Video className="w-4 h-4" />
                    Videos ({videoEvidence.length})
                  </p>
                  <ul className="space-y-1 text-sm text-gray-800">
                    {videoEvidence.map((video, idx) => (
                      <li key={idx} className="flex items-center gap-2">
                        <Video className="w-3 h-3" />
                        {video}
                      </li>
                    ))}
                  </ul>
                </div>
              )}
            </div>
          )}

          {/* Partner Response */}
          {complaint.partnerResponse && (
            <div className="bg-purple-50 rounded-lg p-4">
              <h3 className="font-semibold text-lg mb-3 flex items-center gap-2">
                <User className="w-5 h-5 text-purple-600" />
                Partner Response
              </h3>

              <div className="bg-white rounded p-3 mb-3">
                <p className="text-sm text-gray-800">{complaint.partnerResponse}</p>
              </div>

              <div className="grid grid-cols-2 gap-3 text-xs">
                <div>
                  <p className="text-gray-600 mb-1">Responded On</p>
                  <p className="font-medium">{formatDate(complaint.partnerResponseDate)}</p>
                </div>
                <div>
                  <p className="text-gray-600 mb-1">Admitted Fault</p>
                  <p className="font-medium">
                    {complaint.partnerAdmittedFault === true ? 'Yes' :
                     complaint.partnerAdmittedFault === false ? 'No' : 'N/A'}
                  </p>
                </div>
                <div>
                  <p className="text-gray-600 mb-1">Offered Replacement</p>
                  <p className="font-medium">
                    {complaint.partnerOfferedReplacement ? 'Yes' : 'No'}
                  </p>
                </div>
                <div>
                  <p className="text-gray-600 mb-1">Provided Replacement</p>
                  <p className="font-medium">
                    {complaint.partnerProvidedReplacement ? 'Yes' : 'No'}
                  </p>
                </div>
              </div>
            </div>
          )}

          {/* Admin Notes (if reviewed) */}
          {complaint.adminNotes && (
            <div className="bg-gray-50 rounded-lg p-4">
              <h3 className="font-semibold text-lg mb-3 flex items-center gap-2">
                <AlertCircle className="w-5 h-5 text-gray-600" />
                Admin Notes
              </h3>
              <p className="text-sm text-gray-800">{complaint.adminNotes}</p>
            </div>
          )}

          {/* Fraud Detection Flag */}
          {complaint.isFlaggedSuspicious && (
            <div className="bg-red-50 border-2 border-red-300 rounded-lg p-4">
              <div className="flex items-start gap-3">
                <AlertCircle className="w-6 h-6 text-red-600 flex-shrink-0" />
                <div>
                  <h3 className="font-semibold text-red-900 mb-1">Flagged as Suspicious</h3>
                  <p className="text-sm text-red-800">
                    This complaint has been automatically flagged for manual review.
                    Customer complaint history: {complaint.customerComplaintHistoryCount || 0}
                  </p>
                </div>
              </div>
            </div>
          )}
        </div>

        {/* Actions Footer */}
        {complaint.status?.toLowerCase() !== 'resolved' && complaint.status?.toLowerCase() !== 'rejected' && (
          <div className="sticky bottom-0 bg-white border-t border-gray-200 px-6 py-4 flex gap-3">
            <button
              onClick={() => onResolve(complaint)}
              className="flex-1 px-6 py-3 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors font-semibold"
            >
              Resolve Complaint
            </button>
            <button
              onClick={() => onEscalate(complaint)}
              className="flex-1 px-6 py-3 bg-purple-600 text-white rounded-lg hover:bg-purple-700 transition-colors font-semibold"
            >
              Escalate
            </button>
          </div>
        )}
      </div>
    </>
  );
};

export default ComplaintDetailDrawer;
