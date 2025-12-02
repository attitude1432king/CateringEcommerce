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

// Package Card Component
const PackageCard = ({ pkg, onEdit, onDelete }) => (
    <div className="bg-white rounded-lg shadow-md overflow-hidden transition-all hover:shadow-xl">
        <div className="p-5">
            <h3 className="text-lg font-bold text-neutral-800">{pkg.name}</h3>
            <p className="text-sm text-neutral-500 mt-1 h-10 truncate">{pkg.description}</p>
            <div className="flex justify-between items-center mt-4">
                <p className="text-xl font-bold text-rose-600">₹{pkg.price}</p>
                <span className="text-sm font-medium text-neutral-600 bg-neutral-100 px-3 py-1 rounded-full">
                    {pkg.items.length} Items
                </span>
            </div>
        </div>
        <div className="bg-neutral-50 px-5 py-3 flex justify-end gap-3">
            <button onClick={() => onEdit(pkg)} className="text-sm font-semibold text-blue-600 hover:text-blue-800">Edit</button>
            <button onClick={() => onDelete(pkg)} className="text-sm font-semibold text-red-600 hover:text-red-800">Delete</button>
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
        <div className="bg-white p-6 rounded-xl shadow-sm">
            <div className="flex flex-col md:flex-row justify-between items-center mb-6 gap-4">
                <input
                    type="text"
                    name="name"
                    placeholder="Search by package name..."
                    value={searchQuery}
                    autoComplete="off"
                    onChange={(e) => {
                        setSearchQuery(e.target.value);
                        setCurrentPage(1);
                    }}
                    className="w-full md:w-1/3 px-3 py-2 border border-neutral-300 rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-rose-500 focus:border-rose-500"
                />
                <button onClick={() => handleOpenModal()} className="bg-rose-600 text-white px-4 py-2 rounded-lg font-semibold hover:bg-rose-700 flex items-center gap-2 w-full md:w-auto">
                    <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor"><path fillRule="evenodd" d="M10 3a1 1 0 011 1v5h5a1 1 0 110 2h-5v5a1 1 0 11-2 0v-5H4a1 1 0 110-2h5V4a1 1 0 011-1z" clipRule="evenodd" /></svg>
                    Add New Package
                </button>
            </div>
            {totalCount > 0 ? (
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
                    {packages.map(pkg => (
                        <PackageCard key={pkg.packageId} pkg={pkg} onEdit={handleOpenModal} onDelete={handleDeletePackage} />
                    ))}
                </div>
            ) : (<div className="text-center py-16 bg-white rounded-lg">
                <h3 className="text-xl font-semibold text-neutral-700">No Packages Found</h3>
                <p className="text-neutral-500 mt-2">Try adjusting your search or add a new package.</p>
            </div>)}

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
