/*
========================================
File: src/components/owner/dashboard/availability/AvailabilityManagement.jsx
Modern Redesign - ENYVORA Brand with Motion
========================================
Main container for the Availability Management module.
*/
import React, { useState, useEffect, useCallback } from 'react';
import GlobalStatusCard from './GlobalStatusCard';
import AvailabilityCalendar from './AvailabilityCalendar';
import Loader from '../../../common/Loader';
import { useToast } from '../../../../contexts/ToastContext';
import { ownerApiService } from '../../../../services/ownerApi';
import { AvailabilityStatus, GlobalStatus } from '../../../../utils/staticData';

export default function AvailabilityManagement() {
    const [globalStatus, setGlobalStatus] = useState(GlobalStatus.OPEN);
    const [specialDates, setSpecialDates] = useState({});
    const [isLoading, setIsLoading] = useState(true);
    const [isCalendarLoading, setIsCalendarLoading] = useState(false);
    const { showToast } = useToast();

    const fetchAvailabilityData = async () => {
        setIsLoading(true);
        try {
            const currentDate = new Date();
            const currentYear = currentDate.getFullYear();
            const monthNumber = currentDate.getMonth() + 1;
            const response = await ownerApiService.getAvailability(currentYear, monthNumber);
            setGlobalStatus(response.data.globalStatus);
            setSpecialDates(response.data.specialDates);
        } catch (error) {
            console.error('Error fetching availability data:', error);
            showToast('Failed to load availability data.', 'error');
        }
        finally {
            setIsLoading(false);
        }
    }

    useEffect(() => {
        fetchAvailabilityData();
    }, []);

    const handleGlobalStatusChange = (newStatus) => {
        setGlobalStatus(newStatus);
        ownerApiService.updateGlobalAvailability(newStatus);
        showToast(`Catering is now ${newStatus === GlobalStatus.OPEN ? 'Open' : 'Closed'} globally.`, 'success');
    };

    const handleDateStatusUpdate = (date, status, note) => {
        setIsLoading(true);
        try {
            const updatedDates = { ...specialDates };
            if (status === AvailabilityStatus.OPEN) {
                delete updatedDates[date]; // Remove override if setting back to Open
            } else {
                updatedDates[date] = { status, note };
            }
            setSpecialDates(updatedDates);
            const payload = { date, status, note };
            ownerApiService.updateDateAvailability(payload);
            showToast(`Availability updated for ${date}`, 'success');
        } catch (e) {
            console.error(`Failed to update for ${date}: ${e}`)
            showToast(`Failed to update for ${date}`, 'error')
        }
        finally {
            setIsLoading(false);
        }
    };

    // Function to fetch data when month/year changes
    const fetchMonthData = useCallback(async (year, month) => {
        setIsCalendarLoading(true);
        console.log(`Fetching availability for ${month + 1}/${year}...`);
        try {
            const response = await ownerApiService.getAvailability(year, month + 1);
            setGlobalStatus(response.data.globalStatus);
            setSpecialDates(response.data.specialDates);
            setIsCalendarLoading(false);
        } catch (error) {
            showToast("Failed to fetch calendar data", "error");
            setIsCalendarLoading(false);
        }
    }, [showToast]);

    if (isLoading) return <div className="flex justify-center items-center h-96"><Loader /></div>;

    return (
        <div className="p-4 sm:p-6 lg:p-8 space-y-6">
            {/* Header with fade-in animation */}

            <div className="bg-white rounded-2xl shadow-sm border border-neutral-100 p-6">
                <div className="flex items-center gap-3">
                    <div className="p-3 rounded-xl bg-gradient-to-br from-indigo-100 to-purple-100 animate-pulse-slow">
                        <svg className="w-6 h-6 text-indigo-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                        </svg>
                    </div>
                    <div className="flex-1">
                        <h1 className="text-3xl font-bold text-neutral-900 animate-slide-in-left">Availability Management</h1>
                        <p className="text-neutral-600 mt-1 animate-slide-in-left animation-delay-100">Control when you can accept new catering orders</p>
                    </div>
                </div>
            </div>

            {/* Global Status Card with stagger animation */}
            <div className="animate-slide-up animation-delay-200">
                <GlobalStatusCard
                    status={globalStatus}
                    onStatusChange={handleGlobalStatusChange}
                />
            </div>

            {/* Calendar Management with fade and slide */}
            <div className={`transition-all duration-500 transform ${
                globalStatus === GlobalStatus.CLOSED
                    ? 'opacity-50 scale-98'
                    : 'opacity-100 scale-100'
            } animate-slide-up animation-delay-300`}>
                <AvailabilityCalendar
                    specialDates={specialDates}
                    onDateUpdate={handleDateStatusUpdate}
                    onMonthChange={fetchMonthData} // Pass the fetch handler
                    isLoading={isCalendarLoading} // Pass loading state
                    isDisabled={globalStatus === GlobalStatus.CLOSED}
                />
            </div>

            {/* Warning Message with bounce animation */}
            {globalStatus === GlobalStatus.CLOSED && (
                <div className="bg-gradient-to-r from-amber-50 to-orange-50 border-l-4 border-amber-500 rounded-xl p-6 shadow-sm animate-bounce-in">
                    <div className="flex items-start gap-4">
                        <div className="flex-shrink-0 animate-wiggle">
                            <svg className="w-6 h-6 text-amber-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                            </svg>
                        </div>
                        <div>
                            <h3 className="text-amber-900 font-bold mb-1">Service Currently Closed</h3>
                            <p className="text-amber-800 text-sm">
                                Your catering service is marked as <strong>Closed Globally</strong>. The calendar settings will not apply until you reopen your service.
                            </p>
                        </div>
                    </div>
                </div>
            )}

            <style>{`
                @keyframes fade-in {
                    from {
                        opacity: 0;
                    }
                    to {
                        opacity: 1;
                    }
                }

                @keyframes slide-in-left {
                    from {
                        opacity: 0;
                        transform: translateX(-20px);
                    }
                    to {
                        opacity: 1;
                        transform: translateX(0);
                    }
                }

                @keyframes slide-up {
                    from {
                        opacity: 0;
                        transform: translateY(20px);
                    }
                    to {
                        opacity: 1;
                        transform: translateY(0);
                    }
                }

                @keyframes bounce-in {
                    0% {
                        opacity: 0;
                        transform: scale(0.9) translateY(-10px);
                    }
                    50% {
                        transform: scale(1.02) translateY(0);
                    }
                    100% {
                        opacity: 1;
                        transform: scale(1) translateY(0);
                    }
                }

                @keyframes wiggle {
                    0%, 100% {
                        transform: rotate(0deg);
                    }
                    25% {
                        transform: rotate(-5deg);
                    }
                    75% {
                        transform: rotate(5deg);
                    }
                }

                @keyframes pulse-slow {
                    0%, 100% {
                        opacity: 1;
                    }
                    50% {
                        opacity: 0.8;
                    }
                }

                .animate-fade-in {
                    animation: fade-in 0.5s ease-out;
                }

                .animate-slide-in-left {
                    animation: slide-in-left 0.6s ease-out;
                }

                .animate-slide-up {
                    animation: slide-up 0.6s ease-out;
                }

                .animate-bounce-in {
                    animation: bounce-in 0.6s ease-out;
                }

                .animate-wiggle {
                    animation: wiggle 2s ease-in-out infinite;
                }

                .animate-pulse-slow {
                    animation: pulse-slow 3s ease-in-out infinite;
                }

                .animation-delay-100 {
                    animation-delay: 0.1s;
                    animation-fill-mode: both;
                }

                .animation-delay-200 {
                    animation-delay: 0.2s;
                    animation-fill-mode: both;
                }

                .animation-delay-300 {
                    animation-delay: 0.3s;
                    animation-fill-mode: both;
                }

                .scale-98 {
                    transform: scale(0.98);
                }
            `}</style>
        </div>
    );
}
