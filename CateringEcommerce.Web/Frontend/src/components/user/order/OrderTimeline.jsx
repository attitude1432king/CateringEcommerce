import React from 'react';
import {
  CheckCircle,
  Clock,
  UserCheck,
  Users,
  ChefHat,
  Truck,
  PartyPopper,
  FileCheck,
  Shield,
  MapPin,
  Radio,
  Star
} from 'lucide-react';
import { formatDistanceToNow } from 'date-fns';

/**
 * OrderTimeline Component
 *
 * Real-time activity log showing order progress
 * Displays key events: partner confirmation, guest count lock, dispatch, completion
 */

const OrderTimeline = ({
  order,
  events = [], // Custom events array if provided
  showAllEvents = true
}) => {
  // Generate timeline events from order data
  const generateTimelineEvents = () => {
    const timeline = [];

    // Order Placed
    if (order.createdDate) {
      timeline.push({
        id: 'order_placed',
        title: 'Order Placed',
        description: `Order #${order.orderNumber} created successfully`,
        timestamp: order.createdDate,
        icon: FileCheck,
        status: 'completed',
        color: 'green'
      });
    }

    // Partner Confirmed
    if (order.orderStatus !== 'Pending' && order.confirmedDate) {
      timeline.push({
        id: 'partner_confirmed',
        title: 'Partner Confirmed Order',
        description: `${order.cateringName} accepted your booking`,
        timestamp: order.confirmedDate,
        icon: UserCheck,
        status: 'completed',
        color: 'blue'
      });
    }

    // Guest Count Locked
    if (order.guestCountLockedDate) {
      timeline.push({
        id: 'guest_count_locked',
        title: 'Guest Count Locked',
        description: `Final count: ${order.guestCount} guests`,
        timestamp: order.guestCountLockedDate,
        icon: Users,
        status: 'completed',
        color: 'purple'
      });
    } else {
      // Upcoming lock
      const eventDate = new Date(order.eventDate);
      const lockDate = new Date(eventDate.getTime() - (5 * 24 * 60 * 60 * 1000)); // 5 days before
      const now = new Date();

      if (now < lockDate) {
        timeline.push({
          id: 'guest_count_lock_pending',
          title: 'Guest Count Lock',
          description: `Will be locked 5 days before event`,
          timestamp: lockDate,
          icon: Users,
          status: 'pending',
          color: 'gray'
        });
      }
    }

    // Chef/Team Dispatched
    if (order.teamDispatchedDate) {
      timeline.push({
        id: 'team_dispatched',
        title: 'Chef & Team Dispatched',
        description: 'Service team is on the way to your venue',
        timestamp: order.teamDispatchedDate,
        icon: ChefHat,
        status: 'completed',
        color: 'orange'
      });
    }

    // Supervisor-sourced events (from liveEventStatus)
    const liveStatus = order.liveEventStatus;
    if (liveStatus && liveStatus.supervisorAssigned) {
      const stage = liveStatus.eventTimelineStage;
      const stageOrder = ['Assigned', 'Prepared', 'Dispatched', 'Arrived', 'InProgress', 'Completed'];
      const currentIdx = stageOrder.indexOf(stage);
      const stageTimestamp = liveStatus.lastUpdatedAt || new Date();

      // Supervisor Assigned
      timeline.push({
        id: 'supervisor_assigned',
        title: 'Supervisor Assigned',
        description: liveStatus.supervisorName
          ? `${liveStatus.supervisorName} will oversee your event`
          : 'A verified supervisor has been assigned',
        timestamp: stageTimestamp,
        icon: Shield,
        status: currentIdx >= 0 ? 'completed' : 'pending',
        color: 'green'
      });

      // Team Prepared
      if (currentIdx >= stageOrder.indexOf('Prepared')) {
        timeline.push({
          id: 'team_prepared',
          title: 'Team Prepared',
          description: 'Food and materials verified by supervisor',
          timestamp: stageTimestamp,
          icon: ChefHat,
          status: 'completed',
          color: 'green'
        });
      }

      // Dispatched
      if (currentIdx >= stageOrder.indexOf('Dispatched')) {
        timeline.push({
          id: 'team_dispatched_live',
          title: 'Team Dispatched',
          description: 'Service team is on the way to your venue',
          timestamp: stageTimestamp,
          icon: Truck,
          status: 'completed',
          color: 'orange'
        });
      }

      // Supervisor Arrived
      if (currentIdx >= stageOrder.indexOf('Arrived')) {
        timeline.push({
          id: 'supervisor_arrived',
          title: 'Supervisor Arrived at Venue',
          description: 'On-site supervision has begun',
          timestamp: stageTimestamp,
          icon: MapPin,
          status: 'completed',
          color: 'blue'
        });
      }

      // In Progress
      if (stage === 'InProgress') {
        timeline.push({
          id: 'event_in_progress',
          title: 'Event In Progress',
          description: 'Catering service is being supervised live',
          timestamp: stageTimestamp,
          icon: Radio,
          status: 'in_progress',
          color: 'blue'
        });
      }

      // Completed
      if (stage === 'Completed') {
        timeline.push({
          id: 'event_completed_live',
          title: 'Event Completed',
          description: liveStatus.serviceQualityRating
            ? `Service completed with ${liveStatus.serviceQualityRating}/5 quality rating`
            : 'Event catering service successfully completed',
          timestamp: stageTimestamp,
          icon: PartyPopper,
          status: 'completed',
          color: 'green'
        });
      }
    } else {
      // Fallback: original logic for orders without supervisor
      // Service Completed
      if (order.orderStatus === 'Completed' && order.completedDate) {
        timeline.push({
          id: 'service_completed',
          title: 'Service Completed',
          description: 'Event catering service successfully delivered',
          timestamp: order.completedDate,
          icon: PartyPopper,
          status: 'completed',
          color: 'green'
        });
      } else if (order.orderStatus === 'InProgress') {
        // Event in progress
        timeline.push({
          id: 'event_in_progress',
          title: 'Event In Progress',
          description: 'Catering service is currently being provided',
          timestamp: new Date(),
          icon: Truck,
          status: 'in_progress',
          color: 'blue'
        });
      }
    }

    // Sort by timestamp
    return timeline.sort((a, b) => new Date(a.timestamp) - new Date(b.timestamp));
  };

  const timelineEvents = events.length > 0 ? events : generateTimelineEvents();

  // Get status styling
  const getStatusStyle = (status, color) => {
    const statusStyles = {
      completed: {
        iconBg: `bg-${color}-100`,
        iconColor: `text-${color}-600`,
        line: `bg-${color}-500`
      },
      in_progress: {
        iconBg: `bg-${color}-100 animate-pulse`,
        iconColor: `text-${color}-600`,
        line: `bg-${color}-300`
      },
      pending: {
        iconBg: 'bg-gray-100',
        iconColor: 'text-gray-400',
        line: 'bg-gray-300'
      }
    };

    // Fallback for Tailwind dynamic classes
    const baseStyles = {
      completed: {
        iconBg: 'bg-green-100',
        iconColor: 'text-green-600',
        line: 'bg-green-500'
      },
      in_progress: {
        iconBg: 'bg-blue-100 animate-pulse',
        iconColor: 'text-blue-600',
        line: 'bg-blue-300'
      },
      pending: {
        iconBg: 'bg-gray-100',
        iconColor: 'text-gray-400',
        line: 'bg-gray-300'
      }
    };

    return baseStyles[status] || baseStyles.pending;
  };

  if (!timelineEvents || timelineEvents.length === 0) {
    return null;
  }

  return (
    <div className="bg-white rounded-lg p-6 shadow-sm">
      <h2 className="font-semibold text-lg mb-6 flex items-center gap-2">
        <Clock className="w-5 h-5 text-gray-600" />
        Order Activity Timeline
      </h2>

      <div className="relative">
        {/* Vertical Line */}
        <div className="absolute left-6 top-0 bottom-0 w-0.5 bg-gray-200"></div>

        {/* Timeline Events */}
        <div className="space-y-6">
          {timelineEvents.map((event, index) => {
            const style = getStatusStyle(event.status, event.color);
            const Icon = event.icon;
            const isLast = index === timelineEvents.length - 1;

            return (
              <div key={event.id} className="relative flex items-start gap-4">
                {/* Icon */}
                <div className={`w-12 h-12 rounded-full ${style.iconBg} flex items-center justify-center flex-shrink-0 relative z-10 ring-4 ring-white`}>
                  <Icon className={`w-6 h-6 ${style.iconColor}`} />
                </div>

                {/* Content */}
                <div className="flex-1 pb-6">
                  <div className="flex items-center justify-between mb-1">
                    <h3 className="font-semibold text-gray-900">{event.title}</h3>
                    {event.status === 'completed' && (
                      <CheckCircle className="w-4 h-4 text-green-600" />
                    )}
                    {event.status === 'in_progress' && (
                      <span className="text-xs font-medium text-blue-700 bg-blue-100 px-2 py-1 rounded-full">
                        Live
                      </span>
                    )}
                    {event.status === 'pending' && (
                      <span className="text-xs font-medium text-gray-600 bg-gray-100 px-2 py-1 rounded-full">
                        Upcoming
                      </span>
                    )}
                  </div>

                  <p className="text-sm text-gray-600 mb-1">
                    {event.description}
                  </p>

                  <p className="text-xs text-gray-500">
                    {event.status === 'completed' ? (
                      <>
                        {formatDistanceToNow(new Date(event.timestamp), { addSuffix: true })}
                        {' • '}
                        {new Date(event.timestamp).toLocaleString('en-IN', {
                          day: 'numeric',
                          month: 'short',
                          year: 'numeric',
                          hour: '2-digit',
                          minute: '2-digit'
                        })}
                      </>
                    ) : event.status === 'pending' ? (
                      `Expected: ${new Date(event.timestamp).toLocaleString('en-IN', {
                        day: 'numeric',
                        month: 'short',
                        year: 'numeric'
                      })}`
                    ) : (
                      'Just now'
                    )}
                  </p>
                </div>
              </div>
            );
          })}
        </div>

        {/* Last event indicator */}
        {timelineEvents[timelineEvents.length - 1]?.status === 'completed' && (
          <div className="flex items-center gap-2 text-sm text-gray-500 mt-4 ml-16">
            <CheckCircle className="w-4 h-4 text-green-600" />
            <span>All events up to date</span>
          </div>
        )}
      </div>
    </div>
  );
};

export default OrderTimeline;
