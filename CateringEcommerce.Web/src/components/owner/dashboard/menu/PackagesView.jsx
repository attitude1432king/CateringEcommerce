/*
========================================
File: src/components/owner/dashboard/menu/PackagesView.jsx (REVISED)
========================================
*/
import React, { useState, useEffect, useMemo } from 'react';
import Loader from '../../../common/Loader';
import { useToast } from '../../../../contexts/ToastContext';
import { useConfirmation } from '../../../../contexts/ConfirmationContext';
import { ownerApiService } from '../../../../services/ownerApi'; // For real API calls
import PackageFormModal from './PackageFormModal'; // The new modal component
import Pagination from '../../../common/Pagination'; // Import Pagination

// Modern Package Card Component with ENYVORA Design
const PackageCard = ({ pkg, onEdit, onDelete }) => (
    <div className="group bg-white rounded-2xl shadow-sm border border-neutral-100 overflow-hidden transition-all duration-300 hover:shadow-xl hover:border-orange-100">
        <div className="p-6">
            <div className="flex items-start justify-between mb-3">
                <h3 className="text-lg font-bold text-neutral-900">{pkg.name}</h3>
                <div className="flex items-center gap-1 px-2 py-1 rounded-lg" style={{ background: 'rgba(255,107,53,0.08)' }}>
                    <svg className="w-4 h-4" style={{ color: 'var(--color-primary)' }} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
                    </svg>
                    <span className="text-xs font-bold" style={{ color: 'var(--color-primary)' }}>{pkg.items.length}</span>
                </div>
            </div>
            <p className="text-sm text-neutral-600 leading-relaxed line-clamp-2 mb-4">{pkg.description}</p>
            <div className="flex items-center justify-between pt-4 border-t border-neutral-100">
                <div>
                    <p className="text-xs text-neutral-500 mb-1">Package Price</p>
                    <p className="text-2xl font-bold" style={{ color: 'var(--color-primary)' }}>₹{pkg.price.toLocaleString()}</p>
                </div>
            </div>
        </div>
        <div className="bg-neutral-50 px-6 py-4 flex justify-end gap-3 border-t border-neutral-100">
            <button
                onClick={() => onEdit(pkg)}
                className="flex items-center gap-1 px-4 py-2 bg-white hover:bg-neutral-100 text-neutral-700 rounded-lg font-semibold transition-all shadow-sm hover:shadow"
            >
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                </svg>
                Edit
            </button>
            <button
                onClick={() => onDelete(pkg)}
                className="flex items-center gap-1 px-4 py-2 bg-white hover:bg-red-50 text-red-600 rounded-lg font-semibold transition-all shadow-sm hover:shadow"
            >
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                </svg>
                Delete
            </button>
        </div>
    </div>
);

export default function PackagesView() {
    const [packages, setPackages] = useState([]);
    const [categories, setCategories] = useState([]);
    const [isLoading, setIsLoading] = useState(true);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingPackage, setEditingPackage] = useState(null);
    const { showToast } = useToast();
    const confirm = useConfirmation();

    // New state for filters and pagination
    const [searchQuery, setSearchQuery] = useState('');
    const [debouncedSearch, setDebouncedSearch] = useState("");
    const [currentPage, setCurrentPage] = useState(1);
    const [itemsPerPage, setItemsPerPage] = useState(10);
    const [totalCount, setTotalCount] = useState(0);

    const fetchData = async () => {
        setIsLoading(true);

        try {
            const totalRecords = await ownerApiService.getPackageCount(searchQuery);
            setTotalCount(totalRecords);

            if (totalRecords > 0) {
                const packagesData = await ownerApiService.getPackagesData(currentPage, itemsPerPage, searchQuery);
                setPackages(packagesData);
            }

        } catch (error) {
            showToast('Failed to load package data.', 'error');
        } finally {
            setIsLoading(false);
        }
    };


    const fetchCategories = async () => {
        try {
            const data = await ownerApiService.getFoodCategories();
            setCategories(data);
        } catch (error) {
            showToast('Failed to load categories.', 'error');
        }
    };

    useEffect(() => {
        const timer = setTimeout(() => {
            setDebouncedSearch(searchQuery);
        }, 1000); // delay 500ms after typing stops

        return () => clearTimeout(timer);
    }, [searchQuery]);

    // Load categories one time
    useEffect(() => {
        fetchCategories();
    }, []);

    // Load packages on pagination changes
    useEffect(() => {
        fetchData();
    }, [currentPage, itemsPerPage, debouncedSearch]);


    const handleOpenModal = (pkg = null) => {
        setEditingPackage(pkg);
        setIsModalOpen(true);
    };
    const handleCloseModal = () => {
        setIsModalOpen(false);
        setEditingPackage(null);
    };

    const handleSavePackage = async (packageData) => {
        try {
            let response = null;
            if (editingPackage) {
                packageData.packageId = editingPackage.packageId;
                response = await ownerApiService.updatePackage(packageData);
            } else {
                response = await ownerApiService.createPackage(packageData);
            }

            if (response && response.message && response.result) {
                showToast(response.message, 'success');
                fetchData(); // Refresh the grid
                handleCloseModal();
            }
            else {
                showToast(response.message, response.type ?? 'error');
            }

        } catch (error) {
            showToast('Fail package during process time.', 'error');
        }
    };

    const handleDeletePackage = async (pkg) => {
        confirm({
            type: 'delete',
            title: 'Delete Package',
            message: `Are you sure you want to delete the "${pkg.name}" package? This action cannot be undone.`,
            confirmText: 'Yes, Delete',
            cancelText: 'No, Cancel'
        }).then(async (isConfirmed) => {
            if (isConfirmed) {
                try {
                    const response = await ownerApiService.deletePackage(pkg.packageId);
                    if (response && response.message) {
                        showToast(`${pkg.name} deleted successfully.`, 'success');
                    }
                    else
                        showToast('Failed to delete package.', 'error');
                    fetchData(); // Refresh the grid
                } catch (error) {
                    showToast('Failed to delete package.', 'error');
                }
            }
        });
    };

    if (isLoading) { return <div className="flex justify-center items-center h-64"><Loader /></div>; }

    return (
        <div className="space-y-6">
            {/* Header */}
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
                <div>
                    <h1 className="text-3xl font-bold text-neutral-900">Packages</h1>
                </div>

                {/* Right Side Actions */}
                <div className="flex flex-wrap items-center gap-3">
                    {/* Search Bar */}
                    <div className="relative">
                        <svg className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-neutral-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                        </svg>
                        <input
                            type="text"
                            name="name"
                            placeholder="Search packages..."
                            value={searchQuery}
                            autoComplete="off"
                            onChange={(e) => {
                                setSearchQuery(e.target.value);
                                setCurrentPage(1);
                            }}
                            className="w-64 pl-10 pr-4 py-3 border border-neutral-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-orange-400 focus:border-transparent transition-all"
                        />
                    </div>

                    {/* Add Button */}
                    <button
                        onClick={() => handleOpenModal()}
                        className="flex items-center gap-2 text-white px-6 py-3 rounded-xl font-semibold shadow-lg hover:shadow-xl transition-all duration-200"
                        style={{ background: 'linear-gradient(135deg, #FF6B35 0%, #FF8C42 50%, #FFB627 100%)' }}
                    >
                        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                        </svg>
                        Add Package
                    </button>
                </div>
            </div>

            {/* Grid Section with Animations */}
            {totalCount > 0 ? (
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6 animate-fade-in">
                    {packages.map((pkg, index) => (
                        <div
                            key={pkg.packageId}
                            className="transform transition-all duration-300 hover:scale-105"
                            style={{ animationDelay: `${index * 50}ms` }}
                        >
                            <PackageCard pkg={pkg} onEdit={handleOpenModal} onDelete={handleDeletePackage} />
                        </div>
                    ))}
                </div>
            ) : (
                <div className="bg-white rounded-2xl shadow-sm border border-neutral-200 p-12">
                    <div className="text-center">
                        <svg className="w-20 h-20 mx-auto mb-4 text-neutral-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
                        </svg>
                        <h3 className="text-xl font-semibold text-neutral-900 mb-2">No Packages Found</h3>
                        <p className="text-neutral-600 mb-4">Try adjusting your search or create your first package.</p>
                        <button
                            onClick={() => handleOpenModal()}
                            className="text-white px-6 py-2.5 rounded-xl font-semibold transition-all"
                            style={{ background: 'linear-gradient(135deg, #FF6B35 0%, #FF8C42 50%, #FFB627 100%)' }}
                        >
                            Create First Package
                        </button>
                    </div>
                </div>
            )}

            <Pagination
                currentPage={currentPage}
                totalItems={totalCount}
                itemsPerPage={itemsPerPage}
                onPageChange={setCurrentPage}
                onItemsPerPageChange={setItemsPerPage}
            />

            {isModalOpen && (
                <PackageFormModal isOpen={isModalOpen} onClose={handleCloseModal} onSave={handleSavePackage} editingPackage={editingPackage} categories={categories} />
            )}
        </div>
    );
}
