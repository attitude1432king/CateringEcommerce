import { useState } from 'react';
import { X, CheckCircle, XCircle, Phone, Mail, MapPin, FileText, Image, Calendar, User, Building, CreditCard, Shield, Briefcase, Video, Play } from 'lucide-react';
import PartnerStatusBadge from './PartnerStatusBadge';
import PartnerActionModal from './PartnerActionModal';
import { PermissionButton } from '../ui/PermissionButton';
import { ApprovalStatus } from '../../../services/partnerApprovalApi';
import MediaViewer from '../ui/MediaViewer';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL.replace(/\/$/, '');


/**
 * Partner Detail Drawer Component (UPDATED - Enum-based)
 *
 * Shows complete partner registration details
 * Works with NEW PartnerApprovalController backend
 */
const PartnerDetailDrawer = ({ request, onClose, onActionSuccess }) => {
    const [showActionModal, setShowActionModal] = useState(false);
    const [actionType, setActionType] = useState(null);
    const [mediaViewer, setMediaViewer] = useState({ show: false, items: [], currentIndex: 0 });

    if (!request) return null;

    const handleAction = (type) => {
        setActionType(type);
        setShowActionModal(true);
    };

    const openMediaViewer = (items, index = 0) => {
        setMediaViewer({ show: true, items, currentIndex: index });
    };

    const closeMediaViewer = () => {
        setMediaViewer({ show: false, items: [], currentIndex: 0 });
    };

    const navigateMedia = (newIndex) => {
        setMediaViewer(prev => ({ ...prev, currentIndex: newIndex }));
    };

    const isVideo = (filePath) => {
        const videoExtensions = ['.mp4', '.webm', '.ogg', '.mov', '.avi'];
        return videoExtensions.some(ext => filePath.toLowerCase().endsWith(ext));
    };

    const formatDate = (dateString) => {
        if (!dateString) return 'N/A';
        return new Date(dateString).toLocaleDateString('en-IN', {
            day: '2-digit',
            month: 'short',
            year: 'numeric'
        });
    };

    // Check if can approve/reject (only PENDING requests)
    const canTakeAction = request.approvalStatusId === ApprovalStatus.PENDING;

    return (
        <>
            {/* Backdrop */}
            <div
                className="fixed inset-0 bg-black bg-opacity-50 z-40"
                onClick={onClose}
            ></div>

            {/* Drawer */}
            <div className="fixed right-0 top-0 h-full w-full max-w-4xl bg-white shadow-2xl z-50 overflow-hidden flex flex-col animate-slide-in-right">
                {/* Header */}
                <div className="flex items-center justify-between p-6 border-b border-gray-200 bg-gray-50">
                    <div className="flex-1 min-w-0">
                        <h2 className="text-xl font-bold text-gray-900 truncate">
                            {request.businessName}
                        </h2>
                        <p className="text-sm text-gray-600 mt-1">
                            Owner ID: {request.ownerId} | Registered: {formatDate(request.registrationDate)}
                        </p>
                    </div>
                    <button
                        onClick={onClose}
                        className="ml-4 p-2 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-lg transition-colors"
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
                            <PartnerStatusBadge
                                statusId={request.approvalStatusId}
                                statusName={request.approvalStatusName}
                                size="lg"
                            />
                        </div>

                        {canTakeAction && (
                            <div className="flex items-center space-x-2">
                                <PermissionButton
                                    permission="PARTNER_REQUEST_APPROVE"
                                    onClick={() => handleAction('APPROVE')}
                                    className="inline-flex items-center px-4 py-2 bg-green-600 text-white text-sm font-medium rounded-lg hover:bg-green-700 transition-colors"
                                >
                                    <CheckCircle className="w-4 h-4 mr-2" />
                                    Approve
                                </PermissionButton>

                                <PermissionButton
                                    permission="PARTNER_REQUEST_REJECT"
                                    onClick={() => handleAction('REJECT')}
                                    className="inline-flex items-center px-4 py-2 bg-red-600 text-white text-sm font-medium rounded-lg hover:bg-red-700 transition-colors"
                                >
                                    <XCircle className="w-4 h-4 mr-2" />
                                    Reject
                                </PermissionButton>
                            </div>
                        )}

                        {request.approvalStatusId === ApprovalStatus.REJECTED && request.rejectionReason && (
                            <div className="ml-4 flex-1">
                                <p className="text-sm text-red-600 font-medium">Rejection Reason:</p>
                                <p className="text-sm text-gray-700 mt-1">{request.rejectionReason}</p>
                            </div>
                        )}
                    </div>

                    {/* Business Information */}
                    <Section title="Business Information" icon={Building}>
                        <InfoRow label="Business Name" value={request.businessName} />
                        <InfoRow label="Owner Name" value={request.ownerName} />
                        <InfoRow label="Email" value={request.email} icon={<Mail className="w-4 h-4" />} />
                        <InfoRow label="Phone" value={request.phone} icon={<Phone className="w-4 h-4" />} />
                        {request.supportContact && (
                            <InfoRow label="Support Contact" value={request.supportContact} icon={<Phone className="w-4 h-4" />} />
                        )}
                        {request.whatsAppNumber && (
                            <InfoRow label="WhatsApp" value={request.whatsAppNumber} />
                        )}
                        {request.alternateEmail && (
                            <InfoRow label="Alternate Email" value={request.alternateEmail} />
                        )}
                    </Section>

                    {/* Address Information */}
                    {request.address && (
                        <Section title="Address Details" icon={MapPin}>
                            <InfoRow label="Building" value={request.address.building} />
                            {request.address.street && <InfoRow label="Street" value={request.address.street} />}
                            {request.address.area && <InfoRow label="Area" value={request.address.area} />}
                            <InfoRow label="City" value={request.address.cityName || 'N/A'} />
                            <InfoRow label="State" value={request.address.stateName || 'N/A'} />
                            <InfoRow label="Pincode" value={request.address.pincode} />
                            {request.address.latitude && request.address.longitude && (
                                <InfoRow
                                    label="Coordinates"
                                    value={`${request.address.latitude}, ${request.address.longitude}`}
                                />
                            )}
                        </Section>
                    )}

                    {/* Legal Compliance */}
                    {request.legalCompliance && (
                        <Section title="Legal & Compliance" icon={Shield}>
                            <InfoRow label="FSSAI Number" value={request.legalCompliance.fssaiNumber} />
                            <InfoRow label="FSSAI Expiry" value={formatDate(request.legalCompliance.fssaiExpiryDate)} />
                            {request.legalCompliance.fssaiCertificatePath && (
                                <DocumentLink
                                    label="FSSAI Certificate"
                                    path={request.legalCompliance.fssaiCertificatePath}
                                    onClick={() => openMediaViewer([{
                                        filePath: request.legalCompliance.fssaiCertificatePath,
                                        fileName: 'FSSAI Certificate',
                                        label: 'FSSAI Certificate'
                                    }], 0)}
                                />
                            )}

                            <InfoRow
                                label="GST Applicable"
                                value={request.legalCompliance.gstApplicable ? 'Yes' : 'No'}
                            />
                            {request.legalCompliance.gstApplicable && (
                                <>
                                    <InfoRow label="GST Number" value={request.legalCompliance.gstNumber || 'N/A'} />
                                    {request.legalCompliance.gstCertificatePath && (
                                        <DocumentLink
                                            label="GST Certificate"
                                            path={request.legalCompliance.gstCertificatePath}
                                            onClick={() => openMediaViewer([{
                                                filePath: request.legalCompliance.gstCertificatePath,
                                                fileName: 'GST Certificate',
                                                label: 'GST Certificate'
                                            }], 0)}
                                        />
                                    )}
                                </>
                            )}

                            <InfoRow label="PAN Name" value={request.legalCompliance.panName} />
                            <InfoRow label="PAN Number" value={request.legalCompliance.panNumber} />
                            {request.legalCompliance.panFilePath && (
                                <DocumentLink
                                    label="PAN Card"
                                    path={request.legalCompliance.panFilePath}
                                    onClick={() => openMediaViewer([{
                                        filePath: request.legalCompliance.panFilePath,
                                        fileName: 'PAN Card',
                                        label: 'PAN Card'
                                    }], 0)}
                                />
                            )}
                        </Section>
                    )}

                    {/* Bank Details */}
                    {request.bankDetails && (
                        <Section title="Bank Account Details" icon={CreditCard}>
                            <InfoRow label="Account Holder" value={request.bankDetails.accountHolderName} />
                            <InfoRow label="Account Number" value={request.bankDetails.accountNumber} />
                            <InfoRow label="IFSC Code" value={request.bankDetails.ifscCode} />
                            {request.bankDetails.upiId && <InfoRow label="UPI ID" value={request.bankDetails.upiId} />}
                            {request.bankDetails.chequePath && (
                                <DocumentLink
                                    label="Cancelled Cheque"
                                    path={request.bankDetails.chequePath}
                                    onClick={() => openMediaViewer([{
                                        filePath: request.bankDetails.chequePath,
                                        fileName: 'Cancelled Cheque',
                                        label: 'Cancelled Cheque'
                                    }], 0)}
                                />
                            )}
                        </Section>
                    )}

                    {/* Service Operations */}
                    {request.serviceOperations && (
                        <Section title="Service & Operations" icon={Briefcase}>
                            {request.serviceOperations.cuisineTypes && (
                                <InfoRow label="Cuisine Types" value={request.serviceOperations.cuisineTypes} />
                            )}
                            {request.serviceOperations.serviceTypes && (
                                <InfoRow label="Service Types" value={request.serviceOperations.serviceTypes} />
                            )}
                            {request.serviceOperations.eventTypes && (
                                <InfoRow label="Event Types" value={request.serviceOperations.eventTypes} />
                            )}
                            {request.serviceOperations.foodTypes && (
                                <InfoRow label="Food Types" value={request.serviceOperations.foodTypes} />
                            )}
                            {request.serviceOperations.minGuestCount && (
                                <InfoRow label="Min Guest Count" value={request.serviceOperations.minGuestCount} />
                            )}
                            <InfoRow
                                label="Delivery Available"
                                value={request.serviceOperations.deliveryAvailable ? 'Yes' : 'No'}
                            />
                            {request.serviceOperations.deliveryRadiusKm && (
                                <InfoRow
                                    label="Delivery Radius"
                                    value={`${request.serviceOperations.deliveryRadiusKm} km`}
                                />
                            )}
                        </Section>
                    )}

                    {/* Documents */}
                    {request.documents && request.documents.length > 0 && (
                        <Section title="Uploaded Kitchen Image/Videos" icon={FileText}>
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                                {request.documents.map((doc) => (
                                    <div
                                        key={doc.mediaId}
                                        className="flex items-center justify-between p-3 bg-gray-50 border border-gray-200 rounded-lg hover:bg-gray-100 transition-colors"
                                    >
                                        <div className="flex items-center space-x-3 flex-1 min-w-0">
                                            <FileText className="w-5 h-5 text-indigo-600 flex-shrink-0" />
                                            <div className="flex-1 min-w-0">
                                                <p className="text-sm font-medium text-gray-900 truncate">
                                                    {doc.fileName}
                                                </p>
                                                <p className="text-xs text-gray-500">
                                                    {formatDate(doc.uploadedAt)} • {doc.extension.toUpperCase()}
                                                </p>
                                            </div>
                                        </div>
                                        <a
                                            href={doc.filePath}
                                            target="_blank"
                                            rel="noopener noreferrer"
                                            className="ml-2 px-3 py-1 text-sm text-indigo-600 hover:bg-indigo-50 rounded transition-colors"
                                        >
                                            View
                                        </a>
                                    </div>
                                ))}
                            </div>
                        </Section>
                    )}

                    {/* Photos & Videos */}
                    {request.photos && request.photos.length > 0 && (
                        <Section title="Uploaded Kitchen Media" icon={Image}>
                            <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
                                {request.photos.map((photo, index) => {
                                    const mediaUrl = `${API_BASE_URL}${photo.filePath}`;
                                    const isVideoFile = isVideo(photo.filePath);

                                    return (
                                        <button
                                            key={photo.mediaId}
                                            onClick={() => openMediaViewer(request.photos, index)}
                                            className="group relative aspect-square bg-gray-100 rounded-lg overflow-hidden border-2 border-gray-200 hover:border-indigo-500 transition-all"
                                        >
                                            {isVideoFile ? (
                                                <>
                                                    <video
                                                        src={mediaUrl}
                                                        className="w-full h-full object-cover"
                                                        muted
                                                    />
                                                    <div className="absolute inset-0 bg-black bg-opacity-30 flex items-center justify-center">
                                                        <div className="bg-white bg-opacity-90 rounded-full p-3">
                                                            <Video className="w-6 h-6 text-indigo-600" />
                                                        </div>
                                                    </div>
                                                    <div className="absolute inset-0 bg-black bg-opacity-0 group-hover:bg-opacity-40 transition-opacity flex items-center justify-center">
                                                        <Play className="w-8 h-8 text-white opacity-0 group-hover:opacity-100 transition-opacity" />
                                                    </div>
                                                </>
                                            ) : (
                                                <>
                                                    <img
                                                        src={mediaUrl}
                                                        alt={photo.fileName}
                                                        className="w-full h-full object-cover group-hover:scale-105 transition-transform"
                                                    />
                                                    <div className="absolute inset-0 bg-black bg-opacity-0 group-hover:bg-opacity-20 transition-opacity flex items-center justify-center">
                                                        <Image className="w-6 h-6 text-white opacity-0 group-hover:opacity-100 transition-opacity" />
                                                    </div>
                                                </>
                                            )}
                                        </button>
                                    );
                                })}
                            </div>
                        </Section>
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

            {/* Media Viewer */}
            {mediaViewer.show && (
                <MediaViewer
                    mediaItems={mediaViewer.items}
                    currentIndex={mediaViewer.currentIndex}
                    onClose={closeMediaViewer}
                    onNavigate={navigateMedia}
                />
            )}
        </>
    );
};

// Helper Components
const Section = ({ title, icon: Icon, children }) => (
    <section>
        <h3 className="text-lg font-semibold text-gray-900 flex items-center mb-4">
            <Icon className="w-5 h-5 mr-2 text-indigo-600" />
            {title}
        </h3>
        <div className="bg-white border border-gray-200 rounded-lg p-4 space-y-3">
            {children}
        </div>
    </section>
);

const InfoRow = ({ label, value, icon }) => (
    <div className="flex items-start justify-between py-2 border-b border-gray-100 last:border-0">
        <span className="text-sm font-medium text-gray-600 flex items-center">
            {icon && <span className="mr-2 text-gray-400">{icon}</span>}
            {label}
        </span>
        <span className="text-sm text-gray-900 text-right ml-4">
            {value || 'N/A'}
        </span>
    </div>
);

const DocumentLink = ({ label, path, onClick }) => (
    <div className="flex items-start justify-between py-2 border-b border-gray-100 last:border-0">
        <span className="text-sm font-medium text-gray-600">{label}</span>
        <button
            onClick={onClick}
            className="text-sm text-indigo-600 hover:text-indigo-700 underline ml-4 focus:outline-none"
        >
            View Document
        </button>
    </div>
);

export default PartnerDetailDrawer;
