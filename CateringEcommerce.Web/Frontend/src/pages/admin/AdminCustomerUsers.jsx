import { useState, useEffect, useCallback } from 'react';
import {
    Users, Search, RefreshCw, Download, Trash2, RotateCcw,
    ShieldOff, ShieldCheck, Eye, ChevronLeft, ChevronRight,
    Filter, X, MapPin, UserCheck, UserX
} from 'lucide-react';
import AdminLayout from '../../components/admin/layout/AdminLayout';
import { ProtectedRoute } from '../../components/admin/auth/ProtectedRoute';
import UserDetailDrawer from '../../components/admin/users/UserDetailDrawer';
import { userApi, locationApi } from '../../services/adminApi';
import { useConfirmation } from '../../contexts/ConfirmationContext';
import { toast } from 'react-hot-toast';
import { formatDistanceToNow } from 'date-fns';

const AdminCustomerUsers = () => {
    const confirm = useConfirmation();
    const [users, setUsers] = useState([]);
    const [loading, setLoading] = useState(true);
    const [selectedUser, setSelectedUser] = useState(null);
    const [showDetailDrawer, setShowDetailDrawer] = useState(false);
    const [showFilters, setShowFilters] = useState(false);
    const [showDeletedUsers, setShowDeletedUsers] = useState(false);

    // Block reason modal state
    const [blockReasonModal, setBlockReasonModal] = useState({ open: false, user: null, reason: '' });

    // Location data for filters
    const [states, setStates] = useState([]);
    const [cities, setCities] = useState([]);

    // Stats
    const [stats, setStats] = useState({
        total: 0,
        active: 0,
        blocked: 0,
        inactive: 0
    });

    // Filters
    const [filters, setFilters] = useState({
        searchTerm: '',
        isActive: null,
        isBlocked: null,
        isDeleted: null,
        stateId: null,
        cityId: null,
        pageNumber: 1,
        pageSize: 15,
        sortBy: 'CreatedDate',
        sortOrder: 'DESC'
    });

    const [pagination, setPagination] = useState({
        totalRecords: 0,
        totalPages: 0,
        currentPage: 1
    });

    // Debounced search
    const [searchInput, setSearchInput] = useState('');
    useEffect(() => {
        const timer = setTimeout(() => {
            setFilters(prev => ({ ...prev, searchTerm: searchInput, pageNumber: 1 }));
        }, 400);
        return () => clearTimeout(timer);
    }, [searchInput]);

    // Fetch users
    const fetchUsers = useCallback(async () => {
        setLoading(true);
        try {
            const params = {};
            if (filters.searchTerm) params.SearchTerm = filters.searchTerm;
            if (filters.isActive !== null) params.IsActive = filters.isActive;
            if (filters.isBlocked !== null) params.IsBlocked = filters.isBlocked;
            if (showDeletedUsers) params.IsDeleted = true;
            if (filters.stateId) params.StateId = filters.stateId;
            if (filters.cityId) params.CityId = filters.cityId;
            params.PageNumber = filters.pageNumber;
            params.PageSize = filters.pageSize;
            params.SortBy = filters.sortBy;
            params.SortOrder = filters.sortOrder;

            const result = await userApi.getAll(params);

            if (result.result && result.data) {
                const data = result.data;
                setUsers(data.users || []);
                setPagination({
                    totalRecords: data.totalRecords || 0,
                    totalPages: data.totalPages || 0,
                    currentPage: data.pageNumber || 1
                });

                // Calculate stats from current page data context
                const activeCount = (data.users || []).filter(u => u.isActive && !u.isBlocked).length;
                const blockedCount = (data.users || []).filter(u => u.isBlocked).length;
                const inactiveCount = (data.users || []).filter(u => !u.isActive).length;

                setStats({
                    total: data.totalRecords || 0,
                    active: activeCount,
                    blocked: blockedCount,
                    inactive: inactiveCount
                });
            }
        } catch (error) {
            console.error('Error fetching users:', error);
            toast.error('Failed to load users');
        } finally {
            setLoading(false);
        }
    }, [filters, showDeletedUsers]);

    useEffect(() => {
        fetchUsers();
    }, [fetchUsers]);

    // Fetch states on mount
    useEffect(() => {
        const loadStates = async () => {
            try {
                const result = await locationApi.getStates();
                if (Array.isArray(result)) {
                    setStates(result);
                } else if (result?.data && Array.isArray(result.data)) {
                    setStates(result.data);
                }
            } catch (error) {
                console.error('Error loading states:', error);
            }
        };
        loadStates();
    }, []);

    // Fetch cities when state changes
    useEffect(() => {
        const loadCities = async () => {
            if (!filters.stateId) {
                setCities([]);
                return;
            }
            try {
                const result = await locationApi.getCities(filters.stateId);
                if (Array.isArray(result)) {
                    setCities(result);
                } else if (result?.data && Array.isArray(result.data)) {
                    setCities(result.data);
                }
            } catch (error) {
                console.error('Error loading cities:', error);
            }
        };
        loadCities();
    }, [filters.stateId]);

    // Actions
    const handleBlockUnblock = async (user) => {
        const newBlocked = !user.isBlocked;

        if (newBlocked) {
            // Show block reason modal
            setBlockReasonModal({ open: true, user, reason: '' });
            return;
        }

        // Unblock flow - use confirmation
        const confirmed = await confirm({
            title: `Unblock ${user.fullName}?`,
            message: 'This user will be able to access the platform again.',
            type: 'info',
            confirmText: 'Unblock',
            cancelText: 'Cancel'
        });
        if (!confirmed) return;

        try {
            const result = await userApi.updateStatus(user.userId, false, null);
            if (result.result) {
                toast.success('User unblocked successfully');
                fetchUsers();
            } else {
                toast.error(result.message || 'Failed to unblock user');
            }
        } catch (error) {
            toast.error('Network error. Please try again.');
        }
    };

    const handleBlockConfirm = async () => {
        const { user, reason } = blockReasonModal;
        setBlockReasonModal({ open: false, user: null, reason: '' });

        try {
            const result = await userApi.updateStatus(user.userId, true, reason || null);
            if (result.result) {
                toast.success('User blocked successfully');
                fetchUsers();
            } else {
                toast.error(result.message || 'Failed to block user');
            }
        } catch (error) {
            toast.error('Network error. Please try again.');
        }
    };

    const handleDelete = async (user) => {
        const confirmed = await confirm({
            title: `Delete ${user.fullName}?`,
            message: 'This will deactivate the account. The user can be restored later.',
            type: 'delete',
            confirmText: 'Delete',
            cancelText: 'Cancel'
        });
        if (!confirmed) return;

        try {
            const result = await userApi.deleteUser(user.userId);
            if (result.result) {
                toast.success('User deleted successfully');
                fetchUsers();
            } else {
                toast.error(result.message || 'Failed to delete user');
            }
        } catch (error) {
            toast.error('Network error. Please try again.');
        }
    };

    const handleRestore = async (user) => {
        const confirmed = await confirm({
            title: `Restore ${user.fullName}?`,
            message: 'This will reactivate the user account.',
            type: 'info',
            confirmText: 'Restore',
            cancelText: 'Cancel'
        });
        if (!confirmed) return;

        try {
            const result = await userApi.restoreUser(user.userId);
            if (result.result) {
                toast.success('User restored successfully');
                fetchUsers();
            } else {
                toast.error(result.message || 'Failed to restore user');
            }
        } catch (error) {
            toast.error('Network error. Please try again.');
        }
    };

    const handleViewDetail = async (userId) => {
        try {
            const result = await userApi.getById(userId);
            if (result.result && result.data) {
                setSelectedUser(result.data);
                setShowDetailDrawer(true);
            } else {
                toast.error('Failed to load user details');
            }
        } catch (error) {
            toast.error('Network error. Please try again.');
        }
    };

    const handleExport = async () => {
        try {
            const params = {};
            if (filters.searchTerm) params.SearchTerm = filters.searchTerm;
            if (filters.isActive !== null) params.IsActive = filters.isActive;
            if (filters.isBlocked !== null) params.IsBlocked = filters.isBlocked;
            if (filters.stateId) params.StateId = filters.stateId;
            if (filters.cityId) params.CityId = filters.cityId;

            const response = await userApi.exportUsers(params);

            if (!response.ok) {
                toast.error('Failed to export users');
                return;
            }

            const blob = await response.blob();
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `users_export_${new Date().toISOString().split('T')[0]}.csv`;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            window.URL.revokeObjectURL(url);

            toast.success('User list exported successfully');
        } catch (error) {
            toast.error('Failed to export users');
        }
    };

    const handleFilterChange = (key, value) => {
        setFilters(prev => ({ ...prev, [key]: value, pageNumber: 1 }));
    };

    const handlePageChange = (page) => {
        setFilters(prev => ({ ...prev, pageNumber: page }));
    };

    const handleSort = (column) => {
        setFilters(prev => ({
            ...prev,
            sortBy: column,
            sortOrder: prev.sortBy === column && prev.sortOrder === 'ASC' ? 'DESC' : 'ASC',
            pageNumber: 1
        }));
    };

    const clearFilters = () => {
        setSearchInput('');
        setFilters({
            searchTerm: '',
            isActive: null,
            isBlocked: null,
            isDeleted: null,
            stateId: null,
            cityId: null,
            pageNumber: 1,
            pageSize: 15,
            sortBy: 'CreatedDate',
            sortOrder: 'DESC'
        });
        setShowDeletedUsers(false);
    };

    const hasActiveFilters = filters.isActive !== null || filters.isBlocked !== null ||
        filters.stateId || filters.cityId || showDeletedUsers;

    const formatLastLogin = (lastLogin) => {
        if (!lastLogin) return 'Never';
        return formatDistanceToNow(new Date(lastLogin), { addSuffix: true });
    };

    const getStatusBadge = (user) => {
        if (user.isDeleted) {
            return <span className="inline-flex px-2 py-1 text-xs font-semibold rounded-full bg-red-100 text-red-800">Deleted</span>;
        }
        if (user.isBlocked) {
            return <span className="inline-flex px-2 py-1 text-xs font-semibold rounded-full bg-orange-100 text-orange-800">Blocked</span>;
        }
        if (!user.isActive) {
            return <span className="inline-flex px-2 py-1 text-xs font-semibold rounded-full bg-gray-100 text-gray-800">Inactive</span>;
        }
        return <span className="inline-flex px-2 py-1 text-xs font-semibold rounded-full bg-green-100 text-green-800">Active</span>;
    };

    const SortHeader = ({ column, label }) => (
        <th
            onClick={() => handleSort(column)}
            className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider cursor-pointer hover:bg-gray-100 select-none"
        >
            <div className="flex items-center gap-1">
                {label}
                {filters.sortBy === column && (
                    <span className="text-indigo-600">{filters.sortOrder === 'ASC' ? '\u2191' : '\u2193'}</span>
                )}
            </div>
        </th>
    );

    return (
        <ProtectedRoute requireSuperAdmin={true}>
            <AdminLayout>
                <div className="p-6">
                    {/* Header */}
                    <div className="mb-6">
                        <div className="flex items-center justify-between">
                            <div>
                                <h1 className="text-2xl font-bold text-gray-900 flex items-center gap-2">
                                    <Users className="w-7 h-7 text-indigo-600" />
                                    User Management
                                </h1>
                                <p className="text-gray-600 mt-1">
                                    Manage registered customer accounts
                                </p>
                            </div>
                            <div className="flex gap-3">
                                <button
                                    onClick={() => setShowFilters(!showFilters)}
                                    className={`flex items-center gap-2 px-4 py-2 border rounded-lg transition-colors ${
                                        hasActiveFilters
                                            ? 'bg-indigo-50 border-indigo-300 text-indigo-700'
                                            : 'bg-white border-gray-300 text-gray-700 hover:bg-gray-50'
                                    }`}
                                >
                                    <Filter className="w-4 h-4" />
                                    Filters
                                    {hasActiveFilters && (
                                        <span className="bg-indigo-600 text-white text-xs rounded-full w-5 h-5 flex items-center justify-center">
                                            !
                                        </span>
                                    )}
                                </button>
                                <button
                                    onClick={handleExport}
                                    className="flex items-center gap-2 px-4 py-2 text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
                                >
                                    <Download className="w-4 h-4" />
                                    Export CSV
                                </button>
                                <button
                                    onClick={fetchUsers}
                                    className="flex items-center gap-2 px-4 py-2 text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
                                >
                                    <RefreshCw className="w-4 h-4" />
                                    Refresh
                                </button>
                            </div>
                        </div>

                        {/* Stats Cards */}
                        <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mt-6">
                            <div className="bg-white p-4 rounded-xl border border-gray-200 shadow-sm">
                                <div className="flex items-center justify-between">
                                    <div>
                                        <div className="text-sm text-gray-500">Total Users</div>
                                        <div className="text-2xl font-bold text-gray-900 mt-1">{stats.total}</div>
                                    </div>
                                    <div className="p-3 bg-indigo-50 rounded-lg">
                                        <Users className="w-6 h-6 text-indigo-600" />
                                    </div>
                                </div>
                            </div>
                            <div className="bg-white p-4 rounded-xl border border-green-200 shadow-sm">
                                <div className="flex items-center justify-between">
                                    <div>
                                        <div className="text-sm text-green-600">Active</div>
                                        <div className="text-2xl font-bold text-green-700 mt-1">{stats.active}</div>
                                    </div>
                                    <div className="p-3 bg-green-50 rounded-lg">
                                        <UserCheck className="w-6 h-6 text-green-600" />
                                    </div>
                                </div>
                            </div>
                            <div className="bg-white p-4 rounded-xl border border-orange-200 shadow-sm">
                                <div className="flex items-center justify-between">
                                    <div>
                                        <div className="text-sm text-orange-600">Blocked</div>
                                        <div className="text-2xl font-bold text-orange-700 mt-1">{stats.blocked}</div>
                                    </div>
                                    <div className="p-3 bg-orange-50 rounded-lg">
                                        <ShieldOff className="w-6 h-6 text-orange-600" />
                                    </div>
                                </div>
                            </div>
                            <div className="bg-white p-4 rounded-xl border border-gray-200 shadow-sm">
                                <div className="flex items-center justify-between">
                                    <div>
                                        <div className="text-sm text-gray-500">Inactive</div>
                                        <div className="text-2xl font-bold text-gray-700 mt-1">{stats.inactive}</div>
                                    </div>
                                    <div className="p-3 bg-gray-50 rounded-lg">
                                        <UserX className="w-6 h-6 text-gray-500" />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    {/* Search + Filters */}
                    <div className="bg-white rounded-xl border border-gray-200 shadow-sm mb-6">
                        {/* Search Bar */}
                        <div className="p-4 border-b border-gray-100">
                            <div className="relative">
                                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" />
                                <input
                                    type="text"
                                    placeholder="Search by name, phone, or email..."
                                    value={searchInput}
                                    onChange={(e) => setSearchInput(e.target.value)}
                                    className="w-full pl-11 pr-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent text-sm"
                                />
                                {searchInput && (
                                    <button
                                        onClick={() => setSearchInput('')}
                                        className="absolute right-3 top-1/2 transform -translate-y-1/2"
                                    >
                                        <X className="w-4 h-4 text-gray-400 hover:text-gray-600" />
                                    </button>
                                )}
                            </div>
                        </div>

                        {/* Advanced Filters */}
                        {showFilters && (
                            <div className="p-4 bg-gray-50 border-b border-gray-100">
                                <div className="grid grid-cols-1 md:grid-cols-5 gap-4">
                                    {/* Status Filter */}
                                    <div>
                                        <label className="block text-xs font-medium text-gray-600 mb-1.5">Status</label>
                                        <select
                                            value={filters.isActive === null ? '' : filters.isActive.toString()}
                                            onChange={(e) => handleFilterChange('isActive', e.target.value === '' ? null : e.target.value === 'true')}
                                            className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                                        >
                                            <option value="">All Status</option>
                                            <option value="true">Active</option>
                                            <option value="false">Inactive</option>
                                        </select>
                                    </div>

                                    {/* Blocked Filter */}
                                    <div>
                                        <label className="block text-xs font-medium text-gray-600 mb-1.5">Blocked</label>
                                        <select
                                            value={filters.isBlocked === null ? '' : filters.isBlocked.toString()}
                                            onChange={(e) => handleFilterChange('isBlocked', e.target.value === '' ? null : e.target.value === 'true')}
                                            className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                                        >
                                            <option value="">All</option>
                                            <option value="true">Blocked Only</option>
                                            <option value="false">Not Blocked</option>
                                        </select>
                                    </div>

                                    {/* State Filter */}
                                    <div>
                                        <label className="block text-xs font-medium text-gray-600 mb-1.5">State</label>
                                        <select
                                            value={filters.stateId || ''}
                                            onChange={(e) => {
                                                const val = e.target.value ? parseInt(e.target.value) : null;
                                                handleFilterChange('stateId', val);
                                                handleFilterChange('cityId', null);
                                            }}
                                            className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                                        >
                                            <option value="">All States</option>
                                            {states.map((s) => (
                                                <option key={s.stateId || s.c_stateid} value={s.stateId || s.c_stateid}>
                                                    {s.stateName || s.c_statename}
                                                </option>
                                            ))}
                                        </select>
                                    </div>

                                    {/* City Filter */}
                                    <div>
                                        <label className="block text-xs font-medium text-gray-600 mb-1.5">City</label>
                                        <select
                                            value={filters.cityId || ''}
                                            onChange={(e) => handleFilterChange('cityId', e.target.value ? parseInt(e.target.value) : null)}
                                            disabled={!filters.stateId}
                                            className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500 focus:border-transparent disabled:bg-gray-100 disabled:cursor-not-allowed"
                                        >
                                            <option value="">All Cities</option>
                                            {cities.map((c) => (
                                                <option key={c.cityId || c.c_cityid} value={c.cityId || c.c_cityid}>
                                                    {c.cityName || c.c_cityname}
                                                </option>
                                            ))}
                                        </select>
                                    </div>

                                    {/* Show Deleted Toggle */}
                                    <div>
                                        <label className="block text-xs font-medium text-gray-600 mb-1.5">Deleted Users</label>
                                        <button
                                            onClick={() => setShowDeletedUsers(!showDeletedUsers)}
                                            className={`w-full px-3 py-2 border rounded-lg text-sm transition-colors ${
                                                showDeletedUsers
                                                    ? 'bg-red-50 border-red-300 text-red-700'
                                                    : 'bg-white border-gray-300 text-gray-700 hover:bg-gray-50'
                                            }`}
                                        >
                                            {showDeletedUsers ? 'Showing Deleted' : 'Show Deleted'}
                                        </button>
                                    </div>
                                </div>

                                {hasActiveFilters && (
                                    <div className="mt-3 flex justify-end">
                                        <button
                                            onClick={clearFilters}
                                            className="flex items-center gap-1 text-sm text-gray-500 hover:text-gray-700"
                                        >
                                            <X className="w-3.5 h-3.5" />
                                            Clear all filters
                                        </button>
                                    </div>
                                )}
                            </div>
                        )}
                    </div>

                    {/* Table */}
                    <div className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
                        <div className="overflow-x-auto">
                            <table className="min-w-full divide-y divide-gray-200">
                                <thead className="bg-gray-50">
                                    <tr>
                                        <SortHeader column="FullName" label="User" />
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Contact</th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Location</th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
                                        <SortHeader column="TotalOrders" label="Orders" />
                                        <SortHeader column="TotalSpent" label="Spent" />
                                        <SortHeader column="CreatedDate" label="Registered" />
                                        <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
                                    </tr>
                                </thead>
                                <tbody className="bg-white divide-y divide-gray-200">
                                    {loading ? (
                                        <tr>
                                            <td colSpan="8" className="px-6 py-16 text-center">
                                                <div className="flex flex-col items-center gap-3">
                                                    <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600"></div>
                                                    <span className="text-sm text-gray-500">Loading users...</span>
                                                </div>
                                            </td>
                                        </tr>
                                    ) : users.length === 0 ? (
                                        <tr>
                                            <td colSpan="8" className="px-6 py-16 text-center">
                                                <div className="flex flex-col items-center gap-2">
                                                    <Users className="w-12 h-12 text-gray-300" />
                                                    <span className="text-gray-500 font-medium">No users found</span>
                                                    <span className="text-sm text-gray-400">Try adjusting your filters</span>
                                                </div>
                                            </td>
                                        </tr>
                                    ) : (
                                        users.map((user) => (
                                            <tr key={user.userId} className={`hover:bg-gray-50 transition-colors ${user.isDeleted ? 'bg-red-50/30' : ''}`}>
                                                {/* User Name */}
                                                <td className="px-6 py-4 whitespace-nowrap">
                                                    <div className="flex items-center gap-3">
                                                        <div className="w-9 h-9 rounded-full bg-indigo-100 flex items-center justify-center flex-shrink-0">
                                                            <span className="text-sm font-semibold text-indigo-600">
                                                                {user.fullName?.charAt(0)?.toUpperCase() || '?'}
                                                            </span>
                                                        </div>
                                                        <div>
                                                            <div className="text-sm font-medium text-gray-900">{user.fullName}</div>
                                                            <div className="text-xs text-gray-400">ID: {user.userId}</div>
                                                        </div>
                                                    </div>
                                                </td>

                                                {/* Contact */}
                                                <td className="px-6 py-4 whitespace-nowrap">
                                                    <div className="text-sm text-gray-900">{user.phone}</div>
                                                    <div className="text-xs text-gray-500">{user.email || '-'}</div>
                                                </td>

                                                {/* Location */}
                                                <td className="px-6 py-4 whitespace-nowrap">
                                                    {user.cityName || user.stateName ? (
                                                        <div className="flex items-center gap-1 text-sm text-gray-600">
                                                            <MapPin className="w-3.5 h-3.5 text-gray-400" />
                                                            <span>{[user.cityName, user.stateName].filter(Boolean).join(', ')}</span>
                                                        </div>
                                                    ) : (
                                                        <span className="text-sm text-gray-400">-</span>
                                                    )}
                                                </td>

                                                {/* Status */}
                                                <td className="px-6 py-4 whitespace-nowrap">
                                                    {getStatusBadge(user)}
                                                </td>

                                                {/* Orders */}
                                                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-700 font-medium">
                                                    {user.totalOrders}
                                                </td>

                                                {/* Spent */}
                                                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-700">
                                                    <span className="font-medium">{'\u20B9'}{user.totalSpent?.toLocaleString('en-IN', { minimumFractionDigits: 0 })}</span>
                                                </td>

                                                {/* Registered */}
                                                <td className="px-6 py-4 whitespace-nowrap">
                                                    <div className="text-sm text-gray-700">
                                                        {new Date(user.createdDate).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' })}
                                                    </div>
                                                    <div className="text-xs text-gray-400">
                                                        Last: {formatLastLogin(user.lastLogin)}
                                                    </div>
                                                </td>

                                                {/* Actions */}
                                                <td className="px-6 py-4 whitespace-nowrap text-right">
                                                    <div className="flex items-center justify-end gap-1">
                                                        <button
                                                            onClick={() => handleViewDetail(user.userId)}
                                                            className="p-1.5 text-gray-500 hover:text-indigo-600 hover:bg-indigo-50 rounded-lg transition-colors"
                                                            title="View Details"
                                                        >
                                                            <Eye className="w-4 h-4" />
                                                        </button>

                                                        {user.isDeleted ? (
                                                            <button
                                                                onClick={() => handleRestore(user)}
                                                                className="p-1.5 text-green-500 hover:text-green-700 hover:bg-green-50 rounded-lg transition-colors"
                                                                title="Restore User"
                                                            >
                                                                <RotateCcw className="w-4 h-4" />
                                                            </button>
                                                        ) : (
                                                            <>
                                                                <button
                                                                    onClick={() => handleBlockUnblock(user)}
                                                                    className={`p-1.5 rounded-lg transition-colors ${
                                                                        user.isBlocked
                                                                            ? 'text-green-500 hover:text-green-700 hover:bg-green-50'
                                                                            : 'text-orange-500 hover:text-orange-700 hover:bg-orange-50'
                                                                    }`}
                                                                    title={user.isBlocked ? 'Unblock User' : 'Block User'}
                                                                >
                                                                    {user.isBlocked ? <ShieldCheck className="w-4 h-4" /> : <ShieldOff className="w-4 h-4" />}
                                                                </button>
                                                                <button
                                                                    onClick={() => handleDelete(user)}
                                                                    className="p-1.5 text-red-500 hover:text-red-700 hover:bg-red-50 rounded-lg transition-colors"
                                                                    title="Delete User"
                                                                >
                                                                    <Trash2 className="w-4 h-4" />
                                                                </button>
                                                            </>
                                                        )}
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
                                <div className="hidden sm:flex sm:items-center">
                                    <p className="text-sm text-gray-700">
                                        Showing{' '}
                                        <span className="font-medium">
                                            {(pagination.currentPage - 1) * filters.pageSize + 1}
                                        </span>{' '}
                                        to{' '}
                                        <span className="font-medium">
                                            {Math.min(pagination.currentPage * filters.pageSize, pagination.totalRecords)}
                                        </span>{' '}
                                        of <span className="font-medium">{pagination.totalRecords}</span> users
                                    </p>
                                </div>
                                <div className="flex items-center gap-2">
                                    <button
                                        onClick={() => handlePageChange(pagination.currentPage - 1)}
                                        disabled={pagination.currentPage === 1}
                                        className="p-2 border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                                    >
                                        <ChevronLeft className="w-4 h-4" />
                                    </button>
                                    {[...Array(Math.min(pagination.totalPages, 7))].map((_, index) => {
                                        let pageNum;
                                        if (pagination.totalPages <= 7) {
                                            pageNum = index + 1;
                                        } else if (pagination.currentPage <= 4) {
                                            pageNum = index + 1;
                                        } else if (pagination.currentPage >= pagination.totalPages - 3) {
                                            pageNum = pagination.totalPages - 6 + index;
                                        } else {
                                            pageNum = pagination.currentPage - 3 + index;
                                        }

                                        return (
                                            <button
                                                key={pageNum}
                                                onClick={() => handlePageChange(pageNum)}
                                                className={`w-9 h-9 rounded-lg text-sm font-medium transition-colors ${
                                                    pagination.currentPage === pageNum
                                                        ? 'bg-indigo-600 text-white shadow-sm'
                                                        : 'text-gray-700 hover:bg-gray-50 border border-gray-300'
                                                }`}
                                            >
                                                {pageNum}
                                            </button>
                                        );
                                    })}
                                    <button
                                        onClick={() => handlePageChange(pagination.currentPage + 1)}
                                        disabled={pagination.currentPage === pagination.totalPages}
                                        className="p-2 border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                                    >
                                        <ChevronRight className="w-4 h-4" />
                                    </button>
                                </div>
                            </div>
                        )}
                    </div>
                </div>

                {/* User Detail Drawer */}
                {showDetailDrawer && selectedUser && (
                    <UserDetailDrawer
                        user={selectedUser}
                        onClose={() => {
                            setShowDetailDrawer(false);
                            setSelectedUser(null);
                        }}
                        onBlockUnblock={() => {
                            setShowDetailDrawer(false);
                            handleBlockUnblock(selectedUser);
                        }}
                        onDelete={() => {
                            setShowDetailDrawer(false);
                            handleDelete(selectedUser);
                        }}
                        onRestore={() => {
                            setShowDetailDrawer(false);
                            handleRestore(selectedUser);
                        }}
                    />
                )}

                {/* Block Reason Modal */}
                {blockReasonModal.open && (
                    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
                        <div
                            className="fixed inset-0 bg-black/50 backdrop-blur-sm"
                            onClick={() => setBlockReasonModal({ open: false, user: null, reason: '' })}
                        />
                        <div className="relative bg-white rounded-xl shadow-2xl w-full max-w-md transform transition-all">
                            <div className="p-6">
                                <div className="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-orange-100 mb-4">
                                    <ShieldOff className="w-6 h-6 text-orange-600" />
                                </div>
                                <h3 className="text-lg font-semibold text-gray-900 text-center">
                                    Block {blockReasonModal.user?.fullName}?
                                </h3>
                                <p className="text-sm text-gray-500 text-center mt-1">
                                    This user will be unable to access the platform.
                                </p>
                                <div className="mt-4">
                                    <label className="block text-sm font-medium text-gray-700 mb-1.5">
                                        Reason for blocking (optional)
                                    </label>
                                    <textarea
                                        value={blockReasonModal.reason}
                                        onChange={(e) => setBlockReasonModal(prev => ({ ...prev, reason: e.target.value }))}
                                        placeholder="Enter reason..."
                                        rows={3}
                                        className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-orange-500 focus:border-transparent resize-none"
                                    />
                                </div>
                            </div>
                            <div className="bg-gray-50 px-6 py-4 flex flex-col sm:flex-row-reverse gap-3 rounded-b-xl">
                                <button
                                    onClick={handleBlockConfirm}
                                    className="w-full sm:w-auto px-4 py-2 bg-orange-500 text-white rounded-lg hover:bg-orange-600 text-sm font-medium transition-colors"
                                >
                                    Block User
                                </button>
                                <button
                                    onClick={() => setBlockReasonModal({ open: false, user: null, reason: '' })}
                                    className="w-full sm:w-auto px-4 py-2 bg-white border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 text-sm font-medium transition-colors"
                                >
                                    Cancel
                                </button>
                            </div>
                        </div>
                    </div>
                )}
            </AdminLayout>
        </ProtectedRoute>
    );
};

export default AdminCustomerUsers;
