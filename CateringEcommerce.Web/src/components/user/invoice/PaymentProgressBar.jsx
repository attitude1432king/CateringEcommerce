import React, { useEffect, useState } from 'react';
import { CheckCircle, Circle, AlertCircle } from 'lucide-react';
import invoiceApi from '../../../services/invoiceApi';

/**
 * Payment Progress Bar Component
 * Displays payment milestones: 40% (Booking) → 75% (Pre-Event) → 100% (Complete)
 */
const PaymentProgressBar = ({ orderId }) => {
    const [progress, setProgress] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        fetchPaymentProgress();
    }, [orderId]);

    const fetchPaymentProgress = async () => {
        try {
            setLoading(true);
            const response = await invoiceApi.getPaymentProgress(orderId);

            if (response.success) {
                setProgress(response.data);
            }
        } catch (err) {
            console.error('Error fetching payment progress:', err);
            setError('Failed to load payment progress');
        } finally {
            setLoading(false);
        }
    };

    if (loading) {
        return (
            <div className="animate-pulse">
                <div className="h-20 bg-gray-200 rounded-lg"></div>
            </div>
        );
    }

    if (error) {
        return (
            <div className="bg-red-50 border border-red-200 rounded-lg p-4">
                <div className="flex items-center gap-2 text-red-600">
                    <AlertCircle size={20} />
                    <span>{error}</span>
                </div>
            </div>
        );
    }

    if (!progress) return null;

    const milestones = [
        {
            label: 'Booking Payment',
            percentage: 40,
            description: 'Order confirmation',
            completed: progress.milestones.booking.completed
        },
        {
            label: 'Pre-Event Payment',
            percentage: 75,
            description: 'Event can start',
            completed: progress.milestones.preEvent.completed
        },
        {
            label: 'Final Payment',
            percentage: 100,
            description: 'Order complete',
            completed: progress.milestones.fullPayment.completed
        }
    ];

    const currentPercentage = progress.progressPercentage;

    return (
        <div className="bg-white rounded-lg shadow-sm p-6">
            <div className="mb-6">
                <div className="flex items-center justify-between mb-2">
                    <h3 className="text-lg font-semibold text-neutral-900">
                        Payment Progress
                    </h3>
                    <span className="text-2xl font-bold text-blue-600">
                        {progress.formattedProgress}
                    </span>
                </div>

                {/* Progress bar */}
                <div className="relative">
                    <div className="h-3 bg-gray-200 rounded-full overflow-hidden">
                        <div
                            className="h-full bg-gradient-to-r from-blue-500 to-blue-600 transition-all duration-500"
                            style={{ width: `${currentPercentage}%` }}
                        ></div>
                    </div>

                    {/* Milestone markers */}
                    <div className="absolute top-0 left-0 w-full h-3 flex justify-between px-1">
                        {milestones.map((milestone, index) => (
                            <div
                                key={index}
                                className="relative"
                                style={{ left: `${milestone.percentage - 2}%` }}
                            >
                                <div
                                    className={`w-1 h-3 ${
                                        currentPercentage >= milestone.percentage
                                            ? 'bg-white opacity-50'
                                            : 'bg-gray-400'
                                    }`}
                                ></div>
                            </div>
                        ))}
                    </div>
                </div>
            </div>

            {/* Milestones list */}
            <div className="space-y-4">
                {milestones.map((milestone, index) => (
                    <div
                        key={index}
                        className={`flex items-start gap-3 p-3 rounded-lg transition-all ${
                            milestone.completed
                                ? 'bg-green-50 border border-green-200'
                                : currentPercentage >= milestone.percentage - 10
                                ? 'bg-yellow-50 border border-yellow-200'
                                : 'bg-gray-50 border border-gray-200'
                        }`}
                    >
                        <div className="flex-shrink-0 mt-0.5">
                            {milestone.completed ? (
                                <CheckCircle className="text-green-600" size={24} />
                            ) : (
                                <Circle className="text-gray-400" size={24} />
                            )}
                        </div>

                        <div className="flex-1">
                            <div className="flex items-center justify-between">
                                <h4 className="font-semibold text-neutral-900">
                                    {milestone.label}
                                </h4>
                                <span
                                    className={`text-sm font-medium ${
                                        milestone.completed
                                            ? 'text-green-600'
                                            : 'text-neutral-500'
                                    }`}
                                >
                                    {milestone.percentage}%
                                </span>
                            </div>
                            <p className="text-sm text-neutral-600 mt-1">
                                {milestone.description}
                            </p>
                            {milestone.completed && (
                                <p className="text-xs text-green-600 mt-1 font-medium">
                                    ✓ Completed
                                </p>
                            )}
                        </div>
                    </div>
                ))}
            </div>

            {/* Critical 75% notice */}
            {currentPercentage < 75 && (
                <div className="mt-6 bg-yellow-50 border border-yellow-200 rounded-lg p-4">
                    <div className="flex items-start gap-2">
                        <AlertCircle className="text-yellow-600 flex-shrink-0 mt-0.5" size={20} />
                        <div>
                            <p className="text-sm font-medium text-yellow-900">
                                Payment Required Before Event
                            </p>
                            <p className="text-xs text-yellow-700 mt-1">
                                Your event cannot start until 75% payment (Booking + Pre-Event) is received.
                                {currentPercentage < 40 && ' Please complete the booking payment first.'}
                            </p>
                        </div>
                    </div>
                </div>
            )}

            {/* Completion celebration */}
            {currentPercentage >= 100 && (
                <div className="mt-6 bg-green-50 border border-green-200 rounded-lg p-4">
                    <div className="flex items-start gap-2">
                        <CheckCircle className="text-green-600 flex-shrink-0 mt-0.5" size={20} />
                        <div>
                            <p className="text-sm font-medium text-green-900">
                                All Payments Complete! 🎉
                            </p>
                            <p className="text-xs text-green-700 mt-1">
                                Thank you for completing all payments. Your order is now fully settled.
                            </p>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default PaymentProgressBar;
