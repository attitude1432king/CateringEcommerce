/*
========================================
File: src/components/common/CustomSelectDropdown.jsx (NEW FILE)
========================================
A reusable, styled single-select dropdown to replace the native <select>
*/
import React, { useState, useRef, useEffect } from 'react';

export default function CustomSelectDropdown({ options, value, onChange }) {
    const [isOpen, setIsOpen] = useState(false);
    const wrapperRef = useRef(null);

    useEffect(() => {
        function handleClickOutside(event) {
            if (wrapperRef.current && !wrapperRef.current.contains(event.target)) {
                setIsOpen(false);
            }
        }
        document.addEventListener("mousedown", handleClickOutside);
        return () => {
            document.removeEventListener("mousedown", handleClickOutside);
        };
    }, [wrapperRef]);

    const handleSelect = (newValue) => {
        onChange(newValue);
        setIsOpen(false);
    };

    const selectedOption = options.find(opt => opt.id === value) || options[0];

    return (
        <div className="relative w-full" ref={wrapperRef}>
            <button
                type="button"
                onClick={() => setIsOpen(!isOpen)}
                className="w-full px-3 py-1 border border-neutral-300 rounded-md shadow-sm bg-white text-left flex items-center justify-between focus:outline-none focus:ring-1 focus:ring-rose-500 focus:border-rose-500"
            >
                <span className="truncate text-sm font-medium text-neutral-700">{selectedOption.name}</span>
                <svg className="w-5 h-5 text-neutral-400 transform transition-transform" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
                    <path fillRule="evenodd" d="M5.293 7.293a1 1 0 011.414 0L10 10.586l3.293-3.293a1 1 0 111.414 1.414l-4 4a1 1 0 01-1.414 0l-4-4a1 1 0 010-1.414z" clipRule="evenodd" />
                </svg>
            </button>

            {isOpen && (
                <div className="absolute z-10 w-full bottom-full mb-1 bg-white rounded-md shadow-lg border border-neutral-200 max-h-60 overflow-y-auto">
                    <ul className="py-1">
                        {options.map(option => (
                            <li
                                key={option.id}
                                onClick={() => handleSelect(option.id)}
                                className={`px-3 py-2 text-sm cursor-pointer ${option.id === value ? 'font-bold bg-rose-50 text-rose-600' : 'text-neutral-700 hover:bg-neutral-100'}`}
                            >
                                {option.name}
                            </li>
                        ))}
                    </ul>
                </div>
            )}
        </div>
    );
}
