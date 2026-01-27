import { Inbox } from 'lucide-react';

const EmptyState = ({ icon: Icon = Inbox, title, description, action }) => {
  return (
    <div className="text-center py-12 bg-white rounded-lg border border-gray-200">
      <div className="inline-flex items-center justify-center w-16 h-16 bg-gray-100 rounded-full mb-4">
        <Icon className="w-8 h-8 text-gray-400" />
      </div>
      <h3 className="text-lg font-medium text-gray-900 mb-2">{title}</h3>
      {description && (
        <p className="text-sm text-gray-500 mb-6 max-w-md mx-auto">
          {description}
        </p>
      )}
      {action && <div>{action}</div>}
    </div>
  );
};

export default EmptyState;
