/*
========================================
File: src/components/owner/dashboard/availability/AvailabilityCalendar.jsx
========================================
A custom calendar component to manage day-wise status.
*/
import React, { useState } from 'react';
import DateStatusModal from './DateStatusModal';
import { DAYS, MONTHS } from '../../../../utils/staticDropDownData';

// Helper to get days in month
const getDaysInMonth = (year, month) => new Date(year, month + 1, 0).getDate();
const getFirstDayOfMonth = (year, month) => new Date(year, month, 1).getDay();

export default function AvailabilityCalendar({ specialDates, onDateUpdate }) {
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

    const handleDateClick = (day) => {
        const dateStr = `${year}-${String(month + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
        const currentStatus = specialDates[dateStr] || { status: 'OPEN', note: '' };

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
            days.push(<div key={`empty-${i}`} className="h-24 md:h-32 bg-neutral-50 border-r border-b border-neutral-100"></div>);
        }

        for (let day = 1; day <= daysInMonth; day++) {
            const dateStr = `${year}-${String(month + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
            const isToday = dateStr === today.toISOString().split('T')[0];
            const dateInfo = specialDates[dateStr];

            let statusColor = 'bg-white hover:bg-neutral-50'; // Default Open
            let statusBadge = null;

            if (dateInfo) {
                if (dateInfo.status === 'CLOSED') {
                    statusColor = 'bg-red-50 hover:bg-red-100 border-red-200';
                    statusBadge = <span className="text-[10px] font-bold text-red-600 bg-red-100 px-1.5 py-0.5 rounded">CLOSED</span>;
                } else if (dateInfo.status === 'FULLY_BOOKED') {
                    statusColor = 'bg-orange-50 hover:bg-orange-100 border-orange-200';
                    statusBadge = <span className="text-[10px] font-bold text-orange-600 bg-orange-100 px-1.5 py-0.5 rounded">FULL</span>;
                }
            }

            days.push(
                <div
                    key={day}
                    onClick={() => handleDateClick(day)}
                    className={`h-24 md:h-32 border-r border-b border-neutral-200 p-2 cursor-pointer transition-colors relative flex flex-col justify-between ${statusColor}`}
                >
                    <div className="flex justify-between items-start">
                        <span className={`text-sm font-medium ${isToday ? 'bg-rose-600 text-white w-6 h-6 flex items-center justify-center rounded-full' : 'text-neutral-700'}`}>
                            {day}
                        </span>
                        {statusBadge}
                    </div>
                    {dateInfo && dateInfo.note && (
                        <p className="text-xs text-neutral-500 truncate mt-1" title={dateInfo.note}>{dateInfo.note}</p>
                    )}
                </div>
            );
        }
        return days;
    };

    return (
        <div className="bg-white rounded-xl shadow-sm border border-neutral-200 overflow-hidden">
            {/* Calendar Header */}
            <div className="p-4 border-b flex justify-between items-center bg-white">
                <h2 className="text-lg font-bold text-neutral-800">{MONTHS[month]} {year}</h2>
                <div className="flex gap-2">
                    <button onClick={handlePrevMonth} className="p-2 hover:bg-neutral-100 rounded-full text-neutral-600">
                        <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor"><path fillRule="evenodd" d="M12.707 5.293a1 1 0 010 1.414L9.414 10l3.293 3.293a1 1 0 01-1.414 1.414l-4-4a1 1 0 010-1.414l4-4a1 1 0 011.414 0z" clipRule="evenodd" /></svg>
                    </button>
                    <button onClick={handleNextMonth} className="p-2 hover:bg-neutral-100 rounded-full text-neutral-600">
                        <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor"><path fillRule="evenodd" d="M7.293 14.707a1 1 0 010-1.414L10.586 10 7.293 6.707a1 1 0 011.414-1.414l4 4a1 1 0 010 1.414l-4 4a1 1 0 01-1.414 0z" clipRule="evenodd" /></svg>
                    </button>
                </div>
            </div>

            {/* Weekday Header */}
            <div className="grid grid-cols-7 border-b border-neutral-200 bg-neutral-50">
                {DAYS.map(day => (
                    <div key={day} className="py-2 text-center text-xs font-semibold text-neutral-500 uppercase tracking-wide">
                        {day}
                    </div>
                ))}
            </div>

            {/* Calendar Grid */}
            <div className="grid grid-cols-7 border-l border-t border-neutral-200">
                {renderCalendarDays()}
            </div>

            {/* Legend */}
            <div className="p-4 bg-neutral-50 flex gap-6 text-sm border-t border-neutral-200">
                <div className="flex items-center gap-2"><div className="w-3 h-3 bg-white border border-neutral-300 rounded"></div> <span className="text-neutral-600">Open</span></div>
                <div className="flex items-center gap-2"><div className="w-3 h-3 bg-red-100 border border-red-200 rounded"></div> <span className="text-neutral-600">Closed</span></div>
                <div className="flex items-center gap-2"><div className="w-3 h-3 bg-orange-100 border border-orange-200 rounded"></div> <span className="text-neutral-600">Fully Booked</span></div>
            </div>

            <DateStatusModal
                isOpen={isModalOpen}
                onClose={() => setIsModalOpen(false)}
                dateData={selectedDate}
                onSave={handleModalSave}
            />
        </div>
    );
}