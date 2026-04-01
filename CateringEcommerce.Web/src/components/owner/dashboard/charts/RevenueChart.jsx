import React, { useEffect, useRef } from 'react';
import { Line } from 'react-chartjs-2';
import { lineChartOptions, chartColors, formatCurrency } from '../../../../utils/chartConfig';

/**
 * Revenue Line Chart Component
 * Displays revenue trends over time
 */
const RevenueChart = ({ data, period = 'month', height = 300 }) => {
    const chartRef = useRef(null);

    // Prepare chart data
    const chartData = {
        labels: data?.labels || [],
        datasets: [
            {
                label: 'Revenue',
                data: data?.data || [],
                borderColor: chartColors.success,
                backgroundColor: chartColors.successLight,
                fill: true,
                tension: 0.4,
                borderWidth: 2,
                pointRadius: 4,
                pointHoverRadius: 6,
                pointBackgroundColor: chartColors.success,
                pointBorderColor: '#fff',
                pointBorderWidth: 2,
                pointHoverBackgroundColor: chartColors.success,
                pointHoverBorderColor: '#fff',
                isCurrency: true
            }
        ]
    };

    // Custom options for this chart
    const options = {
        ...lineChartOptions,
        plugins: {
            ...lineChartOptions.plugins,
            title: {
                display: true,
                text: `Revenue Trend (${period.charAt(0).toUpperCase() + period.slice(1)})`,
                font: {
                    size: 16,
                    weight: 'bold',
                    family: "'Inter', sans-serif"
                },
                padding: {
                    top: 10,
                    bottom: 20
                }
            }
        },
        scales: {
            ...lineChartOptions.scales,
            y: {
                ...lineChartOptions.scales.y,
                isCurrency: true
            }
        }
    };

    useEffect(() => {
        // Cleanup on unmount
        return () => {
            if (chartRef.current) {
                chartRef.current.destroy();
            }
        };
    }, []);

    if (!data || !data.labels || data.labels.length === 0) {
        return (
            <div className="flex items-center justify-center" style={{ height: `${height}px` }}>
                <div className="text-center text-gray-500">
                    <svg className="mx-auto h-12 w-12 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
                    </svg>
                    <p className="mt-2">No revenue data available</p>
                </div>
            </div>
        );
    }

    return (
        <div style={{ height: `${height}px` }}>
            <Line ref={chartRef} data={chartData} options={options} />
        </div>
    );
};

export default RevenueChart;
