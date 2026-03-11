import { useState } from 'react';
import { X, CheckCircle, XCircle, AlertTriangle } from 'lucide-react';
import { partnerApprovalApi } from '../../../services/partnerApprovalApi';
import { toast } from 'react-hot-toast';

/**
 * Partner Action Modal Component (UPDATED - Enum-based)
 *
 * Handles Approve and Reject actions
 * Works with NEW PartnerApprovalController backend
 */
const PartnerActionModal = ({ request, actionType, onClose, onSuccess }) => {
    const [loading, setLoading] = useState(false);
    const [formData, setFormData] = useState({
        remarks: '',
        rejectionReason: '',
        sendNotification: true
    });

    if (!request) return null;

    const handleSubmit = async (e) => {
        e.preventDefault();

        // Validation
        if (actionType === 'REJECT' && !formData.rejectionReason.trim()) {
            toast.error('Rejection reason is required');
            return;
        }

        setLoading(true);

        try {
            let response;

            if (actionType === 'APPROVE') {
                response = await partnerApprovalApi.approvePartner(request.ownerId, {
                    remarks: formData.remarks,
                    sendNotification: formData.sendNotification
                });
            } else if (actionType === 'REJECT') {
                response = await partnerApprovalApi.rejectPartner(request.ownerId, {
                    rejectionReason: formData.rejectionReason,
                    sendNotification: formData.sendNotification
                });
            }

            if (response && response.result) {
                toast.success(response.message || `Partner ${actionType === 'APPROVE' ? 'approved' : 'rejected'} successfully`);
                onSuccess();
            } else {
                toast.error(response.message || 'Action failed');
            }
        } catch (error) {
            console.error('Error performing action:', error);
            toast.error('Network error. Please try again.');
        } finally {
            setLoading(false);
        }
    };

    const modalConfig = {
        APPROVE: {
            title: 'Approve Partner Request',
            icon: CheckCircle,
            iconColor: 'text-green-600',
            bgColor: 'bg-green-50',
            buttonColor: 'bg-green-600 hover:bg-green-700',
            message: `Are you sure you want to approve "${request.businessName}"? This will activate their account and allow them to start receiving orders.`
        },
        REJECT: {
            title: 'Reject Partner Request',
            icon: XCircle,
            iconColor: 'text-red-600',
            bgColor: 'bg-red-50',
            buttonColor: 'bg-red-600 hover:bg-red-700',
            message: `Are you sure you want to reject "${request.businessName}"? Please provide a reason for rejection.`
        }
    };

    const config = modalConfig[actionType] || modalConfig.APPROVE;
    const Icon = config.icon;

    return (
        <>
            {/* Backdrop */}
            <div
                className="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4"
                onClick={onClose}
            >
                {/* Modal */}
                <div
                    className="bg-white rounded-lg shadow-2xl max-w-lg w-full"
                    onClick={(e) => e.stopPropagation()}
                >
                    {/* Header */}
                    <div className="flex items-center justify-between p-6 border-b border-gray-200">
                        <div className="flex items-center space-x-3">
                            <div className={`p-2 rounded-lg ${config.bgColor}`}>
                                <Icon className={`w-6 h-6 ${config.iconColor}`} />
                            </div>
                            <h2 className="text-xl font-bold text-gray-900">{config.title}</h2>
                        </div>
                        <button
                            onClick={onClose}
                            className="p-2 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-lg transition-colors"
                        >
                            <X className="w-5 h-5" />
                        </button>
                    </div>

                    {/* Content */}
                    <form onSubmit={handleSubmit}>
                        <div className="p-6 space-y-6">
                            {/* Partner Info */}
                            <div className="bg-gray-50 border border-gray-200 rounded-lg p-4">
                                <div className="space-y-2">
                                    <div className="flex justify-between">
                                        <span className="text-sm font-medium text-gray-600">Business Name</span>
                                        <span className="text-sm text-gray-900">{request.businessName}</span>
                                    </div>
                                    <div className="flex justify-between">
                                        <span className="text-sm font-medium text-gray-600">Owner</span>
                                        <span className="text-sm text-gray-900">{request.ownerName}</span>
                                    </div>
                                    <div className="flex justify-between">
                                        <span className="text-sm font-medium text-gray-600">Phone</span>
                                        <span className="text-sm text-gray-900">{request.phone}</span>
                                    </div>
                                    <div className="flex justify-between">
                                        <span className="text-sm font-medium text-gray-600">Email</span>
                                        <span className="text-sm text-gray-900">{request.email}</span>
                                    </div>
                                </div>
                            </div>

                            {/* Warning Message */}
                            <div className={`p-4 rounded-lg border-2 ${config.bgColor} ${config.iconColor.replace('text-', 'border-')}`}>
                                <div className="flex items-start space-x-3">
                                    <AlertTriangle className={`w-5 h-5 ${config.iconColor} flex-shrink-0 mt-0.5`} />
                                    <p className="text-sm text-gray-700">{config.message}</p>
                                </div>
                            </div>

                            {/* Approve: Optional Remarks */}
                            {actionType === 'APPROVE' && (
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-2">
                                        Remarks (Optional)
                                    </label>
                                    <textarea
                                        value={formData.remarks}
                                        onChange={(e) => setFormData({ ...formData, remarks: e.target.value })}
                                        placeholder="Add any internal notes or comments..."
                                        rows={3}
                                        className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-green-500"
                                    />
                                    <p className="mt-1 text-xs text-gray-500">
                                        These remarks are for internal use only and will not be sent to the partner.
                                    </p>
                                </div>
                            )}

                            {/* Reject: Rejection Reason (MANDATORY) */}
                            {actionType === 'REJECT' && (
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-2">
                                        Rejection Reason <span className="text-red-600">*</span>
                                    </label>
                                    <textarea
                                        value={formData.rejectionReason}
                                        onChange={(e) => setFormData({ ...formData, rejectionReason: e.target.value })}
                                        placeholder="Please provide a clear reason for rejection. This will be sent to the partner."
                                        rows={4}
                                        required
                                        className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500"
                                    />
                                    <p className="mt-1 text-xs text-gray-500">
                                        This reason will be sent to the partner via email/SMS.
                                    </p>
                                </div>
                            )}

                            {/* Send Notification */}
                            <div className="flex items-center">
                                <input
                                    type="checkbox"
                                    id="sendNotification"
                                    checked={formData.sendNotification}
                                    onChange={(e) => setFormData({ ...formData, sendNotification: e.target.checked })}
                                    className="w-4 h-4 text-indigo-600 border-gray-300 rounded focus:ring-indigo-500"
                                />
                                <label htmlFor="sendNotification" className="ml-2 text-sm text-gray-700">
                                    Send notification to partner via email/SMS
                                </label>
                            </div>
                        </div>

                        {/* Footer */}
                        <div className="flex items-center justify-end space-x-3 p-6 border-t border-gray-200 bg-gray-50">
                            <button
                                type="button"
                                onClick={onClose}
                                disabled={loading}
                                className="px-4 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-100 transition-colors disabled:opacity-50"
                            >
                                Cancel
                            </button>
                            <button
                                type="submit"
                                disabled={loading || (actionType === 'REJECT' && !formData.rejectionReason.trim())}
                                className={`px-6 py-2 text-white rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed ${config.buttonColor}`}
                            >
                                {loading ? (
                                    <span className="flex items-center">
                                        <svg className="animate-spin -ml-1 mr-2 h-4 w-4 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                                            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                                            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                                        </svg>
                                        Processing...
                                    </span>
                                ) : (
                                    `${actionType === 'APPROVE' ? 'Approve' : 'Reject'} Partner`
                                )}
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        </>
    );
};

export default PartnerActionModal;
