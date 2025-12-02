/*
========================================
File: src/components/owner/Step2_AddressLocation.jsx (NEW FILE)
========================================
*/
import React, { useState, useEffect } from 'react';
import { ownerApiService } from '../../services/ownerApi';


export default function Step2_AddressLocation({ formData, setFormData, errors }) {
    const [isPincodeLoading, setIsPincodeLoading] = useState(false);
    const [pincodeError, setPincodeError] = useState('');

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData({ ...formData, [name]: value });
        if (name === 'pincode') {
            setPincodeError('');
        }
    };

    useEffect(() => {
        if (formData.pincode && formData.pincode.length === 6) {
            const fetchDetails = async () => {
                setIsPincodeLoading(true);
                setPincodeError('');
                try {
                    const proxyUrl = `https://api.allorigins.win/get?url=${encodeURIComponent(
                        `https://api.postalpincode.in/pincode/${formData.pincode}`
                    )}`;
                    const details = await ownerApiService.getPincodeDetails(proxyUrl);
                    const parsed = JSON.parse(details.contents);
                    setFormData(prev => ({
                        ...prev,
                        city: parsed[0].PostOffice[0].District,
                        state: parsed[0].PostOffice[0].State
                    }));
                } catch (error) {
                    setPincodeError(error.message);
                    setFormData(prev => ({ ...prev, city: '', state: '' }));
                } finally {
                    setIsPincodeLoading(false);
                }
            };
            fetchDetails();
        }
    }, [formData.pincode, setFormData]);

    return (
        <div className="animate-fade-in">
            <h3 className="text-2xl font-bold text-neutral-800 mb-2">Catering Address</h3>
            <p className="text-neutral-500 text-sm mb-6">Where is your business located? This helps customers find you.</p>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                    <label className="block text-sm font-medium text-neutral-700 mb-1">Shop No. / Building <span className="text-red-500">*</span></label>
                    <input type="text" name="shopNo" value={formData.shopNo || ''} onChange={handleChange} autoComplete="off" className="w-full p-2 border border-neutral-300 rounded-md" />
                    {errors.shopNo && <p className="text-xs text-red-600 mt-1">{errors.shopNo}</p>}
                </div>
                <div>
                    <label className="block text-sm font-medium text-neutral-700 mb-1">Floor / Tower <span className="text-red-500">*</span></label>
                    <input type="text" name="floor" value={formData.floor || ''} onChange={handleChange} autoComplete="off" className="w-full p-2 border border-neutral-300 rounded-md" />
                    {errors.floor && <p className="text-xs text-red-600 mt-1">{errors.floor}</p>}
                </div>
                <div className="md:col-span-2">
                    <label className="block text-sm font-medium text-neutral-700 mb-1">Area / Street / Landmark <span className="text-red-500">*</span></label>
                    <input type="text" name="landmark" value={formData.landmark || ''} onChange={handleChange} autoComplete="off" className="w-full p-2 border border-neutral-300 rounded-md" />
                    {errors.landmark && <p className="text-xs text-red-600 mt-1">{errors.landmark}</p>}
                </div>
                <div>
                    <label className="block text-sm font-medium text-neutral-700 mb-1">Pincode <span className="text-red-500">*</span></label>
                    <div className="relative">
                        <input type="text" name="pincode" value={formData.pincode || ''} onChange={handleChange} autoComplete="off" maxLength="6" className="w-full p-2 border border-neutral-300 rounded-md" />
                        {isPincodeLoading && <span className="absolute right-2 top-2 h-5 w-5 border-2 border-t-rose-600 border-r-rose-600 border-b-rose-600 border-l-transparent rounded-full animate-spin"></span>}
                    </div>
                    {(errors.pincode || pincodeError) && <p className="text-xs text-red-600 mt-1">{errors.pincode || pincodeError}</p>}
                </div>
                <div></div>
                <div>
                    <label className="block text-sm font-medium text-neutral-700 mb-1">State</label>
                    <input type="text" name="state" value={formData.state || ''} readOnly className="w-full p-2 border border-neutral-300 rounded-md bg-neutral-100" />
                </div>
                <div>
                    <label className="block text-sm font-medium text-neutral-700 mb-1">City</label>
                    <input type="text" name="city" value={formData.city || ''} readOnly className="w-full p-2 border border-neutral-300 rounded-md bg-neutral-100" />
                </div>
                <div className="md:col-span-2 mt-2">
                    <p className="text-sm font-medium text-neutral-700 mb-1">Location Coordinates (Optional)</p>
                    <div className="p-4 border rounded-md bg-neutral-50 text-center text-sm text-neutral-600">
                        Google Map integration will be available soon. Please enter coordinates manually if known.
                    </div>
                </div>
                <div>
                    <label className="block text-sm font-medium text-neutral-700 mb-1">Latitude</label>
                    <input type="text" name="latitude" value={formData.latitude || ''} onChange={handleChange} autoComplete="off" className="w-full p-2 border border-neutral-300 rounded-md" />
                </div>
                <div>
                    <label className="block text-sm font-medium text-neutral-700 mb-1">Longitude</label>
                    <input type="text" name="longitude" value={formData.longitude || ''} onChange={handleChange} autoComplete="off" className="w-full p-2 border border-neutral-300 rounded-md" />
                </div>
            </div>
        </div>
    );
}