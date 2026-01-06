/*
========================================
File: src/components/owner/Step2_AddressLocation.jsx (NEW FILE)
========================================
*/
import React, { useState, useEffect } from 'react';
import { ownerApiService } from '../../services/ownerApi';
import { apiService } from '../../services/userApi'; // Import the API service


export default function Step2_AddressLocation({ formData, setFormData, errors }) {
    const [isPincodeLoading, setIsPincodeLoading] = useState(false);
    const [pincodeDetails, setPincodeDetails] = useState({ area: '', city: '' });
    const [pincodeError, setPincodeError] = useState('');
    const [states, setStates] = useState([]);
    const [cities, setCities] = useState([]);

    useEffect(() => {
        const fetchStates = async () => {
            try {
                const data = await apiService.getStates();
                setStates(data);
            } catch (err) {
                console.error("Failed to fetch states", err);
            }
        };
        fetchStates();
    }, []);

    const handleChange = async (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));

        if (name === 'pincode') {
            setPincodeError('');
        }

        if (name === 'stateID') {
            setFormData(prev => ({ ...prev, cityID: '' })); // Reset city when state changes
            setCities([]);
            if (value) {
                try {
                    const citiesData = await apiService.getCities(value);
                    setCities(citiesData);
                } catch (err) {
                    console.error("Failed to fetch cities", err);
                }
            }
        }
    };

    useEffect(() => {
        const fetchPincodeDetails = async () => {
            const pincode = formData.pincode;

            // Only fetch if exactly 6 digits
            if (pincode && pincode.length === 6 && /^\d+$/.test(pincode)) {
                setIsPincodeLoading(true);
                setPincodeError('');

                try {
                    const proxyUrl = `https://api.allorigins.win/get?url=${encodeURIComponent(
                        `https://api.postalpincode.in/pincode/${formData.pincode}`
                    )}`;
                    const details = await ownerApiService.getPincodeDetails(proxyUrl);
                    const data = JSON.parse(details.contents);

                    if (data && data[0].Status === 'Success') {
                        const postOffice = data[0].PostOffice[0];
                        const area = postOffice.Name;
                        const city = postOffice.District;

                        // Update local UI state
                        setPincodeDetails({ area, city });

                    } else {
                        setPincodeError('Invalid Pincode. Please check and try again.');
                        setPincodeDetails({ area: '', city: '' });
                        // Optionally clear city/state in formData if invalid
                        setFormData(prev => ({ ...prev, city: '', state: '' }));
                    }
                } catch (error) {
                    console.error("Pincode fetch error:", error);
                    setPincodeError('Unable to fetch location details.');
                } finally {
                    setIsPincodeLoading(false);
                }
            } else if (pincode && pincode.length > 0 && pincode.length !== 6) {
                // Clear details if user is editing and length is invalid
                setPincodeDetails({ area: '', city: '' });
            }
        };

        // Debounce the call to avoid hitting API on every keystroke
        const timerId = setTimeout(() => {
            fetchPincodeDetails();
        }, 500);

        return () => clearTimeout(timerId);
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
                        <input
                            type="text"
                            name="pincode"
                            value={formData.pincode || ''}
                            onChange={handleChange}
                            maxLength="6"
                            className={`w-full p-2 border rounded-md ${pincodeError ? 'border-red-500' : 'border-neutral-300'}`}
                        />
                        {isPincodeLoading && (
                            <span className="absolute right-2 top-2 h-5 w-5 border-2 border-t-rose-600 border-r-rose-600 border-b-rose-600 border-l-transparent rounded-full animate-spin"></span>
                        )}
                    </div>

                    {/* Success Message: Area and City */}
                    {pincodeDetails.area && (
                        <div className="mt-2 text-sm font-medium text-green-600 flex flex-col animate-fade-in">
                            <span className="flex items-center gap-1">
                                <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
                                    <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
                                </svg>
                                {pincodeDetails.area}, {pincodeDetails.city}.
                            </span>
                        </div>
                    )}

                    {/* Error Message */}
                    {(errors.pincode || pincodeError) && (
                        <p className="text-xs text-red-600 mt-1">{errors.pincode || pincodeError}</p>
                    )}
                </div>
                <div></div>
                {/* State Dropdown */}
                <div>
                    <label htmlFor="stateID" className="block text-sm font-medium text-neutral-700 mb-1">State <span className="text-red-500">*</span></label>
                    <select name="stateID" value={formData.stateID || ''} onChange={handleChange} className="w-full p-2 border border-neutral-300 rounded-md bg-white" >
                        <option value="">Select State</option>
                        {states.map(s => <option key={s.stateID} value={s.stateID}>{s.stateName}</option>)}
                    </select>
                    {errors.stateID && <p className="text-xs text-red-600 mt-1">{errors.stateID}</p>}
                </div>

                {/* City Dropdown */}
                <div>
                    <label htmlFor="cityID" className="block text-sm font-medium text-neutral-700 mb-1">City <span className="text-red-500">*</span></label>
                    <select name="cityID" value={formData.cityID || ''} onChange={handleChange} className="w-full p-2 border border-neutral-300 rounded-md bg-white" disabled={!formData.stateID || cities.length === 0}>
                        <option value="">Select City</option>
                        {cities.map(c => <option key={c.cityID} value={c.cityID}>{c.cityName}</option>)}
                    </select>
                    {errors.cityID && <p className="text-xs text-red-600 mt-1">{errors.cityID}</p>}
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