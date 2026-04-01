/**
 * RegistrationProgressTracker Component
 * Displays registration workflow status and progress
 */

import { useEffect, useState } from 'react';
import PropTypes from 'prop-types';
import { CheckCircle, Clock, AlertCircle, FileText } from 'lucide-react';
import { WorkflowStepper, ProgressCircle } from '../common';
import { registrationApi } from '../../../services/api/supervisor';
import { getRegistrationSteps } from '../../../utils/supervisor/supervisorEnums';
import { formatTimestamp } from '../../../utils/supervisor/helpers';
import toast from 'react-hot-toast';

const RegistrationProgressTracker = ({ registrationId }) => {
  const [progress, setProgress] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchProgress();
    // Poll every 30 seconds for updates
    const interval = setInterval(fetchProgress, 30000);
    return () => clearInterval(interval);
  }, [registrationId]);

  const fetchProgress = async () => {
    try {
      const response = await registrationApi.getRegistrationProgress(registrationId);
      if (response.success) {
        setProgress(response.data);
      }
    } catch (error) {
      console.error('Failed to fetch progress:', error);
      toast.error('Failed to load registration progress');
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="flex justify-center items-center py-12">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  if (!progress) {
    return (
      <div className="text-center py-12">
        <AlertCircle className="w-12 h-12 text-red-500 mx-auto mb-4" />
        <p className="text-gray-600">Registration not found</p>
      </div>
    );
  }

  const workflowSteps = getRegistrationSteps();

  return (
    <div className="max-w-4xl mx-auto px-4 py-8">
      {/* Header */}
      <div className="text-center mb-8">
        <h1 className="text-3xl font-bold text-gray-900">
          Registration Progress
        </h1>
        <p className="text-gray-600 mt-2">
          Track your supervisor registration status
        </p>
      </div>

      {/* Progress Summary Card */}
      <div className="bg-white rounded-lg shadow-md p-6 mb-6">
        <div className="flex items-center justify-between mb-6">
          <div>
            <h2 className="text-xl font-semibold text-gray-900">
              {progress.status === 'APPROVED' ? 'Approved!' : 'In Progress'}
            </h2>
            <p className="text-sm text-gray-600 mt-1">
              Current Stage: <span className="font-medium">{progress.currentStage.replace(/_/g, ' ')}</span>
            </p>
          </div>

          {/* Progress Circle */}
          <ProgressCircle
            percentage={progress.progressPercentage}
            size={100}
            showPercentage={true}
          />
        </div>

        {/* Workflow Stepper */}
        <WorkflowStepper
          steps={workflowSteps}
          currentStep={progress.completedStages + 1}
        />

        {/* Expected Activation Date */}
        {progress.expectedActivationDate && progress.status !== 'APPROVED' && (
          <div className="mt-6 bg-blue-50 border border-blue-200 rounded-lg p-4">
            <div className="flex items-start gap-2">
              <Clock className="w-5 h-5 text-blue-600 mt-0.5" />
              <div>
                <p className="text-sm font-medium text-blue-900">
                  Expected Activation
                </p>
                <p className="text-sm text-blue-700 mt-1">
                  {formatTimestamp(progress.expectedActivationDate, 'long')}
                </p>
              </div>
            </div>
          </div>
        )}

        {/* Approved Status */}
        {progress.status === 'APPROVED' && (
          <div className="mt-6 bg-green-50 border border-green-200 rounded-lg p-4">
            <div className="flex items-start gap-2">
              <CheckCircle className="w-5 h-5 text-green-600 mt-0.5" />
              <div>
                <p className="text-sm font-medium text-green-900">
                  Congratulations! You're Activated
                </p>
                <p className="text-sm text-green-700 mt-1">
                  You can now receive event assignments
                </p>
              </div>
            </div>
          </div>
        )}

        {/* Rejected Status */}
        {progress.status === 'REJECTED' && (
          <div className="mt-6 bg-red-50 border border-red-200 rounded-lg p-4">
            <div className="flex items-start gap-2">
              <AlertCircle className="w-5 h-5 text-red-600 mt-0.5" />
              <div>
                <p className="text-sm font-medium text-red-900">
                  Registration Rejected
                </p>
                <p className="text-sm text-red-700 mt-1">
                  Please contact support for more information
                </p>
              </div>
            </div>
          </div>
        )}
      </div>

      {/* Stage History */}
      <div className="bg-white rounded-lg shadow-md p-6">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">
          <FileText className="inline w-5 h-5 mr-2" />
          Registration Timeline
        </h3>

        <div className="space-y-4">
          {progress.stageHistory.map((stage, index) => (
            <div
              key={index}
              className={`flex items-start gap-4 pb-4 ${
                index < progress.stageHistory.length - 1 ? 'border-b border-gray-200' : ''
              }`}
            >
              {/* Icon */}
              <div className={`flex-shrink-0 w-10 h-10 rounded-full flex items-center justify-center ${
                stage.isCompleted
                  ? 'bg-green-100'
                  : stage.isCurrentStage
                  ? 'bg-blue-100'
                  : 'bg-gray-100'
              }`}>
                {stage.isCompleted ? (
                  <CheckCircle className="w-5 h-5 text-green-600" />
                ) : stage.isCurrentStage ? (
                  <Clock className="w-5 h-5 text-blue-600" />
                ) : (
                  <div className="w-3 h-3 rounded-full bg-gray-400"></div>
                )}
              </div>

              {/* Content */}
              <div className="flex-1">
                <div className="flex items-center justify-between">
                  <h4 className={`text-sm font-medium ${
                    stage.isCurrentStage ? 'text-blue-900' : 'text-gray-900'
                  }`}>
                    {stage.stage.replace(/_/g, ' ')}
                  </h4>
                  {stage.completedDate && (
                    <span className="text-xs text-gray-500">
                      {formatTimestamp(stage.completedDate, 'short')}
                    </span>
                  )}
                </div>

                {stage.notes && (
                  <p className="text-sm text-gray-600 mt-1">
                    {stage.notes}
                  </p>
                )}

                {stage.isCurrentStage && !stage.isCompleted && (
                  <p className="text-xs text-blue-600 mt-1">
                    Currently in progress...
                  </p>
                )}
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Help */}
      <div className="mt-6 text-center text-sm text-gray-600">
        <p>Questions? Contact us at support@example.com or call 1800-XXX-XXXX</p>
      </div>
    </div>
  );
};

RegistrationProgressTracker.propTypes = {
  registrationId: PropTypes.number.isRequired,
};

export default RegistrationProgressTracker;
