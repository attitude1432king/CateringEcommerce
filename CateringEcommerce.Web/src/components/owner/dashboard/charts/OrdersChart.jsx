import React, { useEffect, useRef } from 'react';
import { Bar } from 'react-chartjs-2';
import { barChartOptions, chartColors } from '../../../../utils/chartConfig';

/**
 * Orders Bar Chart Component
 * Displays order count trends over time
 */
const OrdersChart = ({ data, period = 'month', height = 300 }) => {
    const chartRef = useRef(null);

    // Prepare chart data
    const chartData = {
        labels: data?.labels || [],
        datasets: [
            {
                label: 'Orders',
                data: data?.data || [],
                backgroundColor: chartColors.primaryLight,
                borderColor: chartColors.primary,
                borderWidth: 2,
                borderRadius: 6,
                borderSkipped: false,
                barThickness: 40
            }
        ]
    };

    // Custom options for this chart
    const options = {
        ...barChartOptions,
        plugins: {
            ...barChartOptions.plugins,
            title: {
                display: true,
                text: `Orders Trend (${period.charAt(0).toUpperCase() + period.slice(1)})`,
                font: {
                    size: 16,
                    weight: 'bold',
                    family: "'Inter', sans-serif"
                },
                padding: {
                    top: 10,
                    bottom: 20
                }
            },
            legend: {
                display: true,
                position: 'top',
                labels: {
                    usePointStyle: true,
                    padding: 15
                }
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
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                    </svg>
                    <p className="mt-2">No orders data available</p>
                </div>
            </div>
        );
    }

    return (
        <div style={{ height: `${height}px` }}>
            <Bar ref={chartRef} data={chartData} options={options} />
        </div>
    );
};

export default OrdersChart;
