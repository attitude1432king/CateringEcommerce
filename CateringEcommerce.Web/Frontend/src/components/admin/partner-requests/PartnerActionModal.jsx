import { useState, useEffect } from 'react';
import { X, CheckCircle, XCircle, FileQuestion, Mail, MessageSquare, Send, Eye, AlertCircle } from 'lucide-react';
import { partnerRequestApi } from '../../../services/partnerRequestApi';
import { toast } from 'react-hot-toast';

/**
 * Partner Action Modal Component
 *
 * Handles Approve, Reject, and Request Info actions
 * Features:
 * - Action confirmation
 * - Admin remarks input
 * - Communication channel selection (Email, SMS, Both)
 * - Message template editing
 * - Preview before send
 */
const PartnerActionModal = ({ request, actionType, onClose, onSuccess }) => {
  const [step, setStep] = useState(1); // 1: Action Details, 2: Communication, 3: Preview
  const [loading, setLoading] = useState(false);

  // Form State
  const [formData, setFormData] = useState({
    remarks: '',
    rejectionReason: '',
    infoRequested: [''],
    sendNotification: true,
    notificationChannels: ['EMAIL'],
    customMessage: '',
    subject: ''
  });

  // Message Templates
  const [templates, setTemplates] = useState({
    APPROVE: {
      subject: 'Welcome to Our Platform - Registration Approved!',
      message: `Dear ${request.ownerInfo.name},

Congratulations! We're excited to inform you that your catering business "${request.businessInfo.businessName}" has been successfully approved and is now live on our platform.

You can now:
✅ Login to your partner dashboard
✅ Manage your menu and services
✅ Start receiving orders

Login Credentials:
Email: ${request.ownerInfo.email}
Portal: https://partner.cateringecommerce.com

If you have any questions, feel free to reach out to our partner support team.

Best Regards,
Partner Onboarding Team`
    },
    REJECT: {
      subject: 'Registration Update Required',
      message: `Dear ${request.ownerInfo.name},

Thank you for your interest in partnering with us.

After reviewing your registration for "${request.businessInfo.businessName}", we need you to update the following information:

[Reason will be added here]

Please resubmit your application with the corrected information at your earliest convenience.

If you need assistance, please contact our support team.

Best Regards,
Partner Onboarding Team`
    },
    REQUEST_INFO: {
      subject: 'Additional Information Required',
      message: `Dear ${request.ownerInfo.name},

Thank you for registering "${request.businessInfo.businessName}" with us.

To proceed with your application, we need the following additional information:

[Information requests will be listed here]

Please upload the required documents or provide the information through your partner portal.

Portal Link: https://partner.cateringecommerce.com/complete-registration

Best Regards,
Partner Onboarding Team`
    }
  });

  // Load template on mount
  useEffect(() => {
    const template = templates[actionType];
    if (template) {
      setFormData(prev => ({
        ...prev,
        subject: template.subject,
        customMessage: template.message
      }));
    }
  }, [actionType]);

  const getActionConfig = () => {
    switch (actionType) {
      case 'APPROVE':
        return {
          title: 'Approve Partner Request',
          icon: CheckCircle,
          iconColor: 'text-green-600',
          iconBg: 'bg-green-100',
          buttonColor: 'bg-green-600 hover:bg-green-700',
          description: 'This will approve the partner request and create an active catering account.'
        };
      case 'REJECT':
        return {
          title: 'Reject Partner Request',
          icon: XCircle,
          iconColor: 'text-red-600',
          iconBg: 'bg-red-100',
          buttonColor: 'bg-red-600 hover:bg-red-700',
          description: 'This will reject the partner request. Please provide a clear reason.'
        };
      case 'REQUEST_INFO':
        return {
          title: 'Request Additional Information',
          icon: FileQuestion,
          iconColor: 'text-indigo-600',
          iconBg: 'bg-indigo-100',
          buttonColor: 'bg-indigo-600 hover:bg-indigo-700',
          description: 'Request additional documents or information from the partner.'
        };
      default:
        return {};
    }
  };

  const config = getActionConfig();
  const Icon = config.icon;

  const handleChannelToggle = (channel) => {
    setFormData(prev => {
      const channels = [...prev.notificationChannels];
      const index = channels.indexOf(channel);

      if (index > -1) {
        channels.splice(index, 1);
      } else {
        channels.push(channel);
      }

      return { ...prev, notificationChannels: channels };
    });
  };

  const handleAddInfoRequest = () => {
    setFormData(prev => ({
      ...prev,
      infoRequested: [...prev.infoRequested, '']
    }));
  };

  const handleRemoveInfoRequest = (index) => {
    setFormData(prev => ({
      ...prev,
      infoRequested: prev.infoRequested.filter((_, i) => i !== index)
    }));
  };

  const handleInfoRequestChange = (index, value) => {
    setFormData(prev => {
      const updated = [...prev.infoRequested];
      updated[index] = value;
      return { ...prev, infoRequested: updated };
    });
  };

  const validateStep1 = () => {
    if (actionType === 'REJECT' && !formData.rejectionReason.trim()) {
      toast.error('Please provide a rejection reason');
      return false;
    }

    if (actionType === 'REQUEST_INFO') {
      const validRequests = formData.infoRequested.filter(r => r.trim());
      if (validRequests.length === 0) {
        toast.error('Please specify at least one information request');
        return false;
      }
    }

    return true;
  };

  const validateStep2 = () => {
    if (formData.sendNotification) {
      if (formData.notificationChannels.length === 0) {
        toast.error('Please select at least one communication channel');
        return false;
      }

      if (!formData.subject.trim()) {
        toast.error('Please enter a subject');
        return false;
      }

      if (!formData.customMessage.trim()) {
        toast.error('Please enter a message');
        return false;
      }
    }

    return true;
  };

  const handleNext = () => {
    if (step === 1 && !validateStep1()) return;
    if (step === 2 && !validateStep2()) return;
    setStep(step + 1);
  };

  const handleBack = () => {
    setStep(step - 1);
  };

  const handleSubmit = async () => {
    setLoading(true);

    try {
      // Prepare request data
      const requestData = {
        action: actionType,
        remarks: formData.remarks,
        sendNotification: formData.sendNotification,
        notificationChannels: formData.notificationChannels
      };

      if (actionType === 'REJECT') {
        requestData.rejectionReason = formData.rejectionReason;
      }

      if (actionType === 'REQUEST_INFO') {
        requestData.infoRequested = formData.infoRequested.filter(r => r.trim());
      }

      if (formData.sendNotification) {
        requestData.customMessage = formData.customMessage;
        requestData.subject = formData.subject;
      }

      // Call API
      const result = await partnerRequestApi.updateStatus(request.requestId, requestData);

      if (result.success) {
        toast.success(result.message || 'Action completed successfully');
        onSuccess();
      } else {
        toast.error(result.message || 'Action failed');
      }
    } catch (error) {
      console.error('Error performing action:', error);
      toast.error('Network error. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  // Generate final message with replacements
  const getFinalMessage = () => {
    let message = formData.customMessage;

    if (actionType === 'REJECT' && formData.rejectionReason) {
      message = message.replace('[Reason will be added here]', formData.rejectionReason);
    }

    if (actionType === 'REQUEST_INFO') {
      const requests = formData.infoRequested
        .filter(r => r.trim())
        .map((r, i) => `${i + 1}. ${r}`)
        .join('\n');
      message = message.replace('[Information requests will be listed here]', requests);
    }

    return message;
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4">
      <div className="bg-white rounded-lg shadow-2xl max-w-3xl w-full max-h-[90vh] flex flex-col overflow-hidden">
        {/* Header */}
        <div className="px-6 py-4 border-b border-gray-200 flex items-center justify-between">
          <div className="flex items-center space-x-3">
            <div className={`w-10 h-10 ${config.iconBg} rounded-full flex items-center justify-center`}>
              <Icon className={`w-6 h-6 ${config.iconColor}`} />
            </div>
            <div>
              <h2 className="text-xl font-bold text-gray-900">{config.title}</h2>
              <p className="text-sm text-gray-600">{request.businessInfo.businessName}</p>
            </div>
          </div>

          <button
            onClick={onClose}
            className="p-2 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-lg transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Steps Indicator */}
        <div className="px-6 py-4 bg-gray-50 border-b border-gray-200">
          <div className="flex items-center justify-between max-w-lg mx-auto">
            {['Action Details', 'Communication', 'Preview'].map((label, index) => (
              <div key={label} className="flex items-center">
                <div className={`flex items-center space-x-2 ${
                  step > index + 1 ? 'text-green-600' :
                  step === index + 1 ? 'text-indigo-600' :
                  'text-gray-400'
                }`}>
                  <div className={`w-8 h-8 rounded-full flex items-center justify-center font-semibold ${
                    step > index + 1 ? 'bg-green-100' :
                    step === index + 1 ? 'bg-indigo-100' :
                    'bg-gray-100'
                  }`}>
                    {step > index + 1 ? '✓' : index + 1}
                  </div>
                  <span className="text-sm font-medium hidden sm:inline">{label}</span>
                </div>
                {index < 2 && (
                  <div className={`w-12 h-1 mx-2 ${
                    step > index + 1 ? 'bg-green-600' : 'bg-gray-200'
                  }`}></div>
                )}
              </div>
            ))}
          </div>
        </div>

        {/* Content */}
        <div className="flex-1 overflow-y-auto p-6">
          {/* Step 1: Action Details */}
          {step === 1 && (
            <div className="space-y-6">
              <div className="p-4 bg-blue-50 border border-blue-200 rounded-lg">
                <p className="text-sm text-blue-800">{config.description}</p>
              </div>

              {/* Rejection Reason */}
              {actionType === 'REJECT' && (
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Rejection Reason <span className="text-red-600">*</span>
                  </label>
                  <textarea
                    value={formData.rejectionReason}
                    onChange={(e) => setFormData({ ...formData, rejectionReason: e.target.value })}
                    rows="4"
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500"
                    placeholder="Explain why this request is being rejected..."
                  />
                </div>
              )}

              {/* Info Requested */}
              {actionType === 'REQUEST_INFO' && (
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Information Requested <span className="text-red-600">*</span>
                  </label>
                  {formData.infoRequested.map((item, index) => (
                    <div key={index} className="flex items-center space-x-2 mb-2">
                      <input
                        type="text"
                        value={item}
                        onChange={(e) => handleInfoRequestChange(index, e.target.value)}
                        className="flex-1 px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500"
                        placeholder="e.g., Updated FSSAI certificate"
                      />
                      {formData.infoRequested.length > 1 && (
                        <button
                          onClick={() => handleRemoveInfoRequest(index)}
                          className="p-2 text-red-600 hover:bg-red-50 rounded-lg"
                        >
                          <X className="w-5 h-5" />
                        </button>
                      )}
                    </div>
                  ))}
                  <button
                    onClick={handleAddInfoRequest}
                    className="text-sm text-indigo-600 hover:text-indigo-700 font-medium"
                  >
                    + Add Another Item
                  </button>
                </div>
              )}

              {/* Admin Remarks (Optional) */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Admin Remarks (Optional)
                </label>
                <textarea
                  value={formData.remarks}
                  onChange={(e) => setFormData({ ...formData, remarks: e.target.value })}
                  rows="3"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  placeholder="Internal notes (not visible to partner)..."
                />
              </div>
            </div>
          )}

          {/* Step 2: Communication */}
          {step === 2 && (
            <div className="space-y-6">
              {/* Send Notification Toggle */}
              <div className="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
                <div>
                  <p className="font-medium text-gray-900">Send Notification to Partner</p>
                  <p className="text-sm text-gray-600 mt-1">
                    Notify the partner about this action via email/SMS
                  </p>
                </div>
                <label className="relative inline-flex items-center cursor-pointer">
                  <input
                    type="checkbox"
                    checked={formData.sendNotification}
                    onChange={(e) => setFormData({ ...formData, sendNotification: e.target.checked })}
                    className="sr-only peer"
                  />
                  <div className="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-indigo-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-indigo-600"></div>
                </label>
              </div>

              {formData.sendNotification && (
                <>
                  {/* Communication Channels */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-3">
                      Communication Channels <span className="text-red-600">*</span>
                    </label>
                    <div className="grid grid-cols-2 gap-3">
                      <button
                        onClick={() => handleChannelToggle('EMAIL')}
                        className={`p-4 border-2 rounded-lg transition-all ${
                          formData.notificationChannels.includes('EMAIL')
                            ? 'border-indigo-600 bg-indigo-50'
                            : 'border-gray-200 hover:border-gray-300'
                        }`}
                      >
                        <Mail className={`w-6 h-6 mx-auto mb-2 ${
                          formData.notificationChannels.includes('EMAIL')
                            ? 'text-indigo-600'
                            : 'text-gray-400'
                        }`} />
                        <p className="text-sm font-medium">Email</p>
                        <p className="text-xs text-gray-500 mt-1">{request.ownerInfo.email}</p>
                      </button>

                      <button
                        onClick={() => handleChannelToggle('SMS')}
                        className={`p-4 border-2 rounded-lg transition-all ${
                          formData.notificationChannels.includes('SMS')
                            ? 'border-indigo-600 bg-indigo-50'
                            : 'border-gray-200 hover:border-gray-300'
                        }`}
                      >
                        <MessageSquare className={`w-6 h-6 mx-auto mb-2 ${
                          formData.notificationChannels.includes('SMS')
                            ? 'text-indigo-600'
                            : 'text-gray-400'
                        }`} />
                        <p className="text-sm font-medium">SMS</p>
                        <p className="text-xs text-gray-500 mt-1">{request.ownerInfo.phone}</p>
                      </button>
                    </div>
                  </div>

                  {/* Subject */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Subject {formData.notificationChannels.includes('EMAIL') && <span className="text-red-600">*</span>}
                    </label>
                    <input
                      type="text"
                      value={formData.subject}
                      onChange={(e) => setFormData({ ...formData, subject: e.target.value })}
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500"
                      placeholder="Email subject..."
                    />
                  </div>

                  {/* Message */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Message <span className="text-red-600">*</span>
                    </label>
                    <textarea
                      value={formData.customMessage}
                      onChange={(e) => setFormData({ ...formData, customMessage: e.target.value })}
                      rows="10"
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 font-mono text-sm"
                      placeholder="Message content..."
                    />
                    <p className="text-xs text-gray-500 mt-2">
                      This message will be sent to the partner via selected channels
                    </p>
                  </div>
                </>
              )}
            </div>
          )}

          {/* Step 3: Preview */}
          {step === 3 && (
            <div className="space-y-6">
              <div className="p-4 bg-yellow-50 border border-yellow-200 rounded-lg flex items-start space-x-3">
                <AlertCircle className="w-5 h-5 text-yellow-600 flex-shrink-0 mt-0.5" />
                <div>
                  <p className="text-sm font-medium text-yellow-900">Review Before Confirming</p>
                  <p className="text-sm text-yellow-700 mt-1">
                    Please review all details carefully before submitting.
                  </p>
                </div>
              </div>

              {/* Action Summary */}
              <div className="border border-gray-200 rounded-lg p-4">
                <h3 className="font-semibold text-gray-900 mb-3">Action Summary</h3>
                <dl className="space-y-2">
                  <div className="flex justify-between">
                    <dt className="text-sm text-gray-600">Action:</dt>
                    <dd className="text-sm font-medium text-gray-900">{config.title}</dd>
                  </div>
                  <div className="flex justify-between">
                    <dt className="text-sm text-gray-600">Partner:</dt>
                    <dd className="text-sm font-medium text-gray-900">{request.businessInfo.businessName}</dd>
                  </div>
                  {actionType === 'REJECT' && formData.rejectionReason && (
                    <div>
                      <dt className="text-sm text-gray-600 mb-1">Rejection Reason:</dt>
                      <dd className="text-sm text-gray-900 bg-gray-50 p-2 rounded">{formData.rejectionReason}</dd>
                    </div>
                  )}
                  {actionType === 'REQUEST_INFO' && (
                    <div>
                      <dt className="text-sm text-gray-600 mb-1">Information Requested:</dt>
                      <dd className="text-sm text-gray-900">
                        <ul className="list-disc list-inside bg-gray-50 p-2 rounded">
                          {formData.infoRequested.filter(r => r.trim()).map((item, i) => (
                            <li key={i}>{item}</li>
                          ))}
                        </ul>
                      </dd>
                    </div>
                  )}
                </dl>
              </div>

              {/* Communication Preview */}
              {formData.sendNotification && (
                <div className="border border-gray-200 rounded-lg p-4">
                  <h3 className="font-semibold text-gray-900 mb-3 flex items-center">
                    <Eye className="w-5 h-5 mr-2 text-indigo-600" />
                    Message Preview
                  </h3>
                  <div className="space-y-3">
                    <div>
                      <p className="text-xs text-gray-600 mb-1">Channels:</p>
                      <div className="flex items-center space-x-2">
                        {formData.notificationChannels.map(channel => (
                          <span key={channel} className="inline-flex items-center px-2 py-1 bg-indigo-100 text-indigo-700 text-xs font-medium rounded">
                            {channel === 'EMAIL' ? <Mail className="w-3 h-3 mr-1" /> : <MessageSquare className="w-3 h-3 mr-1" />}
                            {channel}
                          </span>
                        ))}
                      </div>
                    </div>
                    <div>
                      <p className="text-xs text-gray-600 mb-1">Subject:</p>
                      <p className="text-sm font-medium text-gray-900">{formData.subject}</p>
                    </div>
                    <div>
                      <p className="text-xs text-gray-600 mb-1">Message:</p>
                      <div className="bg-gray-50 p-3 rounded text-sm text-gray-900 whitespace-pre-wrap font-mono">
                        {getFinalMessage()}
                      </div>
                    </div>
                  </div>
                </div>
              )}
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="px-6 py-4 border-t border-gray-200 flex items-center justify-between bg-gray-50">
          <button
            onClick={step === 1 ? onClose : handleBack}
            className="px-6 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-100 transition-colors"
          >
            {step === 1 ? 'Cancel' : 'Back'}
          </button>

          <div className="flex items-center space-x-3">
            {step < 3 ? (
              <button
                onClick={handleNext}
                className="px-6 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors"
              >
                Next
              </button>
            ) : (
              <button
                onClick={handleSubmit}
                disabled={loading}
                className={`px-6 py-2 text-white rounded-lg transition-colors flex items-center ${config.buttonColor} disabled:opacity-50 disabled:cursor-not-allowed`}
              >
                {loading ? (
                  <>
                    <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin mr-2"></div>
                    Processing...
                  </>
                ) : (
                  <>
                    <Send className="w-4 h-4 mr-2" />
                    Confirm & {actionType === 'APPROVE' ? 'Approve' : actionType === 'REJECT' ? 'Reject' : 'Send'}
                  </>
                )}
              </button>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default PartnerActionModal;
