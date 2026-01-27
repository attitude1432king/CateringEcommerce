import { useState, useEffect } from 'react';
import { Search, Filter, Eye, CheckCircle, XCircle, Ban, Trash2, Plus } from 'lucide-react';
import AdminLayout from '../../components/admin/layout/AdminLayout';
import Card from '../../components/admin/ui/Card';
import Table, { TableHeader, TableHeaderCell, TableBody, TableRow, TableCell, TablePagination } from '../../components/admin/ui/Table';
import Badge from '../../components/admin/ui/Badge';
import Button from '../../components/admin/ui/Button';
import Modal from '../../components/admin/ui/Modal';
import EmptyState from '../../components/admin/ui/EmptyState';
import LoadingSkeleton from '../../components/admin/ui/LoadingSkeleton';
import { cateringApi } from '../../services/adminApi';

const AdminCaterings = () => {
  const [caterings, setCaterings] = useState([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [pagination, setPagination] = useState({
    pageNumber: 1,
    pageSize: 20,
    totalRecords: 0,
    totalPages: 0,
  });

  // Modals
  const [selectedCatering, setSelectedCatering] = useState(null);
  const [showStatusModal, setShowStatusModal] = useState(false);
  const [statusAction, setStatusAction] = useState('');
  const [statusReason, setStatusReason] = useState('');
  const [actionLoading, setActionLoading] = useState(false);

  useEffect(() => {
    loadCaterings();
  }, [pagination.pageNumber, searchTerm, statusFilter]);

  const loadCaterings = async () => {
    setLoading(true);
    try {
      const params = {
        pageNumber: pagination.pageNumber,
        pageSize: pagination.pageSize,
        sortBy: 'CreatedDate',
        sortOrder: 'DESC',
      };

      if (searchTerm) params.searchTerm = searchTerm;
      if (statusFilter) params.status = statusFilter;

      const response = await cateringApi.getAll(params);

      if (response.result) {
        setCaterings(response.data.caterings);
        setPagination({
          ...pagination,
          totalRecords: response.data.totalRecords,
          totalPages: response.data.totalPages,
        });
      }
    } catch (error) {
      console.error('Error loading caterings:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleStatusChange = async () => {
    if (!selectedCatering || !statusAction) return;

    setActionLoading(true);
    try {
      await cateringApi.updateStatus(
        selectedCatering.cateringId,
        statusAction,
        statusReason || null
      );

      // Reload caterings
      loadCaterings();

      // Close modal
      setShowStatusModal(false);
      setSelectedCatering(null);
      setStatusAction('');
      setStatusReason('');
    } catch (error) {
      console.error('Error updating status:', error);
      alert('Failed to update catering status');
    } finally {
      setActionLoading(false);
    }
  };

  const openStatusModal = (catering, action) => {
    setSelectedCatering(catering);
    setStatusAction(action);
    setShowStatusModal(true);
  };

  return (
    <AdminLayout>
      <div className="space-y-6">
        {/* Page Header */}
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Caterings Management</h1>
            <p className="text-gray-600 mt-1">Manage and approve catering partners</p>
          </div>
          <Button icon={Plus} variant="primary">
            Add Catering
          </Button>
        </div>

        {/* Filters */}
        <Card>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            {/* Search */}
            <div className="relative">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
              <input
                type="text"
                placeholder="Search by name, phone, email..."
                value={searchTerm}
                onChange={(e) => {
                  setSearchTerm(e.target.value);
                  setPagination({ ...pagination, pageNumber: 1 });
                }}
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500"
              />
            </div>

            {/* Status Filter */}
            <div className="relative">
              <Filter className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
              <select
                value={statusFilter}
                onChange={(e) => {
                  setStatusFilter(e.target.value);
                  setPagination({ ...pagination, pageNumber: 1 });
                }}
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 appearance-none"
              >
                <option value="">All Status</option>
                <option value="Pending">Pending</option>
                <option value="Approved">Approved</option>
                <option value="Rejected">Rejected</option>
                <option value="Blocked">Blocked</option>
              </select>
            </div>

            {/* Clear Filters */}
            {(searchTerm || statusFilter) && (
              <Button
                variant="ghost"
                onClick={() => {
                  setSearchTerm('');
                  setStatusFilter('');
                  setPagination({ ...pagination, pageNumber: 1 });
                }}
              >
                Clear Filters
              </Button>
            )}
          </div>
        </Card>

        {/* Table */}
        <Card padding={false}>
          {loading ? (
            <LoadingSkeleton type="table" />
          ) : caterings.length === 0 ? (
            <EmptyState
              title="No caterings found"
              description="No catering partners match your current filters"
            />
          ) : (
            <>
              <Table>
                <TableHeader>
                  <TableHeaderCell>Business Name</TableHeaderCell>
                  <TableHeaderCell>Owner</TableHeaderCell>
                  <TableHeaderCell>Location</TableHeaderCell>
                  <TableHeaderCell>Status</TableHeaderCell>
                  <TableHeaderCell>Rating</TableHeaderCell>
                  <TableHeaderCell>Orders</TableHeaderCell>
                  <TableHeaderCell>Actions</TableHeaderCell>
                </TableHeader>
                <TableBody>
                  {caterings.map((catering) => (
                    <TableRow key={catering.cateringId}>
                      <TableCell>
                        <div>
                          <p className="font-medium text-gray-900">{catering.businessName}</p>
                          <p className="text-xs text-gray-500">{catering.phone}</p>
                        </div>
                      </TableCell>
                      <TableCell>
                        <div>
                          <p className="text-sm">{catering.ownerName}</p>
                          <p className="text-xs text-gray-500">{catering.email}</p>
                        </div>
                      </TableCell>
                      <TableCell>
                        <div>
                          <p className="text-sm">{catering.city}</p>
                          <p className="text-xs text-gray-500">{catering.state}</p>
                        </div>
                      </TableCell>
                      <TableCell>
                        <Badge status={catering.status} dot />
                      </TableCell>
                      <TableCell>
                        <div className="flex items-center">
                          <span className="text-yellow-500 mr-1">★</span>
                          <span>{catering.rating?.toFixed(1) || 'N/A'}</span>
                          <span className="text-xs text-gray-500 ml-1">({catering.totalReviews})</span>
                        </div>
                      </TableCell>
                      <TableCell>{catering.totalOrders}</TableCell>
                      <TableCell>
                        <div className="flex items-center space-x-2">
                          <button
                            className="p-1.5 text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
                            title="View Details"
                          >
                            <Eye className="w-4 h-4" />
                          </button>
                          {catering.status === 'Pending' && (
                            <>
                              <button
                                onClick={() => openStatusModal(catering, 'Approved')}
                                className="p-1.5 text-green-600 hover:bg-green-50 rounded-lg transition-colors"
                                title="Approve"
                              >
                                <CheckCircle className="w-4 h-4" />
                              </button>
                              <button
                                onClick={() => openStatusModal(catering, 'Rejected')}
                                className="p-1.5 text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                                title="Reject"
                              >
                                <XCircle className="w-4 h-4" />
                              </button>
                            </>
                          )}
                          {catering.status === 'Approved' && (
                            <button
                              onClick={() => openStatusModal(catering, 'Blocked')}
                              className="p-1.5 text-orange-600 hover:bg-orange-50 rounded-lg transition-colors"
                              title="Block"
                            >
                              <Ban className="w-4 h-4" />
                            </button>
                          )}
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>

              <TablePagination
                currentPage={pagination.pageNumber}
                totalPages={pagination.totalPages}
                totalRecords={pagination.totalRecords}
                pageSize={pagination.pageSize}
                onPageChange={(page) => setPagination({ ...pagination, pageNumber: page })}
              />
            </>
          )}
        </Card>
      </div>

      {/* Status Change Modal */}
      <Modal
        isOpen={showStatusModal}
        onClose={() => setShowStatusModal(false)}
        title={`${statusAction} Catering`}
        footer={
          <>
            <Button
              variant="ghost"
              onClick={() => setShowStatusModal(false)}
            >
              Cancel
            </Button>
            <Button
              variant={statusAction === 'Approved' ? 'success' : 'danger'}
              onClick={handleStatusChange}
              loading={actionLoading}
            >
              Confirm {statusAction}
            </Button>
          </>
        }
      >
        <div className="space-y-4">
          <p>
            Are you sure you want to <strong>{statusAction.toLowerCase()}</strong> catering{' '}
            <strong>{selectedCatering?.businessName}</strong>?
          </p>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Reason (Optional)
            </label>
            <textarea
              value={statusReason}
              onChange={(e) => setStatusReason(e.target.value)}
              rows={3}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500"
              placeholder="Enter reason for this action..."
            />
          </div>
        </div>
      </Modal>
    </AdminLayout>
  );
};

export default AdminCaterings;
