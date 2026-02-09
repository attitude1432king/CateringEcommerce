import React, { useState, useEffect } from 'react';
import {
  AlertTriangle,
  CheckCircle,
  Filter,
  RefreshCw,
  Search
} from 'lucide-react';
import {
  getPendingComplaints,
  resolveComplaint,
  escalateComplaint
} from '../../services/adminComplaintApi';
import {
  ComplaintTable,
  ComplaintDetailDrawer,
  ComplaintResolutionModal
} from '../../components/admin/complaints';

const AdminComplaints = () => {
  const [complaints, setComplaints] = useState([]);
  const [filteredComplaints, setFilteredComplaints] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);
  const [successMessage, setSuccessMessage] = useState(null);

  // Filters
  const [statusFilter, setStatusFilter] = useState('all');
  const [severityFilter, setSeverityFilter] = useState('all');
  const [searchQuery, setSearchQuery] = useState('');

  // Modals & Drawers
  const [selectedComplaint, setSelectedComplaint] = useState(null);
  const [isDetailDrawerOpen, setIsDetailDrawerOpen] = useState(false);
  const [isResolutionModalOpen, setIsResolutionModalOpen] = useState(false);

  useEffect(() => {
    fetchComplaints();
  }, []);

  useEffect(() => {
    filterComplaints();
  }, [complaints, statusFilter, severityFilter, searchQuery]);

  useEffect(() => {
    // Clear messages after 5 seconds
    if (successMessage || error) {
      const timer = setTimeout(() => {
        setSuccessMessage(null);
        setError(null);
      }, 5000);
      return () => clearTimeout(timer);
    }
  }, [successMessage, error]);

  const fetchComplaints = async () => {
    setIsLoading(true);
    setError(null);

    try {
      const response = await getPendingComplaints();

      if (response.success && response.data) {
        setComplaints(response.data);
      } else {
        setError(response.message || 'Failed to load complaints');
      }
    } catch (error) {
      console.error('Error fetching complaints:', error);
      setError('An error occurred while loading complaints');
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

    // Severity filter
    if (severityFilter !== 'all') {
      filtered = filtered.filter(c => c.severity?.toLowerCase() === severityFilter);
    }

    // Search filter
    if (searchQuery.trim()) {
      const query = searchQuery.toLowerCase();
      filtered = filtered.filter(c =>
        c.complaintId?.toString().includes(query) ||
        c.orderId?.toString().includes(query) ||
        c.complaintType?.toLowerCase().includes(query) ||
        c.complaintSummary?.toLowerCase().includes(query)
      );
    }

    setFilteredComplaints(filtered);
  };

  const handleViewDetails = (complaint) => {
    setSelectedComplaint(complaint);
    setIsDetailDrawerOpen(true);
  };

  const handleResolve = (complaint) => {
    setSelectedComplaint(complaint);
    setIsDetailDrawerOpen(false);
    setIsResolutionModalOpen(true);
  };

  const handleEscalate = async (complaint) => {
    if (!window.confirm('Are you sure you want to escalate this complaint?')) {
      return;
    }

    try {
      const response = await escalateComplaint(complaint.complaintId);

      if (response.success) {
        setSuccessMessage('Complaint escalated successfully');
        setIsDetailDrawerOpen(false);
        fetchComplaints(); // Refresh list
      } else {
        setError(response.message || 'Failed to escalate complaint');
      }
    } catch (error) {
      console.error('Error escalating complaint:', error);
      setError('An error occurred while escalating the complaint');
    }
  };

  const handleSubmitResolution = async (resolutionData) => {
    try {
      const response = await resolveComplaint(resolutionData);

      if (response.success) {
        setSuccessMessage('Complaint resolved successfully');
        setIsResolutionModalOpen(false);
        setSelectedComplaint(null);
        fetchComplaints(); // Refresh list
      } else {
        setError(response.message || 'Failed to resolve complaint');
        throw new Error(response.message);
      }
    } catch (error) {
      console.error('Error resolving complaint:', error);
      throw error; // Re-throw to let modal handle the error state
    }
  };

  const getStats = () => {
    return {
      total: complaints.length,
      pending: complaints.filter(c => c.status?.toLowerCase() === 'open' || c.status?.toLowerCase() === 'under_investigation').length,
      resolved: complaints.filter(c => c.status?.toLowerCase() === 'resolved').length,
      rejected: complaints.filter(c => c.status?.toLowerCase() === 'rejected').length,
      critical: complaints.filter(c => c.severity?.toLowerCase() === 'critical').length
    };
  };

  const stats = getStats();

  return (
    <div className="min-h-screen bg-gray-50 p-6">
      <div className="max-w-7xl mx-auto">
        {/* Page Header */}
        <div className="mb-6">
          <h1 className="text-3xl font-bold text-gray-900 mb-2">Complaint Management</h1>
          <p className="text-gray-600">Review and resolve customer complaints</p>
        </div>

        {/* Success Message */}
        {successMessage && (
          <div className="mb-6 bg-green-50 border-2 border-green-300 text-green-800 px-6 py-4 rounded-lg flex items-center gap-3">
            <CheckCircle className="w-6 h-6 flex-shrink-0" />
            <p className="font-semibold">{successMessage}</p>
          </div>
        )}

        {/* Error Message */}
        {error && (
          <div className="mb-6 bg-red-50 border-2 border-red-300 text-red-800 px-6 py-4 rounded-lg flex items-center gap-3">
            <AlertTriangle className="w-6 h-6 flex-shrink-0" />
            <p className="font-semibold">{error}</p>
          </div>
        )}

        {/* Stats Cards */}
        <div className="grid grid-cols-1 md:grid-cols-5 gap-4 mb-6">
          <div className="bg-white rounded-lg shadow p-4">
            <p className="text-sm text-gray-600 mb-1">Total Complaints</p>
            <p className="text-3xl font-bold text-gray-900">{stats.total}</p>
          </div>
          <div className="bg-white rounded-lg shadow p-4">
            <p className="text-sm text-gray-600 mb-1">Pending Review</p>
            <p className="text-3xl font-bold text-blue-600">{stats.pending}</p>
          </div>
          <div className="bg-white rounded-lg shadow p-4">
            <p className="text-sm text-gray-600 mb-1">Resolved</p>
            <p className="text-3xl font-bold text-green-600">{stats.resolved}</p>
          </div>
          <div className="bg-white rounded-lg shadow p-4">
            <p className="text-sm text-gray-600 mb-1">Rejected</p>
            <p className="text-3xl font-bold text-red-600">{stats.rejected}</p>
          </div>
          <div className="bg-white rounded-lg shadow p-4">
            <p className="text-sm text-gray-600 mb-1">Critical Issues</p>
            <p className="text-3xl font-bold text-orange-600">{stats.critical}</p>
          </div>
        </div>

        {/* Filters & Actions */}
        <div className="bg-white rounded-lg shadow p-4 mb-6">
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            {/* Search */}
            <div className="relative md:col-span-2">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
              <input
                type="text"
                placeholder="Search by ID, order, or description..."
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
                <option value="open">Open</option>
                <option value="under_investigation">Under Investigation</option>
                <option value="resolved">Resolved</option>
                <option value="rejected">Rejected</option>
                <option value="escalated">Escalated</option>
              </select>
            </div>

            {/* Severity Filter */}
            <div className="relative">
              <select
                value={severityFilter}
                onChange={(e) => setSeverityFilter(e.target.value)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent appearance-none"
              >
                <option value="all">All Severity</option>
                <option value="critical">Critical</option>
                <option value="major">Major</option>
                <option value="minor">Minor</option>
              </select>
            </div>
          </div>

          {/* Actions Row */}
          <div className="flex items-center justify-between mt-4 pt-4 border-t border-gray-200">
            <p className="text-sm text-gray-600">
              Showing {filteredComplaints.length} of {complaints.length} complaints
            </p>
            <button
              onClick={fetchComplaints}
              disabled={isLoading}
              className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50"
            >
              <RefreshCw className={`w-4 h-4 ${isLoading ? 'animate-spin' : ''}`} />
              Refresh
            </button>
          </div>
        </div>

        {/* Complaints Table */}
        <ComplaintTable
          complaints={filteredComplaints}
          onViewDetails={handleViewDetails}
          isLoading={isLoading}
        />

        {/* Complaint Detail Drawer */}
        <ComplaintDetailDrawer
          complaint={selectedComplaint}
          isOpen={isDetailDrawerOpen}
          onClose={() => {
            setIsDetailDrawerOpen(false);
            setSelectedComplaint(null);
          }}
          onResolve={handleResolve}
          onEscalate={handleEscalate}
        />

        {/* Complaint Resolution Modal */}
        <ComplaintResolutionModal
          complaint={selectedComplaint}
          isOpen={isResolutionModalOpen}
          onClose={() => {
            setIsResolutionModalOpen(false);
            setSelectedComplaint(null);
          }}
          onSubmitResolution={handleSubmitResolution}
        />
      </div>
    </div>
  );
};

export default AdminComplaints;
