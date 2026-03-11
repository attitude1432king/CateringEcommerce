import { useState, useEffect } from 'react';
import { Filter, Download, Search, RefreshCw } from 'lucide-react';
import AdminLayout from '../../components/admin/layout/AdminLayout';
import { ProtectedRoute } from '../../components/admin/auth/ProtectedRoute';
import { PermissionButton } from '../../components/admin/ui/PermissionButton';
import PartnerRequestsTable from '../../components/admin/partner-requests/PartnerRequestsTable';
import PartnerDetailDrawer from '../../components/admin/partner-requests/PartnerDetailDrawer';
import PartnerFilters from '../../components/admin/partner-requests/PartnerFilters';
import { partnerApprovalApi, ApprovalStatus } from '../../services/partnerApprovalApi';
import { toast } from 'react-hot-toast';

/**
 * Admin Partner Requests Management Page (UPDATED - Enum-based)
 *
 * Features:
 * - List of all partner registration requests
 * - Advanced filtering (status, priority, city, date range, search)
 * - Detail drawer with full partner information
 * - Approve/Reject actions with validation
 * - Priority management
 * - Enum-based status and priority handling
 */
const AdminPartnerRequests = () => {
    const [requests, setRequests] = useState([]);
    const [loading, setLoading] = useState(true);
    const [selectedRequest, setSelectedRequest] = useState(null);
    const [showDetailDrawer, setShowDetailDrawer] = useState(false);
    const [showFilters, setShowFilters] = useState(false);
    const [stats, setStats] = useState({
        totalRequests: 0,
        pendingCount: 0,
        approvedCount: 0,
        rejectedCount: 0,
        underReviewCount: 0,
        infoRequestedCount: 0
    });

    // Pagination
    const [pagination, setPagination] = useState({
        currentPage: 1,
        pageSize: 20,
        totalPages: 0,
        totalCount: 0
    });

    // Filters state (uses enum IDs instead of strings)
    const [filters, setFilters] = useState({
        approvalStatusId: null,  // INT enum value (1-5)
        priorityId: null,        // INT enum value (0-3)
        cityId: null,
        fromDate: null,
        toDate: null,
        searchTerm: '',
        pageNumber: 1,
        pageSize: 20,
        sortBy: 'c_createddate',
        sortOrder: 'DESC'
    });

    // Fetch partner requests
    useEffect(() => {
        fetchPartnerRequests();
    }, [filters]);

    const fetchPartnerRequests = async () => {
        setLoading(true);
        try {
            const response = await partnerApprovalApi.getPendingRequests(filters);

            if (response.result) {
                setRequests(response.data.requests || []);
                setStats(response.data.stats || {});
                setPagination({
                    currentPage: response.data.pageNumber || 1,
                    pageSize: response.data.pageSize || 20,
                    totalPages: response.data.totalPages || 0,
                    totalCount: response.data.totalCount || 0
                });
            } else {
                console.error('❌ API returned failure:', response);
                toast.error(response.message || 'Failed to load partner requests');
            }
        } catch (error) {
            console.error('❌ Error fetching partner requests:', error);
            console.error('Error details:', {
                message: error.message,
                stack: error.stack
            });
            toast.error(`Network error: ${error.message}`);
        } finally {
            setLoading(false);
        }
    };

    const handleViewDetails = async (request) => {
        setLoading(true);
        try {
            // Fetch full details
            const response = await partnerApprovalApi.getPartnerDetail(request.ownerId);

            if (response.result) {
                setSelectedRequest(response.data);
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

    // P1 FIX: Implement CSV export functionality
    const handleExport = async () => {
        try {
            if (requests.length === 0) {
                toast.error('No data to export');
                return;
            }

            // Define CSV headers
            const headers = [
                'Application ID',
                'Business Name',
                'Owner Name',
                'Email',
                'Phone',
                'City',
                'Status',
                'Priority',
                'Applied Date',
                'Reviewed Date',
                'Notes'
            ];

            // Convert requests to CSV rows
            const rows = requests.map(req => [
                req.partnershipApplicationId || '',
                req.businessName || '',
                `${req.firstName || ''} ${req.lastName || ''}`.trim(),
                req.email || '',
                req.phoneNumber || '',
                req.city || '',
                req.approvalStatusName || '',
                req.priorityName || '',
                req.createdDate ? new Date(req.createdDate).toLocaleDateString() : '',
                req.reviewedDate ? new Date(req.reviewedDate).toLocaleDateString() : '',
                req.reviewNotes ? `"${req.reviewNotes.replace(/"/g, '""')}"` : '' // Escape quotes
            ]);

            // Create CSV content
            const csvContent = [
                headers.join(','),
                ...rows.map(row => row.join(','))
            ].join('\n');

            // Create blob and download
            const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
            const link = document.createElement('a');
            const url = URL.createObjectURL(blob);

            link.setAttribute('href', url);
            link.setAttribute('download', `partner-requests-${new Date().toISOString().split('T')[0]}.csv`);
            link.style.visibility = 'hidden';

            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);

            toast.success(`Exported ${requests.length} partner requests to CSV`);
        } catch (error) {
            console.error('Error exporting CSV:', error);
            toast.error('Failed to export CSV');
        }
    };

    // Quick filter buttons (using enum IDs)
    const quickFilters = [
        {
            label: 'All',
            value: null,
            count: stats.totalRequests,
            color: 'gray'
        },
        {
            label: 'Pending',
            value: ApprovalStatus.PENDING,
            count: stats.pendingCount,
            color: 'orange'
        },
        {
            label: 'Under Review',
            value: ApprovalStatus.UNDER_REVIEW,
            count: stats.underReviewCount,
            color: 'blue'
        },
        {
            label: 'Info Requested',
            value: ApprovalStatus.INFO_REQUESTED,
            count: stats.infoRequestedCount,
            color: 'purple'
        }
    ];

    const getQuickFilterStyles = (filterValue) => {
        const isActive = filters.approvalStatusId === filterValue;

        const colorMap = {
            gray: {
                active: 'border-gray-600 bg-gray-50',
                inactive: 'border-gray-200 bg-white hover:border-gray-300'
            },
            orange: {
                active: 'border-orange-600 bg-orange-50',
                inactive: 'border-gray-200 bg-white hover:border-gray-300'
            },
            blue: {
                active: 'border-blue-600 bg-blue-50',
                inactive: 'border-gray-200 bg-white hover:border-gray-300'
            },
            purple: {
                active: 'border-purple-600 bg-purple-50',
                inactive: 'border-gray-200 bg-white hover:border-gray-300'
            }
        };

        const filter = quickFilters.find(f => f.value === filterValue);
        const color = filter?.color || 'gray';

        return isActive ? colorMap[color].active : colorMap[color].inactive;
    };

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

                    {/* Stats Cards (Quick Filters) */}
                    <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
                        {quickFilters.map((filter) => (
                            <button
                                key={filter.label}
                                onClick={() => handleFilterChange({ approvalStatusId: filter.value })}
                                className={`p-4 rounded-lg border-2 transition-all text-left ${getQuickFilterStyles(filter.value)}`}
                            >
                                <div className="text-2xl font-bold text-gray-900">
                                    {filter.count || 0}
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
                                placeholder="Search by business name, owner name, phone, email..."
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
                        currentPage={pagination.currentPage}
                        pageSize={pagination.pageSize}
                        totalRecords={pagination.totalCount}
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
