import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { getMyComplaints } from '../services/complaintApi';
import {
  Clock,
  CheckCircle,
  XCircle,
  AlertCircle,
  FileText,
  Search,
  Filter
} from 'lucide-react';

const MyComplaintsPage = () => {
  const navigate = useNavigate();
  const [complaints, setComplaints] = useState([]);
  const [filteredComplaints, setFilteredComplaints] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);
  const [statusFilter, setStatusFilter] = useState('all');
  const [searchQuery, setSearchQuery] = useState('');

  useEffect(() => {
    fetchComplaints();
  }, []);

  useEffect(() => {
    filterComplaints();
  }, [complaints, statusFilter, searchQuery]);

  const fetchComplaints = async () => {
    setIsLoading(true);
    setError(null);

    try {
      const response = await getMyComplaints();

      if (response.success && response.data) {
        setComplaints(response.data);
      } else {
        setError(response.message || 'Failed to load complaints');
      }
    } catch (error) {
      console.error('Error fetching complaints:', error);
      setError('An error occurred while loading your complaints');
    } finally {
      setIsLoading(false);
    }
  };

  const filterComplaints = () => {
    let filtered = [...complaints];

    // Status filter
    if (statusFilter !== 'all') {
      filtered = filtered.filter(c => c.status?.toLowerCase() === statusFilter);
    }

    // Search filter
    if (searchQuery.trim()) {
      const query = searchQuery.toLowerCase();
      filtered = filtered.filter(c =>
        c.complaintType?.toLowerCase().includes(query) ||
        c.complaintSummary?.toLowerCase().includes(query) ||
        c.orderId?.toString().includes(query)
      );
    }

    setFilteredComplaints(filtered);
  };

  const getStatusConfig = (status) => {
    const statusLower = status?.toLowerCase() || '';

    const configs = {
      'open': { icon: Clock, color: 'blue', label: 'Under Review' },
      'under_investigation': { icon: Search, color: 'purple', label: 'Investigating' },
      'resolved': { icon: CheckCircle, color: 'green', label: 'Resolved' },
      'rejected': { icon: XCircle, color: 'red', label: 'Rejected' },
      'escalated': { icon: AlertCircle, color: 'amber', label: 'Escalated' }
    };

    return configs[statusLower] || { icon: FileText, color: 'gray', label: status };
  };

  const getSeverityColor = (severity) => {
    const severityLower = severity?.toLowerCase() || '';
    const colors = {
      'critical': 'bg-red-100 text-red-800 border-red-300',
      'major': 'bg-orange-100 text-orange-800 border-orange-300',
      'minor': 'bg-yellow-100 text-yellow-800 border-yellow-300'
    };
    return colors[severityLower] || 'bg-gray-100 text-gray-800 border-gray-300';
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-100 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-red-500 mx-auto"></div>
          <p className="mt-4 text-gray-600">Loading your complaints...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-100 py-8">
      <div className="max-w-6xl mx-auto px-4">
        {/* Page Header */}
        <div className="mb-6">
          <h1 className="text-3xl font-bold mb-2">My Complaints</h1>
          <p className="text-gray-600">Track and manage your filed complaints</p>
        </div>

        {/* Error Message */}
        {error && (
          <div className="bg-red-50 border border-red-200 text-red-800 px-4 py-3 rounded-lg mb-6">
            {error}
          </div>
        )}

        {/* Filters */}
        <div className="bg-white rounded-lg p-4 shadow-sm mb-6">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {/* Search */}
            <div className="relative">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
              <input
                type="text"
                placeholder="Search complaints..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>

            {/* Status Filter */}
            <div className="relative">
              <Filter className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
              <select
                value={statusFilter}
                onChange={(e) => setStatusFilter(e.target.value)}
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent appearance-none"
              >
                <option value="all">All Status</option>
                <option value="open">Under Review</option>
                <option value="under_investigation">Investigating</option>
                <option value="resolved">Resolved</option>
                <option value="rejected">Rejected</option>
                <option value="escalated">Escalated</option>
              </select>
            </div>
          </div>
        </div>

        {/* Complaints List */}
        {filteredComplaints.length === 0 ? (
          <div className="bg-white rounded-lg p-12 text-center shadow-sm">
            <FileText className="w-24 h-24 mx-auto text-gray-300 mb-4" />
            <h2 className="text-2xl font-semibold mb-2">
              {complaints.length === 0 ? 'No complaints filed' : 'No complaints match your filters'}
            </h2>
            <p className="text-gray-600 mb-6">
              {complaints.length === 0
                ? 'Your filed complaints will appear here'
                : 'Try adjusting your filters to see more results'}
            </p>
            {complaints.length === 0 && (
              <button
                onClick={() => navigate('/my-orders')}
                className="px-6 py-3 bg-red-500 text-white rounded-lg hover:bg-red-600 transition-colors"
              >
                View My Orders
              </button>
            )}
          </div>
        ) : (
          <div className="space-y-4">
            {filteredComplaints.map((complaint) => {
              const statusConfig = getStatusConfig(complaint.status);
              const StatusIcon = statusConfig.icon;

              return (
                <div
                  key={complaint.complaintId}
                  className="bg-white rounded-lg p-6 shadow-sm hover:shadow-md transition-shadow cursor-pointer border-l-4"
                  style={{
                    borderLeftColor: statusConfig.color === 'green' ? '#10b981' :
                      statusConfig.color === 'red' ? '#ef4444' :
                      statusConfig.color === 'blue' ? '#3b82f6' :
                      statusConfig.color === 'purple' ? '#8b5cf6' :
                      statusConfig.color === 'amber' ? '#f59e0b' : '#6b7280'
                  }}
                  onClick={() => navigate(`/complaints/${complaint.complaintId}`)}
                >
                  <div className="flex items-start justify-between mb-4">
                    <div>
                      <h3 className="text-lg font-semibold mb-1">
                        Complaint #{complaint.complaintId}
                      </h3>
                      <p className="text-sm text-gray-600">
                        Order #{complaint.orderId} - Filed on{' '}
                        {new Date(complaint.reportedAt || complaint.createdDate).toLocaleDateString('en-IN', {
                          day: 'numeric',
                          month: 'long',
                          year: 'numeric'
                        })}
                      </p>
                    </div>

                    <span
                      className={`px-3 py-1 rounded-full text-sm font-semibold flex items-center gap-2 bg-${statusConfig.color}-100 text-${statusConfig.color}-800`}
                    >
                      <StatusIcon className="w-4 h-4" />
                      {statusConfig.label}
                    </span>
                  </div>

                  <div className="mb-4">
                    <div className="flex items-center gap-2 mb-2">
                      <span className="font-medium text-gray-900">
                        {complaint.complaintType?.replace(/_/g, ' ')}
                      </span>
                      <span className={`px-2 py-0.5 rounded text-xs font-medium border ${getSeverityColor(complaint.severity)}`}>
                        {complaint.severity || 'N/A'}
                      </span>
                    </div>
                    <p className="text-gray-700">
                      {complaint.complaintSummary || complaint.complaintDetails?.substring(0, 150)}
                      {complaint.complaintDetails?.length > 150 ? '...' : ''}
                    </p>
                  </div>

                  {complaint.refundAmount > 0 && (
                    <div className="bg-green-50 border border-green-200 rounded-lg p-3 mb-3">
                      <p className="text-sm text-green-800">
                        <strong>Refund Approved:</strong> ₹{complaint.refundAmount.toFixed(2)}
                      </p>
                    </div>
                  )}

                  <div className="flex items-center justify-between pt-3 border-t border-gray-200">
                    <div className="text-sm text-gray-600">
                      {complaint.reviewedDate ? (
                        <span>Reviewed on {new Date(complaint.reviewedDate).toLocaleDateString('en-IN')}</span>
                      ) : (
                        <span>Awaiting review</span>
                      )}
                    </div>
                    <button className="text-blue-600 hover:text-blue-700 font-medium text-sm">
                      View Details →
                    </button>
                  </div>
                </div>
              );
            })}
          </div>
        )}

        {/* Summary Stats */}
        {complaints.length > 0 && (
          <div className="mt-6 bg-white rounded-lg p-4 shadow-sm">
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-center">
              <div>
                <p className="text-2xl font-bold text-gray-900">{complaints.length}</p>
                <p className="text-sm text-gray-600">Total Complaints</p>
              </div>
              <div>
                <p className="text-2xl font-bold text-blue-600">
                  {complaints.filter(c => c.status?.toLowerCase() === 'open').length}
                </p>
                <p className="text-sm text-gray-600">Under Review</p>
              </div>
              <div>
                <p className="text-2xl font-bold text-green-600">
                  {complaints.filter(c => c.status?.toLowerCase() === 'resolved').length}
                </p>
                <p className="text-sm text-gray-600">Resolved</p>
              </div>
              <div>
                <p className="text-2xl font-bold text-red-600">
                  {complaints.filter(c => c.status?.toLowerCase() === 'rejected').length}
                </p>
                <p className="text-sm text-gray-600">Rejected</p>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default MyComplaintsPage;
