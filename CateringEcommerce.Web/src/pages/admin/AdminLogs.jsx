import { useCallback, useEffect, useMemo, useState } from 'react';
import { FileWarning } from 'lucide-react';
import AdminLayout from '../../components/admin/layout/AdminLayout';
import { ProtectedRoute } from '../../components/admin/auth/ProtectedRoute';
import LogDetailDrawer from '../../components/admin/logs/LogDetailDrawer';
import LogFilters from '../../components/admin/logs/LogFilters';
import LogTable from '../../components/admin/logs/LogTable';
import { logsApi } from '../../services/adminApi';

const emptyFilters = {
    fromDate: '',
    toDate: '',
    errorId: '',
    userId: '',
    userRole: '',
    requestPath: '',
    httpMethod: '',
    statusCode: '',
    environment: '',
    keyword: '',
};

const toApiParams = (filters, pageNumber, pageSize, sortBy, sortOrder) => {
    const params = {
        pageNumber,
        pageSize,
        sortBy,
        sortOrder,
    };

    Object.entries(filters).forEach(([key, value]) => {
        if (value !== '') {
            params[key] = value;
        }
    });

    return params;
};

const AdminLogsContent = () => {
    const [filters, setFilters] = useState(emptyFilters);
    const [appliedFilters, setAppliedFilters] = useState(emptyFilters);
    const [logs, setLogs] = useState([]);
    const [pageNumber, setPageNumber] = useState(1);
    const [pageSize, setPageSize] = useState(20);
    const [totalCount, setTotalCount] = useState(0);
    const [totalPages, setTotalPages] = useState(0);
    const [sortBy, setSortBy] = useState('createdAt');
    const [sortOrder, setSortOrder] = useState('DESC');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const [selectedLog, setSelectedLog] = useState(null);
    const [detailLoading, setDetailLoading] = useState(false);

    const params = useMemo(
        () => toApiParams(appliedFilters, pageNumber, pageSize, sortBy, sortOrder),
        [appliedFilters, pageNumber, pageSize, sortBy, sortOrder]
    );

    const fetchLogs = useCallback(async () => {
        setLoading(true);
        setError('');

        try {
            const response = await logsApi.getLogs(params);
            const data = response.data || {};
            setLogs(data.logs || []);
            setTotalCount(data.totalCount || 0);
            setTotalPages(data.totalPages || 0);
        } catch (err) {
            setError(err.message || 'Failed to load logs.');
            setLogs([]);
            setTotalCount(0);
            setTotalPages(0);
        } finally {
            setLoading(false);
        }
    }, [params]);

    useEffect(() => {
        fetchLogs();
    }, [fetchLogs]);

    const applyFilters = () => {
        setPageNumber(1);
        setAppliedFilters(filters);
    };

    const resetFilters = () => {
        setFilters(emptyFilters);
        setAppliedFilters(emptyFilters);
        setPageNumber(1);
    };

    const handleSort = (column) => {
        if (sortBy === column) {
            setSortOrder((current) => (current === 'ASC' ? 'DESC' : 'ASC'));
        } else {
            setSortBy(column);
            setSortOrder('DESC');
        }
        setPageNumber(1);
    };

    const openDetails = async (log) => {
        setSelectedLog(log);
        setDetailLoading(true);

        try {
            const response = await logsApi.getLogById(log.id);
            setSelectedLog(response.data || log);
        } catch {
            setSelectedLog(log);
        } finally {
            setDetailLoading(false);
        }
    };

    const firstItem = totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1;
    const lastItem = Math.min(pageNumber * pageSize, totalCount);

    return (
        <AdminLayout>
            <div className="space-y-6">
                <div className="flex flex-wrap items-center justify-between gap-4">
                    <div>
                        <div className="flex items-center gap-3">
                            <div className="rounded-lg bg-red-100 p-2 text-red-700">
                                <FileWarning className="h-6 w-6" />
                            </div>
                            <div>
                                <h1 className="text-2xl font-bold text-gray-900">System Logs</h1>
                                <p className="text-sm text-gray-600">Review server exceptions and trace production issues.</p>
                            </div>
                        </div>
                    </div>
                    <div className="text-sm text-gray-600">
                        Showing {firstItem}-{lastItem} of {totalCount}
                    </div>
                </div>

                <LogFilters
                    filters={filters}
                    onChange={setFilters}
                    onApply={applyFilters}
                    onReset={resetFilters}
                    loading={loading}
                />

                {error && (
                    <div className="rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
                        {error}
                    </div>
                )}

                <LogTable
                    logs={logs}
                    loading={loading}
                    sortBy={sortBy}
                    sortOrder={sortOrder}
                    onSort={handleSort}
                    onSelect={openDetails}
                />

                <div className="flex flex-wrap items-center justify-between gap-3">
                    <select
                        value={pageSize}
                        onChange={(event) => {
                            setPageSize(Number(event.target.value));
                            setPageNumber(1);
                        }}
                        className="rounded-md border border-gray-300 px-3 py-2 text-sm"
                    >
                        {[10, 20, 50, 100].map((size) => (
                            <option key={size} value={size}>{size} per page</option>
                        ))}
                    </select>

                    <div className="flex items-center gap-2">
                        <button
                            type="button"
                            onClick={() => setPageNumber((page) => Math.max(1, page - 1))}
                            disabled={pageNumber <= 1 || loading}
                            className="rounded-md border border-gray-300 px-3 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50"
                        >
                            Previous
                        </button>
                        <span className="text-sm text-gray-600">
                            Page {pageNumber} of {Math.max(1, totalPages)}
                        </span>
                        <button
                            type="button"
                            onClick={() => setPageNumber((page) => Math.min(Math.max(1, totalPages), page + 1))}
                            disabled={pageNumber >= totalPages || loading}
                            className="rounded-md border border-gray-300 px-3 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50"
                        >
                            Next
                        </button>
                    </div>
                </div>
            </div>

            <LogDetailDrawer
                log={selectedLog}
                loading={detailLoading}
                onClose={() => setSelectedLog(null)}
            />
        </AdminLayout>
    );
};

const AdminLogs = () => (
    <ProtectedRoute requireSuperAdmin={true}>
        <AdminLogsContent />
    </ProtectedRoute>
);

export default AdminLogs;
