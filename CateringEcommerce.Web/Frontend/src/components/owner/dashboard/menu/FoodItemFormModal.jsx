/*
========================================
File: src/components/owner/dashboard/menu/FoodItemFormModal.jsx (REVISED - FULL IMPLEMENTATION)
========================================
The modal form for adding or editing a food item.
*/
import React, { useState, useEffect, useCallback } from 'react';
import { useToast } from '../../../../contexts/ToastContext';
import { ownerApiService } from '../../../../services/ownerApi';
import ToggleSwitch from '../../../common/ToggleSwitch';
import MediaGridUploader from '../../../common/MediaGridUploader';
import MediaLightbox from '../../../common/MediaLightbox';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

// Reusable sub-components for the form
const RequiredAsterisk = () => <span className="text-red-500 ml-1">*</span>;
const ValidationError = ({ message }) => message ? <p className="text-red-500 text-xs mt-1">{message}</p> : null;

export default function FoodItemFormModal({ isOpen, onClose, onSave, editingItem, categories, cuisines }) {
    const getInitialState = () => ({
        name: '', description: '', categoryId: '', typeId: 0, price: '', isPackageItem: false, isSampleTaste: false, status: true, media: []
    });

    const [formData, setFormData] = useState(getInitialState());
    const [errors, setErrors] = useState({});
    const [lightboxMedia, setLightboxMedia] = useState(null);
    const { showToast } = useToast();

    // 🔹 When editing existing item, map existing media to include preview URLs
    useEffect(() => {
        if (isOpen) {
            const initialMedia = editingItem?.media || [];
            setFormData(editingItem ? { ...editingItem, media: initialMedia } : getInitialState());
            setErrors({});
        }
    }, [editingItem, isOpen]);

    const validate = useCallback(() => {
        const newErrors = {};
        if (!formData.name.trim()) newErrors.name = 'Item Name is required.';
        if (!formData.categoryId) newErrors.categoryId = 'Category is required.';
        if (!formData.price || formData.price <= 0) newErrors.price = 'A valid price is required.';
        if (formData.media.length === 0) newErrors.media = 'At least one photo or video is required.';
        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    }, [formData]);

    const handleChange = (e) => {
        const { name, value, type, checked } = e.target;
        setFormData(prev => ({ ...prev, [name]: type === 'checkbox' ? checked : value }));
    };

    const handleToggle = (name) => setFormData(prev => ({ ...prev, [name]: !prev[name] }));

    const handleMediaChange = (newMedia) => setFormData(prev => ({ ...prev, media: newMedia }));

    const handleSubmit = (e) => {
        e.preventDefault();
        if (validate()) {
            onSave(formData);
        } else {
            showToast('Please fill all required fields correctly.', 'error');
        }
    };

    if (!isOpen) return null;

    return (
        <>
            <MediaLightbox mediaItem={lightboxMedia} onClose={() => setLightboxMedia(null)} />
            <div className="fixed inset-0 bg-black bg-opacity-60 flex justify-center items-center z-50 p-4">
                <div className="bg-white rounded-xl shadow-2xl w-full max-w-2xl max-h-[90vh] flex flex-col">
                    <h2 className="p-6 border-b text-2xl font-bold text-neutral-800">{editingItem ? 'Edit Food Item' : 'Add New Food Item'}</h2>
                    <form onSubmit={handleSubmit} className="overflow-y-auto flex-1 p-6 space-y-4">
                        <div>
                            <label htmlFor="name" className="block text-sm font-medium text-neutral-700">Name <RequiredAsterisk /></label>
                            <input type="text" name="name" id="name" value={formData.name} onChange={handleChange} autoComplete="off" className={`mt-1 block w-full px-3 py-2 border ${errors.name ? 'border-red-500' : 'border-neutral-300'} rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-rose-500`} />
                            <ValidationError message={errors.name} />
                        </div>
                        <div>
                            <label htmlFor="description" className="block text-sm font-medium text-neutral-700">Description</label>
                            <textarea name="description" id="description" value={formData.description} onChange={handleChange} rows="3" className="mt-1 block w-full px-3 py-2 border border-neutral-300 rounded-md shadow-sm"></textarea>
                        </div>

                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <div>
                                <label htmlFor="categoryId" className="block text-sm font-medium text-neutral-700">Category <RequiredAsterisk /></label>
                                <select name="categoryId" id="categoryId" value={formData.categoryId} onChange={handleChange} className={`mt-1 block w-full px-3 py-2 border ${errors.categoryId ? 'border-red-500' : 'border-neutral-300'} rounded-md`}>
                                    <option value="" disabled>Select a category</option>
                                    {categories.map(c => <option key={c.categoryId} value={c.categoryId}>{c.name}</option>)}
                                </select>
                                <ValidationError message={errors.categoryId} />
                            </div>
                            <div>
                                <label htmlFor="typeId" className="block text-sm font-medium text-neutral-700">Cuisine Type</label>
                                <select name="typeId" id="typeId" value={formData.typeId} onChange={handleChange} className="mt-1 block w-full px-3 py-2 border border-neutral-300 rounded-md">
                                    <option value="">Select a cuisine (optional)</option>
                                    {cuisines.map(c => <option key={c.typeId} value={parseInt(c.typeId)}>{c.typeName}</option>)}
                                </select>
                            </div>
                        </div>

                        <div>
                            <label htmlFor="price" className="block text-sm font-medium text-neutral-700">Price (₹) <RequiredAsterisk /></label>
                            <input type="number" name="price" id="price" value={formData.price} onChange={handleChange} autoComplete="off" className={`mt-1 block w-full px-3 py-2 border ${errors.price ? 'border-red-500' : 'border-neutral-300'} rounded-md`} />
                            <ValidationError message={errors.price} />
                        </div>

                        <MediaGridUploader
                            label="Food Item Media"
                            subtext="Upload high-quality photos or videos of your food item."
                            initialMedia={formData.media || []}
                            onMediaChange={handleMediaChange}
                            onMediaClick={setLightboxMedia}
                            error={errors.media}
                        />

                        <div className="space-y-3 pt-2">
                            <div className="flex items-center">
                                <input type="checkbox" name="isPackageItem" id="isPackageItem" checked={formData.isPackageItem} onChange={handleChange} className="h-4 w-4 text-rose-600 border-neutral-300 rounded" />
                                <label htmlFor="isPackageItem" className="ml-2 block text-sm text-neutral-700">Can be included in packages?</label>
                            </div>
                            {/* NEW TOGGLE ADDED */}
                            <div className="flex items-center">
                                <input type="checkbox" name="isSampleTaste" id="isSampleTaste" checked={formData.isSampleTaste} onChange={handleChange} className="h-4 w-4 text-rose-600 border-neutral-300 rounded" />
                                <label htmlFor="isSampleTaste" className="ml-2 block text-sm text-neutral-700">Available for Sample Taste?</label>
                            </div>
                            <ToggleSwitch label="Status" enabled={formData.status} setEnabled={(value) => handleToggle('status', value)} />
                            <p className="text-xs text-neutral-500">{formData.status ? 'This item is active and visible to customers.' : 'This item is inactive and hidden from customers.'}</p>
                        </div>

                    </form>
                    <div className="p-6 bg-neutral-50 border-t flex justify-end gap-3">
                        <button type="button" onClick={onClose} className="px-5 py-2 rounded-md font-semibold text-neutral-700 bg-neutral-200 hover:bg-neutral-300">Cancel</button>
                        <button type="button" onClick={handleSubmit} className="px-5 py-2 rounded-md font-semibold text-white bg-rose-600 hover:bg-rose-700">
                            {editingItem ? 'Update Item' : 'Save Item'}
                        </button>
                    </div>
                </div>
            </div>
        </>
    );
}