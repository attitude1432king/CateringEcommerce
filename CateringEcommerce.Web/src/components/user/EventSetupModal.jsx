/**
 * EventSetupModal - Guest Segmentation (MANDATORY BEFORE PACKAGES)
 *
 * This modal MUST be completed before users can select packages.
 * Enforces guest category breakdown and validation.
 * Fetches categories dynamically from backend based on catering's supported food types.
 */
import React, { useState, useEffect } from 'react';
import { useEvent, GUEST_CATEGORIES, CATEGORY_CONFIG } from '../../contexts/EventContext';
import { fetchApi } from '../../services/apiUtils';
import { useAppSettings } from '../../contexts/AppSettingsContext';

const getMinBookingDate = (minAdvanceBookingDays) => {
    const minDate = new Date();
    minDate.setHours(0, 0, 0, 0);
    minDate.setDate(minDate.getDate() + minAdvanceBookingDays);
    return minDate.toISOString().split('T')[0];
};

export default function EventSetupModal({ isOpen, onClose, onComplete, cateringId }) {
    const { getInt } = useAppSettings();
    const {
        eventData,
        updateEventDetails,
        updateGuestCategory,
        completeEventSetup
    } = useEvent();

    // Dynamic categories from backend
    const [availableCategories, setAvailableCategories] = useState([]);
    const [minGuests, setMinGuests] = useState(50);
    const [isLoadingCategories, setIsLoadingCategories] = useState(false);

    const [formData, setFormData] = useState({
        eventName: eventData.eventName || '',
        eventDate: eventData.eventDate || '',
        totalGuests: eventData.totalGuests || 50
    });

    const [guestBreakdown, setGuestBreakdown] = useState({});
    const [errors, setErrors] = useState({});
    const minAdvanceBookingDays = getInt('BUSINESS.MIN_ADVANCE_BOOKING_DAYS', 5);
    const minBookingDate = getMinBookingDate(minAdvanceBookingDays);

    // Fetch available categories from backend when modal opens
    useEffect(() => {
        if (isOpen && cateringId) {
            fetchCateringCategories();
        }
    }, [isOpen, cateringId]);

    const fetchCateringCategories = async () => {
        try {
            setIsLoadingCategories(true);
            const response = await fetchApi(`/User/Home/Catering/${cateringId}/GuestCategories`, 'GET');

            if (response.success && response.data) {
                const { supportedCategories, minimumGuests, defaultGuests } = response.data;

                setAvailableCategories(supportedCategories || []);
                setMinGuests(minimumGuests || 50);

                // Initialize guest breakdown with 0 for all categories
                const initialBreakdown = {};
                supportedCategories.forEach(cat => {
                    initialBreakdown[cat.categoryId] = 0;
                });

                // Set all guests to first category by default
                if (supportedCategories.length > 0) {
                    initialBreakdown[supportedCategories[0].categoryId] = defaultGuests || minimumGuests || 50;
                }

                setGuestBreakdown(initialBreakdown);
                setFormData(prev => ({
                    ...prev,
                    totalGuests: defaultGuests || minimumGuests || 50
                }));
            }
        } catch (error) {
            console.error('Error fetching guest categories:', error);
            // Fallback to default categories
            setAvailableCategories([{
                categoryId: 0,
                categoryName: 'Regular',
                description: 'Standard vegetarian/non-vegetarian meals'
            }]);
            setGuestBreakdown({ 0: 50 });
        } finally {
            setIsLoadingCategories(false);
        }
    };

    // Sync with context when modal opens
    useEffect(() => {
        if (isOpen) {
            setFormData({
                eventName: eventData.eventName || '',
                eventDate: eventData.eventDate || '',
                totalGuests: eventData.totalGuests || minGuests || 50
            });
            // Only sync if we have existing data
            if (Object.keys(eventData.guestCategories || {}).length > 0) {
                setGuestBreakdown(eventData.guestCategories);
            }
        }
    }, [isOpen, eventData, minGuests]);

    // Calculate current total from breakdown
    const currentTotal = Object.values(guestBreakdown).reduce((sum, count) => sum + count, 0);
    const isValid = currentTotal === formData.totalGuests && formData.totalGuests >= minGuests;
    const difference = formData.totalGuests - currentTotal;

    // Handle form field changes
    const handleFormChange = (field, value) => {
        setFormData(prev => ({
            ...prev,
            [field]: value
        }));

        if (field === 'totalGuests') {
            const newTotal = parseInt(value) || minGuests;
            // Auto-adjust first category when total changes
            if (availableCategories.length > 0) {
                const firstCategoryId = availableCategories[0].categoryId;
                const otherCategoriesTotal = Object.entries(guestBreakdown)
                    .filter(([key]) => key != firstCategoryId)
                    .reduce((sum, [, count]) => sum + count, 0);

                setGuestBreakdown(prev => ({
                    ...prev,
                    [firstCategoryId]: Math.max(0, newTotal - otherCategoriesTotal)
                }));
            }
        }
    };

    // Handle guest category changes
    const handleCategoryChange = (category, value) => {
        const count = Math.max(0, parseInt(value) || 0);
        setGuestBreakdown(prev => ({
            ...prev,
            [category]: count
        }));
    };

    // Quick distribute remaining guests to first category
    const distributeRemaining = () => {
        if (difference > 0 && availableCategories.length > 0) {
            const firstCategoryId = availableCategories[0].categoryId;
            setGuestBreakdown(prev => ({
                ...prev,
                [firstCategoryId]: (prev[firstCategoryId] || 0) + difference
            }));
        }
    };

    // Handle submit
    const handleSubmit = () => {
        // Validate minimum guests
        if (formData.totalGuests < minGuests) {
            setErrors({ totalGuests: `Minimum ${minGuests} guests required for event catering` });
            return;
        }

        if (!formData.eventDate || formData.eventDate < minBookingDate) {
            setErrors({ eventDate: `Event date must be at least ${minAdvanceBookingDays} days in advance` });
            return;
        }

        // Validate guest totals match
        if (!isValid) {
            setErrors({ breakdown: 'Guest category totals must equal total guest count' });
            return;
        }

        // Validate at least one category has guests
        const hasGuests = Object.values(guestBreakdown).some(count => count > 0);
        if (!hasGuests) {
            setErrors({ breakdown: 'At least one guest category must have guests' });
            return;
        }

        // Update context
        updateEventDetails({
            eventName: formData.eventName,
            eventDate: formData.eventDate,
            totalGuests: formData.totalGuests
        });

        Object.entries(guestBreakdown).forEach(([category, count]) => {
            updateGuestCategory(category, count);
        });

        // Complete setup
        if (completeEventSetup()) {
            onComplete && onComplete();
            onClose();
        }
    };

    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 bg-black bg-opacity-70 z-50 flex items-center justify-center p-4 overflow-y-auto">
            <div className="bg-white rounded-2xl max-w-4xl w-full max-h-[95vh] overflow-hidden flex flex-col my-8">
                {/* Header */}
                <div className="bg-gradient-to-r from-indigo-600 to-purple-600 text-white p-6">
                    <div className="flex items-center justify-between">
                        <div>
                            <h2 className="text-2xl font-bold mb-2">Event Setup & Guest Segmentation</h2>
                            <p className="text-indigo-100 text-sm">
                                ⚡ Required before package selection | Ensures accurate kitchen instructions
                            </p>
                        </div>
                        <button
                            onClick={onClose}
                            className="text-white hover:bg-white/20 rounded-full p-2 transition-colors"
                        >
                            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                            </svg>
                        </button>
                    </div>
                </div>

                {/* Content */}
                <div className="flex-1 overflow-y-auto p-6 space-y-6">
                    {/* Event Details */}
                    <div className="bg-gray-50 rounded-xl p-6 border-2 border-gray-200">
                        <h3 className="text-lg font-bold text-neutral-900 mb-4 flex items-center gap-2">
                            <svg className="w-6 h-6 text-indigo-600" fill="currentColor" viewBox="0 0 20 20">
                                <path fillRule="evenodd" d="M6 2a1 1 0 00-1 1v1H4a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V6a2 2 0 00-2-2h-1V3a1 1 0 10-2 0v1H7V3a1 1 0 00-1-1zm0 5a1 1 0 000 2h8a1 1 0 100-2H6z" clipRule="evenodd" />
                            </svg>
                            Event Details
                        </h3>

                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <div>
                                <label className="block text-sm font-semibold text-neutral-700 mb-2">
                                    Event Name <span className="text-gray-400">(Optional)</span>
                                </label>
                                <input
                                    type="text"
                                    value={formData.eventName}
                                    onChange={(e) => handleFormChange('eventName', e.target.value)}
                                    placeholder="e.g., Raj & Priya Wedding"
                                    className="w-full px-4 py-3 border-2 border-gray-300 rounded-lg focus:outline-none focus:border-indigo-500"
                                />
                            </div>

                            <div>
                                <label className="block text-sm font-semibold text-neutral-700 mb-2">
                                    Event Date <span className="text-red-500">*</span>
                                </label>
                                <input
                                    type="date"
                                    value={formData.eventDate}
                                    onChange={(e) => handleFormChange('eventDate', e.target.value)}
                                    min={minBookingDate}
                                    className="w-full px-4 py-3 border-2 border-gray-300 rounded-lg focus:outline-none focus:border-indigo-500"
                                    required
                                />
                                {errors.eventDate && <p className="text-xs text-red-600 mt-2">{errors.eventDate}</p>}
                            </div>
                        </div>
                    </div>

                    {/* Total Guests */}
                    <div className="bg-gradient-to-r from-blue-50 to-indigo-50 rounded-xl p-6 border-2 border-blue-200">
                        <label className="block text-lg font-bold text-neutral-900 mb-3">
                            Total Guest Count <span className="text-red-500">*</span>
                        </label>
                        <div className="flex items-center gap-4">
                            <button
                                onClick={() => handleFormChange('totalGuests', Math.max(50, formData.totalGuests - 10))}
                                className="w-12 h-12 flex items-center justify-center bg-white border-2 border-indigo-300 rounded-lg hover:bg-indigo-50 transition-colors"
                            >
                                <svg className="w-6 h-6 text-indigo-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 12H4" />
                                </svg>
                            </button>
                            <input
                                type="number"
                                value={formData.totalGuests}
                                onChange={(e) => handleFormChange('totalGuests', e.target.value)}
                                min={minGuests}
                                className="flex-1 text-center px-4 py-3 border-2 border-indigo-300 rounded-lg font-bold text-2xl text-indigo-600 focus:outline-none focus:border-indigo-500"
                            />
                            <button
                                onClick={() => handleFormChange('totalGuests', formData.totalGuests + 10)}
                                className="w-12 h-12 flex items-center justify-center bg-white border-2 border-indigo-300 rounded-lg hover:bg-indigo-50 transition-colors"
                            >
                                <svg className="w-6 h-6 text-indigo-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                                </svg>
                            </button>
                        </div>
                        <p className="text-sm text-neutral-600 mt-2 text-center">
                            Minimum {minGuests} guests for event catering service
                        </p>
                    </div>

                    {/* Guest Category Breakdown */}
                    <div className="bg-white rounded-xl p-6 border-2 border-gray-300">
                        <div className="flex items-center justify-between mb-4">
                            <h3 className="text-lg font-bold text-neutral-900">
                                Guest Category Breakdown <span className="text-red-500">*</span>
                            </h3>
                            <div className={`px-4 py-2 rounded-full font-bold text-sm ${
                                isValid
                                    ? 'bg-green-100 text-green-700'
                                    : difference > 0
                                    ? 'bg-orange-100 text-orange-700 animate-pulse'
                                    : 'bg-red-100 text-red-700 animate-pulse'
                            }`}>
                                {currentTotal} / {formData.totalGuests} assigned
                                {difference !== 0 && ` (${difference > 0 ? '+' : ''}${difference})`}
                            </div>
                        </div>

                        {/* Validation Message */}
                        {!isValid && (
                            <div className={`mb-4 p-4 rounded-lg border-2 ${
                                difference > 0
                                    ? 'bg-orange-50 border-orange-300'
                                    : 'bg-red-50 border-red-300'
                            }`}>
                                <div className="flex items-start gap-3">
                                    <svg className={`w-6 h-6 mt-0.5 ${difference > 0 ? 'text-primary' : 'text-red-600'}`} fill="currentColor" viewBox="0 0 20 20">
                                        <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                                    </svg>
                                    <div className="flex-1">
                                        <p className={`font-semibold ${difference > 0 ? 'text-orange-800' : 'text-red-800'}`}>
                                            {difference > 0
                                                ? `${difference} guest${difference !== 1 ? 's' : ''} remaining to assign`
                                                : `Over by ${Math.abs(difference)} guest${Math.abs(difference) !== 1 ? 's' : ''}`
                                            }
                                        </p>
                                        {difference > 0 && availableCategories.length > 0 && (
                                            <button
                                                onClick={distributeRemaining}
                                                className="mt-2 text-sm text-orange-700 hover:text-orange-800 font-medium underline"
                                            >
                                                Add remaining to {availableCategories[0]?.categoryName || 'first'} category →
                                            </button>
                                        )}
                                    </div>
                                </div>
                            </div>
                        )}

                        {/* Category Inputs */}
                        {isLoadingCategories ? (
                            <div className="text-center py-8">
                                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600 mx-auto"></div>
                                <p className="text-neutral-600 mt-3">Loading guest categories...</p>
                            </div>
                        ) : (
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                {availableCategories.map((category) => (
                                    <div key={category.categoryId} className="bg-gray-50 rounded-lg p-4 border-2 border-gray-200">
                                        <div className="flex items-center justify-between mb-3">
                                            <div className="flex items-center gap-2">
                                                <div>
                                                    <h4 className="font-bold text-neutral-900">{category.categoryName}</h4>
                                                    <p className="text-xs text-neutral-600">{category.description}</p>
                                                </div>
                                            </div>
                                        </div>
                                        <input
                                            type="number"
                                            value={guestBreakdown[category.categoryId] || 0}
                                            onChange={(e) => handleCategoryChange(category.categoryId, e.target.value)}
                                            min="0"
                                            max={formData.totalGuests}
                                            className="w-full px-4 py-2 border-2 border-gray-300 rounded-lg font-bold text-lg text-center focus:outline-none focus:border-indigo-500"
                                        />
                                    </div>
                                ))}
                            </div>
                        )}

                        {/* Info Banner */}
                        <div className="mt-4 bg-blue-50 border border-blue-200 rounded-lg p-4">
                            <div className="flex items-start gap-3">
                                <svg className="w-5 h-5 text-blue-600 mt-0.5" fill="currentColor" viewBox="0 0 20 20">
                                    <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" />
                                </svg>
                                <div className="flex-1 text-sm text-blue-800">
                                    <p className="font-semibold mb-1">Why guest segmentation?</p>
                                    <ul className="list-disc list-inside space-y-1">
                                        <li>Ensures accurate pricing (different categories may have different costs)</li>
                                        <li>Kitchen receives correct dietary instructions</li>
                                        <li>Prevents food-related issues at your event</li>
                                    </ul>
                                </div>
                            </div>
                        </div>
                    </div>

                    {/* Error Display */}
                    {errors.breakdown && (
                        <div className="bg-red-50 border-l-4 border-red-500 p-4 rounded">
                            <p className="text-red-800 font-semibold">{errors.breakdown}</p>
                        </div>
                    )}
                </div>

                {/* Footer */}
                <div className="border-t-2 border-gray-200 bg-gray-50 p-6">
                    <div className="flex items-center justify-between gap-4">
                        <div className="text-sm text-neutral-600">
                            {isValid ? (
                                <div className="flex items-center gap-2 text-green-600">
                                    <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                                        <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                                    </svg>
                                    <span className="font-semibold">Ready to proceed!</span>
                                </div>
                            ) : (
                                <span>Complete guest breakdown to continue</span>
                            )}
                        </div>
                        <div className="flex gap-3">
                            <button
                                onClick={onClose}
                                className="px-6 py-3 border-2 border-gray-300 rounded-xl font-bold text-neutral-700 hover:bg-gray-100 transition-colors"
                            >
                                Cancel
                            </button>
                            <button
                                onClick={handleSubmit}
                                disabled={!isValid || !formData.eventDate}
                                className={`px-8 py-3 rounded-xl font-bold text-white transition-all ${
                                    isValid && formData.eventDate
                                        ? 'bg-gradient-to-r from-indigo-600 to-purple-600 hover:shadow-xl transform hover:scale-105'
                                        : 'bg-gray-300 cursor-not-allowed'
                                }`}
                            >
                                Continue to Packages →
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}
