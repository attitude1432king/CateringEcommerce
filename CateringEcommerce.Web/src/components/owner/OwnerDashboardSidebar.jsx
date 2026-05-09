import React from 'react';
import { NavLink, useLocation } from 'react-router-dom';
import {
    LayoutDashboard, ClipboardList, Calendar, BookOpen,
    Sparkles, Users, Tag, Image, DollarSign, Star,
    Headphones, Settings, LogOut,
} from 'lucide-react';
import { useAuth } from '../../contexts/AuthContext';

const NAV_ITEMS = [
    { to: '/owner/dashboard',             icon: LayoutDashboard, label: 'Dashboard',      exact: true },
    { to: '/owner/dashboard/bookings',    icon: ClipboardList,   label: 'Bookings'                   },
    { to: '/owner/dashboard/events',      icon: Calendar,        label: 'Event Orders'               },
    { to: '/owner/dashboard/menu',        icon: BookOpen,        label: 'Menu & Packages'            },
    { to: '/owner/dashboard/decorations', icon: Sparkles,        label: 'Decorations'                },
    { to: '/owner/dashboard/staff',       icon: Users,           label: 'Staff'                      },
    { to: '/owner/dashboard/discounts',   icon: Tag,             label: 'Discounts'                  },
    { to: '/owner/dashboard/banners',     icon: Image,           label: 'Banners'                    },
    { to: '/owner/dashboard/earnings',    icon: DollarSign,      label: 'Earnings'                   },
    { to: '/owner/dashboard/reviews',     icon: Star,            label: 'Reviews'                    },
    { to: '/owner/dashboard/support',     icon: Headphones,      label: 'Support'                    },
];

function SidebarLink({ to, icon: Icon, label, badge, exact }) {
    const location = useLocation();
    const isActive = exact
        ? location.pathname === to
        : location.pathname.startsWith(to);

    return (
        <NavLink
            to={to}
            className={`nav-item${isActive ? ' is-active' : ''}`}
        >
            <span className="ic flex-shrink-0">
                <Icon size={18} strokeWidth={1.75} />
            </span>
            <span className="flex-1">{label}</span>
            {badge != null && (
                <span className="badge">{badge}</span>
            )}
        </NavLink>
    );
}

export default function OwnerDashboardSidebar({ isOpen, onClose }) {
    const { user, logout } = useAuth();

    const initials = user?.name
        ? user.name.split(' ').map(n => n[0]).slice(0, 2).join('').toUpperCase()
        : 'P';

    return (
        <>
            {/* Mobile backdrop */}
            {isOpen && (
                <div
                    className="fixed inset-0 bg-black/40 z-40 md:hidden"
                    onClick={onClose}
                    aria-hidden="true"
                />
            )}

            <aside className={`sidebar sidebar--partner${isOpen ? ' is-open' : ''}`}>
                {/* Brand */}
                <div className="sidebar__brand">
                    <img src="/logo.svg" alt="ENYVORA" />
                    <span className="role">Partner</span>
                </div>

                {/* Navigation */}
                <nav className="flex-1 overflow-y-auto space-y-0.5" style={{ scrollbarWidth: 'none' }}>
                    {NAV_ITEMS.map(item => (
                        <SidebarLink key={item.to} {...item} />
                    ))}
                </nav>

                {/* Bottom */}
                <div className="sidebar__bottom space-y-0.5">
                    <NavLink
                        to="/owner/dashboard/profile"
                        className={({ isActive }) => `nav-item${isActive ? ' is-active' : ''}`}
                    >
                        <span className="ic flex-shrink-0">
                            <Settings size={18} strokeWidth={1.75} />
                        </span>
                        <span className="flex-1">Settings</span>
                    </NavLink>

                    <button
                        onClick={logout}
                        className="nav-item w-full text-left hover:bg-red-50 hover:text-red-600"
                        style={{ border: 'none', background: 'none' }}
                    >
                        <span className="flex-shrink-0 text-neutral-500">
                            <LogOut size={18} strokeWidth={1.75} />
                        </span>
                        <span className="flex-1">Logout</span>
                    </button>

                    {/* User card */}
                    <div className="user-card mt-3">
                        <div className="avatar">{initials}</div>
                        <div className="min-w-0">
                            <div className="user-card__t truncate">{user?.name || 'Partner'}</div>
                            <div className="user-card__d truncate">{user?.email || ''}</div>
                        </div>
                    </div>
                </div>
            </aside>
        </>
    );
}
