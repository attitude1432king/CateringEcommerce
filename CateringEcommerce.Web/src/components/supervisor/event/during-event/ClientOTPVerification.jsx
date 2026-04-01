/**
 * ClientOTPVerification Component
 * Verify client OTP for extra quantity approval
 */

import { useState } from 'react';
import PropTypes from 'prop-types';
import { ShieldCheck, RefreshCw, CheckCircle2, XCircle, Clock } from 'lucide-react';
import { eventSupervisionApi } from '../../../../services/api/supervisor';
import OTPInput from '../../common/forms/OTPInput';
import toast from 'react-hot-toast';

const ClientOTPVerification = ({ assignmentId, purpose = 'EXTRA_QUANTITY', onVerified }) => {
  const [otp, setOtp] = useState('');
  const [verifying, setVerifying] = useState(false);
  const [resending, setResending] = useState(false);
  const [status, setStatus] = useState('PENDING'); // PENDING, VERIFIED, FAILED
  const [attempts, setAttempts] = useState(0);

  const handleVerifyOTP = async (otpValue) => {
    const code = otpValue || otp;
    if (code.length !== 6) {
      toast.error('Please enter all 6 digits');
      return;
    }

    setVerifying(true);
    try {
      const response = await eventSupervisionApi.verifyClientOTP({
        assignmentId,
        otp: code,
        purpose,
        verifiedAt: new Date().toISOString(),
      });

      if (response.success && response.data?.data?.verified) {
        setStatus('VERIFIED');
        toast.success('OTP verified successfully');
        onVerified?.(response.data.data);
      } else {
        setAttempts((prev) => prev + 1);
        setStatus('FAILED');
        toast.error(response.data?.data?.message || 'Invalid OTP');
        setOtp('');
      }
    } catch {
      toast.error('Verification failed');
    } finally {
      setVerifying(false);
    }
  };

  const handleResendOTP = async () => {
    setResending(true);
    try {
      const response = await eventSupervisionApi.resendClientOTP(assignmentId, purpose);
      if (response.success) {
        toast.success('OTP resent to client');
        setStatus('PENDING');
        setOtp('');
      } else {
        toast.error(response.message);
      }
    } catch {
      toast.error('Failed to resend OTP');
    } finally {
      setResending(false);
    }
  };

  return (
    <div className="bg-white rounded-lg shadow-md p-6">
      <div className="flex items-center gap-3 mb-4">
        <ShieldCheck className="w-6 h-6 text-indigo-600" />
        <h2 className="text-xl font-semibold text-gray-900">Client OTP Verification</h2>
      </div>

      {status === 'VERIFIED' ? (
        <div className="text-center py-8">
          <CheckCircle2 className="w-16 h-16 text-green-500 mx-auto mb-4" />
          <h3 className="text-lg font-semibold text-green-800">OTP Verified</h3>
          <p className="text-sm text-gray-600 mt-1">Client has approved the request.</p>
        </div>
      ) : (
        <>
          <div className="text-center mb-6">
            <p className="text-sm text-gray-600">
              Ask the client to share the 6-digit OTP sent to their registered phone number.
            </p>
            {purpose === 'EXTRA_QUANTITY' && (
              <p className="text-xs text-gray-500 mt-1">
                This OTP is required to approve the extra quantity request.
              </p>
            )}
          </div>

          {/* OTP Input */}
          <div className="mb-6">
            <OTPInput
              length={6}
              value={otp}
              onChange={setOtp}
              onComplete={handleVerifyOTP}
              disabled={verifying}
              error={status === 'FAILED'}
            />
          </div>

          {/* Failed Attempt Warning */}
          {status === 'FAILED' && (
            <div className="bg-red-50 border border-red-200 rounded-lg p-3 mb-4">
              <div className="flex items-center gap-2">
                <XCircle className="w-4 h-4 text-red-600" />
                <p className="text-sm text-red-800">
                  Invalid OTP. {attempts >= 3 ? 'Please resend a new OTP.' : `Attempt ${attempts}/3`}
                </p>
              </div>
            </div>
          )}

          {/* Verify Button */}
          <button
            onClick={() => handleVerifyOTP()}
            disabled={verifying || otp.length !== 6}
            className="w-full px-4 py-3 bg-indigo-600 text-white rounded-lg font-medium hover:bg-indigo-700 disabled:opacity-50"
          >
            {verifying ? (
              <span className="flex items-center justify-center gap-2">
                <Clock className="w-4 h-4 animate-spin" /> Verifying...
              </span>
            ) : (
              'Verify OTP'
            )}
          </button>

          {/* Resend OTP */}
          <div className="text-center mt-4">
            <button
              onClick={handleResendOTP}
              disabled={resending}
              className="text-sm text-indigo-600 hover:text-indigo-700 font-medium flex items-center gap-1 mx-auto"
            >
              <RefreshCw className={`w-4 h-4 ${resending ? 'animate-spin' : ''}`} />
              {resending ? 'Resending...' : 'Resend OTP to Client'}
            </button>
          </div>
        </>
      )}
    </div>
  );
};

ClientOTPVerification.propTypes = {
  assignmentId: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired,
  purpose: PropTypes.string,
  onVerified: PropTypes.func,
};

export default ClientOTPVerification;
