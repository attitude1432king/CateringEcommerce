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
import App from '../App'; // The main layout
import HomePage from '../pages/HomePage';
import MyProfilePage from '../pages/MyProfilePage';
import OwnerRegistrationPage from '../pages/OwnerRegistrationPage';
import OwnerDashboardPage from '../pages/OwnerDashboardPage';
import PartnerLoginPage from '../pages/PartnerLoginPage';
import CateringListPage from '../pages/CateringListPage';
import CateringDetailPage from '../pages/CateringDetailPage';
import CartPage from '../pages/CartPage';
import CheckoutPage from '../pages/CheckoutPage';
import MyOrdersPage from '../pages/MyOrdersPage';
import OrderDetailPage from '../pages/OrderDetailPage';
import AdminRoutes from './AdminRoutes';

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
                        <Route path="checkout" element={<CheckoutPage />} />
                        <Route path="my-orders" element={<MyOrdersPage />} />
                        <Route path="orders/:orderId" element={<OrderDetailPage />} />
                    </Route>
                </Route>

                {/* Full-screen Standalone Routes */}
                <Route path="/partner-registration" element={<OwnerRegistrationPage />} />
                <Route path="/partner-login" element={<PartnerLoginPage />} />

                {/* Full-screen Owner Dashboard Routes */}
                <Route path="/owner" element={<OwnerProtectedRoute />}>
                    <Route path="dashboard/*" element={<OwnerDashboardPage />} />
                </Route>

                {/* Admin Routes */}
                <Route path="/admin/*" element={
                    <AdminAuthProvider>
                        <AdminRoutes />
                    </AdminAuthProvider>
                } />

                <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
        </BrowserRouter>
    );
}
