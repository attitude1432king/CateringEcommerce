/**
 * Package Selection Validation Utilities
 *
 * Provides validation functions for package selection feature.
 * Ensures users select the correct quantity of items per category.
 */

/**
 * Validates user's package selection against package rules
 * @param {Object} packageData - Package structure from API
 * @param {Object} selections - User's selections { categoryId: [foodId1, foodId2] }
 * @returns {Object} { isValid: boolean, errors: {} }
 */
export function validatePackageSelection(packageData, selections) {
    const errors = {};

    if (!packageData || !packageData.categories) {
        return { isValid: false, errors: { general: 'Invalid package data' } };
    }

    // Validate each category
    packageData.categories.forEach(category => {
        const selectedItems = selections[category.categoryId] || [];
        const allowedQuantity = category.allowedQuantity;

        // Check if correct quantity is selected
        if (selectedItems.length !== allowedQuantity) {
            errors[category.categoryId] = `Please select exactly ${allowedQuantity} item(s) from ${category.categoryName}`;
        }

        // Check if selected items actually exist in the category
        selectedItems.forEach(foodId => {
            const itemExists = category.foodItems.some(item => item.foodId === foodId);
            if (!itemExists) {
                errors[category.categoryId] = `Invalid item selected in ${category.categoryName}`;
            }
        });
    });

    return {
        isValid: Object.keys(errors).length === 0,
        errors
    };
}

/**
 * Checks if a category selection is complete
 * @param {Object} category - Category object from package data
 * @param {Array} selectedIds - Array of selected food IDs
 * @returns {boolean}
 */
export function isCategoryComplete(category, selectedIds) {
    if (!category || !Array.isArray(selectedIds)) return false;
    return selectedIds.length === category.allowedQuantity;
}

/**
 * Checks if all categories have complete selections
 * @param {Object} packageData - Package structure from API
 * @param {Object} selections - User's selections
 * @returns {boolean}
 */
export function areAllCategoriesComplete(packageData, selections) {
    if (!packageData || !packageData.categories) return false;

    return packageData.categories.every(category => {
        const selectedIds = selections[category.categoryId] || [];
        return isCategoryComplete(category, selectedIds);
    });
}

/**
 * Gets selection progress summary
 * @param {Object} packageData - Package structure from API
 * @param {Object} selections - User's selections
 * @returns {Object} Progress summary
 */
export function getSelectionProgress(packageData, selections) {
    if (!packageData || !packageData.categories) {
        return { total: 0, completed: 0, percentage: 0 };
    }

    const total = packageData.categories.length;
    const completed = packageData.categories.filter(category => {
        const selectedIds = selections[category.categoryId] || [];
        return isCategoryComplete(category, selectedIds);
    }).length;

    return {
        total,
        completed,
        percentage: total > 0 ? Math.round((completed / total) * 100) : 0
    };
}

/**
 * Formats selection data for API submission
 * @param {Object} packageData - Package structure from API
 * @param {Object} selections - User's selections
 * @returns {Object} Formatted data for API
 */
export function formatSelectionForSubmission(packageData, selections) {
    return {
        packageId: packageData.packageId,
        selections: packageData.categories.map(category => ({
            categoryId: category.categoryId,
            selectedFoodIds: selections[category.categoryId] || []
        }))
    };
}
