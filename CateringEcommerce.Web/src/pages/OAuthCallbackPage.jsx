import React, { useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL.replace(/\/$/, '');

const OAuthCallbackPage = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const { login } = useAuth();
  const [status, setStatus] = useState('processing');
  const [message, setMessage] = useState('Processing login...');

  useEffect(() => {
    handleCallback();
  }, []);

  const handleCallback = async () => {
    const code = searchParams.get('code');
    const state = searchParams.get('state');
    const error = searchParams.get('error');
    const provider = localStorage.getItem('oauth_provider') || 'google';

    if (error) {
      setStatus('error');
      setMessage(`Authentication failed: ${error}`);
      setTimeout(() => navigate('/'), 3000);
      return;
    }

    if (!code || !state) {
      setStatus('error');
      setMessage('Missing authentication parameters');
      setTimeout(() => navigate('/'), 3000);
      return;
    }

    try {
      // Call backend OAuth callback endpoint
      const response = await fetch(
        `${API_BASE_URL}/api/oauth/${provider}/callback?code=${encodeURIComponent(code)}&state=${encodeURIComponent(state)}`,
        {
          method: 'GET',
          headers: {
            'Content-Type': 'application/json'
          },
          credentials: 'include' // CRITICAL: Required to set httpOnly cookie
        }
      );

      const data = await response.json();

      if (data.success && data.data) {
        // Token is set as httpOnly cookie by backend — only store profile data
        localStorage.setItem('user', JSON.stringify({
          userId: data.data.userId,
          email: data.data.email,
          name: data.data.name,
          picture: data.data.picture,
          role: 'User'
        }));

        // Update auth context
        if (login) {
          login({
            userId: data.data.userId,
            email: data.data.email,
            name: data.data.name,
            picture: data.data.picture,
            token: data.data.token
          });
        }

        setStatus('success');
        setMessage(data.message || 'Login successful!');

        // Clean up
        localStorage.removeItem('oauth_provider');

        // Redirect to home or intended page
        setTimeout(() => {
          const redirectUrl = localStorage.getItem('oauth_redirect') || '/';
          localStorage.removeItem('oauth_redirect');
          navigate(redirectUrl);
        }, 1500);
      } else {
        throw new Error(data.message || 'Authentication failed');
      }
    } catch (error) {
      console.error('OAuth callback error:', error);
      setStatus('error');
      setMessage(error.message || 'Authentication failed. Please try again.');
      setTimeout(() => navigate('/'), 3000);
    }
  };

  return (
    <div className="min-h-screen bg-gray-100 flex items-center justify-center p-4">
      <div className="bg-white rounded-lg shadow-lg p-8 max-w-md w-full text-center">
        {status === 'processing' && (
          <>
            <div className="animate-spin rounded-full h-16 w-16 border-b-2 border-blue-500 mx-auto mb-4"></div>
            <h2 className="text-xl font-semibold text-gray-800 mb-2">Processing Login</h2>
            <p className="text-gray-600">{message}</p>
            <p className="text-sm text-gray-500 mt-4">Please wait while we authenticate you...</p>
          </>
        )}

        {status === 'success' && (
          <>
            <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
              <svg className="w-8 h-8 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M5 13l4 4L19 7" />
              </svg>
            </div>
            <h2 className="text-xl font-semibold text-gray-800 mb-2">Success!</h2>
            <p className="text-gray-600">{message}</p>
            <p className="text-sm text-gray-500 mt-4">Redirecting you to the app...</p>
          </>
        )}

        {status === 'error' && (
          <>
            <div className="w-16 h-16 bg-red-100 rounded-full flex items-center justify-center mx-auto mb-4">
              <svg className="w-8 h-8 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </div>
            <h2 className="text-xl font-semibold text-gray-800 mb-2">Authentication Failed</h2>
            <p className="text-gray-600 mb-4">{message}</p>
            <button
              onClick={() => navigate('/')}
              className="px-6 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors"
            >
              Go to Home
            </button>
          </>
        )}
      </div>
    </div>
  );
};

export default OAuthCallbackPage;
