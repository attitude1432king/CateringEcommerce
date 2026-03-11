/**
 * Google Maps API loader utility
 */

let isLoaded = false;
let loadPromise = null;

export const loadGoogleMapsAPI = () => {
  // Return existing promise if already loading
  if (loadPromise) {
    return loadPromise;
  }

  // Return resolved promise if already loaded
  if (isLoaded && window.google && window.google.maps) {
    return Promise.resolve();
  }

  // Create new loading promise
  loadPromise = new Promise((resolve, reject) => {
    // Check if script is already in DOM
    const existingScript = document.querySelector('script[src*="maps.googleapis.com"]');
    if (existingScript) {
      existingScript.addEventListener('load', () => {
        isLoaded = true;
        resolve();
      });
      return;
    }

    // Create and load script
    const script = document.createElement('script');
    const apiKey = import.meta.env.VITE_GOOGLE_MAPS_API_KEY || 'YOUR_GOOGLE_MAPS_API_KEY';
    script.src = `https://maps.googleapis.com/maps/api/js?key=${apiKey}&libraries=places`;
    script.async = true;
    script.defer = true;

    script.addEventListener('load', () => {
      isLoaded = true;
      resolve();
    });

    script.addEventListener('error', (error) => {
      loadPromise = null;
      reject(new Error('Failed to load Google Maps API'));
    });

    document.head.appendChild(script);
  });

  return loadPromise;
};

export const isGoogleMapsLoaded = () => {
  return isLoaded && window.google && window.google.maps;
};
