import React, { useEffect, useMemo, useState } from 'react';
import VegNonVegIcon from '../common/VegNonVegIcon';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL.replace(/\/$/, '');

export default function SampleTasteSelectionPanel({
    foodItems = [],
    packageData = null,
    initialSelections = [],
    onConfirm,
    confirmLabel = 'Save Sample Selections'
}) {
    const [selectedItems, setSelectedItems] = useState({});
    const [validationErrors, setValidationErrors] = useState({});

    const sampleItemsByCategory = useMemo(() => {
        const eligibleItems = (foodItems || []).filter(item => item.isSampleTasted && !item.isIncludedInPackage);

        const grouped = eligibleItems.reduce((acc, item) => {
            const category = item.categoryName || 'Others';
            if (!acc[category]) {
                acc[category] = {
                    categoryId: item.categoryId,
                    categoryName: category,
                    items: [],
                    allowedQuantity: 3
                };
            }
            acc[category].items.push(item);
            return acc;
        }, {});

        if (packageData?.categories && Array.isArray(packageData.categories)) {
            packageData.categories.forEach(category => {
                const categoryName = category.categoryName;
                if (grouped[categoryName]) {
                    grouped[categoryName].allowedQuantity = category.allowedQuantity || grouped[categoryName].allowedQuantity;
                }
            });
        }

        return grouped;
    }, [foodItems, packageData]);

    useEffect(() => {
        const preselected = (initialSelections || []).reduce((acc, category) => {
            if (category?.categoryName && Array.isArray(category.selectedItems)) {
                acc[category.categoryName] = category.selectedItems
                    .map(item => item.foodItemId)
                    .filter(Boolean);
            }
            return acc;
        }, {});
        setSelectedItems(preselected);
    }, [initialSelections]);

    const toggleItem = (categoryName, foodItemId) => {
        setSelectedItems(prev => {
            const categorySelections = prev[categoryName] || [];
            const isSelected = categorySelections.includes(foodItemId);

            if (isSelected) {
                return {
                    ...prev,
                    [categoryName]: categorySelections.filter(id => id !== foodItemId)
                };
            }

            const allowedQty = sampleItemsByCategory[categoryName]?.allowedQuantity || 0;
            if (categorySelections.length >= allowedQty) {
                setValidationErrors(prevErrors => ({
                    ...prevErrors,
                    [categoryName]: `You can only select ${allowedQty} items from ${categoryName}`
                }));
                return prev;
            }

            setValidationErrors(prevErrors => {
                const next = { ...prevErrors };
                delete next[categoryName];
                return next;
            });

            return {
                ...prev,
                [categoryName]: [...categorySelections, foodItemId]
            };
        });
    };

    const getSelections = () =>
        Object.entries(selectedItems).map(([categoryName, foodIds]) => {
            const category = sampleItemsByCategory[categoryName];
            return {
                categoryId: category.categoryId,
                categoryName,
                selectedItems: foodIds
                    .map(foodId => category.items.find(item => item.foodItemId === foodId))
                    .filter(Boolean)
                    .map(item => ({
                        foodItemId: item.foodItemId,
                        name: item.name,
                        price: item.price,
                        isVegetarian: item.isVegetarian
                    }))
            };
        }).filter(category => category.selectedItems.length > 0);

    const getTotalSelected = () =>
        Object.values(selectedItems).reduce((sum, items) => sum + items.length, 0);

    return (
        <div className="bg-white rounded-2xl border border-neutral-200 overflow-hidden">
            <div className="bg-gradient-to-r from-orange-500 to-rose-500 text-white px-6 py-5 border-b border-orange-200">
                <div className="flex items-center justify-between gap-4">
                    <div>
                        <h3 className="text-xl font-bold">Sample Taste</h3>
                        <p className="text-sm text-orange-100">Choose standalone sample items from the selected package categories.</p>
                    </div>
                    <div className="text-sm font-semibold bg-white/20 rounded-lg px-3 py-2">
                        {getTotalSelected()} selected
                    </div>
                </div>
            </div>

            <div className="p-6 space-y-6">
                {Object.entries(sampleItemsByCategory).length === 0 ? (
                    <div className="rounded-xl border border-dashed border-neutral-300 p-8 text-center text-neutral-500">
                        No standalone sample taste items are available for the current package selection.
                    </div>
                ) : (
                    Object.entries(sampleItemsByCategory).map(([categoryName, categoryData]) => (
                        <div key={categoryName} className="border border-neutral-200 rounded-xl overflow-hidden">
                            <div className="px-4 py-3 bg-neutral-50 border-b border-neutral-200 flex items-center justify-between">
                                <div>
                                    <h4 className="font-semibold text-neutral-900">{categoryName}</h4>
                                    <p className="text-xs text-neutral-500">
                                        Select up to {categoryData.allowedQuantity} item{categoryData.allowedQuantity !== 1 ? 's' : ''}
                                    </p>
                                </div>
                                <div className="text-xs font-semibold px-3 py-1 rounded-full bg-white border border-neutral-200 text-neutral-700">
                                    {(selectedItems[categoryName] || []).length} / {categoryData.allowedQuantity}
                                </div>
                            </div>

                            {validationErrors[categoryName] && (
                                <div className="px-4 py-2 text-xs text-red-700 bg-red-50 border-b border-red-100">
                                    {validationErrors[categoryName]}
                                </div>
                            )}

                            <div className="p-4 grid grid-cols-1 md:grid-cols-2 gap-3">
                                {categoryData.items.map(item => {
                                    const isSelected = (selectedItems[categoryName] || []).includes(item.foodItemId);

                                    return (
                                        <button
                                            type="button"
                                            key={item.foodItemId}
                                            onClick={() => toggleItem(categoryName, item.foodItemId)}
                                            className={`text-left flex gap-3 p-3 border-2 rounded-xl transition-all ${
                                                isSelected
                                                    ? 'border-catering-primary bg-orange-50'
                                                    : 'border-neutral-200 hover:border-neutral-300 bg-white'
                                            }`}
                                        >
                                            <div className="w-16 h-16 rounded-lg overflow-hidden bg-neutral-100 flex-shrink-0">
                                                {item.imageUrls && item.imageUrls.length > 0 ? (
                                                    <img
                                                        src={item.imageUrls[0].startsWith('http') ? item.imageUrls[0] : `${API_BASE_URL}${item.imageUrls[0]}`}
                                                        alt={item.name}
                                                        className="w-full h-full object-cover"
                                                    />
                                                ) : (
                                                    <VegNonVegIcon isVeg={item.isVegetarian} placeholder />
                                                )}
                                            </div>
                                            <div className="flex-1 min-w-0">
                                                <div className="flex items-start justify-between gap-2">
                                                    <h5 className="font-semibold text-sm text-neutral-900 line-clamp-1">{item.name}</h5>
                                                    {(item.isVegetarian ?? false) && (
                                                        <span className="text-[10px] border border-green-600 text-green-600 px-1 rounded">VEG</span>
                                                    )}
                                                </div>
                                                <p className="text-xs text-neutral-500 line-clamp-2 mt-1">{item.description}</p>
                                                <p className="text-xs font-bold text-neutral-700 mt-2">Rs. {item.price}</p>
                                            </div>
                                        </button>
                                    );
                                })}
                            </div>
                        </div>
                    ))
                )}
            </div>

            <div className="px-6 py-4 border-t border-neutral-200 bg-white">
                <button
                    type="button"
                    onClick={() => onConfirm(getSelections())}
                    className="w-full px-6 py-3 rounded-xl font-semibold bg-gradient-catering text-white hover:shadow-lg transition-all"
                >
                    {confirmLabel} ({getTotalSelected()})
                </button>
            </div>
        </div>
    );
}
