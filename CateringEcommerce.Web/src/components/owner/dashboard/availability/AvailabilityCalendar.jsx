/*
========================================
File: src/components/owner/dashboard/availability/AvailabilityCalendar.jsx
Modern Redesign - ENYVORA Brand with Motion
========================================
A custom calendar component to manage day-wise status.
*/
import React, { useState, useEffect } from 'react';
import DateStatusModal from './DateStatusModal';
import { DAYS, MONTHS, AvailabilityStatus } from '../../../../utils/staticData';

// Helper to get days in month
const getDaysInMonth = (year, month) => new Date(year, month + 1, 0).getDate();
const getFirstDayOfMonth = (year, month) => new Date(year, month, 1).getDay();

export default function AvailabilityCalendar({ specialDates, onDateUpdate, onMonthChange, isLoading, isDisabled }) {
    const today = new Date();
    const [currentDate, setCurrentDate] = useState(new Date());
    const [selectedDate, setSelectedDate] = useState(null);
    const [isModalOpen, setIsModalOpen] = useState(false);

    const year = currentDate.getFullYear();
    const month = currentDate.getMonth();

    const daysInMonth = getDaysInMonth(year, month);
    const firstDay = getFirstDayOfMonth(year, month);

    const handlePrevMonth = () => setCurrentDate(new Date(year, month - 1, 1));
    const handleNextMonth = () => setCurrentDate(new Date(year, month + 1, 1));

    // Trigger onMonthChange when month/year changes
    useEffect(() => {
        if (onMonthChange) {
            onMonthChange(year, month);
        }
    }, [year, month, onMonthChange]);

    const handleDateClick = (day) => {
        const dateObj = new Date(year, month, day);
        dateObj.setHours(0, 0, 0, 0);

        const presentDay = new Date(dateObj.getFullYear(), dateObj.getMonth(), dateObj.getDate());
        const todayDate = new Date(today.getFullYear(), today.getMonth(), today.getDate());
        // Prevent clicking past dates
        if (presentDay < todayDate) {
            return;
        }

        const dateStr = `${year}-${String(month + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
        const currentStatus = specialDates[dateStr] || { status: AvailabilityStatus.OPEN, note: '' };

        setSelectedDate({ date: dateStr, ...currentStatus });
        setIsModalOpen(true);
    };

    const handleModalSave = (status, note) => {
        onDateUpdate(selectedDate.date, status, note);
        setIsModalOpen(false);
    };

    const renderCalendarDays = () => {
        const days = [];

        // Empty slots for previous month
        for (let i = 0; i < firstDay; i++) {
            days.push(
                <div
                    key={`empty-${i}`}
                    className="h-28 bg-neutral-50 border border-neutral-100 animate-fade-in"
                    style={{ animationDelay: `${i * 20}ms` }}
                ></div>
            );
        }

        for (let day = 1; day <= daysInMonth; day++) {
            const dateStr = `${year}-${String(month + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
            const isToday = dateStr === today.toISOString().split('T')[0];
            const dateInfo = specialDates[dateStr];

            let statusColor = 'bg-white hover:bg-orange-50'; // Default Open
            let statusBadge = null;
            let borderColor = 'border-neutral-200';

            if (dateInfo) {
                if (dateInfo.status === AvailabilityStatus.CLOSED) {
                    statusColor = 'bg-red-50 hover:bg-red-100';
                    borderColor = 'border-red-200';
                    statusBadge = (
                        <span className="flex items-center gap-1 text-[10px] font-bold text-red-700 bg-red-100 px-2 py-1 rounded-full border border-red-200 animate-badge-in">
                            <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                            </svg>
                            CLOSED
                        </span>
                    );
                } else if (dateInfo.status === AvailabilityStatus.FULLY_BOOKED) {
                    statusColor = 'bg-orange-50 hover:bg-orange-100';
                    borderColor = 'border-orange-200';
                    statusBadge = (
                        <span className="flex items-center gap-1 text-[10px] font-bold text-orange-700 bg-orange-100 px-2 py-1 rounded-full border border-orange-200 animate-badge-in">
                            <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                            </svg>
                            FULL
                        </span>
                    );
                }
            }

            const delayIndex = firstDay + day - 1;

            days.push(
                <div
                    key={day}
                    onClick={() => handleDateClick(day)}
                    className={`h-28 border ${borderColor} p-3 ${isDisabled ? 'cursor-not-allowed opacity-60' : 'cursor-pointer'} transition-all duration-300 relative flex flex-col justify-between ${statusColor} group animate-scale-in hover:scale-105 hover:-translate-y-1 hover:shadow-lg hover:z-10`}
                    style={{ animationDelay: `${delayIndex * 20}ms` }}
                >
                    {/* Date Number */}
                    <div className="flex justify-between items-start">
                        {isToday ? (
                            <div className="flex items-center justify-center w-8 h-8 text-white text-sm font-bold rounded-full shadow-lg animate-pulse-ring" style={{ background: 'linear-gradient(135deg, #FF6B35 0%, #FF8C42 50%, #FFB627 100%)' }}>
                                {day}
                            </div>
                        ) : (
                            <span className="text-sm font-semibold text-neutral-700 group-hover:scale-110 transition-transform duration-300">
                                {day}
                            </span>
                        )}
                        {statusBadge}
                    </div>

                    {/* Note */}
                    {dateInfo && dateInfo.note && (
                        <div className="mt-auto animate-slide-up">
                            <p className="text-xs text-neutral-600 truncate font-medium" title={dateInfo.note}>
                                {dateInfo.note}
                            </p>
                        </div>
                    )}

                    {/* Hover Effect */}
                    {!isDisabled && (
                        <div className="absolute inset-0 opacity-0 group-hover:opacity-5 transition-opacity duration-300 pointer-events-none rounded" style={{ background: 'linear-gradient(135deg, #FF6B35, #FFB627)' }}></div>
                    )}

                    {/* Click Ripple Effect */}
                    {!isDisabled && (
                        <div className="absolute inset-0 overflow-hidden pointer-events-none rounded">
                            <div className="ripple"></div>
                        </div>
                    )}
                </div>
            );
        }
        return days;
    };

    return (
        <div className="bg-white rounded-2xl shadow-sm border border-neutral-100 overflow-hidden hover:shadow-md transition-shadow duration-300">

            {isLoading && (
                <div className="absolute inset-0 bg-white/50 z-10 flex justify-center items-center backdrop-blur-sm">
                    <div className="animate-spin rounded-full h-8 w-8 border-b-2" style={{ borderColor: 'var(--color-primary)' }}></div>
                </div>
            )}
        
            {/* Calendar Header */}
            <div className="p-6 border-b border-neutral-100 bg-neutral-50 animate-gradient-slide">
                <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
                    <div className="animate-slide-in-left">
                        <h2 className="text-2xl font-bold text-neutral-900">{MONTHS[month]} {year}</h2>
                        <p className="text-sm text-neutral-600 mt-1">Click on any date to set its availability</p>
                    </div>

                    <div className="flex items-center gap-2 animate-slide-in-right">
                        <button
                            onClick={handlePrevMonth}
                            className="p-2.5 hover:bg-white rounded-xl text-neutral-600 transition-all duration-300 border border-transparent hover:border-orange-200 transform hover:scale-110 active:scale-95"
                            style={{ '--hover-color': 'var(--color-primary)' }}
                            disabled={isDisabled}
                        >
                            <svg className="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
                            </svg>
                        </button>

                        <button
                            onClick={() => setCurrentDate(new Date())}
                            className="px-4 py-2.5 text-sm font-semibold bg-white hover:bg-orange-50 rounded-xl transition-all duration-300 border border-orange-200 transform hover:scale-105 active:scale-95 hover:shadow-md"
                            style={{ color: 'var(--color-primary)' }}
                            disabled={isDisabled}
                        >
                            Today
                        </button>

                        <button
                            onClick={handleNextMonth}
                            className="p-2.5 hover:bg-white rounded-xl text-neutral-600 transition-all duration-300 border border-transparent hover:border-orange-200 transform hover:scale-110 active:scale-95"
                            disabled={isDisabled}
                        >
                            <svg className="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                            </svg>
                        </button>
                    </div>
                </div>
            </div>

            {/* Weekday Header */}
            <div className="grid grid-cols-7 border-b border-neutral-200 bg-gradient-to-r from-neutral-50 to-neutral-100">
                {DAYS.map((day, index) => (
                    <div
                        key={day}
                        className="py-3 text-center text-xs font-bold text-neutral-600 uppercase tracking-wider animate-fade-in"
                        style={{ animationDelay: `${index * 50}ms` }}
                    >
                        {day}
                    </div>
                ))}
            </div>

            {/* Calendar Grid */}
            <div className="grid grid-cols-7">
                {renderCalendarDays()}
            </div>

            {/* Legend */}
            <div className="p-6 bg-neutral-50 border-t border-neutral-200">
                <div className="flex flex-wrap gap-6 text-sm">
                    {[
                        { color: 'bg-white border-2 border-neutral-300', label: 'Open' },
                        { color: 'bg-red-100 border-2 border-red-300', label: 'Closed' },
                        { color: 'bg-orange-100 border-2 border-orange-300', label: 'Fully Booked' },
                        { color: '', label: 'Today', rounded: true, gradient: true }
                    ].map((item, index) => (
                        <div
                            key={item.label}
                            className="flex items-center gap-2 animate-fade-in-up group cursor-default"
                            style={{ animationDelay: `${index * 100}ms` }}
                        >
                            <div
                                className={`w-4 h-4 ${item.color} ${item.rounded ? 'rounded-full' : 'rounded'} group-hover:scale-125 transition-transform duration-300`}
                                style={item.gradient ? { background: 'linear-gradient(135deg, #FF6B35 0%, #FF8C42 50%, #FFB627 100%)' } : undefined}
                            ></div>
                            <span className="text-neutral-700 font-medium">{item.label}</span>
                        </div>
                    ))}
                </div>
            </div>

            <DateStatusModal
                isOpen={isModalOpen}
                onClose={() => setIsModalOpen(false)}
                dateData={selectedDate}
                onSave={handleModalSave}
            />

            <style>{`
                @keyframes fade-in {
                    from { opacity: 0; }
                    to { opacity: 1; }
                }

                @keyframes scale-in {
                    from {
                        opacity: 0;
                        transform: scale(0.8);
                    }
                    to {
                        opacity: 1;
                        transform: scale(1);
                    }
                }

                @keyframes badge-in {
                    from {
                        opacity: 0;
                        transform: scale(0) rotate(-45deg);
                    }
                    to {
                        opacity: 1;
                        transform: scale(1) rotate(0deg);
                    }
                }

                @keyframes pulse-ring {
                    0%, 100% {
                        box-shadow: 0 0 0 0 rgba(255, 107, 53, 0.7);
                    }
                    50% {
                        box-shadow: 0 0 0 8px rgba(255, 107, 53, 0);
                    }
                }

                @keyframes slide-up {
                    from {
                        opacity: 0;
                        transform: translateY(5px);
                    }
                    to {
                        opacity: 1;
                        transform: translateY(0);
                    }
                }

                @keyframes gradient-slide {
                    0%, 100% {
                        background-position: 0% 50%;
                    }
                    50% {
                        background-position: 100% 50%;
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

                @keyframes slide-in-right {
                    from {
                        opacity: 0;
                        transform: translateX(20px);
                    }
                    to {
                        opacity: 1;
                        transform: translateX(0);
                    }
                }

                @keyframes fade-in-up {
                    from {
                        opacity: 0;
                        transform: translateY(10px);
                    }
                    to {
                        opacity: 1;
                        transform: translateY(0);
                    }
                }

                .animate-fade-in {
                    animation: fade-in 0.4s ease-out forwards;
                    opacity: 0;
                }

                .animate-scale-in {
                    animation: scale-in 0.3s ease-out forwards;
                    opacity: 0;
                }

                .animate-badge-in {
                    animation: badge-in 0.4s cubic-bezier(0.34, 1.56, 0.64, 1) forwards;
                }

                .animate-pulse-ring {
                    animation: pulse-ring 2s ease-in-out infinite;
                }

                .animate-slide-up {
                    animation: slide-up 0.3s ease-out;
                }

                .animate-gradient-slide {
                    background-size: 200% 200%;
                    animation: gradient-slide 3s ease infinite;
                }

                .animate-slide-in-left {
                    animation: slide-in-left 0.5s ease-out;
                }

                .animate-slide-in-right {
                    animation: slide-in-right 0.5s ease-out;
                }

                .animate-fade-in-up {
                    animation: fade-in-up 0.5s ease-out forwards;
                    opacity: 0;
                }
            `}</style>
        </div>
    );
}
