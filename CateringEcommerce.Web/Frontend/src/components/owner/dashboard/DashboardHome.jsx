/*
========================================
File: src/components/owner/dashboard/DashboardHome.jsx (NEW FILE)
========================================
*/
import React from 'react';

const StatCard = ({ title, value, change, icon }) => (
    <div className="bg-white p-6 rounded-lg shadow-md">
        <div className="flex items-center justify-between">
            <div>
                <p className="text-sm font-medium text-neutral-500">{title}</p>
                <p className="text-2xl font-bold text-neutral-800">{value}</p>
                <p className={`text-xs ${change.startsWith('+') ? 'text-green-600' : 'text-red-600'}`}>{change}</p>
            </div>
            <div className="text-4xl">{icon}</div>
        </div>
    </div>
);

export default function DashboardHome() {
    return (
        <div className="animate-fade-in space-y-6">
            <h1 className="text-3xl font-bold text-neutral-800">Dashboard</h1>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
                <StatCard title="Today's Earnings" value="₹25,500" change="+5.2% vs yesterday" icon="💰" />
                <StatCard title="New Orders" value="5" change="+2 vs yesterday" icon="📥" />
                <StatCard title="Active Orders" value="12" change="In Progress" icon="🔄" />
                <StatCard title="Overall Rating" value="4.8 / 5" change="from 120 reviews" icon="⭐" />
            </div>
            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                <div className="lg:col-span-2 bg-white p-6 rounded-lg shadow-md">
                    <h3 className="font-semibold mb-4">Recent Orders</h3>
                    {/* Placeholder for recent orders list */}
                    <p className="text-neutral-500">Recent orders will be displayed here.</p>
                </div>
                <div className="bg-white p-6 rounded-lg shadow-md">
                    <h3 className="font-semibold mb-4">Notifications</h3>
                    {/* Placeholder for notifications */}
                    <p className="text-neutral-500">Notifications will be displayed here.</p>
                </div>
            </div>
        </div>
    );
}