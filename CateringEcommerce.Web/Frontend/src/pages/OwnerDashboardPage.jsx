/*
========================================
File: src/pages/owner/OwnerDashboardPage.jsx (NEW FILE)
========================================
This is the main layout for the entire owner dashboard.
*/
import React from 'react';
import { Routes, Route, NavLink, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext'; 

// Import the new dashboard components
import DashboardHome from '../components/owner/dashboard/DashboardHome';
import MenuManagement from '../components/owner/dashboard/MenuManagement';
import Decorations from '../components/owner/dashboard/Decorations';
import BookingRequests from '../components/owner/dashboard/BookingRequests';
import EventOrders from '../components/owner/dashboard/EventOrders';
import Earnings from '../components/owner/dashboard/Earnings';
import Reviews from '../components/owner/dashboard/Reviews';
import Support from '../components/owner/dashboard/Support';
import ProfileSettings from '../components/owner/dashboard/ProfileSettings';




// SVG Icons for the sidebar
const DashboardIcon = () => <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2V6zM14 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2V6zM4 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2v-2zM14 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2v-2z" /></svg>;
const BookingsIcon = () => <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 8h.01M12 12h.01M15 12h.01M9 12h.01" /></svg>;
const EventsIcon = () => <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" /></svg>;
const MenuIcon = () => <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6.253v11.494m-9-5.747h18" /></svg>;
const EarningsIcon = () => <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 7h8m0 0v8m0-8l-8 8-4-4-6 6" /></svg>;
const ReviewsIcon = () => <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 3v4M3 5h4M6 17v4m-2-2h4m5-14v4m-2-2h4m5 10v4m-2-2h4" /></svg>;
const SupportIcon = () => <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8.228 9c.549-1.165 2.03-2 3.772-2 2.21 0 4 1.343 4 3 0 1.4-1.278 2.575-3.006 2.907-.542.104-.994.54-.994 1.093m0 3h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>;
const SettingsIcon = () => <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.096 2.572-1.065z" /><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" /></svg>;
const LogoutIcon = () => <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" /></svg>;
const DecorationIcon = () => <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 3l1.76 3.56L18 7.24l-2.64 2.57.62 3.62L12 11.77l-3.98 2.09.62-3.62L6 7.24l4.24-.68L12 3z" /> <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 21h16M8 17h8" /></svg>;

const SidebarLink = ({ to, icon, children }) => {
    return (
        <NavLink
            to={to}
            end={to === "/owner/dashboard"}
            className={({ isActive }) => `flex items-center gap-3 px-3 py-2 rounded-md text-sm font-medium transition-colors ${isActive ? 'bg-rose-600 text-white shadow-sm' : 'text-neutral-600 hover:bg-rose-50 hover:text-rose-700'
                }`}
        >
            {icon}
            {children}
        </NavLink>
    );
};

export default function OwnerDashboardPage() {
    const { logout } = useAuth();
    const navigate = useNavigate();

    const handleLogout = () => {
        logout();
        navigate('/partner-login');
    };

    return (
        <div className="min-h-screen bg-neutral-100 flex">
            <aside className="w-64 bg-white border-r border-neutral-200 flex-shrink-0 flex flex-col">
                <div className="h-16 flex items-center px-4 border-b border-neutral-200">
                    <h1 className="text-2xl font-bold text-rose-600">QuickFeast</h1>
                </div>
                <nav className="flex-1 p-4 space-y-1">
                    <SidebarLink to="/owner/dashboard" icon={<DashboardIcon />}>Dashboard</SidebarLink>
                    <SidebarLink to="/owner/dashboard/bookings" icon={<BookingsIcon />}>Booking Requests</SidebarLink>
                    <SidebarLink to="/owner/dashboard/events" icon={<EventsIcon />}>Event Orders</SidebarLink>
                    <SidebarLink to="/owner/dashboard/menu" icon={<MenuIcon />}>Menu & Packages</SidebarLink>
                    <SidebarLink to="/owner/dashboard/decorations" icon={<DecorationIcon />}>Decorations</SidebarLink>
                    <SidebarLink to="/owner/dashboard/payments" icon={<EarningsIcon />}>Earnings</SidebarLink>
                    <SidebarLink to="/owner/dashboard/reviews" icon={<ReviewsIcon />}>Reviews</SidebarLink>
                    <SidebarLink to="/owner/dashboard/support" icon={<SupportIcon />}>Support</SidebarLink>
                    <SidebarLink to="/owner/dashboard/settings" icon={<SettingsIcon />}>Profile & Settings</SidebarLink>
                </nav>
                <div className="p-4 border-t border-neutral-200">
                    <button onClick={handleLogout} className="w-full flex items-center gap-3 px-3 py-2 rounded-md text-sm font-medium text-neutral-600 hover:bg-rose-50 hover:text-rose-700">
                        <LogoutIcon /> Logout
                    </button>
                </div>
            </aside>

            <main className="flex-1 p-6 lg:p-8 overflow-y-auto">
                <Routes>
                    <Route index element={<DashboardHome />} />
                    <Route path="bookings" element={<BookingRequests />} />
                    <Route path="events" element={<EventOrders />} />
                    <Route path="menu" element={<MenuManagement />} />
                    <Route path="decorations" element={<Decorations />} />
                    <Route path="payments" element={<Earnings />} />
                    <Route path="reviews" element={<Reviews />} />
                    <Route path="support" element={<Support />} />
                    <Route path="settings" element={<ProfileSettings />} />
                </Routes>
            </main>
        </div>
    );
}