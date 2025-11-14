/*
========================================
File: src/components/common/MultiSelectDropdown.jsx (NEW FILE)
========================================
A reusable multi-select dropdown with checkboxes.
*/
import React, { useState, useRef, useEffect } from 'react';

export default function MultiSelectDropdown({ placeholder, options, selectedIds, onChange }) {
    const [isOpen, setIsOpen] = useState(false);
    const wrapperRef = useRef(null);

    // Handle clicks outside the dropdown to close it
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

    const handleSelect = (id) => {
        let newSelectedIds;
        if (selectedIds.includes(id)) {
            newSelectedIds = selectedIds.filter(item => item !== id);
        } else {
            newSelectedIds = [...selectedIds, id];
        }
        onChange(newSelectedIds);
    };

    const handleClear = (e) => {
        e.stopPropagation();
        onChange([]);
    };

    const getButtonText = () => {
        if (selectedIds.length === 0) return placeholder;
        if (selectedIds.length === 1) {
            const selectedOption = options.find(opt => opt.id === selectedIds[0]);
            return selectedOption ? selectedOption.name : '1 selected';
        }
        return `${selectedIds.length} selected`;
    };

    return (
        <div className="relative w-full" ref={wrapperRef}>
            <button
                type="button"
                onClick={() => setIsOpen(!isOpen)}
                className="w-full px-3 py-2 border border-neutral-300 rounded-md shadow-sm bg-white text-left flex items-center justify-between focus:outline-none focus:ring-1 focus:ring-rose-500 focus:border-rose-500"
            >
                <span className="truncate text-sm font-medium text-neutral-700">{getButtonText()}</span>
                <svg className={`w-5 h-5 text-neutral-400 transform transition-transform ${isOpen ? 'rotate-180' : ''}`} xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
                    <path fillRule="evenodd" d="M5.293 7.293a1 1 0 011.414 0L10 10.586l3.293-3.293a1 1 0 111.414 1.414l-4 4a1 1 0 01-1.414 0l-4-4a1 1 0 010-1.414z" clipRule="evenodd" />
                </svg>
            </button>

            {isOpen && (
                <div className="absolute z-10 w-full mt-1 bg-white rounded-md shadow-lg border border-neutral-200 max-h-60 overflow-y-auto">
                    <ul className="py-1">
                        {options.map(option => (
                            <li
                                key={option.id}
                                onClick={() => handleSelect(option.id)}
                                className="px-3 py-2 text-sm text-neutral-700 cursor-pointer hover:bg-neutral-100 flex items-center"
                            >
                                <input
                                    type="checkbox"
                                    checked={selectedIds.includes(option.id)}
                                    readOnly
                                    className="h-4 w-4 text-rose-600 border-neutral-300 rounded mr-3"
                                />
                                {option.name}
                            </li>
                        ))}
                    </ul>
                    {selectedIds.length > 0 && (
                        <div className="border-t p-2">
                            <button
                                type="button"
                                onClick={handleClear}
                                className="w-full text-center text-sm font-medium text-rose-600 hover:text-rose-800"
                            >
                                Clear All
                            </button>
                        </div>
                    )}
                </div>
            )}
        </div>
    );
}