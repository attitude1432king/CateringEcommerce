/*
========================================
File: src/components/common/Toast.jsx (NEW FILE)
========================================
The visual component for a single toast notification.
*/
import React, { useEffect, useState } from 'react';

const icons = {
    success: (
        <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
    ),
    error: (
        <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
    ),
    warning: (
        <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
        </svg>
    ),
    info: (
        <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
    ),
};

const styles = {
    success: { bg: 'bg-green-50', text: 'text-green-800', icon: 'text-green-500' },
    error: { bg: 'bg-red-50', text: 'text-red-800', icon: 'text-red-500' },
    warning: { bg: 'bg-yellow-50', text: 'text-yellow-800', icon: 'text-yellow-500' },
    info: { bg: 'bg-blue-50', text: 'text-blue-800', icon: 'text-blue-500' },
};

export default function Toast({ message, type = 'info', onClose }) {
    const [isVisible, setIsVisible] = useState(false);
    const style = styles[type] || styles.info;

    useEffect(() => {
        // Mount animation
        setIsVisible(true);
    }, []);

    const handleClose = () => {
        setIsVisible(false);
        // Allow time for fade-out animation before calling the remove function
        setTimeout(onClose, 300);
    };

    return (
        <div
            className={`
                flex items-start p-4 rounded-xl shadow-lg w-full transition-all duration-300 transform
                ${style.bg}
                ${isVisible ? 'opacity-100 translate-x-0' : 'opacity-0 translate-x-full'}
            `}
        >
            <div className={`flex-shrink-0 ${style.icon}`}>
                {icons[type]}
            </div>
            <div className="ml-3 flex-1">
                <p className={`text-sm font-medium ${style.text}`}>
                    {message}
                </p>
            </div>
            <div className="ml-4 flex-shrink-0">
                <button onClick={handleClose} className={`inline-flex rounded-md p-1 ${style.text} opacity-70 hover:opacity-100 focus:outline-none`}>
                    <span className="sr-only">Close</span>
                    <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                    </svg>
                </button>
            </div>
        </div>
    );
}