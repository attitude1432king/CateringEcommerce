/**
 * AuthorityLevelBadge Component
 * Displays supervisor authority level with icon
 */

import PropTypes from 'prop-types';
import { Shield, ShieldCheck, ShieldAlert, Lock } from 'lucide-react';
import { getAuthorityLevelLabel, AuthorityLevel } from '../../../../utils/supervisor/supervisorEnums';

const AuthorityLevelBadge = ({ level, showIcon = true, className = '' }) => {
  const label = getAuthorityLevelLabel(level);

  const config = {
    [AuthorityLevel.BASIC]: {
      color: 'bg-blue-100 text-blue-800 border-blue-200',
      icon: Shield,
    },
    [AuthorityLevel.INTERMEDIATE]: {
      color: 'bg-cyan-100 text-cyan-800 border-cyan-200',
      icon: ShieldAlert,
    },
    [AuthorityLevel.ADVANCED]: {
      color: 'bg-purple-100 text-purple-800 border-purple-200',
      icon: ShieldCheck,
    },
    [AuthorityLevel.FULL]: {
      color: 'bg-amber-100 text-amber-800 border-amber-200',
      icon: Lock,
    },
  };

  const { color, icon: Icon } = config[level] || config[AuthorityLevel.BASIC];

  return (
    <span
      className={`inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-medium border ${color} ${className}`}
    >
      {showIcon && <Icon className="w-3 h-3" />}
      {label}
    </span>
  );
};

AuthorityLevelBadge.propTypes = {
  level: PropTypes.string.isRequired,
  showIcon: PropTypes.bool,
  className: PropTypes.string,
};

export default AuthorityLevelBadge;
