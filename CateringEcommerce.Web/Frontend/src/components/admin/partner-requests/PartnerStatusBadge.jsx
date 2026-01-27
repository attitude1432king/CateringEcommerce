import { Clock, CheckCircle, XCircle, AlertCircle, Eye, FileQuestion } from 'lucide-react';

/**
 * Partner Request Status Badge Component
 *
 * Displays a colored badge with icon for different partner request statuses
 */
const PartnerStatusBadge = ({ status, size = 'md', showIcon = true }) => {
  const statusConfig = {
    PENDING: {
      label: 'Pending Review',
      color: 'orange',
      bgColor: 'bg-orange-100',
      textColor: 'text-orange-800',
      borderColor: 'border-orange-200',
      icon: Clock
    },
    UNDER_REVIEW: {
      label: 'Under Review',
      color: 'blue',
      bgColor: 'bg-blue-100',
      textColor: 'text-blue-800',
      borderColor: 'border-blue-200',
      icon: Eye
    },
    INFO_REQUESTED: {
      label: 'Info Requested',
      color: 'purple',
      bgColor: 'bg-purple-100',
      textColor: 'text-purple-800',
      borderColor: 'border-purple-200',
      icon: FileQuestion
    },
    APPROVED: {
      label: 'Approved',
      color: 'green',
      bgColor: 'bg-green-100',
      textColor: 'text-green-800',
      borderColor: 'border-green-200',
      icon: CheckCircle
    },
    REJECTED: {
      label: 'Rejected',
      color: 'red',
      bgColor: 'bg-red-100',
      textColor: 'text-red-800',
      borderColor: 'border-red-200',
      icon: XCircle
    },
    INCOMPLETE: {
      label: 'Incomplete',
      color: 'gray',
      bgColor: 'bg-gray-100',
      textColor: 'text-gray-800',
      borderColor: 'border-gray-200',
      icon: AlertCircle
    }
  };

  const config = statusConfig[status] || statusConfig.PENDING;
  const Icon = config.icon;

  const sizeClasses = {
    sm: 'text-xs px-2 py-0.5',
    md: 'text-sm px-2.5 py-1',
    lg: 'text-base px-3 py-1.5'
  };

  const iconSizes = {
    sm: 'w-3 h-3',
    md: 'w-4 h-4',
    lg: 'w-5 h-5'
  };

  return (
    <span
      className={`
        inline-flex items-center rounded-full border font-medium
        ${config.bgColor} ${config.textColor} ${config.borderColor}
        ${sizeClasses[size]}
      `}
    >
      {showIcon && <Icon className={`${iconSizes[size]} mr-1.5`} />}
      <span>{config.label}</span>
    </span>
  );
};

export default PartnerStatusBadge;
