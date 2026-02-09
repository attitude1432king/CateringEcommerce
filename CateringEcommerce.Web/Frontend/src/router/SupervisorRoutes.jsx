/**
 * SupervisorRoutes
 * Route configuration for the Supervisor Portal
 */

import { Routes, Route, Navigate, Outlet } from 'react-router-dom';
import { useSupervisorAuth } from '../contexts/SupervisorAuthContext';

// Pages
import {
    SupervisorDashboard,
    SupervisorProfile,
    AssignmentsList,
    AssignmentDetails,
    EventExecution,
    EarningsPage,
} from '../pages/supervisor';

// Registration (public - no auth needed)
import { RegistrationWizard } from '../components/supervisor/registration/RegistrationWizard';

// Protected route wrapper
const SupervisorProtectedRoute = () => {
    const { isAuthenticated, loading } = useSupervisorAuth();

    if (loading) {
        return (
            <div className="min-h-screen bg-gray-50 flex items-center justify-center">
                <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-blue-600" />
            </div>
        );
    }

    return isAuthenticated ? <Outlet /> : <Navigate to="/supervisor/login" replace />;
};

const SupervisorRoutes = () => {
    return (
        <Routes>
            {/* Public Routes */}
            <Route path="/register" element={<RegistrationWizard />} />

            {/* Protected Routes */}
            <Route element={<SupervisorProtectedRoute />}>
                <Route path="/dashboard" element={<SupervisorDashboard />} />
                <Route path="/profile" element={<SupervisorProfile />} />
                <Route path="/assignments" element={<AssignmentsList />} />
                <Route path="/assignments/:assignmentId" element={<AssignmentDetails />} />
                <Route path="/assignments/:assignmentId/execute" element={<EventExecution />} />
                <Route path="/earnings" element={<EarningsPage />} />
            </Route>

            {/* Default redirect */}
            <Route path="/" element={<Navigate to="/supervisor/dashboard" replace />} />
            <Route path="*" element={<Navigate to="/supervisor/dashboard" replace />} />
        </Routes>
    );
};

export default SupervisorRoutes;
