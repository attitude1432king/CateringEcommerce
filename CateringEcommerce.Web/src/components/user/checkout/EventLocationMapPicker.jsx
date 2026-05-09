import React, { useState, useEffect, useRef } from 'react';
import { loadGoogleMapsAPI } from '../../../utils/googleMaps';

const EventLocationMapPicker = ({ value, onChange, error }) => {
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState(null);
  const [selectedPlace, setSelectedPlace] = useState(null);
  const [mapCenter, setMapCenter] = useState({ lat: 18.5204, lng: 73.8567 }); // Default: Pune, India

  const mapRef = useRef(null);
  const mapInstanceRef = useRef(null);
  const markerRef = useRef(null);
  const autocompleteInputRef = useRef(null);
  const autocompleteRef = useRef(null);

  useEffect(() => {
    const initMap = async () => {
      try {
        setIsLoading(true);
        await loadGoogleMapsAPI();

        // Initialize map
        const map = new window.google.maps.Map(mapRef.current, {
          center: mapCenter,
          zoom: 13,
          mapTypeControl: true,
          streetViewControl: false,
          fullscreenControl: true,
          zoomControl: true,
        });
        mapInstanceRef.current = map;

        // Add marker
        const marker = new window.google.maps.Marker({
          map: map,
          draggable: true,
          position: mapCenter,
        });
        markerRef.current = marker;

        // Handle marker drag
        marker.addListener('dragend', async (event) => {
          const position = {
            lat: event.latLng.lat(),
            lng: event.latLng.lng()
          };
          await reverseGeocode(position);
        });

        // Initialize autocomplete
        const autocomplete = new window.google.maps.places.Autocomplete(
          autocompleteInputRef.current,
          {
            componentRestrictions: { country: 'in' }, // Restrict to India
            fields: ['formatted_address', 'geometry', 'name', 'place_id'],
            types: ['establishment', 'geocode']
          }
        );
        autocompleteRef.current = autocomplete;

        // Handle place selection from autocomplete
        autocomplete.addListener('place_changed', () => {
          const place = autocomplete.getPlace();

          if (!place.geometry || !place.geometry.location) {
            return;
          }

          const position = {
            lat: place.geometry.location.lat(),
            lng: place.geometry.location.lng()
          };

          // Update map and marker
          map.setCenter(position);
          map.setZoom(15);
          marker.setPosition(position);

          // Update state and parent
          const locationData = {
            address: place.formatted_address || place.name,
            lat: position.lat,
            lng: position.lng,
            placeId: place.place_id
          };

          setSelectedPlace(locationData);
          setMapCenter(position);
          onChange(locationData);
        });

        // Try to get user's current location
        if (navigator.geolocation && !value) {
          navigator.geolocation.getCurrentPosition(
            (position) => {
              const userLocation = {
                lat: position.coords.latitude,
                lng: position.coords.longitude
              };
              map.setCenter(userLocation);
              marker.setPosition(userLocation);
              setMapCenter(userLocation);
            },
            (error) => {
              console.warn('Geolocation error:', error);
            }
          );
        }

        // If there's an existing value, set it
        if (value && typeof value === 'object' && value.lat && value.lng) {
          const position = { lat: value.lat, lng: value.lng };
          map.setCenter(position);
          marker.setPosition(position);
          setMapCenter(position);
          setSelectedPlace(value);
          if (value.address) {
            autocompleteInputRef.current.value = value.address;
          }
        } else if (value && typeof value === 'string') {
          autocompleteInputRef.current.value = value;
        }

        setIsLoading(false);
      } catch (error) {
        console.error('Error initializing Google Maps:', error);
        setLoadError('Failed to load map. Please check your internet connection.');
        setIsLoading(false);
      }
    };

    initMap();

    return () => {
      // Cleanup
      if (markerRef.current) {
        markerRef.current.setMap(null);
      }
      if (autocompleteRef.current) {
        window.google?.maps?.event?.clearInstanceListeners(autocompleteRef.current);
      }
    };
  }, []);

  const reverseGeocode = async (position) => {
    try {
      const geocoder = new window.google.maps.Geocoder();
      const response = await geocoder.geocode({ location: position });

      if (response.results && response.results[0]) {
        const place = response.results[0];
        const locationData = {
          address: place.formatted_address,
          lat: position.lat,
          lng: position.lng,
          placeId: place.place_id
        };

        setSelectedPlace(locationData);
        autocompleteInputRef.current.value = place.formatted_address;
        onChange(locationData);
      }
    } catch (error) {
      console.error('Reverse geocoding error:', error);
    }
  };

  const handleCurrentLocation = () => {
    if (!navigator.geolocation) {
      alert('Geolocation is not supported by your browser');
      return;
    }

    navigator.geolocation.getCurrentPosition(
      async (position) => {
        const userLocation = {
          lat: position.coords.latitude,
          lng: position.coords.longitude
        };

        if (mapInstanceRef.current) {
          mapInstanceRef.current.setCenter(userLocation);
          mapInstanceRef.current.setZoom(15);
        }

        if (markerRef.current) {
          markerRef.current.setPosition(userLocation);
        }

        setMapCenter(userLocation);
        await reverseGeocode(userLocation);
      },
      (error) => {
        alert('Unable to retrieve your location: ' + error.message);
      }
    );
  };

  if (loadError) {
    return (
      <div className="bg-red-50 border border-red-200 rounded-lg p-4 text-red-800">
        <p className="font-medium">Map Loading Error</p>
        <p className="text-sm mt-1">{loadError}</p>
      </div>
    );
  }

  return (
    <div className="space-y-3">
      {/* Search Input */}
      <div className="relative">
        <input
          ref={autocompleteInputRef}
          type="text"
          placeholder="Search for location (e.g., Hotel Taj, Pune)"
          className={`w-full px-4 py-3 pr-12 border rounded-lg focus:ring-2 focus:ring-red-500 focus:border-transparent ${
            error ? 'border-red-500' : 'border-gray-300'
          }`}
          disabled={isLoading}
        />
        <button
          type="button"
          onClick={handleCurrentLocation}
          className="absolute right-2 top-1/2 -translate-y-1/2 p-2 text-neutral-500 hover:text-red-500 transition-colors"
          title="Use current location"
          disabled={isLoading}
        >
          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
          </svg>
        </button>
      </div>

      {/* Map Container */}
      <div className="relative">
        {isLoading && (
          <div className="absolute inset-0 bg-gray-100 rounded-lg flex items-center justify-center z-10">
            <div className="text-center">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-red-500 mx-auto mb-3"></div>
              <p className="text-neutral-600">Loading map...</p>
            </div>
          </div>
        )}
        <div
          ref={mapRef}
          className="w-full h-80 rounded-lg border border-gray-300"
          style={{ minHeight: '320px' }}
        />
      </div>

      {/* Selected Location Display */}
      {selectedPlace && selectedPlace.address && (
        <div className="bg-green-50 border border-green-200 rounded-lg p-3">
          <div className="flex items-start">
            <svg className="w-5 h-5 text-green-600 mt-0.5 mr-2 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <div className="flex-1">
              <p className="text-sm font-medium text-green-800">Selected Location:</p>
              <p className="text-sm text-green-700 mt-1">{selectedPlace.address}</p>
            </div>
          </div>
        </div>
      )}

      {error && (
        <p className="text-sm text-red-600">{error}</p>
      )}

      <p className="text-xs text-neutral-500">
        Search for your event location or drag the marker to the exact spot. You can also use the location icon to use your current location.
      </p>
    </div>
  );
};

export default EventLocationMapPicker;
