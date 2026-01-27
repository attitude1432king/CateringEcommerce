/*
========================================
File: src/main.jsx
========================================
This is the entry point of your application.
*/
import React from 'react';
import { createRoot } from 'react-dom/client';
import './index.css';
import { AuthProvider } from './contexts/AuthContext.jsx';
import { CartProvider } from './contexts/CartContext.jsx';
import { PaymentProvider } from './contexts/PaymentContext.jsx';
import { EventProvider } from './contexts/EventContext.jsx';
import { ToastProvider } from './contexts/ToastContext';
import { ConfirmationProvider } from './contexts/ConfirmationContext';
import Router from './router/Router.jsx';


createRoot(document.getElementById('root')).render(
    <React.StrictMode>
        <AuthProvider>
            <EventProvider>
                <CartProvider>
                    <PaymentProvider>
                        <ToastProvider>
                            <ConfirmationProvider>
                                <Router />
                            </ConfirmationProvider>
                        </ToastProvider>
                    </PaymentProvider>
                </CartProvider>
            </EventProvider>
        </AuthProvider>
    </React.StrictMode>
);
