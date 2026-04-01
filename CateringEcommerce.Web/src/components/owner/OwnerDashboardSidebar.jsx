/*
========================================
File: src/components/owner/OwnerDashboardSidebar.jsx
========================================
MODERN PROFESSIONAL REDESIGN - Inspired by Shopify/Stripe
✨ Features:
- Gradient accents
- Professional indigo/purple color scheme
- Smooth animations
- Icon badges
- Modern hover effects
*/
import React from 'react';
import { NavLink, useLocation } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';

// Modern Icon Components
const HomeIcon = () => (
    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
    </svg>
);

const BookingsIcon = () => (
    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
    </svg>
);

const EventsIcon = () => (
    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
    </svg>
);

const MenuIcon = () => (
    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" />
    </svg>
);

const SparklesIcon = () => (
    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 3v4M3 5h4M6 17v4m-2-2h4m5-16l2.286 6.857L21 12l-5.714 2.143L13 21l-2.286-6.857L5 12l5.714-2.143L13 3z" />
    </svg>
);

const StaffIcon = () => (
    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
    </svg>
);

const DiscountIcon = () => (
    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z" />
    </svg>
);

const EarningsIcon = () => (
    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
    </svg>
);

const ReviewsIcon = () => (
    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11.049 2.927c.3-.921 1.603-.921 1.902 0l1.519 4.674a1 1 0 00.95.69h4.915c.969 0 1.371 1.24.588 1.81l-3.976 2.888a1 1 0 00-.363 1.118l1.518 4.674c.3.922-.755 1.688-1.538 1.118l-3.976-2.888a1 1 0 00-1.176 0l-3.976 2.888c-.783.57-1.838-.197-1.538-1.118l1.518-4.674a1 1 0 00-.363-1.118l-3.976-2.888c-.784-.57-.38-1.81.588-1.81h4.914a1 1 0 00.951-.69l1.519-4.674z" />
    </svg>
);

const SupportIcon = () => (
    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M18.364 5.636l-3.536 3.536m0 5.656l3.536 3.536M9.172 9.172L5.636 5.636m3.536 9.192l-3.536 3.536M21 12a9 9 0 11-18 0 9 9 0 0118 0zm-5 0a4 4 0 11-8 0 4 4 0 018 0z" />
    </svg>
);

const BannerIcon = () => (
    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
    </svg>
);

const ProfileIcon = () => (
    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
    </svg>
);

const LogoutIcon = () => (
    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
    </svg>
);

/**
 * Modern Sidebar Link Component
 */
const SidebarLink = ({ to, icon, label, badge }) => {
    const location = useLocation();
    const isActive = to === '/owner/dashboard'
        ? location.pathname === to
        : location.pathname.startsWith(to);

    return (
        <NavLink
            to={to}
            className={`group relative flex items-center gap-3 px-4 py-3 rounded-xl mx-2 font-medium transition-all duration-200 ${
                isActive
                    ? 'bg-gradient-to-r from-indigo-50 to-purple-50 text-indigo-700 shadow-sm'
                    : 'text-neutral-600 hover:bg-neutral-50 hover:text-neutral-900'
            }`}
        >
            {/* Active Indicator */}
            {isActive && (
                <div className="absolute left-0 top-1/2 -translate-y-1/2 w-1 h-8 bg-gradient-to-b from-indigo-600 to-purple-600 rounded-r-full"></div>
            )}

            {/* Icon */}
            <div className={`flex-shrink-0 ${isActive ? 'text-indigo-600' : 'text-neutral-500 group-hover:text-neutral-700'}`}>
                {icon}
            </div>

            {/* Label */}
            <span className="flex-1">{label}</span>

            {/* Badge */}
            {badge && (
                <span className="flex-shrink-0 px-2 py-0.5 text-xs font-bold text-white bg-gradient-to-r from-indigo-600 to-purple-600 rounded-full">
                    {badge}
                </span>
            )}
        </NavLink>
    );
};

export default function OwnerDashboardSidebar() {
    const { logout } = useAuth();

    return (
        <aside className="w-72 bg-white shadow-lg hidden md:flex flex-col border-r border-neutral-100">
            {/* Logo Section */}
            <div className="p-6 border-b border-neutral-100">
                <div className="flex items-center gap-3">
                    <img src="/logo.svg" alt="ENYVORA" className="h-12 w-auto" />
                </div>
                <p className="text-xs text-neutral-500 font-medium mt-2 ml-1">Partner Dashboard</p>
            </div>

            {/* Navigation */}
            <nav className="flex-1 overflow-y-auto py-4 px-2 space-y-1">
                <SidebarLink to="/owner/dashboard" icon={<HomeIcon />} label="Dashboard" />
                <SidebarLink to="/owner/dashboard/bookings" icon={<BookingsIcon />} label="Bookings" />
                <SidebarLink to="/owner/dashboard/events" icon={<EventsIcon />} label="Event Orders" />
                <SidebarLink to="/owner/dashboard/menu" icon={<MenuIcon />} label="Menu & Packages" />
                <SidebarLink to="/owner/dashboard/decorations" icon={<SparklesIcon />} label="Decorations" />
                <SidebarLink to="/owner/dashboard/staff" icon={<StaffIcon />} label="Staff" />
                <SidebarLink to="/owner/dashboard/discounts" icon={<DiscountIcon />} label="Discounts" />
                <SidebarLink to="/owner/dashboard/banners" icon={<BannerIcon />} label="Banners" />
                <SidebarLink to="/owner/dashboard/earnings" icon={<EarningsIcon />} label="Earnings" />
                <SidebarLink to="/owner/dashboard/reviews" icon={<ReviewsIcon />} label="Reviews" />
                <SidebarLink to="/owner/dashboard/support" icon={<SupportIcon />} label="Support" />
            </nav>

            {/* Bottom Section */}
            <div className="border-t border-neutral-100 p-4 space-y-1">
                <SidebarLink to="/owner/dashboard/profile" icon={<ProfileIcon />} label="Settings" />
                <button
                    onClick={logout}
                    className="w-full flex items-center gap-3 px-4 py-3 text-neutral-600 hover:bg-red-50 hover:text-red-600 rounded-xl font-medium transition-all duration-200"
                >
                    <LogoutIcon />
                    <span>Logout</span>
                </button>
            </div>
        </aside>
    );
}
