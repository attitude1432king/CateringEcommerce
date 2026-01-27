import { useState } from 'react';
import { X, CheckCircle, XCircle, FileQuestion, Phone, Mail, MapPin, FileText, Image, Calendar, User, Building } from 'lucide-react';
import PartnerStatusBadge from './PartnerStatusBadge';
import PartnerActionModal from './PartnerActionModal';
import { PermissionGuard } from '../auth/PermissionGuard';
import { PermissionButton } from '../ui/PermissionButton';

/**
 * Partner Detail Drawer Component
 *
 * Slide-out panel showing complete partner registration details
 * with action buttons for approve/reject/request info
 */
const PartnerDetailDrawer = ({ request, onClose, onActionSuccess }) => {
  const [showActionModal, setShowActionModal] = useState(false);
  const [actionType, setActionType] = useState(null);
  const [selectedDocument, setSelectedDocument] = useState(null);

  const handleAction = (type) => {
    setActionType(type);
    setShowActionModal(true);
  };

  const formatDate = (dateString) => {
    if (!dateString) return 'N/A';
    return new Date(dateString).toLocaleDateString('en-IN', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  return (
    <>
      {/* Backdrop */}
      <div
        className="fixed inset-0 bg-black bg-opacity-50 z-40"
        onClick={onClose}
      ></div>

      {/* Drawer */}
      <div className="fixed right-0 top-0 h-full w-full max-w-3xl bg-white shadow-2xl z-50 overflow-hidden flex flex-col animate-slide-in-right">
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b border-gray-200 bg-gray-50">
          <div className="flex-1 min-w-0">
            <h2 className="text-xl font-bold text-gray-900 truncate">
              {request.businessInfo.businessName}
            </h2>
            <p className="text-sm text-gray-600 mt-1">
              Request ID: {request.requestNumber}
            </p>
          </div>
          <button
            onClick={onClose}
            className="ml-4 p-2 text-gray-400 hover:text-gray-600 hover:bg-gray-200 rounded-lg transition-colors"
          >
            <X className="w-6 h-6" />
          </button>
        </div>

        {/* Content */}
        <div className="flex-1 overflow-y-auto p-6 space-y-6">
          {/* Status & Quick Actions */}
          <div className="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
            <div>
              <div className="text-sm text-gray-600 mb-2">Current Status</div>
              <PartnerStatusBadge status={request.status} size="lg" />
            </div>

            {request.status === 'PENDING' && (
              <div className="flex items-center space-x-2">
                <PermissionButton
                  permission="PARTNER_REQUEST_APPROVE"
                  variant="success"
                  onClick={() => handleAction('APPROVE')}
                  className="bg-green-600 hover:bg-green-700"
                >
                  <CheckCircle className="w-4 h-4 mr-2" />
                  Approve
                </PermissionButton>

                <PermissionButton
                  permission="PARTNER_REQUEST_REJECT"
                  variant="danger"
                  onClick={() => handleAction('REJECT')}
                  className="bg-red-600 hover:bg-red-700"
                >
                  <XCircle className="w-4 h-4 mr-2" />
                  Reject
                </PermissionButton>

                <PermissionButton
                  permission="PARTNER_REQUEST_REQUEST_INFO"
                  variant="secondary"
                  onClick={() => handleAction('REQUEST_INFO')}
                >
                  <FileQuestion className="w-4 h-4 mr-2" />
                  Request Info
                </PermissionButton>
              </div>
            )}
          </div>

          {/* Business Information */}
          <section>
            <h3 className="text-lg font-semibold text-gray-900 flex items-center mb-4">
              <Building className="w-5 h-5 mr-2 text-indigo-600" />
              Business Information
            </h3>
            <div className="bg-white border border-gray-200 rounded-lg p-4 space-y-3">
              <InfoRow label="Business Name" value={request.businessInfo.businessName} />
              <InfoRow label="Business Type" value={request.businessInfo.businessType} />
              <InfoRow
                label="Cuisine Types"
                value={request.businessInfo.cuisineTypes?.join(', ') || 'N/A'}
              />
              <InfoRow label="Description" value={request.businessInfo.description} />
              <InfoRow
                label="Delivery Radius"
                value={`${request.businessInfo.deliveryRadius || 0} km`}
              />
            </div>
          </section>

          {/* Owner Information */}
          <section>
            <h3 className="text-lg font-semibold text-gray-900 flex items-center mb-4">
              <User className="w-5 h-5 mr-2 text-indigo-600" />
              Owner Information
            </h3>
            <div className="bg-white border border-gray-200 rounded-lg p-4 space-y-3">
              <InfoRow label="Name" value={request.ownerInfo.name} />
              <InfoRow
                label="Phone"
                value={request.ownerInfo.phone}
                icon={<Phone className="w-4 h-4" />}
              />
              {request.ownerInfo.alternatePhone && (
                <InfoRow
                  label="Alternate Phone"
                  value={request.ownerInfo.alternatePhone}
                  icon={<Phone className="w-4 h-4" />}
                />
              )}
              <InfoRow
                label="Email"
                value={request.ownerInfo.email}
                icon={<Mail className="w-4 h-4" />}
              />
            </div>
          </section>

          {/* Location Information */}
          <section>
            <h3 className="text-lg font-semibold text-gray-900 flex items-center mb-4">
              <MapPin className="w-5 h-5 mr-2 text-indigo-600" />
              Location Information
            </h3>
            <div className="bg-white border border-gray-200 rounded-lg p-4 space-y-3">
              <InfoRow label="Address Line 1" value={request.location.addressLine1} />
              {request.location.addressLine2 && (
                <InfoRow label="Address Line 2" value={request.location.addressLine2} />
              )}
              <InfoRow label="City" value={request.location.city} />
              <InfoRow label="State" value={request.location.state} />
              <InfoRow label="Pincode" value={request.location.pincode} />
              {request.location.latitude && request.location.longitude && (
                <InfoRow
                  label="Coordinates"
                  value={`${request.location.latitude}, ${request.location.longitude}`}
                />
              )}
            </div>

            {/* Map View (Optional) */}
            {request.location.latitude && request.location.longitude && (
              <div className="mt-3 border border-gray-200 rounded-lg overflow-hidden h-48 bg-gray-100 flex items-center justify-center">
                <p className="text-gray-500 text-sm">Map view placeholder</p>
                {/* Integrate Google Maps or similar here */}
              </div>
            )}
          </section>

          {/* Documents */}
          <section>
            <h3 className="text-lg font-semibold text-gray-900 flex items-center mb-4">
              <FileText className="w-5 h-5 mr-2 text-indigo-600" />
              Documents
            </h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {request.documents.gstNumber && (
                <DocumentCard
                  title="GST Certificate"
                  number={request.documents.gstNumber}
                  url={request.documents.gstDocumentUrl}
                  onClick={() => setSelectedDocument(request.documents.gstDocumentUrl)}
                />
              )}
              {request.documents.fssaiNumber && (
                <DocumentCard
                  title="FSSAI License"
                  number={request.documents.fssaiNumber}
                  url={request.documents.fssaiDocumentUrl}
                  onClick={() => setSelectedDocument(request.documents.fssaiDocumentUrl)}
                />
              )}
              {request.documents.panNumber && (
                <DocumentCard
                  title="PAN Card"
                  number={request.documents.panNumber}
                  url={request.documents.panDocumentUrl}
                  onClick={() => setSelectedDocument(request.documents.panDocumentUrl)}
                />
              )}
              {request.documents.bankAccountProofUrl && (
                <DocumentCard
                  title="Bank Account Proof"
                  url={request.documents.bankAccountProofUrl}
                  onClick={() => setSelectedDocument(request.documents.bankAccountProofUrl)}
                />
              )}
            </div>
          </section>

          {/* Photos */}
          <section>
            <h3 className="text-lg font-semibold text-gray-900 flex items-center mb-4">
              <Image className="w-5 h-5 mr-2 text-indigo-600" />
              Photos
            </h3>

            {/* Logo */}
            {request.images.logoUrl && (
              <div className="mb-4">
                <p className="text-sm font-medium text-gray-700 mb-2">Business Logo</p>
                <img
                  src={request.images.logoUrl}
                  alt="Business Logo"
                  className="w-32 h-32 object-cover rounded-lg border border-gray-200"
                />
              </div>
            )}

            {/* Kitchen Photos */}
            {request.images.kitchenPhotos && request.images.kitchenPhotos.length > 0 && (
              <div className="mb-4">
                <p className="text-sm font-medium text-gray-700 mb-2">Kitchen Photos</p>
                <div className="grid grid-cols-3 gap-3">
                  {request.images.kitchenPhotos.map((photo, index) => (
                    <img
                      key={index}
                      src={photo}
                      alt={`Kitchen ${index + 1}`}
                      className="w-full h-32 object-cover rounded-lg border border-gray-200 cursor-pointer hover:opacity-75 transition-opacity"
                      onClick={() => setSelectedDocument(photo)}
                    />
                  ))}
                </div>
              </div>
            )}

            {/* Menu Photos */}
            {request.images.menuPhotos && request.images.menuPhotos.length > 0 && (
              <div>
                <p className="text-sm font-medium text-gray-700 mb-2">Menu Photos</p>
                <div className="grid grid-cols-3 gap-3">
                  {request.images.menuPhotos.map((photo, index) => (
                    <img
                      key={index}
                      src={photo}
                      alt={`Menu ${index + 1}`}
                      className="w-full h-32 object-cover rounded-lg border border-gray-200 cursor-pointer hover:opacity-75 transition-opacity"
                      onClick={() => setSelectedDocument(photo)}
                    />
                  ))}
                </div>
              </div>
            )}
          </section>

          {/* Timeline / Audit Log */}
          <section>
            <h3 className="text-lg font-semibold text-gray-900 flex items-center mb-4">
              <Calendar className="w-5 h-5 mr-2 text-indigo-600" />
              Timeline
            </h3>
            <div className="space-y-3">
              {request.actionsLog && request.actionsLog.map((action, index) => (
                <div key={action.actionId} className="flex items-start space-x-3 p-3 bg-gray-50 rounded-lg">
                  <div className="flex-shrink-0 w-8 h-8 bg-indigo-100 rounded-full flex items-center justify-center">
                    <span className="text-xs font-semibold text-indigo-600">
                      {index + 1}
                    </span>
                  </div>
                  <div className="flex-1">
                    <p className="text-sm font-medium text-gray-900">
                      {action.actionType.replace('_', ' ')}
                    </p>
                    {action.adminName && (
                      <p className="text-xs text-gray-600 mt-1">by {action.adminName}</p>
                    )}
                    {action.remarks && (
                      <p className="text-xs text-gray-600 mt-1">{action.remarks}</p>
                    )}
                    <p className="text-xs text-gray-500 mt-1">
                      {formatDate(action.actionDate)}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          </section>
        </div>

        {/* Footer Actions */}
        <div className="border-t border-gray-200 p-6 bg-gray-50 flex items-center justify-between">
          <button
            onClick={onClose}
            className="px-6 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-100 transition-colors"
          >
            Close
          </button>

          {request.status === 'PENDING' && (
            <div className="flex items-center space-x-3">
              <PermissionButton
                permission="PARTNER_REQUEST_REQUEST_INFO"
                variant="secondary"
                onClick={() => handleAction('REQUEST_INFO')}
              >
                <FileQuestion className="w-4 h-4 mr-2" />
                Request Info
              </PermissionButton>

              <PermissionButton
                permission="PARTNER_REQUEST_REJECT"
                onClick={() => handleAction('REJECT')}
                className="px-6 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700"
              >
                <XCircle className="w-4 h-4 mr-2" />
                Reject
              </PermissionButton>

              <PermissionButton
                permission="PARTNER_REQUEST_APPROVE"
                onClick={() => handleAction('APPROVE')}
                className="px-6 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700"
              >
                <CheckCircle className="w-4 h-4 mr-2" />
                Approve
              </PermissionButton>
            </div>
          )}
        </div>
      </div>

      {/* Action Modal */}
      {showActionModal && (
        <PartnerActionModal
          request={request}
          actionType={actionType}
          onClose={() => {
            setShowActionModal(false);
            setActionType(null);
          }}
          onSuccess={() => {
            setShowActionModal(false);
            setActionType(null);
            onActionSuccess();
          }}
        />
      )}

      {/* Document Viewer Modal */}
      {selectedDocument && (
        <DocumentViewerModal
          url={selectedDocument}
          onClose={() => setSelectedDocument(null)}
        />
      )}
    </>
  );
};

// Helper Components
const InfoRow = ({ label, value, icon }) => (
  <div className="flex items-start justify-between py-2 border-b border-gray-100 last:border-0">
    <span className="text-sm font-medium text-gray-600">{label}</span>
    <span className="text-sm text-gray-900 text-right flex items-center space-x-2">
      {icon && <span className="text-gray-500">{icon}</span>}
      <span>{value || 'N/A'}</span>
    </span>
  </div>
);

const DocumentCard = ({ title, number, url, onClick }) => (
  <div
    onClick={onClick}
    className="p-4 border border-gray-200 rounded-lg hover:border-indigo-600 hover:shadow-md transition-all cursor-pointer"
  >
    <div className="flex items-center justify-between mb-2">
      <h4 className="text-sm font-semibold text-gray-900">{title}</h4>
      <FileText className="w-5 h-5 text-indigo-600" />
    </div>
    {number && (
      <p className="text-xs text-gray-600 mb-2 font-mono">{number}</p>
    )}
    <p className="text-xs text-indigo-600 font-medium">Click to view</p>
  </div>
);

const DocumentViewerModal = ({ url, onClose }) => (
  <>
    <div
      className="fixed inset-0 bg-black bg-opacity-75 z-50 flex items-center justify-center p-4"
      onClick={onClose}
    >
      <div className="relative max-w-4xl w-full bg-white rounded-lg overflow-hidden">
        <button
          onClick={onClose}
          className="absolute top-4 right-4 p-2 bg-white rounded-full shadow-lg hover:bg-gray-100 transition-colors z-10"
        >
          <X className="w-6 h-6" />
        </button>
        <img
          src={url}
          alt="Document"
          className="w-full h-auto max-h-[80vh] object-contain"
          onClick={(e) => e.stopPropagation()}
        />
      </div>
    </div>
  </>
);

export default PartnerDetailDrawer;
