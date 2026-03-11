import { Clock, CheckCircle, XCircle, AlertCircle, Eye, FileQuestion } from 'lucide-react';
import { ApprovalStatus } from '../../../services/partnerApprovalApi';

/**
 * Partner Request Status Badge Component (UPDATED - Enum-based)
 *
 * Displays a colored badge with icon for different partner request statuses
 * Now works with INT enum IDs instead of string statuses
 *
 * Usage:
 *   <PartnerStatusBadge statusId={1} statusName="Pending" />
 *   <PartnerStatusBadge statusId={ApprovalStatus.APPROVED} statusName="Approved" />
 */
const PartnerStatusBadge = ({ statusId, statusName, size = 'md', showIcon = true }) => {
    // Map enum IDs to visual config
    const statusConfigById = {
        [ApprovalStatus.PENDING]: {
            color: 'orange',
            bgColor: 'bg-orange-100',
            textColor: 'text-orange-800',
            borderColor: 'border-orange-200',
            icon: Clock,
            defaultLabel: 'Pending'
        },
        [ApprovalStatus.APPROVED]: {
            color: 'green',
            bgColor: 'bg-green-100',
            textColor: 'text-green-800',
            borderColor: 'border-green-200',
            icon: CheckCircle,
            defaultLabel: 'Approved'
        },
        [ApprovalStatus.REJECTED]: {
            color: 'red',
            bgColor: 'bg-red-100',
            textColor: 'text-red-800',
            borderColor: 'border-red-200',
            icon: XCircle,
            defaultLabel: 'Rejected'
        },
        [ApprovalStatus.UNDER_REVIEW]: {
            color: 'blue',
            bgColor: 'bg-blue-100',
            textColor: 'text-blue-800',
            borderColor: 'border-blue-200',
            icon: Eye,
            defaultLabel: 'Under Review'
        },
        [ApprovalStatus.INFO_REQUESTED]: {
            color: 'purple',
            bgColor: 'bg-purple-100',
            textColor: 'text-purple-800',
            borderColor: 'border-purple-200',
            icon: FileQuestion,
            defaultLabel: 'Info Requested'
        }
    };

    // Get config for this status ID (default to PENDING if unknown)
    const config = statusConfigById[statusId] || statusConfigById[ApprovalStatus.PENDING];
    const Icon = config.icon;

    // Use statusName from API if available, otherwise use default label
    const displayLabel = statusName || config.defaultLabel;

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
            <span>{displayLabel}</span>
        </span>
    );
};

export default PartnerStatusBadge;
