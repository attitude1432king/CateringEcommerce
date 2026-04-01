/*
========================================
File: src/components/owner/dashboard/banners/BannersView.jsx
Main view for managing banners
========================================
*/
import React, { useState, useEffect } from 'react';
import Loader from '../../../common/Loader';
import { useToast } from '../../../../contexts/ToastContext';
import BannersGrid from './BannersGrid';
import BannerFormModal from './BannerFormModal';
import { ownerApiService } from '../../../../services/ownerApi';
import { useConfirmation } from '../../../../contexts/ConfirmationContext';

export default function BannersView() {
    const [banners, setBanners] = useState([]);
    const [isLoading, setIsLoading] = useState(true);
    const { showToast } = useToast();
    const confirm = useConfirmation();

    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingItem, setEditingItem] = useState(null);

    // Pagination states
    const [currentPage, setCurrentPage] = useState(1);
    const [itemsPerPage, setItemsPerPage] = useState(10);
    const [totalCount, setTotalCount] = useState(0);

    // Filters
    const [filters, setFilters] = useState({
        title: '',
        isActive: null
    });

    // Debounced search
    const [debouncedFilters, setDebouncedFilters] = useState(filters);

    // Apply 500ms debounce
    useEffect(() => {
        const timer = setTimeout(() => {
            setDebouncedFilters(filters);
        }, 500);
        return () => clearTimeout(timer);
    }, [filters]);

    // Fetch data
    const fetchData = async () => {
        setIsLoading(true);
        const filterJson = JSON.stringify(debouncedFilters);

        try {
            // Get total count first
            const totalRecords = await ownerApiService.getBannersCount(filterJson);
            setTotalCount(totalRecords);

            // If no records, skip list API
            if (totalRecords === 0) {
                setBanners([]);
                return;
            }

            // Fetch data only if count > 0
            const items = await ownerApiService.getBannersList(
                currentPage,
                itemsPerPage,
                filterJson
            );

            setBanners(items);
        } catch (error) {
            console.error('Error fetching banners:', error);
            showToast('Failed to load banners.', 'error');
        } finally {
            setIsLoading(false);
        }
    };

    // Fetch data when pagination or filters change
    useEffect(() => {
        fetchData();
    }, [currentPage, itemsPerPage, debouncedFilters]);

    // Reset to page 1 when filters change
    useEffect(() => {
        setCurrentPage(1);
    }, [debouncedFilters]);

    // Modal handlers
    const handleOpenModal = (item = null) => {
        setEditingItem(item);
        setIsModalOpen(true);
    };

    const handleCloseModal = () => {
        setEditingItem(null);
        setIsModalOpen(false);
    };

    // Save handler
    const handleSaveItem = async (itemData) => {
        setIsLoading(true);
        try {
            let response = null;
            if (editingItem) {
                itemData.id = editingItem.id;
                response = await ownerApiService.updateBanner(itemData);
            } else {
                response = await ownerApiService.createBanner(itemData);
            }

            if (response?.result) {
                showToast(response.message, "success");
                fetchData();
            } else {
                showToast(response.message, response.type);
            }
            handleCloseModal();
        } catch (error) {
            showToast('Failed to save banner.' + error, 'error');
        } finally {
            setIsLoading(false);
        }
    };

    // Delete handler
    const handleDeleteItem = async (item) => {
        confirm({
            type: "delete",
            title: "Delete Banner",
            message: `Are you sure you want to delete "${item.title}"?`,
            confirmText: "Yes, Delete",
            cancelText: "Cancel"
        }).then(async (confirmed) => {
            if (!confirmed) return;

            try {
                await ownerApiService.deleteBanner(item.id);
                showToast(`${item.title} deleted successfully.`, "success");
                fetchData();
            } catch (error) {
                showToast("Failed to delete banner." + error, "error");
            }
        });
    };

    // Status change handler
    const handleStatusChange = async (item, newStatus) => {
        setIsLoading(true);
        try {
            const response = await ownerApiService.updateBannerStatus(item.id, newStatus);
            if (response?.result) {
                showToast(`${item.title} status updated successfully.`, "success");
                fetchData();
            } else {
                showToast(response.message, response.type);
            }
        } catch (error) {
            showToast('Failed to update status' + error, 'error');
            fetchData();
        } finally {
            setIsLoading(false);
        }
    };

    if (isLoading) return <div className="flex justify-center items-center h-64"><Loader /></div>;

    return (
        <div>
            <BannersGrid
                items={banners}
                filters={filters}
                setFilters={setFilters}
                totalCount={totalCount}
                currentPage={currentPage}
                itemsPerPage={itemsPerPage}
                setCurrentPage={setCurrentPage}
                setItemsPerPage={setItemsPerPage}
                onEditItem={handleOpenModal}
                onDeleteItem={handleDeleteItem}
                onAddItem={() => handleOpenModal(null)}
                onStatusChange={handleStatusChange}
            />

            {isModalOpen && (
                <BannerFormModal
                    isOpen={isModalOpen}
                    onClose={handleCloseModal}
                    onSave={handleSaveItem}
                    editingItem={editingItem}
                />
            )}
        </div>
    );
}
