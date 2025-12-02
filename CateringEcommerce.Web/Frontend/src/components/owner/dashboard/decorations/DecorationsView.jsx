/*
========================================
File: src/components/owner/dashboard/decorations/DecorationsView.jsx (FINAL)
========================================
*/
import React, { useState, useEffect } from 'react';
import Loader from '../../../common/Loader';
import { useToast } from '../../../../contexts/ToastContext';
import DecorationGrid from './DecorationGrid';
import DecorationFormModal from './DecorationFormModal';
import { ownerApiService } from '../../../../services/ownerApi';
import { useConfirmation } from '../../../../contexts/ConfirmationContext';

export default function DecorationsView() {

    const [decorations, setDecorations] = useState([]);
    const [themes, setThemes] = useState([]);
    const [packages, setPackages] = useState([]);

    const [isLoading, setIsLoading] = useState(true);
    const { showToast } = useToast();
    const confirm = useConfirmation();

    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingItem, setEditingItem] = useState(null);

    // 🔥 Backend pagination states
    const [currentPage, setCurrentPage] = useState(1);
    const [itemsPerPage, setItemsPerPage] = useState(10);
    const [totalCount, setTotalCount] = useState(0);

    // 🔥 Filters moved here
    const [filters, setFilters] = useState({
        name: '',
        themeIds: [],
        status: '',
        packageIds: []
    });

    // 🔥 Debounced search
    const [debouncedFilters, setDebouncedFilters] = useState(filters);

    // Apply 500ms debounce for ANY filter
    useEffect(() => {
        const timer = setTimeout(() => {
            setDebouncedFilters(filters);
        }, 500);
        return () => clearTimeout(timer);
    }, [filters]);

    // ---------------- FETCH DECORATION LIST ----------------
    const fetchData = async () => {
        setIsLoading(true);

        const filterJson = JSON.stringify(debouncedFilters);

        try {
            // Count + Data combined
            const [totalRecords, items] = await Promise.all([
                ownerApiService.getDecorationsCount(filterJson),
                ownerApiService.getDecorations(currentPage, itemsPerPage, filterJson)
            ]);

            setTotalCount(totalRecords);
            setDecorations(totalRecords > 0 ? items : []);

        } catch (error) {
            showToast('Failed to load decoration data.', 'error');
        } finally {
            setIsLoading(false);
        }
    };

    // Fetch Themes + Packages only ONCE
    const fetchCategories = async () => {
        try {
            const [themeData, packageData] = await Promise.all([
                ownerApiService.getDecorationThemes(),
                ownerApiService.getPackagesLookup()
            ]);
            setThemes(themeData);
            setPackages(packageData);

        } catch (error) {
            showToast('Failed to load lookup data.', 'error');
        }
    };

    // First Load
    useEffect(() => { fetchCategories(); }, []);

    // 🔥 Fetch data anytime pagination OR filters change
    useEffect(() => {
        fetchData();
    }, [currentPage, itemsPerPage, debouncedFilters]);


    // Reset to page 1 when filters change
    useEffect(() => {
        setCurrentPage(1);
    }, [debouncedFilters]);


    // ---------------- MODAL HANDLERS ----------------
    const handleOpenModal = (item = null) => {
        setEditingItem(item);
        setIsModalOpen(true);
    };

    const handleCloseModal = () => {
        setEditingItem(null);
        setIsModalOpen(false);
    };


    // ---------------- SAVE ----------------
    const handleSaveItem = async (itemData) => {
        setIsLoading(true);
        try {
            let response = null;
            if (editingItem) {
                itemData.id = editingItem.id;
                response = await ownerApiService.updateDecorations(itemData);
            } else {
                response = await ownerApiService.addDecorations(itemData);
            }

            if (response?.result) {
                showToast(response.message, "success");
                fetchData();
            } else {
                showToast(response.message, response.type);
            }
            handleCloseModal();

        } catch (error) {
            showToast('Failed to save decoration setup.', 'error');
        } finally {
            setIsLoading(false);
        }
    };

    // ---------------- DELETE ----------------
    const handleDeleteItem = async (item) => {
        confirm({
            type: "delete",
            title: "Delete Decoration",
            message: `Are you sure you want to delete "${item.name}"?`,
            confirmText: "Yes, Delete",
            cancelText: "Cancel"
        }).then(async (confirmed) => {
            if (!confirmed) return;

            try {
                await ownerApiService.deleteDecorations(item.id);
                showToast(`${item.name} deleted successfully.`, "success");
                fetchData();
            } catch (error) {
                showToast("Failed to delete decoration setup.", "error");
            }
        });
    };

    const handleStatusChange = async (item, newStatus) => {
        // ... (Optimistic update logic remains the same)
        isLoading(true);
        try {
            const response = await ownerApiService.updateDecorationStatus(item.Id, newStatus);
            if (response?.result) {
                showToast(`${item.name} status updated successfully.`, "success");
                fetchData();
            } else {
                showToast(response.message, response.type);
            }
        } catch (error) {
            showToast('Failed to update status', 'error');
            // Revert
            fetchData(); // Re-fetch to be safe
        }
        finally {
            isLoading(false);
        }
    };

    if (isLoading) return <div className="flex justify-center items-center h-64"><Loader /></div>;

    return (
        <div>
            <DecorationGrid
                items={decorations}
                filters={filters}
                setFilters={setFilters}
                themes={themes}
                packages={packages}
                totalCount={totalCount}
                currentPage={currentPage}
                itemsPerPage={itemsPerPage}
                setCurrentPage={setCurrentPage}
                setItemsPerPage={setItemsPerPage}
                onEditItem={handleOpenModal}
                onDeleteItem={handleDeleteItem}
                onAddItem={() => handleOpenModal(null)}
                onStatusChange={() => handleStatusChange} 
            />

            {isModalOpen && (
                <DecorationFormModal
                    isOpen={isModalOpen}
                    onClose={handleCloseModal}
                    onSave={handleSaveItem}
                    editingItem={editingItem}
                    themes={themes}
                    packages={packages}
                />
            )}
        </div>
    );
}
