/*
========================================
File: src/components/common/GoogleMapSelector.jsx (NEW FILE)
========================================
A reusable component for selecting a location using Google Maps.
*/
import React, { useState, useEffect, useRef, useCallback } from 'react';

export default function GoogleMapSelector({ onLocationChange, initialCenter = { lat: 23.0225, lng: 72.5714 } }) {
    const mapRef = useRef(null);
    const [map, setMap] = useState(null);
    const [marker, setMarker] = useState(null);
    const [isLargeMap, setIsLargeMap] = useState(false);

    const initMap = useCallback(() => {
        if (mapRef.current && !map) {
            const googleMap = new window.google.maps.Map(mapRef.current, {
                center: initialCenter,
                zoom: 13,
                disableDefaultUI: true,
                zoomControl: true,
            });
            setMap(googleMap);

            const mapMarker = new window.google.maps.Marker({
                position: initialCenter,
                map: googleMap,
                draggable: true,
                title: "Drag me!"
            });
            setMarker(mapMarker);
        }
    }, [mapRef, map, initialCenter]);

    useEffect(() => {
        if (window.google) {
            initMap();
        }
    }, [initMap]);

    useEffect(() => {
        if (marker) {
            window.google.maps.event.addListener(marker, 'dragend', (event) => {
                const lat = event.latLng.lat();
                const lng = event.latLng.lng();
                onLocationChange({ lat, lng });
            });
        }
    }, [marker, onLocationChange]);

    // Recenter map when toggling large view
    useEffect(() => {
        if (map && marker) {
            const currentPos = marker.getPosition();
            window.google.maps.event.trigger(map, 'resize');
            map.setCenter(currentPos);
        }
    }, [isLargeMap, map, marker]);

    return (
        <div className={`transition-all duration-300 ${isLargeMap ? 'fixed inset-0 z-50 bg-white p-4' : 'relative'}`}>
            <div className={`w-full rounded-md overflow-hidden ${isLargeMap ? 'h-full' : 'h-48'}`} ref={mapRef}>
                {/* Map will be rendered here */}
            </div>
            <div className={`flex ${isLargeMap ? 'absolute top-6 right-6' : 'mt-2 justify-end'}`}>
                <button type="button" onClick={() => setIsLargeMap(!isLargeMap)} className="bg-white text-neutral-700 border border-neutral-300 px-3 py-1 rounded-md text-xs font-semibold hover:bg-neutral-100">
                    {isLargeMap ? 'Close Large Map' : 'View Large Map'}
                </button>
            </div>
        </div>
    );
}
