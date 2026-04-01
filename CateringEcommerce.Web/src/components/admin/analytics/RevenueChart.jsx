import { Line } from 'react-chartjs-2';
import {
    Chart as ChartJS,
    CategoryScale,
    LinearScale,
    PointElement,
    LineElement,
    Title,
    Tooltip,
    Legend,
    Filler,
} from 'chart.js';

// Register ChartJS components
ChartJS.register(
    CategoryScale,
    LinearScale,
    PointElement,
    LineElement,
    Title,
    Tooltip,
    Legend,
    Filler
);

/**
 * Revenue Chart Component
 * Displays revenue and commission trends over time
 */
const RevenueChart = ({ data, loading, granularity = 'day' }) => {
    if (loading) {
        return (
            <div className="h-80 flex items-center justify-center">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
            </div>
        );
    }

    if (!data || !data.dataPoints || data.dataPoints.length === 0) {
        return (
            <div className="h-80 flex flex-col items-center justify-center text-gray-400">
                <p className="text-lg font-medium">No data available</p>
                <p className="text-sm mt-1">Try selecting a different date range</p>
            </div>
        );
    }

    const chartData = {
        labels: data.dataPoints.map((point) => point.label),
        datasets: [
            {
                label: 'Revenue',
                data: data.dataPoints.map((point) => point.revenue),
                borderColor: 'rgb(59, 130, 246)',
                backgroundColor: 'rgba(59, 130, 246, 0.1)',
                tension: 0.4,
                fill: true,
                pointRadius: 4,
                pointHoverRadius: 6,
                pointBackgroundColor: 'rgb(59, 130, 246)',
                pointBorderColor: '#fff',
                pointBorderWidth: 2,
            },
            {
                label: 'Commission',
                data: data.dataPoints.map((point) => point.commission),
                borderColor: 'rgb(34, 197, 94)',
                backgroundColor: 'rgba(34, 197, 94, 0.1)',
                tension: 0.4,
                fill: true,
                pointRadius: 4,
                pointHoverRadius: 6,
                pointBackgroundColor: 'rgb(34, 197, 94)',
                pointBorderColor: '#fff',
                pointBorderWidth: 2,
            },
        ],
    };

    const options = {
        responsive: true,
        maintainAspectRatio: false,
        interaction: {
            mode: 'index',
            intersect: false,
        },
        plugins: {
            legend: {
                position: 'top',
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
                bodySpacing: 6,
                usePointStyle: true,
                callbacks: {
                    label: function (context) {
                        let label = context.dataset.label || '';
                        if (label) {
                            label += ': ';
                        }
                        label += new Intl.NumberFormat('en-IN', {
                            style: 'currency',
                            currency: 'INR',
                            minimumFractionDigits: 0,
                        }).format(context.parsed.y);
                        return label;
                    },
                },
            },
        },
        scales: {
            x: {
                grid: {
                    display: false,
                },
                ticks: {
                    font: {
                        size: 11,
                    },
                },
            },
            y: {
                beginAtZero: true,
                grid: {
                    color: 'rgba(0, 0, 0, 0.05)',
                },
                ticks: {
                    font: {
                        size: 11,
                    },
                    callback: function (value) {
                        if (value >= 1000000) {
                            return '₹' + (value / 1000000).toFixed(1) + 'M';
                        } else if (value >= 1000) {
                            return '₹' + (value / 1000).toFixed(0) + 'K';
                        }
                        return '₹' + value;
                    },
                },
            },
        },
    };

    return (
        <div>
            {/* Summary Stats */}
            <div className="grid grid-cols-3 gap-4 mb-6">
                <div className="bg-blue-50 rounded-lg p-4 border border-blue-100">
                    <p className="text-sm text-blue-600 font-medium mb-1">Total Revenue</p>
                    <p className="text-2xl font-bold text-blue-900">
                        {new Intl.NumberFormat('en-IN', {
                            style: 'currency',
                            currency: 'INR',
                            minimumFractionDigits: 0,
                        }).format(data.totalRevenue)}
                    </p>
                </div>
                <div className="bg-green-50 rounded-lg p-4 border border-green-100">
                    <p className="text-sm text-green-600 font-medium mb-1">Total Commission</p>
                    <p className="text-2xl font-bold text-green-900">
                        {new Intl.NumberFormat('en-IN', {
                            style: 'currency',
                            currency: 'INR',
                            minimumFractionDigits: 0,
                        }).format(data.totalCommission)}
                    </p>
                </div>
                <div className="bg-purple-50 rounded-lg p-4 border border-purple-100">
                    <p className="text-sm text-purple-600 font-medium mb-1">Total Orders</p>
                    <p className="text-2xl font-bold text-purple-900">
                        {data.totalOrders.toLocaleString()}
                    </p>
                </div>
            </div>

            {/* Chart */}
            <div className="h-80">
                <Line data={chartData} options={options} />
            </div>
        </div>
    );
};

export default RevenueChart;
