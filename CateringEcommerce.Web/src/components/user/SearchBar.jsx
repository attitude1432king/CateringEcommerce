import React, { useRef, useEffect } from 'react';

export default function SearchBar({ onSearch }) {
    const inputRef = useRef(null);

    useEffect(() => {
        if (window.google && window.google.maps && window.google.maps.places && inputRef.current) {
            try {
                const ac = new window.google.maps.places.Autocomplete(inputRef.current, { types: ['geocode'] });
                ac.setFields(['formatted_address', 'geometry', 'place_id']);
                ac.addListener('place_changed', () => {
                    const place = ac.getPlace();
                    if (onSearch) onSearch({ address: place.formatted_address, place });
                });
            } catch (e) {
                // Places API not available or API key missing in this environment
                // Fall back to basic input behaviour
            }
        }
    }, [onSearch]);

    const handleSubmit = (e) => {
        e.preventDefault();
        if (onSearch && inputRef.current) onSearch({ address: inputRef.current.value });
    };

    return (
        <form onSubmit={handleSubmit} className="flex items-center gap-3">
            <div className="relative flex-1">
                <input
                    ref={inputRef}
                    aria-label="Search location or caterer"
                    placeholder="Search caterers, cuisines or locations"
                    className="w-full pl-4 pr-4 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-amber-300"
                />
                <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                    <svg className="h-5 w-5 text-neutral-400" viewBox="0 0 20 20" fill="currentColor"><path fillRule="evenodd" d="M8 4a4 4 0 100 8 4 4 0 000-8zM2 8a6 6 0 1110.89 3.476l4.817 4.817a1 1 0 01-1.414 1.414l-4.816-4.816A6 6 0 012 8z" clipRule="evenodd" /></svg>
                </div>
            </div>

            <button type="submit" className="bg-rose-600 hover:bg-rose-700 text-white px-4 py-2 rounded-md">Search</button>
        </form>
    );
}