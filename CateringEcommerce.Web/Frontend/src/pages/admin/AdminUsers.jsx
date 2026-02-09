import { useState, useEffect } from 'react';
import { UserPlus, Search, RefreshCw, Filter, Download, Shield, ShieldCheck, Key, Edit, Trash2, Power } from 'lucide-react';
import AdminLayout from '../../components/admin/layout/AdminLayout';
import { ProtectedRoute } from '../../components/admin/auth/ProtectedRoute';
import { PermissionButton } from '../../components/admin/ui/PermissionButton';
import RoleBadge from '../../components/admin/user-management/RoleBadge';
import { adminManagementApi, roleManagementApi } from '../../services/adminApi';
import { toast } from 'react-hot-toast';
import { formatDistanceToNow } from 'date-fns';
import AdminUserForm from '../../components/admin/user-management/AdminUserForm';

/**
 * Admin Users Management Page
 *
 * Features:
 * - List all admin users with pagination
 * - Search by name, email, username
 * - Filter by role and status
 * - Create new admin users (Super Admin only, or with ADMIN_CREATE permission)
 * - Edit admin information
 * - Activate/Deactivate admin accounts (Super Admin only)
 * - Reset admin passwords (Super Admin only)
 * - Delete admin users (Super Admin only, or with ADMIN_DELETE permission)
 * - Super Admin protection (cannot delete last Super Admin)
 */
const AdminUsers = () => {
    const [admins, setAdmins] = useState([]);
    const [roles, setRoles] = useState([]);
    const [loading, setLoading] = useState(true);
    const [selectedAdmin, setSelectedAdmin] = useState(null);
    const [showUserForm, setShowUserForm] = useState(false);
    const [formMode, setFormMode] = useState('create'); // 'create' or 'edit'
    const [stats, setStats] = useState({
        total: 0,
        active: 0,
        inactive: 0,
        superAdmins: 0
    });

    // Filters state
    const [filters, setFilters] = useState({
        searchTerm: '',
        roleId: null,
        isActive: null,
        pageNumber: 1,
        pageSize: 10,
        sortBy: 'CreatedDate',
        sortOrder: 'DESC'
    });

    const [pagination, setPagination] = useState({
        totalCount: 0,
        totalPages: 0,
        currentPage: 1
    });

    // Fetch admin users
    useEffect(() => {
        fetchAdminUsers();
    }, [filters]);

    // Fetch roles for filter dropdown
    useEffect(() => {
        fetchRoles();
    }, []);

    const fetchAdminUsers = async () => {
        setLoading(true);
        try {
            const result = await adminManagementApi.getAdmins(filters);

            if (result.success && result.data) {
                setAdmins(result.data.admins || []);
                setPagination({
                    totalCount: result.data.totalCount || 0,
                    totalPages: result.data.totalPages || 0,
                    currentPage: result.data.pageNumber || 1
                });

                // Calculate stats
                const activeCount = result.data.admins?.filter(a => a.isActive).length || 0;
                const superAdminCount = result.data.admins?.filter(a => a.roleCode === 'SUPER_ADMIN').length || 0;

                setStats({
                    total: result.data.totalCount || 0,
                    active: activeCount,
                    inactive: (result.data.totalCount || 0) - activeCount,
                    superAdmins: superAdminCount
                });
            } else {
                toast.error(result.message || 'Failed to load admin users');
            }
        } catch (error) {
            console.error('Error fetching admin users:', error);
            toast.error('Network error. Please try again.');
        } finally {
            setLoading(false);
        }
    };

    const fetchRoles = async () => {
        try {
            const result = await roleManagementApi.getRoles();
            if (result.success && result.data) {
                setRoles(result.data);
            }
        } catch (error) {
            console.error('Error fetching roles:', error);
        }
    };

    const handleCreateAdmin = () => {
        setSelectedAdmin(null);
        setFormMode('create');
        setShowUserForm(true);
    };

    const handleEditAdmin = async (adminId) => {
        try {
            const result = await adminManagementApi.getAdminById(adminId);
            if (result.success && result.data) {
                setSelectedAdmin(result.data);
                setFormMode('edit');
                setShowUserForm(true);
            } else {
                toast.error('Failed to load admin details');
            }
        } catch (error) {
            console.error('Error fetching admin details:', error);
            toast.error('Network error. Please try again.');
        }
    };

    const handleToggleStatus = async (admin) => {
        const newStatus = !admin.isActive;
        const action = newStatus ? 'activate' : 'deactivate';

        if (!window.confirm(`Are you sure you want to ${action} ${admin.fullName}?`)) {
            return;
        }

        try {
            const result = await adminManagementApi.updateAdminStatus(admin.adminId, newStatus);

            if (result.success) {
                toast.success(`Admin user ${action}d successfully`);
                fetchAdminUsers(); // Refresh list
            } else {
                toast.error(result.message || `Failed to ${action} admin user`);
            }
        } catch (error) {
            console.error(`Error ${action}ing admin:`, error);
            toast.error('Network error. Please try again.');
        }
    };

    const handleResetPassword = async (admin) => {
        if (!window.confirm(`Are you sure you want to reset the password for ${admin.fullName}?`)) {
            return;
        }

        try {
            // Generate a temporary password (in production, this should be done server-side)
            const tempPassword = Math.random().toString(36).slice(-10);
            const passwordHash = btoa(tempPassword); // Simple encoding for demo; use proper SHA256 in production

            const result = await adminManagementApi.resetPassword(admin.adminId, passwordHash, true);

            if (result.success) {
                toast.success(`Password reset successfully. Temporary password: ${tempPassword}`);
                // In production, send this via email instead
            } else {
                toast.error(result.message || 'Failed to reset password');
            }
        } catch (error) {
            console.error('Error resetting password:', error);
            toast.error('Network error. Please try again.');
        }
    };

    const handleDeleteAdmin = async (admin) => {
        if (!window.confirm(`Are you sure you want to delete ${admin.fullName}? This action cannot be undone.`)) {
            return;
        }

        try {
            const result = await adminManagementApi.deleteAdmin(admin.adminId);

            if (result.success) {
                toast.success('Admin user deleted successfully');
                fetchAdminUsers(); // Refresh list
            } else {
                toast.error(result.message || 'Failed to delete admin user');
            }
        } catch (error) {
            console.error('Error deleting admin:', error);
            toast.error('Network error. Please try again.');
        }
    };

    const handleFormSuccess = () => {
        setShowUserForm(false);
        setSelectedAdmin(null);
        fetchAdminUsers(); // Refresh list
    };

    const handleFilterChange = (key, value) => {
        setFilters({
            ...filters,
            [key]: value,
            pageNumber: 1 // Reset to first page
        });
    };

    const handlePageChange = (pageNumber) => {
        setFilters({
            ...filters,
            pageNumber
        });
    };

    const formatLastLogin = (lastLogin) => {
        if (!lastLogin) return 'Never';
        return formatDistanceToNow(new Date(lastLogin), { addSuffix: true });
    };

    return (
        <ProtectedRoute requiredPermissions={['ADMIN_VIEW']}>
            <AdminLayout>
                <div className="p-6">
                    {/* Header */}
                    <div className="mb-6">
                        <div className="flex items-center justify-between">
                            <div>
                                <h1 className="text-2xl font-bold text-gray-900 flex items-center gap-2">
                                    <ShieldCheck className="w-7 h-7 text-blue-600" />
                                    Admin Users
                                </h1>
                                <p className="text-gray-600 mt-1">
                                    Manage admin users, roles, and permissions
                                </p>
                            </div>
                            <div className="flex gap-3">
                                <button
                                    onClick={fetchAdminUsers}
                                    className="flex items-center gap-2 px-4 py-2 text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
                                >
                                    <RefreshCw className="w-4 h-4" />
                                    Refresh
                                </button>
                                <PermissionButton
                                    requiredPermissions={['ADMIN_CREATE']}
                                    onClick={handleCreateAdmin}
                                    className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
                                >
                                    <UserPlus className="w-4 h-4" />
                                    Add Admin User
                                </PermissionButton>
                            </div>
                        </div>

                        {/* Stats Cards */}
                        <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mt-6">
                            <div className="bg-white p-4 rounded-lg border border-gray-200">
                                <div className="text-sm text-gray-600">Total Admins</div>
                                <div className="text-2xl font-bold text-gray-900 mt-1">{stats.total}</div>
                            </div>
                            <div className="bg-green-50 p-4 rounded-lg border border-green-200">
                                <div className="text-sm text-green-700">Active</div>
                                <div className="text-2xl font-bold text-green-900 mt-1">{stats.active}</div>
                            </div>
                            <div className="bg-gray-50 p-4 rounded-lg border border-gray-200">
                                <div className="text-sm text-gray-600">Inactive</div>
                                <div className="text-2xl font-bold text-gray-900 mt-1">{stats.inactive}</div>
                            </div>
                            <div className="bg-red-50 p-4 rounded-lg border border-red-200">
                                <div className="text-sm text-red-700">Super Admins</div>
                                <div className="text-2xl font-bold text-red-900 mt-1">{stats.superAdmins}</div>
                            </div>
                        </div>
                    </div>

                    {/* Filters */}
                    <div className="bg-white p-4 rounded-lg border border-gray-200 mb-6">
                        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
                            {/* Search */}
                            <div className="md:col-span-2">
                                <label className="block text-sm font-medium text-gray-700 mb-1">
                                    Search
                                </label>
                                <div className="relative">
                                    <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-4 h-4 text-gray-400" />
                                    <input
                                        type="text"
                                        placeholder="Search by name, email, or username..."
                                        value={filters.searchTerm}
                                        onChange={(e) => handleFilterChange('searchTerm', e.target.value)}
                                        className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                                    />
                                </div>
                            </div>

                            {/* Role Filter */}
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">
                                    Role
                                </label>
                                <select
                                    value={filters.roleId || ''}
                                    onChange={(e) => handleFilterChange('roleId', e.target.value ? parseInt(e.target.value) : null)}
                                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                                >
                                    <option value="">All Roles</option>
                                    {roles.map((role) => (
                                        <option key={role.roleId} value={role.roleId}>
                                            {role.roleName}
                                        </option>
                                    ))}
                                </select>
                            </div>

                            {/* Status Filter */}
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">
                                    Status
                                </label>
                                <select
                                    value={filters.isActive === null ? '' : filters.isActive.toString()}
                                    onChange={(e) => handleFilterChange('isActive', e.target.value === '' ? null : e.target.value === 'true')}
                                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                                >
                                    <option value="">All Status</option>
                                    <option value="true">Active</option>
                                    <option value="false">Inactive</option>
                                </select>
                            </div>
                        </div>
                    </div>

                    {/* Table */}
                    <div className="bg-white rounded-lg border border-gray-200 overflow-hidden">
                        <div className="overflow-x-auto">
                            <table className="min-w-full divide-y divide-gray-200">
                                <thead className="bg-gray-50">
                                    <tr>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                            Admin
                                        </th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                            Role
                                        </th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                            Status
                                        </th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                            Last Login
                                        </th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                            Created Date
                                        </th>
                                        <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                                            Actions
                                        </th>
                                    </tr>
                                </thead>
                                <tbody className="bg-white divide-y divide-gray-200">
                                    {loading ? (
                                        <tr>
                                            <td colSpan="6" className="px-6 py-12 text-center">
                                                <div className="flex justify-center">
                                                    <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
                                                </div>
                                            </td>
                                        </tr>
                                    ) : admins.length === 0 ? (
                                        <tr>
                                            <td colSpan="6" className="px-6 py-12 text-center text-gray-500">
                                                No admin users found
                                            </td>
                                        </tr>
                                    ) : (
                                        admins.map((admin) => (
                                            <tr key={admin.adminId} className="hover:bg-gray-50">
                                                <td className="px-6 py-4 whitespace-nowrap">
                                                    <div>
                                                        <div className="text-sm font-medium text-gray-900">
                                                            {admin.fullName}
                                                        </div>
                                                        <div className="text-sm text-gray-500">{admin.email}</div>
                                                        <div className="text-xs text-gray-400">@{admin.username}</div>
                                                    </div>
                                                </td>
                                                <td className="px-6 py-4 whitespace-nowrap">
                                                    <RoleBadge
                                                        roleName={admin.roleName}
                                                        roleColor={admin.roleColor}
                                                    />
                                                </td>
                                                <td className="px-6 py-4 whitespace-nowrap">
                                                    {admin.isActive ? (
                                                        <span className="inline-flex px-2 py-1 text-xs font-semibold rounded-full bg-green-100 text-green-800">
                                                            Active
                                                        </span>
                                                    ) : (
                                                        <span className="inline-flex px-2 py-1 text-xs font-semibold rounded-full bg-gray-100 text-gray-800">
                                                            Inactive
                                                        </span>
                                                    )}
                                                </td>
                                                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                                    {formatLastLogin(admin.lastLogin)}
                                                </td>
                                                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                                    {new Date(admin.createdDate).toLocaleDateString()}
                                                </td>
                                                <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                                                    <div className="flex items-center justify-end gap-2">
                                                        <PermissionButton
                                                            requiredPermissions={['ADMIN_EDIT']}
                                                            onClick={() => handleEditAdmin(admin.adminId)}
                                                            className="text-blue-600 hover:text-blue-900"
                                                            title="Edit"
                                                        >
                                                            <Edit className="w-4 h-4" />
                                                        </PermissionButton>
                                                        <PermissionButton
                                                            requiredPermissions={['ADMIN_EDIT']}
                                                            onClick={() => handleToggleStatus(admin)}
                                                            className={admin.isActive ? 'text-orange-600 hover:text-orange-900' : 'text-green-600 hover:text-green-900'}
                                                            title={admin.isActive ? 'Deactivate' : 'Activate'}
                                                        >
                                                            <Power className="w-4 h-4" />
                                                        </PermissionButton>
                                                        <PermissionButton
                                                            requireSuperAdmin={true}
                                                            onClick={() => handleResetPassword(admin)}
                                                            className="text-purple-600 hover:text-purple-900"
                                                            title="Reset Password"
                                                        >
                                                            <Key className="w-4 h-4" />
                                                        </PermissionButton>
                                                        <PermissionButton
                                                            requiredPermissions={['ADMIN_DELETE']}
                                                            onClick={() => handleDeleteAdmin(admin)}
                                                            className="text-red-600 hover:text-red-900"
                                                            title="Delete"
                                                        >
                                                            <Trash2 className="w-4 h-4" />
                                                        </PermissionButton>
                                                    </div>
                                                </td>
                                            </tr>
                                        ))
                                    )}
                                </tbody>
                            </table>
                        </div>

                        {/* Pagination */}
                        {pagination.totalPages > 1 && (
                            <div className="bg-white px-4 py-3 flex items-center justify-between border-t border-gray-200 sm:px-6">
                                <div className="flex-1 flex justify-between sm:hidden">
                                    <button
                                        onClick={() => handlePageChange(pagination.currentPage - 1)}
                                        disabled={pagination.currentPage === 1}
                                        className="relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                                    >
                                        Previous
                                    </button>
                                    <button
                                        onClick={() => handlePageChange(pagination.currentPage + 1)}
                                        disabled={pagination.currentPage === pagination.totalPages}
                                        className="ml-3 relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                                    >
                                        Next
                                    </button>
                                </div>
                                <div className="hidden sm:flex-1 sm:flex sm:items-center sm:justify-between">
                                    <div>
                                        <p className="text-sm text-gray-700">
                                            Showing{' '}
                                            <span className="font-medium">
                                                {(pagination.currentPage - 1) * filters.pageSize + 1}
                                            </span>{' '}
                                            to{' '}
                                            <span className="font-medium">
                                                {Math.min(pagination.currentPage * filters.pageSize, pagination.totalCount)}
                                            </span>{' '}
                                            of <span className="font-medium">{pagination.totalCount}</span> results
                                        </p>
                                    </div>
                                    <div>
                                        <nav className="relative z-0 inline-flex rounded-md shadow-sm -space-x-px">
                                            <button
                                                onClick={() => handlePageChange(pagination.currentPage - 1)}
                                                disabled={pagination.currentPage === 1}
                                                className="relative inline-flex items-center px-2 py-2 rounded-l-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                                            >
                                                Previous
                                            </button>
                                            {[...Array(pagination.totalPages)].map((_, index) => (
                                                <button
                                                    key={index + 1}
                                                    onClick={() => handlePageChange(index + 1)}
                                                    className={`relative inline-flex items-center px-4 py-2 border text-sm font-medium ${
                                                        pagination.currentPage === index + 1
                                                            ? 'z-10 bg-blue-50 border-blue-500 text-blue-600'
                                                            : 'bg-white border-gray-300 text-gray-500 hover:bg-gray-50'
                                                    }`}
                                                >
                                                    {index + 1}
                                                </button>
                                            ))}
                                            <button
                                                onClick={() => handlePageChange(pagination.currentPage + 1)}
                                                disabled={pagination.currentPage === pagination.totalPages}
                                                className="relative inline-flex items-center px-2 py-2 rounded-r-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                                            >
                                                Next
                                            </button>
                                        </nav>
                                    </div>
                                </div>
                            </div>
                        )}
                    </div>
                </div>

                {/* Admin User Form Modal */}
                {showUserForm && (
                    <AdminUserForm
                        mode={formMode}
                        adminData={selectedAdmin}
                        roles={roles}
                        onSuccess={handleFormSuccess}
                        onCancel={() => {
                            setShowUserForm(false);
                            setSelectedAdmin(null);
                        }}
                    />
                )}
            </AdminLayout>
        </ProtectedRoute>
    );
};

export default AdminUsers;
