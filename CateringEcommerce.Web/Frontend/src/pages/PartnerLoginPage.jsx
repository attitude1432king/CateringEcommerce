/*
========================================
File: src/pages/PartnerLoginPage.jsx (REDESIGNED)
========================================
*/
import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import AuthModal from '../components/user/AuthModal'; // We will reuse the AuthModal

export default function PartnerLoginPage() {
    const [isAuthModalOpen, setIsAuthModalOpen] = useState(false);
    const navigate = useNavigate();

    return (
        <>
            <AuthModal
                isOpen={isAuthModalOpen}
                onClose={() => setIsAuthModalOpen(false)}
                isPartnerLogin={true} // Prop to tell the modal it's for partners
            />
            <div className="min-h-screen bg-gradient-to-br from-rose-50 via-white to-amber-50 flex flex-col relative overflow-hidden">
                {/* Decorative background elements */}
                <div className="absolute top-0 left-0 w-72 h-72 bg-rose-200 rounded-full mix-blend-multiply filter blur-xl opacity-30 animate-blob"></div>
                <div className="absolute top-0 right-0 w-72 h-72 bg-amber-200 rounded-full mix-blend-multiply filter blur-xl opacity-30 animate-blob animation-delay-2000"></div>
                <div className="absolute -bottom-8 left-20 w-72 h-72 bg-pink-200 rounded-full mix-blend-multiply filter blur-xl opacity-30 animate-blob animation-delay-4000"></div>

                {/* Header */}
                <header className="relative z-10 p-6 md:p-8">
                    <div className="flex items-center gap-3">
                        <img src="/logo.svg" alt="ENYVORA Partners" className="h-10 md:h-12 w-auto" />
                    </div>
                </header>

                {/* Main Content */}
                <main className="relative z-10 flex-1 flex flex-col lg:flex-row items-center justify-center text-center lg:text-left px-6 py-8 md:px-12 max-w-7xl mx-auto w-full gap-12">
                    {/* Left Side - Content */}
                    <div className="flex-1 space-y-6 max-w-xl">
                        <div className="inline-block">
                            <span className="px-4 py-2 bg-rose-100 text-rose-600 rounded-full text-sm font-semibold">
                                Join 5000+ Partners
                            </span>
                        </div>

                        <h2 className="text-4xl md:text-5xl lg:text-6xl font-bold text-neutral-800 leading-tight">
                            Grow Your <span className="bg-gradient-to-r from-rose-600 to-amber-500 bg-clip-text text-transparent">Catering Business</span>
                        </h2>

                        <p className="text-lg text-neutral-600 leading-relaxed">
                            Partner with ENYVORA to reach thousands of customers. Manage orders, track earnings, and grow your business with our powerful partner dashboard.
                        </p>

                        {/* Feature highlights */}
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 pt-4">
                            <div className="flex items-start gap-3 text-left">
                                <div className="w-10 h-10 bg-green-100 rounded-lg flex items-center justify-center flex-shrink-0">
                                    <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5 text-green-600" viewBox="0 0 20 20" fill="currentColor">
                                        <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                                    </svg>
                                </div>
                                <div>
                                    <h3 className="font-semibold text-neutral-800">Zero Commission</h3>
                                    <p className="text-sm text-neutral-600">First 3 months free</p>
                                </div>
                            </div>
                            <div className="flex items-start gap-3 text-left">
                                <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center flex-shrink-0">
                                    <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5 text-blue-600" viewBox="0 0 20 20" fill="currentColor">
                                        <path d="M2 11a1 1 0 011-1h2a1 1 0 011 1v5a1 1 0 01-1 1H3a1 1 0 01-1-1v-5zM8 7a1 1 0 011-1h2a1 1 0 011 1v9a1 1 0 01-1 1H9a1 1 0 01-1-1V7zM14 4a1 1 0 011-1h2a1 1 0 011 1v12a1 1 0 01-1 1h-2a1 1 0 01-1-1V4z" />
                                    </svg>
                                </div>
                                <div>
                                    <h3 className="font-semibold text-neutral-800">Real-time Analytics</h3>
                                    <p className="text-sm text-neutral-600">Track your growth</p>
                                </div>
                            </div>
                            <div className="flex items-start gap-3 text-left">
                                <div className="w-10 h-10 bg-purple-100 rounded-lg flex items-center justify-center flex-shrink-0">
                                    <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5 text-purple-600" viewBox="0 0 20 20" fill="currentColor">
                                        <path d="M8.433 7.418c.155-.103.346-.196.567-.267v1.698a2.305 2.305 0 01-.567-.267C8.07 8.34 8 8.114 8 8c0-.114.07-.34.433-.582zM11 12.849v-1.698c.22.071.412.164.567.267.364.243.433.468.433.582 0 .114-.07.34-.433.582a2.305 2.305 0 01-.567.267z" />
                                        <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm1-13a1 1 0 10-2 0v.092a4.535 4.535 0 00-1.676.662C6.602 6.234 6 7.009 6 8c0 .99.602 1.765 1.324 2.246.48.32 1.054.545 1.676.662v1.941c-.391-.127-.68-.317-.843-.504a1 1 0 10-1.51 1.31c.562.649 1.413 1.076 2.353 1.253V15a1 1 0 102 0v-.092a4.535 4.535 0 001.676-.662C13.398 13.766 14 12.991 14 12c0-.99-.602-1.765-1.324-2.246A4.535 4.535 0 0011 9.092V7.151c.391.127.68.317.843.504a1 1 0 101.511-1.31c-.563-.649-1.413-1.076-2.354-1.253V5z" clipRule="evenodd" />
                                    </svg>
                                </div>
                                <div>
                                    <h3 className="font-semibold text-neutral-800">Weekly Payouts</h3>
                                    <p className="text-sm text-neutral-600">Fast settlements</p>
                                </div>
                            </div>
                            <div className="flex items-start gap-3 text-left">
                                <div className="w-10 h-10 bg-amber-100 rounded-lg flex items-center justify-center flex-shrink-0">
                                    <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5 text-amber-600" viewBox="0 0 20 20" fill="currentColor">
                                        <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-8-3a1 1 0 00-.867.5 1 1 0 11-1.731-1A3 3 0 0113 8a3.001 3.001 0 01-2 2.83V11a1 1 0 11-2 0v-1a1 1 0 011-1 1 1 0 100-2zm0 8a1 1 0 100-2 1 1 0 000 2z" clipRule="evenodd" />
                                    </svg>
                                </div>
                                <div>
                                    <h3 className="font-semibold text-neutral-800">24/7 Support</h3>
                                    <p className="text-sm text-neutral-600">We're here to help</p>
                                </div>
                            </div>
                        </div>

                        {/* Action buttons */}
                        <div className="flex flex-col sm:flex-row gap-4 pt-6">
                            <button
                                onClick={() => setIsAuthModalOpen(true)}
                                className="group relative px-8 py-4 bg-gradient-to-r from-rose-600 to-rose-500 text-white rounded-xl font-semibold shadow-lg hover:shadow-xl transform hover:-translate-y-0.5 transition-all duration-200 overflow-hidden"
                            >
                                <span className="relative z-10">Login to Dashboard</span>
                                <div className="absolute inset-0 bg-gradient-to-r from-rose-700 to-rose-600 opacity-0 group-hover:opacity-100 transition-opacity"></div>
                            </button>
                            <button
                                onClick={() => navigate('/partner-registration')}
                                className="px-8 py-4 bg-white text-rose-600 rounded-xl font-semibold border-2 border-rose-200 hover:border-rose-300 hover:bg-rose-50 transition-all duration-200 shadow-md hover:shadow-lg"
                            >
                                Start Registration
                            </button>
                        </div>

                        <p className="text-xs text-neutral-500 pt-4">
                            By continuing, you agree to our <a href="#" className="text-rose-600 hover:underline">Terms of Service</a> and <a href="#" className="text-rose-600 hover:underline">Privacy Policy</a>.
                        </p>
                    </div>

                    {/* Right Side - Illustration */}
                    <div className="flex-1 relative max-w-lg">
                        <div className="relative">
                            {/* Decorative card backgrounds */}
                            <div className="absolute -top-4 -right-4 w-full h-full bg-gradient-to-br from-rose-200 to-amber-200 rounded-3xl transform rotate-6 opacity-50"></div>
                            <div className="absolute -top-2 -right-2 w-full h-full bg-gradient-to-br from-rose-300 to-amber-300 rounded-3xl transform rotate-3 opacity-30"></div>

                            {/* Main image container */}
                            <div className="relative bg-white p-8 rounded-3xl shadow-2xl">
                                <img
                                    src="https://img.freepik.com/free-vector/chef-cooking-kitchen-restaurant-cartoon-art-illustration_56104-649.jpg"
                                    alt="Chef Cooking"
                                    className="w-full h-auto object-contain"
                                />

                                {/* Floating stats cards */}
                                <div className="absolute -bottom-6 -left-6 bg-white p-4 rounded-xl shadow-xl border border-neutral-100">
                                    <div className="flex items-center gap-3">
                                        <div className="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center">
                                            <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6 text-green-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 7h8m0 0v8m0-8l-8 8-4-4-6 6" />
                                            </svg>
                                        </div>
                                        <div>
                                            <p className="text-2xl font-bold text-neutral-800">150%</p>
                                            <p className="text-xs text-neutral-600">Avg. Growth</p>
                                        </div>
                                    </div>
                                </div>

                                <div className="absolute -top-6 -right-6 bg-white p-4 rounded-xl shadow-xl border border-neutral-100">
                                    <div className="flex items-center gap-3">
                                        <div className="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center">
                                            <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6 text-blue-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
                                            </svg>
                                        </div>
                                        <div>
                                            <p className="text-2xl font-bold text-neutral-800">5000+</p>
                                            <p className="text-xs text-neutral-600">Partners</p>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </main>
            </div>

            <style>{`
                @keyframes blob {
                    0% { transform: translate(0px, 0px) scale(1); }
                    33% { transform: translate(30px, -50px) scale(1.1); }
                    66% { transform: translate(-20px, 20px) scale(0.9); }
                    100% { transform: translate(0px, 0px) scale(1); }
                }
                .animate-blob {
                    animation: blob 7s infinite;
                }
                .animation-delay-2000 {
                    animation-delay: 2s;
                }
                .animation-delay-4000 {
                    animation-delay: 4s;
                }
            `}</style>
        </>
    );
}