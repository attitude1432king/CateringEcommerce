import { ChevronLeft, ChevronRight, ChevronsLeft, ChevronsRight } from 'lucide-react';

const Table = ({ children, className = '' }) => {
  return (
    <div className="overflow-x-auto">
      <table className={`min-w-full divide-y divide-gray-200 ${className}`}>
        {children}
      </table>
    </div>
  );
};

export const TableHeader = ({ children }) => {
  return (
    <thead className="bg-gray-50">
      <tr>{children}</tr>
    </thead>
  );
};

export const TableHeaderCell = ({ children, className = '' }) => {
  return (
    <th
      className={`
        px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider
        ${className}
      `}
    >
      {children}
    </th>
  );
};

export const TableBody = ({ children }) => {
  return <tbody className="bg-white divide-y divide-gray-200">{children}</tbody>;
};

export const TableRow = ({ children, onClick, className = '' }) => {
  return (
    <tr
      onClick={onClick}
      className={`
        ${onClick ? 'cursor-pointer hover:bg-gray-50' : ''}
        transition-colors
        ${className}
      `}
    >
      {children}
    </tr>
  );
};

export const TableCell = ({ children, className = '' }) => {
  return (
    <td className={`px-6 py-4 whitespace-nowrap text-sm text-gray-900 ${className}`}>
      {children}
    </td>
  );
};

// Pagination Component
export const TablePagination = ({
  currentPage,
  totalPages,
  onPageChange,
  pageSize,
  totalRecords,
}) => {
  const start = (currentPage - 1) * pageSize + 1;
  const end = Math.min(currentPage * pageSize, totalRecords);

  return (
    <div className="flex items-center justify-between px-6 py-3 bg-white border-t border-gray-200">
      <div className="text-sm text-gray-700">
        Showing <span className="font-medium">{start}</span> to{' '}
        <span className="font-medium">{end}</span> of{' '}
        <span className="font-medium">{totalRecords}</span> results
      </div>

      <div className="flex items-center space-x-2">
        <button
          onClick={() => onPageChange(1)}
          disabled={currentPage === 1}
          className="p-2 rounded-md hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          <ChevronsLeft className="w-4 h-4" />
        </button>
        <button
          onClick={() => onPageChange(currentPage - 1)}
          disabled={currentPage === 1}
          className="p-2 rounded-md hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          <ChevronLeft className="w-4 h-4" />
        </button>

        <span className="px-4 py-2 text-sm font-medium text-gray-700">
          Page {currentPage} of {totalPages}
        </span>

        <button
          onClick={() => onPageChange(currentPage + 1)}
          disabled={currentPage === totalPages}
          className="p-2 rounded-md hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          <ChevronRight className="w-4 h-4" />
        </button>
        <button
          onClick={() => onPageChange(totalPages)}
          disabled={currentPage === totalPages}
          className="p-2 rounded-md hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          <ChevronsRight className="w-4 h-4" />
        </button>
      </div>
    </div>
  );
};

export default Table;
