/*
========================================
File: src/components/owner/dashboard/staff/StaffView.jsx (REVISED)
========================================
Main view for managing staff. Now passes 'categories' to the modal.
*/
import React, { useState, useEffect } from 'react';
import Loader from '../../../common/Loader';
import { useToast } from '../../../../contexts/ToastContext';
import StaffGrid from './StaffGrid';
import StaffFormModal from './StaffFormModal';
import { ownerApiService } from '../../../../services/ownerApi';

// Mock Data
const mockStaff = [
    { id: 1, profilePath: 'https://placehold.co/100x100/f8b4b4/7f1d1d?text=Chef', name: "Ramesh Kumar", role: "Head Chef", expertise: "North Indian", salaryType: "Monthly", salaryAmount: 35000, availability: true },
    { id: 2, photo: [{ id: 'm2', type: 'image', path: 'https://placehold.co/100x100/bbf7d0/166534?text=Server' }], name: "Suresh Singh", role: "Server", expertise: null, salaryType: "Per Day", salaryAmount: 1500, availability: false },
    { id: 3, photo: [{ id: 'm3', type: 'image', path: 'https://placehold.co/100x100/bfdbfe/1e3a8a?text=Helper' }], name: "Mina Devi", role: "Helper", expertise: null, salaryType: "Monthly", salaryAmount: 15000, availability: true },
    { id: 4, photo: [], name: "Vikram Bhat", role: "Pastry Chef", expertise: "Dessert", salaryType: "Monthly", salaryAmount: 28000, availability: true },
];

export default function StaffView() {
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingItem, setEditingItem] = useState(null);
    const [staff, setStaff] = useState([]);
    const [categories, setCategories] = useState([]); // For "Expertise"
    const [isLoading, setIsLoading] = useState(true);
    const [totalCount, setTotalCount] = useState(0);
    const [currentPage, setCurrentPage] = useState(1);
    const [itemsPerPage, setItemsPerPage] = useState(10);
    const [filters, setFilters] = useState({
        name: "",
        role: "",
        status: "",
    });
    const { showToast } = useToast();
    // 🔥 Debounced search
    const [debouncedFilters, setDebouncedFilters] = useState(filters);

    // 👉 Build Filter Object as JSON
    const filterJson = JSON.stringify(debouncedFilters);

    // Apply 500ms debounce for ANY filter
    useEffect(() => {
        const timer = setTimeout(() => {
            setDebouncedFilters(filters);
        }, 500);
        return () => clearTimeout(timer);
    }, [filters]);

    const fetchData = async () => {
        setIsLoading(true);
        try {
            // STEP 1: Get count + data TOGETHER in one await block
            const [totalRecords, items] = await Promise.all([
                ownerApiService.getStaffCount(filterJson),
                ownerApiService.getStaffList(currentPage, itemsPerPage, filterJson),
            ]);

            // STEP 2: Update state after both API calls finish
            setTotalCount(totalRecords);
            setStaff(totalRecords > 0 ? items : []);
            
        } catch (error) {
            showToast('Failed to load staff data.', 'error');
        } finally {
            setIsLoading(false);
        }
    };

    // Fetch Food Categories only ONCE
    const fetchCategories = async () => {
        try {
            const categoriesData = await ownerApiService.getFoodCategories(); // Re-using food categories for expertise
            setCategories(categoriesData);
        } catch (error) {
            showToast('Failed to load food categories data.', 'error');
        }
    };

    // First Load
    useEffect(() => { fetchCategories(); }, []);

    useEffect(() => {
        fetchData();
    }, [currentPage, itemsPerPage, filterJson]);

    const handleOpenModal = (item = null) => {
        setEditingItem(item);
        setIsModalOpen(true);
    };

    const handleCloseModal = () => {
        setIsModalOpen(false);
        setEditingItem(null);
    };

    const handleSaveItem = async (itemData, filesToDelete) => {
        setIsLoading(true);
        try {

            let response = null;
            if (editingItem) {
                itemData.id = editingItem.id;
                response = await ownerApiService.updateStaffMember(itemData, filesToDelete);
            } else {
                response = await ownerApiService.createStaffMember(itemData);
            }

            if (response && response.message && response.result) {
                showToast(response.message, 'success');
                fetchData(); // Refresh the grid
            }
            else {
                showToast(response.message, response.type ?? 'error');
            }

            handleCloseModal();
        } catch (error) {
            showToast(`Error: ${error.message || 'Could not save staff member.'}`, 'error');
        }
        finally {
            setIsLoading(false);
        }
    };

    const handleDeleteItem = async (item) => {
        setIsLoading(true);
        try {
            const response = await ownerApiService.deleteStaffMember(item.id);
            if (response && response.message) {
                showToast(`${item.name} deleted successfully.`, 'success');
            }
            else
                showToast('Failed to delete staff member.', 'error');
            fetchData(); // Refresh the grid
        } catch (error) {
            showToast('Failed to delete staff member.', 'error');
        }
        finally {
            setIsLoading(false);
        }
    };

    const handleStatusChange = async (item, newStatus) => {
        // ... (Optimistic update logic remains the same)
        setIsLoading(true);
        try {
            const response = await ownerApiService.updateStaffStatus(item.id, newStatus);
            if (response?.result) {
                showToast(`${item.name} is ${newStatus ? 'Available' : 'Unavailable'}.`, "success");
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
            setIsLoading(false);
        }
    };


    if (isLoading) { return <div className="flex justify-center items-center h-64"><Loader /></div>; }

    return (
        <div>
            <StaffGrid
                items={staff}
                totalCount={totalCount}
                filters={filters}
                setFilters={setFilters}
                currentPage={currentPage}
                setCurrentPage={setCurrentPage}
                itemsPerPage={itemsPerPage}
                setItemsPerPage={setItemsPerPage}
                onEditItem={handleOpenModal}
                onDeleteItem={handleDeleteItem}
                onAddItem={() => handleOpenModal(null)}
                onStatusChange={handleStatusChange}
            />

            {isModalOpen && (
                <StaffFormModal
                    isOpen={isModalOpen}
                    onClose={handleCloseModal}
                    onSave={handleSaveItem}
                    editingItem={editingItem}
                    expertiseCategories={categories} // <-- PASS CATEGORIES HERE
                />
            )}
        </div>
    );
}