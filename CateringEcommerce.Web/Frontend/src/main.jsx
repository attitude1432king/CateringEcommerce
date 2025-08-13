/*
========================================
File: src/main.jsx
========================================
This is the entry point of your application.
*/

import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';
import { AuthProvider } from './contexts/AuthContext.jsx';
import Router from './router/Router.jsx'; // Import the new Router

ReactDOM.createRoot(document.getElementById('root')).render(
    <React.StrictMode>
        <AuthProvider>
            <Router />
        </AuthProvider>
    </React.StrictMode>,
);
