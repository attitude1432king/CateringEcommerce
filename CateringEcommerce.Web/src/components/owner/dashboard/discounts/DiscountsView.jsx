/*
========================================
File: src/components/owner/dashboard/discounts/DiscountsView.jsx (REVISED)
========================================
Main view for managing discounts with comprehensive filtering.
Updated to use Enum integers for Type and Mode.
*/
import React, { useState, useEffect } from 'react';
import Loader from '../../../common/Loader';
import { useToast } from '../../../../contexts/ToastContext';
import CreateDiscountForm from './CreateDiscountForm';
import CustomSelectDropdown from '../../../common/CustomSelectDropdown';
import ToggleSwitch from '../../../common/ToggleSwitch';
import Pagination from '../../../common/Pagination';
import { useConfirmation } from '../../../../contexts/ConfirmationContext'; // Import confirmation hook
import { ownerApiService } from '../../../../services/ownerApi';
import { discountTypeOptions, DISCOUNT_TYPE_LABELS } from '../../../../utils/staticData';


export default function DiscountsView() {
    const [discounts, setDiscounts] = useState([]);
    const [isLoading, setIsLoading] = useState(true);
    const [foodItems, setFoodItems] = useState([]);
    const [packages, setPackages] = useState([]);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingDiscount, setEditingDiscount] = useState(null); // State for editing
    const { showToast } = useToast();
    const confirm = useConfirmation(); // Hook for delete confirmation
    const [totalCount, setTotalCount] = useState(0);

    // Filter State (as requested)
    const [filters, setFilters] = useState({
        name: "",
        type: 0, // Default to 0 for "All"
        status: "", // "" for all, "active" for active only
    });

    // 👉 Build Filter Object as JSON
    const filterJson = JSON.stringify(filters);
    // Pagination States
    const [currentPage, setCurrentPage] = useState(1);
    const [itemsPerPage, setItemsPerPage] = useState(10);

    // Debounce Search Term (derived from filters.name)
    const [debouncedSearchName, setDebouncedSearchName] = useState('');

    // Debounce Search
    useEffect(() => {
        const handler = setTimeout(() => {
            setDebouncedSearchName(filters.name);
        }, 500); // 300ms debounce
        return () => clearTimeout(handler);
    }, [filters.name]);

    useEffect(() => {
        fetchData();
    }, [currentPage, itemsPerPage, filterJson]);

    const fetchData = async () => {
        setIsLoading(true);

        try {
            // STEP 1: Get total count first
            const totalRecords = await ownerApiService.getDiscountCount(filterJson);

            // STEP 2: Update count
            setTotalCount(totalRecords);

            // STEP 3: If no records, skip list API
            if (totalRecords === 0) {
                setDiscounts([]);
                return;
            }

            // STEP 4: Fetch data only if count > 0
            const items = await ownerApiService.getDiscountList(
                currentPage,
                itemsPerPage,
                filterJson
            );

            setDiscounts(items);

        } catch (error) {
            console.error('Error fetching discount data:', error);
            showToast('Failed to load discount data.', 'error');
        } finally {
            setIsLoading(false);
        }
    };


    // Fetch Themes + Packages only ONCE
    const fetchCategories = async () => {
        try {
            const [foodItemData, packageData] = await Promise.all([
                ownerApiService.getFoodItemsLookup(),
                ownerApiService.getPackagesLookup()
            ]);
            setFoodItems(foodItemData);
            setPackages(packageData);

        } catch (error) {
            showToast('Failed to load lookup data.' + error, 'error');
        }
    };

    // First Load
    useEffect(() => { fetchCategories(); }, []);

    const handleOpenCreate = () => {
        setEditingDiscount(null);
        setIsModalOpen(true);
    };

    const handleOpenEdit = (discount) => {
        setEditingDiscount(discount);
        setIsModalOpen(true);
    };

    const handleSaveDiscount = async (discountData) => {
        setIsLoading(true);
        try {

            let response = null;
            if (editingDiscount) {
                discountData.id = editingDiscount.id;
                response = await ownerApiService.updateDiscount(discountData);
            } else {
                response = await ownerApiService.createDiscount(discountData);
            }

            if (response && response.message && response.result) {
                showToast(response.message, 'success');
                fetchData(); // Refresh the grid
            }
            else {
                showToast(response.message, response.type ?? 'error');
            }

            setIsModalOpen(false);
        } catch (error) {
            showToast(`Error: ${error.message || 'Could not save discount.'}`, 'error');
        }
        finally {
            setIsLoading(false);
        }
    };

    const handleDeleteDiscount = async (discount) => {
        // The confirmation is now handled inside the form for individual delete, 
        // but can also be triggered from the list view action buttons here.
        confirm({
            type: 'delete',
            title: 'Delete Discount',
            message: 'Are you sure you want to delete this discount? This action cannot be undone.',
            confirmText: 'Delete',
            cancelText: 'Cancel'
        }).then(async (confirmed) => {
            if (confirmed) {
                const response = await ownerApiService.deleteDiscount(discount.id);
                if (response.result)
                    showToast(`${discount.name} deleted successfully.`, 'success');
                else {
                    showToast("Failed to delete discount", 'error');
                }
                fetchData();
            }
        });
    };

    // --- HELPER: Get Status String ---
    const getDiscountStatus = (discount) => {
        const now = new Date();
        const endDate = new Date(discount.endDate);

        if (!discount.isActive) return 'Disabled';
        if (endDate < now) return 'Expired';
        return 'Active';
    };

    // Reset page when filters change
    useEffect(() => {
        setCurrentPage(1);
    }, [debouncedSearchName, filters.type, filters.status]);

    // Handler helpers for UI controls
    const handleNameSearchChange = (e) => setFilters(prev => ({ ...prev, name: e.target.value }));
    const handleTypeChange = (e) => {
        setFilters({
            ...filters,
            type: Number(e.target.value) // convert to number
        });
    };

    //const handleTypeChange = (val) => setFilters(prev => ({ ...prev, type: val }));
    const handleStatusToggle = (isActive) => setFilters(prev => ({ ...prev, status: isActive ? true : "" }));


    if (isLoading) return <div className="flex justify-center items-center h-64"><Loader /></div>;

    return (
        <div className="space-y-6">
            {/* Header Section */}
            <div className="bg-white rounded-2xl shadow-sm border border-neutral-100 p-6">
                <div className="flex flex-col lg:flex-row justify-between items-start lg:items-center gap-4">
                    <div className="flex-1">
                        <h2 className="text-3xl font-bold text-neutral-900">Discounts & Offers</h2>
                    </div>

                    {/* Modern Add Button */}
                    <button
                        onClick={handleOpenCreate}
                        className="group flex items-center gap-2 bg-gradient-to-r from-indigo-600 to-purple-600 hover:from-indigo-700 hover:to-purple-700 text-white px-6 py-3 rounded-xl font-semibold shadow-lg hover:shadow-xl transition-all duration-300 transform hover:scale-105"
                    >
                        <svg className="w-5 h-5 transition-transform group-hover:rotate-90 duration-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                        </svg>
                        Create Discount
                    </button>
                </div>
            </div>

            {/* Filter Section */}
            <div className="bg-white rounded-2xl shadow-sm border border-neutral-100 p-6">
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                    {/* Search with Icon */}
                    <div className="relative">
                        <svg className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-neutral-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                        </svg>
                        <input
                            type="text"
                            placeholder="Search discounts..."
                            value={filters.name}
                            onChange={handleNameSearchChange}
                            className="w-full pl-10 pr-4 py-3 border border-neutral-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition-all"
                        />
                    </div>

                    {/* Type Filter */}
                    <select
                        value={filters.type}
                        onChange={handleTypeChange}
                        className="w-full px-4 py-3 border border-neutral-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition-all"
                    >
                        {discountTypeOptions.map((option) => (
                            <option key={option.id} value={option.id}>
                                {option.name}
                            </option>
                        ))}
                    </select>

                    {/* Status Toggle */}
                    <div className="flex items-center gap-2 px-3">
                        <ToggleSwitch
                            label="Show Active Only"
                            enabled={filters.status}
                            setEnabled={handleStatusToggle}
                        />
                    </div>
                </div>
            </div>

            {/* Modern Discount Cards Grid */}
            {totalCount > 0 ? (
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6 animate-fade-in">
                    {discounts.map((discount, index) => {
                        const statusLabel = getDiscountStatus(discount);
                        let statusColorClass = 'bg-neutral-100 text-neutral-600 border-neutral-200';
                        if (statusLabel === 'Active') statusColorClass = 'bg-green-100 text-green-800 border-green-200';
                        if (statusLabel === 'Expired') statusColorClass = 'bg-red-100 text-red-800 border-red-200';

                        return (
                            <div
                                key={discount.id}
                                className="group bg-white rounded-2xl shadow-sm border border-neutral-100 overflow-hidden transition-all duration-300 hover:shadow-xl hover:border-indigo-200"
                                style={{ animationDelay: `${index * 50}ms` }}
                            >
                                <div className="p-6">
                                    {/* Header with Status */}
                                    <div className="flex items-start justify-between mb-3">
                                        <h3 className="text-lg font-bold text-neutral-900 group-hover:text-indigo-600 transition-colors line-clamp-1">
                                            {discount.name}
                                        </h3>
                                        <span className={`px-2.5 py-1 rounded-full text-xs font-bold border ${statusColorClass}`}>
                                            {statusLabel}
                                        </span>
                                    </div>

                                    {/* Discount Code */}
                                    <div className="mb-4">
                                        <span className="inline-block px-3 py-1.5 bg-indigo-50 text-indigo-700 rounded-lg text-sm font-mono font-bold">
                                            {discount.code}
                                        </span>
                                    </div>

                                    {/* Type Badge */}
                                    <div className="mb-3">
                                        <span className="text-xs text-neutral-500 font-medium">
                                            {DISCOUNT_TYPE_LABELS[discount.type]}
                                        </span>
                                    </div>

                                    {/* Value Display */}
                                    <div className="flex items-center justify-between p-4 bg-gradient-to-r from-indigo-50 to-purple-50 rounded-xl mb-4">
                                        <span className="text-xs text-neutral-600 font-medium">Discount Value</span>
                                        <span className="text-2xl font-bold bg-gradient-to-r from-indigo-600 to-purple-600 bg-clip-text text-transparent">
                                            {discount.mode === 1 ? `${discount.value}%` : `₹${discount.value}`}
                                        </span>
                                    </div>

                                    {/* Validity */}
                                    <div className="space-y-2 text-xs text-neutral-600 mb-4">
                                        <div className="flex items-center gap-2">
                                            <svg className="w-4 h-4 text-indigo-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                                            </svg>
                                            <span className="font-medium">Valid from: {discount.startDate}</span>
                                        </div>
                                        <div className="flex items-center gap-2">
                                            <svg className="w-4 h-4 text-indigo-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                                            </svg>
                                            <span className="font-medium">Valid until: {discount.endDate}</span>
                                        </div>
                                    </div>
                                </div>

                                {/* Action Buttons */}
                                <div className="bg-gradient-to-r from-neutral-50 to-indigo-50 px-6 py-4 flex gap-3 border-t border-neutral-100">
                                    <button
                                        onClick={() => handleOpenEdit(discount)}
                                        className="flex-1 flex items-center justify-center gap-1 px-4 py-2 bg-white hover:bg-indigo-50 text-indigo-600 rounded-lg font-semibold transition-all shadow-sm hover:shadow"
                                    >
                                        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                                        </svg>
                                        Edit
                                    </button>
                                    <button
                                        onClick={() => handleDeleteDiscount(discount)}
                                        className="flex-1 flex items-center justify-center gap-1 px-4 py-2 bg-white hover:bg-red-50 text-red-600 rounded-lg font-semibold transition-all shadow-sm hover:shadow"
                                    >
                                        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                                        </svg>
                                        Delete
                                    </button>
                                </div>
                            </div>
                        );
                    })}
                </div>
            ) : (
                <div className="bg-white rounded-2xl shadow-sm border border-neutral-200 p-12">
                    <div className="text-center">
                        <svg className="w-20 h-20 mx-auto mb-4 text-neutral-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z" />
                        </svg>
                        <h3 className="text-xl font-semibold text-neutral-900 mb-2">No Discounts Found</h3>
                        <p className="text-neutral-600 mb-4">Try adjusting your filters or create your first discount.</p>
                        <button
                            onClick={handleOpenCreate}
                            className="bg-gradient-to-r from-indigo-600 to-purple-600 hover:from-indigo-700 hover:to-purple-700 text-white px-6 py-2.5 rounded-xl font-semibold transition-all"
                        >
                            Create First Discount
                        </button>
                    </div>
                </div>
            )}

            {/* Pagination Controls */}
            <Pagination
                currentPage={currentPage}
                totalItems={totalCount}
                itemsPerPage={itemsPerPage}
                onPageChange={setCurrentPage}
                onItemsPerPageChange={setItemsPerPage}
            />

            {isModalOpen && (
                <CreateDiscountForm
                    isOpen={isModalOpen}
                    onClose={() => setIsModalOpen(false)}
                    onSave={handleSaveDiscount}
                    editingDiscount={editingDiscount} // Pass editing data
                    onDelete={handleDeleteDiscount} // Pass delete handler for modal
                    listFoodItems={foodItems} // Pass list of food items 
                    listPackages={packages} // Pass list of packages
                />
            )}
        </div>
    );
}