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
} from 'lucide-react';
import { useAdminAuth } from '../../../contexts/AdminAuthContext';

const AdminSidebar = () => {
  const [collapsed, setCollapsed] = useState(false);
  const location = useLocation();
  const { logout } = useAdminAuth();

  const navigation = [
    { name: 'Dashboard', href: '/admin/dashboard', icon: LayoutDashboard },
    { name: 'Caterings', href: '/admin/caterings', icon: Store },
    { name: 'Partner Requests', href: '/admin/partner-requests', icon: FileCheck },
    { name: 'Users', href: '/admin/users', icon: Users },
    { name: 'Orders', href: '/admin/orders', icon: ShoppingCart },
    { name: 'Earnings', href: '/admin/earnings', icon: DollarSign },
    { name: 'Reviews', href: '/admin/reviews', icon: Star },
    { name: 'Settings', href: '/admin/settings', icon: Settings },
  ];

  const isActive = (path) => location.pathname === path;

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
        {navigation.map((item) => {
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
                <span className="font-medium">{item.name}</span>
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
