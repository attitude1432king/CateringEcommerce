import { Eye, MapPin, Calendar, Phone, Mail, FileText } from 'lucide-react';
import PartnerStatusBadge from './PartnerStatusBadge';
import LoadingSkeleton from '../ui/LoadingSkeleton';
import EmptyState from '../ui/EmptyState';

/**
 * Partner Requests Table Component
 *
 * Displays list of partner registration requests in a table format
 */
const PartnerRequestsTable = ({
    requests,
    loading,
    onViewDetails,
    onRefresh,
    currentPage,
    pageSize,
    totalRecords,
    onPageChange
}) => {
    const formatDate = (dateString) => {
        return new Date(dateString).toLocaleDateString('en-IN', {
            day: '2-digit',
            month: 'short',
            year: 'numeric'
        });
    };

    const totalPages = Math.ceil(totalRecords / pageSize);

    if (loading) {
        return <LoadingSkeleton type="table" rows={5} />;
    }

    if (!requests || requests.length === 0) {
        return (
            <EmptyState
                icon={FileText}
                title="No Partner Requests"
                description="No partner registration requests found matching your filters."
                action={
                    <button
                        onClick={onRefresh}
                        className="inline-flex items-center px-4 py-2 bg-indigo-600 text-white text-sm font-medium rounded-lg hover:bg-indigo-700 transition-colors"
                    >
                        Clear Filters
                    </button>
                }
            />
        );
    }

    return (
        <div className="bg-white rounded-lg border border-gray-200 overflow-hidden">
            {/* Table */}
            <div className="overflow-x-auto">
                <table className="min-w-full divide-y divide-gray-200">
                    <thead className="bg-gray-50">
                        <tr>
                            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                Request Details
                            </th>
                            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                Owner Information
                            </th>
                            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                Location
                            </th>
                            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                Status
                            </th>
                            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                Submitted
                            </th>
                            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                Actions
                            </th>
                        </tr>
                    </thead>
                    <tbody className="bg-white divide-y divide-gray-200">
                        {requests.map((request) => (
                            <tr
                                key={request.requestId}
                                className="hover:bg-gray-50 transition-colors"
                            >
                                {/* Request Details */}
                                <td className="px-6 py-4">
                                    <div className="flex items-start space-x-3">
                                        {/* Logo/Icon */}
                                        <div className="flex-shrink-0">
                                            <div className="w-12 h-12 bg-gradient-to-br from-indigo-500 to-purple-600 rounded-lg flex items-center justify-center text-white font-bold text-lg">
                                                {request.businessName.charAt(0)}
                                            </div>
                                        </div>

                                        {/* Business Info */}
                                        <div className="flex-1 min-w-0">
                                            <div className="flex items-center space-x-2">
                                                <p className="text-sm font-semibold text-gray-900 truncate">
                                                    {request.businessName}
                                                </p>
                                                {request.hasUnreadDocuments && (
                                                    <span className="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-red-100 text-red-800">
                                                        New
                                                    </span>
                                                )}
                                            </div>
                                            <p className="text-xs text-gray-500 mt-1">
                                                ID: {request.requestNumber}
                                            </p>
                                            <div className="flex items-center space-x-3 mt-1 text-xs text-gray-500">
                                                <span className="flex items-center">
                                                    <FileText className="w-3 h-3 mr-1" />
                                                    {request.documentCount} docs
                                                </span>
                                                {request.photoCount > 0 && (
                                                    <span className="flex items-center">
                                                        📷 {request.photoCount} photos
                                                    </span>
                                                )}
                                            </div>
                                        </div>
                                    </div>
                                </td>

                                {/* Owner Information */}
                                <td className="px-6 py-4">
                                    <div className="text-sm">
                                        <p className="font-medium text-gray-900">
                                            {request.ownerName}
                                        </p>
                                        <div className="flex items-center text-gray-500 mt-1">
                                            <Phone className="w-3 h-3 mr-1" />
                                            <span className="text-xs">{request.phone}</span>
                                        </div>
                                        <div className="flex items-center text-gray-500 mt-1">
                                            <Mail className="w-3 h-3 mr-1" />
                                            <span className="text-xs truncate" title={request.email}>
                                                {request.email}
                                            </span>
                                        </div>
                                    </div>
                                </td>

                                {/* Location */}
                                <td className="px-6 py-4">
                                    <div className="flex items-start text-sm text-gray-600">
                                        <MapPin className="w-4 h-4 mr-1.5 flex-shrink-0 mt-0.5" />
                                        <div>
                                            <div className="font-medium text-gray-900">
                                                {request.city}
                                            </div>
                                            <div className="text-xs text-gray-500">
                                                {request.state}
                                            </div>
                                        </div>
                                    </div>
                                </td>

                                {/* Status */}
                                <td className="px-6 py-4 whitespace-nowrap">
                                    <PartnerStatusBadge
                                        statusId={request.approvalStatusId}
                                        statusName={request.approvalStatusName}
                                    />
                                </td>

                                {/* Submitted Date */}
                                <td className="px-6 py-4 whitespace-nowrap">
                                    <div className="flex items-center text-sm text-gray-600">
                                        <Calendar className="w-4 h-4 mr-1.5" />
                                        {formatDate(request.registrationDate)}
                                    </div>
                                </td>

                                {/* Actions */}
                                <td className="px-6 py-4 whitespace-nowrap">
                                    <button
                                        onClick={() => onViewDetails(request)}
                                        className="inline-flex items-center px-3 py-1.5 bg-indigo-600 text-white text-sm font-medium rounded-lg hover:bg-indigo-700 transition-colors"
                                    >
                                        <Eye className="w-4 h-4 mr-1.5" />
                                        View Details
                                    </button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>

            {/* Pagination */}
            {totalPages > 1 && (
                <div className="px-6 py-4 border-t border-gray-200 flex items-center justify-between">
                    <div className="text-sm text-gray-700">
                        Showing {(currentPage - 1) * pageSize + 1} to{' '}
                        {Math.min(currentPage * pageSize, totalRecords)} of {totalRecords} results
                    </div>

                    <div className="flex items-center space-x-2">
                        <button
                            onClick={() => onPageChange(currentPage - 1)}
                            disabled={currentPage === 1}
                            className="px-3 py-1 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                            Previous
                        </button>

                        {/* Page Numbers */}
                        {[...Array(totalPages)].map((_, index) => {
                            const pageNumber = index + 1;

                            // Show first page, last page, current page, and 2 pages around current
                            if (
                                pageNumber === 1 ||
                                pageNumber === totalPages ||
                                (pageNumber >= currentPage - 1 && pageNumber <= currentPage + 1)
                            ) {
                                return (
                                    <button
                                        key={pageNumber}
                                        onClick={() => onPageChange(pageNumber)}
                                        className={`px-3 py-1 border rounded-lg text-sm font-medium transition-colors ${pageNumber === currentPage
                                                ? 'bg-indigo-600 text-white border-indigo-600'
                                                : 'border-gray-300 text-gray-700 hover:bg-gray-50'
                                            }`}
                                    >
                                        {pageNumber}
                                    </button>
                                );
                            } else if (
                                pageNumber === currentPage - 2 ||
                                pageNumber === currentPage + 2
                            ) {
                                return (
                                    <span key={pageNumber} className="px-2 text-gray-400">
                                        ...
                                    </span>
                                );
                            }
                            return null;
                        })}

                        <button
                            onClick={() => onPageChange(currentPage + 1)}
                            disabled={currentPage === totalPages}
                            className="px-3 py-1 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                            Next
                        </button>
                    </div>
                </div>
            )}
        </div>
    );
};

export default PartnerRequestsTable;
