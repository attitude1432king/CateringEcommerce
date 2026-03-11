import React from 'react';
import { Database, MapPin, UtensilsCrossed, ChefHat, Apple, CalendarDays, Truck, Users, Palette, X } from 'lucide-react';
import { Outlet, NavLink, useNavigate } from 'react-router-dom';

const MasterDataLayout = () => {
  const navigate = useNavigate();

  const handleClose = () => {
    navigate('/admin/dashboard');
  };

  const tabs = [
    { path: 'cities', label: 'Cities', icon: MapPin },
    { path: 'food-categories', label: 'Food Categories', icon: UtensilsCrossed },
    { path: 'cuisine-types', label: 'Cuisine Types', icon: ChefHat },
    { path: 'food-types', label: 'Food Types', icon: Apple },
    { path: 'event-types', label: 'Event Types', icon: CalendarDays },
    { path: 'service-types', label: 'Service Types', icon: Truck },
    { path: 'guest-categories', label: 'Guest Categories', icon: Users },
    { path: 'themes', label: 'Themes', icon: Palette },
  ];

  return (
    <div className="p-6">
      {/* Page Header */}
      <div className="mb-6">
        <div className="flex items-center justify-between mb-2">
          <div className="flex items-center gap-3">
            <Database className="w-8 h-8 text-purple-600" />
            <h1 className="text-3xl font-bold text-gray-900">Master Data Management</h1>
          </div>
          <button
            onClick={handleClose}
            className="p-2 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-lg transition-colors"
            title="Close and return to dashboard"
          >
            <X className="w-6 h-6" />
          </button>
        </div>
        <p className="text-gray-600">Manage system master data and reference tables</p>
        <div className="mt-3 inline-flex items-center px-3 py-1 bg-purple-100 text-purple-700 rounded-full text-sm font-medium">
          <span className="w-2 h-2 bg-purple-600 rounded-full mr-2"></span>
          Super Admin Only
        </div>
      </div>

      {/* Horizontal Tab Navigation */}
      <div className="bg-white rounded-lg shadow-sm border mb-6">
        <nav className="flex overflow-x-auto">
          {tabs.map(tab => (
            <NavLink
              key={tab.path}
              to={`/admin/master-data/${tab.path}`}
              className={({ isActive }) =>
                `flex items-center gap-2 px-6 py-4 border-b-2 whitespace-nowrap transition-all
                ${isActive
                  ? 'border-purple-600 text-purple-600 font-semibold bg-purple-50'
                  : 'border-transparent text-gray-600 hover:text-purple-600 hover:bg-gray-50'}`
              }
            >
              <tab.icon className="w-5 h-5" />
              <span>{tab.label}</span>
            </NavLink>
          ))}
        </nav>
      </div>

      {/* Content Area */}
      <Outlet />
    </div>
  );
};

export default MasterDataLayout;
