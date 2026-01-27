/*
========================================
File: src/components/UserProfileSettings.jsx (REVISED)
========================================
Updated to include a robust profile photo upload and crop workflow using ImageUploader.
*/
import React, { useState, useEffect, useRef } from 'react';
import { useAuth } from '../../contexts/AuthContext';
import { apiService } from '../../services/userApi'; // Import the API service
import ImageCropUploader from '../owner/dashboard/settings/ImageCropUploader'; // Importing the existing crop uploader
import { useToast } from '../../contexts/ToastContext';


const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

// Helper to generate initials avatar if no photo exists
const generateInitialsAvatar = (name) => {
    if (!name) return '';
    const initials = name.split(' ').map(n => n[0]).slice(0, 2).join('').toUpperCase();
    const canvas = document.createElement('canvas');
    canvas.width = 128;
    canvas.height = 128;
    const context = canvas.getContext('2d');
    context.fillStyle = '#fde2e2'; // Light rose
    context.fillRect(0, 0, canvas.width, canvas.height);
    context.font = 'bold 56px Arial';
    context.fillStyle = '#9f1239'; // Dark rose
    context.textAlign = 'center';
    context.textBaseline = 'middle';
    context.fillText(initials, canvas.width / 2, canvas.height / 2);
    return canvas.toDataURL();
};


// A smaller, reusable OTP modal for inline verification
const VerificationOtpModal = ({ isOpen, onClose, onVerify, verificationType, identifier }) => {
    const [otp, setOtp] = useState('');
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState('');

    if (!isOpen) return null;

    const handleSubmit = async (e) => {
        e.preventDefault();
        setIsLoading(true);
        setError('');
        try {
            await onVerify(otp);
            onClose(); // Close on success
        } catch (err) {
            setError(err.message);
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
            <div className="bg-white p-6 rounded-lg shadow-xl w-full max-w-sm">
                <h3 className="text-lg font-semibold text-center mb-2">Verify {verificationType}</h3>
                <p className="text-center text-sm text-neutral-500 mb-4">Enter the 6-digit code sent to {identifier}</p>
                {error && <p className="text-red-500 text-xs text-center mb-2">{error}</p>}
                <form onSubmit={handleSubmit}>
                    <input
                        type="text"
                        value={otp}
                        onChange={(e) => setOtp(e.target.value)}
                        placeholder="______"
                        maxLength="6"
                        className="w-full text-center text-2xl tracking-[0.5em] border-neutral-300 rounded-md"
                    />
                    <div className="flex gap-4 mt-4">
                        <button type="button" onClick={onClose} className="w-full py-2 px-4 border border-neutral-300 rounded-md text-sm">Cancel</button>
                        <button type="submit" disabled={isLoading} className="w-full py-2 px-4 bg-rose-600 text-white rounded-md text-sm disabled:bg-rose-300">
                            {isLoading ? 'Verifying...' : 'Verify'}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
};


export default function UserProfileSettings() {
    const { user, updateUserProfileInContext } = useAuth();
    const [profile, setProfile] = useState(null);
    const [isLoading, setIsLoading] = useState(true);
    const [formErrors, setFormErrors] = useState({});
    const { showToast } = useToast();

    const [states, setStates] = useState([]);
    const [cities, setCities] = useState([]);
    const uploaderRef = useRef(null);

    const [otpModalInfo, setOtpModalInfo] = useState({ isOpen: false, type: null, value: '' });

    // Regex for validation
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    const phoneRegex = /^[0-9]{10}$/;

    // Fetch initial data (profile and states)
    useEffect(() => {
        const loadInitialData = async () => {
            if (user?.pkid) {
                try {
                    setIsLoading(true);
                    setFormErrors({});
                    const [profileData, statesData] = await Promise.all([
                        apiService.getUserProfile(),
                        apiService.getStates()
                    ]);

                    profileData.profilePhoto = profileData.profilePhoto ? `${API_BASE_URL}${profileData.profilePhoto}` : generateInitialsAvatar(profileData.fullName);
                    
                    setProfile(profileData);
                    setStates(statesData);

                    if (profileData.stateID) {
                        const citiesData = await apiService.getCities(profileData.stateID);
                        setCities(citiesData);
                    }
                } catch (err) {
                    setFormErrors({ general: err.message });
                } finally {
                    setIsLoading(false);
                }
            }
        };
        loadInitialData();
    }, [user]);

    const handleInputChange = async (e) => {
        const { name, value } = e.target;
        setProfile(prev => ({ ...prev, [name]: value }));

        if (name === 'stateID') {
            setCities([]);
            if (value) {
                const citiesData = await apiService.getCities(value);
                setCities(citiesData);
                setProfile(prev => ({ ...prev, cityID: '' }));
            }
        }
    };

    const validateField = (name, value) => {
        let error = '';
        if (name === 'phone' && !phoneRegex.test(value)) {
            error = 'Please enter a valid 10-digit phone number.';
        }
        if (name === 'email' && value && !emailRegex.test(value)) {
            error = 'Please enter a valid email address.';
        }
        setFormErrors(prev => ({ ...prev, [name]: error }));
        return !error;
    };

    const handleVerifyClick = async (type) => {
        const value = profile[type];
        if (!value || !validateField(type, value)) {
            setFormErrors(prev => ({ ...prev, [type]: `Please enter a valid ${type} to verify.` }));
            return;
        }

        try {
            const { result, message } = await apiService.sendVerificationOtp(type, value, 'User');
            if (!result) {
                setFormErrors(prev => ({ ...prev, [type]: message || `Failed to send OTP to ${value}.` }));
                throw new Error(message || `Failed to send OTP to ${value}.`);
            }
            setOtpModalInfo({ isOpen: true, type: type, value: value });
        } catch (err) {
            setFormErrors(prev => ({ ...prev, [type]: err.message }));
        }
    };

    const handleOtpVerify = async (otp) => {
        const { type, value } = otpModalInfo;
        const { result, message } = await apiService.verifyUpdateOtp(type, value, otp, profile.pkID);
        // On success, update profile state to reflect verification
        if (!result) {
            setFormErrors(prev => ({ ...prev, [type]: message || 'OTP verification failed.' }));
            throw new Error(message || 'OTP verification failed.');
        }
        setProfile(prev => ({ ...prev, [`is${type.charAt(0).toUpperCase() + type.slice(1)}Verified`]: true }));
    };

    // Handler for when the crop is finished and confirmed in the modal
    const handlePhotoCropComplete = async (croppedBlob) => {
        if (!croppedBlob) return;

        try {
            setIsLoading(true);

            // 1. Convert blob to base64 for storage and preview
            const base64String = await new Promise((resolve, reject) => {
                const reader = new FileReader();
                reader.onload = () => resolve(reader.result);
                reader.onerror = reject;
                reader.readAsDataURL(croppedBlob);
            });

            // 2. Update local state immediately for instant preview
            setProfile(prev => ({ ...prev, profilePhoto: base64String }));

            
            // 3. Upload to backend (API call)
            // Assuming apiService has an uploadProfilePhoto method
            // If not, you'll need to create one that accepts FormData with the cropped image
            try {
                const response = await apiService.uploadProfilePhoto(base64String);

                // 4. Update global auth context with the new photo URL from backend response
                if (response.photoUrl) {  
                    updateUserProfileInContext({ profilePhoto: `${API_BASE_URL}${response.photoUrl}` });
                } else {
                    // Fallback: use the base64 if backend doesn't return the URL
                    updateUserProfileInContext({ profilePhoto: base64String });
                }

                showToast('Profile photo updated successfully!', 'success');
            } catch (uploadError) {
                console.error("Photo upload to backend failed:", uploadError);
                // Keep the preview but show error
                showToast('Photo preview updated, but failed to save to server.', 'warning');
            }

        } catch (error) {
            console.error("Photo processing failed:", error);
            showToast('Failed to process profile photo.', 'error');
            // Revert the profile state on error
            setProfile(prev => ({ ...prev, profilePhoto: profile.profilePhoto }));
        } finally {
            setIsLoading(false);
        }
    };

    const validateForm = () => {
        const errors = {};
        if (!profile.fullName?.trim()) {
            errors.fullName = "Full Name is required.";
        }
        if (!profile.stateID) {
            errors.stateID = "Please select a state.";
        }
        if (!profile.cityID) {
            errors.cityID = "Please select a city.";
        }
        setFormErrors(errors);
        return Object.keys(errors).length === 0;
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!validateForm()) {
            return; // Stop submission if validation fails
        }
        setIsLoading(true);
        try {
            const response = await apiService.updateUserProfile(profile);
            if(response.message)
                showToast(response.message, 'success');
        } catch (err) {
            setFormErrors({ general: err.message });
        } finally {
            setIsLoading(false);
        }
    };

    if (isLoading && !profile) {
        return <div className="p-6 bg-white rounded-lg shadow-md text-center">Loading Profile...</div>;
    }

    if (!profile) {
        return <div className="p-6 bg-white rounded-lg shadow-md text-center text-red-500">{formErrors.general || "Could not load profile."}</div>;
    }

    return (
        <>
            <VerificationOtpModal
                isOpen={otpModalInfo.isOpen}
                onClose={() => setOtpModalInfo({ isOpen: false, type: null, value: '' })}
                onVerify={handleOtpVerify}
                verificationType={otpModalInfo.type}
                identifier={otpModalInfo.value}
            />
            <div className="bg-white p-6 sm:p-8 rounded-2xl shadow-sm max-w-4xl mx-auto animate-fade-in">
                <h3 className="text-2xl font-bold text-neutral-800 mb-6">Profile Settings</h3>

                <div className="flex flex-col sm:flex-row items-center gap-8 mb-10 border-b border-neutral-100 pb-8">
                    <div className="relative group">
                        <img
                            src={profile.profilePhoto}
                            alt="Profile"
                            className="w-32 h-32 rounded-full object-cover border-4 border-white shadow-md bg-amber-100"
                        />
                        {/* Hover overlay hint - triggering the ref of the crop uploader */}
                        <div className="absolute inset-0 rounded-full bg-black/30 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity cursor-pointer" onClick={() => uploaderRef.current?.triggerFileSelect()}>
                            <svg xmlns="http://www.w3.org/2000/svg" className="h-8 w-8 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 9a2 2 0 012-2h.93a2 2 0 001.664-.89l.812-1.22A2 2 0 0110.07 4h3.86a2 2 0 011.664.89l.812 1.22A2 2 0 0018.07 7H19a2 2 0 012 2v9a2 2 0 01-2 2H5a2 2 0 01-2-2V9z" />
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 13a3 3 0 11-6 0 3 3 0 016 0z" />
                            </svg>
                        </div>
                    </div>

                    <div className="text-center sm:text-left">
                        <h2 className="text-xl font-bold text-neutral-800">{profile.fullName}</h2>
                        <p className="text-neutral-500 mb-4">{profile.email}</p>

                        {/* Use the existing ImageCropUploader component. */}
                        <ImageCropUploader
                            ref={uploaderRef}
                            onCropComplete={handlePhotoCropComplete}
                            aspect={1} // Circular/Square aspect ratio
                        />

                        <button
                            type="button"
                            onClick={() => uploaderRef.current?.triggerFileSelect()}
                            className="px-4 py-2 bg-white border border-neutral-300 rounded-md text-sm font-medium text-neutral-700 hover:bg-neutral-50 transition-colors shadow-sm"
                        >
                            Change Photo
                        </button>
                        <p className="text-xs text-neutral-400 mt-2">
                            JPG, GIF or PNG. Max size of 800K
                        </p>
                    </div>
                </div>

                <form onSubmit={handleSubmit}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-x-6 gap-y-4">
                        {/* Full Name */}
                        <div>
                            <label htmlFor="fullName" className="block text-sm font-medium text-neutral-700 mb-1">Full Name</label>
                            <input type="text" name="fullName" value={profile.fullName} onChange={handleInputChange} className="w-full px-3 py-2 border border-neutral-300 rounded-md" />
                            {formErrors.fullName && <p className="text-xs text-red-600 mt-1">{formErrors.fullName}</p>}
                        </div>

                        {/* Phone Number */}
                        <div>
                            <label htmlFor="phone" className="block text-sm font-medium text-neutral-700 mb-1">Phone Number</label>
                            <div className="flex gap-2">
                                <input type="tel" name="phone" value={profile.phone} onChange={handleInputChange} onBlur={(e) => validateField('phone', e.target.value)} disabled={profile.isPhoneVerified} className="w-full px-3 py-2 border border-neutral-300 rounded-md disabled:bg-neutral-100" />
                                {profile.isPhoneVerified ? (
                                    <span className="flex-shrink-0 text-green-600 px-3 py-2 rounded-md text-sm font-medium flex items-center gap-1"><svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" viewBox="0 0 20 20" fill="currentColor"><path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" /></svg> Verified</span>
                                ) : (
                                    <button type="button" onClick={() => handleVerifyClick('phone')} disabled={!phoneRegex.test(profile.phone)} className="flex-shrink-0 bg-amber-500 text-white px-3 py-2 rounded-md text-sm font-medium hover:bg-amber-600 disabled:bg-amber-300 disabled:cursor-not-allowed">Verify</button>
                                )}
                            </div>
                            {formErrors.phone && <p className="text-xs text-red-600 mt-1">{formErrors.phone}</p>}
                        </div>

                        {/* Email Address */}
                        <div>
                            <label htmlFor="email" className="block text-sm font-medium text-neutral-700 mb-1">Email Address</label>
                            <div className="flex gap-2">
                                <input type="email" name="email" value={profile.email} onChange={handleInputChange} onBlur={(e) => validateField('email', e.target.value)} disabled={profile.isEmailVerified} className="w-full px-3 py-2 border border-neutral-300 rounded-md disabled:bg-neutral-100" />
                                {profile.isEmailVerified ? (
                                    <span className="flex-shrink-0 text-green-600 px-3 py-2 rounded-md text-sm font-medium flex items-center gap-1"><svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" viewBox="0 0 20 20" fill="currentColor"><path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" /></svg> Verified</span>
                                ) : (
                                    <button type="button" onClick={() => handleVerifyClick('email')} disabled={!profile.email || !emailRegex.test(profile.email)} className="flex-shrink-0 bg-amber-500 text-white px-3 py-2 rounded-md text-sm font-medium hover:bg-amber-600 disabled:bg-amber-300 disabled:cursor-not-allowed">Verify</button>
                                )}
                            </div>
                            {formErrors.email && <p className="text-xs text-red-600 mt-1">{formErrors.email}</p>}
                        </div>

                        {/* State Dropdown */}
                        <div>
                            <label htmlFor="stateID" className="block text-sm font-medium text-neutral-700 mb-1">State</label>
                            <select name="stateID" value={profile.stateID || ''} onChange={handleInputChange} className="w-full px-3 py-2 border border-neutral-300 rounded-md" >
                                <option value="">Select State</option>
                                {states.map(s => <option key={s.stateID} value={s.stateID}>{s.stateName}</option>)}
                            </select>
                            {formErrors.stateID && <p className="text-xs text-red-600 mt-1">{formErrors.stateID}</p>}
                        </div>

                        {/* City Dropdown */}
                        <div>
                            <label htmlFor="cityID" className="block text-sm font-medium text-neutral-700 mb-1">City</label>
                            <select name="cityID" value={profile.cityID || ''} onChange={handleInputChange} className="w-full px-3 py-2 border border-neutral-300 rounded-md" disabled={!profile.stateID || cities.length === 0}>
                                <option value="">Select City</option>
                                {cities.map(c => <option key={c.cityID} value={c.cityID}>{c.cityName}</option>)}
                            </select>
                            {formErrors.cityID && <p className="text-xs text-red-600 mt-1">{formErrors.cityID}</p>}
                        </div>

                        {/* About Me */}
                        <div className="md:col-span-2">
                            <label htmlFor="description" className="block text-sm font-medium text-neutral-700 mb-1">About Me</label>
                            <textarea name="description" rows="3" value={profile.description || ''} onChange={handleInputChange} className="w-full px-3 py-2 border border-neutral-300 rounded-md"></textarea>
                        </div>
                    </div>

                    <div className="mt-8 text-right">
                        <button type="submit" disabled={isLoading} className="bg-rose-600 text-white px-6 py-2 rounded-md text-sm font-medium hover:bg-rose-700 disabled:bg-rose-300">
                            {isLoading ? 'Saving...' : 'Save Changes'}
                        </button>
                    </div>
                </form>
            </div>
        </>
    );
}