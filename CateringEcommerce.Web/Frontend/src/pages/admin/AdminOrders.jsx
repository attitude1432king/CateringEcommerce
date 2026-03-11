import { useState, useEffect } from 'react';
import { ShoppingCart, Search, RefreshCw, Eye, Filter, Calendar, DollarSign, Package, Download, Edit } from 'lucide-react';
import AdminLayout from '../../components/admin/layout/AdminLayout';
import { ProtectedRoute } from '../../components/admin/auth/ProtectedRoute';
import { apiCall } from '../../services/apiUtils'; // P3 FIX: Use consolidated apiUtils
import { toast } from 'react-hot-toast';
import { format } from 'date-fns';

/**
 * Admin Orders Management Page
 *
 * Features:
 * - Paginated order listing
 * - Advanced filtering (status, payment, date range, search)
 * - Order statistics dashboard
 * - Update order status
 * - View order details
 * - Export orders to CSV
 */
const AdminOrders = () => {
    const [orders, setOrders] = useState([]);
    const [loading, setLoading] = useState(true);
    const [stats, setStats] = useState({
        totalOrders: 0,
        pendingOrders: 0,
        confirmedOrders: 0,
        inProgressOrders: 0,
        completedOrders: 0,
        cancelledOrders: 0,
        totalRevenue: 0
    });

    const [filters, setFilters] = useState({
        pageNumber: 1,
        pageSize: 20,
        searchTerm: '',
        orderStatus: '',
        paymentStatus: '',
        startDate: '',
        endDate: '',
        sortBy: 'CreatedDate',
        sortOrder: 'DESC'
    });

    const [pagination, setPagination] = useState({
        totalCount: 0,
        totalPages: 0,
        currentPage: 1
    });

    // P1 FIX: Order details modal state
    const [selectedOrder, setSelectedOrder] = useState(null);
    const [showOrderDetails, setShowOrderDetails] = useState(false);
    const [loadingDetails, setLoadingDetails] = useState(false);

    useEffect(() => {
        fetchOrders();
        fetchStats();
    }, [filters]);

    const fetchOrders = async () => {
        setLoading(true);
        try {
            const queryParams = new URLSearchParams({
                ...filters,
                orderStatus: filters.orderStatus || '',
                paymentStatus: filters.paymentStatus || '',
                startDate: filters.startDate || '',
                endDate: filters.endDate || ''
            }).toString();

            const response = await apiCall(`/admin/orders?${queryParams}`);

            if (response.result && response.data) {
                setOrders(response.data.orders || []);
                setPagination({
                    totalCount: response.data.totalCount || 0,
                    totalPages: response.data.totalPages || 0,
                    currentPage: response.data.pageNumber || 1
                });
            } else {
                toast.error('Failed to load orders');
            }
        } catch (error) {
            console.error('Error fetching orders:', error);
            toast.error('Network error. Please try again.');
        } finally {
            setLoading(false);
        }
    };

    const fetchStats = async () => {
        try {
            const response = await apiCall('/admin/orders/stats');

            if (response.result && response.data) {
                setStats({
                    totalOrders: response.data.totalOrders || 0,
                    pendingOrders: response.data.pendingOrders || 0,
                    confirmedOrders: response.data.confirmedOrders || 0,
                    inProgressOrders: response.data.inProgressOrders || 0,
                    completedOrders: response.data.completedOrders || 0,
                    cancelledOrders: response.data.cancelledOrders || 0,
                    totalRevenue: response.data.totalRevenue || 0
                });
            }
        } catch (error) {
            console.error('Error fetching stats:', error);
        }
    };

    // P1 FIX: Fetch order details
    const fetchOrderDetails = async (orderId) => {
        setLoadingDetails(true);
        try {
            const response = await apiCall(`/admin/orders/${orderId}`);

            if (response.result && response.data) {
                setSelectedOrder(response.data);
                setShowOrderDetails(true);
            } else {
                toast.error('Failed to load order details');
            }
        } catch (error) {
            console.error('Error fetching order details:', error);
            toast.error('Network error loading order details');
        } finally {
            setLoadingDetails(false);
        }
    };

    const handleFilterChange = (key, value) => {
        setFilters({
            ...filters,
            [key]: value,
            pageNumber: 1
        });
    };

    const handlePageChange = (newPage) => {
        setFilters({
            ...filters,
            pageNumber: newPage
        });
    };

    const handleExport = async () => {
        try {
            const queryParams = new URLSearchParams({
                ...filters,
                orderStatus: filters.orderStatus || '',
                paymentStatus: filters.paymentStatus || ''
            }).toString();

            const response = await fetch(`${import.meta.env.VITE_API_BASE_URL || 'https://localhost:44368'}/api/admin/orders/export?${queryParams}`, {
                credentials: 'include'
            });

            if (response.ok) {
                const blob = await response.blob();
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = `Orders_${new Date().toISOString().split('T')[0]}.csv`;
                document.body.appendChild(a);
                a.click();
                window.URL.revokeObjectURL(url);
                document.body.removeChild(a);
                toast.success('Orders exported successfully');
            } else {
                toast.error('Failed to export orders');
            }
        } catch (error) {
            console.error('Error exporting orders:', error);
            toast.error('Network error. Please try again.');
        }
    };

    const getStatusBadge = (status) => {
        const statusConfig = {
            'Pending': { bg: 'bg-yellow-100', text: 'text-yellow-800', label: 'Pending' },
            'Confirmed': { bg: 'bg-blue-100', text: 'text-blue-800', label: 'Confirmed' },
            'InProgress': { bg: 'bg-purple-100', text: 'text-purple-800', label: 'In Progress' },
            'Completed': { bg: 'bg-green-100', text: 'text-green-800', label: 'Completed' },
            'Cancelled': { bg: 'bg-red-100', text: 'text-red-800', label: 'Cancelled' },
        };

        const config = statusConfig[status] || { bg: 'bg-gray-100', text: 'text-gray-800', label: status };

        return (
            <span className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${config.bg} ${config.text}`}>
                {config.label}
            </span>
        );
    };

    const getPaymentStatusBadge = (status) => {
        const statusConfig = {
            'Pending': { bg: 'bg-yellow-100', text: 'text-yellow-800' },
            'AdvancePaid': { bg: 'bg-blue-100', text: 'text-blue-800', label: 'Advance Paid' },
            'Advance Paid': { bg: 'bg-blue-100', text: 'text-blue-800', label: 'Advance Paid' },
            'FullyPaid': { bg: 'bg-green-100', text: 'text-green-800', label: 'Fully Paid' },
            'Fully Paid': { bg: 'bg-green-100', text: 'text-green-800', label: 'Fully Paid' },
            'Refunded': { bg: 'bg-red-100', text: 'text-red-800' },
        };

        const config = statusConfig[status] || { bg: 'bg-gray-100', text: 'text-gray-800', label: status };
        const label = config.label || status;

        return (
            <span className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${config.bg} ${config.text}`}>
                {label}
            </span>
        );
    };

    const formatCurrency = (amount) => {
        return new Intl.NumberFormat('en-IN', {
            style: 'currency',
            currency: 'INR',
            minimumFractionDigits: 0
        }).format(amount || 0);
    };

    return (
        <ProtectedRoute requiredPermissions={['ORDER_VIEW']}>
            <AdminLayout>
                <div className="p-6">
                    {/* Header */}
                    <div className="mb-6">
                        <div className="flex items-center justify-between">
                            <div>
                                <h1 className="text-2xl font-bold text-gray-900 flex items-center gap-2">
                                    <ShoppingCart className="w-7 h-7 text-blue-600" />
                                    Orders Management
                                </h1>
                                <p className="text-gray-600 mt-1">
                                    Monitor and manage customer orders
                                </p>
                            </div>
                            <div className="flex gap-3">
                                <button
                                    onClick={fetchOrders}
                                    className="flex items-center gap-2 px-4 py-2 text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
                                >
                                    <RefreshCw className="w-4 h-4" />
                                    Refresh
                                </button>
                                <button
                                    onClick={handleExport}
                                    className="flex items-center gap-2 px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors"
                                >
                                    <Download className="w-4 h-4" />
                                    Export CSV
                                </button>
                            </div>
                        </div>

                        {/* Stats Cards */}
                        <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mt-6">
                            <div className="bg-white p-4 rounded-lg border border-gray-200">
                                <div className="flex items-center justify-between mb-2">
                                    <span className="text-sm text-gray-600">Total Orders</span>
                                    <Package className="w-5 h-5 text-gray-400" />
                                </div>
                                <div className="text-2xl font-bold text-gray-900">{stats.totalOrders}</div>
                                <div className="text-xs text-gray-500 mt-1">All time</div>
                            </div>

                            <div className="bg-yellow-50 p-4 rounded-lg border border-yellow-200">
                                <div className="flex items-center justify-between mb-2">
                                    <span className="text-sm text-yellow-700">Pending</span>
                                    <Calendar className="w-5 h-5 text-yellow-500" />
                                </div>
                                <div className="text-2xl font-bold text-yellow-900">{stats.pendingOrders}</div>
                                <div className="text-xs text-yellow-600 mt-1">Awaiting confirmation</div>
                            </div>

                            <div className="bg-blue-50 p-4 rounded-lg border border-blue-200">
                                <div className="flex items-center justify-between mb-2">
                                    <span className="text-sm text-blue-700">Active</span>
                                    <ShoppingCart className="w-5 h-5 text-blue-500" />
                                </div>
                                <div className="text-2xl font-bold text-blue-900">
                                    {stats.confirmedOrders + stats.inProgressOrders}
                                </div>
                                <div className="text-xs text-blue-600 mt-1">Confirmed + In Progress</div>
                            </div>

                            <div className="bg-green-50 p-4 rounded-lg border border-green-200">
                                <div className="flex items-center justify-between mb-2">
                                    <span className="text-sm text-green-700">Revenue</span>
                                    <DollarSign className="w-5 h-5 text-green-500" />
                                </div>
                                <div className="text-2xl font-bold text-green-900">
                                    {formatCurrency(stats.totalRevenue)}
                                </div>
                                <div className="text-xs text-green-600 mt-1">Total revenue</div>
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
                                        placeholder="Search by order number, customer, catering..."
                                        value={filters.searchTerm}
                                        onChange={(e) => handleFilterChange('searchTerm', e.target.value)}
                                        className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                                    />
                                </div>
                            </div>

                            {/* Order Status Filter */}
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">
                                    Order Status
                                </label>
                                <select
                                    value={filters.orderStatus}
                                    onChange={(e) => handleFilterChange('orderStatus', e.target.value)}
                                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                                >
                                    <option value="">All Status</option>
                                    <option value="Pending">Pending</option>
                                    <option value="Confirmed">Confirmed</option>
                                    <option value="InProgress">In Progress</option>
                                    <option value="Completed">Completed</option>
                                    <option value="Cancelled">Cancelled</option>
                                </select>
                            </div>

                            {/* Payment Status Filter */}
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">
                                    Payment Status
                                </label>
                                <select
                                    value={filters.paymentStatus}
                                    onChange={(e) => handleFilterChange('paymentStatus', e.target.value)}
                                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                                >
                                    <option value="">All Payments</option>
                                    <option value="Pending">Pending</option>
                                    <option value="Advance Paid">Advance Paid</option>
                                    <option value="Fully Paid">Fully Paid</option>
                                    <option value="Refunded">Refunded</option>
                                </select>
                            </div>

                            {/* P1 FIX: Date Range Filter */}
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">
                                    Start Date
                                </label>
                                <input
                                    type="date"
                                    value={filters.startDate || ''}
                                    onChange={(e) => handleFilterChange('startDate', e.target.value)}
                                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                                />
                            </div>

                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">
                                    End Date
                                </label>
                                <input
                                    type="date"
                                    value={filters.endDate || ''}
                                    onChange={(e) => handleFilterChange('endDate', e.target.value)}
                                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                                />
                            </div>
                        </div>
                    </div>

                    {/* Orders Table */}
                    <div className="bg-white rounded-lg border border-gray-200 overflow-hidden">
                        <div className="overflow-x-auto">
                            <table className="min-w-full divide-y divide-gray-200">
                                <thead className="bg-gray-50">
                                    <tr>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                            Order Details
                                        </th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                            Customer
                                        </th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                            Catering
                                        </th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                            Event Date
                                        </th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                            Amount
                                        </th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                            Status
                                        </th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                            Payment
                                        </th>
                                        <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                                            Actions
                                        </th>
                                    </tr>
                                </thead>
                                <tbody className="bg-white divide-y divide-gray-200">
                                    {loading ? (
                                        <tr>
                                            <td colSpan="8" className="px-6 py-12 text-center">
                                                <div className="flex justify-center">
                                                    <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
                                                </div>
                                            </td>
                                        </tr>
                                    ) : orders.length === 0 ? (
                                        <tr>
                                            <td colSpan="8" className="px-6 py-12 text-center text-gray-500">
                                                No orders found
                                            </td>
                                        </tr>
                                    ) : (
                                        orders.map((order) => (
                                            <tr key={order.orderId} className="hover:bg-gray-50">
                                                <td className="px-6 py-4 whitespace-nowrap">
                                                    <div className="text-sm font-medium text-gray-900">
                                                        #{order.orderNumber || order.orderId}
                                                    </div>
                                                    <div className="text-xs text-gray-500">
                                                        {order.createdDate ? format(new Date(order.createdDate), 'MMM dd, yyyy') : 'N/A'}
                                                    </div>
                                                </td>
                                                <td className="px-6 py-4 whitespace-nowrap">
                                                    <div className="text-sm text-gray-900">{order.userName || 'N/A'}</div>
                                                    <div className="text-xs text-gray-500">{order.userEmail || order.userPhone || ''}</div>
                                                </td>
                                                <td className="px-6 py-4">
                                                    <div className="text-sm text-gray-900 max-w-xs truncate">
                                                        {order.cateringName || 'N/A'}
                                                    </div>
                                                </td>
                                                <td className="px-6 py-4 whitespace-nowrap">
                                                    <div className="text-sm text-gray-900">
                                                        {order.eventDate ? format(new Date(order.eventDate), 'MMM dd, yyyy') : 'N/A'}
                                                    </div>
                                                    <div className="text-xs text-gray-500">{order.guestCount} guests</div>
                                                </td>
                                                <td className="px-6 py-4 whitespace-nowrap">
                                                    <div className="text-sm font-semibold text-gray-900">
                                                        {formatCurrency(order.totalAmount)}
                                                    </div>
                                                </td>
                                                <td className="px-6 py-4 whitespace-nowrap">
                                                    {getStatusBadge(order.orderStatus)}
                                                </td>
                                                <td className="px-6 py-4 whitespace-nowrap">
                                                    {getPaymentStatusBadge(order.paymentStatus)}
                                                </td>
                                                <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                                                    <button
                                                        onClick={() => fetchOrderDetails(order.orderId)}
                                                        className="text-blue-600 hover:text-blue-900 transition-colors"
                                                        title="View Details"
                                                        disabled={loadingDetails}
                                                    >
                                                        <Eye className="w-4 h-4" />
                                                    </button>
                                                </td>
                                            </tr>
                                        ))
                                    )}
                                </tbody>
                            </table>
                        </div>

                        {/* Pagination */}
                        {pagination.totalPages > 1 && (
                            <div className="bg-white px-4 py-3 flex items-center justify-between border-t border-gray-200">
                                <div className="flex-1 flex justify-between sm:hidden">
                                    <button
                                        onClick={() => handlePageChange(pagination.currentPage - 1)}
                                        disabled={pagination.currentPage === 1}
                                        className="relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50"
                                    >
                                        Previous
                                    </button>
                                    <button
                                        onClick={() => handlePageChange(pagination.currentPage + 1)}
                                        disabled={pagination.currentPage === pagination.totalPages}
                                        className="ml-3 relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50"
                                    >
                                        Next
                                    </button>
                                </div>
                                <div className="hidden sm:flex-1 sm:flex sm:items-center sm:justify-between">
                                    <div>
                                        <p className="text-sm text-gray-700">
                                            Showing <span className="font-medium">{(pagination.currentPage - 1) * filters.pageSize + 1}</span> to{' '}
                                            <span className="font-medium">{Math.min(pagination.currentPage * filters.pageSize, pagination.totalCount)}</span> of{' '}
                                            <span className="font-medium">{pagination.totalCount}</span> results
                                        </p>
                                    </div>
                                    <div>
                                        <nav className="relative z-0 inline-flex rounded-md shadow-sm -space-x-px">
                                            <button
                                                onClick={() => handlePageChange(pagination.currentPage - 1)}
                                                disabled={pagination.currentPage === 1}
                                                className="relative inline-flex items-center px-2 py-2 rounded-l-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50 disabled:opacity-50"
                                            >
                                                Previous
                                            </button>
                                            {[...Array(Math.min(5, pagination.totalPages))].map((_, index) => {
                                                const pageNum = index + 1;
                                                return (
                                                    <button
                                                        key={pageNum}
                                                        onClick={() => handlePageChange(pageNum)}
                                                        className={`relative inline-flex items-center px-4 py-2 border text-sm font-medium ${
                                                            pagination.currentPage === pageNum
                                                                ? 'z-10 bg-blue-50 border-blue-500 text-blue-600'
                                                                : 'bg-white border-gray-300 text-gray-500 hover:bg-gray-50'
                                                        }`}
                                                    >
                                                        {pageNum}
                                                    </button>
                                                );
                                            })}
                                            <button
                                                onClick={() => handlePageChange(pagination.currentPage + 1)}
                                                disabled={pagination.currentPage === pagination.totalPages}
                                                className="relative inline-flex items-center px-2 py-2 rounded-r-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50 disabled:opacity-50"
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

                {/* P1 FIX: Order Details Modal */}
                {showOrderDetails && selectedOrder && (
                    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
                        <div className="bg-white rounded-lg max-w-4xl w-full max-h-[90vh] overflow-y-auto shadow-2xl">
                            {/* Modal Header */}
                            <div className="sticky top-0 bg-gradient-to-r from-blue-600 to-indigo-600 text-white px-6 py-4 flex items-center justify-between">
                                <div>
                                    <h2 className="text-2xl font-bold">Order Details</h2>
                                    <p className="text-sm text-blue-100">#{selectedOrder.orderNumber}</p>
                                </div>
                                <button
                                    onClick={() => setShowOrderDetails(false)}
                                    className="text-white hover:bg-white/20 rounded-lg p-2 transition-colors"
                                >
                                    <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M6 18L18 6M6 6l12 12" />
                                    </svg>
                                </button>
                            </div>

                            {/* Modal Body */}
                            <div className="p-6 space-y-6">
                                {/* Order Summary */}
                                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                    <div className="bg-gray-50 p-4 rounded-lg">
                                        <h3 className="text-sm font-semibold text-gray-600 mb-2">Customer Information</h3>
                                        <p className="text-lg font-bold text-gray-900">{selectedOrder.userName || 'N/A'}</p>
                                        <p className="text-sm text-gray-600">{selectedOrder.userEmail}</p>
                                        <p className="text-sm text-gray-600">{selectedOrder.userPhone}</p>
                                    </div>
                                    <div className="bg-gray-50 p-4 rounded-lg">
                                        <h3 className="text-sm font-semibold text-gray-600 mb-2">Catering Provider</h3>
                                        <p className="text-lg font-bold text-gray-900">{selectedOrder.cateringName || 'N/A'}</p>
                                        <p className="text-sm text-gray-600">{selectedOrder.vendorEmail}</p>
                                        <p className="text-sm text-gray-600">{selectedOrder.vendorPhone}</p>
                                    </div>
                                </div>

                                {/* Event Details */}
                                <div className="border-t pt-4">
                                    <h3 className="text-lg font-bold text-gray-900 mb-3">Event Details</h3>
                                    <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
                                        <div>
                                            <p className="text-sm text-gray-600">Event Date</p>
                                            <p className="font-semibold text-gray-900">
                                                {selectedOrder.eventDate ? format(new Date(selectedOrder.eventDate), 'MMM dd, yyyy') : 'N/A'}
                                            </p>
                                        </div>
                                        <div>
                                            <p className="text-sm text-gray-600">Guest Count</p>
                                            <p className="font-semibold text-gray-900">{selectedOrder.guestCount} guests</p>
                                        </div>
                                        <div>
                                            <p className="text-sm text-gray-600">Event Type</p>
                                            <p className="font-semibold text-gray-900">{selectedOrder.eventType || 'N/A'}</p>
                                        </div>
                                        {selectedOrder.eventLocation && (
                                            <div className="col-span-2 md:col-span-3">
                                                <p className="text-sm text-gray-600">Location</p>
                                                <p className="font-semibold text-gray-900">{selectedOrder.eventLocation}</p>
                                            </div>
                                        )}
                                    </div>
                                </div>

                                {/* Order Status */}
                                <div className="border-t pt-4">
                                    <h3 className="text-lg font-bold text-gray-900 mb-3">Order Status</h3>
                                    <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                                        <div>
                                            <p className="text-sm text-gray-600">Order Status</p>
                                            <span className={`inline-block mt-1 px-3 py-1 rounded-full text-xs font-semibold ${
                                                selectedOrder.orderStatus === 'Completed' ? 'bg-green-100 text-green-800' :
                                                selectedOrder.orderStatus === 'InProgress' ? 'bg-blue-100 text-blue-800' :
                                                selectedOrder.orderStatus === 'Cancelled' ? 'bg-red-100 text-red-800' :
                                                'bg-yellow-100 text-yellow-800'
                                            }`}>
                                                {selectedOrder.orderStatus}
                                            </span>
                                        </div>
                                        <div>
                                            <p className="text-sm text-gray-600">Payment Status</p>
                                            <span className={`inline-block mt-1 px-3 py-1 rounded-full text-xs font-semibold ${
                                                selectedOrder.paymentStatus === 'Paid' ? 'bg-green-100 text-green-800' :
                                                selectedOrder.paymentStatus === 'PartiallyPaid' ? 'bg-yellow-100 text-yellow-800' :
                                                'bg-red-100 text-red-800'
                                            }`}>
                                                {selectedOrder.paymentStatus}
                                            </span>
                                        </div>
                                        <div>
                                            <p className="text-sm text-gray-600">Payment Method</p>
                                            <p className="font-semibold text-gray-900">{selectedOrder.paymentMethod || 'N/A'}</p>
                                        </div>
                                        <div>
                                            <p className="text-sm text-gray-600">Total Amount</p>
                                            <p className="font-bold text-lg text-blue-600">₹{selectedOrder.totalAmount?.toFixed(2)}</p>
                                        </div>
                                    </div>
                                </div>

                                {/* Order Items */}
                                {selectedOrder.orderItems && selectedOrder.orderItems.length > 0 && (
                                    <div className="border-t pt-4">
                                        <h3 className="text-lg font-bold text-gray-900 mb-3">Order Items</h3>
                                        <div className="bg-gray-50 rounded-lg overflow-hidden">
                                            <table className="min-w-full divide-y divide-gray-200">
                                                <thead className="bg-gray-100">
                                                    <tr>
                                                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Item</th>
                                                        <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase">Quantity</th>
                                                        <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase">Price</th>
                                                        <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase">Total</th>
                                                    </tr>
                                                </thead>
                                                <tbody className="bg-white divide-y divide-gray-200">
                                                    {selectedOrder.orderItems.map((item, index) => (
                                                        <tr key={index}>
                                                            <td className="px-4 py-3 text-sm text-gray-900">{item.itemName}</td>
                                                            <td className="px-4 py-3 text-sm text-gray-900 text-center">{item.quantity}</td>
                                                            <td className="px-4 py-3 text-sm text-gray-900 text-right">₹{item.unitPrice?.toFixed(2)}</td>
                                                            <td className="px-4 py-3 text-sm font-semibold text-gray-900 text-right">₹{item.totalPrice?.toFixed(2)}</td>
                                                        </tr>
                                                    ))}
                                                </tbody>
                                            </table>
                                        </div>
                                    </div>
                                )}

                                {/* Payment Stages */}
                                {selectedOrder.paymentStages && selectedOrder.paymentStages.length > 0 && (
                                    <div className="border-t pt-4">
                                        <h3 className="text-lg font-bold text-gray-900 mb-3">Payment Stages</h3>
                                        <div className="space-y-2">
                                            {selectedOrder.paymentStages.map((stage, index) => (
                                                <div key={index} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                                                    <div>
                                                        <p className="font-semibold text-gray-900">{stage.stageName}</p>
                                                        <p className="text-xs text-gray-600">
                                                            Due: {stage.dueDate ? format(new Date(stage.dueDate), 'MMM dd, yyyy') : 'N/A'}
                                                        </p>
                                                    </div>
                                                    <div className="text-right">
                                                        <p className="font-bold text-gray-900">₹{stage.amount?.toFixed(2)}</p>
                                                        <span className={`text-xs px-2 py-1 rounded ${
                                                            stage.isPaid ? 'bg-green-100 text-green-800' : 'bg-yellow-100 text-yellow-800'
                                                        }`}>
                                                            {stage.isPaid ? 'Paid' : 'Pending'}
                                                        </span>
                                                    </div>
                                                </div>
                                            ))}
                                        </div>
                                    </div>
                                )}

                                {/* Additional Notes */}
                                {selectedOrder.notes && (
                                    <div className="border-t pt-4">
                                        <h3 className="text-lg font-bold text-gray-900 mb-2">Notes</h3>
                                        <p className="text-sm text-gray-700 bg-yellow-50 p-3 rounded-lg">{selectedOrder.notes}</p>
                                    </div>
                                )}

                                {/* Timestamps */}
                                <div className="border-t pt-4">
                                    <h3 className="text-lg font-bold text-gray-900 mb-3">Timeline</h3>
                                    <div className="grid grid-cols-2 md:grid-cols-3 gap-4 text-sm">
                                        <div>
                                            <p className="text-gray-600">Created</p>
                                            <p className="font-semibold text-gray-900">
                                                {selectedOrder.createdDate ? format(new Date(selectedOrder.createdDate), 'MMM dd, yyyy HH:mm') : 'N/A'}
                                            </p>
                                        </div>
                                        {selectedOrder.modifiedDate && (
                                            <div>
                                                <p className="text-gray-600">Last Modified</p>
                                                <p className="font-semibold text-gray-900">
                                                    {format(new Date(selectedOrder.modifiedDate), 'MMM dd, yyyy HH:mm')}
                                                </p>
                                            </div>
                                        )}
                                    </div>
                                </div>
                            </div>

                            {/* Modal Footer */}
                            <div className="sticky bottom-0 bg-gray-50 px-6 py-4 flex justify-end gap-3 border-t">
                                <button
                                    onClick={() => setShowOrderDetails(false)}
                                    className="px-4 py-2 bg-gray-200 text-gray-800 rounded-lg hover:bg-gray-300 transition-colors font-medium"
                                >
                                    Close
                                </button>
                            </div>
                        </div>
                    </div>
                )}
            </AdminLayout>
        </ProtectedRoute>
    );
};

export default AdminOrders;
