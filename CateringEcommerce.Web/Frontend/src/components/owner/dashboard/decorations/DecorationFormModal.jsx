/*
========================================
File: src/components/owner/dashboard/decorations/DecorationFormModal.jsx (REVISED)
========================================
*/
import React, { useState, useEffect, useCallback } from 'react';
import { useToast } from '../../../../contexts/ToastContext';
import MediaGridUploader from '../../../common/MediaGridUploader';
import MediaLightbox from '../../../common/MediaLightbox';
import ToggleSwitch from '../../../common/ToggleSwitch';
import MultiSelectDropdown from '../../../common/MultiSelectDropdown'; // Import multi-select

const RequiredAsterisk = () => <span className="text-red-500 ml-1">*</span>;
const ValidationError = ({ message }) => message ? <p className="text-red-500 text-xs mt-1">{message}</p> : null;

export default function DecorationFormModal({ isOpen, onClose, onSave, editingItem, themes, packages }) {
    const getInitialState = () => ({
        name: '', theme: '', description: '', price: 0, status: true, media: [], linkedPackageIds: []
    });

    const [formData, setFormData] = useState(getInitialState());
    const [errors, setErrors] = useState({});
    const [lightboxMedia, setLightboxMedia] = useState(null);
    const { showToast } = useToast();

    useEffect(() => {
        if (isOpen) {
            const initialMedia = editingItem?.media || [];
            // Convert linkedPackages array to an array of IDs for the form state
            const initialPackageIds = editingItem?.linkedPackages?.map(p => p.id) || [];

            setFormData(editingItem
                ? { ...editingItem, media: initialMedia, linkedPackageIds: initialPackageIds }
                : getInitialState()
            );
            setErrors({});
        }
    }, [editingItem, isOpen]);

    const validate = useCallback(() => {
        const newErrors = {};
        if (!formData.name.trim()) newErrors.name = 'Decoration Name is required.';
        if (!formData.themeId) newErrors.theme = 'Theme Type is required.';
        if (formData.media.length === 0) newErrors.media = 'At least one photo or video is required.';
        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    }, [formData]);

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));
    };

    const handleMultiSelectChange = (name, ids) => {
        setFormData(prev => ({ ...prev, [name]: ids }));
    };

    const handleToggle = (name, value) => setFormData(prev => ({ ...prev, [name]: value }));
    const handleMediaChange = (newMedia) => setFormData(prev => ({ ...prev, media: newMedia }));

    const handleSubmit = (e) => {
        e.preventDefault();
        if (validate()) {
            // The formData already contains `linkedPackageIds`
            onSave(formData);
        } else {
            showToast('Please fill all required fields correctly.', 'error');
        }
    };

    const packageOptions = packages.map(p => ({ id: p.id, name: p.name }));

    if (!isOpen) return null;

    return (
        <>
            <MediaLightbox mediaItem={lightboxMedia} onClose={() => setLightboxMedia(null)} />
            <div className="fixed inset-0 bg-black bg-opacity-60 flex justify-center items-center z-50 p-4">
                <div className="bg-white rounded-xl shadow-2xl w-full max-w-2xl max-h-[90vh] flex flex-col">
                    <h2 className="p-6 border-b text-2xl font-bold text-neutral-800">{editingItem ? 'Edit Decoration' : 'Add New Decoration'}</h2>
                    <form onSubmit={handleSubmit} className="overflow-y-auto flex-1 p-6 space-y-4">
                        <div>
                            <label htmlFor="name" className="block text-sm font-medium text-neutral-700">Decoration Name <RequiredAsterisk /></label>
                            <input type="text" name="name" id="name" value={formData.name} onChange={handleChange} autoComplete="off" className={`mt-1 block w-full px-3 py-2 border ${errors.name ? 'border-red-500' : 'border-neutral-300'} rounded-md shadow-sm focus:outline-none focus:ring-1 focus:ring-rose-500`} />
                            <ValidationError message={errors.name} />
                        </div>
                        <div>
                            <label htmlFor="description" className="block text-sm font-medium text-neutral-700">Description</label>
                            <textarea name="description" id="description" value={formData.description} onChange={handleChange} autoComplete="off" rows="3" className="mt-1 block w-full px-3 py-2 border border-neutral-300 rounded-md shadow-sm"></textarea>
                        </div>

                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <div>
                                <label htmlFor="themeId" className="block text-sm font-medium text-neutral-700">Theme Type <RequiredAsterisk /></label>
                                <select name="themeId" id="themeId" value={formData.themeId} onChange={handleChange} className={`mt-1 block w-full px-3 py-2 border ${errors.theme ? 'border-red-500' : 'border-neutral-300'} rounded-md`}>
                                    <option value="">Select a theme</option>
                                    {themes.map(t => <option key={t.themeId} value={t.themeId}>{t.themeName}</option>)}
                                </select>
                                <ValidationError message={errors.theme} />
                            </div>
                            <div>
                                <label htmlFor="price" className="block text-sm font-medium text-neutral-700">Price (₹) (Optional)</label>
                                <input type="number" name="price" id="price" value={formData.price > 0 ? formData.price : '' } onChange={handleChange} autoComplete="off" className="mt-1 block w-full px-3 py-2 border border-neutral-300 rounded-md" />
                            </div>
                        </div>

                        <div>
                            <label className="block text-sm font-medium text-neutral-700 mb-1">Linked Packages (Optional)</label>
                            <MultiSelectDropdown
                                placeholder="Select packages..."
                                options={packageOptions}
                                selectedIds={formData.linkedPackageIds}
                                onChange={(ids) => handleMultiSelectChange('linkedPackageIds', ids)}
                            />
                        </div>

                        <MediaGridUploader
                            label="Decoration & Setup Media"
                            subtext="Add images or videos that represent your decoration theme — such as counter setup, lighting style, serving area design, or dining arrangements used in real events."
                            initialMedia={formData.media || []}
                            onMediaChange={handleMediaChange}
                            onMediaClick={setLightboxMedia}
                            error={errors.media}
                        />

                        <div className="space-y-3 pt-2">
                            <ToggleSwitch label="Status" enabled={formData.status} setEnabled={(value) => handleToggle('status', value)} />
                            <p className="text-xs text-neutral-500">{formData.status ? 'This setup is active and visible to customers.' : 'This setup is inactive and hidden from customers.'}</p>
                        </div>

                    </form>
                    <div className="p-6 bg-neutral-50 border-t flex justify-end gap-3">
                        <button type="button" onClick={onClose} className="px-5 py-2 rounded-md font-semibold text-neutral-700 bg-neutral-200 hover:bg-neutral-300">Cancel</button>
                        <button type="button" onClick={handleSubmit} className="px-5 py-2 rounded-md font-semibold text-white bg-rose-600 hover:bg-rose-700">Save Setup</button>
                    </div>
                </div>
            </div>
        </>
    );
}