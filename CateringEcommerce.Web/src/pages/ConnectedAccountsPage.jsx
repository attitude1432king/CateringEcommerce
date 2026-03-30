import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { getConnectedAccounts, unlinkOAuthAccount, setPrimaryConnection } from '../services/oauthApi';
import { Link2, Unlink, Star, AlertCircle, ArrowLeft, Shield } from 'lucide-react';

const ConnectedAccountsPage = () => {
  const navigate = useNavigate();
  const [accounts, setAccounts] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);
  const [successMessage, setSuccessMessage] = useState(null);

  // Unlink confirmation modal state
  const [unlinkConfirmation, setUnlinkConfirmation] = useState({
    isOpen: false,
    oauthId: null,
    provider: ''
  });

  useEffect(() => {
    fetchConnectedAccounts();
  }, []);

  const fetchConnectedAccounts = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await getConnectedAccounts();
      if (response.success) {
        setAccounts(response.data || []);
      } else {
        setError(response.message || 'Failed to load connected accounts');
      }
    } catch (error) {
      setError('An error occurred while loading your connected accounts');
    } finally {
      setIsLoading(false);
    }
  };

  const handleUnlink = (oauthId, provider) => {
    setUnlinkConfirmation({
      isOpen: true,
      oauthId,
      provider
    });
  };

  const confirmUnlink = async () => {
    const { oauthId, provider } = unlinkConfirmation;
    setUnlinkConfirmation({ isOpen: false, oauthId: null, provider: '' });

    try {
      const response = await unlinkOAuthAccount(oauthId);
      if (response.success) {
        setSuccessMessage(`${provider} account unlinked successfully`);
        await fetchConnectedAccounts();

        // Clear success message after 3 seconds
        setTimeout(() => setSuccessMessage(null), 3000);
      } else {
        alert(response.message || 'Failed to unlink account');
      }
    } catch (error) {
      alert(error.message || 'Failed to unlink account');
    }
  };

  const handleSetPrimary = async (oauthId, provider) => {
    try {
      const response = await setPrimaryConnection(oauthId);
      if (response.success) {
        setSuccessMessage(`${provider} set as primary login method`);
        await fetchConnectedAccounts();

        // Clear success message after 3 seconds
        setTimeout(() => setSuccessMessage(null), 3000);
      } else {
        alert(response.message || 'Failed to set primary connection');
      }
    } catch (error) {
      alert(error.message || 'Failed to set primary connection');
    }
  };

  const getProviderIcon = (provider) => {
    if (provider?.toLowerCase() === 'google') {
      return (
        <svg className="w-8 h-8" viewBox="0 0 24 24">
          <path fill="#4285F4" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"/>
          <path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"/>
          <path fill="#FBBC05" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"/>
          <path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"/>
        </svg>
      );

    }
    return <Link2 className="w-8 h-8 text-gray-400" />;
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-100 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500 mx-auto"></div>
          <p className="mt-4 text-gray-600">Loading connected accounts...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-100 py-8">
      <div className="max-w-4xl mx-auto px-4">
        {/* Back Button */}
        <button
          onClick={() => navigate('/profile')}
          className="mb-4 flex items-center gap-2 text-blue-600 hover:text-blue-700 font-medium"
        >
          <ArrowLeft className="w-5 h-5" />
          Back to Profile
        </button>

        {/* Page Header */}
        <div className="mb-6">
          <h1 className="text-3xl font-bold text-gray-900 mb-2">Connected Accounts</h1>
          <p className="text-gray-600">
            Manage your OAuth login connections. Connect or disconnect social accounts for quick and secure login.
          </p>
        </div>

        {/* Success Message */}
        {successMessage && (
          <div className="mb-6 bg-green-50 border-2 border-green-300 text-green-800 px-6 py-4 rounded-lg flex items-center gap-3">
            <Shield className="w-6 h-6 flex-shrink-0" />
            <p className="font-semibold">{successMessage}</p>
          </div>
        )}

        {/* Error Message */}
        {error && (
          <div className="bg-red-50 border border-red-200 text-red-800 px-4 py-3 rounded-lg mb-6">
            {error}
          </div>
        )}

        {/* Security Notice */}
        <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-6">
          <div className="flex items-start gap-3">
            <Shield className="w-5 h-5 text-blue-700 flex-shrink-0 mt-0.5" />
            <div className="text-sm text-blue-900">
              <p className="font-medium mb-1">Security Notice</p>
              <p>
                Connected accounts allow you to sign in without a password. You must keep at least one login method
                (OAuth account or password) to access your account.
              </p>
            </div>
          </div>
        </div>

        {accounts.length === 0 ? (
          <div className="bg-white rounded-lg p-12 text-center shadow-sm">
            <AlertCircle className="w-16 h-16 mx-auto text-gray-300 mb-4" />
            <h2 className="text-xl font-semibold mb-2">No Connected Accounts</h2>
            <p className="text-gray-600 mb-6">
              Connect your Google account for quick and secure login
            </p>
            <p className="text-sm text-gray-500">
              You can connect your Google account by logging in with Google from the login page
            </p>
          </div>
        ) : (
          <div className="space-y-4">
            {accounts.map((account) => (
              <div
                key={account.oauthId}
                className="bg-white rounded-lg p-6 shadow-sm hover:shadow-md transition-shadow border border-gray-200"
              >
                <div className="flex items-start justify-between">
                  <div className="flex items-start gap-4 flex-1">
                    {/* Provider Icon */}
                    <div className="flex-shrink-0">
                      {getProviderIcon(account.provider)}
                    </div>

                    {/* Account Info */}
                    <div className="flex-1">
                      <div className="flex items-center gap-2 mb-1">
                        <h3 className="text-lg font-semibold capitalize">
                          {account.provider?.toLowerCase()}
                        </h3>
                        {account.isPrimary && (
                          <span className="flex items-center gap-1 px-2 py-0.5 bg-blue-100 text-blue-700 text-xs font-medium rounded-full">
                            <Star className="w-3 h-3 fill-current" />
                            Primary
                          </span>
                        )}
                      </div>

                      <p className="text-gray-700 mb-1">{account.providerEmail || account.providerName}</p>

                      <div className="flex flex-col text-xs text-gray-500 gap-1">
                        <p>
                          <strong>Connected:</strong>{' '}
                          {new Date(account.linkedDate).toLocaleDateString('en-IN', {
                            day: 'numeric',
                            month: 'long',
                            year: 'numeric'
                          })}
                        </p>
                        {account.lastLogin && (
                          <p>
                            <strong>Last login:</strong>{' '}
                            {new Date(account.lastLogin).toLocaleDateString('en-IN', {
                              day: 'numeric',
                              month: 'short',
                              year: 'numeric'
                            })}
                          </p>
                        )}
                      </div>
                    </div>
                  </div>

                  {/* Action Buttons */}
                  <div className="flex flex-col gap-2 ml-4">
                    {!account.isPrimary && (
                      <button
                        onClick={() => handleSetPrimary(account.oauthId, account.provider)}
                        className="px-4 py-2 text-sm border-2 border-blue-500 text-blue-600 rounded-lg hover:bg-blue-50 transition-colors font-medium whitespace-nowrap"
                      >
                        Set as Primary
                      </button>
                    )}

                    {account.canUnlink ? (
                      <button
                        onClick={() => handleUnlink(account.oauthId, account.provider)}
                        className="px-4 py-2 text-sm bg-red-500 text-white rounded-lg hover:bg-red-600 transition-colors flex items-center gap-2 font-medium justify-center"
                      >
                        <Unlink className="w-4 h-4" />
                        Unlink
                      </button>
                    ) : (
                      <div className="text-xs text-center text-gray-500 px-4 py-2 border border-gray-300 rounded-lg bg-gray-50">
                        Cannot unlink<br />
                        <span className="text-[10px]">(Only login method)</span>
                      </div>
                    )}
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}

        {/* Info Section */}
        <div className="mt-8 bg-gray-50 rounded-lg p-6">
          <h3 className="font-semibold text-gray-900 mb-3">About Connected Accounts</h3>
          <div className="space-y-2 text-sm text-gray-700">
            <p>
              <strong>Primary Account:</strong> Your primary OAuth account is the preferred method for quick login.
            </p>
            <p>
              <strong>Unlinking:</strong> You can unlink an account only if you have at least one other login method
              (another OAuth account or a password set).
            </p>
            <p>
              <strong>Privacy:</strong> We store minimal information from your OAuth provider (email, name, profile picture).
              We never access your password or post on your behalf.
            </p>
          </div>
        </div>
      </div>

      {/* Unlink Confirmation Modal */}
      {unlinkConfirmation.isOpen && (
        <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4">
          <div className="bg-white rounded-xl shadow-2xl max-w-md w-full p-6 animate-fade-in">
            <div className="text-center mb-6">
              <div className="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-orange-100 mb-4">
                <Unlink className="h-6 w-6 text-orange-600" />
              </div>
              <h3 className="text-lg font-semibold text-gray-900 mb-2">Unlink Account?</h3>
              <p className="text-sm text-gray-600">
                Are you sure you want to unlink your {unlinkConfirmation.provider} account? You will need to use another login method to access your account.
              </p>
            </div>

            <div className="flex gap-3">
              <button
                onClick={() => setUnlinkConfirmation({ isOpen: false, oauthId: null, provider: '' })}
                className="flex-1 px-4 py-2.5 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-colors font-medium"
              >
                Cancel
              </button>
              <button
                onClick={confirmUnlink}
                className="flex-1 px-4 py-2.5 bg-orange-600 text-white rounded-lg hover:bg-orange-700 transition-colors font-medium"
              >
                Unlink Account
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default ConnectedAccountsPage;
