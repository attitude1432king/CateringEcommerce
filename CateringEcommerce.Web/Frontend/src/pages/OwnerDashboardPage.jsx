/*
========================================
File: src/pages/owner/OwnerDashboardPage.jsx (NEW FILE)
========================================
This is the main layout for the entire owner dashboard.
*/
import React from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import OwnerDashboardSidebar from '../components/owner/OwnerDashboardSidebar';
import OwnerDashboardHeader from '../components/owner/OwnerDashboardHeader';


// Import the new dashboard components
import DashboardHome from '../components/owner/dashboard/DashboardHome';
import MenuManagement from '../components/owner/dashboard/MenuManagement';
import Decorations from '../components/owner/dashboard/Decorations';
import Discounts from '../components/owner/dashboard/Discounts';
import BookingRequests from '../components/owner/dashboard/BookingRequests';
import EventOrders from '../components/owner/dashboard/EventOrders';
import StaffManagement from '../components/owner/dashboard/StaffManagement';
import Earnings from '../components/owner/dashboard/Earnings';
import Reviews from '../components/owner/dashboard/Reviews';
import Support from '../components/owner/dashboard/Support';
import ProfileSettings from '../components/owner/dashboard/ProfileSettings';




export default function OwnerDashboardPage() {
    return (
        <div className="flex h-screen bg-neutral-100">
            <OwnerDashboardSidebar />
            <div className="flex-1 flex flex-col overflow-hidden">
                <OwnerDashboardHeader /> {/* New Header */}
                <main className="flex-1 overflow-x-hidden overflow-y-auto bg-neutral-100">
                    <Routes>
                        <Route index element={<DashboardHome />} />
                        <Route path="bookings" element={<BookingRequests />} />
                        <Route path="events" element={<EventOrders />} />
                        <Route path="menu" element={<MenuManagement />} />
                        <Route path="decorations" element={<Decorations />} />
                        <Route path="staff" element={<StaffManagement />} />
                        <Route path="discounts" element={<Discounts />} />
                        {/* Removed availability route since it's now a modal */}
                        <Route path="earnings" element={<Earnings />} />
                        <Route path="reviews" element={<Reviews />} />
                        <Route path="support" element={<Support />} />
                        <Route path="profile" element={<ProfileSettings />} />
                        <Route path="*" element={<Navigate to="." replace />} />
                    </Routes>
                </main>
            </div>
        </div>
    );
}