import React, { useState } from 'react';
import { AlertTriangle } from 'lucide-react';
import { ConfirmActionModal } from '../../../common/safety';

/**
 * AllergyEmergencyButton Component
 *
 * Emergency allergy modification for event day
 * - No pricing shown
 * - Requires reason + confirmation
 * - Direct communication with chef
 */

const AllergyEmergencyButton = ({ onEmergency }) => {
  const [showModal, setShowModal] = useState(false);
  const [allergyDetails, setAllergyDetails] = useState('');
  const [affectedGuest, setAffectedGuest] = useState('');
  const [severity, setSeverity] = useState('');

  const handleSubmit = () => {
    if (!allergyDetails || !affectedGuest || !severity) {
      alert('Please fill in all required fields');
      return;
    }

    onEmergency({
      allergyDetails,
      affectedGuest,
      severity,
      timestamp: new Date()
    });

    setShowModal(false);
    setAllergyDetails('');
    setAffectedGuest('');
    setSeverity('');
  };

  return (
    <>
      <button
        onClick={() => setShowModal(true)}
        className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors font-medium flex items-center gap-2 shadow-lg"
      >
        <AlertTriangle className="w-5 h-5" />
        Allergy Emergency
      </button>

      {showModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black bg-opacity-50 backdrop-blur-sm">
          <div className="bg-white rounded-xl max-w-md w-full shadow-2xl overflow-hidden">
            {/* Header */}
            <div className="bg-red-600 p-6 text-white">
              <div className="flex items-center gap-3">
                <div className="w-12 h-12 bg-white bg-opacity-20 rounded-full flex items-center justify-center">
                  <AlertTriangle className="w-6 h-6" />
                </div>
                <div>
                  <h2 className="text-2xl font-bold">Allergy Emergency</h2>
                  <p className="text-sm text-red-100">Immediate chef notification</p>
                </div>
              </div>
            </div>

            {/* Body */}
            <div className="p-6 space-y-4">
              <div className="bg-red-50 border border-red-200 rounded-lg p-3">
                <p className="text-sm text-red-900">
                  <strong>This is for severe allergy emergencies only.</strong> The chef will be
                  immediately notified to adjust preparation.
                </p>
              </div>

              {/* Affected Guest */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Affected Guest <span className="text-red-600">*</span>
                </label>
                <input
                  type="text"
                  value={affectedGuest}
                  onChange={(e) => setAffectedGuest(e.target.value)}
                  placeholder="Guest name or identifier"
                  className="w-full px-4 py-2 border-2 border-gray-300 rounded-lg focus:ring-2 focus:ring-red-500 focus:border-transparent"
                  required
                />
              </div>

              {/* Severity */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Severity Level <span className="text-red-600">*</span>
                </label>
                <select
                  value={severity}
                  onChange={(e) => setSeverity(e.target.value)}
                  className="w-full px-4 py-2 border-2 border-gray-300 rounded-lg focus:ring-2 focus:ring-red-500 focus:border-transparent"
                  required
                >
                  <option value="">Select severity...</option>
                  <option value="life-threatening">Life-Threatening (Anaphylaxis risk)</option>
                  <option value="severe">Severe (Strong reaction)</option>
                  <option value="moderate">Moderate (Noticeable symptoms)</option>
                </select>
              </div>

              {/* Allergy Details */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Allergy Details <span className="text-red-600">*</span>
                </label>
                <textarea
                  value={allergyDetails}
                  onChange={(e) => setAllergyDetails(e.target.value)}
                  placeholder="Specify allergen (e.g., peanuts, shellfish, dairy) and any previous reactions..."
                  rows={4}
                  className="w-full px-4 py-2 border-2 border-gray-300 rounded-lg focus:ring-2 focus:ring-red-500 focus:border-transparent"
                  required
                />
                <p className="text-xs text-gray-600 mt-1">
                  Be as specific as possible. Include cross-contamination concerns if any.
                </p>
              </div>

              <div className="bg-amber-50 border border-amber-200 rounded-lg p-3">
                <p className="text-xs text-amber-900">
                  <strong>Note:</strong> This notification goes directly to the chef. For
                  life-threatening emergencies, please also contact event staff immediately.
                </p>
              </div>
            </div>

            {/* Footer */}
            <div className="p-6 bg-gray-50 border-t border-gray-200 flex gap-3">
              <button
                onClick={() => setShowModal(false)}
                className="flex-1 px-6 py-3 border-2 border-gray-300 text-gray-700 rounded-lg hover:bg-white transition-colors font-medium"
              >
                Cancel
              </button>
              <button
                onClick={handleSubmit}
                disabled={!allergyDetails || !affectedGuest || !severity}
                className="flex-1 px-6 py-3 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors font-medium disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Notify Chef Now
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
};

export default AllergyEmergencyButton;
