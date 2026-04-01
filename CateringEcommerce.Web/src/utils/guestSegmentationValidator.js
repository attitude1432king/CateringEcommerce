/**
 * Guest Segmentation Validation Utilities
 *
 * Enforces business rules for guest category assignment,
 * package compatibility, and menu selections.
 */

import { GUEST_CATEGORIES } from '../contexts/EventContext';

/**
 * Validates guest totals match target
 */
export const validateGuestTotals = (guestCategories, totalGuests) => {
    const sum = Object.values(guestCategories).reduce((acc, val) => acc + (parseInt(val) || 0), 0);
    return {
        isValid: sum === totalGuests,
        currentTotal: sum,
        difference: totalGuests - sum
    };
};

/**
 * Validates minimum guest count for event catering
 */
export const validateMinimumGuests = (totalGuests, minGuests = 50) => {
    return {
        isValid: totalGuests >= minGuests,
        minRequired: minGuests,
        message: totalGuests < minGuests ? `Minimum ${minGuests} guests required for event catering` : null
    };
};

/**
 * Validates at least one guest category has count > 0
 */
export const validateAtLeastOneCategory = (guestCategories) => {
    const hasActiveCategory = Object.values(guestCategories).some(count => count > 0);
    return {
        isValid: hasActiveCategory,
        message: !hasActiveCategory ? 'At least one guest category must have guests' : null
    };
};

/**
 * Validates event date is in future
 */
export const validateEventDate = (eventDate) => {
    if (!eventDate) {
        return {
            isValid: false,
            message: 'Event date is required'
        };
    }

    const selectedDate = new Date(eventDate);
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    return {
        isValid: selectedDate >= today,
        message: selectedDate < today ? 'Event date must be today or in the future' : null
    };
};

/**
 * Validates package supports all active guest categories
 */
export const validatePackageCategorySupport = (packageData, activeCategories) => {
    const unsupportedCategories = [];

    activeCategories.forEach(({ category, count }) => {
        if (!packageData.categoryPricing || !packageData.categoryPricing[category]) {
            unsupportedCategories.push(category);
        }
    });

    return {
        isValid: unsupportedCategories.length === 0,
        unsupportedCategories,
        message: unsupportedCategories.length > 0
            ? `This package does not support: ${unsupportedCategories.join(', ')}`
            : null
    };
};

/**
 * Validates food item compatibility with guest category
 */
export const validateFoodItemCategory = (foodItem, guestCategory) => {
    // If food item has dietary restrictions, check compatibility
    if (foodItem.dietaryRestrictions && foodItem.dietaryRestrictions.length > 0) {
        const isCompatible = foodItem.dietaryRestrictions.includes(guestCategory);
        return {
            isValid: isCompatible,
            message: !isCompatible ? `This item is not compatible with ${guestCategory} dietary requirements` : null
        };
    }

    // If no restrictions specified, assume compatible with all
    return { isValid: true, message: null };
};

/**
 * Validates menu selection for a guest category
 */
export const validateMenuSelection = (selectedItems, guestCategory, requiredCategories = []) => {
    const errors = [];

    // Check if required categories are selected
    requiredCategories.forEach(reqCategory => {
        const hasSelection = selectedItems.some(item =>
            item.category === reqCategory && item.guestCategory === guestCategory
        );
        if (!hasSelection) {
            errors.push(`Please select at least one ${reqCategory} for ${guestCategory} guests`);
        }
    });

    // Check food item compatibility
    selectedItems.forEach(item => {
        const validation = validateFoodItemCategory(item, guestCategory);
        if (!validation.isValid) {
            errors.push(`${item.name}: ${validation.message}`);
        }
    });

    return {
        isValid: errors.length === 0,
        errors
    };
};

/**
 * Calculates price for a package across all active guest categories
 */
export const calculatePackagePrice = (packageData, guestCategories) => {
    let total = 0;
    const breakdown = [];

    Object.entries(guestCategories).forEach(([category, count]) => {
        if (count > 0 && packageData.categoryPricing && packageData.categoryPricing[category]) {
            const pricePerPlate = packageData.categoryPricing[category];
            const categoryTotal = pricePerPlate * count;
            total += categoryTotal;
            breakdown.push({
                category,
                count,
                pricePerPlate,
                total: categoryTotal
            });
        }
    });

    return {
        total,
        breakdown,
        isValid: breakdown.length > 0
    };
};

/**
 * Validates entire event setup before allowing package selection
 */
export const validateEventSetup = (eventData, config = {}) => {
    const minGuests = config.minGuests ?? 50;
    const errors = [];

    // Validate event date
    const dateValidation = validateEventDate(eventData.eventDate);
    if (!dateValidation.isValid) {
        errors.push(dateValidation.message);
    }

    // Validate minimum guests
    const minGuestsValidation = validateMinimumGuests(eventData.totalGuests, minGuests);
    if (!minGuestsValidation.isValid) {
        errors.push(minGuestsValidation.message);
    }

    // Validate guest totals
    const totalsValidation = validateGuestTotals(eventData.guestCategories, eventData.totalGuests);
    if (!totalsValidation.isValid) {
        errors.push(`Guest category totals (${totalsValidation.currentTotal}) must equal total guests (${eventData.totalGuests})`);
    }

    // Validate at least one category
    const categoryValidation = validateAtLeastOneCategory(eventData.guestCategories);
    if (!categoryValidation.isValid) {
        errors.push(categoryValidation.message);
    }

    return {
        isValid: errors.length === 0,
        errors,
        isSetupComplete: errors.length === 0
    };
};

/**
 * Validates cart item before adding
 */
export const validateCartItem = (item, eventData) => {
    const errors = [];

    // Check if event setup is complete
    if (!eventData.isSetupComplete) {
        errors.push('Please complete event setup before adding items to cart');
    }

    // Check if item has category pricing
    if (!item.guestCategory) {
        errors.push('Guest category must be specified');
    }

    // Check if category is active in event
    if (item.guestCategory && !eventData.guestCategories[item.guestCategory]) {
        errors.push(`${item.guestCategory} is not an active category in your event`);
    }

    // Check if quantity is valid
    if (item.quantity <= 0) {
        errors.push('Quantity must be greater than 0');
    }

    return {
        isValid: errors.length === 0,
        errors
    };
};

/**
 * Get active guest categories with count > 0
 */
export const getActiveCategories = (guestCategories) => {
    return Object.entries(guestCategories)
        .filter(([_, count]) => count > 0)
        .map(([category, count]) => ({ category, count }));
};

/**
 * Format category breakdown for display
 */
export const formatCategoryBreakdown = (guestCategories) => {
    return getActiveCategories(guestCategories)
        .map(({ category, count }) => `${category}: ${count}`)
        .join(' | ');
};

/**
 * Check if package is fully compatible with event
 */
export const isPackageCompatible = (packageData, eventData) => {
    const activeCategories = getActiveCategories(eventData.guestCategories);
    const validation = validatePackageCategorySupport(packageData, activeCategories);
    return validation.isValid;
};

export default {
    validateGuestTotals,
    validateMinimumGuests,
    validateAtLeastOneCategory,
    validateEventDate,
    validatePackageCategorySupport,
    validateFoodItemCategory,
    validateMenuSelection,
    calculatePackagePrice,
    validateEventSetup,
    validateCartItem,
    getActiveCategories,
    formatCategoryBreakdown,
    isPackageCompatible
};
