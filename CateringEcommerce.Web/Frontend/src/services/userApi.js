const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:44368';


import { fetchApi } from './apiUtils';


export const apiService = {

    // Auth endpoints
    sendOtp: (currentAction, phoneNumber, isPartnerLogin) => fetchApi('/api/User/Auth/send-otp', 'POST', { currentAction, phoneNumber, isPartnerLogin }),

    /**
    * Verifies the OTP. If a name is provided, it's a signup verification.
    * @param {string} phoneNumber - The phone number.
    * @param {string} otp - The OTP code.
    * @param {string|null} name - The user's full name for signups.
    * @returns {Promise<object>}
    */
    verifyOtp: async (currentAction, phoneNumber, name, otp, isPartnerLogin ) => {
        const response = await fetch(`${API_BASE_URL}/api/User/Auth/verify-otp`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ currentAction, phoneNumber, name, otp, isPartnerLogin }),
        });
        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.message || 'Invalid OTP');
        }
        return response.json(); // Should return { token, user }
    },


    // Login endpoint
    finalVerify: async (userPKID) => fetchApi(`/User/Auth/final-verify?userPKID=${userPKID}`, 'POST'),
    
    // Profile endpoints
    getUserProfile: (userPKID) => fetchApi(`/User/ProfileSettings/GetUserProfile?userPKID=${userPKID}`),
    updateUserProfile: (userPKID, profileData) => fetchApi(`/User/ProfileSettings/UpdateProfile/${userPKID}`, 'POST', profileData),

    // Verification endpoints
    sendVerificationOtp: (type, value, role) => fetchApi('/Common/Auth/send-otp', 'POST', { type, value, role }),
    verifyUpdateOtp: (type, value, otp, pkId, role) => fetchApi('/Common/Auth/verify-otp', 'POST', { type, value, otp, pkId, role }),

    
    // Location endpoints
    getStates: () => fetchApi('/Common/Locations'),
    getCities: (stateID) => fetchApi(`/Common/Locations/cities/${stateID}`),

    // Google Auth endpoint
    getGoogleAuthUrl: async () => {
        const response = await fetch(`${API_BASE_URL}/api/auth/google/login`);
        return handleResponse(response);
    },

};
