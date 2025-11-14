
/*
========================================
File: src/components/owner/dashboard/decorations/DecorationsView.jsx (REVISED)
========================================
*/
import React, { useState, useEffect } from 'react';
import Loader from '../../../common/Loader';
import { useToast } from '../../../../contexts/ToastContext';
import DecorationGrid from './DecorationGrid';
import DecorationFormModal from './DecorationFormModal';
import { ownerApiService } from '../../../../services/ownerApi';
import { useConfirmation } from '../../../../contexts/ConfirmationContext';

// Mock Data
const mockPackagesLookup = [
    { id: 1, name: "Standard Veg Buffet" },
    { id: 2, name: "Premium Non-Veg Gala" },
];

const mockDecorations = [
    { id: 1, name: "Royal Wedding Entrance", theme: "Royal", description: "Grand entrance setup with red carpets and floral arrangements.", price: 25000, status: true, media: [{ id: 'm1', mediaType: 'image', filePath: 'https://placehold.co/400x300/8c2d2d/ffffff?text=Royal+Entrance' }], linkedPackages: [{ id: 2, name: "Premium Non-Veg Gala" }] },
];
const mockThemes = ["Royal", "Modern", "Gold", "Minimalist", "Floral"];
// End Mock Data

export default function DecorationsView() {
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingItem, setEditingItem] = useState(null);
    const [decorations, setDecorations] = useState([]);
    const [themes, setThemes] = useState([]);
    const [packages, setPackages] = useState([]); // New state for packages
    const [isLoading, setIsLoading] = useState(true);
    const { showToast } = useToast();
    const confirm = useConfirmation();


    const fetchData = async () => {
        setIsLoading(true);
        try {
            // Real API calls
            const [decorData, themeData, packagesData] = await Promise.all([
                ownerApiService.getDecorations(),
                ownerApiService.getDecorationThemes(),
                ownerApiService.getPackagesLookup(), // Fetch packages
            ]);

            setDecorations(decorData);
            setThemes(themeData);
            setPackages(packagesData); // Set packages
        } catch (error) {
            showToast('Failed to load decorations data.', 'error');
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
        try {
            let response = null;
            if (editingItem) {
                itemData.id = editingItem.id;
                response = await ownerApiService.updateDecorations(itemData);
            } else {
                response = await ownerApiService.addDecorations(itemData);
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
            showToast('Fail package during process time.', 'error');
        }
    };

    const handleDeleteItem = async (item) => {
        confirm({
            type: 'delete',
            title: 'Delete Decoration',
            message: `Are you sure you want to delete "${item.name}"?`,
            confirmText: 'Yes, Delete',
            cancelText: 'No, Cancel'
        }).then(async (isConfirmed) => {
            if (isConfirmed) {
                try {
                    const response = await ownerApiService.deleteDecorations(item.id);
                    if (response && response.message) {
                        showToast(`${item.name} deleted successfully.`, 'success');
                    }
                    else
                        showToast('Failed to delete decoration setup.', 'error');
                    fetchData(); // Refresh the grid
                } catch (error) {
                    showToast('Failed to delete decoration setup.', 'error');
                }
            }
        });
    };

    if (isLoading) { return <div className="flex justify-center items-center h-64"><Loader /></div>; }

    return (
        <div>
            <DecorationGrid
                items={decorations}
                themes={themes}
                packages={packages} // Pass packages to grid
                onEditItem={handleOpenModal}
                onDeleteItem={handleDeleteItem}
                onAddItem={() => handleOpenModal(null)}
            />

            {isModalOpen && (
                <DecorationFormModal
                    isOpen={isModalOpen}
                    onClose={handleCloseModal}
                    onSave={handleSaveItem}
                    editingItem={editingItem}
                    themes={themes}
                    packages={packages} // Pass packages to modal
                />
            )}
        </div>
    );
}