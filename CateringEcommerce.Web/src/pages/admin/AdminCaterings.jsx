import { useState, useEffect, useCallback } from 'react';
import {
    Search, RefreshCw, Download, Trash2, RotateCcw,
    Eye, ChevronLeft, ChevronRight, Filter, X,
    MapPin, CheckCircle, XCircle, Ban, ShieldCheck,
    ShieldOff, Star, Store, TrendingUp, Clock,
    AlertTriangle, FileSearch
} from 'lucide-react';
import AdminLayout from '../../components/admin/layout/AdminLayout';
import { cateringApi, locationApi } from '../../services/adminApi';
import { useConfirmation } from '../../contexts/ConfirmationContext';
import { toast } from 'react-hot-toast';

// Status enum mapping (matches ApprovalStatus C# enum)
const STATUS_MAP = {
    1: { label: 'Pending', color: 'bg-yellow-100 text-yellow-800', icon: Clock },
    2: { label: 'Approved', color: 'bg-green-100 text-green-800', icon: CheckCircle },
    3: { label: 'Rejected', color: 'bg-red-100 text-red-800', icon: XCircle },
    4: { label: 'Under Review', color: 'bg-blue-100 text-blue-800', icon: FileSearch },
    5: { label: 'Info Requested', color: 'bg-purple-100 text-purple-800', icon: AlertTriangle },
};

const getStatusBadge = (status, isBlocked, isDeleted) => {
    if (isDeleted) return { label: 'Deleted', color: 'bg-gray-100 text-gray-800', icon: Trash2 };
    if (isBlocked) return { label: 'Blocked', color: 'bg-orange-100 text-orange-800', icon: Ban };
    return STATUS_MAP[status] || { label: 'Unknown', color: 'bg-gray-100 text-gray-500', icon: Clock };
};

const AdminCaterings = () => {
    const confirm = useConfirmation();
    const [caterings, setCaterings] = useState([]);
    const [loading, setLoading] = useState(true);
    const [showFilters, setShowFilters] = useState(false);
    const [showDeletedCaterings, setShowDeletedCaterings] = useState(false);

    // Location data
    const [states, setStates] = useState([]);
    const [cities, setCities] = useState([]);

    // Stats
    const [stats, setStats] = useState({ total: 0, approved: 0, pending: 0, blocked: 0 });

    // Status reason modal
    const [reasonModal, setReasonModal] = useState({ open: false, catering: null, action: null, reason: '' });

    // Search & Filters
    const [searchTerm, setSearchTerm] = useState('');
    const [filters, setFilters] = useState({
        status: '',
        stateId: '',
        cityId: '',
        verificationStatus: '',
        isActive: '',
    });

    // Pagination & Sorting
    const [pagination, setPagination] = useState({
        pageNumber: 1,
        pageSize: 20,
        totalRecords: 0,
        totalPages: 0,
    });
    const [sortBy, setSortBy] = useState('CreatedDate');
    const [sortOrder, setSortOrder] = useState('DESC');

    // Debounced search
    const [debouncedSearch, setDebouncedSearch] = useState('');
    useEffect(() => {
        const timer = setTimeout(() => {
            setDebouncedSearch(searchTerm);
            setPagination(prev => ({ ...prev, pageNumber: 1 }));
        }, 400);
        return () => clearTimeout(timer);
    }, [searchTerm]);

    // Fetch caterings
    const fetchCaterings = useCallback(async () => {
        setLoading(true);
        try {
            const params = {
                pageNumber: pagination.pageNumber,
                pageSize: pagination.pageSize,
                sortBy,
                sortOrder,
            };
            if (debouncedSearch) params.searchTerm = debouncedSearch;
            if (filters.status) params.status = filters.status;
            if (filters.stateId) params.stateId = filters.stateId;
            if (filters.cityId) params.cityId = filters.cityId;
            if (filters.verificationStatus) params.verificationStatus = filters.verificationStatus;
            if (filters.isActive) params.isActive = filters.isActive === 'true';
            if (showDeletedCaterings) params.isDeleted = true;

            const response = await cateringApi.getAll(params);
            if (response.result) {
                const data = response.data;
                setCaterings(data.caterings || []);
                setPagination(prev => ({
                    ...prev,
                    totalRecords: data.totalRecords,
                    totalPages: data.totalPages,
                }));

                const list = data.caterings || [];
                setStats({
                    total: data.totalRecords,
                    approved: list.filter(c => c.status === 2 && !c.isBlocked).length,
                    pending: list.filter(c => c.status === 1).length,
                    blocked: list.filter(c => c.isBlocked).length,
                });
            }
        } catch (error) {
            console.error('Error loading caterings:', error);
            toast.error('Failed to load caterings');
        } finally {
            setLoading(false);
        }
    }, [pagination.pageNumber, pagination.pageSize, debouncedSearch, filters, sortBy, sortOrder, showDeletedCaterings]);

    useEffect(() => { fetchCaterings(); }, [fetchCaterings]);

    // Load states on mount
    useEffect(() => {
        const loadStates = async () => {
            try {
                const result = await locationApi.getStates();
                if (result.result) setStates(result.data || []);
            } catch { /* ignore */ }
        };
        loadStates();
    }, []);

    // Load cities when state changes
    useEffect(() => {
        if (!filters.stateId) { setCities([]); return; }
        const loadCities = async () => {
            try {
                const result = await locationApi.getCities(filters.stateId);
                if (result.result) setCities(result.data || []);
            } catch { /* ignore */ }
        };
        loadCities();
    }, [filters.stateId]);

    // === ACTIONS ===

    const handleApprove = async (catering) => {
        const confirmed = await confirm({
            title: `Approve ${catering.businessName}?`,
            message: 'This catering partner will be activated and visible to customers.',
            type: 'info',
            confirmText: 'Approve',
            cancelText: 'Cancel'
        });
        if (!confirmed) return;

        try {
            const result = await cateringApi.updateStatus(catering.cateringId, 2, null);
            if (result.result) {
                toast.success('Catering approved successfully');
                fetchCaterings();
            } else {
                toast.error(result.message || 'Failed to approve catering');
            }
        } catch {
            toast.error('Network error. Please try again.');
        }
    };

    const handleReject = (catering) => {
        setReasonModal({ open: true, catering, action: 'reject', reason: '' });
    };

    const handleBlock = (catering) => {
        setReasonModal({ open: true, catering, action: 'block', reason: '' });
    };

    const handleUnblock = async (catering) => {
        const confirmed = await confirm({
            title: `Unblock ${catering.businessName}?`,
            message: 'This catering partner will be unblocked and active again.',
            type: 'info',
            confirmText: 'Unblock',
            cancelText: 'Cancel'
        });
        if (!confirmed) return;

        try {
            const result = await cateringApi.updateStatus(catering.cateringId, 2, null);
            if (result.result) {
                toast.success('Catering unblocked successfully');
                fetchCaterings();
            } else {
                toast.error(result.message || 'Failed to unblock catering');
            }
        } catch {
            toast.error('Network error. Please try again.');
        }
    };

    const handleReasonConfirm = async () => {
        const { catering, action, reason } = reasonModal;
        setReasonModal({ open: false, catering: null, action: null, reason: '' });

        const statusCode = 3; // Rejected status
        try {
            const result = await cateringApi.updateStatus(catering.cateringId, statusCode, reason || null);
            if (result.result) {
                toast.success(`Catering ${action === 'reject' ? 'rejected' : 'blocked'} successfully`);
                fetchCaterings();
            } else {
                toast.error(result.message || `Failed to ${action} catering`);
            }
        } catch {
            toast.error('Network error. Please try again.');
        }
    };

    const handleDelete = async (catering) => {
        const confirmed = await confirm({
            title: `Delete ${catering.businessName}?`,
            message: 'This will deactivate the catering. It can be restored later.',
            type: 'delete',
            confirmText: 'Delete',
            cancelText: 'Cancel'
        });
        if (!confirmed) return;

        try {
            const result = await cateringApi.delete(catering.cateringId);
            if (result.result) {
                toast.success('Catering deleted successfully');
                fetchCaterings();
            } else {
                toast.error(result.message || 'Failed to delete catering');
            }
        } catch {
            toast.error('Network error. Please try again.');
        }
    };

    const handleRestore = async (catering) => {
        const confirmed = await confirm({
            title: `Restore ${catering.businessName}?`,
            message: 'This will reactivate the catering partner.',
            type: 'info',
            confirmText: 'Restore',
            cancelText: 'Cancel'
        });
        if (!confirmed) return;

        try {
            const result = await cateringApi.restore(catering.cateringId);
            if (result.result) {
                toast.success('Catering restored successfully');
                fetchCaterings();
            } else {
                toast.error(result.message || 'Failed to restore catering');
            }
        } catch {
            toast.error('Network error. Please try again.');
        }
    };

    const handleExport = async () => {
        try {
            const params = {};
            if (debouncedSearch) params.searchTerm = debouncedSearch;
            if (filters.status) params.status = filters.status;
            if (filters.stateId) params.stateId = filters.stateId;
            if (filters.cityId) params.cityId = filters.cityId;
            if (showDeletedCaterings) params.isDeleted = true;

            const response = await cateringApi.exportCaterings(params);
            if (!response.ok) throw new Error('Export failed');

            const blob = await response.blob();
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `caterings_export_${new Date().toISOString().slice(0, 10)}.csv`;
            document.body.appendChild(a);
            a.click();
            a.remove();
            window.URL.revokeObjectURL(url);
            toast.success('Export downloaded successfully');
        } catch {
            toast.error('Failed to export caterings');
        }
    };

    // Sorting
    const handleSort = (column) => {
        if (sortBy === column) {
            setSortOrder(prev => prev === 'ASC' ? 'DESC' : 'ASC');
        } else {
            setSortBy(column);
            setSortOrder('DESC');
        }
    };

    const SortIcon = ({ column }) => {
        if (sortBy !== column) return <span className="text-gray-300 ml-1">&#8597;</span>;
        return <span className="ml-1 text-indigo-600">{sortOrder === 'ASC' ? '\u25B2' : '\u25BC'}</span>;
    };

    const activeFilterCount = Object.values(filters).filter(v => v !== '').length + (showDeletedCaterings ? 1 : 0);

    const clearFilters = () => {
        setFilters({ status: '', stateId: '', cityId: '', verificationStatus: '', isActive: '' });
        setShowDeletedCaterings(false);
        setSearchTerm('');
    };

    return (
        <AdminLayout>
            <div className="space-y-6">
                {/* Page Header */}
                <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
                    <div>
                        <h1 className="text-2xl font-bold text-gray-900 flex items-center gap-2">
                            <Store className="w-7 h-7 text-indigo-600" />
                            Caterings Management
                        </h1>
                        <p className="text-gray-500 mt-1 text-sm">Manage and monitor catering partners</p>
                    </div>
                    <div className="flex items-center gap-2">
                        <button
                            onClick={handleExport}
                            className="inline-flex items-center gap-2 px-4 py-2 bg-white border border-gray-300 rounded-lg text-sm font-medium text-gray-700 hover:bg-gray-50 transition-colors"
                        >
                            <Download className="w-4 h-4" />
                            Export CSV
                        </button>
                        <button
                            onClick={fetchCaterings}
                            className="inline-flex items-center gap-2 px-4 py-2 bg-white border border-gray-300 rounded-lg text-sm font-medium text-gray-700 hover:bg-gray-50 transition-colors"
                        >
                            <RefreshCw className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`} />
                            Refresh
                        </button>
                    </div>
                </div>

                {/* Stats Cards */}
                <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
                    {[
                        { label: 'Total Caterings', value: stats.total, icon: Store, color: 'text-indigo-600 bg-indigo-50' },
                        { label: 'Approved', value: stats.approved, icon: CheckCircle, color: 'text-green-600 bg-green-50' },
                        { label: 'Pending', value: stats.pending, icon: Clock, color: 'text-yellow-600 bg-yellow-50' },
                        { label: 'Blocked', value: stats.blocked, icon: Ban, color: 'text-orange-600 bg-orange-50' },
                    ].map(stat => (
                        <div key={stat.label} className="bg-white rounded-xl border border-gray-200 p-4 flex items-center gap-4">
                            <div className={`p-3 rounded-lg ${stat.color}`}>
                                <stat.icon className="w-5 h-5" />
                            </div>
                            <div>
                                <p className="text-2xl font-bold text-gray-900">{stat.value}</p>
                                <p className="text-xs text-gray-500">{stat.label}</p>
                            </div>
                        </div>
                    ))}
                </div>

                {/* Search & Filter Bar */}
                <div className="bg-white rounded-xl border border-gray-200 p-4">
                    <div className="flex flex-col sm:flex-row gap-3">
                        <div className="relative flex-1">
                            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                            <input
                                type="text"
                                placeholder="Search by business name, owner, phone, email..."
                                value={searchTerm}
                                onChange={(e) => setSearchTerm(e.target.value)}
                                className="w-full pl-10 pr-4 py-2.5 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                            />
                        </div>
                        <div className="flex gap-2">
                            <button
                                onClick={() => setShowFilters(!showFilters)}
                                className={`inline-flex items-center gap-2 px-4 py-2.5 border rounded-lg text-sm font-medium transition-colors ${showFilters ? 'bg-indigo-50 border-indigo-300 text-indigo-700' : 'bg-white border-gray-300 text-gray-700 hover:bg-gray-50'}`}
                            >
                                <Filter className="w-4 h-4" />
                                Filters
                                {activeFilterCount > 0 && (
                                    <span className="inline-flex items-center justify-center w-5 h-5 text-xs font-bold bg-indigo-600 text-white rounded-full">
                                        {activeFilterCount}
                                    </span>
                                )}
                            </button>
                            {activeFilterCount > 0 && (
                                <button onClick={clearFilters} className="inline-flex items-center gap-1 px-3 py-2.5 text-sm text-red-600 hover:bg-red-50 rounded-lg transition-colors">
                                    <X className="w-4 h-4" /> Clear
                                </button>
                            )}
                        </div>
                    </div>

                    {/* Advanced Filters */}
                    {showFilters && (
                        <div className="mt-4 pt-4 border-t border-gray-200 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-3">
                            <div>
                                <label className="block text-xs font-medium text-gray-600 mb-1">Status</label>
                                <select
                                    value={filters.status}
                                    onChange={(e) => setFilters(prev => ({ ...prev, status: e.target.value }))}
                                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                                >
                                    <option value="">All Statuses</option>
                                    <option value="1">Pending</option>
                                    <option value="2">Approved</option>
                                    <option value="3">Rejected</option>
                                    <option value="4">Under Review</option>
                                    <option value="5">Info Requested</option>
                                </select>
                            </div>
                            <div>
                                <label className="block text-xs font-medium text-gray-600 mb-1">State</label>
                                <select
                                    value={filters.stateId}
                                    onChange={(e) => setFilters(prev => ({ ...prev, stateId: e.target.value, cityId: '' }))}
                                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                                >
                                    <option value="">All States</option>
                                    {states.map(s => (
                                        <option key={s.stateId} value={s.stateId}>{s.stateName}</option>
                                    ))}
                                </select>
                            </div>
                            <div>
                                <label className="block text-xs font-medium text-gray-600 mb-1">City</label>
                                <select
                                    value={filters.cityId}
                                    onChange={(e) => setFilters(prev => ({ ...prev, cityId: e.target.value }))}
                                    disabled={!filters.stateId}
                                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500 focus:border-transparent disabled:bg-gray-100"
                                >
                                    <option value="">All Cities</option>
                                    {cities.map(c => (
                                        <option key={c.cityId} value={c.cityId}>{c.cityName}</option>
                                    ))}
                                </select>
                            </div>
                            <div>
                                <label className="block text-xs font-medium text-gray-600 mb-1">Verification</label>
                                <select
                                    value={filters.verificationStatus}
                                    onChange={(e) => setFilters(prev => ({ ...prev, verificationStatus: e.target.value }))}
                                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                                >
                                    <option value="">All</option>
                                    <option value="Verified">Verified</option>
                                    <option value="Unverified">Unverified</option>
                                </select>
                            </div>
                            <div>
                                <label className="block text-xs font-medium text-gray-600 mb-1">Active Status</label>
                                <select
                                    value={filters.isActive}
                                    onChange={(e) => setFilters(prev => ({ ...prev, isActive: e.target.value }))}
                                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                                >
                                    <option value="">All</option>
                                    <option value="true">Active</option>
                                    <option value="false">Inactive</option>
                                </select>
                            </div>
                            <div className="flex items-end sm:col-span-2 lg:col-span-5">
                                <label className="inline-flex items-center gap-2 cursor-pointer">
                                    <input
                                        type="checkbox"
                                        checked={showDeletedCaterings}
                                        onChange={(e) => setShowDeletedCaterings(e.target.checked)}
                                        className="w-4 h-4 text-indigo-600 rounded focus:ring-indigo-500"
                                    />
                                    <span className="text-sm text-gray-600">Show deleted caterings</span>
                                </label>
                            </div>
                        </div>
                    )}
                </div>

                {/* Table */}
                <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
                    {loading ? (
                        <div className="p-8 space-y-4">
                            {[...Array(5)].map((_, i) => (
                                <div key={i} className="animate-pulse flex items-center gap-4">
                                    <div className="h-10 w-10 bg-gray-200 rounded-lg" />
                                    <div className="flex-1 space-y-2">
                                        <div className="h-4 bg-gray-200 rounded w-1/3" />
                                        <div className="h-3 bg-gray-200 rounded w-1/4" />
                                    </div>
                                    <div className="h-6 w-20 bg-gray-200 rounded-full" />
                                </div>
                            ))}
                        </div>
                    ) : caterings.length === 0 ? (
                        <div className="p-12 text-center">
                            <Store className="w-12 h-12 text-gray-300 mx-auto mb-4" />
                            <h3 className="text-lg font-semibold text-gray-900">No caterings found</h3>
                            <p className="text-gray-500 mt-1 text-sm">No catering partners match your current filters</p>
                        </div>
                    ) : (
                        <>
                            <div className="overflow-x-auto">
                                <table className="min-w-full divide-y divide-gray-200">
                                    <thead className="bg-gray-50">
                                        <tr>
                                            <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider cursor-pointer hover:text-gray-900" onClick={() => handleSort('BusinessName')}>
                                                Business <SortIcon column="BusinessName" />
                                            </th>
                                            <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider">Owner</th>
                                            <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider">Location</th>
                                            <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider">Status</th>
                                            <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider cursor-pointer hover:text-gray-900" onClick={() => handleSort('Rating')}>
                                                Rating <SortIcon column="Rating" />
                                            </th>
                                            <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider cursor-pointer hover:text-gray-900" onClick={() => handleSort('TotalOrders')}>
                                                Orders <SortIcon column="TotalOrders" />
                                            </th>
                                            <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider cursor-pointer hover:text-gray-900" onClick={() => handleSort('TotalEarnings')}>
                                                Earnings <SortIcon column="TotalEarnings" />
                                            </th>
                                            <th className="px-4 py-3 text-right text-xs font-semibold text-gray-600 uppercase tracking-wider">Actions</th>
                                        </tr>
                                    </thead>
                                    <tbody className="divide-y divide-gray-100">
                                        {caterings.map((catering) => {
                                            const statusInfo = getStatusBadge(catering.status, catering.isBlocked, catering.isDeleted);
                                            const StatusIcon = statusInfo.icon;
                                            return (
                                                <tr key={catering.cateringId} className={`hover:bg-gray-50 transition-colors ${catering.isDeleted ? 'opacity-60' : ''}`}>
                                                    <td className="px-4 py-3">
                                                        <div className="flex items-center gap-3">
                                                            <div className="flex-shrink-0 w-9 h-9 bg-indigo-100 rounded-lg flex items-center justify-center">
                                                                <Store className="w-4 h-4 text-indigo-600" />
                                                            </div>
                                                            <div className="min-w-0">
                                                                <p className="text-sm font-medium text-gray-900 truncate">{catering.businessName}</p>
                                                                <p className="text-xs text-gray-500">{catering.phone}</p>
                                                            </div>
                                                        </div>
                                                    </td>
                                                    <td className="px-4 py-3">
                                                        <p className="text-sm text-gray-900">{catering.ownerName}</p>
                                                        <p className="text-xs text-gray-500">{catering.email}</p>
                                                    </td>
                                                    <td className="px-4 py-3">
                                                        <div className="flex items-center gap-1.5 text-sm text-gray-600">
                                                            <MapPin className="w-3.5 h-3.5 text-gray-400 flex-shrink-0" />
                                                            <span className="truncate">{catering.city}{catering.state ? `, ${catering.state}` : ''}</span>
                                                        </div>
                                                    </td>
                                                    <td className="px-4 py-3">
                                                        <span className={`inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-medium ${statusInfo.color}`}>
                                                            <StatusIcon className="w-3 h-3" />
                                                            {statusInfo.label}
                                                        </span>
                                                        {catering.isVerified && (
                                                            <span className="ml-1.5 inline-flex items-center gap-0.5 text-xs text-blue-600" title="Verified">
                                                                <ShieldCheck className="w-3.5 h-3.5" />
                                                            </span>
                                                        )}
                                                    </td>
                                                    <td className="px-4 py-3">
                                                        <div className="flex items-center gap-1">
                                                            <Star className="w-4 h-4 text-yellow-400 fill-yellow-400" />
                                                            <span className="text-sm font-medium">{catering.rating?.toFixed(1) || '0.0'}</span>
                                                            <span className="text-xs text-gray-400">({catering.totalReviews})</span>
                                                        </div>
                                                    </td>
                                                    <td className="px-4 py-3 text-sm text-gray-700 font-medium">{catering.totalOrders}</td>
                                                    <td className="px-4 py-3">
                                                        <div className="flex items-center gap-1 text-sm font-medium text-gray-900">
                                                            <TrendingUp className="w-3.5 h-3.5 text-green-500" />
                                                            {new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 0 }).format(catering.totalEarnings)}
                                                        </div>
                                                    </td>
                                                    <td className="px-4 py-3">
                                                        <div className="flex items-center justify-end gap-1">
                                                            <button
                                                                className="p-1.5 text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
                                                                title="View Details"
                                                            >
                                                                <Eye className="w-4 h-4" />
                                                            </button>

                                                            {catering.isDeleted ? (
                                                                <button
                                                                    onClick={() => handleRestore(catering)}
                                                                    className="p-1.5 text-emerald-600 hover:bg-emerald-50 rounded-lg transition-colors"
                                                                    title="Restore"
                                                                >
                                                                    <RotateCcw className="w-4 h-4" />
                                                                </button>
                                                            ) : (
                                                                <>
                                                                    {catering.status === 1 && (
                                                                        <>
                                                                            <button
                                                                                onClick={() => handleApprove(catering)}
                                                                                className="p-1.5 text-green-600 hover:bg-green-50 rounded-lg transition-colors"
                                                                                title="Approve"
                                                                            >
                                                                                <CheckCircle className="w-4 h-4" />
                                                                            </button>
                                                                            <button
                                                                                onClick={() => handleReject(catering)}
                                                                                className="p-1.5 text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                                                                                title="Reject"
                                                                            >
                                                                                <XCircle className="w-4 h-4" />
                                                                            </button>
                                                                        </>
                                                                    )}

                                                                    {catering.status === 2 && !catering.isBlocked && (
                                                                        <button
                                                                            onClick={() => handleBlock(catering)}
                                                                            className="p-1.5 text-orange-600 hover:bg-orange-50 rounded-lg transition-colors"
                                                                            title="Block"
                                                                        >
                                                                            <ShieldOff className="w-4 h-4" />
                                                                        </button>
                                                                    )}

                                                                    {catering.isBlocked && (
                                                                        <button
                                                                            onClick={() => handleUnblock(catering)}
                                                                            className="p-1.5 text-emerald-600 hover:bg-emerald-50 rounded-lg transition-colors"
                                                                            title="Unblock"
                                                                        >
                                                                            <ShieldCheck className="w-4 h-4" />
                                                                        </button>
                                                                    )}

                                                                    <button
                                                                        onClick={() => handleDelete(catering)}
                                                                        className="p-1.5 text-red-500 hover:bg-red-50 rounded-lg transition-colors"
                                                                        title="Delete"
                                                                    >
                                                                        <Trash2 className="w-4 h-4" />
                                                                    </button>
                                                                </>
                                                            )}
                                                        </div>
                                                    </td>
                                                </tr>
                                            );
                                        })}
                                    </tbody>
                                </table>
                            </div>

                            {/* Pagination */}
                            <div className="px-4 py-3 border-t border-gray-200 flex flex-col sm:flex-row items-center justify-between gap-3">
                                <p className="text-sm text-gray-600">
                                    Showing <span className="font-medium">{((pagination.pageNumber - 1) * pagination.pageSize) + 1}</span> to{' '}
                                    <span className="font-medium">{Math.min(pagination.pageNumber * pagination.pageSize, pagination.totalRecords)}</span> of{' '}
                                    <span className="font-medium">{pagination.totalRecords}</span> results
                                </p>
                                <div className="flex items-center gap-2">
                                    <button
                                        onClick={() => setPagination(prev => ({ ...prev, pageNumber: prev.pageNumber - 1 }))}
                                        disabled={pagination.pageNumber <= 1}
                                        className="inline-flex items-center gap-1 px-3 py-1.5 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                                    >
                                        <ChevronLeft className="w-4 h-4" /> Prev
                                    </button>
                                    <span className="text-sm text-gray-600">
                                        Page {pagination.pageNumber} of {pagination.totalPages || 1}
                                    </span>
                                    <button
                                        onClick={() => setPagination(prev => ({ ...prev, pageNumber: prev.pageNumber + 1 }))}
                                        disabled={pagination.pageNumber >= pagination.totalPages}
                                        className="inline-flex items-center gap-1 px-3 py-1.5 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                                    >
                                        Next <ChevronRight className="w-4 h-4" />
                                    </button>
                                </div>
                            </div>
                        </>
                    )}
                </div>
            </div>

            {/* Reason Modal (for Reject / Block) */}
            {reasonModal.open && (
                <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
                    <div
                        className="fixed inset-0 bg-black/50 backdrop-blur-sm"
                        onClick={() => setReasonModal({ open: false, catering: null, action: null, reason: '' })}
                    />
                    <div className="relative bg-white rounded-xl shadow-2xl w-full max-w-md transform transition-all">
                        <div className="p-6">
                            <div className={`mx-auto flex items-center justify-center h-12 w-12 rounded-full mb-4 ${reasonModal.action === 'reject' ? 'bg-red-100' : 'bg-orange-100'}`}>
                                {reasonModal.action === 'reject' ? (
                                    <XCircle className="w-6 h-6 text-red-600" />
                                ) : (
                                    <ShieldOff className="w-6 h-6 text-orange-600" />
                                )}
                            </div>
                            <h3 className="text-lg font-semibold text-gray-900 text-center">
                                {reasonModal.action === 'reject' ? 'Reject' : 'Block'} {reasonModal.catering?.businessName}?
                            </h3>
                            <p className="text-sm text-gray-500 text-center mt-1">
                                {reasonModal.action === 'reject'
                                    ? 'This catering partner application will be rejected.'
                                    : 'This catering partner will be blocked from the platform.'}
                            </p>
                            <div className="mt-4">
                                <label className="block text-sm font-medium text-gray-700 mb-1.5">
                                    Reason (optional)
                                </label>
                                <textarea
                                    value={reasonModal.reason}
                                    onChange={(e) => setReasonModal(prev => ({ ...prev, reason: e.target.value }))}
                                    placeholder="Enter reason..."
                                    rows={3}
                                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500 focus:border-transparent resize-none"
                                />
                            </div>
                        </div>
                        <div className="bg-gray-50 px-6 py-4 flex flex-col sm:flex-row-reverse gap-3 rounded-b-xl">
                            <button
                                onClick={handleReasonConfirm}
                                className={`w-full sm:w-auto px-4 py-2 text-white rounded-lg text-sm font-medium transition-colors ${reasonModal.action === 'reject' ? 'bg-red-600 hover:bg-red-700' : 'bg-orange-500 hover:bg-orange-600'}`}
                            >
                                {reasonModal.action === 'reject' ? 'Reject Catering' : 'Block Catering'}
                            </button>
                            <button
                                onClick={() => setReasonModal({ open: false, catering: null, action: null, reason: '' })}
                                className="w-full sm:w-auto px-4 py-2 bg-white border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 text-sm font-medium transition-colors"
                            >
                                Cancel
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </AdminLayout>
    );
};

export default AdminCaterings;
