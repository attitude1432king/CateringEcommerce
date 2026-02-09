import { Doughnut } from 'react-chartjs-2';
import {
    Chart as ChartJS,
    ArcElement,
    Tooltip,
    Legend,
} from 'chart.js';

// Register ChartJS components
ChartJS.register(ArcElement, Tooltip, Legend);

/**
 * Order Status Chart Component
 * Displays order status distribution as a doughnut chart
 */
const OrderStatusChart = ({ data, loading }) => {
    if (loading) {
        return (
            <div className="h-80 flex items-center justify-center">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
            </div>
        );
    }

    if (!data || !data.statusDistribution || data.statusDistribution.length === 0) {
        return (
            <div className="h-80 flex flex-col items-center justify-center text-gray-400">
                <p className="text-lg font-medium">No data available</p>
                <p className="text-sm mt-1">Try selecting a different date range</p>
            </div>
        );
    }

    const statusColors = {
        Pending: { bg: 'rgba(251, 191, 36, 0.8)', border: 'rgb(251, 191, 36)' },
        Confirmed: { bg: 'rgba(59, 130, 246, 0.8)', border: 'rgb(59, 130, 246)' },
        'In Progress': { bg: 'rgba(168, 85, 247, 0.8)', border: 'rgb(168, 85, 247)' },
        Completed: { bg: 'rgba(34, 197, 94, 0.8)', border: 'rgb(34, 197, 94)' },
        Cancelled: { bg: 'rgba(239, 68, 68, 0.8)', border: 'rgb(239, 68, 68)' },
        Rejected: { bg: 'rgba(156, 163, 175, 0.8)', border: 'rgb(156, 163, 175)' },
    };

    const chartData = {
        labels: data.statusDistribution.map((item) => item.status),
        datasets: [
            {
                data: data.statusDistribution.map((item) => item.count),
                backgroundColor: data.statusDistribution.map(
                    (item) => statusColors[item.status]?.bg || 'rgba(156, 163, 175, 0.8)'
                ),
                borderColor: data.statusDistribution.map(
                    (item) => statusColors[item.status]?.border || 'rgb(156, 163, 175)'
                ),
                borderWidth: 2,
            },
        ],
    };

    const options = {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: {
                position: 'bottom',
                labels: {
                    usePointStyle: true,
                    padding: 15,
                    font: {
                        size: 12,
                        weight: '500',
                    },
                },
            },
            tooltip: {
                backgroundColor: 'rgba(0, 0, 0, 0.8)',
                padding: 12,
                titleFont: {
                    size: 13,
                    weight: 'bold',
                },
                bodyFont: {
                    size: 12,
                },
                callbacks: {
                    label: function (context) {
                        const label = context.label || '';
                        const value = context.parsed;
                        const total = context.dataset.data.reduce((a, b) => a + b, 0);
                        const percentage = ((value / total) * 100).toFixed(1);
                        return `${label}: ${value} (${percentage}%)`;
                    },
                },
            },
        },
        cutout: '65%',
    };

    return (
        <div>
            {/* Summary Stats */}
            <div className="grid grid-cols-4 gap-3 mb-6">
                <div className="bg-gray-50 rounded-lg p-3 border border-gray-200">
                    <p className="text-xs text-gray-600 font-medium mb-1">Total Orders</p>
                    <p className="text-xl font-bold text-gray-900">
                        {data.totalOrders.toLocaleString()}
                    </p>
                </div>
                <div className="bg-green-50 rounded-lg p-3 border border-green-100">
                    <p className="text-xs text-green-600 font-medium mb-1">Completed</p>
                    <p className="text-xl font-bold text-green-900">
                        {data.completedOrders.toLocaleString()}
                    </p>
                </div>
                <div className="bg-yellow-50 rounded-lg p-3 border border-yellow-100">
                    <p className="text-xs text-yellow-600 font-medium mb-1">Pending</p>
                    <p className="text-xl font-bold text-yellow-900">
                        {data.pendingOrders.toLocaleString()}
                    </p>
                </div>
                <div className="bg-red-50 rounded-lg p-3 border border-red-100">
                    <p className="text-xs text-red-600 font-medium mb-1">Cancelled</p>
                    <p className="text-xl font-bold text-red-900">
                        {data.cancelledOrders.toLocaleString()}
                    </p>
                </div>
            </div>

            {/* Chart */}
            <div className="h-72 flex items-center justify-center">
                <div className="w-full max-w-md">
                    <Doughnut data={chartData} options={options} />
                </div>
            </div>

            {/* Status List */}
            <div className="mt-6 grid grid-cols-2 gap-3">
                {data.statusDistribution.map((item) => (
                    <div
                        key={item.status}
                        className="flex items-center justify-between p-3 bg-gray-50 rounded-lg border border-gray-200"
                    >
                        <div className="flex items-center gap-2">
                            <div
                                className="w-3 h-3 rounded-full"
                                style={{
                                    backgroundColor: statusColors[item.status]?.border || 'rgb(156, 163, 175)',
                                }}
                            ></div>
                            <span className="text-sm font-medium text-gray-700">{item.status}</span>
                        </div>
                        <div className="text-right">
                            <p className="text-sm font-bold text-gray-900">{item.count}</p>
                            <p className="text-xs text-gray-500">{item.percentage.toFixed(1)}%</p>
                        </div>
                    </div>
                ))}
            </div>
        </div>
    );
};

export default OrderStatusChart;
