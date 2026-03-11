import React, { useEffect, useRef } from 'react';
import { Doughnut } from 'react-chartjs-2';
import { doughnutChartOptions, chartColorPalette, formatCurrency } from '../../../../utils/chartConfig';

/**
 * Revenue Breakdown Doughnut Chart Component
 * Displays revenue distribution by category
 */
const RevenueBreakdownChart = ({ data, title = 'Revenue Breakdown', height = 300 }) => {
    const chartRef = useRef(null);

    // Prepare chart data
    const chartData = {
        labels: data?.labels || [],
        datasets: [
            {
                data: data?.data || [],
                backgroundColor: chartColorPalette,
                borderColor: '#fff',
                borderWidth: 2,
                hoverOffset: 10,
                isCurrency: true
            }
        ]
    };

    // Custom options for this chart
    const options = {
        ...doughnutChartOptions,
        plugins: {
            ...doughnutChartOptions.plugins,
            title: {
                display: true,
                text: title,
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
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 3.055A9.001 9.001 0 1020.945 13H11V3.055z" />
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20.488 9H15V3.512A9.025 9.025 0 0120.488 9z" />
                    </svg>
                    <p className="mt-2">No breakdown data available</p>
                </div>
            </div>
        );
    }

    return (
        <div style={{ height: `${height}px` }} className="flex items-center justify-center">
            <Doughnut ref={chartRef} data={chartData} options={options} />
        </div>
    );
};

export default RevenueBreakdownChart;
