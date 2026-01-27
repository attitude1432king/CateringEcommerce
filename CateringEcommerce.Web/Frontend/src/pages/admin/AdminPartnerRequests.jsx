import { useState, useEffect } from 'react';
import { Plus, Filter, Download, Search, RefreshCw } from 'lucide-react';
import AdminLayout from '../../components/admin/layout/AdminLayout';
import { ProtectedRoute } from '../../components/admin/auth/ProtectedRoute';
import { PermissionButton } from '../../components/admin/ui/PermissionButton';
import PartnerRequestsTable from '../../components/admin/partner-requests/PartnerRequestsTable';
import PartnerDetailDrawer from '../../components/admin/partner-requests/PartnerDetailDrawer';
import PartnerFilters from '../../components/admin/partner-requests/PartnerFilters';
import { partnerRequestApi } from '../../services/partnerRequestApi';
import { toast } from 'react-hot-toast';

/**
 * Admin Partner Requests Management Page
 *
 * Features:
 * - List of all partner registration requests
 * - Advanced filtering (status, city, date range, search)
 * - Detail drawer with full partner information
 * - Approve/Reject/Request Info actions
 * - Multi-channel communication
 * - Export functionality
 * - Real-time status updates
 */
const AdminPartnerRequests = () => {
    const [requests, setRequests] = useState([]);
    const [loading, setLoading] = useState(true);
    const [selectedRequest, setSelectedRequest] = useState(null);
    const [showDetailDrawer, setShowDetailDrawer] = useState(false);
    const [showFilters, setShowFilters] = useState(false);
    const [stats, setStats] = useState({
        total: 0,
        pending: 0,
        underReview: 0,
        infoRequested: 0,
        approved: 0,
        rejected: 0
    });

    // Filters state
    const [filters, setFilters] = useState({
        status: '',
        city: '',
        dateFrom: '',
        dateTo: '',
        searchTerm: '',
        pageNumber: 1,
        pageSize: 20,
        sortBy: 'SubmittedDate',
        sortOrder: 'DESC'
    });

    // Fetch partner requests
    useEffect(() => {
        fetchPartnerRequests();
    }, [filters]);

    const fetchPartnerRequests = async () => {
        setLoading(true);
        try {
            const result = await partnerRequestApi.getAll(filters);

            if (result.success) {
                setRequests(result.data.requests || []);
                setStats({
                    total: result.data.totalRecords || 0,
                    pending: result.data.pendingCount || 0,
                    underReview: result.data.underReviewCount || 0,
                    infoRequested: result.data.infoRequestedCount || 0,
                    approved: 0, // Can be added to API response
                    rejected: 0  // Can be added to API response
                });
            } else {
                toast.error(result.message || 'Failed to load partner requests');
            }
        } catch (error) {
            console.error('Error fetching partner requests:', error);
            toast.error('Network error. Please try again.');
        } finally {
            setLoading(false);
        }
    };

    const handleViewDetails = async (request) => {
        setLoading(true);
        try {
            // Fetch full details
            const result = await partnerRequestApi.getDetails(request.requestId);

            if (result.success) {
                setSelectedRequest(result.data);
                setShowDetailDrawer(true);
            } else {
                toast.error('Failed to load request details');
            }
        } catch (error) {
            console.error('Error fetching request details:', error);
            toast.error('Network error. Please try again.');
        } finally {
            setLoading(false);
        }
    };

    const handleActionSuccess = () => {
        // Refresh the list
        fetchPartnerRequests();

        // Close drawer
        setShowDetailDrawer(false);
        setSelectedRequest(null);
    };

    const handleFilterChange = (newFilters) => {
        setFilters({
            ...filters,
            ...newFilters,
            pageNumber: 1 // Reset to first page
        });
    };

    const handlePageChange = (pageNumber) => {
        setFilters({
            ...filters,
            pageNumber
        });
    };

    const handleExport = async () => {
        try {
            toast.loading('Exporting data...');
            const result = await partnerRequestApi.export(filters, 'EXCEL');

            if (result.success) {
                toast.success('Export completed successfully');
                // Download file logic
            } else {
                toast.error('Export failed');
            }
        } catch (error) {
            toast.error('Export failed');
        }
    };

    // Quick filter buttons
    const quickFilters = [
        { label: 'All', value: '', count: stats.total },
        { label: 'Pending', value: 'PENDING', count: stats.pending },
        { label: 'Under Review', value: 'UNDER_REVIEW', count: stats.underReview },
        { label: 'Info Requested', value: 'INFO_REQUESTED', count: stats.infoRequested }
    ];

    return (
        <ProtectedRoute permission="PARTNER_REQUEST_VIEW">
            <AdminLayout>
                <div className="space-y-6">
                    {/* Page Header */}
                    <div className="flex items-center justify-between">
                        <div>
                            <h1 className="text-2xl font-bold text-gray-900">
                                Partner Registration Requests
                            </h1>
                            <p className="text-sm text-gray-600 mt-1">
                                Review and approve new partner registrations
                            </p>
                        </div>

                        {/* Action Buttons */}
                        <div className="flex items-center space-x-3">
                            <button
                                onClick={() => fetchPartnerRequests()}
                                className="inline-flex items-center px-3 py-2 border border-gray-300 text-gray-700 bg-white rounded-lg hover:bg-gray-50 transition-colors"
                                title="Refresh"
                            >
                                <RefreshCw className="w-4 h-4" />
                            </button>

                            <button
                                onClick={() => setShowFilters(!showFilters)}
                                className={`inline-flex items-center space-x-2 px-4 py-2 border rounded-lg transition-colors ${showFilters
                                        ? 'border-indigo-600 bg-indigo-50 text-indigo-700'
                                        : 'border-gray-300 bg-white text-gray-700 hover:bg-gray-50'
                                    }`}
                            >
                                <Filter className="w-4 h-4" />
                                <span>Filters</span>
                            </button>

                            <PermissionButton
                                permission="PARTNER_REQUEST_EXPORT"
                                variant="secondary"
                                onClick={handleExport}
                            >
                                <Download className="w-4 h-4 mr-2" />
                                Export
                            </PermissionButton>
                        </div>
                    </div>

                    {/* Stats Cards */}
                    <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
                        {quickFilters.map((filter) => (
                            <button
                                key={filter.value}
                                onClick={() => handleFilterChange({ status: filter.value })}
                                className={`p-4 rounded-lg border-2 transition-all text-left ${filters.status === filter.value
                                        ? 'border-indigo-600 bg-indigo-50'
                                        : 'border-gray-200 bg-white hover:border-gray-300'
                                    }`}
                            >
                                <div className="text-2xl font-bold text-gray-900">
                                    {filter.count}
                                </div>
                                <div className="text-sm text-gray-600 mt-1">
                                    {filter.label}
                                </div>
                            </button>
                        ))}
                    </div>

                    {/* Advanced Filters Panel */}
                    {showFilters && (
                        <PartnerFilters
                            filters={filters}
                            onFilterChange={handleFilterChange}
                            onClose={() => setShowFilters(false)}
                        />
                    )}

                    {/* Search Bar */}
                    <div className="flex items-center space-x-4">
                        <div className="flex-1 relative">
                            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
                            <input
                                type="text"
                                placeholder="Search by business name, owner name, phone..."
                                value={filters.searchTerm}
                                onChange={(e) => handleFilterChange({ searchTerm: e.target.value })}
                                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500"
                            />
                        </div>
                    </div>

                    {/* Partner Requests Table */}
                    <PartnerRequestsTable
                        requests={requests}
                        loading={loading}
                        onViewDetails={handleViewDetails}
                        onRefresh={fetchPartnerRequests}
                        currentPage={filters.pageNumber}
                        pageSize={filters.pageSize}
                        totalRecords={stats.total}
                        onPageChange={handlePageChange}
                    />

                    {/* Partner Detail Drawer */}
                    {showDetailDrawer && selectedRequest && (
                        <PartnerDetailDrawer
                            request={selectedRequest}
                            onClose={() => {
                                setShowDetailDrawer(false);
                                setSelectedRequest(null);
                            }}
                            onActionSuccess={handleActionSuccess}
                        />
                    )}
                </div>
            </AdminLayout>
        </ProtectedRoute>
    );
};

export default AdminPartnerRequests;
