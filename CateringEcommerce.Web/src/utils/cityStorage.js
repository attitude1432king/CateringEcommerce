
export const CITY_KEY = 'selected_city';

// getStoredCity: function to retrieve the stored city from local storage
export const getStoredCity = () => {
    return localStorage.getItem(CITY_KEY);
};

// storeCity: function to store the selected city in local storage
export const storeCity = (city) => {
    if (city) {
        localStorage.setItem(CITY_KEY, city);
    }
};

// clearCity: function to remove the stored city from local storage
export const clearCity = () => {
    localStorage.removeItem(CITY_KEY);
};
