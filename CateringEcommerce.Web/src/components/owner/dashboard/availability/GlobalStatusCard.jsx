/*
========================================
File: src/components/owner/dashboard/availability/GlobalStatusCard.jsx
Modern Redesign - ENYVORA Brand with Motion
========================================
The "Master Switch" for the catering business.
*/
import React from 'react';
import ToggleSwitch from '../../../common/ToggleSwitch';
import { GlobalStatus } from '../../../../utils/staticData'; 

export default function GlobalStatusCard({ status, onStatusChange }) {
    const isOpen = status === GlobalStatus.OPEN;

    return (
        <div className="bg-white rounded-2xl shadow-sm border border-neutral-100 overflow-hidden hover:shadow-lg transition-all duration-300 transform hover:-translate-y-1">
            {/* Animated Color Bar */}
            <div className={`h-2 transition-all duration-500 ${
                isOpen
                    ? 'bg-gradient-to-r from-green-500 via-emerald-500 to-green-500 animate-gradient-x'
                    : 'bg-gradient-to-r from-red-500 via-rose-500 to-red-500 animate-gradient-x'
            }`}></div>

            <div className="p-6">
                <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-6">
                    {/* Left Section - Status Info with animations */}
                    <div className="flex items-start gap-4 flex-1">
                        {/* Animated Icon */}
                        <div className={`p-4 rounded-xl transition-all duration-500 transform hover:scale-110 hover:rotate-6 ${
                            isOpen ? 'bg-green-100 animate-pulse-green' : 'bg-red-100 animate-pulse-red'
                        }`}>
                            {isOpen ? (
                                <svg className="w-8 h-8 text-green-600 animate-check-draw" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                                </svg>
                            ) : (
                                <svg className="w-8 h-8 text-red-600 animate-x-draw" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
                                </svg>
                            )}
                        </div>

                        <div className="flex-1">
                            <div className="flex items-center gap-3 mb-2">
                                <h2 className="text-xl font-bold text-neutral-900 animate-fade-in">Global Service Status</h2>
                                {/* Animated Badge */}
                                <span className={`px-3 py-1 rounded-full text-xs font-bold transition-all duration-500 transform animate-bounce-subtle ${
                                    isOpen
                                        ? 'bg-green-100 text-green-800 border border-green-200 animate-pulse-border-green'
                                        : 'bg-red-100 text-red-800 border border-red-200 animate-pulse-border-red'
                                }`}>
                                    {isOpen ? 'OPEN' : 'CLOSED'}
                                </span>
                            </div>

                            <p className="text-sm text-neutral-600 leading-relaxed animate-slide-in-fade">
                                {isOpen
                                    ? "Your business is currently visible and accepting bookings based on your calendar settings."
                                    : "Your business is temporarily closed. Customers cannot place new orders at this time."}
                            </p>

                            {/* Quick Stats with animation */}
                            <div className="mt-4 flex gap-4 animate-slide-up-delay">
                                <div className="flex items-center gap-2 text-sm group">
                                    <svg className="w-4 h-4 group-hover:scale-125 transition-transform duration-300" style={{ color: 'var(--color-primary)' }} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                                    </svg>
                                    <span className="text-neutral-700 font-medium">{isOpen ? 'Visible to customers' : 'Hidden from customers'}</span>
                                </div>
                            </div>
                        </div>
                    </div>

                    {/* Right Section - Toggle with animations */}
                    <div className="w-full md:w-auto animate-scale-in">
                        <div className="bg-gradient-to-br from-neutral-50 to-neutral-100 px-6 py-4 rounded-xl border border-neutral-200 shadow-inner hover:shadow-md transition-all duration-300">
                            <div className="flex items-center justify-between gap-4">
                                <span className={`text-sm font-bold transition-all duration-300 transform ${
                                    isOpen ? 'text-green-600 scale-110' : 'text-neutral-400 scale-100'
                                }`}>
                                    OPEN
                                </span>

                                <div className="relative transform hover:scale-110 transition-transform duration-300">
                                    <ToggleSwitch
                                        label=""
                                        enabled={!isOpen}
                                        setEnabled={(val) => onStatusChange(val ? GlobalStatus.CLOSED : GlobalStatus.OPEN)}
                                    />
                                </div>

                                <span className={`text-sm font-bold transition-all duration-300 transform ${
                                    !isOpen ? 'text-red-600 scale-110' : 'text-neutral-400 scale-100'
                                }`}>
                                    CLOSED
                                </span>
                            </div>

                            <p className="text-xs text-neutral-500 text-center mt-2 animate-fade-in-delay">
                                Toggle to change status
                            </p>
                        </div>
                    </div>
                </div>

                {/* Additional Info with slide animation */}
                {isOpen && (
                    <div className="mt-6 p-4 rounded-xl border animate-expand-down" style={{ background: 'rgba(255,107,53,0.06)', borderColor: 'rgba(255,107,53,0.2)' }}>
                        <div className="flex items-start gap-3">
                            <svg className="w-5 h-5 flex-shrink-0 mt-0.5 animate-bounce-subtle" style={{ color: 'var(--color-primary)' }} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                            </svg>
                            <div>
                                <p className="text-sm font-medium text-neutral-900 mb-1">Pro Tip</p>
                                <p className="text-sm text-neutral-700">
                                    Use the calendar below to set specific dates as closed or fully booked while keeping your service open.
                                </p>
                            </div>
                        </div>
                    </div>
                )}
            </div>

            <style>{`
                @keyframes gradient-x {
                    0%, 100% {
                        background-position: 0% 50%;
                    }
                    50% {
                        background-position: 100% 50%;
                    }
                }

                @keyframes pulse-green {
                    0%, 100% {
                        box-shadow: 0 0 0 0 rgba(34, 197, 94, 0.4);
                    }
                    50% {
                        box-shadow: 0 0 0 10px rgba(34, 197, 94, 0);
                    }
                }

                @keyframes pulse-red {
                    0%, 100% {
                        box-shadow: 0 0 0 0 rgba(239, 68, 68, 0.4);
                    }
                    50% {
                        box-shadow: 0 0 0 10px rgba(239, 68, 68, 0);
                    }
                }

                @keyframes check-draw {
                    0% {
                        stroke-dasharray: 0, 100;
                        opacity: 0;
                    }
                    100% {
                        stroke-dasharray: 100, 0;
                        opacity: 1;
                    }
                }

                @keyframes x-draw {
                    0% {
                        stroke-dasharray: 0, 100;
                        opacity: 0;
                    }
                    100% {
                        stroke-dasharray: 100, 0;
                        opacity: 1;
                    }
                }

                @keyframes bounce-subtle {
                    0%, 100% {
                        transform: translateY(0);
                    }
                    50% {
                        transform: translateY(-3px);
                    }
                }

                @keyframes pulse-border-green {
                    0%, 100% {
                        border-color: rgb(187, 247, 208);
                    }
                    50% {
                        border-color: rgb(34, 197, 94);
                    }
                }

                @keyframes pulse-border-red {
                    0%, 100% {
                        border-color: rgb(254, 202, 202);
                    }
                    50% {
                        border-color: rgb(239, 68, 68);
                    }
                }

                @keyframes slide-in-fade {
                    from {
                        opacity: 0;
                        transform: translateX(-10px);
                    }
                    to {
                        opacity: 1;
                        transform: translateX(0);
                    }
                }

                @keyframes slide-up-delay {
                    from {
                        opacity: 0;
                        transform: translateY(10px);
                    }
                    to {
                        opacity: 1;
                        transform: translateY(0);
                    }
                }

                @keyframes scale-in {
                    from {
                        opacity: 0;
                        transform: scale(0.9);
                    }
                    to {
                        opacity: 1;
                        transform: scale(1);
                    }
                }

                @keyframes expand-down {
                    from {
                        opacity: 0;
                        max-height: 0;
                        transform: scaleY(0);
                    }
                    to {
                        opacity: 1;
                        max-height: 200px;
                        transform: scaleY(1);
                    }
                }

                .animate-gradient-x {
                    background-size: 200% 200%;
                    animation: gradient-x 3s ease infinite;
                }

                .animate-pulse-green {
                    animation: pulse-green 2s ease-in-out infinite;
                }

                .animate-pulse-red {
                    animation: pulse-red 2s ease-in-out infinite;
                }

                .animate-check-draw {
                    animation: check-draw 0.6s ease-out;
                }

                .animate-x-draw {
                    animation: x-draw 0.6s ease-out;
                }

                .animate-bounce-subtle {
                    animation: bounce-subtle 2s ease-in-out infinite;
                }

                .animate-pulse-border-green {
                    animation: pulse-border-green 2s ease-in-out infinite;
                }

                .animate-pulse-border-red {
                    animation: pulse-border-red 2s ease-in-out infinite;
                }

                .animate-fade-in {
                    animation: slide-in-fade 0.6s ease-out;
                }

                .animate-slide-in-fade {
                    animation: slide-in-fade 0.7s ease-out;
                }

                .animate-slide-up-delay {
                    animation: slide-up-delay 0.8s ease-out;
                }

                .animate-scale-in {
                    animation: scale-in 0.6s ease-out;
                }

                .animate-fade-in-delay {
                    animation: slide-in-fade 0.9s ease-out;
                }

                .animate-expand-down {
                    animation: expand-down 0.5s ease-out;
                    transform-origin: top;
                }
            `}</style>
        </div>
    );
}
