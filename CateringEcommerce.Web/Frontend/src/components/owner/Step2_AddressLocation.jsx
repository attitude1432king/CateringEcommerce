/*
========================================
File: src/components/owner/Step2_AddressLocation.jsx (REDESIGNED WITH MAP)
========================================
*/
import React, { useState, useEffect, useRef } from 'react';
import { ownerApiService } from '../../services/ownerApi';
import { apiService } from '../../services/userApi';

// Google Maps Loader - Simple implementation
const loadGoogleMapsScript = (callback) => {
    const existingScript = document.getElementById('googleMaps');
    if (!existingScript) {
        const script = document.createElement('script');
        script.src = `https://maps.googleapis.com/maps/api/js?key=YOUR_GOOGLE_MAPS_API_KEY&libraries=places`;
        script.id = 'googleMaps';
        document.body.appendChild(script);
        script.onload = () => {
            if (callback) callback();
        };
    } else {
        if (callback) callback();
    }
};

// Map Component
const LocationMapSelector = ({ latitude, longitude, onLocationSelect }) => {
    const mapRef = useRef(null);
    const [map, setMap] = useState(null);
    const [marker, setMarker] = useState(null);
    const [mapLoaded, setMapLoaded] = useState(false);

    useEffect(() => {
        loadGoogleMapsScript(() => {
            setMapLoaded(true);
        });
    }, []);

    useEffect(() => {
        if (mapLoaded && mapRef.current && !map) {
            const defaultLat = latitude || 28.6139; // Default to Delhi
            const defaultLng = longitude || 77.2090;

            const googleMap = new window.google.maps.Map(mapRef.current, {
                center: { lat: parseFloat(defaultLat), lng: parseFloat(defaultLng) },
                zoom: 15,
                mapTypeControl: false,
                streetViewControl: false,
                fullscreenControl: true,
            });

            const googleMarker = new window.google.maps.Marker({
                position: { lat: parseFloat(defaultLat), lng: parseFloat(defaultLng) },
                map: googleMap,
                draggable: true,
                animation: window.google.maps.Animation.DROP,
            });

            googleMarker.addListener('dragend', (event) => {
                const lat = event.latLng.lat();
                const lng = event.latLng.lng();
                onLocationSelect(lat, lng);
            });

            googleMap.addListener('click', (event) => {
                const lat = event.latLng.lat();
                const lng = event.latLng.lng();
                googleMarker.setPosition({ lat, lng });
                onLocationSelect(lat, lng);
            });

            setMap(googleMap);
            setMarker(googleMarker);
        }
    }, [mapLoaded, map, latitude, longitude, onLocationSelect]);

    // Update marker position when latitude/longitude props change
    useEffect(() => {
        if (marker && latitude && longitude) {
            const newPosition = { lat: parseFloat(latitude), lng: parseFloat(longitude) };
            marker.setPosition(newPosition);
            if (map) {
                map.setCenter(newPosition);
            }
        }
    }, [latitude, longitude, marker, map]);

    if (!mapLoaded) {
        return (
            <div className="w-full h-96 bg-neutral-100 rounded-xl flex items-center justify-center">
                <div className="text-center">
                    <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-rose-600 mx-auto mb-3"></div>
                    <p className="text-neutral-600 text-sm">Loading map...</p>
                </div>
            </div>
        );
    }

    return (
        <div className="relative">
            <div ref={mapRef} className="w-full h-96 rounded-xl border-2 border-neutral-200 shadow-lg"></div>
            <div className="absolute top-4 left-4 bg-white px-4 py-2 rounded-lg shadow-lg text-sm">
                <p className="text-neutral-600 flex items-center gap-2">
                    <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4 text-rose-600" viewBox="0 0 20 20" fill="currentColor">
                        <path fillRule="evenodd" d="M5.05 4.05a7 7 0 119.9 9.9L10 18.9l-4.95-4.95a7 7 0 010-9.9zM10 11a2 2 0 100-4 2 2 0 000 4z" clipRule="evenodd" />
                    </svg>
                    Click or drag marker to set location
                </p>
            </div>
        </div>
    );
};

export default function Step2_AddressLocation({ formData, setFormData, errors }) {
    const [isPincodeLoading, setIsPincodeLoading] = useState(false);
    const [pincodeDetails, setPincodeDetails] = useState({ area: '', city: '' });
    const [pincodeError, setPincodeError] = useState('');
    const [states, setStates] = useState([]);
    const [cities, setCities] = useState([]);
    const [useCurrentLocation, setUseCurrentLocation] = useState(false);

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
            setFormData(prev => ({ ...prev, cityID: '' }));
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

    const handleLocationSelect = (lat, lng) => {
        setFormData(prev => ({
            ...prev,
            latitude: lat.toFixed(6),
            longitude: lng.toFixed(6)
        }));
    };

    const handleGetCurrentLocation = () => {
        setUseCurrentLocation(true);
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(
                (position) => {
                    const lat = position.coords.latitude;
                    const lng = position.coords.longitude;
                    handleLocationSelect(lat, lng);
                    setUseCurrentLocation(false);
                },
                (error) => {
                    console.error('Error getting location:', error);
                    alert('Unable to get your location. Please enable location services.');
                    setUseCurrentLocation(false);
                }
            );
        } else {
            alert('Geolocation is not supported by your browser.');
            setUseCurrentLocation(false);
        }
    };

    useEffect(() => {
        const fetchPincodeDetails = async () => {
            const pincode = formData.pincode;

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

                        setPincodeDetails({ area, city });

                    } else {
                        setPincodeError('Invalid Pincode. Please check and try again.');
                        setPincodeDetails({ area: '', city: '' });
                        setFormData(prev => ({ ...prev, city: '', state: '' }));
                    }
                } catch (error) {
                    console.error("Pincode fetch error:", error);
                    setPincodeError('Unable to fetch location details.');
                } finally {
                    setIsPincodeLoading(false);
                }
            } else if (pincode && pincode.length > 0 && pincode.length !== 6) {
                setPincodeDetails({ area: '', city: '' });
            }
        };

        const timerId = setTimeout(() => {
            fetchPincodeDetails();
        }, 500);

        return () => clearTimeout(timerId);
    }, [formData.pincode, setFormData]);

    return (
        <div className="animate-fade-in space-y-8">
            {/* Header */}
            <div className="bg-gradient-to-r from-blue-50 to-cyan-50 p-6 rounded-xl border-l-4 border-blue-500">
                <h3 className="text-3xl font-bold text-neutral-800 mb-2 flex items-center gap-2">
                    <svg xmlns="http://www.w3.org/2000/svg" className="h-8 w-8 text-blue-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                    </svg>
                    Address & Location
                </h3>
                <p className="text-neutral-600 text-sm leading-relaxed">
                    Where is your catering business located? Accurate location helps customers find you easily.
                </p>
            </div>

            {/* Business Rules Notice */}
            <div className="bg-blue-50 border-l-4 border-blue-400 p-5 rounded-lg">
                <div className="flex items-start gap-3">
                    <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6 text-blue-600 flex-shrink-0 mt-0.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                    <div className="flex-1">
                        <h4 className="font-bold text-blue-900 mb-2">Location Guidelines</h4>
                        <ul className="text-sm text-blue-800 space-y-1 list-disc list-inside">
                            <li>Enter your business's physical address (not a PO Box or virtual address)</li>
                            <li>This address must match the address on your FSSAI certificate</li>
                            <li>Pincode will auto-fetch area and city details for verification</li>
                            <li>Use the map to mark your exact location for better customer experience</li>
                        </ul>
                    </div>
                </div>
            </div>

            {/* Address Details Section */}
            <section className="bg-white p-6 rounded-xl border-2 border-neutral-100 shadow-sm">
                <div className="flex items-center gap-3 mb-6">
                    <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
                        <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6 text-blue-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
                        </svg>
                    </div>
                    <div>
                        <h4 className="text-xl font-bold text-neutral-800">Business Address</h4>
                        <p className="text-sm text-neutral-500">Complete address details of your catering location</p>
                    </div>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div>
                        <label className="block text-sm font-semibold text-neutral-800 mb-2">
                            Shop No. / Building <span className="text-rose-500">*</span>
                        </label>
                        <input
                            type="text"
                            name="shopNo"
                            value={formData.shopNo || ''}
                            onChange={handleChange}
                            autoComplete="off"
                            placeholder="e.g., Shop 101, Building A"
                            className={`w-full px-4 py-3 border-2 rounded-lg transition-all duration-200 ${
                                errors.shopNo
                                    ? 'border-red-400 bg-red-50'
                                    : 'border-neutral-200 focus:border-blue-400 focus:ring-2 focus:ring-blue-100'
                            } focus:outline-none`}
                        />
                        {errors.shopNo && (
                            <p className="text-xs text-red-600 mt-1.5 flex items-center gap-1">
                                <svg xmlns="http://www.w3.org/2000/svg" className="h-3.5 w-3.5" viewBox="0 0 20 20" fill="currentColor">
                                    <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                                </svg>
                                {errors.shopNo}
                            </p>
                        )}
                    </div>

                    <div>
                        <label className="block text-sm font-semibold text-neutral-800 mb-2">
                            Floor / Tower <span className="text-rose-500">*</span>
                        </label>
                        <input
                            type="text"
                            name="floor"
                            value={formData.floor || ''}
                            onChange={handleChange}
                            autoComplete="off"
                            placeholder="e.g., 2nd Floor, Tower B"
                            className={`w-full px-4 py-3 border-2 rounded-lg transition-all duration-200 ${
                                errors.floor
                                    ? 'border-red-400 bg-red-50'
                                    : 'border-neutral-200 focus:border-blue-400 focus:ring-2 focus:ring-blue-100'
                            } focus:outline-none`}
                        />
                        {errors.floor && (
                            <p className="text-xs text-red-600 mt-1.5 flex items-center gap-1">
                                <svg xmlns="http://www.w3.org/2000/svg" className="h-3.5 w-3.5" viewBox="0 0 20 20" fill="currentColor">
                                    <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                                </svg>
                                {errors.floor}
                            </p>
                        )}
                    </div>

                    <div className="md:col-span-2">
                        <label className="block text-sm font-semibold text-neutral-800 mb-2">
                            Area / Street / Landmark <span className="text-rose-500">*</span>
                        </label>
                        <input
                            type="text"
                            name="landmark"
                            value={formData.landmark || ''}
                            onChange={handleChange}
                            autoComplete="off"
                            placeholder="e.g., Near City Mall, MG Road"
                            className={`w-full px-4 py-3 border-2 rounded-lg transition-all duration-200 ${
                                errors.landmark
                                    ? 'border-red-400 bg-red-50'
                                    : 'border-neutral-200 focus:border-blue-400 focus:ring-2 focus:ring-blue-100'
                            } focus:outline-none`}
                        />
                        {errors.landmark && (
                            <p className="text-xs text-red-600 mt-1.5 flex items-center gap-1">
                                <svg xmlns="http://www.w3.org/2000/svg" className="h-3.5 w-3.5" viewBox="0 0 20 20" fill="currentColor">
                                    <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                                </svg>
                                {errors.landmark}
                            </p>
                        )}
                    </div>

                    <div>
                        <label className="block text-sm font-semibold text-neutral-800 mb-2">
                            Pincode <span className="text-rose-500">*</span>
                        </label>
                        <div className="relative">
                            <input
                                type="text"
                                name="pincode"
                                value={formData.pincode || ''}
                                autoComplete="off"
                                onChange={handleChange}
                                maxLength="6"
                                placeholder="6-digit pincode"
                                className={`w-full px-4 py-3 border-2 rounded-lg transition-all duration-200 ${
                                    pincodeError
                                        ? 'border-red-400 bg-red-50'
                                        : 'border-neutral-200 focus:border-blue-400 focus:ring-2 focus:ring-blue-100'
                                } focus:outline-none`}
                            />
                            {isPincodeLoading && (
                                <span className="absolute right-3 top-3 h-6 w-6 border-2 border-t-blue-600 border-r-blue-600 border-b-blue-600 border-l-transparent rounded-full animate-spin"></span>
                            )}
                        </div>

                        {pincodeDetails.area && (
                            <div className="mt-2 p-3 bg-green-50 border border-green-200 rounded-lg animate-fade-in">
                                <p className="text-sm font-medium text-green-800 flex items-center gap-2">
                                    <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                                        <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                                    </svg>
                                    {pincodeDetails.area}, {pincodeDetails.city}
                                </p>
                            </div>
                        )}

                        {(errors.pincode || pincodeError) && (
                            <p className="text-xs text-red-600 mt-1.5 flex items-center gap-1">
                                <svg xmlns="http://www.w3.org/2000/svg" className="h-3.5 w-3.5" viewBox="0 0 20 20" fill="currentColor">
                                    <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                                </svg>
                                {errors.pincode || pincodeError}
                            </p>
                        )}
                    </div>

                    <div></div>

                    <div>
                        <label htmlFor="stateID" className="block text-sm font-semibold text-neutral-800 mb-2">
                            State <span className="text-rose-500">*</span>
                        </label>
                        <select
                            name="stateID"
                            value={formData.stateID || ''}
                            onChange={handleChange}
                            className={`w-full px-4 py-3 border-2 rounded-lg bg-white transition-all duration-200 ${
                                errors.stateID
                                    ? 'border-red-400 bg-red-50'
                                    : 'border-neutral-200 focus:border-blue-400 focus:ring-2 focus:ring-blue-100'
                            } focus:outline-none`}
                        >
                            <option value="">Select State</option>
                            {states.map(s => <option key={s.stateID} value={s.stateID}>{s.stateName}</option>)}
                        </select>
                        {errors.stateID && (
                            <p className="text-xs text-red-600 mt-1.5 flex items-center gap-1">
                                <svg xmlns="http://www.w3.org/2000/svg" className="h-3.5 w-3.5" viewBox="0 0 20 20" fill="currentColor">
                                    <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                                </svg>
                                {errors.stateID}
                            </p>
                        )}
                    </div>

                    <div>
                        <label htmlFor="cityID" className="block text-sm font-semibold text-neutral-800 mb-2">
                            City <span className="text-rose-500">*</span>
                        </label>
                        <select
                            name="cityID"
                            value={formData.cityID || ''}
                            onChange={handleChange}
                            disabled={!formData.stateID || cities.length === 0}
                            className={`w-full px-4 py-3 border-2 rounded-lg bg-white transition-all duration-200 ${
                                errors.cityID
                                    ? 'border-red-400 bg-red-50'
                                    : !formData.stateID || cities.length === 0
                                        ? 'border-neutral-200 bg-neutral-50 cursor-not-allowed'
                                        : 'border-neutral-200 focus:border-blue-400 focus:ring-2 focus:ring-blue-100'
                            } focus:outline-none`}
                        >
                            <option value="">Select City</option>
                            {cities.map(c => <option key={c.cityID} value={c.cityID}>{c.cityName}</option>)}
                        </select>
                        {errors.cityID && (
                            <p className="text-xs text-red-600 mt-1.5 flex items-center gap-1">
                                <svg xmlns="http://www.w3.org/2000/svg" className="h-3.5 w-3.5" viewBox="0 0 20 20" fill="currentColor">
                                    <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                                </svg>
                                {errors.cityID}
                            </p>
                        )}
                    </div>
                </div>
            </section>

            {/* Map Location Section */}
            <section className="bg-white p-6 rounded-xl border-2 border-neutral-100 shadow-sm">
                <div className="flex items-center justify-between mb-6">
                    <div className="flex items-center gap-3">
                        <div className="w-10 h-10 bg-rose-100 rounded-lg flex items-center justify-center">
                            <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6 text-rose-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 20l-5.447-2.724A1 1 0 013 16.382V5.618a1 1 0 011.447-.894L9 7m0 13l6-3m-6 3V7m6 10l4.553 2.276A1 1 0 0021 18.382V7.618a1 1 0 00-.553-.894L15 4m0 13V4m0 0L9 7" />
                            </svg>
                        </div>
                        <div>
                            <h4 className="text-xl font-bold text-neutral-800">Map Location Selector</h4>
                            <p className="text-sm text-neutral-500">Pin your exact location on the map</p>
                        </div>
                    </div>
                    <button
                        type="button"
                        onClick={handleGetCurrentLocation}
                        disabled={useCurrentLocation}
                        className="flex items-center gap-2 px-4 py-2 bg-blue-500 text-white rounded-lg text-sm font-semibold hover:bg-blue-600 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                        {useCurrentLocation ? (
                            <>
                                <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                                Getting...
                            </>
                        ) : (
                            <>
                                <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                                </svg>
                                Use Current Location
                            </>
                        )}
                    </button>
                </div>

                <div className="bg-amber-50 border border-amber-200 rounded-lg p-4 mb-6">
                    <p className="text-sm text-amber-800 flex items-start gap-2">
                        <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5 flex-shrink-0 mt-0.5" viewBox="0 0 20 20" fill="currentColor">
                            <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" />
                        </svg>
                        <span><strong>Note:</strong> Replace 'YOUR_GOOGLE_MAPS_API_KEY' in the code with your actual Google Maps API key. Click on the map or drag the marker to set your exact location. This helps customers find you accurately.</span>
                    </p>
                </div>

                <LocationMapSelector
                    latitude={formData.latitude}
                    longitude={formData.longitude}
                    onLocationSelect={handleLocationSelect}
                />

                <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mt-6">
                    <div>
                        <label className="block text-sm font-semibold text-neutral-800 mb-2">
                            Latitude
                        </label>
                        <input
                            type="text"
                            name="latitude"
                            value={formData.latitude || ''}
                            onChange={handleChange}
                            autoComplete="off"
                            placeholder="Auto-filled from map"
                            className="w-full px-4 py-3 border-2 border-neutral-200 rounded-lg bg-neutral-50 focus:outline-none"
                            readOnly
                        />
                    </div>
                    <div>
                        <label className="block text-sm font-semibold text-neutral-800 mb-2">
                            Longitude
                        </label>
                        <input
                            type="text"
                            name="longitude"
                            value={formData.longitude || ''}
                            onChange={handleChange}
                            autoComplete="off"
                            placeholder="Auto-filled from map"
                            className="w-full px-4 py-3 border-2 border-neutral-200 rounded-lg bg-neutral-50 focus:outline-none"
                            readOnly
                        />
                    </div>
                </div>
            </section>
        </div>
    );
}
