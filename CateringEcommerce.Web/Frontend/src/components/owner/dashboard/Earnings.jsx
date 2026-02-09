/*
========================================
File: src/components/owner/dashboard/Earnings.jsx
Financial Dashboard with Real Data & Chart.js
========================================
*/
import React, { useState, useEffect } from 'react';
import { ownerApiService } from '../../../services/ownerApi';
import RevenueChart from './charts/RevenueChart';
import RevenueBreakdownChart from './charts/RevenueBreakdownChart';
import { formatCurrency, formatDate, exportToCSV } from '../../../utils/exportUtils';

// Stat Card for Earnings
const EarningStatCard = ({ title, value, change, icon, iconBg, iconColor, subtitle, isLoading }) => (
    <div className="bg-white rounded-2xl shadow-sm border border-neutral-100 p-6 hover:shadow-md transition-shadow">
        <div className="flex items-start justify-between mb-4">
            <div className={`p-3 rounded-xl ${iconBg}`}>
                <div className={iconColor}>
                    {icon}
                </div>
            </div>
            {!isLoading && change !== undefined && (
                <div className={`flex items-center gap-1 px-2 py-1 rounded-lg ${
                    change >= 0 ? 'bg-green-100' : 'bg-red-100'
                }`}>
                    <svg className={`w-4 h-4 ${change >= 0 ? 'text-green-600' : 'text-red-600'}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d={change >= 0 ? 'M13 7h8m0 0v8m0-8l-8 8-4-4-6 6' : 'M13 17h8m0 0V9m0 8l-8-8-4 4-6-6'} />
                    </svg>
                    <span className={`text-xs font-bold ${change >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                        {change >= 0 ? '+' : ''}{change.toFixed(1)}%
                    </span>
                </div>
            )}
        </div>
        <p className="text-sm font-medium text-neutral-500 mb-1">{title}</p>
        {isLoading ? (
            <div className="animate-pulse">
                <div className="h-8 bg-gray-200 rounded w-32 mb-1"></div>
                {subtitle && <div className="h-4 bg-gray-200 rounded w-24"></div>}
            </div>
        ) : (
            <>
                <h3 className="text-3xl font-bold text-neutral-900 mb-1">{value}</h3>
                {subtitle && <p className="text-xs text-neutral-500">{subtitle}</p>}
            </>
        )}
    </div>
);

// Transaction Row Component
const TransactionRow = ({ transaction }) => {
    const statusColors = {
        Completed: 'bg-green-100 text-green-800 border-green-200',
        Pending: 'bg-yellow-100 text-yellow-800 border-yellow-200',
        Failed: 'bg-red-100 text-red-800 border-red-200',
    };

    const isCredit = transaction.type === 'credit' || transaction.paidAmount > 0;

    return (
        <div className="flex items-center justify-between p-4 hover:bg-neutral-50 rounded-xl transition-colors border-b border-neutral-100 last:border-b-0">
            <div className="flex items-center gap-4 flex-1">
                <div className={`p-3 rounded-xl ${isCredit ? 'bg-green-100' : 'bg-orange-100'}`}>
                    <svg className={`w-5 h-5 ${isCredit ? 'text-green-600' : 'text-orange-600'}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        {isCredit ? (
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                        ) : (
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z" />
                        )}
                    </svg>
                </div>
                <div className="flex-1">
                    <p className="font-semibold text-neutral-900">{transaction.description || `Order ${transaction.orderNumber}`}</p>
                    <p className="text-sm text-neutral-500">{transaction.orderNumber} • {formatDate(transaction.date, 'medium')}</p>
                </div>
            </div>
            <div className="flex items-center gap-4">
                <div className="text-right">
                    <p className={`text-lg font-bold ${isCredit ? 'text-green-600' : 'text-orange-600'}`}>
                        {isCredit ? '+' : '-'}{formatCurrency(transaction.amount)}
                    </p>
                    <span className={`inline-block px-2 py-0.5 rounded-full text-xs font-semibold border ${statusColors[transaction.status] || 'bg-gray-100 text-gray-800'}`}>
                        {transaction.status}
                    </span>
                </div>
            </div>
        </div>
    );
};

export default function Earnings() {
    const [timePeriod, setTimePeriod] = useState('month');
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    // State for earnings data
    const [revenueReport, setRevenueReport] = useState(null);
    const [revenueChartData, setRevenueChartData] = useState(null);
    const [revenueBreakdown, setRevenueBreakdown] = useState(null);
    const [transactions, setTransactions] = useState([]);

    // Fetch earnings data
    useEffect(() => {
        fetchEarningsData();
    }, [timePeriod]);

    const fetchEarningsData = async () => {
        try {
            setLoading(true);
            setError(null);

            // Calculate date range based on period
            const endDate = new Date();
            const startDate = new Date();

            switch (timePeriod) {
                case 'week':
                    startDate.setDate(endDate.getDate() - 7);
                    break;
                case 'month':
                    startDate.setMonth(endDate.getMonth() - 1);
                    break;
                case '3months':
                    startDate.setMonth(endDate.getMonth() - 3);
                    break;
                case 'year':
                    startDate.setFullYear(endDate.getFullYear() - 1);
                    break;
                default:
                    startDate.setMonth(endDate.getMonth() - 1);
            }

            const filters = {
                startDate: startDate.toISOString(),
                endDate: endDate.toISOString()
            };

            // Fetch all earnings data in parallel
            const [revenueReportData, revenueChartData, revenueBreakdownData, ordersData] = await Promise.all([
                ownerApiService.generateRevenueReport(filters),
                ownerApiService.getRevenueChart(timePeriod === 'week' ? 'day' : timePeriod === 'year' ? 'month' : 'month'),
                ownerApiService.getRevenueBreakdown(),
                ownerApiService.getOrdersList(1, 10, { orderStatus: 'Completed', sortBy: 'OrderDate', sortOrder: 'DESC' })
            ]);

            if (revenueReportData.success) setRevenueReport(revenueReportData.data);
            if (revenueChartData.success) setRevenueChartData(revenueChartData.data);
            if (revenueBreakdownData.success) {
                const breakdown = revenueBreakdownData.data;
                setRevenueBreakdown({
                    labels: Object.keys(breakdown.byEventType || {}),
                    data: Object.values(breakdown.byEventType || {})
                });
            }
            if (ordersData.success && ordersData.data.orders) {
                // Transform orders to transaction format
                const orderTransactions = ordersData.data.orders.map(order => ({
                    orderNumber: order.orderNumber,
                    description: `${order.eventType} - ${order.customerName}`,
                    date: order.orderDate,
                    amount: order.totalAmount,
                    paidAmount: order.paidAmount,
                    type: 'credit',
                    status: order.paymentStatus
                }));
                setTransactions(orderTransactions);
            }

        } catch (err) {
            console.error('Error fetching earnings data:', err);
            setError('Failed to load earnings data');
        } finally {
            setLoading(false);
        }
    };

    const handleExportTransactions = () => {
        if (transactions.length === 0) return;

        const exportData = transactions.map(t => ({
            'Order Number': t.orderNumber,
            'Description': t.description,
            'Date': formatDate(t.date, 'medium'),
            'Amount': t.amount,
            'Status': t.status
        }));

        exportToCSV(exportData, `earnings_transactions_${new Date().toISOString().split('T')[0]}.csv`);
    };

    return (
        <div className="min-h-screen bg-neutral-50">
            <div className="p-4 sm:p-6 lg:p-8 space-y-6">
                {/* Header */}
                <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
                    <div>
                        <h1 className="text-3xl font-bold text-neutral-900">Earnings & Revenue</h1>
                        <p className="text-neutral-600 mt-1">Track your financial performance</p>
                    </div>

                    {/* Time Period Selector */}
                    <div className="flex items-center gap-2 bg-white rounded-xl p-1 shadow-sm border border-neutral-200">
                        {[
                            { value: 'week', label: 'Week' },
                            { value: 'month', label: 'Month' },
                            { value: '3months', label: '3 Months' },
                            { value: 'year', label: 'Year' }
                        ].map(period => (
                            <button
                                key={period.value}
                                onClick={() => setTimePeriod(period.value)}
                                className={`px-4 py-2 rounded-lg text-sm font-semibold transition-all ${
                                    timePeriod === period.value
                                        ? 'bg-indigo-600 text-white shadow-md'
                                        : 'text-neutral-700 hover:bg-neutral-100'
                                }`}
                            >
                                {period.label}
                            </button>
                        ))}
                    </div>
                </div>

                {/* Error Message */}
                {error && (
                    <div className="bg-red-50 border border-red-200 rounded-xl p-4">
                        <p className="text-red-800">{error}</p>
                        <button
                            onClick={fetchEarningsData}
                            className="mt-2 text-sm font-semibold text-red-600 hover:text-red-700"
                        >
                            Retry
                        </button>
                    </div>
                )}

                {/* Stats Grid */}
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
                    <EarningStatCard
                        title="Gross Revenue"
                        value={revenueReport ? formatCurrency(revenueReport.grossRevenue) : '₹0'}
                        change={revenueReport?.revenueGrowth}
                        icon={
                            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                            </svg>
                        }
                        iconBg="bg-green-100"
                        iconColor="text-green-600"
                        subtitle="Total revenue earned"
                        isLoading={loading}
                    />

                    <EarningStatCard
                        title="Net Revenue"
                        value={revenueReport ? formatCurrency(revenueReport.netRevenue) : '₹0'}
                        icon={
                            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 8h6m-5 0a3 3 0 110 6H9l3 3m-3-6h6m6 1a9 9 0 11-18 0 9 9 0 0118 0z" />
                            </svg>
                        }
                        iconBg="bg-blue-100"
                        iconColor="text-blue-600"
                        subtitle="After taxes & discounts"
                        isLoading={loading}
                    />

                    <EarningStatCard
                        title="Pending Payments"
                        value={revenueReport ? formatCurrency(revenueReport.pendingPayments) : '₹0'}
                        icon={
                            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                            </svg>
                        }
                        iconBg="bg-orange-100"
                        iconColor="text-orange-600"
                        subtitle="Outstanding amount"
                        isLoading={loading}
                    />

                    <EarningStatCard
                        title="Tax Collected"
                        value={revenueReport ? formatCurrency(revenueReport.totalTax) : '₹0'}
                        icon={
                            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 7h6m0 10v-3m-3 3h.01M9 17h.01M9 14h.01M12 14h.01M15 11h.01M12 11h.01M9 11h.01M7 21h10a2 2 0 002-2V5a2 2 0 00-2-2H7a2 2 0 00-2 2v14a2 2 0 002 2z" />
                            </svg>
                        }
                        iconBg="bg-purple-100"
                        iconColor="text-purple-600"
                        subtitle="Total tax amount"
                        isLoading={loading}
                    />
                </div>

                {/* Charts Section */}
                <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                    {/* Revenue Trend Chart */}
                    <div className="lg:col-span-2 bg-white rounded-2xl shadow-sm border border-neutral-100 p-6">
                        <h2 className="text-lg font-bold text-neutral-900 mb-4">Revenue Trend</h2>
                        {loading ? (
                            <div className="h-80 flex items-center justify-center">
                                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600"></div>
                            </div>
                        ) : (
                            <RevenueChart
                                data={revenueChartData}
                                period={timePeriod === 'week' ? 'day' : timePeriod === 'year' ? 'month' : 'month'}
                                height={320}
                            />
                        )}
                    </div>

                    {/* Revenue Breakdown Chart */}
                    <div className="bg-white rounded-2xl shadow-sm border border-neutral-100 p-6">
                        <h2 className="text-lg font-bold text-neutral-900 mb-4">Revenue by Event Type</h2>
                        {loading ? (
                            <div className="h-80 flex items-center justify-center">
                                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600"></div>
                            </div>
                        ) : (
                            <RevenueBreakdownChart
                                data={revenueBreakdown}
                                title=""
                                height={320}
                            />
                        )}
                    </div>
                </div>

                {/* Monthly Revenue Table */}
                {revenueReport && revenueReport.monthlyRevenue && revenueReport.monthlyRevenue.length > 0 && (
                    <div className="bg-white rounded-2xl shadow-sm border border-neutral-100 p-6">
                        <div className="flex items-center justify-between mb-4">
                            <h2 className="text-lg font-bold text-neutral-900">Monthly Revenue Breakdown</h2>
                            <button
                                onClick={() => exportToCSV(
                                    revenueReport.monthlyRevenue.map(m => ({
                                        'Month': m.month,
                                        'Year': m.year,
                                        'Gross Revenue': m.grossRevenue,
                                        'Net Revenue': m.netRevenue,
                                        'Tax': m.taxAmount,
                                        'Discounts': m.discountAmount,
                                        'Orders': m.orderCount
                                    })),
                                    'monthly_revenue.csv'
                                )}
                                className="text-sm font-semibold text-indigo-600 hover:text-indigo-700"
                            >
                                Export CSV
                            </button>
                        </div>
                        <div className="overflow-x-auto">
                            <table className="w-full">
                                <thead>
                                    <tr className="border-b-2 border-neutral-200">
                                        <th className="text-left py-3 px-4 font-semibold text-neutral-700">Month</th>
                                        <th className="text-right py-3 px-4 font-semibold text-neutral-700">Orders</th>
                                        <th className="text-right py-3 px-4 font-semibold text-neutral-700">Gross Revenue</th>
                                        <th className="text-right py-3 px-4 font-semibold text-neutral-700">Tax</th>
                                        <th className="text-right py-3 px-4 font-semibold text-neutral-700">Discounts</th>
                                        <th className="text-right py-3 px-4 font-semibold text-neutral-700">Net Revenue</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {revenueReport.monthlyRevenue.map((month, index) => (
                                        <tr key={index} className="border-b border-neutral-100 hover:bg-neutral-50">
                                            <td className="py-3 px-4 font-medium text-neutral-900">{month.month} {month.year}</td>
                                            <td className="py-3 px-4 text-right text-neutral-700">{month.orderCount}</td>
                                            <td className="py-3 px-4 text-right font-semibold text-green-600">{formatCurrency(month.grossRevenue)}</td>
                                            <td className="py-3 px-4 text-right text-neutral-700">{formatCurrency(month.taxAmount)}</td>
                                            <td className="py-3 px-4 text-right text-neutral-700">{formatCurrency(month.discountAmount)}</td>
                                            <td className="py-3 px-4 text-right font-bold text-neutral-900">{formatCurrency(month.netRevenue)}</td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    </div>
                )}

                {/* Recent Transactions */}
                <div className="bg-white rounded-2xl shadow-sm border border-neutral-100 p-6">
                    <div className="flex items-center justify-between mb-4">
                        <h2 className="text-lg font-bold text-neutral-900">Recent Transactions</h2>
                        <button
                            onClick={handleExportTransactions}
                            className="text-sm font-semibold text-indigo-600 hover:text-indigo-700"
                            disabled={transactions.length === 0}
                        >
                            Export CSV
                        </button>
                    </div>
                    <div className="space-y-2">
                        {loading ? (
                            <div className="text-center py-8">
                                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600 mx-auto"></div>
                            </div>
                        ) : transactions.length > 0 ? (
                            transactions.map((transaction, index) => (
                                <TransactionRow key={index} transaction={transaction} />
                            ))
                        ) : (
                            <div className="text-center py-8 text-neutral-500">
                                <svg className="w-16 h-16 mx-auto mb-3 text-neutral-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                                </svg>
                                <p>No transactions found</p>
                            </div>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
}
