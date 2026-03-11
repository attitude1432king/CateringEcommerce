/**
 * SupervisorLogin Page
 * Modern login page for Event Supervisors with ENYVORA branding
 */

import { useState } from 'react';
import { Navigate, useNavigate, Link } from 'react-router-dom';
import { Mail, Lock, AlertCircle, Eye, EyeOff } from 'lucide-react';
import { useSupervisorAuth } from '../../contexts/SupervisorAuthContext';
import { supervisorLogin } from '../../services/api/supervisor/supervisorAuthApi';

const SupervisorLogin = () => {
    const [identifier, setIdentifier] = useState('');
    const [password, setPassword] = useState('');
    const [showPassword, setShowPassword] = useState(false);
    const [rememberMe, setRememberMe] = useState(false);
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);

    const { login, isAuthenticated } = useSupervisorAuth();
    const navigate = useNavigate();

    if (isAuthenticated) {
        return <Navigate to="/supervisor/dashboard" replace />;
    }

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');

        if (!identifier || !password) {
            setError('Please enter both email/phone and password');
            return;
        }

        setLoading(true);

        try {
            const result = await supervisorLogin(identifier, password);

            if (result.success) {
                const data = result.data?.data || result.data;
                const token = data.token;
                const supervisorData = data.supervisor || data;

                login(token, supervisorData);
                navigate('/supervisor/dashboard');
            } else {
                setError(result.message || 'Invalid credentials. Please try again.');
            }
        } catch {
            setError('An error occurred. Please try again.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="min-h-screen flex flex-col justify-center bg-gradient-to-br from-rose-50 via-white to-amber-50 py-10 px-4 relative overflow-hidden">
            {/* Decorative background elements */}
            <div className="absolute top-0 left-0 w-96 h-96 bg-rose-200 rounded-full mix-blend-multiply filter blur-xl opacity-20 animate-blob"></div>
            <div className="absolute bottom-0 right-0 w-96 h-96 bg-amber-200 rounded-full mix-blend-multiply filter blur-xl opacity-20 animate-blob animation-delay-2000"></div>
            <div className="absolute top-1/2 left-1/2 w-72 h-72 bg-pink-200 rounded-full mix-blend-multiply filter blur-xl opacity-15 animate-blob animation-delay-4000"></div>

            <div className="relative z-10 max-w-md w-full mx-auto">
                {/* Logo & Title */}
                <div className="text-center mb-8">
                    <div className="flex justify-center mb-6">
                        <img src="/logo.svg" alt="ENYVORA" className="h-16 w-auto block" />
                    </div>
                    <h2 className="text-3xl font-bold bg-gradient-to-r from-rose-600 to-amber-600 bg-clip-text text-transparent mb-2">
                        Supervisor Portal
                    </h2>
                    <p className="text-neutral-600 text-sm">
                        Sign in to manage your event assignments
                    </p>
                </div>

                {/* Login Form Card */}
                <div className="bg-white rounded-2xl shadow-2xl border-2 border-neutral-100 p-8">
                    <form onSubmit={handleSubmit} className="space-y-6">
                        {/* Error Alert */}
                        {error && (
                            <div className="flex items-center gap-3 p-4 bg-red-50 border-2 border-red-200 rounded-xl text-red-800 animate-fade-in">
                                <AlertCircle className="w-5 h-5 flex-shrink-0" />
                                <p className="text-sm font-medium">{error}</p>
                            </div>
                        )}

                        {/* Email/Phone */}
                        <div>
                            <label htmlFor="identifier" className="block text-sm font-semibold text-neutral-800 mb-2">
                                Email or Phone Number
                            </label>
                            <div className="relative">
                                <Mail className="absolute left-3.5 top-1/2 -translate-y-1/2 w-5 h-5 text-neutral-400" />
                                <input
                                    id="identifier"
                                    type="text"
                                    value={identifier}
                                    onChange={(e) => setIdentifier(e.target.value)}
                                    className="w-full pl-11 pr-4 py-3 border-2 border-neutral-200 rounded-xl focus:outline-none focus:border-rose-400 focus:ring-2 focus:ring-rose-100 transition-all duration-200"
                                    placeholder="Enter your email or phone"
                                    autoFocus
                                    autoComplete="username"
                                />
                            </div>
                        </div>

                        {/* Password */}
                        <div>
                            <label htmlFor="password" className="block text-sm font-semibold text-neutral-800 mb-2">
                                Password
                            </label>
                            <div className="relative">
                                <Lock className="absolute left-3.5 top-1/2 -translate-y-1/2 w-5 h-5 text-neutral-400" />
                                <input
                                    id="password"
                                    type={showPassword ? 'text' : 'password'}
                                    value={password}
                                    onChange={(e) => setPassword(e.target.value)}
                                    className="w-full pl-11 pr-12 py-3 border-2 border-neutral-200 rounded-xl focus:outline-none focus:border-rose-400 focus:ring-2 focus:ring-rose-100 transition-all duration-200"
                                    placeholder="Enter your password"
                                    autoComplete="current-password"
                                />
                                <button
                                    type="button"
                                    onClick={() => setShowPassword(!showPassword)}
                                    className="absolute right-3.5 top-1/2 -translate-y-1/2 text-neutral-400 hover:text-neutral-600 transition-colors"
                                >
                                    {showPassword ? <EyeOff className="w-5 h-5" /> : <Eye className="w-5 h-5" />}
                                </button>
                            </div>
                        </div>

                        {/* Remember Me & Forgot Password */}
                        <div className="flex items-center justify-between">
                            <label className="flex items-center cursor-pointer group">
                                <input
                                    type="checkbox"
                                    checked={rememberMe}
                                    onChange={(e) => setRememberMe(e.target.checked)}
                                    className="w-4 h-4 text-rose-600 border-neutral-300 rounded focus:ring-rose-200 cursor-pointer"
                                />
                                <span className="ml-2 text-sm text-neutral-700 group-hover:text-neutral-900">
                                    Remember me
                                </span>
                            </label>
                            <button
                                type="button"
                                className="text-sm font-semibold text-rose-600 hover:text-rose-700 transition-colors"
                            >
                                Forgot password?
                            </button>
                        </div>

                        {/* Submit Button */}
                        <button
                            type="submit"
                            disabled={loading}
                            className="group relative w-full px-6 py-3.5 bg-gradient-to-r from-rose-600 to-rose-500 text-white rounded-xl font-semibold shadow-lg hover:shadow-xl transform hover:-translate-y-0.5 transition-all duration-200 disabled:opacity-60 disabled:cursor-not-allowed disabled:transform-none disabled:hover:shadow-lg overflow-hidden"
                        >
                            <span className="relative z-10 flex items-center justify-center gap-2">
                                {loading ? (
                                    <>
                                        <div className="animate-spin rounded-full h-5 w-5 border-2 border-white border-t-transparent"></div>
                                        Signing in...
                                    </>
                                ) : (
                                    'Sign In'
                                )}
                            </span>
                            <div className="absolute inset-0 bg-gradient-to-r from-rose-700 to-rose-600 opacity-0 group-hover:opacity-100 transition-opacity"></div>
                        </button>
                    </form>

                    {/* Divider */}
                    <div className="relative my-6">
                        <div className="absolute inset-0 flex items-center">
                            <div className="w-full border-t-2 border-neutral-100"></div>
                        </div>
                        <div className="relative flex justify-center text-sm">
                            <span className="px-4 bg-white text-neutral-500 font-medium">New to ENYVORA?</span>
                        </div>
                    </div>

                    {/* Register Link */}
                    <Link
                        to="/supervisor/register"
                        className="block w-full px-6 py-3.5 bg-white text-neutral-700 rounded-xl font-semibold border-2 border-neutral-200 hover:border-rose-300 hover:bg-rose-50 transition-all duration-200 text-center shadow-sm hover:shadow-md"
                    >
                        Register as Supervisor
                    </Link>
                </div>

                {/* Footer */}
                <p className="mt-8 text-center text-sm text-neutral-500">
                    ENYVORA Catering Platform &copy; {new Date().getFullYear()}
                </p>
            </div>

            <style>{`
                @keyframes blob {
                    0% { transform: translate(0px, 0px) scale(1); }
                    33% { transform: translate(30px, -50px) scale(1.1); }
                    66% { transform: translate(-20px, 20px) scale(0.9); }
                    100% { transform: translate(0px, 0px) scale(1); }
                }
                .animate-blob { animation: blob 7s infinite; }
                .animation-delay-2000 { animation-delay: 2s; }
                .animation-delay-4000 { animation-delay: 4s; }
                @keyframes fade-in {
                    from { opacity: 0; transform: translateY(10px); }
                    to { opacity: 1; transform: translateY(0); }
                }
                .animate-fade-in { animation: fade-in 0.3s ease-out; }
            `}</style>
        </div>
    );
};

export default SupervisorLogin;
