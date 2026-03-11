import { useState, useEffect } from 'react';
import { DollarSign, TrendingUp, TrendingDown, Calendar, Download, RefreshCw, Building2, Filter } from 'lucide-react';
import AdminLayout from '../../components/admin/layout/AdminLayout';
import { ProtectedRoute } from '../../components/admin/auth/ProtectedRoute';
import { apiCall } from '../../services/apiUtils'; // P3 FIX: Use consolidated apiUtils
import { toast } from 'react-hot-toast';
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
    Filler
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
    Legend,
    Filler
);

/**
 * Admin Earnings Page
 *
 * Features:
 * - Earnings summary dashboard
 * - Revenue trends chart
 * - Earnings by catering partner
 * - Monthly reports
 * - Date range filtering
 * - Export earnings data
 */
const AdminEarnings = () => {
    const [loading, setLoading] = useState(true);
    const [summary, setSummary] = useState(null);
    const [monthlyData, setMonthlyData] = useState([]);
    const [cateringEarnings, setCateringEarnings] = useState([]);
    const [selectedYear, setSelectedYear] = useState(new Date().getFullYear());
    const [dateRange, setDateRange] = useState({
        startDate: new Date(new Date().setDate(new Date().getDate() - 30)).toISOString().split('T')[0],
        endDate: new Date().toISOString().split('T')[0],
        groupBy: 'day'
    });
    const [pagination, setPagination] = useState({
        pageNumber: 1,
        pageSize: 10,
        totalCount: 0,
        totalPages: 0
    });

    useEffect(() => {
        fetchEarningsSummary();
        fetchMonthlyReport();
        fetchCateringEarnings();
    }, []);

    useEffect(() => {
        fetchMonthlyReport();
    }, [selectedYear]);

    const fetchEarningsSummary = async () => {
        try {
            const response = await apiCall('/admin/earnings/summary');
            if (response.result && response.data) {
                setSummary(response.data);
            } else {
                toast.error('Failed to load earnings summary');
            }
        } catch (error) {
            console.error('Error fetching earnings summary:', error);
            toast.error('Network error. Please try again.');
        }
    };

    const fetchMonthlyReport = async () => {
        setLoading(true);
        try {
            const response = await apiCall(`/admin/earnings/monthly-report?year=${selectedYear}`);
            if (response.result && response.data) {
                setMonthlyData(response.data);
            }
        } catch (error) {
            console.error('Error fetching monthly report:', error);
        } finally {
            setLoading(false);
        }
    };

    const fetchCateringEarnings = async (page = 1) => {
        try {
            const response = await apiCall(`/admin/earnings/by-catering?pageNumber=${page}&pageSize=${pagination.pageSize}`);
            if (response.result && response.data) {
                setCateringEarnings(response.data.caterings || []);
                setPagination({
                    pageNumber: response.data.pageNumber || 1,
                    pageSize: response.data.pageSize || 10,
                    totalCount: response.data.totalCount || 0,
                    totalPages: response.data.totalPages || 0
                });
            }
        } catch (error) {
            console.error('Error fetching catering earnings:', error);
        }
    };

    const handlePageChange = (newPage) => {
        fetchCateringEarnings(newPage);
    };

    const handleExport = () => {
        toast.success('Export feature coming soon!');
    };

    // Chart data for monthly revenue
    const chartData = {
        labels: monthlyData.map(m => m.monthName || `Month ${m.month}`),
        datasets: [
            {
                label: 'Total Revenue',
                data: monthlyData.map(m => m.totalRevenue || 0),
                borderColor: 'rgb(59, 130, 246)',
                backgroundColor: 'rgba(59, 130, 246, 0.1)',
                fill: true,
                tension: 0.4
            },
            {
                label: 'Commission Earned',
                data: monthlyData.map(m => m.totalCommission || 0),
                borderColor: 'rgb(16, 185, 129)',
                backgroundColor: 'rgba(16, 185, 129, 0.1)',
                fill: true,
                tension: 0.4
            }
        ]
    };

    const chartOptions = {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: {
                position: 'top',
            },
            title: {
                display: true,
                text: `Revenue Trends - ${selectedYear}`
            }
        },
        scales: {
            y: {
                beginAtZero: true,
                ticks: {
                    callback: function(value) {
                        return '₹' + value.toLocaleString();
                    }
                }
            }
        }
    };

    const formatCurrency = (amount) => {
        return new Intl.NumberFormat('en-IN', {
            style: 'currency',
            currency: 'INR',
            minimumFractionDigits: 0
        }).format(amount || 0);
    };

    return (
        <ProtectedRoute requiredPermissions={['EARNINGS_VIEW']}>
            <AdminLayout>
                <div className="p-6">
                    {/* Header */}
                    <div className="mb-6">
                        <div className="flex items-center justify-between">
                            <div>
                                <h1 className="text-2xl font-bold text-gray-900 flex items-center gap-2">
                                    <DollarSign className="w-7 h-7 text-green-600" />
                                    Earnings & Revenue
                                </h1>
                                <p className="text-gray-600 mt-1">
                                    Monitor platform revenue, commissions, and partner earnings
                                </p>
                            </div>
                            <div className="flex gap-3">
                                <button
                                    onClick={() => {
                                        fetchEarningsSummary();
                                        fetchMonthlyReport();
                                        fetchCateringEarnings();
                                    }}
                                    className="flex items-center gap-2 px-4 py-2 text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
                                >
                                    <RefreshCw className="w-4 h-4" />
                                    Refresh
                                </button>
                                <button
                                    onClick={handleExport}
                                    className="flex items-center gap-2 px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors"
                                >
                                    <Download className="w-4 h-4" />
                                    Export Report
                                </button>
                            </div>
                        </div>

                        {/* Summary Cards */}
                        {summary && (
                            <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mt-6">
                                <div className="bg-gradient-to-br from-blue-500 to-blue-600 p-6 rounded-lg text-white">
                                    <div className="flex items-center justify-between mb-2">
                                        <span className="text-blue-100 text-sm">Total Revenue</span>
                                        <TrendingUp className="w-5 h-5 text-blue-100" />
                                    </div>
                                    <div className="text-3xl font-bold">{formatCurrency(summary.totalRevenue)}</div>
                                    <div className="text-xs text-blue-100 mt-1">All time earnings</div>
                                </div>

                                <div className="bg-gradient-to-br from-green-500 to-green-600 p-6 rounded-lg text-white">
                                    <div className="flex items-center justify-between mb-2">
                                        <span className="text-green-100 text-sm">Commission Earned</span>
                                        <DollarSign className="w-5 h-5 text-green-100" />
                                    </div>
                                    <div className="text-3xl font-bold">{formatCurrency(summary.totalCommission)}</div>
                                    <div className="text-xs text-green-100 mt-1">Platform commission</div>
                                </div>

                                <div className="bg-gradient-to-br from-purple-500 to-purple-600 p-6 rounded-lg text-white">
                                    <div className="flex items-center justify-between mb-2">
                                        <span className="text-purple-100 text-sm">This Month</span>
                                        <Calendar className="w-5 h-5 text-purple-100" />
                                    </div>
                                    <div className="text-3xl font-bold">{formatCurrency(summary.monthlyRevenue)}</div>
                                    <div className="text-xs text-purple-100 mt-1">Current month revenue</div>
                                </div>

                                <div className="bg-gradient-to-br from-orange-500 to-orange-600 p-6 rounded-lg text-white">
                                    <div className="flex items-center justify-between mb-2">
                                        <span className="text-orange-100 text-sm">Active Partners</span>
                                        <Building2 className="w-5 h-5 text-orange-100" />
                                    </div>
                                    <div className="text-3xl font-bold">{summary.activeCaterings || 0}</div>
                                    <div className="text-xs text-orange-100 mt-1">Earning partners</div>
                                </div>
                            </div>
                        )}
                    </div>

                    {/* Chart Section */}
                    <div className="bg-white p-6 rounded-lg border border-gray-200 mb-6">
                        <div className="flex items-center justify-between mb-4">
                            <h2 className="text-lg font-semibold text-gray-900">Revenue Trends</h2>
                            <select
                                value={selectedYear}
                                onChange={(e) => setSelectedYear(parseInt(e.target.value))}
                                className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                            >
                                {[2024, 2025, 2026].map(year => (
                                    <option key={year} value={year}>{year}</option>
                                ))}
                            </select>
                        </div>
                        <div className="h-80">
                            {loading ? (
                                <div className="h-full flex items-center justify-center">
                                    <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
                                </div>
                            ) : (
                                <Line data={chartData} options={chartOptions} />
                            )}
                        </div>
                    </div>

                    {/* Earnings by Catering Table */}
                    <div className="bg-white rounded-lg border border-gray-200">
                        <div className="px-6 py-4 border-b border-gray-200">
                            <h2 className="text-lg font-semibold text-gray-900">Earnings by Partner</h2>
                        </div>
                        <div className="overflow-x-auto">
                            <table className="min-w-full divide-y divide-gray-200">
                                <thead className="bg-gray-50">
                                    <tr>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                            Partner Name
                                        </th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                            Total Orders
                                        </th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                            Total Revenue
                                        </th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                            Commission
                                        </th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                            Commission %
                                        </th>
                                    </tr>
                                </thead>
                                <tbody className="bg-white divide-y divide-gray-200">
                                    {cateringEarnings.length === 0 ? (
                                        <tr>
                                            <td colSpan="5" className="px-6 py-12 text-center text-gray-500">
                                                No earnings data available
                                            </td>
                                        </tr>
                                    ) : (
                                        cateringEarnings.map((catering, index) => (
                                            <tr key={index} className="hover:bg-gray-50">
                                                <td className="px-6 py-4 whitespace-nowrap">
                                                    <div className="text-sm font-medium text-gray-900">
                                                        {catering.cateringName || 'Unknown'}
                                                    </div>
                                                </td>
                                                <td className="px-6 py-4 whitespace-nowrap">
                                                    <div className="text-sm text-gray-900">{catering.totalOrders || 0}</div>
                                                </td>
                                                <td className="px-6 py-4 whitespace-nowrap">
                                                    <div className="text-sm font-semibold text-gray-900">
                                                        {formatCurrency(catering.totalRevenue)}
                                                    </div>
                                                </td>
                                                <td className="px-6 py-4 whitespace-nowrap">
                                                    <div className="text-sm font-semibold text-green-600">
                                                        {formatCurrency(catering.totalCommission)}
                                                    </div>
                                                </td>
                                                <td className="px-6 py-4 whitespace-nowrap">
                                                    <span className="inline-flex px-2 py-1 text-xs font-semibold rounded-full bg-green-100 text-green-800">
                                                        {catering.commissionPercentage?.toFixed(1) || '0.0'}%
                                                    </span>
                                                </td>
                                            </tr>
                                        ))
                                    )}
                                </tbody>
                            </table>
                        </div>

                        {/* Pagination */}
                        {pagination.totalPages > 1 && (
                            <div className="bg-white px-4 py-3 flex items-center justify-between border-t border-gray-200">
                                <div className="flex-1 flex justify-between sm:hidden">
                                    <button
                                        onClick={() => handlePageChange(pagination.pageNumber - 1)}
                                        disabled={pagination.pageNumber === 1}
                                        className="relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50"
                                    >
                                        Previous
                                    </button>
                                    <button
                                        onClick={() => handlePageChange(pagination.pageNumber + 1)}
                                        disabled={pagination.pageNumber === pagination.totalPages}
                                        className="ml-3 relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50"
                                    >
                                        Next
                                    </button>
                                </div>
                                <div className="hidden sm:flex-1 sm:flex sm:items-center sm:justify-between">
                                    <div>
                                        <p className="text-sm text-gray-700">
                                            Showing <span className="font-medium">{(pagination.pageNumber - 1) * pagination.pageSize + 1}</span> to{' '}
                                            <span className="font-medium">{Math.min(pagination.pageNumber * pagination.pageSize, pagination.totalCount)}</span> of{' '}
                                            <span className="font-medium">{pagination.totalCount}</span> results
                                        </p>
                                    </div>
                                    <div>
                                        <nav className="relative z-0 inline-flex rounded-md shadow-sm -space-x-px">
                                            <button
                                                onClick={() => handlePageChange(pagination.pageNumber - 1)}
                                                disabled={pagination.pageNumber === 1}
                                                className="relative inline-flex items-center px-2 py-2 rounded-l-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50 disabled:opacity-50"
                                            >
                                                Previous
                                            </button>
                                            <button
                                                onClick={() => handlePageChange(pagination.pageNumber + 1)}
                                                disabled={pagination.pageNumber === pagination.totalPages}
                                                className="relative inline-flex items-center px-2 py-2 rounded-r-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50 disabled:opacity-50"
                                            >
                                                Next
                                            </button>
                                        </nav>
                                    </div>
                                </div>
                            </div>
                        )}
                    </div>
                </div>
            </AdminLayout>
        </ProtectedRoute>
    );
};

export default AdminEarnings;
