const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;


import { fetchApi } from './apiUtils';

export const apiService = {

    // Auth endpoints

    /**
     * Send the OTP. and check the role data is exist or not.
     * @param {string} currentAction - The currentAction Mode.
     * @param {string} phoneNumber - The phone number.
     * @param {bool|false} isPartnerLogin - Is Partner or User Login flag.
     * @param {string} deviceFingerprint - Device fingerprint for 2FA/device tracking.
     * @returns {Promise<object>}
     */

    sendOtp: async (currentAction, phoneNumber, isPartnerLogin, deviceFingerprint = null) => {
        const response = await fetch(`${API_BASE_URL}/api/User/Auth/send-otp`, {
            method: 'POST',
            credentials: 'include',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ currentAction, phoneNumber, isPartnerLogin, deviceFingerprint }),
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
    * @param {object} deviceInfo - Device information for 2FA (fingerprint, trustDevice, browser, os).
    * @returns {Promise<object>}
    */
    verifyOtp: async (currentAction, phoneNumber, name, otp, isPartnerLogin, deviceInfo = {}) => {
        const response = await fetch(`${API_BASE_URL}/api/User/Auth/verify-otp`, {
            method: 'POST',
            credentials: 'include',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                currentAction,
                phoneNumber,
                name,
                otp,
                isPartnerLogin,
                deviceFingerprint: deviceInfo.deviceFingerprint || null,
                trustDevice: deviceInfo.trustDevice || false,
                browser: deviceInfo.browser || null,
                os: deviceInfo.os || null
            }),
        });
        if (!response.ok) {
            const errorData = await response.json();
            return errorData;
        }
        return response.json();
    },

    // Login endpoint
    finalVerify: async (userPKID, role) => {
        const response = await fetch(`${API_BASE_URL}/api/User/Auth/final-verify`, {
            method: 'POST',
            credentials: 'include',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({userPKID, role}),
        });
        if (!response.ok) {
            const errorData = await response.json();
            return errorData;
        }
        return response.json();
    },

    // Profile endpoints
    getUserProfile: () => fetchApi(`/User/ProfileSettings/GetUserProfile`),
    updateUserProfile: (profileData) => fetchApi(`/User/ProfileSettings/UpdateProfile`, 'POST', profileData),
    uploadProfilePhoto: (file) => {
        const fd = new FormData();
        fd.append('profilePhoto', file, file.name || 'profile.jpg');
        return fetchApi(`/User/ProfileSettings/UploadProfilePhoto`, 'POST', fd);
    },

    // Verification endpoints
    sendVerificationOtp: async (type, value, role) => {
        const response = await fetch(`${API_BASE_URL}/api/Common/Auth/send-otp`, {
            method: 'POST',
            credentials: 'include',
            headers: { 'Content-Type': 'application/json' },
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
            credentials: 'include',
            headers: { 'Content-Type': 'application/json' },
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

    // ===================================
    // DEVICE MANAGEMENT (2FA/Trusted Devices)
    // ===================================

    /**
     * Get all trusted devices for the authenticated user
     * @returns {Promise<object>}
     */
    getTrustedDevices: async () => {
        const response = await fetch(`${API_BASE_URL}/api/User/Auth/trusted-devices`, {
            method: 'GET',
            credentials: 'include',
            headers: { 'Content-Type': 'application/json' }
        });
        if (!response.ok) {
            const errorData = await response.json();
            return errorData;
        }
        return response.json();
    },

    /**
     * Revoke a specific trusted device
     * @param {number} deviceId - The device ID to revoke
     * @param {string} reason - Reason for revocation
     * @returns {Promise<object>}
     */
    revokeTrustedDevice: async (deviceId, reason = 'User revoked') => {
        const response = await fetch(`${API_BASE_URL}/api/User/Auth/trusted-devices/${deviceId}`, {
            method: 'DELETE',
            credentials: 'include',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ reason })
        });
        if (!response.ok) {
            const errorData = await response.json();
            return errorData;
        }
        return response.json();
    },

    /**
     * Revoke all trusted devices (security measure)
     * @param {string} reason - Reason for revocation
     * @returns {Promise<object>}
     */
    revokeAllTrustedDevices: async (reason = 'Security measure') => {
        const response = await fetch(`${API_BASE_URL}/api/User/Auth/revoke-all-devices`, {
            method: 'POST',
            credentials: 'include',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ reason })
        });
        if (!response.ok) {
            const errorData = await response.json();
            return errorData;
        }
        return response.json();
    },

    // Contact Us
    submitContact: async (data) => {
        const response = await fetch(`${API_BASE_URL}/api/contact`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data),
        });
        return response.json();
    },

};
