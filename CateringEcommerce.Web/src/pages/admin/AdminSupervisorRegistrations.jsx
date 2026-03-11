/**
 * AdminSupervisorRegistrations Page
 * Admin queue for managing supervisor registration pipeline
 */

import { useState, useEffect } from 'react';
import { Search, RefreshCw, Filter, ChevronDown, Eye, CheckCircle2, XCircle, Clock } from 'lucide-react';
import AdminLayout from '../../components/admin/layout/AdminLayout';
import { registrationApi } from '../../services/api/supervisor';
import { toast } from 'react-hot-toast';

const STAGES = [
  { value: '', label: 'All Stages' },
  { value: 'DOCUMENT_VERIFICATION', label: 'Document Verification' },
  { value: 'INTERVIEW', label: 'Interview' },
  { value: 'TRAINING', label: 'Training' },
  { value: 'CERTIFICATION', label: 'Certification' },
  { value: 'ACTIVATED', label: 'Activated' },
  { value: 'REJECTED', label: 'Rejected' },
];

const getStageColor = (stage) => {
  const colors = {
    DOCUMENT_VERIFICATION: 'bg-blue-100 text-blue-800',
    INTERVIEW: 'bg-purple-100 text-purple-800',
    TRAINING: 'bg-yellow-100 text-yellow-800',
    CERTIFICATION: 'bg-orange-100 text-orange-800',
    ACTIVATED: 'bg-green-100 text-green-800',
    REJECTED: 'bg-red-100 text-red-800',
  };
  return colors[stage] || 'bg-gray-100 text-gray-800';
};

const AdminSupervisorRegistrations = () => {
  const [registrations, setRegistrations] = useState([]);
  const [loading, setLoading] = useState(true);
  const [stats, setStats] = useState(null);
  const [selectedStage, setSelectedStage] = useState('');
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedRegistration, setSelectedRegistration] = useState(null);
  const [actionLoading, setActionLoading] = useState(false);

  useEffect(() => {
    loadData();
  }, [selectedStage]);

  const loadData = async () => {
    setLoading(true);
    try {
      const [regRes, statsRes] = await Promise.all([
        selectedStage
          ? registrationApi.getRegistrationsByStage(selectedStage)
          : registrationApi.getAllRegistrations(),
        registrationApi.getRegistrationStatistics(),
      ]);

      if (regRes.success) {
        setRegistrations(regRes.data?.data || regRes.data || []);
      }
      if (statsRes.success) {
        setStats(statsRes.data?.data || statsRes.data);
      }
    } catch {
      toast.error('Failed to load registrations');
    } finally {
      setLoading(false);
    }
  };

  const handleVerifyDocs = async (registrationId, approved) => {
    setActionLoading(true);
    try {
      const response = await registrationApi.verifyDocuments({
        registrationId,
        isApproved: approved,
        verificationNotes: approved ? 'Documents verified' : 'Documents incomplete',
      });
      if (response.success) {
        toast.success(approved ? 'Documents verified' : 'Documents rejected');
        loadData();
      } else {
        toast.error(response.message);
      }
    } catch {
      toast.error('Action failed');
    } finally {
      setActionLoading(false);
    }
  };

  const handleActivate = async (registrationId) => {
    setActionLoading(true);
    try {
      const response = await registrationApi.activateRegistration(registrationId);
      if (response.success) {
        toast.success('Supervisor activated successfully');
        loadData();
      } else {
        toast.error(response.message);
      }
    } catch {
      toast.error('Activation failed');
    } finally {
      setActionLoading(false);
    }
  };

  const handleReject = async (registrationId) => {
    const reason = prompt('Enter rejection reason:');
    if (!reason) return;

    setActionLoading(true);
    try {
      const response = await registrationApi.rejectRegistration(registrationId, reason);
      if (response.success) {
        toast.success('Registration rejected');
        loadData();
      } else {
        toast.error(response.message);
      }
    } catch {
      toast.error('Rejection failed');
    } finally {
      setActionLoading(false);
    }
  };

  const filteredRegistrations = searchTerm
    ? registrations.filter(
        (r) =>
          r.fullName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
          r.email?.toLowerCase().includes(searchTerm.toLowerCase()) ||
          r.phone?.includes(searchTerm)
      )
    : registrations;

  return (
    <AdminLayout>
      <div className="p-6">
        <div className="flex items-center justify-between mb-6">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Supervisor Registrations</h1>
            <p className="text-sm text-gray-500 mt-1">Manage supervisor registration pipeline</p>
          </div>
          <button
            onClick={loadData}
            className="flex items-center gap-2 px-4 py-2 text-sm border border-gray-300 rounded-lg hover:bg-gray-50"
          >
            <RefreshCw className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`} />
            Refresh
          </button>
        </div>

        {/* Stats Cards */}
        {stats && (
          <div className="grid grid-cols-2 md:grid-cols-5 gap-4 mb-6">
            {[
              { label: 'Total', value: stats.total || 0, color: 'bg-blue-50 text-blue-700' },
              { label: 'Pending Docs', value: stats.pendingDocVerification || 0, color: 'bg-yellow-50 text-yellow-700' },
              { label: 'In Interview', value: stats.pendingInterview || 0, color: 'bg-purple-50 text-purple-700' },
              { label: 'In Training', value: stats.inTraining || 0, color: 'bg-orange-50 text-orange-700' },
              { label: 'Activated', value: stats.activated || 0, color: 'bg-green-50 text-green-700' },
            ].map((stat) => (
              <div key={stat.label} className={`rounded-lg p-4 ${stat.color}`}>
                <p className="text-sm font-medium">{stat.label}</p>
                <p className="text-2xl font-bold">{stat.value}</p>
              </div>
            ))}
          </div>
        )}

        {/* Filters */}
        <div className="bg-white rounded-lg shadow-md mb-6">
          <div className="p-4 flex items-center gap-4">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
              <input
                type="text"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                placeholder="Search by name, email, phone..."
                className="w-full pl-10 pr-4 py-2 text-sm border border-gray-300 rounded-lg"
              />
            </div>
            <select
              value={selectedStage}
              onChange={(e) => setSelectedStage(e.target.value)}
              className="px-3 py-2 text-sm border border-gray-300 rounded-lg"
            >
              {STAGES.map((stage) => (
                <option key={stage.value} value={stage.value}>{stage.label}</option>
              ))}
            </select>
          </div>
        </div>

        {/* Registrations Table */}
        <div className="bg-white rounded-lg shadow-md overflow-hidden">
          {loading ? (
            <div className="p-8 text-center">
              <div className="animate-spin rounded-full h-8 w-8 border-t-2 border-b-2 border-blue-600 mx-auto" />
              <p className="text-sm text-gray-500 mt-2">Loading registrations...</p>
            </div>
          ) : filteredRegistrations.length === 0 ? (
            <div className="p-8 text-center text-gray-500">
              <p>No registrations found</p>
            </div>
          ) : (
            <table className="w-full text-sm">
              <thead className="bg-gray-50 border-b border-gray-200">
                <tr>
                  <th className="text-left px-4 py-3 font-medium text-gray-600">Name</th>
                  <th className="text-left px-4 py-3 font-medium text-gray-600">Contact</th>
                  <th className="text-left px-4 py-3 font-medium text-gray-600">Stage</th>
                  <th className="text-left px-4 py-3 font-medium text-gray-600">Submitted</th>
                  <th className="text-right px-4 py-3 font-medium text-gray-600">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {filteredRegistrations.map((reg) => (
                  <tr key={reg.id || reg.registrationId} className="hover:bg-gray-50">
                    <td className="px-4 py-3">
                      <p className="font-medium text-gray-900">{reg.fullName || reg.name || 'N/A'}</p>
                      <p className="text-xs text-gray-500">{reg.supervisorType || 'REGISTERED'}</p>
                    </td>
                    <td className="px-4 py-3">
                      <p className="text-gray-600">{reg.email || 'N/A'}</p>
                      <p className="text-xs text-gray-500">{reg.phone || ''}</p>
                    </td>
                    <td className="px-4 py-3">
                      <span className={`text-xs px-2 py-1 rounded-full font-medium ${getStageColor(reg.currentStage || reg.stage)}`}>
                        {(reg.currentStage || reg.stage || 'N/A').replace(/_/g, ' ')}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-gray-500">
                      {reg.submittedDate ? new Date(reg.submittedDate).toLocaleDateString() : 'N/A'}
                    </td>
                    <td className="px-4 py-3 text-right">
                      <div className="flex items-center justify-end gap-2">
                        {(reg.currentStage === 'DOCUMENT_VERIFICATION') && (
                          <>
                            <button
                              onClick={() => handleVerifyDocs(reg.id || reg.registrationId, true)}
                              disabled={actionLoading}
                              className="px-2 py-1 text-xs bg-green-600 text-white rounded hover:bg-green-700 disabled:opacity-50"
                            >
                              Verify
                            </button>
                            <button
                              onClick={() => handleVerifyDocs(reg.id || reg.registrationId, false)}
                              disabled={actionLoading}
                              className="px-2 py-1 text-xs bg-red-600 text-white rounded hover:bg-red-700 disabled:opacity-50"
                            >
                              Reject
                            </button>
                          </>
                        )}
                        {(reg.currentStage === 'CERTIFICATION') && (
                          <button
                            onClick={() => handleActivate(reg.id || reg.registrationId)}
                            disabled={actionLoading}
                            className="px-2 py-1 text-xs bg-green-600 text-white rounded hover:bg-green-700 disabled:opacity-50"
                          >
                            Activate
                          </button>
                        )}
                        <button
                          onClick={() => handleReject(reg.id || reg.registrationId)}
                          disabled={actionLoading}
                          className="px-2 py-1 text-xs border border-red-300 text-red-600 rounded hover:bg-red-50 disabled:opacity-50"
                        >
                          <XCircle className="w-3 h-3" />
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

export default AdminSupervisorRegistrations;
