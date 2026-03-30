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
import ModernCheckoutPage from '../pages/ModernCheckoutPage'; // P1 FIX: Use modern 4-step checkout
import MyOrdersPage from '../pages/MyOrdersPage';
import OrderDetailPage from '../pages/OrderDetailPage';
import OrderModificationPage from '../pages/OrderModificationPage'; // P1 FIX: Order modification page
import InvoicesPage from '../pages/InvoicesPage'; // P1 FIX: Invoices page
import SampleDeliveryTrackingPage from '../pages/SampleDeliveryTrackingPage'; // P1 FIX: Sample delivery tracking
import WishlistPage from '../pages/WishlistPage'; // P0 FIX: Add wishlist page import
import FileComplaintPage from '../pages/FileComplaintPage';
import MyComplaintsPage from '../pages/MyComplaintsPage';
import ComplaintDetailPage from '../pages/ComplaintDetailPage';
import OAuthCallbackPage from '../pages/OAuthCallbackPage';
import ConnectedAccountsPage from '../pages/ConnectedAccountsPage';
import TrustedDevicesPage from '../pages/TrustedDevicesPage';
import AdminRoutes from './AdminRoutes';
import SupervisorRoutes from './SupervisorRoutes';
import { SupervisorAuthProvider } from '../contexts/SupervisorAuthContext';
import { lazy, Suspense } from 'react';

// Static header nav pages (lazy loaded)
const EventsPage = lazy(() => import('../pages/static/EventsPage'));
const CorporatePage = lazy(() => import('../pages/static/CorporatePage'));

// Static footer pages (lazy loaded)
const AboutUs = lazy(() => import('../pages/static/AboutUs'));
const Blog = lazy(() => import('../pages/static/Blog'));
const Careers = lazy(() => import('../pages/static/Careers'));
const PressKit = lazy(() => import('../pages/static/PressKit'));
const BecomePartner = lazy(() => import('../pages/static/BecomePartner'));
const PartnerDashboardInfo = lazy(() => import('../pages/static/PartnerDashboardInfo'));
const PartnerSupport = lazy(() => import('../pages/static/PartnerSupport'));
const GrowthResources = lazy(() => import('../pages/static/GrowthResources'));
const HelpCenter = lazy(() => import('../pages/static/HelpCenter'));
const ContactUs = lazy(() => import('../pages/static/ContactUs'));
const TermsConditions = lazy(() => import('../pages/static/TermsConditions'));
const PrivacyPolicy = lazy(() => import('../pages/static/PrivacyPolicy'));

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

                    {/* Static header nav pages */}
                    <Route path="events" element={<Suspense fallback={null}><EventsPage /></Suspense>} />
                    <Route path="corporate" element={<Suspense fallback={null}><CorporatePage /></Suspense>} />

                    {/* Static footer pages */}
                    <Route path="about-us" element={<Suspense fallback={null}><AboutUs /></Suspense>} />
                    <Route path="blog" element={<Suspense fallback={null}><Blog /></Suspense>} />
                    <Route path="careers" element={<Suspense fallback={null}><Careers /></Suspense>} />
                    <Route path="press-kit" element={<Suspense fallback={null}><PressKit /></Suspense>} />
                    <Route path="become-partner" element={<Suspense fallback={null}><BecomePartner /></Suspense>} />
                    <Route path="partner-dashboard" element={<Suspense fallback={null}><PartnerDashboardInfo /></Suspense>} />
                    <Route path="partner-support" element={<Suspense fallback={null}><PartnerSupport /></Suspense>} />
                    <Route path="growth-resources" element={<Suspense fallback={null}><GrowthResources /></Suspense>} />
                    <Route path="help-center" element={<Suspense fallback={null}><HelpCenter /></Suspense>} />
                    <Route path="contact-us" element={<Suspense fallback={null}><ContactUs /></Suspense>} />
                    <Route path="terms-and-conditions" element={<Suspense fallback={null}><TermsConditions /></Suspense>} />
                    <Route path="privacy-policy" element={<Suspense fallback={null}><PrivacyPolicy /></Suspense>} />

                    <Route element={<ClientProtectedRoute />}>
                        <Route path="profile" element={<MyProfilePage />} />
                        <Route path="checkout" element={<ModernCheckoutPage />} /> {/* P1 FIX: Use modern 4-step checkout */}
                        <Route path="my-orders" element={<MyOrdersPage />} />
                        <Route path="orders/:orderId" element={<OrderDetailPage />} />
                        <Route path="orders/:orderId/modifications" element={<OrderModificationPage />} /> {/* P1 FIX: Order modifications */}
                        <Route path="orders/:orderId/sample-delivery" element={<SampleDeliveryTrackingPage />} /> {/* P1 FIX: Sample delivery */}
                        <Route path="invoices" element={<InvoicesPage />} /> {/* P1 FIX: Invoices page */}
                        <Route path="wishlist" element={<WishlistPage />} /> {/* P0 FIX: Add wishlist route */}
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
