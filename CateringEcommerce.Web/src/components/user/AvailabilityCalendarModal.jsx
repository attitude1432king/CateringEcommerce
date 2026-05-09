import React, { useEffect, useMemo, useState } from 'react';

const DAY_LABELS = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

const formatYmd = (date) => {
    const year = date.getFullYear();
    const month = `${date.getMonth() + 1}`.padStart(2, '0');
    const day = `${date.getDate()}`.padStart(2, '0');
    return `${year}-${month}-${day}`;
};

const isSameDay = (left, right) =>
    left &&
    right &&
    left.getFullYear() === right.getFullYear() &&
    left.getMonth() === right.getMonth() &&
    left.getDate() === right.getDate();

const startOfMonth = (date) => new Date(date.getFullYear(), date.getMonth(), 1);

export default function AvailabilityCalendarModal({
    isOpen,
    onClose,
    minDate,
    selectedDate,
    blockedDates = [],
    onSelectDate,
    onMonthChange,
    isChecking = false,
    availabilityResult = null,
    onProceedToBooking
}) {
    const [visibleMonth, setVisibleMonth] = useState(startOfMonth(selectedDate || minDate || new Date()));
    const blockedDateSet = useMemo(() => new Set(blockedDates), [blockedDates]);

    useEffect(() => {
        if (!isOpen) {
            return;
        }

        const targetMonth = startOfMonth(selectedDate || minDate || new Date());
        setVisibleMonth(targetMonth);
    }, [isOpen]);

    useEffect(() => {
        if (!isOpen) {
            return;
        }

        onMonthChange?.(visibleMonth);
    }, [isOpen, onMonthChange, visibleMonth]);

    if (!isOpen) {
        return null;
    }

    const monthStart = startOfMonth(visibleMonth);
    const calendarStart = new Date(monthStart);
    calendarStart.setDate(monthStart.getDate() - monthStart.getDay());

    const days = Array.from({ length: 42 }, (_, index) => {
        const value = new Date(calendarStart);
        value.setDate(calendarStart.getDate() + index);
        return value;
    });

    const canGoPrevious = startOfMonth(visibleMonth) > startOfMonth(minDate);

    const resultTone = availabilityResult?.isAvailable
        ? 'border-emerald-200 bg-emerald-50 text-emerald-800'
        : 'border-rose-200 bg-rose-50 text-rose-700';

    const handleDateClick = (day) => {
        setVisibleMonth(startOfMonth(day));
        onSelectDate?.(day);
    };

    return (
        <div className="fixed inset-0 z-[80] bg-black/60 backdrop-blur-sm flex items-end md:items-center justify-center p-0 md:p-4">
            <div className="w-full md:max-w-3xl bg-white rounded-t-3xl md:rounded-3xl shadow-2xl overflow-hidden max-h-[92vh] flex flex-col">
                <div className="px-5 py-4 md:px-6 md:py-5 border-b border-neutral-200 bg-gradient-to-r from-orange-50 via-white to-amber-50">
                    <div className="flex items-start justify-between gap-4">
                        <div>
                            <p className="text-xs font-semibold uppercase tracking-[0.2em] text-primary">Check Availability</p>
                            <h2 className="text-xl md:text-2xl font-bold text-neutral-900 mt-1">Choose your event date</h2>
                            <p className="text-sm text-neutral-600 mt-1">
                                Booking opens from {minDate.toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' })}.
                            </p>
                        </div>
                        <button
                            type="button"
                            onClick={onClose}
                            className="shrink-0 rounded-full p-2 text-neutral-500 hover:bg-white hover:text-neutral-900 transition-colors"
                            aria-label="Close availability calendar"
                        >
                            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                            </svg>
                        </button>
                    </div>
                </div>

                <div className="flex-1 overflow-y-auto p-5 md:p-6 space-y-5">
                    <div className="flex items-center justify-between">
                        <button
                            type="button"
                            onClick={() => canGoPrevious && setVisibleMonth(new Date(visibleMonth.getFullYear(), visibleMonth.getMonth() - 1, 1))}
                            disabled={!canGoPrevious}
                            className={`rounded-full border px-4 py-2 text-sm font-semibold transition-colors ${
                                canGoPrevious
                                    ? 'border-neutral-200 text-neutral-700 hover:bg-neutral-50'
                                    : 'border-neutral-100 text-neutral-300 cursor-not-allowed'
                            }`}
                        >
                            Previous
                        </button>
                        <div className="text-center">
                            <div className="text-lg font-bold text-neutral-900">
                                {visibleMonth.toLocaleDateString('en-IN', { month: 'long', year: 'numeric' })}
                            </div>
                            <div className="text-xs text-neutral-500 mt-1">Closed and full dates are disabled</div>
                        </div>
                        <button
                            type="button"
                            onClick={() => setVisibleMonth(new Date(visibleMonth.getFullYear(), visibleMonth.getMonth() + 1, 1))}
                            className="rounded-full border border-neutral-200 px-4 py-2 text-sm font-semibold text-neutral-700 hover:bg-neutral-50 transition-colors"
                        >
                            Next
                        </button>
                    </div>

                    <div className="grid grid-cols-7 gap-2">
                        {DAY_LABELS.map((label) => (
                            <div key={label} className="text-center text-xs font-semibold uppercase tracking-wide text-neutral-500 py-1">
                                {label}
                            </div>
                        ))}
                        {days.map((day) => {
                            const ymd = formatYmd(day);
                            const isOutsideMonth = day.getMonth() !== visibleMonth.getMonth();
                            const isBeforeMinDate = formatYmd(day) < formatYmd(minDate);
                            const isBlocked = blockedDateSet.has(ymd);
                            const isDisabled = isOutsideMonth || isBeforeMinDate || isBlocked;
                            const isSelected = isSameDay(day, selectedDate);

                            return (
                                <button
                                    key={ymd}
                                    type="button"
                                    onClick={() => !isDisabled && handleDateClick(day)}
                                    disabled={isDisabled}
                                    className={`aspect-square rounded-2xl border text-sm font-semibold transition-all ${
                                        isSelected
                                            ? 'border-orange-500 bg-primary text-white shadow-lg shadow-orange-200'
                                            : isDisabled
                                                ? 'border-neutral-100 bg-neutral-50 text-neutral-300 cursor-not-allowed'
                                                : 'border-neutral-200 bg-white text-neutral-800 hover:border-orange-300 hover:bg-primary/5'
                                    }`}
                                >
                                    {day.getDate()}
                                </button>
                            );
                        })}
                    </div>

                    <div className="rounded-2xl border border-neutral-200 bg-neutral-50 p-4">
                        <div className="flex flex-wrap gap-3 text-xs font-medium text-neutral-600">
                            <span className="inline-flex items-center gap-2">
                                <span className="w-3 h-3 rounded-full bg-white border border-neutral-300" />
                                Selectable
                            </span>
                            <span className="inline-flex items-center gap-2">
                                <span className="w-3 h-3 rounded-full bg-neutral-200 border border-neutral-200" />
                                Disabled
                            </span>
                            <span className="inline-flex items-center gap-2">
                                <span className="w-3 h-3 rounded-full bg-primary border border-orange-500" />
                                Selected
                            </span>
                        </div>
                    </div>

                    {selectedDate && (
                        <div className="rounded-2xl border border-neutral-200 bg-white p-4">
                            <div className="text-sm font-semibold text-neutral-800">
                                Selected date: {selectedDate.toLocaleDateString('en-IN', { weekday: 'long', day: '2-digit', month: 'long', year: 'numeric' })}
                            </div>
                            <div className="text-xs text-neutral-500 mt-1">
                                We check live availability as soon as you pick a valid date.
                            </div>
                        </div>
                    )}

                    {isChecking && (
                        <div className="rounded-2xl border border-orange-200 bg-orange-50 p-4 text-sm font-medium text-orange-800">
                            Checking live availability...
                        </div>
                    )}

                    {availabilityResult && !isChecking && (
                        <div className={`rounded-2xl border p-4 ${resultTone}`}>
                            <div className="text-base font-bold">{availabilityResult.message}</div>
                            <div className="text-sm mt-1">
                                {availabilityResult.isAvailable
                                    ? `${availabilityResult.availableSlots} booking slots remaining for this date.`
                                    : 'This date cannot be booked right now. Please choose another date.'}
                            </div>
                        </div>
                    )}
                </div>

                <div className="border-t border-neutral-200 bg-white px-5 py-4 md:px-6 flex flex-col-reverse md:flex-row gap-3 md:items-center md:justify-end">
                    <button
                        type="button"
                        onClick={onClose}
                        className="px-5 py-3 rounded-xl border border-neutral-200 text-neutral-700 font-semibold hover:bg-neutral-50 transition-colors"
                    >
                        Close
                    </button>
                    <button
                        type="button"
                        onClick={onProceedToBooking}
                        disabled={!availabilityResult?.isAvailable}
                        className={`px-5 py-3 rounded-xl font-semibold transition-all ${
                            availabilityResult?.isAvailable
                                ? 'bg-gradient-to-r from-orange-600 to-orange-500 text-white shadow-lg hover:shadow-xl'
                                : 'bg-neutral-200 text-neutral-400 cursor-not-allowed'
                        }`}
                    >
                        Proceed to Booking
                    </button>
                </div>
            </div>
        </div>
    );
}
