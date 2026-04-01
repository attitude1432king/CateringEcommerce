/**
 * SupervisorDashboard Page (REDESIGNED)
 * Modern portal landing page for Event Supervisors
 */

import { useEffect, useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import {
    LayoutDashboard,
    Calendar,
    CheckCircle,
    Star,
    IndianRupee,
    Clock,
    TrendingUp,
    User,
    ClipboardList,
    LogOut,
    ChevronRight,
} from 'lucide-react';
import { getDashboard } from '../../services/api/supervisor/supervisorApi';
import { useSupervisorAuth } from '../../contexts/SupervisorAuthContext';
import {
    SupervisorStatusBadge,
    SupervisorTypeBadge,
    AuthorityLevelBadge,
} from '../../components/supervisor/common/badges';
import { formatCurrency } from '../../utils/supervisor/helpers';
import toast from 'react-hot-toast';

// Navigation Header Component (reusable across supervisor pages)
export const SupervisorNavHeader = ({ activePath }) => {
    const navigate = useNavigate();
    const { supervisor, logout } = useSupervisorAuth();

    const handleLogout = () => {
        logout();
        navigate('/supervisor/login');
    };

    const navLinks = [
        { to: '/supervisor/dashboard', label: 'Dashboard', icon: LayoutDashboard },
        { to: '/supervisor/assignments', label: 'Assignments', icon: ClipboardList },
        { to: '/supervisor/earnings', label: 'Earnings', icon: IndianRupee },
        { to: '/supervisor/profile', label: 'Profile', icon: User },
    ];

    return (
        <nav className="bg-white border-b-2 border-neutral-100 shadow-sm sticky top-0 z-50">
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                <div className="flex items-center justify-between h-16">
                    <div className="flex items-center gap-6">
                        <Link to="/supervisor/dashboard" className="flex-shrink-0">
                            <img src="/logo.svg" alt="ENYVORA" className="h-10 w-auto" />
                        </Link>
                        <div className="hidden md:flex items-center gap-1">
                            {navLinks.map(({ to, label, icon: Icon }) => (
                                <Link
                                    key={to}
                                    to={to}
                                    className={`flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition-all duration-200 ${
                                        activePath === to
                                            ? 'bg-rose-50 text-rose-700'
                                            : 'text-neutral-600 hover:bg-neutral-50 hover:text-neutral-900'
                                    }`}
                                >
                                    <Icon className="w-4 h-4" />
                                    {label}
                                </Link>
                            ))}
                        </div>
                    </div>
                    <div className="flex items-center gap-4">
                        <div className="hidden sm:flex items-center gap-2">
                            {supervisor?.photoUrl ? (
                                <img src={supervisor.photoUrl} alt="" className="w-8 h-8 rounded-full object-cover border-2 border-neutral-200" />
                            ) : (
                                <div className="w-8 h-8 rounded-full bg-gradient-to-br from-rose-400 to-amber-400 flex items-center justify-center text-white font-bold text-xs">
                                    {supervisor?.firstName?.[0]}{supervisor?.lastName?.[0]}
                                </div>
                            )}
                            <span className="text-sm font-medium text-neutral-700">{supervisor?.firstName}</span>
                        </div>
                        <button
                            onClick={handleLogout}
                            className="p-2 text-neutral-500 hover:text-rose-600 hover:bg-rose-50 rounded-lg transition-all duration-200"
                            title="Logout"
                        >
                            <LogOut className="w-5 h-5" />
                        </button>
                    </div>
                </div>
            </div>
            {/* Mobile Nav */}
            <div className="md:hidden border-t border-neutral-100">
                <div className="flex justify-around px-2 py-2">
                    {navLinks.map(({ to, label, icon: Icon }) => (
                        <Link
                            key={to}
                            to={to}
                            className={`flex flex-col items-center gap-1 px-3 py-1.5 rounded-lg text-xs font-medium transition-all ${
                                activePath === to ? 'text-rose-600' : 'text-neutral-500'
                            }`}
                        >
                            <Icon className="w-5 h-5" />
                            {label}
                        </Link>
                    ))}
                </div>
            </div>
        </nav>
    );
};

// Stats Card
const StatsCard = ({ icon: Icon, iconColor, iconBg, label, value }) => (
    <div className="bg-white rounded-xl border-2 border-neutral-100 shadow-sm p-6 hover:shadow-md hover:border-neutral-200 transition-all duration-200">
        <div className="flex items-center justify-between mb-4">
            <div className={`p-3 rounded-xl ${iconBg}`}>
                <Icon className={`w-6 h-6 ${iconColor}`} />
            </div>
        </div>
        <p className="text-sm font-medium text-neutral-500">{label}</p>
        <p className="text-2xl font-bold text-neutral-900 mt-1">{value}</p>
    </div>
);

// Recent Assignments Widget
const RecentAssignmentsWidget = ({ assignments, onViewAll }) => (
    <div className="bg-white rounded-xl border-2 border-neutral-100 shadow-sm overflow-hidden">
        <div className="bg-gradient-to-r from-rose-50 to-amber-50 px-6 py-4 border-b-2 border-neutral-100">
            <div className="flex items-center justify-between">
                <div className="flex items-center gap-3">
                    <div className="w-10 h-10 bg-rose-100 rounded-lg flex items-center justify-center">
                        <ClipboardList className="w-5 h-5 text-rose-600" />
                    </div>
                    <h2 className="text-lg font-bold text-neutral-800">Recent Assignments</h2>
                </div>
                <button onClick={onViewAll} className="text-sm font-semibold text-rose-600 hover:text-rose-700 flex items-center gap-1 transition-colors">
                    View All <ChevronRight className="w-4 h-4" />
                </button>
            </div>
        </div>
        <div className="p-6">
            {assignments && assignments.length > 0 ? (
                <div className="space-y-3">
                    {assignments.map((assignment) => (
                        <Link
                            key={assignment.assignmentId}
                            to={`/supervisor/assignments/${assignment.assignmentId}`}
                            className="flex items-center justify-between p-4 border-2 border-neutral-100 rounded-xl hover:border-rose-200 hover:bg-rose-50/30 transition-all duration-200 group"
                        >
                            <div className="flex-1">
                                <p className="text-sm font-semibold text-neutral-800 group-hover:text-rose-700 transition-colors">
                                    {assignment.assignmentNumber}
                                </p>
                                <p className="text-xs text-neutral-500 mt-1">
                                    {assignment.cateringName || assignment.partnerName} &bull; {new Date(assignment.eventDate).toLocaleDateString()}
                                </p>
                            </div>
                            <span className={`text-xs px-3 py-1.5 rounded-full font-semibold ${
                                assignment.status === 'COMPLETED' ? 'bg-green-100 text-green-800' :
                                assignment.status === 'IN_PROGRESS' ? 'bg-blue-100 text-blue-800' :
                                'bg-amber-100 text-amber-800'
                            }`}>
                                {assignment.status}
                            </span>
                        </Link>
                    ))}
                </div>
            ) : (
                <div className="text-center py-10">
                    <div className="mx-auto w-16 h-16 bg-neutral-100 rounded-full flex items-center justify-center mb-3">
                        <ClipboardList className="w-8 h-8 text-neutral-400" />
                    </div>
                    <p className="text-sm font-medium text-neutral-500">No assignments yet</p>
                    <p className="text-xs text-neutral-400 mt-1">Your assignments will appear here</p>
                </div>
            )}
        </div>
    </div>
);

// Permissions Display
const PermissionsDisplay = ({ supervisor }) => (
    <div className="bg-white rounded-xl border-2 border-neutral-100 shadow-sm overflow-hidden">
        <div className="bg-gradient-to-r from-blue-50 to-indigo-50 px-6 py-4 border-b-2 border-neutral-100">
            <div className="flex items-center gap-3">
                <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
                    <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5 text-blue-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
                    </svg>
                </div>
                <h2 className="text-lg font-bold text-neutral-800">Your Permissions</h2>
            </div>
        </div>
        <div className="p-6 space-y-4">
            <PermissionRow label="Release Payment" granted={supervisor.canReleasePayment} description="Approve final payment release" />
            <PermissionRow label="Approve Refund" granted={supervisor.canApproveRefund} description="Approve refund requests" />
            <PermissionRow label="Mentor Others" granted={supervisor.canMentorOthers} description="Guide new supervisors" />
            <div className="bg-blue-50 border-l-4 border-blue-400 rounded-lg p-4 mt-4">
                <p className="text-xs text-blue-800">
                    <strong>Note:</strong> Payment release and refund approval require admin authorization.
                </p>
            </div>
        </div>
    </div>
);

const PermissionRow = ({ label, granted, description }) => (
    <div className="flex items-start gap-3">
        <div className={`flex-shrink-0 w-8 h-8 rounded-lg flex items-center justify-center mt-0.5 ${granted ? 'bg-green-100' : 'bg-neutral-100'}`}>
            {granted ? (
                <CheckCircle className="w-4 h-4 text-green-600" />
            ) : (
                <div className="w-2.5 h-2.5 rounded-full bg-neutral-400"></div>
            )}
        </div>
        <div className="flex-1">
            <p className={`text-sm font-semibold ${granted ? 'text-neutral-800' : 'text-neutral-500'}`}>{label}</p>
            <p className="text-xs text-neutral-500 mt-0.5">{description}</p>
        </div>
    </div>
);

// Main Dashboard Component
const SupervisorDashboard = () => {
    const navigate = useNavigate();
    const { supervisorId } = useSupervisorAuth(); // P1 FIX: Use context instead of localStorage
    const [dashboard, setDashboard] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        fetchDashboard();
    }, []);

    const fetchDashboard = async () => {
        try {
            // P1 FIX: supervisorId now from context instead of localStorage
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
            <div className="min-h-screen bg-gradient-to-br from-rose-50/30 via-white to-amber-50/30 flex items-center justify-center">
                <div className="text-center">
                    <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-rose-600 mx-auto mb-4"></div>
                    <p className="text-neutral-600 text-sm">Loading dashboard...</p>
                </div>
            </div>
        );
    }

    if (!dashboard) {
        return (
            <div className="min-h-screen bg-gradient-to-br from-rose-50/30 via-white to-amber-50/30 flex items-center justify-center">
                <div className="text-center">
                    <div className="mx-auto w-16 h-16 bg-neutral-100 rounded-full flex items-center justify-center mb-4">
                        <LayoutDashboard className="w-8 h-8 text-neutral-400" />
                    </div>
                    <p className="text-neutral-600 font-medium">Failed to load dashboard</p>
                    <button onClick={fetchDashboard} className="mt-4 px-5 py-2.5 bg-gradient-to-r from-rose-600 to-rose-500 text-white rounded-xl text-sm font-semibold hover:shadow-lg transition-all">
                        Try Again
                    </button>
                </div>
            </div>
        );
    }

    const { supervisor } = dashboard;

    return (
        <div className="min-h-screen bg-gradient-to-br from-rose-50/30 via-white to-amber-50/30">
            <SupervisorNavHeader activePath="/supervisor/dashboard" />

            {/* Welcome Header */}
            <div className="bg-gradient-to-r from-rose-50 to-amber-50 border-b-2 border-neutral-100">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                    <div className="flex items-center justify-between flex-wrap gap-4">
                        <div className="flex items-center gap-5">
                            {supervisor.photoUrl ? (
                                <img
                                    src={supervisor.photoUrl}
                                    alt={`${supervisor.firstName} ${supervisor.lastName}`}
                                    className="rounded-xl object-cover border-4 border-white shadow-lg"
                                    style={{ width: '72px', height: '72px' }}
                                />
                            ) : (
                                <div className="rounded-xl bg-gradient-to-br from-rose-500 to-amber-500 flex items-center justify-center text-white font-bold text-2xl shadow-lg border-4 border-white" style={{ width: '72px', height: '72px' }}>
                                    {supervisor.firstName[0]}{supervisor.lastName[0]}
                                </div>
                            )}
                            <div>
                                <h1 className="text-3xl font-bold text-neutral-800">
                                    Welcome back, {supervisor.firstName}!
                                </h1>
                                <div className="flex items-center gap-2 mt-2 flex-wrap">
                                    <SupervisorTypeBadge type={supervisor.supervisorType} />
                                    <SupervisorStatusBadge status={supervisor.supervisorStatus} />
                                    <AuthorityLevelBadge level={supervisor.authorityLevel} />
                                </div>
                            </div>
                        </div>
                        <button
                            onClick={() => navigate('/supervisor/profile')}
                            className="hidden sm:flex items-center gap-2 px-5 py-2.5 bg-white text-neutral-700 rounded-xl font-semibold border-2 border-neutral-200 hover:border-rose-300 hover:bg-rose-50 transition-all duration-200 shadow-sm"
                        >
                            <User className="w-4 h-4" />
                            Edit Profile
                        </button>
                    </div>
                </div>
            </div>

            {/* Main Content */}
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6 gap-4 mb-8">
                    <StatsCard icon={Calendar} iconColor="text-blue-600" iconBg="bg-blue-100" label="Total Assignments" value={dashboard.totalAssignments} />
                    <StatsCard icon={CheckCircle} iconColor="text-green-600" iconBg="bg-green-100" label="Completed" value={dashboard.completedAssignments} />
                    <StatsCard icon={Clock} iconColor="text-amber-600" iconBg="bg-amber-100" label="Upcoming" value={dashboard.upcomingAssignments} />
                    <StatsCard icon={Star} iconColor="text-yellow-600" iconBg="bg-yellow-100" label="Avg Rating" value={dashboard.averageRating ? `${dashboard.averageRating}/5` : 'N/A'} />
                    <StatsCard icon={IndianRupee} iconColor="text-purple-600" iconBg="bg-purple-100" label="Total Earnings" value={formatCurrency(dashboard.totalEarnings)} />
                    <StatsCard icon={TrendingUp} iconColor="text-cyan-600" iconBg="bg-cyan-100" label="Pending Payments" value={dashboard.pendingPayments} />
                </div>

                <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                    <div className="lg:col-span-2">
                        <RecentAssignmentsWidget assignments={dashboard.recentAssignments} onViewAll={() => navigate('/supervisor/assignments')} />
                    </div>
                    <div>
                        <PermissionsDisplay supervisor={supervisor} />
                    </div>
                </div>
            </div>
        </div>
    );
};

export default SupervisorDashboard;
