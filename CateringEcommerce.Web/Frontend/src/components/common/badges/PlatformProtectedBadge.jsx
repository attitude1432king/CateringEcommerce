import React from 'react';
import { Shield, Lock, CheckCircle, RefreshCw, Info } from 'lucide-react';

/**
 * PlatformProtectedBadge Component
 *
 * Displays on order pages to show platform protection features
 * - Escrow payment protection
 * - Refund guarantee
 * - Quality assurance
 */

const PlatformProtectedBadge = ({
  variant = 'full', // 'full', 'compact', 'icon'
  showDetails = true,
  className = ''
}) => {
  const protections = [
    {
      icon: Shield,
      label: 'Escrow Protected',
      description: 'Your payment is held securely until service completion'
    },
    {
      icon: RefreshCw,
      label: 'Refund Guarantee',
      description: 'Up to 30% refund for valid quality complaints'
    },
    {
      icon: CheckCircle,
      label: 'Quality Verified',
      description: 'All catering partners are KYC verified with proven track record'
    }
  ];

  // Full variant with expandable details
  if (variant === 'full') {
    return (
      <div className={`bg-gradient-to-r from-green-50 to-emerald-50 border-2 border-green-300 rounded-lg p-4 ${className}`}>
        <div className="flex items-start gap-3">
          <div className="w-12 h-12 bg-green-600 rounded-full flex items-center justify-center flex-shrink-0">
            <Shield className="w-7 h-7 text-white" />
          </div>
          <div className="flex-1">
            <h3 className="font-bold text-green-900 text-lg mb-1">
              Platform Protected Order
            </h3>
            <p className="text-sm text-green-800 mb-3">
              This booking is protected by our security measures and quality guarantee
            </p>

            {showDetails && (
              <div className="space-y-2">
                {protections.map((protection, index) => {
                  const Icon = protection.icon;
                  return (
                    <div key={index} className="flex items-start gap-2 text-sm">
                      <Icon className="w-4 h-4 text-green-700 flex-shrink-0 mt-0.5" />
                      <div>
                        <p className="font-semibold text-green-900">{protection.label}</p>
                        <p className="text-green-700">{protection.description}</p>
                      </div>
                    </div>
                  );
                })}
              </div>
            )}

            <div className="mt-3 pt-3 border-t border-green-200">
              <a href="#" className="text-sm text-green-700 hover:text-green-900 font-medium underline">
                Learn more about buyer protection →
              </a>
            </div>
          </div>
        </div>
      </div>
    );
  }

  // Compact variant
  if (variant === 'compact') {
    return (
      <div className={`bg-green-100 border border-green-300 rounded-lg px-4 py-2 flex items-center gap-2 ${className} group relative`}>
        <Shield className="w-5 h-5 text-green-700" />
        <span className="font-semibold text-green-900 text-sm">Platform Protected</span>
        <Info className="w-4 h-4 text-green-600 ml-auto cursor-help" />

        {/* Tooltip with protections */}
        <div className="absolute left-0 top-full mt-2 w-80 bg-gray-900 text-white text-xs rounded-lg p-4 opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all duration-200 z-50 shadow-xl">
          <p className="font-semibold mb-2">Your order is protected by:</p>
          <div className="space-y-2">
            {protections.map((protection, index) => {
              const Icon = protection.icon;
              return (
                <div key={index} className="flex items-start gap-2">
                  <Icon className="w-4 h-4 flex-shrink-0 mt-0.5" />
                  <div>
                    <p className="font-semibold">{protection.label}</p>
                    <p className="text-gray-300">{protection.description}</p>
                  </div>
                </div>
              );
            })}
          </div>

          {/* Arrow */}
          <div className="absolute bottom-full left-6 mb-[-1px]">
            <div className="border-4 border-transparent border-b-gray-900"></div>
          </div>
        </div>
      </div>
    );
  }

  // Icon only variant
  return (
    <div className={`inline-flex items-center gap-1.5 ${className} group relative`}>
      <div className="w-6 h-6 bg-green-600 rounded-full flex items-center justify-center">
        <Shield className="w-4 h-4 text-white" />
      </div>
      <span className="text-sm font-medium text-green-900">Protected</span>

      {/* Minimal tooltip */}
      <div className="absolute left-0 top-full mt-1 w-48 bg-gray-900 text-white text-xs rounded-lg py-2 px-3 opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all duration-200 z-50">
        Platform protected with escrow & refund guarantee
        <div className="absolute bottom-full left-4 mb-[-1px]">
          <div className="border-4 border-transparent border-b-gray-900"></div>
        </div>
      </div>
    </div>
  );
};

export default PlatformProtectedBadge;
