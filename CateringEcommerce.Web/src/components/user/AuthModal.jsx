import { useNavigate, Link } from 'react-router-dom';
import React, { useState, useEffect } from 'react';
import { apiService } from '../../services/userApi';
import { initiateOAuthLogin } from '../../services/oauthApi';
import { useAuth } from '../../contexts/AuthContext';
import { getOrGenerateFingerprint, getDeviceInfo } from '../../utils/deviceFingerprint';

// Error Banner Component
const ErrorBanner = ({ message }) => {
    if (!message) return null;
    return (
        <div className="bg-red-50 border-l-4 border-red-500 text-red-700 p-3 rounded-md text-sm flex items-start mb-4 animate-fadeIn">
            <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5 mr-2 flex-shrink-0 mt-0.5" viewBox="0 0 20 20" fill="currentColor">
                <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
            </svg>
            <span className="flex-1">{message}</span>
        </div>
    );
};

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export default function AuthModal({ isOpen, onClose, isPartnerLogin = false }) {
    const [view, setView] = useState('login'); // 'login', 'signup', 'otp'
    const navigate = useNavigate();
    const { login } = useAuth();

    // Data States
    const [phone, setPhone] = useState('');
    const [otp, setOtp] = useState('');
    const [fullName, setFullName] = useState('');
    const [authAction, setAuthAction] = useState('login');

    // UI States
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState('');
    const [successMessage, setSuccessMessage] = useState('');

    // Validation States
    const [isPhoneValid, setIsPhoneValid] = useState(false);
    const [phoneError, setPhoneError] = useState('');
    const [isOtpValid, setIsOtpValid] = useState(false);

    // Device Tracking & 2FA States
    const [deviceFingerprint, setDeviceFingerprint] = useState(null);
    const [deviceInfo, setDeviceInfo] = useState(null);
    const [trustDevice, setTrustDevice] = useState(false);
    const [otpPurpose, setOtpPurpose] = useState(null); // OTP purpose message from backend

    // Generate device fingerprint when modal opens
    useEffect(() => {
        const initDeviceFingerprint = async () => {
            try {
                const fingerprint = await getOrGenerateFingerprint();
                const info = getDeviceInfo();
                setDeviceFingerprint(fingerprint);
                setDeviceInfo(info);
            } catch (error) {
                console.error('Failed to generate device fingerprint:', error);
                // Continue without fingerprint - backend will handle gracefully
            }
        };

        if (isOpen) {
            initDeviceFingerprint();
        }
    }, [isOpen]);

    // Validate phone number (exactly 10 digits)
    useEffect(() => {
        const phoneRegex = /^[0-9]{10}$/;
        if (phone.length === 0) {
            setIsPhoneValid(false);
            setPhoneError('');
        } else if (!phoneRegex.test(phone)) {
            setIsPhoneValid(false);
            if (phone.length > 0 && phone.length < 10) {
                setPhoneError('Phone number must be 10 digits');
            } else if (phone.length > 10) {
                setPhoneError('Phone number cannot exceed 10 digits');
            } else {
                setPhoneError('Please enter only numbers');
            }
        } else {
            setIsPhoneValid(true);
            setPhoneError('');
        }
    }, [phone]);

    // Validate OTP (exactly 6 digits)
    useEffect(() => {
        const otpRegex = /^[0-9]{6}$/;
        setIsOtpValid(otpRegex.test(otp));
    }, [otp]);

    if (!isOpen) return null;

    const resetState = () => {
        setPhone('');
        setOtp('');
        setFullName('');
        setIsLoading(false);
        setError('');
        setSuccessMessage('');
        setAuthAction('login');
        setPhoneError('');
        setIsPhoneValid(false);
        setIsOtpValid(false);
        setTrustDevice(false);
        setOtpPurpose(null);
    };

    const handleClose = () => {
        resetState();
        setView('login');
        onClose();
    };

    // Handle phone input - only allow numbers and max 10 digits
    const handlePhoneChange = (e) => {
        const value = e.target.value.replace(/\D/g, ''); // Remove non-digits
        if (value.length <= 10) {
            setPhone(value);
        }
    };

    // Handle OTP input - only allow numbers and max 6 digits
    const handleOtpChange = (e) => {
        const value = e.target.value.replace(/\D/g, ''); // Remove non-digits
        if (value.length <= 6) {
            setOtp(value);
        }
    };

    const handleSendOtp = async (currentAction) => {
        setError('');
        setIsLoading(true);
        try {
            const response = await apiService.sendOtp(currentAction, phone, isPartnerLogin, deviceFingerprint);
            if (!response.result) {
                setError(response.message);
                setIsLoading(false);
                return;
            }

            setAuthAction(currentAction);

            // Capture OTP purpose from backend response
            if (response.data && response.data.purpose) {
                setOtpPurpose(response.data.purpose);
            } else {
                // Fallback purpose
                setOtpPurpose({
                    userMessage: currentAction === 'signup' ? 'Verify your account' : 'Verify to continue',
                    description: currentAction === 'signup'
                        ? "We've sent a verification code to your phone number"
                        : "Enter the verification code sent to your phone"
                });
            }

            setIsLoading(false);
            setView('otp');
        } catch (err) {
            setError(err.message || 'Failed to send OTP. Please try again.');
            setIsLoading(false);
        }
    };

    const handleVerifyOtp = async (e) => {
        e.preventDefault();
        setError('');
        setIsLoading(true);
        setSuccessMessage('');
        try {
            const nameToSend = authAction === 'signup' ? fullName : '';

            // Prepare device info for backend
            const deviceInfoPayload = {
                deviceFingerprint,
                trustDevice: trustDevice && !isPartnerLogin, // Only allow device trust for users, not partners
                browser: deviceInfo?.browser || null,
                os: deviceInfo?.os || null
            };

            const response = await apiService.verifyOtp(
                authAction,
                phone,
                nameToSend,
                otp,
                isPartnerLogin,
                deviceInfoPayload
            );

            if (!response.result) {
                setError(response.message || 'OTP verification failed');
                throw new Error(response.message || 'OTP verification failed');
            }

            const { token, user, role, deviceTrusted } = response;

            setIsLoading(false);
            await apiService.finalVerify(user.pkID, role, token);

            // Show success message with device trust info
            let welcomeMsg = `Welcome, ${user.fullName ?? user.cateringName}!`;
            if (deviceTrusted && !isPartnerLogin) {
                welcomeMsg += ' This device is now trusted for 30 days.';
            }
            setSuccessMessage(welcomeMsg);

            setTimeout(() => {
                login({
                    pkid: user.pkID,
                    name: user.fullName ?? user.cateringName,
                    role: role,
                    token: token,
                    profilePhoto: (user.profilePhoto ? `${API_BASE_URL}${user.profilePhoto}` : undefined)
                });
                handleClose();

                // Check for stored redirect path from auth guard
                const authRedirect = localStorage.getItem('auth_redirect');
                if (authRedirect) {
                    localStorage.removeItem('auth_redirect');
                    navigate(authRedirect);
                } else if (role === 'Owner') {
                    navigate('/owner/dashboard/');
                }
            }, 1500);

        } catch (err) {
            setError(err.message || 'Verification failed. Please try again.');
            setIsLoading(false);
        }
    };

    const handleGoogleLogin = async () => {
        setError('');
        setIsLoading(true);
        try {
            // Store provider and redirect path for post-auth navigation
            localStorage.setItem('oauth_provider', 'google');

            // SECURITY FIX: Validate redirect URL to prevent open redirect attacks
            const authRedirect = localStorage.getItem('auth_redirect');
            const currentPath = window.location.pathname;
            let redirectPath = authRedirect || currentPath;

            // Import security utility dynamically
            const { sanitizeRedirectUrl } = await import('../../utils/securityUtils');
            redirectPath = sanitizeRedirectUrl(redirectPath, '/');

            localStorage.setItem('oauth_redirect', redirectPath);

            // Get authorization URL from backend
            const response = await initiateOAuthLogin('google');

            if (response.success && response.data && response.data.authorizationUrl) {
                // Redirect to Google OAuth
                window.location.href = response.data.authorizationUrl;
            } else {
                throw new Error(response.message || 'Failed to initiate Google login');
            }
        } catch (error) {
            console.error('Google login error:', error);
            setError(error.message || 'Google login is currently unavailable. Please try phone login.');
            setIsLoading(false);
        }
    };

    const renderLoginView = () => (
        <div className="space-y-4">
            <div className="text-center mb-6">
                <h3 className="text-2xl font-bold text-gray-800 mb-2">
                    {isPartnerLogin ? 'Partner Login' : 'Welcome Back'}
                </h3>
                <p className="text-gray-600 text-sm">
                    {isPartnerLogin
                        ? 'Sign in to manage your catering business'
                        : 'Sign in to order delicious food'}
                </p>
            </div>

            <ErrorBanner message={error} />

            <form onSubmit={(e) => { e.preventDefault(); handleSendOtp('login'); }} className="space-y-4">
                <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                        Phone Number
                    </label>
                    <div className="relative">
                        <div className="flex items-center border-2 border-gray-300 rounded-lg overflow-hidden transition-all focus-within:border-rose-500 focus-within:ring-2 focus-within:ring-rose-200">
                            <span className="px-4 py-3 bg-gray-50 text-gray-700 font-medium text-sm border-r-2 border-gray-300">
                                +91
                            </span>
                            <input
                                type="tel"
                                placeholder="Enter 10 digit mobile number"
                                value={phone}
                                onChange={handlePhoneChange}
                                className="w-full px-4 py-3 focus:outline-none text-sm"
                                maxLength="10"
                                required
                            />
                        </div>
                        {phoneError && (
                            <p className="mt-1 text-xs text-red-600 flex items-center">
                                <svg className="w-4 h-4 mr-1" fill="currentColor" viewBox="0 0 20 20">
                                    <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                                </svg>
                                {phoneError}
                            </p>
                        )}
                        {isPhoneValid && (
                            <p className="mt-1 text-xs text-green-600 flex items-center">
                                <svg className="w-4 h-4 mr-1" fill="currentColor" viewBox="0 0 20 20">
                                    <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                                </svg>
                                Valid phone number
                            </p>
                        )}
                    </div>
                </div>

                <button
                    type="submit"
                    disabled={!isPhoneValid || isLoading}
                    className="w-full bg-gradient-to-r from-rose-500 to-rose-600 text-white py-3 px-4 rounded-lg hover:from-rose-600 hover:to-rose-700 transition-all duration-200 font-medium shadow-lg hover:shadow-xl disabled:from-gray-300 disabled:to-gray-400 disabled:cursor-not-allowed disabled:shadow-none transform hover:scale-[1.02] active:scale-[0.98]"
                >
                    {isLoading ? (
                        <span className="flex items-center justify-center">
                            <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                            </svg>
                            Sending OTP...
                        </span>
                    ) : (
                        'Send One Time Password'
                    )}
                </button>
            </form>

            <div className="relative my-6">
                <div className="absolute inset-0 flex items-center">
                    <div className="w-full border-t border-gray-300"></div>
                </div>
                <div className="relative flex justify-center text-sm">
                    <span className="px-4 bg-white text-gray-500 font-medium">or continue with</span>
                </div>
            </div>

            <div className="space-y-3">
                <button
                    onClick={handleGoogleLogin}
                    disabled={isLoading}
                    className="w-full border-2 border-gray-300 text-gray-700 py-3 px-4 rounded-lg hover:bg-gray-50 hover:border-gray-400 transition-all duration-200 font-medium flex items-center justify-center space-x-3 disabled:bg-gray-100 disabled:cursor-not-allowed"
                >
                    <svg className="w-5 h-5" viewBox="0 0 24 24">
                        <path fill="#4285F4" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"/>
                        <path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"/>
                        <path fill="#FBBC05" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"/>
                        <path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"/>
                    </svg>
                    <span>Sign in with Google</span>
                </button>
            </div>

            <p className="mt-6 text-center text-sm text-gray-600">
                {isPartnerLogin ? (
                    <>
                        New partner?{' '}
                        <Link to="/partner-registration" onClick={handleClose} className="text-rose-600 hover:text-rose-700 font-semibold hover:underline">
                            Register your business
                        </Link>
                    </>
                ) : (
                    <>
                        Don't have an account?{' '}
                        <button type="button" onClick={() => setView('signup')} className="text-rose-600 hover:text-rose-700 font-semibold hover:underline">
                            Sign up
                        </button>
                    </>
                )}
            </p>
        </div>
    );

    const renderOtpView = () => (
        <div className="space-y-4">
            <div className="text-center mb-6">
                <div className="mx-auto w-16 h-16 bg-rose-100 rounded-full flex items-center justify-center mb-4">
                    <svg className="w-8 h-8 text-rose-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
                    </svg>
                </div>
                <h3 className="text-2xl font-bold text-gray-800 mb-2">
                    {otpPurpose?.userMessage || 'Verify Your Number'}
                </h3>
                <p className="text-gray-600 text-sm mb-2">
                    {otpPurpose?.description || "We've sent a 6-digit code to"}<br />
                    <span className="font-semibold text-gray-800">+91 {phone}</span>
                </p>
                {/* Show security badge for 2FA */}
                {authAction === 'login' && !isPartnerLogin && (
                    <div className="inline-flex items-center gap-1 px-3 py-1 bg-blue-50 text-blue-700 rounded-full text-xs font-medium mt-2">
                        <svg className="w-3 h-3" fill="currentColor" viewBox="0 0 20 20">
                            <path fillRule="evenodd" d="M5 9V7a5 5 0 0110 0v2a2 2 0 012 2v5a2 2 0 01-2 2H5a2 2 0 01-2-2v-5a2 2 0 012-2zm8-2v2H7V7a3 3 0 016 0z" clipRule="evenodd" />
                        </svg>
                        Two-Factor Authentication
                    </div>
                )}
            </div>

            <ErrorBanner message={error} />

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
                        disabled={!!successMessage}
                        autoComplete="one-time-code"
                    />
                    {otp.length > 0 && !isOtpValid && otp.length < 6 && (
                        <p className="mt-2 text-xs text-gray-500 text-center">
                            {6 - otp.length} more digit{6 - otp.length !== 1 ? 's' : ''} required
                        </p>
                    )}
                </div>

                {/* Trust Device Checkbox - Only for Users during Login, NOT for Partners/Signup */}
                {authAction === 'login' && !isPartnerLogin && (
                    <div className="bg-green-50 border border-green-200 rounded-lg p-3">
                        <label className="flex items-start gap-3 cursor-pointer group">
                            <input
                                type="checkbox"
                                checked={trustDevice}
                                onChange={(e) => setTrustDevice(e.target.checked)}
                                className="mt-0.5 w-4 h-4 text-rose-600 border-gray-300 rounded focus:ring-rose-500 cursor-pointer"
                            />
                            <div className="flex-1">
                                <span className="text-sm font-medium text-gray-800 block">
                                    Trust this device for 30 days
                                </span>
                                <span className="text-xs text-gray-600 block mt-0.5">
                                    Skip OTP verification on this device for the next 30 days
                                </span>
                            </div>
                        </label>
                    </div>
                )}

                <button
                    type="submit"
                    disabled={!isOtpValid || isLoading || !!successMessage}
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
                    ) : (
                        'Verify & Continue'
                    )}
                </button>
            </form>

            <div className="text-center space-y-3">
                <button
                    type="button"
                    onClick={() => handleSendOtp(authAction)}
                    disabled={isLoading}
                    className="text-sm text-rose-600 hover:text-rose-700 font-medium hover:underline disabled:text-gray-400"
                >
                    Resend OTP
                </button>
                <p className="text-sm text-gray-600">
                    Wrong number?{' '}
                    <button
                        type="button"
                        onClick={() => {
                            setView(authAction === 'signup' ? 'signup' : 'login');
                            setOtp('');
                            setError('');
                        }}
                        className="text-rose-600 hover:text-rose-700 font-semibold hover:underline"
                    >
                        Change number
                    </button>
                </p>
            </div>
        </div>
    );

    const renderSignupView = () => (
        <div className="space-y-4">
            <div className="text-center mb-6">
                <h3 className="text-2xl font-bold text-gray-800 mb-2">
                    {isPartnerLogin ? 'Partner Registration' : 'Create Account'}
                </h3>
                <p className="text-gray-600 text-sm">
                    {isPartnerLogin
                        ? 'Start your catering business journey'
                        : 'Join us for amazing food experiences'}
                </p>
            </div>

            <ErrorBanner message={error} />

            <form onSubmit={(e) => { e.preventDefault(); handleSendOtp('signup'); }} className="space-y-4">
                <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                        Full Name
                    </label>
                    <input
                        type="text"
                        placeholder="Enter your full name"
                        value={fullName}
                        onChange={(e) => setFullName(e.target.value)}
                        className="w-full px-4 py-3 border-2 border-gray-300 rounded-lg focus:outline-none focus:border-rose-500 focus:ring-2 focus:ring-rose-200 text-sm transition-all"
                        required
                    />
                </div>

                <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                        Phone Number
                    </label>
                    <div className="relative">
                        <div className="flex items-center border-2 border-gray-300 rounded-lg overflow-hidden transition-all focus-within:border-rose-500 focus-within:ring-2 focus-within:ring-rose-200">
                            <span className="px-4 py-3 bg-gray-50 text-gray-700 font-medium text-sm border-r-2 border-gray-300">
                                +91
                            </span>
                            <input
                                type="tel"
                                placeholder="Enter 10 digit mobile number"
                                value={phone}
                                onChange={handlePhoneChange}
                                className="w-full px-4 py-3 focus:outline-none text-sm"
                                maxLength="10"
                                required
                            />
                        </div>
                        {phoneError && (
                            <p className="mt-1 text-xs text-red-600 flex items-center">
                                <svg className="w-4 h-4 mr-1" fill="currentColor" viewBox="0 0 20 20">
                                    <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                                </svg>
                                {phoneError}
                            </p>
                        )}
                        {isPhoneValid && (
                            <p className="mt-1 text-xs text-green-600 flex items-center">
                                <svg className="w-4 h-4 mr-1" fill="currentColor" viewBox="0 0 20 20">
                                    <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                                </svg>
                                Valid phone number
                            </p>
                        )}
                    </div>
                </div>

                <button
                    type="submit"
                    disabled={!isPhoneValid || !fullName.trim() || isLoading}
                    className="w-full bg-gradient-to-r from-rose-500 to-rose-600 text-white py-3 px-4 rounded-lg hover:from-rose-600 hover:to-rose-700 transition-all duration-200 font-medium shadow-lg hover:shadow-xl disabled:from-gray-300 disabled:to-gray-400 disabled:cursor-not-allowed disabled:shadow-none transform hover:scale-[1.02] active:scale-[0.98]"
                >
                    {isLoading ? (
                        <span className="flex items-center justify-center">
                            <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                            </svg>
                            Creating Account...
                        </span>
                    ) : (
                        'Create Account & Send OTP'
                    )}
                </button>
            </form>

            <div className="relative my-6">
                <div className="absolute inset-0 flex items-center">
                    <div className="w-full border-t border-gray-300"></div>
                </div>
                <div className="relative flex justify-center text-sm">
                    <span className="px-4 bg-white text-gray-500 font-medium">or continue with</span>
                </div>
            </div>

            <div className="space-y-3">
                <button
                    onClick={handleGoogleLogin}
                    disabled={isLoading}
                    className="w-full border-2 border-gray-300 text-gray-700 py-3 px-4 rounded-lg hover:bg-gray-50 hover:border-gray-400 transition-all duration-200 font-medium flex items-center justify-center space-x-3 disabled:bg-gray-100 disabled:cursor-not-allowed"
                >
                    <svg className="w-5 h-5" viewBox="0 0 24 24">
                        <path fill="#4285F4" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"/>
                        <path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"/>
                        <path fill="#FBBC05" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"/>
                        <path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"/>
                    </svg>
                    <span>Sign up with Google</span>
                </button>
            </div>

            <p className="mt-6 text-center text-sm text-gray-600">
                Already have an account?{' '}
                <button
                    type="button"
                    onClick={() => { setView('login'); resetState(); }}
                    className="text-rose-600 hover:text-rose-700 font-semibold hover:underline"
                >
                    Sign in
                </button>
            </p>
        </div>
    );

    const renderSuccessView = () => (
        <div className="text-center py-8">
            <div className="mx-auto bg-gradient-to-br from-green-400 to-green-500 rounded-full h-20 w-20 flex items-center justify-center shadow-lg mb-6 animate-bounce">
                <svg className="h-12 w-12 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={3}>
                    <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
                </svg>
            </div>
            <h3 className="text-2xl font-bold text-gray-800 mb-2">Success!</h3>
            <p className="text-gray-600 text-lg">{successMessage}</p>
            <div className="mt-6">
                <div className="inline-flex items-center text-sm text-gray-500">
                    <svg className="animate-spin h-4 w-4 mr-2 text-rose-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                    </svg>
                    Redirecting...
                </div>
            </div>
        </div>
    );

    return (
        <div className="modal-overlay" onClick={handleClose}>
            <div
                className="modal-content relative max-w-md w-full"
                onClick={e => e.stopPropagation()}
            >
                {/* Close Button - Top Right Corner */}
                <button
                    onClick={handleClose}
                    className="absolute top-4 right-4 z-10 text-gray-400 hover:text-gray-600 transition-colors duration-200 bg-gray-100 hover:bg-gray-200 rounded-full p-2 group"
                    aria-label="Close"
                >
                    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                    </svg>
                </button>

                {/* Modal Content */}
                <div className="p-6">
                    {successMessage ? (
                        renderSuccessView()
                    ) : (
                        <>
                        <div className="relative mb-8">
                            {/* Centered Brand */}
                            <div className="flex flex-col items-center justify-center text-center">
                                <img
                                    src="/logo.svg"
                                    alt="ENYVORA"
                                    className="h-10 w-auto mb-3 mx-auto"
                                />
                            </div>
                        </div>

                        {/* View Content */}
                        {view === 'login' && renderLoginView()}
                        {view === 'otp' && renderOtpView()}
                        {view === 'signup' && renderSignupView()}
                        </>
                    )}
                </div>
            </div>
        </div>
    );
}
