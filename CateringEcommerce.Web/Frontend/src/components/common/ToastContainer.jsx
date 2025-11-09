/*
========================================
File: src/components/common/ToastContainer.jsx (NEW FILE)
========================================
Manages the positioning and rendering of all active toast notifications.
*/
import React from 'react';
import Toast from './Toast';

export default function ToastContainer({ toasts, removeToast }) {
    return (
        <div className="fixed top-20 right-4 z-[100] w-full max-w-sm space-y-3">
            {toasts.map(toast => (
                <Toast
                    key={toast.id}
                    message={toast.message}
                    type={toast.type}
                    onClose={() => removeToast(toast.id)}
                />
            ))}
        </div>
    );
}