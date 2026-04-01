import React, { useState } from 'react';
import { CheckCircle2, Star, FileText, AlertCircle, IndianRupee, Loader2 } from 'lucide-react';
import OTPVerificationModal from '../../common/OTPVerificationModal';
import { useAuth } from '../../../contexts/AuthContext';

/**
 * PostEventPaymentSection Component
 *
 * Shown on OrderDetailPage after event completion when supervisor report is submitted.
 * Displays: final amount summary, quality rating, supervisor notes, "Approve & Pay" CTA.
 * CTA only enabled when supervisorReportSubmitted=true AND paymentRequestRaised=true.
 */

const PostEventPaymentSection = ({ order, liveEventStatus, onPaymentApproved }) => {
  const { user } = useAuth();
  const [isProcessing, setIsProcessing] = useState(false);
  const [paymentComplete, setPaymentComplete] = useState(false);
  const [showOtpModal, setShowOtpModal] = useState(false);

  if (!liveEventStatus || !liveEventStatus.supervisorReportSubmitted) return null;

  const canPay = liveEventStatus.supervisorReportSubmitted && liveEventStatus.paymentRequestRaised;
  const postEventAmount = order.totalAmount * 0.3;
  const extraCharges = liveEventStatus.extraChargesAmount || 0;
  const finalAmount = liveEventStatus.finalPayableAmount || (postEventAmount + extraCharges);

  // Show OTP modal before approving payment
  const handleApproveAndPay = () => {
    if (!canPay || isProcessing) return;
    setShowOtpModal(true);
  };

  // Handle OTP verification success
  const handleOtpVerified = async (otp, token) => {
    setShowOtpModal(false);
    await actuallyApprovePayment();
  };

  // Actually approve the payment after OTP verification
  const actuallyApprovePayment = async () => {
    setIsProcessing(true);
    try {
      if (onPaymentApproved) {
        await onPaymentApproved(order.orderId, finalAmount);
      }
      setPaymentComplete(true);
    } catch (error) {
      console.error('Payment failed:', error);
      alert(error.message || 'Payment processing failed. Please try again.');
    } finally {
      setIsProcessing(false);
    }
  };

  // Star rating display
  const StarRating = ({ rating }) => {
    if (!rating) return <span className="text-gray-400 text-sm">Not rated yet</span>;
    return (
      <div className="flex items-center gap-1">
        {[1, 2, 3, 4, 5].map(i => (
          <Star
            key={i}
            className={`w-4 h-4 ${i <= rating ? 'text-yellow-500 fill-yellow-500' : 'text-gray-300'}`}
          />
        ))}
        <span className="text-sm text-gray-600 ml-1">{rating}/5</span>
      </div>
    );
  };

  if (paymentComplete) {
    return (
      <div className="bg-green-50 border border-green-200 rounded-lg p-6 mb-4">
        <div className="flex items-center gap-3">
          <CheckCircle2 className="w-8 h-8 text-green-600" />
          <div>
            <h3 className="font-semibold text-green-900 text-lg">Payment Approved Successfully</h3>
            <p className="text-sm text-green-700 mt-1">
              Final payment of ₹{finalAmount.toFixed(2)} has been processed. Thank you!
            </p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg shadow-sm border border-blue-200 overflow-hidden mb-4">
      {/* Header */}
      <div className="bg-blue-600 text-white px-6 py-3 flex items-center gap-2">
        <FileText className="w-5 h-5" />
        <span className="font-semibold">Event Completed — Final Payment</span>
      </div>

      <div className="p-6 space-y-5">
        {/* Supervisor Quality Rating */}
        {liveEventStatus.serviceQualityRating && (
          <div className="flex items-center justify-between">
            <span className="text-gray-700 font-medium">Service Quality Rating</span>
            <StarRating rating={liveEventStatus.serviceQualityRating} />
          </div>
        )}

        {/* Supervisor Notes */}
        {liveEventStatus.supervisorNotes && (
          <div className="bg-gray-50 rounded-lg p-4">
            <h4 className="text-sm font-medium text-gray-700 mb-2">Supervisor Notes</h4>
            <p className="text-sm text-gray-600">{liveEventStatus.supervisorNotes}</p>
          </div>
        )}

        {/* Actual Guest Count */}
        {liveEventStatus.actualGuestCount && (
          <div className="flex items-center justify-between text-sm">
            <span className="text-gray-600">Actual Guest Count</span>
            <span className="font-medium">
              {liveEventStatus.actualGuestCount}
              {liveEventStatus.actualGuestCount !== order.guestCount && (
                <span className="text-amber-600 ml-1">(originally {order.guestCount})</span>
              )}
            </span>
          </div>
        )}

        {/* Payment Breakdown */}
        <div className="border-t pt-4 space-y-2">
          <h4 className="font-semibold text-gray-900 mb-3">Final Payment Breakdown</h4>

          <div className="flex justify-between text-sm">
            <span className="text-gray-600">Post-Completion Amount (30%)</span>
            <span>₹{postEventAmount.toFixed(2)}</span>
          </div>

          {extraCharges > 0 && (
            <div className="flex justify-between text-sm text-amber-700">
              <span>Extra Charges (additional quantities)</span>
              <span>+₹{extraCharges.toFixed(2)}</span>
            </div>
          )}

          <div className="flex justify-between font-bold text-lg border-t pt-2 mt-2">
            <span>Amount Due</span>
            <span className="text-blue-600 flex items-center gap-1">
              <IndianRupee className="w-4 h-4" />
              {finalAmount.toFixed(2)}
            </span>
          </div>
        </div>

        {/* Approve & Pay CTA */}
        {canPay ? (
          <button
            onClick={handleApproveAndPay}
            disabled={isProcessing}
            className="w-full py-3 bg-blue-600 text-white font-semibold rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
          >
            {isProcessing ? (
              <>
                <Loader2 className="w-5 h-5 animate-spin" />
                Processing Payment...
              </>
            ) : (
              <>
                <CheckCircle2 className="w-5 h-5" />
                Approve & Pay ₹{finalAmount.toFixed(2)}
              </>
            )}
          </button>
        ) : (
          <div className="flex items-start gap-3 bg-amber-50 border border-amber-200 rounded-lg p-4">
            <AlertCircle className="w-5 h-5 text-amber-600 flex-shrink-0 mt-0.5" />
            <div>
              <h4 className="font-medium text-amber-900">Awaiting Verification</h4>
              <p className="text-sm text-amber-700 mt-1">
                The supervisor's completion report is being reviewed. Payment will be unlocked once verification is complete.
              </p>
            </div>
          </div>
        )}

        {/* Platform Protection */}
        <p className="text-xs text-gray-500 text-center">
          Payments are held in escrow until verified. Platform protected.
        </p>
      </div>

      {/* OTP Verification Modal - Required before final payment approval */}
      <OTPVerificationModal
        isOpen={showOtpModal}
        onClose={() => setShowOtpModal(false)}
        onVerify={handleOtpVerified}
        purpose="Approve Final Payment"
        phoneNumber={user?.phone || order.contactPhone}
        actionDescription={`Approve and release final payment of ₹${finalAmount.toFixed(2)} to the catering partner`}
        requireOtp={true}
        autoSendOtp={true}
      />
    </div>
  );
};

export default PostEventPaymentSection;
