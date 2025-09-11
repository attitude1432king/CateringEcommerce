/*
========================================
File: src/components/owner/dashboard/settings/ServicesSettings.jsx (REVISED)
========================================
*/
import React, { useState, useRef, useCallback, useEffect } from 'react';
import Loader from '../../../common/Loader';
import { Tooltip } from '../../../common/Tooltip';
import { ownerApiService } from '../../../../services/ownerApi';


const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

// New Reusable Multi-Select Component
const SelectGroup = ({ title, options, selectedIds = [], onSelectionChange, error }) => {
    const selectedSet = new Set(selectedIds);

    return (
        <div>
            <label className="block text-sm font-medium text-neutral-700 mb-2">{title} <span className="text-red-500">*</span></label>
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
            {error && <p className="text-xs text-red-600 mt-1">{error}</p>}
        </div>
    );
};

function isImageType(type) {
    const imageExtensions = ['.jpg', '.jpeg', '.png', '.gif', '.webp', '.bmp'];
    return imageExtensions.includes(type.toLowerCase());
}

// Media Uploader Component
const MediaSettingsUploader = ({ initialMedia = [], onUpdateMedia }) => {
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
            <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-4">
                {mediaFiles.map(media => (
                    <div key={media.id} className="relative aspect-square group">
                        {isImageType(media.mediaType) ? (
                            <img src={media.url} alt="Kitchen media" className="w-full h-full object-cover rounded-lg" />
                        ) : (
                            <video src={media.url} className="w-full h-full object-cover rounded-lg" controls={false} />
                        )}
                        <div className="absolute inset-0 bg-black bg-opacity-0 group-hover:bg-opacity-50 transition-all duration-300 rounded-lg flex items-center justify-center">
                            <button
                                type="button"
                                onClick={() => handleRemoveMedia(media.id)}
                                className="w-8 h-8 bg-white/80 rounded-full text-red-600 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity"
                            >
                                <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor"><path fillRule="evenodd" d="M9 2a1 1 0 00-.894.553L7.382 4H4a1 1 0 000 2v10a2 2 0 002 2h8a2 2 0 002-2V6a1 1 0 100-2h-3.382l-.724-1.447A1 1 0 0011 2H9zM7 8a1 1 0 012 0v6a1 1 0 11-2 0V8zm5-1a1 1 0 00-1 1v6a1 1 0 102 0V8a1 1 0 00-1-1z" clipRule="evenodd" /></svg>
                            </button>
                        </div>
                    </div>
                ))}
                <button
                    type="button"
                    onClick={() => fileInputRef.current.click()}
                    className="aspect-square border-2 border-dashed border-neutral-300 rounded-lg flex flex-col items-center justify-center text-neutral-500 hover:border-rose-400 hover:text-rose-600 transition-colors"
                >
                    <svg xmlns="http://www.w3.org/2000/svg" className="h-8 w-8" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1}><path strokeLinecap="round" strokeLinejoin="round" d="M12 4v16m8-8H4" /></svg>
                    <span className="text-xs font-medium mt-1">Add Photos/Videos</span>
                </button>
                <input
                    type="file"
                    multiple
                    ref={fileInputRef}
                    onChange={handleFileChange}
                    accept="image/jpeg, image/png, video/mp4"
                    className="hidden"
                />
            </div>
        </div>
    );
};

export default function ServicesSettings({ initialData, onUpdate }) {
    const [formData, setFormData] = useState(initialData);
    const [options, setOptions] = useState({
        cuisineTypes: [],
        foodTypes: [],
        serviceTypes: [],
        eventTypes: [],
        servingSlots: []
    });
    const [isLoading, setIsLoading] = useState(true);

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
        formData.deliveryRediusKm = Number(formData.deliveryRediusKm);
        onUpdate(formData);
    };

    if (isLoading) {
        return <div className="flex justify-center items-center h-64"><Loader /></div>;
    }

    return (
        <form onSubmit={handleSubmit} className="bg-white p-6 sm:p-8 rounded-xl shadow-sm">
            <div className="space-y-10">
                <div className="space-y-6">
                    <h3 className="text-xl font-semibold text-neutral-800">Service Offerings</h3>

                    <SelectGroup
                        title="Cuisine Types Offered"
                        options={options.cuisineTypes}
                        selectedIds={formData.cuisineTypeIds || []}
                        onSelectionChange={(ids) => handleSelectionChange('cuisineTypeIds', ids)}
                    />

                    <SelectGroup
                        title="Food Types"
                        options={options.foodTypes}
                        selectedIds={formData.foodTypeIds || []}
                        onSelectionChange={(ids) => handleSelectionChange('foodTypeIds', ids)}
                    />

                    <SelectGroup
                        title="Service Types"
                        options={options.serviceTypes}
                        selectedIds={formData.serviceTypeIds || []}
                        onSelectionChange={(ids) => handleSelectionChange('serviceTypeIds', ids)}
                    />

                    <SelectGroup
                        title="Event Types"
                        options={options.eventTypes}
                        selectedIds={formData.eventTypeIds || []}
                        onSelectionChange={(ids) => handleSelectionChange('eventTypeIds', ids)}
                    />

                    <SelectGroup
                        title="Serving Slots"
                        options={options.servingSlots}
                        selectedIds={formData.servingSlots || []}
                        onSelectionChange={(ids) => handleSelectionChange('servingSlots', ids)}
                    />

                    <div>
                        <label htmlFor="minOrderValue" className="block text-sm font-medium text-neutral-700 mb-1">Minimum Order Value (?)*</label>
                        <input type="number" name="minOrderValue" id="minOrderValue" value={formData.minOrderValue || ''} onChange={handleChange} className="w-full p-2 border border-neutral-300 rounded-md" />
                    </div>

                    <div>
                        <label htmlFor="deliveryRediusKm" className="block text-sm font-medium text-neutral-700 mb-1">Delivery Radius (in KM)</label>
                        <input type="number" name="deliveryRediusKm" id="deliveryRediusKm" value={formData.deliveryRediusKm || ''} onChange={handleChange} className="w-full p-2 border border-neutral-300 rounded-md" />
                    </div>
                </div>

                <div className="space-y-4">
                    <h3 className="text-xl font-semibold text-neutral-800">Kitchen & Staff Media</h3>
                    <p className="text-sm text-neutral-500">Showcase your kitchen's hygiene and your team's professionalism. High-quality photos and videos build trust with customers.</p>
                    <MediaSettingsUploader
                        initialMedia={initialData.cateringMedia || []}
                        onUpdateMedia={handleMediaUpdate}
                    />
                </div>

                <div className="pt-4 text-right">
                    <button type="submit" className="bg-rose-600 text-white px-5 py-2 rounded-md font-medium hover:bg-rose-700">
                        Save Changes
                    </button>
                </div>
            </div>
        </form>
    );
}