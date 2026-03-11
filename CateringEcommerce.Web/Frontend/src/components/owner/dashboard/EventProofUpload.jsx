import React, { useState } from 'react';
import { Camera, Upload, CheckCircle, Clock, MapPin, Image } from 'lucide-react';
import { DisabledButton } from '../../common/safety';

/**
 * EventProofUpload Component
 *
 * Partner uploads timestamped proof photos:
 * - Arrival photo (team arrived at venue)
 * - Setup photo (service setup complete)
 * - Completion photo (event service finished)
 */

const EventProofUpload = ({
  order,
  existingProofs = {},
  onUploadProof,
  isLoading = false
}) => {
  const [selectedProofType, setSelectedProofType] = useState(null);
  const [uploadedFile, setUploadedFile] = useState(null);
  const [uploadPreview, setUploadPreview] = useState(null);
  const [notes, setNotes] = useState('');

  // Proof types configuration
  const proofTypes = [
    {
      id: 'arrival',
      label: 'Arrival Proof',
      icon: MapPin,
      description: 'Photo of team arrived at venue',
      color: 'blue',
      required: true,
      completed: !!existingProofs.arrival
    },
    {
      id: 'setup',
      label: 'Setup Proof',
      icon: Image,
      description: 'Photo of service setup complete',
      color: 'purple',
      required: true,
      completed: !!existingProofs.setup
    },
    {
      id: 'completion',
      label: 'Completion Proof',
      icon: CheckCircle,
      description: 'Photo after service completion',
      color: 'green',
      required: true,
      completed: !!existingProofs.completion
    }
  ];

  // Handle file selection
  const handleFileSelect = (e) => {
    const file = e.target.files[0];
    if (file) {
      setUploadedFile(file);
      setUploadPreview(URL.createObjectURL(file));
    }
  };

  // Handle upload submit
  const handleSubmitProof = () => {
    if (!uploadedFile || !selectedProofType) return;

    const proofData = {
      type: selectedProofType,
      file: uploadedFile,
      timestamp: new Date(),
      notes: notes.trim(),
      orderId: order.orderId
    };

    onUploadProof(proofData);

    // Reset
    setUploadedFile(null);
    setUploadPreview(null);
    setNotes('');
    setSelectedProofType(null);
  };

  // Get completion percentage
  const completedCount = proofTypes.filter(p => p.completed).length;
  const completionPercentage = (completedCount / proofTypes.length) * 100;

  return (
    <div className="bg-white rounded-lg p-6 shadow-sm border border-gray-200">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h3 className="font-semibold text-lg flex items-center gap-2">
            <Camera className="w-5 h-5 text-blue-600" />
            Event Proof Upload
          </h3>
          <p className="text-sm text-gray-600 mt-1">
            Order #{order.orderNumber} • {order.eventType}
          </p>
        </div>

        {/* Completion Badge */}
        <div className="text-center">
          <div className="relative w-16 h-16 mx-auto mb-2">
            <svg className="w-full h-full transform -rotate-90">
              <circle
                cx="32"
                cy="32"
                r="28"
                stroke="#E5E7EB"
                strokeWidth="6"
                fill="none"
              />
              <circle
                cx="32"
                cy="32"
                r="28"
                stroke={completionPercentage === 100 ? '#10B981' : '#3B82F6'}
                strokeWidth="6"
                fill="none"
                strokeDasharray={`${2 * Math.PI * 28}`}
                strokeDashoffset={`${2 * Math.PI * 28 * (1 - completionPercentage / 100)}`}
                strokeLinecap="round"
              />
            </svg>
            <div className="absolute inset-0 flex items-center justify-center">
              <span className="text-sm font-bold">{Math.round(completionPercentage)}%</span>
            </div>
          </div>
          <p className="text-xs text-gray-600">{completedCount}/{proofTypes.length} Complete</p>
        </div>
      </div>

      {/* Proof Type Selection */}
      <div className="grid grid-cols-3 gap-3 mb-6">
        {proofTypes.map(proof => {
          const ProofIcon = proof.icon;

          return (
            <button
              key={proof.id}
              onClick={() => !proof.completed && setSelectedProofType(proof.id)}
              disabled={proof.completed}
              className={`
                border-2 rounded-lg p-4 transition-all relative
                ${proof.completed
                  ? 'border-green-300 bg-green-50 cursor-not-allowed'
                  : selectedProofType === proof.id
                  ? `border-${proof.color}-500 bg-${proof.color}-50`
                  : 'border-gray-300 hover:border-gray-400'
                }
              `}
            >
              {proof.completed && (
                <div className="absolute top-2 right-2">
                  <CheckCircle className="w-5 h-5 text-green-600" />
                </div>
              )}

              <div className={`
                w-12 h-12 rounded-full mx-auto mb-3 flex items-center justify-center
                ${proof.completed ? 'bg-green-200' : `bg-${proof.color}-100`}
              `}>
                <ProofIcon className={`w-6 h-6 ${
                  proof.completed ? 'text-green-700' : `text-${proof.color}-700`
                }`} />
              </div>

              <p className="font-medium text-sm text-center mb-1">{proof.label}</p>
              <p className="text-xs text-gray-600 text-center">{proof.description}</p>

              {proof.completed && (
                <p className="text-xs text-green-700 font-medium text-center mt-2">
                  ✓ Uploaded
                </p>
              )}
            </button>
          );
        })}
      </div>

      {/* Upload Section */}
      {selectedProofType && !proofTypes.find(p => p.id === selectedProofType)?.completed && (
        <div className="border-2 border-blue-300 rounded-lg p-6 bg-blue-50">
          <h4 className="font-semibold mb-4 flex items-center gap-2">
            <Upload className="w-5 h-5" />
            Upload {proofTypes.find(p => p.id === selectedProofType)?.label}
          </h4>

          {!uploadPreview ? (
            <div className="border-2 border-dashed border-blue-300 rounded-lg p-8 text-center bg-white">
              <Camera className="w-12 h-12 text-blue-500 mx-auto mb-4" />
              <p className="text-gray-700 font-medium mb-2">Take or Upload Photo</p>
              <p className="text-sm text-gray-600 mb-4">
                Photo will be timestamped automatically
              </p>
              <input
                type="file"
                accept="image/*"
                capture="environment"
                onChange={handleFileSelect}
                className="hidden"
                id="proof-upload"
              />
              <label
                htmlFor="proof-upload"
                className="inline-block px-6 py-3 bg-blue-600 text-white rounded-lg cursor-pointer hover:bg-blue-700 transition-colors"
              >
                Choose Photo
              </label>
            </div>
          ) : (
            <div className="space-y-4">
              {/* Preview */}
              <div className="relative">
                <img
                  src={uploadPreview}
                  alt="Proof preview"
                  className="w-full h-64 object-cover rounded-lg border-2 border-gray-300"
                />
                <div className="absolute top-2 right-2 bg-black bg-opacity-70 text-white text-xs px-3 py-1 rounded-full flex items-center gap-1">
                  <Clock className="w-3 h-3" />
                  {new Date().toLocaleTimeString('en-IN')}
                </div>
                <button
                  onClick={() => {
                    setUploadedFile(null);
                    setUploadPreview(null);
                  }}
                  className="absolute top-2 left-2 bg-red-600 text-white text-xs px-3 py-1 rounded-full hover:bg-red-700"
                >
                  Change Photo
                </button>
              </div>

              {/* Notes */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Notes (Optional)
                </label>
                <textarea
                  value={notes}
                  onChange={(e) => setNotes(e.target.value)}
                  placeholder="Add any relevant notes about this proof..."
                  rows={3}
                  className="w-full px-4 py-2 border-2 border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>

              {/* Upload Button */}
              <DisabledButton
                onClick={handleSubmitProof}
                disabled={!uploadedFile}
                disabledReason="Please select a photo first"
                variant="primary"
                fullWidth
                loading={isLoading}
                icon={Upload}
              >
                Upload Proof with Timestamp
              </DisabledButton>
            </div>
          )}
        </div>
      )}

      {/* Existing Proofs Display */}
      {Object.keys(existingProofs).length > 0 && (
        <div className="mt-6 pt-6 border-t border-gray-200">
          <h4 className="font-semibold mb-4">Uploaded Proofs</h4>
          <div className="grid grid-cols-3 gap-4">
            {proofTypes.filter(p => existingProofs[p.id]).map(proof => {
              const proofData = existingProofs[proof.id];
              const ProofIcon = proof.icon;

              return (
                <div key={proof.id} className="border border-gray-200 rounded-lg overflow-hidden">
                  <img
                    src={proofData.url}
                    alt={proof.label}
                    className="w-full h-32 object-cover"
                  />
                  <div className="p-3 bg-gray-50">
                    <div className="flex items-center gap-2 mb-2">
                      <ProofIcon className="w-4 h-4 text-gray-600" />
                      <p className="text-sm font-medium">{proof.label}</p>
                    </div>
                    <p className="text-xs text-gray-600 flex items-center gap-1">
                      <Clock className="w-3 h-3" />
                      {new Date(proofData.timestamp).toLocaleString('en-IN')}
                    </p>
                    {proofData.notes && (
                      <p className="text-xs text-gray-600 mt-2">{proofData.notes}</p>
                    )}
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      )}

      {/* Completion Notice */}
      {completionPercentage === 100 && (
        <div className="mt-4 bg-green-100 border border-green-300 rounded-lg p-4">
          <div className="flex items-center gap-3">
            <CheckCircle className="w-6 h-6 text-green-700 flex-shrink-0" />
            <div>
              <p className="font-semibold text-green-900">All Proofs Uploaded</p>
              <p className="text-sm text-green-800">
                Service documentation complete. Payment release processing can proceed.
              </p>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default EventProofUpload;
