import { useState, useEffect } from 'react';
import { Users, Store, ShoppingCart, DollarSign, TrendingUp, Download, RefreshCw } from 'lucide-react';
import AdminLayout from '../../components/admin/layout/AdminLayout';
import StatCard from '../../components/admin/analytics/StatCard';
import DateRangeFilter from '../../components/admin/analytics/DateRangeFilter';
import RevenueChart from '../../components/admin/analytics/RevenueChart';
import OrderStatusChart from '../../components/admin/analytics/OrderStatusChart';
import { adminAnalyticsApi } from '../../services/analyticsApi';
import { toast } from 'react-hot-toast';

/**
 * Admin Analytics Dashboard
 * Comprehensive analytics with charts, metrics, and date filtering
 */
const AdminAnalytics = () => {
    const [loading, setLoading] = useState(true);
    const [dateRange, setDateRange] = useState({
        from: null,
        to: null,
    });

    // Analytics Data
    const [metrics, setMetrics] = useState(null);
    const [revenueChart, setRevenueChart] = useState(null);
    const [orderAnalytics, setOrderAnalytics] = useState(null);
    const [topPartners, setTopPartners] = useState([]);
    const [recentOrders, setRecentOrders] = useState([]);

    // Chart granularity
    const [revenueGranularity, setRevenueGranularity] = useState('day');

    useEffect(() => {
        // Initialize with default date range (last 30 days)
        const today = new Date();
        const thirtyDaysAgo = new Date(today);
        thirtyDaysAgo.setDate(today.getDate() - 30);

        setDateRange({
            from: thirtyDaysAgo.toISOString().split('T')[0],
            to: today.toISOString().split('T')[0],
        });
    }, []);

    useEffect(() => {
        if (dateRange.from && dateRange.to) {
            loadAllAnalytics();
        }
    }, [dateRange]);

    const loadAllAnalytics = async () => {
        setLoading(true);
        try {
            // Load all analytics in parallel
            const [
                metricsRes,
                revenueRes,
                ordersRes,
                partnersRes,
                recentRes,
            ] = await Promise.all([
                adminAnalyticsApi.getDashboardMetrics(dateRange.from, dateRange.to),
                adminAnalyticsApi.getRevenueChart(dateRange.from, dateRange.to, revenueGranularity),
                adminAnalyticsApi.getOrderAnalytics(dateRange.from, dateRange.to),
                adminAnalyticsApi.getTopPartners(dateRange.from, dateRange.to, 5),
                adminAnalyticsApi.getRecentOrders(10),
            ]);

            if (metricsRes.result) setMetrics(metricsRes.data);
            if (revenueRes.result) setRevenueChart(revenueRes.data);
            if (ordersRes.result) setOrderAnalytics(ordersRes.data);
            if (partnersRes.result) setTopPartners(partnersRes.data.topPartners || []);
            if (recentRes.result) setRecentOrders(recentRes.data || []);
        } catch (error) {
            console.error('Error loading analytics:', error);
            toast.error('Failed to load analytics data');
        } finally {
            setLoading(false);
        }
    };

    const handleDateChange = (from, to) => {
        setDateRange({ from, to });
    };

    const handleRefresh = () => {
        loadAllAnalytics();
        toast.success('Analytics refreshed');
    };

    const handleExport = async () => {
        try {
            const response = await adminAnalyticsApi.exportAnalytics(
                'revenue',
                'excel',
                dateRange.from,
                dateRange.to
            );

            if (response.result) {
                toast.success('Analytics exported successfully');
                // Handle file download
            } else {
                toast.error('Failed to export analytics');
            }
        } catch (error) {
            console.error('Error exporting analytics:', error);
            toast.error('Failed to export analytics');
        }
    };

    return (
        <AdminLayout>
            <div className="p-6 space-y-6">
                {/* Header */}
                <div className="flex items-center justify-between">
                    <div>
                        <h1 className="text-2xl font-bold text-gray-900">Analytics Dashboard</h1>
                        <p className="text-gray-600 mt-1">
                            Comprehensive analytics and insights for your platform
                        </p>
                    </div>
                    <div className="flex items-center gap-3">
                        <button
                            onClick={handleRefresh}
                            className="px-4 py-2 bg-white border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors flex items-center gap-2"
                            disabled={loading}
                        >
                            <RefreshCw className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`} />
                            <span>Refresh</span>
                        </button>
                        <button
                            onClick={handleExport}
                            className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors flex items-center gap-2"
                        >
                            <Download className="w-4 h-4" />
                            <span>Export</span>
                        </button>
                    </div>
                </div>

                {/* Date Range Filter */}
                <DateRangeFilter onDateChange={handleDateChange} defaultRange="last30days" />

                {/* Key Metrics */}
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
                    <StatCard
                        title="Total Revenue"
                        value={
                            metrics?.totalRevenue
                                ? new Intl.NumberFormat('en-IN', {
                                      style: 'currency',
                                      currency: 'INR',
                                      minimumFractionDigits: 0,
                                  }).format(metrics.totalRevenue)
                                : '₹0'
                        }
                        change={metrics?.revenueChangePercent}
                        icon={DollarSign}
                        iconBgColor="bg-green-100"
                        iconColor="text-green-600"
                        loading={loading}
                    />
                    <StatCard
                        title="Total Orders"
                        value={metrics?.totalOrders?.toLocaleString() || '0'}
                        change={metrics?.ordersChangePercent}
                        icon={ShoppingCart}
                        iconBgColor="bg-blue-100"
                        iconColor="text-blue-600"
                        loading={loading}
                    />
                    <StatCard
                        title="Active Partners"
                        value={metrics?.activeCaterings?.toLocaleString() || '0'}
                        change={metrics?.cateringsChangePercent}
                        icon={Store}
                        iconBgColor="bg-purple-100"
                        iconColor="text-purple-600"
                        loading={loading}
                    />
                    <StatCard
                        title="Total Users"
                        value={metrics?.totalUsers?.toLocaleString() || '0'}
                        change={metrics?.usersChangePercent}
                        icon={Users}
                        iconBgColor="bg-indigo-100"
                        iconColor="text-indigo-600"
                        loading={loading}
                    />
                </div>

                {/* Additional Metrics */}
                <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                    <StatCard
                        title="Average Order Value"
                        value={
                            metrics?.averageOrderValue
                                ? new Intl.NumberFormat('en-IN', {
                                      style: 'currency',
                                      currency: 'INR',
                                      minimumFractionDigits: 0,
                                  }).format(metrics.averageOrderValue)
                                : '₹0'
                        }
                        icon={TrendingUp}
                        iconBgColor="bg-yellow-100"
                        iconColor="text-yellow-600"
                        loading={loading}
                    />
                    <StatCard
                        title="Total Commission"
                        value={
                            metrics?.totalCommission
                                ? new Intl.NumberFormat('en-IN', {
                                      style: 'currency',
                                      currency: 'INR',
                                      minimumFractionDigits: 0,
                                  }).format(metrics.totalCommission)
                                : '₹0'
                        }
                        icon={DollarSign}
                        iconBgColor="bg-emerald-100"
                        iconColor="text-emerald-600"
                        loading={loading}
                    />
                    <StatCard
                        title="Average Rating"
                        value={metrics?.averageRating?.toFixed(2) || '0.00'}
                        icon={TrendingUp}
                        iconBgColor="bg-orange-100"
                        iconColor="text-orange-600"
                        loading={loading}
                    />
                </div>

                {/* Charts Row */}
                <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                    {/* Revenue Chart */}
                    <div className="lg:col-span-2 bg-white rounded-lg border border-gray-200 p-6">
                        <div className="flex items-center justify-between mb-6">
                            <div>
                                <h2 className="text-lg font-bold text-gray-900">Revenue Overview</h2>
                                <p className="text-sm text-gray-600 mt-1">
                                    Revenue and commission trends
                                </p>
                            </div>
                            <select
                                value={revenueGranularity}
                                onChange={(e) => {
                                    setRevenueGranularity(e.target.value);
                                    loadAllAnalytics();
                                }}
                                className="px-3 py-1.5 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                            >
                                <option value="day">Daily</option>
                                <option value="week">Weekly</option>
                                <option value="month">Monthly</option>
                            </select>
                        </div>
                        <RevenueChart data={revenueChart} loading={loading} granularity={revenueGranularity} />
                    </div>

                    {/* Order Status Chart */}
                    <div className="bg-white rounded-lg border border-gray-200 p-6">
                        <div className="mb-6">
                            <h2 className="text-lg font-bold text-gray-900">Order Status</h2>
                            <p className="text-sm text-gray-600 mt-1">Distribution by status</p>
                        </div>
                        <OrderStatusChart data={orderAnalytics} loading={loading} />
                    </div>
                </div>

                {/* Tables Row */}
                <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                    {/* Top Partners */}
                    <div className="bg-white rounded-lg border border-gray-200 p-6">
                        <h2 className="text-lg font-bold text-gray-900 mb-4">Top Performing Partners</h2>
                        <div className="space-y-3">
                            {loading ? (
                                <div className="space-y-3">
                                    {[...Array(5)].map((_, i) => (
                                        <div key={i} className="animate-pulse flex items-center gap-3 p-3">
                                            <div className="w-10 h-10 bg-gray-200 rounded-full"></div>
                                            <div className="flex-1 space-y-2">
                                                <div className="h-4 bg-gray-200 rounded w-3/4"></div>
                                                <div className="h-3 bg-gray-200 rounded w-1/2"></div>
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            ) : topPartners.length === 0 ? (
                                <p className="text-center text-gray-500 py-8">No partners found</p>
                            ) : (
                                topPartners.map((partner, index) => (
                                    <div
                                        key={partner.cateringOwnerId}
                                        className="flex items-center justify-between p-3 hover:bg-gray-50 rounded-lg transition-colors border border-gray-100"
                                    >
                                        <div className="flex items-center gap-3">
                                            <div className="w-10 h-10 bg-blue-100 rounded-full flex items-center justify-center">
                                                <span className="text-blue-600 font-bold">#{index + 1}</span>
                                            </div>
                                            <div>
                                                <p className="font-semibold text-gray-900">
                                                    {partner.businessName}
                                                </p>
                                                <p className="text-sm text-gray-500">
                                                    {partner.city} • {partner.totalOrders} orders
                                                </p>
                                            </div>
                                        </div>
                                        <div className="text-right">
                                            <p className="font-bold text-gray-900">
                                                {new Intl.NumberFormat('en-IN', {
                                                    style: 'currency',
                                                    currency: 'INR',
                                                    minimumFractionDigits: 0,
                                                }).format(partner.totalRevenue)}
                                            </p>
                                            <p className="text-sm text-yellow-600">
                                                ★ {partner.averageRating.toFixed(1)}
                                            </p>
                                        </div>
                                    </div>
                                ))
                            )}
                        </div>
                    </div>

                    {/* Recent Orders */}
                    <div className="bg-white rounded-lg border border-gray-200 p-6">
                        <h2 className="text-lg font-bold text-gray-900 mb-4">Recent Orders</h2>
                        <div className="space-y-3">
                            {loading ? (
                                <div className="space-y-3">
                                    {[...Array(5)].map((_, i) => (
                                        <div key={i} className="animate-pulse flex items-center gap-3 p-3">
                                            <div className="flex-1 space-y-2">
                                                <div className="h-4 bg-gray-200 rounded w-3/4"></div>
                                                <div className="h-3 bg-gray-200 rounded w-1/2"></div>
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            ) : recentOrders.length === 0 ? (
                                <p className="text-center text-gray-500 py-8">No recent orders</p>
                            ) : (
                                recentOrders.map((order) => (
                                    <div
                                        key={order.orderId}
                                        className="flex items-center justify-between p-3 hover:bg-gray-50 rounded-lg transition-colors border border-gray-100"
                                    >
                                        <div>
                                            <p className="font-semibold text-gray-900">
                                                {order.customerName}
                                            </p>
                                            <p className="text-sm text-gray-500">{order.cateringName}</p>
                                            <p className="text-xs text-gray-400 mt-1">
                                                {new Date(order.orderDate).toLocaleDateString()}
                                            </p>
                                        </div>
                                        <div className="text-right">
                                            <p className="font-bold text-gray-900">
                                                {new Intl.NumberFormat('en-IN', {
                                                    style: 'currency',
                                                    currency: 'INR',
                                                    minimumFractionDigits: 0,
                                                }).format(order.totalAmount)}
                                            </p>
                                            <span
                                                className={`inline-block px-2 py-0.5 text-xs font-medium rounded-full mt-1 ${
                                                    order.orderStatus === 'Completed'
                                                        ? 'bg-green-100 text-green-700'
                                                        : order.orderStatus === 'Pending'
                                                        ? 'bg-yellow-100 text-yellow-700'
                                                        : 'bg-gray-100 text-gray-700'
                                                }`}
                                            >
                                                {order.orderStatus}
                                            </span>
                                        </div>
                                    </div>
                                ))
                            )}
                        </div>
                    </div>
                </div>
            </div>
        </AdminLayout>
    );
};

export default AdminAnalytics;
