/*
========================================
File: src/components/owner/dashboard/Earnings.jsx
Modern Redesign - Financial Dashboard with Metrics
========================================
*/
import React, { useState } from 'react';

// Stat Card for Earnings
const EarningStatCard = ({ title, value, change, icon, iconBg, iconColor, subtitle }) => (
    <div className="bg-white rounded-2xl shadow-sm border border-neutral-100 p-6 hover:shadow-md transition-shadow">
        <div className="flex items-start justify-between mb-4">
            <div className={`p-3 rounded-xl ${iconBg}`}>
                <div className={iconColor}>
                    {icon}
                </div>
            </div>
            {change !== undefined && (
                <div className={`flex items-center gap-1 px-2 py-1 rounded-lg ${
                    change >= 0 ? 'bg-green-100' : 'bg-red-100'
                }`}>
                    <svg className={`w-4 h-4 ${change >= 0 ? 'text-green-600' : 'text-red-600'}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d={change >= 0 ? 'M13 7h8m0 0v8m0-8l-8 8-4-4-6 6' : 'M13 17h8m0 0V9m0 8l-8-8-4 4-6-6'} />
                    </svg>
                    <span className={`text-xs font-bold ${change >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                        {change >= 0 ? '+' : ''}{change}%
                    </span>
                </div>
            )}
        </div>
        <p className="text-sm font-medium text-neutral-500 mb-1">{title}</p>
        <h3 className="text-3xl font-bold text-neutral-900 mb-1">{value}</h3>
        {subtitle && <p className="text-xs text-neutral-500">{subtitle}</p>}
    </div>
);

// Transaction Row Component
const TransactionRow = ({ transaction }) => {
    const statusColors = {
        completed: 'bg-green-100 text-green-800 border-green-200',
        pending: 'bg-yellow-100 text-yellow-800 border-yellow-200',
        failed: 'bg-red-100 text-red-800 border-red-200',
    };

    return (
        <div className="flex items-center justify-between p-4 hover:bg-neutral-50 rounded-xl transition-colors border-b border-neutral-100 last:border-b-0">
            <div className="flex items-center gap-4 flex-1">
                <div className={`p-3 rounded-xl ${transaction.type === 'credit' ? 'bg-green-100' : 'bg-orange-100'}`}>
                    <svg className={`w-5 h-5 ${transaction.type === 'credit' ? 'text-green-600' : 'text-orange-600'}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        {transaction.type === 'credit' ? (
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                        ) : (
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z" />
                        )}
                    </svg>
                </div>
                <div className="flex-1">
                    <p className="font-semibold text-neutral-900">{transaction.description}</p>
                    <p className="text-sm text-neutral-500">{transaction.orderId} • {transaction.date}</p>
                </div>
            </div>
            <div className="flex items-center gap-4">
                <div className="text-right">
                    <p className={`text-lg font-bold ${transaction.type === 'credit' ? 'text-green-600' : 'text-orange-600'}`}>
                        {transaction.type === 'credit' ? '+' : '-'}₹{transaction.amount.toLocaleString()}
                    </p>
                    <span className={`inline-block px-2 py-0.5 rounded-full text-xs font-semibold border ${statusColors[transaction.status]}`}>
                        {transaction.status.charAt(0).toUpperCase() + transaction.status.slice(1)}
                    </span>
                </div>
            </div>
        </div>
    );
};

// Simple Bar Chart Component (Pure CSS)
const SimpleBarChart = ({ data }) => {
    const maxValue = Math.max(...data.map(d => d.value));

    return (
        <div className="space-y-3">
            {data.map((item, index) => (
                <div key={index}>
                    <div className="flex items-center justify-between mb-1">
                        <span className="text-sm font-medium text-neutral-700">{item.label}</span>
                        <span className="text-sm font-bold text-neutral-900">₹{item.value.toLocaleString()}</span>
                    </div>
                    <div className="w-full bg-neutral-200 rounded-full h-3">
                        <div
                            className="bg-gradient-to-r from-indigo-600 to-purple-600 h-3 rounded-full transition-all duration-500"
                            style={{ width: `${(item.value / maxValue) * 100}%` }}
                        />
                    </div>
                </div>
            ))}
        </div>
    );
};

export default function Earnings() {
    const [timePeriod, setTimePeriod] = useState('this-month');

    // Mock data - replace with real API data
    const stats = {
        totalEarnings: '₹2,45,000',
        pendingPayouts: '₹45,000',
        completedOrders: '28',
        averageOrderValue: '₹8,750',
    };

    const weeklyData = [
        { label: 'Mon', value: 12000 },
        { label: 'Tue', value: 19000 },
        { label: 'Wed', value: 15000 },
        { label: 'Thu', value: 25000 },
        { label: 'Fri', value: 22000 },
        { label: 'Sat', value: 30000 },
        { label: 'Sun', value: 28000 },
    ];

    const transactions = [
        {
            id: 1,
            type: 'credit',
            description: 'Payment Received - Wedding Catering',
            orderId: '#ORD2001',
            date: 'Jan 12, 2026',
            amount: 75000,
            status: 'completed'
        },
        {
            id: 2,
            type: 'debit',
            description: 'Platform Fee',
            orderId: '#ORD2001',
            date: 'Jan 12, 2026',
            amount: 7500,
            status: 'completed'
        },
        {
            id: 3,
            type: 'credit',
            description: 'Payment Received - Corporate Event',
            orderId: '#ORD2002',
            date: 'Jan 11, 2026',
            amount: 45000,
            status: 'pending'
        },
        {
            id: 4,
            type: 'credit',
            description: 'Payment Received - Birthday Party',
            orderId: '#ORD2003',
            date: 'Jan 10, 2026',
            amount: 25000,
            status: 'completed'
        },
    ];

    const payoutBreakdown = [
        { label: 'Completed Orders', value: 245000, percentage: 85 },
        { label: 'Platform Fees', value: 24500, percentage: 8 },
        { label: 'Taxes', value: 20000, percentage: 7 },
    ];

    return (
        <div className="min-h-screen bg-neutral-50">
            <div className="p-4 sm:p-6 lg:p-8 space-y-6">
                {/* Header */}
                <div className="flex flex-col lg:flex-row justify-between items-start lg:items-center gap-4">
                    <div>
                        <h1 className="text-3xl font-bold text-neutral-900">Earnings & Payments</h1>
                        <p className="text-neutral-600 mt-1">Track your revenue and payment history</p>
                    </div>

                    {/* Time Period Filter */}
                    <div className="flex gap-2 bg-white rounded-xl p-1 shadow-sm border border-neutral-200">
                        {['today', 'this-week', 'this-month', 'this-year'].map((period) => (
                            <button
                                key={period}
                                onClick={() => setTimePeriod(period)}
                                className={`px-4 py-2 rounded-lg text-sm font-semibold transition-all ${
                                    timePeriod === period
                                        ? 'bg-indigo-600 text-white shadow-sm'
                                        : 'text-neutral-600 hover:bg-neutral-50'
                                }`}
                            >
                                {period.split('-').map(w => w.charAt(0).toUpperCase() + w.slice(1)).join(' ')}
                            </button>
                        ))}
                    </div>
                </div>

                {/* Stats Grid */}
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
                    <EarningStatCard
                        title="Total Earnings"
                        value={stats.totalEarnings}
                        change={12}
                        subtitle="This month"
                        icon={<svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>}
                        iconBg="bg-green-100"
                        iconColor="text-green-600"
                    />
                    <EarningStatCard
                        title="Pending Payouts"
                        value={stats.pendingPayouts}
                        subtitle="Available in 2 days"
                        icon={<svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>}
                        iconBg="bg-yellow-100"
                        iconColor="text-yellow-600"
                    />
                    <EarningStatCard
                        title="Completed Orders"
                        value={stats.completedOrders}
                        change={8}
                        subtitle="This month"
                        icon={<svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>}
                        iconBg="bg-blue-100"
                        iconColor="text-blue-600"
                    />
                    <EarningStatCard
                        title="Avg Order Value"
                        value={stats.averageOrderValue}
                        change={-3}
                        subtitle="Per order"
                        icon={<svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 7h8m0 0v8m0-8l-8 8-4-4-6 6" /></svg>}
                        iconBg="bg-purple-100"
                        iconColor="text-purple-600"
                    />
                </div>

                <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                    {/* Weekly Earnings Chart */}
                    <div className="lg:col-span-2 bg-white rounded-2xl shadow-sm border border-neutral-100 p-6">
                        <div className="flex items-center justify-between mb-6">
                            <div>
                                <h2 className="text-lg font-bold text-neutral-900">Weekly Earnings</h2>
                                <p className="text-sm text-neutral-500">Daily revenue breakdown</p>
                            </div>
                            <button className="text-sm font-semibold text-indigo-600 hover:text-indigo-700">
                                View Report →
                            </button>
                        </div>
                        <SimpleBarChart data={weeklyData} />
                    </div>

                    {/* Payout Breakdown */}
                    <div className="bg-white rounded-2xl shadow-sm border border-neutral-100 p-6">
                        <h2 className="text-lg font-bold text-neutral-900 mb-6">Payout Breakdown</h2>
                        <div className="space-y-4">
                            {payoutBreakdown.map((item, index) => (
                                <div key={index} className="space-y-2">
                                    <div className="flex items-center justify-between">
                                        <span className="text-sm font-medium text-neutral-700">{item.label}</span>
                                        <span className="text-sm font-bold text-neutral-900">{item.percentage}%</span>
                                    </div>
                                    <div className="flex items-center gap-3">
                                        <div className="flex-1 bg-neutral-200 rounded-full h-2">
                                            <div
                                                className={`h-2 rounded-full ${
                                                    index === 0 ? 'bg-green-500' :
                                                    index === 1 ? 'bg-orange-500' :
                                                    'bg-red-500'
                                                }`}
                                                style={{ width: `${item.percentage}%` }}
                                            />
                                        </div>
                                        <span className="text-sm font-bold text-neutral-900 min-w-[80px] text-right">
                                            ₹{item.value.toLocaleString()}
                                        </span>
                                    </div>
                                </div>
                            ))}
                        </div>

                        {/* Withdraw Button */}
                        <button className="w-full mt-6 bg-gradient-to-r from-indigo-600 to-purple-600 hover:from-indigo-700 hover:to-purple-700 text-white px-4 py-3 rounded-xl font-semibold transition-all shadow-md">
                            Withdraw Earnings
                        </button>
                    </div>
                </div>

                {/* Recent Transactions */}
                <div className="bg-white rounded-2xl shadow-sm border border-neutral-100 p-6">
                    <div className="flex items-center justify-between mb-6">
                        <div>
                            <h2 className="text-lg font-bold text-neutral-900">Recent Transactions</h2>
                            <p className="text-sm text-neutral-500">Your latest payment activity</p>
                        </div>
                        <button className="text-sm font-semibold text-indigo-600 hover:text-indigo-700">
                            View All →
                        </button>
                    </div>
                    <div className="space-y-2">
                        {transactions.map((transaction) => (
                            <TransactionRow key={transaction.id} transaction={transaction} />
                        ))}
                    </div>
                </div>

                {/* Payment Info Banner */}
                <div className="bg-gradient-to-br from-indigo-600 to-purple-600 rounded-2xl shadow-lg p-6 text-white">
                    <div className="flex items-start justify-between">
                        <div>
                            <h2 className="text-xl font-bold mb-2">Payment Schedule</h2>
                            <p className="text-indigo-100 mb-4">Payments are processed every week</p>
                            <ul className="space-y-2 text-sm">
                                <li className="flex items-center gap-2">
                                    <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20"><path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" /></svg>
                                    <span>Next payout: Friday, Jan 17, 2026</span>
                                </li>
                                <li className="flex items-center gap-2">
                                    <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20"><path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" /></svg>
                                    <span>Estimated amount: ₹45,000</span>
                                </li>
                                <li className="flex items-center gap-2">
                                    <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20"><path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" /></svg>
                                    <span>Bank account ending in ****4567</span>
                                </li>
                            </ul>
                        </div>
                        <div className="hidden sm:block">
                            <svg className="w-24 h-24 opacity-20" fill="currentColor" viewBox="0 0 24 24">
                                <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1.41 16.09V20h-2.67v-1.93c-1.71-.36-3.16-1.46-3.27-3.4h1.96c.1 1.05.82 1.87 2.65 1.87 1.96 0 2.4-.98 2.4-1.59 0-.83-.44-1.61-2.67-2.14-2.48-.6-4.18-1.62-4.18-3.67 0-1.72 1.39-2.84 3.11-3.21V4h2.67v1.95c1.86.45 2.79 1.86 2.85 3.39H14.3c-.05-1.11-.64-1.87-2.22-1.87-1.5 0-2.4.68-2.4 1.64 0 .84.65 1.39 2.67 1.91s4.18 1.39 4.18 3.91c-.01 1.83-1.38 2.83-3.12 3.16z" />
                            </svg>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}