/*
========================================
File: src/components/owner/Step1_BusinessAccount.jsx (HEAVILY REVISED)
========================================
*/
import ImageUploader from '../common/ImageUploader';

// Reusable input component for this form
const FormInput = ({ label, name, type, value, onChange, error, disabled, children, required }) => (
    <div>
        <label className="block text-sm font-medium text-neutral-700 mb-1">
            {label} {required && <span className="text-red-500">*</span>}
        </label>
        <div className="flex gap-2">
            <input type={type} name={name} value={value || ''} onChange={onChange} disabled={disabled} autoComplete="off" className={`w-full p-2 border rounded-md ${error ? 'border-red-500' : 'border-neutral-300'}`} />
            {children}
        </div>
        {error && <p className="text-xs text-red-600 mt-1">{error}</p>}
    </div>
);

const VerifiedBadge = () => (
    <span className="flex-shrink-0 text-green-600 px-3 py-2 rounded-md text-sm font-medium flex items-center gap-1">
        <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" viewBox="0 0 20 20" fill="currentColor"><path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" /></svg>
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
        <div className="animate-fade-in">
            <ImageUploader onImageCropped={handleLogoCropped} aspect={1} circularCrop={true} triggerId="logo-upload-input" />
            <h3 className="text-2xl font-bold text-neutral-800 mb-2">Business & Account Setup</h3>
            <p className="text-neutral-500 text-sm mb-6">Let's start with the basics. Tell us about your business and create your account.</p>

            <div className="space-y-6">
                <section>
                    <h4 className="text-md font-semibold text-rose-600 border-b pb-2">Catering Details</h4>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4 pt-4">
                        <FormInput label="Catering Name" name="cateringName" type="text" value={formData.cateringName} onChange={handleChange} error={errors.cateringName} required />
                        <FormInput label="Owner Full Name" name="ownerName" type="text" value={formData.ownerName} onChange={handleChange} error={errors.ownerName} required />
                        <div className="md:col-span-2">
                            <label className="block text-sm font-medium text-neutral-700 mb-1">Upload Catering Logo</label>
                            <label htmlFor="logo-upload-input" className="cursor-pointer w-full h-32 border-2 border-dashed border-neutral-300 rounded-lg flex flex-col items-center justify-center text-center hover:bg-neutral-50">
                                {formData.cateringLogo ? (
                                    <img src={formData.cateringLogo} alt="Logo Preview" className="h-28 w-28 rounded-full object-cover" />
                                ) : (
                                    <div>
                                        <svg className="mx-auto h-12 w-12 text-neutral-400" stroke="currentColor" fill="none" viewBox="0 0 48 48" aria-hidden="true"><path d="M28 8H12a4 4 0 00-4 4v20m32-12v8m0 0v8a4 4 0 01-4 4H12a4 4 0 01-4-4v-4m32-4l-3.172-3.172a4 4 0 00-5.656 0L28 28M8 32l9.172-9.172a4 4 0 015.656 0L28 28m0 0l4 4m4-24h8m-4-4v8" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"></path></svg>
                                            <p className="text-sm text-neutral-500">Click to upload logo</p>
                                            <p className="text-xs text-neutral-400">PNG, JPG, JPEG up to 5MB</p>
                                    </div>
                                )}
                            </label>
                            {errors.cateringLogo && <p className="text-xs text-red-600 mt-1">{errors.cateringLogo}</p>}
                        </div>
                    </div>
                </section>

                <section>
                    <h4 className="text-md font-semibold text-rose-600 border-b pb-2">Account Info</h4>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4 pt-4">
                        <FormInput label="Mobile Number" name="mobile" type="tel" value={formData.mobile} onChange={handleChange} disabled={formData.isPhoneVerified} error={errors.mobile} required>
                            {formData.isPhoneVerified ? <VerifiedBadge /> : (
                                <button type="button" onClick={() => onVerifyClick('phone', formData.mobile, 'Owner')} className="flex-shrink-0 bg-amber-500 text-white px-3 py-2 rounded-md text-sm font-medium hover:bg-amber-600">Verify</button>
                            )}
                        </FormInput>

                        <FormInput label="Email" name="email" type="email" value={formData.email} onChange={handleChange} disabled={formData.isEmailVerified} error={errors.email} required>
                            {formData.isEmailVerified ? <VerifiedBadge /> : (
                                <button type="button" onClick={() => onVerifyClick('email', formData.email, 'Owner')} className="flex-shrink-0 bg-amber-500 text-white px-3 py-2 rounded-md text-sm font-medium hover:bg-amber-600">Verify</button>
                            )}
                        </FormInput>

                        <div>
                            <label className="block text-sm font-medium text-neutral-700 mb-1">Catering Contact Number <span className="text-red-500">*</span></label>
                            <div className="flex gap-2">
                                <input type="tel" name="cateringNumber" value={formData.cateringNumber} onChange={handleChange} disabled={formData.cateringNumberSameAsMobile} className={`w-full p-2 border rounded-md ${errors.cateringNumber ? 'border-red-500' : 'border-neutral-300'} disabled:bg-neutral-100`} />
                                {!formData.cateringNumberSameAsMobile && (
                                    formData.isCateringNumberVerified ? <VerifiedBadge /> : (
                                        <button type="button" onClick={() => onVerifyClick('cateringNumber', formData.cateringNumber, 'Owner')} className="flex-shrink-0 bg-amber-500 text-white px-3 py-2 rounded-md text-sm font-medium hover:bg-amber-600">Verify</button>
                                    )
                                )}
                            </div>
                            {errors.cateringNumber && <p className="text-xs text-red-600 mt-1">{errors.cateringNumber}</p>}
                            <label className="flex items-center mt-2 text-xs">
                                <input type="checkbox" checked={formData.cateringNumberSameAsMobile} onChange={handleSameAsMobileCheck} className="mr-2 h-4 w-4 text-rose-600" />
                                Same as Mobile Number
                            </label>
                        </div>

                        <FormInput label="STD Number (Optional)" name="stdNumber" type="text" value={formData.stdNumber} onChange={handleChange} error={errors.stdNumber} />
                    </div>
                </section>
            </div>
        </div>
    );
}