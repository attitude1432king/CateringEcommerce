import React, { useState } from 'react';
import {
  FileText,
  Camera,
  DollarSign,
  CheckCircle,
  AlertCircle,
  ChevronRight,
  ChevronLeft,
  Info,
  Upload
} from 'lucide-react';
import { DisabledButton } from '../../common/safety';

/**
 * ComplaintSubmissionWizard Component
 *
 * Multi-step complaint submission:
 * Step 1: Complaint type with VALID/INVALID examples
 * Step 2: Mandatory photo/video upload with timestamp
 * Step 3: Refund estimate display (read-only)
 */

const ComplaintSubmissionWizard = ({
  order,
  onSubmit,
  onCancel,
  isLoading = false
}) => {
  const [currentStep, setCurrentStep] = useState(1);
  const [complaintData, setComplaintData] = useState({
    type: '',
    description: '',
    affectedItems: [],
    media: [],
    severity: '',
    guestCount: order.guestCount,
    guestsAffected: 0
  });

  // Complaint types with validation examples
  const complaintTypes = [
    {
      id: 'quality',
      label: 'Food Quality Issues',
      maxRefundPercentage: 15,
      validExamples: [
        'Food was cold or undercooked',
        'Dishes tasted stale or spoiled',
        'Wrong spice levels despite instructions'
      ],
      invalidExamples: [
        'I didn\'t like the taste (personal preference)',
        'Portion size was smaller than expected',
        'Food was too spicy (if not specified in order)'
      ]
    },
    {
      id: 'service',
      label: 'Service Delivery Issues',
      maxRefundPercentage: 20,
      validExamples: [
        'Food arrived significantly late (>1 hour)',
        'Incomplete order (missing items)',
        'Serving staff was unprofessional'
      ],
      invalidExamples: [
        'Staff uniform wasn\'t to my liking',
        'Minor delays (<30 minutes)',
        'Partner used different serving dishes'
      ]
    },
    {
      id: 'hygiene',
      label: 'Hygiene & Safety Concerns',
      maxRefundPercentage: 30,
      validExamples: [
        'Foreign objects found in food',
        'Food poisoning or illness reported',
        'Unhygienic food handling observed'
      ],
      invalidExamples: [
        'Food looked different from photos',
        'Garnish was missing',
        'Didn\'t see chef washing hands (without proof)'
      ]
    },
    {
      id: 'quantity',
      label: 'Quantity Shortage',
      maxRefundPercentage: 25,
      validExamples: [
        'Confirmed guest count not served',
        'Dishes ran out before all guests served',
        'Measurable shortage in agreed portions'
      ],
      invalidExamples: [
        'Felt portions were small (subjective)',
        'Guests wanted seconds but none available',
        'Expected more variety'
      ]
    }
  ];

  const selectedType = complaintTypes.find(t => t.id === complaintData.type);

  // Calculate estimated refund
  const calculateRefund = () => {
    if (!selectedType) return { min: 0, max: 0 };

    const baseRefund = order.totalAmount * (selectedType.maxRefundPercentage / 100);

    // Adjust based on severity and affected guests
    const affectedPercentage = complaintData.guestsAffected / order.guestCount;
    const severityMultiplier = {
      minor: 0.3,
      moderate: 0.6,
      major: 1.0
    }[complaintData.severity] || 0.5;

    const estimatedRefund = baseRefund * affectedPercentage * severityMultiplier;

    return {
      min: estimatedRefund * 0.7, // 70% of estimated
      max: Math.min(estimatedRefund * 1.3, baseRefund), // 130% or max cap
      maxCap: baseRefund
    };
  };

  // Handle media upload
  const handleMediaUpload = (e) => {
    const files = Array.from(e.target.files);
    const mediaWithTimestamp = files.map(file => ({
      file,
      timestamp: new Date(),
      preview: URL.createObjectURL(file)
    }));

    setComplaintData(prev => ({
      ...prev,
      media: [...prev.media, ...mediaWithTimestamp]
    }));
  };

  // Remove media
  const handleRemoveMedia = (index) => {
    setComplaintData(prev => ({
      ...prev,
      media: prev.media.filter((_, i) => i !== index)
    }));
  };

  // Handle submit
  const handleSubmit = () => {
    const refundEstimate = calculateRefund();
    onSubmit({
      ...complaintData,
      orderId: order.orderId,
      refundEstimate,
      submittedAt: new Date()
    });
  };

  // Validation for each step
  const canProceedToStep = (step) => {
    switch (step) {
      case 2:
        return complaintData.type && complaintData.description.trim().length >= 50;
      case 3:
        return complaintData.media.length > 0 && complaintData.severity && complaintData.guestsAffected > 0;
      default:
        return true;
    }
  };

  const steps = [
    { number: 1, title: 'Complaint Type', icon: FileText },
    { number: 2, title: 'Evidence Upload', icon: Camera },
    { number: 3, title: 'Review & Submit', icon: DollarSign }
  ];

  return (
    <div className="bg-white rounded-lg shadow-lg max-w-4xl mx-auto">
      {/* Progress Steps */}
      <div className="border-b border-gray-200 p-6">
        <div className="flex items-center justify-between">
          {steps.map((step, index) => {
            const StepIcon = step.icon;
            const isActive = currentStep === step.number;
            const isCompleted = currentStep > step.number;

            return (
              <React.Fragment key={step.number}>
                <div className="flex flex-col items-center flex-1">
                  <div className={`
                    w-12 h-12 rounded-full flex items-center justify-center mb-2
                    ${isActive ? 'bg-blue-600 text-white' :
                      isCompleted ? 'bg-green-600 text-white' :
                      'bg-gray-200 text-neutral-600'}
                  `}>
                    {isCompleted ? (
                      <CheckCircle className="w-6 h-6" />
                    ) : (
                      <StepIcon className="w-6 h-6" />
                    )}
                  </div>
                  <span className={`text-sm font-medium ${isActive ? 'text-blue-700' : 'text-neutral-600'}`}>
                    {step.title}
                  </span>
                </div>
                {index < steps.length - 1 && (
                  <div className={`flex-1 h-1 mx-2 mt-[-30px] ${
                    isCompleted ? 'bg-green-600' : 'bg-gray-200'
                  }`} />
                )}
              </React.Fragment>
            );
          })}
        </div>
      </div>

      {/* Step Content */}
      <div className="p-6 min-h-[500px]">
        {/* Step 1: Complaint Type Selection */}
        {currentStep === 1 && (
          <div className="space-y-4">
            <h2 className="text-2xl font-bold mb-4">Select Complaint Type</h2>

            {complaintTypes.map(type => (
              <div
                key={type.id}
                className={`border-2 rounded-lg p-4 cursor-pointer transition-all ${
                  complaintData.type === type.id
                    ? 'border-blue-500 bg-blue-50'
                    : 'border-gray-300 hover:border-blue-300'
                }`}
                onClick={() => setComplaintData(prev => ({ ...prev, type: type.id }))}
              >
                <div className="flex items-center justify-between mb-2">
                  <h3 className="font-semibold text-lg">{type.label}</h3>
                  <span className="text-sm bg-gray-100 px-3 py-1 rounded-full">
                    Max {type.maxRefundPercentage}% refund
                  </span>
                </div>

                {complaintData.type === type.id && (
                  <div className="mt-4 space-y-3">
                    {/* Valid Examples */}
                    <div className="bg-green-50 border border-green-200 rounded p-3">
                      <p className="font-medium text-green-900 text-sm mb-2 flex items-center gap-2">
                        <CheckCircle className="w-4 h-4" />
                        VALID Examples:
                      </p>
                      <ul className="list-disc list-inside space-y-1 text-sm text-green-800">
                        {type.validExamples.map((example, i) => (
                          <li key={i}>{example}</li>
                        ))}
                      </ul>
                    </div>

                    {/* Invalid Examples */}
                    <div className="bg-red-50 border border-red-200 rounded p-3">
                      <p className="font-medium text-red-900 text-sm mb-2 flex items-center gap-2">
                        <AlertCircle className="w-4 h-4" />
                        INVALID Examples (will be rejected):
                      </p>
                      <ul className="list-disc list-inside space-y-1 text-sm text-red-800">
                        {type.invalidExamples.map((example, i) => (
                          <li key={i}>{example}</li>
                        ))}
                      </ul>
                    </div>
                  </div>
                )}
              </div>
            ))}

            {/* Description */}
            {complaintData.type && (
              <div className="mt-6">
                <label className="block font-medium mb-2">
                  Detailed Description <span className="text-red-600">*</span>
                </label>
                <textarea
                  value={complaintData.description}
                  onChange={(e) => setComplaintData(prev => ({ ...prev, description: e.target.value }))}
                  placeholder="Describe the issue in detail. Be specific about what went wrong..."
                  rows={5}
                  className="w-full px-4 py-3 border-2 border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
                <p className="text-sm text-neutral-600 mt-1">
                  {complaintData.description.length} / 50 minimum characters
                </p>
              </div>
            )}
          </div>
        )}

        {/* Step 2: Evidence Upload */}
        {currentStep === 2 && (
          <div className="space-y-4">
            <h2 className="text-2xl font-bold mb-4">Upload Evidence</h2>

            <div className="bg-amber-50 border border-amber-200 rounded-lg p-4 mb-4">
              <div className="flex items-start gap-2">
                <Info className="w-5 h-5 text-amber-700 flex-shrink-0 mt-0.5" />
                <div className="text-sm text-amber-900">
                  <p className="font-medium mb-1">Evidence Requirements:</p>
                  <ul className="list-disc list-inside space-y-1">
                    <li>Photos or videos showing the issue clearly</li>
                    <li>Timestamp will be auto-captured for authenticity</li>
                    <li>Multiple angles recommended for food quality issues</li>
                    <li>Include context (e.g., full table setup for quantity issues)</li>
                  </ul>
                </div>
              </div>
            </div>

            {/* Upload Area */}
            <div className="border-2 border-dashed border-gray-300 rounded-lg p-8 text-center">
              <Upload className="w-12 h-12 text-gray-400 mx-auto mb-4" />
              <p className="text-neutral-700 font-medium mb-2">Upload Photos or Videos</p>
              <p className="text-sm text-neutral-600 mb-4">
                Accepted formats: JPG, PNG, MP4, MOV (Max 10MB per file)
              </p>
              <input
                type="file"
                accept="image/*,video/*"
                multiple
                onChange={handleMediaUpload}
                className="hidden"
                id="media-upload"
              />
              <label
                htmlFor="media-upload"
                className="inline-block px-6 py-3 bg-blue-600 text-white rounded-lg cursor-pointer hover:bg-blue-700 transition-colors"
              >
                Choose Files
              </label>
            </div>

            {/* Uploaded Media Preview */}
            {complaintData.media.length > 0 && (
              <div className="grid grid-cols-2 md:grid-cols-3 gap-4 mt-4">
                {complaintData.media.map((media, index) => (
                  <div key={index} className="relative border rounded-lg overflow-hidden">
                    {media.file.type.startsWith('image/') ? (
                      <img src={media.preview} alt={`Evidence ${index + 1}`} className="w-full h-32 object-cover" />
                    ) : (
                      <video src={media.preview} className="w-full h-32 object-cover" />
                    )}
                    <div className="absolute top-0 right-0 bg-black bg-opacity-50 text-white text-xs px-2 py-1">
                      {media.timestamp.toLocaleTimeString()}
                    </div>
                    <button
                      onClick={() => handleRemoveMedia(index)}
                      className="absolute bottom-2 right-2 bg-red-600 text-white px-2 py-1 rounded text-xs hover:bg-red-700"
                    >
                      Remove
                    </button>
                  </div>
                ))}
              </div>
            )}

            {/* Additional Details */}
            <div className="grid grid-cols-2 gap-4 mt-6">
              <div>
                <label className="block font-medium mb-2">
                  Severity <span className="text-red-600">*</span>
                </label>
                <select
                  value={complaintData.severity}
                  onChange={(e) => setComplaintData(prev => ({ ...prev, severity: e.target.value }))}
                  className="w-full px-4 py-2 border-2 border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                >
                  <option value="">Select severity...</option>
                  <option value="minor">Minor - Small inconvenience</option>
                  <option value="moderate">Moderate - Noticeable impact</option>
                  <option value="major">Major - Significant issue</option>
                </select>
              </div>

              <div>
                <label className="block font-medium mb-2">
                  Guests Affected <span className="text-red-600">*</span>
                </label>
                <input
                  type="number"
                  value={complaintData.guestsAffected}
                  onChange={(e) => setComplaintData(prev => ({
                    ...prev,
                    guestsAffected: Math.min(parseInt(e.target.value) || 0, order.guestCount)
                  }))}
                  min="1"
                  max={order.guestCount}
                  className="w-full px-4 py-2 border-2 border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
                <p className="text-xs text-neutral-600 mt-1">Out of {order.guestCount} total guests</p>
              </div>
            </div>
          </div>
        )}

        {/* Step 3: Review & Submit */}
        {currentStep === 3 && (
          <div className="space-y-6">
            <h2 className="text-2xl font-bold mb-4">Review & Submit</h2>

            {/* Summary */}
            <div className="bg-gray-50 rounded-lg p-4">
              <h3 className="font-semibold mb-3">Complaint Summary</h3>
              <dl className="space-y-2 text-sm">
                <div className="flex justify-between">
                  <dt className="text-neutral-600">Type:</dt>
                  <dd className="font-medium">{selectedType?.label}</dd>
                </div>
                <div className="flex justify-between">
                  <dt className="text-neutral-600">Severity:</dt>
                  <dd className="font-medium capitalize">{complaintData.severity}</dd>
                </div>
                <div className="flex justify-between">
                  <dt className="text-neutral-600">Guests Affected:</dt>
                  <dd className="font-medium">{complaintData.guestsAffected} / {order.guestCount}</dd>
                </div>
                <div className="flex justify-between">
                  <dt className="text-neutral-600">Evidence Files:</dt>
                  <dd className="font-medium">{complaintData.media.length} file(s)</dd>
                </div>
              </dl>
            </div>

            {/* Refund Estimate */}
            <div className="bg-blue-50 border-2 border-blue-300 rounded-lg p-6">
              <div className="flex items-start gap-3 mb-4">
                <DollarSign className="w-6 h-6 text-blue-700 flex-shrink-0" />
                <div className="flex-1">
                  <h3 className="font-semibold text-blue-900 text-lg mb-1">Estimated Refund Range</h3>
                  <p className="text-sm text-blue-800">
                    Based on complaint type, severity, and affected guests
                  </p>
                </div>
              </div>

              <div className="bg-white rounded-lg p-4 mb-3">
                <div className="flex items-center justify-between mb-2">
                  <span className="text-neutral-700">Estimated Range:</span>
                  <span className="text-2xl font-bold text-blue-700">
                    ₹{calculateRefund().min.toFixed(2)} - ₹{calculateRefund().max.toFixed(2)}
                  </span>
                </div>
                <div className="flex items-center justify-between text-sm text-neutral-600">
                  <span>Maximum Possible:</span>
                  <span className="font-medium">₹{calculateRefund().maxCap.toFixed(2)}</span>
                </div>
              </div>

              <p className="text-xs text-blue-800 bg-blue-100 rounded p-2">
                <Info className="w-3 h-3 inline mr-1" />
                Final refund amount will be determined after vendor review and investigation. This is an estimate only.
              </p>
            </div>

            {/* Important Notice */}
            <div className="bg-amber-50 border border-amber-200 rounded-lg p-4">
              <p className="font-semibold text-amber-900 mb-2">Important:</p>
              <ul className="list-disc list-inside space-y-1 text-sm text-amber-800">
                <li>Complaints are reviewed within 24-48 hours</li>
                <li>Partner will have opportunity to respond</li>
                <li>False or exaggerated claims may result in account penalties</li>
                <li>You will be notified of the decision via email and app notification</li>
              </ul>
            </div>
          </div>
        )}
      </div>

      {/* Navigation Buttons */}
      <div className="border-t border-gray-200 p-6 flex items-center justify-between">
        <button
          onClick={currentStep === 1 ? onCancel : () => setCurrentStep(prev => prev - 1)}
          className="px-6 py-3 border-2 border-gray-300 text-neutral-700 rounded-lg hover:bg-gray-50 transition-colors font-medium flex items-center gap-2"
        >
          <ChevronLeft className="w-5 h-5" />
          {currentStep === 1 ? 'Cancel' : 'Previous'}
        </button>

        {currentStep < 3 ? (
          <DisabledButton
            onClick={() => setCurrentStep(prev => prev + 1)}
            disabled={!canProceedToStep(currentStep + 1)}
            disabledReason={
              currentStep === 1
                ? 'Please select a complaint type and provide detailed description (min 50 characters)'
                : 'Please upload evidence and fill in all required fields'
            }
            variant="primary"
            icon={ChevronRight}
          >
            Next Step
          </DisabledButton>
        ) : (
          <DisabledButton
            onClick={handleSubmit}
            disabled={isLoading}
            variant="primary"
            loading={isLoading}
            icon={CheckCircle}
          >
            Submit Complaint
          </DisabledButton>
        )}
      </div>
    </div>
  );
};

export default ComplaintSubmissionWizard;
