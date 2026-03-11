/*
========================================
File: src/main.jsx
========================================
This is the entry point of your application.
*/
import React from 'react';
import { createRoot } from 'react-dom/client';
import './index.css';
import { AppSettingsProvider } from './contexts/AppSettingsContext.jsx';
import { AuthProvider } from './contexts/AuthContext.jsx';
import { CartProvider } from './contexts/CartContext.jsx';
import { PaymentProvider } from './contexts/PaymentContext.jsx';
import { EventProvider } from './contexts/EventContext.jsx';
import { ToastProvider } from './contexts/ToastContext';
import { ConfirmationProvider } from './contexts/ConfirmationContext';
import Router from './router/Router.jsx';
import { Toaster } from 'react-hot-toast'; // P1 FIX: Enable toast notifications for Admin/Supervisor


createRoot(document.getElementById('root')).render(
    <React.StrictMode>
        <AppSettingsProvider>
            <AuthProvider>
                <EventProvider>
                    <CartProvider>
                        <PaymentProvider>
                            <ToastProvider>
                                <ConfirmationProvider>
                                    <Router />
                                    {/* P1 FIX: react-hot-toast renderer for Admin/Supervisor notifications */}
                                    <Toaster
                                        position="top-right"
                                        reverseOrder={false}
                                        toastOptions={{
                                            duration: 4000,
                                            style: {
                                                background: '#363636',
                                                color: '#fff',
                                            },
                                            success: {
                                                duration: 3000,
                                                iconTheme: {
                                                    primary: '#10b981',
                                                    secondary: '#fff',
                                                },
                                            },
                                            error: {
                                                duration: 5000,
                                                iconTheme: {
                                                    primary: '#ef4444',
                                                    secondary: '#fff',
                                                },
                                            },
                                        }}
                                    />
                                </ConfirmationProvider>
                            </ToastProvider>
                        </PaymentProvider>
                    </CartProvider>
                </EventProvider>
            </AuthProvider>
        </AppSettingsProvider>
    </React.StrictMode>
);
