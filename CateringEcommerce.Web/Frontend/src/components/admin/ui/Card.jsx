const Card = ({ children, className = '', padding = true, hover = false }) => {
  return (
    <div
      className={`
        bg-white rounded-lg border border-gray-200 shadow-sm
        ${padding ? 'p-6' : ''}
        ${hover ? 'hover:shadow-md transition-shadow duration-200' : ''}
        ${className}
      `}
    >
      {children}
    </div>
  );
};

export const CardHeader = ({ children, className = '' }) => {
  return (
    <div className={`pb-4 border-b border-gray-200 ${className}`}>
      {children}
    </div>
  );
};

export const CardTitle = ({ children, className = '' }) => {
  return (
    <h3 className={`text-lg font-semibold text-gray-900 ${className}`}>
      {children}
    </h3>
  );
};

export const CardDescription = ({ children, className = '' }) => {
  return (
    <p className={`text-sm text-gray-500 mt-1 ${className}`}>
      {children}
    </p>
  );
};

export const CardBody = ({ children, className = '' }) => {
  return <div className={`pt-4 ${className}`}>{children}</div>;
};

export default Card;
