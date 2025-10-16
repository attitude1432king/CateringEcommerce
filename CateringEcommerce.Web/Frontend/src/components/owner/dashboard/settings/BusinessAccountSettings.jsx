/*
========================================
File: src/components/owner/dashboard/settings/BusinessAccountSettings.jsx (REVISED)
========================================
*/
import React, { useState, useRef, useEffect } from 'react';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export default function BusinessAccountSettings({ initialData, onUpdate }) {
    const [formData, setFormData] = useState(initialData);
    const [logoPreview, setLogoPreview] = useState('');
    const fileInputRef = useRef(null);

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));
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

    const handleLogoChange = (e) => {
        const file = e.target.files[0];
        if (file) {
            // For a new upload, create a temporary local URL for immediate preview
            const previewUrl = URL.createObjectURL(file);
            setLogoPreview(previewUrl);
            setFormData(prev => ({ ...prev, newLogoFile: file })); // Store the new file object separately
        }
    };

    const handleSubmit = (e) => {
        e.preventDefault();
        onUpdate(formData);
    };

    return (
        <form onSubmit={handleSubmit} className="bg-white p-6 sm:p-8 rounded-xl shadow-sm">
            <h3 className="text-xl font-semibold text-neutral-800 mb-6">Business & Account Details</h3>

            <div className="space-y-6">
                {/* Catering Logo */}
                <div className="flex flex-col items-center sm:flex-row gap-6">
                    <img
                        src={logoPreview}
                        alt="Catering Logo"
                        className="w-32 h-32 rounded-full object-cover border-4 border-neutral-100"
                    />
                    <div className="text-center sm:text-left">
                        <h4 className="font-semibold text-neutral-700">Catering Logo</h4>
                        <p className="text-xs text-neutral-500 mt-1 mb-2">Upload a high-quality logo (JPG, PNG). Recommended size: 400x400 pixels.</p>
                        <button
                            type="button"
                            onClick={() => fileInputRef.current.click()}
                            className="text-sm font-medium text-rose-600 hover:text-rose-800"
                        >
                            Change Logo
                        </button>
                        <input
                            type="file"
                            ref={fileInputRef}
                            onChange={handleLogoChange}
                            accept="image/png, image/jpeg"
                            className="hidden"
                        />
                    </div>
                </div>

                {/* Form Fields */}
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 pt-4">
                    <div>
                        <label htmlFor="cateringName" className="block text-sm font-medium text-neutral-700">Catering Name</label>
                        <input type="text" name="cateringName" id="cateringName" value={formData.cateringName} onChange={handleChange} className="mt-1 w-full p-2 border border-neutral-300 rounded-md" />
                    </div>
                    <div>
                        <label htmlFor="ownerName" className="block text-sm font-medium text-neutral-700">Owner Full Name</label>
                        <input type="text" name="ownerName" id="ownerName" value={formData.ownerName} onChange={handleChange} className="mt-1 w-full p-2 border border-neutral-300 rounded-md" />
                    </div>
                    <div className="flex gap-2">
                        <div className="w-1/3">
                            <label htmlFor="stdCode" className="block text-sm font-medium text-neutral-700">STD</label>
                            <input type="text" name="stdCode" id="stdCode" value={formData.stdNumber} onChange={handleChange} className="mt-1 w-full p-2 border border-neutral-300 rounded-md" />
                        </div>
                        <div className="w-2/3">
                            <label htmlFor="cateringNumber" className="block text-sm font-medium text-neutral-700">Catering Number</label>
                            <input type="tel" name="cateringNumber" id="cateringNumber" value={formData.cateringNumber} onChange={handleChange} className="mt-1 w-full p-2 border border-neutral-300 rounded-md" />
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

                <div className="pt-4 text-right">
                    <button type="submit" className="bg-rose-600 text-white px-5 py-2 rounded-md font-medium hover:bg-rose-700">
                        Save Changes
                    </button>
                </div>
            </div>
        </form>
    );
}