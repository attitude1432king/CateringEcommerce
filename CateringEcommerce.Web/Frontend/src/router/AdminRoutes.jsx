import { Routes, Route, Navigate, Outlet } from 'react-router-dom';
import { ChefHat, Apple, CalendarDays, Truck } from 'lucide-react';
import { useAdminAuth } from '../contexts/AdminAuthContext';
import AdminLogin from '../pages/admin/AdminLogin';
import AdminDashboard from '../pages/admin/AdminDashboard';
import AdminCaterings from '../pages/admin/AdminCaterings';
import AdminPartnerRequests from '../pages/admin/AdminPartnerRequests';
import AdminUsers from '../pages/admin/AdminUsers';
import AdminCustomerUsers from '../pages/admin/AdminCustomerUsers';
import AdminSettings from '../pages/admin/AdminSettings';
import AdminAnalytics from '../pages/admin/AdminAnalytics';
import AdminComplaints from '../pages/admin/AdminComplaints';
import AdminOrders from '../pages/admin/AdminOrders';
import AdminEarnings from '../pages/admin/AdminEarnings';
import AdminReviews from '../pages/admin/AdminReviews';
import Forbidden from '../pages/admin/Forbidden';

// Master Data Management
import MasterDataLayout from '../pages/admin/masterdata/MasterDataLayout';
import CityManagement from '../pages/admin/masterdata/CityManagement';
import FoodCategoryManagement from '../pages/admin/masterdata/FoodCategoryManagement';
import CateringTypeManagement from '../pages/admin/masterdata/CateringTypeManagement';
import GuestCategoryManagement from '../pages/admin/masterdata/GuestCategoryManagement';
import ThemeManagement from '../pages/admin/masterdata/ThemeManagement';

// Supervisor Management
import PendingSupervisorRequests from '../pages/admin/PendingSupervisorRequests';
import ApprovedSupervisors from '../pages/admin/ApprovedSupervisors';

// Protected route wrapper - redirects to admin login if not authenticated
const AdminProtectedRoute = () => {
  const { loading, isAuthenticated } = useAdminAuth();

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="text-center">
          <div className="w-12 h-12 border-4 border-indigo-600 border-t-transparent rounded-full animate-spin mx-auto mb-4" />
          <p className="text-gray-600 text-sm">Loading...</p>
        </div>
      </div>
    );
  }

  return isAuthenticated() ? <Outlet /> : <Navigate to="/admin/login" replace />;
};

const AdminRoutes = () => {
  return (
    <Routes>
      {/* Public Route */}
      <Route path="/login" element={<AdminLogin />} />

      {/* Protected Routes */}
      <Route element={<AdminProtectedRoute />}>
        {/* P0 FIX: 403 Forbidden page for permission denied scenarios */}
        <Route path="/403" element={<Forbidden />} />

        <Route path="/dashboard" element={<AdminDashboard />} />
        <Route path="/analytics" element={<AdminAnalytics />} />
        <Route path="/caterings" element={<AdminCaterings />} />
        <Route path="/partner-requests" element={<AdminPartnerRequests />} />
        <Route path="/complaints" element={<AdminComplaints />} />
        <Route path="/users" element={<AdminCustomerUsers />} />
        <Route path="/users/admins" element={<AdminUsers />} />
        <Route path="/orders" element={<AdminOrders />} />
        <Route path="/earnings" element={<AdminEarnings />} />
        <Route path="/reviews" element={<AdminReviews />} />
        <Route path="/settings" element={<AdminSettings />} />

        {/* Master Data Management Routes - Super Admin Only */}
        <Route path="/master-data" element={<MasterDataLayout />}>
          <Route path="cities" element={<CityManagement />} />
          <Route path="food-categories" element={<FoodCategoryManagement />} />
          <Route path="cuisine-types" element={<CateringTypeManagement categoryId={2} categoryName="Cuisine Types" icon={ChefHat} />} />
          <Route path="food-types" element={<CateringTypeManagement categoryId={1} categoryName="Food Types" icon={Apple} />} />
          <Route path="event-types" element={<CateringTypeManagement categoryId={3} categoryName="Event Types" icon={CalendarDays} />} />
          <Route path="service-types" element={<CateringTypeManagement categoryId={4} categoryName="Service Types" icon={Truck} />} />
          <Route path="guest-categories" element={<GuestCategoryManagement />} />
          <Route path="themes" element={<ThemeManagement />} />
          <Route index element={<Navigate to="cities" replace />} />
        </Route>

        {/* Supervisor Management - Sub-tabs */}
        <Route path="/supervisor-management" element={<Navigate to="/admin/supervisor-management/pending" replace />} />
        <Route path="/supervisor-management/pending" element={<PendingSupervisorRequests />} />
        <Route path="/supervisor-management/approved" element={<ApprovedSupervisors />} />
      </Route>

      {/* Default redirect */}
      <Route path="/" element={<Navigate to="/admin/dashboard" replace />} />
      <Route path="*" element={<Navigate to="/admin/dashboard" replace />} />
    </Routes>
  );
};

export default AdminRoutes;
