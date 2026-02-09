/**
 * SupervisorDashboard Page
 * Main landing page for Event Supervisors
 */

import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
    LayoutDashboard,
    Calendar,
    CheckCircle,
    Star,
    IndianRupee,
    Clock,
    TrendingUp,
} from 'lucide-react';
import { getDashboard } from '../../services/api/supervisor/supervisorApi';
import {
    SupervisorStatusBadge,
    SupervisorTypeBadge,
    AuthorityLevelBadge,
} from '../../components/supervisor/common/badges';
import { formatCurrency } from '../../utils/supervisor/helpers';
import toast from 'react-hot-toast';

const SupervisorDashboard = () => {
    const navigate = useNavigate();
    const [dashboard, setDashboard] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        fetchDashboard();
    }, []);

    const fetchDashboard = async () => {
        try {
            const supervisorId = localStorage.getItem('supervisorId');
            if (!supervisorId) {
                navigate('/supervisor/login');
                return;
            }

            const response = await getDashboard(supervisorId);
            if (response.success) {
                setDashboard(response.data);
            } else {
                toast.error('Failed to load dashboard');
            }
        } catch (error) {
            console.error('Dashboard error:', error);
            toast.error('Failed to load dashboard');
        } finally {
            setLoading(false);
        }
    };

    if (loading) {
        return (
            <div className="flex justify-center items-center min-h-screen">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
            </div>
        );
    }

    if (!dashboard) {
        return (
            <div className="flex justify-center items-center min-h-screen">
                <p className="text-gray-600">Failed to load dashboard</p>
            </div>
        );
    }

    const { supervisor } = dashboard;

    return (
        <div className="min-h-screen bg-gray-50">
            {/* Header */}
            <div className="bg-white border-b border-gray-200">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
                    <div className="flex items-center justify-between">
                        <div className="flex items-center gap-4">
                            {/* Profile Photo */}
                            {supervisor.photoUrl ? (
                                <img
                                    src={supervisor.photoUrl}
                                    alt={`${supervisor.firstName} ${supervisor.lastName}`}
                                    className="w-16 h-16 rounded-full object-cover border-2 border-gray-200"
                                />
                            ) : (
                                <div className="w-16 h-16 rounded-full bg-blue-100 flex items-center justify-center text-blue-600 font-bold text-xl">
                                    {supervisor.firstName[0]}{supervisor.lastName[0]}
                                </div>
                            )}

                            {/* Name & Info */}
                            <div>
                                <h1 className="text-2xl font-bold text-gray-900">
                                    {supervisor.firstName} {supervisor.lastName}
                                </h1>
                                <div className="flex items-center gap-2 mt-1">
                                    <SupervisorTypeBadge type={supervisor.supervisorType} />
                                    <SupervisorStatusBadge status={supervisor.supervisorStatus} />
                                    <AuthorityLevelBadge level={supervisor.authorityLevel} />
                                </div>
                            </div>
                        </div>

                        {/* Quick Actions */}
                        <button
                            onClick={() => navigate('/supervisor/profile')}
                            className="px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
                        >
                            Edit Profile
                        </button>
                    </div>
                </div>
            </div>

            {/* Main Content */}
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                {/* Stats Grid */}
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
                    {/* Total Assignments */}
                    <StatsCard
                        icon={Calendar}
                        iconColor="text-blue-600"
                        iconBg="bg-blue-100"
                        label="Total Assignments"
                        value={dashboard.totalAssignments}
                        trend={null}
                    />

                    {/* Completed */}
                    <StatsCard
                        icon={CheckCircle}
                        iconColor="text-green-600"
                        iconBg="bg-green-100"
                        label="Completed"
                        value={dashboard.completedAssignments}
                        trend={null}
                    />

                    {/* Upcoming */}
                    <StatsCard
                        icon={Clock}
                        iconColor="text-orange-600"
                        iconBg="bg-orange-100"
                        label="Upcoming"
                        value={dashboard.upcomingAssignments}
                        trend={null}
                    />

                    {/* Average Rating */}
                    <StatsCard
                        icon={Star}
                        iconColor="text-yellow-600"
                        iconBg="bg-yellow-100"
                        label="Average Rating"
                        value={dashboard.averageRating ? `${dashboard.averageRating}/5` : 'N/A'}
                        trend={null}
                    />

                    {/* Total Earnings */}
                    <StatsCard
                        icon={IndianRupee}
                        iconColor="text-purple-600"
                        iconBg="bg-purple-100"
                        label="Total Earnings"
                        value={formatCurrency(dashboard.totalEarnings)}
                        trend={null}
                    />

                    {/* Pending Payments */}
                    <StatsCard
                        icon={TrendingUp}
                        iconColor="text-cyan-600"
                        iconBg="bg-cyan-100"
                        label="Pending Payments"
                        value={dashboard.pendingPayments}
                        trend={null}
                    />
                </div>

                <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                    {/* Recent Assignments */}
                    <div className="lg:col-span-2">
                        <RecentAssignmentsWidget
                            assignments={dashboard.recentAssignments}
                            onViewAll={() => navigate('/supervisor/assignments')}
                        />
                    </div>

                    {/* Permissions */}
                    <div>
                        <PermissionsDisplay supervisor={supervisor} />
                    </div>
                </div>
            </div>
        </div>
    );
};

// StatsCard Component
const StatsCard = ({ icon: Icon, iconColor, iconBg, label, value, trend }) => (
    <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
        <div className="flex items-center justify-between mb-4">
            <div className={`p-3 rounded-lg ${iconBg}`}>
                <Icon className={`w-6 h-6 ${iconColor}`} />
            </div>
            {trend && (
                <span className={`text-xs font-medium ${trend > 0 ? 'text-green-600' : 'text-red-600'}`}>
                    {trend > 0 ? '+' : ''}{trend}%
                </span>
            )}
        </div>
        <div>
            <p className="text-sm text-gray-600">{label}</p>
            <p className="text-2xl font-bold text-gray-900 mt-1">{value}</p>
        </div>
    </div>
);

// Recent Assignments Widget
const RecentAssignmentsWidget = ({ assignments, onViewAll }) => (
    <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
        <div className="flex items-center justify-between mb-4">
            <h2 className="text-lg font-semibold text-gray-900">
                <LayoutDashboard className="inline w-5 h-5 mr-2" />
                Recent Assignments
            </h2>
            <button
                onClick={onViewAll}
                className="text-sm font-medium text-blue-600 hover:text-blue-700"
            >
                View All →
            </button>
        </div>

        {assignments && assignments.length > 0 ? (
            <div className="space-y-3">
                {assignments.map((assignment) => (
                    <div
                        key={assignment.assignmentId}
                        className="flex items-center justify-between p-3 border border-gray-200 rounded-lg hover:bg-gray-50 cursor-pointer"
                        onClick={() => window.location.href = `/supervisor/assignments/${assignment.assignmentId}`}
                    >
                        <div className="flex-1">
                            <p className="text-sm font-medium text-gray-900">
                                {assignment.assignmentNumber}
                            </p>
                            <p className="text-xs text-gray-600 mt-1">
                                {assignment.vendorName} • {new Date(assignment.eventDate).toLocaleDateString()}
                            </p>
                        </div>
                        <span className={`text-xs px-2 py-1 rounded-full ${assignment.status === 'COMPLETED' ? 'bg-green-100 text-green-800' :
                                assignment.status === 'IN_PROGRESS' ? 'bg-blue-100 text-blue-800' :
                                    'bg-yellow-100 text-yellow-800'
                            }`}>
                            {assignment.status}
                        </span>
                    </div>
                ))}
            </div>
        ) : (
            <p className="text-sm text-gray-600 text-center py-8">
                No assignments yet
            </p>
        )}
    </div>
);

// Permissions Display
const PermissionsDisplay = ({ supervisor }) => (
    <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
        <h2 className="text-lg font-semibold text-gray-900 mb-4">
            Your Permissions
        </h2>

        <div className="space-y-3">
            {/* Release Payment */}
            <PermissionRow
                label="Release Payment"
                granted={supervisor.canReleasePayment}
                description="Approve final payment release"
            />

            {/* Approve Refund */}
            <PermissionRow
                label="Approve Refund"
                granted={supervisor.canApproveRefund}
                description="Approve refund requests"
            />

            {/* Mentor Others */}
            <PermissionRow
                label="Mentor Others"
                granted={supervisor.canMentorOthers}
                description="Guide new supervisors"
            />
        </div>

        {/* Info Box */}
        <div className="mt-4 bg-blue-50 border border-blue-200 rounded-lg p-3">
            <p className="text-xs text-blue-800">
                <strong>Note:</strong> Payment release and refund approval require admin authorization. You can request these actions, which will be reviewed by administrators.
            </p>
        </div>
    </div>
);

const PermissionRow = ({ label, granted, description }) => (
    <div className="flex items-start gap-3">
        <div className={`flex-shrink-0 w-5 h-5 rounded-full flex items-center justify-center mt-0.5 ${granted ? 'bg-green-100' : 'bg-gray-100'
            }`}>
            {granted ? (
                <CheckCircle className="w-3 h-3 text-green-600" />
            ) : (
                <div className="w-2 h-2 rounded-full bg-gray-400"></div>
            )}
        </div>
        <div className="flex-1">
            <p className={`text-sm font-medium ${granted ? 'text-gray-900' : 'text-gray-500'}`}>
                {label}
            </p>
            <p className="text-xs text-gray-600 mt-0.5">
                {description}
            </p>
        </div>
    </div>
);

export default SupervisorDashboard;
