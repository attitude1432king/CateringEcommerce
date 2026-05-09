/*
========================================
File: src/pages/CateringDetailPage.jsx
========================================
Premium Catering Detail Page - Following Zomato/Airbnb Best Practices
User mindset: "Can this caterer handle my event, and what food experience will my guests get?"
Structure: Banner+Logo Hero → Packages → À La Carte Menu → Sample Tasting → Decorations → Reviews → Kitchen
*/
import React, { useState, useEffect, useRef, useMemo } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { motion as Motion } from 'framer-motion';
import { cateringApi, extractData, isSuccessResponse } from '../services/cateringApi';
import { useCart } from '../contexts/CartContext';
import { useEvent } from '../contexts/EventContext';
import { useToast } from '../contexts/ToastContext';
import Loader from '../components/common/Loader';
import MediaViewer from '../components/admin/ui/MediaViewer';
import PackageSelectionModal from '../components/user/PackageSelectionModal';
import SampleTasteModal from '../components/user/SampleTasteModal';
import EventSetupModal from '../components/user/EventSetupModal';
import AvailabilityCalendarModal from '../components/user/AvailabilityCalendarModal';
import VegNonVegIcon from '../components/common/VegNonVegIcon';
import { useAuthGuard } from '../hooks/useAuthGuard';
import AuthModal from '../components/user/AuthModal';
import { useAppSettings } from '../contexts/AppSettingsContext';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL.replace(/\/$/, '');

const formatDateToYmd = (date) => {
    const year = date.getFullYear();
    const month = `${date.getMonth() + 1}`.padStart(2, '0');
    const day = `${date.getDate()}`.padStart(2, '0');
    return `${year}-${month}-${day}`;
};

const parseYmdToDate = (value) => {
    if (!value) return null;
    const [year, month, day] = value.split('-').map(Number);
    if (!year || !month || !day) return null;
    return new Date(year, month - 1, day);
};

export default function CateringDetailPage() {
    const { id } = useParams();
    const navigate = useNavigate();
    const { addToCart } = useCart();
    const { eventData, isSetupComplete, updateEventDetails } = useEvent();
    const { showToast } = useToast();
    const { getInt } = useAppSettings();
    const { isAuthenticated, triggerAuth, showAuthModal, handleAuthClose, handleAuthSuccess: handleAuthSuccessCart } = useAuthGuard();

    // State management
    const [cateringDetail, setCateringDetail] = useState(null);
    const [packages, setPackages] = useState([]);
    const [packageCategories, setPackageCategories] = useState({}); // { packageId: [categories] }
    const [foodCategories, setFoodCategories] = useState([]);
    const [foodItems, setFoodItems] = useState([]);
    const [sampleItems, setSampleItems] = useState([]);
    const [decorations, setDecorations] = useState([]);
    const [reviews, setReviews] = useState([]);
    const [coupons, setCoupons] = useState([]);
    const [isLoading, setIsLoading] = useState(true);
    const [activeSection, setActiveSection] = useState('packages');
    const [selectedPackage, setSelectedPackage] = useState(null);
    const [selectedCategory, setSelectedCategory] = useState(null);
    const guestCount = null; // Guest count is finalized during checkout

    // Package Selection Modal State
    const [isPackageModalOpen, setIsPackageModalOpen] = useState(false);
    const [packageForSelection, setPackageForSelection] = useState(null);
    const [packageSelectedItems, setPackageSelectedItems] = useState(null);

    // Sample Taste Modal State
    const [isSampleModalOpen, setIsSampleModalOpen] = useState(false);
    const [selectedSampleItems, setSelectedSampleItems] = useState([]);

    // Sample Taste Modal for Individual Items
    const [isIndividualSampleModalOpen, setIsIndividualSampleModalOpen] = useState(false);
    const [individualSampleItems, setIndividualSampleItems] = useState([]);
    const [pendingIndividualItemsCart, setPendingIndividualItemsCart] = useState(null);

    // Event Setup Modal State (MANDATORY BEFORE ADD TO CART)
    const [isEventSetupModalOpen, setIsEventSetupModalOpen] = useState(false);
    const [isAvailabilityModalOpen, setIsAvailabilityModalOpen] = useState(false);
    const [selectedAvailabilityDate, setSelectedAvailabilityDate] = useState(() => parseYmdToDate(eventData?.eventDate));
    const [availabilityResult, setAvailabilityResult] = useState(null);
    const [isCheckingAvailability, setIsCheckingAvailability] = useState(false);
    const [blockedCalendarDates, setBlockedCalendarDates] = useState([]);
    const [availabilityMonthCache, setAvailabilityMonthCache] = useState({});

    // Cart Replacement Confirmation Modal
    const [cartReplaceConfirmation, setCartReplaceConfirmation] = useState({
        isOpen: false,
        message: '',
        pendingCartData: null
    });

    // Individual food items selection (À la carte)
    const [selectedIndividualItems, setSelectedIndividualItems] = useState([]);
    const [selectedStandaloneDecorations, setSelectedStandaloneDecorations] = useState([]);

    // Media Viewer for Food Items
    const [foodMediaViewer, setFoodMediaViewer] = useState({
        isOpen: false,
        mediaUrl: null,
        mediaType: 'image',
        foodName: '',
        allMedia: []
    });
    const [kitchenMediaViewer, setKitchenMediaViewer] = useState({
        isOpen: false,
        currentIndex: 0
    });

    // Tooltip state for package categories
    const [categoryTooltip, setCategoryTooltip] = useState(null); // packageId or null

    // Ref for category filter scrolling
    const categoryScrollRef = useRef(null);
    const [isDragging, setIsDragging] = useState(false);
    const [startX, setStartX] = useState(0);
    const [scrollLeft, setScrollLeft] = useState(0);
    const minAdvanceBookingDays = getInt('BUSINESS.MIN_ADVANCE_BOOKING_DAYS', 5);
    const minSelectableDate = new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDate() + minAdvanceBookingDays);
    const kitchenMediaItems = [
        ...(cateringDetail?.kitchenPhotos || []).map((media, index) => ({
            filePath: media.mediaUrl,
            fileName: media.caption || `Kitchen Photo ${index + 1}`,
            label: media.caption || 'Kitchen Photo'
        })),
        ...(cateringDetail?.kitchenVideos || []).map((media, index) => ({
            filePath: media.mediaUrl,
            fileName: media.caption || `Kitchen Video ${index + 1}`,
            label: media.caption || 'Kitchen Video'
        }))
    ];
    const standaloneDecorations = useMemo(() => {
        const packageIds = new Set((packages || []).map(pkg => String(pkg.packageId)));

        return (decorations || []).filter((decoration) => {
            const linkedPackageIds = (decoration.includedInPackageIds || '')
                .split(',')
                .map(value => value.trim())
                .filter(Boolean);

            return !linkedPackageIds.some(linkedPackageId => packageIds.has(linkedPackageId));
        });
    }, [decorations, packages]);

    // Mouse drag scroll handlers
    const handleMouseDown = (e) => {
        if (!categoryScrollRef.current) return;
        setIsDragging(true);
        setStartX(e.pageX - categoryScrollRef.current.offsetLeft);
        setScrollLeft(categoryScrollRef.current.scrollLeft);
        categoryScrollRef.current.style.cursor = 'grabbing';
    };

    const handleMouseLeave = () => {
        setIsDragging(false);
        if (categoryScrollRef.current) {
            categoryScrollRef.current.style.cursor = 'grab';
        }
    };

    const handleMouseUp = () => {
        setIsDragging(false);
        if (categoryScrollRef.current) {
            categoryScrollRef.current.style.cursor = 'grab';
        }
    };

    const handleMouseMove = (e) => {
        if (!isDragging || !categoryScrollRef.current) return;
        e.preventDefault();
        const x = e.pageX - categoryScrollRef.current.offsetLeft;
        const walk = (x - startX) * 2; // Scroll speed multiplier
        categoryScrollRef.current.scrollLeft = scrollLeft - walk;
    };

    // Fetch all data on mount
    useEffect(() => {
        setAvailabilityMonthCache({});
        setBlockedCalendarDates([]);
        setAvailabilityResult(null);
        loadCateringData();

        // Prevent body scroll
        document.body.style.overflow = 'hidden';
        return () => {
            document.body.style.overflow = 'unset';
        };
    }, [id]);

    useEffect(() => {
        const contextDate = parseYmdToDate(eventData?.eventDate);
        if (contextDate) {
            setSelectedAvailabilityDate(contextDate);
        }
    }, [eventData?.eventDate]);

    // Intersection Observer for active section tracking
    useEffect(() => {
        const observerOptions = {
            root: null,
            rootMargin: '-100px 0px -80% 0px',
            threshold: 0
        };

        const observerCallback = (entries) => {
            entries.forEach((entry) => {
                if (entry.isIntersecting) {
                    setActiveSection(entry.target.id);
                }
            });
        };

        const observer = new IntersectionObserver(observerCallback, observerOptions);

        // Observe all sections
        const sections = ['overview', 'packages', 'menu', 'decorations', 'reviews', 'kitchen'];
        sections.forEach((sectionId) => {
            const element = document.getElementById(sectionId);
            if (element) {
                observer.observe(element);
            }
        });

        return () => {
            sections.forEach((sectionId) => {
                const element = document.getElementById(sectionId);
                if (element) {
                    observer.unobserve(element);
                }
            });
        };
    }, []);

    const loadCateringData = async () => {
        try {
            setIsLoading(true);

            // Parallel API calls for better performance
            const [detailRes, packagesRes, categoriesRes, foodItemsRes, decorationsRes, reviewsRes, couponsRes] = await Promise.all([
                cateringApi.getCateringDetail(id),
                cateringApi.getPackages(id),
                cateringApi.getFoodCategories(),
                cateringApi.getFoodItems(id),
                cateringApi.getDecorations(id),
                cateringApi.getReviews(id, 1, 6),
                cateringApi.getAvailableCoupons(id).catch(() => null)
            ]);

            if (isSuccessResponse(detailRes)) {
                setCateringDetail(detailRes.data);
            }

            if (isSuccessResponse(packagesRes)) {
                const pkgs = extractData(packagesRes);
                setPackages(pkgs);
                // Fetch categories for each package
                loadPackageCategories(pkgs);
            }

            if (isSuccessResponse(categoriesRes)) {
                setFoodCategories(extractData(categoriesRes));
            }

            if (isSuccessResponse(foodItemsRes)) {
                const items = extractData(foodItemsRes);
                // Separate standalone sample items from regular menu items
                const samples = items.filter(item => item.isSampleTasted && !item.isIncludedInPackage);
                const regularItems = items.filter(item => !item.isSampleTasted);
                setSampleItems(samples);
                setFoodItems(regularItems);
            }

            if (isSuccessResponse(decorationsRes)) {
                setDecorations(extractData(decorationsRes));
            }

            if (isSuccessResponse(reviewsRes)) {
                setReviews(extractData(reviewsRes));
            }

            if (couponsRes && isSuccessResponse(couponsRes)) {
                setCoupons(extractData(couponsRes) || []);
            }

            setIsLoading(false);
        } catch (error) {
            console.error('Error loading catering data:', error);
            setIsLoading(false);
        }
    };

    // Load categories for all packages
    const loadPackageCategories = async (packagesList) => {
        try {
            // Fetch categories for all packages in parallel
            const categoriesPromises = packagesList.map(pkg =>
                cateringApi.getPackageCategories(id, pkg.packageId)
            );

            const categoriesResults = await Promise.all(categoriesPromises);

            // Map package IDs to their categories
            const categoriesMap = {};
            packagesList.forEach((pkg, index) => {
                const result = categoriesResults[index];
                if (isSuccessResponse(result)) {
                    categoriesMap[pkg.packageId] = extractData(result);
                } else {
                    categoriesMap[pkg.packageId] = [];
                }
            });

            setPackageCategories(categoriesMap);
        } catch (error) {
            console.error('Error loading package categories:', error);
        }
    };

    const loadAvailabilityCalendarMonth = async (monthDate) => {
        const year = monthDate.getFullYear();
        const month = monthDate.getMonth() + 1;
        const cacheKey = `${year}-${month}`;

        if (availabilityMonthCache[cacheKey]) {
            setBlockedCalendarDates(availabilityMonthCache[cacheKey]);
            return;
        }

        try {
            const response = await cateringApi.getAvailabilityCalendar(id, year, month);
            const blockedDates = extractData(response)?.blockedDates || response?.data?.blockedDates || [];
            setAvailabilityMonthCache(prev => ({ ...prev, [cacheKey]: blockedDates }));
            setBlockedCalendarDates(blockedDates);
        } catch (error) {
            console.error('Error loading availability calendar:', error);
            setBlockedCalendarDates([]);
        }
    };

    const handleOpenAvailabilityModal = () => {
        setAvailabilityResult(null);
        setIsAvailabilityModalOpen(true);
        const initialDate = selectedAvailabilityDate || parseYmdToDate(eventData?.eventDate) || minSelectableDate;
        setSelectedAvailabilityDate(initialDate);
        loadAvailabilityCalendarMonth(initialDate);
    };

    const handleAvailabilityDateSelect = async (dateValue) => {
        setSelectedAvailabilityDate(dateValue);
        setAvailabilityResult(null);
        setIsCheckingAvailability(true);

        try {
            const response = await cateringApi.checkAvailability(id, formatDateToYmd(dateValue));
            const resultData = extractData(response) || response?.data;
            setAvailabilityResult(resultData);
        } catch (error) {
            setAvailabilityResult({
                isAvailable: false,
                message: error.message || 'Not available',
                availableSlots: 0
            });
        } finally {
            setIsCheckingAvailability(false);
        }
    };

    const handleProceedToBookingFromAvailability = () => {
        if (!selectedAvailabilityDate || !availabilityResult?.isAvailable) {
            return;
        }

        const selectedDateValue = formatDateToYmd(selectedAvailabilityDate);
        updateEventDetails({ eventDate: selectedDateValue });
        setIsAvailabilityModalOpen(false);
        setIsEventSetupModalOpen(true);
        scrollToSection('packages');
        showToast('Date locked in. Complete your event setup to start booking.', 'success');
    };

    // Filter food items by selected category
    const filterFoodItemsByCategory = async (categoryId) => {
        try {
            setSelectedCategory(categoryId);
            const response = await cateringApi.getFoodItems(id, { categoryId });
            if (isSuccessResponse(response)) {
                const items = extractData(response);
                setFoodItems(items.filter(item => !item.isSampleTasted));
            }
        } catch (error) {
            console.error('Error filtering food items:', error);
        }
    };

    // Open package selection modal
    const handleSelectPackageItems = (pkg) => {
        // ✅ ENFORCEMENT: Check if event setup is complete before package customization
        if (!isSetupComplete) {
            showToast('Please complete event setup first', 'warning');
            setIsEventSetupModalOpen(true);
            return;
        }

        setPackageForSelection(pkg);
        setIsPackageModalOpen(true);
    };

    // Handle package item selection completion
    const handlePackageSelectionComplete = (selectionData) => {
        setPackageSelectedItems(selectionData);
        setSelectedSampleItems(selectionData?.sampleTasteSelections || []);
        setSelectedPackage(packageForSelection);
    };

    // Helper function to process addToCart result
    const processAddToCartResult = (result, cartData, successMessage = 'Added to cart successfully!') => {
        if (result.success) {
            showToast(successMessage, 'success');
            return true;
        } else if (result.needsConfirmation) {
            // Show confirmation modal
            setCartReplaceConfirmation({
                isOpen: true,
                message: result.message,
                pendingCartData: cartData
            });
            return false;
        }
        return false;
    };

    // Handle cart replace confirmation
    const handleCartReplaceConfirm = () => {
        const { pendingCartData } = cartReplaceConfirmation;
        if (pendingCartData) {
            const result = addToCart(pendingCartData, true); // force = true
            if (result.success) {
                showToast('Cart updated successfully!', 'success');
            }
        }
        setCartReplaceConfirmation({ isOpen: false, message: '', pendingCartData: null });
    };

    // Handle cart replace cancellation
    const handleCartReplaceCancel = () => {
        setCartReplaceConfirmation({ isOpen: false, message: '', pendingCartData: null });
    };

    // Handle add to cart with validation
    const handleAddToCart = () => {
        // Auth check — show login popup immediately if not signed in
        if (!isAuthenticated) {
            triggerAuth(() => handleAddToCart());
            return;
        }

        // ✅ ENFORCEMENT: Check if event setup is complete
        if (!isSetupComplete) {
            showToast('Please complete event setup first', 'warning');
            setIsEventSetupModalOpen(true);
            return;
        }

        if (!selectedPackage && selectedIndividualItems.length === 0 && selectedStandaloneDecorations.length === 0) {
            showToast('Please select a package or add individual items to cart', 'error');
            return;
        }

        if (selectedPackage) {
            if (!packageSelectedItems && selectedPackage.requiresItemSelection) {
                showToast('Please select items for this package first', 'warning');
                return;
            }

            const cartData = {
                cateringId: Number(id),
                cateringName: cateringDetail?.cateringName || 'Unknown Caterer',
                cateringLogo: cateringDetail?.logoUrl || '',
                packageId: selectedPackage.packageId,
                packageName: selectedPackage.name,
                packagePrice: selectedPackage.pricePerPerson,
                guestCount: guestCount || null, // Will be set during checkout
                eventDate: eventData?.eventDate || (selectedAvailabilityDate ? formatDateToYmd(selectedAvailabilityDate) : null),
                eventType: null,
                eventLocation: null,
                decorationId: packageSelectedItems?.selectedDecoration?.decorationId ?? null,
                decorationName: packageSelectedItems?.selectedDecoration?.name ?? null,
                decorationPrice: packageSelectedItems?.selectedDecoration?.price ?? 0,
                standaloneDecorations: selectedStandaloneDecorations,
                additionalItems: selectedIndividualItems,
                packageSelections: packageSelectedItems || null,
                sampleTasteSelections: selectedSampleItems.length > 0 ? selectedSampleItems : null
            };

            const result = addToCart(cartData);
            processAddToCartResult(result, cartData, 'Package added to cart successfully!');
        } else if (selectedIndividualItems.length > 0 || selectedStandaloneDecorations.length > 0) {
            // Check if any selected individual items have sample tasting available
            const selectedFoodIds = selectedIndividualItems.map(item => item.foodId);

            // Find sample items that match the selected food items (by name or ID)
            const availableSampleItems = sampleItems.filter(sampleItem => {
                // Check if there's a matching food item by foodItemId
                return selectedFoodIds.includes(sampleItem.foodItemId) ||
                    // Or check by name (in case IDs don't match)
                    selectedIndividualItems.some(selectedItem =>
                        selectedItem.name === sampleItem.name
                    );
            });

            if (availableSampleItems.length > 0) {
                // Show Sample Taste Modal for individual items
                const cartData = {
                    cateringId: Number(id),
                    cateringName: cateringDetail?.cateringName || 'Unknown Caterer',
                    cateringLogo: cateringDetail?.logoUrl || '',
                    packageId: null,
                    packageName: 'Custom Order',
                    packagePrice: 0,
                    guestCount: guestCount || null,
                    eventDate: eventData?.eventDate || (selectedAvailabilityDate ? formatDateToYmd(selectedAvailabilityDate) : null),
                    eventType: null,
                    eventLocation: null,
                    decorationId: null,
                    decorationName: null,
                    decorationPrice: 0,
                    standaloneDecorations: selectedStandaloneDecorations,
                    additionalItems: selectedIndividualItems,
                    packageSelections: null,
                    sampleTasteSelections: null
                };

                setIndividualSampleItems(availableSampleItems);
                setPendingIndividualItemsCart(cartData);
                setIsIndividualSampleModalOpen(true);
            } else {
                // No sample items available, add directly to cart
                const cartData = {
                    cateringId: Number(id),
                    cateringName: cateringDetail?.cateringName || 'Unknown Caterer',
                    cateringLogo: cateringDetail?.logoUrl || '',
                    packageId: null,
                    packageName: 'Custom Order',
                    packagePrice: 0,
                    guestCount: guestCount || null,
                    eventDate: eventData?.eventDate || (selectedAvailabilityDate ? formatDateToYmd(selectedAvailabilityDate) : null),
                    eventType: null,
                    eventLocation: null,
                    decorationId: null,
                    decorationName: null,
                    decorationPrice: 0,
                    standaloneDecorations: selectedStandaloneDecorations,
                    additionalItems: selectedIndividualItems,
                    packageSelections: null,
                    sampleTasteSelections: null
                };

                const result = addToCart(cartData);
                processAddToCartResult(result, cartData, 'Items added to cart successfully!');
            }
        }
    };

    // Handle sample taste request
    const handleRequestSampleTaste = () => {
        if (!selectedPackage) {
            showToast('Please select a package first', 'warning');
            return;
        }

        if (sampleItems.length === 0) {
            showToast('No sample taste items available', 'info');
            return;
        }

        setIsSampleModalOpen(true);
    };

    // Handle sample taste selection confirmation
    const handleSampleSelectionConfirm = (selections) => {
        setSelectedSampleItems(selections);
        showToast('Sample taste items selected successfully!', 'success');
    };

    // Handle individual items sample taste completion
    const handleIndividualSampleTasteComplete = (sampleSelections) => {
        setIsIndividualSampleModalOpen(false);

        if (pendingIndividualItemsCart) {
            // Add sample taste selections to cart data
            const finalCartData = {
                ...pendingIndividualItemsCart,
                sampleTasteSelections: sampleSelections && sampleSelections.length > 0 ? sampleSelections : null
            };

            const result = addToCart(finalCartData);
            processAddToCartResult(result, finalCartData, 'Items added to cart successfully!');

            // Clear pending state
            setPendingIndividualItemsCart(null);
        }
    };

    // Handle individual sample taste modal close (skip sample tasting)
    const handleIndividualSampleTasteClose = () => {
        setIsIndividualSampleModalOpen(false);

        if (pendingIndividualItemsCart) {
            // Add to cart without sample taste selections
            const result = addToCart(pendingIndividualItemsCart);
            processAddToCartResult(result, pendingIndividualItemsCart, 'Items added to cart successfully!');

            // Clear pending state
            setPendingIndividualItemsCart(null);
        }
    };

    // Toggle individual food item selection
    const toggleIndividualItem = (item) => {
        const existingIndex = selectedIndividualItems.findIndex(i => i.foodId === item.foodItemId);

        if (existingIndex >= 0) {
            setSelectedIndividualItems(prev => prev.filter((_, idx) => idx !== existingIndex));
            showToast(`${item.name} removed from selection`, 'info');
        } else {
            setSelectedIndividualItems(prev => [
                ...prev,
                {
                    foodId: item.foodItemId,
                    name: item.name,
                    price: item.price,
                    quantity: 1,
                    isVeg: item.isVegetarian
                }
            ]);
            showToast(`${item.name} added to selection`, 'success');
        }
    };

    const isItemSelected = (foodItemId) => {
        return selectedIndividualItems.some(item => item.foodId === foodItemId);
    };

    const toggleStandaloneDecoration = (decoration) => {
        const existing = selectedStandaloneDecorations.some(item => item.decorationId === decoration.decorationId);

        if (existing) {
            setSelectedStandaloneDecorations(prev =>
                prev.filter(item => item.decorationId !== decoration.decorationId)
            );
            showToast(`${decoration.name} removed from selection`, 'info');
            return;
        }

        setSelectedStandaloneDecorations(prev => [
            ...prev,
            {
                decorationId: decoration.decorationId,
                name: decoration.name,
                price: decoration.price,
            }
        ]);
        showToast(`${decoration.name} added to selection`, 'success');
    };

    const isStandaloneDecorationSelected = (decorationId) => {
        return selectedStandaloneDecorations.some(item => item.decorationId === decorationId);
    };

    const handleBack = () => {
        navigate(-1);
    };

    const scrollToSection = (sectionId) => {
        setActiveSection(sectionId);
        document.getElementById(sectionId)?.scrollIntoView({
            behavior: 'smooth',
            block: 'start'
        });
    };

    if (isLoading) {
        return (
            <div className="fixed inset-0 z-50 bg-white flex justify-center items-center">
                <Loader />
            </div>
        );
    }

    if (!cateringDetail) {
        return (
            <div className="fixed inset-0 z-50 bg-white flex flex-col justify-center items-center">
                <p className="text-xl text-neutral-600 mb-4">Catering not found</p>
                <button onClick={handleBack} className="btn-primary px-6 py-2">
                    Go Back
                </button>
            </div>
        );
    }

    // Group food items by category
    const foodItemsByCategory = foodItems.reduce((acc, item) => {
        const category = item.categoryName || 'Others';
        if (!acc[category]) {
            acc[category] = [];
        }
        acc[category].push(item);
        return acc;
    }, {});

    return (
        <Motion.div
            initial={{ y: '100%' }}
            animate={{ y: 0 }}
            exit={{ y: '100%' }}
            transition={{ type: 'spring', damping: 25, stiffness: 200 }}
            className="fixed inset-0 z-[100] bg-white overflow-y-auto"
        >
            {/* Sticky Header */}
            <div className="sticky top-0 z-10 bg-white/95 backdrop-blur-md shadow-sm border-b border-neutral-100 px-4 py-3">
                <div className="flex items-center justify-between max-w-7xl mx-auto">
                    <div className="flex items-center gap-4 flex-1">
                        <button onClick={handleBack} className="icon-btn shrink-0" aria-label="Go back">
                            <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 19l-7-7m0 0l7-7m-7 7h18" />
                            </svg>
                        </button>
                        <div className="flex-1 min-w-0">
                            <h1 className="text-lg font-bold text-neutral-900 leading-tight truncate">
                                {cateringDetail.cateringName}
                            </h1>
                            <div className="flex items-center text-xs text-neutral-500">
                                <span className="bg-green-100 text-green-800 px-1.5 rounded font-bold mr-2">
                                    {cateringDetail.averageRating.toFixed(1)} ★
                                </span>
                                <span>({cateringDetail.totalReviews} reviews) • {cateringDetail.city}</span>
                            </div>
                        </div>
                    </div>
                    <button
                        className="p-2 hover:bg-neutral-100 rounded-full text-neutral-600"
                        title="Share"
                    >
                        <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8.684 13.342C8.886 12.938 9 12.482 9 12c0-.482-.114-.938-.316-1.342m0 2.684a3 3 0 110-2.684m0 2.684l6.632 3.316m-6.632-6l6.632-3.316m0 0a3 3 0 105.367-2.684 3 3 0 00-5.367 2.684zm0 9.316a3 3 0 105.368 2.684 3 3 0 00-5.368-2.684z" />
                        </svg>
                    </button>
                </div>
            </div>

            {/* ✅ ENHANCED HERO SECTION: Rich Banner with Overlay Content (Swiggy/Zomato Inspired) */}
            <div className="relative h-96 md:h-[32rem] lg:h-[36rem] bg-gradient-to-br from-orange-100 via-rose-100 to-pink-100 overflow-hidden">
                {/* Background Banner Image/Video with Blur */}
                {cateringDetail.bannerUrl ? (
                    <>
                        <img
                            src={`${API_BASE_URL}${cateringDetail.bannerUrl}`}
                            alt={`${cateringDetail.cateringName} catering setup`}
                            className="w-full h-full object-cover"
                            loading="lazy" decoding="async"
                        />
                        {/* Dark Gradient Overlay for better content visibility */}
                        <div className="absolute inset-0 bg-gradient-to-t from-black/80 via-black/40 to-black/20"></div>
                    </>
                ) : (
                    <div className="w-full h-full bg-gradient-to-br from-orange-400 via-rose-400 to-pink-500">
                        <div className="absolute inset-0 bg-gradient-to-t from-black/60 via-black/30 to-transparent"></div>
                    </div>
                )}

                {/* Logo Badge - Top Left */}
                {cateringDetail.logoUrl && (
                    <div className="absolute top-6 left-6 bg-white/95 backdrop-blur-sm rounded-2xl shadow-2xl p-2.5 border border-white/50 hover:scale-105 transition-transform">
                        <img
                            src={`${API_BASE_URL}${cateringDetail.logoUrl}`}
                            alt={`${cateringDetail.cateringName} logo`}
                            className="w-14 h-14 md:w-16 md:h-16 object-contain"
                        />
                    </div>
                )}

                {/* Status Badge - Top Right */}
                <div className="absolute top-6 right-6 flex flex-col gap-2 items-end">
                    {cateringDetail.isVerifiedByAdmin && (
                        <div className="bg-blue-600 text-white text-xs px-3 py-1.5 rounded-full font-semibold shadow-lg flex items-center gap-1.5">
                            <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                                <path fillRule="evenodd" d="M6.267 3.455a3.066 3.066 0 001.745-.723 3.066 3.066 0 013.976 0 3.066 3.066 0 001.745.723 3.066 3.066 0 012.812 2.812c.051.643.304 1.254.723 1.745a3.066 3.066 0 010 3.976 3.066 3.066 0 00-.723 1.745 3.066 3.066 0 01-2.812 2.812 3.066 3.066 0 00-1.745.723 3.066 3.066 0 01-3.976 0 3.066 3.066 0 00-1.745-.723 3.066 3.066 0 01-2.812-2.812 3.066 3.066 0 00-.723-1.745 3.066 3.066 0 010-3.976 3.066 3.066 0 00.723-1.745 3.066 3.066 0 012.812-2.812zm7.44 5.252a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                            </svg>
                            Verified
                        </div>
                    )}
                    <div className={`text-xs px-3 py-1.5 rounded-full font-bold shadow-lg ${
                        cateringDetail.isOnline
                            ? 'bg-green-500 text-white'
                            : 'bg-red-500 text-white'
                    }`}>
                        {cateringDetail.isOnline ? '● Available Now' : '● Fully Booked'}
                    </div>
                </div>

                {/* Hero Content - Bottom Positioned (Swiggy Style) */}
                <div className="absolute bottom-0 left-0 right-0 px-4 md:px-8 pb-24 md:pb-28">
                    <div className="max-w-7xl mx-auto">
                        {/* Main Info */}
                        <div className="mb-4">
                            <h1 className="text-3xl md:text-5xl lg:text-6xl font-bold text-white mb-3 drop-shadow-2xl">
                                {cateringDetail.cateringName}
                            </h1>
                            <div className="flex flex-wrap items-center gap-3 mb-3">
                                {/* Rating Badge */}
                                <div className="bg-green-600 text-white px-3 py-1.5 rounded-lg font-bold text-sm flex items-center gap-1 shadow-lg">
                                    <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                                        <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                                    </svg>
                                    {cateringDetail.averageRating.toFixed(1)}
                                </div>
                                <span className="text-white/90 text-sm font-medium">
                                    {cateringDetail.totalReviews} reviews
                                </span>
                                <span className="text-white/70">•</span>
                                <span className="text-white/90 text-sm font-medium">
                                    📍 {cateringDetail.city}, {cateringDetail.area}
                                </span>
                            </div>
                            {/* Tags */}
                            <div className="flex flex-wrap gap-2">
                                {['Wedding', 'Corporate', 'Party', 'Events'].map((tag, idx) => (
                                    <span key={idx} className="bg-white/20 backdrop-blur-md text-white text-xs px-3 py-1 rounded-full font-medium border border-white/30">
                                        {tag}
                                    </span>
                                ))}
                            </div>
                        </div>

                        {/* CTA Buttons Row */}
                        <div className="flex flex-wrap gap-3">
                            <button
                                onClick={() => scrollToSection('packages')}
                                className="bg-gradient-to-r from-primary to-primary-dark hover:from-primary-dark hover:to-primary text-white px-6 py-3 rounded-xl font-bold text-sm shadow-2xl hover:shadow-primary/30 transition-all hover:scale-105 flex items-center gap-2"
                            >
                                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
                                </svg>
                                View Packages
                            </button>
                            <button
                                onClick={handleOpenAvailabilityModal}
                                className="bg-white/95 backdrop-blur-sm hover:bg-white text-neutral-900 px-6 py-3 rounded-xl font-bold text-sm shadow-xl transition-all hover:scale-105 flex items-center gap-2"
                            >
                                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                                </svg>
                                Check Availability
                            </button>
                            {sampleItems.length > 0 && (
                                <button
                                    onClick={() => setIsSampleModalOpen(true)}
                                    className="bg-purple-600/90 backdrop-blur-sm hover:bg-purple-700 text-white px-6 py-3 rounded-xl font-bold text-sm shadow-xl transition-all hover:scale-105 flex items-center gap-2"
                                >
                                    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M14.828 14.828a4 4 0 01-5.656 0M9 10h.01M15 10h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                                    </svg>
                                    Request Sample Taste
                                </button>
                            )}
                        </div>
                    </div>
                </div>
            </div>

            {/* ✅ SWIGGY-STYLE INFO CARD (Floating Glassmorphism Card) */}
            <div className="relative -mt-16 px-4 md:px-8 mb-8 z-10">
                <div className="max-w-7xl mx-auto">
                    <div className="bg-white/95 backdrop-blur-xl rounded-2xl shadow-2xl border border-neutral-200 p-6 md:p-8">
                        <div className="grid grid-cols-2 md:grid-cols-5 gap-6">
                            {/* Price per Plate */}
                            <div className="flex flex-col items-center text-center">
                                <div className="w-12 h-12 bg-gradient-to-br from-primary to-primary-dark rounded-xl flex items-center justify-center mb-3 shadow-lg">
                                    <svg className="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                                    </svg>
                                </div>
                                <div className="text-2xl font-bold text-neutral-900">₹{packages.length > 0 ? packages[0].pricePerPerson : cateringDetail.minOrderValue}</div>
                                <div className="text-xs text-neutral-500 mt-1">per plate</div>
                            </div>

                            {/* Cuisine Types */}
                            <div className="flex flex-col items-center text-center">
                                <div className="w-12 h-12 bg-gradient-to-br from-green-500 to-green-600 rounded-xl flex items-center justify-center mb-3 shadow-lg">
                                    <svg className="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z" />
                                    </svg>
                                </div>
                                <div className="text-sm font-bold text-neutral-900">Multi-Cuisine</div>
                                <div className="text-xs text-neutral-500 mt-1">Available</div>
                            </div>

                            {/* Min Order */}
                            <div className="flex flex-col items-center text-center">
                                <div className="w-12 h-12 bg-gradient-to-br from-blue-500 to-blue-600 rounded-xl flex items-center justify-center mb-3 shadow-lg">
                                    <svg className="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                                    </svg>
                                </div>
                                <div className="text-lg font-bold text-neutral-900">₹{cateringDetail.minOrderValue}</div>
                                <div className="text-xs text-neutral-500 mt-1">Min Order</div>
                            </div>

                            {/* Delivery Radius */}
                            <div className="flex flex-col items-center text-center">
                                <div className="w-12 h-12 bg-gradient-to-br from-purple-500 to-purple-600 rounded-xl flex items-center justify-center mb-3 shadow-lg">
                                    <svg className="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                                    </svg>
                                </div>
                                <div className="text-lg font-bold text-neutral-900">{cateringDetail.deliveryRadiusKm} km</div>
                                <div className="text-xs text-neutral-500 mt-1">Delivery</div>
                            </div>

                            {/* Open/Close Time */}
                            <div className="flex flex-col items-center text-center">
                                <div className="w-12 h-12 bg-gradient-to-br from-rose-500 to-rose-600 rounded-xl flex items-center justify-center mb-3 shadow-lg">
                                    <svg className="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                                    </svg>
                                </div>
                                <div className="text-sm font-bold text-neutral-900">9 AM - 9 PM</div>
                                <div className="text-xs text-neutral-500 mt-1">Open Hours</div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            {/* OFFERS & DISCOUNTS SECTION - Dynamic from database */}
            {coupons.length > 0 && (
                <div className="px-4 md:px-8 mb-8">
                    <div className="max-w-7xl mx-auto">
                        <h3 className="text-xl font-bold text-neutral-900 mb-4 flex items-center gap-2">
                            <svg className="w-6 h-6 text-primary" fill="currentColor" viewBox="0 0 20 20">
                                <path d="M2 6a2 2 0 012-2h6a2 2 0 012 2v8a2 2 0 01-2 2H4a2 2 0 01-2-2V6zM14.553 7.106A1 1 0 0014 8v4a1 1 0 00.553.894l2 1A1 1 0 0018 13V7a1 1 0 00-1.447-.894l-2 1z" />
                            </svg>
                            Available Offers
                        </h3>
                        <div className="flex gap-4 overflow-x-auto scrollbar-hide pb-2">
                            {coupons.map((coupon, index) => {
                                const isPercentage = coupon.discountType === 'Percentage';
                                const isBest = index === 0 && isPercentage;
                                const heading = isPercentage
                                    ? `${coupon.discountValue}% OFF`
                                    : `Flat ₹${Number(coupon.discountValue).toLocaleString('en-IN')} OFF`;
                                const subtext = coupon.minOrderValue
                                    ? `On orders above ₹${Number(coupon.minOrderValue).toLocaleString('en-IN')}`
                                    : coupon.description || coupon.name;
                                const validity = coupon.validTo
                                    ? `Valid till ${new Date(coupon.validTo).toLocaleDateString('en-IN', { day: 'numeric', month: 'short', year: 'numeric' })}`
                                    : '';

                                if (isBest) {
                                    return (
                                        <div key={coupon.discountId} className="min-w-[280px] bg-gradient-to-br from-primary to-primary-dark text-white rounded-2xl p-5 shadow-xl border-2 border-orange-400 relative overflow-hidden">
                                            <div className="absolute top-2 right-2 bg-yellow-400 text-orange-900 text-xs px-2 py-1 rounded-full font-bold">
                                                BEST OFFER
                                            </div>
                                            <div className="text-3xl font-bold mb-1">{heading}</div>
                                            {subtext && <div className="text-sm opacity-90 mb-2">{subtext}</div>}
                                            {coupon.maxDiscount && (
                                                <div className="text-xs opacity-80 mb-2">Max discount ₹{Number(coupon.maxDiscount).toLocaleString('en-IN')}</div>
                                            )}
                                            <div className="flex items-center gap-2 flex-wrap mt-1">
                                                {validity && (
                                                    <div className="text-xs bg-white/20 rounded-lg px-3 py-1.5 inline-block">{validity}</div>
                                                )}
                                                <div className="text-xs bg-white/30 rounded-lg px-3 py-1.5 inline-block font-mono font-bold tracking-wider">{coupon.couponCode}</div>
                                            </div>
                                        </div>
                                    );
                                }

                                return (
                                    <div key={coupon.discountId} className="min-w-[280px] bg-white border-2 border-purple-200 rounded-2xl p-5 shadow-lg">
                                        <div className="text-2xl font-bold text-purple-600 mb-1">{heading}</div>
                                        {subtext && <div className="text-sm text-neutral-600 mb-2">{subtext}</div>}
                                        {coupon.maxDiscount && (
                                            <div className="text-xs text-neutral-500 mb-2">Max discount ₹{Number(coupon.maxDiscount).toLocaleString('en-IN')}</div>
                                        )}
                                        <div className="flex items-center gap-2 flex-wrap mt-1">
                                            {validity && (
                                                <div className="text-xs bg-purple-100 text-purple-700 rounded-lg px-3 py-1.5 inline-block font-medium">{validity}</div>
                                            )}
                                            <div className="text-xs bg-neutral-100 text-neutral-700 rounded-lg px-3 py-1.5 inline-block font-mono font-bold tracking-wider">{coupon.couponCode}</div>
                                        </div>
                                    </div>
                                );
                            })}
                        </div>
                    </div>
                </div>
            )}

            {/* ✅ ENHANCED STICKY TAB NAVIGATION (Swiggy Style with Smooth Animations) */}
            <div className="sticky top-[73px] z-20 bg-white/95 backdrop-blur-xl border-b-2 border-neutral-200 shadow-lg">
                <div className="max-w-7xl mx-auto px-4">
                    <div className="flex space-x-1 overflow-x-auto scrollbar-hide">
                        {[
                            { id: 'overview', label: 'Overview', icon: '📋' },
                            { id: 'packages', label: 'Packages', icon: '📦' },
                            { id: 'menu', label: 'Menu', icon: '🍴' },
                            { id: 'decorations', label: 'Decorations', icon: '🎨' },
                            { id: 'reviews', label: 'Reviews', icon: '⭐' },
                            { id: 'kitchen', label: 'Kitchen', icon: '👨‍🍳' }
                        ].map((tab) => (
                            <button
                                key={tab.id}
                                onClick={() => scrollToSection(tab.id)}
                                className={`relative py-4 px-6 whitespace-nowrap text-sm font-bold transition-all duration-300 ${
                                    activeSection === tab.id
                                        ? 'text-primary'
                                        : 'text-neutral-600 hover:text-neutral-900'
                                }`}
                            >
                                <span className="flex items-center gap-2">
                                    <span className="text-base">{tab.icon}</span>
                                    {tab.label}
                                </span>
                                {/* Animated underline */}
                                {activeSection === tab.id && (
                                    <Motion.div
                                        layoutId="activeTab"
                                        className="absolute bottom-0 left-0 right-0 h-1 bg-gradient-to-r from-primary to-primary-dark rounded-t-full"
                                        transition={{ type: "spring", bounce: 0.2, duration: 0.6 }}
                                    />
                                )}
                            </button>
                        ))}
                    </div>
                </div>
            </div>

            {/* Main Content */}
            <div className="max-w-7xl mx-auto p-4 md:p-6 space-y-16 pb-32">

                {/* ✅ 0. OVERVIEW SECTION (New Addition) */}
                <section id="overview" className="scroll-mt-32">
                    <div className="bg-gradient-to-br from-orange-50 via-white to-rose-50 rounded-3xl p-8 md:p-10 border-2 border-orange-100 shadow-xl">
                        <div className="flex items-start gap-6 mb-6">
                            <div className="w-16 h-16 bg-gradient-to-br from-primary to-primary-dark rounded-2xl flex items-center justify-center flex-shrink-0 shadow-lg">
                                <svg className="w-8 h-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                                </svg>
                            </div>
                            <div className="flex-1">
                                <h2 className="text-2xl md:text-3xl font-bold text-neutral-900 mb-3">About {cateringDetail.cateringName}</h2>
                                <p className="text-neutral-700 leading-relaxed text-base">
                                    {cateringDetail.description || `${cateringDetail.cateringName} is a premium catering service provider specializing in weddings, corporate events, and special occasions. We offer a wide range of packages and à la carte menu options to suit your event needs.`}
                                </p>
                            </div>
                        </div>

                        {/* Sample Taste Availability Highlight */}
                        {sampleItems.length > 0 && (
                            <div className="bg-gradient-to-r from-purple-500 to-purple-600 text-white rounded-2xl p-6 shadow-xl">
                                <div className="flex items-start justify-between gap-4">
                                    <div className="flex-1">
                                        <div className="flex items-center gap-3 mb-2">
                                            <svg className="w-7 h-7" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                                            </svg>
                                            <h3 className="text-xl font-bold">Sample Taste Available!</h3>
                                        </div>
                                        <p className="text-purple-100 text-sm mb-4">
                                            Try before you commit! We offer complimentary sample tasting for <span className="font-bold text-white">{sampleItems.length} premium dishes</span> from our menu.
                                        </p>
                                        <div className="flex flex-wrap gap-2">
                                            <div className="bg-white/20 backdrop-blur-sm rounded-lg px-3 py-2 text-xs font-medium flex items-center gap-2">
                                                <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                                                    <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                                                </svg>
                                                Free Tasting
                                            </div>
                                            <div className="bg-white/20 backdrop-blur-sm rounded-lg px-3 py-2 text-xs font-medium flex items-center gap-2">
                                                <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                                                    <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                                                </svg>
                                                {sampleItems.length} Items Available
                                            </div>
                                            <div className="bg-white/20 backdrop-blur-sm rounded-lg px-3 py-2 text-xs font-medium flex items-center gap-2">
                                                <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                                                    <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                                                </svg>
                                                No Commitment Required
                                            </div>
                                        </div>
                                    </div>
                                    <button
                                        onClick={() => setIsSampleModalOpen(true)}
                                        className="bg-white text-purple-600 px-6 py-3 rounded-xl font-bold text-sm shadow-lg hover:shadow-xl transition-all hover:scale-105 whitespace-nowrap"
                                    >
                                        Select Items
                                    </button>
                                </div>
                            </div>
                        )}

                        {/* No Sample Taste Available */}
                        {sampleItems.length === 0 && (
                            <div className="bg-neutral-100 border-2 border-neutral-200 rounded-2xl p-5 text-center">
                                <div className="text-4xl mb-2">ℹ️</div>
                                <p className="text-neutral-600 text-sm font-medium">
                                    Sample taste not available at this time. Please contact us for more details.
                                </p>
                            </div>
                        )}
                    </div>
                </section>

                {/* ✅ 1. PACKAGES SECTION (With Sample Tasting Inside) */}
                <section id="packages" className="scroll-mt-32">
                    <div className="mb-6">
                        <h2 className="text-2xl md:text-3xl font-bold text-neutral-900">Choose Your Package</h2>
                        <p className="text-sm text-neutral-600 mt-1">Complete catering solutions for your event. Guest count will be set during checkout.</p>
                    </div>

                    {packages.length === 0 ? (
                        <div className="text-center py-16 text-neutral-500 bg-neutral-50 rounded-2xl">
                            <div className="text-6xl mb-4">📦</div>
                            <p className="text-lg">No packages available at the moment</p>
                        </div>
                    ) : (
                        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                            {packages.map(pkg => (
                                <div
                                    key={pkg.packageId}
                                    onClick={() => setSelectedPackage(pkg)}
                                    className={`group relative border-3 rounded-2xl overflow-hidden shadow-lg cursor-pointer transition-all duration-300 hover:shadow-2xl hover:-translate-y-2 ${
                                        selectedPackage?.packageId === pkg.packageId
                                            ? 'border-primary ring-4 ring-primary/20 bg-gradient-to-br from-orange-50 to-rose-50'
                                            : 'border-neutral-200 hover:border-neutral-300 bg-white'
                                    }`}
                                >
                                    {/* Package Hero Image */}
                                    {pkg.imageUrl && (
                                        <div className="h-48 overflow-hidden">
                                            <img
                                                src={`${API_BASE_URL}${pkg.imageUrl}`}
                                                alt={pkg.name}
                                                className="w-full h-full object-cover group-hover:scale-110 transition-transform duration-500"
                                            />
                                        </div>
                                    )}

                                    {/* Selected Checkmark */}
                                    {selectedPackage?.packageId === pkg.packageId && (
                                        <div className="absolute top-4 right-4 bg-primary text-white rounded-full p-2 shadow-xl">
                                            <svg className="w-6 h-6" fill="currentColor" viewBox="0 0 20 20">
                                                <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
                                            </svg>
                                        </div>
                                    )}

                                    <div className="p-6">
                                        {/* Package Name & Description */}
                                        <div className="mb-4">
                                            <h3 className="font-bold text-xl text-neutral-900 mb-2">{pkg.name}</h3>
                                            <p className="text-sm text-neutral-600 line-clamp-2">{pkg.description}</p>
                                        </div>

                                        {/* Price */}
                                        <div className="flex items-baseline gap-2 mb-4">
                                            <div className="text-3xl font-bold text-primary">
                                                ₹{pkg.pricePerPerson}
                                            </div>
                                            <div className="text-sm text-neutral-500">per person</div>
                                        </div>

                                        {/* Food Categories Included */}
                                        {packageCategories[pkg.packageId] && packageCategories[pkg.packageId].length > 0 && (
                                            <div className="border-t border-neutral-200 pt-4 mb-4">
                                                <div className="text-sm font-semibold text-neutral-700 mb-3 flex items-center gap-2">
                                                    <svg className="w-4 h-4 text-primary" fill="currentColor" viewBox="0 0 20 20">
                                                        <path d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z" />
                                                    </svg>
                                                    Food Categories
                                                </div>
                                                <div className="flex flex-wrap gap-2">
                                                    {packageCategories[pkg.packageId].slice(0, 4).map((category, idx) => (
                                                        <span
                                                            key={idx}
                                                            className="inline-flex items-center px-3 py-1.5 rounded-full text-xs font-semibold bg-gradient-to-r from-orange-100 to-rose-100 text-orange-800 border border-orange-200"
                                                        >
                                                            {category.categoryName || category.name || category}
                                                        </span>
                                                    ))}
                                                    {packageCategories[pkg.packageId].length > 4 && (
                                                        <div className="relative inline-block">
                                                            <button
                                                                onMouseEnter={() => setCategoryTooltip(pkg.packageId)}
                                                                onMouseLeave={() => setCategoryTooltip(null)}
                                                                className="inline-flex items-center px-3 py-1.5 rounded-full text-xs font-semibold bg-blue-100 text-blue-700 border border-blue-200 hover:bg-blue-200 transition-colors cursor-pointer"
                                                            >
                                                                +{packageCategories[pkg.packageId].length - 4} More...
                                                            </button>
                                                            {categoryTooltip === pkg.packageId && (
                                                                <div className="absolute z-50 bottom-full left-0 mb-2 w-64 bg-white border-2 border-neutral-200 rounded-xl shadow-2xl p-4">
                                                                    <div className="text-xs font-bold text-neutral-800 mb-2">All Categories:</div>
                                                                    <div className="flex flex-wrap gap-1.5 max-h-40 overflow-y-auto">
                                                                        {packageCategories[pkg.packageId].map((category, idx) => (
                                                                            <span
                                                                                key={idx}
                                                                                className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-gradient-to-r from-orange-100 to-rose-100 text-orange-800 border border-orange-200"
                                                                            >
                                                                                {category.categoryName || category.name || category}
                                                                            </span>
                                                                        ))}
                                                                    </div>
                                                                    {/* Tooltip arrow */}
                                                                    <div className="absolute bottom-[-8px] left-4 w-4 h-4 bg-white border-r-2 border-b-2 border-neutral-200 transform rotate-45"></div>
                                                                </div>
                                                            )}
                                                        </div>
                                                    )}
                                                </div>
                                            </div>
                                        )}

                                        {/* Items Included (Expandable) - Fallback if no categories */}
                                        {(!packageCategories[pkg.packageId] || packageCategories[pkg.packageId].length === 0) && pkg.items && pkg.items.length > 0 && (
                                            <div className="border-t border-neutral-200 pt-4 mb-4">
                                                <div className="text-sm font-semibold text-neutral-700 mb-3 flex items-center gap-2">
                                                    <svg className="w-4 h-4 text-primary" fill="currentColor" viewBox="0 0 20 20">
                                                        <path d="M9 2a1 1 0 000 2h2a1 1 0 100-2H9z" />
                                                        <path fillRule="evenodd" d="M4 5a2 2 0 012-2 3 3 0 003 3h2a3 3 0 003-3 2 2 0 012 2v11a2 2 0 01-2 2H6a2 2 0 01-2-2V5zm3 4a1 1 0 000 2h.01a1 1 0 100-2H7zm3 0a1 1 0 000 2h3a1 1 0 100-2h-3zm-3 4a1 1 0 100 2h.01a1 1 0 100-2H7zm3 0a1 1 0 100 2h3a1 1 0 100-2h-3z" clipRule="evenodd" />
                                                    </svg>
                                                    {pkg.items.length} Items Included
                                                </div>
                                                <ul className="space-y-2">
                                                    {pkg.items.slice(0, 4).map((item, i) => (
                                                        <li key={i} className="text-sm text-neutral-600 flex items-start gap-2">
                                                            <span className="w-1.5 h-1.5 bg-primary rounded-full mt-1.5 flex-shrink-0"></span>
                                                            <span className="flex-1">{item.description}</span>
                                                        </li>
                                                    ))}
                                                    {pkg.items.length > 4 && (
                                                        <li className="text-sm text-primary font-semibold pt-1">
                                                            +{pkg.items.length - 4} more delicious items
                                                        </li>
                                                    )}
                                                </ul>
                                            </div>
                                        )}

                                        {/* Select Package CTA */}
                                        <button
                                            onClick={(e) => {
                                                e.stopPropagation();
                                                handleSelectPackageItems(pkg);
                                            }}
                                            className="w-full bg-gradient-to-r from-primary to-primary-dark text-white py-3 px-4 rounded-xl font-semibold text-sm hover:shadow-lg transition-all flex items-center justify-center gap-2"
                                        >
                                            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                                            </svg>
                                            Select Package
                                        </button>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}

                    {/* ✅ Sample Tasting Section (Inside Packages) */}
                    {selectedPackage && sampleItems.length > 0 && (
                        <div className="mt-8 bg-gradient-to-br from-amber-50 via-orange-50 to-rose-50 border-2 border-orange-200 rounded-2xl p-8 shadow-lg">
                            <div className="flex items-start justify-between mb-6">
                                <div className="flex-1">
                                    <div className="flex items-center gap-4 mb-3">
                                        <div className="bg-primary text-white p-3 rounded-xl shadow-md">
                                            <svg className="w-7 h-7" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                                            </svg>
                                        </div>
                                        <div>
                                            <h3 className="text-2xl font-bold text-neutral-900">
                                                {selectedPackage.name} Selected
                                            </h3>
                                            <p className="text-sm text-neutral-600 mt-1">
                                                ₹{selectedPackage.pricePerPerson}/person • Guest count will be set during checkout
                                            </p>
                                        </div>
                                    </div>

                                    {/* Sample Selection Indicator */}
                                    {selectedSampleItems.length > 0 && (
                                        <div className="ml-16 flex items-center gap-2 text-sm bg-white rounded-lg px-4 py-2 inline-flex shadow-sm">
                                            <svg className="w-5 h-5 text-green-600" fill="currentColor" viewBox="0 0 20 20">
                                                <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                                            </svg>
                                            <span className="font-semibold text-green-700">
                                                {selectedSampleItems.reduce((sum, cat) => sum + cat.selectedItems.length, 0)} sample items selected for tasting
                                            </span>
                                        </div>
                                    )}
                                </div>
                                <button
                                    onClick={() => setSelectedPackage(null)}
                                    className="text-neutral-400 hover:text-neutral-700 transition-colors p-2"
                                    title="Remove selection"
                                >
                                    <svg className="w-7 h-7" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                                    </svg>
                                </button>
                            </div>

                            {/* Sample Items Grid */}
                            <div className="bg-white rounded-xl p-6 mb-6 shadow-sm">
                                <div className="flex items-center gap-3 mb-4">
                                    <svg className="w-6 h-6 text-amber-600" fill="currentColor" viewBox="0 0 20 20">
                                        <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                                    </svg>
                                    <h4 className="font-bold text-lg text-neutral-900">Available for Sample Tasting</h4>
                                </div>
                                <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-3">
                                    {sampleItems.slice(0, 8).map(item => {
                                        const sampleImg = item.imageUrls && item.imageUrls.length > 0
                                            ? (item.imageUrls[0].startsWith('http') ? item.imageUrls[0] : `${API_BASE_URL}${item.imageUrls[0]}`)
                                            : null;
                                        const sampleVideo = item.videoUrl
                                            ? (item.videoUrl.startsWith('http') ? item.videoUrl : `${API_BASE_URL}${item.videoUrl}`)
                                            : null;
                                        return (
                                            <div key={item.foodItemId} className="flex items-center gap-2 text-sm text-neutral-700 bg-neutral-50 rounded-lg p-2">
                                                <div className="w-10 h-10 rounded overflow-hidden flex-shrink-0 relative bg-neutral-200">
                                                    {sampleImg ? (
                                                        <img src={sampleImg} alt={item.name} className="w-full h-full object-cover" />
                                                    ) : sampleVideo ? (
                                                        <video src={sampleVideo} className="w-full h-full object-cover" muted playsInline />
                                                    ) : (
                                                        <div className="w-full h-full flex items-center justify-center text-lg">
                                                            {item.isVegetarian ? '🥗' : '🍖'}
                                                        </div>
                                                    )}
                                                </div>
                                                <span className="flex-1 line-clamp-2 text-xs font-medium">{item.name}</span>
                                            </div>
                                        );
                                    })}
                                </div>
                                {sampleItems.length > 8 && (
                                    <p className="text-sm text-neutral-600 mt-3">+{sampleItems.length - 8} more items available for tasting</p>
                                )}
                            </div>

                            {/* Action Buttons */}
                            <div className="flex gap-4">
                                <button
                                    onClick={handleRequestSampleTaste}
                                    className="flex-1 bg-white border-3 border-primary text-primary py-4 px-6 rounded-xl font-bold text-base hover:bg-primary/5 transition-all shadow-md hover:shadow-lg flex items-center justify-center gap-3"
                                >
                                    <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M14.828 14.828a4 4 0 01-5.656 0M9 10h.01M15 10h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                                    </svg>
                                    {selectedSampleItems.length > 0 ? 'Update Sample Taste' : 'Request Sample Taste'}
                                </button>
                                <button
                                    onClick={() => handleSelectPackageItems(selectedPackage)}
                                    className="flex-1 bg-gradient-to-r from-primary to-primary-dark text-white py-4 px-6 rounded-xl font-bold text-base hover:shadow-xl transition-all shadow-lg flex items-center justify-center gap-3"
                                >
                                    <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                                    </svg>
                                    Customize Package
                                </button>
                            </div>
                        </div>
                    )}
                </section>

                {/* ✅ 2. À LA CARTE MENU SECTION (Individual Items ONLY) */}
                <section id="menu" className="scroll-mt-32">
                    <div className="mb-6">
                        <h2 className="text-2xl md:text-3xl font-bold text-neutral-900">Indivaul Items</h2>
                        <p className="text-sm text-neutral-600 mt-1">Customize your menu with individual dishes</p>
                    </div>

                    {/* Category Filter - Horizontal Scrollable with Mouse Drag */}
                    {foodCategories.length > 0 && (
                        <div
                            ref={categoryScrollRef}
                            onMouseDown={handleMouseDown}
                            onMouseLeave={handleMouseLeave}
                            onMouseUp={handleMouseUp}
                            onMouseMove={handleMouseMove}
                            className="mb-8 flex gap-3 overflow-x-auto pb-2 cursor-grab select-none"
                            style={{
                                scrollbarWidth: 'none', // Firefox
                                msOverflowStyle: 'none', // IE and Edge
                            }}
                        >
                            <style>{`
                                .category-scroll::-webkit-scrollbar {
                                    display: none; /* Chrome, Safari, Opera */
                                }
                            `}</style>
                            <button
                                onClick={() => {
                                    setSelectedCategory(null);
                                    loadCateringData();
                                }}
                                className={`px-6 py-3 rounded-xl text-sm font-semibold whitespace-nowrap transition-all shadow-sm flex-shrink-0 ${
                                    selectedCategory === null
                                        ? 'bg-gradient-to-r from-primary to-primary-dark text-white shadow-lg scale-105'
                                        : 'bg-white text-neutral-700 border-2 border-neutral-200 hover:border-primary'
                                }`}
                            >
                                All Items
                            </button>
                            {foodCategories.map(cat => (
                                <button
                                    key={cat.categoryId}
                                    onClick={() => filterFoodItemsByCategory(cat.categoryId)}
                                    className={`px-6 py-3 rounded-xl text-sm font-semibold whitespace-nowrap transition-all shadow-sm flex-shrink-0 ${
                                        selectedCategory === cat.categoryId
                                            ? 'bg-gradient-to-r from-primary to-primary-dark text-white shadow-lg scale-105'
                                            : 'bg-white text-neutral-700 border-2 border-neutral-200 hover:border-primary'
                                    }`}
                                >
                                    {cat.name}
                                </button>
                            ))}
                        </div>
                    )}

                    {foodItems.length === 0 ? (
                        <div className="text-center py-16 text-neutral-500 bg-neutral-50 rounded-2xl">
                            <div className="text-6xl mb-4">🍽️</div>
                            <p className="text-lg">No menu items available</p>
                        </div>
                    ) : (
                        Object.entries(foodItemsByCategory).map(([category, items]) => (
                            <div key={category} className="mb-12">
                                {/* Category Header */}
                                <h3 className="text-xl font-bold text-neutral-800 mb-6 flex items-center gap-3">
                                    <div className="h-1 w-12 bg-gradient-to-r from-primary to-primary-dark rounded-full"></div>
                                    {category}
                                    <div className="h-1 flex-1 bg-neutral-200 rounded-full"></div>
                                </h3>

                                {/* Food Items Grid */}
                                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-5">
                                    {items.map(item => {
                                        const selected = isItemSelected(item.foodItemId);
                                        const hasVideo = item.videoUrl && item.videoUrl.trim() !== '';
                                        const hasImages = item.imageUrls && item.imageUrls.length > 0;
                                        const displayMedia = hasVideo ? item.videoUrl : (hasImages ? item.imageUrls[0] : null);
                                        const mediaType = hasVideo ? 'video' : 'image';

                                        return (
                                            <div
                                                key={item.foodItemId}
                                                className={`group rounded-2xl overflow-hidden border-2 shadow-md hover:shadow-2xl transition-all duration-300 hover:-translate-y-1 ${
                                                    selected ? 'border-primary ring-4 ring-primary/30 bg-orange-50' : 'border-neutral-200 bg-white'
                                                }`}
                                            >
                                                {/* ✅ Media Section (Image or Video) */}
                                                <div
                                                    className="relative h-48 overflow-hidden bg-gradient-to-br from-gray-100 to-gray-200 cursor-pointer"
                                                    onClick={() => {
                                                        if (displayMedia) {
                                                            setFoodMediaViewer({
                                                                isOpen: true,
                                                                mediaUrl: displayMedia.startsWith('http') ? displayMedia : `${API_BASE_URL}${displayMedia}`,
                                                                mediaType: mediaType,
                                                                foodName: item.name,
                                                                allMedia: hasImages ? item.imageUrls : []
                                                            });
                                                        }
                                                    }}
                                                >
                                                    {hasVideo ? (
                                                        <div className="relative w-full h-full">
                                                            <video
                                                                src={item.videoUrl.startsWith('http') ? item.videoUrl : `${API_BASE_URL}${item.videoUrl}`}
                                                                className="w-full h-full object-cover"
                                                                muted
                                                                loop
                                                                playsInline
                                                            />
                                                            {/* Play Icon Overlay */}
                                                            <div className="absolute inset-0 flex items-center justify-center bg-black bg-opacity-30 group-hover:bg-opacity-40 transition-all">
                                                                <div className="w-16 h-16 bg-white bg-opacity-90 rounded-full flex items-center justify-center shadow-xl">
                                                                    <svg className="w-8 h-8 text-primary ml-1" fill="currentColor" viewBox="0 0 20 20">
                                                                        <path d="M6.3 2.841A1.5 1.5 0 004 4.11V15.89a1.5 1.5 0 002.3 1.269l9.344-5.89a1.5 1.5 0 000-2.538L6.3 2.84z" />
                                                                    </svg>
                                                                </div>
                                                            </div>
                                                        </div>
                                                    ) : hasImages ? (
                                                        <img
                                                            src={item.imageUrls[0].startsWith('http') ? item.imageUrls[0] : `${API_BASE_URL}${item.imageUrls[0]}`}
                                                            alt={item.name}
                                                            className="w-full h-full object-cover group-hover:scale-110 transition-transform duration-500"
                                                            onError={(e) => {
                                                                // SECURITY FIX: Use createElement instead of innerHTML to prevent XSS
                                                                e.target.style.display = 'none';
                                                                const parent = e.target.parentElement;
                                                                const fallbackDiv = document.createElement('div');
                                                                fallbackDiv.className = 'w-full h-full flex items-center justify-center text-6xl';
                                                                fallbackDiv.textContent = item.isVegetarian ? '🥗' : '🍖';
                                                                parent.innerHTML = ''; // Clear parent
                                                                parent.appendChild(fallbackDiv);
                                                            }}
                                                        />
                                                    ) : (
                                                        <VegNonVegIcon isVeg={item.isVegetarian} placeholder />
                                                    )}

                                                    {/* Veg/Non-Veg Icon overlay — only when media exists */}
                                                    {(hasVideo || hasImages) && (
                                                        <div className="absolute top-3 left-3 bg-white rounded-md p-1.5 shadow-lg">
                                                            <VegNonVegIcon isVeg={item.isVegetarian} size="lg" />
                                                        </div>
                                                    )}

                                                    {/* Media Count Badge */}
                                                    {hasImages && item.imageUrls.length > 1 && (
                                                        <div className="absolute top-3 right-3 bg-black bg-opacity-70 text-white text-xs font-bold px-2 py-1 rounded-full flex items-center gap-1">
                                                            <svg className="w-3 h-3" fill="currentColor" viewBox="0 0 20 20">
                                                                <path fillRule="evenodd" d="M4 3a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V5a2 2 0 00-2-2H4zm12 12H4l4-8 3 6 2-4 3 6z" clipRule="evenodd" />
                                                            </svg>
                                                            {item.imageUrls.length}
                                                        </div>
                                                    )}

                                                    {/* Selected Checkmark */}
                                                    {selected && (
                                                        <div className="absolute bottom-3 right-3 bg-green-500 text-white rounded-full p-1.5 shadow-lg">
                                                            <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                                                                <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
                                                            </svg>
                                                        </div>
                                                    )}
                                                </div>

                                                {/* Item Details */}
                                                <div className="p-4 bg-white">
                                                    <h4 className="font-bold text-base text-neutral-900 mb-2 line-clamp-1">
                                                        {item.name}
                                                    </h4>
                                                    <p className="text-xs text-neutral-600 line-clamp-2 mb-3">
                                                        {item.description}
                                                    </p>
                                                    <div className="flex items-center justify-between">
                                                        <p className="text-lg font-bold text-neutral-900">₹{item.price}</p>
                                                        <button
                                                            onClick={() => toggleIndividualItem(item)}
                                                            className={`px-4 py-2 rounded-lg text-sm font-bold transition-all shadow-md ${
                                                                selected
                                                                    ? 'bg-red-500 text-white hover:bg-red-600'
                                                                    : 'bg-gradient-to-r from-primary to-primary-dark text-white hover:shadow-lg'
                                                            }`}
                                                        >
                                                            {selected ? '✓ Added' : '+ Add'}
                                                        </button>
                                                    </div>
                                                </div>
                                            </div>
                                        );
                                    })}
                                </div>
                            </div>
                        ))
                    )}
                </section>

                {/* ✅ 3. DECORATIONS SECTION (Images + Videos) */}
                <section id="decorations" className="scroll-mt-32">
                    <div className="mb-6">
                        <h2 className="text-2xl md:text-3xl font-bold text-neutral-900">Decoration Themes</h2>
                        <p className="text-sm text-neutral-600 mt-1">Transform your event with stunning decorations</p>
                    </div>

                    {standaloneDecorations.length === 0 ? (
                        <div className="text-center py-16 text-neutral-500 bg-neutral-50 rounded-2xl">
                            <div className="text-6xl mb-4">🎨</div>
                            <p className="text-lg">No standalone decorations available</p>
                        </div>
                    ) : (
                        <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
                            {standaloneDecorations.map(decor => {
                                const isSelected = isStandaloneDecorationSelected(decor.decorationId);

                                return (
                                <div
                                    key={decor.decorationId}
                                    className={`group rounded-2xl overflow-hidden border-2 shadow-lg bg-white transition-all duration-300 hover:-translate-y-1 ${
                                        isSelected
                                            ? 'border-orange-500 ring-4 ring-orange-100'
                                            : 'border-neutral-200 hover:shadow-2xl'
                                    }`}
                                >
                                    {/* Decoration Image/Video */}
                                    <div className="h-56 bg-gradient-to-br from-purple-100 to-pink-100 relative overflow-hidden">
                                        {decor.videoUrl ? (
                                            <video
                                                src={decor.videoUrl.startsWith('http') ? decor.videoUrl : `${API_BASE_URL}${decor.videoUrl}`}
                                                className="w-full h-full object-cover group-hover:scale-110 transition-transform duration-500"
                                                controls
                                                muted
                                            />
                                        ) : decor.thumbnailUrl ? (
                                            <img
                                                src={decor.thumbnailUrl.startsWith('http') ? decor.thumbnailUrl : `${API_BASE_URL}${decor.thumbnailUrl}`}
                                                alt={decor.name}
                                                className="w-full h-full object-cover group-hover:scale-110 transition-transform duration-500"
                                            />
                                        ) : (
                                            <div className="w-full h-full flex items-center justify-center text-7xl">
                                                🎨
                                            </div>
                                        )}
                                    </div>

                                    {/* Decoration Details */}
                                    <div className="p-5">
                                        <h4 className="font-bold text-lg text-neutral-900 mb-1">{decor.name}</h4>
                                        <p className="text-sm text-neutral-600 mb-3">{decor.themeName}</p>
                                        <div className="flex items-center justify-between">
                                            <span className="text-xl font-bold text-primary">₹{decor.price}</span>
                                            <div className="flex items-center gap-2">
                                                {decor.isAvailable && (
                                                    <span className="text-xs bg-green-100 text-green-700 px-3 py-1 rounded-full font-semibold">Available</span>
                                                )}
                                                <button
                                                    type="button"
                                                    onClick={() => toggleStandaloneDecoration(decor)}
                                                    className={`px-4 py-2 rounded-lg text-sm font-bold transition-all shadow-md ${
                                                        isSelected
                                                            ? 'bg-red-500 text-white hover:bg-red-600'
                                                            : 'bg-gradient-to-r from-primary to-primary-dark text-white hover:shadow-lg'
                                                    }`}
                                                >
                                                    {isSelected ? 'âœ“ Added' : '+ Add'}
                                                </button>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            )})}
                        </div>
                    )}
                </section>

                {/* ✅ 4. REVIEWS SECTION */}
                <section id="reviews" className="scroll-mt-32">
                    <div className="mb-6">
                        <h2 className="text-2xl md:text-3xl font-bold text-neutral-900">Customer Reviews</h2>
                        <p className="text-sm text-neutral-600 mt-1">What our happy customers say</p>
                    </div>

                    {reviews.length === 0 ? (
                        <div className="text-center py-16 text-neutral-500 bg-neutral-50 rounded-2xl">
                            <div className="text-6xl mb-4">⭐</div>
                            <p className="text-lg">No reviews yet. Be the first to review!</p>
                        </div>
                    ) : (
                        <div className="space-y-5">
                            {reviews.map(review => (
                                <div
                                    key={review.reviewId}
                                    className="bg-white border-2 border-neutral-200 rounded-2xl p-6 shadow-md hover:shadow-lg transition-shadow"
                                >
                                    <div className="flex items-start gap-5">
                                        <div className="w-14 h-14 rounded-full bg-gradient-to-br from-primary to-primary-dark text-white flex items-center justify-center font-bold text-xl flex-shrink-0 shadow-md">
                                            {review.userName ? review.userName.charAt(0).toUpperCase() : 'U'}
                                        </div>
                                        <div className="flex-1">
                                            <div className="flex items-center justify-between mb-2">
                                                <div>
                                                    <h4 className="font-bold text-lg text-neutral-900">{review.userName || 'Anonymous'}</h4>
                                                    <p className="text-xs text-neutral-500">
                                                        {new Date(review.reviewDate).toLocaleDateString('en-US', {
                                                            year: 'numeric',
                                                            month: 'long',
                                                            day: 'numeric'
                                                        })}
                                                    </p>
                                                </div>
                                                <div className="bg-green-100 text-green-800 px-3 py-1 rounded-lg text-base font-bold">
                                                    {review.rating} ★
                                                </div>
                                            </div>
                                            {review.title && (
                                                <h5 className="font-semibold text-neutral-800 mb-2">{review.title}</h5>
                                            )}
                                            <p className="text-sm text-neutral-700 leading-relaxed">
                                                {review.reviewText}
                                            </p>
                                            {review.wouldRecommend && (
                                                <div className="mt-3 flex items-center gap-2 text-sm text-green-700 font-medium">
                                                    <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                                                        <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                                                    </svg>
                                                    Would recommend to others
                                                </div>
                                            )}
                                        </div>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </section>

                {/* ✅ 5. KITCHEN & HYGIENE SECTION (Images + Videos) */}
                <section id="kitchen" className="scroll-mt-32">
                    <div className="mb-6">
                        <h2 className="text-2xl md:text-3xl font-bold text-neutral-900">Kitchen & Hygiene</h2>
                        <p className="text-sm text-neutral-600 mt-1">See our hygienic kitchen and food preparation process</p>
                    </div>

                    {(!cateringDetail.kitchenPhotos || cateringDetail.kitchenPhotos.length === 0) &&
                     (!cateringDetail.kitchenVideos || cateringDetail.kitchenVideos.length === 0) ? (
                        <div className="text-center py-16 text-neutral-500 bg-neutral-50 rounded-2xl">
                            <div className="text-6xl mb-4">👨‍🍳</div>
                            <p className="text-lg">No kitchen media available</p>
                        </div>
                    ) : (
                        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
                            {/* Photos */}
                            {cateringDetail.kitchenPhotos?.map(media => (
                                <div
                                    key={media.mediaId}
                                    className="aspect-square rounded-xl overflow-hidden bg-neutral-100 shadow-md hover:shadow-xl transition-shadow border-2 border-neutral-200 cursor-pointer group relative"
                                    onClick={() => {
                                        const clickedIndex = kitchenMediaItems.findIndex(item => item.filePath === media.mediaUrl);
                                        setKitchenMediaViewer({
                                            isOpen: true,
                                            currentIndex: clickedIndex >= 0 ? clickedIndex : 0
                                        });
                                    }}
                                >
                                    <img
                                        src={`${API_BASE_URL}${media.mediaUrl}`}
                                        alt={media.caption || 'Kitchen'}
                                        className="w-full h-full object-cover"
                                    />
                                    <div className="absolute inset-0 bg-black/0 group-hover:bg-black/30 transition-colors flex items-center justify-center">
                                        <span className="opacity-0 group-hover:opacity-100 px-3 py-1.5 rounded-full bg-white/90 text-neutral-900 text-sm font-semibold transition-opacity">
                                            View Image
                                        </span>
                                    </div>
                                </div>
                            ))}
                            {/* Videos */}
                            {cateringDetail.kitchenVideos?.map(media => (
                                <div
                                    key={media.mediaId}
                                    className="aspect-square rounded-xl overflow-hidden bg-neutral-100 shadow-md hover:shadow-xl transition-shadow border-2 border-neutral-200 cursor-pointer group relative"
                                    onClick={() => {
                                        const clickedIndex = kitchenMediaItems.findIndex(item => item.filePath === media.mediaUrl);
                                        setKitchenMediaViewer({
                                            isOpen: true,
                                            currentIndex: clickedIndex >= 0 ? clickedIndex : 0
                                        });
                                    }}
                                >
                                    <video
                                        src={`${API_BASE_URL}${media.mediaUrl}`}
                                        className="w-full h-full object-cover"
                                        muted
                                        playsInline
                                    />
                                    <div className="absolute inset-0 bg-black/20 group-hover:bg-black/35 transition-colors flex items-center justify-center">
                                        <div className="w-14 h-14 rounded-full bg-white/90 flex items-center justify-center shadow-lg">
                                            <svg className="w-6 h-6 text-neutral-900 ml-1" fill="currentColor" viewBox="0 0 24 24">
                                                <path d="M8 5v14l11-7z" />
                                            </svg>
                                        </div>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </section>

            </div>

            {/* Fixed Bottom Bar - Add to Cart */}
            <div className="fixed bottom-0 left-0 right-0 bg-white border-t-2 border-neutral-200 shadow-2xl z-20">
                <div className="max-w-7xl mx-auto px-4 py-5">
                    <div className="flex items-center justify-between gap-6">
                        <div className="flex-1">
                            {selectedPackage ? (
                                <div>
                                    <div className="text-base font-bold text-neutral-900">
                                        {selectedPackage.name}
                                    </div>
                                    <div className="text-sm text-neutral-600 mt-1">
                                        ₹{selectedPackage.pricePerPerson}/person
                                        {selectedIndividualItems.length > 0 && (
                                            <span className="ml-2 text-primary font-bold">
                                                + {selectedIndividualItems.length} add-ons
                                            </span>
                                        )}
                                        {selectedStandaloneDecorations.length > 0 && (
                                            <span className="ml-2 text-primary font-bold">
                                                + {selectedStandaloneDecorations.length} decorations
                                            </span>
                                        )}
                                    </div>
                                </div>
                            ) : selectedIndividualItems.length > 0 || selectedStandaloneDecorations.length > 0 ? (
                                <div>
                                    <div className="text-base font-bold text-neutral-900">
                                        Custom Order
                                    </div>
                                    <div className="text-sm text-neutral-600 mt-1">
                                        {selectedIndividualItems.length} items and {selectedStandaloneDecorations.length} decorations selected
                                    </div>
                                </div>
                            ) : (
                                <div className="text-sm text-neutral-500">
                                    Select a package or add items and decorations to cart
                                </div>
                            )}
                        </div>
                        <button
                            onClick={handleAddToCart}
                            disabled={!selectedPackage && selectedIndividualItems.length === 0 && selectedStandaloneDecorations.length === 0}
                            className={`px-10 py-4 rounded-xl font-bold text-white text-base transition-all shadow-lg ${
                                selectedPackage || selectedIndividualItems.length > 0 || selectedStandaloneDecorations.length > 0
                                    ? 'bg-gradient-to-r from-primary to-primary-dark hover:shadow-2xl hover:scale-105'
                                    : 'bg-neutral-300 cursor-not-allowed'
                            }`}
                        >
                            {selectedPackage || selectedIndividualItems.length > 0 || selectedStandaloneDecorations.length > 0 ? 'Add to Cart' : 'Select Items'}
                        </button>
                    </div>
                </div>
            </div>

            {/* Modals */}
            {/* ✅ MANDATORY EVENT SETUP MODAL - Shows before Add to Cart */}
            <AvailabilityCalendarModal
                isOpen={isAvailabilityModalOpen}
                onClose={() => setIsAvailabilityModalOpen(false)}
                minDate={minSelectableDate}
                selectedDate={selectedAvailabilityDate}
                blockedDates={blockedCalendarDates}
                onSelectDate={handleAvailabilityDateSelect}
                onMonthChange={loadAvailabilityCalendarMonth}
                isChecking={isCheckingAvailability}
                availabilityResult={availabilityResult}
                onProceedToBooking={handleProceedToBookingFromAvailability}
            />

            <EventSetupModal
                isOpen={isEventSetupModalOpen}
                onClose={() => setIsEventSetupModalOpen(false)}
                onComplete={() => {
                    setIsEventSetupModalOpen(false);
                    showToast('Event setup completed! You can now add items to cart.', 'success');
                }}
                cateringId={Number(id)}
            />

            {packageForSelection && (
                <PackageSelectionModal
                    isOpen={isPackageModalOpen}
                    onClose={() => setIsPackageModalOpen(false)}
                    cateringId={Number(id)}
                    packageId={packageForSelection.packageId}
                    packageName={packageForSelection.name}
                    sampleItems={sampleItems}
                    onSelectionComplete={handlePackageSelectionComplete}
                />
            )}

            <SampleTasteModal
                isOpen={isSampleModalOpen}
                onClose={() => setIsSampleModalOpen(false)}
                foodItems={sampleItems}
                packageData={selectedPackage}
                onConfirm={handleSampleSelectionConfirm}
            />

            {/* Sample Taste Modal for Individual Items */}
            {isIndividualSampleModalOpen && (
                <SampleTasteModal
                    isOpen={isIndividualSampleModalOpen}
                    onClose={handleIndividualSampleTasteClose}
                    foodItems={individualSampleItems}
                    onConfirm={handleIndividualSampleTasteComplete}
                />
            )}

            {/* Food Item Media Viewer */}
            {foodMediaViewer.isOpen && (
                <div
                    className="fixed inset-0 bg-black bg-opacity-95 z-[60] flex items-center justify-center p-4"
                    onClick={() => setFoodMediaViewer({ ...foodMediaViewer, isOpen: false })}
                >
                    <div className="relative w-full max-w-5xl max-h-[90vh] flex flex-col">
                        {/* Close Button */}
                        <button
                            onClick={() => setFoodMediaViewer({ ...foodMediaViewer, isOpen: false })}
                            className="absolute -top-12 right-0 text-white hover:text-gray-300 transition-colors z-10"
                        >
                            <svg className="w-10 h-10" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                            </svg>
                        </button>

                        {/* Food Name */}
                        <div className="text-white text-xl font-bold mb-4 text-center">
                            {foodMediaViewer.foodName}
                        </div>

                        {/* Media Display */}
                        <div
                            className="bg-black rounded-lg overflow-hidden flex items-center justify-center"
                            onClick={(e) => e.stopPropagation()}
                        >
                            {foodMediaViewer.mediaType === 'video' ? (
                                <video
                                    src={foodMediaViewer.mediaUrl}
                                    controls
                                    autoPlay
                                    className="w-full h-auto max-h-[70vh] object-contain"
                                />
                            ) : (
                                <img
                                    src={foodMediaViewer.mediaUrl}
                                    alt={foodMediaViewer.foodName}
                                    className="w-full h-auto max-h-[70vh] object-contain"
                                />
                            )}
                        </div>

                        {/* Image Counter (if multiple images) */}
                        {foodMediaViewer.allMedia && foodMediaViewer.allMedia.length > 1 && (
                            <div className="mt-4 text-center text-white text-sm">
                                <p className="mb-2">1 of {foodMediaViewer.allMedia.length} images</p>
                                <div className="flex gap-2 justify-center flex-wrap max-w-2xl mx-auto">
                                    {foodMediaViewer.allMedia.map((mediaUrl, index) => (
                                        <img
                                            key={index}
                                            src={mediaUrl.startsWith('http') ? mediaUrl : `${API_BASE_URL}${mediaUrl}`}
                                            alt={`${foodMediaViewer.foodName} ${index + 1}`}
                                            className="w-16 h-16 object-cover rounded-lg cursor-pointer border-2 border-white hover:scale-110 transition-transform"
                                            onClick={() => setFoodMediaViewer({
                                                ...foodMediaViewer,
                                                mediaUrl: mediaUrl.startsWith('http') ? mediaUrl : `${API_BASE_URL}${mediaUrl}`
                                            })}
                                        />
                                    ))}
                                </div>
                            </div>
                        )}
                    </div>
                </div>
            )}

            {/* Cart Replacement Confirmation Modal */}
            {cartReplaceConfirmation.isOpen && (
                <div className="fixed inset-0 bg-black bg-opacity-50 z-[70] flex items-center justify-center p-4">
                    <Motion.div
                        initial={{ opacity: 0, scale: 0.95 }}
                        animate={{ opacity: 1, scale: 1 }}
                        className="bg-white rounded-xl shadow-2xl max-w-md w-full p-6"
                        onClick={(e) => e.stopPropagation()}
                    >
                        <div className="text-center mb-6">
                            <div className="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-yellow-100 mb-4">
                                <svg className="h-6 w-6 text-yellow-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                                </svg>
                            </div>
                            <h3 className="text-lg font-semibold text-neutral-900 mb-2">Replace Cart?</h3>
                            <p className="text-sm text-neutral-600">{cartReplaceConfirmation.message}</p>
                        </div>

                        <div className="flex gap-3">
                            <button
                                onClick={handleCartReplaceCancel}
                                className="flex-1 px-4 py-2.5 bg-neutral-100 text-neutral-700 rounded-lg hover:bg-gray-200 transition-colors font-medium"
                            >
                                Cancel
                            </button>
                            <button
                                onClick={handleCartReplaceConfirm}
                                className="flex-1 px-4 py-2.5 bg-primary text-white rounded-lg hover:bg-primary transition-colors font-medium"
                            >
                                Replace Cart
                            </button>
                        </div>
                    </Motion.div>
                </div>
            )}
        {/* Auth Modal — triggered on Add to Cart when not signed in */}
        <AuthModal
            isOpen={showAuthModal}
            onClose={handleAuthClose}
            onSuccess={handleAuthSuccessCart}
        />

        {kitchenMediaViewer.isOpen && kitchenMediaItems.length > 0 && (
            <MediaViewer
                mediaItems={kitchenMediaItems}
                currentIndex={kitchenMediaViewer.currentIndex}
                onClose={() => setKitchenMediaViewer({ isOpen: false, currentIndex: 0 })}
                onNavigate={(index) => setKitchenMediaViewer({ isOpen: true, currentIndex: index })}
            />
        )}

        </Motion.div>
    );
}
