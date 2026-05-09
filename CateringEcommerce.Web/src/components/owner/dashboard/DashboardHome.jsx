import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
    TrendingUp, DollarSign, Clock, Star,
    ClipboardList, Calendar, Plus, Tag, CheckCircle,
} from 'lucide-react';
import { ownerApiService } from '../../../services/ownerApi';
import RevenueChart from './charts/RevenueChart';
import OrdersChart from './charts/OrdersChart';
import { formatCurrency, formatDate } from '../../../utils/exportUtils';
import { Skeleton } from '../../../design-system/components';
import Button from '../../../design-system/components/Button';

// ── Stat card using portal .stat classes ─────────────────────
const StatCard = ({ title, value, change, icon: Icon, iconBgStyle, isLoading }) => (
    <div className="stat">
        <div className="stat__ic" style={iconBgStyle}>
            <Icon size={20} strokeWidth={1.75} />
        </div>
        <div className="stat__l">{title}</div>
        {isLoading ? (
            <>
                <Skeleton className="h-7 w-24 mt-1 mb-2 rounded" />
                <Skeleton className="h-4 w-32 rounded" />
            </>
        ) : (
            <>
                <div className="stat__v">{value}</div>
                {change != null && (
                    <div className={`stat__d ${change >= 0 ? 'up' : 'dn'}`}>
                        {change >= 0 ? '▲' : '▼'} {Math.abs(change).toFixed(1)}%
                        <span className="muted">vs last month</span>
                    </div>
                )}
            </>
        )}
    </div>
);

// ── Order row using portal table classes ─────────────────────
const statusClass = (s) => {
    const m = { Pending: 's-pending', Confirmed: 's-confirmed', Completed: 's-completed', Cancelled: 's-cancelled' };
    return m[s] || 's-pending';
};

const OrderRow = ({ order, onClick }) => (
    <tr onClick={onClick} className="cursor-pointer hover:bg-neutral-50 transition-colors">
        <td>
            <div className="cell-user">
                <div className="avatar">{(order.customerName || 'U')[0].toUpperCase()}</div>
                <div>
                    <div className="name">{order.orderNumber}</div>
                    <div className="sub">{order.customerName}</div>
                </div>
            </div>
        </td>
        <td className="hidden sm:table-cell">{formatDate(order.eventDate, 'medium')}</td>
        <td>
            <span className={`status-pill ${statusClass(order.orderStatus)}`}>
                <span className="dot" />
                {order.orderStatus}
            </span>
        </td>
        <td className="amount text-right">{formatCurrency(order.totalAmount)}</td>
    </tr>
);

// ── Upcoming event card ───────────────────────────────────────
const EventCard = ({ event }) => {
    const urgent = event.daysUntilEvent <= 2;
    return (
        <div className={`rounded-xl p-4 border ${urgent ? 'border-red-200 bg-red-50' : 'border-neutral-100 bg-neutral-50'}`}>
            <div className="flex items-start justify-between gap-3">
                <div className="min-w-0">
                    <div className="flex items-center gap-2 flex-wrap">
                        <span className="font-semibold text-sm text-neutral-900">{event.orderNumber}</span>
                        {urgent && (
                            <span className="status-pill s-cancelled" style={{ fontSize: 10 }}>URGENT</span>
                        )}
                    </div>
                    <p className="text-xs text-neutral-500 mt-0.5">{event.customerName}</p>
                    <p className="text-xs text-neutral-400">{event.eventType}</p>
                </div>
                <div className="text-right flex-shrink-0">
                    <div className="text-lg font-bold" style={{ color: 'var(--color-primary)' }}>
                        {event.daysUntilEvent}d
                    </div>
                    <div className="text-xs text-neutral-500">to go</div>
                </div>
            </div>
            <div className="flex items-center justify-between text-xs mt-3 pt-3 border-t border-neutral-200">
                <span className="text-neutral-500">{formatDate(event.eventDate, 'medium')}</span>
                <span className="amount text-sm">{formatCurrency(event.totalAmount)}</span>
            </div>
        </div>
    );
};

export default function DashboardHome() {
    const navigate = useNavigate();
    const [loading, setLoading] = useState(true);
    const [period, setPeriod] = useState('month');
    const [metrics, setMetrics] = useState(null);
    const [recentOrders, setRecentOrders] = useState([]);
    const [upcomingEvents, setUpcomingEvents] = useState([]);
    const [revenueChartData, setRevenueChartData] = useState(null);
    const [ordersChartData, setOrdersChartData] = useState(null);
    const [error, setError] = useState(null);

    useEffect(() => { fetchDashboardData(); }, [period]);

    const fetchDashboardData = async () => {
        try {
            setLoading(true);
            setError(null);
            const [metricsData, recentOrdersData, upcomingEventsData, revenueData, ordersData] = await Promise.all([
                ownerApiService.getDashboardMetrics(),
                ownerApiService.getRecentOrders(5),
                ownerApiService.getUpcomingEvents(7),
                ownerApiService.getRevenueChart(period),
                ownerApiService.getOrdersChart(period),
            ]);
            if (metricsData.success)       setMetrics(metricsData.data);
            if (recentOrdersData.success)  setRecentOrders(recentOrdersData.data);
            if (upcomingEventsData.success) setUpcomingEvents(upcomingEventsData.data);
            if (revenueData.success)       setRevenueChartData(revenueData.data);
            if (ordersData.success)        setOrdersChartData(ordersData.data);
        } catch (err) {
            console.error('Error fetching dashboard data:', err);
            setError('Failed to load dashboard data');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="space-y-5">
            {/* Error banner */}
            {error && (
                <div className="panel" style={{ borderColor: 'rgba(239,68,68,0.3)', background: '#FEF2F2' }}>
                    <p className="text-sm font-medium" style={{ color: '#B91C1C' }}>{error}</p>
                    <button
                        onClick={fetchDashboardData}
                        className="mt-2 text-sm font-semibold"
                        style={{ color: 'var(--color-primary)' }}
                    >
                        Retry
                    </button>
                </div>
            )}

            {/* Stat grid */}
            <div className="stat-grid">
                <StatCard
                    title="Total Orders"
                    value={metrics ? metrics.totalOrders.toString() : '—'}
                    change={metrics?.ordersChange}
                    icon={TrendingUp}
                    iconBgStyle={{ background: 'rgba(59,130,246,0.1)', color: '#3B82F6' }}
                    isLoading={loading}
                />
                <StatCard
                    title="Total Revenue"
                    value={metrics ? formatCurrency(metrics.totalRevenue) : '—'}
                    change={metrics?.revenueChange}
                    icon={DollarSign}
                    iconBgStyle={{ background: 'rgba(34,197,94,0.1)', color: '#16A34A' }}
                    isLoading={loading}
                />
                <StatCard
                    title="Pending Orders"
                    value={metrics ? metrics.pendingOrders.toString() : '—'}
                    change={metrics?.pendingOrdersChange}
                    icon={Clock}
                    iconBgStyle={{ background: 'rgba(255,107,53,0.1)', color: 'var(--color-primary)' }}
                    isLoading={loading}
                />
                <StatCard
                    title="Customer Rating"
                    value={metrics ? metrics.customerSatisfaction.toFixed(1) + ' ★' : '—'}
                    change={null}
                    icon={Star}
                    iconBgStyle={{ background: 'rgba(255,182,39,0.15)', color: 'var(--color-accent)' }}
                    isLoading={loading}
                />
            </div>

            {/* Charts row */}
            <div className="row-grid">
                <div className="panel" style={{ marginBottom: 0 }}>
                    <div className="panel__h">
                        <h3>Revenue Trend</h3>
                        <select
                            value={period}
                            onChange={e => setPeriod(e.target.value)}
                            className="text-sm border border-neutral-200 rounded-lg px-3 py-1.5 focus:outline-none"
                            style={{ borderRadius: 8 }}
                        >
                            <option value="day">Daily</option>
                            <option value="week">Weekly</option>
                            <option value="month">Monthly</option>
                            <option value="year">Yearly</option>
                        </select>
                    </div>
                    {loading
                        ? <Skeleton className="h-64 w-full rounded-xl" />
                        : <RevenueChart data={revenueChartData} period={period} height={260} />
                    }
                </div>

                <div className="panel" style={{ marginBottom: 0 }}>
                    <div className="panel__h"><h3>Orders Trend</h3></div>
                    {loading
                        ? <Skeleton className="h-64 w-full rounded-xl" />
                        : <OrdersChart data={ordersChartData} period={period} height={260} />
                    }
                </div>
            </div>

            {/* Quick actions */}
            <div className="panel">
                <div className="panel__h"><h3>Quick Actions</h3></div>
                <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
                    {[
                        { icon: Plus,          label: 'Add Menu Item',    to: '/owner/dashboard/menu' },
                        { icon: ClipboardList, label: 'View Bookings',    to: '/owner/dashboard/bookings' },
                        { icon: Tag,           label: 'Create Discount',  to: '/owner/dashboard/discounts' },
                        { icon: Calendar,      label: 'Set Availability', to: null },
                    ].map(({ icon: Icon, label, to }) => (
                        <button
                            key={label}
                            onClick={() => to && navigate(to)}
                            className="flex flex-col items-center gap-2 p-4 rounded-xl border border-neutral-200 bg-white hover:border-transparent hover:shadow-md transition-all text-sm font-semibold text-neutral-700 hover:text-neutral-900"
                        >
                            <div className="w-9 h-9 rounded-xl flex items-center justify-center" style={{ background: 'rgba(255,107,53,0.08)', color: 'var(--color-primary)' }}>
                                <Icon size={18} strokeWidth={1.75} />
                            </div>
                            {label}
                        </button>
                    ))}
                </div>
            </div>

            {/* Recent orders + Upcoming events */}
            <div className="row-grid">
                {/* Recent orders */}
                <div className="panel" style={{ marginBottom: 0 }}>
                    <div className="panel__h">
                        <h3>Recent Orders</h3>
                        <span className="link" onClick={() => navigate('/owner/dashboard/bookings')}>View all →</span>
                    </div>
                    {loading ? (
                        <div className="space-y-3">
                            {[...Array(4)].map((_, i) => <Skeleton key={i} className="h-12 w-full rounded-lg" />)}
                        </div>
                    ) : recentOrders.length > 0 ? (
                        <div className="overflow-x-auto">
                            <table className="portal-table">
                                <thead>
                                    <tr>
                                        <th>Customer</th>
                                        <th className="hidden sm:table-cell">Event Date</th>
                                        <th>Status</th>
                                        <th className="text-right">Amount</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {recentOrders.map(order => (
                                        <OrderRow
                                            key={order.orderId}
                                            order={order}
                                            onClick={() => navigate('/owner/dashboard/bookings')}
                                        />
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    ) : (
                        <div className="text-center py-10 text-neutral-400">
                            <ClipboardList size={40} strokeWidth={1} className="mx-auto mb-3" />
                            <p className="text-sm">No recent orders</p>
                        </div>
                    )}
                </div>

                {/* Upcoming events */}
                <div className="panel" style={{ marginBottom: 0 }}>
                    <div className="panel__h">
                        <h3>Upcoming Events <span className="text-xs font-normal text-neutral-400">(7 days)</span></h3>
                        <span className="link" onClick={() => navigate('/owner/dashboard/events')}>View all →</span>
                    </div>
                    {loading ? (
                        <div className="space-y-3">
                            {[...Array(3)].map((_, i) => <Skeleton key={i} className="h-20 w-full rounded-xl" />)}
                        </div>
                    ) : upcomingEvents.length > 0 ? (
                        <div className="space-y-3 max-h-96 overflow-y-auto pr-1">
                            {upcomingEvents.map(ev => <EventCard key={ev.orderId} event={ev} />)}
                        </div>
                    ) : (
                        <div className="text-center py-10 text-neutral-400">
                            <Calendar size={40} strokeWidth={1} className="mx-auto mb-3" />
                            <p className="text-sm">No upcoming events in the next 7 days</p>
                        </div>
                    )}
                </div>
            </div>

            {/* Performance insights */}
            {metrics && (
                <div className="panel" style={{
                    background: 'linear-gradient(135deg, #FF6B35 0%, #FF8C42 50%, #FFB627 100%)',
                    border: 'none',
                    color: '#fff',
                    marginBottom: 0,
                }}>
                    <div className="flex items-start justify-between gap-4">
                        <div>
                            <h3 style={{ color: '#fff', marginBottom: 4 }}>Performance Insights</h3>
                            <p style={{ fontSize: 13, opacity: 0.85, marginBottom: 12 }}>Your business is growing! Keep it up.</p>
                            <ul className="space-y-2 text-sm" style={{ opacity: 0.95 }}>
                                <li className="flex items-center gap-2">
                                    <CheckCircle size={15} /> {metrics.totalOrders} orders this period
                                </li>
                                <li className="flex items-center gap-2">
                                    <CheckCircle size={15} /> {metrics.customerSatisfaction?.toFixed(1)} ★ avg customer rating
                                </li>
                                <li className="flex items-center gap-2">
                                    <CheckCircle size={15} /> {metrics.totalCustomers} total customers
                                </li>
                                {upcomingEvents.length > 0 && (
                                    <li className="flex items-center gap-2">
                                        <CheckCircle size={15} /> {upcomingEvents.length} events coming up this week
                                    </li>
                                )}
                            </ul>
                        </div>
                        <Star size={72} strokeWidth={0.75} style={{ opacity: 0.2, flexShrink: 0 }} />
                    </div>
                </div>
            )}
        </div>
    );
}
