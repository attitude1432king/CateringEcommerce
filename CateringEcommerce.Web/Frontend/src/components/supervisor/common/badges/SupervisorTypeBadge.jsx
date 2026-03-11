/**
 * SupervisorTypeBadge Component
 * Displays supervisor type (CAREER vs REGISTERED)
 * For Event Supervisor Portal, always shows REGISTERED
 */

import PropTypes from 'prop-types';
import { SupervisorType } from '../../../../utils/supervisor/supervisorEnums';

const SupervisorTypeBadge = ({ type, className = '' }) => {
  const config = {
    [SupervisorType.CAREER]: {
      label: 'Core Supervisor',
      color: 'bg-blue-100 text-blue-800 border-blue-200',
    },
    [SupervisorType.REGISTERED]: {
      label: 'Registered Supervisor',
      color: 'bg-green-100 text-green-800 border-green-200',
    },
  };

  const { label, color } = config[type] || config[SupervisorType.REGISTERED];

  return (
    <span
      className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border ${color} ${className}`}
    >
      {label}
    </span>
  );
};

SupervisorTypeBadge.propTypes = {
  type: PropTypes.string.isRequired,
  className: PropTypes.string,
};

export default SupervisorTypeBadge;
