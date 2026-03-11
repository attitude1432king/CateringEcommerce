/**
 * AdminSupervisorPayments Page
 * Admin approval queue for supervisor payment requests
 */

import { useState, useEffect } from 'react';
import { IndianRupee, Search, RefreshCw, CheckCircle2, XCircle, Clock, User } from 'lucide-react';
import AdminLayout from '../../components/admin/layout/AdminLayout';
import { paymentApi } from '../../services/api/supervisor';
import { toast } from 'react-hot-toast';

const AdminSupervisorPayments = () => {
  const [pendingApprovals, setPendingApprovals] = useState([]);
  const [summary, setSummary] = useState(null);
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState(null);
  const [searchTerm, setSearchTerm] = useState('');

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    setLoading(true);
    try {
      const [approvalsRes, summaryRes] = await Promise.all([
        paymentApi.getPendingApprovals(),
        paymentApi.getPaymentSummary(),
      ]);

      if (approvalsRes.success) {
        setPendingApprovals(approvalsRes.data?.data || approvalsRes.data || []);
      }
      if (summaryRes.success) {
        setSummary(summaryRes.data?.data || summaryRes.data);
      }
    } catch {
      toast.error('Failed to load payment data');
    } finally {
      setLoading(false);
    }
  };

  const handleApprove = async (assignmentId) => {
    setActionLoading(assignmentId);
    try {
      const response = await paymentApi.approvePayment(assignmentId, 'Approved by admin');
      if (response.success) {
        toast.success('Payment approved');
        loadData();
      } else {
        toast.error(response.message);
      }
    } catch {
      toast.error('Failed to approve payment');
    } finally {
      setActionLoading(null);
    }
  };

  const handleReject = async (assignmentId) => {
    const reason = prompt('Enter rejection reason:');
    if (!reason) return;

    setActionLoading(assignmentId);
    try {
      const response = await paymentApi.rejectPayment(assignmentId, reason);
      if (response.success) {
        toast.success('Payment rejected');
        loadData();
      } else {
        toast.error(response.message);
      }
    } catch {
      toast.error('Failed to reject payment');
    } finally {
      setActionLoading(null);
    }
  };

  const filteredApprovals = searchTerm
    ? pendingApprovals.filter(
        (a) =>
          a.supervisorName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
          a.eventName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
          String(a.assignmentId).includes(searchTerm)
      )
    : pendingApprovals;

  return (
    <AdminLayout>
      <div className="p-6">
        <div className="flex items-center justify-between mb-6">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Supervisor Payments</h1>
            <p className="text-sm text-gray-500 mt-1">Approve or reject supervisor payment requests</p>
          </div>
          <button
            onClick={loadData}
            className="flex items-center gap-2 px-4 py-2 text-sm border border-gray-300 rounded-lg hover:bg-gray-50"
          >
            <RefreshCw className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`} />
            Refresh
          </button>
        </div>

        {/* Summary Cards */}
        {summary && (
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
            <div className="bg-white rounded-lg shadow-md p-4">
              <div className="flex items-center gap-2 mb-2">
                <Clock className="w-5 h-5 text-yellow-600" />
                <span className="text-sm text-gray-600">Pending Approval</span>
              </div>
              <p className="text-2xl font-bold text-yellow-600">{pendingApprovals.length}</p>
            </div>
            <div className="bg-white rounded-lg shadow-md p-4">
              <div className="flex items-center gap-2 mb-2">
                <IndianRupee className="w-5 h-5 text-blue-600" />
                <span className="text-sm text-gray-600">Total Pending Amount</span>
              </div>
              <p className="text-2xl font-bold text-blue-600">
                Rs. {(summary.totalPendingAmount || pendingApprovals.reduce((sum, a) => sum + (a.amount || 0), 0)).toLocaleString()}
              </p>
            </div>
            <div className="bg-white rounded-lg shadow-md p-4">
              <div className="flex items-center gap-2 mb-2">
                <CheckCircle2 className="w-5 h-5 text-green-600" />
                <span className="text-sm text-gray-600">Approved This Month</span>
              </div>
              <p className="text-2xl font-bold text-green-600">
                Rs. {(summary.approvedThisMonth || 0).toLocaleString()}
              </p>
            </div>
            <div className="bg-white rounded-lg shadow-md p-4">
              <div className="flex items-center gap-2 mb-2">
                <User className="w-5 h-5 text-purple-600" />
                <span className="text-sm text-gray-600">Active Supervisors</span>
              </div>
              <p className="text-2xl font-bold text-purple-600">{summary.activeSupervisors || 0}</p>
            </div>
          </div>
        )}

        {/* Search */}
        <div className="bg-white rounded-lg shadow-md mb-6">
          <div className="p-4">
            <div className="relative">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
              <input
                type="text"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                placeholder="Search by supervisor name, event, or assignment ID..."
                className="w-full pl-10 pr-4 py-2 text-sm border border-gray-300 rounded-lg"
              />
            </div>
          </div>
        </div>

        {/* Payments Table */}
        <div className="bg-white rounded-lg shadow-md overflow-hidden">
          {loading ? (
            <div className="p-8 text-center">
              <div className="animate-spin rounded-full h-8 w-8 border-t-2 border-b-2 border-blue-600 mx-auto" />
              <p className="text-sm text-gray-500 mt-2">Loading payments...</p>
            </div>
          ) : filteredApprovals.length === 0 ? (
            <div className="p-8 text-center text-gray-500">
              <CheckCircle2 className="w-12 h-12 mx-auto mb-3 text-green-300" />
              <p className="font-medium">No pending payment approvals</p>
              <p className="text-sm mt-1">All payment requests have been processed</p>
            </div>
          ) : (
            <table className="w-full text-sm">
              <thead className="bg-gray-50 border-b border-gray-200">
                <tr>
                  <th className="text-left px-4 py-3 font-medium text-gray-600">Supervisor</th>
                  <th className="text-left px-4 py-3 font-medium text-gray-600">Event / Assignment</th>
                  <th className="text-right px-4 py-3 font-medium text-gray-600">Amount</th>
                  <th className="text-left px-4 py-3 font-medium text-gray-600">Requested</th>
                  <th className="text-left px-4 py-3 font-medium text-gray-600">Type</th>
                  <th className="text-right px-4 py-3 font-medium text-gray-600">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {filteredApprovals.map((approval) => (
                  <tr key={approval.assignmentId || approval.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3">
                      <p className="font-medium text-gray-900">{approval.supervisorName || 'N/A'}</p>
                      <p className="text-xs text-gray-500">
                        {approval.supervisorType === 'CAREER' ? (
                          <span className="text-blue-600">Career</span>
                        ) : (
                          <span className="text-green-600">Registered</span>
                        )}
                      </p>
                    </td>
                    <td className="px-4 py-3">
                      <p className="text-gray-600">{approval.eventName || `Assignment #${approval.assignmentId}`}</p>
                      <p className="text-xs text-gray-500">
                        {approval.eventDate ? new Date(approval.eventDate).toLocaleDateString() : ''}
                      </p>
                    </td>
                    <td className="px-4 py-3 text-right">
                      <p className="font-bold text-gray-900">Rs. {(approval.amount || 0).toLocaleString()}</p>
                    </td>
                    <td className="px-4 py-3 text-gray-500">
                      {approval.requestedDate ? new Date(approval.requestedDate).toLocaleDateString() : 'N/A'}
                    </td>
                    <td className="px-4 py-3">
                      <span className={`text-xs px-2 py-1 rounded ${
                        approval.supervisorType === 'CAREER'
                          ? 'bg-blue-100 text-blue-800'
                          : 'bg-yellow-100 text-yellow-800'
                      }`}>
                        {approval.supervisorType === 'CAREER' ? 'Direct Release' : 'Needs Approval'}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-right">
                      <div className="flex items-center justify-end gap-2">
                        <button
                          onClick={() => handleApprove(approval.assignmentId)}
                          disabled={actionLoading === approval.assignmentId}
                          className="px-3 py-1.5 text-xs bg-green-600 text-white rounded hover:bg-green-700 disabled:opacity-50 flex items-center gap-1"
                        >
                          <CheckCircle2 className="w-3 h-3" />
                          Approve
                        </button>
                        <button
                          onClick={() => handleReject(approval.assignmentId)}
                          disabled={actionLoading === approval.assignmentId}
                          className="px-3 py-1.5 text-xs bg-red-600 text-white rounded hover:bg-red-700 disabled:opacity-50 flex items-center gap-1"
                        >
                          <XCircle className="w-3 h-3" />
                          Reject
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </div>
    </AdminLayout>
  );
};

export default AdminSupervisorPayments;
