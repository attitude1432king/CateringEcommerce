/*
========================================
File: src/components/owner/dashboard/settings/LegalPaymentSettings.jsx (REVISED)
========================================
*/
import React, { useState } from 'react';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

// A simple component to render a file input row
const FileInputRow = ({ label, fileName, onFileChange }) => (
    <div>
        <label className="block text-sm font-medium text-neutral-700">{label}</label>
        <div className="mt-1 flex items-center justify-between p-2 border border-neutral-300 rounded-md">
            <span className="text-sm text-neutral-600 truncate pr-2">{fileName || 'No file selected'}</span>
            <label htmlFor={label.replace(/\s+/g, '-')} className="cursor-pointer text-sm font-medium text-rose-600 hover:text-rose-800">
                Change
                <input id={label.replace(/\s+/g, '-')} type="file" className="sr-only" onChange={onFileChange} />
            </label>
        </div>
    </div>
);

// A view-only row for the PAN card
const ViewOnlyFileRow = ({ label, fileName }) => (
    <div>
        <label className="block text-sm font-medium text-neutral-700">{label}</label>
        <div className="mt-1 flex items-center justify-between p-2 border border-neutral-300 rounded-md bg-neutral-50">
            <span className="text-sm text-neutral-600 truncate pr-2">{fileName}</span>
            <button type="button" className="text-sm font-medium text-rose-600 hover:text-rose-800">
                View
            </button>
        </div>
    </div>
);

// This component is now for viewing existing secure documents and providing a link.
const ViewSecureFileRow = ({ label, filePath }) => {
    if (!filePath) {
        return (
            <div>
                <label className="block text-sm font-medium text-neutral-700">{label}</label>
                <div className="mt-1 flex items-center justify-between p-2 border border-neutral-300 rounded-md bg-neutral-50">
                    <span className="text-sm text-neutral-500 italic">No document uploaded</span>
                </div>
            </div>
        );
    }

    // Construct the full URL to the secure document
    const fileUrl = `${API_BASE_URL}${filePath}`;
    const fileName = filePath.split('/').pop();

    return (
        <div>
            <label className="block text-sm font-medium text-neutral-700">{label}</label>
            <div className="mt-1 flex items-center justify-between p-2 border border-neutral-300 rounded-md bg-neutral-50">
                <span className="text-sm text-neutral-600 truncate pr-2">{fileName}</span>
                <a
                    href={fileUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-sm font-medium text-rose-600 hover:text-rose-800"
                >
                    View
                </a>
            </div>
        </div>
    );
};


export default function LegalPaymentSettings({ initialData, onUpdate, isSaving }) {
    const [formData, setFormData] = useState(initialData);
    const [isVerifying, setIsVerifying] = useState(false);
    const [isVerified, setIsVerified] = useState(true); // Assume already verified on load

    const handleVerifyUpi = () => {
        if (!formData.upiId) return;
        setIsVerifying(true);
        // Simulate API call
        setTimeout(() => {
            setIsVerifying(false);
            setIsVerified(true);
            alert("UPI ID Verified Successfully!");
        }, 1500);
    };

    const handleChange = (e) => {
        const { name, value, type, checked } = e.target;

        if (name === "upiId") {
            setIsVerified(false); // Reset verification status on change
        }

        setFormData(prev => ({
            ...prev,
            [name]: type === 'checkbox' ? checked : value
        }));
    };

    function formatDateToYMD(dateString) {
        const date = new Date(dateString);

        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0'); // Months are 0-based
        const day = String(date.getDate()).padStart(2, '0');

        return `${year}-${month}-${day}`;
    }

    const handleFileChange = (e, fieldName) => {
        const file = e.target.files[0];
        if (!file) return;
        // In a real app, you'd handle the file upload and get a new URL/name
        setFormData(prev => ({ ...prev, [`${fieldName}Name`]: file.name }));
    };

    const handleSubmit = (e) => {
        e.preventDefault();
        onUpdate(formData);
    };

    return (
        <form onSubmit={handleSubmit} className="bg-white p-6 sm:p-8 rounded-xl shadow-sm">
            <div className="space-y-10">

                {/* FSSAI Section */}
                <div className="space-y-4">
                    <h3 className="text-xl font-semibold text-neutral-800">FSSAI License</h3>
                    <div>
                        <label htmlFor="fssaiNumber" className="block text-sm font-medium text-neutral-700">FSSAI Number*</label>
                        <input type="text" name="fssaiNumber" id="fssaiNumber" value={formData.fssaiNumber} onChange={handleChange} className="mt-1 w-full p-2 border border-neutral-300 rounded-md" />
                    </div>
                    <div>
                        <label htmlFor="fssaiExpiryDate" className="block text-sm font-medium text-neutral-700">FSSAI Expiry Date*</label>
                        <input type="date" name="fssaiExpiryDate" id="fssaiExpiryDate" value={formatDateToYMD(formData.fssaiExpiryDate)} onChange={handleChange} className="mt-1 w-full p-2 border border-neutral-300 rounded-md" />
                    </div>
                    {/* <FileInputRow label="FSSAI Certificate (PDF)*" fileName={formData.fssaiCertificateName} onFileChange={(e) => handleFileChange(e, 'fssaiCertificate')} />*/}
                    <ViewSecureFileRow label="FSSAI Certificate (PDF)*" filePath={initialData.fssaiCertificatePath} />
                </div>

                {/* GST Section */}
                <div className="space-y-4">
                    <h3 className="text-xl font-semibold text-neutral-800">GST Details</h3>
                    <label className="flex items-center space-x-2">
                        <input type="checkbox" name="isGstApplicable" checked={formData.isGstApplicable} onChange={handleChange} className="h-4 w-4 rounded border-gray-300 text-rose-600 focus:ring-rose-500" />
                        <span className="text-sm text-neutral-700">Is GST Applicable?</span>
                    </label>
                    {formData.isGstApplicable && (
                        <>
                            <div>
                                <label htmlFor="gstNumber" className="block text-sm font-medium text-neutral-700">GST Number*</label>
                                <input type="text" name="gstNumber" id="gstNumber" value={formData.gstNumber} onChange={handleChange} className="mt-1 w-full p-2 border border-neutral-300 rounded-md" />
                            </div>
                            {/*<FileInputRow label="GST Certificate (PDF)*" fileName={formData.gstCertificateName} onFileChange={(e) => handleFileChange(e, 'gstCertificate')} />*/}
                            <ViewSecureFileRow label="GST Certificate (PDF)*" filePath={initialData.gstCertificatePath} />
                        </>
                    )}
                </div>

                {/* PAN Section */}
                <div className="space-y-4">
                    <h3 className="text-xl font-semibold text-neutral-800">PAN Details</h3>
                    <div>
                        <label htmlFor="panHolderName" className="block text-sm font-medium text-neutral-700">PAN Holder Name*</label>
                        <input type="text" name="panHolderName" id="panHolderName" value={formData.panHolderName} onChange={handleChange} className="mt-1 w-full p-2 border border-neutral-300 rounded-md" />
                    </div>
                    <div>
                        <label htmlFor="panNumber" className="block text-sm font-medium text-neutral-700">PAN Number*</label>
                        <input type="text" name="panNumber" id="panNumber" value={formData.panNumber} onChange={handleChange} className="mt-1 w-full p-2 border border-neutral-300 rounded-md" />
                    </div>
                    <ViewSecureFileRow label="PAN Card*" filePath={initialData.panCertificatePath} />
                </div>

                {/* Bank & Payment Section */}
                <div className="space-y-4">
                    <h3 className="text-xl font-semibold text-neutral-800">Bank & Payment Details</h3>
                    <div>
                        <label htmlFor="accountHolderName" className="block text-sm font-medium text-neutral-700">Account Holder Name*</label>
                        <input type="text" name="accountHolderName" id="accountHolderName" value={formData.accountHolderName} onChange={handleChange} className="mt-1 w-full p-2 border border-neutral-300 rounded-md" />
                    </div>
                    <div>
                        <label htmlFor="bankAccountNumber" className="block text-sm font-medium text-neutral-700">Bank Account Number*</label>
                        <input type="text" name="bankAccountNumber" id="bankAccountNumber" value={formData.bankAccountNumber} onChange={handleChange} className="mt-1 w-full p-2 border border-neutral-300 rounded-md" />
                    </div>
                    <div>
                        <label htmlFor="ifscCode" className="block text-sm font-medium text-neutral-700">IFSC Code*</label>
                        <input type="text" name="ifscCode" id="ifscCode" value={formData.ifscCode} onChange={handleChange} className="mt-1 w-full p-2 border border-neutral-300 rounded-md" />
                    </div>
                    <div>
                        <label htmlFor="upiId" className="block text-sm font-medium text-neutral-700 mb-1">UPI ID</label>
                        <div className="flex items-center gap-2">
                            <input type="text" name="upiId" id="upiId" value={formData.upiId} onChange={handleChange} className="flex-1 p-2 border border-neutral-300 rounded-md" />
                            {isVerified ? (
                                <div className="flex items-center gap-1 text-green-600">
                                    <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor"><path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" /></svg>
                                    <span className="text-sm font-medium">Verified</span>
                                </div>
                            ) : (
                                <button type="button" onClick={handleVerifyUpi} disabled={isVerifying || !formData.upiId} className="bg-white text-rose-600 border border-rose-600 px-4 py-2 rounded-md font-medium hover:bg-rose-50 text-sm disabled:opacity-50 disabled:cursor-not-allowed">
                                    {isVerifying ? 'Verifying...' : 'Verify'}
                                </button>
                            )}
                        </div>
                    </div>
                </div>

                <div className="mt-8 pt-5 border-t border-neutral-200 text-right">
                    <button type="submit" /* ... */ >
                        {isSaving ? 'Saving...' : 'Save Changes'}
                    </button>
                </div>
            </div>
        </form>
    );
}