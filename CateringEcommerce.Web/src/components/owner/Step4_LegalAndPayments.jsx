/*
========================================
File: src/components/owner/registration/Step4_LegalAndPayments.jsx
========================================
*/
import React, { useState, useEffect } from 'react';
import FileUploader from '../common/FileUploader'; // Import the new component

// Reusable component for displaying the uploaded file preview
const FilePreview = ({ file, onRemove }) => {
    if (!file) return null;
    const isImage = file.type.startsWith('image/');
    return (
        <div className="mt-2 border rounded-lg p-2 flex items-center justify-between bg-green-50">
            <div className="flex items-center gap-3">
                {isImage ? (
                    <img src={file.previewUrl} alt="preview" className="w-10 h-10 object-cover rounded-md" />
                ) : (
                    <div className="w-10 h-10 bg-red-100 text-red-600 flex items-center justify-center rounded-md font-bold text-xs">PDF</div>
                )}
                <span className="text-sm text-neutral-700 truncate">{file.name}</span>
            </div>
            <button type="button" onClick={onRemove} className="text-neutral-500 hover:text-red-600">&times;</button>
        </div>
    );
};

export default function Step4_LegalAndPayments({ formData = {}, setFormData, errors = {} }) {
    const [isCheckingIfsc, setIsCheckingIfsc] = useState(false);
    const [bankDetails, setBankDetails] = useState(null);
    const [ifscError, setIfscError] = useState('');

    const handleChange = (e) => {
        const { name, value, type, checked } = e.target;
        setFormData({ ...formData, [name]: type === 'checkbox' ? checked : value });
    };

    const handleFileChange = (fieldName, file) => {
        if (file) {
            file.previewUrl = URL.createObjectURL(file);
        }
        setFormData({ ...formData, [fieldName]: file });
    };

    const handleInputChange = (e) => {
        const { name, value } = e.target;

        // Step 1: Force uppercase + remove invalid chars
        let formattedValue = value
            .toUpperCase()
            .replace(/[^A-Z0-9]/g, '');

        // Step 2: Apply field-specific limits
        if (name === "panNumber") {
            formattedValue = formattedValue.slice(0, 10); // PAN = 10 chars
        }

        if (name === "gstNumber") {
            formattedValue = formattedValue.slice(0, 15); // GST = 15 chars
        }

        // Step 3: Update state
        setFormData((prev) => ({
            ...prev,
            [name]: formattedValue
        }));
    };


    // Watch IFSC code and trigger API check when it reaches 11 characters
    useEffect(() => {
        const checkIfsc = async () => {
            const ifsc = formData?.ifscCode?.trim().toUpperCase();

            // Standard length for an IFSC code in India is 11
            if (!ifsc || ifsc.length !== 11) {
                setBankDetails(null);
                setIfscError('');
                return;
            }

            setIsCheckingIfsc(true);
            setIfscError('');
            setBankDetails(null);

            try {
                const response = await fetch(`https://ifsc.razorpay.com/${ifsc}`);
                if (response.ok) {
                    const data = await response.json();
                    setBankDetails(data); // Stores BANK, BRANCH, ADDRESS, etc.
                    setIfscError('');
                } else if (response.status === 404) {
                    setIfscError('Invalid IFSC Code. Please check and try again.');
                    setBankDetails(null);
                } else {
                    setIfscError('Unable to verify IFSC right now.');
                    setBankDetails(null);
                }
            } catch (error) {
                console.error("IFSC check failed:", error);
                setIfscError('Network error while verifying IFSC.');
                setBankDetails(null);
            } finally {
                setIsCheckingIfsc(false);
            }
        };

        // Debounce to prevent immediate API spam while typing the last character
        const timeoutId = setTimeout(() => {
            checkIfsc();
        }, 400);

        return () => clearTimeout(timeoutId);
    }, [formData?.ifscCode]);

    // Helper to render errors
    const renderError = (errorMsg) => {
        if (!errorMsg) return null;
        if (typeof errorMsg === 'string') {
            return <p className="text-xs text-red-600 mt-1">{errorMsg}</p>;
        }
        // If it's an object (like a React element), wrap it safely or stringify
        return <div className="text-xs text-red-600 mt-1">{errorMsg}</div>;
    }

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
                            <input type="text" name="fssaiNumber" value={formData?.fssaiNumber || ''} onChange={handleChange} className="w-full p-2 border border-neutral-300 rounded-md" />
                            {renderError(errors.fssaiNumber)}
                        </div>
                        <div>
                            <label className="block text-sm font-medium text-neutral-700 mb-1">FSSAI Expiry Date</label>
                            <input type="date" name="fssaiExpiry" value={formData?.fssaiExpiry || ''} onChange={handleChange} className="w-full p-2 border border-neutral-300 rounded-md" />
                            {renderError(errors.fssaiExpiry)}
                        </div>
                        <div className="md:col-span-2">
                            <label className="block text-sm font-medium text-neutral-700 mb-1">Upload FSSAI Certificate (PDF)</label>
                            <input
                                type="file"
                                accept="application/pdf"
                                onChange={(e) => handleFileChange('fssaiCertificate', e.target.files[0])}
                                className="block w-full text-sm text-neutral-500 file:mr-4 file:py-2 file:px-4 file:rounded-md file:border-0 file:text-sm file:font-semibold file:bg-orange-50 file:text-orange-700 hover:file:bg-orange-100"
                            />
                            <FilePreview file={formData?.fssaiCertificate} onRemove={() => handleFileChange('fssaiCertificate', null)} />
                            {renderError(errors.fssaiCertificate)}
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
                        <input type="checkbox" name="isGstApplicable" id="isGstApplicable" checked={formData?.isGstApplicable || false} onChange={handleChange} className="h-4 w-4 text-orange-500" />
                        <label htmlFor="isGstApplicable" className="ml-2 block text-sm font-medium text-neutral-700">I have a GST Number</label>
                    </div>
                    {formData?.isGstApplicable && (
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 animate-fade-in">
                            <div>
                                <label className="block text-sm font-medium text-neutral-700 mb-1">GST Number <span className="text-red-500">*</span></label>
                                <input type="text" name="gstNumber" value={formData?.gstNumber || ''} onChange={handleInputChange} className="w-full p-2 border border-neutral-300 rounded-md" />
                                {renderError(errors.gstNumber)}
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-neutral-700 mb-1">Upload GST Certificate (PDF) <span className="text-red-500">*</span></label>
                                <input
                                    type="file"
                                    accept="application/pdf"
                                    onChange={(e) => handleFileChange('gstCertificate', e.target.files[0])}
                                    className="block w-full text-sm text-neutral-500 file:mr-4 file:py-2 file:px-4 file:rounded-md file:border-0 file:text-sm file:font-semibold file:bg-orange-50 file:text-orange-700 hover:file:bg-orange-100"
                                />
                                <FilePreview file={formData?.gstCertificate} onRemove={() => handleFileChange('gstCertificate', null)} />
                                {renderError(errors.gstCertificate)}
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
                            <input type="text" name="panHolderName" value={formData?.panHolderName || ''} onChange={handleChange} className="w-full p-2 border border-neutral-300 rounded-md" />
                            {renderError(errors.panHolderName)}
                        </div>
                        <div>
                            <label className="block text-sm font-medium text-neutral-700 mb-1">PAN Number <span className="text-red-500">*</span></label>
                            <input type="text" name="panNumber" value={formData?.panNumber || ''} onChange={handleInputChange} className="w-full p-2 border border-neutral-300 rounded-md" />
                            {renderError(errors.panNumber)}
                        </div>
                        <div className="md:col-span-2">
                            <label className="block text-sm font-medium text-neutral-700 mb-1">Upload PAN Card (Image or PDF) <span className="text-red-500">*</span></label>
                            <input
                                type="file"
                                accept="image/*,application/pdf"
                                onChange={(e) => handleFileChange('panCard', e.target.files[0])}
                                className="block w-full text-sm text-neutral-500 file:mr-4 file:py-2 file:px-4 file:rounded-md file:border-0 file:text-sm file:font-semibold file:bg-orange-50 file:text-orange-700 hover:file:bg-orange-100"
                            />
                            <FilePreview file={formData?.panCard} onRemove={() => handleFileChange('panCard', null)} />
                            {renderError(errors.panCard)}
                        </div>
                        <hr className="md:col-span-2 my-2" />
                        <div>
                            <label className="block text-sm font-medium text-neutral-700 mb-1">Account Holder Name <span className="text-red-500">*</span></label>
                            <input type="text" name="bankAccountName" value={formData?.bankAccountName || ''} onChange={handleChange} className="w-full p-2 border border-neutral-300 rounded-md" />
                            {renderError(errors.bankAccountName)}
                        </div>
                        <div>
                            <label className="block text-sm font-medium text-neutral-700 mb-1">Bank Account Number <span className="text-red-500">*</span></label>
                            <input type="text" name="bankAccountNumber" value={formData?.bankAccountNumber || ''} onChange={handleChange} className="w-full p-2 border border-neutral-300 rounded-md" />
                            {renderError(errors.bankAccountNumber)}
                        </div>

                        {/* IFSC Check Integration */}
                        <div className="md:col-span-2">
                            <label className="block text-sm font-medium text-neutral-700 mb-1">IFSC Code <span className="text-red-500">*</span></label>
                            <div className="relative">
                                <input
                                    type="text"
                                    name="ifscCode"
                                    value={formData?.ifscCode || ''}
                                    onChange={(e) => handleChange({ target: { name: 'ifscCode', value: e.target.value.toUpperCase(), type: 'text' } })}
                                    placeholder="e.g. HDFC0001234"
                                    maxLength="11"
                                    className={`w-full p-2 border rounded-md focus:outline-none focus:ring-orange-400 focus:border-orange-400 pr-10 ${ifscError ? 'border-red-500' : bankDetails ? 'border-green-500' : 'border-neutral-300'}`}
                                />

                                {/* Loading Indicator */}
                                {isCheckingIfsc && (
                                    <div className="absolute right-3 top-2.5">
                                        <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-orange-500"></div>
                                    </div>
                                )}

                                {/* Success Icon */}
                                {!isCheckingIfsc && bankDetails && (
                                    <div className="absolute right-3 top-2.5 text-green-600">
                                        <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                                            <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                                        </svg>
                                    </div>
                                )}
                            </div>

                            {/* Validation & Feedback */}
                            {renderError(errors.ifscCode)}
                            {ifscError && <p className="text-xs text-red-600 mt-1">{ifscError}</p>}

                            {/* Display Fetched Bank Details */}
                            {bankDetails && (
                                <div className="mt-2 p-3 bg-green-50 border border-green-100 rounded-md flex items-start gap-3">
                                    <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5 text-green-600 mt-0.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 14v3m4-3v3m4-3v3M3 21h18M3 10h18M3 7l9-4 9 4M4 10h16v11H4V10z" />
                                    </svg>
                                    <div>
                                        <p className="text-sm font-bold text-green-800">{bankDetails.BANK}</p>
                                        <p className="text-xs text-green-700">{bankDetails.BRANCH}, {bankDetails.CITY}</p>
                                    </div>
                                </div>
                            )}
                        </div>

                        {/* Add the Cancel Cheque upload here if needed, following the same pattern as above for file uploads and previews. */}
                        <div className="md:col-span-2">
                            <label className="block text-sm font-medium text-neutral-700 mb-1">Upload Cancel Cheque (PDF)</label>
                            <input
                                type="file"
                                accept="application/pdf"
                                onChange={(e) => handleFileChange('chequeCopy', e.target.files[0])}
                                className="block w-full text-sm text-neutral-500 file:mr-4 file:py-2 file:px-4 file:rounded-md file:border-0 file:text-sm file:font-semibold file:bg-orange-50 file:text-orange-700 hover:file:bg-orange-100"
                            />
                            <FilePreview file={formData?.chequeCopy} onRemove={() => handleFileChange('chequeCopy', null)} />
                            {renderError(errors.chequeCopy)}
                        </div>
                    </div>
                </section>
            </div>
        </div>
    );
}