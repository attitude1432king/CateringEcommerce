/**
 * EarningsPage
 * Supervisor earnings overview, payment history, and payment requests
 */

import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  IndianRupee,
  ArrowLeft,
  Clock,
  CheckCircle2,
  XCircle,
  TrendingUp,
  Download,
  Filter,
} from 'lucide-react';
import { paymentApi } from '../../services/api/supervisor';
import { PaymentStatusBadge } from '../../components/supervisor/common/badges';
import toast from 'react-hot-toast';

const EarningsPage = () => {
  const navigate = useNavigate();
  const [earnings, setEarnings] = useState(null);
  const [history, setHistory] = useState([]);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState('overview');
  const [requestingPayment, setRequestingPayment] = useState(null);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      const [earningsRes, historyRes] = await Promise.all([
        paymentApi.getEarnings(),
        paymentApi.getPaymentHistory(),
      ]);

      if (earningsRes.success) {
        setEarnings(earningsRes.data?.data || earningsRes.data);
      }
      if (historyRes.success) {
        setHistory(historyRes.data?.data || historyRes.data || []);
      }
    } catch {
      toast.error('Failed to load earnings data');
    } finally {
      setLoading(false);
    }
  };

  const handleRequestPayment = async (assignmentId, amount) => {
    setRequestingPayment(assignmentId);
    try {
      const response = await paymentApi.requestPayment(assignmentId, amount, '');
      if (response.success) {
        toast.success('Payment request submitted');
        loadData();
      } else {
        toast.error(response.message);
      }
    } catch {
      toast.error('Failed to request payment');
    } finally {
      setRequestingPayment(null);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-blue-600" />
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white shadow-sm border-b border-gray-200">
        <div className="max-w-4xl mx-auto px-4 py-4">
          <div className="flex items-center gap-4">
            <button
              onClick={() => navigate(-1)}
              className="p-2 text-gray-600 hover:text-gray-900 hover:bg-gray-100 rounded-lg"
            >
              <ArrowLeft className="w-5 h-5" />
            </button>
            <div>
              <h1 className="text-xl font-semibold text-gray-900">Earnings & Payments</h1>
              <p className="text-sm text-gray-500">Track your earnings and payment status</p>
            </div>
          </div>
        </div>
      </div>

      <div className="max-w-4xl mx-auto px-4 py-6">
        {/* Earnings Summary Cards */}
        {earnings && (
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
            <div className="bg-white rounded-lg shadow-md p-4">
              <div className="flex items-center gap-2 mb-2">
                <IndianRupee className="w-5 h-5 text-green-600" />
                <span className="text-sm text-gray-600">Total Earned</span>
              </div>
              <p className="text-2xl font-bold text-gray-900">
                Rs. {(earnings.totalEarned || 0).toLocaleString()}
              </p>
            </div>
            <div className="bg-white rounded-lg shadow-md p-4">
              <div className="flex items-center gap-2 mb-2">
                <CheckCircle2 className="w-5 h-5 text-blue-600" />
                <span className="text-sm text-gray-600">Paid</span>
              </div>
              <p className="text-2xl font-bold text-blue-600">
                Rs. {(earnings.totalPaid || 0).toLocaleString()}
              </p>
            </div>
            <div className="bg-white rounded-lg shadow-md p-4">
              <div className="flex items-center gap-2 mb-2">
                <Clock className="w-5 h-5 text-yellow-600" />
                <span className="text-sm text-gray-600">Pending</span>
              </div>
              <p className="text-2xl font-bold text-yellow-600">
                Rs. {(earnings.totalPending || 0).toLocaleString()}
              </p>
            </div>
            <div className="bg-white rounded-lg shadow-md p-4">
              <div className="flex items-center gap-2 mb-2">
                <TrendingUp className="w-5 h-5 text-purple-600" />
                <span className="text-sm text-gray-600">This Month</span>
              </div>
              <p className="text-2xl font-bold text-purple-600">
                Rs. {(earnings.thisMonthEarnings || 0).toLocaleString()}
              </p>
            </div>
          </div>
        )}

        {/* Tabs */}
        <div className="bg-white rounded-lg shadow-md">
          <div className="border-b border-gray-200">
            <div className="flex">
              {[
                { id: 'overview', label: 'Overview' },
                { id: 'history', label: 'Payment History' },
                { id: 'pending', label: 'Pending Payments' },
              ].map((tab) => (
                <button
                  key={tab.id}
                  onClick={() => setActiveTab(tab.id)}
                  className={`flex-1 py-3 text-sm font-medium border-b-2 ${
                    activeTab === tab.id
                      ? 'border-blue-600 text-blue-600'
                      : 'border-transparent text-gray-500 hover:text-gray-700'
                  }`}
                >
                  {tab.label}
                </button>
              ))}
            </div>
          </div>

          <div className="p-6">
            {activeTab === 'overview' && (
              <div className="space-y-4">
                {earnings?.recentPayments?.length > 0 ? (
                  earnings.recentPayments.map((payment, i) => (
                    <div key={i} className="flex items-center justify-between border-b border-gray-100 pb-3">
                      <div>
                        <p className="text-sm font-medium text-gray-900">
                          {payment.eventName || `Assignment #${payment.assignmentId}`}
                        </p>
                        <p className="text-xs text-gray-500">
                          {payment.date ? new Date(payment.date).toLocaleDateString() : 'N/A'}
                        </p>
                      </div>
                      <div className="text-right">
                        <p className="text-sm font-bold text-gray-900">
                          Rs. {(payment.amount || 0).toLocaleString()}
                        </p>
                        <PaymentStatusBadge status={payment.status} />
                      </div>
                    </div>
                  ))
                ) : (
                  <div className="text-center py-8 text-gray-500">
                    <IndianRupee className="w-12 h-12 mx-auto mb-3 text-gray-300" />
                    <p>No recent payments</p>
                  </div>
                )}
              </div>
            )}

            {activeTab === 'history' && (
              <div className="space-y-3">
                {history.length > 0 ? (
                  history.map((item, i) => (
                    <div key={i} className="flex items-center justify-between bg-gray-50 rounded-lg px-4 py-3">
                      <div>
                        <p className="text-sm font-medium text-gray-900">
                          {item.eventName || `Assignment #${item.assignmentId}`}
                        </p>
                        <p className="text-xs text-gray-500">
                          {item.completedDate ? new Date(item.completedDate).toLocaleDateString() : 'N/A'}
                        </p>
                      </div>
                      <div className="text-right">
                        <p className="text-sm font-bold text-green-700">
                          Rs. {(item.amount || 0).toLocaleString()}
                        </p>
                        <PaymentStatusBadge status={item.paymentStatus} />
                      </div>
                    </div>
                  ))
                ) : (
                  <div className="text-center py-8 text-gray-500">
                    <p>No payment history yet</p>
                  </div>
                )}
              </div>
            )}

            {activeTab === 'pending' && (
              <div className="space-y-3">
                {history.filter((h) => h.paymentStatus === 'PENDING' || h.paymentStatus === 'REQUESTED').length > 0 ? (
                  history
                    .filter((h) => h.paymentStatus === 'PENDING' || h.paymentStatus === 'REQUESTED')
                    .map((item, i) => (
                      <div key={i} className="flex items-center justify-between bg-yellow-50 rounded-lg px-4 py-3 border border-yellow-200">
                        <div>
                          <p className="text-sm font-medium text-gray-900">
                            {item.eventName || `Assignment #${item.assignmentId}`}
                          </p>
                          <p className="text-xs text-gray-500">
                            Rs. {(item.amount || 0).toLocaleString()}
                          </p>
                        </div>
                        <div>
                          {item.paymentStatus === 'PENDING' ? (
                            <button
                              onClick={() => handleRequestPayment(item.assignmentId, item.amount)}
                              disabled={requestingPayment === item.assignmentId}
                              className="px-3 py-1.5 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50"
                            >
                              {requestingPayment === item.assignmentId ? 'Requesting...' : 'Request Payment'}
                            </button>
                          ) : (
                            <span className="text-xs px-2 py-1 bg-yellow-100 text-yellow-800 rounded">
                              <Clock className="w-3 h-3 inline mr-1" />
                              Awaiting Approval
                            </span>
                          )}
                        </div>
                      </div>
                    ))
                ) : (
                  <div className="text-center py-8 text-gray-500">
                    <CheckCircle2 className="w-12 h-12 mx-auto mb-3 text-gray-300" />
                    <p>No pending payments</p>
                  </div>
                )}
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default EarningsPage;
