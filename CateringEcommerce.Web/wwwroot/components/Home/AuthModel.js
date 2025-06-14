
// AuthModal Component
function AuthModal({ isOpen, onClose, onSuccessfulAuth }) {
    const [isLoginView, setIsLoginView] = useState(true);
    const [phoneNumber, setPhone] = useState('');
    const [email, setEmail] = useState('');
    const [fullName, setFullName] = useState('');
    const [agreedToTerms, setAgreedToTerms] = useState(false);


    if (!isOpen) return null;

    const handleSwitchView = () => {
        setIsLoginView(!isLoginView);
        // Reset form fields on view switch
        setPhone('');
        setEmail('');
        setFullName('');
        setAgreedToTerms(false);
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        if (!phoneNumber) {
            alert('Please enter a valid phone number.');
            return;
        }

        if (!otpSent) {
            // Step 1: Send OTP
            const result = await sendOtp(phoneNumber);
            setMessage(result.message);
            if (result.success) setOtpSent(true);
        } else {
            // Step 2: Verify OTP
            const result = await verifyOtp(phoneNumber, otp);
            setMessage(result.message);
            if (result.success) {
                onSuccessfulAuth();
                onClose();
            }
        }
    };

    return (
        <div className="modal-overlay" onClick={onClose}>
            <div className="modal-content" onClick={e => e.stopPropagation()}>
                <button onClick={onClose} className="absolute top-2 right-3 text-neutral-500 hover:text-neutral-800 text-2xl font-bold">&times;</button>

                <div className="text-center mb-4">
                    <h2 className="text-2xl font-bold text-rose-600 mb-1">
                        <span className="icon-placeholder text-3xl">🍽️</span> QuickFeast
                    </h2>
                    <p className="text-neutral-600 text-sm">Your next delicious meal is just a click away!</p>
                </div>

                {isLoginView ? (
                    // Login View
                    <div>
                        <h3 className="text-xl font-semibold text-neutral-800 mb-4 text-center">Login</h3>
                        <form onSubmit={handleSubmit}>
                            <div className="mb-4">
                                <div className="flex items-center border border-neutral-300 rounded-md overflow-hidden">
                                    <span className="px-3 py-2 bg-neutral-100 text-neutral-600 text-sm">+91</span>
                                    <input
                                        type="tel"
                                        placeholder="Phone"
                                        value={phoneNumber}
                                        onChange={(e) => setPhone(e.target.value)}
                                        className="w-full px-3 py-2 focus:outline-none focus:ring-1 focus:ring-rose-500 sm:text-sm"
                                        required
                                    />
                                </div>
                            </div>
                            <button type="submit" className="w-full bg-rose-600 text-white py-2 px-4 rounded-md hover:bg-rose-700 transition-colors text-sm font-medium">
                                Send One Time Password
                            </button>
                            {otpSent && (
                                <div className="mb-4">
                                    <input
                                        type="text"
                                        placeholder="Enter OTP"
                                        value={otp}
                                        onChange={(e) => setOtp(e.target.value)}
                                        className="w-full px-3 py-2 border border-neutral-300 rounded-md focus:outline-none focus:ring-1 focus:ring-rose-500 sm:text-sm"
                                        required
                                    />
                                </div>
                            )}
                            <div className="my-3 text-center text-neutral-500 text-xs">or</div>
                            <button type="button" onClick={() => console.log("Continue with Email")} className="w-full border border-neutral-300 text-neutral-700 py-2 px-4 rounded-md hover:bg-neutral-50 transition-colors text-sm flex items-center justify-center">
                                <span className="icon-placeholder mr-2">📧</span> Continue with Email
                            </button>
                            <button type="button" onClick={() => console.log("Sign in with Google")} className="w-full border border-neutral-300 text-neutral-700 py-2 px-4 rounded-md hover:bg-neutral-50 transition-colors text-sm flex items-center justify-center mt-2">
                                <img src="https://placehold.co/20x20/FFFFFF/000000?text=G" alt="Google" className="mr-2 h-5 w-5" /> Sign in with Google
                            </button>
                            <p className="mt-4 text-center text-xs text-neutral-600">
                                New to QuickFeast? <button type="button" onClick={handleSwitchView} className="text-rose-600 hover:underline font-medium">Create account</button>
                            </p>
                        </form>
                    </div>
                ) : (
                    // Sign Up View
                    <div>
                        <h3 className="text-xl font-semibold text-neutral-800 mb-4 text-center">Sign Up</h3>
                        <form onSubmit={handleSubmit}>
                            <div className="mb-3">
                                <input
                                    type="text"
                                    placeholder="Full Name"
                                    value={fullName}
                                    onChange={(e) => setFullName(e.target.value)}
                                    className="w-full px-3 py-2 border border-neutral-300 rounded-md focus:outline-none focus:ring-1 focus:ring-rose-500 sm:text-sm"
                                    required
                                />
                            </div>
                            <div className="mb-3">
                                <input
                                    type="email"
                                    placeholder="Email"
                                    value={email}
                                    onChange={(e) => setEmail(e.target.value)}
                                    className="w-full px-3 py-2 border border-neutral-300 rounded-md focus:outline-none focus:ring-1 focus:ring-rose-500 sm:text-sm"
                                    required
                                />
                            </div>
                            <div className="mb-4 flex items-start">
                                <input
                                    type="checkbox"
                                    id="terms"
                                    checked={agreedToTerms}
                                    onChange={(e) => setAgreedToTerms(e.target.checked)}
                                    className="h-4 w-4 text-rose-600 border-neutral-300 rounded focus:ring-rose-500 mt-1"
                                    required
                                />
                                <label htmlFor="terms" className="ml-2 block text-xs text-neutral-600">
                                    I agree to QuickFeast's <a href="#" className="text-rose-600 hover:underline">Terms of Service</a>, <a href="#" className="text-rose-600 hover:underline">Privacy Policy</a> and Content Policies.
                                </label>
                            </div>
                            <button type="submit" className="w-full bg-rose-600 text-white py-2 px-4 rounded-md hover:bg-rose-700 transition-colors text-sm font-medium">
                                Create account
                            </button>
                            <div className="my-3 text-center text-neutral-500 text-xs">or</div>
                            <button type="button" onClick={() => console.log("Sign up with Google")} className="w-full border border-neutral-300 text-neutral-700 py-2 px-4 rounded-md hover:bg-neutral-50 transition-colors text-sm flex items-center justify-center">
                                <img src="https://placehold.co/20x20/FFFFFF/000000?text=G" alt="Google" className="mr-2 h-5 w-5" /> Sign up with Google
                            </button>
                            <p className="mt-4 text-center text-xs text-neutral-600">
                                Already have an account? <button type="button" onClick={handleSwitchView} className="text-rose-600 hover:underline font-medium">Log in</button>
                            </p>
                        </form>
                    </div>
                )}
            </div>
        </div>
    );
}
