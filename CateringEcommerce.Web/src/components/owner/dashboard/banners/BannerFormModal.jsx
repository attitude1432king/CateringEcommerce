/*
========================================
File: src/components/owner/dashboard/banners/BannerFormModal.jsx
Modal form for creating/editing banners
========================================
*/
import React, { useState, useEffect } from 'react';
import { useToast } from '../../../../contexts/ToastContext';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL.replace(/\/$/, '');

export default function BannerFormModal({ isOpen, onClose, onSave, editingItem }) {
    const { showToast } = useToast();
    const [formData, setFormData] = useState({
        title: '',
        description: '',
        linkUrl: '',
        displayOrder: 0,
        isActive: true,
        startDate: '',
        endDate: ''
    });

    const [imagePreview, setImagePreview] = useState(null);
    const [imageFile, setImageFile] = useState(null);

    useEffect(() => {
        if (editingItem) {
            setFormData({
                title: editingItem.title || '',
                description: editingItem.description || '',
                linkUrl: editingItem.linkUrl || '',
                displayOrder: editingItem.displayOrder || 0,
                isActive: editingItem.isActive ?? true,
                startDate: editingItem.startDate ? editingItem.startDate.split('T')[0] : '',
                endDate: editingItem.endDate ? editingItem.endDate.split('T')[0] : ''
            });

            if (editingItem.imagePath) {
                setImagePreview(`${API_BASE_URL}${editingItem.imagePath}`);
            }
        }
    }, [editingItem]);

    const handleInputChange = (e) => {
        const { name, value, type, checked } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: type === 'checkbox' ? checked : value
        }));
    };

    const handleImageChange = (e) => {
        const file = e.target.files[0];
        if (file) {
            // Validate file type
            if (!file.type.startsWith('image/')) {
                showToast('Please select a valid image file.', 'warning');
                return;
            }

            // Validate file size (max 20MB)
            if (file.size > 20 * 1024 * 1024) {
                showToast('Image size should not exceed 20MB.', 'warning');
                return;
            }

            setImagePreview(URL.createObjectURL(file));
            setImageFile(file);
        }
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        // Validation
        if (!formData.title.trim()) {
            showToast('Banner title is required.', 'warning');
            return;
        }

        if (!editingItem && !imageFile) {
            showToast('Banner image is required.', 'warning');
            return;
        }

        // Prepare data
        const bannerData = {
            ...formData,
            displayOrder: parseInt(formData.displayOrder) || 0
        };

        if (imageFile) {
            bannerData.bannerImage = imageFile;
        }

        if (editingItem && editingItem.imagePath) {
            bannerData.imagePath = editingItem.imagePath;
        }

        await onSave(bannerData);
    };

    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
            <div className="bg-white rounded-2xl shadow-2xl max-w-2xl w-full max-h-[90vh] overflow-y-auto">
                {/* Header */}
                <div className="sticky top-0 text-white px-6 py-4 rounded-t-2xl flex justify-between items-center"
                    style={{ background: 'linear-gradient(135deg, #FF6B35 0%, #FF8C42 50%, #FFB627 100%)' }}>
                    <h2 className="text-2xl font-bold">
                        {editingItem ? 'Edit Banner' : 'Create New Banner'}
                    </h2>
                    <button
                        onClick={onClose}
                        className="text-white hover:bg-white hover:bg-opacity-20 rounded-lg p-2 transition-all"
                    >
                        <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                        </svg>
                    </button>
                </div>

                {/* Form */}
                <form onSubmit={handleSubmit} className="p-6 space-y-6">
                    {/* Image Upload */}
                    <div>
                        <label className="block text-sm font-semibold text-neutral-700 mb-2">
                            Banner Image <span className="text-red-500">*</span>
                        </label>
                        <div className="border-2 border-dashed border-neutral-300 rounded-xl p-4 text-center hover:border-orange-400 transition-colors">
                            {imagePreview ? (
                                <div className="relative">
                                    <img
                                        src={imagePreview}
                                        alt="Preview"
                                        className="max-h-48 mx-auto rounded-lg"
                                    />
                                    <button
                                        type="button"
                                        onClick={() => {
                                            setImagePreview(null);
                                            setImageFile(null);
                                        }}
                                        className="absolute top-2 right-2 bg-red-500 text-white rounded-full p-2 hover:bg-red-600"
                                    >
                                        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                                        </svg>
                                    </button>
                                </div>
                            ) : (
                                <div className="py-8">
                                    <svg className="w-12 h-12 mx-auto text-neutral-400 mb-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                                    </svg>
                                    <p className="text-neutral-600 mb-2">Click to upload banner image</p>
                                    <p className="text-xs text-neutral-500">Recommended size: 1920x600px (Max 20MB)</p>
                                </div>
                            )}
                            <input
                                type="file"
                                accept="image/*"
                                onChange={handleImageChange}
                                className="hidden"
                                id="bannerImage"
                            />
                            <label
                                htmlFor="bannerImage"
                                className="inline-block mt-3 px-4 py-2 text-white rounded-lg cursor-pointer transition-all"
                                style={{ background: 'var(--color-primary)' }}
                            >
                                Choose Image
                            </label>
                        </div>
                    </div>

                    {/* Title */}
                    <div>
                        <label className="block text-sm font-semibold text-neutral-700 mb-2">
                            Banner Title <span className="text-red-500">*</span>
                        </label>
                        <input
                            type="text"
                            name="title"
                            value={formData.title}
                            onChange={handleInputChange}
                            className="w-full px-4 py-3 border border-neutral-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-orange-400"
                            placeholder="Enter banner title"
                            required
                        />
                    </div>

                    {/* Description */}
                    <div>
                        <label className="block text-sm font-semibold text-neutral-700 mb-2">
                            Description
                        </label>
                        <textarea
                            name="description"
                            value={formData.description}
                            onChange={handleInputChange}
                            rows="3"
                            className="w-full px-4 py-3 border border-neutral-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-orange-400"
                            placeholder="Enter banner description"
                        />
                    </div>

                    {/* Link URL */}
                    <div>
                        <label className="block text-sm font-semibold text-neutral-700 mb-2">
                            Link URL
                        </label>
                        <input
                            type="url"
                            name="linkUrl"
                            value={formData.linkUrl}
                            onChange={handleInputChange}
                            className="w-full px-4 py-3 border border-neutral-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-orange-400"
                            placeholder="https://example.com"
                        />
                        <p className="text-xs text-neutral-500 mt-1">Optional: Where should users go when clicking this banner?</p>
                    </div>

                    {/* Display Order */}
                    <div>
                        <label className="block text-sm font-semibold text-neutral-700 mb-2">
                            Display Order
                        </label>
                        <input
                            type="number"
                            name="displayOrder"
                            value={formData.displayOrder}
                            onChange={handleInputChange}
                            min="0"
                            className="w-full px-4 py-3 border border-neutral-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-orange-400"
                            placeholder="0"
                        />
                        <p className="text-xs text-neutral-500 mt-1">Lower numbers appear first</p>
                    </div>

                    {/* Date Range */}
                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                        <div>
                            <label className="block text-sm font-semibold text-neutral-700 mb-2">
                                Start Date
                            </label>
                            <input
                                type="date"
                                name="startDate"
                                value={formData.startDate}
                                onChange={handleInputChange}
                                className="w-full px-4 py-3 border border-neutral-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-orange-400"
                            />
                        </div>
                        <div>
                            <label className="block text-sm font-semibold text-neutral-700 mb-2">
                                End Date
                            </label>
                            <input
                                type="date"
                                name="endDate"
                                value={formData.endDate}
                                onChange={handleInputChange}
                                className="w-full px-4 py-3 border border-neutral-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-orange-400"
                            />
                        </div>
                    </div>

                    {/* Active Status */}
                    <div className="flex items-center gap-3 p-4 bg-neutral-50 rounded-xl">
                        <input
                            type="checkbox"
                            id="isActive"
                            name="isActive"
                            checked={formData.isActive}
                            onChange={handleInputChange}
                            className="w-5 h-5 text-orange-500 rounded focus:ring-2 focus:ring-orange-400"
                        />
                        <label htmlFor="isActive" className="text-sm font-semibold text-neutral-700">
                            Active (Banner will be visible on homepage)
                        </label>
                    </div>

                    {/* Action Buttons */}
                    <div className="flex gap-3 pt-4 border-t border-neutral-200">
                        <button
                            type="button"
                            onClick={onClose}
                            className="flex-1 px-6 py-3 border-2 border-neutral-300 text-neutral-700 rounded-xl font-semibold hover:bg-neutral-50 transition-all"
                        >
                            Cancel
                        </button>
                        <button
                            type="submit"
                            className="flex-1 px-6 py-3 text-white rounded-xl font-semibold shadow-lg hover:shadow-xl transition-all"
                            style={{ background: 'linear-gradient(135deg, #FF6B35 0%, #FF8C42 50%, #FFB627 100%)' }}
                        >
                            {editingItem ? 'Update Banner' : 'Create Banner'}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
}
