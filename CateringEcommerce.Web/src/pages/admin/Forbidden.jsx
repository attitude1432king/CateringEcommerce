import { Shield, ArrowLeft, Home } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { useAdminAuth } from '../../contexts/AdminAuthContext';

/**
 * Forbidden (403) Page
 *
 * Displayed when a user tries to access a resource they don't have permission for.
 * This page is shown by ProtectedRoute when permission checks fail.
 */
const Forbidden = () => {
  const navigate = useNavigate();
  const { admin } = useAdminAuth();

  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center px-4">
      <div className="max-w-md w-full text-center">
        {/* Icon */}
        <div className="flex justify-center mb-6">
          <div className="w-20 h-20 bg-red-100 rounded-full flex items-center justify-center animate-pulse">
            <Shield className="w-10 h-10 text-red-600" />
          </div>
        </div>

        {/* Error Code */}
        <h1 className="text-6xl font-bold text-gray-900 mb-2">403</h1>

        {/* Title */}
        <h2 className="text-2xl font-semibold text-gray-700 mb-4">
          Access Denied
        </h2>

        {/* Message */}
        <p className="text-gray-600 mb-2">
          You don't have permission to access this resource.
        </p>
        <p className="text-sm text-gray-500 mb-8">
          {admin ? (
            <>Your current role: <span className="font-medium">{admin.role}</span></>
          ) : (
            'Please contact your administrator if you believe this is an error.'
          )}
        </p>

        {/* Permission Info Box */}
        <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-8 text-left">
          <h3 className="text-sm font-semibold text-blue-900 mb-2">
            What can I do?
          </h3>
          <ul className="text-sm text-blue-800 space-y-1">
            <li>• Return to the dashboard to access available features</li>
            <li>• Contact your system administrator for access</li>
            <li>• Check if you're logged in with the correct account</li>
          </ul>
        </div>

        {/* Action Buttons */}
        <div className="flex flex-col sm:flex-row items-center justify-center space-y-3 sm:space-y-0 sm:space-x-4">
          <button
            onClick={() => navigate(-1)}
            className="inline-flex items-center space-x-2 px-6 py-3 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-100 transition-colors w-full sm:w-auto justify-center"
          >
            <ArrowLeft className="w-4 h-4" />
            <span>Go Back</span>
          </button>

          <button
            onClick={() => navigate('/admin/dashboard')}
            className="inline-flex items-center space-x-2 px-6 py-3 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors w-full sm:w-auto justify-center"
          >
            <Home className="w-4 h-4" />
            <span>Go to Dashboard</span>
          </button>
        </div>

        {/* Help Text */}
        <p className="text-xs text-gray-400 mt-8">
          Error Code: 403 - Insufficient Permissions
        </p>
      </div>
    </div>
  );
};

export default Forbidden;
