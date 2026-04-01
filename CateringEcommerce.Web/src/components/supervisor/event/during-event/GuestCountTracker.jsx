/**
 * GuestCountTracker Component
 * Track and update actual guest count vs expected during event
 */

import { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import { Users, TrendingUp, TrendingDown, Minus, Camera } from 'lucide-react';
import { eventSupervisionApi } from '../../../../services/api/supervisor';
import { TimestampedEvidenceUpload } from '../../common/forms';
import toast from 'react-hot-toast';

const GuestCountTracker = ({ assignmentId, expectedGuests = 0, onUpdate }) => {
  const [actualCount, setActualCount] = useState(0);
  const [countHistory, setCountHistory] = useState([]);
  const [notes, setNotes] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [showEvidence, setShowEvidence] = useState(false);
  const [evidenceUrl, setEvidenceUrl] = useState(null);

  useEffect(() => {
    loadTracking();
  }, [assignmentId]);

  const loadTracking = async () => {
    const response = await eventSupervisionApi.getDuringEventTracking(assignmentId);
    if (response.success && response.data?.data) {
      const tracking = response.data.data;
      if (tracking.guestCountUpdates) {
        setCountHistory(tracking.guestCountUpdates);
        const latest = tracking.guestCountUpdates[tracking.guestCountUpdates.length - 1];
        if (latest) setActualCount(latest.count);
      }
    }
  };

  const handleSubmitCount = async () => {
    if (actualCount <= 0) {
      toast.error('Please enter a valid guest count');
      return;
    }

    setSubmitting(true);
    try {
      const response = await eventSupervisionApi.updateGuestCount({
        assignmentId,
        actualGuestCount: actualCount,
        expectedGuestCount: expectedGuests,
        timestamp: new Date().toISOString(),
        notes,
        evidenceUrl: evidenceUrl?.url || null,
      });

      if (response.success) {
        setCountHistory((prev) => [
          ...prev,
          {
            count: actualCount,
            timestamp: new Date().toISOString(),
            notes,
          },
        ]);
        setNotes('');
        setEvidenceUrl(null);
        toast.success('Guest count updated');
        onUpdate?.();
      } else {
        toast.error(response.message);
      }
    } catch {
      toast.error('Failed to update guest count');
    } finally {
      setSubmitting(false);
    }
  };

  const variance = actualCount - expectedGuests;
  const variancePercent = expectedGuests > 0 ? ((variance / expectedGuests) * 100).toFixed(1) : 0;

  return (
    <div className="bg-white rounded-lg shadow-md p-6">
      <div className="flex items-center gap-3 mb-4">
        <Users className="w-6 h-6 text-purple-600" />
        <h2 className="text-xl font-semibold text-gray-900">Guest Count Tracker</h2>
      </div>

      {/* Expected vs Actual Display */}
      <div className="grid grid-cols-3 gap-4 mb-6">
        <div className="text-center p-4 bg-gray-50 rounded-lg">
          <p className="text-sm text-gray-600">Expected</p>
          <p className="text-2xl font-bold text-gray-900">{expectedGuests}</p>
        </div>
        <div className="text-center p-4 bg-blue-50 rounded-lg">
          <p className="text-sm text-blue-600">Actual</p>
          <p className="text-2xl font-bold text-blue-900">{actualCount}</p>
        </div>
        <div
          className={`text-center p-4 rounded-lg ${
            variance > 0 ? 'bg-orange-50' : variance < 0 ? 'bg-green-50' : 'bg-gray-50'
          }`}
        >
          <p className="text-sm text-gray-600">Variance</p>
          <div className="flex items-center justify-center gap-1">
            {variance > 0 ? (
              <TrendingUp className="w-4 h-4 text-orange-600" />
            ) : variance < 0 ? (
              <TrendingDown className="w-4 h-4 text-green-600" />
            ) : (
              <Minus className="w-4 h-4 text-gray-600" />
            )}
            <p className={`text-2xl font-bold ${variance > 0 ? 'text-orange-600' : variance < 0 ? 'text-green-600' : 'text-gray-600'}`}>
              {variance > 0 ? `+${variance}` : variance}
            </p>
          </div>
          <p className="text-xs text-gray-500">{variancePercent}%</p>
        </div>
      </div>

      {/* Over-capacity warning */}
      {variance > expectedGuests * 0.1 && (
        <div className="bg-orange-50 border border-orange-200 rounded-lg p-3 mb-4">
          <p className="text-sm font-medium text-orange-800">
            Guest count is {variancePercent}% over expected. Consider requesting extra quantity.
          </p>
        </div>
      )}

      {/* Update Count */}
      <div className="space-y-3">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Update Actual Guest Count</label>
          <input
            type="number"
            min="0"
            value={actualCount}
            onChange={(e) => setActualCount(Number(e.target.value))}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Notes</label>
          <textarea
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            rows={2}
            placeholder="Observation notes..."
            className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
          />
        </div>

        <div className="flex items-center gap-3">
          <button
            onClick={() => setShowEvidence(true)}
            className="flex items-center gap-2 px-3 py-2 text-sm border border-gray-300 rounded-lg hover:bg-gray-50"
          >
            <Camera className="w-4 h-4" />
            {evidenceUrl ? 'Evidence Added' : 'Add Evidence'}
          </button>

          <button
            onClick={handleSubmitCount}
            disabled={submitting || actualCount <= 0}
            className="flex-1 px-4 py-2 bg-purple-600 text-white rounded-lg font-medium hover:bg-purple-700 disabled:opacity-50"
          >
            {submitting ? 'Updating...' : 'Update Count'}
          </button>
        </div>
      </div>

      {/* Count History */}
      {countHistory.length > 0 && (
        <div className="mt-6 pt-4 border-t border-gray-200">
          <h4 className="text-sm font-medium text-gray-700 mb-2">Count History</h4>
          <div className="space-y-2 max-h-40 overflow-y-auto">
            {countHistory.map((entry, i) => (
              <div key={i} className="flex items-center justify-between text-sm bg-gray-50 rounded px-3 py-2">
                <span className="font-medium">{entry.count} guests</span>
                <span className="text-gray-500">{new Date(entry.timestamp).toLocaleTimeString()}</span>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Evidence Modal */}
      {showEvidence && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg max-w-md w-full p-6">
            <h3 className="text-lg font-semibold mb-4">Upload Guest Count Evidence</h3>
            <TimestampedEvidenceUpload
              onUploadComplete={(data) => { setEvidenceUrl(data); setShowEvidence(false); toast.success('Evidence uploaded'); }}
              allowedTypes={['photo']}
            />
            <button
              onClick={() => setShowEvidence(false)}
              className="mt-4 w-full px-4 py-2 border border-gray-300 rounded-lg text-sm text-gray-700 hover:bg-gray-50"
            >
              Cancel
            </button>
          </div>
        </div>
      )}
    </div>
  );
};

GuestCountTracker.propTypes = {
  assignmentId: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired,
  expectedGuests: PropTypes.number,
  onUpdate: PropTypes.func,
};

export default GuestCountTracker;
