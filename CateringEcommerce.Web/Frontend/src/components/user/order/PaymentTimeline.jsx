import React, { useState, useEffect } from 'react';
import { CheckCircle2, Clock, Lock, XCircle, AlertTriangle, Info } from 'lucide-react';

/**
 * PaymentTimeline Component
 *
 * Displays payment milestones for an order with visual states:
 * - 40% Advance (Paid at booking)
 * - 30% Pre-Event Lock (Auto-charge at T-48h)
 * - 30% Post-Completion (Released after verification)
 *
 * States: ✔ Paid, ⏳ Upcoming, 🔒 Locked, ❌ Failed
 */

const PaymentTimeline = ({
  order,
  layout = 'horizontal' // 'horizontal' or 'vertical'
}) => {
  const [timeUntilLock, setTimeUntilLock] = useState(null);

  // Calculate time until pre-event lock (48 hours before event)
  useEffect(() => {
    if (!order?.eventDate) return;

    const calculateTimeRemaining = () => {
      const eventDate = new Date(order.eventDate);
      const lockTime = new Date(eventDate.getTime() - (48 * 60 * 60 * 1000)); // 48 hours before
      const now = new Date();
      const diff = lockTime - now;

      if (diff <= 0) {
        setTimeUntilLock(null);
        return;
      }

      const hours = Math.floor(diff / (1000 * 60 * 60));
      const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));

      setTimeUntilLock({ hours, minutes, totalHours: hours });
    };

    calculateTimeRemaining();
    const interval = setInterval(calculateTimeRemaining, 60000); // Update every minute

    return () => clearInterval(interval);
  }, [order?.eventDate]);

  // Calculate milestone states based on order data
  const getMilestoneStates = () => {
    const totalAmount = order?.totalAmount || 0;
    const advanceAmount = totalAmount * 0.4;
    const preEventAmount = totalAmount * 0.3;
    const postCompletionAmount = totalAmount * 0.3;

    // Default milestone states
    const milestones = [
      {
        id: 'advance',
        label: 'Advance Payment',
        percentage: 40,
        amount: advanceAmount,
        status: 'paid', // paid, upcoming, locked, failed
        icon: CheckCircle2,
        timestamp: order?.createdDate,
        description: 'Paid at booking'
      },
      {
        id: 'pre-event',
        label: 'Pre-Event Lock',
        percentage: 30,
        amount: preEventAmount,
        status: 'upcoming',
        icon: Clock,
        timestamp: null,
        description: 'Auto-charge at T-48h',
        autoChargeWarning: true
      },
      {
        id: 'post-completion',
        label: 'Post-Completion',
        percentage: 30,
        amount: postCompletionAmount,
        status: 'upcoming',
        icon: Lock,
        timestamp: null,
        description: 'Released after verification'
      }
    ];

    // Determine actual states based on order status and payment status
    const eventDate = new Date(order?.eventDate);
    const now = new Date();
    const lockTime = new Date(eventDate.getTime() - (48 * 60 * 60 * 1000));
    const isPastLockTime = now >= lockTime;
    const isEventCompleted = order?.orderStatus === 'Completed';

    // Update pre-event status
    if (isPastLockTime || order?.preEventPaymentStatus === 'Paid') {
      milestones[1].status = 'paid';
      milestones[1].icon = CheckCircle2;
      milestones[1].timestamp = order?.preEventPaymentDate || lockTime;
    } else if (order?.preEventPaymentStatus === 'Failed') {
      milestones[1].status = 'failed';
      milestones[1].icon = XCircle;
      milestones[1].failureReason = order?.preEventPaymentFailureReason || 'Payment failed';
    } else if (order?.preEventPaymentStatus === 'Locked') {
      milestones[1].status = 'locked';
      milestones[1].icon = Lock;
    }

    // Update post-completion status based on live event status
    const liveStatus = order?.liveEventStatus;
    const isInProgress = order?.orderStatus === 'InProgress';

    if (isEventCompleted && order?.postCompletionPaymentStatus === 'Paid') {
      milestones[2].status = 'paid';
      milestones[2].icon = CheckCircle2;
      milestones[2].timestamp = order?.postCompletionPaymentDate;
    } else if (isEventCompleted && liveStatus?.supervisorReportSubmitted && liveStatus?.paymentRequestRaised) {
      // Supervisor report submitted + payment request raised = unlocked for payment
      milestones[2].status = 'upcoming';
      milestones[2].icon = CheckCircle2;
      milestones[2].description = 'Ready — Approve & Pay';
    } else if (isEventCompleted && liveStatus?.supervisorReportSubmitted) {
      // Report submitted but payment not yet requested
      milestones[2].status = 'locked';
      milestones[2].description = 'Awaiting payment request approval';
    } else if (isEventCompleted) {
      milestones[2].status = 'locked';
      milestones[2].description = 'Awaiting supervisor report';
    } else if (isInProgress) {
      // During live event — explicitly locked
      milestones[2].status = 'locked';
      milestones[2].description = 'Locked during live event';
    }

    return milestones;
  };

  const milestones = getMilestoneStates();

  // Get status styling
  const getStatusStyle = (status) => {
    const styles = {
      paid: {
        bg: 'bg-green-100',
        border: 'border-green-500',
        text: 'text-green-700',
        iconColor: 'text-green-600'
      },
      upcoming: {
        bg: 'bg-blue-50',
        border: 'border-blue-300',
        text: 'text-blue-700',
        iconColor: 'text-blue-500'
      },
      locked: {
        bg: 'bg-gray-100',
        border: 'border-gray-400',
        text: 'text-gray-700',
        iconColor: 'text-gray-600'
      },
      failed: {
        bg: 'bg-red-100',
        border: 'border-red-500',
        text: 'text-red-700',
        iconColor: 'text-red-600'
      }
    };
    return styles[status] || styles.upcoming;
  };

  // Show warning banner for pre-event auto-charge
  const showPreEventWarning = () => {
    const preEventMilestone = milestones.find(m => m.id === 'pre-event');
    if (preEventMilestone.status === 'upcoming' && timeUntilLock && timeUntilLock.totalHours <= 72) {
      return true;
    }
    return false;
  };

  // Show failure alert
  const showFailureAlert = () => {
    return milestones.some(m => m.status === 'failed');
  };

  const failedMilestone = milestones.find(m => m.status === 'failed');

  return (
    <div className="space-y-4">
      {/* Live Event Payment Lock Banner */}
      {order?.orderStatus === 'InProgress' && (
        <div className="bg-purple-50 border-l-4 border-purple-500 p-4 rounded-lg flex items-start gap-3">
          <Lock className="w-5 h-5 text-purple-600 flex-shrink-0 mt-0.5" />
          <div className="flex-1">
            <h4 className="font-semibold text-purple-900 mb-1">
              Payments Locked — Event In Progress
            </h4>
            <p className="text-sm text-purple-800">
              All payment processing is paused while your event is live. The final 30% payment will be unlocked after event completion and supervisor verification.
            </p>
          </div>
        </div>
      )}

      {/* Warning Banner for Pre-Event Auto-Charge */}
      {showPreEventWarning() && (
        <div className="bg-amber-50 border-l-4 border-amber-500 p-4 rounded-lg flex items-start gap-3">
          <AlertTriangle className="w-5 h-5 text-amber-600 flex-shrink-0 mt-0.5" />
          <div className="flex-1">
            <h4 className="font-semibold text-amber-900 mb-1">
              Pre-Event Payment Auto-Charge in {timeUntilLock.hours}h {timeUntilLock.minutes}m
            </h4>
            <p className="text-sm text-amber-800">
              Pre-event payment (₹{milestones[1].amount.toFixed(2)}) will be automatically charged in 48 hours.
              <br />
              <span className="font-medium">Failed payment = automatic cancellation (no refund).</span>
            </p>
            <div className="mt-2 text-sm text-amber-700">
              <div className="flex items-center gap-2">
                <Clock className="w-4 h-4" />
                <span>Countdown: {timeUntilLock.hours} hours, {timeUntilLock.minutes} minutes remaining</span>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Failure Alert */}
      {showFailureAlert() && (
        <div className="bg-red-50 border-l-4 border-red-500 p-4 rounded-lg">
          <div className="flex items-start gap-3">
            <XCircle className="w-6 h-6 text-red-600 flex-shrink-0" />
            <div className="flex-1">
              <h4 className="font-semibold text-red-900 mb-1">Payment Failed</h4>
              <p className="text-sm text-red-800 mb-2">
                {failedMilestone?.failureReason || 'Payment processing failed'}
              </p>
              <p className="text-sm text-red-900 font-medium mb-3">
                Your order has been automatically cancelled. No refund applicable as per cancellation policy.
              </p>
              <button className="px-4 py-2 bg-red-600 text-white text-sm rounded-lg hover:bg-red-700 transition-colors">
                Contact Support
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Payment Timeline */}
      <div className="bg-white rounded-lg p-6 shadow-sm">
        <div className="flex items-center gap-2 mb-6">
          <h2 className="font-semibold text-lg">Payment Milestones</h2>
          <div className="relative group">
            <Info className="w-4 h-4 text-gray-400 cursor-help" />
            <div className="absolute left-0 top-6 w-64 bg-gray-900 text-white text-xs rounded-lg p-3 opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all z-10">
              Payments are processed in three stages: 40% advance at booking, 30% before event, and 30% after completion.
            </div>
          </div>
        </div>

        {layout === 'horizontal' ? (
          <HorizontalTimeline milestones={milestones} getStatusStyle={getStatusStyle} />
        ) : (
          <VerticalTimeline milestones={milestones} getStatusStyle={getStatusStyle} />
        )}

        {/* Platform Protection Badge */}
        <div className="mt-6 pt-4 border-t border-gray-200">
          <div className="flex items-center gap-2 text-sm text-gray-600">
            <CheckCircle2 className="w-4 h-4 text-green-600" />
            <span className="font-medium text-gray-900">Platform Protected</span>
            <span>•</span>
            <span>Escrow payments with refund protection</span>
          </div>
        </div>
      </div>
    </div>
  );
};

// Horizontal Timeline Layout
const HorizontalTimeline = ({ milestones, getStatusStyle }) => {
  return (
    <div className="relative">
      {/* Progress Line */}
      <div className="absolute top-8 left-0 right-0 h-1 bg-gray-200 -translate-y-1/2">
        <div
          className="h-full bg-gradient-to-r from-green-500 to-blue-500 transition-all duration-500"
          style={{
            width: `${(milestones.filter(m => m.status === 'paid').length / milestones.length) * 100}%`
          }}
        />
      </div>

      {/* Milestones */}
      <div className="grid grid-cols-3 gap-4 relative">
        {milestones.map((milestone, index) => {
          const style = getStatusStyle(milestone.status);
          const Icon = milestone.icon;

          return (
            <div key={milestone.id} className="flex flex-col items-center">
              {/* Icon Circle */}
              <div className={`w-16 h-16 rounded-full ${style.bg} ${style.border} border-4 flex items-center justify-center relative z-10`}>
                <Icon className={`w-8 h-8 ${style.iconColor}`} />
              </div>

              {/* Milestone Info */}
              <div className="mt-4 text-center">
                <p className={`font-semibold ${style.text}`}>{milestone.label}</p>
                <p className="text-xs text-gray-500 mt-1">{milestone.description}</p>
                <p className="text-sm font-bold text-gray-900 mt-2">
                  {milestone.percentage}% • ₹{milestone.amount.toFixed(2)}
                </p>
                {milestone.timestamp && (
                  <p className="text-xs text-gray-500 mt-1">
                    {new Date(milestone.timestamp).toLocaleDateString('en-IN', {
                      day: 'numeric',
                      month: 'short',
                      year: 'numeric'
                    })}
                  </p>
                )}
                {milestone.status === 'paid' && (
                  <span className="inline-block mt-2 text-xs font-medium text-green-700 bg-green-100 px-2 py-1 rounded-full">
                    ✔ Paid
                  </span>
                )}
                {milestone.status === 'upcoming' && (
                  <span className="inline-block mt-2 text-xs font-medium text-blue-700 bg-blue-100 px-2 py-1 rounded-full">
                    ⏳ Upcoming
                  </span>
                )}
                {milestone.status === 'locked' && (
                  <span className="inline-block mt-2 text-xs font-medium text-gray-700 bg-gray-100 px-2 py-1 rounded-full">
                    🔒 Locked
                  </span>
                )}
                {milestone.status === 'failed' && (
                  <span className="inline-block mt-2 text-xs font-medium text-red-700 bg-red-100 px-2 py-1 rounded-full">
                    ❌ Failed
                  </span>
                )}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
};

// Vertical Timeline Layout
const VerticalTimeline = ({ milestones, getStatusStyle }) => {
  return (
    <div className="relative pl-8">
      {/* Vertical Line */}
      <div className="absolute left-8 top-0 bottom-0 w-1 bg-gray-200">
        <div
          className="w-full bg-gradient-to-b from-green-500 to-blue-500 transition-all duration-500"
          style={{
            height: `${(milestones.filter(m => m.status === 'paid').length / milestones.length) * 100}%`
          }}
        />
      </div>

      {/* Milestones */}
      <div className="space-y-8">
        {milestones.map((milestone, index) => {
          const style = getStatusStyle(milestone.status);
          const Icon = milestone.icon;

          return (
            <div key={milestone.id} className="flex items-start gap-6 relative">
              {/* Icon Circle */}
              <div className={`w-16 h-16 rounded-full ${style.bg} ${style.border} border-4 flex items-center justify-center flex-shrink-0 relative z-10`}>
                <Icon className={`w-8 h-8 ${style.iconColor}`} />
              </div>

              {/* Milestone Info */}
              <div className="flex-1 pb-8">
                <div className="flex items-center justify-between mb-2">
                  <p className={`font-semibold text-lg ${style.text}`}>{milestone.label}</p>
                  {milestone.status === 'paid' && (
                    <span className="text-xs font-medium text-green-700 bg-green-100 px-3 py-1 rounded-full">
                      ✔ Paid
                    </span>
                  )}
                  {milestone.status === 'upcoming' && (
                    <span className="text-xs font-medium text-blue-700 bg-blue-100 px-3 py-1 rounded-full">
                      ⏳ Upcoming
                    </span>
                  )}
                  {milestone.status === 'locked' && (
                    <span className="text-xs font-medium text-gray-700 bg-gray-100 px-3 py-1 rounded-full">
                      🔒 Locked
                    </span>
                  )}
                  {milestone.status === 'failed' && (
                    <span className="text-xs font-medium text-red-700 bg-red-100 px-3 py-1 rounded-full">
                      ❌ Failed
                    </span>
                  )}
                </div>
                <p className="text-sm text-gray-600 mb-2">{milestone.description}</p>
                <p className="text-lg font-bold text-gray-900">
                  {milestone.percentage}% • ₹{milestone.amount.toFixed(2)}
                </p>
                {milestone.timestamp && (
                  <p className="text-sm text-gray-500 mt-1">
                    Paid on {new Date(milestone.timestamp).toLocaleDateString('en-IN', {
                      day: 'numeric',
                      month: 'long',
                      year: 'numeric',
                      hour: '2-digit',
                      minute: '2-digit'
                    })}
                  </p>
                )}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
};

export default PaymentTimeline;
