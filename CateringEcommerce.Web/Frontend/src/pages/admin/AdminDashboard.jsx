import { useState, useEffect } from 'react';
import { Users, Store, ShoppingCart, DollarSign, Star, TrendingUp, TrendingDown, Activity } from 'lucide-react';
import AdminLayout from '../../components/admin/layout/AdminLayout';
import Card, { CardHeader, CardTitle, CardDescription } from '../../components/admin/ui/Card';
import Badge from '../../components/admin/ui/Badge';
import LoadingSkeleton from '../../components/admin/ui/LoadingSkeleton';
import { dashboardApi } from '../../services/adminApi';
import { Line, Bar } from 'react-chartjs-2';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  BarElement,
  Title,
  Tooltip,
  Legend,
} from 'chart.js';

// Register ChartJS components
ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  BarElement,
  Title,
  Tooltip,
  Legend
);

const StatCard = ({ title, value, change, icon: Icon, color }) => {
  const isPositive = change >= 0;

  return (
    <Card className="relative overflow-hidden">
      <div className="flex items-start justify-between">
        <div>
          <p className="text-sm font-medium text-gray-600">{title}</p>
          <p className="text-2xl font-bold text-gray-900 mt-2">{value}</p>
          {change !== undefined && (
            <div className={`flex items-center mt-2 text-sm ${isPositive ? 'text-green-600' : 'text-red-600'}`}>
              {isPositive ? <TrendingUp className="w-4 h-4 mr-1" /> : <TrendingDown className="w-4 h-4 mr-1" />}
              <span className="font-medium">{Math.abs(change)}%</span>
              <span className="text-gray-500 ml-1">from last month</span>
            </div>
          )}
        </div>
        <div className={`w-12 h-12 rounded-lg ${color} bg-opacity-10 flex items-center justify-center`}>
          <Icon className={`w-6 h-6 ${color.replace('bg-', 'text-')}`} />
        </div>
      </div>
    </Card>
  );
};

const AdminDashboard = () => {
  const [metrics, setMetrics] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadDashboardMetrics();
  }, []);

  const loadDashboardMetrics = async () => {
    try {
      const response = await dashboardApi.getMetrics();
      if (response.result) {
        setMetrics(response.data);
      }
    } catch (error) {
      console.error('Error loading dashboard:', error);
    } finally {
      setLoading(false);
    }
  };

  // Chart data
  const revenueChartData = {
    labels: metrics?.revenueChart?.map(item => item.date) || [],
    datasets: [
      {
        label: 'Revenue',
        data: metrics?.revenueChart?.map(item => item.revenue) || [],
        borderColor: 'rgb(99, 102, 241)',
        backgroundColor: 'rgba(99, 102, 241, 0.1)',
        tension: 0.4,
      },
      {
        label: 'Commission',
        data: metrics?.revenueChart?.map(item => item.commission) || [],
        borderColor: 'rgb(34, 197, 94)',
        backgroundColor: 'rgba(34, 197, 94, 0.1)',
        tension: 0.4,
      },
    ],
  };

  const chartOptions = {
    responsive: true,
    plugins: {
      legend: {
        position: 'top',
      },
    },
    scales: {
      y: {
        beginAtZero: true,
      },
    },
  };

  if (loading) {
    return (
      <AdminLayout>
        <div className="space-y-6">
          <LoadingSkeleton type="stat" count={4} />
          <LoadingSkeleton type="card" count={2} />
        </div>
      </AdminLayout>
    );
  }

  return (
    <AdminLayout>
      <div className="space-y-6">
        {/* Page Header */}
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>
          <p className="text-gray-600 mt-1">Welcome back! Here's what's happening today.</p>
        </div>

        {/* Stats Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          <StatCard
            title="Total Users"
            value={metrics?.totalUsers?.toLocaleString() || '0'}
            change={15}
            icon={Users}
            color="bg-blue-500"
          />
          <StatCard
            title="Active Caterings"
            value={metrics?.activeCaterings?.toLocaleString() || '0'}
            change={8}
            icon={Store}
            color="bg-green-500"
          />
          <StatCard
            title="Total Orders"
            value={metrics?.totalOrders?.toLocaleString() || '0'}
            change={-3}
            icon={ShoppingCart}
            color="bg-purple-500"
          />
          <StatCard
            title="Total Revenue"
            value={
              metrics?.totalRevenue != null
                ? `₹${(metrics.totalRevenue / 1000).toFixed(1)}K`
                : '₹0'
            }
            change={20}
            icon={DollarSign}
            color="bg-indigo-500"
          />
        </div>

        {/* Charts Row */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Revenue Chart */}
          <Card>
            <CardHeader>
              <CardTitle>Revenue Overview</CardTitle>
              <CardDescription>Last 7 days performance</CardDescription>
            </CardHeader>
            <div className="mt-6">
              <Line data={revenueChartData} options={chartOptions} />
            </div>
          </Card>

          {/* Quick Stats */}
          <Card>
            <CardHeader>
              <CardTitle>Quick Stats</CardTitle>
              <CardDescription>Platform metrics at a glance</CardDescription>
            </CardHeader>
            <div className="mt-6 space-y-4">
              <div className="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
                <div>
                  <p className="text-sm text-gray-600">Pending Approvals</p>
                  <p className="text-2xl font-bold text-gray-900 mt-1">{metrics?.pendingApprovals || 0}</p>
                </div>
                <Activity className="w-8 h-8 text-yellow-500" />
              </div>
              <div className="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
                <div>
                  <p className="text-sm text-gray-600">Average Rating</p>
                  <p className="text-2xl font-bold text-gray-900 mt-1">{metrics?.averageRating?.toFixed(1) || '0.0'}</p>
                </div>
                <Star className="w-8 h-8 text-yellow-500" />
              </div>
              <div className="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
                <div>
                  <p className="text-sm text-gray-600">Today's Revenue</p>
                  <p className="text-2xl font-bold text-gray-900 mt-1">₹{metrics?.todayRevenue?.toLocaleString() || '0'}</p>
                </div>
                <TrendingUp className="w-8 h-8 text-green-500" />
              </div>
            </div>
          </Card>
        </div>

        {/* Recent Activities */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Top Caterings */}
          <Card>
            <CardHeader>
              <CardTitle>Top Performing Caterings</CardTitle>
              <CardDescription>Highest earning caterings this month</CardDescription>
            </CardHeader>
            <div className="mt-6 space-y-4">
              {metrics?.topCaterings?.slice(0, 5).map((catering, index) => (
                <div key={index} className="flex items-center justify-between p-3 hover:bg-gray-50 rounded-lg transition-colors">
                  <div className="flex items-center space-x-3">
                    <div className="w-10 h-10 bg-indigo-100 rounded-full flex items-center justify-center">
                      <span className="text-indigo-600 font-semibold">#{index + 1}</span>
                    </div>
                    <div>
                      <p className="font-medium text-gray-900">{catering.businessName}</p>
                      <p className="text-sm text-gray-500">{catering.city}</p>
                    </div>
                  </div>
                  <div className="text-right">
                    <p className="font-semibold text-gray-900">₹{catering.totalEarnings?.toLocaleString()}</p>
                    <div className="flex items-center text-sm text-gray-500">
                      <Star className="w-3 h-3 text-yellow-500 mr-1" />
                      {catering.rating?.toFixed(1)}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </Card>

          {/* Recent Orders */}
          <Card>
            <CardHeader>
              <CardTitle>Recent Orders</CardTitle>
              <CardDescription>Latest transactions on the platform</CardDescription>
            </CardHeader>
            <div className="mt-6 space-y-4">
              {metrics?.recentOrders?.slice(0, 5).map((order, index) => (
                <div key={index} className="flex items-center justify-between p-3 hover:bg-gray-50 rounded-lg transition-colors">
                  <div>
                    <p className="font-medium text-gray-900">{order.customerName}</p>
                    <p className="text-sm text-gray-500">{order.cateringName}</p>
                  </div>
                  <div className="text-right">
                    <p className="font-semibold text-gray-900">₹{order.totalAmount?.toLocaleString()}</p>
                    <Badge status={order.status?.toLowerCase()} dot />
                  </div>
                </div>
              ))}
            </div>
          </Card>
        </div>
      </div>
    </AdminLayout>
  );
};

export default AdminDashboard;
