import { createContext, useContext, useState, useEffect } from 'react';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:44368';

const AppSettingsContext = createContext({});

export const AppSettingsProvider = ({ children }) => {
    const [settings, setSettings] = useState({});
    const [loaded, setLoaded] = useState(false);

    useEffect(() => {
        loadSettings();
    }, []);

    const loadSettings = async () => {
        try {
            const response = await fetch(`${API_BASE_URL}/api/app-settings`, {
                credentials: 'include',
            });
            const data = await response.json();
            if (data.result) {
                setSettings(data.data);
            }
        } catch (error) {
            console.error('Failed to load app settings:', error);
        } finally {
            setLoaded(true);
        }
    };

    const getSetting = (key, defaultValue = '') => settings[key] ?? defaultValue;
    const getBool = (key, defaultValue = false) => getSetting(key, String(defaultValue)) === 'true';
    const getInt = (key, defaultValue = 0) => parseInt(getSetting(key, String(defaultValue))) || defaultValue;
    const getDecimal = (key, defaultValue = 0) => parseFloat(getSetting(key, String(defaultValue))) || defaultValue;

    return (
        <AppSettingsContext.Provider
            value={{
                settings,
                loaded,
                getSetting,
                getBool,
                getInt,
                getDecimal,
                refreshSettings: loadSettings,
            }}
        >
            {children}
        </AppSettingsContext.Provider>
    );
};

export const useAppSettings = () => useContext(AppSettingsContext);

export default AppSettingsContext;
