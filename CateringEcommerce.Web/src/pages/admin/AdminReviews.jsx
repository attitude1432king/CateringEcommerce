import { useState, useEffect } from 'react';
import { Star, Search, RefreshCw, Eye, EyeOff, Trash2, Filter, AlertTriangle } from 'lucide-react';
import AdminLayout from '../../components/admin/layout/AdminLayout';
import { ProtectedRoute } from '../../components/admin/auth/ProtectedRoute';
import { apiCall } from '../../services/apiUtils'; // P3 FIX: Use consolidated apiUtils
import { toast } from 'react-hot-toast';
import { formatDistanceToNow } from 'date-fns';
import { useConfirmation } from '../../contexts/ConfirmationContext'; // P1 FIX: Replace window.confirm

/**
 * Admin Reviews Management Page
 *
 * Features:
 * - List all reviews with pagination
 * - Filter by rating, status, catering
 * - Search by reviewer or catering name
 * - Hide/unhide reviews (moderation)
 * - Delete inappropriate reviews
 * - View review details
 */
const AdminReviews = () => {
    const confirm = useConfirmation(); // P1 FIX: Use confirmation hook
    const [reviews, setReviews] = useState([]);
    const [loading, setLoading] = useState(true);
    const [stats, setStats] = useState({
        totalReviews: 0,
        averageRating: 0,
        hiddenReviews: 0,
        pendingModeration: 0
    });

    const [filters, setFilters] = useState({
        searchTerm: '',
        rating: null,
        isHidden: null,
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

    useEffect(() => {
        fetchReviews();
    }, [filters]);

    const fetchReviews = async () => {
        setLoading(true);
        try {
            const queryParams = new URLSearchParams({
                ...filters,
                rating: filters.rating || '',
                isHidden: filters.isHidden === null ? '' : filters.isHidden
            }).toString();

            const response = await apiCall(`/admin/reviews?${queryParams}`);

            if (response.result && response.data) {
                setReviews(response.data.reviews || []);
                setPagination({
                    totalCount: response.data.totalCount || 0,
                    totalPages: response.data.totalPages || 0,
                    currentPage: response.data.pageNumber || 1
                });

                // Calculate stats
                const total = response.data.totalCount || 0;
                const hidden = response.data.reviews?.filter(r => r.isHidden).length || 0;
                const avgRating = response.data.averageRating || 0;

                setStats({
                    totalReviews: total,
                    averageRating: avgRating,
                    hiddenReviews: hidden,
                    pendingModeration: 0 // Can be extended
                });
            } else {
                toast.error('Failed to load reviews');
            }
        } catch (error) {
            console.error('Error fetching reviews:', error);
            toast.error('Network error. Please try again.');
        } finally {
            setLoading(false);
        }
    };

    const handleToggleVisibility = async (review) => {
        const newHiddenState = !review.isHidden;
        const action = newHiddenState ? 'hide' : 'unhide';

        // P1 FIX: Replace window.confirm with confirmation context
        const confirmed = await confirm({
            title: `${action.charAt(0).toUpperCase() + action.slice(1)} Review`,
            message: `Are you sure you want to ${action} this review?`,
            type: newHiddenState ? 'warning' : 'info',
            confirmText: action.charAt(0).toUpperCase() + action.slice(1),
            cancelText: 'Cancel'
        });

        if (!confirmed) {
            return;
        }

        try {
            const response = await apiCall(`/admin/reviews/${review.reviewId}/hide`, 'PUT',
                {
                    isHidden: newHiddenState,
                    reason: newHiddenState ? 'Inappropriate content' : 'Review restored'
                });
            

            if (response.result) {
                toast.success(`Review ${action}d successfully`);
                fetchReviews(); // Refresh list
            } else {
                toast.error(response.message || `Failed to ${action} review`);
            }
        } catch (error) {
            console.error(`Error ${action}ing review:`, error);
            toast.error('Network error. Please try again.');
        }
    };

    const handleDeleteReview = async (reviewId) => {
        // P1 FIX: Replace window.confirm with confirmation context
        const confirmed = await confirm({
            title: 'Delete Review',
            message: 'Are you sure you want to permanently delete this review? This action cannot be undone.',
            type: 'delete',
            confirmText: 'Delete',
            cancelText: 'Cancel'
        });

        if (!confirmed) {
            return;
        }

        try {
            const response = await apiCall(`/admin/reviews/${reviewId}`, 'DELETE');

            if (response.result) {
                toast.success('Review deleted successfully');
                fetchReviews(); // Refresh list
            } else {
                toast.error(response.message || 'Failed to delete review');
            }
        } catch (error) {
            console.error('Error deleting review:', error);
            toast.error('Network error. Please try again.');
        }
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

    const renderStars = (rating) => {
        return (
            <div className="flex items-center gap-0.5">
                {[1, 2, 3, 4, 5].map((star) => (
                    <Star
                        key={star}
                        className={`w-4 h-4 ${
                            star <= rating
                                ? 'fill-yellow-400 text-yellow-400'
                                : 'text-gray-300'
                        }`}
                    />
                ))}
            </div>
        );
    };

    return (
        <ProtectedRoute requiredPermissions={['REVIEW_VIEW']}>
            <AdminLayout>
                <div className="p-6">
                    {/* Header */}
                    <div className="mb-6">
                        <div className="flex items-center justify-between">
                            <div>
                                <h1 className="text-2xl font-bold text-gray-900 flex items-center gap-2">
                                    <Star className="w-7 h-7 text-yellow-500" />
                                    Reviews Management
                                </h1>
                                <p className="text-gray-600 mt-1">
                                    Moderate and manage customer reviews
                                </p>
                            </div>
                            <div className="flex gap-3">
                                <button
                                    onClick={fetchReviews}
                                    className="flex items-center gap-2 px-4 py-2 text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
                                >
                                    <RefreshCw className="w-4 h-4" />
                                    Refresh
                                </button>
                            </div>
                        </div>

                        {/* Stats Cards */}
                        <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mt-6">
                            <div className="bg-white p-4 rounded-lg border border-gray-200">
                                <div className="text-sm text-gray-600">Total Reviews</div>
                                <div className="text-2xl font-bold text-gray-900 mt-1">{stats.totalReviews}</div>
                            </div>
                            <div className="bg-yellow-50 p-4 rounded-lg border border-yellow-200">
                                <div className="text-sm text-yellow-700">Average Rating</div>
                                <div className="text-2xl font-bold text-yellow-900 mt-1 flex items-center gap-2">
                                    {stats.averageRating.toFixed(1)}
                                    <Star className="w-5 h-5 fill-yellow-400 text-yellow-400" />
                                </div>
                            </div>
                            <div className="bg-gray-50 p-4 rounded-lg border border-gray-200">
                                <div className="text-sm text-gray-600">Hidden Reviews</div>
                                <div className="text-2xl font-bold text-gray-900 mt-1">{stats.hiddenReviews}</div>
                            </div>
                            <div className="bg-blue-50 p-4 rounded-lg border border-blue-200">
                                <div className="text-sm text-blue-700">Pending Moderation</div>
                                <div className="text-2xl font-bold text-blue-900 mt-1">{stats.pendingModeration}</div>
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
                                        placeholder="Search by reviewer or catering name..."
                                        value={filters.searchTerm}
                                        onChange={(e) => handleFilterChange('searchTerm', e.target.value)}
                                        className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                                    />
                                </div>
                            </div>

                            {/* Rating Filter */}
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">
                                    Rating
                                </label>
                                <select
                                    value={filters.rating || ''}
                                    onChange={(e) => handleFilterChange('rating', e.target.value ? parseInt(e.target.value) : null)}
                                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                                >
                                    <option value="">All Ratings</option>
                                    <option value="5">5 Stars</option>
                                    <option value="4">4 Stars</option>
                                    <option value="3">3 Stars</option>
                                    <option value="2">2 Stars</option>
                                    <option value="1">1 Star</option>
                                </select>
                            </div>

                            {/* Visibility Filter */}
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">
                                    Status
                                </label>
                                <select
                                    value={filters.isHidden === null ? '' : filters.isHidden.toString()}
                                    onChange={(e) => handleFilterChange('isHidden', e.target.value === '' ? null : e.target.value === 'true')}
                                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                                >
                                    <option value="">All Reviews</option>
                                    <option value="false">Visible</option>
                                    <option value="true">Hidden</option>
                                </select>
                            </div>
                        </div>
                    </div>

                    {/* Reviews Table */}
                    <div className="bg-white rounded-lg border border-gray-200 overflow-hidden">
                        <div className="overflow-x-auto">
                            <table className="min-w-full divide-y divide-gray-200">
                                <thead className="bg-gray-50">
                                    <tr>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                            Review
                                        </th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                            Catering
                                        </th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                            Rating
                                        </th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                            Status
                                        </th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                            Created
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
                                    ) : reviews.length === 0 ? (
                                        <tr>
                                            <td colSpan="6" className="px-6 py-12 text-center text-gray-500">
                                                No reviews found
                                            </td>
                                        </tr>
                                    ) : (
                                        reviews.map((review) => (
                                            <tr key={review.reviewId} className="hover:bg-gray-50">
                                                <td className="px-6 py-4">
                                                    <div className="max-w-xs">
                                                        <div className="text-sm font-medium text-gray-900">
                                                            {review.reviewerName || 'Anonymous'}
                                                        </div>
                                                        <div className="text-sm text-gray-500 mt-1 line-clamp-2">
                                                            {review.reviewText || 'No comment'}
                                                        </div>
                                                    </div>
                                                </td>
                                                <td className="px-6 py-4 whitespace-nowrap">
                                                    <div className="text-sm text-gray-900">{review.cateringName || 'N/A'}</div>
                                                </td>
                                                <td className="px-6 py-4 whitespace-nowrap">
                                                    <div className="flex items-center gap-2">
                                                        {renderStars(review.rating || 0)}
                                                        <span className="text-sm font-medium text-gray-700">
                                                            {review.rating || 0}
                                                        </span>
                                                    </div>
                                                </td>
                                                <td className="px-6 py-4 whitespace-nowrap">
                                                    {review.isHidden ? (
                                                        <span className="inline-flex items-center gap-1 px-2 py-1 text-xs font-semibold rounded-full bg-red-100 text-red-800">
                                                            <EyeOff className="w-3 h-3" />
                                                            Hidden
                                                        </span>
                                                    ) : (
                                                        <span className="inline-flex items-center gap-1 px-2 py-1 text-xs font-semibold rounded-full bg-green-100 text-green-800">
                                                            <Eye className="w-3 h-3" />
                                                            Visible
                                                        </span>
                                                    )}
                                                </td>
                                                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                                    {review.createdDate ? formatDistanceToNow(new Date(review.createdDate), { addSuffix: true }) : 'N/A'}
                                                </td>
                                                <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                                                    <div className="flex items-center justify-end gap-2">
                                                        <button
                                                            onClick={() => handleToggleVisibility(review)}
                                                            className={`${
                                                                review.isHidden
                                                                    ? 'text-green-600 hover:text-green-900'
                                                                    : 'text-orange-600 hover:text-orange-900'
                                                            }`}
                                                            title={review.isHidden ? 'Unhide' : 'Hide'}
                                                        >
                                                            {review.isHidden ? <Eye className="w-4 h-4" /> : <EyeOff className="w-4 h-4" />}
                                                        </button>
                                                        <button
                                                            onClick={() => handleDeleteReview(review.reviewId)}
                                                            className="text-red-600 hover:text-red-900"
                                                            title="Delete"
                                                        >
                                                            <Trash2 className="w-4 h-4" />
                                                        </button>
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
                                            {/* P1 FIX: Limit pagination buttons to max 5 to prevent UI overflow */}
                                            {[...Array(Math.min(5, pagination.totalPages))].map((_, index) => (
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
            </AdminLayout>
        </ProtectedRoute>
    );
};

export default AdminReviews;
