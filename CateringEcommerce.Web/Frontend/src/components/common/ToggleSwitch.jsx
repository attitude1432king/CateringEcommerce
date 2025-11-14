/*
========================================
File: src/components/common/ToggleSwitch.jsx (NEW FILE)
========================================
A reusable, styled toggle switch component.
*/
import React from 'react';

export default function ToggleSwitch({ label, enabled, setEnabled }) {
    return (
        <label className="flex items-center cursor-pointer">
            <span className="mr-3 text-sm font-medium text-neutral-700">{label}</span>
            <div className="relative">
                <input
                    type="checkbox"
                    className="sr-only"
                    checked={enabled}
                    onChange={() => setEnabled(!enabled)}
                />
                <div className={`block w-11 h-6 rounded-full transition-colors ${enabled ? 'bg-rose-600' : 'bg-neutral-300'}`}></div>
                <div className={`dot absolute left-1 top-1 bg-white w-4 h-4 rounded-full transition-transform ${enabled ? 'translate-x-5' : ''}`}></div>
            </div>
        </label>
    );
}