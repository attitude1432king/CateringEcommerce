/// <reference path="../pages/OwnerRegistrationPage.jsx" />
/*
========================================
File: src/router/Router.jsx (NEW FILE - Place in src/router/)
========================================
This file defines all the application routes.
*/
import React from 'react';
import { BrowserRouter, Routes, Route, Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import App from '../App'; // The main layout
import HomePage from '../pages/HomePage';
import MyProfilePage from '../pages/MyProfilePage';
import OwnerRegistrationPage from '../pages/OwnerRegistrationPage';
import OwnerDashboardPage from '../pages/OwnerDashboardPage';
import PartnerLoginPage from '../pages/PartnerLoginPage';

// Wrapper for routes only accessible to logged-in clients
const ClientProtectedRoute = () => {
    const { user } = useAuth();
    return user && user.role === 'client' ? <Outlet /> : <Navigate to="/" replace />;
};

// Wrapper for routes only accessible to logged-in owners
const OwnerProtectedRoute = () => {
    debugger; // Debugging line to check if this route is hit
    const { user } = useAuth();
    return user && user.role === 'owner' ? <Outlet /> : <Navigate to="/partner-login" replace />;
};

export default function Router() {
    return (
        <BrowserRouter>
            <Routes>
                {/* Routes with Header/Footer */}
                <Route path="/" element={<App />}>
                    <Route index element={<HomePage />} />
                    <Route path="partner-registration" element={<OwnerRegistrationPage />} />

                    <Route element={<ClientProtectedRoute />}>
                        <Route path="profile" element={<MyProfilePage />} />
                    </Route>
                </Route>

                {/* Full-screen Standalone Routes */}
                <Route path="/partner-registration" element={<OwnerRegistrationPage />} />
                <Route path="/partner-login" element={<PartnerLoginPage />} />

                {/* Full-screen Owner Dashboard Routes */}
                <Route path="/owner" element={<OwnerProtectedRoute />}>
                    <Route path="dashboard/*" element={<OwnerDashboardPage />} />
                </Route>

                <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
        </BrowserRouter>
    );
}
