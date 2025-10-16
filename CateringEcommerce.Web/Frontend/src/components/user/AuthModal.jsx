import { useNavigate, Link } from 'react-router-dom';
import React, { useState } from 'react';
import { apiService } from '../../services/userApi'; // Import the API service for authentication
import { useAuth } from '../../contexts/AuthContext'; // Import the AuthContext to access login function

// Small component for the error message banner
const ErrorBanner = ({ message }) => {
    if (!message) return null;
    return (
        <div className="bg-red-50 text-red-700 p-3 rounded-md text-xs flex items-center mb-4">
            <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4 mr-2 flex-shrink-0" viewBox="0 0 20 20" fill="currentColor">
                <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
            </svg>
            <span>{message}</span>
        </div>
    );
};

export default function AuthModal({ isOpen, onClose, isPartnerLogin = false }) {
    const [view, setView] = useState('login'); // 'login', 'signup', 'otp'
    const navigate = useNavigate();
    // Data States
    const { login } = useAuth();
    const [phone, setPhone] = useState('');
    const [otp, setOtp] = useState('');
    const [fullName, setFullName] = useState('');
    const [authAction, setAuthAction] = useState('login'); // To distinguish between 'login' and 'signup' flows

    // UI States
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState('');
    const [successMessage, setSuccessMessage] = useState('');

    if (!isOpen) return null;

    const resetState = () => {
        setPhone('');
        setOtp('');
        setFullName('');
        setIsLoading(false);
        setError('');
        setSuccessMessage('');
        setAuthAction('login');
    };

    const handleClose = () => {
        resetState();
        setView('login');
        onClose();
    };

    const handleSendOtp = async (currentAction) => {
        const identifier = `+91${phone}`;
        setError('');
        setIsLoading(true);
        try {
            const { result, message } = await apiService.sendOtp(currentAction, identifier, isPartnerLogin);
            if (!result) {
                setError(message);
                setIsLoading(false);
                return;
            }
            setAuthAction(currentAction); // Set whether this is a 'login' or 'signup' OTP
            setIsLoading(false);
            setView('otp'); // Move to OTP view on success
        } catch (err) {
            setError(err.message);
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
            const { result, message, token, user, role } = await apiService.verifyOtp(authAction, `+91${phone}`, nameToSend, otp, isPartnerLogin);

            if (!result) {
                setError(message || 'OTP verification failed');
                throw new Error(message || 'OTP verification failed');
            }
            setIsLoading(false);
            await apiService.finalVerify(user.pkID, role, token); // Final verification step
            setSuccessMessage(`Welcome, ${user.fullName ?? user.cateringName}!`);

            setTimeout(() => {
                login({ pkid: user.pkID, name: user.fullName ?? user.cateringName, role: role,token: token }); // Call login from context
                handleClose();
                if (role === 'Owner') {
                    navigate('/owner/dashboard/');
                }
            }, 1500);

        } catch (err) {
            setError(err.message);
            setIsLoading(false);
        }
    };

    const handleGoogleLogin = async () => {
        // This function remains the same
        setError('');
        setIsLoading(true);
        try {
            const { googleAuthUrl } = await apiService.getGoogleAuthUrl();
            window.location.href = googleAuthUrl;
        } catch (err) {
            setError('Could not connect to Google. Please try again.');
            setIsLoading(false);
        }
    };

    const renderLoginView = () => (
        <div>
            <h3 className="text-xl font-semibold text-neutral-800 mb-4 text-center">Login</h3>
            <ErrorBanner message={error} />
            <form onSubmit={(e) => { e.preventDefault(); handleSendOtp('login'); }}>
                <div className="mb-4">
                    <div className="flex items-center border border-neutral-300 rounded-md overflow-hidden focus-within:ring-1 focus-within:ring-rose-500">
                        <span className="px-3 py-2 bg-neutral-100 text-neutral-600 text-sm">+91</span>
                        <input type="tel" placeholder="Phone" value={phone} onChange={(e) => setPhone(e.target.value)} className="w-full px-3 py-2 focus:outline-none sm:text-sm" required />
                    </div>
                </div>
                <button type="submit" disabled={isLoading} className="w-full bg-rose-600 text-white py-2 px-4 rounded-md hover:bg-rose-700 transition-colors text-sm font-medium disabled:bg-rose-300 disabled:cursor-not-allowed">
                    {isLoading ? 'Sending...' : 'Send One Time Password'}
                </button>
            </form>
            <div className="my-3 text-center text-neutral-500 text-xs">or</div>
            <button onClick={handleGoogleLogin} disabled={isLoading} className="w-full border border-neutral-300 text-neutral-700 py-2 px-4 rounded-md hover:bg-neutral-50 transition-colors text-sm flex items-center justify-center disabled:bg-neutral-200">
                <img src="https://placehold.co/20x20/FFFFFF/000000?text=G" alt="Google" className="mr-2 h-5 w-5" /> Sign in with Google
            </button>
            <p className="mt-4 text-center text-xs text-neutral-600">
                New to Feasto?{' '}
                {isPartnerLogin ? (
                    <Link to="/partner-registration" onClick={handleClose} className="text-rose-600 hover:underline font-medium">Create account</Link>
                ) : (
                    <button type="button" onClick={() => setView('signup')} className="text-rose-600 hover:underline font-medium">Create account</button>
                )}
            </p>
        </div>
    );

    const renderOtpView = () => (
        <div>
            <h3 className="text-xl font-semibold text-neutral-800 mb-2 text-center">Enter Verification Code</h3>
            <p className="text-center text-xs text-neutral-500 mb-4">An OTP was sent to +91{phone}</p>
            <ErrorBanner message={error} />
            <form onSubmit={handleVerifyOtp}>
                <div className="mb-4">
                    <input type="text" placeholder="6-Digit Code" value={otp} onChange={(e) => setOtp(e.target.value)} className="w-full px-3 py-2 border border-neutral-300 rounded-md focus:outline-none focus:ring-1 focus:ring-rose-500 sm:text-sm text-center tracking-[0.5em]" maxLength="6" required disabled={!!successMessage} />
                </div>
                <button type="submit" disabled={isLoading || !!successMessage} className="w-full bg-rose-600 text-white py-2 px-4 rounded-md hover:bg-rose-700 transition-colors text-sm font-medium disabled:bg-rose-300 disabled:cursor-not-allowed">
                    {isLoading ? 'Verifying...' : 'Verify & Proceed'}
                </button>
            </form>
            <p className="mt-4 text-center text-xs text-neutral-600">
                Incorrect number? <button type="button" onClick={() => { setView(authAction === 'signup' ? 'signup' : 'login'); setOtp(''); setError(''); }} className="text-rose-600 hover:underline font-medium">Change</button>
            </p>
        </div>
    );

    const renderSignupView = () => (
        <div>
            <h3 className="text-xl font-semibold text-neutral-800 mb-4 text-center">Sign Up</h3>
            <ErrorBanner message={error} />
            <form onSubmit={(e) => { e.preventDefault(); handleSendOtp('signup'); }}>
                <div className="mb-3">
                    <input type="text" placeholder="Full Name" value={fullName} onChange={(e) => setFullName(e.target.value)} className="w-full px-3 py-2 border border-neutral-300 rounded-md focus:outline-none focus:ring-1 focus:ring-rose-500 sm:text-sm" required />
                </div>
                <div className="mb-4">
                    <div className="flex items-center border border-neutral-300 rounded-md overflow-hidden focus-within:ring-1 focus-within:ring-rose-500">
                        <span className="px-3 py-2 bg-neutral-100 text-neutral-600 text-sm">+91</span>
                        <input type="tel" placeholder="Phone" value={phone} onChange={(e) => setPhone(e.target.value)} className="w-full px-3 py-2 focus:outline-none sm:text-sm" required />
                    </div>
                </div>
                <button type="submit" disabled={isLoading} className="w-full bg-rose-600 text-white py-2 px-4 rounded-md hover:bg-rose-700 transition-colors text-sm font-medium disabled:bg-rose-300 disabled:cursor-not-allowed">
                    {isLoading ? 'Sending OTP...' : 'Create Account & Send OTP'}
                </button>
            </form>
            <div className="my-3 text-center text-neutral-500 text-xs">or</div>
            <button onClick={handleGoogleLogin} className="w-full border border-neutral-300 text-neutral-700 py-2 px-4 rounded-md hover:bg-neutral-50 transition-colors text-sm flex items-center justify-center">
                <img src="https://placehold.co/20x20/FFFFFF/000000?text=G" alt="Google" className="mr-2 h-5 w-5" /> Sign up with Google
            </button>
            <p className="mt-4 text-center text-xs text-neutral-600">
                Already have an account? <button type="button" onClick={() => { setView('login'); resetState(); }} className="text-rose-600 hover:underline font-medium">Log in</button>
            </p>
        </div>
    );

    const renderSuccessView = () => (
        <div className="text-center py-8">
            <div className="mx-auto bg-green-100 rounded-full h-16 w-16 flex items-center justify-center">
                <svg className="h-10 w-10 text-green-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M5 13l4 4L19 7" />
                </svg>
            </div>
            <h3 className="text-xl font-semibold text-neutral-800 mt-4">Success!</h3>
            <p className="text-neutral-600 mt-1">{successMessage}</p>
        </div>
    );


    return (
        <div className="modal-overlay" onClick={handleClose}>
            <div className="modal-content" onClick={e => e.stopPropagation()}>
                <button onClick={handleClose} className="absolute top-2 right-3 text-neutral-500 hover:text-neutral-800 text-2xl font-bold z-10">&times;</button>

                {successMessage ? (
                    renderSuccessView()
                ) : (
                    <>
                        <div className="text-center mb-4">
                            <h2 className="text-2xl font-bold text-rose-600 mb-1">
                                <span className="icon-placeholder text-3xl">🍽️</span> Feasto
                            </h2>
                            <p className="text-neutral-600 text-sm">Your next delicious meal is just a click away!</p>
                        </div>

                        {view === 'login' && renderLoginView()}
                        {view === 'otp' && renderOtpView()}
                        {view === 'signup' && renderSignupView()}
                    </>
                )}
            </div>
        </div>
    );
}