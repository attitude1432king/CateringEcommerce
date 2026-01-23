/*
========================================
File: src/components/user/SampleTasteModal.jsx
========================================
Modal for selecting sample taste items from a package.
Features: Category-wise grouping, quantity validation, checkbox selection
*/
import React, { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';

export default function SampleTasteModal({
    isOpen,
    onClose,
    foodItems,
    packageData,
    onConfirm
}) {
    const [selectedItems, setSelectedItems] = useState({});
    const [validationErrors, setValidationErrors] = useState({});
    const [sampleItemsByCategory, setSampleItemsByCategory] = useState({});

    // Group sample items by category and set allowed quantities
    useEffect(() => {
        if (!foodItems || foodItems.length === 0) {
            setSampleItemsByCategory({});
            return;
        }

        const grouped = foodItems
            .filter(item => item.isSampleTasted)
            .reduce((acc, item) => {
                const category = item.categoryName || 'Others';
                if (!acc[category]) {
                    acc[category] = {
                        categoryId: item.categoryId,
                        categoryName: category,
                        items: [],
                        allowedQuantity: 3 // Default: allow selecting up to 3 items per category for tasting
                    };
                }
                acc[category].items.push(item);
                return acc;
            }, {});

        // Update allowed quantities from package data if available
        if (packageData?.items && Array.isArray(packageData.items)) {
            packageData.items.forEach(pkgItem => {
                const categoryName = pkgItem.categoryName;
                if (grouped[categoryName]) {
                    // Use the quantity from package, or default to 3 if not specified
                    grouped[categoryName].allowedQuantity = pkgItem.quantity || 3;
                }
            });
        }

        setSampleItemsByCategory(grouped);
    }, [foodItems, packageData]);

    // Toggle item selection
    const toggleItem = (categoryName, foodItemId) => {
        setSelectedItems(prev => {
            const categorySelections = prev[categoryName] || [];
            const isSelected = categorySelections.includes(foodItemId);

            let newSelections;
            if (isSelected) {
                // Remove item
                newSelections = categorySelections.filter(id => id !== foodItemId);
            } else {
                // Check if we can add more items to this category
                const allowedQty = sampleItemsByCategory[categoryName]?.allowedQuantity || 0;

                if (categorySelections.length >= allowedQty) {
                    // Show validation error
                    setValidationErrors(prevErrors => ({
                        ...prevErrors,
                        [categoryName]: `You can only select ${allowedQty} items from ${categoryName}`
                    }));
                    return prev;
                }

                // Add item
                newSelections = [...categorySelections, foodItemId];

                // Clear validation error for this category
                setValidationErrors(prevErrors => {
                    const newErrors = { ...prevErrors };
                    delete newErrors[categoryName];
                    return newErrors;
                });
            }

            return {
                ...prev,
                [categoryName]: newSelections
            };
        });
    };

    // Check if item is selected
    const isItemSelected = (categoryName, foodItemId) => {
        return selectedItems[categoryName]?.includes(foodItemId) || false;
    };

    // Get selected count for category
    const getSelectedCount = (categoryName) => {
        return selectedItems[categoryName]?.length || 0;
    };

    // Handle confirm
    const handleConfirm = () => {
        // Prepare selection data
        const selections = Object.entries(selectedItems).map(([categoryName, foodIds]) => {
            const category = sampleItemsByCategory[categoryName];
            return {
                categoryId: category.categoryId,
                categoryName: categoryName,
                selectedItems: foodIds.map(foodId => {
                    const item = foodItems.find(f => f.foodItemId === foodId);
                    return {
                        foodItemId: item.foodItemId,
                        name: item.name,
                        price: item.price,
                        isVegetarian: item.isVegetarian
                    };
                })
            };
        }).filter(sel => sel.selectedItems.length > 0);

        onConfirm(selections);
        onClose();
    };

    // Calculate total selected items
    const getTotalSelected = () => {
        return Object.values(selectedItems).reduce((sum, items) => sum + items.length, 0);
    };

    if (!isOpen) return null;

    return (
        <AnimatePresence>
            <div className="fixed inset-0 z-[200] overflow-hidden">
                {/* Backdrop */}
                <motion.div
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1 }}
                    exit={{ opacity: 0 }}
                    onClick={onClose}
                    className="absolute inset-0 bg-black/60 backdrop-blur-sm"
                />

                {/* Modal */}
                <motion.div
                    initial={{ y: '100%' }}
                    animate={{ y: 0 }}
                    exit={{ y: '100%' }}
                    transition={{ type: 'spring', damping: 30, stiffness: 300 }}
                    className="absolute bottom-0 left-0 right-0 bg-white rounded-t-3xl shadow-2xl max-h-[90vh] overflow-hidden flex flex-col"
                >
                    {/* Header */}
                    <div className="sticky top-0 z-10 bg-gradient-to-r from-orange-500 to-rose-500 text-white px-6 py-5 border-b border-orange-200">
                        <div className="flex items-center justify-between">
                            <div className="flex-1">
                                <h2 className="text-2xl font-bold mb-1">Select Sample Taste Items</h2>
                                <p className="text-sm text-orange-100">
                                    {Object.keys(sampleItemsByCategory).length > 0 ? (
                                        <>Choose items you'd like to taste before your event (up to {Object.values(sampleItemsByCategory).reduce((sum, cat) => sum + (cat.allowedQuantity || 0), 0)} items total)</>
                                    ) : (
                                        <>Choose items you'd like to taste before your event</>
                                    )}
                                </p>
                            </div>
                            <button
                                onClick={onClose}
                                className="p-2 hover:bg-white/20 rounded-full transition-colors"
                            >
                                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                                </svg>
                            </button>
                        </div>

                        {/* Selection Summary */}
                        {getTotalSelected() > 0 && (
                            <div className="mt-3 bg-white/20 rounded-lg px-4 py-2 flex items-center justify-between">
                                <span className="text-sm font-medium">
                                    ✓ {getTotalSelected()} item{getTotalSelected() !== 1 ? 's' : ''} selected for tasting
                                </span>
                            </div>
                        )}
                    </div>

                    {/* Content - Scrollable */}
                    <div className="flex-1 overflow-y-auto px-6 py-6">
                        {Object.entries(sampleItemsByCategory).length === 0 ? (
                            <div className="text-center py-12 text-neutral-500">
                                <div className="text-5xl mb-4">😔</div>
                                <p className="text-lg font-semibold mb-2">No Sample Items Available</p>
                                <p className="text-sm">This package doesn't have any items available for sample tasting.</p>
                            </div>
                        ) : (
                            <>
                                {/* Info Box */}
                                <div className="bg-blue-50 border border-blue-200 rounded-xl p-4 mb-6">
                                    <div className="flex items-start gap-3">
                                        <div className="bg-blue-500 text-white rounded-full p-2 flex-shrink-0">
                                            <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                                                <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd"/>
                                            </svg>
                                        </div>
                                        <div className="flex-1">
                                            <h4 className="font-bold text-blue-900 mb-1 text-sm">How Sample Tasting Works</h4>
                                            <ul className="text-xs text-blue-800 space-y-1">
                                                <li>• Select items from each category you'd like to taste</li>
                                                <li>• Each category has a maximum selection limit</li>
                                                <li>• Sample portions will be prepared before your event</li>
                                            </ul>
                                        </div>
                                    </div>
                                </div>

                                <div className="space-y-6">
                                {Object.entries(sampleItemsByCategory).map(([categoryName, categoryData]) => {
                                    const selectedCount = getSelectedCount(categoryName);
                                    const allowedQty = categoryData.allowedQuantity;
                                    const hasError = validationErrors[categoryName];

                                    return (
                                        <div key={categoryName} className="border-2 border-neutral-200 rounded-xl overflow-hidden">
                                            {/* Category Header */}
                                            <div className={`px-4 py-3 border-b border-neutral-200 ${
                                                hasError ? 'bg-red-50' : 'bg-neutral-50'
                                            }`}>
                                                <div className="flex items-center justify-between">
                                                    <h3 className="text-lg font-bold text-neutral-800 flex items-center gap-2">
                                                        <span className="w-1 h-6 bg-catering-primary rounded"></span>
                                                        {categoryName}
                                                    </h3>
                                                    <div className={`text-sm font-semibold px-3 py-1 rounded-full transition-all ${
                                                        selectedCount > allowedQty
                                                            ? 'bg-red-100 text-red-700 ring-2 ring-red-300'
                                                            : selectedCount === allowedQty
                                                                ? 'bg-green-100 text-green-700'
                                                                : selectedCount > 0
                                                                    ? 'bg-blue-100 text-blue-700'
                                                                    : 'bg-neutral-200 text-neutral-700'
                                                    }`}>
                                                        {selectedCount} / {allowedQty} selected {selectedCount === allowedQty && '✓'}
                                                    </div>
                                                </div>
                                                <div className="mt-2 text-xs text-neutral-600">
                                                    {selectedCount === 0 && (
                                                        <span>Select up to {allowedQty} item{allowedQty !== 1 ? 's' : ''} from this category</span>
                                                    )}
                                                    {selectedCount > 0 && selectedCount < allowedQty && (
                                                        <span className="text-blue-600">You can select {allowedQty - selectedCount} more item{(allowedQty - selectedCount) !== 1 ? 's' : ''}</span>
                                                    )}
                                                    {selectedCount === allowedQty && (
                                                        <span className="text-green-600">Maximum items selected for this category</span>
                                                    )}
                                                </div>
                                                {hasError && (
                                                    <p className="text-xs text-red-700 mt-2 flex items-center gap-1 bg-red-100 px-2 py-1 rounded">
                                                        <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                                                            <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                                                        </svg>
                                                        {hasError}
                                                    </p>
                                                )}
                                            </div>

                                            {/* Items Grid */}
                                            <div className="p-4 grid grid-cols-1 sm:grid-cols-2 gap-3">
                                                {categoryData.items.map(item => {
                                                    const isSelected = isItemSelected(categoryName, item.foodItemId);

                                                    return (
                                                        <div
                                                            key={item.foodItemId}
                                                            onClick={() => toggleItem(categoryName, item.foodItemId)}
                                                            className={`flex gap-3 p-3 border-2 rounded-xl cursor-pointer transition-all ${
                                                                isSelected
                                                                    ? 'border-catering-primary bg-orange-50'
                                                                    : 'border-neutral-200 hover:border-neutral-300 bg-white'
                                                            }`}
                                                        >
                                                            {/* Checkbox */}
                                                            <div className="flex-shrink-0 pt-1">
                                                                <div className={`w-6 h-6 rounded-lg border-2 flex items-center justify-center transition-all ${
                                                                    isSelected
                                                                        ? 'bg-catering-primary border-catering-primary'
                                                                        : 'border-neutral-300'
                                                                }`}>
                                                                    {isSelected && (
                                                                        <svg className="w-4 h-4 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={3} d="M5 13l4 4L19 7" />
                                                                        </svg>
                                                                    )}
                                                                </div>
                                                            </div>

                                                            {/* Item Image */}
                                                            <div className="w-16 h-16 flex-shrink-0 rounded-lg overflow-hidden bg-neutral-100">
                                                                {item.imageUrls && item.imageUrls.length > 0 ? (
                                                                    <img
                                                                        src={item.imageUrls[0]}
                                                                        alt={item.name}
                                                                        className="w-full h-full object-cover"
                                                                    />
                                                                ) : (
                                                                    <div className="w-full h-full flex items-center justify-center text-2xl">
                                                                        {item.isVegetarian ? '🥗' : '🍖'}
                                                                    </div>
                                                                )}
                                                            </div>

                                                            {/* Item Details */}
                                                            <div className="flex-1 min-w-0">
                                                                <div className="flex items-start gap-2 mb-1">
                                                                    <h4 className="font-semibold text-sm text-neutral-800 flex-1 line-clamp-1">
                                                                        {item.name}
                                                                    </h4>
                                                                    {item.isVegetarian && (
                                                                        <span className="text-[10px] border border-green-600 text-green-600 px-1 rounded flex-shrink-0">VEG</span>
                                                                    )}
                                                                </div>
                                                                <p className="text-xs text-neutral-500 line-clamp-2 mb-1">
                                                                    {item.description}
                                                                </p>
                                                                <p className="text-xs font-bold text-neutral-700">₹{item.price}</p>
                                                            </div>
                                                        </div>
                                                    );
                                                })}
                                            </div>
                                        </div>
                                    );
                                })}
                                </div>
                            </>
                        )}
                    </div>

                    {/* Footer - Action Buttons */}
                    <div className="sticky bottom-0 bg-white border-t border-neutral-200 px-6 py-4">
                        <div className="flex gap-3">
                            <button
                                onClick={onClose}
                                className="flex-1 px-6 py-3 border-2 border-neutral-300 text-neutral-700 rounded-lg font-semibold hover:bg-neutral-50 transition-colors"
                            >
                                Cancel
                            </button>
                            <button
                                onClick={handleConfirm}
                                disabled={getTotalSelected() === 0}
                                className={`flex-1 px-6 py-3 rounded-lg font-semibold transition-all ${
                                    getTotalSelected() > 0
                                        ? 'bg-gradient-catering text-white hover:shadow-lg'
                                        : 'bg-neutral-200 text-neutral-400 cursor-not-allowed'
                                }`}
                            >
                                Confirm Selection ({getTotalSelected()})
                            </button>
                        </div>
                    </div>
                </motion.div>
            </div>
        </AnimatePresence>
    );
}
