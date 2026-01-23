import { Routes, Route, Navigate } from 'react-router-dom';
import AdminLogin from '../pages/admin/AdminLogin';
import AdminDashboard from '../pages/admin/AdminDashboard';
import AdminCaterings from '../pages/admin/AdminCaterings';
import AdminPartnerRequests from '../pages/admin/AdminPartnerRequests';
// Import other pages as you create them
// import AdminUsers from '../pages/admin/AdminUsers';
// import AdminEarnings from '../pages/admin/AdminEarnings';
// import AdminReviews from '../pages/admin/AdminReviews';

const AdminRoutes = () => {
  return (
    <Routes>
      {/* Public Route */}
      <Route path="/login" element={<AdminLogin />} />

      {/* Protected Routes */}
      <Route path="/dashboard" element={<AdminDashboard />} />
      <Route path="/caterings" element={<AdminCaterings />} />
      <Route path="/partner-requests" element={<AdminPartnerRequests />} />

      {/* Add more routes as you create pages */}
      {/* <Route path="/users" element={<AdminUsers />} /> */}
      {/* <Route path="/earnings" element={<AdminEarnings />} /> */}
      {/* <Route path="/reviews" element={<AdminReviews />} /> */}

      {/* Default redirect */}
      <Route path="/" element={<Navigate to="/admin/dashboard" replace />} />
      <Route path="*" element={<Navigate to="/admin/dashboard" replace />} />
    </Routes>
  );
};

export default AdminRoutes;
