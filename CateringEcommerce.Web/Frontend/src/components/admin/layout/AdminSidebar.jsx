import { useState } from 'react';
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
  IndianRupee,
  MessageSquare,
} from 'lucide-react';
import { useAdminAuth } from '../../../contexts/AdminAuthContext';
import { usePermissions } from '../../../contexts/PermissionContext';

const AdminSidebar = () => {
  const [collapsed, setCollapsed] = useState(false);
  const location = useLocation();
  const { logout } = useAdminAuth();
  const { hasPermission, isSuperAdmin } = usePermissions();

  const navigation = [
    { name: 'Dashboard', href: '/admin/dashboard', icon: LayoutDashboard },
    { name: 'Analytics', href: '/admin/analytics', icon: BarChart3 },
    { name: 'Caterings', href: '/admin/caterings', icon: Store, permission: 'CATERING_VIEW' },
    { name: 'Partner Requests', href: '/admin/partner-requests', icon: FileCheck, permission: 'PARTNER_REQUEST_VIEW' },
    { name: 'Complaints', href: '/admin/complaints', icon: MessageSquare },
    { name: 'Users', href: '/admin/users', icon: Users, permission: 'USER_VIEW' },
    { name: 'Admin Users', href: '/admin/users/admins', icon: ShieldCheck, requireSuperAdmin: true },
    { name: 'Orders', href: '/admin/orders', icon: ShoppingCart, permission: 'ORDER_VIEW' },
    { name: 'Earnings', href: '/admin/earnings', icon: DollarSign, permission: 'EARNINGS_VIEW' },
    { name: 'Reviews', href: '/admin/reviews', icon: Star, permission: 'REVIEW_VIEW' },
    { name: 'Supervisor Registrations', href: '/admin/supervisor-registrations', icon: ClipboardCheck },
    { name: 'Supervisor Payments', href: '/admin/supervisor-payments', icon: IndianRupee },
    { name: 'Master Data', href: '/admin/master-data/cities', icon: Database, badge: 'Super Admin', requireSuperAdmin: true },
    { name: 'Settings', href: '/admin/settings', icon: Settings, permission: 'SYSTEM_CONFIG' },
  ];

  // Filter navigation items based on permissions
  const visibleNavigation = navigation.filter(item => {
    // Dashboard and Settings are always visible
    if (!item.permission && !item.requireSuperAdmin) return true;

    // Check if super admin is required
    if (item.requireSuperAdmin && !isSuperAdmin) return false;

    // Check if user has the required permission
    if (item.permission && !hasPermission(item.permission)) return false;

    return true;
  });

  const isActive = (path) => {
    // For nested routes like master-data, check if current path starts with the base path
    if (path.includes('/master-data')) {
      return location.pathname.startsWith('/admin/master-data');
    }
    return location.pathname === path;
  };

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
                ${
                  active
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
