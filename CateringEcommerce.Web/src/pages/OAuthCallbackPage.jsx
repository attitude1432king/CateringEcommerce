import React, { useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { CheckCircle, XCircle, Home } from 'lucide-react';
import { useAuth } from '../contexts/AuthContext';
import { Skeleton } from '../design-system/components';
import { sanitizeRedirectUrl } from '../utils/securityUtils';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL.replace(/\/$/, '');

const Spinner = () => (
    <div className="flex items-center justify-center w-16 h-16 mx-auto mb-5">
        <svg className="animate-spin w-10 h-10" viewBox="0 0 40 40" fill="none">
            <circle cx="20" cy="20" r="17" stroke="var(--color-primary)" strokeOpacity="0.15" strokeWidth="4" />
            <path d="M37 20a17 17 0 0 0-17-17" stroke="url(#sp)" strokeWidth="4" strokeLinecap="round" />
            <defs>
                <linearGradient id="sp" x1="20" y1="3" x2="37" y2="20" gradientUnits="userSpaceOnUse">
                    <stop stopColor="var(--color-primary)" />
                    <stop offset="1" stopColor="var(--color-secondary)" />
                </linearGradient>
            </defs>
        </svg>
    </div>
);

const OAuthCallbackPage = () => {
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();
    const { login } = useAuth();
    const [status, setStatus] = useState('processing');
    const [message, setMessage] = useState('Processing login...');

    useEffect(() => {
        handleCallback();
    }, []);

    const handleCallback = async () => {
        const code = searchParams.get('code');
        const state = searchParams.get('state');
        const error = searchParams.get('error');
        const provider = localStorage.getItem('oauth_provider') || 'google';

        if (error) {
            setStatus('error');
            setMessage(`Authentication failed: ${error}`);
            setTimeout(() => navigate('/'), 3000);
            return;
        }

        if (!code || !state) {
            setStatus('error');
            setMessage('Missing authentication parameters');
            setTimeout(() => navigate('/'), 3000);
            return;
        }

        try {
            const response = await fetch(
                `${API_BASE_URL}/api/oauth/${provider}/callback?code=${encodeURIComponent(code)}&state=${encodeURIComponent(state)}`,
                {
                    method: 'GET',
                    headers: { 'Content-Type': 'application/json' },
                    credentials: 'include',
                }
            );

            const data = await response.json();

            if (data.success && data.data) {
                if (login) {
                    login({
                        pkid:         data.data.userId,
                        name:         data.data.name,
                        email:        data.data.email,
                        profilePhoto: data.data.picture || undefined,
                        role:         'User',
                        phone:        '',
                    });
                }

                setStatus('success');
                setMessage(data.message || 'Login successful!');
                localStorage.removeItem('oauth_provider');

                setTimeout(() => {
                    const rawRedirect = localStorage.getItem('auth_redirect') || '/';
                    localStorage.removeItem('auth_redirect');
                    localStorage.removeItem('oauth_redirect');
                    navigate(sanitizeRedirectUrl(rawRedirect, '/'));
                }, 1500);
            } else {
                throw new Error(data.message || 'Authentication failed');
            }
        } catch (err) {
            console.error('OAuth callback error:', err);
            setStatus('error');
            setMessage(err.message || 'Authentication failed. Please try again.');
            setTimeout(() => navigate('/'), 3000);
        }
    };

    return (
        <div className="min-h-screen flex items-center justify-center p-4" style={{ background: 'linear-gradient(135deg, #f9fafb, #fff, rgba(255,107,53,0.03))' }}>
            <AnimatePresence mode="wait">
                <motion.div
                    key={status}
                    initial={{ opacity: 0, scale: 0.95, y: 12 }}
                    animate={{ opacity: 1, scale: 1, y: 0 }}
                    exit={{ opacity: 0, scale: 0.95, y: -8 }}
                    transition={{ type: 'spring', stiffness: 360, damping: 28 }}
                    className="bg-white rounded-3xl shadow-card p-10 max-w-sm w-full text-center border border-neutral-100"
                >
                    {status === 'processing' && (
                        <>
                            <Spinner />
                            <h2 className="text-lg font-bold text-neutral-900 mb-2">Signing you in…</h2>
                            <p className="text-sm text-neutral-500 mb-6">{message}</p>
                            <div className="space-y-2.5">
                                <Skeleton className="h-3 w-3/4 mx-auto rounded-full" />
                                <Skeleton className="h-3 w-1/2 mx-auto rounded-full" />
                            </div>
                        </>
                    )}

                    {status === 'success' && (
                        <>
                            <div
                                className="w-16 h-16 rounded-full flex items-center justify-center mx-auto mb-5"
                                style={{ background: 'rgba(34,197,94,0.1)' }}
                            >
                                <CheckCircle size={32} style={{ color: 'var(--color-success)' }} />
                            </div>
                            <h2 className="text-lg font-bold text-neutral-900 mb-1">Success!</h2>
                            <p className="text-sm text-neutral-500">{message}</p>
                            <p className="text-xs text-neutral-400 mt-3">Redirecting you to the app…</p>
                        </>
                    )}

                    {status === 'error' && (
                        <>
                            <div
                                className="w-16 h-16 rounded-full flex items-center justify-center mx-auto mb-5"
                                style={{ background: 'rgba(239,68,68,0.08)' }}
                            >
                                <XCircle size={32} style={{ color: 'var(--color-danger)' }} />
                            </div>
                            <h2 className="text-lg font-bold text-neutral-900 mb-2">Authentication Failed</h2>
                            <p className="text-sm text-neutral-500 mb-6">{message}</p>
                            <button
                                onClick={() => navigate('/')}
                                className="inline-flex items-center gap-2 px-6 py-3 rounded-xl text-white font-bold text-sm transition-all hover:scale-105"
                                style={{ background: 'var(--gradient-catering)' }}
                            >
                                <Home size={15} /> Go to Home
                            </button>
                        </>
                    )}
                </motion.div>
            </AnimatePresence>
        </div>
    );
};

export default OAuthCallbackPage;
