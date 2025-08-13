/*
========================================
File: src/pages/owner/OwnerDashboardPage.jsx (NEW FILE)
========================================
This is the main layout for the entire owner dashboard.
*/
import React from 'react';
import { Routes, Route, Link, useLocation } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

// Import the new dashboard components
import DashboardHome from '../components/owner/dashboard/DashboardHome';
import OrdersManagement from '../components/owner/dashboard/OrdersManagement';
import MenuManagement from '../components/owner/dashboard/MenuManagement';
import HistoryPage from '../components/owner/dashboard/HistoryPage';

const SidebarLink = ({ to, icon, children }) => {
    const location = useLocation();
    const isActive = location.pathname === to;
    return (
        <Link to={to} className={`flex items-center gap-3 px-3 py-2 rounded-md text-sm font-medium transition-colors ${isActive ? 'bg-rose-100 text-rose-700' : 'text-neutral-600 hover:bg-amber-100'
            }`}>
            {icon}
            {children}
        </Link>
    );
};

export default function OwnerDashboardPage() {
    const { logout } = useAuth();

    return (
        <div className="min-h-screen bg-amber-50 flex">
            {/* Persistent Sidebar */}
            <aside className="w-64 bg-white shadow-md flex-shrink-0 flex flex-col">
                <div className="h-16 flex items-center justify-center border-b">
                    <h1 className="text-2xl font-bold text-rose-600">Feasto Partner</h1>
                </div>
                <nav className="flex-1 p-4 space-y-2">
                    <SidebarLink to="/owner/dashboard" icon={'??'}>Dashboard</SidebarLink>
                    <SidebarLink to="/owner/orders" icon={'??'}>Orders</SidebarLink>
                    <SidebarLink to="/owner/menu" icon={'??'}>Menu Management</SidebarLink>
                    <SidebarLink to="/owner/history" icon={'??'}>History</SidebarLink>
                    {/* Other links can be added here */}
                </nav>
                <div className="p-4 border-t">
                    <button onClick={logout} className="w-full flex items-center gap-3 px-3 py-2 rounded-md text-sm font-medium text-neutral-600 hover:bg-amber-100">
                        {'??'} Logout
                    </button>
                </div>
            </aside>

            {/* Main Content Area */}
            <main className="flex-1 p-6 overflow-y-auto">
                <Routes>
                    <Route index element={<DashboardHome />} />
                    <Route path="orders" element={<OrdersManagement />} />
                    <Route path="menu" element={<MenuManagement />} />
                    <Route path="history" element={<HistoryPage />} />
                </Routes>
            </main>
        </div>
    );
}