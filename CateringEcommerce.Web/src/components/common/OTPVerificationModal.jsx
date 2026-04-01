import React, { useState, useEffect } from 'react';
import { apiService } from '../../services/userApi';
import { getOrGenerateFingerprint, getDeviceInfo } from '../../utils/deviceFingerprint';
import { X, Shield, AlertCircle, CheckCircle } from 'lucide-react';

/**
 * OTPVerificationModal - Reusable modal for sensitive action OTP verification
 *
 * Used for:
 * - Order placement
 * - Payment processing
 * - Final event payment approval
 * - Bank details update
 * - Fund withdrawal
 */
const OTPVerificationModal = ({
    isOpen,
    onClose,
    onVerify,
    purpose = 'Verify Action', // e.g., "Place Order", "Make Payment", "Approve Final Payment"
    phoneNumber,
    actionDescription = 'Complete this action', // Detailed description
    requireOtp = true, // Can be controlled by backend
    autoSendOtp = true // Automatically send OTP when modal opens
}) => {
    const [otp, setOtp] = useState('');
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState('');
    const [successMessage, setSuccessMessage] = useState('');
    const [otpSent, setOtpSent] = useState(false);
    const [deviceFingerprint, setDeviceFingerprint] = useState(null);
    const [resendTimer, setResendTimer] = useState(0);
    const [attempts, setAttempts] = useState(0);
    const [isVerified, setIsVerified] = useState(false);

    // Validate OTP (exactly 6 digits)
    const isOtpValid = /^[0-9]{6}$/.test(otp);

    // Initialize device fingerprint
    useEffect(() => {
        const initFingerprint = async () => {
            try {
                const fingerprint = await getOrGenerateFingerprint();
                setDeviceFingerprint(fingerprint);
            } catch (error) {
                console.error('Failed to get device fingerprint:', error);
            }
        };

        if (isOpen) {
            initFingerprint();
        }
    }, [isOpen]);

    // Auto-send OTP when modal opens
    useEffect(() => {
        if (isOpen && autoSendOtp && phoneNumber && !otpSent && requireOtp) {
            handleSendOtp();
        }
    }, [isOpen, autoSendOtp, phoneNumber, otpSent, requireOtp]);

    // Resend timer countdown
    useEffect(() => {
        if (resendTimer > 0) {
            const timer = setTimeout(() => setResendTimer(resendTimer - 1), 1000);
            return () => clearTimeout(timer);
        }
    }, [resendTimer]);

    // Reset state when modal closes
    useEffect(() => {
        if (!isOpen) {
            resetState();
        }
    }, [isOpen]);

    const resetState = () => {
        setOtp('');
        setError('');
        setSuccessMessage('');
        setOtpSent(false);
        setResendTimer(0);
        setAttempts(0);
        setIsVerified(false);
        setIsLoading(false);
    };

    const handleSendOtp = async () => {
        setError('');
        setIsLoading(true);
        try {
            // Send OTP for sensitive action verification
            const response = await apiService.sendOtp('login', phoneNumber, false, deviceFingerprint);

            if (!response.result) {
                setError(response.message || 'Failed to send OTP');
                setIsLoading(false);
                return;
            }

            setOtpSent(true);
            setResendTimer(60); // 60 seconds cooldown
            setSuccessMessage('OTP sent to your phone');

            // Clear success message after 3 seconds
            setTimeout(() => setSuccessMessage(''), 3000);

        } catch (err) {
            setError(err.message || 'Failed to send OTP. Please try again.');
        } finally {
            setIsLoading(false);
        }
    };

    const handleVerifyOtp = async (e) => {
        e.preventDefault();
        setError('');
        setIsLoading(true);
        setAttempts(attempts + 1);

        try {
            // Prepare device info
            const deviceInfoPayload = {
                deviceFingerprint,
                trustDevice: false, // Don't trust device for sensitive actions
                browser: getDeviceInfo().browser,
                os: getDeviceInfo().os
            };

            // Verify OTP
            const response = await apiService.verifyOtp(
                'login', // Using login action for verification
                phoneNumber,
                '', // No name needed for verification
                otp,
                false, // Not partner login
                deviceInfoPayload
            );

            if (!response.result) {
                setError(response.message || 'Invalid OTP. Please try again.');
                setIsLoading(false);

                // Lock after 5 failed attempts
                if (attempts >= 4) {
                    setError('Too many failed attempts. Please try again later.');
                    setTimeout(() => {
                        handleClose();
                    }, 2000);
                }
                return;
            }

            // OTP verified successfully
            setIsVerified(true);
            setSuccessMessage('OTP verified! Proceeding...');
            setIsLoading(false);

            // Wait a moment to show success, then call onVerify
            setTimeout(() => {
                onVerify(otp, response.token);
                handleClose();
            }, 1000);

        } catch (err) {
            setError(err.message || 'Verification failed. Please try again.');
            setIsLoading(false);
        }
    };

    const handleOtpChange = (e) => {
        const value = e.target.value.replace(/\D/g, ''); // Remove non-digits
        if (value.length <= 6) {
            setOtp(value);
        }
    };

    const handleClose = () => {
        resetState();
        onClose();
    };

    if (!isOpen) return null;

    // If OTP is not required (user is already on trusted device), skip OTP and call onVerify immediately
    if (!requireOtp) {
        setTimeout(() => {
            onVerify(null, null);
            handleClose();
        }, 100);
        return null;
    }

    return (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
            <div className="bg-white rounded-lg shadow-2xl max-w-md w-full animate-fadeIn">
                {/* Header */}
                <div className="flex items-center justify-between p-6 border-b border-gray-200">
                    <div className="flex items-center gap-3">
                        <div className="w-10 h-10 bg-rose-100 rounded-full flex items-center justify-center">
                            <Shield className="w-5 h-5 text-rose-600" />
                        </div>
                        <div>
                            <h3 className="text-xl font-bold text-gray-800">{purpose}</h3>
                            <p className="text-xs text-gray-500">Verification required for security</p>
                        </div>
                    </div>
                    <button
                        onClick={handleClose}
                        className="text-gray-400 hover:text-gray-600 transition-colors"
                        disabled={isLoading}
                    >
                        <X className="w-6 h-6" />
                    </button>
                </div>

                {/* Body */}
                <div className="p-6 space-y-4">
                    {/* Description */}
                    <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                        <div className="flex items-start gap-3">
                            <AlertCircle className="w-5 h-5 text-blue-600 flex-shrink-0 mt-0.5" />
                            <div className="text-sm text-blue-900">
                                <p className="font-medium mb-1">Security Verification</p>
                                <p>{actionDescription}</p>
                                <p className="mt-2 text-xs text-blue-700">
                                    We've sent a 6-digit code to <strong>+91 {phoneNumber}</strong>
                                </p>
                            </div>
                        </div>
                    </div>

                    {/* Success Message */}
                    {successMessage && (
                        <div className="bg-green-50 border border-green-200 text-green-800 px-4 py-3 rounded-lg flex items-center gap-2 animate-fadeIn">
                            <CheckCircle className="w-5 h-5 flex-shrink-0" />
                            <span className="text-sm font-medium">{successMessage}</span>
                        </div>
                    )}

                    {/* Error Message */}
                    {error && (
                        <div className="bg-red-50 border border-red-200 text-red-800 px-4 py-3 rounded-lg flex items-center gap-2 animate-fadeIn">
                            <AlertCircle className="w-5 h-5 flex-shrink-0" />
                            <span className="text-sm font-medium">{error}</span>
                        </div>
                    )}

                    {/* OTP Form */}
                    <form onSubmit={handleVerifyOtp} className="space-y-4">
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">
                                Enter OTP
                            </label>
                            <input
                                type="text"
                                placeholder="000000"
                                value={otp}
                                onChange={handleOtpChange}
                                className="w-full px-4 py-3 border-2 border-gray-300 rounded-lg focus:outline-none focus:border-rose-500 focus:ring-2 focus:ring-rose-200 text-center text-2xl font-semibold tracking-[0.5em] transition-all"
                                maxLength="6"
                                required
                                disabled={isLoading || isVerified}
                                autoComplete="one-time-code"
                                autoFocus
                            />
                            {otp.length > 0 && !isOtpValid && otp.length < 6 && (
                                <p className="mt-2 text-xs text-gray-500 text-center">
                                    {6 - otp.length} more digit{6 - otp.length !== 1 ? 's' : ''} required
                                </p>
                            )}
                        </div>

                        {/* Verify Button */}
                        <button
                            type="submit"
                            disabled={!isOtpValid || isLoading || isVerified}
                            className="w-full bg-gradient-to-r from-rose-500 to-rose-600 text-white py-3 px-4 rounded-lg hover:from-rose-600 hover:to-rose-700 transition-all duration-200 font-medium shadow-lg hover:shadow-xl disabled:from-gray-300 disabled:to-gray-400 disabled:cursor-not-allowed disabled:shadow-none transform hover:scale-[1.02] active:scale-[0.98]"
                        >
                            {isLoading ? (
                                <span className="flex items-center justify-center">
                                    <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                                        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                                        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                                    </svg>
                                    Verifying...
                                </span>
                            ) : isVerified ? (
                                <span className="flex items-center justify-center">
                                    <CheckCircle className="w-5 h-5 mr-2" />
                                    Verified
                                </span>
                            ) : (
                                'Verify & Continue'
                            )}
                        </button>
                    </form>

                    {/* Resend OTP */}
                    <div className="text-center">
                        {resendTimer > 0 ? (
                            <p className="text-sm text-gray-500">
                                Resend OTP in <span className="font-semibold text-gray-700">{resendTimer}s</span>
                            </p>
                        ) : (
                            <button
                                type="button"
                                onClick={handleSendOtp}
                                disabled={isLoading || isVerified}
                                className="text-sm text-rose-600 hover:text-rose-700 font-medium hover:underline disabled:text-gray-400 disabled:cursor-not-allowed"
                            >
                                Resend OTP
                            </button>
                        )}
                    </div>

                    {/* Attempts Warning */}
                    {attempts >= 3 && attempts < 5 && (
                        <div className="bg-yellow-50 border border-yellow-200 text-yellow-800 px-4 py-2 rounded-lg text-xs text-center">
                            ⚠️ {5 - attempts} attempt{5 - attempts !== 1 ? 's' : ''} remaining
                        </div>
                    )}
                </div>

                {/* Footer */}
                <div className="px-6 py-4 bg-gray-50 border-t border-gray-200 rounded-b-lg">
                    <p className="text-xs text-gray-600 text-center">
                        This verification helps protect your account from unauthorized actions
                    </p>
                </div>
            </div>
        </div>
    );
};

export default OTPVerificationModal;
