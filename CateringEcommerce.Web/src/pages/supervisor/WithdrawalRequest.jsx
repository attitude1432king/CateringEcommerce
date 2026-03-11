/**
 * WithdrawalRequest Page (Modern UI)
 * Supervisor earnings overview + payment withdrawal requests
 * Uses paymentApi for all API calls
 */

import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
    IndianRupee, ArrowLeft, Clock, CheckCircle2, XCircle, AlertTriangle,
    Wallet, ArrowUpRight, Calendar, MapPin, Building, Send,
} from 'lucide-react';
import { paymentApi } from '../../services/api/supervisor';
import { SupervisorNavHeader } from './SupervisorDashboard';
import toast from 'react-hot-toast';

const WithdrawalRequest = () => {
    const navigate = useNavigate();
    const [earnings, setEarnings] = useState(null);
    const [history, setHistory] = useState([]);
    const [loading, setLoading] = useState(true);
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
            if (earningsRes.success) setEarnings(earningsRes.data);
            if (historyRes.success) setHistory(historyRes.data?.payments || historyRes.data || []);
        } catch {
            toast.error('Failed to load earnings data');
        } finally {
            setLoading(false);
        }
    };

    const handleRequestWithdrawal = async (assignmentId, amount) => {
        setRequestingPayment(assignmentId);
        try {
            const result = await paymentApi.requestPayment(assignmentId, amount, '');
            if (result.success) {
                toast.success(result.data?.message || 'Withdrawal request submitted!');
                loadData(); // Refresh data
            } else {
                toast.error(result.message || 'Request failed');
            }
        } catch {
            toast.error('Failed to submit withdrawal request');
        } finally {
            setRequestingPayment(null);
        }
    };

    const getStatusBadge = (status) => {
        const styles = {
            NOT_REQUESTED: 'bg-neutral-100 text-neutral-600 border-neutral-200',
            PENDING: 'bg-amber-50 text-amber-700 border-amber-200',
            APPROVED: 'bg-blue-50 text-blue-700 border-blue-200',
            RELEASED: 'bg-green-50 text-green-700 border-green-200',
            REJECTED: 'bg-red-50 text-red-700 border-red-200',
        };
        const icons = {
            NOT_REQUESTED: Wallet,
            PENDING: Clock,
            APPROVED: CheckCircle2,
            RELEASED: CheckCircle2,
            REJECTED: XCircle,
        };
        const Icon = icons[status] || Clock;
        return (
            <span className={`inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-semibold border ${styles[status] || styles.PENDING}`}>
                <Icon className="w-3 h-3" />
                {status?.replace('_', ' ')}
            </span>
        );
    };

    if (loading) {
        return (
            <div className="min-h-screen bg-gradient-to-br from-neutral-50 to-rose-50">
                <SupervisorNavHeader activePath="earnings" />
                <div className="flex items-center justify-center h-96">
                    <div className="w-10 h-10 border-4 border-rose-200 border-t-rose-600 rounded-full animate-spin" />
                </div>
            </div>
        );
    }

    const notRequestedPayments = history.filter((p) => p.paymentStatus === 'NOT_REQUESTED');
    const pendingPayments = history.filter((p) => p.paymentStatus === 'PENDING');
    const completedPayments = history.filter((p) => ['APPROVED', 'RELEASED'].includes(p.paymentStatus));

    return (
        <div className="min-h-screen bg-gradient-to-br from-neutral-50 to-rose-50">
            <SupervisorNavHeader activePath="earnings" />

            <div className="max-w-6xl mx-auto px-4 py-6 space-y-6">
                {/* Page Header */}
                <div className="flex items-center gap-4">
                    <button
                        onClick={() => navigate('/supervisor/dashboard')}
                        className="p-2 bg-white rounded-lg border-2 border-neutral-200 hover:border-neutral-300 transition-colors"
                    >
                        <ArrowLeft className="w-5 h-5 text-neutral-600" />
                    </button>
                    <div>
                        <h1 className="text-2xl font-bold text-neutral-800">Withdrawal Requests</h1>
                        <p className="text-sm text-neutral-500">Manage your earnings and request payment releases</p>
                    </div>
                </div>

                {/* Earnings Summary Cards */}
                {earnings && (
                    <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                        <div className="bg-white rounded-xl border-2 border-neutral-100 shadow-sm p-5">
                            <div className="flex items-center gap-3 mb-3">
                                <div className="w-9 h-9 bg-rose-100 rounded-lg flex items-center justify-center">
                                    <IndianRupee className="w-4 h-4 text-rose-600" />
                                </div>
                                <span className="text-xs font-semibold text-neutral-500 uppercase tracking-wide">Total</span>
                            </div>
                            <p className="text-2xl font-bold text-neutral-800">
                                {'\u20B9'}{(earnings.totalEarnings || 0).toLocaleString('en-IN')}
                            </p>
                        </div>

                        <div className="bg-white rounded-xl border-2 border-neutral-100 shadow-sm p-5">
                            <div className="flex items-center gap-3 mb-3">
                                <div className="w-9 h-9 bg-green-100 rounded-lg flex items-center justify-center">
                                    <CheckCircle2 className="w-4 h-4 text-green-600" />
                                </div>
                                <span className="text-xs font-semibold text-neutral-500 uppercase tracking-wide">Released</span>
                            </div>
                            <p className="text-2xl font-bold text-green-700">
                                {'\u20B9'}{(earnings.releasedPayments || 0).toLocaleString('en-IN')}
                            </p>
                        </div>

                        <div className="bg-white rounded-xl border-2 border-neutral-100 shadow-sm p-5">
                            <div className="flex items-center gap-3 mb-3">
                                <div className="w-9 h-9 bg-amber-100 rounded-lg flex items-center justify-center">
                                    <Clock className="w-4 h-4 text-amber-600" />
                                </div>
                                <span className="text-xs font-semibold text-neutral-500 uppercase tracking-wide">Pending</span>
                            </div>
                            <p className="text-2xl font-bold text-amber-700">
                                {'\u20B9'}{(earnings.pendingPayments || 0).toLocaleString('en-IN')}
                            </p>
                        </div>

                        <div className="bg-white rounded-xl border-2 border-neutral-100 shadow-sm p-5">
                            <div className="flex items-center gap-3 mb-3">
                                <div className="w-9 h-9 bg-blue-100 rounded-lg flex items-center justify-center">
                                    <Wallet className="w-4 h-4 text-blue-600" />
                                </div>
                                <span className="text-xs font-semibold text-neutral-500 uppercase tracking-wide">Available</span>
                            </div>
                            <p className="text-2xl font-bold text-blue-700">
                                {'\u20B9'}{(earnings.notRequestedPayments || 0).toLocaleString('en-IN')}
                            </p>
                        </div>
                    </div>
                )}

                {/* Available for Withdrawal */}
                {notRequestedPayments.length > 0 && (
                    <div className="bg-white rounded-xl border-2 border-neutral-100 shadow-sm">
                        <div className="px-6 py-4 border-b-2 border-neutral-100">
                            <div className="flex items-center gap-3">
                                <div className="w-8 h-8 bg-blue-100 rounded-lg flex items-center justify-center">
                                    <ArrowUpRight className="w-4 h-4 text-blue-600" />
                                </div>
                                <div>
                                    <h2 className="text-lg font-bold text-neutral-800">Available for Withdrawal</h2>
                                    <p className="text-xs text-neutral-500">{notRequestedPayments.length} payment(s) ready to request</p>
                                </div>
                            </div>
                        </div>
                        <div className="divide-y-2 divide-neutral-50">
                            {notRequestedPayments.map((payment) => (
                                <div key={payment.assignmentId} className="px-6 py-4 flex flex-col sm:flex-row sm:items-center justify-between gap-4">
                                    <div className="flex-1 space-y-1">
                                        <p className="font-semibold text-neutral-800">
                                            Assignment #{payment.assignmentNumber || payment.assignmentId}
                                        </p>
                                        <div className="flex flex-wrap items-center gap-3 text-xs text-neutral-500">
                                            {payment.eventDate && (
                                                <span className="flex items-center gap-1">
                                                    <Calendar className="w-3 h-3" />
                                                    {new Date(payment.eventDate).toLocaleDateString('en-IN')}
                                                </span>
                                            )}
                                            {payment.eventLocation && (
                                                <span className="flex items-center gap-1">
                                                    <MapPin className="w-3 h-3" />
                                                    {payment.eventLocation}
                                                </span>
                                            )}
                                        </div>
                                    </div>
                                    <div className="flex items-center gap-4">
                                        <p className="text-lg font-bold text-neutral-800">
                                            {'\u20B9'}{(payment.supervisorFee || 0).toLocaleString('en-IN')}
                                        </p>
                                        <button
                                            onClick={() => handleRequestWithdrawal(payment.assignmentId, payment.supervisorFee)}
                                            disabled={requestingPayment === payment.assignmentId}
                                            className="px-5 py-2.5 bg-gradient-to-r from-rose-600 to-rose-500 text-white rounded-xl font-semibold text-sm shadow-lg hover:shadow-xl transition-all duration-200 flex items-center gap-2 disabled:opacity-50"
                                        >
                                            {requestingPayment === payment.assignmentId ? (
                                                <>
                                                    <div className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                                                    Requesting...
                                                </>
                                            ) : (
                                                <>
                                                    <Send className="w-4 h-4" />
                                                    Request
                                                </>
                                            )}
                                        </button>
                                    </div>
                                </div>
                            ))}
                        </div>
                    </div>
                )}

                {/* Pending Approvals */}
                {pendingPayments.length > 0 && (
                    <div className="bg-white rounded-xl border-2 border-neutral-100 shadow-sm">
                        <div className="px-6 py-4 border-b-2 border-neutral-100">
                            <div className="flex items-center gap-3">
                                <div className="w-8 h-8 bg-amber-100 rounded-lg flex items-center justify-center">
                                    <Clock className="w-4 h-4 text-amber-600" />
                                </div>
                                <div>
                                    <h2 className="text-lg font-bold text-neutral-800">Pending Approval</h2>
                                    <p className="text-xs text-neutral-500">{pendingPayments.length} request(s) awaiting admin approval</p>
                                </div>
                            </div>
                        </div>
                        <div className="divide-y-2 divide-neutral-50">
                            {pendingPayments.map((payment) => (
                                <div key={payment.assignmentId} className="px-6 py-4 flex flex-col sm:flex-row sm:items-center justify-between gap-4">
                                    <div className="flex-1 space-y-1">
                                        <p className="font-semibold text-neutral-800">
                                            Assignment #{payment.assignmentNumber || payment.assignmentId}
                                        </p>
                                        <div className="flex flex-wrap items-center gap-3 text-xs text-neutral-500">
                                            {payment.eventDate && (
                                                <span className="flex items-center gap-1">
                                                    <Calendar className="w-3 h-3" />
                                                    {new Date(payment.eventDate).toLocaleDateString('en-IN')}
                                                </span>
                                            )}
                                            {payment.eventLocation && (
                                                <span className="flex items-center gap-1">
                                                    <MapPin className="w-3 h-3" />
                                                    {payment.eventLocation}
                                                </span>
                                            )}
                                        </div>
                                    </div>
                                    <div className="flex items-center gap-4">
                                        <p className="text-lg font-bold text-amber-700">
                                            {'\u20B9'}{(payment.supervisorFee || 0).toLocaleString('en-IN')}
                                        </p>
                                        {getStatusBadge('PENDING')}
                                    </div>
                                </div>
                            ))}
                        </div>
                        <div className="px-6 py-3 bg-amber-50 border-t-2 border-amber-100 rounded-b-xl">
                            <p className="text-xs text-amber-700 flex items-center gap-2">
                                <AlertTriangle className="w-3.5 h-3.5" />
                                Pending requests are reviewed by admin. You'll be notified once approved.
                            </p>
                        </div>
                    </div>
                )}

                {/* Completed Payments */}
                {completedPayments.length > 0 && (
                    <div className="bg-white rounded-xl border-2 border-neutral-100 shadow-sm">
                        <div className="px-6 py-4 border-b-2 border-neutral-100">
                            <div className="flex items-center gap-3">
                                <div className="w-8 h-8 bg-green-100 rounded-lg flex items-center justify-center">
                                    <CheckCircle2 className="w-4 h-4 text-green-600" />
                                </div>
                                <div>
                                    <h2 className="text-lg font-bold text-neutral-800">Completed Payments</h2>
                                    <p className="text-xs text-neutral-500">{completedPayments.length} payment(s) released</p>
                                </div>
                            </div>
                        </div>
                        <div className="divide-y-2 divide-neutral-50">
                            {completedPayments.map((payment) => (
                                <div key={payment.assignmentId} className="px-6 py-4 flex flex-col sm:flex-row sm:items-center justify-between gap-4">
                                    <div className="flex-1 space-y-1">
                                        <p className="font-semibold text-neutral-800">
                                            Assignment #{payment.assignmentNumber || payment.assignmentId}
                                        </p>
                                        <div className="flex flex-wrap items-center gap-3 text-xs text-neutral-500">
                                            {payment.eventDate && (
                                                <span className="flex items-center gap-1">
                                                    <Calendar className="w-3 h-3" />
                                                    {new Date(payment.eventDate).toLocaleDateString('en-IN')}
                                                </span>
                                            )}
                                            {payment.eventLocation && (
                                                <span className="flex items-center gap-1">
                                                    <MapPin className="w-3 h-3" />
                                                    {payment.eventLocation}
                                                </span>
                                            )}
                                        </div>
                                    </div>
                                    <div className="flex items-center gap-4">
                                        <p className="text-lg font-bold text-green-700">
                                            {'\u20B9'}{(payment.supervisorFee || 0).toLocaleString('en-IN')}
                                        </p>
                                        {getStatusBadge(payment.paymentStatus)}
                                    </div>
                                </div>
                            ))}
                        </div>
                    </div>
                )}

                {/* Empty State */}
                {history.length === 0 && (
                    <div className="text-center py-16 bg-white rounded-xl border-2 border-neutral-100 shadow-sm">
                        <Wallet className="w-16 h-16 text-neutral-200 mx-auto mb-4" />
                        <h3 className="text-lg font-bold text-neutral-800 mb-2">No Payments Yet</h3>
                        <p className="text-sm text-neutral-500 max-w-md mx-auto">
                            Complete event supervision assignments to start earning. Your payments will appear here once events are completed.
                        </p>
                    </div>
                )}

                {/* Info Box */}
                <div className="bg-blue-50 border-l-4 border-blue-400 rounded-lg p-4">
                    <p className="text-sm text-blue-800">
                        <strong>Payment Process:</strong> After completing an event, request a withdrawal.
                        Career supervisors with FULL authority get instant release. Others require admin approval.
                    </p>
                </div>
            </div>
        </div>
    );
};

export default WithdrawalRequest;
