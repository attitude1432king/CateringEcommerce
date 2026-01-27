/*
========================================
File: src/components/owner/Step1_BusinessAccount.jsx (REDESIGNED)
========================================
*/
import ImageUploader from '../common/ImageUploader';

// Reusable input component for this form
const FormInput = ({ label, name, type, value, onChange, error, disabled, children, required, placeholder, helpText }) => (
    <div>
        <label className="block text-sm font-semibold text-neutral-800 mb-2">
            {label} {required && <span className="text-rose-500">*</span>}
        </label>
        {helpText && (
            <p className="text-xs text-neutral-500 mb-2 flex items-start gap-1">
                <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4 text-blue-500 flex-shrink-0 mt-0.5" viewBox="0 0 20 20" fill="currentColor">
                    <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" />
                </svg>
                <span>{helpText}</span>
            </p>
        )}
        <div className="flex gap-2">
            <input
                type={type}
                name={name}
                value={value || ''}
                onChange={onChange}
                disabled={disabled}
                autoComplete="off"
                placeholder={placeholder}
                className={`w-full px-4 py-3 border-2 rounded-lg transition-all duration-200 ${
                    error
                        ? 'border-red-400 bg-red-50 focus:border-red-500 focus:ring-2 focus:ring-red-200'
                        : disabled
                            ? 'border-neutral-200 bg-neutral-50 cursor-not-allowed'
                            : 'border-neutral-200 focus:border-rose-400 focus:ring-2 focus:ring-rose-100'
                } focus:outline-none`}
            />
            {children}
        </div>
        {error && <p className="text-xs text-red-600 mt-1.5 flex items-center gap-1">
            <svg xmlns="http://www.w3.org/2000/svg" className="h-3.5 w-3.5" viewBox="0 0 20 20" fill="currentColor">
                <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
            </svg>
            {error}
        </p>}
    </div>
);

const VerifiedBadge = () => (
    <span className="flex-shrink-0 bg-green-50 text-green-700 border-2 border-green-200 px-4 py-3 rounded-lg text-sm font-semibold flex items-center gap-2 shadow-sm">
        <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
            <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
        </svg>
        Verified
    </span>
);

export default function Step1_BusinessAccount({ formData, setFormData, errors, onVerifyClick }) {
    const handleChange = (e) => {
        setFormData({ ...formData, [e.target.name]: e.target.value });
    };

    const handleSameAsMobileCheck = (e) => {
        const isChecked = e.target.checked;
        setFormData(prev => ({
            ...prev,
            cateringNumberSameAsMobile: isChecked,
            cateringNumber: isChecked ? prev.mobile : '',
            isCateringNumberVerified: isChecked ? prev.isPhoneVerified : false
        }));
    };

    const handleLogoCropped = (base64Image) => {
        setFormData({ ...formData, cateringLogo: base64Image });
    };

    return (
        <div className="animate-fade-in space-y-8">
            <ImageUploader onImageCropped={handleLogoCropped} aspect={1} circularCrop={true} triggerId="logo-upload-input" />

            {/* Header */}
            <div className="bg-gradient-to-r from-rose-50 to-amber-50 p-6 rounded-xl border-l-4 border-rose-500">
                <h3 className="text-3xl font-bold text-neutral-800 mb-2 flex items-center gap-2">
                    <svg xmlns="http://www.w3.org/2000/svg" className="h-8 w-8 text-rose-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
                    </svg>
                    Business & Account Setup
                </h3>
                <p className="text-neutral-600 text-sm leading-relaxed">
                    Let's start with your business essentials. Ensure all information matches your official documents.
                </p>
            </div>

            {/* Business Rules Notice */}
            <div className="bg-blue-50 border-l-4 border-blue-400 p-5 rounded-lg">
                <div className="flex items-start gap-3">
                    <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6 text-blue-600 flex-shrink-0 mt-0.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                    <div className="flex-1">
                        <h4 className="font-bold text-blue-900 mb-2">Important Business Rules</h4>
                        <ul className="text-sm text-blue-800 space-y-1 list-disc list-inside">
                            <li>All contact details (mobile, email, catering number) must be verified via OTP</li>
                            <li>Your catering name should match your legal business name or FSSAI certificate</li>
                            <li>Once verified, contact details cannot be changed without admin approval</li>
                            <li>Logo should be clear, professional, and represent your brand</li>
                        </ul>
                    </div>
                </div>
            </div>

            {/* Catering Details Section */}
            <section className="bg-white p-6 rounded-xl border-2 border-neutral-100 shadow-sm">
                <div className="flex items-center gap-3 mb-6">
                    <div className="w-10 h-10 bg-rose-100 rounded-lg flex items-center justify-center">
                        <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6 text-rose-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 13.255A23.931 23.931 0 0112 15c-3.183 0-6.22-.62-9-1.745M16 6V4a2 2 0 00-2-2h-4a2 2 0 00-2 2v2m4 6h.01M5 20h14a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                        </svg>
                    </div>
                    <div>
                        <h4 className="text-xl font-bold text-neutral-800">Catering Details</h4>
                        <p className="text-sm text-neutral-500">Your business identity and branding</p>
                    </div>
                </div>

                <div className="space-y-6">
                    <FormInput
                        label="Catering Business Name"
                        name="cateringName"
                        type="text"
                        value={formData.cateringName}
                        onChange={handleChange}
                        error={errors.cateringName}
                        placeholder="e.g., Royal Kitchen Caterers"
                        helpText="Enter your registered business name as it appears on official documents"
                        required
                    />

                    <FormInput
                        label="Owner Full Name"
                        name="ownerName"
                        type="text"
                        value={formData.ownerName}
                        onChange={handleChange}
                        error={errors.ownerName}
                        placeholder="e.g., Rajesh Kumar"
                        helpText="Full name of the business owner (must match PAN card)"
                        required
                    />

                    <div>
                        <label className="block text-sm font-semibold text-neutral-800 mb-2">
                            Upload Catering Logo <span className="text-rose-500">*</span>
                        </label>
                        <p className="text-xs text-neutral-500 mb-3 flex items-start gap-1">
                            <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4 text-blue-500 flex-shrink-0 mt-0.5" viewBox="0 0 20 20" fill="currentColor">
                                <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" />
                            </svg>
                            <span>Upload a high-quality square logo. This will be displayed on your profile and order pages. Recommended size: 500x500px</span>
                        </p>
                        <label
                            htmlFor="logo-upload-input"
                            className="group cursor-pointer w-full h-40 border-3 border-dashed border-neutral-300 rounded-xl flex flex-col items-center justify-center text-center hover:border-rose-400 hover:bg-rose-50 transition-all duration-200"
                        >
                            {formData.cateringLogo ? (
                                <div className="relative">
                                    <img src={formData.cateringLogo} alt="Logo Preview" className="h-32 w-32 rounded-full object-cover border-4 border-white shadow-lg" />
                                    <div className="absolute inset-0 bg-black bg-opacity-0 group-hover:bg-opacity-10 rounded-full transition-all duration-200 flex items-center justify-center">
                                        <p className="text-white font-semibold opacity-0 group-hover:opacity-100 transition-opacity">Change Logo</p>
                                    </div>
                                </div>
                            ) : (
                                <div className="space-y-2">
                                    <div className="mx-auto w-16 h-16 bg-neutral-100 rounded-full flex items-center justify-center group-hover:bg-rose-100 transition-colors">
                                        <svg className="h-8 w-8 text-neutral-400 group-hover:text-rose-500 transition-colors" stroke="currentColor" fill="none" viewBox="0 0 48 48" aria-hidden="true">
                                            <path d="M28 8H12a4 4 0 00-4 4v20m32-12v8m0 0v8a4 4 0 01-4 4H12a4 4 0 01-4-4v-4m32-4l-3.172-3.172a4 4 0 00-5.656 0L28 28M8 32l9.172-9.172a4 4 0 015.656 0L28 28m0 0l4 4m4-24h8m-4-4v8" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"></path>
                                        </svg>
                                    </div>
                                    <div>
                                        <p className="text-base font-semibold text-neutral-700 group-hover:text-rose-600 transition-colors">Click to upload logo</p>
                                        <p className="text-xs text-neutral-500 mt-1">PNG, JPG, JPEG • Max 5MB • Square format preferred</p>
                                    </div>
                                </div>
                            )}
                        </label>
                        {errors.cateringLogo && (
                            <p className="text-xs text-red-600 mt-2 flex items-center gap-1">
                                <svg xmlns="http://www.w3.org/2000/svg" className="h-3.5 w-3.5" viewBox="0 0 20 20" fill="currentColor">
                                    <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                                </svg>
                                {errors.cateringLogo}
                            </p>
                        )}
                    </div>
                </div>
            </section>

            {/* Account Information Section */}
            <section className="bg-white p-6 rounded-xl border-2 border-neutral-100 shadow-sm">
                <div className="flex items-center gap-3 mb-6">
                    <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
                        <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6 text-blue-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
                        </svg>
                    </div>
                    <div>
                        <h4 className="text-xl font-bold text-neutral-800">Account & Contact Verification</h4>
                        <p className="text-sm text-neutral-500">Secure your account with verified contact details</p>
                    </div>
                </div>

                <div className="bg-amber-50 border border-amber-200 rounded-lg p-4 mb-6">
                    <p className="text-sm text-amber-800 flex items-start gap-2">
                        <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5 flex-shrink-0 mt-0.5" viewBox="0 0 20 20" fill="currentColor">
                            <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                        </svg>
                        <span><strong>Verification Required:</strong> Click "Verify" button to receive OTP on your mobile/email. All contacts must be verified to proceed.</span>
                    </p>
                </div>

                <div className="space-y-6">
                    <FormInput
                        label="Mobile Number"
                        name="mobile"
                        type="tel"
                        value={formData.mobile}
                        onChange={handleChange}
                        disabled={formData.isPhoneVerified}
                        error={errors.mobile}
                        placeholder="10-digit mobile number"
                        helpText="Your primary contact number for order notifications and account recovery"
                        required
                    >
                        {formData.isPhoneVerified ? (
                            <VerifiedBadge />
                        ) : (
                            <button
                                type="button"
                                onClick={() => onVerifyClick('phone', formData.mobile, 'Owner')}
                                className="flex-shrink-0 bg-gradient-to-r from-amber-500 to-amber-600 text-white px-5 py-3 rounded-lg text-sm font-semibold hover:from-amber-600 hover:to-amber-700 transition-all duration-200 shadow-md hover:shadow-lg whitespace-nowrap"
                            >
                                Send OTP
                            </button>
                        )}
                    </FormInput>

                    <FormInput
                        label="Email Address"
                        name="email"
                        type="email"
                        value={formData.email}
                        onChange={handleChange}
                        disabled={formData.isEmailVerified}
                        error={errors.email}
                        placeholder="your.email@example.com"
                        helpText="Official email for business communication and order confirmations"
                        required
                    >
                        {formData.isEmailVerified ? (
                            <VerifiedBadge />
                        ) : (
                            <button
                                type="button"
                                onClick={() => onVerifyClick('email', formData.email, 'Owner')}
                                className="flex-shrink-0 bg-gradient-to-r from-amber-500 to-amber-600 text-white px-5 py-3 rounded-lg text-sm font-semibold hover:from-amber-600 hover:to-amber-700 transition-all duration-200 shadow-md hover:shadow-lg whitespace-nowrap"
                            >
                                Send OTP
                            </button>
                        )}
                    </FormInput>

                    <div>
                        <label className="block text-sm font-semibold text-neutral-800 mb-2">
                            Catering Contact Number <span className="text-rose-500">*</span>
                        </label>
                        <p className="text-xs text-neutral-500 mb-3 flex items-start gap-1">
                            <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4 text-blue-500 flex-shrink-0 mt-0.5" viewBox="0 0 20 20" fill="currentColor">
                                <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" />
                            </svg>
                            <span>Business contact number displayed to customers. Can be different from your personal mobile</span>
                        </p>
                        <div className="flex gap-2">
                            <input
                                type="tel"
                                name="cateringNumber"
                                value={formData.cateringNumber}
                                onChange={handleChange}
                                disabled={formData.cateringNumberSameAsMobile}
                                placeholder="10-digit catering contact"
                                className={`w-full px-4 py-3 border-2 rounded-lg transition-all duration-200 ${
                                    errors.cateringNumber
                                        ? 'border-red-400 bg-red-50'
                                        : formData.cateringNumberSameAsMobile
                                            ? 'border-neutral-200 bg-neutral-50 cursor-not-allowed'
                                            : 'border-neutral-200 focus:border-rose-400 focus:ring-2 focus:ring-rose-100'
                                } focus:outline-none`}
                            />
                            {!formData.cateringNumberSameAsMobile && (
                                formData.isCateringNumberVerified ? (
                                    <VerifiedBadge />
                                ) : (
                                    <button
                                        type="button"
                                        onClick={() => onVerifyClick('cateringNumber', formData.cateringNumber, 'Owner')}
                                        className="flex-shrink-0 bg-gradient-to-r from-amber-500 to-amber-600 text-white px-5 py-3 rounded-lg text-sm font-semibold hover:from-amber-600 hover:to-amber-700 transition-all duration-200 shadow-md hover:shadow-lg whitespace-nowrap"
                                    >
                                        Send OTP
                                    </button>
                                )
                            )}
                        </div>
                        {errors.cateringNumber && (
                            <p className="text-xs text-red-600 mt-1.5 flex items-center gap-1">
                                <svg xmlns="http://www.w3.org/2000/svg" className="h-3.5 w-3.5" viewBox="0 0 20 20" fill="currentColor">
                                    <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                                </svg>
                                {errors.cateringNumber}
                            </p>
                        )}
                        <label className="flex items-center mt-3 text-sm cursor-pointer group">
                            <input
                                type="checkbox"
                                checked={formData.cateringNumberSameAsMobile}
                                onChange={handleSameAsMobileCheck}
                                className="mr-3 h-5 w-5 text-rose-600 border-neutral-300 rounded focus:ring-2 focus:ring-rose-200 cursor-pointer"
                            />
                            <span className="text-neutral-700 group-hover:text-neutral-900">
                                Same as Mobile Number (auto-verifies if mobile is verified)
                            </span>
                        </label>
                    </div>

                    <FormInput
                        label="STD Number (Optional)"
                        name="stdNumber"
                        type="text"
                        value={formData.stdNumber}
                        onChange={handleChange}
                        error={errors.stdNumber}
                        placeholder="e.g., 022-12345678"
                        helpText="Landline number with STD code (optional)"
                    />
                </div>
            </section>
        </div>
    );
}