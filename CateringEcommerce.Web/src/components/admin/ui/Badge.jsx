import { statusConfig } from '../../../config/adminTheme';

const Badge = ({ status, children, dot = false, className = '' }) => {
  const config = statusConfig[status?.toLowerCase()] || statusConfig.inactive;

  return (
    <span
      className={`
        inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium
        ${config.color} ${config.bg} border ${config.border}
        ${className}
      `}
    >
      {dot && (
        <span className={`w-1.5 h-1.5 rounded-full mr-1.5 ${config.dot}`} />
      )}
      {children || status}
    </span>
  );
};

export default Badge;
