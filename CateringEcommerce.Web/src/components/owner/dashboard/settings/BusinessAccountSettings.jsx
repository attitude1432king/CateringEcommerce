/*
========================================
File: src/components/owner/dashboard/settings/BusinessAccountSettings.jsx (REVISED)
========================================
*/
import React, { useState, useRef, useEffect } from 'react';
import ImageCropUploader from './ImageCropUploader';
import { useToast } from '../../../../contexts/ToastContext';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL.replace(/\/$/, '');

// Helper components for validation
const RequiredAsterisk = () => <span className="text-red-500">*</span>;
const ValidationError = ({ message }) => message ? <p className="text-red-500 text-xs mt-1">{message}</p> : null;

export default function BusinessAccountSettings({ initialData, onUpdate, isSaving }) {
    const [formData, setFormData] = useState(initialData);
    const [logoPreview, setLogoPreview] = useState('');
    const [errors, setErrors] = useState({});
    const uploaderRef = useRef(null);
    const { showToast } = useToast();

    // Create a ref for the uploader component to trigger its file input

    const handleChange = (e) => {
        const { name, value } = e.target;
        const updatedFormData = { ...formData, [name]: value };
        setFormData(updatedFormData);
        validate(updatedFormData); // Validate on every change
    };

    useEffect(() => {
        // Construct the full URL for the logo when the component loads
        if (initialData.logoPath) {
            setLogoPreview(`${API_BASE_URL}${initialData.logoPath}`);
        } else {
            // A default placeholder if no logo is set
            setLogoPreview('https://placehold.co/200x200/fde2e2/9f1239?text=Logo');
        }
    }, [initialData.logoPath]);


    const validate = (currentData) => {
        const newErrors = {};
        if (!currentData.cateringName?.trim()) newErrors.cateringName = 'Catering Name is required.';
        if (!currentData.ownerName?.trim()) newErrors.ownerName = 'Owner Full Name is required.';
        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleLogoCropComplete = (croppedImageBlob) => {
        if (croppedImageBlob) {
            // Revoke the old URL to prevent memory leaks if it exists
            if (logoPreview && logoPreview.startsWith('blob:')) {
                URL.revokeObjectURL(logoPreview);
            }
            const previewUrl = URL.createObjectURL(croppedImageBlob);
            setLogoPreview(previewUrl);
            setFormData(prev => ({ ...prev, newLogoFile: croppedImageBlob }));
        }
    };

    const handleSubmit = (e) => {
        e.preventDefault();

        const initialDataForCompare = { ...initialData };
        delete initialDataForCompare.newLogoFile; // Don't compare the new file object
        const formDataForCompare = { ...formData };
        delete formDataForCompare.newLogoFile;

        const hasTextChanged = JSON.stringify(formDataForCompare) !== JSON.stringify(initialDataForCompare);
        const hasNewFile = !!formData.newLogoFile;

        if (!hasTextChanged && !hasNewFile) {
            showToast('No changes were made.', 'warning');
            return;
        }

        if (validate(formData)) {
            onUpdate(formData);
        } else {
            showToast('Please fix the errors before saving.', 'error');
        }
    };

    return (
        <form onSubmit={handleSubmit} className="bg-white p-6 sm:p-8 rounded-xl shadow-sm">
            <h2 className="text-xl font-bold text-neutral-800 mb-6">Business & Account Details</h2>

            <div className="flex flex-col items-center sm:items-start sm:flex-row gap-6 mb-8">
                {logoPreview && (
                    <img
                        src={logoPreview}
                        alt="Catering Logo"
                        className="w-32 h-32 rounded-full object-cover border-4 border-neutral-100"
                    />
                )}
                <div className="text-center sm:text-left">
                    <h3 className="font-bold text-neutral-700">Catering Logo</h3>
                    <p className="text-sm text-neutral-500 mt-1">Upload a high-quality logo (JPG, PNG).<br />Recommended size: 400x400 pixels.</p>

                    <ImageCropUploader
                        ref={uploaderRef}
                        onCropComplete={handleLogoCropComplete}
                        aspect={1}
                    />

                    <button type="button" onClick={() => uploaderRef.current?.triggerFileSelect()} className="mt-3 text-sm font-semibold text-rose-600 hover:text-rose-800">
                        Change Logo
                    </button>
                </div>
            </div>

                {/* Form Fields */}
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 pt-4">
                    <div>
                        <label htmlFor="cateringName" className="block text-sm font-medium text-neutral-700">Catering Name <RequiredAsterisk /></label>
                        <input type="text" name="cateringName" id="cateringName" value={formData.cateringName} onChange={handleChange} autoComplete="off" className={`mt-1 block w-full px-3 py-2 border ${errors.cateringName ? 'border-red-500' : 'border-neutral-300'} rounded-md shadow-sm focus:outline-none focus:ring-rose-500 focus:border-rose-500`} />
                        <ValidationError message={errors.cateringName} />
                    </div>
                    <div>
                        <label htmlFor="ownerName" className="block text-sm font-medium text-neutral-700">Owner Full Name <RequiredAsterisk /></label>
                        <input type="text" name="ownerName" id="ownerName" value={formData.ownerName} onChange={handleChange} autoComplete="off" className={`mt-1 block w-full px-3 py-2 border ${errors.ownerName ? 'border-red-500' : 'border-neutral-300'} rounded-md shadow-sm focus:outline-none focus:ring-rose-500 focus:border-rose-500`} />
                        <ValidationError message={errors.ownerName} />
                    </div>
                    <div className="flex gap-2">
                        <div className="w-1/3">
                            <label htmlFor="stdNumber" className="block text-sm font-medium text-neutral-700">STD</label>
                            <input type="text" name="stdNumber" id="stdNumber" value={formData.stdNumber} onChange={handleChange} autoComplete="off" className="mt-1 w-full p-2 border border-neutral-300 rounded-md" />
                        </div>
                        <div className="w-2/3">
                            <label htmlFor="cateringNumber" className="block text-sm font-medium text-neutral-700">Catering Number</label>
                            <input type="tel" name="cateringNumber" id="cateringNumber" value={formData.cateringNumber} disabled onChange={handleChange} className="mt-1 w-full p-2 border border-neutral-300 rounded-md bg-neutral-100 cursor-not-allowed" />
                        </div>
                    </div>
                    <div>
                        <label htmlFor="whatsAppNumber" className="block text-sm font-medium text-neutral-700">WhatsApp Number</label>
                        <input type="tel" name="whatsAppNumber" id="whatsAppNumber" value={formData.whatsAppNumber} onChange={handleChange} className="mt-1 w-full p-2 border border-neutral-300 rounded-md" />
                    </div>
                    <div>
                        <label htmlFor="supportEmail" className="block text-sm font-medium text-neutral-700">Support Alternate Email</label>
                        <input type="email" name="supportEmail" id="supportEmail" value={formData.supportEmail} onChange={handleChange} autoComplete="off" className="mt-1 w-full p-2 border border-neutral-300 rounded-md" />
                    </div>
                    <div>
                        <label htmlFor="mobile" className="block text-sm font-medium text-neutral-700">Owner Number (Read-only)</label>
                        <input type="tel" name="mobile" id="mobile" value={formData.phone} disabled className="mt-1 w-full p-2 border border-neutral-300 rounded-md bg-neutral-100 cursor-not-allowed" />
                    </div>
                    <div>
                        <label htmlFor="email" className="block text-sm font-medium text-neutral-700">Email (Read-only)</label>
                        <input type="email" name="email" id="email" value={formData.email} disabled className="mt-1 w-full p-2 border border-neutral-300 rounded-md bg-neutral-100 cursor-not-allowed" />
                    </div>
                </div>

            <div className="mt-8 pt-5 border-t border-neutral-200 text-right">
                <button type="submit" /* ... */ >
                    {isSaving ? 'Saving...' : 'Save Changes'}
                </button>
            </div>
        </form>
    );
}