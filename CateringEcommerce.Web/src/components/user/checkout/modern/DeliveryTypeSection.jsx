import React from 'react';
import { getDeliveryTypeDisplay } from '../../../../utils/checkoutValidator';

const deliveryOptions = [
    {
        value: 'event',
        title: 'Event Catering Delivery',
        description: 'Delivery aligned with your event date and time.'
    },
    {
        value: 'sample',
        title: 'Sample Delivery',
        description: 'Request a tasting sample before final event execution.'
    }
];

const StepHeader = ({ stepNumber, title, subtitle, isActive, isCompleted, onEdit }) => (
    <div className="flex items-center justify-between mb-5">
        <div className="flex items-center gap-3">
            <div className={`w-8 h-8 rounded-full flex items-center justify-center text-sm font-semibold ${isCompleted ? 'bg-green-600 text-white' : isActive ? 'bg-rose-600 text-white' : 'bg-gray-200 text-gray-700'}`}>
                {isCompleted ? '✓' : stepNumber}
            </div>
            <div>
                <h3 className="text-lg font-semibold text-gray-900">{title}</h3>
                <p className="text-sm text-gray-600">{subtitle}</p>
            </div>
        </div>
        {isCompleted && !isActive && (
            <button onClick={onEdit} className="text-sm font-medium text-rose-600 hover:text-rose-700">
                Edit
            </button>
        )}
    </div>
);

const DeliveryTypeSection = ({
    stepNumber,
    isActive,
    isCompleted,
    checkoutData,
    updateCheckoutData,
    errors = {},
    onComplete,
    onEdit
}) => {
    return (
        <div className="bg-white rounded-xl shadow-sm border border-neutral-200 p-6">
            <StepHeader
                stepNumber={stepNumber}
                title="Delivery Type"
                subtitle="Choose how you want your order delivered"
                isActive={isActive}
                isCompleted={isCompleted}
                onEdit={onEdit}
            />

            {!isActive && isCompleted ? (
                <p className="text-sm text-gray-700">
                    <span className="font-medium">Selected:</span> {getDeliveryTypeDisplay(checkoutData.deliveryType)}
                </p>
            ) : null}

            {isActive && (
                <div className="space-y-4">
                    <div className="space-y-3">
                        {deliveryOptions.map((option) => (
                            <label
                                key={option.value}
                                className={`block border rounded-lg p-4 cursor-pointer transition ${checkoutData.deliveryType === option.value ? 'border-rose-500 bg-rose-50' : 'border-gray-200 hover:border-gray-300'}`}
                            >
                                <div className="flex items-start gap-3">
                                    <input
                                        type="radio"
                                        name="deliveryType"
                                        value={option.value}
                                        checked={checkoutData.deliveryType === option.value}
                                        onChange={(e) => updateCheckoutData('deliveryType', e.target.value)}
                                        className="mt-1"
                                    />
                                    <div>
                                        <p className="font-medium text-gray-900">{option.title}</p>
                                        <p className="text-sm text-gray-600">{option.description}</p>
                                    </div>
                                </div>
                            </label>
                        ))}
                    </div>

                    {checkoutData.deliveryType === 'sample' && (
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">
                                Preferred Dispatch Time
                            </label>
                            <input
                                type="datetime-local"
                                value={checkoutData.scheduledDispatchTime || ''}
                                onChange={(e) => updateCheckoutData('scheduledDispatchTime', e.target.value)}
                                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-rose-500 focus:border-transparent"
                            />
                        </div>
                    )}

                    {errors.deliveryType && <p className="text-xs text-red-600">{errors.deliveryType}</p>}

                    <button
                        onClick={onComplete}
                        className="w-full px-6 py-3 bg-rose-600 text-white rounded-lg font-medium hover:bg-rose-700 transition"
                    >
                        Continue to Payment
                    </button>
                </div>
            )}
        </div>
    );
};

export default DeliveryTypeSection;
