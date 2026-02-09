import React from 'react';
import { ShoppingCart, Calendar, Users, Lock, AlertTriangle } from 'lucide-react';

/**
 * ProcurementReminder Component
 *
 * Shows upcoming events with locked guest counts
 * Helps vendors plan ingredient procurement
 */

const ProcurementReminder = ({ upcomingOrders = [] }) => {
  // Filter orders that need procurement attention (within 7 days)
  const procurementOrders = upcomingOrders
    .filter(order => {
      const eventDate = new Date(order.eventDate);
      const now = new Date();
      const daysUntil = Math.ceil((eventDate - now) / (1000 * 60 * 60 * 24));
      return daysUntil <= 7 && daysUntil > 0;
    })
    .sort((a, b) => new Date(a.eventDate) - new Date(b.eventDate));

  const getDaysUntilEvent = (eventDate) => {
    const now = new Date();
    const event = new Date(eventDate);
    return Math.ceil((event - now) / (1000 * 60 * 60 * 24));
  };

  const getUrgencyLevel = (days) => {
    if (days <= 2) return { level: 'critical', color: 'red', label: 'URGENT' };
    if (days <= 4) return { level: 'high', color: 'orange', label: 'HIGH' };
    return { level: 'normal', color: 'blue', label: 'NORMAL' };
  };

  if (procurementOrders.length === 0) {
    return (
      <div className="bg-white rounded-lg p-6 shadow-sm border border-gray-200">
        <h3 className="font-semibold text-lg mb-4 flex items-center gap-2">
          <ShoppingCart className="w-5 h-5 text-gray-600" />
          Procurement Reminders
        </h3>
        <div className="text-center py-6">
          <ShoppingCart className="w-12 h-12 text-gray-400 mx-auto mb-3" />
          <p className="text-gray-600">No upcoming procurements</p>
          <p className="text-sm text-gray-500 mt-1">Events beyond 7 days</p>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg p-6 shadow-sm border border-gray-200">
      <div className="flex items-center justify-between mb-4">
        <h3 className="font-semibold text-lg flex items-center gap-2">
          <ShoppingCart className="w-5 h-5 text-blue-600" />
          Procurement Reminders
        </h3>
        <span className="bg-blue-100 text-blue-800 text-sm font-semibold px-3 py-1 rounded-full">
          {procurementOrders.length} upcoming
        </span>
      </div>

      <div className="space-y-3">
        {procurementOrders.map(order => {
          const daysUntil = getDaysUntilEvent(order.eventDate);
          const urgency = getUrgencyLevel(daysUntil);

          return (
            <div
              key={order.orderId}
              className={`border-2 rounded-lg p-4 ${
                urgency.level === 'critical'
                  ? 'border-red-300 bg-red-50'
                  : urgency.level === 'high'
                  ? 'border-orange-300 bg-orange-50'
                  : 'border-blue-300 bg-blue-50'
              }`}
            >
              {/* Header */}
              <div className="flex items-center justify-between mb-3">
                <div className="flex items-center gap-3">
                  <div className={`
                    w-10 h-10 rounded-full flex items-center justify-center
                    ${urgency.level === 'critical' ? 'bg-red-200' : ''}
                    ${urgency.level === 'high' ? 'bg-orange-200' : ''}
                    ${urgency.level === 'normal' ? 'bg-blue-200' : ''}
                  `}>
                    <Calendar className={`w-5 h-5 ${
                      urgency.level === 'critical' ? 'text-red-700' : ''
                    }${urgency.level === 'high' ? 'text-orange-700' : ''}${
                      urgency.level === 'normal' ? 'text-blue-700' : ''
                    }`} />
                  </div>

                  <div>
                    <h4 className="font-semibold text-gray-900">
                      Order #{order.orderNumber}
                    </h4>
                    <p className="text-sm text-gray-600">{order.eventType}</p>
                  </div>
                </div>

                {/* Urgency Badge */}
                <div className={`
                  px-3 py-1.5 rounded-full font-bold text-xs
                  ${urgency.level === 'critical' ? 'bg-red-600 text-white animate-pulse' : ''}
                  ${urgency.level === 'high' ? 'bg-orange-600 text-white' : ''}
                  ${urgency.level === 'normal' ? 'bg-blue-600 text-white' : ''}
                `}>
                  {urgency.label}
                </div>
              </div>

              {/* Event Details */}
              <div className="grid grid-cols-2 gap-3 mb-3">
                <div className="bg-white rounded p-2">
                  <p className="text-xs text-gray-600 mb-1">Event Date</p>
                  <p className="font-semibold text-sm">
                    {new Date(order.eventDate).toLocaleDateString('en-IN', {
                      day: 'numeric',
                      month: 'short',
                      year: 'numeric'
                    })}
                  </p>
                  <p className="text-xs text-gray-500">{daysUntil} days away</p>
                </div>

                <div className="bg-white rounded p-2">
                  <p className="text-xs text-gray-600 mb-1 flex items-center gap-1">
                    <Users className="w-3 h-3" />
                    Guest Count
                  </p>
                  <p className="font-semibold text-sm flex items-center gap-2">
                    {order.guestCount}
                    {order.guestCountLocked && (
                      <Lock className="w-3 h-3 text-gray-600" />
                    )}
                  </p>
                  <p className="text-xs text-gray-500">
                    {order.guestCountLocked ? 'Locked' : 'Not locked yet'}
                  </p>
                </div>
              </div>

              {/* Menu Summary */}
              <div className="bg-white rounded p-3">
                <p className="text-xs font-medium text-gray-700 mb-2">Menu Items:</p>
                <div className="space-y-1">
                  {(order.menuItems || []).slice(0, 3).map((item, idx) => (
                    <div key={idx} className="flex items-center justify-between text-xs">
                      <span className="text-gray-700">{item.itemName}</span>
                      <span className="text-gray-600">×{item.quantity}</span>
                    </div>
                  ))}
                  {order.menuItems?.length > 3 && (
                    <p className="text-xs text-gray-500">
                      +{order.menuItems.length - 3} more items
                    </p>
                  )}
                </div>
              </div>

              {/* Procurement Status */}
              {!order.procurementCompleted && (
                <div className={`mt-3 rounded p-2 ${
                  urgency.level === 'critical'
                    ? 'bg-red-100 border border-red-300'
                    : urgency.level === 'high'
                    ? 'bg-orange-100 border border-orange-300'
                    : 'bg-blue-100 border border-blue-300'
                }`}>
                  <div className="flex items-start gap-2">
                    <AlertTriangle className={`w-4 h-4 flex-shrink-0 mt-0.5 ${
                      urgency.level === 'critical' ? 'text-red-700' : ''
                    }${urgency.level === 'high' ? 'text-orange-700' : ''}${
                      urgency.level === 'normal' ? 'text-blue-700' : ''
                    }`} />
                    <p className={`text-xs ${
                      urgency.level === 'critical' ? 'text-red-900' : ''
                    }${urgency.level === 'high' ? 'text-orange-900' : ''}${
                      urgency.level === 'normal' ? 'text-blue-900' : ''
                    }`}>
                      {urgency.level === 'critical'
                        ? 'Procurement needed immediately! Event is in 2 days or less.'
                        : urgency.level === 'high'
                        ? 'Start procurement soon. Event is in 4 days or less.'
                        : 'Plan procurement. Guest count will lock in a few days.'}
                    </p>
                  </div>
                </div>
              )}

              {order.procurementCompleted && (
                <div className="mt-3 bg-green-100 border border-green-300 rounded p-2">
                  <p className="text-xs text-green-900 flex items-center gap-2">
                    <ShoppingCart className="w-4 h-4" />
                    Procurement marked as complete
                  </p>
                </div>
              )}
            </div>
          );
        })}
      </div>

      {/* Bulk Procurement Action */}
      {procurementOrders.filter(o => o.guestCountLocked).length > 0 && (
        <div className="mt-4 pt-4 border-t border-gray-200">
          <button className="w-full px-4 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors font-medium flex items-center justify-center gap-2">
            <ShoppingCart className="w-5 h-5" />
            Generate Combined Procurement List
          </button>
        </div>
      )}
    </div>
  );
};

export default ProcurementReminder;
