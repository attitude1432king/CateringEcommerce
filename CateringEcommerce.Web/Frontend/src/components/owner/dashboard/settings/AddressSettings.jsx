/*
========================================
File: src/components/owner/dashboard/settings/AddressSettings.jsx (REVISED)
========================================
*/
import React, { useState, useEffect, useCallback } from 'react';
import { useToast } from '../../../../contexts/ToastContext';
// import { apiService } from '../../../../services/api'; // For real pincode API calls


// Reusing helper components
const RequiredAsterisk = () => <span className="text-red-500">*</span>;
const ValidationError = ({ message }) => message ? <p className="text-red-500 text-xs mt-1">{message}</p> : null;

export default function AddressSettings({ initialData, onUpdate, isSaving }) {
    const [formData, setFormData] = useState(initialData);
    //const [pincodeError, setPincodeError] = useState('');
    const { showToast } = useToast();
    const [errors, setErrors] = useState({});

    //const handlePincodeChange = useCallback(async (pincode) => {
    //    if (pincode.length === 6) {
    //        setPincodeError('');
    //        try {
    //            // In a real app, you would uncomment this
    //            // const data = await apiService.getPincodeDetails(pincode);
    //            // setFormData(prev => ({ ...prev, city: data.city, state: data.state }));

    //            // Mock API call for demonstration
    //            console.log("Fetching details for pincode:", pincode);
    //            setTimeout(() => {
    //                if (pincode === "395007") {
    //                    setFormData(prev => ({ ...prev, city: 'Surat', state: 'Gujarat' }));
    //                }
    //            }, 500);

    //        } catch (error) {
    //            setPincodeError('Could not fetch details for this pincode.');
    //            console.error(error);
    //        }
    //    }
    //}, []);

    const validate = (currentData) => {
        const newErrors = {};
        if (!currentData.shopNo?.trim()) newErrors.shopNo = 'Shop No. / Building is required.';
        if (!currentData.area?.trim()) newErrors.area = 'Floor / Tower is required.';
        if (!currentData.street?.trim()) newErrors.street = 'Area / Street is required.';
        //if (!currentData.pincode?.trim()) newErrors.pincode = 'Pincode is required.';
        //if (!currentData.city?.trim()) newErrors.city = 'City is required.';
        //if (!currentData.state?.trim()) newErrors.state = 'State is required.';
        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleChange = (e) => {
        const { name, value } = e.target;
        const updatedFormData = { ...formData, [name]: value };
        setFormData(updatedFormData);
        validate(updatedFormData);
    };

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

    return (
        <form onSubmit={handleSubmit} className="bg-white p-6 sm:p-8 rounded-xl shadow-sm">
            <h3 className="text-xl font-semibold text-neutral-800 mb-6">Catering Address</h3>
            <div className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div>
                        <label htmlFor="shopNo" className="block text-sm font-medium text-neutral-700">Shop No. / Building <RequiredAsterisk /></label>
                        <input type="text" name="shopNo" id="shopNo" value={formData.shopNo} onChange={handleChange} className="mt-1 w-full p-2 border border-neutral-300 rounded-md" required />
                        <ValidationError message={errors.shopNo} />
                    </div>
                    <div>
                        <label htmlFor="area" className="block text-sm font-medium text-neutral-700">Floor / Tower <RequiredAsterisk /></label>
                        <input type="text" name="area" id="area" value={formData.area} onChange={handleChange} className="mt-1 w-full p-2 border border-neutral-300 rounded-md" />
                        <ValidationError message={errors.area} />
                    </div>
                </div>
                <div>
                    <label htmlFor="street" className="block text-sm font-medium text-neutral-700">Area / Street / Landmark <RequiredAsterisk /></label>
                    <input type="text" name="street" id="street" value={formData.street} onChange={handleChange} className="mt-1 w-full p-2 border border-neutral-300 rounded-md" required />
                    <ValidationError message={errors.street} />
                </div>
                {/*<div>*/}
                {/*    <label htmlFor="pincode" className="block text-sm font-medium text-neutral-700">Pincode <RequiredAsterisk /></label>*/}
                {/*    <input type="text" name="pincode" id="pincode" value={formData.pincode} onChange={handleChange} className="mt-1 w-full p-2 border border-neutral-300 rounded-md input-readonly" required maxLength="6" />*/}
                {/*    {pincodeError && <p className="text-xs text-red-500 mt-1">{pincodeError}</p>}*/}
                {/*    <ValidationError message={errors.pincode} />*/}
                {/*</div>*/}

                {/*<div className="grid grid-cols-1 md:grid-cols-2 gap-4">*/}
                {/*    <div>*/}
                {/*        <label htmlFor="city" className="block text-sm font-medium text-neutral-700">City <RequiredAsterisk /></label>*/}
                {/*        <input type="text" name="city" id="city" value={formData.city} onChange={handleChange} className="mt-1 w-full p-2 border border-neutral-300 rounded-md" required />*/}
                {/*        <ValidationError message={errors.city} />*/}
                {/*    </div>*/}

                {/*    <div>*/}
                {/*        <label htmlFor="state" className="block text-sm font-medium text-neutral-700">State <RequiredAsterisk /></label>*/}
                {/*        <input type="text" name="state" id="state" value={formData.state} onChange={handleChange} className="mt-1 w-full p-2 border border-neutral-300 rounded-md" required />*/}
                {/*        <ValidationError message={errors.state} />*/}
                {/*    </div>*/}
                {/*</div>*/}

                <div>
                    <label htmlFor="pincode" className="block text-sm font-medium text-neutral-700">
                        Pincode
                    </label>
                    <input
                        type="text"
                        name="pincode"
                        id="pincode"
                        value={formData.pincode}
                        readOnly
                        className="mt-1 w-full p-2 border rounded-md input-readonly"
                        required
                        maxLength="6"
                    />
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div>
                        <label htmlFor="city" className="block text-sm font-medium text-neutral-700">
                            City
                        </label>
                        <input
                            type="text"
                            name="city"
                            id="city"
                            value={formData.city}
                            readOnly
                            className="mt-1 w-full p-2 border rounded-md input-readonly"
                            required
                        />
                    </div>

                    <div>
                        <label htmlFor="state" className="block text-sm font-medium text-neutral-700">
                            State
                        </label>
                        <input
                            type="text"
                            name="state"
                            id="state"
                            value={formData.state}
                            readOnly
                            className="mt-1 w-full p-2 border rounded-md input-readonly"
                            required
                        />
                    </div>
                </div>



                {/* Map Section */}
                <div className="pt-4">
                    <h4 className="text-md font-semibold text-neutral-800 mb-2">Pin Your Location</h4>
                    <p className="text-sm text-neutral-500 mb-3">Drag the pin to set your precise kitchen location. This helps customers find you accurately.</p>
                    <div className="w-full h-64 bg-neutral-200 rounded-lg flex items-center justify-center">
                        <p className="text-neutral-500">Map integration would appear here.</p>
                        {/* In a real app, you would replace this div with the <GoogleMapSelector /> component */}
                    </div>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div>
                        <label htmlFor="latitude" className="block text-sm font-medium text-neutral-700">Latitude</label>
                        <input type="text" name="latitude" id="latitude" value={formData.latitude} onChange={handleChange} className="mt-1 w-full p-2 border border-neutral-300 rounded-md" />
                    </div>
                    <div>
                        <label htmlFor="longitude" className="block text-sm font-medium text-neutral-700">Longitude</label>
                        <input type="text" name="longitude" id="longitude" value={formData.longitude} onChange={handleChange} className="mt-1 w-full p-2 border border-neutral-300 rounded-md" />
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
