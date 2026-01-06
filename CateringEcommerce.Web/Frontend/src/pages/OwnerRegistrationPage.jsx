/*
========================================
File: src/components/owner/OwnerRegistrationPage.jsx (REVISED)
========================================
*/
import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import MultiStepProgressBar from '../components/owner/MultiStepProgressBar';
import Step1_BusinessAccount from '../components/owner/Step1_BusinessAccount';
import Step2_AddressLocation from '../components/owner/Step2_AddressLocation';
import Step3_ServicesAndMedia from '../components/owner/Step3_ServicesAndMedia';
import Step4_LegalAndPayments from '../components/owner/Step4_LegalAndPayments';
import RegistrationSuccess from '../components/owner/RegistrationSuccess';
import { apiService } from '../services/userApi';
import { ownerApiService } from '../services/ownerApi';
// A generic OTP modal - can be moved to a common folder later
const VerificationOtpModal = ({ isOpen, onClose, onVerify, verificationType, identifier }) => {
    const [otp, setOtp] = useState('');
    if (!isOpen) return null;
    return (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
            <div className="bg-white p-6 rounded-lg shadow-xl w-full max-w-sm">
                <h3 className="text-lg font-semibold text-center mb-2">Verify {verificationType}</h3>
                <p className="text-center text-sm text-neutral-500 mb-4">Enter the 6-digit code sent to {identifier}</p>
                <form onSubmit={(e) => { e.preventDefault(); onVerify(otp); }}>
                    <input type="text" value={otp} onChange={(e) => setOtp(e.target.value)} placeholder="______" maxLength="6" className="w-full text-center text-2xl tracking-[0.5em] border-neutral-300 rounded-md" />
                    <div className="flex gap-4 mt-4">
                        <button type="button" onClick={onClose} className="w-full py-2 px-4 border border-neutral-300 rounded-md text-sm">Cancel</button>
                        <button type="submit" className="w-full py-2 px-4 bg-rose-600 text-white rounded-md text-sm">Verify</button>
                    </div>
                </form>
            </div>
        </div>
    );
};

const stepNames = ["Business & Account", "Address", "Services", "Legal & Payment"];
const totalSteps = stepNames.length;

const imageForStep = [
    'https://placehold.co/600x800/FFF7ED/DB2777?text=Welcome!',
    'https://placehold.co/600x800/FEF3C7/DB2777?text=Location',
    'https://placehold.co/600x800/FFEDD5/DB2777?text=Your+Dishes',
    'https://placehold.co/600x800/FEF9C3/DB2777?text=Final+Step'
];

const initialFormData = {
    cateringNumberSameAsMobile: false,
    cuisineIds: '', foodTypeIds: '', serviceTypeIds: '', eventTypeIds: '',
    cateringMedia: [],
    isPhoneVerified: false, isEmailVerified: false, isCateringNumberVerified: false,
    cateringLogo: null,
    fssaiCertificate: null,
    gstCertificate: null,
    panCard: null,
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
            if (!formData.minOrderValue) newErrors.minOrderValue = "Minimum order value is required.";
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
        if (otp === response.otp) {
            const verificationFlag = `is${otpModalInfo.type.charAt(0).toUpperCase() + otpModalInfo.type.slice(1)}Verified`;
            setFormData(prev => ({ ...prev, [verificationFlag]: true }));
            setOtpModalInfo({ isOpen: false, type: null, value: '' });
        } else {
            alert("Invalid OTP");
        }
        otp = '';
    };

    const nextStep = () => {
        if (currentStep != 4 && validateStep()) {
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
            const formDataToSend = new FormData();

            // Add files
            (formData.cateringMedia ?? []).forEach((item) => {
                if (item?.file instanceof File) {
                    // 'CateringMedia' must match backend parameter name
                    formDataToSend.append("CateringMedia", item.file, item.file.name);
                }
            });

            // Remove non-serializable items (File objects, blob URLs)
            //delete formData.cateringMedia;
            console.log("Preparing to submit form data:", formData);
            // --- Step 1: Register owner ---
            const response = await ownerApiService.registerOwner(formData);

            if (!response?.result || !response?.data) {
                setErrors({ submit: response?.message || "Submission failed. Please try again." });
                return;
            }

            // --- Step 2: Upload Files (only if files exist) ---
            const fileResponse = await ownerApiService.uploadOwnerFiles(response.data, formDataToSend);

            if (!fileResponse?.result) {
                setErrors({ submit: "File upload failed. Please try again." });
                return;
            }

            // Success
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
        <div className="min-h-screen bg-amber-50 flex items-center justify-center p-4">
            <VerificationOtpModal
                isOpen={otpModalInfo.isOpen}
                onClose={() => setOtpModalInfo({ isOpen: false, type: null, value: '' })}
                onVerify={handleOtpVerify}
                verificationType={otpModalInfo.type}
                identifier={otpModalInfo.value}
            />
            <div className="container mx-auto max-w-6xl">
                <div className="bg-white rounded-xl shadow-2xl flex flex-col md:flex-row">
                    <div className="w-full md:w-2/5 p-8 bg-rose-50 rounded-l-xl hidden md:flex flex-col justify-center items-center">
                        <img src={imageForStep[currentStep - 1]} alt="Catering" className="w-full h-auto object-cover rounded-lg shadow-lg" />
                        <h2 className="text-2xl font-bold text-rose-800 mt-6 text-center">Join the Best Catering Network</h2>
                        <p className="text-neutral-600 mt-2 text-center text-sm">Reach more customers and grow your business with Feasto.</p>
                    </div>
                    <div className="w-full md:w-3/5 p-8 relative">
                        <button onClick={() => navigate('/')} className="absolute top-4 right-4 text-neutral-400 hover:text-neutral-700 text-2xl">&times;</button>
                        <MultiStepProgressBar currentStep={currentStep} steps={stepNames} />
                        <form onSubmit={handleSubmit}>
                            {currentStep === 1 && <Step1_BusinessAccount formData={formData} setFormData={setFormData} errors={errors} onVerifyClick={handleVerifyClick} />}
                            {currentStep === 2 && <Step2_AddressLocation formData={formData} setFormData={setFormData} errors={errors} />}
                            {currentStep === 3 && <Step3_ServicesAndMedia formData={formData} setFormData={setFormData} errors={errors} />}
                            {currentStep === 4 && <Step4_LegalAndPayments formData={formData} setFormData={setFormData} errors={errors} />}
                            <div className="mt-8 flex justify-between items-center">
                                {currentStep > 1 ? (
                                    <button type="button" onClick={prevStep} className="bg-neutral-200 text-neutral-700 px-6 py-2 rounded-md font-medium hover:bg-neutral-300">Back</button>
                                ) : <div />}
                                {currentStep < totalSteps ? (
                                    <button type="button" onClick={nextStep} className="bg-rose-600 text-white px-6 py-2 rounded-md font-medium hover:bg-rose-700">Next</button>
                                ) : (
                                    <button type="submit" className="bg-green-600 text-white px-6 py-2 rounded-md font-medium hover:bg-green-700">Submit for Verification</button>
                                )}
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </div>
    );
}