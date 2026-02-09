/**
 * AssignmentStatusBadge Component
 * Displays assignment status with appropriate color
 */

import PropTypes from 'prop-types';
import { getAssignmentStatusLabel, getAssignmentStatusColor } from '../../../../utils/supervisor/supervisorEnums';

const AssignmentStatusBadge = ({ status, className = '' }) => {
  const label = getAssignmentStatusLabel(status);
  const color = getAssignmentStatusColor(status);

  const colorClasses = {
    green: 'bg-green-100 text-green-800 border-green-200',
    blue: 'bg-blue-100 text-blue-800 border-blue-200',
    yellow: 'bg-yellow-100 text-yellow-800 border-yellow-200',
    orange: 'bg-orange-100 text-orange-800 border-orange-200',
    red: 'bg-red-100 text-red-800 border-red-200',
    gray: 'bg-gray-100 text-gray-800 border-gray-200',
  };

  return (
    <span
      className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border ${
        colorClasses[color] || colorClasses.gray
      } ${className}`}
    >
      {label}
    </span>
  );
};

AssignmentStatusBadge.propTypes = {
  status: PropTypes.string.isRequired,
  className: PropTypes.string,
};

export default AssignmentStatusBadge;
