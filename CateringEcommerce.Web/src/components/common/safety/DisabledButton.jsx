import React from 'react';
import { Info, Lock, AlertCircle } from 'lucide-react';

/**
 * DisabledButton Component
 *
 * A button wrapper that shows why an action is disabled when the button cannot be clicked.
 * Implements UX Safety Rule: "Disable buttons instead of allowing invalid actions"
 * and "Show reasons WHY actions are disabled"
 */

const DisabledButton = ({
  children,
  onClick,
  disabled = false,
  disabledReason = '',
  variant = 'primary', // primary, secondary, danger
  size = 'md', // sm, md, lg
  icon: Icon = null,
  fullWidth = false,
  loading = false,
  className = '',
  ...props
}) => {
  // Variant styles
  const variantStyles = {
    primary: {
      enabled: 'bg-gradient-to-r from-catering-primary to-catering-secondary text-white hover:shadow-lg',
      disabled: 'bg-gray-300 text-neutral-500 cursor-not-allowed'
    },
    secondary: {
      enabled: 'bg-white border-2 border-catering-primary text-catering-primary hover:bg-catering-light',
      disabled: 'bg-gray-100 border-2 border-gray-300 text-gray-400 cursor-not-allowed'
    },
    danger: {
      enabled: 'bg-red-600 text-white hover:bg-red-700',
      disabled: 'bg-red-200 text-red-400 cursor-not-allowed'
    }
  };

  // Size styles
  const sizeStyles = {
    sm: 'px-3 py-1.5 text-sm',
    md: 'px-6 py-3 text-base',
    lg: 'px-8 py-4 text-lg'
  };

  const baseStyles = 'rounded-lg font-medium transition-all duration-300 relative';
  const widthStyle = fullWidth ? 'w-full' : '';

  const currentVariant = disabled ? variantStyles[variant].disabled : variantStyles[variant].enabled;
  const currentSize = sizeStyles[size];

  // Determine tooltip icon based on disabled reason
  const getTooltipIcon = () => {
    if (disabledReason.toLowerCase().includes('lock')) {
      return <Lock className="w-4 h-4" />;
    } else if (disabledReason.toLowerCase().includes('error') || disabledReason.toLowerCase().includes('invalid')) {
      return <AlertCircle className="w-4 h-4" />;
    }
    return <Info className="w-4 h-4" />;
  };

  return (
    <div className={`relative inline-block ${fullWidth ? 'w-full' : ''} group`}>
      <button
        onClick={!disabled && !loading ? onClick : undefined}
        disabled={disabled || loading}
        className={`${baseStyles} ${currentVariant} ${currentSize} ${widthStyle} ${className} flex items-center justify-center gap-2`}
        {...props}
      >
        {loading ? (
          <>
            <div className="w-5 h-5 border-2 border-current border-t-transparent rounded-full animate-spin" />
            <span>Loading...</span>
          </>
        ) : (
          <>
            {Icon && <Icon className="w-5 h-5" />}
            {children}
          </>
        )}
      </button>

      {/* Tooltip showing disabled reason */}
      {disabled && disabledReason && (
        <div className="absolute bottom-full left-1/2 -translate-x-1/2 mb-2 w-max max-w-xs opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all duration-200 z-50">
          <div className="bg-gray-900 text-white text-sm rounded-lg py-2 px-3 shadow-lg">
            <div className="flex items-start gap-2">
              <div className="flex-shrink-0 mt-0.5">
                {getTooltipIcon()}
              </div>
              <p className="leading-tight">{disabledReason}</p>
            </div>
            {/* Arrow */}
            <div className="absolute top-full left-1/2 -translate-x-1/2 -mt-1">
              <div className="border-4 border-transparent border-t-gray-900"></div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default DisabledButton;
