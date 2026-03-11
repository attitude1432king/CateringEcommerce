import { useState, useEffect, useCallback } from 'react';
import {
    Search, RefreshCw, Download, Trash2, RotateCcw,
    ChevronLeft, ChevronRight, Filter, X,
    CheckCircle, XCircle, Ban, Star,
    Users, UserCheck, MapPin, ShieldOff, ShieldCheck, Calendar
} from 'lucide-react';
import AdminLayout from '../../components/admin/layout/AdminLayout';
import { supervisorManagementApi } from '../../services/adminApi';
import { useConfirmation } from '../../contexts/ConfirmationContext';
import { toast } from 'react-hot-toast';

// Centralized status enum (matches SupervisorApprovalStatus C# enum: 0-4)
const SUPERVISOR_STATUS = Object.freeze({
    PENDING: 0,
    APPROVED: 1,
    REJECTED: 2,
    UNDER_REVIEW: 3,
    INFO_REQUESTED: 4,
});

const ACTIVE_STATUS_MAP = Object.freeze({
    ACTIVE: { label: 'Active', color: 'bg-green-100 text-green-800', icon: CheckCircle },
    SUSPENDED: { label: 'Blocked', color: 'bg-orange-100 text-orange-800', icon: Ban },
    DEACTIVATED: { label: 'Deactivated', color: 'bg-gray-100 text-gray-800', icon: XCircle },
});

const TYPE_BADGE = Object.freeze({
    CAREER: { label: 'Career', color: 'bg-blue-100 text-blue-700' },
    REGISTERED: { label: 'Registered', color: 'bg-teal-100 text-teal-700' },
});

const ApprovedSupervisors = () => {
    const confirm = useConfirmation();
    const [data, setData] = useState([]);
    const [loading, setLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState('');
    const [debouncedSearch, setDebouncedSearch] = useState('');
    const [showFilters, setShowFilters] = useState(false);
    const [showDeleted, setShowDeleted] = useState(false);
    const [filters, setFilters] = useState({ supervisorType: '', city: '', state: '', isBlocked: '', dateFrom: '', dateTo: '' });
    const [pagination, setPagination] = useState({ pageNumber: 1, pageSize: 20, totalRecords: 0, totalPages: 0 });
    const [sortBy, setSortBy] = useState('CreatedDate');
    const [sortOrder, setSortOrder] = useState('DESC');
    const [reasonModal, setReasonModal] = useState({ open: false, supervisor: null, reason: '' });

    useEffect(() => {
        const timer = setTimeout(() => {
            setDebouncedSearch(searchTerm);
            setPagination(prev => ({ ...prev, pageNumber: 1 }));
        }, 400);
        return () => clearTimeout(timer);
    }, [searchTerm]);

    const fetchData = useCallback(async () => {
        setLoading(true);
        try {
            const params = {
                pageNumber: pagination.pageNumber,
                pageSize: pagination.pageSize,
                sortBy,
                sortOrder,
            };
            if (debouncedSearch) params.searchTerm = debouncedSearch;
            if (filters.supervisorType) params.supervisorType = filters.supervisorType;
            if (filters.city) params.city = filters.city;
            if (filters.state) params.state = filters.state;
            if (filters.isBlocked === 'true') params.isBlocked = true;
            if (filters.dateFrom) params.dateFrom = filters.dateFrom;
            if (filters.dateTo) params.dateTo = filters.dateTo;
            if (showDeleted) params.isDeleted = true;

            const response = await supervisorManagementApi.getActiveSupervisors(params);
            if (response.result) {
                const d = response.data;
                setData(d.supervisors || []);
                setPagination(prev => ({
                    ...prev,
                    totalRecords: d.totalRecords,
                    totalPages: d.totalPages,
                }));
            }
        } catch {
            toast.error('Failed to load supervisors');
        } finally {
            setLoading(false);
        }
    }, [pagination.pageNumber, pagination.pageSize, debouncedSearch, filters, sortBy, sortOrder, showDeleted]);

    useEffect(() => { fetchData(); }, [fetchData]);

    const stats = {
        total: pagination.totalRecords,
        blocked: data.filter(s => s.isBlocked).length,
        career: data.filter(s => s.supervisorType === 'CAREER').length,
        registered: data.filter(s => s.supervisorType === 'REGISTERED').length,
    };

    const handleBlock = (supervisor) => {
        setReasonModal({ open: true, supervisor, reason: '' });
    };

    const handleBlockConfirm = async () => {
        const { supervisor, reason } = reasonModal;
        setReasonModal({ open: false, supervisor: null, reason: '' });

        try {
            const result = await supervisorManagementApi.blockSupervisor(supervisor.supervisorId, reason || null);
            if (result.result) {
                toast.success('Supervisor blocked successfully');
                fetchData();
            } else {
                toast.error(result.message || 'Failed to block supervisor');
            }
        } catch {
            toast.error('Network error. Please try again.');
        }
    };

    const handleUnblock = async (supervisor) => {
        const confirmed = await confirm({
            title: `Unblock ${supervisor.fullName}?`,
            message: 'This supervisor will be reactivated and can receive event assignments again.',
            type: 'info',
            confirmText: 'Unblock',
            cancelText: 'Cancel'
        });
        if (!confirmed) return;

        try {
            const result = await supervisorManagementApi.unblockSupervisor(supervisor.supervisorId);
            if (result.result) {
                toast.success('Supervisor unblocked successfully');
                fetchData();
            } else {
                toast.error(result.message || 'Failed to unblock supervisor');
            }
        } catch {
            toast.error('Network error. Please try again.');
        }
    };

    const handleDelete = async (supervisor) => {
        const confirmed = await confirm({
            title: `Delete ${supervisor.fullName}?`,
            message: 'This will soft-delete the supervisor. It can be restored later.',
            type: 'delete',
            confirmText: 'Delete',
            cancelText: 'Cancel'
        });
        if (!confirmed) return;

        try {
            const result = await supervisorManagementApi.deleteSupervisor(supervisor.supervisorId);
            if (result.result) {
                toast.success('Supervisor deleted successfully');
                fetchData();
            } else {
                toast.error(result.message || 'Failed to delete supervisor');
            }
        } catch {
            toast.error('Network error. Please try again.');
        }
    };

    const handleRestore = async (supervisor) => {
        const confirmed = await confirm({
            title: `Restore ${supervisor.fullName}?`,
            message: 'This will restore the deleted supervisor.',
            type: 'info',
            confirmText: 'Restore',
            cancelText: 'Cancel'
        });
        if (!confirmed) return;

        try {
            const result = await supervisorManagementApi.restoreSupervisor(supervisor.supervisorId);
            if (result.result) {
                toast.success('Supervisor restored successfully');
                fetchData();
            } else {
                toast.error(result.message || 'Failed to restore supervisor');
            }
        } catch {
            toast.error('Network error. Please try again.');
        }
    };

    const handleExport = async () => {
        try {
            const params = {};
            if (debouncedSearch) params.searchTerm = debouncedSearch;
            if (filters.supervisorType) params.supervisorType = filters.supervisorType;
            if (filters.city) params.city = filters.city;
            if (filters.state) params.state = filters.state;
            if (showDeleted) params.isDeleted = true;

            const response = await supervisorManagementApi.exportSupervisors(params);
            if (!response.ok) throw new Error('Export failed');

            const blob = await response.blob();
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `supervisors_export_${new Date().toISOString().slice(0, 10)}.csv`;
            document.body.appendChild(a);
            a.click();
            a.remove();
            window.URL.revokeObjectURL(url);
            toast.success('Export downloaded successfully');
        } catch {
            toast.error('Failed to export supervisors');
        }
    };

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

    const activeFilterCount = Object.values(filters).filter(v => v !== '').length + (showDeleted ? 1 : 0);

    const getStatusBadge = (currentStatus, isDeleted, isBlocked) => {
        if (isDeleted) return { label: 'Deleted', color: 'bg-gray-100 text-gray-800', icon: Trash2 };
        if (isBlocked) return ACTIVE_STATUS_MAP.SUSPENDED;
        return ACTIVE_STATUS_MAP[currentStatus] || ACTIVE_STATUS_MAP.ACTIVE;
    };

    const clearFilters = () => {
        setFilters({ supervisorType: '', city: '', state: '', isBlocked: '', dateFrom: '', dateTo: '' });
        setShowDeleted(false);
        setSearchTerm('');
    };

    return (
        <AdminLayout>
            <div className="space-y-6">
                {/* Page Header */}
                <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
                    <div>
                        <h1 className="text-2xl font-bold text-gray-900 flex items-center gap-2">
                            <UserCheck className="w-7 h-7 text-indigo-600" />
                            Approved Supervisors
                        </h1>
                        <p className="text-gray-500 mt-1 text-sm">Manage approved and active supervisors</p>
                    </div>
                </div>

                {/* Stats Cards */}
                <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
                    {[
                        { label: 'Total Supervisors', value: stats.total, icon: Users, color: 'text-indigo-600 bg-indigo-50' },
                        { label: 'Blocked', value: stats.blocked, icon: Ban, color: 'text-orange-600 bg-orange-50' },
                        { label: 'Career', value: stats.career, icon: UserCheck, color: 'text-blue-600 bg-blue-50' },
                        { label: 'Registered', value: stats.registered, icon: Users, color: 'text-teal-600 bg-teal-50' },
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

                {/* Search & Filter */}
                <div className="bg-white rounded-xl border border-gray-200 p-4">
                    <div className="flex flex-col sm:flex-row gap-3">
                        <div className="relative flex-1">
                            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                            <input
                                type="text"
                                placeholder="Search by name, email, phone..."
                                value={searchTerm}
                                onChange={(e) => setSearchTerm(e.target.value)}
                                className="w-full pl-10 pr-4 py-2.5 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                            />
                        </div>
                        <div className="flex gap-2">
                            <button onClick={handleExport} className="inline-flex items-center gap-2 px-4 py-2.5 bg-white border border-gray-300 rounded-lg text-sm font-medium text-gray-700 hover:bg-gray-50">
                                <Download className="w-4 h-4" /> Export
                            </button>
                            <button
                                onClick={() => setShowFilters(!showFilters)}
                                className={`inline-flex items-center gap-2 px-4 py-2.5 border rounded-lg text-sm font-medium transition-colors ${showFilters ? 'bg-indigo-50 border-indigo-300 text-indigo-700' : 'bg-white border-gray-300 text-gray-700 hover:bg-gray-50'}`}
                            >
                                <Filter className="w-4 h-4" />
                                Filters
                                {activeFilterCount > 0 && (
                                    <span className="inline-flex items-center justify-center w-5 h-5 text-xs font-bold bg-indigo-600 text-white rounded-full">{activeFilterCount}</span>
                                )}
                            </button>
                            {activeFilterCount > 0 && (
                                <button onClick={clearFilters} className="inline-flex items-center gap-1 px-3 py-2.5 text-sm text-red-600 hover:bg-red-50 rounded-lg">
                                    <X className="w-4 h-4" /> Clear
                                </button>
                            )}
                            <button onClick={fetchData} className="inline-flex items-center gap-2 px-4 py-2.5 bg-white border border-gray-300 rounded-lg text-sm font-medium text-gray-700 hover:bg-gray-50">
                                <RefreshCw className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`} />
                            </button>
                        </div>
                    </div>
                    {showFilters && (
                        <div className="mt-4 pt-4 border-t border-gray-200 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
                            <div>
                                <label className="block text-xs font-medium text-gray-600 mb-1">City</label>
                                <input
                                    type="text"
                                    placeholder="Filter by city..."
                                    value={filters.city}
                                    onChange={(e) => setFilters(prev => ({ ...prev, city: e.target.value }))}
                                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                                />
                            </div>
                            <div>
                                <label className="block text-xs font-medium text-gray-600 mb-1">State</label>
                                <input
                                    type="text"
                                    placeholder="Filter by state..."
                                    value={filters.state}
                                    onChange={(e) => setFilters(prev => ({ ...prev, state: e.target.value }))}
                                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                                />
                            </div>
                            <div>
                                <label className="block text-xs font-medium text-gray-600 mb-1">Status</label>
                                <select value={filters.isBlocked} onChange={(e) => setFilters(prev => ({ ...prev, isBlocked: e.target.value }))} className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500 focus:border-transparent">
                                    <option value="">All</option>
                                    <option value="true">Blocked Only</option>
                                </select>
                            </div>
                            <div>
                                <label className="block text-xs font-medium text-gray-600 mb-1">Date From</label>
                                <div className="relative">
                                    <Calendar className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                                    <input
                                        type="date"
                                        value={filters.dateFrom}
                                        onChange={(e) => setFilters(prev => ({ ...prev, dateFrom: e.target.value }))}
                                        className="w-full pl-10 pr-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                                    />
                                </div>
                            </div>
                            <div>
                                <label className="block text-xs font-medium text-gray-600 mb-1">Date To</label>
                                <div className="relative">
                                    <Calendar className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                                    <input
                                        type="date"
                                        value={filters.dateTo}
                                        onChange={(e) => setFilters(prev => ({ ...prev, dateTo: e.target.value }))}
                                        className="w-full pl-10 pr-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                                    />
                                </div>
                            </div>
                            <div className="flex items-end">
                                <label className="inline-flex items-center gap-2 cursor-pointer pb-2">
                                    <input type="checkbox" checked={showDeleted} onChange={(e) => setShowDeleted(e.target.checked)} className="w-4 h-4 text-indigo-600 rounded focus:ring-indigo-500" />
                                    <span className="text-sm text-gray-600">Show deleted</span>
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
                                    <div className="flex-1 space-y-2"><div className="h-4 bg-gray-200 rounded w-1/3" /><div className="h-3 bg-gray-200 rounded w-1/4" /></div>
                                    <div className="h-6 w-20 bg-gray-200 rounded-full" />
                                </div>
                            ))}
                        </div>
                    ) : data.length === 0 ? (
                        <div className="p-12 text-center">
                            <Users className="w-12 h-12 text-gray-300 mx-auto mb-4" />
                            <h3 className="text-lg font-semibold text-gray-900">No supervisors found</h3>
                            <p className="text-gray-500 mt-1 text-sm">No supervisors match your current filters</p>
                        </div>
                    ) : (
                        <>
                            <div className="overflow-x-auto">
                                <table className="min-w-full divide-y divide-gray-200">
                                    <thead className="bg-gray-50">
                                        <tr>
                                            <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider cursor-pointer hover:text-gray-900" onClick={() => handleSort('FullName')}>Name <SortIcon column="FullName" /></th>
                                            <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider">Email</th>
                                            <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider">Mobile</th>
                                            <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider">City</th>
                                            <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider">State</th>
                                            <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider">Status</th>
                                            <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider cursor-pointer hover:text-gray-900" onClick={() => handleSort('CreatedDate')}>Created Date <SortIcon column="CreatedDate" /></th>
                                            <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider cursor-pointer hover:text-gray-900" onClick={() => handleSort('LastUpdated')}>Last Updated <SortIcon column="LastUpdated" /></th>
                                            <th className="px-4 py-3 text-right text-xs font-semibold text-gray-600 uppercase tracking-wider">Actions</th>
                                        </tr>
                                    </thead>
                                    <tbody className="divide-y divide-gray-100">
                                        {data.map((sup) => {
                                            const statusInfo = getStatusBadge(sup.currentStatus, sup.isDeleted, sup.isBlocked);
                                            const StatusIcon = statusInfo.icon;
                                            const typeBadge = TYPE_BADGE[sup.supervisorType] || TYPE_BADGE.REGISTERED;
                                            return (
                                                <tr key={sup.supervisorId} className={`hover:bg-gray-50 transition-colors ${sup.isDeleted ? 'opacity-60' : ''}`}>
                                                    <td className="px-4 py-3">
                                                        <div className="flex items-center gap-3">
                                                            <div className="flex-shrink-0 w-9 h-9 bg-indigo-100 rounded-lg flex items-center justify-center">
                                                                <span className="text-sm font-semibold text-indigo-700">{sup.fullName?.charAt(0)?.toUpperCase()}</span>
                                                            </div>
                                                            <div>
                                                                <p className="text-sm font-medium text-gray-900">{sup.fullName}</p>
                                                                <span className={`inline-flex items-center px-1.5 py-0.5 rounded text-[10px] font-medium ${typeBadge.color}`}>{typeBadge.label}</span>
                                                            </div>
                                                        </div>
                                                    </td>
                                                    <td className="px-4 py-3 text-sm text-gray-600">{sup.email}</td>
                                                    <td className="px-4 py-3 text-sm text-gray-600">{sup.phone}</td>
                                                    <td className="px-4 py-3">
                                                        <div className="flex items-center gap-1 text-sm text-gray-600">
                                                            <MapPin className="w-3.5 h-3.5 text-gray-400 flex-shrink-0" />
                                                            <span className="truncate">{sup.city || '-'}</span>
                                                        </div>
                                                    </td>
                                                    <td className="px-4 py-3 text-sm text-gray-600">{sup.state || '-'}</td>
                                                    <td className="px-4 py-3">
                                                        <span className={`inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-medium ${statusInfo.color}`}>
                                                            <StatusIcon className="w-3 h-3" />
                                                            {statusInfo.label}
                                                        </span>
                                                    </td>
                                                    <td className="px-4 py-3 text-sm text-gray-500">
                                                        {new Date(sup.createdDate).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' })}
                                                    </td>
                                                    <td className="px-4 py-3 text-sm text-gray-500">
                                                        {sup.lastUpdated
                                                            ? new Date(sup.lastUpdated).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' })
                                                            : '-'
                                                        }
                                                    </td>
                                                    <td className="px-4 py-3">
                                                        <div className="flex items-center justify-end gap-1">
                                                            {sup.isDeleted ? (
                                                                <button onClick={() => handleRestore(sup)} className="p-1.5 text-emerald-600 hover:bg-emerald-50 rounded-lg transition-colors" title="Restore">
                                                                    <RotateCcw className="w-4 h-4" />
                                                                </button>
                                                            ) : (
                                                                <>
                                                                    {sup.isBlocked ? (
                                                                        <button onClick={() => handleUnblock(sup)} className="p-1.5 text-green-600 hover:bg-green-50 rounded-lg transition-colors" title="Unblock">
                                                                            <ShieldCheck className="w-4 h-4" />
                                                                        </button>
                                                                    ) : sup.currentStatus === 'ACTIVE' && (
                                                                        <button onClick={() => handleBlock(sup)} className="p-1.5 text-orange-600 hover:bg-orange-50 rounded-lg transition-colors" title="Block">
                                                                            <ShieldOff className="w-4 h-4" />
                                                                        </button>
                                                                    )}
                                                                    <button onClick={() => handleDelete(sup)} className="p-1.5 text-red-500 hover:bg-red-50 rounded-lg transition-colors" title="Delete">
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
                                    <button onClick={() => setPagination(prev => ({ ...prev, pageNumber: prev.pageNumber - 1 }))} disabled={pagination.pageNumber <= 1} className="inline-flex items-center gap-1 px-3 py-1.5 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed">
                                        <ChevronLeft className="w-4 h-4" /> Prev
                                    </button>
                                    <span className="text-sm text-gray-600">Page {pagination.pageNumber} of {pagination.totalPages || 1}</span>
                                    <button onClick={() => setPagination(prev => ({ ...prev, pageNumber: prev.pageNumber + 1 }))} disabled={pagination.pageNumber >= pagination.totalPages} className="inline-flex items-center gap-1 px-3 py-1.5 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed">
                                        Next <ChevronRight className="w-4 h-4" />
                                    </button>
                                </div>
                            </div>
                        </>
                    )}
                </div>

                {/* Block Reason Modal */}
                {reasonModal.open && (
                    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
                        <div className="fixed inset-0 bg-black/50 backdrop-blur-sm" onClick={() => setReasonModal({ open: false, supervisor: null, reason: '' })} />
                        <div className="relative bg-white rounded-xl shadow-2xl w-full max-w-md">
                            <div className="p-6">
                                <div className="mx-auto flex items-center justify-center h-12 w-12 rounded-full mb-4 bg-orange-100">
                                    <ShieldOff className="w-6 h-6 text-orange-600" />
                                </div>
                                <h3 className="text-lg font-semibold text-gray-900 text-center">Block {reasonModal.supervisor?.fullName}?</h3>
                                <p className="text-sm text-gray-500 text-center mt-1">This supervisor will be suspended from the platform and cannot log in.</p>
                                <div className="mt-4">
                                    <label className="block text-sm font-medium text-gray-700 mb-1.5">Reason (optional)</label>
                                    <textarea
                                        value={reasonModal.reason}
                                        onChange={(e) => setReasonModal(prev => ({ ...prev, reason: e.target.value }))}
                                        placeholder="Enter reason for blocking..."
                                        rows={3}
                                        className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500 focus:border-transparent resize-none"
                                    />
                                </div>
                            </div>
                            <div className="bg-gray-50 px-6 py-4 flex flex-col sm:flex-row-reverse gap-3 rounded-b-xl">
                                <button onClick={handleBlockConfirm} className="w-full sm:w-auto px-4 py-2 bg-orange-500 hover:bg-orange-600 text-white rounded-lg text-sm font-medium transition-colors">
                                    Block Supervisor
                                </button>
                                <button onClick={() => setReasonModal({ open: false, supervisor: null, reason: '' })} className="w-full sm:w-auto px-4 py-2 bg-white border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 text-sm font-medium">
                                    Cancel
                                </button>
                            </div>
                        </div>
                    </div>
                )}
            </div>
        </AdminLayout>
    );
};

export default ApprovedSupervisors;
