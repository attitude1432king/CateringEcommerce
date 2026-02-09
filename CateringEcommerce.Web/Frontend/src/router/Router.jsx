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
import { AdminAuthProvider } from '../contexts/AdminAuthContext';
import { PermissionProvider } from '../contexts/PermissionContext';
import App from '../App'; // The main layout
import HomePage from '../pages/HomePage';
import MyProfilePage from '../pages/MyProfilePage';
import OwnerRegistrationPage from '../pages/OwnerRegistrationPage';
import OwnerDashboardPage from '../pages/OwnerDashboardPage';
import PartnerLoginPage from '../pages/PartnerLoginPage';
import CateringListPage from '../pages/CateringListPage';
import CateringDetailPage from '../pages/CateringDetailPage';
import CartPage from '../pages/CartPage';
import EnhancedCheckoutPage from '../pages/EnhancedCheckoutPage';
import MyOrdersPage from '../pages/MyOrdersPage';
import OrderDetailPage from '../pages/OrderDetailPage';
import FileComplaintPage from '../pages/FileComplaintPage';
import MyComplaintsPage from '../pages/MyComplaintsPage';
import ComplaintDetailPage from '../pages/ComplaintDetailPage';
import OAuthCallbackPage from '../pages/OAuthCallbackPage';
import ConnectedAccountsPage from '../pages/ConnectedAccountsPage';
import TrustedDevicesPage from '../pages/TrustedDevicesPage';
import AdminRoutes from './AdminRoutes';
import SupervisorRoutes from './SupervisorRoutes';
import { SupervisorAuthProvider } from '../contexts/SupervisorAuthContext';

// Wrapper for routes only accessible to logged-in clients
const ClientProtectedRoute = () => {
    const { user } = useAuth();
    return user && user.role === 'User' ? <Outlet /> : <Navigate to="/" replace />;
};

// Wrapper for routes only accessible to logged-in owners
const OwnerProtectedRoute = () => {
    const { user } = useAuth();
    return user && user.role === 'Owner' ? <Outlet /> : <Navigate to="/partner-login" replace />;
};

export default function Router() {
    return (
        <BrowserRouter>
            <Routes>
                {/* Routes with Header/Footer */}
                <Route path="/" element={<App />}>
                    <Route index element={<HomePage />} />
                    <Route path="caterings" element={<CateringListPage />} />
                    <Route path="caterings/:id" element={<CateringDetailPage />} />
                    <Route path="cart" element={<CartPage />} />
                    <Route path="partner-registration" element={<OwnerRegistrationPage />} />

                    <Route element={<ClientProtectedRoute />}>
                        <Route path="profile" element={<MyProfilePage />} />
                        <Route path="checkout" element={<EnhancedCheckoutPage />} />
                        <Route path="my-orders" element={<MyOrdersPage />} />
                        <Route path="orders/:orderId" element={<OrderDetailPage />} />
                        <Route path="complaints" element={<MyComplaintsPage />} />
                        <Route path="complaints/file/:orderId" element={<FileComplaintPage />} />
                        <Route path="complaints/:complaintId" element={<ComplaintDetailPage />} />
                        <Route path="connected-accounts" element={<ConnectedAccountsPage />} />
                        <Route path="trusted-devices" element={<TrustedDevicesPage />} />
                    </Route>
                </Route>

                {/* Full-screen Standalone Routes */}
                <Route path="/partner-registration" element={<OwnerRegistrationPage />} />
                <Route path="/partner-login" element={<PartnerLoginPage />} />
                <Route path="/oauth-callback" element={<OAuthCallbackPage />} />

                {/* Full-screen Owner Dashboard Routes */}
                <Route path="/owner" element={<OwnerProtectedRoute />}>
                    <Route path="dashboard/*" element={<OwnerDashboardPage />} />
                </Route>

                {/* Admin Routes - Wrapped with Auth & Permission Providers */}
                <Route path="/admin/*" element={
                    <AdminAuthProvider>
                        <PermissionProvider>
                            <AdminRoutes />
                        </PermissionProvider>
                    </AdminAuthProvider>
                } />

                {/* Supervisor Routes - Wrapped with Supervisor Auth Provider */}
                <Route path="/supervisor/*" element={
                    <SupervisorAuthProvider>
                        <SupervisorRoutes />
                    </SupervisorAuthProvider>
                } />

                <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
        </BrowserRouter>
    );
}
