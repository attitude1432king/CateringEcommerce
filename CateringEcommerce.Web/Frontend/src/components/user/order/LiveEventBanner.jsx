import React from 'react';
import { Shield, User, Lock, Radio, CheckCircle2, Clock, ChefHat, Truck, MapPin } from 'lucide-react';

/**
 * LiveEventBanner Component
 *
 * Displayed on OrderDetailPage when order status = InProgress
 * Shows: Event timeline stages, supervisor assurance, payment lock notice
 * All data driven by backend liveEventStatus - no frontend time logic
 */

const TIMELINE_STAGES = [
  { key: 'Prepared', label: 'Prepared', icon: ChefHat },
  { key: 'Dispatched', label: 'Dispatched', icon: Truck },
  { key: 'Arrived', label: 'Arrived', icon: MapPin },
  { key: 'InProgress', label: 'In Progress', icon: Radio },
  { key: 'Completed', label: 'Completed', icon: CheckCircle2 }
];

const LiveEventBanner = ({ order, liveEventStatus }) => {
  if (!liveEventStatus) return null;

  const currentStageIndex = TIMELINE_STAGES.findIndex(
    s => s.key === liveEventStatus.eventTimelineStage
  );

  return (
    <div className="bg-gradient-to-r from-purple-50 to-blue-50 border border-purple-200 rounded-lg overflow-hidden mb-4">
      {/* Live Header */}
      <div className="bg-purple-600 text-white px-6 py-3 flex items-center justify-between">
        <div className="flex items-center gap-2">
          <Radio className="w-5 h-5 animate-pulse" />
          <span className="font-semibold text-lg">Live Event In Progress</span>
        </div>
        {liveEventStatus.lastUpdatedAt && (
          <span className="text-purple-200 text-sm">
            Last updated: {new Date(liveEventStatus.lastUpdatedAt).toLocaleTimeString('en-IN', {
              hour: '2-digit',
              minute: '2-digit'
            })}
          </span>
        )}
      </div>

      <div className="p-6 space-y-6">
        {/* Event Summary */}
        <div className="flex items-center gap-4 text-sm text-gray-700">
          <span className="font-medium">{order.cateringName}</span>
          <span className="text-gray-400">|</span>
          <span>{new Date(order.eventDate).toLocaleDateString('en-IN', { day: 'numeric', month: 'short', year: 'numeric' })}</span>
          <span className="text-gray-400">|</span>
          <span>{order.guestCount} Guests</span>
        </div>

        {/* Visual Timeline Stepper */}
        <div className="relative">
          {/* Progress Line */}
          <div className="absolute top-6 left-0 right-0 h-1 bg-gray-200">
            <div
              className="h-full bg-gradient-to-r from-purple-500 to-blue-500 transition-all duration-700"
              style={{
                width: currentStageIndex >= 0
                  ? `${(currentStageIndex / (TIMELINE_STAGES.length - 1)) * 100}%`
                  : '0%'
              }}
            />
          </div>

          {/* Stage Nodes */}
          <div className="grid grid-cols-5 relative">
            {TIMELINE_STAGES.map((stage, index) => {
              const Icon = stage.icon;
              const isCompleted = index < currentStageIndex;
              const isCurrent = index === currentStageIndex;
              const isPending = index > currentStageIndex;

              return (
                <div key={stage.key} className="flex flex-col items-center">
                  <div
                    className={`w-12 h-12 rounded-full flex items-center justify-center relative z-10 border-4 transition-all duration-500 ${
                      isCompleted
                        ? 'bg-purple-100 border-purple-500'
                        : isCurrent
                        ? 'bg-blue-100 border-blue-500 animate-pulse ring-4 ring-blue-200'
                        : 'bg-gray-100 border-gray-300'
                    }`}
                  >
                    <Icon
                      className={`w-5 h-5 ${
                        isCompleted
                          ? 'text-purple-600'
                          : isCurrent
                          ? 'text-blue-600'
                          : 'text-gray-400'
                      }`}
                    />
                  </div>
                  <span
                    className={`mt-2 text-xs font-medium text-center ${
                      isCompleted
                        ? 'text-purple-700'
                        : isCurrent
                        ? 'text-blue-700'
                        : 'text-gray-400'
                    }`}
                  >
                    {stage.label}
                  </span>
                  {isCurrent && (
                    <span className="mt-1 text-xs font-medium text-blue-600 bg-blue-100 px-2 py-0.5 rounded-full">
                      Current
                    </span>
                  )}
                </div>
              );
            })}
          </div>
        </div>

        {/* Supervisor Assurance Block */}
        {liveEventStatus.supervisorAssigned && (
          <div className="flex items-start gap-3 bg-white rounded-lg p-4 border border-green-200">
            <div className="w-10 h-10 rounded-full bg-green-100 flex items-center justify-center flex-shrink-0">
              <Shield className="w-5 h-5 text-green-600" />
            </div>
            <div>
              <h4 className="font-semibold text-green-900">Platform Verified Supervisor On-Site</h4>
              <p className="text-sm text-green-700 mt-1">
                {liveEventStatus.supervisorName
                  ? `${liveEventStatus.supervisorName} is actively monitoring your event to ensure quality and timely service.`
                  : 'A verified supervisor is actively monitoring your event to ensure quality and timely service.'}
              </p>
            </div>
          </div>
        )}

        {/* Actual Guest Count (if updated) */}
        {liveEventStatus.actualGuestCount && liveEventStatus.actualGuestCount !== order.guestCount && (
          <div className="flex items-center gap-2 bg-amber-50 border border-amber-200 rounded-lg px-4 py-3 text-sm">
            <User className="w-4 h-4 text-amber-600" />
            <span className="text-amber-800">
              <strong>Guest count updated:</strong> {liveEventStatus.actualGuestCount} guests (originally {order.guestCount})
            </span>
          </div>
        )}

        {/* Payment Lock Notice */}
        <div className="flex items-start gap-3 bg-gray-50 rounded-lg p-4 border border-gray-200">
          <Lock className="w-5 h-5 text-gray-500 flex-shrink-0 mt-0.5" />
          <div>
            <h4 className="font-medium text-gray-800">Payment Locked During Live Event</h4>
            <p className="text-sm text-gray-600 mt-1">
              All payments and order modifications are locked while your event is in progress.
              The final payment will be unlocked after event completion and supervisor verification.
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default LiveEventBanner;
