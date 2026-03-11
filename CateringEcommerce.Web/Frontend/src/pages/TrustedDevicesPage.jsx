import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { apiService } from '../services/userApi';
import { getOrGenerateFingerprint } from '../utils/deviceFingerprint';
import { Monitor, Smartphone, Tablet, Chrome, Globe, AlertTriangle, Shield, X, CheckCircle, Clock, Calendar } from 'lucide-react';

const TrustedDevicesPage = () => {
    const navigate = useNavigate();
    const [devices, setDevices] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [successMessage, setSuccessMessage] = useState(null);
    const [currentFingerprint, setCurrentFingerprint] = useState(null);
    const [revoking, setRevoking] = useState(null); // Track which device is being revoked

    // Confirmation modals state
    const [revokeConfirmation, setRevokeConfirmation] = useState({
        isOpen: false,
        deviceId: null,
        deviceName: ''
    });
    const [revokeAllConfirmation, setRevokeAllConfirmation] = useState(false);

    useEffect(() => {
        fetchDevices();
        initCurrentFingerprint();
    }, []);

    const initCurrentFingerprint = async () => {
        try {
            const fingerprint = await getOrGenerateFingerprint();
            setCurrentFingerprint(fingerprint);
        } catch (error) {
            console.error('Failed to get device fingerprint:', error);
        }
    };

    const fetchDevices = async () => {
        setLoading(true);
        setError(null);
        try {
            const response = await apiService.getTrustedDevices();
            if (response.result) {
                setDevices(response.data || []);
            } else {
                setError(response.message || 'Failed to load trusted devices');
            }
        } catch (error) {
            setError('An error occurred while loading your trusted devices');
        } finally {
            setLoading(false);
        }
    };

    const handleRevoke = (deviceId, deviceName) => {
        setRevokeConfirmation({
            isOpen: true,
            deviceId,
            deviceName
        });
    };

    const confirmRevokeDevice = async () => {
        const { deviceId, deviceName } = revokeConfirmation;
        setRevokeConfirmation({ isOpen: false, deviceId: null, deviceName: '' });
        setRevoking(deviceId);
        try {
            const response = await apiService.revokeTrustedDevice(deviceId, 'User revoked');
            if (response.result) {
                setSuccessMessage(`Device "${deviceName}" revoked successfully`);
                await fetchDevices();

                // Clear success message after 3 seconds
                setTimeout(() => setSuccessMessage(null), 3000);
            } else {
                alert(response.message || 'Failed to revoke device');
            }
        } catch (error) {
            alert(error.message || 'Failed to revoke device');
        } finally {
            setRevoking(null);
        }
    };

    const handleRevokeAll = () => {
        const activeDevicesCount = devices.filter(d => d.isActive && !d.isExpired).length;

        if (activeDevicesCount === 0) {
            alert('No active devices to revoke');
            return;
        }

        setRevokeAllConfirmation(true);
    };

    const confirmRevokeAll = async () => {
        setRevokeAllConfirmation(false);
        setLoading(true);
        try {
            const response = await apiService.revokeAllTrustedDevices('Security measure - User requested');
            if (response.result) {
                setSuccessMessage(`${response.devicesRevoked || activeDevicesCount} device(s) revoked successfully`);
                await fetchDevices();

                // Clear success message after 3 seconds
                setTimeout(() => setSuccessMessage(null), 3000);
            } else {
                alert(response.message || 'Failed to revoke all devices');
            }
        } catch (error) {
            alert(error.message || 'Failed to revoke all devices');
        } finally {
            setLoading(false);
        }
    };

    const getDeviceIcon = (browser, os) => {
        // Mobile devices
        if (os?.toLowerCase().includes('android') || os?.toLowerCase().includes('ios')) {
            return <Smartphone className="w-6 h-6 text-blue-600" />;
        }
        // Tablets
        if (os?.toLowerCase().includes('ipad') || browser?.toLowerCase().includes('tablet')) {
            return <Tablet className="w-6 h-6 text-purple-600" />;
        }
        // Desktop
        return <Monitor className="w-6 h-6 text-gray-600" />;
    };

    const getBrowserIcon = (browser) => {
        const browserLower = browser?.toLowerCase() || '';
        if (browserLower.includes('chrome')) {
            return <Chrome className="w-4 h-4 text-green-600" />;
        }
        return <Globe className="w-4 h-4 text-gray-600" />;
    };

    const formatDate = (dateString) => {
        const date = new Date(dateString);
        return date.toLocaleDateString('en-IN', {
            day: 'numeric',
            month: 'short',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    };

    const formatRelativeTime = (dateString) => {
        const date = new Date(dateString);
        const now = new Date();
        const diffMs = date - now;
        const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));

        if (diffDays < 0) {
            return 'Expired';
        } else if (diffDays === 0) {
            return 'Expires today';
        } else if (diffDays === 1) {
            return 'Expires tomorrow';
        } else if (diffDays < 7) {
            return `Expires in ${diffDays} days`;
        } else {
            const weeks = Math.floor(diffDays / 7);
            return `Expires in ${weeks} week${weeks > 1 ? 's' : ''}`;
        }
    };

    const isCurrentDevice = (deviceFingerprint) => {
        return deviceFingerprint === currentFingerprint;
    };

    if (loading && devices.length === 0) {
        return (
            <div className="min-h-screen bg-gray-100 flex items-center justify-center">
                <div className="text-center">
                    <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-rose-500 mx-auto"></div>
                    <p className="mt-4 text-gray-600">Loading trusted devices...</p>
                </div>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-gray-100 py-8">
            <div className="max-w-5xl mx-auto px-4">
                {/* Header */}
                <div className="mb-6">
                    <button
                        onClick={() => navigate('/profile')}
                        className="mb-4 flex items-center gap-2 text-rose-600 hover:text-rose-700 font-medium transition-colors"
                    >
                        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
                        </svg>
                        Back to Profile
                    </button>

                    <div className="flex justify-between items-start">
                        <div>
                            <h1 className="text-3xl font-bold text-gray-900 mb-2">Trusted Devices</h1>
                            <p className="text-gray-600">
                                Manage devices that can skip two-factor authentication for 30 days
                            </p>
                        </div>
                        {devices.filter(d => d.isActive && !d.isExpired).length > 0 && (
                            <button
                                onClick={handleRevokeAll}
                                className="bg-red-500 text-white px-4 py-2 rounded-lg hover:bg-red-600 transition-colors font-medium flex items-center gap-2"
                                disabled={loading}
                            >
                                <AlertTriangle className="w-4 h-4" />
                                Revoke All Devices
                            </button>
                        )}
                    </div>
                </div>

                {/* Success Message */}
                {successMessage && (
                    <div className="mb-6 bg-green-50 border-2 border-green-300 text-green-800 px-6 py-4 rounded-lg flex items-center gap-3 animate-fadeIn">
                        <CheckCircle className="w-6 h-6 flex-shrink-0" />
                        <p className="font-semibold">{successMessage}</p>
                    </div>
                )}

                {/* Error Message */}
                {error && (
                    <div className="bg-red-50 border border-red-200 text-red-800 px-4 py-3 rounded-lg mb-6">
                        {error}
                    </div>
                )}

                {/* Security Notice */}
                <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-6">
                    <div className="flex items-start gap-3">
                        <Shield className="w-5 h-5 text-blue-700 flex-shrink-0 mt-0.5" />
                        <div className="text-sm text-blue-900">
                            <p className="font-medium mb-1">About Trusted Devices</p>
                            <p>
                                Trusted devices allow you to skip OTP verification for 30 days. You can revoke device trust at any time
                                for security. Your current device is highlighted below.
                            </p>
                        </div>
                    </div>
                </div>

                {/* Devices List */}
                {devices.length === 0 && !loading ? (
                    <div className="bg-white rounded-lg p-12 text-center shadow-sm">
                        <Shield className="w-16 h-16 mx-auto text-gray-300 mb-4" />
                        <h2 className="text-xl font-semibold mb-2">No Trusted Devices</h2>
                        <p className="text-gray-600 mb-6">
                            When you login and select "Trust this device for 30 days", it will appear here
                        </p>
                        <button
                            onClick={() => navigate('/profile')}
                            className="inline-flex items-center gap-2 bg-rose-500 text-white px-6 py-2 rounded-lg hover:bg-rose-600 transition-colors"
                        >
                            Go to Profile
                        </button>
                    </div>
                ) : (
                    <div className="space-y-4">
                        {devices.map((device) => {
                            const isCurrent = isCurrentDevice(device.deviceFingerprint);
                            const canRevoke = device.isActive && !device.isExpired;

                            return (
                                <div
                                    key={device.deviceId}
                                    className={`bg-white rounded-lg p-6 shadow-sm hover:shadow-md transition-shadow border-2 ${
                                        isCurrent ? 'border-rose-500 bg-rose-50' : 'border-gray-200'
                                    } ${!canRevoke ? 'opacity-60' : ''}`}
                                >
                                    <div className="flex items-start justify-between">
                                        {/* Device Info */}
                                        <div className="flex items-start gap-4 flex-1">
                                            {/* Device Icon */}
                                            <div className="flex-shrink-0 mt-1">
                                                {getDeviceIcon(device.browser, device.os)}
                                            </div>

                                            {/* Device Details */}
                                            <div className="flex-1">
                                                <div className="flex items-center gap-2 mb-1">
                                                    <h3 className="text-lg font-semibold text-gray-800">
                                                        {device.deviceName || 'Unknown Device'}
                                                    </h3>
                                                    {isCurrent && (
                                                        <span className="px-2 py-0.5 bg-rose-500 text-white text-xs font-medium rounded-full">
                                                            Current Device
                                                        </span>
                                                    )}
                                                    {!device.isActive && (
                                                        <span className="px-2 py-0.5 bg-gray-400 text-white text-xs font-medium rounded-full">
                                                            Revoked
                                                        </span>
                                                    )}
                                                    {device.isExpired && device.isActive && (
                                                        <span className="px-2 py-0.5 bg-yellow-500 text-white text-xs font-medium rounded-full">
                                                            Expired
                                                        </span>
                                                    )}
                                                </div>

                                                <div className="space-y-1 text-sm text-gray-600">
                                                    {/* Browser & OS */}
                                                    <div className="flex items-center gap-2">
                                                        {getBrowserIcon(device.browser)}
                                                        <span>{device.browser || 'Unknown Browser'} on {device.os || 'Unknown OS'}</span>
                                                    </div>

                                                    {/* Trusted Date */}
                                                    <div className="flex items-center gap-2">
                                                        <Calendar className="w-4 h-4 text-gray-500" />
                                                        <span>Trusted: {formatDate(device.trustedDate)}</span>
                                                    </div>

                                                    {/* Last Used */}
                                                    {device.lastUsed && (
                                                        <div className="flex items-center gap-2">
                                                            <Clock className="w-4 h-4 text-gray-500" />
                                                            <span>Last used: {formatDate(device.lastUsed)}</span>
                                                        </div>
                                                    )}

                                                    {/* Expiry */}
                                                    {device.isActive && !device.isExpired && (
                                                        <div className="flex items-center gap-2">
                                                            <Shield className="w-4 h-4 text-green-600" />
                                                            <span className="text-green-700 font-medium">
                                                                {formatRelativeTime(device.expiresAt)} ({device.daysUntilExpiry} days)
                                                            </span>
                                                        </div>
                                                    )}

                                                    {/* IP Address */}
                                                    {device.ipAddress && (
                                                        <div className="flex items-center gap-2 text-xs">
                                                            <Globe className="w-3 h-3 text-gray-400" />
                                                            <span className="text-gray-500">IP: {device.ipAddress}</span>
                                                        </div>
                                                    )}

                                                    {/* Revoked Info */}
                                                    {device.revokedDate && (
                                                        <div className="flex items-center gap-2 text-xs text-red-600">
                                                            <X className="w-3 h-3" />
                                                            <span>
                                                                Revoked: {formatDate(device.revokedDate)}
                                                                {device.revokedReason && ` (${device.revokedReason})`}
                                                            </span>
                                                        </div>
                                                    )}
                                                </div>
                                            </div>
                                        </div>

                                        {/* Action Button */}
                                        <div className="flex-shrink-0 ml-4">
                                            {canRevoke ? (
                                                <button
                                                    onClick={() => handleRevoke(device.deviceId, device.deviceName)}
                                                    disabled={revoking === device.deviceId}
                                                    className="px-4 py-2 text-sm bg-red-500 text-white rounded-lg hover:bg-red-600 transition-colors flex items-center gap-2 font-medium disabled:opacity-50 disabled:cursor-not-allowed"
                                                >
                                                    {revoking === device.deviceId ? (
                                                        <>
                                                            <svg className="animate-spin h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                                                                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                                                                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                                                            </svg>
                                                            Revoking...
                                                        </>
                                                    ) : (
                                                        <>
                                                            <X className="w-4 h-4" />
                                                            Revoke
                                                        </>
                                                    )}
                                                </button>
                                            ) : (
                                                <div className="text-xs text-gray-500 text-center px-4 py-2 border border-gray-300 rounded-lg bg-gray-50">
                                                    {device.isExpired ? 'Expired' : 'Revoked'}
                                                </div>
                                            )}
                                        </div>
                                    </div>
                                </div>
                            );
                        })}
                    </div>
                )}

                {/* Info Section */}
                <div className="mt-8 bg-gray-50 rounded-lg p-6">
                    <h3 className="font-semibold text-gray-900 mb-3">Security Tips</h3>
                    <div className="space-y-2 text-sm text-gray-700">
                        <p>
                            <strong>Trusted Devices:</strong> Devices you mark as trusted will skip OTP verification for 30 days.
                        </p>
                        <p>
                            <strong>Automatic Expiry:</strong> After 30 days, you'll need to verify again even on trusted devices.
                        </p>
                        <p>
                            <strong>Revoke Access:</strong> If you lose a device or suspect unauthorized access, revoke it immediately.
                        </p>
                        <p>
                            <strong>Best Practice:</strong> Only trust your personal devices. Don't trust shared or public computers.
                        </p>
                    </div>
                </div>
            </div>

            {/* Revoke Single Device Confirmation Modal */}
            {revokeConfirmation.isOpen && (
                <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4">
                    <div className="bg-white rounded-xl shadow-2xl max-w-md w-full p-6 animate-fade-in">
                        <div className="text-center mb-6">
                            <div className="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-yellow-100 mb-4">
                                <AlertTriangle className="h-6 w-6 text-yellow-600" />
                            </div>
                            <h3 className="text-lg font-semibold text-gray-900 mb-2">Revoke Device?</h3>
                            <p className="text-sm text-gray-600">
                                Are you sure you want to revoke "{revokeConfirmation.deviceName}"? You will need to verify again on next login from this device.
                            </p>
                        </div>

                        <div className="flex gap-3">
                            <button
                                onClick={() => setRevokeConfirmation({ isOpen: false, deviceId: null, deviceName: '' })}
                                className="flex-1 px-4 py-2.5 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-colors font-medium"
                            >
                                Cancel
                            </button>
                            <button
                                onClick={confirmRevokeDevice}
                                className="flex-1 px-4 py-2.5 bg-yellow-600 text-white rounded-lg hover:bg-yellow-700 transition-colors font-medium"
                            >
                                Revoke Device
                            </button>
                        </div>
                    </div>
                </div>
            )}

            {/* Revoke All Devices Confirmation Modal */}
            {revokeAllConfirmation && (
                <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4">
                    <div className="bg-white rounded-xl shadow-2xl max-w-md w-full p-6 animate-fade-in">
                        <div className="text-center mb-6">
                            <div className="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-red-100 mb-4">
                                <Shield className="h-6 w-6 text-red-600" />
                            </div>
                            <h3 className="text-lg font-semibold text-gray-900 mb-2">Revoke All Devices?</h3>
                            <p className="text-sm text-gray-600">
                                Are you sure you want to revoke ALL {devices.filter(d => d.isActive && !d.isExpired).length} trusted device(s)? You will need to verify on all devices on next login.
                            </p>
                        </div>

                        <div className="flex gap-3">
                            <button
                                onClick={() => setRevokeAllConfirmation(false)}
                                className="flex-1 px-4 py-2.5 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-colors font-medium"
                            >
                                Cancel
                            </button>
                            <button
                                onClick={confirmRevokeAll}
                                className="flex-1 px-4 py-2.5 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors font-medium"
                            >
                                Revoke All
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default TrustedDevicesPage;
