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
    const [isLoading, setIsLoading] = useState(true);
    const { showToast } = useToast();
    const confirm = useConfirmation();

    const fetchData = async () => {
        setIsLoading(true);
        try {
            // Real API calls
             const [itemsData, catsData, cuisData] = await Promise.all([
                 ownerApiService.getFoodItems(),
                 ownerApiService.getFoodCategories(),
                 ownerApiService.getCuisines(),
             ]);

            await new Promise(res => setTimeout(res, 500));
            setFoodItems(itemsData);
            setCategories(catsData);
            setCuisines(cuisData);
        } catch (error) {
            showToast('Failed to load food items data.', 'error');
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        fetchData();
    }, []);

    const handleOpenModal = (item = null) => {
        setEditingItem(item);
        setIsModalOpen(true);
    };

    const handleCloseModal = () => {
        setIsModalOpen(false);
        setEditingItem(null);
    };

    const handleSaveItem = async (itemData) => {
        setIsLoading(true);
        try {

            let response = null;
            if (editingItem) {
                itemData.id = editingItem.id;
                response = await ownerApiService.updateFoodItem(itemData);
            } else {
                response = await ownerApiService.addFoodItem(itemData);
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
            showToast('Fail food item during process time.', 'error');
        }
        finally {
            setIsLoading(false);
        }
    };

    const handleDeleteItem = async (item) => {
        confirm({
            type: 'delete',
            title: 'Delete Food Item',
            message: `Are you sure you want to delete "${item.name}"?`,
            confirmText: 'Yes, Delete',
            cancelText: 'No, Cancel'
        }).then(async (isConfirmed) => {
            if (isConfirmed) {
                try {
                    const response = await ownerApiService.deleteFoodItem(item.id);
                    if (response && response.message) {
                        showToast(`${item.name} deleted successfully.`, 'success');
                    }
                    else
                        showToast('Failed to delete food item.', 'error');
                    fetchData(); // Refresh the grid
                } catch (error) {
                    showToast('Failed to delete food item.', 'error');
                }
            }
        });
    };

    if (isLoading) { return <div className="flex justify-center items-center h-64"><Loader /></div>; }

    return (
        <div>

            <FoodItemGrid
                items={foodItems}
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
