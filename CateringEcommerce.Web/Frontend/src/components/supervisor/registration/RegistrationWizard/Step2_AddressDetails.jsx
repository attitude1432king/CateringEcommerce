/**
 * Step 2: Address Details (REDESIGNED)
 * Shop/Building, Floor, Area/Landmark, Pincode, State, City
 */

import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import { MapPin, ArrowRight, ArrowLeft } from 'lucide-react';
import { addressDetailsSchema } from '../../../../utils/supervisor/validationSchemas';
import { apiService } from '../../../../services/userApi';
import { ownerApiService } from '../../../../services/ownerApi';

const Step2_AddressDetails = ({ data, onNext, onBack }) => {
    const [isPincodeLoading, setIsPincodeLoading] = useState(false);
    const [pincodeDetails, setPincodeDetails] = useState({ area: '', city: '' });
    const [pincodeError, setPincodeError] = useState('');
    const [states, setStates] = useState([]);
    const [cities, setCities] = useState([]);

    const {
        register,
        handleSubmit,
        formState: { errors },
        watch,
        setValue,
    } = useForm({
        resolver: zodResolver(addressDetailsSchema),
        defaultValues: data,
    });

    const pincode = watch('pincode');
    const stateID = watch('stateID');

    // Fetch states on mount
    useEffect(() => {
        const fetchStates = async () => {
            try {
                const statesData = await apiService.getStates();
                setStates(statesData);
            } catch (err) {
                console.error("Failed to fetch states", err);
            }
        };
        fetchStates();
    }, []);

    // Fetch cities when state changes
    useEffect(() => {
        if (stateID) {
            const fetchCities = async () => {
                try {
                    const citiesData = await apiService.getCities(stateID);
                    setCities(citiesData);
                } catch (err) {
                    console.error("Failed to fetch cities", err);
                    setCities([]);
                }
            };
            fetchCities();
        } else {
            setCities([]);
            setValue('cityID', undefined);
        }
    }, [stateID, setValue]);

    // Fetch pincode details
    useEffect(() => {
        const fetchPincodeDetails = async () => {
            if (pincode && pincode.length === 6 && /^\d+$/.test(pincode)) {
                setIsPincodeLoading(true);
                setPincodeError('');

                try {
                    const details = await ownerApiService.getPincodeDetails(pincode);

                    if (details && details[0].Status === 'Success') {
                        const postOffice = details[0].PostOffice[0];
                        setPincodeDetails({ 
                            area: postOffice.Name, 
                            city: postOffice.District 
                        });
                    } else {
                        setPincodeError('Invalid Pincode. Please check and try again.');
                        setPincodeDetails({ area: '', city: '' });
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
    }, [pincode]);

    return (
        <form onSubmit={handleSubmit(onNext)} className="space-y-6">
            {/* Section Header */}
            <div className="flex items-center gap-3 mb-6">
                <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
                    <MapPin className="w-5 h-5 text-blue-600" />
                </div>
                <div>
                    <h2 className="text-xl font-bold text-neutral-800">Address Details</h2>
                    <p className="text-sm text-neutral-500">Where is your catering business located?</p>
                </div>
            </div>

            <div className="space-y-5">
                {/* Address */}
                <div>
                    <label className="block text-sm font-semibold text-neutral-800 mb-2">
                        Complete Address <span className="text-rose-500">*</span>
                    </label>
                    <div className="relative">
                        <MapPin className="absolute left-3.5 top-3 w-5 h-5 text-neutral-400" />
                        <textarea
                            {...register('address')}
                            rows={4}
                            className={`w-full pl-11 pr-4 py-3 border-2 rounded-xl transition-all duration-200 ${
                                errors.address ? 'border-red-400 bg-red-50' : 'border-neutral-200 focus:border-blue-400 focus:ring-2 focus:ring-blue-100'
                            } focus:outline-none`}
                            placeholder="Enter your complete address"
                        />
                    </div>
                    {errors.address && <p className="text-xs text-red-600 mt-1.5">{errors.address.message}</p>}
                </div>

                {/* Pincode */}
                <div>
                    <label className="block text-sm font-semibold text-neutral-800 mb-2">
                        Pincode <span className="text-rose-500">*</span>
                    </label>
                    <div className="relative">
                        <input
                            type="text"
                            inputMode="numeric"
                            {...register('pincode', {
                                setValueAs: (value) => String(value ?? '').replace(/\D/g, '').slice(0, 6),
                            })}
                            maxLength={6}
                            className={`w-full px-4 py-3 border-2 rounded-xl transition-all duration-200 ${
                                pincodeError
                                    ? 'border-red-400 bg-red-50'
                                    : 'border-neutral-200 focus:border-blue-400 focus:ring-2 focus:ring-blue-100'
                            } focus:outline-none`}
                            placeholder="6-digit pincode"
                        />
                        {isPincodeLoading && (
                            <span className="absolute right-3 top-3 h-6 w-6 border-2 border-t-blue-600 border-r-blue-600 border-b-blue-600 border-l-transparent rounded-full animate-spin"></span>
                        )}
                    </div>

                    {pincodeDetails.area && (
                        <div className="mt-2 p-3 bg-green-50 border border-green-200 rounded-lg">
                            <p className="text-sm font-medium text-green-800">
                                ✓ {pincodeDetails.area}, {pincodeDetails.city}
                            </p>
                        </div>
                    )}

                    {(errors.pincode || pincodeError) && (
                        <p className="text-xs text-red-600 mt-1.5">{errors.pincode?.message || pincodeError}</p>
                    )}
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                    {/* State */}
                    <div>
                        <label className="block text-sm font-semibold text-neutral-800 mb-2">
                            State <span className="text-rose-500">*</span>
                        </label>
                        <select
                            {...register('stateID', {
                                setValueAs: (value) => (value === '' ? undefined : Number(value)),
                            })}
                            className={`w-full px-4 py-3 border-2 rounded-xl transition-all duration-200 appearance-none bg-white ${
                                errors.stateID ? 'border-red-400 bg-red-50' : 'border-neutral-200 focus:border-blue-400 focus:ring-2 focus:ring-blue-100'
                            } focus:outline-none`}
                        >
                            <option value="">Select State</option>
                            {states.map((s) => (
                                <option key={s.stateID ?? s.stateId} value={s.stateID ?? s.stateId}>
                                    {s.stateName ?? s.statename}
                                </option>
                            ))}
                        </select>
                        {errors.stateID && <p className="text-xs text-red-600 mt-1.5">{errors.stateID.message}</p>}
                    </div>

                    {/* City */}
                    <div>
                        <label className="block text-sm font-semibold text-neutral-800 mb-2">
                            City <span className="text-rose-500">*</span>
                        </label>
                        <select
                            {...register('cityID', {
                                setValueAs: (value) => (value === '' ? undefined : Number(value)),
                            })}
                            disabled={!stateID || cities.length === 0}
                            className={`w-full px-4 py-3 border-2 rounded-xl transition-all duration-200 appearance-none bg-white ${
                                errors.cityID
                                    ? 'border-red-400 bg-red-50'
                                    : !stateID || cities.length === 0
                                        ? 'border-neutral-200 bg-neutral-50 cursor-not-allowed'
                                        : 'border-neutral-200 focus:border-blue-400 focus:ring-2 focus:ring-blue-100'
                            } focus:outline-none`}
                        >
                            <option value="">Select City</option>
                            {cities.map((c) => (
                                <option key={c.cityID ?? c.cityId} value={c.cityID ?? c.cityId}>
                                    {c.cityName ?? c.cityname}
                                </option>
                            ))}
                        </select>
                        {errors.cityID && <p className="text-xs text-red-600 mt-1.5">{errors.cityID.message}</p>}
                    </div>
                </div>
            </div>

            {/* Actions */}
            <div className="flex gap-4 pt-4">
                <button type="button" onClick={onBack} className="flex-1 px-6 py-3.5 bg-white text-neutral-700 rounded-xl font-semibold border-2 border-neutral-200 hover:border-neutral-300 hover:bg-neutral-50 transition-all duration-200 flex items-center justify-center gap-2">
                    <ArrowLeft className="w-4 h-4" /> Back
                </button>
                <button type="submit" className="flex-1 px-6 py-3.5 bg-gradient-to-r from-blue-600 to-blue-500 text-white rounded-xl font-semibold shadow-lg hover:shadow-xl transition-all duration-200 flex items-center justify-center gap-2">
                    Next Step <ArrowRight className="w-4 h-4" />
                </button>
            </div>
        </form>
    );
};

Step2_AddressDetails.propTypes = {
    data: PropTypes.object,
    onNext: PropTypes.func.isRequired,
    onBack: PropTypes.func.isRequired,
};

export default Step2_AddressDetails;
