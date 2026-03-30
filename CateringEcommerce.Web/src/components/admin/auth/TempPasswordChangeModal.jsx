import { useState, useEffect, useCallback } from 'react';
import { Lock, Eye, EyeOff, CheckCircle, XCircle, AlertTriangle } from 'lucide-react';
import { useAdminAuth } from '../../../contexts/AdminAuthContext';
import { authApi } from '../../../services/adminApi';

const STRENGTH_RULES = [
    { label: 'At least 10 characters', test: (p) => p.length >= 10 },
    { label: 'Uppercase letter', test: (p) => /[A-Z]/.test(p) },
    { label: 'Lowercase letter', test: (p) => /[a-z]/.test(p) },
    { label: 'Number', test: (p) => /\d/.test(p) },
    { label: 'Special character (!@#$%…)', test: (p) => /[^A-Za-z0-9]/.test(p) },
];

const PasswordInput = ({ id, label, value, onChange, show, onToggle, autoFocus }) => (
    <div>
        <label htmlFor={id} className="block text-sm font-medium text-gray-700 mb-1">
            {label}
        </label>
        <div className="relative">
            <Lock className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
            <input
                id={id}
                type={show ? 'text' : 'password'}
                value={value}
                onChange={onChange}
                autoFocus={autoFocus}
                autoComplete="new-password"
                className="w-full pl-9 pr-10 py-2.5 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                placeholder={`Enter ${label.toLowerCase()}`}
            />
            <button
                type="button"
                onClick={onToggle}
                className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
                tabIndex={-1}
            >
                {show ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
            </button>
        </div>
    </div>
);

const TempPasswordChangeModal = () => {
    const { clearTempFlag } = useAdminAuth();

    const [currentPassword, setCurrentPassword] = useState('');
    const [newPassword, setNewPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [showCurrent, setShowCurrent] = useState(false);
    const [showNew, setShowNew] = useState(false);
    const [showConfirm, setShowConfirm] = useState(false);
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);
    const [success, setSuccess] = useState(false);

    // Prevent closing via Escape key
    useEffect(() => {
        const handler = (e) => { if (e.key === 'Escape') e.preventDefault(); };
        document.addEventListener('keydown', handler);
        return () => document.removeEventListener('keydown', handler);
    }, []);

    const strengthResults = STRENGTH_RULES.map((r) => ({
        label: r.label,
        passed: r.test(newPassword),
    }));
    const allStrengthPassed = strengthResults.every((r) => r.passed);

    const handleSubmit = useCallback(async (e) => {
        e.preventDefault();
        setError('');

        if (!currentPassword) { setError('Please enter your current (temporary) password.'); return; }
        if (!allStrengthPassed) { setError('New password does not meet the strength requirements.'); return; }
        if (newPassword !== confirmPassword) { setError('New password and confirmation do not match.'); return; }
        if (newPassword === currentPassword) { setError('New password must be different from the current password.'); return; }

        setLoading(true);
        try {
            const result = await authApi.changeTempPassword({
                currentPassword,
                newPassword,
                confirmPassword,
            });

            if (result?.result) {
                setSuccess(true);
                // Give user a moment to see the success message, then unlock dashboard
                setTimeout(() => clearTempFlag(), 1500);
            } else {
                setError(result?.message || 'Failed to change password. Please try again.');
            }
        } catch (err) {
            setError(err?.message || 'An error occurred. Please try again.');
        } finally {
            setLoading(false);
        }
    }, [currentPassword, newPassword, confirmPassword, allStrengthPassed, clearTempFlag]);

    return (
        // Full-screen overlay — not dismissable, blocks all content beneath
        <div
            className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm"
            // Prevent clicks on backdrop from closing
            onClick={(e) => e.stopPropagation()}
        >
            <div className="bg-white rounded-2xl shadow-2xl w-full max-w-md mx-4 overflow-hidden">
                {/* Header */}
                <div className="bg-gradient-to-r from-indigo-600 to-purple-600 px-6 py-5 text-white">
                    <div className="flex items-center gap-3">
                        <div className="bg-white/20 rounded-full p-2">
                            <Lock className="w-5 h-5" />
                        </div>
                        <div>
                            <h2 className="text-lg font-bold">Change Temporary Password</h2>
                            <p className="text-sm text-indigo-100 mt-0.5">
                                You must set a new password before continuing.
                            </p>
                        </div>
                    </div>
                </div>

                <div className="px-6 py-5">
                    {success ? (
                        <div className="flex flex-col items-center py-4 text-center gap-3">
                            <CheckCircle className="w-12 h-12 text-green-500" />
                            <p className="font-semibold text-gray-800">Password changed successfully!</p>
                            <p className="text-sm text-gray-500">Redirecting to dashboard…</p>
                        </div>
                    ) : (
                        <form onSubmit={handleSubmit} className="space-y-4">
                            {/* Security warning banner */}
                            <div className="flex items-start gap-2 p-3 bg-amber-50 border border-amber-200 rounded-lg text-amber-800 text-sm">
                                <AlertTriangle className="w-4 h-4 mt-0.5 flex-shrink-0" />
                                <span>
                                    Your account was created with a temporary password. Please set a new
                                    permanent password to protect your account.
                                </span>
                            </div>

                            <PasswordInput
                                id="current-password"
                                label="Current (Temporary) Password"
                                value={currentPassword}
                                onChange={(e) => setCurrentPassword(e.target.value)}
                                show={showCurrent}
                                onToggle={() => setShowCurrent((v) => !v)}
                                autoFocus
                            />

                            <PasswordInput
                                id="new-password"
                                label="New Password"
                                value={newPassword}
                                onChange={(e) => setNewPassword(e.target.value)}
                                show={showNew}
                                onToggle={() => setShowNew((v) => !v)}
                            />

                            {/* Strength checklist */}
                            {newPassword.length > 0 && (
                                <ul className="grid grid-cols-2 gap-1.5 text-xs">
                                    {strengthResults.map((r) => (
                                        <li key={r.label} className={`flex items-center gap-1.5 ${r.passed ? 'text-green-600' : 'text-gray-400'}`}>
                                            {r.passed
                                                ? <CheckCircle className="w-3.5 h-3.5 flex-shrink-0" />
                                                : <XCircle className="w-3.5 h-3.5 flex-shrink-0" />}
                                            {r.label}
                                        </li>
                                    ))}
                                </ul>
                            )}

                            <PasswordInput
                                id="confirm-password"
                                label="Confirm New Password"
                                value={confirmPassword}
                                onChange={(e) => setConfirmPassword(e.target.value)}
                                show={showConfirm}
                                onToggle={() => setShowConfirm((v) => !v)}
                            />

                            {/* Inline error */}
                            {error && (
                                <div className="flex items-center gap-2 p-3 bg-red-50 border border-red-200 rounded-lg text-red-700 text-sm">
                                    <AlertTriangle className="w-4 h-4 flex-shrink-0" />
                                    {error}
                                </div>
                            )}

                            <button
                                type="submit"
                                disabled={loading}
                                className="w-full py-2.5 px-4 bg-indigo-600 hover:bg-indigo-700 disabled:bg-indigo-400 text-white text-sm font-semibold rounded-lg transition-colors"
                            >
                                {loading ? 'Changing Password…' : 'Set New Password'}
                            </button>
                        </form>
                    )}
                </div>
            </div>
        </div>
    );
};

export default TempPasswordChangeModal;
