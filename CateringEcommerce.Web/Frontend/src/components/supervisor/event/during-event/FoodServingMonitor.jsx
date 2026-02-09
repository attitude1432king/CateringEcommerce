/**
 * FoodServingMonitor Component
 * Track food serving progress during live event
 */

import { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import { UtensilsCrossed, Clock, Camera, CheckCircle2, AlertTriangle } from 'lucide-react';
import { eventSupervisionApi } from '../../../../services/api/supervisor';
import { TimestampedEvidenceUpload } from '../../common/forms';
import toast from 'react-hot-toast';

const SERVING_STAGES = [
  { id: 'starters', label: 'Starters / Appetizers', order: 1 },
  { id: 'main_course', label: 'Main Course', order: 2 },
  { id: 'desserts', label: 'Desserts', order: 3 },
  { id: 'beverages', label: 'Beverages', order: 4 },
];

const FoodServingMonitor = ({ assignmentId, onUpdate }) => {
  const [servingData, setServingData] = useState({});
  const [currentStage, setCurrentStage] = useState(null);
  const [submitting, setSubmitting] = useState(false);
  const [showEvidenceModal, setShowEvidenceModal] = useState(null);
  const [tracking, setTracking] = useState(null);

  useEffect(() => {
    loadTracking();
  }, [assignmentId]);

  const loadTracking = async () => {
    const response = await eventSupervisionApi.getDuringEventTracking(assignmentId);
    if (response.success && response.data?.data) {
      setTracking(response.data.data);
    }
  };

  const handleStartServing = (stageId) => {
    setServingData((prev) => ({
      ...prev,
      [stageId]: {
        ...prev[stageId],
        startTime: new Date().toISOString(),
        status: 'IN_PROGRESS',
      },
    }));
    setCurrentStage(stageId);
  };

  const handleCompleteServing = async (stageId) => {
    setSubmitting(true);
    try {
      const stageData = servingData[stageId] || {};
      const response = await eventSupervisionApi.recordFoodServingMonitor({
        assignmentId,
        servingStage: stageId,
        startTime: stageData.startTime,
        endTime: new Date().toISOString(),
        qualityRating: stageData.qualityRating || 5,
        temperature: stageData.temperature || 'APPROPRIATE',
        notes: stageData.notes || '',
        evidenceUrl: stageData.evidenceUrl || null,
      });

      if (response.success) {
        setServingData((prev) => ({
          ...prev,
          [stageId]: { ...prev[stageId], status: 'COMPLETED', endTime: new Date().toISOString() },
        }));
        setCurrentStage(null);
        toast.success(`${SERVING_STAGES.find((s) => s.id === stageId)?.label} serving recorded`);
        onUpdate?.();
      } else {
        toast.error(response.message);
      }
    } catch {
      toast.error('Failed to record serving data');
    } finally {
      setSubmitting(false);
    }
  };

  const handleEvidenceUpload = (stageId, evidenceData) => {
    setServingData((prev) => ({
      ...prev,
      [stageId]: { ...prev[stageId], evidenceUrl: evidenceData.url },
    }));
    setShowEvidenceModal(null);
    toast.success('Evidence uploaded');
  };

  const getStageStatus = (stageId) => {
    return servingData[stageId]?.status || 'PENDING';
  };

  return (
    <div className="space-y-4">
      <div className="bg-white rounded-lg shadow-md p-6">
        <div className="flex items-center gap-3 mb-4">
          <UtensilsCrossed className="w-6 h-6 text-orange-600" />
          <h2 className="text-xl font-semibold text-gray-900">Food Serving Monitor</h2>
        </div>

        <div className="space-y-4">
          {SERVING_STAGES.map((stage) => {
            const status = getStageStatus(stage.id);
            const stageData = servingData[stage.id] || {};

            return (
              <div
                key={stage.id}
                className={`border rounded-lg p-4 transition-colors ${
                  status === 'COMPLETED'
                    ? 'border-green-200 bg-green-50'
                    : status === 'IN_PROGRESS'
                    ? 'border-blue-200 bg-blue-50'
                    : 'border-gray-200'
                }`}
              >
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-3">
                    {status === 'COMPLETED' ? (
                      <CheckCircle2 className="w-5 h-5 text-green-600" />
                    ) : status === 'IN_PROGRESS' ? (
                      <Clock className="w-5 h-5 text-blue-600 animate-pulse" />
                    ) : (
                      <div className="w-5 h-5 rounded-full border-2 border-gray-300" />
                    )}
                    <div>
                      <p className="font-medium text-gray-900">{stage.label}</p>
                      {status === 'IN_PROGRESS' && stageData.startTime && (
                        <p className="text-xs text-blue-600">
                          Started: {new Date(stageData.startTime).toLocaleTimeString()}
                        </p>
                      )}
                      {status === 'COMPLETED' && stageData.endTime && (
                        <p className="text-xs text-green-600">
                          Completed: {new Date(stageData.endTime).toLocaleTimeString()}
                        </p>
                      )}
                    </div>
                  </div>

                  <div className="flex items-center gap-2">
                    {status === 'PENDING' && (
                      <button
                        onClick={() => handleStartServing(stage.id)}
                        disabled={currentStage && currentStage !== stage.id}
                        className="px-3 py-1.5 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50"
                      >
                        Start
                      </button>
                    )}
                    {status === 'IN_PROGRESS' && (
                      <>
                        <button
                          onClick={() => setShowEvidenceModal(stage.id)}
                          className="p-1.5 text-gray-600 hover:text-blue-600"
                          title="Upload Evidence"
                        >
                          <Camera className="w-5 h-5" />
                        </button>
                        <button
                          onClick={() => handleCompleteServing(stage.id)}
                          disabled={submitting}
                          className="px-3 py-1.5 text-sm bg-green-600 text-white rounded-lg hover:bg-green-700 disabled:opacity-50"
                        >
                          {submitting ? 'Saving...' : 'Complete'}
                        </button>
                      </>
                    )}
                  </div>
                </div>

                {/* Quality & Temperature for in-progress stage */}
                {status === 'IN_PROGRESS' && (
                  <div className="mt-3 pt-3 border-t border-blue-200 grid grid-cols-2 gap-3">
                    <div>
                      <label className="block text-xs font-medium text-gray-700 mb-1">Quality Rating</label>
                      <select
                        value={stageData.qualityRating || 5}
                        onChange={(e) =>
                          setServingData((prev) => ({
                            ...prev,
                            [stage.id]: { ...prev[stage.id], qualityRating: Number(e.target.value) },
                          }))
                        }
                        className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded-lg"
                      >
                        {[5, 4, 3, 2, 1].map((n) => (
                          <option key={n} value={n}>{n} - {['Poor', 'Fair', 'Good', 'Very Good', 'Excellent'][n - 1]}</option>
                        ))}
                      </select>
                    </div>
                    <div>
                      <label className="block text-xs font-medium text-gray-700 mb-1">Temperature</label>
                      <select
                        value={stageData.temperature || 'APPROPRIATE'}
                        onChange={(e) =>
                          setServingData((prev) => ({
                            ...prev,
                            [stage.id]: { ...prev[stage.id], temperature: e.target.value },
                          }))
                        }
                        className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded-lg"
                      >
                        <option value="APPROPRIATE">Appropriate</option>
                        <option value="TOO_HOT">Too Hot</option>
                        <option value="TOO_COLD">Too Cold</option>
                        <option value="NOT_CHECKED">Not Checked</option>
                      </select>
                    </div>
                    <div className="col-span-2">
                      <label className="block text-xs font-medium text-gray-700 mb-1">Notes</label>
                      <textarea
                        value={stageData.notes || ''}
                        onChange={(e) =>
                          setServingData((prev) => ({
                            ...prev,
                            [stage.id]: { ...prev[stage.id], notes: e.target.value },
                          }))
                        }
                        rows={2}
                        placeholder="Any observations..."
                        className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded-lg"
                      />
                    </div>
                  </div>
                )}
              </div>
            );
          })}
        </div>
      </div>

      {/* Evidence Upload Modal */}
      {showEvidenceModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg max-w-md w-full p-6">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Upload Serving Evidence</h3>
            <TimestampedEvidenceUpload
              onUploadComplete={(data) => handleEvidenceUpload(showEvidenceModal, data)}
              allowedTypes={['photo']}
            />
            <button
              onClick={() => setShowEvidenceModal(null)}
              className="mt-4 w-full px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 hover:bg-gray-50"
            >
              Cancel
            </button>
          </div>
        </div>
      )}
    </div>
  );
};

FoodServingMonitor.propTypes = {
  assignmentId: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired,
  onUpdate: PropTypes.func,
};

export default FoodServingMonitor;
