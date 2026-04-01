/**
 * AssignmentCard Component
 * Displays assignment summary card
 */

import PropTypes from 'prop-types';
import { Calendar, MapPin, Building2, IndianRupee, ChevronRight, CheckCircle } from 'lucide-react';
import { AssignmentStatusBadge, PaymentStatusBadge } from '../common/badges';
import { formatCurrency, formatTimestamp } from '../../../utils/supervisor/helpers';
import { AssignmentStatus } from '../../../utils/supervisor/supervisorEnums';

const AssignmentCard = ({ assignment, onClick }) => {
  const {
    assignmentNumber,
    eventDate,
    eventLocation,
    eventType,
    supervisorFee,
    assignmentStatus,
    checkedIn,
    vendorName,
    orderNumber,
    paymentReleaseRequested,
    paymentReleaseApproved,
  } = assignment;

  const paymentStatus = paymentReleaseApproved
    ? 'RELEASED'
    : paymentReleaseRequested
    ? 'PENDING'
    : 'NOT_REQUESTED';

  const canCheckIn = assignmentStatus === AssignmentStatus.ACCEPTED && !checkedIn;
  const isUpcoming = new Date(eventDate) > new Date();
  const isPast = new Date(eventDate) < new Date();

  return (
    <div
      onClick={onClick}
      className="bg-white border border-gray-200 rounded-lg p-5 hover:shadow-md transition-shadow cursor-pointer"
    >
      {/* Header */}
      <div className="flex items-start justify-between mb-4">
        <div className="flex-1">
          <h3 className="text-lg font-semibold text-gray-900 mb-1">
            {assignmentNumber}
          </h3>
          <p className="text-sm text-gray-600">
            Order: {orderNumber}
          </p>
        </div>
        <ChevronRight className="w-5 h-5 text-gray-400 flex-shrink-0" />
      </div>

      {/* Event Details */}
      <div className="space-y-2 mb-4">
        {/* Date */}
        <div className="flex items-center gap-2 text-sm">
          <Calendar className="w-4 h-4 text-gray-400 flex-shrink-0" />
          <span className="text-gray-900 font-medium">
            {formatTimestamp(eventDate, 'long')}
          </span>
          {isUpcoming && (
            <span className="text-xs text-blue-600 font-medium">Upcoming</span>
          )}
          {isPast && assignmentStatus !== AssignmentStatus.COMPLETED && (
            <span className="text-xs text-orange-600 font-medium">Past</span>
          )}
        </div>

        {/* Location */}
        <div className="flex items-start gap-2 text-sm">
          <MapPin className="w-4 h-4 text-gray-400 flex-shrink-0 mt-0.5" />
          <span className="text-gray-600 line-clamp-1">{eventLocation}</span>
        </div>

        {/* Vendor */}
        <div className="flex items-center gap-2 text-sm">
          <Building2 className="w-4 h-4 text-gray-400 flex-shrink-0" />
          <span className="text-gray-600">
            {vendorName} • {eventType}
          </span>
        </div>

        {/* Fee */}
        <div className="flex items-center gap-2 text-sm">
          <IndianRupee className="w-4 h-4 text-gray-400 flex-shrink-0" />
          <span className="text-gray-900 font-semibold">
            {formatCurrency(supervisorFee)}
          </span>
        </div>
      </div>

      {/* Status & Badges */}
      <div className="flex items-center gap-2 flex-wrap">
        <AssignmentStatusBadge status={assignmentStatus} />

        {checkedIn && (
          <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800 border border-green-200">
            <CheckCircle className="w-3 h-3" />
            Checked In
          </span>
        )}

        {canCheckIn && (
          <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-orange-100 text-orange-800 border border-orange-200">
            Action Required: Check In
          </span>
        )}

        {assignmentStatus === AssignmentStatus.COMPLETED && (
          <PaymentStatusBadge status={paymentStatus} />
        )}
      </div>
    </div>
  );
};

AssignmentCard.propTypes = {
  assignment: PropTypes.shape({
    assignmentId: PropTypes.number.isRequired,
    assignmentNumber: PropTypes.string.isRequired,
    eventDate: PropTypes.string.isRequired,
    eventLocation: PropTypes.string.isRequired,
    eventType: PropTypes.string.isRequired,
    supervisorFee: PropTypes.number.isRequired,
    assignmentStatus: PropTypes.string.isRequired,
    checkedIn: PropTypes.bool.isRequired,
    vendorName: PropTypes.string.isRequired,
    orderNumber: PropTypes.string.isRequired,
    paymentReleaseRequested: PropTypes.bool,
    paymentReleaseApproved: PropTypes.bool,
  }).isRequired,
  onClick: PropTypes.func.isRequired,
};

export default AssignmentCard;
