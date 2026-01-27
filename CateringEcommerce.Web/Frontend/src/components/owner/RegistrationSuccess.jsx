/*
========================================
File: src/components/owner/RegistrationSuccess.jsx (REDESIGNED)
========================================
*/
import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

export default function RegistrationSuccess() {
    const navigate = useNavigate();
    const [timeLeft, setTimeLeft] = useState(48 * 60 * 60); // 48 hours in seconds

    useEffect(() => {
        const timer = setInterval(() => {
            setTimeLeft(prevTime => (prevTime > 0 ? prevTime - 1 : 0));
        }, 1000);
        return () => clearInterval(timer);
    }, []);

    const formatTime = (seconds) => {
        const h = Math.floor(seconds / 3600).toString().padStart(2, '0');
        const m = Math.floor((seconds % 3600) / 60).toString().padStart(2, '0');
        const s = (seconds % 60).toString().padStart(2, '0');
        return `${h}:${m}:${s}`;
    };

    const timePercentage = ((48 * 60 * 60 - timeLeft) / (48 * 60 * 60)) * 100;

    return (
        <div className="min-h-screen bg-gradient-to-br from-green-50 via-emerald-50 to-teal-50 flex items-center justify-center p-4 relative overflow-hidden">
            {/* Decorative background elements */}
            <div className="absolute top-0 left-0 w-96 h-96 bg-green-200 rounded-full mix-blend-multiply filter blur-xl opacity-30 animate-blob"></div>
            <div className="absolute bottom-0 right-0 w-96 h-96 bg-emerald-200 rounded-full mix-blend-multiply filter blur-xl opacity-30 animate-blob animation-delay-2000"></div>

            <div className="relative z-10 bg-white p-10 md:p-12 rounded-3xl shadow-2xl text-center max-w-2xl w-full animate-fade-in">
                {/* Success Animation */}
                <div className="relative mb-8">
                    <div className="mx-auto bg-gradient-to-br from-green-400 to-emerald-500 rounded-full h-24 w-24 flex items-center justify-center mb-6 shadow-lg animate-bounce-slow">
                        <svg className="h-14 w-14 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={3}>
                            <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
                        </svg>
                    </div>
                    {/* Confetti circles */}
                    <div className="absolute top-0 left-1/4 w-3 h-3 bg-rose-400 rounded-full animate-ping"></div>
                    <div className="absolute top-4 right-1/4 w-2 h-2 bg-blue-400 rounded-full animate-ping animation-delay-1000"></div>
                    <div className="absolute bottom-4 left-1/3 w-2 h-2 bg-amber-400 rounded-full animate-ping animation-delay-2000"></div>
                </div>

                {/* Header */}
                <h1 className="text-4xl md:text-5xl font-bold bg-gradient-to-r from-green-600 to-emerald-600 bg-clip-text text-transparent mb-4">
                    Registration Submitted Successfully!
                </h1>
                <p className="text-lg text-neutral-600 mb-8 leading-relaxed">
                    Thank you for joining <span className="font-bold text-rose-600">ENYVORA Partners</span>. Your application is now being reviewed by our verification team.
                </p>

                {/* Verification Timeline */}
                <div className="bg-gradient-to-r from-blue-50 to-indigo-50 border-2 border-blue-200 rounded-2xl p-6 mb-8">
                    <h3 className="text-xl font-bold text-neutral-800 mb-6 flex items-center justify-center gap-2">
                        <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6 text-blue-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                        </svg>
                        Verification Process
                    </h3>

                    {/* Timeline Steps */}
                    <div className="space-y-4 text-left">
                        <div className="flex items-start gap-4">
                            <div className="flex-shrink-0 w-10 h-10 bg-green-500 rounded-full flex items-center justify-center text-white font-bold shadow-lg">
                                <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                                    <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
                                </svg>
                            </div>
                            <div className="flex-1">
                                <h4 className="font-semibold text-neutral-800">Step 1: Application Received</h4>
                                <p className="text-sm text-green-600 font-medium">✓ Completed</p>
                            </div>
                        </div>

                        <div className="flex items-start gap-4">
                            <div className="flex-shrink-0 w-10 h-10 bg-blue-500 rounded-full flex items-center justify-center text-white font-bold shadow-lg animate-pulse">2</div>
                            <div className="flex-1">
                                <h4 className="font-semibold text-neutral-800">Step 2: Document Verification</h4>
                                <p className="text-sm text-blue-600 font-medium">⏳ In Progress (12-24 hours)</p>
                                <p className="text-xs text-neutral-600 mt-1">Our team is verifying your FSSAI, GST, PAN, and bank details</p>
                            </div>
                        </div>

                        <div className="flex items-start gap-4">
                            <div className="flex-shrink-0 w-10 h-10 bg-neutral-300 rounded-full flex items-center justify-center text-neutral-600 font-bold">3</div>
                            <div className="flex-1">
                                <h4 className="font-semibold text-neutral-800">Step 3: Business Review</h4>
                                <p className="text-sm text-neutral-500">⏱ Pending (24-48 hours)</p>
                                <p className="text-xs text-neutral-600 mt-1">Quality check of kitchen images and menu offerings</p>
                            </div>
                        </div>

                        <div className="flex items-start gap-4">
                            <div className="flex-shrink-0 w-10 h-10 bg-neutral-300 rounded-full flex items-center justify-center text-neutral-600 font-bold">4</div>
                            <div className="flex-1">
                                <h4 className="font-semibold text-neutral-800">Step 4: Account Activation</h4>
                                <p className="text-sm text-neutral-500">⏱ Pending</p>
                                <p className="text-xs text-neutral-600 mt-1">You'll receive login credentials via email and SMS</p>
                            </div>
                        </div>
                    </div>
                </div>

                {/* Countdown Timer */}
                <div className="bg-gradient-to-r from-rose-50 to-pink-50 border-2 border-rose-200 rounded-2xl p-6 mb-8">
                    <h3 className="text-sm font-semibold text-neutral-600 mb-3 uppercase tracking-wide">Expected Verification Time</h3>
                    <div className="text-5xl font-bold bg-gradient-to-r from-rose-600 to-pink-600 bg-clip-text text-transparent mb-4 font-mono tracking-tight">
                        {formatTime(timeLeft)}
                    </div>
                    {/* Progress Bar */}
                    <div className="w-full bg-neutral-200 rounded-full h-3 overflow-hidden">
                        <div
                            className="bg-gradient-to-r from-rose-500 to-pink-500 h-3 rounded-full transition-all duration-1000 ease-linear"
                            style={{ width: `${timePercentage}%` }}
                        ></div>
                    </div>
                    <p className="text-xs text-neutral-600 mt-3">Most applications are reviewed within 24-48 hours</p>
                </div>

                {/* What's Next Section */}
                <div className="bg-amber-50 border-l-4 border-amber-400 rounded-lg p-5 mb-8 text-left">
                    <div className="flex items-start gap-3">
                        <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6 text-amber-600 flex-shrink-0 mt-0.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                        </svg>
                        <div className="flex-1">
                            <h4 className="font-bold text-amber-900 mb-2">What Happens Next?</h4>
                            <ul className="text-sm text-amber-800 space-y-1.5">
                                <li className="flex items-start gap-2">
                                    <span className="text-amber-600 mt-1">•</span>
                                    <span>You'll receive real-time updates via <strong>Email</strong> and <strong>SMS</strong></span>
                                </li>
                                <li className="flex items-start gap-2">
                                    <span className="text-amber-600 mt-1">•</span>
                                    <span>Our team may contact you if additional information is needed</span>
                                </li>
                                <li className="flex items-start gap-2">
                                    <span className="text-amber-600 mt-1">•</span>
                                    <span>Once approved, you'll get login credentials to access your partner dashboard</span>
                                </li>
                                <li className="flex items-start gap-2">
                                    <span className="text-amber-600 mt-1">•</span>
                                    <span>You can start receiving orders immediately after activation!</span>
                                </li>
                            </ul>
                        </div>
                    </div>
                </div>

                {/* Action Buttons */}
                <div className="flex flex-col sm:flex-row gap-4 justify-center">
                    <button
                        onClick={() => navigate('/')}
                        className="group relative px-8 py-4 bg-gradient-to-r from-rose-600 to-rose-500 text-white rounded-xl font-semibold shadow-lg hover:shadow-xl transform hover:-translate-y-0.5 transition-all duration-200 overflow-hidden"
                    >
                        <span className="relative z-10 flex items-center justify-center gap-2">
                            <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                                <path d="M10.707 2.293a1 1 0 00-1.414 0l-7 7a1 1 0 001.414 1.414L4 10.414V17a1 1 0 001 1h2a1 1 0 001-1v-2a1 1 0 011-1h2a1 1 0 011 1v2a1 1 0 001 1h2a1 1 0 001-1v-6.586l.293.293a1 1 0 001.414-1.414l-7-7z" />
                            </svg>
                            Back to Home
                        </span>
                        <div className="absolute inset-0 bg-gradient-to-r from-rose-700 to-rose-600 opacity-0 group-hover:opacity-100 transition-opacity"></div>
                    </button>

                    <button
                        onClick={() => window.location.href = 'mailto:support@enyvora.com'}
                        className="px-8 py-4 bg-white text-neutral-700 rounded-xl font-semibold border-2 border-neutral-200 hover:border-neutral-300 hover:bg-neutral-50 transition-all duration-200 shadow-md hover:shadow-lg flex items-center justify-center gap-2"
                    >
                        <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M18.364 5.636l-3.536 3.536m0 5.656l3.536 3.536M9.172 9.172L5.636 5.636m3.536 9.192l-3.536 3.536M21 12a9 9 0 11-18 0 9 9 0 0118 0zm-5 0a4 4 0 11-8 0 4 4 0 018 0z" />
                        </svg>
                        Contact Support
                    </button>
                </div>

                {/* Footer Note */}
                <p className="text-xs text-neutral-500 mt-8 px-4">
                    Need help? Email us at <a href="mailto:support@enyvora.com" className="text-rose-600 hover:underline font-semibold">support@enyvora.com</a> or call <a href="tel:+911234567890" className="text-rose-600 hover:underline font-semibold">+91 123-456-7890</a>
                </p>
            </div>

            <style>{`
                @keyframes bounce-slow {
                    0%, 100% { transform: translateY(0); }
                    50% { transform: translateY(-10px); }
                }
                .animate-bounce-slow {
                    animation: bounce-slow 2s infinite;
                }
                @keyframes blob {
                    0% { transform: translate(0px, 0px) scale(1); }
                    33% { transform: translate(30px, -50px) scale(1.1); }
                    66% { transform: translate(-20px, 20px) scale(0.9); }
                    100% { transform: translate(0px, 0px) scale(1); }
                }
                .animate-blob {
                    animation: blob 7s infinite;
                }
                .animation-delay-1000 {
                    animation-delay: 1s;
                }
                .animation-delay-2000 {
                    animation-delay: 2s;
                }
            `}</style>
        </div>
    );
}