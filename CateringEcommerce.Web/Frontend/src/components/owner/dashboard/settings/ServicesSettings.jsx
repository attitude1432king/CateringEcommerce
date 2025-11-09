/*
========================================
File: src/components/owner/dashboard/settings/ServicesSettings.jsx (REVISED)
========================================
*/
import React, { useState, useRef, useCallback, useEffect } from 'react';
import Loader from '../../../common/Loader';
import { Tooltip } from '../../../common/Tooltip';
import { ownerApiService } from '../../../../services/ownerApi';
import { useToast } from '../../../../contexts/ToastContext';
import MediaLightbox from '../../../common/MediaLightbox';


const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;


// Reusing helper components
const RequiredAsterisk = () => <span className="text-red-500">*</span>;
const ValidationError = ({ message }) => message ? <p className="text-red-500 text-xs mt-1">{message}</p> : null;

// New Reusable Multi-Select Component
const SelectGroup = ({ title, options, selectedIds = [], onSelectionChange, error }) => {
    const selectedSet = new Set(selectedIds);

    return (
        <div>
            <label className="block text-sm font-medium text-neutral-700 mb-2">{title} <RequiredAsterisk /></label>
            <div className="flex flex-wrap gap-2">
                {options.map(option => (
                    <Tooltip key={option.typeId} text={option.description}>
                        <button
                            type="button"
                            onClick={() => onSelectionChange(option.typeId.toString())}
                            className={`px-3 py-1 text-sm rounded-full border transition-colors ${selectedSet.has(option.typeId) ? 'bg-rose-600 text-white border-rose-600' : 'bg-white text-neutral-700 border-neutral-300 hover:bg-rose-50'}`}
                        >
                            {option.serviceName}
                        </button>
                    </Tooltip>
                ))}
            </div>
            <ValidationError message={error} />
        </div>
    );
};


// Media Uploader Component
const MediaSettingsUploader = ({ initialMedia = [], onUpdateMedia, error, onMediaClick }) => {
    const [mediaFiles, setMediaFiles] = useState(initialMedia);
    const fileInputRef = useRef(null);

    useEffect(() => {
        if (!initialMedia || initialMedia.length === 0) return;

        const formattedMedia = initialMedia.map(media => ({
            ...media,
            url: media.url || `${API_BASE_URL}${media.filePath}`, // Add .url for DB media
        }));

        setMediaFiles(formattedMedia);
    }, [initialMedia]);

    const handleFileChange = (event) => {
        const files = Array.from(event.target.files);
        if (files.length === 0) return;

        const newFiles = files.map(file => {
            const extension = '.' + file.name.split('.').pop().toLowerCase();

            return {
                id: `new_${Date.now()}_${Math.random()}`,
                filePath: URL.createObjectURL(file),  // Temporary path (used in UI)
                url: URL.createObjectURL(file),       // ?? Key point: Set .url here for UI binding
                mediaType: extension,
                fileName: file.name,
                documentType: 0,
                fileObject: file
            };
        });

        const updatedMedia = [...mediaFiles, ...newFiles];
        setMediaFiles(updatedMedia);
        onUpdateMedia(updatedMedia); // If needed
    };


    const handleRemoveMedia = (idToRemove) => {
        const updatedMedia = mediaFiles.filter(media => media.id !== idToRemove);
        setMediaFiles(updatedMedia);
        onUpdateMedia(updatedMedia);
    };

    return (
        <div>
            <h3 className="text-lg font-bold text-neutral-800 mb-3">Kitchen & Staff Media <RequiredAsterisk /></h3>
            <p className="text-sm text-neutral-500 mb-4">Showcase your kitchen's hygiene and your team's professionalism. High-quality photos and videos build trust with customers.</p>
            <div className="p-4 border rounded-lg bg-neutral-50">
                {/* FIX: Replaced fixed column grid with a responsive auto-filling grid */}
                <div className="grid grid-cols-[repeat(auto-fill,minmax(120px,1fr))] gap-4">
                    {mediaFiles.map(media => (
                        <div key={media.id} className="relative aspect-square group bg-neutral-100 rounded-lg">
                            {ownerApiService.isImageType(media.mediaType) ? (
                                <img src={media.url} alt="Kitchen media" className="w-full h-full object-cover rounded-lg" />
                            ) : (
                                <video src={media.url} className="w-full h-full object-cover rounded-lg" />
                            )}
                            <div className="absolute inset-0 bg-black bg-opacity-0 group-hover:bg-opacity-40 transition-all duration-300 rounded-lg flex items-center justify-center">
                                {/* Fullscreen button */}
                                <button type="button" onClick={() => onMediaClick(media)} className="text-white opacity-0 group-hover:opacity-100 transition-opacity p-2">
                                    <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 8V4m0 0h4M4 4l5 5m11-1V4m0 0h-4m4 0l-5 5M4 16v4m0 0h4m-4 0l5-5m11 1v4m0 0h-4m4 0l-5-5" /></svg>
                                </button>
                                {/* Remove button */}
                                <button type="button" onClick={() => handleRemoveMedia(media.id)} className="absolute top-1 right-1 bg-black bg-opacity-50 text-white rounded-full p-1 opacity-0 group-hover:opacity-100 transition-opacity">
                                    <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" /></svg>
                                </button>
                            </div>
                        </div>
                    ))}
                    <button type="button" onClick={() => fileInputRef.current.click()} className="flex flex-col items-center justify-center aspect-square border-2 border-dashed rounded-lg text-neutral-400 hover:bg-neutral-100 hover:border-rose-500 hover:text-rose-600 transition-colors">
                        <svg xmlns="http://www.w3.org/2000/svg" className="h-8 w-8" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" /></svg>
                        <span className="text-xs mt-1 font-medium">Add Photos/Videos</span>
                    </button>
                </div>
                <input type="file" ref={fileInputRef} onChange={handleFileChange} multiple className="hidden" accept="image/png, image/jpeg, video/mp4" />
            </div>
            <ValidationError message={error} />
        </div>
    );
};

export default function ServicesSettings({ initialData, onUpdate, isSaving }) {
    const [formData, setFormData] = useState(initialData);
    const [options, setOptions] = useState({
        cuisineTypes: [],
        foodTypes: [],
        serviceTypes: [],
        eventTypes: [],
        servingSlots: []
    });
    const [isLoading, setIsLoading] = useState(true);
    const { showToast } = useToast();
    const [errors, setErrors] = useState({});
    const [lightboxMedia, setLightboxMedia] = useState(null); 

    const validate = (currentData) => {
        const newErrors = {};
        if (currentData.cuisineTypeIds?.length === 0) newErrors.cuisineTypeIds = 'Please select at least one cuisine type.';
        if (currentData.foodTypeIds?.length === 0) newErrors.foodTypeIds = 'Please select at least one food type.';
        if (currentData.serviceTypeIds?.length === 0) newErrors.serviceTypeIds = 'Please select at least one service type.';
        if (currentData.eventTypeIds?.length === 0) newErrors.eventTypeIds = 'Please select at least one event type.';
        if (currentData.servingSlots?.length === 0) newErrors.servingSlots = 'Please select at least one serving slot.';
        if (!currentData.minOrderValue || currentData.minOrderValue <= 0) newErrors.minOrderValue = 'Minimum order value is required.';
        if (!currentData.deliveryRediusKm || currentData.deliveryRediusKm <= 0) newErrors.deliveryRediusKm = 'Delivery radius is required.';
        if (currentData.kitchenMedia?.length < 5) newErrors.kitchenMedia = 'Minimum 5 photos/videos are required.';
        if (currentData.kitchenMedia?.length > 10) newErrors.kitchenMedia = 'Maximum 10 photos/videos are allowed.';
        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    useEffect(() => {
        const fetchOptions = async () => {
            setIsLoading(true);
            try {
                // In a real app, you would fetch this from your API
                const apiOptions = await ownerApiService.getRegistrationOptions();
                setOptions(prev => ({
                    ...prev,
                    cuisineTypes: apiOptions.cuisineOptions,
                    foodTypes: apiOptions.foodTypeOptions,
                    serviceTypes: apiOptions.serviceTypeOptions,
                    eventTypes: apiOptions.eventTypeOptions,
                    servingSlots: apiOptions.servingSlotTypeOptions,
                }));
            } catch (error) {
                console.error("Failed to fetch service options", error);
            } finally {
                setIsLoading(false);
            }
        };
        fetchOptions();
    }, []);

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));
    };

    const handleSelectionChange = (fieldName, typeId) => {
        const typeIdInt = Number(typeId); // Ensure it's an integer
        const currentValues = Array.isArray(formData[fieldName]) ? formData[fieldName] : [];

        const newValues = currentValues.includes(typeIdInt)
            ? currentValues.filter(v => v !== typeIdInt)
            : [...currentValues, typeIdInt];

        setFormData({ ...formData, [fieldName]: newValues });

    };

    const handleMediaUpdate = useCallback((media) => {
        setFormData(prev => ({ ...prev, kitchenMedia: media }));
    }, []);

    const handleSubmit = (e) => {
        e.preventDefault();

        const hasChanged = JSON.stringify(formData) !== JSON.stringify(initialData);
        if (!hasChanged) {
            showToast('No changes were made.', 'warning');
            return;
        }

        if (validate(formData)) {
            onUpdate(formData);
        } else {
            showToast('Please fix the errors before saving.', 'error');
        }
    };

    if (isLoading) {
        return <div className="flex justify-center items-center h-64"><Loader /></div>;
    }

    return (
        <>
            <MediaLightbox mediaItem={lightboxMedia} onClose={() => setLightboxMedia(null)} />
            <form onSubmit={handleSubmit} className="bg-white p-6 sm:p-8 rounded-xl shadow-sm">
                <div className="space-y-10">
                    <div className="space-y-6">
                        <h3 className="text-xl font-semibold text-neutral-800">Service Offerings</h3>

                        <SelectGroup
                            title="Cuisine Types Offered"
                            options={options.cuisineTypes}
                            selectedIds={formData.cuisineTypeIds || []}
                            onSelectionChange={(ids) => handleSelectionChange('cuisineTypeIds', ids)}
                            error={errors?.cuisineTypeIds}
                        />

                        <SelectGroup
                            title="Food Types"
                            options={options.foodTypes}
                            selectedIds={formData.foodTypeIds || []}
                            onSelectionChange={(ids) => handleSelectionChange('foodTypeIds', ids)}
                            error={errors?.foodTypeIds}
                        />

                        <SelectGroup
                            title="Service Types"
                            options={options.serviceTypes}
                            selectedIds={formData.serviceTypeIds || []}
                            onSelectionChange={(ids) => handleSelectionChange('serviceTypeIds', ids)}
                            error={errors?.serviceTypeIds}
                        />

                        <SelectGroup
                            title="Event Types"
                            options={options.eventTypes}
                            selectedIds={formData.eventTypeIds || []}
                            onSelectionChange={(ids) => handleSelectionChange('eventTypeIds', ids)}
                            error={errors?.eventTypeIds}
                        />

                        <SelectGroup
                            title="Serving Slots"
                            options={options.servingSlots}
                            selectedIds={formData.servingSlots || []}
                            onSelectionChange={(ids) => handleSelectionChange('servingSlots', ids)}
                            error={errors?.servingSlots}
                        />

                        <div>
                            <label htmlFor="minOrderValue" className="block text-sm font-medium text-neutral-700 mb-1">Minimum Order Value (?) <RequiredAsterisk /></label>
                            <input type="number" name="minOrderValue" id="minOrderValue" value={formData.minOrderValue || ''} onChange={handleChange} className="w-full p-2 border border-neutral-300 rounded-md" />
                            <ValidationError message={errors.minOrderValue} />
                        </div>

                        <div>
                            <label htmlFor="deliveryRediusKm" className="block text-sm font-medium text-neutral-700 mb-1">Delivery Radius (in KM) <RequiredAsterisk /></label>
                            <input type="number" name="deliveryRediusKm" id="deliveryRediusKm" value={formData.deliveryRediusKm || ''} onChange={handleChange} className="w-full p-2 border border-neutral-300 rounded-md" />
                            <ValidationError message={errors.deliveryRediusKm} />
                        </div>
                    </div>


                    <div className="pt-4 border-t border-neutral-200">
                        <MediaSettingsUploader
                            initialMedia={initialData.kitchenMedia || []}
                            onUpdateMedia={handleMediaUpdate}
                            onMediaClick={(mediaFiles) => setLightboxMedia(mediaFiles)} // Pass handler to open lightbox
                            error={errors.kitchenMedia}
                        />
                    </div>

                    <div className="mt-8 pt-5 border-t border-neutral-200 text-right">
                        <button type="submit" /* ... */ >
                            {isSaving ? 'Saving...' : 'Save Changes'}
                        </button>
                    </div>
                </div>
            </form>
        </>
    );
}