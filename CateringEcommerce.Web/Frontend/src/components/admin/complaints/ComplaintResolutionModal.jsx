import React, { useState, useEffect } from 'react';
import {
  X,
  DollarSign,
  Calculator,
  CheckCircle,
  XCircle,
  Loader
} from 'lucide-react';
import { calculateComplaintRefund } from '../../../services/adminComplaintApi';
import { toast } from 'react-hot-toast'; // P1 FIX: Replace alert() with toast

const ComplaintResolutionModal = ({ complaint, isOpen, onClose, onSubmitResolution }) => {
  const [resolutionType, setResolutionType] = useState('PARTIAL_REFUND');
  const [refundAmount, setRefundAmount] = useState(0);
  const [goodwillCredit, setGoodwillCredit] = useState(0);
  const [isValidComplaint, setIsValidComplaint] = useState(true);
  const [validityReason, setValidityReason] = useState('');
  const [resolutionNotes, setResolutionNotes] = useState('');
  const [calculatedRefund, setCalculatedRefund] = useState(null);
  const [isCalculating, setIsCalculating] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (isOpen && complaint) {
      // Reset form
      setResolutionType('PARTIAL_REFUND');
      setRefundAmount(0);
      setGoodwillCredit(0);
      setIsValidComplaint(true);
      setValidityReason('');
      setResolutionNotes('');
      setCalculatedRefund(null);

      // Auto-calculate refund on open
      handleCalculateRefund();
    }
  }, [isOpen, complaint]);

  const handleCalculateRefund = async () => {
    if (!complaint) return;

    setIsCalculating(true);
    try {
      const response = await calculateComplaintRefund(complaint.complaintId);

      if (response.success && response.data) {
        setCalculatedRefund(response.data);
        setRefundAmount(response.data.recommendedRefund || 0);
      }
    } catch (error) {
      console.error('Error calculating refund:', error);
    } finally {
      setIsCalculating(false);
    }
  };

  const handleSubmit = async () => {
    // P1 FIX: Replace alert() with toast notifications for better UX
    if (!isValidComplaint && !validityReason.trim()) {
      toast.error('Please provide a validity reason for rejecting the complaint.');
      return;
    }

    if (isValidComplaint && resolutionType !== 'NO_RESOLUTION' && refundAmount <= 0 && goodwillCredit <= 0) {
      toast.error('Please provide either a refund amount or goodwill credit for approved complaints.');
      return;
    }

    const resolutionData = {
      complaintId: complaint.complaintId,
      resolutionType,
      refundAmount: parseFloat(refundAmount) || 0,
      goodwillCredit: parseFloat(goodwillCredit) || 0,
      isValidComplaint,
      validityReason: isValidComplaint ? '' : validityReason,
      resolutionNotes
    };

    setIsSubmitting(true);
    try {
      await onSubmitResolution(resolutionData);
      onClose();
    } catch (error) {
      console.error('Error submitting resolution:', error);
    } finally {
      setIsSubmitting(false);
    }
  };

  if (!isOpen || !complaint) return null;

  return (
    <>
      {/* Overlay */}
      <div
        className="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4"
        onClick={onClose}
      >
        {/* Modal */}
        <div
          className="bg-white rounded-lg shadow-2xl max-w-3xl w-full max-h-[90vh] overflow-y-auto"
          onClick={(e) => e.stopPropagation()}
        >
          {/* Header */}
          <div className="sticky top-0 bg-white border-b border-gray-200 px-6 py-4 flex items-center justify-between z-10">
            <div>
              <h2 className="text-2xl font-bold text-gray-900">Resolve Complaint</h2>
              <p className="text-sm text-gray-600">Complaint #{complaint.complaintId}</p>
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
            {/* Refund Calculator */}
            {calculatedRefund && (
              <div className="bg-blue-50 border-2 border-blue-300 rounded-lg p-4">
                <div className="flex items-start gap-3 mb-3">
                  <Calculator className="w-6 h-6 text-blue-700 flex-shrink-0" />
                  <div className="flex-1">
                    <h3 className="font-semibold text-blue-900 mb-1">Calculated Refund</h3>
                    <p className="text-sm text-blue-800">
                      Based on complaint type, severity, and impact
                    </p>
                  </div>
                </div>

                <div className="bg-white rounded-lg p-4 space-y-2 text-sm">
                  <div className="flex justify-between">
                    <span className="text-gray-600">Order Total:</span>
                    <span className="font-medium">₹{calculatedRefund.orderTotal?.toFixed(2)}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-600">Severity Factor:</span>
                    <span className="font-medium">{calculatedRefund.severityFactor}x</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-600">Calculated Refund:</span>
                    <span className="font-medium">₹{calculatedRefund.calculatedRefund?.toFixed(2)}</span>
                  </div>
                  <div className="flex justify-between border-t pt-2">
                    <span className="text-gray-600">Max Allowed:</span>
                    <span className="font-medium text-blue-700">₹{calculatedRefund.maxRefundAllowed?.toFixed(2)}</span>
                  </div>
                  <div className="flex justify-between border-t pt-2 font-bold text-lg">
                    <span className="text-gray-900">Recommended:</span>
                    <span className="text-green-700">₹{calculatedRefund.recommendedRefund?.toFixed(2)}</span>
                  </div>
                </div>

                {calculatedRefund.explanation && (
                  <p className="text-xs text-blue-800 mt-3 bg-blue-100 rounded p-2">
                    {calculatedRefund.explanation}
                  </p>
                )}

                <button
                  onClick={handleCalculateRefund}
                  disabled={isCalculating}
                  className="mt-3 w-full px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50 flex items-center justify-center gap-2"
                >
                  {isCalculating ? (
                    <>
                      <Loader className="w-4 h-4 animate-spin" />
                      Recalculating...
                    </>
                  ) : (
                    <>
                      <Calculator className="w-4 h-4" />
                      Recalculate
                    </>
                  )}
                </button>
              </div>
            )}

            {/* Complaint Validity */}
            <div>
              <label className="block font-semibold text-gray-900 mb-3">
                Is this a valid complaint?
              </label>
              <div className="flex gap-4">
                <label className="flex-1 cursor-pointer">
                  <input
                    type="radio"
                    name="validity"
                    checked={isValidComplaint}
                    onChange={() => setIsValidComplaint(true)}
                    className="sr-only"
                  />
                  <div className={`border-2 rounded-lg p-4 transition-colors ${
                    isValidComplaint ? 'border-green-500 bg-green-50' : 'border-gray-300 bg-white'
                  }`}>
                    <div className="flex items-center gap-3">
                      <CheckCircle className={`w-6 h-6 ${isValidComplaint ? 'text-green-600' : 'text-gray-400'}`} />
                      <div>
                        <p className="font-medium text-gray-900">Valid Complaint</p>
                        <p className="text-sm text-gray-600">Approve with refund/credit</p>
                      </div>
                    </div>
                  </div>
                </label>

                <label className="flex-1 cursor-pointer">
                  <input
                    type="radio"
                    name="validity"
                    checked={!isValidComplaint}
                    onChange={() => setIsValidComplaint(false)}
                    className="sr-only"
                  />
                  <div className={`border-2 rounded-lg p-4 transition-colors ${
                    !isValidComplaint ? 'border-red-500 bg-red-50' : 'border-gray-300 bg-white'
                  }`}>
                    <div className="flex items-center gap-3">
                      <XCircle className={`w-6 h-6 ${!isValidComplaint ? 'text-red-600' : 'text-gray-400'}`} />
                      <div>
                        <p className="font-medium text-gray-900">Invalid Complaint</p>
                        <p className="text-sm text-gray-600">Reject with reason</p>
                      </div>
                    </div>
                  </div>
                </label>
              </div>
            </div>

            {/* Resolution Type (only if valid) */}
            {isValidComplaint && (
              <div>
                <label className="block font-semibold text-gray-900 mb-2">
                  Resolution Type <span className="text-red-600">*</span>
                </label>
                <select
                  value={resolutionType}
                  onChange={(e) => setResolutionType(e.target.value)}
                  className="w-full px-4 py-3 border-2 border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                >
                  <option value="FULL_REFUND">Full Refund</option>
                  <option value="PARTIAL_REFUND">Partial Refund</option>
                  <option value="GOODWILL_CREDIT">Goodwill Credit Only</option>
                  <option value="REPLACEMENT">Replacement Service</option>
                  <option value="NO_RESOLUTION">No Financial Resolution</option>
                </select>
              </div>
            )}

            {/* Refund Amount (only if valid) */}
            {isValidComplaint && (resolutionType === 'FULL_REFUND' || resolutionType === 'PARTIAL_REFUND') && (
              <div>
                <label className="block font-semibold text-gray-900 mb-2">
                  Refund Amount (₹) <span className="text-red-600">*</span>
                </label>
                <div className="relative">
                  <DollarSign className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
                  <input
                    type="number"
                    value={refundAmount}
                    onChange={(e) => setRefundAmount(e.target.value)}
                    min="0"
                    step="0.01"
                    className="w-full pl-10 pr-4 py-3 border-2 border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    placeholder="Enter refund amount"
                  />
                </div>
                {calculatedRefund && (
                  <p className="text-xs text-gray-600 mt-1">
                    Recommended: ₹{calculatedRefund.recommendedRefund?.toFixed(2)} | Max: ₹{calculatedRefund.maxRefundAllowed?.toFixed(2)}
                  </p>
                )}
              </div>
            )}

            {/* Goodwill Credit (only if valid) */}
            {isValidComplaint && (
              <div>
                <label className="block font-semibold text-gray-900 mb-2">
                  Goodwill Credit (₹) <span className="text-gray-500 text-sm">(Optional)</span>
                </label>
                <input
                  type="number"
                  value={goodwillCredit}
                  onChange={(e) => setGoodwillCredit(e.target.value)}
                  min="0"
                  step="0.01"
                  className="w-full px-4 py-3 border-2 border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  placeholder="Optional goodwill credit"
                />
              </div>
            )}

            {/* Validity Reason (only if invalid) */}
            {!isValidComplaint && (
              <div>
                <label className="block font-semibold text-gray-900 mb-2">
                  Rejection Reason <span className="text-red-600">*</span>
                </label>
                <textarea
                  value={validityReason}
                  onChange={(e) => setValidityReason(e.target.value)}
                  rows={4}
                  className="w-full px-4 py-3 border-2 border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  placeholder="Explain why this complaint is being rejected..."
                />
              </div>
            )}

            {/* Resolution Notes */}
            <div>
              <label className="block font-semibold text-gray-900 mb-2">
                Resolution Notes <span className="text-gray-500 text-sm">(Optional)</span>
              </label>
              <textarea
                value={resolutionNotes}
                onChange={(e) => setResolutionNotes(e.target.value)}
                rows={4}
                className="w-full px-4 py-3 border-2 border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="Additional notes for internal tracking and customer communication..."
              />
            </div>
          </div>

          {/* Footer Actions */}
          <div className="sticky bottom-0 bg-white border-t border-gray-200 px-6 py-4 flex gap-3">
            <button
              onClick={onClose}
              disabled={isSubmitting}
              className="flex-1 px-6 py-3 border-2 border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors font-semibold disabled:opacity-50"
            >
              Cancel
            </button>
            <button
              onClick={handleSubmit}
              disabled={isSubmitting}
              className="flex-1 px-6 py-3 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors font-semibold disabled:opacity-50 flex items-center justify-center gap-2"
            >
              {isSubmitting ? (
                <>
                  <Loader className="w-5 h-5 animate-spin" />
                  Submitting...
                </>
              ) : (
                <>
                  <CheckCircle className="w-5 h-5" />
                  Submit Resolution
                </>
              )}
            </button>
          </div>
        </div>
      </div>
    </>
  );
};

export default ComplaintResolutionModal;
