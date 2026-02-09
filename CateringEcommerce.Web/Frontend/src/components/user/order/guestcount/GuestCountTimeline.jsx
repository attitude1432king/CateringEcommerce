import React from 'react';
import { Calendar, Lock, TrendingUp, AlertCircle } from 'lucide-react';

/**
 * GuestCountTimeline Component
 *
 * Visual timeline showing guest count modification periods
 * Timeline: Booking → Day -7 → Day -5 (LOCK) → Event
 */

const GuestCountTimeline = ({
  eventDate,
  currentDate = new Date(),
  guestCountLocked = false
}) => {
  const event = new Date(eventDate);
  const now = new Date(currentDate);

  // Calculate key dates
  const booking = now;
  const day7Before = new Date(event.getTime() - (7 * 24 * 60 * 60 * 1000));
  const day5Before = new Date(event.getTime() - (5 * 24 * 60 * 60 * 1000)); // Lock date
  const day3Before = new Date(event.getTime() - (3 * 24 * 60 * 60 * 1000));
  const day2Before = new Date(event.getTime() - (2 * 24 * 60 * 60 * 1000));

  // Determine current phase
  const getCurrentPhase = () => {
    if (now >= event) return 'event';
    if (now >= day2Before) return 'emergency-only';
    if (now >= day3Before) return 'locked';
    if (now >= day5Before) return 'restricted';
    if (now >= day7Before) return 'limited';
    return 'flexible';
  };

  const currentPhase = guestCountLocked ? 'locked' : getCurrentPhase();

  // Timeline milestones
  const milestones = [
    {
      id: 'booking',
      label: 'Booking',
      date: booking,
      phase: 'flexible',
      icon: Calendar,
      active: currentPhase === 'flexible',
      description: 'Full flexibility'
    },
    {
      id: 'day-7',
      label: 'Day -7',
      date: day7Before,
      phase: 'limited',
      icon: TrendingUp,
      active: currentPhase === 'limited',
      description: 'Limited changes'
    },
    {
      id: 'day-5-lock',
      label: 'LOCK',
      date: day5Before,
      phase: 'restricted',
      icon: Lock,
      active: currentPhase === 'locked' || currentPhase === 'restricted' || currentPhase === 'emergency-only',
      description: 'Guest count locked',
      isLockPoint: true
    },
    {
      id: 'event',
      label: 'Event Day',
      date: event,
      phase: 'event',
      icon: Calendar,
      active: currentPhase === 'event',
      description: 'Event begins'
    }
  ];

  // Phase descriptions
  const phaseDescriptions = {
    flexible: {
      title: 'Flexible Period',
      description: 'Increase or decrease guest count freely with real-time price updates',
      color: 'green',
      icon: '✓'
    },
    limited: {
      title: 'Limited Changes Period',
      description: 'Increase allowed (1.2x pricing) • Decrease allowed (70% refund) • Partner approval required',
      color: 'amber',
      icon: '⚠'
    },
    restricted: {
      title: 'Restricted Period',
      description: 'Decrease disabled • Increase limited to +10% • 2-hour partner approval required',
      color: 'orange',
      icon: '🔒'
    },
    locked: {
      title: 'Count Locked',
      description: 'Decrease disabled • Increase limited to +10% • Partner approval required',
      color: 'red',
      icon: '🔒'
    },
    'emergency-only': {
      title: 'Emergency Only',
      description: 'Emergency increase only • Direct partner payment required • Platform not involved',
      color: 'red',
      icon: '⚡'
    },
    event: {
      title: 'Event Day',
      description: 'Read-only locked count • Emergency note only',
      color: 'gray',
      icon: '📅'
    }
  };

  const currentPhaseInfo = phaseDescriptions[currentPhase];

  // Calculate progress percentage
  const totalDuration = event.getTime() - booking.getTime();
  const elapsed = now.getTime() - booking.getTime();
  const progress = Math.min(100, Math.max(0, (elapsed / totalDuration) * 100));

  return (
    <div className="space-y-6">
      {/* Current Phase Banner */}
      <div className={`
        bg-gradient-to-r rounded-lg p-4 border-l-4
        ${currentPhase === 'flexible' ? 'from-green-50 to-emerald-50 border-green-500' : ''}
        ${currentPhase === 'limited' ? 'from-amber-50 to-yellow-50 border-amber-500' : ''}
        ${(currentPhase === 'restricted' || currentPhase === 'locked') ? 'from-orange-50 to-red-50 border-orange-500' : ''}
        ${currentPhase === 'emergency-only' ? 'from-red-50 to-red-100 border-red-600' : ''}
        ${currentPhase === 'event' ? 'from-gray-50 to-gray-100 border-gray-400' : ''}
      `}>
        <div className="flex items-start gap-3">
          <div className="text-2xl">{currentPhaseInfo.icon}</div>
          <div className="flex-1">
            <h3 className={`
              font-bold text-lg mb-1
              ${currentPhase === 'flexible' ? 'text-green-900' : ''}
              ${currentPhase === 'limited' ? 'text-amber-900' : ''}
              ${(currentPhase === 'restricted' || currentPhase === 'locked') ? 'text-orange-900' : ''}
              ${currentPhase === 'emergency-only' ? 'text-red-900' : ''}
              ${currentPhase === 'event' ? 'text-gray-900' : ''}
            `}>
              {currentPhaseInfo.title}
            </h3>
            <p className={`
              text-sm
              ${currentPhase === 'flexible' ? 'text-green-800' : ''}
              ${currentPhase === 'limited' ? 'text-amber-800' : ''}
              ${(currentPhase === 'restricted' || currentPhase === 'locked') ? 'text-orange-800' : ''}
              ${currentPhase === 'emergency-only' ? 'text-red-800' : ''}
              ${currentPhase === 'event' ? 'text-gray-700' : ''}
            `}>
              {currentPhaseInfo.description}
            </p>
          </div>
        </div>
      </div>

      {/* Timeline Visualization */}
      <div className="bg-white rounded-lg p-6 border-2 border-gray-200">
        <h3 className="font-semibold text-gray-900 mb-6 flex items-center gap-2">
          <Calendar className="w-5 h-5" />
          Guest Count Modification Timeline
        </h3>

        <div className="relative">
          {/* Progress Bar */}
          <div className="mb-12">
            <div className="relative h-2 bg-gray-200 rounded-full overflow-hidden">
              <div
                className="absolute top-0 left-0 h-full bg-gradient-to-r from-green-500 via-amber-500 to-red-500 transition-all duration-500"
                style={{ width: `${progress}%` }}
              />
            </div>
          </div>

          {/* Milestones */}
          <div className="grid grid-cols-4 gap-2">
            {milestones.map((milestone, index) => {
              const Icon = milestone.icon;
              const isActive = milestone.active;
              const isPassed = now >= milestone.date;

              return (
                <div key={milestone.id} className="flex flex-col items-center">
                  {/* Icon Circle */}
                  <div className={`
                    w-16 h-16 rounded-full border-4 flex items-center justify-center mb-3
                    transition-all duration-300
                    ${milestone.isLockPoint ? 'ring-4 ring-red-200' : ''}
                    ${isActive ? 'bg-blue-600 border-blue-600 shadow-lg' :
                      isPassed ? 'bg-green-600 border-green-600' :
                      'bg-white border-gray-300'}
                  `}>
                    <Icon className={`
                      w-8 h-8
                      ${isActive || isPassed ? 'text-white' : 'text-gray-400'}
                    `} />
                  </div>

                  {/* Label */}
                  <p className={`
                    font-semibold text-sm mb-1 text-center
                    ${isActive ? 'text-blue-700' : isPassed ? 'text-green-700' : 'text-gray-600'}
                  `}>
                    {milestone.label}
                  </p>

                  {/* Date */}
                  <p className="text-xs text-gray-500 text-center mb-1">
                    {milestone.date.toLocaleDateString('en-IN', {
                      day: 'numeric',
                      month: 'short'
                    })}
                  </p>

                  {/* Description */}
                  <p className="text-xs text-gray-600 text-center">
                    {milestone.description}
                  </p>

                  {/* Active Indicator */}
                  {isActive && (
                    <span className="mt-2 text-xs font-medium text-blue-700 bg-blue-100 px-2 py-1 rounded-full">
                      Current
                    </span>
                  )}
                </div>
              );
            })}
          </div>
        </div>

        {/* Days Remaining */}
        {currentPhase !== 'event' && (
          <div className="mt-6 pt-4 border-t border-gray-200 text-center">
            <p className="text-sm text-gray-600">
              Days until event: <span className="font-bold text-gray-900">
                {Math.ceil((event.getTime() - now.getTime()) / (1000 * 60 * 60 * 24))}
              </span>
            </p>
            {!guestCountLocked && now < day5Before && (
              <p className="text-xs text-amber-700 mt-1">
                Count will be locked in {Math.ceil((day5Before.getTime() - now.getTime()) / (1000 * 60 * 60 * 24))} days
              </p>
            )}
          </div>
        )}
      </div>
    </div>
  );
};

export default GuestCountTimeline;
