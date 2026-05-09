import React, { useState } from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import OwnerDashboardSidebar from '../components/owner/OwnerDashboardSidebar';
import OwnerDashboardHeader from '../components/owner/OwnerDashboardHeader';

import DashboardHome      from '../components/owner/dashboard/DashboardHome';
import MenuManagement     from '../components/owner/dashboard/MenuManagement';
import Decorations        from '../components/owner/dashboard/Decorations';
import Discounts          from '../components/owner/dashboard/Discounts';
import Banners            from '../components/owner/dashboard/Banners';
import BookingRequests    from '../components/owner/dashboard/BookingRequests';
import EventOrders        from '../components/owner/dashboard/EventOrders';
import StaffManagement    from '../components/owner/dashboard/StaffManagement';
import Earnings           from '../components/owner/dashboard/Earnings';
import Reviews            from '../components/owner/dashboard/Reviews';
import Support            from '../components/owner/dashboard/Support';
import ProfileSettings    from '../components/owner/dashboard/ProfileSettings';

export default function OwnerDashboardPage() {
    const [sidebarOpen, setSidebarOpen] = useState(false);

    return (
        <div className="portal-layout">
            <OwnerDashboardSidebar
                isOpen={sidebarOpen}
                onClose={() => setSidebarOpen(false)}
            />

            <div className="portal-main">
                <OwnerDashboardHeader
                    onToggleSidebar={() => setSidebarOpen(v => !v)}
                    isSidebarOpen={sidebarOpen}
                />
                <div className="portal-body">
                    <Routes>
                        <Route index                  element={<DashboardHome />} />
                        <Route path="bookings"        element={<BookingRequests />} />
                        <Route path="events"          element={<EventOrders />} />
                        <Route path="menu"            element={<MenuManagement />} />
                        <Route path="decorations"     element={<Decorations />} />
                        <Route path="staff"           element={<StaffManagement />} />
                        <Route path="discounts"       element={<Discounts />} />
                        <Route path="banners"         element={<Banners />} />
                        <Route path="earnings"        element={<Earnings />} />
                        <Route path="reviews"         element={<Reviews />} />
                        <Route path="support"         element={<Support />} />
                        <Route path="profile"         element={<ProfileSettings />} />
                        <Route path="*"               element={<Navigate to="." replace />} />
                    </Routes>
                </div>
            </div>
        </div>
    );
}
