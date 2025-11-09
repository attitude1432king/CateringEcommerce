/*
========================================
File: src/components/common/ConfirmationModal.jsx (NEW FILE)
========================================
A reusable, professionally designed confirmation modal component.
*/
import React, { useEffect, useState } from 'react';
import ReactDOM from 'react-dom';

// SVG Icons for different types
const ICONS = {
    delete: (
        <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6 text-red-600" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
        </svg>
    ),
    warning: (
        <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6 text-amber-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
        </svg>
    ),
    info: (
        <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6 text-blue-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
    ),
};

// Style configurations for different types
const STYLES = {
    delete: {
        iconBg: 'bg-red-100',
        confirmButton: 'bg-red-600 hover:bg-red-700 focus:ring-red-500',
    },
    warning: {
        iconBg: 'bg-amber-100',
        confirmButton: 'bg-amber-500 hover:bg-amber-600 focus:ring-amber-400',
    },
    info: {
        iconBg: 'bg-blue-100',
        confirmButton: 'bg-blue-600 hover:bg-blue-700 focus:ring-blue-500',
    },
};

export default function ConfirmationModal({
    isOpen,
    onClose,
    onConfirm,
    title = 'Are you sure?',
    message = '',
    type = 'info', // 'delete', 'warning', 'info'
    confirmText = 'Confirm',
    cancelText = 'Cancel',
}) {
    const [isRendered, setIsRendered] = useState(false);
    const styles = STYLES[type] || STYLES.info;
    const icon = ICONS[type] || ICONS.info;

    useEffect(() => {
        if (isOpen) {
            setIsRendered(true);
        }
    }, [isOpen]);

    const handleAnimationEnd = () => {
        if (!isOpen) {
            setIsRendered(false);
        }
    };

    if (!isRendered) return null;

    return ReactDOM.createPortal(
        <div
            className={`fixed inset-0 z-50 flex items-center justify-center p-4 transition-opacity duration-300 ${isOpen ? 'opacity-100' : 'opacity-0'}`}
            onAnimationEnd={handleAnimationEnd}
        >
            <div className="fixed inset-0 bg-black bg-opacity-60" onClick={onClose}></div>
            <div
                className={`relative bg-white rounded-xl shadow-2xl w-full max-w-md m-4 transform transition-all duration-300 ${isOpen ? 'scale-100 opacity-100' : 'scale-95 opacity-0'}`}
            >
                <div className="p-6 text-center">
                    <div className={`mx-auto flex items-center justify-center h-12 w-12 rounded-full ${styles.iconBg}`}>
                        {icon}
                    </div>
                    <h3 className="mt-5 text-xl font-bold text-neutral-900">{title}</h3>
                    <div className="mt-2">
                        <p className="text-sm text-neutral-600">{message}</p>
                    </div>
                </div>
                <div className="bg-neutral-50 px-6 py-4 flex flex-col sm:flex-row-reverse gap-3 rounded-b-xl">
                    <button
                        type="button"
                        className={`w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 text-base font-medium text-white focus:outline-none focus:ring-2 focus:ring-offset-2 sm:w-auto sm:text-sm ${styles.confirmButton}`}
                        onClick={onConfirm}
                    >
                        {confirmText}
                    </button>
                    <button
                        type="button"
                        className="w-full inline-flex justify-center rounded-md border border-neutral-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-neutral-700 hover:bg-neutral-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-neutral-200 sm:w-auto sm:text-sm"
                        onClick={onClose}
                    >
                        {cancelText}
                    </button>
                </div>
            </div>
        </div>,
        document.getElementById('portal-root')
    );
}