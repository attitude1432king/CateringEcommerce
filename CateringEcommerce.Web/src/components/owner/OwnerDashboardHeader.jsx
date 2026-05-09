import React, { useState } from 'react';
import { useLocation } from 'react-router-dom';
import { Clock, Menu, X } from 'lucide-react';
import AvailabilityManagement from './dashboard/availability/AvailabilityManagement';
import { useAuth } from '../../contexts/AuthContext';
import OwnerNotifications from './OwnerNotifications';

const PAGE_TITLES = {
    '/owner/dashboard':             { title: 'Dashboard',       sub: 'Welcome back' },
    '/owner/dashboard/bookings':    { title: 'Bookings',        sub: 'Manage booking requests' },
    '/owner/dashboard/events':      { title: 'Event Orders',    sub: 'Confirmed event orders' },
    '/owner/dashboard/menu':        { title: 'Menu & Packages', sub: 'Manage your offerings' },
    '/owner/dashboard/decorations': { title: 'Decorations',     sub: 'Decoration services' },
    '/owner/dashboard/staff':       { title: 'Staff',           sub: 'Manage your team' },
    '/owner/dashboard/discounts':   { title: 'Discounts',       sub: 'Promotions & offers' },
    '/owner/dashboard/banners':     { title: 'Banners',         sub: 'Marketing banners' },
    '/owner/dashboard/earnings':    { title: 'Earnings',        sub: 'Revenue & payouts' },
    '/owner/dashboard/reviews':     { title: 'Reviews',         sub: 'Customer feedback' },
    '/owner/dashboard/support':     { title: 'Support',         sub: 'Help & tickets' },
    '/owner/dashboard/profile':     { title: 'Settings',        sub: 'Account & business settings' },
};

export default function OwnerDashboardHeader({ onToggleSidebar, isSidebarOpen }) {
    const { user } = useAuth();
    const location = useLocation();
    const [isAvailabilityOpen, setIsAvailabilityOpen] = useState(false);

    const page = PAGE_TITLES[location.pathname] || { title: 'Dashboard', sub: '' };

    const initials = user?.name
        ? user.name.split(' ').map(n => n[0]).slice(0, 2).join('').toUpperCase()
        : 'P';

    return (
        <>
            <header className="topbar">
                {/* Left — mobile hamburger + page title */}
                <div className="flex items-center gap-3 min-w-0">
                    {/* Hamburger — mobile only */}
                    <button
                        className="icon-btn md:hidden flex-shrink-0"
                        onClick={onToggleSidebar}
                        aria-label={isSidebarOpen ? 'Close menu' : 'Open menu'}
                    >
                        {isSidebarOpen ? <X size={18} /> : <Menu size={18} />}
                    </button>

                    <div className="min-w-0">
                        <h1 className="truncate">{page.title}</h1>
                        {page.sub && <p className="truncate hidden md:block">{page.sub}</p>}
                    </div>
                </div>

                {/* Right — actions */}
                <div className="topbar__right">
                    {/* Availability toggle */}
                    <button
                        onClick={() => setIsAvailabilityOpen(true)}
                        className="flex items-center gap-2 px-3 py-2 rounded-lg text-sm font-semibold transition-all duration-200 hidden sm:flex"
                        style={{
                            background: 'linear-gradient(135deg, #FF6B35 0%, #FF8C42 50%, #FFB627 100%)',
                            color: '#fff',
                            border: 'none',
                            boxShadow: 'var(--shadow-card)',
                        }}
                        title="Manage Availability"
                    >
                        <Clock size={15} strokeWidth={2} />
                        <span className="hidden md:inline">Availability</span>
                    </button>

                    {/* Notifications */}
                    <OwnerNotifications />

                    {/* User avatar */}
                    <div className="user-card" style={{ background: 'var(--neutral-50)', padding: '6px 10px', borderRadius: '12px' }}>
                        <div className="avatar" style={{ width: 30, height: 30, fontSize: 11 }}>{initials}</div>
                        <span className="user-card__t hidden md:block text-sm">{user?.name || 'Partner'}</span>
                    </div>
                </div>
            </header>

            {/* Availability Modal */}
            {isAvailabilityOpen && (
                <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4">
                    <div className="bg-white rounded-2xl shadow-2xl w-full max-w-4xl max-h-[90vh] overflow-y-auto relative">
                        <button
                            onClick={() => setIsAvailabilityOpen(false)}
                            className="absolute top-4 right-4 z-10 icon-btn"
                            aria-label="Close"
                        >
                            <X size={18} />
                        </button>
                        <AvailabilityManagement />
                    </div>
                </div>
            )}
        </>
    );
}
