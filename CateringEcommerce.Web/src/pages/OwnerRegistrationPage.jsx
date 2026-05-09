/*
========================================
File: src/components/owner/OwnerRegistrationPage.jsx (REDESIGNED)
========================================
*/
import React, { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import MultiStepProgressBar from '../components/owner/MultiStepProgressBar';
import Step1_BusinessAccount from '../components/owner/Step1_BusinessAccount';
import Step2_AddressLocation from '../components/owner/Step2_AddressLocation';
import Step3_ServicesAndMedia from '../components/owner/Step3_ServicesAndMedia';
import Step4_LegalAndPayments from '../components/owner/Step4_LegalAndPayments';
import Step5_Agreement from '../components/owner/Step5_Agreement';
import RegistrationSuccess from '../components/owner/RegistrationSuccess';
import { apiService } from '../services/userApi';
import { ownerApiService } from '../services/ownerApi';
import { useToast } from '../contexts/ToastContext';

// Modern OTP Modal with individual input boxes
const VerificationOtpModal = ({ isOpen, onClose, onVerify, verificationType, identifier }) => {
    const [otp, setOtp] = useState(['', '', '', '', '', '']);
    const [timer, setTimer] = useState(60);
    const [canResend, setCanResend] = useState(false);
    const [isVerifying, setIsVerifying] = useState(false);
    const [error, setError] = useState('');
    const inputRefs = useRef([]);
    const { showToast } = useToast();

    useEffect(() => {
        if (isOpen) {
            setOtp(['', '', '', '', '', '']);
            setTimer(60);
            setCanResend(false);
            setError('');
            // Focus first input when modal opens
            setTimeout(() => inputRefs.current[0]?.focus(), 100);
        }
    }, [isOpen]);

    useEffect(() => {
        if (isOpen && timer > 0) {
            const interval = setInterval(() => {
                setTimer(prev => {
                    if (prev <= 1) {
                        setCanResend(true);
                        return 0;
                    }
                    return prev - 1;
                });
            }, 1000);
            return () => clearInterval(interval);
        }
    }, [isOpen, timer]);

    const handleChange = (index, value) => {
        if (value.length > 1) {
            // Handle paste
            const pastedData = value.slice(0, 6).split('');
            const newOtp = [...otp];
            pastedData.forEach((char, i) => {
                if (index + i < 6 && /^\d$/.test(char)) {
                    newOtp[index + i] = char;
                }
            });
            setOtp(newOtp);
            const nextIndex = Math.min(index + pastedData.length, 5);
            inputRefs.current[nextIndex]?.focus();
            return;
        }

        if (/^\d*$/.test(value)) {
            const newOtp = [...otp];
            newOtp[index] = value;
            setOtp(newOtp);
            setError('');

            // Move to next input
            if (value && index < 5) {
                inputRefs.current[index + 1]?.focus();
            }
        }
    };

    const handleKeyDown = (index, e) => {
        if (e.key === 'Backspace' && !otp[index] && index > 0) {
            inputRefs.current[index - 1]?.focus();
        }
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        const otpString = otp.join('');
        if (otpString.length !== 6) {
            setError('Please enter complete 6-digit OTP');
            return;
        }
        setIsVerifying(true);
        setError('');
        try {
            await onVerify(otpString);
        } catch (err) {
            setError('Invalid OTP. Please try again.');
        } finally {
            setIsVerifying(false);
        }
    };

    const handleResend = () => {
        setTimer(60);
        setCanResend(false);
        setOtp(['', '', '', '', '', '']);
        setError('');
        inputRefs.current[0]?.focus();
        // Trigger resend OTP logic here
        console.log('Resending OTP to:', identifier);
    };

    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 bg-black bg-opacity-60 flex items-center justify-center z-50 p-4 backdrop-blur-sm animate-fade-in">
            <div className="bg-white rounded-2xl shadow-2xl w-full max-w-md transform transition-all animate-scale-in">
                {/* Header */}
                <div className="p-6 rounded-t-2xl text-white relative overflow-hidden" style={{ background: 'linear-gradient(135deg, #FF6B35 0%, #FF8C42 50%, #FFB627 100%)' }}>
                    <div className="absolute top-0 right-0 w-32 h-32 bg-white opacity-10 rounded-full -mr-16 -mt-16"></div>
                    <div className="absolute bottom-0 left-0 w-24 h-24 bg-white opacity-10 rounded-full -ml-12 -mb-12"></div>
                    <button
                        onClick={onClose}
                        className="absolute top-4 right-4 text-white hover:bg-white hover:bg-opacity-20 rounded-full p-1 transition-all"
                    >
                        <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                        </svg>
                    </button>
                    <div className="relative z-10">
                        <div className="w-16 h-16 bg-white bg-opacity-20 rounded-full flex items-center justify-center mb-4 mx-auto">
                            <svg xmlns="http://www.w3.org/2000/svg" className="h-8 w-8" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
                            </svg>
                        </div>
                        <h3 className="text-2xl font-bold text-center">Verify {verificationType === 'phone' ? 'Mobile' : verificationType === 'email' ? 'Email' : verificationType === 'cateringNumber' ? 'Catering Number' : verificationType}</h3>
                        <p className="text-center text-sm text-white text-opacity-90 mt-2">
                            Enter the 6-digit code sent to
                        </p>
                        <p className="text-center font-semibold mt-1">{identifier}</p>
                    </div>
                </div>

                {/* Body */}
                <div className="p-6 md:p-8">
                    <form onSubmit={handleSubmit}>
                        {/* OTP Input Boxes */}
                        <div className="flex gap-2 md:gap-3 justify-center mb-6">
                            {otp.map((digit, index) => (
                                <input
                                    key={index}
                                    ref={el => inputRefs.current[index] = el}
                                    type="text"
                                    inputMode="numeric"
                                    maxLength={1}
                                    value={digit}
                                    onChange={(e) => handleChange(index, e.target.value)}
                                    onKeyDown={(e) => handleKeyDown(index, e)}
                                    className={`w-12 h-14 md:w-14 md:h-16 text-center text-2xl font-bold border-2 rounded-xl transition-all duration-200 ${
                                        digit
                                            ? 'border-orange-500 bg-orange-50 text-orange-700'
                                            : error
                                                ? 'border-red-400 bg-red-50'
                                                : 'border-neutral-300 hover:border-orange-300 focus:border-orange-500'
                                    } focus:outline-none focus:ring-2 focus:ring-orange-200`}
                                />
                            ))}
                        </div>

                        {/* Error Message */}
                        {error && (
                            <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg flex items-center gap-2 text-sm text-red-700 animate-shake">
                                <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5 flex-shrink-0" viewBox="0 0 20 20" fill="currentColor">
                                    <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
                                </svg>
                                {error}
                            </div>
                        )}

                        {/* Timer & Resend */}
                        <div className="mb-6 text-center">
                            {!canResend ? (
                                <p className="text-sm text-neutral-600 flex items-center justify-center gap-2">
                                    <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" style={{ color: 'var(--color-primary)' }} viewBox="0 0 20 20" fill="currentColor">
                                        <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm1-12a1 1 0 10-2 0v4a1 1 0 00.293.707l2.828 2.829a1 1 0 101.415-1.415L11 9.586V6z" clipRule="evenodd" />
                                    </svg>
                                    Resend OTP in <span className="font-bold" style={{ color: 'var(--color-primary)' }}>{timer}s</span>
                                </p>
                            ) : (
                                <button
                                    type="button"
                                    onClick={handleResend}
                                    className="text-sm font-semibold hover:underline transition-all flex items-center justify-center gap-2 mx-auto"
                                    style={{ color: 'var(--color-primary)' }}
                                >
                                    <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                                    </svg>
                                    Resend OTP
                                </button>
                            )}
                        </div>

                        {/* Action Buttons */}
                        <div className="flex gap-3">
                            <button
                                type="button"
                                onClick={onClose}
                                className="flex-1 py-3 px-4 border-2 border-neutral-300 text-neutral-700 rounded-xl font-semibold hover:bg-neutral-50 transition-all"
                            >
                                Cancel
                            </button>
                            <button
                                type="submit"
                                disabled={isVerifying || otp.join('').length !== 6}
                                className="flex-1 py-3 px-4 text-white rounded-xl font-semibold disabled:opacity-50 disabled:cursor-not-allowed transition-all shadow-lg hover:shadow-xl transform hover:-translate-y-0.5 flex items-center justify-center gap-2"
                                style={{ background: 'linear-gradient(135deg, #FF6B35 0%, #FF8C42 50%, #FFB627 100%)' }}
                            >
                                {isVerifying ? (
                                    <>
                                        <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-white"></div>
                                        Verifying...
                                    </>
                                ) : (
                                    <>
                                        <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                                            <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                                        </svg>
                                        Verify OTP
                                    </>
                                )}
                            </button>
                        </div>
                    </form>

                    {/* Help Text */}
                    <p className="text-xs text-neutral-500 text-center mt-6">
                        Didn't receive the code? Check your spam folder or contact support.
                    </p>
                </div>
            </div>

            <style>{`
                @keyframes scale-in {
                    from { transform: scale(0.9); opacity: 0; }
                    to { transform: scale(1); opacity: 1; }
                }
                .animate-scale-in {
                    animation: scale-in 0.2s ease-out;
                }
                @keyframes shake {
                    0%, 100% { transform: translateX(0); }
                    25% { transform: translateX(-5px); }
                    75% { transform: translateX(5px); }
                }
                .animate-shake {
                    animation: shake 0.3s ease-in-out;
                }
            `}</style>
        </div>
    );
};

const stepNames = ["Business & Account", "Address", "Services", "Legal & Payment", "Agreement"];
const totalSteps = stepNames.length;

// Step content for left panel
const stepContent = [
    {
        title: "Welcome to ENYVORA Partners!",
        description: "Let's get your catering business registered. We'll guide you through every step.",
        benefits: [
            "Reach thousands of customers",
            "Professional order management",
            "Quick weekly payouts",
            "Dedicated partner support"
        ],
        icon: "M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4",
        gradient: "from-indigo-500 via-purple-500 to-pink-500"
    },
    {
        title: "Your Business Location",
        description: "Help customers find you easily. Accurate location improves delivery experience.",
        benefits: [
            "Pin your exact location on map",
            "Auto-verified address details",
            "Better customer discovery",
            "Accurate delivery zones"
        ],
        icon: "M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z M15 11a3 3 0 11-6 0 3 3 0 016 0z",
        gradient: "from-cyan-500 via-blue-500 to-indigo-500"
    },
    {
        title: "Showcase Your Services",
        description: "Tell us about your cuisine, services, and upload mouthwatering photos!",
        benefits: [
            "Highlight your specialties",
            "Upload kitchen & dish photos",
            "Set competitive pricing",
            "Attract more bookings"
        ],
        icon: "M3 9a2 2 0 012-2h.93a2 2 0 001.664-.89l.812-1.22A2 2 0 0110.07 4h3.86a2 2 0 011.664.89l.812 1.22A2 2 0 0018.07 7H19a2 2 0 012 2v9a2 2 0 01-2 2H5a2 2 0 01-2-2V9z M15 13a3 3 0 11-6 0 3 3 0 016 0z",
        gradient: "from-orange-500 via-amber-500 to-yellow-500"
    },
    {
        title: "Legal & Payment Setup",
        description: "Almost done! Verify your business credentials and set up payment details.",
        benefits: [
            "Secure document verification",
            "Fast payment processing",
            "GST compliant platform",
            "24-48 hour approval"
        ],
        icon: "M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z",
        gradient: "from-emerald-500 via-green-500 to-teal-500"
    },
    {
        title: "Terms & Agreement",
        description: "Review and sign our partner agreement to complete your registration.",
        benefits: [
            "Transparent terms & conditions",
            "Digital signature support",
            "Secure agreement storage",
            "Instant activation on approval"
        ],
        icon: "M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z",
        gradient: "from-rose-500 via-pink-500 to-fuchsia-500"
    }
];

// Modern Left Panel Component
const LeftPanel = ({ currentStep }) => {
    const content = stepContent[currentStep - 1];

    return (
        <div className={`w-full md:w-2/5 bg-gradient-to-br ${content.gradient} p-8 rounded-l-3xl hidden md:flex flex-col justify-between text-white relative overflow-hidden`}>
            {/* Background Decorations */}
            <div className="absolute top-0 right-0 w-64 h-64 bg-white opacity-10 rounded-full -mr-32 -mt-32"></div>
            <div className="absolute bottom-0 left-0 w-48 h-48 bg-white opacity-10 rounded-full -ml-24 -mb-24"></div>

            <div className="relative z-10">
                {/* Logo */}
                <div className="mb-8">
                    <div className="flex items-center gap-3">
                        <div className="w-12 h-12 bg-white bg-opacity-20 rounded-xl flex items-center justify-center backdrop-blur-sm">
                            <svg xmlns="http://www.w3.org/2000/svg" className="h-7 w-7" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
                            </svg>
                        </div>
                        <img src="/logo.svg" alt="ENYVORA Partners" className="h-10 w-auto" />
                    </div>
                </div>

                {/* Step Indicator */}
                <div className="mb-8">
                    <div className="flex items-center gap-2 mb-2">
                        <span className="text-sm font-semibold bg-white bg-opacity-20 px-3 py-1 rounded-full backdrop-blur-sm">
                            Step {currentStep} of {totalSteps}
                        </span>
                    </div>
                    <div className="w-full bg-white bg-opacity-20 rounded-full h-2 overflow-hidden backdrop-blur-sm">
                        <div
                            className="bg-white h-2 rounded-full transition-all duration-500 ease-out"
                            style={{ width: `${(currentStep / totalSteps) * 100}%` }}
                        ></div>
                    </div>
                </div>

                {/* Icon */}
                <div className="mb-6">
                    <div className="w-20 h-20 bg-white bg-opacity-20 rounded-2xl flex items-center justify-center backdrop-blur-sm">
                        <svg xmlns="http://www.w3.org/2000/svg" className="h-10 w-10" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                            <path strokeLinecap="round" strokeLinejoin="round" d={content.icon} />
                        </svg>
                    </div>
                </div>

                {/* Content */}
                <h2 className="text-3xl font-bold mb-3 leading-tight">{content.title}</h2>
                <p className="text-white text-opacity-90 mb-6 leading-relaxed">{content.description}</p>

                {/* Benefits */}
                <div className="space-y-3">
                    {content.benefits.map((benefit, index) => (
                        <div key={index} className="flex items-start gap-3 animate-fade-in" style={{ animationDelay: `${index * 0.1}s` }}>
                            <div className="flex-shrink-0 w-6 h-6 bg-white bg-opacity-30 rounded-full flex items-center justify-center backdrop-blur-sm mt-0.5">
                                <svg xmlns="http://www.w3.org/2000/svg" className="h-3.5 w-3.5" viewBox="0 0 20 20" fill="currentColor">
                                    <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
                                </svg>
                            </div>
                            <span className="text-sm leading-relaxed">{benefit}</span>
                        </div>
                    ))}
                </div>
            </div>

            {/* Bottom Section */}
            <div className="relative z-10 mt-8 pt-6 border-t border-white border-opacity-20">
                <div className="flex items-center justify-between text-sm">
                    <div>
                        <p className="text-white text-opacity-70 mb-1">Need Help?</p>
                        <p className="font-semibold">support@enyvora.com</p>
                    </div>
                    <div className="text-right">
                        <p className="text-white text-opacity-70 mb-1">Call Us</p>
                        <p className="font-semibold">+91 123-456-7890</p>
                    </div>
                </div>
            </div>
        </div>
    );
};

const initialFormData = {
    cateringNumberSameAsMobile: false,
    cuisineIds: '', foodTypeIds: '', serviceTypeIds: '', eventTypeIds: '',
    cateringMedia: [],
    isPhoneVerified: false, isEmailVerified: false, isCateringNumberVerified: false,
    cateringLogo: null,
    fssaiCertificate: null,
    gstCertificate: null,
    panCard: null,
    agreementAccepted: false,
    signature: null,
};

export default function OwnerRegistrationPage() {
    const navigate = useNavigate();
    const [currentStep, setCurrentStep] = useState(1);
    const [formData, setFormData] = useState(() => {
        const savedData = sessionStorage.getItem('ownerRegForm');
        // Load only text/boolean data. Files will be managed in state only.
        return savedData ? { ...initialFormData, ...JSON.parse(savedData) } : initialFormData;
    });
    const [errors, setErrors] = useState({});
    const [isSubmitted, setIsSubmitted] = useState(false);
    const [otpModalInfo, setOtpModalInfo] = useState({ isOpen: false, type: null, value: '' });

    // Effect to save form data to sessionStorage on every change
    useEffect(() => {
        // Create a copy of the form data but exclude file objects and Base64 strings
        const dataToStore = { ...formData };
        delete dataToStore.cateringLogo;
        delete dataToStore.fssaiCertificate;
        delete dataToStore.gstCertificate;
        delete dataToStore.panCard;
        delete dataToStore.cateringMedia;
        delete dataToStore.signature;

        sessionStorage.setItem('ownerRegForm', JSON.stringify(dataToStore));
    }, [formData]);

    const validateStep = () => {
        const newErrors = {};

        if (currentStep === 1) {
            if (!formData.cateringName?.trim()) newErrors.cateringName = "Catering name is required.";
            if (!formData.ownerName?.trim()) newErrors.ownerName = "Owner name is required.";
            if (!formData.mobile) newErrors.mobile = "Mobile number is required.";
            if (!formData.email) newErrors.email = "Email is required.";
            if (!formData.cateringLogo) newErrors.cateringLogo = "Catering logo is required.";
        }
        if (currentStep === 2) {
            if (!formData.shopNo) newErrors.shopNo = "Shop No. / Building is required.";
            if (!formData.floor) newErrors.floor = "Floor / Tower is required.";
            if (!formData.landmark) newErrors.landmark = "Landmark is required.";
            if (!formData.pincode) newErrors.pincode = "Pincode is required.";
            if (!formData.stateID) newErrors.stateID = "Please select a state."; 
            if (!formData.cityID) newErrors.cityID = "Please select a city.";
        }
        if (currentStep === 3) {
            if (!formData.cuisineIds) newErrors.cuisineIds = "Please select at least one cuisine type.";
            if (!formData.foodTypeIds) newErrors.foodTypeIds = "Please select at least one food type.";
            if (!formData.serviceTypeIds) newErrors.serviceTypeIds = "Please select at least one service type.";
            if (!formData.eventTypeIds) newErrors.eventTypeIds = "Please select at least one event type.";
            if (!formData.minGuestCount) newErrors.minGuestCount = "Minimum order value is required.";
            if (formData.cateringMedia.length < 5) {
                newErrors.cateringMedia = "Please upload at least 5 photos or videos.";
            } else if (formData.cateringMedia.length > 10) {
                newErrors.cateringMedia = "You can upload a maximum of 10 photos or videos.";
            }
        }
        if (currentStep === 4) {
            const fssaiRegex = /^\d{14}$/;
            const today = new Date();
            const expDate = new Date(formData.fssaiExpiry);

            // Set time to 00:00:00 for accurate comparison
            today.setHours(0, 0, 0, 0);
            expDate.setHours(0, 0, 0, 0);
            if (!fssaiRegex.test(formData.fssaiNumber)) newErrors.fssaiNumber = "Invalid FSSAI number. It must be exactly 14 digits.";
            if (!formData.fssaiExpiry || expDate <= today) newErrors.fssaiExpiry = "FSSAI expiry date must be a future date.";
            if (!formData.fssaiCertificate) newErrors.fssaiCertificate = "Please upload your FSSAI Certificate.";
            if (formData.isGstApplicable) {
                if (!formData.gstNumber) newErrors.gstNumber = "GST Number is required.";
                if (!formData.gstCertificate) newErrors.gstCertificate = "GST Certificate is required.";
            }
            if (!formData.panHolderName) newErrors.panHolderName = "PAN Holder Name is required.";
            if (!formData.panNumber) newErrors.panNumber = "PAN Number is required.";
            if (!formData.panCard) newErrors.panCard = "Please upload your PAN Card.";
            if (!formData.bankAccountName) newErrors.bankAccountName = "Account Holder Name is required.";
            if (!formData.bankAccountNumber) newErrors.bankAccountNumber = "Bank Account Number is required.";
            if (!formData.ifscCode) newErrors.ifscCode = "IFSC Code is required.";
        }
        if (currentStep === 5) {
            if (!formData.agreementAccepted) {
                newErrors.agreementAccepted = "You must accept the agreement to proceed.";
            }
            if (!formData.signature) {
                newErrors.signature = "Please provide your signature.";
            }
        }
        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleVerifyClick = async (type, value, role) => {
        try {
            const newErrors = {};
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            const phoneRegex = /^\d{10}$/;

            let isValid = true;

            if (!value) {
                if (type === 'email') {
                    newErrors.email = "Email address is required.";
                } else if (type === 'phone') {
                    newErrors.mobile = "Mobile number is required.";
                } else if (type === 'cateringNumber') {
                    newErrors.cateringNumber = "Catering number is required.";
                }
                isValid = false;
            } else if (type === 'phone' && !phoneRegex.test(value)) {
                newErrors.mobile = "Please enter a valid mobile number.";
                isValid = false;
            } else if (type === 'cateringNumber' && !phoneRegex.test(value)) {
                newErrors.cateringNumber = "Please enter a valid catering number.";
                isValid = false;
            } else if (type === 'email' && !emailRegex.test(value)) {
                newErrors.email = "Please enter a valid email address.";
                isValid = false;
            }

            if (!isValid) {
                setErrors(newErrors);
                return;
            }

            const { result, message } = await apiService.sendVerificationOtp(type, value, role);
            if (!result) {
                setErrors(prev => ({ ...prev, [type]: message || `Failed to send OTP to ${value}.` }));
                throw new Error(message || `Failed to send OTP to ${value}.`);
            }
            setOtpModalInfo({ isOpen: true, type, value });
            setErrors({}); // Clear previous errors on success
        } catch (err) {
            console.error("Verification error:", err.message);
        }
    };


    const handleOtpVerify = async (otp) => {
        // Here passing the 0 as pkId since this is for registration and we don't have a user PKID yet
        const response = await apiService.verifyUpdateOtp(otpModalInfo.type, otpModalInfo.value, otp, 0, 'Owner');
        console.log(`Verifying OTP ${otp} for ${otpModalInfo.type}`);
        if (response.result) {
            const verificationFlag = `is${otpModalInfo.type.charAt(0).toUpperCase() + otpModalInfo.type.slice(1)}Verified`;
            setFormData(prev => ({ ...prev, [verificationFlag]: true }));
            setOtpModalInfo({ isOpen: false, type: null, value: '' });
        } else {
            showToast("Invalid OTP. Please try again.", "error");
        }
        otp = '';
    };

    const nextStep = () => {
        if (validateStep()) {
            if (currentStep < totalSteps) setCurrentStep(currentStep + 1);
        }
    };

    const prevStep = () => {
        if (currentStep > 1) setCurrentStep(currentStep - 1);
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        if (!validateStep()) return;

        try {

            console.log("Preparing to submit form data:", formData);
            // --- Step 1: Register owner ---
            const response = await ownerApiService.registerOwner(formData);

            if (!response?.result || !response?.data) {
                setErrors({ submit: response?.message || "Submission failed. Please try again." });
                return;
            }

            const mediaFiles = formData.cateringMedia ?? [];

            // If no files, skip upload
            if (mediaFiles.length === 0) {
                setIsSubmitted(true);
                sessionStorage.removeItem("ownerRegForm");
                return;
            }

            // --- Step 2: Upload Files (only if files exist) ---
            // Upload files ONE BY ONE to avoid 413
            for (const item of mediaFiles) {
                if (!(item?.file instanceof File)) continue;

                const formDataToSend = new FormData();
                formDataToSend.append("CateringMedia", item.file, item.file.name);

                const fileResponse = await ownerApiService.uploadOwnerFiles(
                    response.data,   // ownerId / referenceId
                    formDataToSend
                );

                if (!fileResponse?.result) {
                    setErrors({ submit: `Failed to upload ${item.file.name}` });
                    return;
                }
            }

            // ✅ All uploads successful
            setIsSubmitted(true);
            sessionStorage.removeItem("ownerRegForm");

        } catch (error) {
            console.error("Failed to process files for submission:", error);
            setErrors({ submit: "There was an error preparing your files. Please try again." });
        }
    };

    if (isSubmitted) {
        return <RegistrationSuccess />;
    }

    return (
        <div className="min-h-screen bg-gradient-to-br from-neutral-50 to-neutral-100 flex items-center justify-center p-4 relative overflow-hidden">
            {/* Background decorations */}
            <div className="absolute top-0 left-0 w-96 h-96 bg-rose-200 rounded-full mix-blend-multiply filter blur-3xl opacity-20 animate-blob"></div>
            <div className="absolute bottom-0 right-0 w-96 h-96 bg-blue-200 rounded-full mix-blend-multiply filter blur-3xl opacity-20 animate-blob animation-delay-2000"></div>

            <VerificationOtpModal
                isOpen={otpModalInfo.isOpen}
                onClose={() => setOtpModalInfo({ isOpen: false, type: null, value: '' })}
                onVerify={handleOtpVerify}
                verificationType={otpModalInfo.type}
                identifier={otpModalInfo.value}
            />

            <div className="container mx-auto max-w-7xl relative z-10">
                <div className="bg-white rounded-3xl shadow-2xl flex flex-col md:flex-row overflow-hidden min-h-[600px]">
                    {/* Left Panel - Changes dynamically with each step */}
                    <LeftPanel currentStep={currentStep} />

                    {/* Right Panel - Form Content */}
                    <div className="w-full md:w-3/5 p-6 md:p-10 relative flex flex-col">
                        {/* Close Button */}
                        <button
                            onClick={() => navigate('/')}
                            className="absolute top-6 right-6 text-neutral-400 hover:text-neutral-700 hover:bg-neutral-100 rounded-full p-2 transition-all group"
                            aria-label="Close"
                        >
                            <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6 group-hover:rotate-90 transition-transform duration-200" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                            </svg>
                        </button>

                        {/* Progress Bar */}
                        <div className="mb-8">
                            <MultiStepProgressBar currentStep={currentStep} steps={stepNames} />
                        </div>

                        {/* Form Content - Scrollable */}
                        <div className="flex-1 overflow-y-auto pr-2 custom-scrollbar">
                            <form onSubmit={handleSubmit} className="h-full flex flex-col">
                                <div className="flex-1">
                                    {currentStep === 1 && <Step1_BusinessAccount formData={formData} setFormData={setFormData} errors={errors} onVerifyClick={handleVerifyClick} />}
                                    {currentStep === 2 && <Step2_AddressLocation formData={formData} setFormData={setFormData} errors={errors} />}
                                    {currentStep === 3 && <Step3_ServicesAndMedia formData={formData} setFormData={setFormData} errors={errors} />}
                                    {currentStep === 4 && <Step4_LegalAndPayments formData={formData} setFormData={setFormData} errors={errors} />}
                                    {currentStep === 5 && <Step5_Agreement formData={formData} setFormData={setFormData} errors={errors} />}
                                </div>

                                {/* Navigation Buttons - Sticky at bottom */}
                                <div className="mt-8 pt-6 border-t border-neutral-200 flex justify-between items-center gap-4">
                                    {currentStep > 1 ? (
                                        <button
                                            type="button"
                                            onClick={prevStep}
                                            className="flex items-center gap-2 px-6 py-3 bg-neutral-100 text-neutral-700 rounded-xl font-semibold hover:bg-neutral-200 transition-all"
                                        >
                                            <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                                                <path fillRule="evenodd" d="M9.707 16.707a1 1 0 01-1.414 0l-6-6a1 1 0 010-1.414l6-6a1 1 0 011.414 1.414L5.414 9H17a1 1 0 110 2H5.414l4.293 4.293a1 1 0 010 1.414z" clipRule="evenodd" />
                                            </svg>
                                            Back
                                        </button>
                                    ) : <div />}

                                    {currentStep < totalSteps ? (
                                        <button
                                            type="button"
                                            onClick={nextStep}
                                            className="flex items-center gap-2 px-8 py-3 text-white rounded-xl font-semibold transition-all shadow-lg hover:shadow-xl transform hover:-translate-y-0.5"
                                            style={{ background: 'linear-gradient(135deg, #FF6B35 0%, #FF8C42 50%, #FFB627 100%)' }}
                                        >
                                            Next Step
                                            <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                                                <path fillRule="evenodd" d="M10.293 3.293a1 1 0 011.414 0l6 6a1 1 0 010 1.414l-6 6a1 1 0 01-1.414-1.414L14.586 11H3a1 1 0 110-2h11.586l-4.293-4.293a1 1 0 010-1.414z" clipRule="evenodd" />
                                            </svg>
                                        </button>
                                    ) : (
                                        <button
                                            type="submit"
                                            className="flex items-center gap-2 px-8 py-3 bg-gradient-to-r from-green-600 to-emerald-600 text-white rounded-xl font-semibold hover:from-green-700 hover:to-emerald-700 transition-all shadow-lg hover:shadow-xl transform hover:-translate-y-0.5"
                                        >
                                            <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                                                <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                                            </svg>
                                            Submit for Verification
                                        </button>
                                    )}
                                </div>
                            </form>
                        </div>
                    </div>
                </div>
            </div>

            <style>{`
                @keyframes blob {
                    0% { transform: translate(0px, 0px) scale(1); }
                    33% { transform: translate(30px, -50px) scale(1.1); }
                    66% { transform: translate(-20px, 20px) scale(0.9); }
                    100% { transform: translate(0px, 0px) scale(1); }
                }
                .animate-blob {
                    animation: blob 7s infinite;
                }
                .animation-delay-2000 {
                    animation-delay: 2s;
                }
                .custom-scrollbar::-webkit-scrollbar {
                    width: 6px;
                }
                .custom-scrollbar::-webkit-scrollbar-track {
                    background: #f1f1f1;
                    border-radius: 10px;
                }
                .custom-scrollbar::-webkit-scrollbar-thumb {
                    background: #cbd5e0;
                    border-radius: 10px;
                }
                .custom-scrollbar::-webkit-scrollbar-thumb:hover {
                    background: #a0aec0;
                }
            `}</style>
        </div>
    );
}