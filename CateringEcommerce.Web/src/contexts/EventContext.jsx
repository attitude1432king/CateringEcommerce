/**
 * EventContext - Guest Segmentation & Event Setup
 *
 * Manages event details and guest category breakdown for catering bookings.
 * This is MANDATORY before package selection.
 */
import React, { createContext, useContext, useState, useEffect } from 'react';

const EventContext = createContext();

export const useEvent = () => {
    const context = useContext(EventContext);
    if (!context) {
        throw new Error('useEvent must be used within EventProvider');
    }
    return context;
};

// Supported Guest Categories (Controlled List)
export const GUEST_CATEGORIES = {
    REGULAR: 'REGULAR',
    JAIN: 'JAIN',
    VEGAN: 'VEGAN',
    SATVIK: 'SATVIK'
};

// Category Display Config
export const CATEGORY_CONFIG = {
    REGULAR: {
        label: 'Regular',
        color: 'blue',
        icon: '🍽️',
        description: 'Standard vegetarian/non-vegetarian meals'
    },
    JAIN: {
        label: 'Jain',
        color: 'orange',
        icon: '🙏',
        description: 'No onion, garlic, root vegetables'
    },
    VEGAN: {
        label: 'Vegan',
        color: 'green',
        icon: '🌱',
        description: 'Plant-based, no dairy or animal products'
    },
    SATVIK: {
        label: 'Satvik',
        color: 'purple',
        icon: '🕉️',
        description: 'Pure vegetarian, no strong spices'
    }
};

export const EventProvider = ({ children }) => {
    // Event Setup State
    const [eventData, setEventData] = useState(() => {
        const saved = localStorage.getItem('eventSetup');
        return saved ? JSON.parse(saved) : {
            eventName: '',
            eventDate: '',
            totalGuests: 50,
            guestCategories: {
                REGULAR: 50,
                JAIN: 0,
                VEGAN: 0,
                SATVIK: 0
            },
            isSetupComplete: false
        };
    });

    // Persist to localStorage
    useEffect(() => {
        localStorage.setItem('eventSetup', JSON.stringify(eventData));
    }, [eventData]);

    // Update Event Details
    const updateEventDetails = (details) => {
        setEventData(prev => ({
            ...prev,
            ...details
        }));
    };

    // Update Guest Category Count
    const updateGuestCategory = (category, count) => {
        setEventData(prev => ({
            ...prev,
            guestCategories: {
                ...prev.guestCategories,
                [category]: Math.max(0, parseInt(count) || 0)
            }
        }));
    };

    // Validate Guest Totals
    const validateGuestTotals = () => {
        const sum = Object.values(eventData.guestCategories).reduce((acc, val) => acc + val, 0);
        return sum === eventData.totalGuests;
    };

    // Get Active Categories (with count > 0)
    const getActiveCategories = () => {
        return Object.entries(eventData.guestCategories)
            .filter(([_, count]) => count > 0)
            .map(([category, count]) => ({ category, count }));
    };

    // Complete Event Setup
    const completeEventSetup = () => {
        if (validateGuestTotals()) {
            setEventData(prev => ({
                ...prev,
                isSetupComplete: true
            }));
            return true;
        }
        return false;
    };

    // Reset Event Setup
    const resetEventSetup = () => {
        setEventData({
            eventName: '',
            eventDate: '',
            totalGuests: 50,
            guestCategories: {
                REGULAR: 50,
                JAIN: 0,
                VEGAN: 0,
                SATVIK: 0
            },
            isSetupComplete: false
        });
        localStorage.removeItem('eventSetup');
    };

    // Check if specific category is active
    const isCategoryActive = (category) => {
        return eventData.guestCategories[category] > 0;
    };

    // Get category count
    const getCategoryCount = (category) => {
        return eventData.guestCategories[category] || 0;
    };

    // Calculate total for a package across all active categories
    const calculatePackageTotal = (categoryPricing) => {
        let total = 0;
        Object.entries(eventData.guestCategories).forEach(([category, count]) => {
            if (count > 0 && categoryPricing[category]) {
                total += categoryPricing[category] * count;
            }
        });
        return total;
    };

    const value = {
        eventData,
        updateEventDetails,
        updateGuestCategory,
        validateGuestTotals,
        getActiveCategories,
        completeEventSetup,
        resetEventSetup,
        isCategoryActive,
        getCategoryCount,
        calculatePackageTotal,
        isSetupComplete: eventData.isSetupComplete
    };

    return (
        <EventContext.Provider value={value}>
            {children}
        </EventContext.Provider>
    );
};

export default EventContext;
