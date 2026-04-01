import { useState, useRef, useEffect } from 'react';
import { Link, useLocation } from 'react-router-dom';
import {
    LayoutDashboard,
    Store,
    Users,
    ShoppingCart,
    DollarSign,
    Star,
    Settings,
    LogOut,
    ChevronLeft,
    ChevronRight,
    FileCheck,
    ShieldCheck,
    Database,
    BarChart3,
    ClipboardCheck,
    MessageSquare,
    Clock,
    UserCheck,
    MapPin,
    Utensils,
    ChefHat,
    CalendarDays,
    Briefcase,
    UserSquare2,
    Palette,
    Leaf,
} from 'lucide-react';
import { useAdminAuth } from '../../../contexts/AdminAuthContext';
import { usePermissions } from '../../../contexts/PermissionContext';

const AdminSidebar = () => {
    const [collapsed, setCollapsed] = useState(false);
    const [supervisorMenuOpen, setSupervisorMenuOpen] = useState(false);
    const [masterDataMenuOpen, setMasterDataMenuOpen] = useState(false);
    const supervisorMenuRef = useRef(null);
    const supervisorTimeoutRef = useRef(null);
    const masterDataMenuRef = useRef(null);
    const masterDataTimeoutRef = useRef(null);
    const location = useLocation();
    const { logout } = useAdminAuth();
    const { hasPermission, hasRole, isSuperAdmin } = usePermissions();

    const canViewSupervisorManagement = isSuperAdmin || hasRole('CAREER_SUPERVISOR');

    const navigation = [
        { name: 'Dashboard', href: '/admin/dashboard', icon: LayoutDashboard },
        { name: 'Analytics', href: '/admin/analytics', icon: BarChart3 },
        { name: 'Caterings', href: '/admin/caterings', icon: Store, permission: 'CATERING_VIEW' },
        { name: 'Partner Requests', href: '/admin/partner-requests', icon: FileCheck, permission: 'PARTNER_REQUEST_VIEW' },
        { name: 'Complaints', href: '/admin/complaints', icon: MessageSquare },
        { name: 'Users', href: '/admin/users', icon: Users, requireSuperAdmin: true },
        { name: 'Admin Users', href: '/admin/users/admins', icon: ShieldCheck, requireSuperAdmin: true },
        { name: 'Orders', href: '/admin/orders', icon: ShoppingCart, permission: 'ORDER_VIEW' },
        { name: 'Earnings', href: '/admin/earnings', icon: DollarSign, permission: 'EARNINGS_VIEW' },
        { name: 'Reviews', href: '/admin/reviews', icon: Star, permission: 'REVIEW_VIEW' },
        { name: 'Settings', href: '/admin/settings', icon: Settings, requireSuperAdmin: true },
    ];

    const supervisorSubItems = [
        { name: 'Pending Requests', href: '/admin/supervisor-management/pending', icon: Clock },
        { name: 'Approved Supervisors', href: '/admin/supervisor-management/approved', icon: UserCheck },
    ];

    const masterDataSubItems = [
        { name: 'Cities', href: '/admin/master-data/cities', icon: MapPin },
        { name: 'Food Categories', href: '/admin/master-data/food-categories', icon: Utensils },
        { name: 'Cuisine Types', href: '/admin/master-data/cuisine-types', icon: ChefHat },
        { name: 'Food Types', href: '/admin/master-data/food-types', icon: Leaf },
        { name: 'Event Types', href: '/admin/master-data/event-types', icon: CalendarDays },
        { name: 'Service Types', href: '/admin/master-data/service-types', icon: Briefcase },
        { name: 'Guest Categories', href: '/admin/master-data/guest-categories', icon: UserSquare2 },
        { name: 'Themes', href: '/admin/master-data/themes', icon: Palette },
    ];

    const visibleNavigation = navigation.filter(item => {
        if (!item.permission && !item.requireSuperAdmin) return true;
        if (item.requireSuperAdmin && !isSuperAdmin) return false;
        if (item.permission && !hasPermission(item.permission)) return false;
        return true;
    });

    const isActive = (path) => {
        if (path.includes('/master-data')) {
            return location.pathname.startsWith('/admin/master-data');
        }
        return location.pathname === path;
    };

    const isSupervisorActive = location.pathname.startsWith('/admin/supervisor-management');
    const isMasterDataActive = location.pathname.startsWith('/admin/master-data');

    const handleSupervisorMouseEnter = () => {
        if (supervisorTimeoutRef.current) {
            clearTimeout(supervisorTimeoutRef.current);
            supervisorTimeoutRef.current = null;
        }
        setSupervisorMenuOpen(true);
    };

    const handleSupervisorMouseLeave = () => {
        supervisorTimeoutRef.current = setTimeout(() => {
            setSupervisorMenuOpen(false);
        }, 200);
    };

    const handleMasterDataMouseEnter = () => {
        if (masterDataTimeoutRef.current) {
            clearTimeout(masterDataTimeoutRef.current);
            masterDataTimeoutRef.current = null;
        }
        setMasterDataMenuOpen(true);
    };

    const handleMasterDataMouseLeave = () => {
        masterDataTimeoutRef.current = setTimeout(() => {
            setMasterDataMenuOpen(false);
        }, 200);
    };

    useEffect(() => {
        return () => {
            if (supervisorTimeoutRef.current) clearTimeout(supervisorTimeoutRef.current);
            if (masterDataTimeoutRef.current) clearTimeout(masterDataTimeoutRef.current);
        };
    }, []);

    return (
        <div
            className={`
        ${collapsed ? 'w-20' : 'w-64'}
        bg-gray-900 text-white h-screen fixed left-0 top-0
        transition-all duration-300 ease-in-out
        flex flex-col z-50
      `}
        >
            {/* Logo & Brand */}
            <div className="h-16 flex items-center justify-between px-4 border-b border-gray-800">
                {!collapsed && (
                    <div className="flex items-center space-x-3">
                        <img src="/logo-white.svg" alt="ENYVORA Admin" className="h-10 w-auto" />
                    </div>
                )}
                {collapsed && (
                    <div className="flex items-center justify-center w-full">
                        <img src="/logo-icon.svg" alt="ENYVORA" className="h-8 w-8" />
                    </div>
                )}
                {!collapsed && (
                    <button
                        onClick={() => setCollapsed(!collapsed)}
                        className="p-1.5 rounded-lg hover:bg-gray-800 transition-colors"
                    >
                        <ChevronLeft className="w-5 h-5" />
                    </button>
                )}
                {collapsed && (
                    <button
                        onClick={() => setCollapsed(!collapsed)}
                        className="absolute top-4 right-2 p-1.5 rounded-lg hover:bg-gray-800 transition-colors"
                    >
                        <ChevronRight className="w-5 h-5" />
                    </button>
                )}
            </div>

            {/* Navigation */}
            <nav className="flex-1 px-3 py-4 space-y-1 overflow-y-auto">
                {visibleNavigation.map((item) => {
                    const Icon = item.icon;
                    const active = isActive(item.href);

                    return (
                        <Link
                            key={item.name}
                            to={item.href}
                            className={`
                flex items-center space-x-3 px-3 py-2.5 rounded-lg
                transition-all duration-200
                ${active
                                    ? 'bg-indigo-600 text-white shadow-lg shadow-indigo-500/50'
                                    : 'text-gray-400 hover:bg-gray-800 hover:text-white'
                                }
                ${collapsed ? 'justify-center' : ''}
              `}
                            title={collapsed ? item.name : ''}
                        >
                            <Icon className="w-5 h-5 flex-shrink-0" />
                            {!collapsed && (
                                <div className="flex items-center justify-between flex-1">
                                    <span className="font-medium">{item.name}</span>
                                    {item.badge && (
                                        <span className="px-2 py-0.5 text-xs font-medium bg-purple-600 text-white rounded-full">
                                            {item.badge}
                                        </span>
                                    )}
                                </div>
                            )}
                        </Link>
                    );
                })}

                {/* Master Data Management - Hover Submenu */}
                {isSuperAdmin && (
                    <div
                        ref={masterDataMenuRef}
                        className="relative"
                        onMouseEnter={handleMasterDataMouseEnter}
                        onMouseLeave={handleMasterDataMouseLeave}
                    >
                        {/* Parent Item */}
                        <button
                            onClick={() => setMasterDataMenuOpen(prev => !prev)}
                            className={`
                w-full flex items-center space-x-3 px-3 py-2.5 rounded-lg
                transition-all duration-200
                ${isMasterDataActive
                                    ? 'bg-indigo-600 text-white shadow-lg shadow-indigo-500/50'
                                    : 'text-gray-400 hover:bg-gray-800 hover:text-white'
                                }
                ${collapsed ? 'justify-center' : ''}
              `}
                            title={collapsed ? 'Master Data' : ''}
                        >
                            <Database className="w-5 h-5 flex-shrink-0" />
                            {!collapsed && (
                                <div className="flex items-center justify-between flex-1">
                                    <span className="font-medium">Master Data</span>
                                    <ChevronRight
                                        className={`w-4 h-4 transition-transform duration-200 ${masterDataMenuOpen ? 'rotate-90' : ''}`}
                                    />
                                </div>
                            )}
                        </button>

                        {/* Flyout / Inline Submenu */}
                        <div
                            className={`
                ${collapsed ? 'absolute left-full top-0 ml-2' : 'overflow-hidden'}
                ${collapsed ? 'min-w-52' : ''}
                transition-all duration-200 ease-in-out
                ${masterDataMenuOpen
                                    ? collapsed
                                        ? 'opacity-100 visible translate-x-0'
                                        : 'max-h-96 opacity-100'
                                    : collapsed
                                        ? 'opacity-0 invisible -translate-x-2'
                                        : 'max-h-0 opacity-0'
                                }
              `}
                        >
                            <div className={`
                ${collapsed ? 'bg-gray-800 rounded-lg shadow-xl border border-gray-700 py-1' : 'pl-4 mt-1 space-y-0.5'}
              `}>
                                {masterDataSubItems.map((sub) => {
                                    const SubIcon = sub.icon;
                                    const subActive = location.pathname === sub.href;
                                    return (
                                        <Link
                                            key={sub.href}
                                            to={sub.href}
                                            className={`
                        flex items-center gap-2.5 px-3 py-2 rounded-lg text-sm
                        transition-all duration-150
                        ${subActive
                                                    ? 'bg-indigo-500/20 text-indigo-300 font-medium'
                                                    : 'text-gray-400 hover:bg-gray-800 hover:text-white'
                                                }
                      `}
                                        >
                                            <SubIcon className="w-4 h-4 flex-shrink-0" />
                                            <span>{sub.name}</span>
                                        </Link>
                                    );
                                })}
                            </div>
                        </div>
                    </div>
                )}

                {/* Supervisor Management - Hover Submenu */}
                {canViewSupervisorManagement && (
                    <div
                        ref={supervisorMenuRef}
                        className="relative"
                        onMouseEnter={handleSupervisorMouseEnter}
                        onMouseLeave={handleSupervisorMouseLeave}
                    >
                        {/* Parent Item */}
                        <button
                            className={`
                w-full flex items-center space-x-3 px-3 py-2.5 rounded-lg
                transition-all duration-200
                ${isSupervisorActive
                                    ? 'bg-indigo-600 text-white shadow-lg shadow-indigo-500/50'
                                    : 'text-gray-400 hover:bg-gray-800 hover:text-white'
                                }
                ${collapsed ? 'justify-center' : ''}
              `}
                            title={collapsed ? 'Supervisor Management' : ''}
                        >
                            <ClipboardCheck className="w-5 h-5 flex-shrink-0" />
                            {!collapsed && (
                                <div className="flex items-center justify-between flex-1">
                                    <span className="font-medium">Supervisors</span>
                                    <ChevronRight
                                        className={`w-4 h-4 transition-transform duration-200 ${supervisorMenuOpen ? 'rotate-90' : ''
                                            }`}
                                    />
                                </div>
                            )}
                        </button>

                        {/* Flyout Submenu */}
                        <div
                            className={`
                ${collapsed ? 'absolute left-full top-0 ml-2' : 'overflow-hidden'}
                ${collapsed ? 'min-w-48' : ''}
                transition-all duration-200 ease-in-out
                ${supervisorMenuOpen
                                    ? collapsed
                                        ? 'opacity-100 visible translate-x-0'
                                        : 'max-h-40 opacity-100'
                                    : collapsed
                                        ? 'opacity-0 invisible -translate-x-2'
                                        : 'max-h-0 opacity-0'
                                }
              `}
                        >
                            <div className={`
                ${collapsed ? 'bg-gray-800 rounded-lg shadow-xl border border-gray-700 py-1' : 'pl-4 mt-1 space-y-0.5'}
              `}>
                                {supervisorSubItems.map((sub) => {
                                    const SubIcon = sub.icon;
                                    const subActive = location.pathname === sub.href;
                                    return (
                                        <Link
                                            key={sub.href}
                                            to={sub.href}
                                            className={`
                        flex items-center gap-2.5 px-3 py-2 rounded-lg text-sm
                        transition-all duration-150
                        ${subActive
                                                    ? 'bg-indigo-500/20 text-indigo-300 font-medium'
                                                    : 'text-gray-400 hover:bg-gray-800 hover:text-white'
                                                }
                      `}
                                        >
                                            <SubIcon className="w-4 h-4 flex-shrink-0" />
                                            <span>{sub.name}</span>
                                        </Link>
                                    );
                                })}
                            </div>
                        </div>
                    </div>
                )}
            </nav>

            {/* Logout Button */}
            <div className="px-3 py-4 border-t border-gray-800">
                <button
                    onClick={logout}
                    className={`
            flex items-center space-x-3 px-3 py-2.5 rounded-lg
            w-full text-gray-400 hover:bg-red-600 hover:text-white
            transition-all duration-200
            ${collapsed ? 'justify-center' : ''}
          `}
                    title={collapsed ? 'Logout' : ''}
                >
                    <LogOut className="w-5 h-5 flex-shrink-0" />
                    {!collapsed && <span className="font-medium">Logout</span>}
                </button>
            </div>
        </div>
    );
};

export default AdminSidebar;
