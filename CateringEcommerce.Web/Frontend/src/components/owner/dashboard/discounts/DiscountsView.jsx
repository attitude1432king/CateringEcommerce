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
import { discountTypeOptions, DISCOUNT_TYPE_LABELS } from '../../../../utils/staticDropDownData';


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
        <div className="animate-fade-in p-6 bg-white rounded-xl shadow-sm border border-neutral-200">

            {/* Header & Create Button */}
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center mb-6 gap-4">
                <div>
                    <h1 className="text-2xl font-bold text-neutral-800">Discounts & Offers</h1>
                    <p className="text-sm text-neutral-500 mt-1">Manage promotions to attract more customers.</p>
                </div>
                <button
                    onClick={handleOpenCreate}
                    className="bg-rose-600 text-white px-4 py-2 rounded-lg font-semibold hover:bg-rose-700 flex items-center gap-2 transition-colors shadow-sm"
                >
                    <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor"><path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm1-11a1 1 0 10-2 0v2H7a1 1 0 100 2h2v2a1 1 0 102 0v-2h2a1 1 0 100-2h-2V7z" clipRule="evenodd" /></svg>
                    Create Discount
                </button>
            </div>

            {/* --- FILTER SECTION --- */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6 pb-6 border-b border-neutral-100">

                {/* 1. Search Input */}
                <div className="relative">
                    <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                        <svg className="h-5 w-5 text-neutral-400" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
                            <path fillRule="evenodd" d="M8 4a4 4 0 100 8 4 4 0 000-8zM2 8a6 6 0 1110.89 3.476l4.817 4.817a1 1 0 01-1.414 1.414l-4.816-4.816A6 6 0 012 8z" clipRule="evenodd" />
                        </svg>
                    </div>
                    <input
                        type="text"
                        placeholder="Search discount name..."
                        value={filters.name}
                        onChange={handleNameSearchChange}
                        className="w-full pl-10 pr-4 py-2 border border-neutral-300 rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-rose-500 focus:border-rose-500 transition-shadow h-[42px]"
                    />
                </div>

                {/* 2. Type Filter */}
                {/*<div className="relative z-20">*/}
                {/*    <CustomSelectDropdown*/}
                {/*        options={discountTypeOptions}*/}
                {/*        value={filters.type}*/}
                {/*        onChange={handleTypeChange}*/}
                {/*    />*/}
                {/*</div>*/}

                {/* Status */}
                <select
                    value={filters.type}
                    onChange={handleTypeChange}
                    className="w-full px-3 py-2 border border-neutral-300 rounded-md shadow-sm"
                >
                    {discountTypeOptions.map((option) => (
                        <option key={option.id} value={option.id}>
                            {option.name}
                        </option>
                    ))}
                </select>

                {/* 3. Status Toggle */}
                <div className="flex items-center gap-2 px-3 h-[42px]">
                    <ToggleSwitch
                        label="Show Active Only"
                        enabled={filters.status}
                        setEnabled={handleStatusToggle}
                    />
                </div>
            </div>

            {/* Discount List Table */}
            <div className="overflow-hidden border border-neutral-200 rounded-lg">
                <div className="overflow-x-auto">
                    <table className="min-w-full divide-y divide-neutral-200">
                        <thead className="bg-neutral-50">
                            <tr>
                                <th className="px-6 py-3 text-left text-xs font-semibold text-neutral-500 uppercase tracking-wider">Name / Code</th>
                                <th className="px-6 py-3 text-left text-xs font-semibold text-neutral-500 uppercase tracking-wider">Type</th>
                                <th className="px-6 py-3 text-left text-xs font-semibold text-neutral-500 uppercase tracking-wider">Value</th>
                                <th className="px-6 py-3 text-left text-xs font-semibold text-neutral-500 uppercase tracking-wider">Validity</th>
                                <th className="px-6 py-3 text-left text-xs font-semibold text-neutral-500 uppercase tracking-wider">Status</th>
                                <th className="px-6 py-3 text-right text-xs font-semibold text-neutral-500 uppercase tracking-wider">Actions</th>
                            </tr>
                        </thead>
                        <tbody className="bg-white divide-y divide-neutral-200">
                            {totalCount > 0 ? (
                                discounts.map((discount) => {
                                    const statusLabel = getDiscountStatus(discount);
                                    let statusColorClass = 'bg-neutral-100 text-neutral-600'; // Default Disabled
                                    if (statusLabel === 'Active') statusColorClass = 'bg-green-100 text-green-800';
                                    if (statusLabel === 'Expired') statusColorClass = 'bg-red-100 text-red-800';

                                    return (
                                        <tr key={discount.id} className="hover:bg-neutral-50 transition-colors">
                                            <td className="px-6 py-4 whitespace-nowrap">
                                                <div className="text-sm font-medium text-neutral-900">{discount.name}</div>
                                                <div className="text-xs font-mono text-neutral-500 bg-neutral-100 px-1.5 py-0.5 rounded inline-block mt-1">{discount.code}</div>
                                            </td>
                                            <td className="px-6 py-4 whitespace-nowrap text-sm text-neutral-600">
                                                {DISCOUNT_TYPE_LABELS[discount.type]}
                                            </td>
                                            <td className="px-6 py-4 whitespace-nowrap">
                                                <span className="text-sm font-bold text-rose-600 bg-rose-50 px-2 py-1 rounded-md">
                                                    {/* Render Value based on Mode logic */}
                                                    {discount.mode === 1 ? `${discount.value}%` : `₹${discount.value}`}
                                                </span>
                                            </td>
                                            <td className="px-6 py-4 whitespace-nowrap text-xs text-neutral-500">
                                                <div className="flex flex-col">
                                                    <span>Start: {discount.startDate}</span>
                                                    <span>End: {discount.endDate}</span>
                                                </div>
                                            </td>
                                            <td className="px-6 py-4 whitespace-nowrap">
                                                <span className={`px-2.5 py-0.5 inline-flex text-xs font-bold rounded-full ${statusColorClass}`}>
                                                    {statusLabel}
                                                </span>
                                            </td>
                                            <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                                                <div className="flex justify-end gap-3">
                                                    <button
                                                        onClick={() => handleOpenEdit(discount)}
                                                        className="text-blue-600 hover:text-blue-900 transition-colors p-1 rounded-full hover:bg-blue-50"
                                                        title="Edit"
                                                    >
                                                        <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                                                            <path d="M13.586 3.586a2 2 0 112.828 2.828l-.793.793-2.828-2.828.793-.793zM11.379 5.793L3 14.172V17h2.828l8.38-8.379-2.83-2.828z" />
                                                        </svg>
                                                    </button>
                                                    <button
                                                        onClick={() => handleDeleteDiscount(discount)}
                                                        className="text-red-600 hover:text-red-900 transition-colors p-1 rounded-full hover:bg-red-50"
                                                        title="Delete"
                                                    >
                                                        <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                                                            <path fillRule="evenodd" d="M9 2a1 1 0 00-.894.553L7.382 4H4a1 1 0 000 2v10a2 2 0 002 2h8a2 2 0 002-2V6a1 1 0 100-2h-3.382l-.724-1.447A1 1 0 0011 2H9zM7 8a1 1 0 012 0v6a1 1 0 11-2 0V8zm5-1a1 1 0 00-1 1v6a1 1 0 102 0V8a1 1 0 00-1-1z" clipRule="evenodd" />
                                                        </svg>
                                                    </button>
                                                </div>
                                            </td>
                                        </tr>
                                    );
                                })
                            ) : (
                                <tr>
                                    <td colSpan="6" className="px-6 py-12 text-center text-neutral-500">
                                        No discounts found matching your filters.
                                    </td>
                                </tr>
                            )}
                        </tbody>
                    </table>
                </div>
            </div>

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