/**
 * EarningsPage (REDESIGNED)
 * Modern supervisor earnings overview, payment history, and payment requests
 */

import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
    IndianRupee, ArrowLeft, Clock, CheckCircle2, XCircle, TrendingUp, Download, Filter,
} from 'lucide-react';
import { paymentApi } from '../../services/api/supervisor';
import { PaymentStatusBadge } from '../../components/supervisor/common/badges';
import { SupervisorNavHeader } from './SupervisorDashboard';
import toast from 'react-hot-toast';

const EarningsPage = () => {
    const navigate = useNavigate();
    const [earnings, setEarnings] = useState(null);
    const [history, setHistory] = useState([]);
    const [loading, setLoading] = useState(true);
    const [activeTab, setActiveTab] = useState('overview');
    const [requestingPayment, setRequestingPayment] = useState(null);

    useEffect(() => { loadData(); }, []);

    const loadData = async () => {
        try {
            const [earningsRes, historyRes] = await Promise.all([
                paymentApi.getEarnings(),
                paymentApi.getPaymentHistory(),
            ]);
            if (earningsRes.success) setEarnings(earningsRes.data?.data || earningsRes.data);
            if (historyRes.success) setHistory(historyRes.data?.data || historyRes.data || []);
        } catch { toast.error('Failed to load earnings data'); }
        finally { setLoading(false); }
    };

    const handleRequestPayment = async (assignmentId, amount) => {
        setRequestingPayment(assignmentId);
        try {
            const response = await paymentApi.requestPayment(assignmentId, amount, '');
            if (response.success) { toast.success('Payment request submitted'); loadData(); }
            else { toast.error(response.message); }
        } catch { toast.error('Failed to request payment'); }
        finally { setRequestingPayment(null); }
    };

    const tabs = [
        { id: 'overview', label: 'Overview' },
        { id: 'history', label: 'Payment History' },
        { id: 'pending', label: 'Pending Payments' },
    ];

    if (loading) {
        return (
            <div className="min-h-screen bg-gradient-to-br from-rose-50/30 via-white to-amber-50/30 flex items-center justify-center">
                <div className="text-center">
                    <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-rose-600 mx-auto mb-4"></div>
                    <p className="text-neutral-600 text-sm">Loading earnings...</p>
                </div>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-gradient-to-br from-rose-50/30 via-white to-amber-50/30">
            <SupervisorNavHeader activePath="/supervisor/earnings" />

            {/* Header */}
            <div className="bg-gradient-to-r from-green-50 to-emerald-50 border-b-2 border-neutral-100">
                <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                    <div className="flex items-center gap-3">
                        <div className="w-12 h-12 bg-green-100 rounded-xl flex items-center justify-center">
                            <IndianRupee className="w-7 h-7 text-green-600" />
                        </div>
                        <div>
                            <h1 className="text-3xl font-bold text-neutral-800">Earnings & Payments</h1>
                            <p className="text-neutral-600 text-sm mt-1">Track your earnings and payment status</p>
                        </div>
                    </div>
                </div>
            </div>

            <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-6">
                {/* Earnings Summary Cards */}
                {earnings && (
                    <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                        <div className="bg-white rounded-xl border-2 border-neutral-100 shadow-sm p-5 hover:shadow-md transition-shadow">
                            <div className="flex items-center gap-2.5 mb-3">
                                <div className="w-9 h-9 bg-green-100 rounded-lg flex items-center justify-center">
                                    <IndianRupee className="w-4.5 h-4.5 text-green-600" />
                                </div>
                                <span className="text-sm font-medium text-neutral-500">Total Earned</span>
                            </div>
                            <p className="text-2xl font-bold text-neutral-800">
                                Rs. {(earnings.totalEarned || 0).toLocaleString()}
                            </p>
                        </div>
                        <div className="bg-white rounded-xl border-2 border-neutral-100 shadow-sm p-5 hover:shadow-md transition-shadow">
                            <div className="flex items-center gap-2.5 mb-3">
                                <div className="w-9 h-9 bg-blue-100 rounded-lg flex items-center justify-center">
                                    <CheckCircle2 className="w-4.5 h-4.5 text-blue-600" />
                                </div>
                                <span className="text-sm font-medium text-neutral-500">Paid</span>
                            </div>
                            <p className="text-2xl font-bold text-blue-600">
                                Rs. {(earnings.totalPaid || 0).toLocaleString()}
                            </p>
                        </div>
                        <div className="bg-white rounded-xl border-2 border-neutral-100 shadow-sm p-5 hover:shadow-md transition-shadow">
                            <div className="flex items-center gap-2.5 mb-3">
                                <div className="w-9 h-9 bg-amber-100 rounded-lg flex items-center justify-center">
                                    <Clock className="w-4.5 h-4.5 text-amber-600" />
                                </div>
                                <span className="text-sm font-medium text-neutral-500">Pending</span>
                            </div>
                            <p className="text-2xl font-bold text-amber-600">
                                Rs. {(earnings.totalPending || 0).toLocaleString()}
                            </p>
                        </div>
                        <div className="bg-white rounded-xl border-2 border-neutral-100 shadow-sm p-5 hover:shadow-md transition-shadow">
                            <div className="flex items-center gap-2.5 mb-3">
                                <div className="w-9 h-9 bg-purple-100 rounded-lg flex items-center justify-center">
                                    <TrendingUp className="w-4.5 h-4.5 text-purple-600" />
                                </div>
                                <span className="text-sm font-medium text-neutral-500">This Month</span>
                            </div>
                            <p className="text-2xl font-bold text-purple-600">
                                Rs. {(earnings.thisMonthEarnings || 0).toLocaleString()}
                            </p>
                        </div>
                    </div>
                )}

                {/* Tabs */}
                <div className="bg-white rounded-xl border-2 border-neutral-100 shadow-sm overflow-hidden">
                    <div className="border-b-2 border-neutral-100">
                        <div className="flex">
                            {tabs.map((tab) => (
                                <button
                                    key={tab.id}
                                    onClick={() => setActiveTab(tab.id)}
                                    className={`flex-1 py-3.5 text-sm font-semibold border-b-2 transition-all duration-200 ${
                                        activeTab === tab.id
                                            ? 'border-rose-500 text-rose-600 bg-rose-50/50'
                                            : 'border-transparent text-neutral-500 hover:text-neutral-700 hover:bg-neutral-50'
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
                                        <div key={i} className="flex items-center justify-between p-4 bg-neutral-50 rounded-xl hover:bg-neutral-100 transition-colors">
                                            <div>
                                                <p className="text-sm font-semibold text-neutral-800">
                                                    {payment.eventName || `Assignment #${payment.assignmentId}`}
                                                </p>
                                                <p className="text-xs text-neutral-500 mt-0.5">
                                                    {payment.date ? new Date(payment.date).toLocaleDateString() : 'N/A'}
                                                </p>
                                            </div>
                                            <div className="text-right">
                                                <p className="text-sm font-bold text-neutral-800">
                                                    Rs. {(payment.amount || 0).toLocaleString()}
                                                </p>
                                                <div className="mt-1">
                                                    <PaymentStatusBadge status={payment.status} />
                                                </div>
                                            </div>
                                        </div>
                                    ))
                                ) : (
                                    <div className="text-center py-12">
                                        <div className="mx-auto w-16 h-16 bg-neutral-100 rounded-full flex items-center justify-center mb-4">
                                            <IndianRupee className="w-8 h-8 text-neutral-400" />
                                        </div>
                                        <p className="text-lg font-semibold text-neutral-700">No recent payments</p>
                                        <p className="text-sm text-neutral-500 mt-1">Your recent payment activity will appear here</p>
                                    </div>
                                )}
                            </div>
                        )}

                        {activeTab === 'history' && (
                            <div className="space-y-3">
                                {history.length > 0 ? (
                                    history.map((item, i) => (
                                        <div key={i} className="flex items-center justify-between p-4 bg-neutral-50 rounded-xl hover:bg-neutral-100 transition-colors">
                                            <div>
                                                <p className="text-sm font-semibold text-neutral-800">
                                                    {item.eventName || `Assignment #${item.assignmentId}`}
                                                </p>
                                                <p className="text-xs text-neutral-500 mt-0.5">
                                                    {item.completedDate ? new Date(item.completedDate).toLocaleDateString() : 'N/A'}
                                                </p>
                                            </div>
                                            <div className="text-right">
                                                <p className="text-sm font-bold text-green-700">
                                                    Rs. {(item.amount || 0).toLocaleString()}
                                                </p>
                                                <div className="mt-1">
                                                    <PaymentStatusBadge status={item.paymentStatus} />
                                                </div>
                                            </div>
                                        </div>
                                    ))
                                ) : (
                                    <div className="text-center py-12">
                                        <div className="mx-auto w-16 h-16 bg-neutral-100 rounded-full flex items-center justify-center mb-4">
                                            <Clock className="w-8 h-8 text-neutral-400" />
                                        </div>
                                        <p className="text-lg font-semibold text-neutral-700">No payment history yet</p>
                                        <p className="text-sm text-neutral-500 mt-1">Completed payment records will show here</p>
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
                                            <div key={i} className="flex items-center justify-between p-4 bg-amber-50 rounded-xl border-2 border-amber-200">
                                                <div>
                                                    <p className="text-sm font-semibold text-neutral-800">
                                                        {item.eventName || `Assignment #${item.assignmentId}`}
                                                    </p>
                                                    <p className="text-sm font-bold text-amber-700 mt-0.5">
                                                        Rs. {(item.amount || 0).toLocaleString()}
                                                    </p>
                                                </div>
                                                <div>
                                                    {item.paymentStatus === 'PENDING' ? (
                                                        <button
                                                            onClick={() => handleRequestPayment(item.assignmentId, item.amount)}
                                                            disabled={requestingPayment === item.assignmentId}
                                                            className="px-4 py-2 text-sm bg-gradient-to-r from-rose-600 to-rose-500 text-white rounded-xl font-semibold shadow-lg hover:shadow-xl transition-all disabled:opacity-50 disabled:cursor-not-allowed"
                                                        >
                                                            {requestingPayment === item.assignmentId ? 'Requesting...' : 'Request Payment'}
                                                        </button>
                                                    ) : (
                                                        <span className="text-xs px-3 py-1.5 bg-amber-100 text-amber-800 rounded-full font-semibold flex items-center gap-1">
                                                            <Clock className="w-3 h-3" />
                                                            Awaiting Approval
                                                        </span>
                                                    )}
                                                </div>
                                            </div>
                                        ))
                                ) : (
                                    <div className="text-center py-12">
                                        <div className="mx-auto w-16 h-16 bg-neutral-100 rounded-full flex items-center justify-center mb-4">
                                            <CheckCircle2 className="w-8 h-8 text-neutral-400" />
                                        </div>
                                        <p className="text-lg font-semibold text-neutral-700">No pending payments</p>
                                        <p className="text-sm text-neutral-500 mt-1">All payments are up to date</p>
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
