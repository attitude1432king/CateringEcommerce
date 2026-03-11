import React, { useState, useEffect, useRef } from 'react';
import { fetchApi } from '../../services/apiUtils';
import SampleTasteModal from './SampleTasteModal';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:44368';

/**
 * Media Viewer Modal - Full Screen Image/Video Viewer
 */
const MediaViewerModal = ({ isOpen, onClose, mediaUrl, mediaType, foodName }) => {
    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 bg-black bg-opacity-95 z-[100] flex items-center justify-center p-4" onClick={onClose}>
            <button
                onClick={onClose}
                className="absolute top-4 right-4 text-white hover:bg-white/20 rounded-full p-3 transition-colors z-10"
            >
                <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
            </button>

            {/* Food Name Title */}
            <div className="absolute top-4 left-4 bg-black/60 backdrop-blur-sm text-white px-6 py-3 rounded-xl">
                <h3 className="text-xl font-bold">{foodName}</h3>
            </div>

            {/* Media Content */}
            <div className="max-w-7xl max-h-[90vh] w-full" onClick={(e) => e.stopPropagation()}>
                {mediaType === 'video' ? (
                    <video
                        src={`${API_BASE_URL}${mediaUrl}`}
                        controls
                        autoPlay
                        className="w-full h-full max-h-[90vh] object-contain rounded-xl"
                    >
                        Your browser does not support the video tag.
                    </video>
                ) : (
                    <img
                        src={`${API_BASE_URL}${mediaUrl}`}
                        alt={foodName}
                        className="w-full h-full max-h-[90vh] object-contain rounded-xl"
                    />
                )}
            </div>

            {/* Navigation hint */}
            <div className="absolute bottom-4 left-1/2 transform -translate-x-1/2 bg-black/60 backdrop-blur-sm text-white px-6 py-2 rounded-full text-sm">
                Click anywhere to close
            </div>
        </div>
    );
};

/**
 * Utility function to detect media type based on file extension
 */
const getMediaType = (url) => {
    if (!url) return 'image';

    const extension = url.split('.').pop().toLowerCase();

    const imageExtensions = ['jpg', 'jpeg', 'png', 'gif', 'webp', 'svg', 'bmp', 'ico', 'tiff'];
    const videoExtensions = ['mp4', 'webm', 'mov', 'avi', 'mkv', 'flv', 'wmv', 'ogg', 'm4v', '3gp'];

    if (videoExtensions.includes(extension)) {
        return 'video';
    }

    return 'image';
};

/**
 * Package Selection Modal Component - Swiggy/Zomato Style
 *
 * Modern food selection interface with beautiful images & videos
 * Features:
 * - Large food images and videos
 * - Full-screen media viewer
 * - Category-based selection with quantity limits
 * - Real-time validation
 * - Visual feedback for selection progress
 */
export default function PackageSelectionModal({
    isOpen,
    onClose,
    cateringId,
    packageId,
    packageName,
    onSelectionComplete
}) {
    const [packageData, setPackageData] = useState(null);
    const [selections, setSelections] = useState({}); // { categoryId: [foodId1, foodId2, ...] }
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState(null);
    const [validationErrors, setValidationErrors] = useState({});
    const [searchQuery, setSearchQuery] = useState('');
    const [isSearching, setIsSearching] = useState(false);

    // Media viewer state
    const [mediaViewer, setMediaViewer] = useState({
        isOpen: false,
        mediaUrl: null,
        mediaType: 'image',
        foodName: ''
    });

    // Sample Taste Modal state
    const [showSampleTasteModal, setShowSampleTasteModal] = useState(false);
    const [sampleTasteItems, setSampleTasteItems] = useState([]);
    const [pendingPackageSelection, setPendingPackageSelection] = useState(null);

    // Refs for scrolling to invalid categories
    const categoryRefs = useRef({});

    // Fetch package selection data when modal opens
    useEffect(() => {
        if (isOpen && packageId && cateringId) {
            loadPackageData();
        }
    }, [isOpen, packageId, cateringId]);

    // Search with debounce
    useEffect(() => {
        if (!isOpen || !packageId || !cateringId) return;

        const debounceTimer = setTimeout(() => {
            if (searchQuery.trim()) {
                handleSearch(searchQuery);
            } else {
                loadPackageData();
            }
        }, 500);

        return () => clearTimeout(debounceTimer);
    }, [searchQuery]);

    const loadPackageData = async () => {
        try {
            setIsLoading(true);
            setError(null);

            const response = await fetchApi(
                `/User/Home/Catering/${cateringId}/Package/${packageId}/Selection`,
                'GET'
            );

            if (response.success && response.data) {
                setPackageData(response.data);
                // Initialize empty selections for each category
                const initialSelections = {};
                response.data.categories.forEach(cat => {
                    initialSelections[cat.categoryId] = [];
                });
                setSelections(initialSelections);
            } else {
                setError(response.message || 'Failed to load package details');
            }
        } catch (err) {
            console.error('Error loading package data:', err);
            setError('An error occurred while loading package details');
        } finally {
            setIsLoading(false);
        }
    };

    const handleSearch = async (query) => {
        try {
            setIsSearching(true);
            setError(null);

            const response = await fetchApi(
                `/User/Home/Catering/${cateringId}/Package/${packageId}/Selection/Search?searchQuery=${encodeURIComponent(query)}`,
                'GET'
            );

            if (response.success && response.data) {
                setPackageData(response.data);
            } else {
                setError(response.message || 'Search failed');
            }
        } catch (err) {
            console.error('Error searching food items:', err);
            setError('An error occurred while searching');
        } finally {
            setIsSearching(false);
        }
    };

    // Open media viewer
    const openMediaViewer = (mediaUrl, foodName, e) => {
        e.stopPropagation(); // Prevent food item selection when clicking media
        const mediaType = getMediaType(mediaUrl);
        setMediaViewer({
            isOpen: true,
            mediaUrl,
            mediaType,
            foodName
        });
    };

    // Close media viewer
    const closeMediaViewer = () => {
        setMediaViewer({
            isOpen: false,
            mediaUrl: null,
            mediaType: 'image',
            foodName: ''
        });
    };

    // Toggle food item selection
    const toggleFoodItem = (categoryId, foodId, allowedQuantity) => {
        setSelections(prev => {
            const currentSelections = prev[categoryId] || [];
            const isSelected = currentSelections.includes(foodId);

            if (isSelected) {
                // Deselect
                return {
                    ...prev,
                    [categoryId]: currentSelections.filter(id => id !== foodId)
                };
            } else {
                // Check if we can select more
                if (currentSelections.length >= allowedQuantity) {
                    // Show validation error
                    setValidationErrors(prev => ({
                        ...prev,
                        [categoryId]: `You can only select ${allowedQuantity} item(s) from this category`
                    }));

                    // Clear error after 3 seconds
                    setTimeout(() => {
                        setValidationErrors(prev => {
                            const newErrors = { ...prev };
                            delete newErrors[categoryId];
                            return newErrors;
                        });
                    }, 3000);

                    return prev;
                }

                // Select
                return {
                    ...prev,
                    [categoryId]: [...currentSelections, foodId]
                };
            }
        });
    };

    // Check if all required selections are made
    const isSelectionComplete = () => {
        if (!packageData) return false;

        return packageData.categories.every(category => {
            const selected = selections[category.categoryId] || [];
            return selected.length >= 1;
        });
    };

    // Scroll to first invalid category
    const scrollToFirstInvalidCategory = (invalidCategories) => {
        if (invalidCategories.length > 0) {
            const firstInvalidId = invalidCategories[0];
            const element = categoryRefs.current[firstInvalidId];
            if (element) {
                element.scrollIntoView({
                    behavior: 'smooth',
                    block: 'center'
                });
            }
        }
    };

    // Handle selection confirmation
    const handleConfirmSelection = () => {
        const errors = {};
        const invalidCategories = [];

        packageData.categories.forEach(category => {
            const selected = selections[category.categoryId] || [];
            if (selected.length === 0) {
                errors[category.categoryId] = `Please select at least one item from ${category.categoryName}`;
                invalidCategories.push(category.categoryId);
            }
        });

        if (Object.keys(errors).length > 0) {
            setValidationErrors(errors);
            scrollToFirstInvalidCategory(invalidCategories);
            return;
        }

        // Prepare selection data
        const selectionData = {
            packageId: packageData.packageId,
            packageName: packageData.packageName,
            price: packageData.price,
            selections: packageData.categories.map(cat => ({
                categoryId: cat.categoryId,
                categoryName: cat.categoryName,
                selectedFoodIds: selections[cat.categoryId] || [],
                selectedItems: cat.foodItems.filter(item =>
                    (selections[cat.categoryId] || []).includes(item.foodId)
                )
            }))
        };

        // Check if any selected items have sample tasting available
        const allSelectedItems = [];
        packageData.categories.forEach(category => {
            const selectedItems = category.foodItems.filter(item =>
                (selections[category.categoryId] || []).includes(item.foodId)
            );
            allSelectedItems.push(...selectedItems);
        });

        const itemsWithSampleTaste = allSelectedItems.filter(item => item.isSampleTasted === true);

        if (itemsWithSampleTaste.length > 0) {
            // Show Sample Taste Modal
            setSampleTasteItems(itemsWithSampleTaste);
            setPendingPackageSelection(selectionData);
            setShowSampleTasteModal(true);
        } else {
            // No sample taste items, complete selection directly
            onSelectionComplete(selectionData);
            onClose();
        }
    };

    // Handle Sample Taste Modal completion
    const handleSampleTasteComplete = (sampleTasteSelection) => {
        setShowSampleTasteModal(false);

        // Combine package selection with sample taste selection
        const finalSelectionData = {
            ...pendingPackageSelection,
            sampleTasteItems: sampleTasteSelection || []
        };

        onSelectionComplete(finalSelectionData);
        onClose();
    };

    // Handle Sample Taste Modal close (skip sample tasting)
    const handleSampleTasteClose = () => {
        setShowSampleTasteModal(false);

        // Complete package selection without sample tasting
        if (pendingPackageSelection) {
            onSelectionComplete(pendingPackageSelection);
            onClose();
        }
    };

    if (!isOpen) return null;

    return (
        <>
            {/* Main Modal */}
            <div className="fixed inset-0 bg-black bg-opacity-60 z-50 flex items-center justify-center p-4 overflow-y-auto">
                <div className="bg-white rounded-2xl max-w-6xl w-full max-h-[95vh] overflow-hidden flex flex-col my-8">
                    {/* Header */}
                    <div className="bg-gradient-to-r from-orange-500 to-red-500 text-white p-6">
                        <div className="flex justify-between items-start mb-4">
                            <div className="flex-1">
                                <div className="flex items-center gap-2 mb-2">
                                    <svg className="w-6 h-6" fill="currentColor" viewBox="0 0 20 20">
                                        <path d="M9 2a1 1 0 000 2h2a1 1 0 100-2H9z"/>
                                        <path fillRule="evenodd" d="M4 5a2 2 0 012-2 3 3 0 003 3h2a3 3 0 003-3 2 2 0 012 2v11a2 2 0 01-2 2H6a2 2 0 01-2-2V5zm3 4a1 1 0 000 2h.01a1 1 0 100-2H7zm3 0a1 1 0 000 2h3a1 1 0 100-2h-3zm-3 4a1 1 0 100 2h.01a1 1 0 100-2H7zm3 0a1 1 0 100 2h3a1 1 0 100-2h-3z" clipRule="evenodd"/>
                                    </svg>
                                    <h2 className="text-2xl font-bold">Customize Your Package</h2>
                                </div>
                                <p className="text-white/90 font-medium text-lg">{packageData?.packageName || packageName}</p>
                                <p className="text-white/80 text-sm mt-1">₹{packageData?.price} per person</p>
                            </div>
                            <button
                                onClick={onClose}
                                className="text-white hover:bg-white/20 rounded-full p-2 transition-colors flex-shrink-0"
                            >
                                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                                </svg>
                            </button>
                        </div>

                        {/* Search Bar */}
                        <div className="relative">
                            <div className="relative">
                                <input
                                    type="text"
                                    placeholder="Search food items by name..."
                                    value={searchQuery}
                                    onChange={(e) => setSearchQuery(e.target.value)}
                                    className="w-full px-4 py-3 pl-12 pr-10 rounded-xl text-gray-900 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-white/50 transition-all"
                                />
                                <svg
                                    className="absolute left-4 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400"
                                    fill="none"
                                    stroke="currentColor"
                                    viewBox="0 0 24 24"
                                >
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                                </svg>
                                {searchQuery && (
                                    <button
                                        onClick={() => setSearchQuery('')}
                                        className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400 hover:text-gray-600 transition-colors"
                                    >
                                        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                                        </svg>
                                    </button>
                                )}
                                {isSearching && (
                                    <div className="absolute right-3 top-1/2 transform -translate-y-1/2">
                                        <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-orange-500"></div>
                                    </div>
                                )}
                            </div>
                            {searchQuery && (
                                <p className="text-white/80 text-xs mt-2">
                                    {isSearching ? 'Searching...' : `Showing results for "${searchQuery}"`}
                                </p>
                            )}
                        </div>
                    </div>

                    {/* Content */}
                    <div className="flex-1 overflow-y-auto">
                        {isLoading && (
                            <div className="flex items-center justify-center py-20">
                                <div className="text-center">
                                    <div className="animate-spin rounded-full h-16 w-16 border-b-4 border-orange-500 mx-auto mb-4"></div>
                                    <p className="text-neutral-600 font-medium">Loading delicious options...</p>
                                </div>
                            </div>
                        )}

                        {error && (
                            <div className="m-6 bg-red-50 border-l-4 border-red-500 p-4 rounded">
                                <div className="flex items-start">
                                    <svg className="w-6 h-6 text-red-500 mr-3 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
                                        <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd"/>
                                    </svg>
                                    <div>
                                        <p className="font-semibold text-red-800">Error loading package</p>
                                        <p className="text-sm text-red-700 mt-1">{error}</p>
                                    </div>
                                </div>
                            </div>
                        )}

                        {!isLoading && !error && packageData && (
                            <div className="p-6 space-y-8">
                                {/* Instructions */}
                                <div className="bg-gradient-to-r from-blue-50 to-indigo-50 border border-blue-200 rounded-xl p-5">
                                    <div className="flex items-start gap-3">
                                        <div className="bg-blue-500 text-white rounded-full p-2 flex-shrink-0">
                                            <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                                                <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd"/>
                                            </svg>
                                        </div>
                                        <div className="flex-1">
                                            <h3 className="font-bold text-blue-900 mb-2 text-lg">How to customize</h3>
                                            <ul className="text-sm text-blue-800 space-y-1.5">
                                                <li className="flex items-start">
                                                    <span className="mr-2">✓</span>
                                                    <span>Select at least one item from each food category</span>
                                                </li>
                                                <li className="flex items-start">
                                                    <span className="mr-2">✓</span>
                                                    <span>Click on food card to select/deselect items</span>
                                                </li>
                                                <li className="flex items-start">
                                                    <span className="mr-2">✓</span>
                                                    <span>Click on image/video to view in full screen 🔍</span>
                                                </li>
                                                <li className="flex items-start">
                                                    <span className="mr-2">✓</span>
                                                    <span>All categories must have at least one selection before confirming</span>
                                                </li>
                                            </ul>
                                        </div>
                                    </div>
                                </div>

                                {/* No Results Message */}
                                {searchQuery && packageData.categories.every(cat => cat.foodItems.length === 0) && (
                                    <div className="bg-gradient-to-br from-gray-50 to-neutral-100 rounded-xl border-2 border-dashed border-gray-300 p-12 text-center">
                                        <div className="max-w-md mx-auto">
                                            <svg className="w-20 h-20 mx-auto mb-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9.172 16.172a4 4 0 015.656 0M9 10h.01M15 10h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                                            </svg>
                                            <h3 className="text-xl font-bold text-gray-700 mb-2">No food items found</h3>
                                            <p className="text-gray-600 mb-4">
                                                We couldn't find any food items matching "<span className="font-semibold">{searchQuery}</span>"
                                            </p>
                                            <button
                                                onClick={() => setSearchQuery('')}
                                                className="inline-flex items-center gap-2 px-6 py-3 bg-gradient-to-r from-orange-500 to-red-500 text-white rounded-xl font-semibold hover:shadow-lg transition-all"
                                            >
                                                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                                                </svg>
                                                Clear Search
                                            </button>
                                        </div>
                                    </div>
                                )}

                                {/* Categories */}
                                {packageData.categories.map((category) => {
                                    const selectedCount = (selections[category.categoryId] || []).length;
                                    const isComplete = selectedCount >= 1;
                                    const hasError = validationErrors[category.categoryId];

                                    return (
                                        <div
                                            key={category.categoryId}
                                            ref={el => categoryRefs.current[category.categoryId] = el}
                                            className={`bg-white rounded-xl border-2 overflow-hidden shadow-sm hover:shadow-md transition-all ${
                                                hasError
                                                    ? 'border-red-400 ring-4 ring-red-100'
                                                    : 'border-neutral-200'
                                            }`}
                                        >
                                            {/* Category Header */}
                                            <div className={`sticky top-0 z-10 p-5 border-b-2 ${
                                                hasError
                                                    ? 'bg-gradient-to-r from-red-50 to-rose-50 border-red-300'
                                                    : isComplete
                                                    ? 'bg-gradient-to-r from-green-50 to-emerald-50 border-green-300'
                                                    : 'bg-gradient-to-r from-neutral-50 to-gray-50 border-neutral-300'
                                            }`}>
                                                <div className="flex justify-between items-start">
                                                    <div className="flex-1">
                                                        <div className="flex items-center gap-3 mb-1">
                                                            <h3 className="text-xl font-bold text-neutral-900">
                                                                {category.categoryName}
                                                            </h3>
                                                            {isComplete && (
                                                                <div className="bg-green-500 text-white rounded-full p-1">
                                                                    <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                                                                        <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd"/>
                                                                    </svg>
                                                                </div>
                                                            )}
                                                        </div>
                                                        {category.categoryDescription && (
                                                            <p className="text-sm text-neutral-600">{category.categoryDescription}</p>
                                                        )}
                                                    </div>
                                                    <div className="text-right ml-4">
                                                        <div className={`inline-flex items-center px-4 py-2 rounded-full font-bold text-sm ${
                                                            hasError
                                                                ? 'bg-red-500 text-white animate-pulse'
                                                                : isComplete
                                                                ? 'bg-green-500 text-white'
                                                                : 'bg-orange-100 text-orange-700'
                                                        }`}>
                                                            {selectedCount} Selected {selectedCount >= 1 ? '✓' : ''}
                                                        </div>
                                                        {hasError && (
                                                            <div className="mt-2 bg-red-100 border border-red-300 rounded-lg px-3 py-2">
                                                                <p className="text-xs text-red-800 font-semibold flex items-center gap-1">
                                                                    <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                                                                        <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clipRule="evenodd"/>
                                                                    </svg>
                                                                    {hasError}
                                                                </p>
                                                            </div>
                                                        )}
                                                    </div>
                                                </div>
                                            </div>

                                            {/* Food Items Grid */}
                                            <div className="p-5">
                                                {category.foodItems.length === 0 ? (
                                                    <div className="text-center py-12 text-neutral-500">
                                                        <svg className="w-16 h-16 mx-auto mb-3 text-neutral-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/>
                                                        </svg>
                                                        <p className="font-medium">No items available in this category</p>
                                                    </div>
                                                ) : (
                                                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                                                        {category.foodItems.map((food) => {
                                                            const isSelected = (selections[category.categoryId] || []).includes(food.foodId);
                                                            const hasMedia = food.imageUrls && food.imageUrls.length > 0;
                                                            const mediaUrl = hasMedia ? food.imageUrls[0] : null;
                                                            const mediaType = hasMedia ? getMediaType(mediaUrl) : 'image';

                                                            return (
                                                                <button
                                                                    key={food.foodId}
                                                                    onClick={() => toggleFoodItem(category.categoryId, food.foodId, category.allowedQuantity)}
                                                                    className={`text-left rounded-xl overflow-hidden border-3 transition-all transform hover:scale-[1.02] ${
                                                                        isSelected
                                                                            ? 'border-orange-500 shadow-lg ring-4 ring-orange-100'
                                                                            : 'border-neutral-200 hover:border-neutral-300 shadow-sm hover:shadow-md'
                                                                    } bg-white relative group`}
                                                                >
                                                                    {/* Media Section */}
                                                                    <div className="relative h-48 bg-gradient-to-br from-neutral-100 to-neutral-200 overflow-hidden">
                                                                        {hasMedia ? (
                                                                            <>
                                                                                {mediaType === 'video' ? (
                                                                                    <video
                                                                                        src={`${API_BASE_URL}${mediaUrl}`}
                                                                                        className="w-full h-full object-cover"
                                                                                        muted
                                                                                        loop
                                                                                        playsInline
                                                                                    />
                                                                                ) : (
                                                                                    <img
                                                                                        src={`${API_BASE_URL}${mediaUrl}`}
                                                                                        alt={food.foodName}
                                                                                        className="w-full h-full object-cover group-hover:scale-110 transition-transform duration-300"
                                                                                    />
                                                                                )}

                                                                                {/* View Full Screen Button */}
                                                                                <button
                                                                                    onClick={(e) => openMediaViewer(mediaUrl, food.foodName, e)}
                                                                                    className="absolute inset-0 bg-black/0 hover:bg-black/30 flex items-center justify-center opacity-0 hover:opacity-100 transition-all"
                                                                                >
                                                                                    <div className="bg-white/90 backdrop-blur-sm px-4 py-2 rounded-full flex items-center gap-2 transform scale-90 hover:scale-100 transition-transform">
                                                                                        <svg className="w-5 h-5 text-orange-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                                                                                        </svg>
                                                                                        <span className="text-sm font-semibold text-neutral-900">View {mediaType === 'video' ? 'Video' : 'Image'}</span>
                                                                                    </div>
                                                                                </button>

                                                                                {/* Video Indicator */}
                                                                                {mediaType === 'video' && (
                                                                                    <div className="absolute top-3 left-3 bg-red-500 text-white px-3 py-1 rounded-full text-xs font-bold flex items-center gap-1 shadow-lg">
                                                                                        <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                                                                                            <path d="M2 6a2 2 0 012-2h6a2 2 0 012 2v8a2 2 0 01-2 2H4a2 2 0 01-2-2V6zM14.553 7.106A1 1 0 0014 8v4a1 1 0 00.553.894l2 1A1 1 0 0018 13V7a1 1 0 00-1.447-.894l-2 1z" />
                                                                                        </svg>
                                                                                        VIDEO
                                                                                    </div>
                                                                                )}
                                                                            </>
                                                                        ) : (
                                                                            <div className="w-full h-full flex items-center justify-center text-6xl">
                                                                                🍽️
                                                                            </div>
                                                                        )}

                                                                        {/* Selection Indicator */}
                                                                        <div className={`absolute top-3 right-3 w-8 h-8 rounded-full border-3 flex items-center justify-center transition-all ${
                                                                            isSelected
                                                                                ? 'bg-orange-500 border-white scale-110'
                                                                                : 'bg-white border-neutral-300'
                                                                        }`}>
                                                                            {isSelected && (
                                                                                <svg className="w-5 h-5 text-white" fill="currentColor" viewBox="0 0 20 20">
                                                                                    <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd"/>
                                                                                </svg>
                                                                            )}
                                                                        </div>

                                                                        {/* Cuisine Badge */}
                                                                        {food.cuisineType && (
                                                                            <div className="absolute bottom-3 left-3 bg-white/95 backdrop-blur-sm px-3 py-1 rounded-full text-xs font-semibold text-neutral-700 shadow-md">
                                                                                {food.cuisineType}
                                                                            </div>
                                                                        )}
                                                                    </div>

                                                                    {/* Content Section */}
                                                                    <div className="p-4">
                                                                        <h4 className="font-bold text-neutral-900 mb-1 text-lg line-clamp-1">
                                                                            {food.foodName}
                                                                        </h4>
                                                                        {food.description && (
                                                                            <p className="text-xs text-neutral-600 mb-3 line-clamp-2 leading-relaxed">
                                                                                {food.description}
                                                                            </p>
                                                                        )}
                                                                        <div className="flex items-center justify-between">
                                                                            <span className="text-lg font-bold text-orange-600">
                                                                                ₹{food.price}
                                                                            </span>
                                                                            {isSelected && (
                                                                                <span className="text-xs font-bold text-green-600 bg-green-50 px-2 py-1 rounded-full">
                                                                                    SELECTED
                                                                                </span>
                                                                            )}
                                                                        </div>
                                                                    </div>
                                                                </button>
                                                            );
                                                        })}
                                                    </div>
                                                )}
                                            </div>
                                        </div>
                                    );
                                })}
                            </div>
                        )}
                    </div>

                    {/* Footer */}
                    {!isLoading && !error && packageData && (
                        <div className="border-t-2 border-neutral-200 bg-white p-6 sticky bottom-0">
                            <div className="flex items-center justify-between gap-4 max-w-6xl mx-auto">
                                <div className="flex-1">
                                    {isSelectionComplete() ? (
                                        <div className="flex items-center gap-2 text-green-600">
                                            <svg className="w-6 h-6" fill="currentColor" viewBox="0 0 20 20">
                                                <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd"/>
                                            </svg>
                                            <span className="font-bold text-lg">All selections complete!</span>
                                        </div>
                                    ) : (
                                        <div className="text-neutral-600">
                                            <p className="font-medium">⚠️ Please select at least one item from each category</p>
                                            <p className="text-sm text-neutral-500">All food categories require at least one selection to proceed</p>
                                        </div>
                                    )}
                                </div>
                                <div className="flex gap-3">
                                    <button
                                        onClick={onClose}
                                        className="px-6 py-3 border-2 border-neutral-300 rounded-xl font-bold text-neutral-700 hover:bg-neutral-50 transition-colors"
                                    >
                                        Cancel
                                    </button>
                                    <button
                                        onClick={handleConfirmSelection}
                                        disabled={!isSelectionComplete()}
                                        className={`px-8 py-3 rounded-xl font-bold text-white transition-all transform hover:scale-105 ${
                                            isSelectionComplete()
                                                ? 'bg-gradient-to-r from-orange-500 to-red-500 hover:shadow-xl'
                                                : 'bg-neutral-300 cursor-not-allowed'
                                        }`}
                                    >
                                        Confirm Selection
                                    </button>
                                </div>
                            </div>
                        </div>
                    )}
                </div>
            </div>

            {/* Media Viewer Modal */}
            <MediaViewerModal
                isOpen={mediaViewer.isOpen}
                onClose={closeMediaViewer}
                mediaUrl={mediaViewer.mediaUrl}
                mediaType={mediaViewer.mediaType}
                foodName={mediaViewer.foodName}
            />

            {/* Sample Taste Modal - Conditionally shown when selected items have sample tasting */}
            {showSampleTasteModal && (
                <SampleTasteModal
                    isOpen={showSampleTasteModal}
                    onClose={handleSampleTasteClose}
                    foodItems={sampleTasteItems}
                    onConfirm={handleSampleTasteComplete}
                />
            )}
        </>
    );
}
