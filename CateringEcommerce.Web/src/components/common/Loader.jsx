/*
========================================
File: src/components/common/Loader.jsx (NEW FILE)
========================================
A reusable loading spinner component.
*/
import React from 'react';

export default function Loader({ text = "Loading..." }) {
    return (
        <div className="flex flex-col items-center justify-center p-8">
            <div className="w-8 h-8 border-4 border-t-rose-600 border-r-rose-600 border-b-rose-600 border-l-transparent rounded-full animate-spin"></div>
            <p className="mt-3 text-sm text-neutral-600">{text}</p>
        </div>
    );
}
