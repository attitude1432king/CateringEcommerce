/*
========================================
File: src/components/owner/Step3_ServicesAndMedia.jsx (UPDATED)
========================================
*/

import React, { useState, useEffect, useCallback } from 'react'; // Import useCallback
import MultiImageUploader from '../common/MultiImageUploader';
import { Tooltip } from '../common/Tooltip';
import Loader from '../common/Loader';
import { ownerApiService } from '../../services/ownerApi';

const SelectGroup = ({ title, options, selectedIds, onSelect, error }) => {
    const selectedSet = new Set(selectedIds ? selectedIds.split(',').filter(Boolean) : []);

    return (
        <div>
            <label className="block text-sm font-medium text-neutral-700 mb-2">{title} <span className="text-red-500">*</span></label>
            <div className="flex flex-wrap gap-2">
                {options.map(option => (
                    <Tooltip key={option.typeId} text={option.description}>
                        <button
                            type="button"
                            onClick={() => onSelect(option.typeId.toString())}
                            className={`px-3 py-1 text-sm rounded-full border transition-colors ${selectedSet.has(option.typeId.toString()) ? 'bg-rose-600 text-white border-rose-600' : 'bg-white text-neutral-700 border-neutral-300 hover:bg-rose-50'}`}
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

export default function Step3_ServicesAndMedia({ formData, setFormData, errors }) {
    const [isLoading, setIsLoading] = useState(true);
    const [options, setOptions] = useState({
        cuisineOptions: [],
        serviceTypeOptions: [],
        eventTypeOptions: [],
        foodTypeOptions: [],
    });

    // Effect to fetch all options once when the component mounts
    useEffect(() => {
        const fetchOptions = async () => {
            try {
                const data = await ownerApiService.getRegistrationOptions();
                setOptions(data);
            } catch (error) {
                console.error("Failed to fetch registration options:", error);
                // Handle error state if needed
            } finally {
                setIsLoading(false);
            }
        };

        fetchOptions();
    }, []); // Empty dependency array ensures this runs only once

    const handleMultiSelectChange = (fieldName, typeId) => {
        const currentValues = formData[fieldName] ? formData[fieldName].split(',') : [];
        const newValues = currentValues.includes(typeId)
            ? currentValues.filter(v => v !== typeId)
            : [...currentValues, typeId];
        setFormData({ ...formData, [fieldName]: newValues.join(',') });
    };

    const handleFilesChange = useCallback((files) => {
        setFormData(prevFormData => ({ ...prevFormData, cateringMedia: files }));
    }, [setFormData]);

    const mediaUploadInfo = "Showcase your best work! Upload high-quality photos or videos of your kitchen, staff in action, and signature dishes. This is your chance to attract customers. Minimum 5, Maximum 10 files.";

    if (isLoading) {
        return (
            <div className="animate-fade-in flex justify-center items-center min-h-[300px]">
                <Loader text="Loading service options..." />
            </div>
        );
    }

    return (
        <div className="animate-fade-in">
            <h3 className="text-2xl font-bold text-neutral-800 mb-2">Services & Media</h3>
            <p className="text-neutral-500 text-sm mb-6">Define your offerings to attract the right customers.</p>

            <div className="space-y-6">
                <SelectGroup
                    title="Cuisine Types Offered"
                    options={options.cuisineOptions}
                    selectedIds={formData.cuisineIds}
                    onSelect={(id) => handleMultiSelectChange('cuisineIds', id)}
                    error={errors.cuisineIds}
                />
                <SelectGroup
                    title="Food Types"
                    options={options.foodTypeOptions}
                    selectedIds={formData.foodTypeIds}
                    onSelect={(id) => handleMultiSelectChange('foodTypeIds', id)}
                    error={errors.foodTypeIds}
                />
                <SelectGroup
                    title="Service Types"
                    options={options.serviceTypeOptions}
                    selectedIds={formData.serviceTypeIds}
                    onSelect={(id) => handleMultiSelectChange('serviceTypeIds', id)}
                    error={errors.serviceTypeIds}
                />
                <SelectGroup
                    title="Event Types"
                    options={options.eventTypeOptions}
                    selectedIds={formData.eventTypeIds}
                    onSelect={(id) => handleMultiSelectChange('eventTypeIds', id)}
                    error={errors.eventTypeIds}
                />
                <div>
                    <label className="block text-sm font-medium text-neutral-700 mb-1">Minimum Guest Count <span className="text-red-500">*</span></label>
                    <input type="number" name="minGuestCount" value={formData.minGuestCount || ''} onChange={(e) => setFormData({ ...formData, minGuestCount: e.target.value })} className="w-full md:w-1/2 p-2 border border-neutral-300 rounded-md" />
                    {errors.minGuestCount && <p className="text-xs text-red-600 mt-1">{errors.minGuestCount}</p>}
                </div>
                <div>
                    <div className="flex items-center gap-2 mb-2">
                        <label className="block text-sm font-medium text-neutral-700">Upload Catering Photos/Videos <span className="text-red-500">*</span></label>
                        <Tooltip text={mediaUploadInfo}>
                            <span className="text-neutral-400 cursor-help">
                                <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
                            </span>
                        </Tooltip>
                    </div>
                    <MultiImageUploader onFilesChange={handleFilesChange} initialFiles={formData.cateringMedia} />
                    {errors.cateringMedia && <p className="text-xs text-red-600 mt-1">{errors.cateringMedia}</p>}
                </div>
            </div>
        </div>
    );
}
