import { useState } from 'react';
import { Calendar, X } from 'lucide-react';

const INDIA_TIME_ZONE = 'Asia/Kolkata';

const getIndiaNow = () =>
    new Date(new Date().toLocaleString('en-US', { timeZone: INDIA_TIME_ZONE }));

const formatDateInIndia = (date) =>
    new Intl.DateTimeFormat('en-CA', {
        timeZone: INDIA_TIME_ZONE,
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
    }).format(date);

/**
 * Date Range Filter Component
 * Allows selecting date range with preset options
 */
const DateRangeFilter = ({ onDateChange, defaultRange = 'last30days' }) => {
    const [showCustom, setShowCustom] = useState(false);
    const [selectedRange, setSelectedRange] = useState(defaultRange);
    const [customDates, setCustomDates] = useState({
        from: '',
        to: '',
    });

    const presetRanges = [
        { value: 'today', label: 'Today' },
        { value: 'yesterday', label: 'Yesterday' },
        { value: 'last7days', label: 'Last 7 Days' },
        { value: 'last30days', label: 'Last 30 Days' },
        { value: 'thisMonth', label: 'This Month' },
        { value: 'lastMonth', label: 'Last Month' },
        { value: 'last3months', label: 'Last 3 Months' },
        { value: 'thisYear', label: 'This Year' },
        { value: 'custom', label: 'Custom Range' },
    ];

    const calculateDateRange = (range) => {
        const today = getIndiaNow();
        const year = today.getFullYear();
        const month = today.getMonth();
        const date = today.getDate();

        switch (range) {
            case 'today':
                return {
                    from: formatDateInIndia(new Date(year, month, date)),
                    to: formatDateInIndia(new Date(year, month, date)),
                };
            case 'yesterday':
                return {
                    from: formatDateInIndia(new Date(year, month, date - 1)),
                    to: formatDateInIndia(new Date(year, month, date - 1)),
                };
            case 'last7days':
                return {
                    from: formatDateInIndia(new Date(year, month, date - 7)),
                    to: formatDateInIndia(new Date(year, month, date)),
                };
            case 'last30days':
                return {
                    from: formatDateInIndia(new Date(year, month, date - 30)),
                    to: formatDateInIndia(new Date(year, month, date)),
                };
            case 'thisMonth':
                return {
                    from: formatDateInIndia(new Date(year, month, 1)),
                    to: formatDateInIndia(new Date(year, month, date)),
                };
            case 'lastMonth':
                return {
                    from: formatDateInIndia(new Date(year, month - 1, 1)),
                    to: formatDateInIndia(new Date(year, month, 0)),
                };
            case 'last3months':
                return {
                    from: formatDateInIndia(new Date(year, month - 3, date)),
                    to: formatDateInIndia(new Date(year, month, date)),
                };
            case 'thisYear':
                return {
                    from: formatDateInIndia(new Date(year, 0, 1)),
                    to: formatDateInIndia(new Date(year, month, date)),
                };
            default:
                return null;
        }
    };

    const handleRangeChange = (range) => {
        setSelectedRange(range);

        if (range === 'custom') {
            setShowCustom(true);
        } else {
            setShowCustom(false);
            const dates = calculateDateRange(range);
            if (dates && onDateChange) {
                onDateChange(dates.from, dates.to);
            }
        }
    };

    const handleCustomDateChange = (field, value) => {
        const newDates = { ...customDates, [field]: value };
        setCustomDates(newDates);

        if (newDates.from && newDates.to && onDateChange) {
            onDateChange(newDates.from, newDates.to);
        }
    };

    const handleClearCustom = () => {
        setCustomDates({ from: '', to: '' });
        setShowCustom(false);
        setSelectedRange('last30days');
        handleRangeChange('last30days');
    };

    return (
        <div className="bg-white rounded-lg border border-gray-200 p-4">
            <div className="flex items-center gap-2 mb-3">
                <Calendar className="w-5 h-5 text-gray-600" />
                <h3 className="text-sm font-semibold text-gray-900">Date Range</h3>
            </div>

            {/* Preset Ranges */}
            <div className="flex flex-wrap gap-2 mb-3">
                {presetRanges.map((range) => (
                    <button
                        key={range.value}
                        onClick={() => handleRangeChange(range.value)}
                        className={`px-3 py-1.5 text-sm font-medium rounded-lg transition-colors ${
                            selectedRange === range.value
                                ? 'bg-blue-600 text-white'
                                : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                        }`}
                    >
                        {range.label}
                    </button>
                ))}
            </div>

            {/* Custom Date Inputs */}
            {showCustom && (
                <div className="bg-gray-50 rounded-lg p-3 border border-gray-200">
                    <div className="flex items-center justify-between mb-2">
                        <p className="text-sm font-medium text-gray-700">Custom Date Range</p>
                        <button
                            onClick={handleClearCustom}
                            className="text-gray-400 hover:text-gray-600"
                            title="Clear custom range"
                        >
                            <X className="w-4 h-4" />
                        </button>
                    </div>
                    <div className="grid grid-cols-2 gap-3">
                        <div>
                            <label className="block text-xs font-medium text-gray-600 mb-1">
                                From Date
                            </label>
                            <input
                                type="date"
                                value={customDates.from}
                                onChange={(e) => handleCustomDateChange('from', e.target.value)}
                                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                            />
                        </div>
                        <div>
                            <label className="block text-xs font-medium text-gray-600 mb-1">
                                To Date
                            </label>
                            <input
                                type="date"
                                value={customDates.to}
                                onChange={(e) => handleCustomDateChange('to', e.target.value)}
                                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                            />
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default DateRangeFilter;
