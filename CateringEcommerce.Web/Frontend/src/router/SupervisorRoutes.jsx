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
    SupervisorLogin,
    AssignmentsList,
    AssignmentDetails,
    EventExecution,
    EarningsPage,
    WithdrawalRequest,
} from '../pages/supervisor';

// Registration (public - no auth needed)
import { RegistrationWizard } from '../components/supervisor/registration/RegistrationWizard';

// Protected route wrapper
const SupervisorProtectedRoute = () => {
    const { isAuthenticated, loading } = useSupervisorAuth();

    if (loading) {
        return (
            <div className="min-h-screen bg-gradient-to-br from-rose-50 via-white to-amber-50 flex items-center justify-center">
                <div className="text-center">
                    <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-rose-600 mx-auto mb-4" />
                    <p className="text-neutral-600 text-sm">Loading...</p>
                </div>
            </div>
        );
    }

    return isAuthenticated ? <Outlet /> : <Navigate to="/supervisor/login" replace />;
};

const SupervisorRoutes = () => {
    return (
        <Routes>
            {/* Public Routes */}
            <Route path="/login" element={<SupervisorLogin />} />
            <Route path="/register" element={<RegistrationWizard />} />

            {/* Protected Routes */}
            <Route element={<SupervisorProtectedRoute />}>
                <Route path="/dashboard" element={<SupervisorDashboard />} />
                <Route path="/profile" element={<SupervisorProfile />} />
                <Route path="/assignments" element={<AssignmentsList />} />
                <Route path="/assignments/:assignmentId" element={<AssignmentDetails />} />
                <Route path="/assignments/:assignmentId/execute" element={<EventExecution />} />
                <Route path="/earnings" element={<EarningsPage />} />
                <Route path="/withdrawal" element={<WithdrawalRequest />} />
            </Route>

            {/* Default redirect */}
            <Route path="/" element={<Navigate to="/supervisor/dashboard" replace />} />
            <Route path="*" element={<Navigate to="/supervisor/dashboard" replace />} />
        </Routes>
    );
};

export default SupervisorRoutes;
