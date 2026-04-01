import { useEffect, useState, useCallback } from 'react';
import { locationApiService } from '../services/locationApi';
import { getStoredCity, storeCity } from '../utils/cityStorage';

export function useDefaultCity() {
    const [city, setCityState] = useState(null);
    const [loading, setLoading] = useState(true);

    // 🔹 On App Load
    useEffect(() => {
        const localCity = getStoredCity();

        if (localCity) {
            setCityState(localCity);
            setLoading(false);
            return;
        }

        locationApiService.fetchDefaultCity()
            .then((data) => {
                if (data?.city) {
                    storeCity(data.city);
                    setCityState(data.city);
                }
            })
            .catch(() => {
                // fallback if API fails
                setCityState('Surat');
            })
            .finally(() => setLoading(false));
    }, []);

    // 🔹 Manual city change
    const setCity = useCallback(async (newCity) => {
        storeCity(newCity);
        setCityState(newCity);

        try {
            await locationApiService.updateCityOnBackend(newCity);
        } catch {
            // silent fail (UX should not break)
        }
    }, []);

    return {
        city,
        loading,
        setCity
    };
}
