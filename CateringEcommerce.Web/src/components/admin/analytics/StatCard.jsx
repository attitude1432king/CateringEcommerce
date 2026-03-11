import { TrendingUp, TrendingDown } from 'lucide-react';

/**
 * Stat Card Component
 * Displays a metric with icon, value, and trend indicator
 */
const StatCard = ({ title, value, change, changeLabel, icon: Icon, iconBgColor, iconColor, loading }) => {
    const isPositive = change >= 0;

    if (loading) {
        return (
            <div className="bg-white rounded-lg border border-gray-200 p-6 animate-pulse">
                <div className="flex items-start justify-between">
                    <div className="flex-1">
                        <div className="h-4 bg-gray-200 rounded w-24 mb-3"></div>
                        <div className="h-8 bg-gray-200 rounded w-32 mb-3"></div>
                        <div className="h-4 bg-gray-200 rounded w-36"></div>
                    </div>
                    <div className="w-12 h-12 bg-gray-200 rounded-lg"></div>
                </div>
            </div>
        );
    }

    return (
        <div className="bg-white rounded-lg border border-gray-200 p-6 hover:shadow-lg transition-shadow">
            <div className="flex items-start justify-between">
                <div className="flex-1">
                    <p className="text-sm font-medium text-gray-600 mb-2">{title}</p>
                    <p className="text-3xl font-bold text-gray-900 mb-2">{value}</p>
                    {change !== undefined && change !== null && (
                        <div className={`flex items-center gap-1 text-sm ${
                            isPositive ? 'text-green-600' : 'text-red-600'
                        }`}>
                            {isPositive ? (
                                <TrendingUp className="w-4 h-4" />
                            ) : (
                                <TrendingDown className="w-4 h-4" />
                            )}
                            <span className="font-semibold">
                                {isPositive ? '+' : ''}{change.toFixed(1)}%
                            </span>
                            <span className="text-gray-500 ml-1">{changeLabel || 'from last period'}</span>
                        </div>
                    )}
                </div>
                {Icon && (
                    <div
                        className={`w-12 h-12 rounded-lg flex items-center justify-center ${iconBgColor || 'bg-blue-100'}`}
                    >
                        <Icon className={`w-6 h-6 ${iconColor || 'text-blue-600'}`} />
                    </div>
                )}
            </div>
        </div>
    );
};

export default StatCard;
