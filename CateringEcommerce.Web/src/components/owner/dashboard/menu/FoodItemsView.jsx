/*
========================================
File: src/components/owner/dashboard/menu/FoodItemsView.jsx (NEW FILE)
========================================
The main view for managing individual food items, including filters and the grid.
*/
import React, { useState, useEffect } from 'react';
import FoodItemGrid from './FoodItemGrid';
import FoodItemFormModal from './FoodItemFormModal';
import { useToast } from '../../../../contexts/ToastContext';
import Loader from '../../../common/Loader';
import { ownerApiService } from '../../../../services/ownerApi';
import { useConfirmation } from '../../../../contexts/ConfirmationContext';

export default function FoodItemsView() {
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingItem, setEditingItem] = useState(null);

    const [foodItems, setFoodItems] = useState([]);
    const [categories, setCategories] = useState([]);
    const [cuisines, setCuisines] = useState([]);

    const [currentPage, setCurrentPage] = useState(1);
    const [itemsPerPage, setItemsPerPage] = useState(10);

    const [filters, setFilters] = useState({
        name: "",
        categoryIds: [],
        cuisineIds: [],
        status: "",
        isPackageItem: false,
        isSampleTaste: false,
    });

    const [totalCount, setTotalCount] = useState(0);
    const [isLoading, setIsLoading] = useState(true);
    const { showToast } = useToast();
    const confirm = useConfirmation();

    // 👉 Build Filter Object as JSON
    const filterJson = JSON.stringify(filters);

    const fetchData = async () => {
        setIsLoading(true);

        try {
            // STEP 1: Get count + data TOGETHER in one await block
            const [totalRecords, items] = await Promise.all([
                ownerApiService.getFoodItemsCount(filterJson),
                ownerApiService.getFoodItems(currentPage, itemsPerPage, filterJson)
            ]);

            // STEP 2: Update state after both API calls finish
            setTotalCount(totalRecords);
            setFoodItems(totalRecords > 0 ? items : []);

        } catch (error) {
            showToast('Failed to load food items.', 'error');
        } finally {
            // STEP 3: Hide loader after both COUNT and DATA are done
            setIsLoading(false);
        }
    };


    const fetchCategories = async () => {
        try {
            const [catsData, cuisData] = await Promise.all([
                ownerApiService.getFoodCategories(),
                ownerApiService.getCuisines(),
            ]);

            setCategories(catsData);
            setCuisines(cuisData);

        } catch (error) {
            showToast('Failed to load categories.', 'error');
        }
    };

    // Load categories once
    useEffect(() => { fetchCategories(); }, []);

    // ***********************************************************************************
    // 👉 Fetch data whenever page, pageSize, or filters change
    // ***********************************************************************************
    useEffect(() => {
        fetchData();
    }, [currentPage, itemsPerPage, filterJson]);

    // Reset to page 1 when filters change
    useEffect(() => {
        setCurrentPage(1);
    }, [filterJson]);


    const handleOpenModal = (item = null) => {
        setEditingItem(item);
        setIsModalOpen(true);
    };

    const handleCloseModal = () => {
        setEditingItem(null);
        setIsModalOpen(false);
    };


    const handleSaveItem = async (itemData) => {
        setIsLoading(true);

        try {
            let response;
            if (editingItem) {
                itemData.id = editingItem.id;
                response = await ownerApiService.updateFoodItem(itemData);
            } else {
                response = await ownerApiService.addFoodItem(itemData);
            }

            if (response.result) {
                showToast(response.message, "success");
                fetchData();
            } else {
                showToast(response.message, "error");
            }
            handleCloseModal();

        } catch (error) {
            showToast("Failed while saving item.", "error");
        } finally {
            setIsLoading(false);
        }
    };


    const handleDeleteItem = async (item) => {
        confirm({
            type: "delete",
            title: "Delete Food Item",
            message: `Are you sure you want to delete "${item.name}"?`,
            confirmText: "Yes, Delete",
            cancelText: "Cancel"
        }).then(async (confirmed) => {
            if (!confirmed) return;

            try {
                await ownerApiService.deleteFoodItem(item.id);
                showToast(`${item.name} deleted successfully.`, "success");
                fetchData();

            } catch (error) {
                showToast("Failed to delete item.", "error");
            }
        });
    };


    if (isLoading) return <div className="flex justify-center items-center h-64"><Loader /></div>;

    return (
        <div>
            <FoodItemGrid
                items={foodItems}
                totalCount={totalCount}
                filters={filters}
                setFilters={setFilters}
                currentPage={currentPage}
                setCurrentPage={setCurrentPage}
                itemsPerPage={itemsPerPage}
                setItemsPerPage={setItemsPerPage}
                categories={categories}
                cuisines={cuisines}
                onEditItem={handleOpenModal}
                onDeleteItem={handleDeleteItem}
                onAddItem={() => handleOpenModal(null)}
            />

            {isModalOpen && (
                <FoodItemFormModal
                    isOpen={isModalOpen}
                    onClose={handleCloseModal}
                    onSave={handleSaveItem}
                    editingItem={editingItem}
                    categories={categories}
                    cuisines={cuisines}
                />
            )}
        </div>
    );
}
