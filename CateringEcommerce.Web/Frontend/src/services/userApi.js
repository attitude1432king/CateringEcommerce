const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:44368';


import { fetchApi } from './apiUtils';
const token = localStorage.getItem('authToken');

export const apiService = {

    // Auth endpoints
    
    /**
     * Send the OTP. and check the role data is exist or not.
     * @param {string} currentAction - The currentAction Mode.
     * @param {string} phoneNumber - The phone number.
     * @param {bool|false} isPartnerLogin - Is Partner or User Login flag.
     * @returns {Promise<object>}
     */

    sendOtp: async (currentAction, phoneNumber, isPartnerLogin) => {
        const response = await fetch(`${API_BASE_URL}/api/User/Auth/send-otp`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ currentAction, phoneNumber, isPartnerLogin }),
        });
        if (!response.ok) {
            const errorData = await response.json();
            return errorData;
        }
        return response.json();
    },

    /**
    * Verifies the OTP. If a name is provided, it's a signup verification.
    * @param {string} phoneNumber - The phone number.
    * @param {string} otp - The OTP code.
    * @param {string|null} name - The user's full name for signups.
    * @param {bool|false} isPartnerLogin - Is Partner or User Login flag.
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
            return errorData;
        }
        return response.json(); // Should return { token, user }
    },

    // Login endpoint
    finalVerify: async (userPKID, role, token) => {
        const response = await fetch(`${API_BASE_URL}/api/User/Auth/final-verify`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify({userPKID, role}),
        });
        if (!response.ok) {
            const errorData = await response.json();
            return errorData;
        }
        return response.json();
    },

    //finalVerify: async (userPKID, role) => fetchApi(`/User/Auth/final-verify`, 'POST', {userPKID, role}),
    
    // Profile endpoints
    getUserProfile: () => fetchApi(`/User/ProfileSettings/GetUserProfile`),
    updateUserProfile: (profileData) => fetchApi(`/User/ProfileSettings/UpdateProfile`, 'POST', profileData),
    uploadProfilePhoto: (profilePhoto) => fetchApi(`/User/ProfileSettings/UploadProfilePhoto`, 'POST', profilePhoto),

    // Verification endpoints
    sendVerificationOtp: async (type, value, role) => {
        const response = await fetch(`${API_BASE_URL}/api/Common/Auth/send-otp`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify({ type, value, role }),
        });
        if (!response.ok) {
            const errorData = await response.json();
            return errorData;
        }
        return response.json();
    },

    verifyUpdateOtp: async (type, value, otp, pkId, role) => {
        const response = await fetch(`${API_BASE_URL}/api/Common/Auth/verify-otp`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify({ type, value, otp, pkId, role }),
        });
        if (!response.ok) {
            const errorData = await response.json();
            return errorData;
        }
        return response.json();
    },
    
    // Location endpoints
    getStates: () => fetchApi('/Common/Locations/states'),
    getCities: (stateID) => fetchApi(`/Common/Locations/cities/${stateID}`),

    // Google Auth endpoint
    getGoogleAuthUrl: async () => {
        const response = await fetch(`${API_BASE_URL}/api/auth/google/login`);
        return response;
    },

};
