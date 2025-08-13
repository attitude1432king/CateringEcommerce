/*
========================================
File: src/components/common/Tooltip.jsx (NEW FILE)
========================================
*/
import React from 'react';

export const Tooltip = ({ children, text }) => {
    return (
        <div className="relative flex items-center group">
            {children}
            <div className="absolute bottom-full mb-2 w-48 bg-neutral-800 text-white text-xs rounded-md p-2 opacity-0 group-hover:opacity-100 transition-opacity pointer-events-none z-10">
                {text}
            </div>
        </div>
    );
};