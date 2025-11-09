/*
========================================
File: src/contexts/ConfirmationContext.jsx (NEW FILE)
========================================
This context and hook provide a global way to show a confirmation modal.
*/
import React, { createContext, useContext, useState, useCallback } from 'react';
import ConfirmationModal from '../components/common/ConfirmationModal';

// Create the context
const ConfirmationContext = createContext();

// Create a provider component
export const ConfirmationProvider = ({ children }) => {
    const [options, setOptions] = useState(null);
    const [resolvePromise, setResolvePromise] = useState(null);

    const confirm = useCallback((options) => {
        return new Promise((resolve) => {
            setOptions(options);
            setResolvePromise(() => resolve); // Store the resolve function
        });
    }, []);

    const handleClose = () => {
        if (resolvePromise) {
            resolvePromise(false); // Resolve with false when closing without confirming
        }
        setOptions(null);
        setResolvePromise(null);
    };

    const handleConfirm = () => {
        if (resolvePromise) {
            resolvePromise(true); // Resolve with true on confirmation
        }
        setOptions(null);
        setResolvePromise(null);
    };

    return (
        <ConfirmationContext.Provider value={confirm}>
            {children}
            {options && (
                <ConfirmationModal
                    isOpen={!!options}
                    onClose={handleClose}
                    onConfirm={handleConfirm}
                    {...options}
                />
            )}
        </ConfirmationContext.Provider>
    );
};

// Create a custom hook to use the confirmation context
export const useConfirmation = () => {
    const context = useContext(ConfirmationContext);
    if (!context) {
        throw new Error('useConfirmation must be used within a ConfirmationProvider');
    }
    return context;
};