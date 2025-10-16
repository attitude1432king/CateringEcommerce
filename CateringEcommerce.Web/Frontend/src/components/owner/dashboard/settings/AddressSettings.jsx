/*
========================================
File: src/components/owner/dashboard/settings/AddressSettings.jsx (REVISED)
========================================
*/
import React, { useState, useEffect, useCallback } from 'react';
// import { apiService } from '../../../../services/api'; // For real pincode API calls

export default function AddressSettings({ initialData, onUpdate }) {
    const [formData, setFormData] = useState(initialData);
    const [pincodeError, setPincodeError] = useState('');

    const handlePincodeChange = useCallback(async (pincode) => {
        if (pincode.length === 6) {
            setPincodeError('');
            try {
                // In a real app, you would uncomment this
                // const data = await apiService.getPincodeDetails(pincode);
                // setFormData(prev => ({ ...prev, city: data.city, state: data.state }));

                // Mock API call for demonstration
                console.log("Fetching details for pincode:", pincode);
                setTimeout(() => {
                    if (pincode === "395007") {
                        setFormData(prev => ({ ...prev, city: 'Surat', state: 'Gujarat' }));
                    }
                }, 500);

            } catch (error) {
                setPincodeError('Could not fetch details for this pincode.');
                console.error(error);
            }
        }
    }, []);

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));
        if (name === 'pincode') {
            handlePincodeChange(value);
        }
    };

    const handleSubmit = (e) => {
        e.preventDefault();
        onUpdate(formData);
    };

    return (
        <form onSubmit={handleSubmit} className="bg-white p-6 sm:p-8 rounded-xl shadow-sm">
            <h3 className="text-xl font-semibold text-neutral-800 mb-6">Catering Address</h3>
            <div className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div>
                        <label htmlFor="shopNo" className="block text-sm font-medium text-neutral-700">Shop No. / Building*</label>
                        <input type="text" name="shopNo" id="shopNo" value={formData.shopNo} onChange={handleChange} className="mt-1 w-full p-2 border border-neutral-300 rounded-md" required />
                    </div>
                    <div>
                        <label htmlFor="floor" className="block text-sm font-medium text-neutral-700">Floor / Tower</label>
                        <input type="text" name="floor" id="floor" value={formData.area} onChange={handleChange} className="mt-1 w-full p-2 border border-neutral-300 rounded-md" />
                    </div>
                </div>
                <div>
                    <label htmlFor="street" className="block text-sm font-medium text-neutral-700">Area / Street / Landmark*</label>
                    <input type="text" name="street" id="street" value={formData.street} onChange={handleChange} className="mt-1 w-full p-2 border border-neutral-300 rounded-md" required />
                </div>
                <div>
                    <label htmlFor="pincode" className="block text-sm font-medium text-neutral-700">Pincode*</label>
                    <input type="text" name="pincode" id="pincode" value={formData.pincode} onChange={handleChange} className="mt-1 w-full p-2 border border-neutral-300 rounded-md" required maxLength="6" />
                    {pincodeError && <p className="text-xs text-red-500 mt-1">{pincodeError}</p>}
                </div>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div>
                        <label htmlFor="city" className="block text-sm font-medium text-neutral-700">City*</label>
                        <input type="text" name="city" id="city" value={formData.city} onChange={handleChange} className="mt-1 w-full p-2 border border-neutral-300 rounded-md" required />
                    </div>
                    <div>
                        <label htmlFor="state" className="block text-sm font-medium text-neutral-700">State*</label>
                        <input type="text" name="state" id="state" value={formData.state} onChange={handleChange} className="mt-1 w-full p-2 border border-neutral-300 rounded-md" required />
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

                <div className="pt-4 text-right">
                    <button type="submit" className="bg-rose-600 text-white px-5 py-2 rounded-md font-medium hover:bg-rose-700">
                        Save Changes
                    </button>
                </div>
            </div>
        </form>
    );
}
