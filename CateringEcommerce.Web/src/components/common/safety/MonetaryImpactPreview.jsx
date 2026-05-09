import React from 'react';
import { TrendingUp, TrendingDown, AlertCircle, Info } from 'lucide-react';

/**
 * MonetaryImpactPreview Component
 *
 * Shows monetary impact BEFORE user confirms an action
 * Implements UX Safety Rule: "Always show monetary impact BEFORE confirmation"
 */

const MonetaryImpactPreview = ({
  currentAmount,
  newAmount,
  breakdown = [], // [{label, amount, type: 'add'|'deduct'}]
  impactType = 'auto', // 'auto', 'increase', 'decrease', 'neutral'
  currency = '₹',
  showPercentage = false,
  warningMessage = '',
  infoMessage = '',
  className = ''
}) => {
  // Calculate impact
  const difference = newAmount - currentAmount;
  const percentageChange = currentAmount ? ((difference / currentAmount) * 100).toFixed(2) : 0;

  // Determine impact type if auto
  const actualImpactType = impactType === 'auto'
    ? (difference > 0 ? 'increase' : difference < 0 ? 'decrease' : 'neutral')
    : impactType;

  // Impact styling
  const impactStyles = {
    increase: {
      bg: 'bg-red-50',
      border: 'border-red-200',
      text: 'text-red-700',
      icon: TrendingUp,
      iconColor: 'text-red-600',
      label: 'Additional Cost'
    },
    decrease: {
      bg: 'bg-green-50',
      border: 'border-green-200',
      text: 'text-green-700',
      icon: TrendingDown,
      iconColor: 'text-green-600',
      label: 'Refund Amount'
    },
    neutral: {
      bg: 'bg-blue-50',
      border: 'border-blue-200',
      text: 'text-blue-700',
      icon: Info,
      iconColor: 'text-blue-600',
      label: 'No Change'
    }
  };

  const style = impactStyles[actualImpactType];
  const ImpactIcon = style.icon;

  return (
    <div className={`rounded-lg border-2 ${style.border} ${style.bg} p-4 ${className}`}>
      {/* Header */}
      <div className="flex items-center gap-2 mb-3">
        <ImpactIcon className={`w-5 h-5 ${style.iconColor}`} />
        <h3 className={`font-semibold ${style.text}`}>Monetary Impact</h3>
      </div>

      {/* Current vs New Amount */}
      <div className="grid grid-cols-2 gap-4 mb-4">
        <div>
          <p className="text-xs text-neutral-600 mb-1">Current Amount</p>
          <p className="text-lg font-bold text-neutral-900">
            {currency}{currentAmount.toFixed(2)}
          </p>
        </div>
        <div>
          <p className="text-xs text-neutral-600 mb-1">New Amount</p>
          <p className={`text-lg font-bold ${style.text}`}>
            {currency}{newAmount.toFixed(2)}
          </p>
        </div>
      </div>

      {/* Impact Amount */}
      {difference !== 0 && (
        <div className={`border-t-2 ${style.border} pt-3 mb-3`}>
          <div className="flex items-center justify-between">
            <span className={`text-sm font-medium ${style.text}`}>
              {style.label}
            </span>
            <div className="text-right">
              <span className={`text-xl font-bold ${style.text}`}>
                {difference > 0 ? '+' : ''}{currency}{Math.abs(difference).toFixed(2)}
              </span>
              {showPercentage && percentageChange !== 0 && (
                <span className={`block text-xs ${style.text} mt-1`}>
                  ({difference > 0 ? '+' : ''}{percentageChange}%)
                </span>
              )}
            </div>
          </div>
        </div>
      )}

      {/* Breakdown */}
      {breakdown && breakdown.length > 0 && (
        <div className="mb-3">
          <p className="text-xs font-semibold text-neutral-700 mb-2">Breakdown:</p>
          <div className="space-y-2">
            {breakdown.map((item, index) => (
              <div key={index} className="flex items-center justify-between text-sm">
                <span className="text-neutral-700">{item.label}</span>
                <span className={`font-medium ${item.type === 'deduct' ? 'text-red-600' : 'text-green-600'}`}>
                  {item.type === 'deduct' ? '-' : '+'}{currency}{Math.abs(item.amount).toFixed(2)}
                </span>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Warning Message */}
      {warningMessage && (
        <div className="bg-amber-100 border border-amber-300 rounded-md p-3 mb-2">
          <div className="flex items-start gap-2">
            <AlertCircle className="w-4 h-4 text-amber-700 flex-shrink-0 mt-0.5" />
            <p className="text-xs text-amber-900">{warningMessage}</p>
          </div>
        </div>
      )}

      {/* Info Message */}
      {infoMessage && (
        <div className="bg-blue-100 border border-blue-300 rounded-md p-3">
          <div className="flex items-start gap-2">
            <Info className="w-4 h-4 text-blue-700 flex-shrink-0 mt-0.5" />
            <p className="text-xs text-blue-900">{infoMessage}</p>
          </div>
        </div>
      )}
    </div>
  );
};

export default MonetaryImpactPreview;
