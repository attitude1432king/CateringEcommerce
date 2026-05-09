/*
========================================
File: src/components/owner/dashboard/PackageFormModal.jsx (REVISED)
========================================
A modal form for adding and editing packages, including the logic for managing package items.
*/
import React, { useState, useEffect } from 'react';
import { useToast } from '../../../../contexts/ToastContext';


// Helper component for a single table row
const ItemRow = ({ item, onEdit, onDelete }) => (
    <tr className="border-b">
        <td className="py-3 px-4 text-sm text-neutral-700">{item.categoryName}</td>
        <td className="py-3 px-4 text-sm text-neutral-700">{item.quantity}</td>
        <td className="py-3 px-4 text-sm text-right">
            <button type="button" onClick={() => onEdit(item)} className="text-blue-600 hover:text-blue-800 font-medium mr-3">Edit</button>
            <button type="button" onClick={() => onDelete(item.categoryId)} className="text-red-600 hover:text-red-800 font-medium">Delete</button>
        </td>
    </tr>
);

export default function PackageFormModal({ isOpen, onClose, onSave, editingPackage, categories }) {
    const [packageData, setPackageData] = useState({ name: '', description: '', price: '' });
    const [items, setItems] = useState([]);
    // UPDATED: Initial state for the item form now includes packageItemId
    const [itemFormData, setItemFormData] = useState({ packageItemId: 0, categoryId: '', quantity: 1 });
    const [editingItemId, setEditingItemId] = useState(null);
    const { showToast } = useToast();

    useEffect(() => {
        if (editingPackage) {
            setPackageData({
                name: editingPackage.name,
                description: editingPackage.description,
                price: editingPackage.price,
            });
            // The items from the backend will have packageItemId
            setItems(editingPackage.items || []);
        } else {
            // Reset form for new package
            setPackageData({ name: '', description: '', price: '' });
            setItems([]);
        }
    }, [editingPackage, isOpen]);

    const availableCategories = categories.filter(
        cat => !items.find(item => item.categoryId === cat.categoryId && item.categoryId !== editingItemId)
    );

    const handlePackageChange = (e) => {
        const { name, value } = e.target;
        setPackageData(prev => ({ ...prev, [name]: value }));
    };

    const handleItemFormChange = (e) => {
        const { name, value } = e.target;
        setItemFormData(prev => ({ ...prev, [name]: value }));
    };

    const handleAddItem = () => {
        if (!itemFormData.categoryId || itemFormData.quantity < 1) {
            showToast('Please select a category and enter a valid quantity.', 'warning');
            return;
        }
        const category = categories.find(c => c.categoryId === parseInt(itemFormData.categoryId));

        if (editingItemId) { // Update existing item
            setItems(items.map(item =>
                item.categoryId === editingItemId
                    ? {
                        ...item, // This preserves the original packageItemId
                        categoryId: category.categoryId,
                        categoryName: category.name,
                        quantity: parseInt(itemFormData.quantity)
                    }
                    : item
            ));
            setEditingItemId(null);
        } else { // Add new item
            setItems([...items, {
                packageItemId: 0, // New items don't have a database ID yet
                categoryId: category.categoryId,
                categoryName: category.name,
                quantity: parseInt(itemFormData.quantity)
            }]);
        }

        // Reset sub-form
        setItemFormData({ packageItemId: 0, categoryId: '', quantity: 1 });
    };

    const handleEditItem = (itemToEdit) => {
        setEditingItemId(itemToEdit.categoryId);
        // UPDATED: When editing, populate the form with the item's packageItemId
        setItemFormData({
            packageItemId: itemToEdit.packageItemId,
            categoryId: itemToEdit.categoryId,
            quantity: itemToEdit.quantity
        });
    };

    const handleDeleteItem = (categoryIdToDelete) => {
        setItems(items.filter(item => item.categoryId !== categoryIdToDelete));
    };

    const handleCancelEditItem = () => {
        setEditingItemId(null);
        // Reset sub-form
        setItemFormData({ packageItemId: 0, categoryId: '', quantity: 1 });
    };

    const handleSubmit = (e) => {
        e.preventDefault();
        if (!packageData.name || !packageData.price) {
            showToast('Package Name and Price are required.', 'error');
            return;
        }
        // The `items` array now correctly includes packageItemId for existing items
        onSave({ ...packageData, items });
    };

    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 bg-black bg-opacity-60 flex justify-center items-center z-50 p-4">
            <div className="bg-white rounded-xl shadow-2xl w-full max-w-2xl max-h-[90vh] flex flex-col">
                <div className="p-6 border-b">
                    <h2 className="text-2xl font-bold text-neutral-800">{editingPackage ? 'Edit Package' : 'Add New Package'}</h2>
                </div>

                <form onSubmit={handleSubmit} className="overflow-y-auto flex-1">
                    <div className="p-6 space-y-4">
                        <div>
                            <label htmlFor="name" className="block text-sm font-medium text-neutral-700">Package Name</label>
                            <input type="text" name="name" id="name" value={packageData.name} onChange={handlePackageChange} autoComplete="off" className="mt-1 block w-full px-3 py-2 border border-neutral-300 rounded-md shadow-sm focus:outline-none focus:ring-orange-400" />
                        </div>
                        <div>
                            <label htmlFor="description" className="block text-sm font-medium text-neutral-700">Description</label>
                            <textarea name="description" id="description" value={packageData.description} onChange={handlePackageChange} autoComplete="off" rows="3" className="mt-1 block w-full px-3 py-2 border border-neutral-300 rounded-md shadow-sm focus:outline-none focus:ring-orange-400"></textarea>
                        </div>
                        <div>
                            <label htmlFor="price" className="block text-sm font-medium text-neutral-700">Price (₹)</label>
                            <input type="number" name="price" id="price" value={packageData.price} onChange={handlePackageChange} autoComplete="off" className="mt-1 block w-full px-3 py-2 border border-neutral-300 rounded-md shadow-sm focus:outline-none focus:ring-orange-400" />
                        </div>
                    </div>

                    <div className="p-6 border-t">
                        <h3 className="text-lg font-bold text-neutral-800 mb-4">Package Items</h3>

                        {/* Sub-form for adding/editing items */}
                        <div className="flex items-end gap-3 p-4 bg-neutral-50 rounded-lg mb-4">
                            <div className="flex-1">
                                <label htmlFor="categoryId" className="block text-xs font-medium text-neutral-600">Food Category</label>
                                <select name="categoryId" id="categoryId" value={itemFormData.categoryId} onChange={handleItemFormChange} className="mt-1 block w-full px-3 py-2 border border-neutral-300 rounded-md shadow-sm focus:outline-none focus:ring-orange-400 text-sm">
                                    <option value="" disabled>Select a category</option>
                                    {availableCategories.map(cat => (
                                        <option key={cat.categoryId} value={cat.categoryId}>{cat.name}</option>
                                    ))}
                                </select>
                            </div>
                            <div className="w-24">
                                <label htmlFor="quantity" className="block text-xs font-medium text-neutral-600">Quantity</label>
                                <input type="number" name="quantity" id="quantity" value={itemFormData.quantity} onChange={handleItemFormChange} min="1" className="mt-1 block w-full px-3 py-2 border border-neutral-300 rounded-md shadow-sm focus:outline-none focus:ring-orange-400 text-sm" />
                            </div>
                            <button type="button" onClick={handleAddItem}
                                className="px-4 py-2 rounded-md font-semibold text-white text-sm transition-all"
                                style={{ background: editingItemId ? '#2563EB' : 'linear-gradient(135deg, #FF6B35 0%, #FF8C42 50%, #FFB627 100%)' }}
                            >
                                {editingItemId ? 'Update Item' : 'Add Item'}
                            </button>
                            {editingItemId && (
                                <button type="button" onClick={handleCancelEditItem} className="px-4 py-2 rounded-md font-semibold bg-neutral-200 text-neutral-700 text-sm hover:bg-neutral-300">
                                    Cancel
                                </button>
                            )}
                        </div>

                        {/* Data grid for items */}
                        {items.length > 0 ? (
                            <div className="border rounded-lg overflow-hidden">
                                <table className="min-w-full">
                                    <thead className="bg-neutral-50">
                                        <tr>
                                            <th className="py-2 px-4 text-left text-xs font-semibold text-neutral-600 uppercase">Category Name</th>
                                            <th className="py-2 px-4 text-left text-xs font-semibold text-neutral-600 uppercase">Quantity</th>
                                            <th className="py-2 px-4"></th>
                                        </tr>
                                    </thead>
                                    <tbody className="bg-white">
                                        {items.map(item => (
                                            <ItemRow key={item.categoryId} item={item} onEdit={handleEditItem} onDelete={handleDeleteItem} />
                                        ))}
                                    </tbody>
                                </table>
                            </div>
                        ) : (
                            <p className="text-center text-sm text-neutral-500 py-4">No items have been added to this package yet.</p>
                        )}
                    </div>
                </form>

                <div className="p-6 bg-neutral-50 border-t flex justify-end gap-3">
                    <button onClick={onClose} className="px-4 py-2 rounded-md text-sm font-medium text-neutral-700 bg-neutral-200 hover:bg-neutral-300">
                        Cancel
                    </button>
                    <button onClick={handleSubmit}
                        className="px-4 py-2 rounded-md text-sm font-medium text-white transition-all"
                        style={{ background: 'linear-gradient(135deg, #FF6B35 0%, #FF8C42 50%, #FFB627 100%)' }}>
                        {editingPackage ? 'Update Package' : 'Save Package'}
                    </button>
                </div>
            </div>
        </div>
    );
}
