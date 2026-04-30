import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL.replace(/\/$/, '');

/**
 * Order Modification Page
 * Allows users to view and manage modification requests for their orders
 */
const OrderModificationPage = () => {
  const { orderId } = useParams();
  const navigate = useNavigate();
  const { user, token } = useAuth();

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [modifications, setModifications] = useState(null);
  const [processingId, setProcessingId] = useState(null);

  useEffect(() => {
    if (!user) {
      navigate('/');
      return;
    }
    fetchModifications();
  }, [orderId, user]);

  const fetchModifications = async () => {
    try {
      setLoading(true);
      setError(null);

      const response = await fetch(`${API_BASE_URL}/api/User/OrderModifications/${orderId}`, {
        method: 'GET',
        credentials: 'include',
        headers: {
          'Content-Type': 'application/json',
          ...(token && { 'Authorization': `Bearer ${token}` })
        }
      });

      const data = await response.json();

      if (data.success) {
        setModifications(data.data);
      } else {
        setError(data.message || 'Failed to load modifications');
      }
    } catch (err) {
      console.error('Error fetching modifications:', err);
      setError('Unable to load modifications. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const handleApprove = async (modificationId) => {
    try {
      setProcessingId(modificationId);

      const response = await fetch(`${API_BASE_URL}/api/User/OrderModifications/${modificationId}/Approve`, {
        method: 'POST',
        credentials: 'include',
        headers: {
          'Content-Type': 'application/json',
          ...(token && { 'Authorization': `Bearer ${token}` })
        },
        body: JSON.stringify({ notes: '' })
      });

      const data = await response.json();

      if (data.success) {
        // Refresh modifications list
        await fetchModifications();
      } else {
        alert(data.message || 'Failed to approve modification');
      }
    } catch (err) {
      console.error('Error approving modification:', err);
      alert('An error occurred. Please try again.');
    } finally {
      setProcessingId(null);
    }
  };

  const handleReject = async (modificationId) => {
    const reason = prompt('Please provide a reason for rejection:');
    if (!reason) return;

    try {
      setProcessingId(modificationId);

      const response = await fetch(`${API_BASE_URL}/api/User/OrderModifications/${modificationId}/Reject`, {
        method: 'POST',
        credentials: 'include',
        headers: {
          'Content-Type': 'application/json',
          ...(token && { 'Authorization': `Bearer ${token}` })
        },
        body: JSON.stringify({ rejectionReason: reason })
      });

      const data = await response.json();

      if (data.success) {
        // Refresh modifications list
        await fetchModifications();
      } else {
        alert(data.message || 'Failed to reject modification');
      }
    } catch (err) {
      console.error('Error rejecting modification:', err);
      alert('An error occurred. Please try again.');
    } finally {
      setProcessingId(null);
    }
  };

  const getStatusColor = (status) => {
    switch (status?.toLowerCase()) {
      case 'pending': return 'bg-yellow-100 text-yellow-800 border-yellow-300';
      case 'approved': return 'bg-green-100 text-green-800 border-green-300';
      case 'rejected': return 'bg-red-100 text-red-800 border-red-300';
      case 'completed': return 'bg-blue-100 text-blue-800 border-blue-300';
      default: return 'bg-gray-100 text-gray-800 border-gray-300';
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-neutral-50 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-rose-600 mx-auto mb-4"></div>
          <p className="text-neutral-600">Loading modifications...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-neutral-50 p-6">
        <div className="max-w-4xl mx-auto">
          <div className="bg-red-50 border border-red-200 rounded-lg p-6 text-center">
            <svg className="h-12 w-12 text-red-500 mx-auto mb-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <h2 className="text-xl font-semibold text-red-800 mb-2">Error</h2>
            <p className="text-red-600 mb-4">{error}</p>
            <button
              onClick={() => navigate('/my-orders')}
              className="px-6 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition"
            >
              Back to Orders
            </button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-neutral-50 py-8">
      <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <div className="mb-6">
          <button
            onClick={() => navigate(`/orders/${orderId}`)}
            className="inline-flex items-center gap-2 text-neutral-600 hover:text-rose-600 transition mb-4"
          >
            <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 19l-7-7m0 0l7-7m-7 7h18" />
            </svg>
            Back to Order Details
          </button>
          <h1 className="text-3xl font-bold text-neutral-800">Order Modifications</h1>
          <p className="text-neutral-600 mt-2">Order ID: {orderId}</p>
        </div>

        {/* Modifications List */}
        {modifications && modifications.modifications && modifications.modifications.length > 0 ? (
          <div className="space-y-4">
            {modifications.modifications.map((mod) => (
              <div key={mod.modificationId} className="bg-white rounded-lg shadow-sm border border-neutral-200 overflow-hidden">
                <div className="p-6">
                  {/* Header */}
                  <div className="flex justify-between items-start mb-4">
                    <div>
                      <h3 className="text-lg font-semibold text-neutral-800 capitalize">
                        {mod.modificationType?.replace(/([A-Z])/g, ' $1').trim()}
                      </h3>
                      <p className="text-sm text-neutral-500">
                        Requested on {new Date(mod.requestedAt).toLocaleDateString()}
                      </p>
                    </div>
                    <span className={`px-3 py-1 rounded-full text-xs font-medium border ${getStatusColor(mod.status)}`}>
                      {mod.status}
                    </span>
                  </div>

                  {/* Details */}
                  <div className="bg-neutral-50 rounded-lg p-4 mb-4">
                    <h4 className="text-sm font-semibold text-neutral-700 mb-2">Details</h4>
                    <p className="text-neutral-600">{mod.details || 'No details provided'}</p>

                    {mod.oldValue && (
                      <div className="mt-2">
                        <span className="text-sm text-neutral-500">Old Value: </span>
                        <span className="text-sm text-neutral-800 line-through">{mod.oldValue}</span>
                      </div>
                    )}

                    {mod.newValue && (
                      <div className="mt-1">
                        <span className="text-sm text-neutral-500">New Value: </span>
                        <span className="text-sm font-semibold text-rose-600">{mod.newValue}</span>
                      </div>
                    )}

                    {mod.additionalCost > 0 && (
                      <div className="mt-2">
                        <span className="text-sm text-neutral-500">Additional Cost: </span>
                        <span className="text-sm font-semibold text-green-600">₹{mod.additionalCost.toFixed(2)}</span>
                      </div>
                    )}
                  </div>

                  {/* Actions for Pending modifications */}
                  {mod.status === 'Pending' && (
                    <div className="flex gap-3">
                      <button
                        onClick={() => handleApprove(mod.modificationId)}
                        disabled={processingId === mod.modificationId}
                        className="flex-1 px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition disabled:opacity-50 disabled:cursor-not-allowed"
                      >
                        {processingId === mod.modificationId ? 'Processing...' : 'Approve'}
                      </button>
                      <button
                        onClick={() => handleReject(mod.modificationId)}
                        disabled={processingId === mod.modificationId}
                        className="flex-1 px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition disabled:opacity-50 disabled:cursor-not-allowed"
                      >
                        Reject
                      </button>
                    </div>
                  )}

                  {/* Rejection reason */}
                  {mod.rejectionReason && (
                    <div className="mt-4 p-3 bg-red-50 border border-red-200 rounded-lg">
                      <p className="text-sm font-semibold text-red-800">Rejection Reason:</p>
                      <p className="text-sm text-red-700">{mod.rejectionReason}</p>
                    </div>
                  )}
                </div>
              </div>
            ))}
          </div>
        ) : (
          <div className="bg-white rounded-lg shadow-sm border border-neutral-200 p-12 text-center">
            <svg className="h-16 w-16 text-neutral-400 mx-auto mb-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
            <h3 className="text-lg font-semibold text-neutral-800 mb-2">No Modifications</h3>
            <p className="text-neutral-600">There are no modification requests for this order.</p>
          </div>
        )}
      </div>
    </div>
  );
};

export default OrderModificationPage;
