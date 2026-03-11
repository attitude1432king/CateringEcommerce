import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  BarElement,
  ArcElement,
  Title,
  Tooltip,
  Legend,
  Filler
} from 'chart.js';

// Register Chart.js components
ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  BarElement,
  ArcElement,
  Title,
  Tooltip,
  Legend,
  Filler
);

// Color palette for charts
export const chartColors = {
  primary: 'rgb(99, 102, 241)',      // Indigo
  primaryLight: 'rgba(99, 102, 241, 0.2)',
  success: 'rgb(34, 197, 94)',       // Green
  successLight: 'rgba(34, 197, 94, 0.2)',
  warning: 'rgb(251, 191, 36)',      // Amber
  warningLight: 'rgba(251, 191, 36, 0.2)',
  danger: 'rgb(239, 68, 68)',        // Red
  dangerLight: 'rgba(239, 68, 68, 0.2)',
  info: 'rgb(59, 130, 246)',         // Blue
  infoLight: 'rgba(59, 130, 246, 0.2)',
  purple: 'rgb(168, 85, 247)',       // Purple
  purpleLight: 'rgba(168, 85, 247, 0.2)',
  pink: 'rgb(236, 72, 153)',         // Pink
  pinkLight: 'rgba(236, 72, 153, 0.2)',
  orange: 'rgb(249, 115, 22)',       // Orange
  orangeLight: 'rgba(249, 115, 22, 0.2)',
  gray: 'rgb(107, 114, 128)',        // Gray
  grayLight: 'rgba(107, 114, 128, 0.2)',
};

// Multi-color palette for charts with multiple datasets
export const chartColorPalette = [
  'rgb(99, 102, 241)',   // Indigo
  'rgb(34, 197, 94)',    // Green
  'rgb(251, 191, 36)',   // Amber
  'rgb(239, 68, 68)',    // Red
  'rgb(59, 130, 246)',   // Blue
  'rgb(168, 85, 247)',   // Purple
  'rgb(236, 72, 153)',   // Pink
  'rgb(249, 115, 22)',   // Orange
];

// Light versions for backgrounds
export const chartColorPaletteLight = [
  'rgba(99, 102, 241, 0.2)',
  'rgba(34, 197, 94, 0.2)',
  'rgba(251, 191, 36, 0.2)',
  'rgba(239, 68, 68, 0.2)',
  'rgba(59, 130, 246, 0.2)',
  'rgba(168, 85, 247, 0.2)',
  'rgba(236, 72, 153, 0.2)',
  'rgba(249, 115, 22, 0.2)',
];

// Default options for line charts
export const lineChartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      position: 'top',
      labels: {
        usePointStyle: true,
        padding: 15,
        font: {
          size: 12,
          family: "'Inter', sans-serif"
        }
      }
    },
    tooltip: {
      mode: 'index',
      intersect: false,
      backgroundColor: 'rgba(0, 0, 0, 0.8)',
      titleFont: {
        size: 13,
        family: "'Inter', sans-serif"
      },
      bodyFont: {
        size: 12,
        family: "'Inter', sans-serif"
      },
      padding: 12,
      cornerRadius: 8,
      callbacks: {
        label: function(context) {
          let label = context.dataset.label || '';
          if (label) {
            label += ': ';
          }
          if (context.parsed.y !== null) {
            // Check if it's a currency value (starts with ₹)
            if (context.dataset.isCurrency) {
              label += '₹' + context.parsed.y.toLocaleString('en-IN', { maximumFractionDigits: 0 });
            } else {
              label += context.parsed.y.toLocaleString('en-IN');
            }
          }
          return label;
        }
      }
    }
  },
  scales: {
    x: {
      grid: {
        display: false,
        drawBorder: false
      },
      ticks: {
        font: {
          size: 11,
          family: "'Inter', sans-serif"
        }
      }
    },
    y: {
      beginAtZero: true,
      grid: {
        color: 'rgba(0, 0, 0, 0.05)',
        drawBorder: false
      },
      ticks: {
        font: {
          size: 11,
          family: "'Inter', sans-serif"
        },
        callback: function(value) {
          if (this.chart.config.options.scales.y.isCurrency) {
            return '₹' + value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
          }
          return value.toLocaleString('en-IN');
        }
      }
    }
  },
  interaction: {
    mode: 'nearest',
    axis: 'x',
    intersect: false
  }
};

// Default options for bar charts
export const barChartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      display: false
    },
    tooltip: {
      backgroundColor: 'rgba(0, 0, 0, 0.8)',
      titleFont: {
        size: 13,
        family: "'Inter', sans-serif"
      },
      bodyFont: {
        size: 12,
        family: "'Inter', sans-serif"
      },
      padding: 12,
      cornerRadius: 8,
      callbacks: {
        label: function(context) {
          let label = context.dataset.label || '';
          if (label) {
            label += ': ';
          }
          if (context.parsed.y !== null) {
            if (context.dataset.isCurrency) {
              label += '₹' + context.parsed.y.toLocaleString('en-IN', { maximumFractionDigits: 0 });
            } else {
              label += context.parsed.y.toLocaleString('en-IN');
            }
          }
          return label;
        }
      }
    }
  },
  scales: {
    x: {
      grid: {
        display: false,
        drawBorder: false
      },
      ticks: {
        font: {
          size: 11,
          family: "'Inter', sans-serif"
        }
      }
    },
    y: {
      beginAtZero: true,
      grid: {
        color: 'rgba(0, 0, 0, 0.05)',
        drawBorder: false
      },
      ticks: {
        font: {
          size: 11,
          family: "'Inter', sans-serif"
        },
        callback: function(value) {
          if (this.chart.config.options.scales.y.isCurrency) {
            return '₹' + value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
          }
          return value.toLocaleString('en-IN');
        }
      }
    }
  }
};

// Default options for doughnut/pie charts
export const doughnutChartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      position: 'right',
      labels: {
        usePointStyle: true,
        padding: 15,
        font: {
          size: 12,
          family: "'Inter', sans-serif"
        },
        generateLabels: function(chart) {
          const data = chart.data;
          if (data.labels.length && data.datasets.length) {
            return data.labels.map((label, i) => {
              const value = data.datasets[0].data[i];
              const total = data.datasets[0].data.reduce((a, b) => a + b, 0);
              const percentage = ((value / total) * 100).toFixed(1);
              return {
                text: `${label} (${percentage}%)`,
                fillStyle: data.datasets[0].backgroundColor[i],
                hidden: false,
                index: i
              };
            });
          }
          return [];
        }
      }
    },
    tooltip: {
      backgroundColor: 'rgba(0, 0, 0, 0.8)',
      titleFont: {
        size: 13,
        family: "'Inter', sans-serif"
      },
      bodyFont: {
        size: 12,
        family: "'Inter', sans-serif"
      },
      padding: 12,
      cornerRadius: 8,
      callbacks: {
        label: function(context) {
          let label = context.label || '';
          if (label) {
            label += ': ';
          }
          if (context.parsed !== null) {
            const total = context.dataset.data.reduce((a, b) => a + b, 0);
            const percentage = ((context.parsed / total) * 100).toFixed(1);
            if (context.dataset.isCurrency) {
              label += '₹' + context.parsed.toLocaleString('en-IN', { maximumFractionDigits: 0 });
            } else {
              label += context.parsed.toLocaleString('en-IN');
            }
            label += ` (${percentage}%)`;
          }
          return label;
        }
      }
    }
  }
};

// Helper function to create gradient
export const createGradient = (ctx, color1, color2) => {
  const gradient = ctx.createLinearGradient(0, 0, 0, 400);
  gradient.addColorStop(0, color1);
  gradient.addColorStop(1, color2);
  return gradient;
};

// Helper function to format currency
export const formatCurrency = (value) => {
  return '₹' + value.toLocaleString('en-IN', { maximumFractionDigits: 0 });
};

// Helper function to format number
export const formatNumber = (value) => {
  return value.toLocaleString('en-IN');
};

// Helper function to get color by index
export const getChartColor = (index) => {
  return chartColorPalette[index % chartColorPalette.length];
};

// Helper function to get light color by index
export const getChartColorLight = (index) => {
  return chartColorPaletteLight[index % chartColorPaletteLight.length];
};
