import { Routes, Route, Navigate } from 'react-router-dom';
import { ChefHat, Apple, CalendarDays, Truck } from 'lucide-react';
import AdminLogin from '../pages/admin/AdminLogin';
import AdminDashboard from '../pages/admin/AdminDashboard';
import AdminCaterings from '../pages/admin/AdminCaterings';
import AdminPartnerRequests from '../pages/admin/AdminPartnerRequests';
import AdminUsers from '../pages/admin/AdminUsers';
import AdminSettings from '../pages/admin/AdminSettings';
import AdminAnalytics from '../pages/admin/AdminAnalytics';
import AdminComplaints from '../pages/admin/AdminComplaints';

// Master Data Management
import MasterDataLayout from '../pages/admin/masterdata/MasterDataLayout';
import CityManagement from '../pages/admin/masterdata/CityManagement';
import FoodCategoryManagement from '../pages/admin/masterdata/FoodCategoryManagement';
import CateringTypeManagement from '../pages/admin/masterdata/CateringTypeManagement';
import GuestCategoryManagement from '../pages/admin/masterdata/GuestCategoryManagement';
import ThemeManagement from '../pages/admin/masterdata/ThemeManagement';

// Supervisor Management
import AdminSupervisorRegistrations from '../pages/admin/AdminSupervisorRegistrations';
import AdminSupervisorPayments from '../pages/admin/AdminSupervisorPayments';

const AdminRoutes = () => {
  return (
    <Routes>
      {/* Public Route */}
      <Route path="/login" element={<AdminLogin />} />

      {/* Protected Routes */}
      <Route path="/dashboard" element={<AdminDashboard />} />
      <Route path="/analytics" element={<AdminAnalytics />} />
      <Route path="/caterings" element={<AdminCaterings />} />
      <Route path="/partner-requests" element={<AdminPartnerRequests />} />
      <Route path="/complaints" element={<AdminComplaints />} />
      <Route path="/users/admins" element={<AdminUsers />} />
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

      {/* Supervisor Management Routes */}
      <Route path="/supervisor-registrations" element={<AdminSupervisorRegistrations />} />
      <Route path="/supervisor-payments" element={<AdminSupervisorPayments />} />

      {/* Default redirect */}
      <Route path="/" element={<Navigate to="/admin/dashboard" replace />} />
      <Route path="*" element={<Navigate to="/admin/dashboard" replace />} />
    </Routes>
  );
};

export default AdminRoutes;
