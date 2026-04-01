import React from 'react';
import PropTypes from 'prop-types';

/**
 * RoleBadge Component
 * Displays a role name with dynamic background color from database
 */
const RoleBadge = ({ roleName, roleColor = '#6366f1', size = 'md', className = '' }) => {
  const sizeClasses = {
    sm: 'px-2 py-0.5 text-xs',
    md: 'px-2.5 py-0.5 text-xs',
    lg: 'px-3 py-1 text-sm',
  };

  const selectedSize = sizeClasses[size] || sizeClasses.md;

  // Create lighter background from the role color
  const backgroundColor = `${roleColor}20`; // 20% opacity

  return (
    <span
      className={`inline-flex items-center rounded-full font-medium ${selectedSize} ${className}`}
      style={{
        backgroundColor: backgroundColor,
        color: roleColor,
      }}
    >
      {roleName}
    </span>
  );
};

RoleBadge.propTypes = {
  roleName: PropTypes.string.isRequired,
  roleColor: PropTypes.string,
  size: PropTypes.oneOf(['sm', 'md', 'lg']),
  className: PropTypes.string,
};

export default RoleBadge;
