import React, { useState, useEffect } from 'react';
import { useAuth } from '../../contexts/AuthContext';
import { apiService } from '../../services/userApi'; // Import the API service

// Utility function to generate an avatar from initials
const generateInitialsAvatar = (name) => {
    if (!name) return 'https://placehold.co/64x64/E0E7FF/4338CA?text=Q';
    const initials = name.split(' ').map(n => n[0]).slice(0, 2).join('').toUpperCase();
    const svg = `<svg xmlns="http://www.w3.org/2000/svg" width="128" height="128" viewBox="0 0 128 128">
                    <circle cx="64" cy="64" r="64" fill="#e0e7ff"/>
                    <text x="50%" y="50%" dominant-baseline="central" text-anchor="middle" font-family="sans-serif" font-size="56" fill="#4338ca">${initials}</text>
                 </svg>`;
    return `data:image/svg+xml;base64,${btoa(svg)}`;
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


export default function ProfileSettings() {
    const { user, updateUserProfileInContext } = useAuth();
    const [profile, setProfile] = useState(null);
    const [isLoading, setIsLoading] = useState(true);
    const [formErrors, setFormErrors] = useState({});

    const [states, setStates] = useState([]);
    const [cities, setCities] = useState([]);

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
                        apiService.getUserProfile(user.pkid),
                        apiService.getStates()
                    ]);

                    if (!profileData.profilePhoto) {
                        profileData.profilePhoto = generateInitialsAvatar(profileData.fullName);
                    }
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

    const handlePhotoUpload = (e) => {
        const file = e.target.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = (event) => {
                setProfile(prev => ({ ...prev, profilePhoto: event.target.result }));
            };
            reader.readAsDataURL(file);
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
            await apiService.sendVerificationOtp(type, value, 'User');
            setOtpModalInfo({ isOpen: true, type: type, value: value });
        } catch (err) {
            setFormErrors(prev => ({ ...prev, [type]: err.message }));
        }
    };

    const handleOtpVerify = async (otp) => {
        const { type, value } = otpModalInfo;
        await apiService.verifyUpdateOtp(type, value, otp, profile.pkID);
        // On success, update profile state to reflect verification
        setProfile(prev => ({ ...prev, [`is${type.charAt(0).toUpperCase() + type.slice(1)}Verified`]: true }));
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
            const response = await apiService.updateUserProfile(user.pkid, profile);
            // Update the global context so the header photo changes immediately
            updateUserProfileInContext(response.user);
            alert("Profile updated successfully!");
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
            <div className="p-6 bg-white rounded-lg shadow-md">
                <h3 className="text-2xl font-bold text-neutral-800 mb-6">Profile Settings</h3>

                <div className="flex items-center gap-6 mb-8">
                    <img src={profile.profilePhoto} alt="Profile" className="h-24 w-24 rounded-full object-cover bg-amber-100" />
                    <div>
                        <label htmlFor="photo-upload" className="cursor-pointer bg-rose-600 text-white px-4 py-2 rounded-md text-sm font-medium hover:bg-rose-700">
                            Upload New Photo
                        </label>
                        <input type="file" id="photo-upload" className="hidden" accept="image/*" onChange={handlePhotoUpload} />
                        <p className="text-xs text-neutral-500 mt-2">Recommended size: 200x200px</p>
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
                            <select name="stateID" value={profile.stateID} onChange={handleInputChange} className="w-full px-3 py-2 border border-neutral-300 rounded-md" required>
                                <option value="">Select State</option>
                                {states.map(s => <option key={s.stateID} value={s.stateID}>{s.stateName}</option>)}
                            </select>
                            {formErrors.stateID && <p className="text-xs text-red-600 mt-1">{formErrors.stateID}</p>}
                        </div>

                        {/* City Dropdown */}
                        <div>
                            <label htmlFor="cityID" className="block text-sm font-medium text-neutral-700 mb-1">City</label>
                            <select name="cityID" value={profile.cityID} onChange={handleInputChange} className="w-full px-3 py-2 border border-neutral-300 rounded-md" required disabled={!profile.stateID || cities.length === 0}>
                                <option value="">Select City</option>
                                {cities.map(c => <option key={c.cityID} value={c.cityID}>{c.cityName}</option>)}
                            </select>
                            {formErrors.cityID && <p className="text-xs text-red-600 mt-1">{formErrors.cityID}</p>}
                        </div>

                        {/* About Me */}
                        <div className="md:col-span-2">
                            <label htmlFor="description" className="block text-sm font-medium text-neutral-700 mb-1">About Me</label>
                            <textarea name="description" rows="3" value={profile.description} onChange={handleInputChange} className="w-full px-3 py-2 border border-neutral-300 rounded-md"></textarea>
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