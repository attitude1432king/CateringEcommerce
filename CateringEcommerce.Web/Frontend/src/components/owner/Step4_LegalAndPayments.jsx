/*
========================================
File: src/components/owner/Step4_LegalAndPayments.jsx (HEAVILY REVISED)
========================================
*/
import React, { useState } from 'react';
import FileUploader from '../common/FileUploader'; // Import the new component

// Reusable component for displaying the uploaded file preview
const FilePreview = ({ file, onRemove }) => {
    if (!file) return null;
    const isImage = file.type.startsWith('image/');
    return (
        <div className="mt-2 border rounded-lg p-2 flex items-center justify-between bg-green-50">
            <div className="flex items-center gap-3">
                {isImage ? (
                    <img src={file.base64} alt="preview" className="w-10 h-10 object-cover rounded-md" />
                ) : (
                    <div className="w-10 h-10 bg-red-100 text-red-600 flex items-center justify-center rounded-md font-bold text-xs">PDF</div>
                )}
                <span className="text-sm text-neutral-700 truncate">{file.name}</span>
            </div>
            <button type="button" onClick={onRemove} className="text-neutral-500 hover:text-red-600">&times;</button>
        </div>
    );
};

export default function Step4_LegalAndPayments({ formData, setFormData, errors }) {
    const handleChange = (e) => {
        const { name, value, type, checked } = e.target;
        setFormData({ ...formData, [name]: type === 'checkbox' ? checked : value });
    };

    const handleFileChange = (fieldName, fileData) => {
        setFormData({ ...formData, [fieldName]: fileData });
    };

    return (
        <div className="animate-fade-in">
            <h3 className="text-2xl font-bold text-neutral-800 mb-2">Legal & Payments</h3>
            <p className="text-neutral-500 text-sm mb-6">Finally, let's get your legal and bank details for smooth transactions.</p>

            <div className="space-y-8">
                {/* FSSAI Section */}
                <section className="p-4 border rounded-lg bg-neutral-50">
                    <div className="bg-amber-100 border-l-4 border-amber-500 text-amber-800 p-4 rounded-md mb-4">
                        <h4 className="font-bold">Valid FSSAI License Required</h4>
                        <ul className="list-disc list-inside text-sm mt-1">
                            <li>14-digit FSSAI License Number</li>
                            <li>Business Name must match your Catering Name</li>
                            <li>License must be active and not expired</li>
                        </ul>
                    </div>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div>
                            <label className="block text-sm font-medium text-neutral-700 mb-1">FSSAI Number</label>
                            <input type="text" name="fssaiNumber" value={formData.fssaiNumber || ''} onChange={handleChange} className="w-full p-2 border border-neutral-300 rounded-md" />
                            {errors.fssaiNumber && <p className="text-xs text-red-600 mt-1">{errors.fssaiNumber}</p>}
                        </div>
                        <div>
                            <label className="block text-sm font-medium text-neutral-700 mb-1">FSSAI Expiry Date</label>
                            <input type="date" name="fssaiExpiry" value={formData.fssaiExpiry || ''} onChange={handleChange} className="w-full p-2 border border-neutral-300 rounded-md" />
                            {errors.fssaiExpiry && <p className="text-xs text-red-600 mt-1">{errors.fssaiExpiry}</p>}
                        </div>
                        <div className="md:col-span-2">
                            <label className="block text-sm font-medium text-neutral-700 mb-1">Upload FSSAI Certificate (PDF)</label>
                            <div className="h-16 border rounded-md flex items-center justify-center">
                                <FileUploader onFileCropped={(file) => handleFileChange('fssaiCertificate', file)} acceptedTypes="application/pdf" />
                            </div>
                            <FilePreview file={formData.fssaiCertificate} onRemove={() => handleFileChange('fssaiCertificate', null)} />
                            {errors.fssaiCertificate && <p className="text-xs text-red-600 mt-1">{errors.fssaiCertificate}</p>}

                        </div>
                    </div>
                </section>

                {/* GST Section */}
                <section className="p-4 border rounded-lg bg-neutral-50">
                    <div className="bg-amber-100 border-l-4 border-amber-500 text-amber-800 p-4 rounded-md mb-4">
                        <h4 className="font-bold">GST Information</h4>
                        <p className="text-sm mt-1">As per government regulations, providing a valid GST number is mandatory for all partners on e-commerce platforms. Ensure your business name and address on the certificate match your registration details.</p>
                    </div>
                    <div className="flex items-center mb-4">
                        <input type="checkbox" name="isGstApplicable" id="isGstApplicable" checked={formData.isGstApplicable || false} onChange={handleChange} className="h-4 w-4 text-rose-600" />
                        <label htmlFor="isGstApplicable" className="ml-2 block text-sm font-medium text-neutral-700">I have a GST Number</label>
                    </div>
                    {formData.isGstApplicable && (
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 animate-fade-in">
                            <div>
                                <label className="block text-sm font-medium text-neutral-700 mb-1">GST Number <span className="text-red-500">*</span></label>
                                <input type="text" name="gstNumber" value={formData.gstNumber || ''} onChange={handleChange} className="w-full p-2 border border-neutral-300 rounded-md" />
                                {errors.gstNumber && <p className="text-xs text-red-600 mt-1">{errors.gstNumber}</p>}
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-neutral-700 mb-1">Upload GST Certificate (PDF) <span className="text-red-500">*</span></label>
                                <div className="h-16 border rounded-md flex items-center justify-center">
                                    <FileUploader onFileCropped={(file) => handleFileChange('gstCertificate', file)} acceptedTypes="application/pdf" />
                                </div>
                                <FilePreview file={formData.gstCertificate} onRemove={() => handleFileChange('gstCertificate', null)} />
                                {errors.gstCertificate && <p className="text-xs text-red-600 mt-1">{errors.gstCertificate}</p>}
                            </div>
                        </div>
                    )}
                </section>

                {/* PAN & Bank Details Section */}
                <section className="p-4 border rounded-lg bg-neutral-50">
                    <div className="bg-amber-100 border-l-4 border-amber-500 text-amber-800 p-4 rounded-md mb-4">
                        <h4 className="font-bold">PAN & Bank Verification</h4>
                        <p className="text-sm mt-1">Please provide valid PAN and bank details for payment processing. The name on the PAN card and bank account must match the registered owner or business name.</p>
                    </div>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div>
                            <label className="block text-sm font-medium text-neutral-700 mb-1">PAN Holder Name <span className="text-red-500">*</span></label>
                            <input type="text" name="panHolderName" value={formData.panHolderName || ''} onChange={handleChange} className="w-full p-2 border border-neutral-300 rounded-md" />
                            {errors.panHolderName && <p className="text-xs text-red-600 mt-1">{errors.panHolderName}</p>}
                        </div>
                        <div>
                            <label className="block text-sm font-medium text-neutral-700 mb-1">PAN Number <span className="text-red-500">*</span></label>
                            <input type="text" name="panNumber" value={formData.panNumber || ''} onChange={handleChange} className="w-full p-2 border border-neutral-300 rounded-md" />
                            {errors.panNumber && <p className="text-xs text-red-600 mt-1">{errors.panNumber}</p>}
                        </div>
                        <div className="md:col-span-2">
                            <label className="block text-sm font-medium text-neutral-700 mb-1">Upload PAN Card (Image or PDF) <span className="text-red-500">*</span></label>
                            <div className="h-16 border rounded-md flex items-center justify-center">
                                <FileUploader onFileCropped={(file) => handleFileChange('panCard', file)} aspect={1.586} />
                            </div>
                            <FilePreview file={formData.panCard} onRemove={() => handleFileChange('panCard', null)} />
                            {errors.panCard && <p className="text-xs text-red-600 mt-1">{errors.panCard}</p>}
                        </div>
                        <hr className="md:col-span-2 my-2" />
                        <div>
                            <label className="block text-sm font-medium text-neutral-700 mb-1">Account Holder Name <span className="text-red-500">*</span></label>
                            <input type="text" name="bankAccountName" value={formData.bankAccountName || ''} onChange={handleChange} className="w-full p-2 border border-neutral-300 rounded-md" />
                            {errors.bankAccountName && <p className="text-xs text-red-600 mt-1">{errors.bankAccountName}</p>}
                        </div>
                        <div>
                            <label className="block text-sm font-medium text-neutral-700 mb-1">Bank Account Number <span className="text-red-500">*</span></label>
                            <input type="text" name="bankAccountNumber" value={formData.bankAccountNumber || ''} onChange={handleChange} className="w-full p-2 border border-neutral-300 rounded-md" />
                            {errors.bankAccountNumber && <p className="text-xs text-red-600 mt-1">{errors.bankAccountNumber}</p>}
                        </div>
                        <div>
                            <label className="block text-sm font-medium text-neutral-700 mb-1">IFSC Code <span className="text-red-500">*</span></label>
                            <input type="text" name="ifscCode" value={formData.ifscCode || ''} onChange={handleChange} className="w-full p-2 border border-neutral-300 rounded-md" />
                            {errors.ifscCode && <p className="text-xs text-red-600 mt-1">{errors.ifscCode}</p>}
                        </div>
                    </div>
                </section>
            </div>
        </div>
    );
}