import { useState, useEffect, useCallback } from 'react';
import {
    Search, RefreshCw, Filter, X, ChevronLeft, ChevronRight,
    CheckCircle, XCircle, Clock, FileSearch, AlertTriangle,
    Users, ClipboardCheck, MapPin, Eye
} from 'lucide-react';
import AdminLayout from '../../components/admin/layout/AdminLayout';
import { supervisorManagementApi } from '../../services/adminApi';
import { useConfirmation } from '../../contexts/ConfirmationContext';
import { toast } from 'react-hot-toast';
import PartnerStatusBadge from '../../components/admin/partner-requests/PartnerStatusBadge';
import SupervisorDetailDrawer from '../../components/admin/supervisor/SupervisorDetailDrawer';

// Centralized status enum (matches SupervisorApprovalStatus C# enum: 0-4)
const SUPERVISOR_STATUS = Object.freeze({
    PENDING: 0,
    APPROVED: 1,
    REJECTED: 2,
    UNDER_REVIEW: 3,
    INFO_REQUESTED: 4,
});

const STATUS_CONFIG = Object.freeze({
    [SUPERVISOR_STATUS.PENDING]: { label: 'Pending', color: 'bg-yellow-100 text-yellow-800', icon: Clock },
    [SUPERVISOR_STATUS.APPROVED]: { label: 'Approved', color: 'bg-green-100 text-green-800', icon: CheckCircle },
    [SUPERVISOR_STATUS.REJECTED]: { label: 'Rejected', color: 'bg-red-100 text-red-800', icon: XCircle },
    [SUPERVISOR_STATUS.UNDER_REVIEW]: { label: 'Under Review', color: 'bg-blue-100 text-blue-800', icon: FileSearch },
    [SUPERVISOR_STATUS.INFO_REQUESTED]: { label: 'Info Requested', color: 'bg-purple-100 text-purple-800', icon: AlertTriangle },
});

const TYPE_BADGE = Object.freeze({
    CAREER: { label: 'Career', color: 'bg-blue-100 text-blue-700' },
    REGISTERED: { label: 'Registered', color: 'bg-teal-100 text-teal-700' },
});

const PendingSupervisorRequests = () => {
    const confirm = useConfirmation();
    const [data, setData] = useState([]);
    const [loading, setLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState('');
    const [debouncedSearch, setDebouncedSearch] = useState('');
    const [showFilters, setShowFilters] = useState(false);
    const [filters, setFilters] = useState({ status: '', supervisorType: '' });
    const [pagination, setPagination] = useState({ pageNumber: 1, pageSize: 20, totalRecords: 0, totalPages: 0 });
    const [sortBy, setSortBy] = useState('CreatedDate');
    const [sortOrder, setSortOrder] = useState('DESC');
    const [reasonModal, setReasonModal] = useState({ open: false, supervisor: null, action: null, reason: '' });
    const [detailDrawer, setDetailDrawer] = useState({ open: false, supervisorId: null });

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
            if (filters.status !== '') params.status = filters.status;
            if (filters.supervisorType) params.supervisorType = filters.supervisorType;

            const response = await supervisorManagementApi.getRegistrations(params);
            if (response.result) {
                const d = response.data;
                setData(d.registrations || []);
                setPagination(prev => ({
                    ...prev,
                    totalRecords: d.totalRecords,
                    totalPages: d.totalPages,
                }));
            }
        } catch {
            toast.error('Failed to load registration requests');
        } finally {
            setLoading(false);
        }
    }, [pagination.pageNumber, pagination.pageSize, debouncedSearch, filters, sortBy, sortOrder]);

    useEffect(() => { fetchData(); }, [fetchData]);

    const stats = {
        total: pagination.totalRecords,
        pending: data.filter(r => r.status === SUPERVISOR_STATUS.PENDING).length,
        underReview: data.filter(r => r.status === SUPERVISOR_STATUS.UNDER_REVIEW).length,
        infoRequested: data.filter(r => r.status === SUPERVISOR_STATUS.INFO_REQUESTED).length,
    };

    const handleApprove = async (supervisor) => {
        const confirmed = await confirm({
            title: `Approve ${supervisor.fullName}?`,
            message: 'This supervisor will be activated and can receive event assignments.',
            type: 'info',
            confirmText: 'Approve',
            cancelText: 'Cancel'
        });
        if (!confirmed) return;

        try {
            const result = await supervisorManagementApi.updateStatus(supervisor.supervisorId, SUPERVISOR_STATUS.APPROVED, null);
            if (result.result) {
                toast.success('Supervisor approved successfully');
                fetchData();
            } else {
                toast.error(result.message || 'Failed to approve supervisor');
            }
        } catch {
            toast.error('Network error. Please try again.');
        }
    };

    const handleReject = (supervisor) => {
        setReasonModal({ open: true, supervisor, action: 'reject', reason: '' });
    };

    const handleUnderReview = async (supervisor) => {
        const confirmed = await confirm({
            title: `Mark ${supervisor.fullName} as Under Review?`,
            message: 'This supervisor will be placed under review.',
            type: 'info',
            confirmText: 'Under Review',
            cancelText: 'Cancel'
        });
        if (!confirmed) return;

        try {
            const result = await supervisorManagementApi.updateStatus(supervisor.supervisorId, SUPERVISOR_STATUS.UNDER_REVIEW, null);
            if (result.result) {
                toast.success('Supervisor marked as Under Review');
                fetchData();
            } else {
                toast.error(result.message || 'Failed to update status');
            }
        } catch {
            toast.error('Network error. Please try again.');
        }
    };

    const handleInfoRequest = (supervisor) => {
        setReasonModal({ open: true, supervisor, action: 'info_request', reason: '' });
    };

    const handleReasonConfirm = async () => {
        const { supervisor, action, reason } = reasonModal;
        setReasonModal({ open: false, supervisor: null, action: null, reason: '' });

        const statusCode = action === 'reject' ? SUPERVISOR_STATUS.REJECTED : SUPERVISOR_STATUS.INFO_REQUESTED;
        try {
            const result = await supervisorManagementApi.updateStatus(supervisor.supervisorId, statusCode, reason || null);
            if (result.result) {
                toast.success(action === 'reject' ? 'Supervisor rejected' : 'Info requested from supervisor');
                fetchData();
            } else {
                toast.error(result.message || 'Failed to update status');
            }
        } catch {
            toast.error('Network error. Please try again.');
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

    const activeFilterCount = Object.values(filters).filter(v => v !== '').length;

    const isPendingStatus = (status) =>
        status === SUPERVISOR_STATUS.PENDING ||
        status === SUPERVISOR_STATUS.UNDER_REVIEW ||
        status === SUPERVISOR_STATUS.INFO_REQUESTED;

    return (
        <AdminLayout>
            <div className="space-y-6">
                {/* Page Header */}
                <div>
                    <h1 className="text-2xl font-bold text-gray-900 flex items-center gap-2">
                        <ClipboardCheck className="w-7 h-7 text-indigo-600" />
                        Pending Supervisor Requests
                    </h1>
                    <p className="text-gray-500 mt-1 text-sm">Review and manage supervisor registration applications</p>
                </div>

                {/* Stats Cards */}
                <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
                    {[
                        { label: 'Total Requests', value: stats.total, icon: Users, color: 'text-indigo-600 bg-indigo-50' },
                        { label: 'Pending', value: stats.pending, icon: Clock, color: 'text-yellow-600 bg-yellow-50' },
                        { label: 'Under Review', value: stats.underReview, icon: FileSearch, color: 'text-blue-600 bg-blue-50' },
                        { label: 'Info Requested', value: stats.infoRequested, icon: AlertTriangle, color: 'text-purple-600 bg-purple-50' },
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
                                <button onClick={() => { setFilters({ status: '', supervisorType: '' }); setSearchTerm(''); }} className="inline-flex items-center gap-1 px-3 py-2.5 text-sm text-red-600 hover:bg-red-50 rounded-lg">
                                    <X className="w-4 h-4" /> Clear
                                </button>
                            )}
                            <button onClick={fetchData} className="inline-flex items-center gap-2 px-4 py-2.5 bg-white border border-gray-300 rounded-lg text-sm font-medium text-gray-700 hover:bg-gray-50">
                                <RefreshCw className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`} />
                            </button>
                        </div>
                    </div>
                    {showFilters && (
                        <div className="mt-4 pt-4 border-t border-gray-200 grid grid-cols-1 sm:grid-cols-2 gap-3">
                            <div>
                                <label className="block text-xs font-medium text-gray-600 mb-1">Status</label>
                                <select value={filters.status} onChange={(e) => setFilters(prev => ({ ...prev, status: e.target.value }))} className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500 focus:border-transparent">
                                    <option value="">All Statuses</option>
                                    <option value={SUPERVISOR_STATUS.PENDING}>Pending</option>
                                    <option value={SUPERVISOR_STATUS.UNDER_REVIEW}>Under Review</option>
                                    <option value={SUPERVISOR_STATUS.INFO_REQUESTED}>Info Requested</option>
                                    <option value={SUPERVISOR_STATUS.REJECTED}>Rejected</option>
                                </select>
                            </div>
                            <div>
                                <label className="block text-xs font-medium text-gray-600 mb-1">Supervisor Type</label>
                                <select value={filters.supervisorType} onChange={(e) => setFilters(prev => ({ ...prev, supervisorType: e.target.value }))} className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500 focus:border-transparent">
                                    <option value="">All Types</option>
                                    <option value="CAREER">Career</option>
                                    <option value="REGISTERED">Registered</option>
                                </select>
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
                            <ClipboardCheck className="w-12 h-12 text-gray-300 mx-auto mb-4" />
                            <h3 className="text-lg font-semibold text-gray-900">No registration requests found</h3>
                            <p className="text-gray-500 mt-1 text-sm">No requests match your current filters</p>
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
                                            <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider">Experience</th>
                                            <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider cursor-pointer hover:text-gray-900" onClick={() => handleSort('CreatedDate')}>Requested Date <SortIcon column="CreatedDate" /></th>
                                            <th className="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider">Current Status</th>
                                            <th className="px-4 py-3 text-right text-xs font-semibold text-gray-600 uppercase tracking-wider">Actions</th>
                                        </tr>
                                    </thead>
                                    <tbody className="divide-y divide-gray-100">
                                        {data.map((reg) => {
                                            const statusInfo = STATUS_CONFIG[reg.status] || STATUS_CONFIG[SUPERVISOR_STATUS.PENDING];
                                            const StatusIcon = statusInfo.icon;
                                            const typeBadge = TYPE_BADGE[reg.supervisorType] || TYPE_BADGE.REGISTERED;
                                            return (
                                                <tr key={reg.supervisorId} className="hover:bg-gray-50 transition-colors">
                                                    <td className="px-4 py-3">
                                                        <div className="flex items-center gap-3">
                                                            <div className="flex-shrink-0 w-9 h-9 bg-indigo-100 rounded-lg flex items-center justify-center">
                                                                <span className="text-sm font-semibold text-indigo-700">{reg.fullName?.charAt(0)?.toUpperCase()}</span>
                                                            </div>
                                                            <div>
                                                                <p className="text-sm font-medium text-gray-900">{reg.fullName}</p>
                                                                <span className={`inline-flex items-center px-1.5 py-0.5 rounded text-[10px] font-medium ${typeBadge.color}`}>{typeBadge.label}</span>
                                                            </div>
                                                        </div>
                                                    </td>
                                                    <td className="px-4 py-3 text-sm text-gray-600">{reg.email}</td>
                                                    <td className="px-4 py-3 text-sm text-gray-600">{reg.phone}</td>
                                                    <td className="px-4 py-3">
                                                        <div className="flex items-center gap-1 text-sm text-gray-600">
                                                            <MapPin className="w-3.5 h-3.5 text-gray-400 flex-shrink-0" />
                                                            <span className="truncate">{reg.city || '-'}</span>
                                                        </div>
                                                    </td>
                                                    <td className="px-4 py-3 text-sm text-gray-600">{reg.state || '-'}</td>
                                                    <td className="px-4 py-3">
                                                        {reg.hasPriorExperience ? (
                                                            <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-blue-50 text-blue-700">
                                                                Experienced
                                                            </span>
                                                        ) : (
                                                            <span className="text-xs text-gray-400">Fresher</span>
                                                        )}
                                                    </td>
                                                    <td className="px-4 py-3 text-sm text-gray-500">
                                                        {new Date(reg.createdDate).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' })}
                                                    </td>
                                                    <td className="px-4 py-3">
                                                        <PartnerStatusBadge statusId={reg.status} statusName={statusInfo.label} size="sm" />
                                                    </td>
                                                    <td className="px-4 py-3">
                                                        <div className="flex items-center justify-end gap-1">
                                                            <button
                                                                onClick={() => setDetailDrawer({ open: true, supervisorId: reg.supervisorId })}
                                                                className="p-1.5 text-indigo-600 hover:bg-indigo-50 rounded-lg transition-colors"
                                                                title="View Details"
                                                            >
                                                                <Eye className="w-4 h-4" />
                                                            </button>
                                                            {isPendingStatus(reg.status) && (
                                                                <button onClick={() => handleApprove(reg)} className="p-1.5 text-green-600 hover:bg-green-50 rounded-lg transition-colors" title="Approve">
                                                                    <CheckCircle className="w-4 h-4" />
                                                                </button>
                                                            )}
                                                            {isPendingStatus(reg.status) && (
                                                                <button onClick={() => handleReject(reg)} className="p-1.5 text-red-600 hover:bg-red-50 rounded-lg transition-colors" title="Reject">
                                                                    <XCircle className="w-4 h-4" />
                                                                </button>
                                                            )}
                                                            {reg.status === SUPERVISOR_STATUS.PENDING && (
                                                                <>
                                                                    <button onClick={() => handleUnderReview(reg)} className="p-1.5 text-blue-600 hover:bg-blue-50 rounded-lg transition-colors" title="Under Review">
                                                                        <FileSearch className="w-4 h-4" />
                                                                    </button>
                                                                    <button onClick={() => handleInfoRequest(reg)} className="p-1.5 text-purple-600 hover:bg-purple-50 rounded-lg transition-colors" title="Request Info">
                                                                        <AlertTriangle className="w-4 h-4" />
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

                {/* Reason Modal (Reject / Info Request) */}
                {reasonModal.open && (
                    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
                        <div className="fixed inset-0 bg-black/50 backdrop-blur-sm" onClick={() => setReasonModal({ open: false, supervisor: null, action: null, reason: '' })} />
                        <div className="relative bg-white rounded-xl shadow-2xl w-full max-w-md">
                            <div className="p-6">
                                <div className={`mx-auto flex items-center justify-center h-12 w-12 rounded-full mb-4 ${reasonModal.action === 'reject' ? 'bg-red-100' : 'bg-purple-100'}`}>
                                    {reasonModal.action === 'reject' ? <XCircle className="w-6 h-6 text-red-600" /> : <AlertTriangle className="w-6 h-6 text-purple-600" />}
                                </div>
                                <h3 className="text-lg font-semibold text-gray-900 text-center">
                                    {reasonModal.action === 'reject' ? 'Reject' : 'Request Info from'} {reasonModal.supervisor?.fullName}?
                                </h3>
                                <p className="text-sm text-gray-500 text-center mt-1">
                                    {reasonModal.action === 'reject' ? 'This supervisor registration will be rejected.' : 'The supervisor will be notified to provide more information.'}
                                </p>
                                <div className="mt-4">
                                    <label className="block text-sm font-medium text-gray-700 mb-1.5">Reason (optional)</label>
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
                                    className={`w-full sm:w-auto px-4 py-2 text-white rounded-lg text-sm font-medium transition-colors ${reasonModal.action === 'reject' ? 'bg-red-600 hover:bg-red-700' : 'bg-purple-600 hover:bg-purple-700'}`}
                                >
                                    {reasonModal.action === 'reject' ? 'Reject' : 'Request Info'}
                                </button>
                                <button onClick={() => setReasonModal({ open: false, supervisor: null, action: null, reason: '' })} className="w-full sm:w-auto px-4 py-2 bg-white border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 text-sm font-medium">
                                    Cancel
                                </button>
                            </div>
                        </div>
                    </div>
                )}
            </div>

            {/* Supervisor Detail Drawer */}
            {detailDrawer.open && (
                <SupervisorDetailDrawer
                    supervisorId={detailDrawer.supervisorId}
                    onClose={() => setDetailDrawer({ open: false, supervisorId: null })}
                    onStatusUpdate={async (id, status, reason) => {
                        const result = await supervisorManagementApi.updateStatus(id, status, reason);
                        if (result?.result) {
                            toast.success('Supervisor status updated successfully');
                            setDetailDrawer({ open: false, supervisorId: null });
                            fetchData();
                        } else {
                            toast.error(result?.message || 'Failed to update status');
                        }
                    }}
                />
            )}
        </AdminLayout>
    );
};

export default PendingSupervisorRequests;
