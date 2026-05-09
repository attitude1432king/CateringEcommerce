import { useNavigate, Link } from 'react-router-dom';
import React, { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { X, Phone, Shield, CheckCircle, AlertCircle, ArrowRight, Loader2 } from 'lucide-react';
import { apiService } from '../../services/userApi';
import { initiateOAuthLogin } from '../../services/oauthApi';
import { useAuth } from '../../contexts/AuthContext';
import { getOrGenerateFingerprint, getDeviceInfo } from '../../utils/deviceFingerprint';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL.replace(/\/$/, '');

const ErrorBanner = ({ message }) => {
    if (!message) return null;
    return (
        <motion.div
            initial={{ opacity: 0, y: -6 }}
            animate={{ opacity: 1, y: 0 }}
            className="flex items-start gap-3 p-3 rounded-xl bg-danger-bg border border-danger/20 text-sm mb-4"
        >
            <AlertCircle size={16} className="text-danger mt-0.5 shrink-0" />
            <span className="text-red-700 flex-1">{message}</span>
        </motion.div>
    );
};

const GoogleIcon = () => (
    <svg className="w-5 h-5 shrink-0" viewBox="0 0 24 24">
        <path fill="#4285F4" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"/>
        <path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"/>
        <path fill="#FBBC05" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"/>
        <path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"/>
    </svg>
);

const SpinnerIcon = () => (
    <Loader2 size={18} className="animate-spin" />
);

/* ─────────────────────────────────────────────────────────────
   These components MUST live outside AuthModal so React can
   keep a stable component-type reference across re-renders.
   Defining them inside the parent causes unmount/remount on
   every keystroke, which destroys input focus.
───────────────────────────────────────────────────────────── */

const Divider = () => (
    <div className="relative my-5">
        <div className="absolute inset-0 flex items-center">
            <div className="w-full border-t border-neutral-100" />
        </div>
        <div className="relative flex justify-center">
            <span className="px-4 bg-white text-xs text-neutral-400 font-medium uppercase tracking-wider">or continue with</span>
        </div>
    </div>
);

const PrimaryBtn = ({ disabled, loading, children, ...props }) => (
    <button
        {...props}
        disabled={disabled || loading}
        className="w-full flex items-center justify-center gap-2 py-3.5 rounded-xl text-white font-bold text-sm transition-all duration-200 hover:scale-[1.02] active:scale-[0.98] disabled:opacity-50 disabled:cursor-not-allowed disabled:transform-none"
        style={{ background: disabled || loading ? undefined : 'var(--gradient-catering)', boxShadow: disabled || loading ? 'none' : 'var(--shadow-cta)' }}
    >
        {loading ? <><SpinnerIcon /> Processing…</> : children}
    </button>
);

const GoogleBtn = ({ label, onClick, disabled }) => (
    <button
        type="button"
        onClick={onClick}
        disabled={disabled}
        className="w-full flex items-center justify-center gap-3 py-3.5 rounded-xl border-2 border-neutral-200 bg-white text-neutral-700 font-semibold text-sm hover:border-neutral-300 hover:bg-neutral-50 transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed"
    >
        <GoogleIcon /> {label}
    </button>
);

const PhoneField = ({ phone, onChange, phoneError, isPhoneValid }) => (
    <div>
        <label className="block text-xs font-semibold text-neutral-600 uppercase tracking-wider mb-2">
            Phone Number
        </label>
        <div className={`flex items-center rounded-xl border-2 overflow-hidden transition-all duration-200 bg-white ${
            phoneError ? 'border-danger' : isPhoneValid ? 'border-success' : 'border-neutral-200 focus-within:border-primary focus-within:ring-2 focus-within:ring-primary/15'
        }`}>
            <span className="px-4 py-3.5 bg-neutral-50 text-neutral-600 font-medium text-sm border-r-2 border-neutral-200 shrink-0 select-none">
                <Phone size={14} className="inline mr-1 opacity-60" />+91
            </span>
            <input
                type="tel"
                placeholder="10-digit mobile number"
                value={phone}
                onChange={onChange}
                className="w-full px-4 py-3.5 focus:outline-none text-sm bg-transparent text-neutral-900 placeholder-neutral-400"
                maxLength="10"
                required
            />
            {isPhoneValid && (
                <CheckCircle size={16} className="text-success mr-3 shrink-0" />
            )}
        </div>
        {phoneError && (
            <p className="mt-1.5 text-xs text-danger flex items-center gap-1">
                <AlertCircle size={12} /> {phoneError}
            </p>
        )}
    </div>
);

export default function AuthModal({ isOpen, onClose, isPartnerLogin = false }) {
    const [view, setView] = useState('login'); // 'login', 'signup', 'otp'
    const navigate = useNavigate();
    const { login } = useAuth();

    // Data States
    const [phone, setPhone]         = useState('');
    const [otp, setOtp]             = useState('');
    const [fullName, setFullName]   = useState('');
    const [authAction, setAuthAction] = useState('login');

    // UI States
    const [isLoading, setIsLoading]           = useState(false);
    const [error, setError]                   = useState('');
    const [successMessage, setSuccessMessage] = useState('');

    // Validation States
    const [isPhoneValid, setIsPhoneValid] = useState(false);
    const [phoneError, setPhoneError]     = useState('');
    const [isOtpValid, setIsOtpValid]     = useState(false);

    // Device Tracking & 2FA States
    const [deviceFingerprint, setDeviceFingerprint] = useState(null);
    const [deviceInfo, setDeviceInfo]               = useState(null);
    const [trustDevice, setTrustDevice]             = useState(false);
    const [otpPurpose, setOtpPurpose]               = useState(null);

    useEffect(() => {
        const initDeviceFingerprint = async () => {
            try {
                const fingerprint = await getOrGenerateFingerprint();
                const info = getDeviceInfo();
                setDeviceFingerprint(fingerprint);
                setDeviceInfo(info);
            } catch (err) {
                console.error('Failed to generate device fingerprint:', err);
            }
        };
        if (isOpen) initDeviceFingerprint();
    }, [isOpen]);

    useEffect(() => {
        const phoneRegex = /^[0-9]{10}$/;
        if (phone.length === 0) {
            setIsPhoneValid(false);
            setPhoneError('');
        } else if (!phoneRegex.test(phone)) {
            setIsPhoneValid(false);
            if (phone.length > 0 && phone.length < 10) setPhoneError('Phone number must be 10 digits');
            else if (phone.length > 10) setPhoneError('Phone number cannot exceed 10 digits');
            else setPhoneError('Please enter only numbers');
        } else {
            setIsPhoneValid(true);
            setPhoneError('');
        }
    }, [phone]);

    useEffect(() => {
        setIsOtpValid(/^[0-9]{6}$/.test(otp));
    }, [otp]);

    if (!isOpen) return null;

    const resetState = () => {
        setPhone(''); setOtp(''); setFullName('');
        setIsLoading(false); setError(''); setSuccessMessage('');
        setAuthAction('login'); setPhoneError('');
        setIsPhoneValid(false); setIsOtpValid(false);
        setTrustDevice(false); setOtpPurpose(null);
    };

    const handleClose = () => { resetState(); setView('login'); onClose(); };

    const handlePhoneChange = (e) => {
        const value = e.target.value.replace(/\D/g, '');
        if (value.length <= 10) setPhone(value);
    };

    const handleOtpChange = (e) => {
        const value = e.target.value.replace(/\D/g, '');
        if (value.length <= 6) setOtp(value);
    };

    const handleSendOtp = async (currentAction) => {
        setError('');
        setIsLoading(true);
        try {
            const response = await apiService.sendOtp(currentAction, phone, isPartnerLogin, deviceFingerprint);
            if (!response.result) { setError(response.message); setIsLoading(false); return; }
            setAuthAction(currentAction);
            if (response.data && response.data.purpose) {
                setOtpPurpose(response.data.purpose);
            } else {
                setOtpPurpose({
                    userMessage: currentAction === 'signup' ? 'Verify your account' : 'Verify to continue',
                    description: currentAction === 'signup'
                        ? "We've sent a verification code to your phone number"
                        : "Enter the verification code sent to your phone",
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
        setError(''); setIsLoading(true); setSuccessMessage('');
        try {
            const nameToSend = authAction === 'signup' ? fullName : '';
            const deviceInfoPayload = {
                deviceFingerprint,
                trustDevice: trustDevice && !isPartnerLogin,
                browser: deviceInfo?.browser || null,
                os: deviceInfo?.os || null,
            };
            const response = await apiService.verifyOtp(authAction, phone, nameToSend, otp, isPartnerLogin, deviceInfoPayload);
            if (!response.result) { setError(response.message || 'OTP verification failed'); throw new Error(response.message || 'OTP verification failed'); }

            const { token, user, role, deviceTrusted } = response;
            setIsLoading(false);
            await apiService.finalVerify(user.pkID, role);

            let welcomeMsg = `Welcome, ${user.fullName ?? user.cateringName}!`;
            if (deviceTrusted && !isPartnerLogin) welcomeMsg += ' This device is now trusted for 30 days.';
            setSuccessMessage(welcomeMsg);

            setTimeout(() => {
                login({
                    pkid: user.pkID,
                    name: user.fullName ?? user.cateringName,
                    role: role,
                    token: token,
                    phone: user.phone,
                    email: user.email,
                    profilePhoto: user.profilePhoto ? `${API_BASE_URL}${user.profilePhoto}` : undefined,
                });
                handleClose();
                const authRedirect = localStorage.getItem('auth_redirect');
                if (authRedirect) { localStorage.removeItem('auth_redirect'); navigate(authRedirect); }
                else if (role === 'Owner') navigate('/owner/dashboard/');
            }, 1500);
        } catch (err) {
            setError(err.message || 'Verification failed. Please try again.');
            setIsLoading(false);
        }
    };

    const handleGoogleLogin = async () => {
        setError(''); setIsLoading(true);
        // SET auth_redirect first (before any await) if not already set by useAuthGuard.
        // This ensures localStorage always has a value before we read it.
        if (!localStorage.getItem('auth_redirect')) {
            localStorage.setItem('auth_redirect', window.location.pathname);
        }
        const rawRedirect = localStorage.getItem('auth_redirect');
        try {
            const { sanitizeRedirectUrl } = await import('../../utils/securityUtils');
            const redirectPath = sanitizeRedirectUrl(rawRedirect, '/');
            localStorage.setItem('auth_redirect', redirectPath);  // overwrite with sanitized value
            localStorage.setItem('oauth_provider', 'google');
            const response = await initiateOAuthLogin('google');
            if (response.success && response.data?.authorizationUrl) {
                window.location.href = response.data.authorizationUrl;
            } else {
                throw new Error(response.message || 'Failed to initiate Google login');
            }
        } catch (err) {
            console.error('Google login error:', err);
            setError(err.message || 'Google login is currently unavailable. Please try phone login.');
            setIsLoading(false);
        }
    };

    const renderLoginView = () => (
        <div className="space-y-4">
            <div className="text-center mb-5">
                <h3 className="font-extrabold text-neutral-900 text-xl leading-tight mb-1">
                    {isPartnerLogin ? 'Partner Login' : 'Welcome Back'}
                </h3>
                <p className="text-sm text-neutral-500">
                    {isPartnerLogin ? 'Sign in to manage your catering business' : 'Sign in to book amazing catering'}
                </p>
            </div>

            <ErrorBanner message={error} />

            <form onSubmit={(e) => { e.preventDefault(); handleSendOtp('login'); }} className="space-y-4">
                <PhoneField phone={phone} onChange={handlePhoneChange} phoneError={phoneError} isPhoneValid={isPhoneValid} />
                <PrimaryBtn type="submit" disabled={!isPhoneValid} loading={isLoading}>
                    Send OTP <ArrowRight size={16} />
                </PrimaryBtn>
            </form>

            <Divider />
            <GoogleBtn label="Sign in with Google" onClick={handleGoogleLogin} disabled={isLoading} />

            <p className="text-center text-sm text-neutral-500 mt-4">
                {isPartnerLogin ? (
                    <>New partner?{' '}
                        <Link to="/partner-registration" onClick={handleClose} className="text-primary font-semibold hover:underline">
                            Register your business
                        </Link>
                    </>
                ) : (
                    <>Don't have an account?{' '}
                        <button type="button" onClick={() => setView('signup')} className="text-primary font-semibold hover:underline">
                            Sign up
                        </button>
                    </>
                )}
            </p>
        </div>
    );

    const renderSignupView = () => (
        <div className="space-y-4">
            <div className="text-center mb-5">
                <h3 className="font-extrabold text-neutral-900 text-xl leading-tight mb-1">
                    {isPartnerLogin ? 'Partner Registration' : 'Create Account'}
                </h3>
                <p className="text-sm text-neutral-500">
                    {isPartnerLogin ? 'Start your catering business journey' : 'Join us for amazing food experiences'}
                </p>
            </div>

            <ErrorBanner message={error} />

            <form onSubmit={(e) => { e.preventDefault(); handleSendOtp('signup'); }} className="space-y-4">
                <div>
                    <label className="block text-xs font-semibold text-neutral-600 uppercase tracking-wider mb-2">
                        Full Name
                    </label>
                    <input
                        type="text"
                        placeholder="Enter your full name"
                        value={fullName}
                        onChange={(e) => setFullName(e.target.value)}
                        className="w-full px-4 py-3.5 rounded-xl border-2 border-neutral-200 bg-white text-sm text-neutral-900 placeholder-neutral-400 focus:outline-none focus:border-primary focus:ring-2 focus:ring-primary/15 transition-all"
                        required
                    />
                </div>
                <PhoneField phone={phone} onChange={handlePhoneChange} phoneError={phoneError} isPhoneValid={isPhoneValid} />
                <PrimaryBtn type="submit" disabled={!isPhoneValid || !fullName.trim()} loading={isLoading}>
                    Create Account &amp; Send OTP <ArrowRight size={16} />
                </PrimaryBtn>
            </form>

            <Divider />
            <GoogleBtn label="Sign up with Google" onClick={handleGoogleLogin} disabled={isLoading} />

            <p className="text-center text-sm text-neutral-500 mt-4">
                Already have an account?{' '}
                <button type="button" onClick={() => { setView('login'); resetState(); }} className="text-primary font-semibold hover:underline">
                    Sign in
                </button>
            </p>
        </div>
    );

    const renderOtpView = () => (
        <div className="space-y-4">
            <div className="text-center mb-5">
                <div className="mx-auto w-14 h-14 rounded-2xl flex items-center justify-center mb-4" style={{ background: 'var(--gradient-catering)' }}>
                    <Shield size={26} className="text-white" strokeWidth={2} />
                </div>
                <h3 className="font-extrabold text-neutral-900 text-xl leading-tight mb-1">
                    {otpPurpose?.userMessage || 'Verify Your Number'}
                </h3>
                <p className="text-sm text-neutral-500">
                    {otpPurpose?.description || "We've sent a 6-digit code to"}<br />
                    <span className="font-bold text-neutral-800">+91 {phone}</span>
                </p>
                {authAction === 'login' && !isPartnerLogin && (
                    <span className="inline-flex items-center gap-1.5 mt-3 px-3 py-1 rounded-full bg-info-bg border border-info/20 text-xs font-semibold text-blue-700">
                        <Shield size={11} /> Two-Factor Authentication
                    </span>
                )}
            </div>

            <ErrorBanner message={error} />

            <form onSubmit={handleVerifyOtp} className="space-y-4">
                <div>
                    <label className="block text-xs font-semibold text-neutral-600 uppercase tracking-wider mb-2">
                        Enter OTP
                    </label>
                    <input
                        type="text"
                        placeholder="000000"
                        value={otp}
                        onChange={handleOtpChange}
                        className="w-full px-4 py-4 rounded-xl border-2 border-neutral-200 bg-white text-center text-2xl font-bold tracking-[0.6em] text-neutral-900 focus:outline-none focus:border-primary focus:ring-2 focus:ring-primary/15 transition-all"
                        maxLength="6"
                        required
                        disabled={!!successMessage}
                        autoComplete="one-time-code"
                        inputMode="numeric"
                    />
                    {otp.length > 0 && !isOtpValid && otp.length < 6 && (
                        <p className="mt-2 text-xs text-neutral-400 text-center">
                            {6 - otp.length} more digit{6 - otp.length !== 1 ? 's' : ''} required
                        </p>
                    )}
                </div>

                {authAction === 'login' && !isPartnerLogin && (
                    <div className="flex items-start gap-3 p-3.5 rounded-xl bg-neutral-50 border border-neutral-100">
                        <input
                            type="checkbox"
                            id="trustDevice"
                            checked={trustDevice}
                            onChange={(e) => setTrustDevice(e.target.checked)}
                            className="mt-0.5 w-4 h-4 rounded accent-primary cursor-pointer"
                        />
                        <label htmlFor="trustDevice" className="cursor-pointer">
                            <span className="text-sm font-semibold text-neutral-800 block">Trust this device for 30 days</span>
                            <span className="text-xs text-neutral-500">Skip OTP on this device for the next 30 days</span>
                        </label>
                    </div>
                )}

                <PrimaryBtn type="submit" disabled={!isOtpValid || !!successMessage} loading={isLoading}>
                    Verify &amp; Continue <ArrowRight size={16} />
                </PrimaryBtn>
            </form>

            <div className="text-center space-y-2 pt-1">
                <button
                    type="button"
                    onClick={() => handleSendOtp(authAction)}
                    disabled={isLoading}
                    className="text-sm text-primary font-semibold hover:underline disabled:text-neutral-400 disabled:no-underline"
                >
                    Resend OTP
                </button>
                <p className="text-sm text-neutral-500">
                    Wrong number?{' '}
                    <button
                        type="button"
                        onClick={() => { setView(authAction === 'signup' ? 'signup' : 'login'); setOtp(''); setError(''); }}
                        className="text-primary font-semibold hover:underline"
                    >
                        Change number
                    </button>
                </p>
            </div>
        </div>
    );

    const renderSuccessView = () => (
        <div className="text-center py-10">
            <motion.div
                initial={{ scale: 0.5, opacity: 0 }}
                animate={{ scale: 1, opacity: 1 }}
                transition={{ type: 'spring', stiffness: 300, damping: 18 }}
                className="mx-auto w-20 h-20 rounded-full flex items-center justify-center mb-6"
                style={{ background: 'linear-gradient(135deg, #22c55e, #16a34a)' }}
            >
                <CheckCircle size={40} className="text-white" strokeWidth={2.5} />
            </motion.div>
            <h3 className="font-extrabold text-neutral-900 text-2xl mb-2">All Set!</h3>
            <p className="text-neutral-600 mb-6">{successMessage}</p>
            <div className="inline-flex items-center gap-2 text-sm text-neutral-400">
                <Loader2 size={16} className="animate-spin text-primary" />
                Redirecting…
            </div>
        </div>
    );

    return (
        <AnimatePresence>
            {isOpen && (
                <>
                    {/* Backdrop */}
                    <motion.div
                        key="auth-backdrop"
                        initial={{ opacity: 0 }}
                        animate={{ opacity: 1 }}
                        exit={{ opacity: 0 }}
                        transition={{ duration: 0.2 }}
                        className="fixed inset-0 bg-black/60 backdrop-blur-sm z-[10000]"
                        onClick={handleClose}
                    />

                    {/* Panel */}
                    <motion.div
                        key="auth-panel"
                        initial={{ opacity: 0, scale: 0.96, y: 8 }}
                        animate={{ opacity: 1, scale: 1, y: 0 }}
                        exit={{ opacity: 0, scale: 0.96, y: 8 }}
                        transition={{ type: 'spring', stiffness: 380, damping: 30 }}
                        className="fixed inset-0 z-[10001] flex items-center justify-center p-4 pointer-events-none"
                    >
                        <div
                            className="relative bg-white rounded-2xl shadow-[0_25px_60px_rgba(0,0,0,0.18)] max-w-md w-full max-h-[90vh] overflow-y-auto pointer-events-auto"
                            onClick={(e) => e.stopPropagation()}
                        >
                            {/* Gradient top accent strip */}
                            <div className="h-1 w-full rounded-t-2xl" style={{ background: 'var(--gradient-catering)' }} />

                            {/* Close button */}
                            <button
                                onClick={handleClose}
                                className="icon-btn absolute top-4 right-4 z-10"
                                aria-label="Close"
                            >
                                <X size={16} strokeWidth={2.5} />
                            </button>

                            <div className="px-7 pt-7 pb-8">
                                {successMessage ? (
                                    renderSuccessView()
                                ) : (
                                    <>
                                        {/* Brand */}
                                        <div className="flex justify-center mb-6">
                                            <img src="/logo.svg" alt="ENYVORA" className="h-9 w-auto" />
                                        </div>

                                        {view === 'login'  && renderLoginView()}
                                        {view === 'signup' && renderSignupView()}
                                        {view === 'otp'    && renderOtpView()}
                                    </>
                                )}
                            </div>
                        </div>
                    </motion.div>
                </>
            )}
        </AnimatePresence>
    );
}
