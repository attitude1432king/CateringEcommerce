/**
 * ExtraQuantityRequest Component
 * Request additional food quantities with client OTP approval
 */

import { useState } from 'react';
import PropTypes from 'prop-types';
import { Package, AlertTriangle, Clock, CheckCircle2, XCircle } from 'lucide-react';
import { eventSupervisionApi } from '../../../../services/api/supervisor';
import toast from 'react-hot-toast';

const ExtraQuantityRequest = ({ assignmentId, onRequestSent }) => {
  const [items, setItems] = useState([{ itemName: '', quantity: 1, reason: '' }]);
  const [urgencyLevel, setUrgencyLevel] = useState('NORMAL');
  const [additionalNotes, setAdditionalNotes] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [requestHistory, setRequestHistory] = useState([]);

  const handleAddItem = () => {
    setItems((prev) => [...prev, { itemName: '', quantity: 1, reason: '' }]);
  };

  const handleRemoveItem = (index) => {
    if (items.length > 1) {
      setItems((prev) => prev.filter((_, i) => i !== index));
    }
  };

  const handleItemChange = (index, field, value) => {
    setItems((prev) =>
      prev.map((item, i) => (i === index ? { ...item, [field]: value } : item))
    );
  };

  const handleSubmitRequest = async () => {
    const validItems = items.filter((item) => item.itemName.trim());
    if (validItems.length === 0) {
      toast.error('Please add at least one item');
      return;
    }

    setSubmitting(true);
    try {
      const response = await eventSupervisionApi.requestExtraQuantity({
        assignmentId,
        items: validItems,
        urgencyLevel,
        notes: additionalNotes,
        requestedAt: new Date().toISOString(),
      });

      if (response.success) {
        const requestData = response.data?.data;
        setRequestHistory((prev) => [
          ...prev,
          {
            id: requestData?.requestId || Date.now(),
            items: validItems,
            urgencyLevel,
            status: requestData?.requiresOTP ? 'AWAITING_OTP' : 'SUBMITTED',
            requestedAt: new Date().toISOString(),
          },
        ]);
        setItems([{ itemName: '', quantity: 1, reason: '' }]);
        setAdditionalNotes('');
        toast.success(
          requestData?.requiresOTP
            ? 'Request sent. Client OTP verification required.'
            : 'Extra quantity request submitted'
        );
        onRequestSent?.(requestData);
      } else {
        toast.error(response.message);
      }
    } catch {
      toast.error('Failed to submit extra quantity request');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="bg-white rounded-lg shadow-md p-6">
      <div className="flex items-center gap-3 mb-4">
        <Package className="w-6 h-6 text-amber-600" />
        <h2 className="text-xl font-semibold text-gray-900">Extra Quantity Request</h2>
      </div>

      <div className="bg-amber-50 border border-amber-200 rounded-lg p-3 mb-4">
        <div className="flex items-start gap-2">
          <AlertTriangle className="w-4 h-4 text-amber-600 mt-0.5" />
          <p className="text-sm text-amber-800">
            Extra quantity requests require client OTP verification before processing.
          </p>
        </div>
      </div>

      {/* Items */}
      <div className="space-y-3 mb-4">
        <label className="block text-sm font-medium text-gray-700">Items Required</label>
        {items.map((item, index) => (
          <div key={index} className="flex items-start gap-2">
            <div className="flex-1 grid grid-cols-3 gap-2">
              <input
                type="text"
                value={item.itemName}
                onChange={(e) => handleItemChange(index, 'itemName', e.target.value)}
                placeholder="Item name"
                className="px-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
              />
              <input
                type="number"
                min="1"
                value={item.quantity}
                onChange={(e) => handleItemChange(index, 'quantity', Number(e.target.value))}
                placeholder="Qty"
                className="px-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
              />
              <input
                type="text"
                value={item.reason}
                onChange={(e) => handleItemChange(index, 'reason', e.target.value)}
                placeholder="Reason"
                className="px-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
              />
            </div>
            {items.length > 1 && (
              <button
                onClick={() => handleRemoveItem(index)}
                className="p-2 text-red-500 hover:bg-red-50 rounded-lg"
              >
                <XCircle className="w-4 h-4" />
              </button>
            )}
          </div>
        ))}
        <button
          onClick={handleAddItem}
          className="text-sm text-blue-600 hover:text-blue-700 font-medium"
        >
          + Add Another Item
        </button>
      </div>

      {/* Urgency Level */}
      <div className="mb-4">
        <label className="block text-sm font-medium text-gray-700 mb-1">Urgency Level</label>
        <div className="flex gap-2">
          {['NORMAL', 'URGENT', 'CRITICAL'].map((level) => (
            <button
              key={level}
              onClick={() => setUrgencyLevel(level)}
              className={`px-4 py-2 text-sm rounded-lg border ${
                urgencyLevel === level
                  ? level === 'CRITICAL'
                    ? 'bg-red-100 border-red-300 text-red-800'
                    : level === 'URGENT'
                    ? 'bg-orange-100 border-orange-300 text-orange-800'
                    : 'bg-blue-100 border-blue-300 text-blue-800'
                  : 'border-gray-300 text-gray-600 hover:bg-gray-50'
              }`}
            >
              {level}
            </button>
          ))}
        </div>
      </div>

      {/* Notes */}
      <div className="mb-4">
        <label className="block text-sm font-medium text-gray-700 mb-1">Additional Notes</label>
        <textarea
          value={additionalNotes}
          onChange={(e) => setAdditionalNotes(e.target.value)}
          rows={2}
          placeholder="Additional context..."
          className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg"
        />
      </div>

      {/* Submit */}
      <button
        onClick={handleSubmitRequest}
        disabled={submitting || !items.some((i) => i.itemName.trim())}
        className="w-full px-4 py-3 bg-amber-600 text-white rounded-lg font-medium hover:bg-amber-700 disabled:opacity-50"
      >
        {submitting ? 'Submitting...' : 'Submit Extra Quantity Request'}
      </button>

      {/* Request History */}
      {requestHistory.length > 0 && (
        <div className="mt-6 pt-4 border-t border-gray-200">
          <h4 className="text-sm font-medium text-gray-700 mb-2">Request History</h4>
          <div className="space-y-2">
            {requestHistory.map((req) => (
              <div key={req.id} className="flex items-center justify-between bg-gray-50 rounded-lg px-3 py-2">
                <div>
                  <p className="text-sm font-medium">
                    {req.items.map((i) => `${i.itemName} x${i.quantity}`).join(', ')}
                  </p>
                  <p className="text-xs text-gray-500">
                    {new Date(req.requestedAt).toLocaleTimeString()}
                  </p>
                </div>
                <span
                  className={`text-xs px-2 py-1 rounded-full ${
                    req.status === 'AWAITING_OTP'
                      ? 'bg-yellow-100 text-yellow-800'
                      : req.status === 'APPROVED'
                      ? 'bg-green-100 text-green-800'
                      : 'bg-blue-100 text-blue-800'
                  }`}
                >
                  {req.status === 'AWAITING_OTP' ? (
                    <span className="flex items-center gap-1">
                      <Clock className="w-3 h-3" /> Awaiting OTP
                    </span>
                  ) : req.status === 'APPROVED' ? (
                    <span className="flex items-center gap-1">
                      <CheckCircle2 className="w-3 h-3" /> Approved
                    </span>
                  ) : (
                    req.status
                  )}
                </span>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
};

ExtraQuantityRequest.propTypes = {
  assignmentId: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired,
  onRequestSent: PropTypes.func,
};

export default ExtraQuantityRequest;
