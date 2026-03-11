/*
========================================
File: src/components/common/ToggleSwitch.jsx (REVISED FOR 508/ACCESSIBILITY)
========================================
A reusable, styled, and accessible toggle switch component.
It is now focusable and can be toggled with Enter or Space.
*/
import React from 'react';

export default function ToggleSwitch({ label, enabled, setEnabled }) {

    // Handle key press for accessibility
    const handleKeyDown = (e) => {
        if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault();
            setEnabled(!enabled);
        }
    };

    return (
        <label className="flex items-center cursor-pointer">
            <span className="mr-3 text-sm font-medium text-neutral-700">{label}</span>
            <button
                type="button"
                role="switch"
                aria-checked={enabled}
                onClick={() => setEnabled(!enabled)}
                onKeyDown={handleKeyDown}
                className={`relative inline-flex items-center h-6 w-11 rounded-full transition-colors duration-200 focus:outline-none focus:ring-2 focus:ring-rose-500 focus:ring-offset-2 ${enabled ? 'bg-rose-600' : 'bg-neutral-300'
                    }`}
            >
                <span className="sr-only">{label}</span>
                <span
                    className={`inline-block w-4 h-4 transform bg-white rounded-full transition-transform duration-200 ${enabled ? 'translate-x-6' : 'translate-x-1'
                        }`}
                />
            </button>
        </label>
    );
}